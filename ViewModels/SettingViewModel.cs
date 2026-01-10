using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OOP_Semester.Data;
using OOP_Semester.Models;

namespace OOP_Semester.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly User _user;

        // --- 1. Thông tin User ---
        private string _displayName;
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

        private string _account;
        public string Account { get => _account; set => SetProperty(ref _account, value); }

        private string _avatar;
        public string Avatar
        {
            get => _avatar;
            set { if (SetProperty(ref _avatar, value)) OnPropertyChanged(nameof(AvatarSource)); }
        }

        public string AvatarSource
        {
            get
            {
                if (string.IsNullOrEmpty(Avatar)) return "/Images/System/DefaultAvatar.png";
                try
                {
                    if (Avatar.StartsWith("/"))
                    {
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(path)) return path;
                    }
                }
                catch { }
                return Avatar;
            }
        }

        // Mật khẩu (Được gán từ SettingView.xaml.cs)
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";

        // --- 2. Pet & Time ---
        private string _petName;
        public string PetName { get => _petName; set => SetProperty(ref _petName, value); }

        private string _morningTime;
        public string MorningTime { get => _morningTime; set => SetProperty(ref _morningTime, value); }

        private string _afternoonTime;
        public string AfternoonTime { get => _afternoonTime; set => SetProperty(ref _afternoonTime, value); }

        private string _eveningTime;
        public string EveningTime { get => _eveningTime; set => SetProperty(ref _eveningTime, value); }

        private bool _vacationMode;
        public bool VacationMode
        {
            get => _vacationMode;
            set { if (SetProperty(ref _vacationMode, value)) SaveVacationModeToDb(); }
        }
        public event Action? InfoUpdated;

        // --- 3. Phản hồi & Đánh giá (THÊM MỚI) ---
        private string _feedbackContent;
        public string FeedbackContent { get => _feedbackContent; set => SetProperty(ref _feedbackContent, value); }

        private int _feedbackRating;
        public int FeedbackRating { get => _feedbackRating; set => SetProperty(ref _feedbackRating, value); }

        // --- COMMANDS ---
        public ICommand ChooseAvatarCommand { get; }
        public ICommand RemoveAvatarCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand SavePetCommand { get; }
        public ICommand SaveTimesCommand { get; }
        public ICommand SendFeedbackCommand { get; }

        public SettingViewModel(User user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));

            // Load Data
            DisplayName = _user.Name;
            Account = _user.Account;
            Avatar = _user.Avatar;
            VacationMode = _user.VacationMode;

            MorningTime = _user.MorningTime?.ToString(@"hh\:mm") ?? "07:00";
            AfternoonTime = _user.AfternoonTime?.ToString(@"hh\:mm") ?? "12:00";
            EveningTime = _user.EveningTime?.ToString(@"hh\:mm") ?? "19:00";

            LoadPetFromDb();

            // Init Commands
            ChooseAvatarCommand = new RelayCommand(o =>
            {
                var dlg = new OpenFileDialog { Filter = "Image|*.jpg;*.png;*.jpeg" };
                if (dlg.ShowDialog() == true) Avatar = dlg.FileName;
            });
            RemoveAvatarCommand = new RelayCommand(o => Avatar = null);

            SaveCommand = new RelayCommand(o => HandleSaveInfo());
            SavePetCommand = new RelayCommand(o => HandleSavePet());
            SaveTimesCommand = new RelayCommand(o => HandleSaveTimes());


            SendFeedbackCommand = new RelayCommand(o =>
            {
                // 1. Validate dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(FeedbackContent) || FeedbackRating == 0)
                {
                    MessageBox.Show("Vui lòng nhập nội dung và chọn số sao đánh giá trước khi gửi.");
                    return;
                }

                try
                {
                    // 2. Mở kết nối Database và lưu
                    using (var ctx = new AppDbContext())
                    {
                        var newFeedback = new Feedback
                        {
                            UserID = _user.UserID,       // Lấy ID của user đang đăng nhập
                            Content = FeedbackContent,   // Nội dung từ TextBox
                            Rating = (byte) FeedbackRating,     // Số sao từ RadioButton
                            CreatedAt = DateTime.Now     // Thời gian hiện tại
                        };

                        ctx.Feedbacks.Add(newFeedback);  // Thêm vào hàng đợi
                        ctx.SaveChanges();               // Lưu xuống MySQL
                    }

                    // 3. Thông báo thành công & Reset form
                    MessageBox.Show("Cảm ơn bạn! Phản hồi đã được ghi nhận.");
                    FeedbackContent = "";
                    FeedbackRating = 0;

                    // Lưu ý: Vì Rating xử lý ở Code-behind nên UI RadioButton có thể chưa tắt chọn ngay,
                    // nhưng về mặt dữ liệu thì đã reset về 0.
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi gửi phản hồi: " + ex.Message);
                }
            });
        }

        // --- CÁC HÀM XỬ LÝ DB (Giữ nguyên logic cũ) ---
        private void LoadPetFromDb()
        {
            try
            {
                using (var ctx = new AppDbContext())
                {
                    var pet = ctx.Pets.FirstOrDefault(p => p.UserID == _user.UserID);
                    PetName = pet?.Name ?? "";
                }
            }
            catch { PetName = ""; }
        }

        private void HandleSaveInfo()
        {
            try
            {
                using (var ctx = new AppDbContext())
                {
                    // Tìm user trong DB
                    var u = ctx.Users.Find(_user.UserID);
                    if (u == null) return;

                    // --- 1. CẬP NHẬT THÔNG TIN CƠ BẢN (Luôn thực hiện) ---
                    u.Name = DisplayName;
                    u.Avatar = Avatar;

                    // --- 2. XỬ LÝ ĐỔI MẬT KHẨU (Nếu có nhập) ---
                    bool passwordChanged = false;

                    if (!string.IsNullOrEmpty(NewPassword))
                    {
                        // Validate: Bắt buộc phải nhập mật khẩu cũ
                        if (string.IsNullOrEmpty(OldPassword))
                        {
                            MessageBox.Show("Vui lòng nhập mật khẩu hiện tại để xác nhận đổi mật khẩu.");
                            return; // Dừng lại, không lưu gì cả để tránh lỗi logic
                        }

                        // Validate: Mật khẩu cũ phải đúng (Lưu ý: nên dùng hash nếu thực tế)
                        if (u.Password != OldPassword)
                        {
                            MessageBox.Show("Mật khẩu hiện tại không đúng.");
                            return; // Dừng lại
                        }

                        // Hợp lệ -> Gán mật khẩu mới
                        u.Password = NewPassword;
                        passwordChanged = true;
                    }

                    // --- 3. LƯU XUỐNG DATABASE ---
                    ctx.SaveChanges();

                    // --- 4. CẬP NHẬT GIAO DIỆN & LOCAL ---
                    _user.Name = DisplayName;
                    _user.Avatar = Avatar;
               
                    InfoUpdated?.Invoke();
                    GlobalChangeHub.RaiseDisplayNameChanged(this, _user.Name);
                    GlobalChangeHub.RaiseAvatarChanged(this, _user.Avatar);
                    if (passwordChanged)
                    {
                        MessageBox.Show("Cập nhật thông tin và đổi mật khẩu thành công!");
                        // Reset ô nhập mật khẩu
                        OldPassword = "";
                        NewPassword = "";
                    }
                    else
                    {
                        MessageBox.Show("Đã lưu thông tin cá nhân!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message);
            }
        }

        private void HandleSavePet()
        {
            using (var ctx = new AppDbContext())
            {
                var pet = ctx.Pets.FirstOrDefault(p => p.UserID == _user.UserID);
                if (pet == null) ctx.Pets.Add(new Pet { UserID = _user.UserID, Name = PetName, CreatedAt = DateTime.Now });
                else pet.Name = PetName;
                ctx.SaveChanges();
                MessageBox.Show("Đã lưu tên Thú cưng!");
            }
        }

        private void HandleSaveTimes()
        {
            using (var ctx = new AppDbContext())
            {
                var u = ctx.Users.Find(_user.UserID);
                if (u != null)
                {
                    if (TimeSpan.TryParse(MorningTime, out var m)) u.MorningTime = m;
                    if (TimeSpan.TryParse(AfternoonTime, out var a)) u.AfternoonTime = a;
                    if (TimeSpan.TryParse(EveningTime, out var e)) u.EveningTime = e;
                    ctx.SaveChanges();
                    MessageBox.Show("Đã lưu mốc thời gian!");
                }
            }
        }

        private void SaveVacationModeToDb()
        {
            using (var ctx = new AppDbContext())
            {
                var u = ctx.Users.Find(_user.UserID);
                if (u != null) { u.VacationMode = VacationMode; ctx.SaveChanges(); }
            }
        }
    }
}