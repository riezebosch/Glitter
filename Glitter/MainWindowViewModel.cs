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

namespace Glitter
{
    sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FileGraph _graph = new FileGraph(true);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through binding in WPF.")]
        public FileGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        FileSystemWatcher _watcher;
        private bool _disposed;
        private bool _loading = false;
        
        public MainWindowViewModel()
        {
            _watcher = new FileSystemWatcher();
            _watcher.IncludeSubdirectories = true;
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null && !_loading)
            {
                Application.Current.Dispatcher.Invoke(() => PropertyChanged(this, new PropertyChangedEventArgs(property)));
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

            var go = ObjectFileParser.ParseFile(fi);
            if (go != null)
            {
                var source = GetOrAddTargetById(go.Id);
                source.GitObject = go;

                foreach (var reference in go.References)
                {
                    var target = GetOrAddTargetById(reference.Item1);
                    _graph.AddEdge(new FileEdge(source, target) { Name = reference.Item2 });
                }

                foreach (var reference in _graph.Vertices.Where(t => t.GitObject != null && t.GitObject.References.Any(r => r.Item1 == go.Id)))
                {
                    _graph.AddEdge(new FileEdge(reference, source));
                }

                _graph.RemoveEdgeIf(e => e.Source == source && !go.References.Any(p => p.Item1 == e.Target.Id));

                OnPropertyChanged("Graph");
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
                source.Id = 
                    source.GitObject.Id = id;
                OnPropertyChanged("Graph");
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

        public void Dispose()
        {
            if (_disposed)
            {
                if (_watcher != null)
                {
                    _watcher.Dispose();
                    _watcher = null; 
                }

                _disposed = true;
            }
        }

        public void Load(DirectoryInfo di)
        {
            _loading = true;
            _watcher.Path = di.FullName;
            _watcher.Created += (o, e) => AddFileToGraph(new FileInfo(e.FullPath));
            _watcher.Changed += (o, e) => AddFileToGraph(new FileInfo(e.FullPath));
            _watcher.Renamed += (o, e) => UpdateFileOnGraph(new FileInfo(e.OldFullPath), new FileInfo(e.FullPath));
            _watcher.Deleted += (o, e) => RemoveFileFromGraph(new FileInfo(e.FullPath));
            _watcher.EnableRaisingEvents = true;

            foreach (var item in new[] { new FileInfo(Path.Combine(di.FullName, "index")), new FileInfo(Path.Combine(di.FullName, "HEAD")) }
                .Concat(new DirectoryInfo(Path.Combine(di.FullName, "refs")).EnumerateFiles("*.*", SearchOption.AllDirectories))
                .Concat(new DirectoryInfo(Path.Combine(di.FullName, "objects")).EnumerateFiles("*.*", SearchOption.AllDirectories)))
            {
                AddFileToGraph(item);
            }

            _loading = false;
            OnPropertyChanged("Graph");

        }
    }
}
