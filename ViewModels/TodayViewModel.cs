using Microsoft.EntityFrameworkCore;
using OOP_Semester.Data;
using OOP_Semester.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static OOP_Semester.ViewModels.GlobalChangeHub;

namespace OOP_Semester.ViewModels
{
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

    public class TodayViewModel : ViewModelBase
    {
        private readonly User? _user;

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

        private Pet? _currentPet;
        public Pet? CurrentPet { get => _currentPet; set { if (SetProperty(ref _currentPet, value)) { OnPropertyChanged(nameof(CurrentPetImage)); OnPropertyChanged(nameof(CurrentPetLevelStatus)); } } }
        
        public string CurrentPetImage
        {
            get
            {
                if (CurrentPet?.PetType == null) return "/Images/Pet/default.png";
                string path = (CurrentPet.Status == "Hungry")
                    ? CurrentPet.PetType.AppearanceWhenHungry
                    : CurrentPet.PetType.AppearanceWhenHappy;

                return path?.Replace("\\", "/") ?? "/Images/Pet/default.png";
            }
        }

        public string CurrentPetLevelStatus => CurrentPet == null ? "Chưa có thú cưng" : $"Level {CurrentPet.Level} • {CurrentPet.Status}";

        private string _challengeName = "Đang tải...";
        public string ChallengeName { get => _challengeName; set => SetProperty(ref _challengeName, value); }

        private string _challengeProgressText = "";
        public string ChallengeProgressText { get => _challengeProgressText; set => SetProperty(ref _challengeProgressText, value); }

        private int _challengePercent;
        public int ChallengePercent { get => _challengePercent; set => SetProperty(ref _challengePercent, value); }

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
        public ICommand ContinueChallengeCommand{ get; private set; }

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

                GlobalChangeHub.CoinsChanged += OnGlobalCoinsChanged;
                GlobalChangeHub.PetChanged += OnGlobalPetChanged;
                GlobalChangeHub.DisplayNameChanged += OnGlobalDisplayNameChanged;
                GlobalChangeHub.GoldChanged += OnGlobalGoldChanged;

                UpdateGreeting();

                UpdateStreakFromYesterday_OncePerDay(); // ✅ CHỈ CHẠY Ở CTOR

