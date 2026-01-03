using System.Windows;
using System.Windows.Threading; // Cần dòng này để dùng DispatcherUnhandledException
using OOP_Semester.ViewModels;
using OOP_Semester.Views;

namespace OOP_Semester
{
    public partial class App : Application
    {
        // 1. THÊM CONSTRUCTOR ĐỂ BẮT LỖI
        public App()
        {
            // Đăng ký sự kiện: "Nếu có lỗi chưa được xử lý, hãy chạy hàm App_DispatcherUnhandledException"
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        // 2. HÀM XỬ LÝ LỖI (HIỆN THÔNG BÁO THAY VÌ TẮT APP)
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = $"LỖI NGHIÊM TRỌNG (Crash):\n\n{e.Exception.Message}";

            // Nếu có lỗi con bên trong, hiện luôn để dễ sửa
            if (e.Exception.InnerException != null)
            {
                errorMessage += $"\n\nChi tiết (Inner): {e.Exception.InnerException.Message}";
            }
            
            // Hiện StackTrace để biết lỗi ở dòng nào
            // errorMessage += $"\n\nStack Trace:\n{e.Exception.StackTrace}";

            MessageBox.Show(errorMessage, "Báo cáo lỗi (App.xaml.cs)", MessageBoxButton.OK, MessageBoxImage.Error);

            // [QUAN TRỌNG] Dòng này ngăn ứng dụng bị tắt đột ngột
            e.Handled = true;
        }

        // 3. LOGIC KHỞI ĐỘNG CỦA BẠN (GIỮ NGUYÊN)
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Bọc thêm try-catch ở đây cho chắc ăn
            try
            {
                var mainWindow = new MainWindow();
                var mainViewModel = new MainViewModel();
                mainWindow.DataContext = mainViewModel;
                mainWindow.Show();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi khởi động MainWindow: " + ex.Message);
            }
        }
    }
}