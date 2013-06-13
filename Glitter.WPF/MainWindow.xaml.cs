using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Glitter.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private char[] delimiters = new[] { ' ', '\t', '\0', '\r', '\n' };
        FileSystemWatcher watcher = new FileSystemWatcher();

        public MainWindow()
        {
            InitializeComponent();

            watcher.Changed += watcher_Changed;
            watcher.Created += watcher_Changed;
            watcher.Renamed += watcher_Changed;

            watcher.IncludeSubdirectories = true;
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var fi = new FileInfo(e.FullPath);
            Dispatcher.Invoke(() => SP.Children.Add(new TextBlock { Text = string.Format("{0}: {1}", e.ChangeType, fi.FullName) }));

            if (fi.Name == "index" ||
                fi.Extension == ".lock" ||
                fi.Name.StartsWith("tmp") ||
                !fi.Exists)
            {
                return;
            }

            var obj = ObjectFileParser.ParseFile(fi);
            if (obj != null)
            {
                Dispatcher.Invoke(() =>
                    {
                        
                        {
                            var tb = new TextBlock() { Text = obj.Body ?? "Error parsing file", Background = ToColorBrush(obj.Header), Foreground = new SolidColorBrush(Colors.White) };
                            tb.MouseLeftButtonDown += (o, ed) =>
                                {
                                    var pos = tb.GetPositionFromPoint(ed.GetPosition(tb), false);
                                    string text = pos.GetTextInRun(LogicalDirection.Backward);
                                    int textLastIndexOfAny = text.LastIndexOfAny(delimiters);
                                    if (textLastIndexOfAny > 0)
                                    {
                                        text = text.Substring(textLastIndexOfAny);
                                    }

                                    string right = pos.GetTextInRun(LogicalDirection.Forward);
                                    int rightIndexOfAny = right.IndexOfAny(delimiters);
                                    if (rightIndexOfAny > 0)
                                    {
                                        right = right.Substring(0, rightIndexOfAny); 
                                    }

                                    MessageBox.Show(text + right);
                                };
                            SP.Children.Add(tb);
                        }
                    });
                        

                Dispatcher.Invoke(() => SV.ScrollToBottom()); 
            }
        }

        

        private void allowdrop_DragEnter(object sender, DragEventArgs e)
        {

            if (ExtractRepositoryDirectory(e) != null)
            {
                e.Effects = DragDropEffects.All;
            }

        }

        private static DirectoryInfo ExtractRepositoryDirectory(DragEventArgs e)
        {
            DirectoryInfo dir;
            var data = ((string[])e.Data.GetData(DataFormats.FileDrop)).SingleOrDefault();
            if (data != null)
            {
                dir = new DirectoryInfo(data);
                if (dir.Exists && dir.Name == ".git")
                {
                    return dir;
                }

                return dir.GetDirectories(".git").FirstOrDefault();
            }

            return null;
        }

        private void allowdrop_Drop(object sender, DragEventArgs e)
        {
            var dir = ExtractRepositoryDirectory(e);
            Title = dir.FullName;

            watcher.Path = dir.FullName;
            watcher.EnableRaisingEvents = true;

            Banner.Visibility = System.Windows.Visibility.Hidden;
        }

        private static Brush ToColorBrush(ObjectHeader header)
        {
            if (header == null)
            {
                return new SolidColorBrush(Colors.DarkGray);
            }

            switch (header.Type)
            {
                case ObjectType.Tree:
                    return new SolidColorBrush(Color.FromRgb(0x70, 0xAD, 0x47));
                case ObjectType.Blob:
                    return new SolidColorBrush(Color.FromRgb(0x1B, 0xA1, 0xE2));
                case ObjectType.Commit:
                    return new SolidColorBrush(Color.FromRgb(0xED, 0x7D, 0x31));
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

    }
}
