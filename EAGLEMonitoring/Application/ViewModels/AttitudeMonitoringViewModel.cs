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
    public class AttitudeMonitoringViewModel : ViewModel<IAttitudeMonitoringView>
    {
        [ImportingConstructor]
        public AttitudeMonitoringViewModel(IAttitudeMonitoringView view) : base(view)
        {
            Formatter = value => value.ToString("0.###");
            RotVis = true;
            VelVis = false;
            MotorVis = false;

            rollEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            rollMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            rollRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            pitchEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            pitchMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            pitchRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            yawEstPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            yawMeasPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            yawRefPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            velXPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            velYPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            velZPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);

            motorFLPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            motorFRPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            motorBLPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
            motorBRPoints = new GearedValues<LogPoint>().WithQuality(Quality.Medium);
        }

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

        private GearedValues<LogPoint> velXPoints;

        public GearedValues<LogPoint> VelXPoints
        {
            get { return velXPoints; }
            set { SetProperty(ref velXPoints, value); }
        }

        private GearedValues<LogPoint> velYPoints;

        public GearedValues<LogPoint> VelYPoints
        {
            get { return velYPoints; }
            set { SetProperty(ref velYPoints, value); }
        }

        private GearedValues<LogPoint> velZPoints;

        public GearedValues<LogPoint> VelZPoints
        {
            get { return velZPoints; }
            set { SetProperty(ref velZPoints, value); }
        }

        private GearedValues<LogPoint> motorFLPoints;

        public GearedValues<LogPoint> MotorFLPoints
        {
            get { return motorFLPoints; }
            set { SetProperty(ref motorFLPoints, value); }
        }

        private GearedValues<LogPoint> motorFRPoints;

        public GearedValues<LogPoint> MotorFRPoints
        {
            get { return motorFRPoints; }
            set { SetProperty(ref motorFRPoints, value); }
        }

        private GearedValues<LogPoint> motorBLPoints;

        public GearedValues<LogPoint> MotorBLPoints
        {
            get { return motorBLPoints; }
            set { SetProperty(ref motorBLPoints, value); }
        }

        private GearedValues<LogPoint> motorBRPoints;

        public GearedValues<LogPoint> MotorBRPoints
        {
            get { return motorBRPoints; }
            set { SetProperty(ref motorBRPoints, value); }
        }

        private bool rotVis;

        public bool RotVis
        {
            get { return rotVis; }
            set { SetProperty(ref rotVis, value); }
        }

        private bool velVis;

        public bool VelVis
        {
            get { return velVis; }
            set { SetProperty(ref velVis, value); }
        }

        private bool motorVis;

        public bool MotorVis
        {
            get { return motorVis; }
            set { SetProperty(ref motorVis, value); }
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
