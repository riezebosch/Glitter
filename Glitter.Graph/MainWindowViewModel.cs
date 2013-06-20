using Glitter.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Glitter.Graph
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FileGraph _graph = new FileGraph(false);
        public FileGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        FileSystemWatcher _watcher;

        public MainWindowViewModel()
        {
            var di = new DirectoryInfo(@"C:\git\demo\.git");

            _watcher = new FileSystemWatcher(di.FullName) { IncludeSubdirectories = true };
            _watcher.Created += (o, e) => Application.Current.Dispatcher.Invoke(() => AddFileToGraph(new FileInfo(e.FullPath)));
            _watcher.Changed += (o, e) => Application.Current.Dispatcher.Invoke(() => AddFileToGraph(new FileInfo(e.FullPath)));
            _watcher.Renamed += (o, e) => Application.Current.Dispatcher.Invoke(() => UpdateFileOnGraph(new FileInfo(e.OldFullPath), new FileInfo(e.FullPath)));
            _watcher.Deleted += (o, e) => Application.Current.Dispatcher.Invoke(() => RemoveFileFromGraph(new FileInfo(e.FullPath)));
            _watcher.EnableRaisingEvents = true;

            foreach (var item in new DirectoryInfo(Path.Combine(di.FullName, "objects")).EnumerateFiles("*.*", SearchOption.AllDirectories)
                .Concat(new DirectoryInfo(Path.Combine(di.FullName, "refs")).EnumerateFiles("*.*", SearchOption.AllDirectories))
                .Concat(new[] { new FileInfo(Path.Combine(di.FullName, "index")), new FileInfo(Path.Combine(di.FullName, "HEAD")) }))
            {
                AddFileToGraph(item);
            }

            OnPropertyChanged("Graph");
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private void RemoveFileFromGraph(FileInfo fi)
        {
            string id = fi.Directory.Name + fi.Name;
            var target = _graph.Vertices.FirstOrDefault(v => v.Id == id || v.Id == fi.Name);

            if (target != null)
            {
                _graph.RemoveVertex(target);
                OnPropertyChanged("Graph");
            }
        }

        private void AddFileToGraph(FileInfo fi)
        {
            if (fi.Ignore(_watcher.Path))
            {
                return;
            }

            GitObject go = ObjectFileParser.ParseFile(fi);
            if (go != null)
            {
                var source = GetOrAddTargetById(go.Id);
                source.Body = go.Body;

                source.ObjectType = go.Type;
                foreach (var reference in go.References)
                {
                    var target = GetOrAddTargetById(reference);
                    _graph.AddEdge(new FileEdge(source, target));
                }

                _graph.RemoveEdgeIf(e => e.Source == source && !go.References.Contains(e.Target.Id));
            }
        }

        private void UpdateFileOnGraph(FileInfo oldFile, FileInfo newFile)
        {
            if (newFile.Ignore(_watcher.Path))
            {
                return;
            }

            string id = oldFile.Directory.Name + oldFile.Name;
            var source = _graph.Vertices.FirstOrDefault(v => v.Id == id || v.Id == oldFile.Name);

            if (source != null)
            {
                source.Id = id;
            }
            else
            {
                AddFileToGraph(newFile);
            }
        }



        private FileVertex GetOrAddTargetById(string rid)
        {
            var target = _graph.Vertices.FirstOrDefault(v => v.Id == rid);

            if (target == null)
            {
                target = new FileVertex { Id = rid };
                _graph.AddVertex(target);
            }
            return target;
        }
    }
}
