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
    public class AttitudeMonitoringViewModel : ViewModel<IAttitudeMonitoringView>
    {
        [ImportingConstructor]
        public AttitudeMonitoringViewModel(IAttitudeMonitoringView view) : base(view)
        {
            Formatter = value => value.ToString("0.###");
        }

        private SeriesCollection attRollSeries;

        public SeriesCollection AttRollSeries
        {
            get { return attRollSeries; }
            set { SetProperty(ref attRollSeries, value); }
        }

        private SeriesCollection attPitchSeries;

        public SeriesCollection AttPitchSeries
        {
            get { return attPitchSeries; }
            set { SetProperty(ref attPitchSeries, value); }
        }

        private SeriesCollection attYawSeries;

        public SeriesCollection AttYawSeries
        {
            get { return attYawSeries; }
            set { SetProperty(ref attYawSeries, value); }
        }

        private SeriesCollection attVelSeries;

        public SeriesCollection AttVelSeries
        {
            get { return attVelSeries; }
            set { SetProperty(ref attVelSeries, value); }
        }

        private SeriesCollection attMotorsSeries;

        public SeriesCollection AttMotorsSeries
        {
            get { return attMotorsSeries; }
            set { SetProperty(ref attMotorsSeries, value); }
        }


        public Func<double, string> Formatter { get; set; }
    }
}
