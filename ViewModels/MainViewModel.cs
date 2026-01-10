using OOP_Semester.Repositories;
using OOP_Semester.Data;
using OOP_Semester.Models;

namespace OOP_Semester.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // View hiện tại đang hiển thị
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainViewModel()
        {
            // 1. Khởi tạo Database Context
            var context = new AppDbContext();

            // 2. Khởi tạo các Repository cần thiết
            IUserRepository userRepo = new UserRepository(context);
            CurrentView = new AuthViewModel(userRepo, this);
        }

        public void NavigateToHome(User user)
        {
            CurrentView = new HomeViewModel(user);
        }
    }
}