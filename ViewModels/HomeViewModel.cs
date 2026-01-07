using OOP_Semester.Models;
using OOP_Semester.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using static OOP_Semester.ViewModels.GlobalChangeHub;

namespace OOP_Semester.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly User _user;
        private string _activeTab;
        public string ActiveTab
        {
            get => _activeTab;
            set => SetProperty(ref _activeTab, value);
        }
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public string Username
        {
            get => string.IsNullOrEmpty(_user.Name) ? "No Name" : _user.Name;
            set
            {
                _user.Name = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string AvatarUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_user.Avatar)) return "/Images/System/DefaultAvatar.png";

                if (_user.Avatar.StartsWith("/"))
                {
                    string relative = _user.Avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    string full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
                    if (File.Exists(full)) return full;
                    return _user.Avatar;
                }
                return _user.Avatar;
            }
        }

        public TodayViewModel TodayVM { get; set; }
        public SettingViewModel SettingVM { get; set; }
        public FeedingViewModel FeedingVM { get; set; }
        public ShopViewModel ShopVM { get; set; }
        public NotifyViewModel NotifyVM { get; set; }
        public HabitViewModel HabitVM { get; set; }
        public ChallengeViewModel ChallengeVM { get; set; }
        public AdminViewModel AdminVM { get; set; }
        public StatisticViewModel StatisticVM { get; set; }
        private string _selectedMenu = "Today";
        public string SelectedMenu
        {
            get => _selectedMenu;
            set => SetProperty(ref _selectedMenu, value);
        }

        public ICommand NavigateCommand { get; }
 
        public HomeViewModel(User user)
        {
            _user = user;

            // 1. Khởi tạo toàn bộ ViewModels một lần duy nhất
            InitializeViewModels();

            // 2. Thiết lập Hub: Khi nhận yêu cầu điều hướng, gọi hàm thực thi chung
            NavigationHub.RequestNavigateCommand = new RelayCommand<string>(tag => ExecuteNavigation(tag));

            // 3. NavigateCommand cũng gọi chung một hàm thực thi
            NavigateCommand = new RelayCommand(obj => ExecuteNavigation(obj?.ToString()));

            // Thiết lập trạng thái ban đầu
            ExecuteNavigation("Today");

            // Đăng ký sự kiện cập nhật Header
            GlobalChangeHub.AvatarChanged += (s, e) => OnPropertyChanged(nameof(AvatarUrl));
            GlobalChangeHub.DisplayNameChanged += (s, e) => OnPropertyChanged(nameof(Username));
        }

        private void ExecuteNavigation(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            // Cập nhật thuộc tính để Menu XAML sáng đèn
            ActiveTab = tag;
            SelectedMenu = tag;

            // Chuyển đổi View dựa trên các Instance đã khởi tạo
            switch (tag)
            {
                case "Today":
                    CurrentView = TodayVM;
                    TodayVM.Reload(); // Làm mới dữ liệu thói quen mỗi lần quay lại
                    break;
                case "Challenges":
                case "Challenge": // Hỗ trợ cả 2 tag từ Hub hoặc Menu
                    CurrentView = ChallengeVM;
                    ActiveTab = "Challenges"; // Ép Menu sáng đúng mục Challenges
                    break;
                case "Habits":
                    CurrentView = HabitVM;
                    break;
                case "Pet":
                    CurrentView = FeedingVM;
                    FeedingVM.LoadData();
                    break;
                case "Shop":
                    CurrentView = ShopVM;
                    ShopVM.LoadData();
                    break;
                case "Statistics":
                    CurrentView = StatisticVM;
                    break;
                case "Administrator":
                    if (IsAdmin) CurrentView = AdminVM;
                    break;
                case "Settings":
                    CurrentView = SettingVM;
                    break;
                case "Notify":
                    CurrentView = NotifyVM;
                    break;
            }
        }

        private void InitializeViewModels()
        {
            IsAdmin = _user.Account == "admin" || _user.Account == "root";
            TodayVM = new TodayViewModel(_user);
            HabitVM = new HabitViewModel(_user);
            ChallengeVM = new ChallengeViewModel(_user);
            FeedingVM = new FeedingViewModel(_user);
            ShopVM = new ShopViewModel(_user);
            StatisticVM = new StatisticViewModel(_user);
            NotifyVM = new NotifyViewModel(_user);
            SettingVM = new SettingViewModel(_user);
            if (IsAdmin) AdminVM = new AdminViewModel(_user);
        }
    }
}
