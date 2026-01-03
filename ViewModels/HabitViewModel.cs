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
    public class HabitViewModel : ViewModelBase
    {
        private readonly User _user;

        // --- 1. THÔNG TIN THÚ CƯNG (Binding từ DB) ---
        private string _currentPetImage;
        public string CurrentPetImage { get => _currentPetImage; set => SetProperty(ref _currentPetImage, value); }

        private int _petLevel;
        public int PetLevel { get => _petLevel; set => SetProperty(ref _petLevel, value); }

        private int _petXp;
        public int PetXp { get => _petXp; set => SetProperty(ref _petXp, value); }

        private int _petXpMax = 100;
        public int PetXpMax { get => _petXpMax; set => SetProperty(ref _petXpMax, value); }

        // --- 2. FORM NHẬP LIỆU ---
        public string HabitName { get; set; } = "";

        // Icon Picker (Emoji)
        public ObservableCollection<string> IconList { get; } = new ObservableCollection<string>
        {
            "🏃", "💧", "📖", "🧘", "💰", "🎸", "💊", "🥗",
            "💤", "🧹", "💻", "🎨", "🚴", "🏋️", "🚭", "🚫",
            "🎵", "🍳", "🪴", "🐶"
        };
        private string _selectedIcon = "🏃";
        public string SelectedIcon { get => _selectedIcon; set => SetProperty(ref _selectedIcon, value); }

        // Màu sắc
        public ObservableCollection<string> AvailableColors { get; } = new ObservableCollection<string>
        {
            "#F97316", "#3B82F6", "#10B981", "#EF4444", "#8B5CF6", "#F59E0B", "#EC4899", "#6366F1"
        };
        private string _selectedColorHex = "#F97316";
        public string SelectedColorHex { get => _selectedColorHex; set => SetProperty(ref _selectedColorHex, value); }

        // --- 3. MỤC TIÊU (GOAL) ---
        private bool _isDailyGoal = true;
        public bool IsDailyGoal
        {
            get => _isDailyGoal;
            set { if (SetProperty(ref _isDailyGoal, value)) OnPropertyChanged(nameof(IsTotalGoal)); }
        }
        public bool IsTotalGoal
        {
            get => !_isDailyGoal;
            set => IsDailyGoal = !value;
        }

        public string DailyGoalValue { get; set; } = "1";
        public string TotalGoalValue { get; set; } = "";
        public string Unit { get; set; } = "Lần";

        // --- 4. NGÀY THÁNG & LẶP LẠI ---
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime? EndDate { get; set; }

        private bool _isEveryday = true;
        public bool IsEveryday
        {
            get => _isEveryday;
            set
            {
                if (SetProperty(ref _isEveryday, value))
                {
                    if (value) Mon = Tue = Wed = Thu = Fri = Sat = Sun = true;
                    NotifyDaysChanged();
                }
            }
        }
        public bool Mon { get; set; } = true;
        public bool Tue { get; set; } = true;
        public bool Wed { get; set; } = true;
        public bool Thu { get; set; } = true;
        public bool Fri { get; set; } = true;
        public bool Sat { get; set; } = true;
        public bool Sun { get; set; } = true;

        private void NotifyDaysChanged()
        {
            OnPropertyChanged(nameof(Mon)); OnPropertyChanged(nameof(Tue));
            OnPropertyChanged(nameof(Wed)); OnPropertyChanged(nameof(Thu));
            OnPropertyChanged(nameof(Fri)); OnPropertyChanged(nameof(Sat)); OnPropertyChanged(nameof(Sun));
        }

        // --- 5. NHẮC NHỞ (2 CHẾ ĐỘ) ---
        private bool _isReminderBySession = true;
        public bool IsReminderBySession
        {
            get => _isReminderBySession;
            set { if (SetProperty(ref _isReminderBySession, value)) OnPropertyChanged(nameof(IsReminderByTime)); }
        }
        public bool IsReminderByTime { get => !_isReminderBySession; set => IsReminderBySession = !value; }

        public bool RemindMorning { get; set; }
        public bool RemindAfternoon { get; set; }
        public bool RemindEvening { get; set; }

        // Hiển thị giờ user cài đặt lên nút bấm
        public string MorningLabel => _user.MorningTime.HasValue ? $"Sáng ({_user.MorningTime:hh\\:mm})" : "Sáng";
        public string AfternoonLabel => _user.AfternoonTime.HasValue ? $"Chiều ({_user.AfternoonTime:hh\\:mm})" : "Chiều";
        public string EveningLabel => _user.EveningTime.HasValue ? $"Tối ({_user.EveningTime:hh\\:mm})" : "Tối";

        public ObservableCollection<string> ReminderTimes { get; set; } = new ObservableCollection<string>();
        public string ReminderInput { get; set; } = "";

        // --- 6. ĐỘ KHÓ (XP) ---
        public bool IsEasy { get; set; } = true;
        public bool IsMedium { get; set; }
        public bool IsHard { get; set; }

        // --- COMMANDS ---
        public ICommand PickColorCommand { get; }
        public ICommand AddReminderCommand { get; }
        public ICommand RemoveReminderCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // --- CONSTRUCTOR ---
        public HabitViewModel(User user)
        {
            _user = user;
            LoadPetInfo();

            PickColorCommand = new RelayCommand(c => { if (c is string h) SelectedColorHex = h; });

            // Sửa dòng này: đổi "_" thành "obj"
            AddReminderCommand = new RelayCommand(obj =>
            {
                if (!string.IsNullOrWhiteSpace(ReminderInput) && !ReminderTimes.Contains(ReminderInput))
                {
                    // Bây giờ "out _" sẽ được hiểu đúng là discard (bỏ qua kết quả out)
                    if (TimeSpan.TryParse(ReminderInput, out _))
                    {
                        ReminderTimes.Add(ReminderInput);
                        ReminderInput = "";
                        OnPropertyChanged(nameof(ReminderInput));
                    }
                    else
                    {
                        MessageBox.Show("Định dạng giờ không hợp lệ (HH:mm)");
                    }
                }
            });

            RemoveReminderCommand = new RelayCommand(item => { if (item is string time) ReminderTimes.Remove(time); });

            SaveCommand = new RelayCommand(_ => SaveHabit());

            // CancelCommand: Logic đóng window/view tùy cấu trúc navigation của bạn
            CancelCommand = new RelayCommand(_ => { /* Code đóng view */ });
        }

        private void LoadPetInfo()
        {
            using var context = new AppDbContext();
            var pet = context.Pets.Include("PetType").FirstOrDefault(p => p.UserID == _user.UserID && p.Status != "Inactive");
            if (pet != null)
            {
                PetLevel = pet.Level;
                PetXp = pet.Experience;
                PetXpMax = pet.PetType.ExperienceRequired;

                double hours = (DateTime.Now - (pet.LastFedDate ?? DateTime.Now)).TotalHours;
                bool isHungry = hours > 8;
                string path = isHungry ? pet.PetType.AppearanceWhenHungry : pet.PetType.AppearanceWhenHappy;
                CurrentPetImage = path?.Replace("\\", "/") ?? "/Images/Pet/default.png";
            }
            else CurrentPetImage = "/Images/Pet/default.png";
        }

        // ==========================================================
        // 🔥 XỬ LÝ LƯU THÓI QUEN (LOGIC CHÍNH)
        // ==========================================================
        private void SaveHabit()
        {
            // 1. VALIDATION
            if (string.IsNullOrWhiteSpace(HabitName))
            {
                MessageBox.Show("Vui lòng nhập tên thói quen!");
                return;
            }

            decimal goalValue = 0;
            if (IsDailyGoal)
            {
                if (!decimal.TryParse(DailyGoalValue, out goalValue) || goalValue <= 0)
                {
                    MessageBox.Show("Mục tiêu mỗi ngày phải là số > 0!"); return;
                }
            }
            else
            {
                if (!decimal.TryParse(TotalGoalValue, out goalValue) || goalValue <= 0)
                {
                    MessageBox.Show("Mục tiêu tổng cộng phải là số > 0!"); return;
                }
            }

            if (EndDate.HasValue && EndDate < StartDate)
            {
                MessageBox.Show("Ngày kết thúc không được nhỏ hơn ngày bắt đầu!"); return;
            }

            try
            {
                using var context = new AppDbContext();

                // 2. KHỞI TẠO HABIT
                var habit = new Habit
                {
                    UserID = _user.UserID,
                    Name = HabitName,
                    Icon = SelectedIcon,
                    ColorHex = SelectedColorHex,

                    UseGoal = true,
                    GoalUnitType = Unit,
                    GoalValuePerDay = IsDailyGoal ? goalValue : (decimal?)null,
                    TargetTotalAmount = IsTotalGoal ? goalValue : (decimal?)null,

                    StartDate = StartDate,
                    EndDate = EndDate,
                    UseEndCondition = EndDate.HasValue,
                    RepeatEveryday = IsEveryday,

                    // Giá trị khởi tạo
                    CurrentStreak = 0,
                    BestStreak = 0,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };

                context.Habits.Add(habit);
                context.SaveChanges(); // -> Có HabitID

                // 3. LƯU REPEAT DAYS (Nếu không phải mỗi ngày)
                if (!IsEveryday)
                {
                    var repeat = new RepeatDay
                    {
                        HabitID = habit.HabitID,
                        Mon = Mon,
                        Tue = Tue,
                        Wed = Wed,
                        Thu = Thu,
                        Fri = Fri,
                        Sat = Sat,
                        Sun = Sun
                    };
                    context.RepeatDays.Add(repeat);
                }

                // 4. KHỞI TẠO HABIT LOG (Nếu hôm nay cần làm)
                if (StartDate.Date <= DateTime.Today)
                {
                    bool isScheduledToday = IsEveryday;
                    if (!isScheduledToday)
                    {
                        var today = DateTime.Today.DayOfWeek;
                        isScheduledToday = today switch
                        {
                            DayOfWeek.Monday => Mon,
                            DayOfWeek.Tuesday => Tue,
                            DayOfWeek.Wednesday => Wed,
                            DayOfWeek.Thursday => Thu,
                            DayOfWeek.Friday => Fri,
                            DayOfWeek.Saturday => Sat,
                            DayOfWeek.Sunday => Sun,
                            _ => false
                        };
                    }

                    if (isScheduledToday)
                    {
                        context.HabitLogs.Add(new HabitLog
                        {
                            HabitID = habit.HabitID,
                            LogDate = DateTime.Today,
                            Quantity = 0,
                            Completed = false,
                            Skipped = false,
                            TimeOfDay = GetTimeOfDayFromNow()
                        });
                    }
                }

                // 5. XỬ LÝ NHẮC NHỞ & MESSENGER (Random Message)
                // Lấy danh sách ID tin nhắn có sẵn
                var allMesIds = context.Messengers.Select(m => m.MesID).ToList();
                Random rand = new Random();

                // Hàm local để thêm Reminder + Messenger
                void AddReminderWithRandomMsg(TimeSpan time, string type)
                {
                    // a. Lưu Reminder
                    context.HabitReminders.Add(new HabitReminder
                    {
                        HabitID = habit.HabitID,
                        UserID = _user.UserID,
                        ReminderTime = time,
                        ReminderType = type,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });

                    // b. Lưu Messenger (Nếu có tin nhắn trong DB)
                    if (allMesIds.Count > 0)
                    {
                        int randomMesId = allMesIds[rand.Next(allMesIds.Count)];

                        // Đảm bảo bạn đã có DbSet<HabitMessenger> trong AppDbContext
                        context.HabitMessengers.Add(new HabitMessenger
                        {
                            HabitID = habit.HabitID,
                            MesID = randomMesId,
                            ReminderTime = time // Cần khớp với giờ nhắc
                        });
                    }
                }

                if (IsReminderBySession)
                {
                    // Lấy giờ user setting mới nhất
                    var currentUser = context.Users.Find(_user.UserID);
                    if (currentUser != null)
                    {
                        if (RemindMorning && currentUser.MorningTime.HasValue)
                            AddReminderWithRandomMsg(currentUser.MorningTime.Value, "Morning");

                        if (RemindAfternoon && currentUser.AfternoonTime.HasValue)
                            AddReminderWithRandomMsg(currentUser.AfternoonTime.Value, "Afternoon");

                        if (RemindEvening && currentUser.EveningTime.HasValue)
                            AddReminderWithRandomMsg(currentUser.EveningTime.Value, "Evening");
                    }
                }
                else // Theo giờ cụ thể
                {
                    foreach (var timeStr in ReminderTimes)
                    {
                        if (TimeSpan.TryParse(timeStr, out var ts))
                        {
                            AddReminderWithRandomMsg(ts, "Specific");
                        }
                    }
                }

                context.SaveChanges();
                MessageBox.Show("Thêm thói quen thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear Form hoặc đóng window ở đây nếu cần
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu: {ex.Message}\n{ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetTimeOfDayFromNow()
        {
            var h = DateTime.Now.Hour;
            return h < 12 ? "Morning" : (h < 18 ? "Afternoon" : "Evening");
        }
    }
}