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
    public class FeedingViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private readonly User _currentUser;

        // --- PROPERTIES ---
        private Pet _currentPet;
        public Pet CurrentPet { get => _currentPet; set { _currentPet = value; OnPropertyChanged(); } }

        private string _petAvatar;
        public string PetAvatar { get => _petAvatar; set { _petAvatar = value; OnPropertyChanged(); } }

        private int _maxExp = 100;
        public int MaxExp { get => _maxExp; set { _maxExp = value; OnPropertyChanged(); } }

        // Thêm property này để hiển thị text thay vì số khi Max Level
        private string _expDisplay;
        public string ExpDisplay
        {
            get => _expDisplay;
            set { _expDisplay = value; OnPropertyChanged(); }
        }

        private int _hunger;
        public int Hunger { get => _hunger; set { _hunger = value; OnPropertyChanged(); } }

        private int _happiness;
        public int Happiness { get => _happiness; set { _happiness = value; OnPropertyChanged(); } }

        public ObservableCollection<UserFood> UserFoods { get; set; }

        private UserFood _selectedFood;
        public UserFood SelectedFood
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
                    if (value > SelectedFood.Quantity) value = (int)SelectedFood.Quantity;
                    if (value < 1) value = 1;
                }
                _feedingQuantity = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _isChangePetVisible;
        public bool IsChangePetVisible { get => _isChangePetVisible; set { _isChangePetVisible = value; OnPropertyChanged(); } }
        public ObservableCollection<PetType> AvailablePetTypes { get; set; }

        // --- COMMANDS ---
        public ICommand FeedPetCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand OpenChangePetCommand { get; }
        public ICommand CloseChangePetCommand { get; }
        public ICommand SelectPetTypeCommand { get; }

        // --- CONSTRUCTOR ---
        public FeedingViewModel(User user)
        {
            _context = new AppDbContext();
            _currentUser = user;
            UserFoods = new ObservableCollection<UserFood>();
            AvailablePetTypes = new ObservableCollection<PetType>();

            LoadData();

            FeedPetCommand = new RelayCommand(ExecuteFeed, CanFeed);
            IncreaseQuantityCommand = new RelayCommand(_ => FeedingQuantity++);
            DecreaseQuantityCommand = new RelayCommand(_ => FeedingQuantity--);
            OpenChangePetCommand = new RelayCommand(ExecuteOpenChangePet);
            CloseChangePetCommand = new RelayCommand(_ => IsChangePetVisible = false);
            SelectPetTypeCommand = new RelayCommand(ExecuteSelectPetType);
        }

        public void LoadData()
        {
            // 1. Kiểm tra an toàn: Nếu user null thì dừng ngay, không chạy LINQ
            if (_currentUser == null) return;

            try
            {
                // 2. Load Pet từ Database
                CurrentPet = _context.Pets.FirstOrDefault(p => p.UserID == _currentUser.UserID);

                // Nếu chưa có Pet -> Tạo Pet mặc định (Tránh lỗi Null khi tính toán sau này)
                if (CurrentPet == null)
                {
                    CurrentPet = new Pet
                    {
                        UserID = _currentUser.UserID,
                        Name = "Pet Mới",
                        PetTypeID = 1, // Đảm bảo ID này có trong bảng pettype
                        Level = 1,
                        Experience = 0,
                        LastFedDate = DateTime.Now
                    };
                    _context.Pets.Add(CurrentPet);
                    _context.SaveChanges();
                }

                // 3. Cập nhật trạng thái (Hình ảnh, EXP...)
                UpdatePetStatus();

                // 4. Load kho đồ ăn (Refresh lại số lượng sau khi mua Shop)
                ReloadInventory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu Pet: " + ex.Message);
            }
        }
        // CÁCH MỚI (Đúng)
        public void changeData()
        {
            // 1. Phải lấy dữ liệu mới nhất từ Database (Vì Shop vừa lưu vào đó)
            var newData = _context.UserFoods
                .AsNoTracking()
                .Include(uf => uf.Food) // Nhớ Include để lấy tên/ảnh món ăn
                .Where(uf => uf.UserID == _currentUser.UserID && uf.Quantity > 0)
                .ToList();

            // 2. Xóa sạch dữ liệu cũ trên giao diện
            UserFoods.Clear();

            // 3. Đổ dữ liệu mới vào
            foreach (var item in newData)
            {
                UserFoods.Add(item);
            }

        }

        private void ReloadInventory()
        {
            var foods = _context.UserFoods.Include(uf => uf.Food)
                .Where(uf => uf.UserID == _currentUser.UserID && uf.Quantity > 0).ToList();
            UserFoods.Clear();
            foreach (var item in foods) UserFoods.Add(item);
        }

        // --- LOGIC QUAN TRỌNG: CẬP NHẬT TRẠNG THÁI ---
        private void UpdatePetStatus()
        {
            if (CurrentPet == null) return;

            // Load dữ liệu Type để tính toán
            // Sử dụng AsNoTracking để đảm bảo dữ liệu mới nhất và không bị cache sai
            var allLevels = _context.PetTypes.AsNoTracking()
                .Where(pt => pt.PetTypeID == CurrentPet.PetTypeID)
                .OrderBy(pt => pt.Level).ToList();

            var currentLvlInfo = allLevels.FirstOrDefault(x => x.Level == CurrentPet.Level);
            var nextLvlInfo = allLevels.FirstOrDefault(x => x.Level == CurrentPet.Level + 1);

            // 1. Cập nhật Avatar và Đói/Vui
            if (currentLvlInfo != null)
            {
                if (CurrentPet.LastFedDate.HasValue)
                {
                    double mins = (DateTime.Now - CurrentPet.LastFedDate.Value).TotalMinutes;
                    Hunger = (int)Math.Min(100, (mins / 1440.0) * 100);
                }
                else Hunger = 100;

                Happiness = 100 - Hunger;
                PetAvatar = (Hunger > 50) ? currentLvlInfo.AppearanceWhenHungry : currentLvlInfo.AppearanceWhenHappy;
            }

            // 2. Cập nhật MaxExp chuẩn xác
            if (nextLvlInfo != null)
            {
                // Có cấp tiếp theo -> Lấy ExpRequired của cấp đó làm mốc
                MaxExp = nextLvlInfo.ExperienceRequired;
                ExpDisplay = $"{CurrentPet.Experience} / {MaxExp} XP";
            }
            else
            {
                // Đã Max cấp -> Hiển thị Full
                MaxExp = 100;
                CurrentPet.Experience = 100; // Force hiển thị full thanh
                ExpDisplay = "MAX LEVEL";
            }
        }

        private bool CanFeed(object obj) => SelectedFood != null && SelectedFood.Quantity >= FeedingQuantity;

        private void ExecuteFeed(object obj)
        {
            if (!CanFeed(null)) return;

            // [FIX CRASH] Tách biệt logic xử lý data và giao diện
            var foodItem = SelectedFood;
            int qty = FeedingQuantity;

            // 1. Tính toán Exp cộng thêm
            int expGain = (foodItem.Food?.ExperiencePerUnit ?? 0) * qty;
            CurrentPet.Experience += expGain;
            CurrentPet.LastFedDate = DateTime.Now;

            // 2. Logic Lên Cấp (Vòng lặp)
            var allLevels = _context.PetTypes.AsNoTracking()
                .Where(pt => pt.PetTypeID == CurrentPet.PetTypeID)
                .OrderBy(pt => pt.Level).ToList();

            bool leveledUp = false;
            while (true)
            {
                // Tìm thông tin level TIẾP THEO
                var nextLvl = allLevels.FirstOrDefault(x => x.Level == CurrentPet.Level + 1);

                // Nếu không có level tiếp theo (Max) hoặc chưa đủ Exp -> Dừng
                if (nextLvl == null || CurrentPet.Experience < nextLvl.ExperienceRequired) break;

                // Lên cấp: Trừ Exp đã dùng và tăng Level
                CurrentPet.Experience -= nextLvl.ExperienceRequired;
                CurrentPet.Level++;
                leveledUp = true;
            }

            if (leveledUp) MessageBox.Show($"Level Up! {CurrentPet.Name} đã đạt cấp {CurrentPet.Level}!");

            // 3. Cập nhật số lượng tồn kho (DB)
            foodItem.Quantity -= qty;

            // 4. LƯU XUỐNG DB TRƯỚC (Quan trọng)
            try
            {
                if (foodItem.Quantity > 0)
                    _context.UserFoods.Update(foodItem);
                else
                    _context.UserFoods.Remove(foodItem); // Xóa khỏi DB nếu hết

                _context.Pets.Update(CurrentPet);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
                return;
            }

            // 5. CẬP NHẬT GIAO DIỆN SAU CÙNG (Để tránh crash)
            if (foodItem.Quantity <= 0)
            {
                UserFoods.Remove(foodItem); // Xóa khỏi ListBox
                SelectedFood = null;        // Reset chọn
            }

            UpdatePetStatus();
            FeedingQuantity = 1;
            CommandManager.InvalidateRequerySuggested();
        }

        // --- Logic Đổi Pet giữ nguyên ---
        private void ExecuteOpenChangePet(object obj)
        {
            var pets = _context.PetTypes.AsNoTracking().Where(pt => pt.Level == CurrentPet.Level).ToList();
            AvailablePetTypes.Clear();
            foreach (var p in pets) AvailablePetTypes.Add(p);
            IsChangePetVisible = true;
        }

        private void ExecuteSelectPetType(object obj)
        {
            if (obj is PetType newType)
            {
                CurrentPet.PetTypeID = newType.PetTypeID;
                _context.Pets.Update(CurrentPet);
                _context.SaveChanges();
                UpdatePetStatus();
                IsChangePetVisible = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}