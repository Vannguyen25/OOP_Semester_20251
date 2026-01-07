using OOP_Semester.Data;
using OOP_Semester.Models;
using OOP_Semester.ViewModels.Report;
using OOP_Semester.Views.Report;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace OOP_Semester.ViewModels
{
    public class DayModel : ViewModelBase
    {
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday => Date.Date == DateTime.Today;
    }
    public class HabitHistoryViewModel : ViewModelBase
    {
        public Habit Habit { get; set; }
        public ObservableCollection<DayModel> DaysInMonth { get; set; } = new();
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public string MonthYearTitle { get; set; }
        public ICommand OpenHabitHistoryCommand => new RelayCommand<DailyHabitLogVM>(item =>
        {
            if (item?.Habit == null) return;

            // Khởi tạo cửa sổ lịch sử với HabitID tương ứng
            var historyWin = new HabitHistoryWindow(item.Habit.HabitID);

            // Thiết lập cửa sổ chính làm chủ để hiện ở giữa app
            historyWin.Owner = Application.Current.MainWindow;
            historyWin.ShowDialog();
        });
        public HabitHistoryViewModel(int habitId)
        {
            using var db = new AppDbContext();
            Habit = db.Habits.FirstOrDefault(h => h.HabitID == habitId);
            MonthYearTitle = $"Tháng {DateTime.Now:MM/yyyy}";

            // Lấy danh sách ngày hoàn thành
            var completedDates = db.HabitLogs
                .Where(l => l.HabitID == habitId && l.Completed)
                .Select(l => l.LogDate.Date)
                .ToList();

            CalculateStreaks(completedDates);
            GenerateCalendar(DateTime.Now, completedDates);
        }

        private void CalculateStreaks(List<DateTime> dates)
        {
            if (!dates.Any()) return;
            var sorted = dates.Distinct().OrderByDescending(d => d).ToList();

            // Current Streak
            int current = 0;
            DateTime check = DateTime.Today;
            if (!sorted.Contains(check)) check = check.AddDays(-1);
            foreach (var d in sorted)
            {
                if (d.Date == check.Date) { current++; check = check.AddDays(-1); }
                else break;
            }
            CurrentStreak = current;

            // Best Streak
            int best = 0, temp = 0;
            var asc = sorted.OrderBy(d => d).ToList();
            for (int i = 0; i < asc.Count; i++)
            {
                if (i > 0 && (asc[i] - asc[i - 1]).TotalDays == 1) temp++;
                else temp = 1;
                if (temp > best) best = temp;
            }
            BestStreak = best;
        }

        private void GenerateCalendar(DateTime targetMonth, List<DateTime> completedDates)
        {
            DaysInMonth.Clear();
            DateTime first = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            int offset = ((int)first.DayOfWeek + 6) % 7; // Thứ 2 là đầu tuần
            DateTime start = first.AddDays(-offset);

            for (int i = 0; i < 42; i++)
            {
                DateTime d = start.AddDays(i);
                DaysInMonth.Add(new DayModel
                {
                    Date = d,
                    IsCompleted = completedDates.Contains(d.Date),
                    IsCurrentMonth = d.Month == targetMonth.Month
                });
            }
        }
    }
}