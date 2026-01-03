using System.Windows;
using System.Windows.Controls;

namespace OOP_Semester.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        // Logic xử lý nút Hamburger
        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra chiều rộng hiện tại của cột Menu
            if (MenuColumn.Width.Value > 0)
            {
                // Nếu đang mở -> Thu lại về 0
                MenuColumn.Width = new GridLength(0);
            }
            else
            {
                // Nếu đang đóng -> Mở ra 260
                MenuColumn.Width = new GridLength(260);
            }
        }
    }
}