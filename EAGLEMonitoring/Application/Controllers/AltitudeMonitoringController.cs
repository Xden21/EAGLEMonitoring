using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.ViewModels;
using EAGLEMonitoring.Domain;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly ISettingsService settingsService;

        private bool isVisible;
        private bool updating;

        List<LogPoint> altEstBuffer;
        List<LogPoint> altMeasBuffer;
        List<LogPoint> altRefBuffer;
        private bool altBuffersFull;

        [ImportingConstructor]
        public AltitudeMonitoringController(AltitudeMonitoringViewModel altitudeMonitoringViewModel,
            IShellService shellService,
            IGeneralService generalService,
            IDataUpdateService dataUpdateService,
            ISettingsService settingsService)
        {
            this.altitudeMonitoringViewModel = altitudeMonitoringViewModel;
            this.shellService = shellService;
            this.dataUpdateService = dataUpdateService;
            this.generalService = generalService;
            this.settingsService = settingsService;

            altEstBuffer = new List<LogPoint>();
            altMeasBuffer = new List<LogPoint>();
            altRefBuffer = new List<LogPoint>();
            altBuffersFull = false;

            shellService.PropertyChanged += ShellService_PropertyChanged;
            dataUpdateService.DataUpdateEvent += DataUpdateEventHandler;
            updating = false;
        }

        private void ShellService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AltVisible")
            {
                if (shellService.AltVisible)
                {
                    isVisible = true;
                }
                else
                {
                    isVisible = false;
                }
            }
        }

        public void Initialize()
        {
            shellService.AltitudeMonitoring = altitudeMonitoringViewModel;
        }

        private void DataUpdateEventHandler(object sender, DataUpdateEventArgs args)
        {
            if (isVisible)
            {
                if (updating)
                {
                    UpdateAltitudeBuffer(args.DataSets);
                }
                else
                {
                    updating = true;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateAltitude(args.DataSets);
                        updating = false;
                    });
                }
            }
            else
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    UpdateAltitudeBuffer(args.DataSets);
                });
            }
        }

        private void UpdateAltitudeBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)altEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (dataSet.TimeStamp - firstTime) > settingsService.BigPlotTimeFrame)
                {
                    altBuffersFull = true;
                    altEstBuffer.RemoveAt(0);
                    altMeasBuffer.RemoveAt(0);
                    altRefBuffer.RemoveAt(0);

                    firstTime = (float)altEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;
                }
                altEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightEstimate));
                altMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightMeasured));
                altRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightReference));
            }
        }

        private void UpdateAltitude(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (altEstBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                return;
            }

            if (addBuffer)
            {
                UpdateAltitudeBuffer(dataSets);
                if (altBuffersFull)
                {
                    // Buffer will delete all current values
                    altitudeMonitoringViewModel.HeightEstPoints.Clear();
                    altitudeMonitoringViewModel.HeightMeasPoints.Clear();
                    altitudeMonitoringViewModel.HeightRefPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (altitudeMonitoringViewModel.HeightEstPoints.Count > 0 && (altEstBuffer.Last().TimeVal - altitudeMonitoringViewModel.HeightEstPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        altitudeMonitoringViewModel.HeightEstPoints.RemoveAt(0);
                        altitudeMonitoringViewModel.HeightMeasPoints.RemoveAt(0);
                        altitudeMonitoringViewModel.HeightRefPoints.RemoveAt(0);
                    }
                }
                altitudeMonitoringViewModel.HeightEstPoints.AddRange(altEstBuffer);
                altitudeMonitoringViewModel.HeightMeasPoints.AddRange(altMeasBuffer);
                altitudeMonitoringViewModel.HeightRefPoints.AddRange(altRefBuffer);
                altEstBuffer.Clear();
                altMeasBuffer.Clear();
                altRefBuffer.Clear();
                altBuffersFull = false;
            }
            else
            {
                // Search which values to remove
                int index = 0; // First index which stays
                while (index < altitudeMonitoringViewModel.HeightEstPoints.Count && (dataSets.Last().TimeStamp - altitudeMonitoringViewModel.HeightEstPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    altitudeMonitoringViewModel.HeightEstPoints.RemoveAt(0);
                    altitudeMonitoringViewModel.HeightMeasPoints.RemoveAt(0);
                    altitudeMonitoringViewModel.HeightRefPoints.RemoveAt(0);
                }
                List<LogPoint> newEstPoints = new List<LogPoint>();
                List<LogPoint> newMeasPoints = new List<LogPoint>();
                List<LogPoint> newRefPoints = new List<LogPoint>();

                foreach (DataSet dataSet in dataSets)
                {
                    newEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightEstimate));
                    newMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightMeasured));
                    newRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightReference));
                }

                altitudeMonitoringViewModel.HeightEstPoints.AddRange(newEstPoints);
                altitudeMonitoringViewModel.HeightMeasPoints.AddRange(newMeasPoints);
                altitudeMonitoringViewModel.HeightRefPoints.AddRange(newRefPoints);
            }
        }


        public void Reset()
        {
            altitudeMonitoringViewModel.HeightEstPoints.Clear();
            altitudeMonitoringViewModel.HeightMeasPoints.Clear();
            altitudeMonitoringViewModel.HeightRefPoints.Clear();
            altEstBuffer.Clear();
            altMeasBuffer.Clear();
            altRefBuffer.Clear();
            altBuffersFull = false;
            updating = false;
        }
    }
}
