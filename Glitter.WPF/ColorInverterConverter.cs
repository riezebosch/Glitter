using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Glitter.WPF
{
    class ColorInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ToColor(value as GitObject);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }


        private static Color ToColor(GitObject go)
        {
            if (go == null)
            {
                return Colors.DarkGray;
            }

            switch (go.Type)
            {
                case ObjectType.Tree:
                    return Color.FromRgb(0x70, 0xAD, 0x47);
                case ObjectType.Blob:
                    return Color.FromRgb(0x1B, 0xA1, 0xE2);
                case ObjectType.Commit:
                    return Color.FromRgb(0xED, 0x7D, 0x31);
                default:
                    return Colors.White;
            }
        }
    }
}
