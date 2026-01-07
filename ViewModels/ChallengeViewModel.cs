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
    // Wrapper cho Challenge
    public class ChallengeVM : ViewModelBase
    {
        public Challenge Model { get; set; }
        private decimal _progress;
        public decimal Progress { get => _progress; set => SetProperty(ref _progress, value); }

        public string DaysLeftText => Model.EndDate.HasValue
            ? $"Còn {(Model.EndDate.Value - DateTime.Today).Days} ngày"
            : "Vô thời hạn";
    }

    // Wrapper cho Task
    public class ChallengeTaskVM : ViewModelBase
    {
        public ChallengeTask Model { get; set; }
        private bool _isCompleted;
        public bool IsCompleted { get => _isCompleted; set => SetProperty(ref _isCompleted, value); }
    }

    public class ChallengeViewModel : ViewModelBase
    {
        private readonly User _currentUser;

        // DATA SOURCES
        public ObservableCollection<ChallengeVM> AvailableChallenges { get; set; } = new ObservableCollection<ChallengeVM>();
        public ObservableCollection<ChallengeVM> OtherJoinedChallenges { get; set; } = new ObservableCollection<ChallengeVM>();

        // HERO SECTION
        private ChallengeVM _currentChallenge;
        public ChallengeVM CurrentChallenge
        {
            get => _currentChallenge;
            set
            {
                if (SetProperty(ref _currentChallenge, value))
                {
                    HasActiveChallenge = value != null;
                    if (value != null) LoadTasksForCurrent();
                }
            }
        }

        public ObservableCollection<ChallengeTaskVM> CurrentTasks { get; set; } = new ObservableCollection<ChallengeTaskVM>();

        // VISIBILITY FLAG
        private bool _hasActiveChallenge;
        public bool HasActiveChallenge { get => _hasActiveChallenge; set { if (SetProperty(ref _hasActiveChallenge, value)) OnPropertyChanged(nameof(ShowEmptyState)); } }
        public bool ShowEmptyState => !HasActiveChallenge;

        // COMMANDS
        public ICommand JoinCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand SelectChallengeCommand { get; } // Lệnh để đổi Hero Challenge

        public ChallengeViewModel(User user)
        {
            _currentUser = user;
            JoinCommand = new RelayCommand(obj => JoinChallenge(obj as ChallengeVM));
            CheckInCommand = new RelayCommand(obj => CheckInTask(obj as ChallengeTaskVM));
            SelectChallengeCommand = new RelayCommand(obj => SwapToHero(obj as ChallengeVM));

            LoadData();
        }

        public void LoadData()
        {
            try
            {
                using var context = new AppDbContext();
                var today = DateTime.Today;

                // 1. CLEANUP EXPIRED
                var expired = context.UserChallenges
                    .Include(uc => uc.Challenge)
                    .Where(uc => uc.UserID == _currentUser.UserID && uc.Challenge.EndDate < today)
                    .ToList();
                if (expired.Any()) { context.UserChallenges.RemoveRange(expired); context.SaveChanges(); }

                // 2. LOAD ALL ACTIVE CHALLENGES
                var allChallenges = context.Challenges.Include(c => c.ChallengeTasks).Where(c => c.EndDate >= today).ToList();
                var userJoins = context.UserChallenges.Where(u => u.UserID == _currentUser.UserID).ToList();

                // 3. CLEAR LISTS
                AvailableChallenges.Clear();
                OtherJoinedChallenges.Clear();
                CurrentChallenge = null;

                // 4. PHÂN LOẠI
                var joinedList = allChallenges
                    .Where(c => userJoins.Any(u => u.ChallengesID == c.ChallengesID))
                    .Select(c => new ChallengeVM
                    {
                        Model = c,
                        Progress = userJoins.First(u => u.ChallengesID == c.ChallengesID).Progress
                    })
                    .OrderBy(vm => vm.Model.EndDate) // Ưu tiên cái sắp hết hạn
                    .ToList();

                if (joinedList.Count > 0)
                {
                    CurrentChallenge = joinedList[0]; // Cái đầu tiên làm Hero
                    for (int i = 1; i < joinedList.Count; i++) OtherJoinedChallenges.Add(joinedList[i]);
                }

                foreach (var c in allChallenges.Where(c => !userJoins.Any(u => u.ChallengesID == c.ChallengesID)))
                {
                    AvailableChallenges.Add(new ChallengeVM { Model = c, Progress = 0 });
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load: " + ex.Message); }
        }

        // Logic Load Task cho ngày hôm nay (Reset daily)
        private void LoadTasksForCurrent()
        {
            if (CurrentChallenge == null) return;
            CurrentTasks.Clear();

            using var context = new AppDbContext();
            var today = DateTime.Today;

            var tasks = CurrentChallenge.Model.ChallengeTasks.ToList();

            // Tìm những task đã tick hôm nay
            var doneToday = context.UserChallengeTasks
                .Where(x => x.UserID == _currentUser.UserID
                         && x.ChallengesID == CurrentChallenge.Model.ChallengesID
                         && x.LogDate == today && x.IsCompleted)
                .Select(x => x.TaskID).ToList();

            foreach (var t in tasks)
            {
                CurrentTasks.Add(new ChallengeTaskVM
                {
                    Model = t,
                    IsCompleted = doneToday.Contains(t.TaskID)
                });
            }
        }

        // Logic đổi Challenge từ list dưới lên Hero section
        private void SwapToHero(ChallengeVM selected)
        {
            if (selected == null || CurrentChallenge == selected) return;

            // Đưa Hero cũ xuống list dưới
            OtherJoinedChallenges.Add(CurrentChallenge);

            // Xóa cái được chọn khỏi list dưới
            OtherJoinedChallenges.Remove(selected);

            // Đưa cái được chọn lên Hero
            CurrentChallenge = selected;
        }

        private void CheckInTask(ChallengeTaskVM task)
        {
            if (task == null || CurrentChallenge == null) return;

            try
            {
                using var context = new AppDbContext();
                var today = DateTime.Today;

                // 1. Kiểm tra task đã tồn tại chưa
                var existing = context.UserChallengeTasks.FirstOrDefault(x =>
                    x.UserID == _currentUser.UserID &&
                    x.ChallengesID == CurrentChallenge.Model.ChallengesID &&
                    x.TaskID == task.Model.TaskID &&
                    x.LogDate == today);

                // Logic UNCHECK
                if (!task.IsCompleted)
                {
                    if (existing != null && existing.IsCompleted)
                    {
                        task.IsCompleted = true; // bật lại ngay
                        MessageBox.Show("Task hôm nay đã hoàn thành, không thể gỡ lại!");
                        return;
                    }
                    return;
                }

                // Logic CHECK (Hoàn thành)
                bool wasNewlyCompleted = false;
                if (existing == null)
                {
                    context.UserChallengeTasks.Add(new UserChallengeTask
                    {
                        UserID = _currentUser.UserID,
                        ChallengesID = CurrentChallenge.Model.ChallengesID,
                        TaskID = task.Model.TaskID,
                        LogDate = today,
                        IsCompleted = true
                    });
                    wasNewlyCompleted = true;
                }
                else if (!existing.IsCompleted)
                {
                    existing.IsCompleted = true;
                    wasNewlyCompleted = true;
                }

                context.SaveChanges();

                // ✅ THƯỞNG COIN THEO NGÀY (Daily Reward)
                int totalTasksToday = CurrentChallenge.Model.ChallengeTasks.Count;
                int doneTodayCount = context.UserChallengeTasks.Count(x =>
                    x.UserID == _currentUser.UserID &&
                    x.ChallengesID == CurrentChallenge.Model.ChallengesID &&
                    x.LogDate == today &&
                    x.IsCompleted);

                if (wasNewlyCompleted && totalTasksToday > 0 && doneTodayCount >= totalTasksToday)
                {
                    int reward = CurrentChallenge.Model.RewardCoins;
                    var userInDb = context.Users.FirstOrDefault(u => u.UserID == _currentUser.UserID);

                    if (userInDb != null)
                    {
                        userInDb.GoldAmount = (userInDb.GoldAmount ?? 0) + reward;

                        // THÊM LỊCH SỬ GIAO DỊCH
                        context.GoldTransactions.Add(new GoldTransaction
                        {
                            UserID = _currentUser.UserID,
                            Amount = reward,
                            TransactionDate = DateTime.Now,
                            Source = "Thử thách",
                            Note = $"Hoàn thành nhiệm vụ ngày: {CurrentChallenge.Model.Title}"
                        });

                        context.SaveChanges(); // Lưu vàng và lịch sử

                        _currentUser.GoldAmount = userInDb.GoldAmount;
                        GlobalChangeHub.RaiseCoinsChanged(this, (int)userInDb.GoldAmount);
                        MessageBox.Show($"Hoàn thành tất cả nhiệm vụ hôm nay! +{reward} Coins");
                    }
                }

                // 2. Tính lại Progress và thưởng HOÀN THÀNH CHALLENGE (Overall Reward)
                var days = (CurrentChallenge.Model.EndDate.Value - CurrentChallenge.Model.StartDate.Value).Days + 1;
                var totalTarget = days * CurrentChallenge.Model.ChallengeTasks.Count;
                var totalDone = context.UserChallengeTasks.Count(x =>
                    x.UserID == _currentUser.UserID &&
                    x.ChallengesID == CurrentChallenge.Model.ChallengesID &&
                    x.IsCompleted);

                decimal newProgress = totalTarget > 0 ? ((decimal)totalDone / totalTarget) * 100 : 0;
                if (newProgress > 100) newProgress = 100;

                var mainRecord = context.UserChallenges.FirstOrDefault(u =>
                    u.UserID == _currentUser.UserID &&
                    u.ChallengesID == CurrentChallenge.Model.ChallengesID);

                if (mainRecord != null)
                {
                    mainRecord.Progress = newProgress;

                    if (newProgress >= 100 && mainRecord.Status != "Completed")
                    {
                        mainRecord.Status = "Completed";
                        int rewardOverall = CurrentChallenge.Model.RewardCoins; // Hoặc một giá trị thưởng lớn hơn tùy logic

                        var userInDb = context.Users.FirstOrDefault(u => u.UserID == _currentUser.UserID);
                        if (userInDb != null)
                        {
                            userInDb.GoldAmount = (userInDb.GoldAmount ?? 0) + rewardOverall;

                            // THÊM LỊCH SỬ GIAO DỊCH HOÀN THÀNH THỬ THÁCH
                            context.GoldTransactions.Add(new GoldTransaction
                            {
                                UserID = _currentUser.UserID,
                                Amount = rewardOverall,
                                TransactionDate = DateTime.Now,
                                Source = "Thử thách",
                                Note = $"Về đích Thử thách: {CurrentChallenge.Model.Title}"
                            });

                            context.SaveChanges();

                            _currentUser.GoldAmount = userInDb.GoldAmount;
                            GlobalChangeHub.RaiseCoinsChanged(this, (int)userInDb.GoldAmount);
                            MessageBox.Show($"CHÚC MỪNG! Hoàn thành toàn bộ Challenge! +{rewardOverall} Coins");
                        }
                    }
                }

                context.SaveChanges();
                CurrentChallenge.Progress = newProgress;
                OnPropertyChanged(nameof(CurrentChallenge));
                LoadTasksForCurrent();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void JoinChallenge(ChallengeVM vm)
        {
            try
            {
                using var context = new AppDbContext();
                context.UserChallenges.Add(new UserChallenge { UserID = _currentUser.UserID, ChallengesID = vm.Model.ChallengesID, Progress = 0 });
                context.SaveChanges();
                LoadData();
                MessageBox.Show($"Đã tham gia {vm.Model.Title}");
            }
            catch { }
        }
    }
}
