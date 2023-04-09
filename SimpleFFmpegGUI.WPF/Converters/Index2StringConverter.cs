using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SimpleFFmpegGUI.WPF.Converters
{
    public class Index2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new ArgumentException("目标类型应为string", nameof(targetType));
            }
            if (value is not int)
            {
                throw new ArgumentException("源类型应为int", nameof(value));
            }
            string[] strs = null;
            if (parameter is string s1)
            {
                strs = s1.Split(';');
            }
            else if (parameter is string[] s2)
            {
                strs = s2;
            }
            else
            {
                throw new ArgumentException("parameter类型应为string或string[]", nameof(parameter));
            }
            return strs[(int)value];

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
