using System.Windows;
using System.Windows.Controls;
using OOP_Semester.ViewModels;

namespace OOP_Semester.Views
{
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            InitializeComponent();
        }

        // 1. Xử lý khi gõ Mật khẩu
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SettingViewModel vm)
            {
                var pb = sender as PasswordBox;
                if (pb == null) return;

                // Kiểm tra tên control để gán vào đúng biến trong ViewModel
                if (pb.Name == "pbOldPass")
                {
                    vm.OldPassword = pb.Password;
                }
                else if (pb.Name == "pbNewPass")
                {
                    vm.NewPassword = pb.Password;
                }
            }
        }

        // 2. Xử lý khi bấm chọn Sao (Rating)
        private void Rating_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SettingViewModel vm)
            {
                var rb = sender as RadioButton;
                if (rb != null && rb.Tag != null)
                {
                    // Lấy số từ thuộc tính Tag="1", Tag="2"... trong XAML
                    if (int.TryParse(rb.Tag.ToString(), out int rating))
                    {
                        vm.FeedbackRating = rating;
                    }
                }
            }
        }
    }
}