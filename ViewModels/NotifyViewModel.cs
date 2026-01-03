using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;
using OOP_Semester.Models;
using OOP_Semester.Data;

namespace OOP_Semester.ViewModels
{
    public class NotifyViewModel : ViewModelBase
    {
        // Danh sách thông báo
        public ObservableCollection<Notify> Notifications { get; set; }

        // Command xử lý khi click vào thông báo
        public ICommand MarkAsReadCommand { get; set; }

        private User _user;

        public NotifyViewModel(User user)
        {
            _user = user;
            Notifications = new ObservableCollection<Notify>();

            // Khởi tạo Command
            MarkAsReadCommand = new RelayCommand<Notify>((notify) => MarkAsRead(notify));

            LoadNotifications();
        }

        private void LoadNotifications()
        {
            using (var context = new AppDbContext())
            {
                // Lấy danh sách, sắp xếp mới nhất lên đầu
                var list = context.Notify
                                  .Where(n => n.UserID == _user.UserID)
                                  .OrderByDescending(n => n.NotifyTime)
                                  .ToList();

                Notifications.Clear();
                foreach (var item in list)
                {
                    Notifications.Add(item);
                }
            }
        }

        private void MarkAsRead(Notify notify)
        {
            if (notify == null || notify.IsRead) return; // Đã đọc rồi thì thôi

            // 1. Cập nhật Database
            using (var context = new AppDbContext())
            {
                var itemInDb = context.Notify.FirstOrDefault(n => n.NotifyID == notify.NotifyID);
                if (itemInDb != null)
                {
                    itemInDb.IsRead = true;
                    context.SaveChanges();
                }
            }

            // 2. Cập nhật trên giao diện (Model sẽ tự báo đổi màu nhờ INotifyPropertyChanged)
            notify.IsRead = true;
        }
    }

    // --- Helper Class: RelayCommand (Để xử lý sự kiện Click) ---
    // Nếu bạn đã có file RelayCommand riêng thì xóa đoạn class bên dưới đi
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}