                Reload();
            }
        }
        private void UpdateStreakFromYesterday_OncePerDay()
        {
            if (_user == null) return;

            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            using var context = new AppDbContext();

            // Lấy danh sách habit còn hạn ở "hôm qua"
            var habits = context.Habits
                .Where(h => h.UserID == _user.UserID
                            && (h.Status == null || h.Status == "Active")
                            && h.StartDate.Date <= yesterday.Date
                            && (!h.UseEndCondition || h.EndDate == null || h.EndDate.Value.Date >= yesterday.Date))
                .ToList();

            if (habits.Count == 0) return;

            // Lấy repeat map để check habit có chạy vào ngày hôm qua không
            var ids = habits.Select(h => h.HabitID).ToList();
            var repeatMap = context.RepeatDays
                .Where(r => ids.Contains(r.HabitID))
                .ToList()
                .ToDictionary(r => r.HabitID, r => r);

            bool IsOnYesterday(Habit h)
            {
                if (h.RepeatEveryday) return true;
                if (!repeatMap.TryGetValue(h.HabitID, out var r)) return false;
                var prev = h.LastStreakDate?.Date;
                if (prev.HasValue && prev.Value < yesterday.AddDays(-1).Date)
                {
                    if (h.CurrentStreak > h.BestStreak) h.BestStreak = h.CurrentStreak;
                    h.CurrentStreak = 0;
                }
                return yesterday.DayOfWeek switch
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

            var dueHabits = habits.Where(IsOnYesterday).ToList();
            if (dueHabits.Count == 0) return;

            // Lấy log của ngày hôm qua cho các habit đó
            var dueIds = dueHabits.Select(h => h.HabitID).ToList();
            var logs = context.HabitLogs
                .Where(l => dueIds.Contains(l.HabitID) && l.LogDate.Date == yesterday.Date)
                .ToList()
                .ToDictionary(l => l.HabitID);

            foreach (var h in dueHabits)
            {
                // ✅ Nếu đã xử lý streak cho hôm qua rồi thì bỏ qua (mở app lần 2 trong ngày)
                if (h.LastStreakDate.HasValue && h.LastStreakDate.Value.Date == yesterday.Date)
                    continue;

                logs.TryGetValue(h.HabitID, out var log);

                // 1) Nếu SKIP -> giữ nguyên hiện trạng, chỉ đánh dấu đã xử lý
                if (log != null && log.Skipped)
                {
                    h.LastStreakDate = yesterday;
                    continue;
                }

                // 2) Nếu COMPLETED -> tăng streak + cập nhật best + đánh dấu đã xử lý
                if (log != null && log.Completed)
                {
                    // nếu liền mạch với last streak date cũ thì +1, không thì reset về 1
                    var prev = h.LastStreakDate?.Date;
                    if (prev.HasValue && prev.Value == yesterday.AddDays(-1).Date)
                        h.CurrentStreak += 1;
                    else
                        h.CurrentStreak = 1;

                    if (h.CurrentStreak > h.BestStreak)
                        h.BestStreak = h.CurrentStreak;

                    h.LastStreakDate = yesterday;
                    continue;
                }

                // 3) Không completed (và không skip) -> chốt best rồi reset current về 0, đánh dấu đã xử lý
                if (h.CurrentStreak > h.BestStreak)
                    h.BestStreak = h.CurrentStreak;

                h.CurrentStreak = 0;
                h.LastStreakDate = yesterday;
            }

            context.SaveChanges();
        }

        private void OnGlobalGoldChanged(object sender, int newGold)
        {
            if (ReferenceEquals(sender, this)) return;

            Coins = newGold;
            if (_user != null) _user.GoldAmount = newGold;
        }

        private void OnGlobalPetChanged(object sender)
        {
            if (ReferenceEquals(sender, this)) return;
            LoadPetStats();
        }

        private void OnGlobalCoinsChanged(object sender, int newCoins)
        {
            if (_user == null) return;
            if (ReferenceEquals(sender, this)) return;

            _user.GoldAmount = newCoins;
            Coins = newCoins;
        }

   

        private void OnGlobalDisplayNameChanged(object sender, string? _)
        {
            if (_user == null) return;
            UpdateGreeting();
        }

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
            ContinueChallengeCommand = new RelayCommand( obj => { NavigationHub.NavigateTo("Challenge"); }) ; 
            
        }

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

        private void LoadPetStats()
        {
            using var context = new AppDbContext();

            var pet = context.Pets
                             .Include(p => p.PetType)
                             .FirstOrDefault(p => p.UserID == _user!.UserID && p.Status != "Inactive");

            if (pet != null)
            {
                CurrentPet = pet;

                if (pet.LastFedDate.HasValue)
                {
                    double mins = (DateTime.Now - pet.LastFedDate.Value).TotalMinutes;
                    HungerPercent = (int)Math.Min(100, (mins / 1440.0) * 100);
                }
                else
                {
                    HungerPercent = 100;
                }

                HappinessPercent = 100 - HungerPercent;

                if (HungerPercent > 70) HungerColor = "#EF4444";
                else if (HungerPercent > 30) HungerColor = "#F97316";
                else HungerColor = "#22C55E";

                pet.Status = (HungerPercent > 50) ? "Hungry" : "Happy";

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
                            (!h.UseEndCondition || h.EndDate == null || h.EndDate >= day))
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

            // Chỉ đảo ngược trạng thái hoàn thành và cập nhật vàng
            if (!item.IsCounterType)
            {
                item.IsCompleted = !item.IsCompleted;
                item.CurrentCount = item.IsCompleted ? item.TargetCount : 0;
            }
            else
            {
                // Với dạng đếm, mặc định là đặt lên max khi check
                item.CurrentCount = item.TargetCount;
                item.IsCompleted = true;
            }

            // Cập nhật vàng dựa trên trạng thái mới
            if (item.IsCompleted) UpdateUserGold(2);
            else UpdateUserGold(-2);

            UpdateSubtitle(item);
            UpsertHabitLog(item); // Lưu vào HabitLog với TimeOfDay động
            UpdateHeader();       // Cập nhật tiến độ % trên thanh tiêu đề
        }

        private void UpsertHabitLog(HabitItemDisplay item)
        {
            if (_user == null) return;
            var day = SelectedDate.Date;
            using var db = new AppDbContext(); // Dùng using để đóng kết nối ngay, tránh lỗi File in use

            var log = db.HabitLogs.FirstOrDefault(l => l.HabitID == item.HabitID && l.LogDate == day);

            // Quyết định buổi dựa trên giờ thực tế
            int hour = DateTime.Now.Hour;
            string session = hour < 12 ? "Morning" : (hour < 18 ? "Afternoon" : "Evening");

            if (log == null)
            {
                db.HabitLogs.Add(new HabitLog
                {
                    HabitID = item.HabitID,
                    LogDate = day,
                    Quantity = item.IsSkipped ? 0 : item.CurrentCount,
                    Completed = !item.IsSkipped && item.IsCompleted,
                    Skipped = item.IsSkipped,
                    TimeOfDay = session
                });
            }
            else
            {
                log.Quantity = item.IsSkipped ? 0 : item.CurrentCount;
                log.Completed = !item.IsSkipped && item.IsCompleted;
                log.Skipped = item.IsSkipped;
                log.TimeOfDay = session;
            }
            db.SaveChanges();
        }

        private void ChangeCounterAndSave(HabitItemDisplay item, int delta)
        {
            if (item.IsSkipped) item.IsSkipped = false;
            if (!item.IsCounterType) return;

            bool wasCompleted = item.IsCompleted;

            int next = item.CurrentCount + delta;
            if (next < 0) next = 0;
            if (next > item.TargetCount) next = item.TargetCount;

            item.CurrentCount = next;
            item.IsCompleted = item.CurrentCount >= item.TargetCount;

            if (!wasCompleted && item.IsCompleted) UpdateUserGold(2);
            else if (wasCompleted && !item.IsCompleted) UpdateUserGold(-2);

            UpdateSubtitle(item);
            UpsertHabitLog(item);
            UpdateHeader();
        }

        private void ToggleSkipAndSave(HabitItemDisplay item)
        {
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

        private void UpdateUserGold(int amount)
        {
            if (_user == null || amount == 0) return;

            int currentGold = _user.GoldAmount ?? 0;
            int newGold = Math.Max(0, currentGold + amount);

            // Cập nhật local UI
            _user.GoldAmount = newGold;
            Coins = newGold;

            using (var context = new AppDbContext())
            {
                var userInDb = context.Users.FirstOrDefault(u => u.UserID == _user.UserID);
                if (userInDb != null)
                {
                    // 1. Cập nhật số dư vàng của User
                    userInDb.GoldAmount = newGold;

                    // 2. Tạo bản ghi lịch sử giao dịch mới
                    var transaction = new GoldTransaction
                    {
                        UserID = _user.UserID,
                        Amount = amount,
                        TransactionDate = DateTime.Now,
                        // Tự động xác định nguồn dựa trên số tiền cộng hay trừ
                        Source = amount > 0 ? "Nhiệm vụ" : "Cửa hàng",
                        Note = amount > 0 ? $"Thưởng hoàn thành thói quen (+{amount})" : $"Mua vật phẩm ({amount})"
                    };

                    context.GoldTransactions.Add(transaction);

                    // 3. Lưu toàn bộ thay đổi xuống DB
                    context.SaveChanges();

                    // 4. Phát tín hiệu đồng bộ cho các ViewModel khác (như Shop)
                    GlobalChangeHub.RaiseCoinsChanged(this, newGold);
                }
            }
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

