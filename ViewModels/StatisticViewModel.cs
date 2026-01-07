using OOP_Semester.Models;
using System.Windows.Input;
using OOP_Semester.ViewModels.Report;

namespace OOP_Semester.ViewModels
{
    public class StatisticViewModel : ViewModelBase
    {
        private readonly User _user;
        private object _currentReportView;
        public object CurrentReportView { get => _currentReportView; set => SetProperty(ref _currentReportView, value); }

        // Thuộc tính kiểm tra quyền Admin
        public bool IsAdmin
        {
            get
            {
                // So sánh trực tiếp giá trị Enum
                return _user.Role == UserRole.Admin;
            }
        }

        public ICommand SwitchReportCommand { get; }

        public StatisticViewModel(User user)
        {
            _user = user;
            SwitchReportCommand = new RelayCommand(obj => SwitchReport(obj?.ToString()));
            SwitchReport("Daily");
        }

        private void SwitchReport(string tag)
        {
            CurrentReportView = tag switch
            {
                "Daily" => new DailyReportViewModel(_user),
                "Gold" => new GoldTransactionReportViewModel(_user),
                // Chỉ khởi tạo View Admin nếu user có quyền
                "ChallengeAdmin" when IsAdmin => new AdChallengeReportViewModel(),
                "UserAdmin" when IsAdmin => new AdUserReportViewModel(),
                _ => CurrentReportView
            };
        }
    }
}