using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using OOP_Semester.Data;
using OOP_Semester.Models;

namespace OOP_Semester.ViewModels
{
    // Class đại diện cho 1 ô ngày trên lịch
    public class MonthDayCell
    {
        public DateTime Date { get; set; }
        public string DayNumber { get; set; } = "";
        public bool IsInMonth { get; set; }     // Có thuộc tháng đang hiển thị không
        public bool IsScheduled { get; set; }   // Có lịch làm không

        // Trạng thái logic
        public bool IsCompleted { get; set; }
        public bool IsSkipped { get; set; }
        public bool IsMissed { get; set; }      // Quên làm (Quá khứ + Có lịch + Chưa làm)

        // Màu sắc hiển thị (Binding ra View)
        public Brush HighlightBackground { get; set; } = Brushes.Transparent;
        public Brush HighlightBorder { get; set; } = Brushes.Transparent;
        public Brush Foreground { get; set; } = Brushes.Black;
    }

    public class HabitMonthWindowViewModel : ViewModelBase
    {
        private readonly int _habitId;
        private Habit? _habit;
        private RepeatDay? _repeat;

        public ObservableCollection<MonthDayCell> Days { get; } = new();

        private DateTime _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        public string MonthTitle
        {
            get => $"Tháng {_displayMonth:MM/yyyy}";
        }

        public ICommand PrevMonthCommand { get; }
        public ICommand NextMonthCommand { get; }

        public HabitMonthWindowViewModel(int habitId)
        {
            _habitId = habitId;

            PrevMonthCommand = new RelayCommand(_ =>
            {
                _displayMonth = _displayMonth.AddMonths(-1);
                BuildCalendar();
            });

            NextMonthCommand = new RelayCommand(_ =>
            {
                _displayMonth = _displayMonth.AddMonths(1);
                BuildCalendar();
            });

            LoadFromDb();
            BuildCalendar();
        }

        private void LoadFromDb()
        {
            using var ctx = new AppDbContext();
            _habit = ctx.Habits.FirstOrDefault(h => h.HabitID == _habitId);
            _repeat = ctx.RepeatDays.FirstOrDefault(r => r.HabitID == _habitId);
        }

        private void BuildCalendar()
        {
            OnPropertyChanged(nameof(MonthTitle));
            Days.Clear();

            if (_habit == null) return;

            // 1. Chuẩn bị màu sắc
            // Màu Habit (Mặc định Cam nếu null)
            string habitHex = string.IsNullOrWhiteSpace(_habit.ColorHex) ? "#FEA500" : _habit.ColorHex!;
            var mainBrush = BrushFromHex(habitHex, 1.0);          // Đậm (100% opacity)
            var lightBrush = BrushFromHex(habitHex, 0.25);        // Nhạt (25% opacity)

            // Màu Skipped (Xám)
            var skippedBg = new SolidColorBrush(Color.FromRgb(243, 244, 246)); // #F3F4F6
            var skippedText = Brushes.Gray;

            // Màu Missed (Đỏ nhạt)
            var missedBg = new SolidColorBrush(Color.FromRgb(254, 226, 226)); // #FEE2E2
            var missedBorder = Brushes.Red;

            // 2. Tính toán ngày bắt đầu lưới lịch (42 ô)
            var startOfMonth = new DateTime(_displayMonth.Year, _displayMonth.Month, 1);
            var startGridDate = GetMonday(startOfMonth); // Lùi về thứ 2 gần nhất
            var endGridDate = startGridDate.AddDays(42);

            // 3. Lấy dữ liệu Log trong khoảng thời gian lưới lịch
            using var ctx = new AppDbContext();
            var logs = ctx.HabitLogs
                          .Where(l => l.HabitID == _habitId &&
                                      l.LogDate >= startGridDate &&
                                      l.LogDate <= endGridDate)
                          .ToList();

            // 4. Duyệt qua 42 ô để tạo dữ liệu
            for (int i = 0; i < 42; i++)
            {
                var d = startGridDate.AddDays(i);
                bool inMonth = d.Month == _displayMonth.Month;
                bool scheduled = inMonth && IsHabitScheduledOn(d);

                // Tìm log tương ứng
                var log = logs.FirstOrDefault(l => l.LogDate.Date == d.Date);

                bool completed = (log?.Completed ?? false);
                bool skipped = (log?.Skipped ?? false);
                // Missed = Ngày quá khứ + Có lịch + Chưa làm + Chưa skip
                bool missed = d.Date < DateTime.Today && scheduled && !completed && !skipped;

                // --- LOGIC TÔ MÀU ---
                Brush bg = Brushes.Transparent;
                Brush border = Brushes.Transparent;
                Brush fg = Brushes.Black; // Màu chữ mặc định

                if (scheduled)
                {
                    if (completed)
                    {
                        bg = lightBrush;
                        border = mainBrush;
                        fg = mainBrush;
                    }
                    else if (skipped)
                    {
                        bg = skippedBg;
                        fg = skippedText;
                    }
                    else if (missed)
                    {
                        bg = missedBg;
                        border = missedBorder;
                        fg = Brushes.DarkRed;
                    }
                    else if (d.Date == DateTime.Today)
                    {
                        // Hôm nay chưa làm -> Viền xanh dương nhắc nhở
                        border = Brushes.DodgerBlue;
                        fg = Brushes.DodgerBlue;
                    }
                }

                // Nếu không phải tháng hiện tại -> Làm mờ đi
                if (!inMonth)
                {
                    fg = Brushes.LightGray;
                    bg = Brushes.Transparent;
                    border = Brushes.Transparent;
                }

                Days.Add(new MonthDayCell
                {
                    Date = d,
                    DayNumber = d.Day.ToString(),
                    IsInMonth = inMonth,
                    IsScheduled = scheduled,
                    IsCompleted = completed,
                    IsSkipped = skipped,
                    IsMissed = missed,

                    HighlightBackground = bg,
                    HighlightBorder = border,
                    Foreground = fg
                });
            }
        }

        // --- CÁC HÀM HELPER ---

        private bool IsHabitScheduledOn(DateTime date)
        {
            if (_habit == null) return false;
            var d = date.Date;

            // Kiểm tra ngày bắt đầu/kết thúc
            if (_habit.StartDate.Date > d) return false;
            if (_habit.UseEndCondition && _habit.EndDate.HasValue && _habit.EndDate.Value.Date < d)
                return false;

            if (_habit.RepeatEveryday) return true;
            if (_repeat == null) return false;

            return d.DayOfWeek switch
            {
                DayOfWeek.Monday => _repeat.Mon,
                DayOfWeek.Tuesday => _repeat.Tue,
                DayOfWeek.Wednesday => _repeat.Wed,
                DayOfWeek.Thursday => _repeat.Thu,
                DayOfWeek.Friday => _repeat.Fri,
                DayOfWeek.Saturday => _repeat.Sat,
                DayOfWeek.Sunday => _repeat.Sun,
                _ => false
            };
        }

        private static DateTime GetMonday(DateTime d)
        {
            int diff = (7 + (int)d.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return d.Date.AddDays(-diff);
        }

        private static Brush BrushFromHex(string hex, double opacity)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                var brush = new SolidColorBrush(color) { Opacity = opacity };
                brush.Freeze();
                return brush;
            }
            catch
            {
                return Brushes.Orange;
            }
        }
    }
}