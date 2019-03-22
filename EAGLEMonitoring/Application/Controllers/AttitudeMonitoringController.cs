using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.ViewModels;
using EAGLEMonitoring.Domain;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Controllers
{
    [Export]
    public class AttitudeMonitoringController
    {
        private readonly AttitudeMonitoringViewModel attitudeMonitoringViewModel;
        private readonly IShellService shellService;
        private readonly IDataUpdateService dataUpdateService;

        [ImportingConstructor]
        public AttitudeMonitoringController(AttitudeMonitoringViewModel attitudeMonitoringViewModel,
            IShellService shellService,
            IDataUpdateService dataUpdateService)
        {
            this.attitudeMonitoringViewModel = attitudeMonitoringViewModel;
            this.shellService = shellService;
            this.dataUpdateService = dataUpdateService;
            //dataUpdateService.DataUpdateEvent += DataUpdateEventHandler;
        }

        public void Initialize()
        {
            shellService.AttitudeMonitoring = attitudeMonitoringViewModel;
        }

        public void InitializePlots()
        {
            InitializeAttitude();
        }

        private void DataUpdateEventHandler(object sender, DataUpdateEventArgs args)
        {
            if (args.DataSets == null || args.DataSets.Count == 0)
                return; // No updating to be done

            ThreadPool.QueueUserWorkItem((x) =>
            {
                UpdateAttitude(args.DataSets);
            });
        }

        public void RefreshCharts()
        { }

        private void InitializeAttitude()
        {
            SeriesCollection RollCollection = new SeriesCollection();
            SeriesCollection PitchCollection = new SeriesCollection();
            SeriesCollection YawCollection = new SeriesCollection();
            SeriesCollection VelCollection = new SeriesCollection();
            SeriesCollection MotorsCollection = new SeriesCollection();

            attitudeMonitoringViewModel.AttRollSeries = RollCollection;
            attitudeMonitoringViewModel.AttPitchSeries = PitchCollection;
            attitudeMonitoringViewModel.AttYawSeries = YawCollection;
            attitudeMonitoringViewModel.AttVelSeries = VelCollection;
            attitudeMonitoringViewModel.AttMotorsSeries = MotorsCollection;

            RollCollection.Add(new LineSeries()
            {
                Title = "Roll Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            RollCollection.Add(new LineSeries()
            {
                Title = "Roll Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            RollCollection.Add(new LineSeries()
            {
                Title = "Roll Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });

            // Make line series for pitch
            PitchCollection.Add(new LineSeries()
            {
                Title = "Pitch Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            PitchCollection.Add(new LineSeries()
            {
                Title = "Pitch Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            PitchCollection.Add(new LineSeries()
            {
                Title = "Pitch Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });

            // Make line series for yaw
            YawCollection.Add(new LineSeries()
            {
                Title = "Yaw Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            YawCollection.Add(new LineSeries()
            {
                Title = "Yaw Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            YawCollection.Add(new LineSeries()
            {
                Title = "Yaw Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });

            // Make line series for velocities
            VelCollection.Add(new LineSeries()
            {
                Title = "Velocity X-axis",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            VelCollection.Add(new LineSeries()
            {
                Title = "Velocity Y-axis",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            VelCollection.Add(new LineSeries()
            {
                Title = "Velocity Z-axis",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });

            // Make line series for motors
            MotorsCollection.Add(new LineSeries()
            {
                Title = "Motor FL",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            MotorsCollection.Add(new LineSeries()
            {
                Title = "Motor FR",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            MotorsCollection.Add(new LineSeries()
            {
                Title = "Motor BL",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            MotorsCollection.Add(new LineSeries()
            {
                Title = "Motor BR" +
                "",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
        }

        private void UpdateAttitude(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            List<LogPoint> newRollEstPoints = new List<LogPoint>();
            List<LogPoint> newRollMeasPoints = new List<LogPoint>();
            List<LogPoint> newRollRefPoints = new List<LogPoint>();

            List<LogPoint> newPitchEstPoints = new List<LogPoint>();
            List<LogPoint> newPitchMeasPoints = new List<LogPoint>();
            List<LogPoint> newPitchRefPoints = new List<LogPoint>();

            List<LogPoint> newYawEstPoints = new List<LogPoint>();
            List<LogPoint> newYawMeasPoints = new List<LogPoint>();
            List<LogPoint> newYawRefPoints = new List<LogPoint>();

            List<LogPoint> newVelXPoints = new List<LogPoint>();
            List<LogPoint> newVelYPoints = new List<LogPoint>();
            List<LogPoint> newVelZPoints = new List<LogPoint>();

            List<LogPoint> newMotorFLPoints = new List<LogPoint>();
            List<LogPoint> newMotorFRPoints = new List<LogPoint>();
            List<LogPoint> newMotorBLPoints = new List<LogPoint>();
            List<LogPoint> newMotorBRPoints = new List<LogPoint>();

            foreach (DataSet dataSet in dataSets)
            {
                newRollEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Roll));
                newRollMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Roll));
                newRollRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Roll));

                newPitchEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Pitch));
                newPitchMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Pitch));
                newPitchRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Pitch));

                newYawEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Yaw));
                newYawMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Yaw));
                newYawRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Yaw));

                newVelXPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.XVelocity));
                newVelYPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.YVelocity));
                newVelZPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.ZVelocity));

                newMotorFLPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorFL));
                newMotorFRPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorFR));
                newMotorBLPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorBL));
                newMotorBRPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorBR));
            }

            attitudeMonitoringViewModel.AttRollSeries[0].Values.AddRange(newRollEstPoints);
            attitudeMonitoringViewModel.AttRollSeries[1].Values.AddRange(newRollMeasPoints);
            attitudeMonitoringViewModel.AttRollSeries[2].Values.AddRange(newRollRefPoints);

            attitudeMonitoringViewModel.AttPitchSeries[0].Values.AddRange(newPitchEstPoints);
            attitudeMonitoringViewModel.AttPitchSeries[1].Values.AddRange(newPitchMeasPoints);
            attitudeMonitoringViewModel.AttPitchSeries[2].Values.AddRange(newPitchRefPoints);

            attitudeMonitoringViewModel.AttYawSeries[0].Values.AddRange(newYawEstPoints);
            attitudeMonitoringViewModel.AttYawSeries[1].Values.AddRange(newYawMeasPoints);
            attitudeMonitoringViewModel.AttYawSeries[2].Values.AddRange(newYawRefPoints);

            attitudeMonitoringViewModel.AttVelSeries[0].Values.AddRange(newVelXPoints);
            attitudeMonitoringViewModel.AttVelSeries[1].Values.AddRange(newVelYPoints);
            attitudeMonitoringViewModel.AttVelSeries[2].Values.AddRange(newVelZPoints);

            attitudeMonitoringViewModel.AttMotorsSeries[0].Values.AddRange(newMotorFLPoints);
            attitudeMonitoringViewModel.AttMotorsSeries[1].Values.AddRange(newMotorFRPoints);
            attitudeMonitoringViewModel.AttMotorsSeries[2].Values.AddRange(newMotorBLPoints);
            attitudeMonitoringViewModel.AttMotorsSeries[3].Values.AddRange(newMotorBRPoints);
        }
    }
}
