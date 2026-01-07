using System;
using System.Globalization;
using System.Windows.Data;

namespace OOP_Semester.Converters
{
    public class ColorToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Logic tạm thời
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}