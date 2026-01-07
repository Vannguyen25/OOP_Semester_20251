using Microsoft.EntityFrameworkCore;
using OOP_Semester.Data;
using OOP_Semester.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using YourNamespace.Models;

namespace OOP_Semester.ViewModels
{
    public class HabitViewModel : ViewModelBase
    {
        private readonly User _user;

        // --- 1. THÔNG TIN THÚ CƯNG ---
        private string _currentPetImage;
        public string CurrentPetImage { get => _currentPetImage; set => SetProperty(ref _currentPetImage, value); }

        private int _petLevel;
        public int PetLevel { get => _petLevel; set => SetProperty(ref _petLevel, value); }

        private int _petXp;
        public int PetXp { get => _petXp; set => SetProperty(ref _petXp, value); }

        private int _petXpMax = 100;
        public int PetXpMax { get => _petXpMax; set => SetProperty(ref _petXpMax, value); }

        // --- 2. FORM NHẬP LIỆU ---
        private string _habitName = "";
        public string HabitName { get => _habitName; set => SetProperty(ref _habitName, value); }

        // Icon Picker
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
        public bool IsTotalGoal { get => !_isDailyGoal; set => IsDailyGoal = !value; }

        private string _dailyGoalValue = "1";
        public string DailyGoalValue { get => _dailyGoalValue; set => SetProperty(ref _dailyGoalValue, value); }

        private string _totalGoalValue = "";
        public string TotalGoalValue { get => _totalGoalValue; set => SetProperty(ref _totalGoalValue, value); }

        private string _unit = "Lần";
        public string Unit { get => _unit; set => SetProperty(ref _unit, value); }

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

        // Các biến ngày trong tuần cần raise PropertyChanged để UI cập nhật khi IsEveryday thay đổi
        private bool _mon = true; public bool Mon { get => _mon; set => SetProperty(ref _mon, value); }
        private bool _tue = true; public bool Tue { get => _tue; set => SetProperty(ref _tue, value); }
        private bool _wed = true; public bool Wed { get => _wed; set => SetProperty(ref _wed, value); }
        private bool _thu = true; public bool Thu { get => _thu; set => SetProperty(ref _thu, value); }
        private bool _fri = true; public bool Fri { get => _fri; set => SetProperty(ref _fri, value); }
        private bool _sat = true; public bool Sat { get => _sat; set => SetProperty(ref _sat, value); }
        private bool _sun = true; public bool Sun { get => _sun; set => SetProperty(ref _sun, value); }

        private void NotifyDaysChanged()
        {
            OnPropertyChanged(nameof(Mon)); OnPropertyChanged(nameof(Tue));
            OnPropertyChanged(nameof(Wed)); OnPropertyChanged(nameof(Thu));
            OnPropertyChanged(nameof(Fri)); OnPropertyChanged(nameof(Sat)); OnPropertyChanged(nameof(Sun));
        }

        // --- 5. NHẮC NHỞ ---
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

        public string MorningLabel => _user.MorningTime.HasValue ? $"Sáng ({_user.MorningTime:hh\\:mm})" : "Sáng";
        public string AfternoonLabel => _user.AfternoonTime.HasValue ? $"Chiều ({_user.AfternoonTime:hh\\:mm})" : "Chiều";
        public string EveningLabel => _user.EveningTime.HasValue ? $"Tối ({_user.EveningTime:hh\\:mm})" : "Tối";

        public ObservableCollection<string> ReminderTimes { get; set; } = new ObservableCollection<string>();

        private string _reminderInput = "";
        public string ReminderInput { get => _reminderInput; set => SetProperty(ref _reminderInput, value); }

        // --- 6. TEMPLATE SUPPORT (MỚI) ---
        public ObservableCollection<HabitTemplate> Templates { get; set; } = new ObservableCollection<HabitTemplate>();

        private bool _isTemplatePopupVisible;
        public bool IsTemplatePopupVisible { get => _isTemplatePopupVisible; set => SetProperty(ref _isTemplatePopupVisible, value); }

        // --- COMMANDS ---
        public ICommand PickColorCommand { get; }
        public ICommand AddReminderCommand { get; }
        public ICommand RemoveReminderCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Template Commands
        public ICommand OpenTemplateLibraryCommand { get; }
        public ICommand CloseTemplateLibraryCommand { get; }
        public ICommand ApplyTemplateCommand { get; }

        // --- CONSTRUCTOR ---
        public HabitViewModel(User user)
        {
            _user = user;
            LoadPetInfo();

            PickColorCommand = new RelayCommand(c => { if (c is string h) SelectedColorHex = h; });

            AddReminderCommand = new RelayCommand(obj =>
            {
                if (!string.IsNullOrWhiteSpace(ReminderInput) && !ReminderTimes.Contains(ReminderInput))
                {
                    if (TimeSpan.TryParse(ReminderInput, out _))
                    {
                        ReminderTimes.Add(ReminderInput);
                        ReminderInput = "";
                    }
                    else MessageBox.Show("Định dạng giờ không hợp lệ (HH:mm)");
                }
            });

            RemoveReminderCommand = new RelayCommand(item => { if (item is string time) ReminderTimes.Remove(time); });
            SaveCommand = new RelayCommand(_ => SaveHabit());
            CancelCommand = new RelayCommand(_ => { /* Logic đóng window */ });

            // --- Template Logic ---
            OpenTemplateLibraryCommand = new RelayCommand(_ => LoadAndOpenTemplates());
            CloseTemplateLibraryCommand = new RelayCommand(_ => IsTemplatePopupVisible = false);
            ApplyTemplateCommand = new RelayCommand(obj =>
            {
                if (obj is HabitTemplate template)
                {
                    ApplyTemplateToForm(template);
                    IsTemplatePopupVisible = false;
                }
            });
        }

