using Microsoft.EntityFrameworkCore;
using OOP_Semester.Data;
using OOP_Semester.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace OOP_Semester.ViewModels
{
    // --- CÁC CLASS PHỤ (Model hiển thị) ---
    public class HabitItemDisplay : ViewModelBase
    {
        public int HabitID { get; set; }
        public string Title { get; set; } = "";

        private string _subtitle = "";
        public string Subtitle { get => _subtitle; set => SetProperty(ref _subtitle, value); }

        public string Icon { get; set; } = "🏃";
        public string ColorHex { get; set; } = "#E3F2FD";
        public string Unit { get; set; } = "Lần";
        public int TargetCount { get; set; }
        public bool IsCounterType { get; set; }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set { if (SetProperty(ref _isCompleted, value)) NotifyStatusChanged(); }
        }

        private bool _isSkipped;
        public bool IsSkipped
        {
            get => _isSkipped;
            set { if (SetProperty(ref _isSkipped, value)) NotifyStatusChanged(); }
        }

        private void NotifyStatusChanged()
        {
            OnPropertyChanged(nameof(HasStatus));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusIcon));
        }

        public bool HasStatus => IsCompleted || IsSkipped;
        public string StatusText => IsCompleted ? "Đã hoàn thành" : (IsSkipped ? "Bỏ qua" : "");
        public string StatusIcon => IsCompleted ? "✅" : (IsSkipped ? "⏭" : "");

        private int _currentCount;
        public int CurrentCount
        {
            get => _currentCount;
            set
            {
                if (SetProperty(ref _currentCount, value))
                {
                    OnPropertyChanged(nameof(Remaining));
                    OnPropertyChanged(nameof(HalfStep));
                    OnPropertyChanged(nameof(CanAct));
                    OnPropertyChanged(nameof(CanHalf));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public int Remaining => Math.Max(0, TargetCount - CurrentCount);

        public int HalfStep
        {
            get
            {
                if (!IsCounterType || Remaining <= 0) return 0;
                int half = Remaining / 2;
                return half == 0 ? 1 : half;
            }
        }

        public bool CanAct => Remaining > 0;
        public bool CanHalf => HalfStep > 0;
    }

    public class WeekDayDisplay : ViewModelBase
    {
        public DateTime Date { get; set; }
        public string DayLabel { get; set; } = "";
        public string DateLabel { get; set; } = "";

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    // --- MAIN VIEWMODEL ---
    public class TodayViewModel : ViewModelBase
    {
        private readonly User? _user; // User nhận từ input

        // --- 1. Pet Stats & Header ---
        private double _hungerPercent;
        public double HungerPercent { get => _hungerPercent; set => SetProperty(ref _hungerPercent, value); }

        private double _happinessPercent;
        public double HappinessPercent { get => _happinessPercent; set => SetProperty(ref _happinessPercent, value); }

        private string _hungerColor = "#22C55E";
        public string HungerColor { get => _hungerColor; set => SetProperty(ref _hungerColor, value); }

        private string _greeting = "Good Morning!";
        public string Greeting { get => _greeting; set => SetProperty(ref _greeting, value); }

        private string _streakMessage = "";
        public string StreakMessage { get => _streakMessage; set => SetProperty(ref _streakMessage, value); }

        private int _dailyProgressPercent;
        public int DailyProgressPercent { get => _dailyProgressPercent; set => SetProperty(ref _dailyProgressPercent, value); }

        private int _coins;
        public int Coins { get => _coins; set => SetProperty(ref _coins, value); }

        // --- 2. Pet Info ---
        private Pet? _currentPet;
        public Pet? CurrentPet { get => _currentPet; set { if (SetProperty(ref _currentPet, value)) { OnPropertyChanged(nameof(CurrentPetImage)); OnPropertyChanged(nameof(CurrentPetLevelStatus)); } } }

        public string CurrentPetImage
        {
            get
            {
                if (CurrentPet?.PetType == null) return "/Images/Pet/default.png";

                // Logic hiển thị ảnh dựa trên Status vừa tính toán ở trên
                // Nếu Status là Hungry (do Hunger > 50%) -> Lấy ảnh đói
                string path = (CurrentPet.Status == "Hungry")
                    ? CurrentPet.PetType.AppearanceWhenHungry
                    : CurrentPet.PetType.AppearanceWhenHappy;

                return path?.Replace("\\", "/") ?? "/Images/Pet/default.png";
            }
        }

        public string CurrentPetLevelStatus => CurrentPet == null ? "Chưa có thú cưng" : $"Level {CurrentPet.Level} • {CurrentPet.Status}";

        // --- 3. Challenge (Thử thách) ---
        private string _challengeName = "Đang tải...";
        public string ChallengeName { get => _challengeName; set => SetProperty(ref _challengeName, value); }

        private string _challengeProgressText = "";
        public string ChallengeProgressText { get => _challengeProgressText; set => SetProperty(ref _challengeProgressText, value); }

        private int _challengePercent;
        public int ChallengePercent { get => _challengePercent; set => SetProperty(ref _challengePercent, value); }

        // --- 4. Collections ---
        public ObservableCollection<HabitItemDisplay> Habits { get; set; } = new();
        public ObservableCollection<WeekDayDisplay> WeekDays { get; } = new();

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    Reload();
                }
            }
        }

        private string _weekTitle = "";
        public string WeekTitle { get => _weekTitle; set => SetProperty(ref _weekTitle, value); }

        // --- Commands ---
        public ICommand CompleteHabitCommand { get; private set; }
        public ICommand AddOneCommand { get; private set; }
        public ICommand AddHalfCommand { get; private set; }
        public ICommand CompleteAllCommand { get; private set; }
        public ICommand SelectDateCommand { get; private set; }
        public ICommand PrevWeekCommand { get; private set; }
        public ICommand NextWeekCommand { get; private set; }
        public ICommand SkipHabitCommand { get; private set; }
        public ICommand DeleteHabitCommand { get; private set; }
        public ICommand OpenHabitMonthCommand { get; private set; }

        public TodayViewModel()
        {
            InitializeCommands();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Greeting = "Designer Mode";
                ChallengeName = "Designer Challenge";
            }
        }

        public TodayViewModel(User user) : this()
        {
            _user = user;
            if (_user != null)
            {
                Coins = _user.GoldAmount ?? 0;
                UpdateGreeting();
                Reload();
            }
        }

        // --- SETUP COMMANDS ---
        private void InitializeCommands()
        {
            CompleteHabitCommand = new RelayCommand(obj => { if (obj is HabitItemDisplay item) ToggleCompleteAndSave(item); });

            AddOneCommand = new RelayCommand(obj =>
            {
                if (obj is HabitItemDisplay item)
                {
                    if (item.IsCounterType) ChangeCounterAndSave(item, +1);
                    else if (!item.IsCompleted) ToggleCompleteAndSave(item);
                }
            });

            AddHalfCommand = new RelayCommand(obj =>
            {
                if (obj is HabitItemDisplay item && item.IsCounterType && item.HalfStep > 0)
                    ChangeCounterAndSave(item, item.HalfStep);
            });

            CompleteAllCommand = new RelayCommand(obj =>
            {
                if (obj is HabitItemDisplay item)
                {
                    if (!item.IsCounterType)
                    {
                        ToggleCompleteAndSave(item);
                    }
                    else
                    {
                        if (item.IsSkipped) item.IsSkipped = false;

                        // 🔥 LOGIC VÀNG: Nếu chưa hoàn thành thì mới cộng vàng
                        if (!item.IsCompleted) UpdateUserGold(2);

                        item.CurrentCount = item.TargetCount;
                        item.IsCompleted = true;
                        item.Subtitle = $"{item.CurrentCount}/{item.TargetCount} {item.Unit}";

                        UpsertHabitLog(item);
                        UpdateHeader();
                    }
                }
            });

            SelectDateCommand = new RelayCommand(obj => { if (obj is WeekDayDisplay day) SelectedDate = day.Date; });
            PrevWeekCommand = new RelayCommand(_ => SelectedDate = SelectedDate.AddDays(-7));
            NextWeekCommand = new RelayCommand(_ => SelectedDate = SelectedDate.AddDays(7));

            SkipHabitCommand = new RelayCommand(obj => { if (obj is HabitItemDisplay item) ToggleSkipAndSave(item); });
            DeleteHabitCommand = new RelayCommand(obj => { if (obj is HabitItemDisplay item) DeleteHabit(item); });
            OpenHabitMonthCommand = new RelayCommand(obj => { if (obj is HabitItemDisplay item) OpenHabitMonth(item.HabitID); });
        }

        // --- LOGIC LOADING DATA ---
        public void Reload()
        {
            BuildWeekDays(SelectedDate);
            if (_user != null)
            {
                LoadHabitsForDate(SelectedDate);
                LoadPetStats();
                LoadChallengeData();
                UpdateHeader();
            }
        }

        private void UpdateGreeting()
        {
            if (_user == null) return;
            var hour = DateTime.Now.Hour;
            var part = hour < 12 ? "Buổi sáng" : (hour < 18 ? "Buổi chiều" : "Buổi tối");
            var icon = hour < 12 ? "☀️" : (hour < 18 ? "🌤️" : "🌙");
            var name = string.IsNullOrWhiteSpace(_user.Name) ? _user.Account : _user.Name;
            Greeting = $"Chào {part}, {name}! {icon}";
        }

        private void LoadChallengeData()
        {
            using var context = new AppDbContext();
            var userChallenge = context.UserChallenges
                                       .Include(uc => uc.Challenge)
                                       .Where(uc => uc.UserID == _user!.UserID)
                                       .OrderByDescending(uc => uc.Challenge.StartDate)
                                       .FirstOrDefault();

            if (userChallenge != null && userChallenge.Challenge != null)
            {
                ChallengeName = userChallenge.Challenge.Title ?? "Thử thách không tên";
                int progress = (int)(userChallenge.Progress);
                ChallengePercent = progress > 100 ? 100 : progress;
                ChallengeProgressText = $"Tiến độ: {ChallengePercent}%";
            }
            else
            {
                ChallengeName = "Bạn chưa tham gia thử thách nào";
                ChallengePercent = 0;
                ChallengeProgressText = "Hãy vào mục Thử thách để bắt đầu!";
            }
        }

        // ✅ LOGIC THÚ CƯNG (PET) - ĐÃ ĐỒNG BỘ VỚI FEEDING VIEW MODEL
        private void LoadPetStats()
        {
            using var context = new AppDbContext();

            // 1. Lấy Pet Active và thông tin Level từ PetType
            var pet = context.Pets
                             .Include(p => p.PetType)
                             .FirstOrDefault(p => p.UserID == _user!.UserID && p.Status != "Inactive");

            if (pet != null)
            {
                CurrentPet = pet;

                // 2. Lấy thông tin Level hiện tại để lấy đúng ảnh (Dựa trên FeedingViewModel)
                // Lưu ý: Cần query lại PetType nếu EF Core chưa load đủ list level (hoặc giả định PetType đã include đủ)
                // Ở đây ta truy vấn lại bảng PetTypes để lấy đúng dòng ứng với Level hiện tại của Pet
                var currentLvlInfo = context.PetTypes
                    .FirstOrDefault(pt => pt.PetTypeID == pet.PetTypeID && pt.Level == pet.Level);

                // 3. Tính toán Hunger/Happiness (Logic chuẩn: 1440 phút = 24h)
                if (pet.LastFedDate.HasValue)
                {
                    double mins = (DateTime.Now - pet.LastFedDate.Value).TotalMinutes;
                    // Công thức: (Số phút trôi qua / 1440) * 100 -> Max là 100
                    HungerPercent = (int) Math.Min(100, (mins / 1440.0) * 100);
                }
                else
                {
                    HungerPercent = 100; // Chưa ăn bao giờ -> Đói meo
                }

                // Happiness ngược lại với Hunger
                HappinessPercent = 100 - HungerPercent;

                // 4. Đổi màu thanh chỉ số
                if (HungerPercent > 70) HungerColor = "#EF4444"; // Đỏ (Rất đói)
                else if (HungerPercent > 30) HungerColor = "#F97316"; // Cam (Hơi đói)
                else HungerColor = "#22C55E"; // Xanh (No)

                // 5. Cập nhật Status để Binding ảnh (Dùng logic > 50% là đói)
                // Lưu ý: Cần cập nhật property Status của object Pet để View nhận diện
                pet.Status = (HungerPercent > 50) ? "Hungry" : "Happy";

                // 6. Kích hoạt cập nhật ảnh (Property CurrentPetImage sẽ tự tính toán lại dựa trên Status mới)
                OnPropertyChanged(nameof(CurrentPetImage));
                OnPropertyChanged(nameof(CurrentPetLevelStatus));
            }
            else
            {
                CurrentPet = null;
                HungerPercent = 0;
                HappinessPercent = 0;
            }
        }
        // --- LOGIC THÓI QUEN (HABIT) ---
        private void LoadHabitsForDate(DateTime date)
        {
            if (_user == null) return;
            Habits.Clear();
            var day = date.Date;

            using var context = new AppDbContext();

            var habits = context.Habits
                .Where(h => h.UserID == _user.UserID &&
                            (h.Status == null || h.Status == "Active") &&
                            h.StartDate <= day &&
                            ( ! h.UseEndCondition || h.EndDate == null || h.EndDate >= day))
                .ToList();

            var ids = habits.Select(h => h.HabitID).ToList();
            var repeatMap = context.RepeatDays
                .Where(r => ids.Contains(r.HabitID))
                .ToList()
                .ToDictionary(r => r.HabitID, r => r);

            bool IsOnDay(int habitId)
            {
                var h = habits.First(x => x.HabitID == habitId);
                if (h.RepeatEveryday) return true;
                if (!repeatMap.TryGetValue(habitId, out var r)) return false;

                return day.DayOfWeek switch
                {
                    DayOfWeek.Monday => r.Mon,
                    DayOfWeek.Tuesday => r.Tue,
                    DayOfWeek.Wednesday => r.Wed,
                    DayOfWeek.Thursday => r.Thu,
                    DayOfWeek.Friday => r.Fri,
                    DayOfWeek.Saturday => r.Sat,
                    DayOfWeek.Sunday => r.Sun,
                    _ => false
                };
            }

            foreach (var h in habits.Where(h => IsOnDay(h.HabitID)))
            {
                var log = context.HabitLogs.FirstOrDefault(l => l.HabitID == h.HabitID && l.LogDate == day);

                int target = (int)Math.Round(h.GoalValuePerDay ?? 1);
                int current = (int)Math.Round(log?.Quantity ?? 0);
                bool skipped = log?.Skipped ?? false;
                bool completed = (log?.Completed ?? false);
                bool isCounter = target > 1;

                string unit = h.GoalUnitType ?? "Lần";
                string subtitle = isCounter
                    ? $"{current}/{target} {unit}"
                    : (completed ? "Hoàn thành" : (skipped ? "Đã bỏ qua" : $"{target} {unit}"));

                var item = new HabitItemDisplay
                {
                    HabitID = h.HabitID,
                    Title = h.Name ?? "",
                    Unit = unit,
                    TargetCount = target,
                    CurrentCount = skipped ? 0 : current,
                    IsCompleted = completed,
                    IsSkipped = skipped,
                    IsCounterType = isCounter,
                    Subtitle = subtitle,
                    Icon = string.IsNullOrWhiteSpace(h.Icon) ? "🏃" : h.Icon!,
                    ColorHex = string.IsNullOrWhiteSpace(h.ColorHex) ? "#E3F2FD" : h.ColorHex!,
                };

                Habits.Add(item);
            }
        }

        private void ToggleCompleteAndSave(HabitItemDisplay item)
        {
            if (item.IsSkipped) item.IsSkipped = false;

            if (!item.IsCounterType)
            {
                bool isTurningOn = !item.IsCompleted;
                item.IsCompleted = isTurningOn;
                item.CurrentCount = item.IsCompleted ? item.TargetCount : 0;

                // 🔥 LOGIC VÀNG: Checkbox
                if (isTurningOn) UpdateUserGold(2);
                else UpdateUserGold(-2);
            }
            else
            {
                // Counter habit (bấm nút check là set full)
                if (!item.IsCompleted) UpdateUserGold(2); // Cộng vàng nếu trước đó chưa xong
                item.CurrentCount = item.TargetCount;
                item.IsCompleted = true;
            }

            UpdateSubtitle(item);
            UpsertHabitLog(item);
            UpdateHeader();
        }

        private void ChangeCounterAndSave(HabitItemDisplay item, int delta)
        {
            if (item.IsSkipped) item.IsSkipped = false;
            if (!item.IsCounterType) return;

            bool wasCompleted = item.IsCompleted; // Trạng thái cũ

            int next = item.CurrentCount + delta;
            if (next < 0) next = 0;
            if (next > item.TargetCount) next = item.TargetCount;

            item.CurrentCount = next;
            item.IsCompleted = item.CurrentCount >= item.TargetCount;

            // 🔥 LOGIC VÀNG: Counter
            // Nếu vừa hoàn thành -> Cộng
            if (!wasCompleted && item.IsCompleted) UpdateUserGold(2);
            // Nếu mất trạng thái hoàn thành -> Trừ
            else if (wasCompleted && !item.IsCompleted) UpdateUserGold(-2);

            UpdateSubtitle(item);
            UpsertHabitLog(item);
            UpdateHeader();
        }

        private void ToggleSkipAndSave(HabitItemDisplay item)
        {
            // 🔥 LOGIC VÀNG: Nếu đang hoàn thành mà bấm Skip -> Thu hồi vàng
            if (item.IsCompleted)
            {
                UpdateUserGold(-2);
            }

            item.IsSkipped = !item.IsSkipped;
            if (item.IsSkipped)
            {
                item.IsCompleted = false;
                item.CurrentCount = 0;
            }

            UpdateSubtitle(item);
            UpsertHabitLog(item);
            UpdateHeader();
        }

        private void UpdateSubtitle(HabitItemDisplay item)
        {
            item.Subtitle = item.IsCounterType
               ? $"{item.CurrentCount}/{item.TargetCount} {item.Unit}"
               : (item.IsCompleted ? "Hoàn thành" : (item.IsSkipped ? "Đã bỏ qua" : $"{item.TargetCount} {item.Unit}"));
        }

        // 🔥 HÀM CẬP NHẬT VÀNG
        private void UpdateUserGold(int amount)
        {
            if (_user == null) return;

            // 1. Cập nhật trên giao diện ngay lập tức
            int currentGold = _user.GoldAmount ?? 0;
            int newGold = currentGold + amount;
            if (newGold < 0) newGold = 0; // Không để âm tiền

            _user.GoldAmount = newGold;
            Coins = newGold; // Property Coins đã bind lên View

            // 2. Cập nhật xuống Database
            using (var context = new AppDbContext())
            {
                var userInDb = context.Users.FirstOrDefault(u => u.UserID == _user.UserID);
                if (userInDb != null)
                {
                    userInDb.GoldAmount = newGold;
                    context.SaveChanges();
                }
            }
        }

        private void UpsertHabitLog(HabitItemDisplay item)
        {
            if (_user == null) return;
            var day = SelectedDate.Date;
            using var context = new AppDbContext();

            var log = context.HabitLogs.FirstOrDefault(l => l.HabitID == item.HabitID && l.LogDate == day);

            if (log == null)
            {
                log = new HabitLog
                {
                    HabitID = item.HabitID,
                    LogDate = day,
                    Quantity = item.IsSkipped ? 0 : item.CurrentCount,
                    Completed = !item.IsSkipped && item.IsCompleted,
                    Skipped = item.IsSkipped,
                    TimeOfDay = "Morning"
                };
                context.HabitLogs.Add(log);
            }
            else
            {
                log.Skipped = item.IsSkipped;
                log.Completed = !item.IsSkipped && item.IsCompleted;
                log.Quantity = item.IsSkipped ? 0 : item.CurrentCount;
            }
            context.SaveChanges();
        }

        private void DeleteHabit(HabitItemDisplay item)
        {
            var rs = MessageBox.Show($"Xoá thói quen \"{item.Title}\"?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (rs != MessageBoxResult.Yes) return;

            using var context = new AppDbContext();
            var h = context.Habits.FirstOrDefault(x => x.HabitID == item.HabitID);
            if (h != null)
            {
                h.Status = "Deleted";
                context.SaveChanges();
                Habits.Remove(item);
                UpdateHeader();
            }
        }

        private void OpenHabitMonth(int habitId)
        {
            var win = new OOP_Semester.Views.HabitMonthWindow(habitId);
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }

        private void UpdateHeader()
        {
            int total = Habits.Count;
            int done = Habits.Count(h => h.IsCounterType ? (h.CurrentCount >= h.TargetCount) : h.IsCompleted);
            DailyProgressPercent = total == 0 ? 0 : (int)Math.Round(done * 100.0 / total);
            StreakMessage = $"Ngày {SelectedDate:dd/MM}: hoàn thành {done}/{total} mục tiêu 🔥";
        }

        private static DateTime GetMonday(DateTime d)
        {
            int diff = (7 + (int)d.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return d.Date.AddDays(-diff);
        }

        private void BuildWeekDays(DateTime pivot)
        {
            var mon = GetMonday(pivot);
            WeekTitle = $"{mon:dd/MM} - {mon.AddDays(6):dd/MM}";
            string[] labels = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
            WeekDays.Clear();
            for (int i = 0; i < 7; i++)
            {
                var date = mon.AddDays(i);
                WeekDays.Add(new WeekDayDisplay
                {
                    Date = date,
                    DayLabel = labels[i],
                    DateLabel = date.Day.ToString(),
                    IsSelected = date.Date == pivot.Date
                });
            }
        }
    }
}