using System;
using System.Windows;
using System.Windows.Input;

namespace OOP_Semester.ViewModels
{
    /// <summary>
    /// Hub static duy nhất để đồng bộ thay đổi giữa nhiều ViewModel.
    /// - Coins: Habit/Challenge cộng, Shop trừ -> Today/Shop cập nhật
    /// - Pet: Feeding đổi pet hoặc feed -> Today cập nhật ảnh/stats
    /// - Inventory: Shop mua food hoặc Feeding tiêu food -> Feeding reload
    /// - Avatar/DisplayName: Setting đổi -> Home header + Today greeting cập nhật
    /// </summary>
    public static class GlobalChangeHub
    {
        private static readonly object _lock = new object();

        public static int CurrentCoins { get; private set; }
        public static string? CurrentAvatar { get; private set; }
        public static string? CurrentDisplayName { get; private set; }
        public static class NavigationHub
        {
            // Cầu nối để yêu cầu chuyển trang
            public static ICommand RequestNavigateCommand { get; set; }

            public static void NavigateTo(string pageTag)
            {
                RequestNavigateCommand?.Execute(pageTag);
            }
        }

        // sender để các VM có thể tự bỏ qua event do chính mình bắn (nếu muốn)
        public static event Action<object, int>? CoinsChanged;
        public static event Action<object>? PetChanged;
        public static event Action<object>? InventoryChanged;
        public static event Action<object, string?>? AvatarChanged;
        public static event Action<object, string?>? DisplayNameChanged;
        public static event Action<object, int>? GoldChanged;
 
        public static void InitializeFromUser(Models.User user)
        {
            if (user == null) return;
            lock (_lock)
            {
                CurrentCoins = user.GoldAmount ?? 0;
                CurrentAvatar = user.Avatar;
                CurrentDisplayName = user.Name;
            }
        }

        public static void RaiseCoinsChanged(object sender, int newCoins)
        {
            lock (_lock) CurrentCoins = newCoins;
            InvokeOnUI(() => CoinsChanged?.Invoke(sender, newCoins));
        }


        public static void RaiseAvatarChanged(object sender, string? avatar)
        {
            lock (_lock) CurrentAvatar = avatar;
            InvokeOnUI(() => AvatarChanged?.Invoke(sender, avatar));
        }

        public static void RaiseDisplayNameChanged(object sender, string? displayName)
        {
            lock (_lock) CurrentDisplayName = displayName;
            InvokeOnUI(() => DisplayNameChanged?.Invoke(sender, displayName));
        }
        

        private static void RunOnUI(Action action)
        {
            try
            {
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null) { action(); return; }

                if (dispatcher.CheckAccess()) action();
                else dispatcher.Invoke(action);
            }
            catch
            {
                // Không làm app crash nếu dispatcher lỗi
                action();
            }
        }

        public static void RaiseGoldChanged(object sender, int newGold)
            => RunOnUI(() => GoldChanged?.Invoke(sender, newGold));

        public static void RaiseInventoryChanged(object sender)
            => RunOnUI(() => InventoryChanged?.Invoke(sender));

        public static void RaisePetChanged(object sender)
            => RunOnUI(() => PetChanged?.Invoke(sender));

        private static void InvokeOnUI(Action action)
        {
            try
            {
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null || dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    dispatcher.BeginInvoke(action);
                }
            }
            catch
            {
                // tránh crash do dispatcher/app lifecycle
                try { action(); } catch { }
            }
        }
    }
}
