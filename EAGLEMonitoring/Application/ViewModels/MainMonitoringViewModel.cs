using EAGLEMonitoring.Application.Views;
using EAGLEMonitoring.Domain;
using LiveCharts;
using LiveCharts.Geared;
using LiveCharts.Wpf;
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
    public class MainMonitoringViewModel : ViewModel<IMainMonitoringView>
    {
        #region Constructor

        [ImportingConstructor]
        public MainMonitoringViewModel(IMainMonitoringView view) : base(view)
        {
            Formatter = value => value.ToString("0.###");
            AttEnable = true;
            AltMode = true;
            NavMode = true;

            rollEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            rollMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            rollRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            pitchEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            pitchMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            pitchRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            yawEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            yawMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            yawRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            heightEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            heightMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            heightRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            navXEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navXMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navXRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            navYEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navYMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            navYRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
        }

        #endregion

        #region Vars & Props

        private GearedValues<LogPoint> rollEstPoints;

        public GearedValues<LogPoint> RollEstPoints
        {
            get { return rollEstPoints; }
            set { SetProperty(ref rollEstPoints, value); }
        }

        private GearedValues<LogPoint> rollMeasPoints;

        public GearedValues<LogPoint> RollMeasPoints
        {
            get { return rollMeasPoints; }
            set { SetProperty(ref rollMeasPoints, value); }
        }

        private GearedValues<LogPoint> rollRefPoints;

        public GearedValues<LogPoint> RollRefPoints
        {
            get { return rollRefPoints; }
            set { SetProperty(ref rollRefPoints, value); }
        }

        private GearedValues<LogPoint> pitchEstPoints;

        public GearedValues<LogPoint> PitchEstPoints
        {
            get { return pitchEstPoints; }
            set { SetProperty(ref pitchEstPoints, value); }
        }

        private GearedValues<LogPoint> pitchMeasPoints;

        public GearedValues<LogPoint> PitchMeasPoints
        {
            get { return pitchMeasPoints; }
            set { SetProperty(ref pitchMeasPoints, value); }
        }

        private GearedValues<LogPoint> pitchRefPoints;

        public GearedValues<LogPoint> PitchRefPoints
        {
            get { return pitchRefPoints; }
            set { SetProperty(ref pitchRefPoints, value); }
        }

        private GearedValues<LogPoint> yawEstPoints;

        public GearedValues<LogPoint> YawEstPoints
        {
            get { return yawEstPoints; }
            set { SetProperty(ref yawEstPoints, value); }
        }

        private GearedValues<LogPoint> yawMeasPoints;

        public GearedValues<LogPoint> YawMeasPoints
        {
            get { return yawMeasPoints; }
            set { SetProperty(ref yawMeasPoints, value); }
        }

        private GearedValues<LogPoint> yawRefPoints;

        public GearedValues<LogPoint> YawRefPoints
        {
            get { return yawRefPoints; }
            set { SetProperty(ref yawRefPoints, value); }
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


        private bool altMode;

        public bool AltMode
        {
            get { return altMode; }
            set { SetProperty(ref altMode, value); }
        }

        private bool navMode;

        public bool NavMode
        {
            get { return navMode; }
            set { SetProperty(ref navMode, value); }
        }

        private bool altOverride;

        public bool AltOverride
        {
            get { return altOverride; }
            set { SetProperty(ref altOverride, value); }
        }

        private bool navOverride;

        public bool NavOverride
        {
            get { return navOverride; }
            set { SetProperty(ref navOverride, value); }
        }

        private ICommand toggleVisibilityCommand;

        public ICommand ToggleVisibilityCommand
        {
            get { return toggleVisibilityCommand; }
            set { SetProperty(ref toggleVisibilityCommand, value); }
        }

        private LineSeries selectedSeries;

        public LineSeries SelectedSeries
        {
            get { return selectedSeries; }
            set { SetProperty(ref selectedSeries, value); }
        }

        private bool attEnable;

        public bool AttEnable
        {
            get { return attEnable; }
            set { SetProperty(ref attEnable, value); }
        }

        private float minValue;

        public float MinValue
        {
            get { return minValue; }
            set { SetProperty(ref minValue, value); }
        }

        private float maxValue;

        public float MaxValue
        {
            get { return maxValue; }
            set { SetProperty(ref maxValue, value); }
        }


        public Func<double, string> Formatter { get; set; }

        #endregion

        #region Methods

        #endregion
    }
}
