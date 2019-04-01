using EAGLEMonitoring.Application.Views;
using EAGLEMonitoring.Domain;
using LiveCharts;
using LiveCharts.Geared;
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

            heightEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            heightMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            heightRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
        }

        private GearedValues<LogPoint> heightEstPoints;

        public GearedValues<LogPoint> HeightEstPoints
        {
            get { return heightEstPoints; }
            set { SetProperty(ref heightEstPoints, value); }
        }

        private GearedValues<LogPoint> heightMeasPoints;

        public GearedValues<LogPoint> HeightMeasPoints
        {
            get { return heightMeasPoints; }
            set { SetProperty(ref heightMeasPoints, value); }
        }

        private GearedValues<LogPoint> heightRefPoints;

        public GearedValues<LogPoint> HeightRefPoints
        {
            get { return heightRefPoints; }
            set { SetProperty(ref heightRefPoints, value); }
        }
        
        public Func<double, string> Formatter { get; set; }
    }
}
