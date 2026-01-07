using OOP_Semester.Data;
using OOP_Semester.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOP_Semester.ViewModels.Report
{
    public class AdUserReportViewModel : ViewModelBase
    {
        private int _totalUsers;
        public int TotalUsers { get => _totalUsers; set => SetProperty(ref _totalUsers, value); }

        private int _newUsersThisMonth;
        public int NewUsersThisMonth { get => _newUsersThisMonth; set => SetProperty(ref _newUsersThisMonth, value); }

        private double _growthRate;
        public double GrowthRate { get => _growthRate; set => SetProperty(ref _growthRate, value); }

        public ObservableCollection<UserSummaryVM> RecentUsers { get; set; } = new();
        public ObservableCollection<UserChartItem> MonthlyGrowth { get; set; } = new();

        public AdUserReportViewModel()
        {
            LoadUserData();
        }

        public void LoadUserData()
        {
            using var db = new AppDbContext();
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);

            var allUsers = db.Users.ToList();
            TotalUsers = allUsers.Count;

            // Tính toán thống kê tháng hiện tại và tháng trước
            NewUsersThisMonth = allUsers.Count(u => u.CreatedAt >= startOfMonth);
            int lastMonthUsers = allUsers.Count(u => u.CreatedAt >= startOfLastMonth && u.CreatedAt < startOfMonth);

            if (lastMonthUsers > 0)
                GrowthRate = ((double)(NewUsersThisMonth - lastMonthUsers) / lastMonthUsers) * 100;
            else
                GrowthRate = NewUsersThisMonth > 0 ? 100 : 0;

            // 1. Nạp danh sách 10 người dùng mới nhất
            RecentUsers.Clear();
            var latest = allUsers.OrderByDescending(u => u.CreatedAt).Take(10);
            foreach (var u in latest)
            {
                RecentUsers.Add(new UserSummaryVM
                {
                    Username = u.Account,
                    Name = u.Name,
                    CreatedAt = u.CreatedAt.ToString("dd/MM/yyyy"),
                    GoldBalance = u.GoldAmount ?? 0,
                    Role = u.Role.ToString()
                });
            }

            // 2. Nạp dữ liệu biểu đồ 6 tháng gần nhất
            MonthlyGrowth.Clear();
            int maxInChart = 0;
            var tempChartData = new System.Collections.Generic.List<UserChartItem>();

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                int count = allUsers.Count(u => u.CreatedAt.Month == targetMonth.Month && u.CreatedAt.Year == targetMonth.Year);
                if (count > maxInChart) maxInChart = count;

                tempChartData.Add(new UserChartItem
                {
                    MonthName = targetMonth.ToString("MM/yyyy"),
                    UserCount = count
                });
            }

            // Tính toán chiều cao cột theo tỷ lệ (Max height = 150)
            foreach (var item in tempChartData)
            {
                item.BarHeight = maxInChart > 0 ? ((double)item.UserCount / maxInChart) * 150 + 5 : 5;
                MonthlyGrowth.Add(item);
            }
        }
    }

    public class UserSummaryVM
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string CreatedAt { get; set; }
        public int GoldBalance { get; set; }
        public string Role { get; set; }
    }

    public class UserChartItem
    {
        public string MonthName { get; set; }
        public int UserCount { get; set; }
        public double BarHeight { get; set; }
    }
}