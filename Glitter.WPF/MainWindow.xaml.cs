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
                !fi.Exists)
            {
                return;
            }

            Dispatcher.Invoke(() => SP.Children.Add(new TextBlock
            {
                Text = ObjectFileParser.ParseFile(fi) ?? "Error parsing file",
                Background = new SolidColorBrush(Colors.Green),
                Foreground = new SolidColorBrush(Colors.White)
            }));

            Dispatcher.Invoke(() => SV.ScrollToBottom());
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

            watcher.Path = dir.FullName;
            watcher.EnableRaisingEvents = true;

            Banner.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
