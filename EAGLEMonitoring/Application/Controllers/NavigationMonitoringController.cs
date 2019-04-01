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
using System.Waf.Applications;

namespace EAGLEMonitoring.Application.Controllers
{
    [Export]
    public class NavigationMonitoringController
    {
        private readonly NavigationMonitoringViewModel navigationMonitoringViewModel;
        private readonly IShellService shellService;
        private readonly IDataUpdateService dataUpdateService;
        private readonly ISettingsService settingsService;
        private readonly IGuiUpdateService guiUpdateService;

        private bool coorLast;
        private bool velLast;
        private bool trackLast;

        private bool posUpdating;
        private bool velUpdating;
        private bool trackingUpdating;

        List<LogPoint> navXEstBuffer;
        List<LogPoint> navXMeasBuffer;
        List<LogPoint> navXRefBuffer;

        List<LogPoint> navYEstBuffer;
        List<LogPoint> navYMeasBuffer;
        List<LogPoint> navYRefBuffer;
        private bool posBuffersFull;

        List<LogPoint> navXVelBuffer;
        List<LogPoint> navYVelBuffer;
        private bool velBuffersFull;

        List<PosistionPoint> navTrackingEstBuffer;
        List<PosistionPoint> navTrackingMeasBuffer;
        List<PosistionPoint> navTrackingRefBuffer;

        int currentIndex;

        private int framePosCount;
        private int frameVelCount;

        private DelegateCommand TabChangeCommand;

        [ImportingConstructor]
        public NavigationMonitoringController(NavigationMonitoringViewModel navigationMonitoringViewModel,
            IShellService shellService,
            IDataUpdateService dataUpdateService,
            ISettingsService settingsService,
            IGuiUpdateService guiUpdateService)
        {
            this.navigationMonitoringViewModel = navigationMonitoringViewModel;
            this.shellService = shellService;
            this.dataUpdateService = dataUpdateService;
            this.settingsService = settingsService;
            this.guiUpdateService = guiUpdateService;
            shellService.PropertyChanged += ShellService_PropertyChanged;
            dataUpdateService.DataUpdateEvent += DataUpdateEventHandler;
            TabChangeCommand = new DelegateCommand(TabChange_Command);

            navXEstBuffer = new List<LogPoint>();
            navXMeasBuffer = new List<LogPoint>();
            navXRefBuffer = new List<LogPoint>();
            navYEstBuffer = new List<LogPoint>();
            navYMeasBuffer = new List<LogPoint>();
            navYRefBuffer = new List<LogPoint>();
            posBuffersFull = false;

            navXVelBuffer = new List<LogPoint>();
            navYVelBuffer = new List<LogPoint>();
            velBuffersFull = false;

            navTrackingEstBuffer = new List<PosistionPoint>();
            navTrackingMeasBuffer = new List<PosistionPoint>();
            navTrackingRefBuffer = new List<PosistionPoint>();

            GearedValues<PosistionPoint> estpoints = new GearedValues<PosistionPoint>().WithQuality(Quality.Medium);
            GearedValues<PosistionPoint> measpoints = new GearedValues<PosistionPoint>().WithQuality(Quality.Medium);
            GearedValues<PosistionPoint> refpoints = new GearedValues<PosistionPoint>().WithQuality(Quality.Medium);

            for (int i = 0; i < settingsService.TrackingTimeFrame * 1000; i++)
            {
                estpoints.Add(new PosistionPoint(0, 0));
                measpoints.Add(new PosistionPoint(0, 0));
                refpoints.Add(new PosistionPoint(0, 0));
            }
            navigationMonitoringViewModel.NavTrackingEstPoints = estpoints;
            navigationMonitoringViewModel.NavTrackingMeasPoints = measpoints;
            navigationMonitoringViewModel.NavTrackingRefPoints = refpoints;

            currentIndex = 0;

            posUpdating = false;
            velUpdating = false;
            trackingUpdating = false;

            coorLast = true;
            framePosCount = 0;
            frameVelCount = 0;
        }

        public void Initialize()
        {
            shellService.NavigationMonitoring = navigationMonitoringViewModel;
            navigationMonitoringViewModel.TabChange = TabChangeCommand;
        }

