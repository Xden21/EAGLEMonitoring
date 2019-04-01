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
    /// Interaction logic for NavigationMonitoringView.xaml
    /// </summary>
    [Export(typeof(INavigationMonitoringView))]
    public partial class NavigationMonitoringView : UserControl, INavigationMonitoringView
    {
        public NavigationMonitoringView()
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

        private void ListBox_PreviewMouseDownScatter(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement((ListBox)sender, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null) return;

            var series = (GScatterSeries)item.Content;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }
        private void CartesianChart_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CartesianChart chart = (CartesianChart)sender;
            chart.AxisX[0].MinValue = double.NaN;
            chart.AxisX[0].MaxValue = double.NaN;
            chart.AxisY[0].MinValue = double.NaN;
            chart.AxisY[0].MaxValue = double.NaN;
        }
    }
}
