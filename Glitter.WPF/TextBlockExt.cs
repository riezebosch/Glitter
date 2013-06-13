using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Glitter.WPF
{
    static class TextBlockExt
    {
        public static void Blink(this IEnumerable<TextBlock> items)
        {
            var background = new SolidColorBrush(Colors.Black);
            foreach (var item in items)
            {
                item.Blink(background);
            }
        }

        private static async void Blink(this TextBlock item, Brush background)
        {
            var original = item.Background;

            for (int i = 0; i < 5; i++)
            {
                item.Background = background;
                await Task.Delay(500);

                item.Background = original;
                await Task.Delay(500); 
            }
        }
    }
}
