using Microsoft.EntityFrameworkCore;
using OOP_Semester.Data;
using OOP_Semester.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace OOP_Semester.ViewModels.Report
{
    public class GoldTransactionReportViewModel : ViewModelBase
    {
        private readonly User _user;
        public ObservableCollection<GoldTransaction> Transactions { get; set; } = new();

        // Các thuộc tính thống kê
        private int _totalIncome;
        public int TotalIncome { get => _totalIncome; set => SetProperty(ref _totalIncome, value); }

        private int _totalExpense;
        public int TotalExpense { get => _totalExpense; set => SetProperty(ref _totalExpense, value); }

        private int _currentBalance;
        public int CurrentBalance { get => _currentBalance; set => SetProperty(ref _currentBalance, value); }

        // Filter Properties
        private DateTime _startDate = DateTime.Today.AddDays(-30);
        public DateTime StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        public ICommand FilterCommand { get; }

        public GoldTransactionReportViewModel(User user)
        {
            _user = user;
            FilterCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        public void LoadData()
        {
            using var db = new AppDbContext();
            var list = db.GoldTransactions
                .Where(t => t.UserID == _user.UserID &&
                            t.TransactionDate.Date >= StartDate.Date &&
                            t.TransactionDate.Date <= EndDate.Date)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            Transactions.Clear();
            foreach (var t in list) Transactions.Add(t);

            // Tính toán tổng thu chi
            TotalIncome = list.Where(t => t.Amount > 0).Sum(t => t.Amount);
            TotalExpense = Math.Abs(list.Where(t => t.Amount < 0).Sum(t => t.Amount));
            CurrentBalance = _user.GoldAmount ?? 0;
        }
    }
}