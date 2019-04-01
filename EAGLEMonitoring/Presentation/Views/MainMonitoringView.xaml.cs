using EAGLEMonitoring.Application.Views;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

namespace EAGLEMonitoring.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainMonitoringView.xaml
    /// </summary>
    [Export(typeof(IMainMonitoringView))]
    public partial class MainMonitoringView : UserControl, IMainMonitoringView
    {
        public MainMonitoringView()
        {
            InitializeComponent();
        }

        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement((ListBox)sender, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null) return;

            var series = (GLineSeries)item.Content;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }
    }
}
