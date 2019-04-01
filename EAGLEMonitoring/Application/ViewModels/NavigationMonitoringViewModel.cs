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
using System.Windows.Input;

namespace EAGLEMonitoring.Application.ViewModels
{
    [Export]
    public class NavigationMonitoringViewModel : ViewModel<INavigationMonitoringView>
    {
        [ImportingConstructor]
        public NavigationMonitoringViewModel(INavigationMonitoringView view) : base(view)
        {
            Formatter = value => value.ToString("0.###");


            navXEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navXMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navXRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            navYEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navYMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navYRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            navXVelPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navYVelPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            navTrackingEstPoints = new GearedValues<PosistionPoint>().WithQuality(Quality.High);
            navTrackingMeasPoints = new GearedValues<PosistionPoint>().WithQuality(Quality.High);
            navTrackingRefPoints = new GearedValues<PosistionPoint>().WithQuality(Quality.High);
        }

        private GearedValues<LogPoint> navXEstPoints;

        public GearedValues<LogPoint> NavXEstPoints
        {
            get { return navXEstPoints; }
            set { SetProperty(ref navXEstPoints, value); }
        }

        private GearedValues<LogPoint> navXMeasPoints;

        public GearedValues<LogPoint> NavXMeasPoints
        {
            get { return navXMeasPoints; }
            set { SetProperty(ref navXMeasPoints, value); }
        }

        private GearedValues<LogPoint> navXRefPoints;

        public GearedValues<LogPoint> NavXRefPoints
        {
            get { return navXRefPoints; }
            set { SetProperty(ref navXRefPoints, value); }
        }

        private GearedValues<LogPoint> navYEstPoints;

        public GearedValues<LogPoint> NavYEstPoints
        {
            get { return navYEstPoints; }
            set { SetProperty(ref navYEstPoints, value); }
        }

        private GearedValues<LogPoint> navYMeasPoints;

        public GearedValues<LogPoint> NavYMeasPoints
        {
            get { return navYMeasPoints; }
            set { SetProperty(ref navYMeasPoints, value); }
        }

        private GearedValues<LogPoint> navYRefPoints;

        public GearedValues<LogPoint> NavYRefPoints
        {
            get { return navYRefPoints; }
            set { SetProperty(ref navYRefPoints, value); }
        }

        private GearedValues<LogPoint> navXVelPoints;

        public GearedValues<LogPoint> NavXVelPoints
        {
            get { return navXVelPoints; }
            set { SetProperty(ref navXVelPoints, value); }
        }

        private GearedValues<LogPoint> navYVelPoints;

        public GearedValues<LogPoint> NavYVelPoints
        {
            get { return navYVelPoints; }
            set { SetProperty(ref navYVelPoints, value); }
        }

        private GearedValues<PosistionPoint> navTrackingEstPoints;

        public GearedValues<PosistionPoint> NavTrackingEstPoints
        {
            get { return navTrackingEstPoints; }
            set { SetProperty(ref navTrackingEstPoints, value); }
        }

        private GearedValues<PosistionPoint> navTrackingMeasPoints;

        public GearedValues<PosistionPoint> NavTrackingMeasPoints
        {
            get { return navTrackingMeasPoints; }
            set { SetProperty(ref navTrackingMeasPoints, value); }
        }

        private GearedValues<PosistionPoint> navTrackingRefPoints;

        public GearedValues<PosistionPoint> NavTrackingRefPoints
        {
            get { return navTrackingRefPoints; }
            set { SetProperty(ref navTrackingRefPoints, value); }
        }


        private bool navCoorVis;

        public bool NavCoorVis
        {
            get { return navCoorVis; }
            set { SetProperty(ref navCoorVis, value); }
        }

        private bool navVelVis;

        public bool NavVelVis
        {
            get { return navVelVis; }
            set { SetProperty(ref navVelVis, value); }
        }

        private bool navTrackingVis;

        public bool NavTrackingVis
        {
            get { return navTrackingVis; }
            set { SetProperty(ref navTrackingVis, value); }
        }

        private ICommand tabChange;

        public ICommand TabChange
        {
            get { return tabChange; }
            set { SetProperty(ref tabChange, value); }
        }



        public Func<double, string> Formatter { get; set; }
    }
}
