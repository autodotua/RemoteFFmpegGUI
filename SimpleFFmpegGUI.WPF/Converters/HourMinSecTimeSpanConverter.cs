using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SimpleFFmpegGUI.WPF.Converters
{
    public class HourMinSecTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value is TimeSpan);
            TimeSpan t = (TimeSpan)value;
            if("1".Equals(parameter))
            {
                return $"{t.Days * 24 + t.Hours}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds/10:00}";
            }
            return $"{t.Days * 24 + t.Hours}:{t.Minutes:00}:{t.Seconds:00}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
