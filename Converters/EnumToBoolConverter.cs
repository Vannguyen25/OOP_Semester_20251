using System;
using System.Globalization;
using System.Windows.Data;

namespace OOP_Semester.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        // Kiểm tra: Nếu DailySession (ViewModel) == Parameter (View truyền vào) thì trả về True
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        // Cập nhật ngược lại: Khi chọn RadioButton -> Gán giá trị Parameter vào ViewModel
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }
    }
}