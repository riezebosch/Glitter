
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Glitter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated from WPF resources")]
    class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var color = ToColor((ObjectType)value);
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static Color ToColor(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.Tree:
                    return Color.FromRgb(0x70, 0xAD, 0x47);
                case ObjectType.Blob:
                    return Color.FromRgb(0x1B, 0xA1, 0xE2);
                case ObjectType.Commit:
                    return Color.FromRgb(0xED, 0x7D, 0x31);
                case ObjectType.Branch:
                    return Colors.Red;
                case ObjectType.Pack:
                    return Color.FromRgb(0xCC, 0x99, 0xFF);
                case ObjectType.Info:
                    return Colors.Violet;
                case ObjectType.Index:
                    return Colors.SeaShell;
                case ObjectType.Head:
                    return Colors.Black;
                default:
                    return Colors.Black;
            }
        }
    }
}
