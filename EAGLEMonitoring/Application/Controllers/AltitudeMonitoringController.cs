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
    public class AltitudeMonitoringController
    {
        private readonly AltitudeMonitoringViewModel altitudeMonitoringViewModel;
        private readonly IShellService shellService;
        private readonly IDataUpdateService dataUpdateService;
        private readonly IGeneralService generalService;


        [ImportingConstructor]
        public AltitudeMonitoringController(AltitudeMonitoringViewModel altitudeMonitoringViewModel,
            IShellService shellService,
            IGeneralService generalService,
            IDataUpdateService dataUpdateService)
        {
            this.altitudeMonitoringViewModel = altitudeMonitoringViewModel;
            this.shellService = shellService;
            this.dataUpdateService = dataUpdateService;
            this.generalService = generalService;
            //dataUpdateService.DataUpdateEvent += DataUpdateEventHandler;
        }


        public void Initialize()
        {
            shellService.AltitudeMonitoring = altitudeMonitoringViewModel;
        }

        public void InitializePlots()
        {
            InitializeAltitude();
        }

        private void InitializeAltitude()
        {
            SeriesCollection AltSeries = new SeriesCollection();

            altitudeMonitoringViewModel.AltSeries = AltSeries;

            // Make line series for altitude
            AltSeries.Add(new LineSeries()
            {
                Title = "Height Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            AltSeries.Add(new LineSeries()
            {
                Title = "Height Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });
            AltSeries.Add(new LineSeries()
            {
                Title = "Height Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new ChartValues<LogPoint>(),
                PointGeometry = null
            });

        }

        private void UpdateAltitude(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done


            List<LogPoint> newAltEstPoints = new List<LogPoint>();
            List<LogPoint> newAltMeasPoints = new List<LogPoint>();
            List<LogPoint> newAltRefPoints = new List<LogPoint>();

            foreach (DataSet dataSet in dataSets)
            {
                newAltEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightEstimate));
                newAltMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightMeasured));
                newAltRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightReference));
            }

            altitudeMonitoringViewModel.AltSeries[0].Values.AddRange(newAltEstPoints);
            altitudeMonitoringViewModel.AltSeries[1].Values.AddRange(newAltMeasPoints);
            altitudeMonitoringViewModel.AltSeries[2].Values.AddRange(newAltRefPoints);
        }

        public void RefreshCharts()
        { }

        private void DataResetEventHandler(object sender, EventArgs args)
        {
            altitudeMonitoringViewModel.AltSeries.Clear();

            InitializePlots();
        }
    }
}