        private void DataUpdateEventHandler(object sender, DataUpdateEventArgs args)
        {
            if (args.DataSets == null || args.DataSets.Count == 0)
                return; // No updating to be done

            if (navigationMonitoringViewModel.NavCoorVis)
            {
                if (posUpdating || framePosCount > 0)
                {
                    framePosCount--;
                    UpdateNavigationCoorBuffer(args.DataSets);
                }
                else
                {
                    posUpdating = true;
                    framePosCount = settingsService.FPS / 20;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateNavigationCoor(args.DataSets);
                        posUpdating = false;
                    });
                }

            }
            else
            {
                UpdateNavigationCoorBuffer(args.DataSets);
            }
            if (navigationMonitoringViewModel.NavVelVis)
            {
                if (velUpdating || frameVelCount > 0)
                {
                    frameVelCount--;
                    UpdateNavigationVelBuffer(args.DataSets);
                }
                else
                {
                    velUpdating = true;
                    frameVelCount = settingsService.FPS / 20;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateNavigationVel(args.DataSets);
                        velUpdating = false;
                    });
                }
            }
            else
            {
                UpdateNavigationVelBuffer(args.DataSets);
            }
            if (navigationMonitoringViewModel.NavTrackingVis)
            {
                if (trackingUpdating)
                {
                    UpdateNavigationTrackingBuffer(args.DataSets);
                }
                else
                {
                    trackingUpdating = true;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateNavigationTracking(args.DataSets);
                        trackingUpdating = false;
                    });
                }
            }
            else
            {
                UpdateNavigationTrackingBuffer(args.DataSets);
            }
        }

        private void ShellService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NavVisible")
            {
                if (shellService.NavVisible)
                {
                    navigationMonitoringViewModel.NavCoorVis = coorLast;
                    navigationMonitoringViewModel.NavVelVis = velLast;
                    navigationMonitoringViewModel.NavTrackingVis = trackLast;
                }
                else
                {
                    coorLast = navigationMonitoringViewModel.NavCoorVis;
                    velLast = navigationMonitoringViewModel.NavVelVis;
                    trackLast = navigationMonitoringViewModel.NavTrackingVis;
                    navigationMonitoringViewModel.NavCoorVis = false;
                    navigationMonitoringViewModel.NavVelVis = false;
                    navigationMonitoringViewModel.NavTrackingVis = false;
                }
            }
        }

        private void UpdateNavigationCoor(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (navXEstBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                return;
            }

            if (addBuffer)
            {
                UpdateNavigationCoorBuffer(dataSets);
                if (posBuffersFull)
                {
                    // Buffer will delete all current values
                    navigationMonitoringViewModel.NavXEstPoints.Clear();
                    navigationMonitoringViewModel.NavXMeasPoints.Clear();
                    navigationMonitoringViewModel.NavXRefPoints.Clear();

                    navigationMonitoringViewModel.NavYEstPoints.Clear();
                    navigationMonitoringViewModel.NavYMeasPoints.Clear();
                    navigationMonitoringViewModel.NavYRefPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (navigationMonitoringViewModel.NavXEstPoints.Count > 0 && (navXEstBuffer.Last().TimeVal - navigationMonitoringViewModel.NavXEstPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        navigationMonitoringViewModel.NavXEstPoints.RemoveAt(0);
                        navigationMonitoringViewModel.NavXMeasPoints.RemoveAt(0);
                        navigationMonitoringViewModel.NavXRefPoints.RemoveAt(0);

                        navigationMonitoringViewModel.NavYEstPoints.RemoveAt(0);
                        navigationMonitoringViewModel.NavYMeasPoints.RemoveAt(0);
                        navigationMonitoringViewModel.NavYRefPoints.RemoveAt(0);
                    }
                }
                navigationMonitoringViewModel.NavXEstPoints.AddRange(navXEstBuffer);
                navigationMonitoringViewModel.NavXMeasPoints.AddRange(navXMeasBuffer);
                navigationMonitoringViewModel.NavXRefPoints.AddRange(navXRefBuffer);

                navigationMonitoringViewModel.NavYEstPoints.AddRange(navYEstBuffer);
                navigationMonitoringViewModel.NavYMeasPoints.AddRange(navYMeasBuffer);
                navigationMonitoringViewModel.NavYRefPoints.AddRange(navYRefBuffer);

                navXEstBuffer.Clear();
                navXMeasBuffer.Clear();
                navXRefBuffer.Clear();

                navYEstBuffer.Clear();
                navYMeasBuffer.Clear();
                navYRefBuffer.Clear();
                posBuffersFull = false;
            }
            else
            {
                // Search which values to remove
                int index = 0; // First index which stays
                while (index < navigationMonitoringViewModel.NavXEstPoints.Count && (dataSets.Last().TimeStamp - navigationMonitoringViewModel.NavXEstPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    navigationMonitoringViewModel.NavXEstPoints.RemoveAt(0);
                    navigationMonitoringViewModel.NavXMeasPoints.RemoveAt(0);
                    navigationMonitoringViewModel.NavXRefPoints.RemoveAt(0);

                    navigationMonitoringViewModel.NavYEstPoints.RemoveAt(0);
                    navigationMonitoringViewModel.NavYMeasPoints.RemoveAt(0);
                    navigationMonitoringViewModel.NavYRefPoints.RemoveAt(0);
                }
                List<LogPoint> newXEstPoints = new List<LogPoint>();
                List<LogPoint> newXMeasPoints = new List<LogPoint>();
                List<LogPoint> newXRefPoints = new List<LogPoint>();

                List<LogPoint> newYEstPoints = new List<LogPoint>();
                List<LogPoint> newYMeasPoints = new List<LogPoint>();
                List<LogPoint> newYRefPoints = new List<LogPoint>();

                foreach (DataSet dataSet in dataSets)
                {
                    newXEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.XPosition));
                    newXMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.XPosition));
                    newXRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.XPosition));

                    newYEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.YPosition));
                    newYMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.YPosition));
                    newYRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.YPosition));
                }

                navigationMonitoringViewModel.NavXEstPoints.AddRange(newXEstPoints);
                navigationMonitoringViewModel.NavXMeasPoints.AddRange(newXMeasPoints);
                navigationMonitoringViewModel.NavXRefPoints.AddRange(newXRefPoints);

                navigationMonitoringViewModel.NavYEstPoints.AddRange(newYEstPoints);
                navigationMonitoringViewModel.NavYMeasPoints.AddRange(newYMeasPoints);
                navigationMonitoringViewModel.NavYRefPoints.AddRange(newYRefPoints);
            }
        }

        private void UpdateNavigationVel(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (navXVelBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                return;
            }

            if (addBuffer)
            {
                UpdateNavigationVelBuffer(dataSets);
                if (velBuffersFull)
                {
                    // Buffer will delete all current values
                    navigationMonitoringViewModel.NavXVelPoints.Clear();
                    navigationMonitoringViewModel.NavYVelPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (navigationMonitoringViewModel.NavXVelPoints.Count > 0 && (navXVelBuffer.Last().TimeVal - navigationMonitoringViewModel.NavXVelPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        navigationMonitoringViewModel.NavXVelPoints.RemoveAt(0);
                        navigationMonitoringViewModel.NavYVelPoints.RemoveAt(0);
                    }
                }
                navigationMonitoringViewModel.NavXVelPoints.AddRange(navXVelBuffer);
                navigationMonitoringViewModel.NavYVelPoints.AddRange(navYVelBuffer);
                navXVelBuffer.Clear();
                navYVelBuffer.Clear();
                velBuffersFull = false;
            }
            else
            {
                // Search which values to remove
                int index = 0; // First index which stays
                while (index < navigationMonitoringViewModel.NavXVelPoints.Count && (dataSets.Last().TimeStamp - navigationMonitoringViewModel.NavXVelPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    navigationMonitoringViewModel.NavXVelPoints.RemoveAt(0);
                    navigationMonitoringViewModel.NavYVelPoints.RemoveAt(0);
                }
                List<LogPoint> newXVelPoints = new List<LogPoint>();
                List<LogPoint> newYVelPoints = new List<LogPoint>();

                foreach (DataSet dataSet in dataSets)
                {
                    newXVelPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightEstimate));
                    newYVelPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightMeasured));
                }

                navigationMonitoringViewModel.NavXVelPoints.AddRange(newXVelPoints);
                navigationMonitoringViewModel.NavYVelPoints.AddRange(newYVelPoints);
            }
        }

        private void UpdateNavigationTracking(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (navTrackingEstBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                if (!addBuffer)
                    return;
                else if (navTrackingEstBuffer == null || navTrackingEstBuffer.Count == 0)
                    return;
            }

            if (addBuffer && navTrackingEstBuffer.Count > 0)
            {
                for (int i = 0; i < navTrackingEstBuffer.Count; i++)
                {
                    navigationMonitoringViewModel.NavTrackingEstPoints[currentIndex].XPosition = navTrackingEstBuffer[i].XPosition;
                    navigationMonitoringViewModel.NavTrackingEstPoints[currentIndex].YPosition = navTrackingEstBuffer[i].YPosition;

                    navigationMonitoringViewModel.NavTrackingMeasPoints[currentIndex].XPosition = navTrackingMeasBuffer[i].XPosition;
                    navigationMonitoringViewModel.NavTrackingMeasPoints[currentIndex].YPosition = navTrackingMeasBuffer[i].YPosition;

                    navigationMonitoringViewModel.NavTrackingRefPoints[currentIndex].XPosition = navTrackingRefBuffer[i].XPosition;
                    navigationMonitoringViewModel.NavTrackingRefPoints[currentIndex].YPosition = navTrackingRefBuffer[i].YPosition;

                    if ((++currentIndex) >= settingsService.TrackingTimeFrame * 1000)
                        currentIndex = 0;
                }
                navTrackingEstBuffer = new List<PosistionPoint>();
                navTrackingMeasBuffer = new List<PosistionPoint>();
                navTrackingRefBuffer = new List<PosistionPoint>();
            }

            foreach (DataSet dataSet in dataSets)
            {
                navigationMonitoringViewModel.NavTrackingEstPoints[currentIndex].XPosition = dataSet.PositionEstimate.XPosition;
                navigationMonitoringViewModel.NavTrackingEstPoints[currentIndex].YPosition = dataSet.PositionEstimate.YPosition;

                navigationMonitoringViewModel.NavTrackingMeasPoints[currentIndex].XPosition = dataSet.PositionMeasured.XPosition;
                navigationMonitoringViewModel.NavTrackingMeasPoints[currentIndex].YPosition = dataSet.PositionMeasured.YPosition;

                navigationMonitoringViewModel.NavTrackingRefPoints[currentIndex].XPosition = dataSet.PositionReference.XPosition;
                navigationMonitoringViewModel.NavTrackingRefPoints[currentIndex].YPosition = dataSet.PositionReference.YPosition;

                if ((++currentIndex) >= settingsService.TrackingTimeFrame * 1000)
                    currentIndex = 0;
            }
        }

        private void UpdateNavigationCoorBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)navXEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (navXEstBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.BigPlotTimeFrame)
                {
                    navXEstBuffer.RemoveAt(0);
                    navXMeasBuffer.RemoveAt(0);
                    navXRefBuffer.RemoveAt(0);

                    navYEstBuffer.RemoveAt(0);
                    navYMeasBuffer.RemoveAt(0);
                    navYRefBuffer.RemoveAt(0);

                    firstTime = (float)navXEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;
                }
                navXEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.XPosition));
                navXMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.XPosition));
                navXRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.XPosition));

                navYEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.YPosition));
                navYMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.YPosition));
                navYRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.YPosition));
            }
        }

        private void UpdateNavigationVelBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)navXVelBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (navXVelBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.BigPlotTimeFrame)
                {
                    navXVelBuffer.RemoveAt(0);
                    navYVelBuffer.RemoveAt(0);

                    firstTime = (float)navXVelBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;
                }
                navXVelBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.VelocitySet.XVelocity));
                navYVelBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.VelocitySet.YVelocity));
            }
        }

        private void UpdateNavigationTrackingBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done


            foreach (DataSet dataSet in dataSets)
            {
                if (navTrackingEstBuffer.Count >= settingsService.TrackingTimeFrame * 1000)
                {
                    navTrackingEstBuffer.RemoveAt(0);
                    navTrackingMeasBuffer.RemoveAt(0);
                    navTrackingRefBuffer.RemoveAt(0);

                }
                navTrackingEstBuffer.Add(new PosistionPoint(dataSet.PositionEstimate.XPosition, dataSet.PositionEstimate.YPosition));
                navTrackingMeasBuffer.Add(new PosistionPoint(dataSet.PositionMeasured.XPosition, dataSet.PositionMeasured.YPosition));
                navTrackingRefBuffer.Add(new PosistionPoint(dataSet.PositionReference.XPosition, dataSet.PositionReference.YPosition));
            }
        }

        private void TabChange_Command(object param)
        {
            guiUpdateService.StopGuiUpdate();
            Thread.Sleep(50);
            int tabnum = int.Parse((string)param);

            switch (tabnum)
            {
                case 0:
                    navigationMonitoringViewModel.NavCoorVis = true;
                    navigationMonitoringViewModel.NavVelVis = false;
                    navigationMonitoringViewModel.NavTrackingVis = false;
                    break;
                case 1:
                    navigationMonitoringViewModel.NavCoorVis = false;
                    navigationMonitoringViewModel.NavVelVis = true;
                    navigationMonitoringViewModel.NavTrackingVis = false;
                    break;
                case 2:
                    navigationMonitoringViewModel.NavCoorVis = false;
                    navigationMonitoringViewModel.NavVelVis = false;
                    navigationMonitoringViewModel.NavTrackingVis = true;
                    break;
                default:
                    break;
            }
            Thread.Sleep(50);
            guiUpdateService.StartGuiUpdate();
        }

        public void Reset()
        {
            navigationMonitoringViewModel.NavXEstPoints.Clear();
            navigationMonitoringViewModel.NavXMeasPoints.Clear();
            navigationMonitoringViewModel.NavXRefPoints.Clear();

            navigationMonitoringViewModel.NavYEstPoints.Clear();
            navigationMonitoringViewModel.NavYMeasPoints.Clear();
            navigationMonitoringViewModel.NavYRefPoints.Clear();

            navigationMonitoringViewModel.NavXVelPoints.Clear();
            navigationMonitoringViewModel.NavYVelPoints.Clear();

            GearedValues<PosistionPoint> estpoints = new GearedValues<PosistionPoint>().WithQuality(Quality.Medium);
            GearedValues<PosistionPoint> measpoints = new GearedValues<PosistionPoint>().WithQuality(Quality.Medium);
            GearedValues<PosistionPoint> refpoints = new GearedValues<PosistionPoint>().WithQuality(Quality.Medium);

            for (int i = 0; i < settingsService.TrackingTimeFrame * 1000; i++)
            {
                estpoints.Add(new PosistionPoint(0, 0));
                measpoints.Add(new PosistionPoint(0, 0));
                refpoints.Add(new PosistionPoint(0, 0));
            }
            navigationMonitoringViewModel.NavTrackingEstPoints = estpoints;
            navigationMonitoringViewModel.NavTrackingMeasPoints = measpoints;
            navigationMonitoringViewModel.NavTrackingRefPoints = refpoints;

            currentIndex = 0;

            posUpdating = false;
            velUpdating = false;
            trackingUpdating = false;
            framePosCount = 0;
            frameVelCount = 0;

            navXEstBuffer = new List<LogPoint>();
            navXMeasBuffer = new List<LogPoint>();
            navXRefBuffer = new List<LogPoint>();
            navYEstBuffer = new List<LogPoint>();
            navYMeasBuffer = new List<LogPoint>();
            navYRefBuffer = new List<LogPoint>();
            posBuffersFull = false;

            navXVelBuffer = new List<LogPoint>();
            navYVelBuffer = new List<LogPoint>();
            velBuffersFull = false;
        }
    }
}
