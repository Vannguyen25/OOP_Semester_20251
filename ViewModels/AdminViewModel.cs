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
    public class AdminViewModel : ViewModelBase
    {
        private readonly User _currentUser;

        // --- CHALLENGE PROPERTIES ---
        private string _challengeTitle;
        public string ChallengeTitle { get => _challengeTitle; set => SetProperty(ref _challengeTitle, value); }

        private string _challengeDescription;
        public string ChallengeDescription { get => _challengeDescription; set => SetProperty(ref _challengeDescription, value); }

        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime _endDate = DateTime.Today.AddDays(30);
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private int _rewardCoins = 50;
        public int RewardCoins { get => _rewardCoins; set => SetProperty(ref _rewardCoins, value); }

        public ObservableCollection<ChallengeTask> Tasks { get; set; } = new ObservableCollection<ChallengeTask>();

        // --- TEMPLATE PROPERTIES ---
        private bool _isTemplate;
        public bool IsTemplate { get => _isTemplate; set => SetProperty(ref _isTemplate, value); }

        private string _tempHabitName;
        public string TempHabitName { get => _tempHabitName; set => SetProperty(ref _tempHabitName, value); }

        private string _tempDescription;
        public string TempDescription { get => _tempDescription; set => SetProperty(ref _tempDescription, value); }

        private int? _selectedCategoryID;
        public int? SelectedCategoryID { get => _selectedCategoryID; set => SetProperty(ref _selectedCategoryID, value); }

        // Mặc định icon và màu
        private string _tempIcon = "🏃";
        public string TempIcon { get => _tempIcon; set => SetProperty(ref _tempIcon, value); }

        private string _tempColorHex = "#F97316";
        public string TempColorHex { get => _tempColorHex; set => SetProperty(ref _tempColorHex, value); }

        private string _tempUnit = "Lần";
        public string TempUnit { get => _tempUnit; set => SetProperty(ref _tempUnit, value); }

        private string _tempDefaultGoal = "1";
        public string TempDefaultGoal { get => _tempDefaultGoal; set => SetProperty(ref _tempDefaultGoal, value); }

        // --- DATA SOURCES ---
        public ObservableCollection<HabitCategory> Categories { get; set; } = new ObservableCollection<HabitCategory>();

        // List Icon giống HabitView
        public ObservableCollection<string> IconList { get; set; } = new ObservableCollection<string>
        { "🏃", "💧", "📚", "🧘", "💪", "🍎", "💤", "💻", "🎨", "🎵", "🧹", "💰", "🚴", "🏊" };

        // List Màu giống HabitView
        public ObservableCollection<string> AvailableColors { get; set; } = new ObservableCollection<string>
        { "#F97316", "#EF4444", "#22C55E", "#3B82F6", "#A855F7", "#EC4899", "#6366F1", "#14B8A6" };

        // --- COMMANDS ---
        public ICommand CreateChallengeCommand { get; }
        public ICommand CreateTemplateCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand RemoveTaskCommand { get; }
        public ICommand UploadImageCommand { get; }

        public AdminViewModel(User user)
        {
            _currentUser = user;
            LoadCategories();

            CreateChallengeCommand = new RelayCommand(_ => SaveChallenge());
            CreateTemplateCommand = new RelayCommand(_ => SaveHabitTemplate());

            AddTaskCommand = new RelayCommand(_ => Tasks.Add(new ChallengeTask { Description = "", DailySession = "Morning" }));
            RemoveTaskCommand = new RelayCommand(obj => { if (obj is ChallengeTask task) Tasks.Remove(task); });
            UploadImageCommand = new RelayCommand(_ => MessageBox.Show("Tính năng đang phát triển"));

            // Task mẫu ban đầu
            Tasks.Add(new ChallengeTask { Description = "", DailySession = "Anytime" });
        }

        private void LoadCategories()
        {
            try
            {
                using var context = new AppDbContext();
                var cats = context.HabitCategories.ToList();
                Categories.Clear();
                foreach (var c in cats) Categories.Add(c);
            }
            catch { }
        }

        private void SaveChallenge()
        {
            if (string.IsNullOrWhiteSpace(ChallengeTitle)) { MessageBox.Show("Nhập tên thử thách!"); return; }
            if (Tasks.Count == 0) { MessageBox.Show("Cần ít nhất 1 nhiệm vụ!"); return; }

            try
            {
                using var context = new AppDbContext();
                var newChallenge = new Challenge
                {
                    Title = ChallengeTitle,
                    Description = ChallengeDescription,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    RewardCoins = RewardCoins
                };
                context.Challenges.Add(newChallenge);
                context.SaveChanges();

                foreach (var t in Tasks)
                {
                    if (string.IsNullOrWhiteSpace(t.Description)) continue;
                    var newTask = new ChallengeTask
                    {
                        ChallengesID = newChallenge.ChallengesID,
                        Description = t.Description,
                        DailySession = t.DailySession ?? "Anytime"
                    };
                    context.ChallengeTasks.Add(newTask);
                }
                context.SaveChanges();
                MessageBox.Show("Tạo Thử thách thành công!");

                // Reset
                ChallengeTitle = ""; Tasks.Clear(); Tasks.Add(new ChallengeTask());
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
        }

        private void SaveHabitTemplate()
        {
            if (string.IsNullOrWhiteSpace(TempHabitName)) { MessageBox.Show("Nhập tên Template!"); return; }
            if (SelectedCategoryID == null) { MessageBox.Show("Chọn danh mục!"); return; }

            try
            {
                using var context = new AppDbContext();
                decimal.TryParse(TempDefaultGoal, out decimal goalValue);

                var template = new HabitTemplate
                {
                    Name = TempHabitName,
                    Description = TempDescription,
                    CategoryID = SelectedCategoryID.Value,
                    IconCode = TempIcon,
                    ColorHex = TempColorHex, // Lưu màu
                    DefaultGoalUnitName = TempUnit,
                    DefaultGoalValuePerDay = goalValue,
                    DefaultGoalUnitType = (goalValue > 1) ? "Count" : "Checkbox",
                    IsActive = true,
                    SortOrder = 0,
                    IsRepeatable = true,
                    RepeatType = "Daily",
                    ViewShape = "Rounded"
                };
                context.HabitTemplates.Add(template);
                context.SaveChanges();
                MessageBox.Show("Đã lưu Habit Template!");
                TempHabitName = "";
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}\n{ex.InnerException?.Message}"); }
        }
    }
}