using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleFFmpegGUI.WPF
{
    public class EmptyIfZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is 0 or 0d)
            {
                return "";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}