        private void LoadPetInfo()
        {
            try
            {
                using var context = new AppDbContext();
                var pet = context.Pets.Include("PetType").FirstOrDefault(p => p.UserID == _user.UserID && p.Status != "Inactive");

                PetLevel = pet.Level;
                PetXp = pet.Experience;
                double hours = (DateTime.Now - (pet.LastFedDate ?? DateTime.Now)).TotalHours;
                // bool isHungry = hours > 8;
                string path = pet.PetType.AppearanceWhenHappy;
                CurrentPetImage = path?.Replace("\\", "/");
                
            }
            catch { CurrentPetImage = "/Images/Pet/Pitbull_Level1.png"; }
        }

        // --- TEMPLATE METHODS ---
        private void LoadAndOpenTemplates()
        {
            try
            {
                using var context = new AppDbContext();
                var list = context.HabitTemplates.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToList();
                Templates.Clear();
                foreach (var item in list) Templates.Add(item);
                IsTemplatePopupVisible = true;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải template: " + ex.Message); }
        }

        private void ApplyTemplateToForm(HabitTemplate t)
        {
            HabitName = t.Name;
            if (!string.IsNullOrEmpty(t.IconCode)) SelectedIcon = t.IconCode;
            if (!string.IsNullOrEmpty(t.ColorHex)) SelectedColorHex = t.ColorHex;

            Unit = t.DefaultGoalUnitName;

            if (t.DefaultGoalUnitType == "Count" || t.DefaultGoalUnitType == "Time")
            {
                IsDailyGoal = true;
                DailyGoalValue = t.DefaultGoalValuePerDay.ToString("0.##"); // Format bỏ số 0 thừa
            }
            else // Checkbox
            {
                IsDailyGoal = true;
                DailyGoalValue = "1";
            }

            // Reset về mặc định
            IsEveryday = true;
        }

        // --- SAVE LOGIC (GIỮ NGUYÊN) ---
        private void SaveHabit()
        {
            if (string.IsNullOrWhiteSpace(HabitName)) { MessageBox.Show("Vui lòng nhập tên thói quen!"); return; }

            decimal goalValue = 0;
            if (IsDailyGoal)
            {
                if (!decimal.TryParse(DailyGoalValue, out goalValue) || goalValue <= 0) { MessageBox.Show("Mục tiêu mỗi ngày phải là số > 0!"); return; }
            }
            else
            {
                if (!decimal.TryParse(TotalGoalValue, out goalValue) || goalValue <= 0) { MessageBox.Show("Mục tiêu tổng cộng phải là số > 0!"); return; }
            }

            if (EndDate.HasValue && EndDate < StartDate) { MessageBox.Show("Ngày kết thúc không được nhỏ hơn ngày bắt đầu!"); return; }

            try
            {
                using var context = new AppDbContext();
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
                    CurrentStreak = 0,
                    BestStreak = 0,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };

                context.Habits.Add(habit);
                context.SaveChanges();

                if (!IsEveryday)
                {
                    context.RepeatDays.Add(new RepeatDay { HabitID = habit.HabitID, Mon = Mon, Tue = Tue, Wed = Wed, Thu = Thu, Fri = Fri, Sat = Sat, Sun = Sun });
                }

                if (StartDate.Date <= DateTime.Today)
                {
                    bool isScheduledToday = IsEveryday;
                    if (!isScheduledToday)
                    {
                        var today = DateTime.Today.DayOfWeek;
                        isScheduledToday = today switch { DayOfWeek.Monday => Mon, DayOfWeek.Tuesday => Tue, DayOfWeek.Wednesday => Wed, DayOfWeek.Thursday => Thu, DayOfWeek.Friday => Fri, DayOfWeek.Saturday => Sat, DayOfWeek.Sunday => Sun, _ => false };
                    }
                    if (isScheduledToday)
                    {
                        context.HabitLogs.Add(new HabitLog { HabitID = habit.HabitID, LogDate = DateTime.Today, Quantity = 0, Completed = false, Skipped = false, TimeOfDay = GetTimeOfDayFromNow() });
                    }
                }

                // Reminder Logic
                var allMesIds = context.Messengers.Select(m => m.MesID).ToList();
                Random rand = new Random();

                void AddReminderWithRandomMsg(TimeSpan time, string type)
                {
                    context.HabitReminders.Add(new HabitReminder { HabitID = habit.HabitID, UserID = _user.UserID, ReminderTime = time, ReminderType = type, IsActive = true, CreatedAt = DateTime.Now });
                    if (allMesIds.Count > 0)
                    {
                        context.HabitMessengers.Add(new HabitMessenger { HabitID = habit.HabitID, MesID = allMesIds[rand.Next(allMesIds.Count)], ReminderTime = time });
                    }
                }

                if (IsReminderBySession)
                {
                    var currentUser = context.Users.Find(_user.UserID);
                    if (currentUser != null)
                    {
                        if (RemindMorning && currentUser.MorningTime.HasValue) AddReminderWithRandomMsg(currentUser.MorningTime.Value, "Morning");
                        if (RemindAfternoon && currentUser.AfternoonTime.HasValue) AddReminderWithRandomMsg(currentUser.AfternoonTime.Value, "Afternoon");
                        if (RemindEvening && currentUser.EveningTime.HasValue) AddReminderWithRandomMsg(currentUser.EveningTime.Value, "Evening");
                    }
                }
                else
                {
                    foreach (var timeStr in ReminderTimes)
                    {
                        if (TimeSpan.TryParse(timeStr, out var ts)) AddReminderWithRandomMsg(ts, "Specific");
                    }
                }

                context.SaveChanges();
                MessageBox.Show("Thêm thói quen thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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