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
    // =========================================================================
    // 1. CLASS PHỤ: SHOP ITEM (Giữ nguyên logic, chỉ thêm an toàn)
    // =========================================================================
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
            catch { /* Bỏ qua lỗi nhỏ ở giao diện */ }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // =========================================================================
    // 2. CLASS CHÍNH: SHOP VIEW MODEL (Đã thêm Try-Catch hiển thị lỗi)
    // =========================================================================
    public class ShopViewModel : ViewModelBase
    {
        public event Action? PurchaseSuccess;
        private readonly AppDbContext _context;

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
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

        // --- CONSTRUCTOR (Nơi dễ gây crash nhất) ---
        public ShopViewModel(User user)
        {
            // [DEBUG] Bắt lỗi ngay khi khởi tạo
            try
            {
                // 1. Kiểm tra User đầu vào
                if (user == null)
                {
                    throw new Exception("Lỗi: Dữ liệu User truyền vào Shop bị NULL. Vui lòng kiểm tra lại quá trình Đăng nhập hoặc Chuyển trang.");
                }

                _currentUser = user;
                Products = new ObservableCollection<ShopItemViewModel>();

                // 2. Thử kết nối Database
                try
                {
                    _context = new AppDbContext();
                    // Thử truy vấn nhẹ để xem DB có sống không
                    bool dbExists = _context.Database.CanConnect();
                    if (!dbExists) throw new Exception("Không thể kết nối đến Database MySQL.");
                }
                catch (Exception dbEx)
                {
                    throw new Exception($"Lỗi kết nối Database: {dbEx.Message}. Hãy kiểm tra ConnectionString trong AppDbContext.");
                }

                // 3. Load dữ liệu
                LoadData();
            }
            catch (Exception ex)
            {
                // Hiện MessageBox thay vì tắt ứng dụng
                MessageBox.Show(
                    $"Đã xảy ra lỗi nghiêm trọng khi mở Cửa hàng:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Lỗi Khởi Tạo Shop",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                // 1. Load Pet
                CurrentPet = _context.Pets.FirstOrDefault(p => p.UserID == _currentUser.UserID);

                if (CurrentPet != null)
                {
                    var nextLevel = _context.PetTypes
                        .FirstOrDefault(pt => pt.PetTypeID == CurrentPet.PetTypeID && pt.Level == CurrentPet.Level + 1);
                    MaxExp = nextLevel?.ExperienceRequired ?? (CurrentPet.Experience > 0 ? CurrentPet.Experience : 100);
                }
                else
                {
                    // Cảnh báo nhẹ nếu không thấy Pet
                    // MessageBox.Show("User này chưa có Pet nào. Một số chức năng hiển thị Level sẽ bị ẩn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // 2. Load Foods
                // [DEBUG] Kiểm tra xem bảng Food có dữ liệu không
                var foods = _context.Foods.ToList();

                if (foods == null || foods.Count == 0)
                {
                    MessageBox.Show("Cảnh báo: Bảng 'food' trong Database đang trống hoặc không tồn tại. Cửa hàng sẽ không có gì để bán.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                Products.Clear();
                foreach (var food in foods)
                {
                    var itemVm = new ShopItemViewModel(food, ExecuteBuy);
                    Products.Add(itemVm);
                }

                RefreshAffordability();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi khi tải dữ liệu sản phẩm:\n{ex.Message}\n\nChi tiết: {ex.InnerException?.Message}",
                    "Lỗi Data",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExecuteBuy(ShopItemViewModel item)
        {
            // Bọc try-catch cho hành động mua
            try
            {
                int totalCost = item.TotalPrice;
                int currentGold = _currentUser.GoldAmount ?? 0;

                if (currentGold < totalCost)
                {
                    MessageBox.Show("Bạn không đủ vàng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _currentUser.GoldAmount = currentGold - totalCost;

                var userFood = _context.UserFoods
                    .FirstOrDefault(uf => uf.UserID == _currentUser.UserID && uf.FoodID == item.FoodID);

                if (userFood != null)
                {
                    userFood.Quantity += item.QuantityToBuy;
                    _context.UserFoods.Update(userFood);
                }
                else
                {
                    var newInv = new UserFood
                    {
                        UserID = _currentUser.UserID,
                        FoodID = item.FoodID,
                        Quantity = item.QuantityToBuy
                    };
                    _context.UserFoods.Add(newInv);
                }

                var trans = new GoldTransaction
                {
                    UserID = _currentUser.UserID,
                    Amount = -totalCost
                };
                _context.GoldTransactions.Add(trans);

                _context.Users.Update(_currentUser);
                _context.SaveChanges();

                OnPropertyChanged(nameof(CurrentUser));
                item.QuantityToBuy = 1;
                RefreshAffordability();

                MessageBox.Show($"Mua thành công {item.Name}!", "Shop HabitPet");
                PurchaseSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                // Reload lại User nếu lỗi để tránh sai lệch tiền hiển thị
                try { _context.Entry(_currentUser).Reload(); OnPropertyChanged(nameof(CurrentUser)); } catch { }

                MessageBox.Show(
                    $"Lỗi giao dịch mua hàng:\n{ex.Message}\n\nInner Exception: {ex.InnerException?.Message}",
                    "Lỗi Giao Dịch",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshAffordability()
        {
            try
            {
                int gold = _currentUser.GoldAmount ?? 0;
                foreach (var prod in Products)
                {
                    prod.UpdateAffordability(gold);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi refresh giá: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}