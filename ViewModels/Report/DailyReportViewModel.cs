using Microsoft.EntityFrameworkCore;
using OOP_Semester.Data;
using OOP_Semester.Models;
using OOP_Semester.Views.Report;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
namespace OOP_Semester.ViewModels.Report
{
    public class DailyHabitLogVM : ViewModelBase
    {

        public HabitLog Log { get; set; }
        public Habit Habit { get; set; }

        // Sửa lỗi Crash (image_1e41e8): Thêm set để hỗ trợ binding
        public double ProgressPercent
        {
            get
            {
                // Kiểm tra null hoặc bằng 0 để tránh lỗi chia cho 0
                if (Habit.GoalValuePerDay == null || Habit.GoalValuePerDay == 0)
                    return Log.Completed ? 100 : 0;

                // Ép kiểu cả hai về double để thực hiện phép tính
                double quantity = Convert.ToDouble(Log.Quantity);
                double goal = Convert.ToDouble(Habit.GoalValuePerDay);

                double val = (quantity / goal) * 100;
                return Math.Min(100, val); // Giới hạn tối đa 100%
            }
            set { OnPropertyChanged(); }
        }

        public string ProgressText => $"{Log.Quantity}/{Habit.GoalValuePerDay ?? 1} {Habit.GoalUnitType}";
        public string StatusText => Log.Completed ? "Hoàn thành" : (Log.Skipped ? "Bỏ qua" : "Đang thực hiện");
        public string StatusColor => Log.Completed ? "#10B981" : (Log.Skipped ? "#6B7280" : "#F59E0B");
        public string StatusBg => Log.Completed ? "#DCFCE7" : (Log.Skipped ? "#F3F4F6" : "#FEF3C7");
    }

    public class DailyReportViewModel : ViewModelBase
    {
        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        // Cập nhật Command để bật/tắt lớp phủ
        public ICommand OpenHabitHistoryCommand => new RelayCommand<DailyHabitLogVM>(item =>
        {
            if (item?.Habit == null) return;

            // Hiện lớp phủ mờ
            IsOverlayVisible = true;

            var historyWin = new HabitHistoryWindow(item.Habit.HabitID);
            historyWin.Owner = System.Windows.Application.Current.MainWindow;

            // ShowDialog sẽ chặn luồng cho đến khi đóng cửa sổ
            historyWin.ShowDialog();

            // Ẩn lớp phủ sau khi đóng cửa sổ
            IsOverlayVisible = false;
        });
        
        private readonly User _user;
        public ObservableCollection<DailyHabitLogVM> DailyLogs { get; set; } = new();

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate { get => _selectedDate; set { if (SetProperty(ref _selectedDate, value)) LoadDailyStats(); } }

        public int TotalHabits { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public double OverallProgress { get; set; }

        public DailyReportViewModel(User user) { _user = user; LoadDailyStats(); }

        public void LoadDailyStats()
        {
            using var db = new AppDbContext();
            var day = SelectedDate.Date;
            var dayOfWeek = day.DayOfWeek;

            // 1. Lấy tất cả Habit của User
            var habits = db.Habits
                .Where(h => h.UserID == _user.UserID &&
                            (h.Status == null || h.Status == "Active") &&
                            h.StartDate <= day &&
                            (!h.UseEndCondition || h.EndDate == null || h.EndDate >= day))
                .ToList();

            var hIds = habits.Select(h => h.HabitID).ToList();

            // 2. Truy vấn bảng RepeatDays riêng biệt để đối chiếu
            var repeatSettings = db.RepeatDays
                .Where(r => hIds.Contains(r.HabitID))
                .ToList();

            // 3. Lấy Logs
            var logs = db.HabitLogs
                .Where(l => hIds.Contains(l.HabitID) && l.LogDate == day)
                .ToList();

            DailyLogs.Clear();
            foreach (var h in habits)
            {
                // Kiểm tra lịch lặp từ danh sách vừa lấy
                var r = repeatSettings.FirstOrDefault(x => x.HabitID == h.HabitID);

                bool isScheduled = h.RepeatEveryday || (r != null && dayOfWeek switch
                {
                    DayOfWeek.Monday => r.Mon,
                    DayOfWeek.Tuesday => r.Tue,
                    DayOfWeek.Wednesday => r.Wed,
                    DayOfWeek.Thursday => r.Thu,
                    DayOfWeek.Friday => r.Fri,
                    DayOfWeek.Saturday => r.Sat,
                    DayOfWeek.Sunday => r.Sun,
                    _ => false
                });

                if (isScheduled)
                {
                    var log = logs.FirstOrDefault(l => l.HabitID == h.HabitID)
                              ?? new HabitLog { HabitID = h.HabitID, Quantity = 0, Completed = false, LogDate = day };
                    DailyLogs.Add(new DailyHabitLogVM { Log = log, Habit = h });
                }
            }

            // Tính toán thống kê
            TotalHabits = DailyLogs.Count;
            CompletedCount = DailyLogs.Count(x => x.Log.Completed);
            PendingCount = DailyLogs.Count(x => !x.Log.Completed && !x.Log.Skipped);
            OverallProgress = TotalHabits > 0 ? DailyLogs.Average(x => x.ProgressPercent) : 0;

            OnPropertyChanged(nameof(TotalHabits));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(OverallProgress));
        }

        private bool IsScheduled(Habit h, DateTime d)
        {
            if (h.RepeatEveryday) return true;
            if (h.RepeatDay == null) return false;

            return d.DayOfWeek switch
            {
                DayOfWeek.Monday => h.RepeatDay.Mon,
                DayOfWeek.Tuesday => h.RepeatDay.Tue,
                DayOfWeek.Wednesday => h.RepeatDay.Wed,
                DayOfWeek.Thursday => h.RepeatDay.Thu,
                DayOfWeek.Friday => h.RepeatDay.Fri,
                DayOfWeek.Saturday => h.RepeatDay.Sat,
                DayOfWeek.Sunday => h.RepeatDay.Sun,
                _ => false
            };
        }
    }
}