using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Glitter.WPF
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        private static readonly char[] delimiters = new[] { ' ', '\t', '\0', '\r', '\n' };

        public UserControl1()
        {
            this.InitializeComponent();

            tb.MouseLeftButtonDown += (o, ed) =>
            {
                var pos = tb.GetPositionFromPoint(ed.GetPosition(tb), false);
                if (pos != null)
                {
                    string left = pos.GetTextInRun(LogicalDirection.Backward);

                    int fromDelimiter = left.LastIndexOfAny(delimiters);
                    if (fromDelimiter > 0)
                    {
                        left = left.Substring(fromDelimiter + 1);
                    }

                    string right = pos.GetTextInRun(LogicalDirection.Forward);
                    int untilDelimiter = right.IndexOfAny(delimiters);
                    if (untilDelimiter > 0)
                    {
                        right = right.Substring(0, untilDelimiter);
                    }

                    string id = left + right;
                    IdentifierClick(this, id);
                }
            };
        }

        public string Text
        {
            get { return tb.Text; }
            set { tb.Text = value; }
        }

        public void Blink()
        {
            BeginStoryboard(FindResource("Blink") as Storyboard);
        }

        public event EventHandler<string> IdentifierClick;
    }
}