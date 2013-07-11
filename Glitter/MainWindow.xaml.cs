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
        public MainWindow()
        {
            //DataContext = new MainWindowViewModel();
            InitializeComponent();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "VM is disposed via the DataContext property")]
        private void allowdrop_Drop(object sender, DragEventArgs e)
        {
            var dir = ExtractRepositoryDirectory(e);
            Title = dir.FullName;

            Banner.Visibility = System.Windows.Visibility.Hidden;
            gg_zoomctrl.Visibility = System.Windows.Visibility.Visible;

            Dispose();

            var model = new MainWindowViewModel();
            model.Start(dir);

            //gg_Area.DefaultLayoutAlgorithm = GraphX.LayoutAlgorithmTypeEnum.BoundedFR;
            //gg_Area.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            //gg_Area.DefaulEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.SimpleER;
            gg_Area.GenerateGraph(model.Graph, true);

            DataContext = model;
        }

        public void Dispose()
        {
            var vm = DataContext as IDisposable;
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }

        private void zoom_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext != null)
            {
                //graphLayout.Relayout();
            }
        }
    }
}
