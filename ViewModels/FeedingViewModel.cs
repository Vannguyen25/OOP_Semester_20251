using Microsoft.EntityFrameworkCore;
using OOP_Semester.Data;
using OOP_Semester.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;


namespace OOP_Semester.ViewModels
{
    // ✅ UI wrapper (không đụng vào EF entity)
    public class UserFoodItemViewModel : INotifyPropertyChanged
    {
        public int UserID { get; }
        public int FoodID { get; }
        public Food Food { get; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string FoodName => Food?.Name ?? "(Unknown)";

        public UserFoodItemViewModel(UserFood entity)
        {
            UserID = entity.UserID;
            FoodID = entity.FoodID;
            Food = entity.Food;
            Quantity = (int)entity.Quantity;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class FeedingViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;

        private Pet _currentPet;
        public Pet CurrentPet
        {
            get => _currentPet;
            set { _currentPet = value; OnPropertyChanged(); }
        }

        private string _petAvatar;
        public string PetAvatar
        {
            get => _petAvatar;
            set { _petAvatar = value; OnPropertyChanged(); }
        }

        private int _maxExp = 100;
        public int MaxExp
        {
            get => _maxExp;
            set { _maxExp = value; OnPropertyChanged(); }
        }

        private string _expDisplay;
        public string ExpDisplay
        {
            get => _expDisplay;
            set { _expDisplay = value; OnPropertyChanged(); }
        }

        private int _hunger;
        public int Hunger
        {
            get => _hunger;
            set { _hunger = value; OnPropertyChanged(); }
        }

        private int _happiness;
        public int Happiness
        {
            get => _happiness;
            set { _happiness = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UserFoodItemViewModel> UserFoods { get; } =
            new ObservableCollection<UserFoodItemViewModel>();

        private UserFoodItemViewModel _selectedFood;
        public UserFoodItemViewModel SelectedFood
        {
            get => _selectedFood;
            set
            {
                _selectedFood = value;
                OnPropertyChanged();
                FeedingQuantity = 1;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private int _feedingQuantity = 1;
        public int FeedingQuantity
        {
            get => _feedingQuantity;
            set
            {
                if (SelectedFood != null)
                {
                    if (value > SelectedFood.Quantity) value = SelectedFood.Quantity;
                    if (value < 1) value = 1;
                }

                if (_feedingQuantity == value) return;
                _feedingQuantity = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _isChangePetVisible;
        public bool IsChangePetVisible
        {
            get => _isChangePetVisible;
            set { _isChangePetVisible = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PetType> AvailablePetTypes { get; } =
            new ObservableCollection<PetType>();

        public ICommand FeedPetCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand OpenChangePetCommand { get; }
        public ICommand CloseChangePetCommand { get; }
        public ICommand SelectPetTypeCommand { get; }

        public FeedingViewModel(User user)
        {
            _context = new AppDbContext();
            _currentUser = user;

            // ✅ chỉ subscribe 1 lần (tránh bị reload 2 lần)
            GlobalChangeHub.InventoryChanged += OnGlobalInventoryChanged;

            LoadData();

            FeedPetCommand = new RelayCommand(ExecuteFeed, _ => CanFeed());
            IncreaseQuantityCommand = new RelayCommand(_ => FeedingQuantity++,
                _ => SelectedFood != null && FeedingQuantity < SelectedFood.Quantity);
            DecreaseQuantityCommand = new RelayCommand(_ => FeedingQuantity--,
                _ => SelectedFood != null && FeedingQuantity > 1);

            OpenChangePetCommand = new RelayCommand(ExecuteOpenChangePet);
            CloseChangePetCommand = new RelayCommand(_ => IsChangePetVisible = false);
            SelectPetTypeCommand = new RelayCommand(ExecuteSelectPetType);
        }

        private void OnGlobalInventoryChanged(object sender)
        {
            if (ReferenceEquals(sender, this)) return;
            ReloadInventoryPreserveSelection();
        }

        public void LoadData()
        {
            if (_currentUser == null) return;

            try
            {
                // đọc pet
                CurrentPet = _context.Pets.FirstOrDefault(p => p.UserID == _currentUser.UserID);

                if (CurrentPet == null)
                {
                    CurrentPet = new Pet
                    {
                        UserID = _currentUser.UserID,
                        Name = "Pet Mới",
                        PetTypeID = 1,
                        Level = 1,
                        Experience = 0,
                        LastFedDate = DateTime.Now
                    };
                    _context.Pets.Add(CurrentPet);
                    _context.SaveChanges();
                }

                UpdatePetStatus();
                ReloadInventoryPreserveSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu Pet: " + ex.Message);
            }
        }

        private void ReloadInventoryPreserveSelection()
        {
            int? selectedFoodId = SelectedFood?.FoodID;

            _context.ChangeTracker.Clear();

            var foods = _context.UserFoods.AsNoTracking()
                .Include(uf => uf.Food)
                .Where(uf => uf.UserID == _currentUser.UserID && uf.Quantity > 0)
                .ToList();

            UserFoods.Clear();
            foreach (var entity in foods)
                UserFoods.Add(new UserFoodItemViewModel(entity));

            if (selectedFoodId.HasValue)
                SelectedFood = UserFoods.FirstOrDefault(x => x.FoodID == selectedFoodId.Value);
        }

        private void UpdatePetStatus()
        {
            if (CurrentPet == null) return;

            var allLevels = _context.PetTypes.AsNoTracking()
                .Where(pt => pt.PetTypeID == CurrentPet.PetTypeID)
                .OrderBy(pt => pt.Level)
                .ToList();

            var currentLvlInfo = allLevels.FirstOrDefault(x => x.Level == CurrentPet.Level);
            var nextLvlInfo = allLevels.FirstOrDefault(x => x.Level == CurrentPet.Level + 1);

            if (currentLvlInfo != null)
            {
                if (CurrentPet.LastFedDate.HasValue)
                {
                    var elapsed = DateTime.Now - CurrentPet.LastFedDate.Value;
                    if (elapsed.TotalSeconds < 0) elapsed = TimeSpan.Zero;

                    double ratio = elapsed.TotalDays / 1.0; // 1 ngày = đói 100%
                    Hunger = (int)Math.Clamp(ratio * 100.0, 0.0, 100.0);
                }
                else
                {
                    Hunger = 100;
                }

                Happiness = 100 - Hunger;

                PetAvatar = (Hunger > 50)
                    ? currentLvlInfo.AppearanceWhenHungry
                    : currentLvlInfo.AppearanceWhenHappy;
            }

            if (nextLvlInfo != null)
            {
                MaxExp = nextLvlInfo.ExperienceRequired;
                ExpDisplay = $"{CurrentPet.Experience} / {MaxExp} XP";
            }
            else
            {
                ExpDisplay = "MAX LEVEL";
            }
        }

        private bool CanFeed()
            => SelectedFood != null && SelectedFood.Quantity >= FeedingQuantity && FeedingQuantity >= 1;

        private void ExecuteFeed(object obj)
        {
            if (!CanFeed()) return;

            var selectedVm = SelectedFood;
            int qty = FeedingQuantity;

            try
            {
                // đọc fresh từ DB để tránh lệch do shop / view khác
                _context.ChangeTracker.Clear();

                var petEntity = _context.Pets.FirstOrDefault(p => p.UserID == _currentUser.UserID);
                if (petEntity == null)
                {
                    MessageBox.Show("Không tìm thấy Pet.");
                    LoadData();
                    return;
                }

                var foodEntity = _context.UserFoods
                    .Include(uf => uf.Food)
                    .FirstOrDefault(uf => uf.UserID == _currentUser.UserID && uf.FoodID == selectedVm.FoodID);

                if (foodEntity == null || foodEntity.Quantity <= 0)
                {
                    MessageBox.Show("Món ăn không còn trong kho.");
                    ReloadInventoryPreserveSelection();
                    return;
                }

                int available = (int)foodEntity.Quantity;
                if (qty > available) qty = available;
                if (qty <= 0) return;

                int expGain = (foodEntity.Food?.ExperiencePerUnit ?? 0) * qty;

                petEntity.Experience += expGain;
                petEntity.LastFedDate = DateTime.Now;

                var allLevels = _context.PetTypes.AsNoTracking()
                    .Where(pt => pt.PetTypeID == petEntity.PetTypeID)
                    .OrderBy(pt => pt.Level)
                    .ToList();

                bool leveledUp = false;
                while (true)
                {
                    var nextLvl = allLevels.FirstOrDefault(x => x.Level == petEntity.Level + 1);
                    if (nextLvl == null || petEntity.Experience < nextLvl.ExperienceRequired) break;

                    petEntity.Experience -= nextLvl.ExperienceRequired;
                    petEntity.Level++;
                    leveledUp = true;
                }

                // trừ food
                foodEntity.Quantity = available - qty;

                if (foodEntity.Quantity > 0)
                    _context.UserFoods.Update(foodEntity);
                else
                    _context.UserFoods.Remove(foodEntity);

                _context.Pets.Update(petEntity);
                _context.SaveChanges();

                // ✅ Update UI ngay lập tức
                selectedVm.Quantity = available - qty;
                if (selectedVm.Quantity <= 0)
                {
                    UserFoods.Remove(selectedVm);
                    SelectedFood = null;
                }

                CurrentPet = petEntity;
                UpdatePetStatus();
                FeedingQuantity = 1;

                if (leveledUp)
                    MessageBox.Show($"Level Up! {CurrentPet.Name} đã đạt cấp {CurrentPet.Level}!");

                CommandManager.InvalidateRequerySuggested();

                // ✅ broadcast cho view khác
                GlobalChangeHub.RaisePetChanged(this);
                GlobalChangeHub.RaiseInventoryChanged(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cho ăn: " + ex.Message);
            }
        }

        private void ExecuteOpenChangePet(object obj)
        {
            var pets = _context.PetTypes.AsNoTracking()
                .Where(pt => pt.Level == 1)
                .ToList();

            AvailablePetTypes.Clear();
            foreach (var p in pets) AvailablePetTypes.Add(p);

            IsChangePetVisible = true;
        }

        private void ExecuteSelectPetType(object obj)
        {
            if (obj is not PetType newType) return;
            if (CurrentPet == null) return;

            try
            {
                CurrentPet.PetTypeID = newType.PetTypeID;

                _context.Pets.Update(CurrentPet);
                _context.SaveChanges();

                UpdatePetStatus();
                IsChangePetVisible = false;

                GlobalChangeHub.RaisePetChanged(this);

                MessageBox.Show("Đã đổi thú cưng thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi thú cưng: " + ex.Message);
            }
        }

        public void Dispose()
        {
            GlobalChangeHub.InventoryChanged -= OnGlobalInventoryChanged;
            _context?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
