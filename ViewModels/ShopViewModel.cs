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
    public class ShopItemViewModel : INotifyPropertyChanged
    {
        private readonly Food _food;
        private readonly Action<ShopItemViewModel> _buyAction;

        public ShopItemViewModel(Food food, Action<ShopItemViewModel> buyAction)
        {
            _food = food;
            _buyAction = buyAction;

            IncreaseQuantityCommand = new RelayCommand(_ => QuantityToBuy++);
            DecreaseQuantityCommand = new RelayCommand(_ => QuantityToBuy--);
            BuyCommand = new RelayCommand(_ => _buyAction(this), _ => IsAffordable);
        }

        public int FoodID => _food.FoodID;
        public string Name => _food.Name ?? "Unknown";
        public string Description => _food.Description ?? "";
        public int Price => _food.Price;
        public int ExpGain => _food.ExperiencePerUnit;
        public string ImagePath => _food.ImagePath;
        public bool IsBestSeller => _food.ExperiencePerUnit >= 50;

        private int _quantityToBuy = 1;
        public int QuantityToBuy
        {
            get => _quantityToBuy;
            set
            {
                if (value < 1) value = 1;
                if (value > 99) value = 99;
                _quantityToBuy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public int TotalPrice => Price * QuantityToBuy;

        private bool _isAffordable = true;
        public bool IsAffordable
        {
            get => _isAffordable;
            set { _isAffordable = value; OnPropertyChanged(); }
        }

        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand BuyCommand { get; }

        public void UpdateAffordability(int userGold)
        {
            try
            {
                bool canBuy = userGold >= TotalPrice;
                if (IsAffordable != canBuy)
                {
                    IsAffordable = canBuy;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ShopViewModel : ViewModelBase
    {
        public event Action? PurchaseSuccess;
        private readonly AppDbContext _context;

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); OnPropertyChanged(nameof(UserGold)); }
        }

        public int UserGold
        {
            get => _currentUser?.GoldAmount ?? 0;
            set
            {
                if (_currentUser != null && _currentUser.GoldAmount != value)
                {
                    _currentUser.GoldAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        private Pet _currentPet;
        public Pet CurrentPet
        {
            get => _currentPet;
            set { _currentPet = value; OnPropertyChanged(); }
        }

        private int _maxExp;
        public int MaxExp
        {
            get => _maxExp;
            set { _maxExp = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ShopItemViewModel> Products { get; set; }

        public ShopViewModel(User user)
        {
            try
            {
                if (user == null) throw new Exception("User data is null.");

                _currentUser = user;
                Products = new ObservableCollection<ShopItemViewModel>();

                _context = new AppDbContext();
                if (!_context.Database.CanConnect()) throw new Exception("Cannot connect to Database.");

                // ✅ nghe coins global để UI shop tự update khi Today/Challenge cộng coin
                GlobalChangeHub.CoinsChanged += OnGlobalCoinsChanged;

                LoadData();
                GlobalChangeHub.GoldChanged += OnGlobalGoldChanged;
                GlobalChangeHub.PetChanged += OnGlobalPetChanged;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Shop: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnGlobalGoldChanged(object sender, int newGold)
        {
            if (ReferenceEquals(sender, this)) return;

            // Wrapper UserGold tự OnPropertyChanged + UpdateAffordability
            UserGold = newGold;
            RefreshAffordability();
        }

        private void OnGlobalPetChanged(object sender)
        {
            if (ReferenceEquals(sender, this)) return;
            ReloadPetInfo();
        }

        private void ReloadPetInfo()
        {
            // IMPORTANT: Shop dùng _context sống lâu nên phải Clear tracking để query fresh
            _context.ChangeTracker.Clear();

            CurrentPet = _context.Pets.FirstOrDefault(p => p.UserID == _currentUser.UserID);
            if (CurrentPet != null)
            {
                var nextLevel = _context.PetTypes
                    .FirstOrDefault(pt => pt.PetTypeID == CurrentPet.PetTypeID && pt.Level == CurrentPet.Level + 1);

                MaxExp = nextLevel?.ExperienceRequired ?? (CurrentPet.Experience > 0 ? CurrentPet.Experience : 100);
            }

            OnPropertyChanged(nameof(CurrentPet));
            OnPropertyChanged(nameof(MaxExp));
        }

        private void OnGlobalCoinsChanged(object sender, int newCoins)
        {
            if (ReferenceEquals(sender, this)) return;

            // update UI + affordability
            UserGold = newCoins;
            RefreshAffordability();
        }

        public void LoadData()
        {
            try
            {
                CurrentPet = _context.Pets.FirstOrDefault(p => p.UserID == _currentUser.UserID);
                if (CurrentPet != null)
                {
                    var nextLevel = _context.PetTypes.FirstOrDefault(pt => pt.PetTypeID == CurrentPet.PetTypeID && pt.Level == CurrentPet.Level + 1);
                    MaxExp = nextLevel?.ExperienceRequired ?? (CurrentPet.Experience > 0 ? CurrentPet.Experience : 100);
                }

                var foods = _context.Foods.ToList();
                Products.Clear();
                foreach (var food in foods)
                {
                    Products.Add(new ShopItemViewModel(food, ExecuteBuy));
                }

                RefreshAffordability();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteBuy(ShopItemViewModel item)
        {
            try
            {
                int totalCost = item.TotalPrice;

                int currentGold = UserGold;

                if (currentGold < totalCost)
                {
                    MessageBox.Show("Bạn không đủ vàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                UserGold = currentGold - totalCost;

                var userFood = _context.UserFoods.FirstOrDefault(uf => uf.UserID == _currentUser.UserID && uf.FoodID == item.FoodID);

                if (userFood != null)
                {
                    userFood.Quantity += item.QuantityToBuy;
                    _context.UserFoods.Update(userFood);
                }
                else
                {
                    _context.UserFoods.Add(new UserFood
                    {
                        UserID = _currentUser.UserID,
                        FoodID = item.FoodID,
                        Quantity = item.QuantityToBuy
                    });
                }

                _context.GoldTransactions.Add(new GoldTransaction
                {
                    UserID = _currentUser.UserID,
                    Amount = -totalCost,
                    TransactionDate = DateTime.Now,
                    Source = "Cửa hàng",
                    Note = $"Mua {item.QuantityToBuy} x {item.Name}"
                });

                _context.Users.Update(_currentUser);
                _context.SaveChanges();

                item.QuantityToBuy = 1;
                RefreshAffordability();

                // ✅ broadcast coin + inventory
                GlobalChangeHub.RaiseCoinsChanged(this, UserGold);
                GlobalChangeHub.RaiseInventoryChanged(this);

                MessageBox.Show($"Mua thành công {item.Name}!", "Shop HabitPet");
                PurchaseSuccess?.Invoke(); // giữ nguyên
            }
            catch (Exception ex)
            {
                try { _context.Entry(_currentUser).Reload(); OnPropertyChanged(nameof(UserGold)); } catch { }
                MessageBox.Show($"Lỗi giao dịch: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshAffordability()
        {
            try
            {
                int gold = UserGold;
                foreach (var prod in Products)
                {
                    prod.UpdateAffordability(gold);
                }
            }
            catch { }
        }
    }
}
