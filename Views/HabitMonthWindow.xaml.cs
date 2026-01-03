using System.Windows;
using OOP_Semester.ViewModels;

namespace OOP_Semester.Views
{
    public partial class HabitMonthWindow : Window
    {
        public HabitMonthWindow(int habitId)
        {
            InitializeComponent();
            this.DataContext = new HabitMonthWindowViewModel(habitId);
        }
    }
}