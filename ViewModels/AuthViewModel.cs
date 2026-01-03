using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using OOP_Semester.Data; // Namespace chứa AppDbContext
using OOP_Semester.Models;
using OOP_Semester.Repositories;

namespace OOP_Semester.ViewModels
{
    public class AuthViewModel : ViewModelBase
    {
        private readonly IUserRepository _userRepo;
        private readonly MainViewModel _mainViewModel;

        // ==========================================================
        // 1. PROPERTIES & BINDING
        // ==========================================================

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private bool _isRegisterMode;
        public bool IsRegisterMode
        {
            get => _isRegisterMode;
            set
            {
                if (SetProperty(ref _isRegisterMode, value))
                {
                    OnPropertyChanged(nameof(HeaderTitle));
                    OnPropertyChanged(nameof(SubmitButtonText));
                }
            }
        }

        public string HeaderTitle => IsRegisterMode ? "Tạo tài khoản mới" : "Xin chào! 👋";
        public string SubmitButtonText => IsRegisterMode ? "Đăng ký" : "Đăng nhập";

        // ==========================================================
        // 2. COMMANDS
        // ==========================================================
        public ICommand LoginTabCommand { get; }
        public ICommand RegisterTabCommand { get; }

        public AuthViewModel(IUserRepository userRepo, MainViewModel mainViewModel)
        {
            _userRepo = userRepo;
            _mainViewModel = mainViewModel;

            // Chuyển tab Login/Register
            LoginTabCommand = new RelayCommand(o => IsRegisterMode = false);
            RegisterTabCommand = new RelayCommand(o => IsRegisterMode = true);
        }

        // ==========================================================
        // 3. XỬ LÝ ĐĂNG NHẬP (LOGIN)
        // ==========================================================
        public void HandleSubmit(string password)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập tài khoản và mật khẩu!");
                return;
            }

            try
            {
                var user = _userRepo.Login(Username, password);

                if (user != null)
                {
                    _mainViewModel.NavigateToHome(user);
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        // ==========================================================
        // 4. XỬ LÝ ĐĂNG KÝ (REGISTER) - ĐÃ CẬP NHẬT
        // ==========================================================
        public void HandleSubmit(string password, string confirmPass)
        {
            if (password != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin đăng ký!");
                return;
            }

            try
            {
                // 1. Tạo đối tượng User
                var newUser = new User
                {
                    Account = Username,
                    Password = password,
                    Name = Username,
                    Role = UserRole.User,
                    CreatedAt = DateTime.Now,

                    // --- CẬP NHẬT: Tặng 100 vàng khởi điểm ---
                    GoldAmount = 100,

                    VacationMode = false,
                    Avatar = "Images\\System\\DefaultAvatar.png", // Đảm bảo đường dẫn này tồn tại hoặc sửa lại
                    MorningTime = new TimeSpan(7, 0, 0),
                    AfternoonTime = new TimeSpan(14, 0, 0),
                    EveningTime = new TimeSpan(19, 0, 0)
                };

                // 2. Gọi Repository để lưu User
                bool isSuccess = _userRepo.Register(newUser);

                if (isSuccess)
                {
                    // --- CẬP NHẬT: Khởi tạo Pet và Thức ăn ---
                    InitializeNewUserData(newUser.Account);

                    MessageBox.Show("Đăng ký thành công! Bạn nhận được 100 Gold và bộ quà tặng tân thủ.");
                    IsRegisterMode = false; // Chuyển về tab đăng nhập
                }
                else
                {
                    MessageBox.Show("Tài khoản đã tồn tại hoặc lỗi tạo user.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message);
            }
        }

        // ==========================================================
        // 5. HÀM KHỞI TẠO DỮ LIỆU TÂN THỦ (Helper Method)
        // ==========================================================
        private void InitializeNewUserData(string accountName)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Bước 1: Tìm lại UserID vừa tạo trong DB
                    var user = context.Users.FirstOrDefault(u => u.Account == accountName);

                    if (user != null)
                    {
                        // --- A. TẠO PET MẶC ĐỊNH ---
                        var defaultPet = new Pet
                        {
                            UserID = user.UserID,
                            PetTypeID = 1,       // ID 1: Giả sử là Pitbull/Chó mặc định
                            Name = "Pitbull",
                            Level = 1,
                            Experience = 0,
                            Status = "Happy",
                            LastFedDate = DateTime.Now, // Ăn no ngay khi tạo
                            CreatedAt = DateTime.Now
                        };
                        context.Pets.Add(defaultPet);

                        // --- B. TẠO THỨC ĂN TÂN THỦ (ID 1 -> 4) ---
                        for (int i = 1; i <= 4; i++)
                        {
                            var starterFood = new UserFood
                            {
                                UserID = user.UserID,
                                FoodID = i,    // ID thức ăn từ 1 đến 4
                                Quantity = 1   // Tặng mỗi loại 1 cái
                            };
                            context.UserFoods.Add(starterFood);
                        }

                        // Bước 3: Lưu tất cả thay đổi xuống DB
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                // Không throw lỗi ra ngoài để tránh crash app, chỉ thông báo hoặc log lại
                MessageBox.Show("Cảnh báo: Tạo tài khoản thành công nhưng lỗi khởi tạo vật phẩm: " + ex.Message);
            }
        }
    }
}