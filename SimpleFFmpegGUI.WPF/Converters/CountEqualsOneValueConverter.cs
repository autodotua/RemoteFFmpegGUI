using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleFFmpegGUI.WPF.Converters
{
    public class CountEqualsOneValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (1.Equals(value))
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}