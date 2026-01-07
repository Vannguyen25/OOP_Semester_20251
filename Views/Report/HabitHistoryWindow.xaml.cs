using System.Windows;
using OOP_Semester.ViewModels;

namespace OOP_Semester.Views.Report
{
    public partial class HabitHistoryWindow : Window
    {
        public HabitHistoryWindow(int habitId)
        {
            InitializeComponent();
            DataContext = new HabitHistoryViewModel(habitId);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}