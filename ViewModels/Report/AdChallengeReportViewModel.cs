using Microsoft.EntityFrameworkCore;
using OOP_Semester.Data;
using OOP_Semester.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOP_Semester.ViewModels.Report
{
    public class AdChallengeReportViewModel : ViewModelBase
    {
        // Stats
        private int _activeChallenges;
        public int ActiveChallenges { get => _activeChallenges; set => SetProperty(ref _activeChallenges, value); }

        private int _totalParticipants;
        public int TotalParticipants { get => _totalParticipants; set => SetProperty(ref _totalParticipants, value); }

        private double _overallCompletionRate;
        public double OverallCompletionRate { get => _overallCompletionRate; set => SetProperty(ref _overallCompletionRate, value); }

        public ObservableCollection<ChallengeSummaryVM> ChallengeItems { get; set; } = new();

        public AdChallengeReportViewModel()
        {
            LoadData();
        }

        public void LoadData()
        {
            using var db = new AppDbContext();
            var today = DateTime.Today;

            // 1. Lấy danh sách Challenge Active (StartDate <= Today <= EndDate hoặc EndDate null)
            var activeList = db.Challenges
                .Include(c => c.ChallengeTasks)
                .Where(c => (c.StartDate == null || c.StartDate <= today) &&
                            (c.EndDate == null || c.EndDate >= today))
                .ToList();

            ActiveChallenges = activeList.Count;

            // 2. Đếm số người tham gia các thử thách Active (Không đếm trùng lặp)
            var activeIds = activeList.Select(c => c.ChallengesID).ToList();
            TotalParticipants = db.UserChallenges
                .Where(uc => activeIds.Contains(uc.ChallengesID))
                .Select(uc => uc.UserID)
                .Distinct()
                .Count();

            // 3. Tính tỷ lệ hoàn thành All Challenges (Số người Progress = 100 / Tổng số người tham gia)
            var allUserChallenges = db.UserChallenges.ToList();
            int completedCount = allUserChallenges.Count(uc => uc.Progress >= 100);
            int totalRecords = allUserChallenges.Count;
            OverallCompletionRate = totalRecords > 0 ? (double)completedCount / totalRecords * 100 : 0;

            // 4. Chuẩn bị dữ liệu cho bảng hiển thị
            ChallengeItems.Clear();
            foreach (var c in activeList)
            {
                var participants = db.UserChallenges.Where(uc => uc.ChallengesID == c.ChallengesID).ToList();
                ChallengeItems.Add(new ChallengeSummaryVM
                {
                    Title = c.Title,
                    TimePeriod = $"{c.StartDate:dd/MM/yyyy} - {c.EndDate:dd/MM/yyyy}",
                    ParticipantCount = participants.Count,
                    AvgProgress = participants.Any() ? (double)participants.Average(p => p.Progress) : 0
                });
            }
        }
    }

    public class ChallengeSummaryVM
    {
        public string Title { get; set; }
        public string TimePeriod { get; set; }
        public int ParticipantCount { get; set; }
        public double AvgProgress { get; set; }
    }
}