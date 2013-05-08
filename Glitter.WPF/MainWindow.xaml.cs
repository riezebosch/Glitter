using GitSharp.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private Repository _repository;
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

            var temp = string.Format("{0}{1}", fi.Directory.Name, fi.Name);

            if (ObjectId.IsId(temp))
            {
                var id = ObjectId.FromString(temp);

                ObjectLoader o = null;
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        o = _repository.OpenObject(id);
                    }
                    catch (IOException)
                    {
                    }
                }

                if (o == null)
                {
                    return;
                }

                string text = string.Empty;
                switch ((ObjectType)o.Type)
                {
                    case ObjectType.Bad:
                        break;
                    case ObjectType.Blob:
                        text = ASCIIEncoding.ASCII.GetString(_repository.OpenBlob(id).Bytes);
                        break;
                    case ObjectType.Commit:
                        var commit = _repository.MapCommit(id);
                        text = string.Format("author: {0}{3}message: {1}{3}tree: {2}", 
                            commit.Author.Name, commit.Message, commit.TreeId.Name, Environment.NewLine);
                        break;
                    case ObjectType.DeltaBase:
                        break;
                    case ObjectType.Extension:
                        break;
                    case ObjectType.ObjectType5:
                        break;
                    case ObjectType.OffsetDelta:
                        break;
                    case ObjectType.ReferenceDelta:
                        break;
                    case ObjectType.Tag:
                        var tag = _repository.MapTag(fi.Name);
                        text = tag.ToString();
                        break;
                    case ObjectType.Tree:
                        var tree = _repository.MapTree(id);
                        //tree.
                        text = TreeString(tree, new StringBuilder(), 0).ToString();
                        break;
                    case ObjectType.Unknown:
                        break;
                    default:
                        break;
                }

                Dispatcher.Invoke(() => SP.Children.Add(new TextBlock 
                {
                    Text = text,
                    Background = new SolidColorBrush(Colors.Green),
                    Foreground = new SolidColorBrush(Colors.White)
                }));
            }

            Dispatcher.Invoke(() => SV.ScrollToBottom());
        }

        private StringBuilder TreeString(TreeEntry entry, StringBuilder builder, int level)
        {
            builder.Append("".PadLeft(level));
            if (!string.IsNullOrEmpty(entry.Name))
            {
                builder.Append(entry.Name);
                builder.Append(" ");
            }
            builder.Append(entry.Id.Name);
            builder.AppendLine();

            if (entry is Tree)
            {
                foreach (var child in ((Tree)entry).Members)
                {
                    TreeString(child, builder, level + 3);
                }
            }

            return builder;
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
            _repository = new Repository(dir);
            watcher.Path = dir.FullName;
            watcher.EnableRaisingEvents = true;

            Banner.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
