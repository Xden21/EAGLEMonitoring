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
    public class AltitudeMonitoringViewModel : ViewModel<IAltitudeMonitoringView>
    {
        [ImportingConstructor]
        public AltitudeMonitoringViewModel(IAltitudeMonitoringView view) : base(view)
        {
            Formatter = value => value.ToString("0.###");
        }

        private SeriesCollection altSeries;

        public SeriesCollection AltSeries
        {
            get { return altSeries; }
            set { SetProperty(ref altSeries, value); }
        }


        public Func<double, string> Formatter { get; set; }
    }
}
