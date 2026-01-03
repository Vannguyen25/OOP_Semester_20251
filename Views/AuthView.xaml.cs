using System.Windows;
using System.Windows.Controls;
using OOP_Semester.ViewModels; 

namespace OOP_Semester.Views
{
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra DataContext
            if (this.DataContext is AuthViewModel viewModel)
            {
                // Lấy password chính (dùng chung cho cả 2 mode)
                string pass = txtPassword != null ? txtPassword.Password : "";

                // 2. CHECK MODE TRƯỚC KHI GỌI HÀM
                if (viewModel.IsRegisterMode)
                {
                    // --- MODE ĐĂNG KÝ (Register) ---
                    // Lấy thêm confirm pass
                    string confirmPass = txtConfirmPassword != null ? txtConfirmPassword.Password : "";

                    // Gọi overload 2 tham số
                    viewModel.HandleSubmit(pass, confirmPass);
                }
                else
                {
                    // --- MODE ĐĂNG NHẬP (Login) ---
                    // Chỉ gọi overload 1 tham số -> Code sạch hơn nhiều
                    viewModel.HandleSubmit(pass);

                    // Xóa password sau khi bấm login để bảo mật (nếu muốn)
                    if (txtPassword != null) txtPassword.Clear();
                }
            }
        }
    }
}