using EAGLEMonitoring.Application.Views;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;

namespace EAGLEMonitoring.Application.ViewModels
{
    [Export]
    public class NavigationMonitoringViewModel : ViewModel<INavigationMonitoringView>
    {
        [ImportingConstructor]
        public NavigationMonitoringViewModel(INavigationMonitoringView view) : base(view)
        {
            Formatter = value => value.ToString("0.###");
        }

        private SeriesCollection navXSeries;

        public SeriesCollection NavXSeries
        {
            get { return navXSeries; }
            set { SetProperty(ref navXSeries, value); }
        }

        private SeriesCollection navYSeries;

        public SeriesCollection NavYSeries
        {
            get { return navYSeries; }
            set { SetProperty(ref navYSeries, value); }
        }

        private SeriesCollection navVelSeries;

        public SeriesCollection NavVelSeries
        {
            get { return navVelSeries; }
            set { SetProperty(ref navVelSeries, value); }
        }


        public Func<double, string> Formatter { get; set; }
    }
}
