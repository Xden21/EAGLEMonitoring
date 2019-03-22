using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.ViewModels;
using EAGLEMonitoring.Domain;
using LiveCharts;
using LiveCharts.Geared;
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
    public class NavigationMonitoringController
    {
        private readonly NavigationMonitoringViewModel navigationMonitoringViewModel;
        private readonly IShellService shellService;
        private readonly IDataUpdateService dataUpdateService;
        private readonly ISettingsService settingsService;

        [ImportingConstructor]
        public NavigationMonitoringController(NavigationMonitoringViewModel navigationMonitoringViewModel,
            IShellService shellService,
            IDataUpdateService dataUpdateService,
            ISettingsService settingsService)
        {
            this.navigationMonitoringViewModel = navigationMonitoringViewModel;
            this.shellService = shellService;
            this.dataUpdateService = dataUpdateService;
            this.settingsService = settingsService;
        }

        public void Initialize()
        {
            shellService.NavigationMonitoring = navigationMonitoringViewModel;
        }

        private void DataUpdateEventHandler(object sender, DataUpdateEventArgs args)
        {
            if (args.DataSets == null || args.DataSets.Count == 0)
                return; // No updating to be done

            ThreadPool.QueueUserWorkItem((x) =>
            {
                UpdateNavigation(args.DataSets);
            });
        }

        public void RefreshCharts()
        { }

        private void UpdateNavigation(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float newLastTime = dataSets.Last().TimeStamp;
            GearedValues<LogPoint> XCoorEst = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavXSeries[0].Values;
            GearedValues<LogPoint> XCoorMeas = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavXSeries[1].Values;
            GearedValues<LogPoint> XCoorRef = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavXSeries[2].Values;

            GearedValues<LogPoint> YCoorEst = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavYSeries[0].Values;
            GearedValues<LogPoint> YCoorMeas = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavYSeries[1].Values;
            GearedValues<LogPoint> YCoorRef = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavYSeries[2].Values;

            GearedValues<LogPoint> YVelX = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavVelSeries[0].Values;
            GearedValues<LogPoint> YVelY = (GearedValues<LogPoint>)navigationMonitoringViewModel.NavVelSeries[1].Values;



            List<LogPoint> newXCoorEstPoints = new List<LogPoint>();
            List<LogPoint> newXCoorMeasPoints = new List<LogPoint>();
            List<LogPoint> newXCoorRefPoints = new List<LogPoint>();

            List<LogPoint> newYCoorEstPoints = new List<LogPoint>();
            List<LogPoint> newYCoorMeasPoints = new List<LogPoint>();
            List<LogPoint> newYCoorRefPoints = new List<LogPoint>();

            List<LogPoint> newXVelPoints = new List<LogPoint>();
            List<LogPoint> newYVelPoints = new List<LogPoint>();

            foreach (DataSet dataSet in dataSets)
            {
                newXCoorEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Roll));
                newXCoorMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Roll));
                newXCoorRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Roll));

                newYCoorEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Pitch));
                newYCoorMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Pitch));
                newYCoorRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Pitch));

                newXVelPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.VelocitySet.XVelocity));
                newYVelPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.VelocitySet.YVelocity));
            }

            XCoorEst.AddRange(newXCoorEstPoints);
            XCoorMeas.AddRange(newXCoorMeasPoints);
            XCoorRef.AddRange(newXCoorRefPoints);

            YCoorEst.AddRange(newYCoorEstPoints);
            YCoorMeas.AddRange(newYCoorMeasPoints);
            YCoorRef.AddRange(newYCoorRefPoints);

            YVelX.AddRange(newXVelPoints);
            YVelY.AddRange(newYVelPoints);
        }

        public void InitializePlots()
        {
            InitializeNavigation();
        }

        private void InitializeNavigation()
        {
            SeriesCollection NavXCollection = new SeriesCollection();
            SeriesCollection NavYCollection = new SeriesCollection();
            SeriesCollection NavVelCollection = new SeriesCollection();

            // Make line series for x direction
            NavXCollection.Add(new GLineSeries()
            {
                Title = "X-Direction Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            NavXCollection.Add(new GLineSeries()
            {
                Title = "X-Direction Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            NavXCollection.Add(new GLineSeries()
            {
                Title = "X-Direction Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });

            // Make line series for y direction
            NavYCollection.Add(new GLineSeries()
            {
                Title = "Y-Direction Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            NavYCollection.Add(new GLineSeries()
            {
                Title = "Y-Direction Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            NavYCollection.Add(new GLineSeries()
            {
                Title = "Y-Direction Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });

            NavVelCollection.Add(new GLineSeries()
            {
                Title = "X-Velocity",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            NavVelCollection.Add(new GLineSeries()
            {
                Title = "Y-Velocity",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });

            navigationMonitoringViewModel.NavXSeries = NavXCollection;
            navigationMonitoringViewModel.NavYSeries = NavYCollection;
            navigationMonitoringViewModel.NavVelSeries = NavVelCollection;
        }
    }
}
