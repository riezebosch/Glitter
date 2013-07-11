
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Glitter
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated from WPF resources")]
    class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var type = (ObjectType)value;
            if (type == ObjectType.Unknown)
            {
                return CreateARectangleWithDrawingBrush();
            }
            var color = ToColor(type);
            return new SolidColorBrush(color);
        }

        private Brush CreateARectangleWithDrawingBrush()
        {
            // Create a DrawingBrush
            DrawingBrush blackBrush = new DrawingBrush();
            // Create a Geometry with white background
            GeometryDrawing backgroundSquare =
                new GeometryDrawing(
                    Brushes.DarkGray,
                    null,
                    new RectangleGeometry(new Rect(0, 0, 400, 400)));
            
            // Create a GeometryGroup that will be added to Geometry
            GeometryGroup gGroup = new GeometryGroup();
            gGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 200, 200)));
            gGroup.Children.Add(new RectangleGeometry(new Rect(200, 200, 200, 200)));
            // Create a GeomertyDrawing
            GeometryDrawing checkers = new GeometryDrawing(new SolidColorBrush(Colors.Gray), null, gGroup);

            DrawingGroup checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            blackBrush.Drawing = checkersDrawingGroup;

            // Set Viewport and TimeMode
            blackBrush.Viewport = new Rect(0, 0, 0.1, 0.2);
            blackBrush.TileMode = TileMode.Tile;

            return blackBrush;
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
                    return Color.FromRgb(0x99, 0xFF, 0x33);
                case ObjectType.Blob:
                    return Color.FromRgb(0x00, 0x78, 0xC9);
                case ObjectType.Commit:
                    return Color.FromRgb(0xA8, 0x00, 0x00);
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
