using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OOP_Semester.Converters // Đổi namespace theo project của bạn
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        // Hàm chuyển từ Code -> Giao diện
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue == true)
            {
                return Visibility.Visible; // Nếu true thì Hiện
            }
            return Visibility.Collapsed;   // Nếu false thì Ẩn
        }

        // Hàm chuyển ngược từ Giao diện -> Code (ít dùng, thường để throw exception)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}