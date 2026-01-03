using OOP_Semester.Models;
using OOP_Semester.Views;
using System; // Để dùng String
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;

namespace OOP_Semester.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly User _user;

        // --- 1. Quản lý View con ---
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // --- 2. Thông tin User ---
        public string Username
        {
            get
            {
                if (string.IsNullOrEmpty(_user.Name))
                {
                    return "No Name";
                }
                else
                {
                    return _user.Name;
                }
            }
            set
            {
                _user.Name = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        // [MỚI] Xử lý AvatarUrl
        public string AvatarUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_user.Avatar))
                {
                    return "/Images/System/DefaultAvatar.png";
                }

                // Nếu lưu trong DB là đường dẫn tương đối bắt đầu bằng '/'
                if (_user.Avatar.StartsWith("/"))
                {
                    // Chuyển thành đường dẫn tuyệt đối tới file trong thư mục thực thi
                    string relative = _user.Avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    string full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
                    if (File.Exists(full)) return full;
                    // Nếu không tồn tại file vật lý, fallback trả về relative (WPF có thể load từ resource)
                    return _user.Avatar;
                }

                return _user.Avatar;
            }

        }

        // --- 3. ViewModel con ---
        public TodayViewModel TodayVM { get; set; }
        public SettingViewModel SettingVM { get; set; }
        public FeedingViewModel FeedingVM { get; set; }
        public ShopViewModel ShopVM { get; set; }

        public NotifyViewModel NotifyVM { get; set; }
        public HabitViewModel HabitVM { get; set; }

        // Selected menu key (Today, Habits, Challenges, Pet, Shop, Statistics, Settings)
        private string _selectedMenu = "Today";
        public string SelectedMenu
        {
            get => _selectedMenu;
            set => SetProperty(ref _selectedMenu, value);
        }

        // --- 4. Command ---
        public ICommand NavigateCommand { get; }

        public HomeViewModel(User user)
        {
            _user = user;

            TodayVM = new TodayViewModel(_user);
            SettingVM = new SettingViewModel(_user);
            FeedingVM = new FeedingViewModel(_user);
            ShopVM = new ShopViewModel(_user);
            NotifyVM = new NotifyViewModel(_user);
            HabitVM = new HabitViewModel(_user);
            ShopVM.PurchaseSuccess += () =>
            {

                if (FeedingVM != null)
                {
                    FeedingVM.changeData();
                }
            };
            CurrentView = TodayVM;
            SettingVM.InfoUpdated += () =>
            {
                // Ép giao diện tải lại property AvatarUrl
                OnPropertyChanged(nameof(AvatarUrl));

                // Nếu có hiển thị tên người dùng ở HomeView thì cập nhật luôn
                OnPropertyChanged(nameof(Username));
            };

            SelectedMenu = "Today";

            NavigateCommand = new RelayCommand(obj =>
            {
                var key = obj?.ToString();
                switch (key) // Thêm ? để tránh null crash
                {
                    case "Today":
                        CurrentView = TodayVM; SelectedMenu = "Today"; break;
                    case "Settings":
                        {
                            CurrentView = SettingVM;
                            SelectedMenu = "Settings";
                            break;
                        }
                    case "Pet":
                        CurrentView = FeedingVM; SelectedMenu = "Pet"; break;
                    case "Shop":
                        CurrentView = ShopVM; 
                        SelectedMenu = "Shop";
                        break;
                    case "Notify":
                        CurrentView = NotifyVM;
                        SelectedMenu = "Notify";
                        break;
                    case "Habits":
                        CurrentView = HabitVM; SelectedMenu = "Habits"; break;
                    default:
                        SelectedMenu = key ?? SelectedMenu; break;

                }
            });
        }
    }
}
