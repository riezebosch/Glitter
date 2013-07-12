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

namespace Glitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        private MainWindowViewModel _model = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();

            gg_Area.EdgeCurvingEnabled = true;
            gg_Area.DefaultLayoutAlgorithm = GraphX.LayoutAlgorithmTypeEnum.FR;
            gg_Area.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            gg_Area.DefaulEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.Bundling;
            gg_Area.MoveAnimation = new GraphX.Animations.FadeMoveAnimation(TimeSpan.FromMilliseconds(500));

            _model.PropertyChanged += (f1, f2) => gg_Area.GenerateGraph(_model.Graph, true);
}

        private void allowdrop_DragEnter(object sender, DragEventArgs e)
        {
            if (ExtractRepositoryDirectory(e) != null)
            {
                e.Effects = DragDropEffects.All;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "VM is disposed via the DataContext property")]
        private void allowdrop_Drop(object sender, DragEventArgs e)
        {
            Banner.Visibility = System.Windows.Visibility.Hidden;

            var dir = ExtractRepositoryDirectory(e);
            Title = dir.FullName;

            _model.Load(dir);
            //DataContext = _model;
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

        public void Dispose()
        {
            var vm = DataContext as IDisposable;
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }

            if (_model != null)
            {
                _model.Dispose();
                _model = null;
            }
        }

        private void Algo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gg_Area.RelayoutGraph(true);
        }
    }
}
