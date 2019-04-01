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
using System.Waf.Applications;

namespace EAGLEMonitoring.Application.Controllers
{
    [Export]
    public class MainMonitoringController
    {
        private readonly MainMonitoringViewModel mainMonitoringViewModel;
        private readonly IDataUpdateService dataUpdateService;
        private readonly IShellService shellService;
        private readonly ISettingsService settingsService;
        private readonly IGeneralService generalService;

        private bool isVisible;
        private bool attUpdating;
        private bool altUpdating;
        private bool navUpdating;


        #region Buffers

        private bool attBuffersFull;
        List<LogPoint> attRollEstBuffer;
        List<LogPoint> attRollMeasBuffer;
        List<LogPoint> attRollRefBuffer;

        List<LogPoint> attPitchEstBuffer;
        List<LogPoint> attPitchMeasBuffer;
        List<LogPoint> attPitchRefBuffer;

        List<LogPoint> attYawEstBuffer;
        List<LogPoint> attYawMeasBuffer;
        List<LogPoint> attYawRefBuffer;

        private bool altBuffersFull;
        List<LogPoint> altEstBuffer;
        List<LogPoint> altMeasBuffer;
        List<LogPoint> altRefBuffer;

        private bool navBuffersFull;
        List<LogPoint> navXEstBuffer;
        List<LogPoint> navXMeasBuffer;
        List<LogPoint> navXRefBuffer;

        List<LogPoint> navYEstBuffer;
        List<LogPoint> navYMeasBuffer;
        List<LogPoint> navYRefBuffer;

        #endregion

        [ImportingConstructor]
        public MainMonitoringController(MainMonitoringViewModel mainMonitoringViewModel,
            IDataUpdateService dataUpdateService,
            IShellService shellService,
            ISettingsService settingsService,
            IGeneralService generalService)
        {
            this.mainMonitoringViewModel = mainMonitoringViewModel;
            this.dataUpdateService = dataUpdateService;
            this.shellService = shellService;
            this.settingsService = settingsService;
            this.generalService = generalService;
            dataUpdateService.DataUpdateEvent += DataUpdateEventHandler;
            generalService.PropertyChanged += GeneralService_PropertyChanged;
            shellService.PropertyChanged += ShellService_PropertyChanged;
            isVisible = true;

            attRollEstBuffer = new List<LogPoint>();
            attRollMeasBuffer = new List<LogPoint>();
            attRollRefBuffer = new List<LogPoint>();
            attPitchEstBuffer = new List<LogPoint>();
            attPitchMeasBuffer = new List<LogPoint>();
            attPitchRefBuffer = new List<LogPoint>();
            attYawEstBuffer = new List<LogPoint>();
            attYawMeasBuffer = new List<LogPoint>();
            attYawRefBuffer = new List<LogPoint>();
            attBuffersFull = false;

            altEstBuffer = new List<LogPoint>();
            altMeasBuffer = new List<LogPoint>();
            altRefBuffer = new List<LogPoint>();
            altBuffersFull = false;

            navXEstBuffer = new List<LogPoint>();
            navXMeasBuffer = new List<LogPoint>();
            navXRefBuffer = new List<LogPoint>();
            navYEstBuffer = new List<LogPoint>();
            navYMeasBuffer = new List<LogPoint>();
            navYRefBuffer = new List<LogPoint>();
            navBuffersFull = false;

            mainMonitoringViewModel.MinValue = 0;
            mainMonitoringViewModel.MaxValue = settingsService.TimeFrame;
            attUpdating = false;
            altUpdating = false;
            navUpdating = false;
        }

        private void ShellService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MainVisible")
            {
                if (shellService.MainVisible)
                {
                    isVisible = true;
                    mainMonitoringViewModel.AttEnable = true;
                    switch (generalService.FlightMode)
                    {
                        case 0:
                            mainMonitoringViewModel.AltMode = mainMonitoringViewModel.AltOverride;
                            mainMonitoringViewModel.NavMode = mainMonitoringViewModel.NavOverride;
                            break;
                        case 1:
                            mainMonitoringViewModel.AltMode = true;
                            mainMonitoringViewModel.NavMode = mainMonitoringViewModel.NavOverride;
                            break;
                        case 2:
                            mainMonitoringViewModel.AltMode = true;
                            mainMonitoringViewModel.NavMode = true;
                            break;
                        case 3:
                            mainMonitoringViewModel.AltMode = mainMonitoringViewModel.AltOverride;
                            mainMonitoringViewModel.NavMode = mainMonitoringViewModel.NavOverride;
                            break;
                        default:
                            mainMonitoringViewModel.AltMode = true;
                            mainMonitoringViewModel.NavMode = true;
                            break;
                    }
                }
                else
                {
                    isVisible = false;
                    mainMonitoringViewModel.AttEnable = false;
                    mainMonitoringViewModel.AltMode = false;
                    mainMonitoringViewModel.NavMode = false;
                }
            }
        }

        private void GeneralService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FlightMode")
            {
                switch (generalService.FlightMode)
                {
                    case 0:
                        mainMonitoringViewModel.AltMode = mainMonitoringViewModel.AltOverride;
                        mainMonitoringViewModel.NavMode = mainMonitoringViewModel.NavOverride;
                        break;
                    case 1:
                        mainMonitoringViewModel.AltMode = true;
                        mainMonitoringViewModel.NavMode = mainMonitoringViewModel.NavOverride;
                        break;
                    case 2:
                        mainMonitoringViewModel.AltMode = true;
                        mainMonitoringViewModel.NavMode = true;
                        break;
                    case 3:
                        mainMonitoringViewModel.AltMode = mainMonitoringViewModel.AltOverride;
                        mainMonitoringViewModel.NavMode = mainMonitoringViewModel.NavOverride;
                        break;
                    default:
                        mainMonitoringViewModel.AltMode = true;
                        mainMonitoringViewModel.NavMode = true;
                        break;
                }
            }
        }


        public void Initialize()
        {
            shellService.MainMonitoring = mainMonitoringViewModel;
            mainMonitoringViewModel.PropertyChanged += MainMonitoringViewModel_PropertyChanged;
        }

        private void MainMonitoringViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AltOverride")
            {
                if (mainMonitoringViewModel.AltOverride)
                    mainMonitoringViewModel.AltMode = true;
                else
                {
                    mainMonitoringViewModel.AltMode = (generalService.FlightMode == 1 || generalService.FlightMode == 2);
                }
            }
            else if (e.PropertyName == "NavOverride")
            {
                if (mainMonitoringViewModel.NavOverride)
                    mainMonitoringViewModel.NavMode = true;
                else
                {
                    mainMonitoringViewModel.NavMode = (generalService.FlightMode == 2);
                }
            }
        }


        private void DataUpdateEventHandler(object sender, DataUpdateEventArgs args)
        {
            if (args.DataSets == null || args.DataSets.Count == 0)
                return; // No updating to be done

            generalService.FlightMode = args.DataSets.Last().FlightMode; //Set flight mode to most up to date value

            if (isVisible)
            {
                if (attUpdating)
                {
                    UpdateAttitudeBuffer(args.DataSets);
                }
                else
                {
                    attUpdating = true;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateAttitude(args.DataSets);
                        attUpdating = false;
                    });
                }
                if (mainMonitoringViewModel.AltMode || mainMonitoringViewModel.AltOverride)
                {
                    if (altUpdating)
                    {
                        UpdateAltitudeBuffer(args.DataSets);
                    }
                    else
                    {
                        altUpdating = true;
                        ThreadPool.QueueUserWorkItem((x) =>
                        {
                            UpdateAltitude(args.DataSets);
                            altUpdating = false;
                        });
                    }
                }
                if (mainMonitoringViewModel.NavMode || mainMonitoringViewModel.NavOverride)
                {
                    if (navUpdating)
                    {
                        UpdateNavigationBuffer(args.DataSets);
                    }
                    else
                    {
                        navUpdating = true;
                        ThreadPool.QueueUserWorkItem((x) =>
                        {
                            UpdateNavigation(args.DataSets);
                            navUpdating = false;
                        });
                    }
                }
            }
            else
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    UpdateAttitudeBuffer(args.DataSets);

                    UpdateAltitudeBuffer(args.DataSets);

                    UpdateNavigationBuffer(args.DataSets);
                });
            }
        }


        private void UpdateAttitude(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (attRollEstBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                return;
            }

            if(addBuffer)
            {
                UpdateAttitudeBuffer(dataSets);
                if (attBuffersFull)
                {
                    mainMonitoringViewModel.RollEstPoints.Clear();
                    mainMonitoringViewModel.RollMeasPoints.Clear();
                    mainMonitoringViewModel.RollRefPoints.Clear();

                    mainMonitoringViewModel.PitchEstPoints.Clear();
                    mainMonitoringViewModel.PitchMeasPoints.Clear();
                    mainMonitoringViewModel.PitchRefPoints.Clear();

                    mainMonitoringViewModel.YawEstPoints.Clear();
                    mainMonitoringViewModel.YawMeasPoints.Clear();
                    mainMonitoringViewModel.YawRefPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (mainMonitoringViewModel.RollEstPoints.Count > 0 && (attRollEstBuffer.Last().TimeVal - mainMonitoringViewModel.RollEstPoints[index].TimeVal) > settingsService.TimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        mainMonitoringViewModel.RollEstPoints.RemoveAt(0);
                        mainMonitoringViewModel.RollMeasPoints.RemoveAt(0);
                        mainMonitoringViewModel.RollRefPoints.RemoveAt(0);

                        mainMonitoringViewModel.PitchEstPoints.RemoveAt(0);
                        mainMonitoringViewModel.PitchMeasPoints.RemoveAt(0);
                        mainMonitoringViewModel.PitchRefPoints.RemoveAt(0);

                        mainMonitoringViewModel.YawEstPoints.RemoveAt(0);
                        mainMonitoringViewModel.YawMeasPoints.RemoveAt(0);
                        mainMonitoringViewModel.YawRefPoints.RemoveAt(0);
                    }                   
                }
                mainMonitoringViewModel.RollEstPoints.AddRange(attRollEstBuffer);
                mainMonitoringViewModel.RollMeasPoints.AddRange(attRollMeasBuffer);
                mainMonitoringViewModel.RollRefPoints.AddRange(attRollRefBuffer);

                mainMonitoringViewModel.PitchEstPoints.AddRange(attPitchEstBuffer);
                mainMonitoringViewModel.PitchMeasPoints.AddRange(attPitchMeasBuffer);
                mainMonitoringViewModel.PitchRefPoints.AddRange(attPitchRefBuffer);

                mainMonitoringViewModel.YawEstPoints.AddRange(attYawEstBuffer);
                mainMonitoringViewModel.YawMeasPoints.AddRange(attYawMeasBuffer);
                mainMonitoringViewModel.YawRefPoints.AddRange(attYawRefBuffer);

                attRollEstBuffer.Clear();
                attRollMeasBuffer.Clear();
                attRollRefBuffer.Clear();

                attPitchEstBuffer.Clear();
                attPitchMeasBuffer.Clear();
                attPitchRefBuffer.Clear();

                attYawEstBuffer.Clear();
                attYawMeasBuffer.Clear();
                attYawRefBuffer.Clear();
                attBuffersFull = false;
            }
            else
            {
                int index = 0; // First index which stays
                while (index < mainMonitoringViewModel.RollEstPoints.Count && (dataSets.Last().TimeStamp - mainMonitoringViewModel.RollEstPoints[index].TimeVal) > settingsService.TimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    mainMonitoringViewModel.RollEstPoints.RemoveAt(0);
                    mainMonitoringViewModel.RollMeasPoints.RemoveAt(0);
                    mainMonitoringViewModel.RollRefPoints.RemoveAt(0);

                    mainMonitoringViewModel.PitchEstPoints.RemoveAt(0);
                    mainMonitoringViewModel.PitchMeasPoints.RemoveAt(0);
                    mainMonitoringViewModel.PitchRefPoints.RemoveAt(0);

                    mainMonitoringViewModel.YawEstPoints.RemoveAt(0);
                    mainMonitoringViewModel.YawMeasPoints.RemoveAt(0);
                    mainMonitoringViewModel.YawRefPoints.RemoveAt(0);
                }
                List<LogPoint> newRollEstPoints = new List<LogPoint>();
                List<LogPoint> newRollMeasPoints = new List<LogPoint>();
                List<LogPoint> newRollRefPoints = new List<LogPoint>();

                List<LogPoint> newPitchEstPoints = new List<LogPoint>();
                List<LogPoint> newPitchMeasPoints = new List<LogPoint>();
                List<LogPoint> newPitchRefPoints = new List<LogPoint>();

                List<LogPoint> newYawEstPoints = new List<LogPoint>();
                List<LogPoint> newYawMeasPoints = new List<LogPoint>();
                List<LogPoint> newYawRefPoints = new List<LogPoint>();

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
                }

                mainMonitoringViewModel.RollEstPoints.AddRange(newRollEstPoints);
                mainMonitoringViewModel.RollMeasPoints.AddRange(newRollMeasPoints);
                mainMonitoringViewModel.RollRefPoints.AddRange(newRollRefPoints);

                mainMonitoringViewModel.PitchEstPoints.AddRange(newPitchEstPoints);
                mainMonitoringViewModel.PitchMeasPoints.AddRange(newPitchMeasPoints);
                mainMonitoringViewModel.PitchRefPoints.AddRange(newPitchRefPoints);

                mainMonitoringViewModel.YawEstPoints.AddRange(newYawEstPoints);
                mainMonitoringViewModel.YawMeasPoints.AddRange(newYawMeasPoints);
                mainMonitoringViewModel.YawRefPoints.AddRange(newYawRefPoints);
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
                    mainMonitoringViewModel.HeightEstPoints.Clear();
                    mainMonitoringViewModel.HeightMeasPoints.Clear();
                    mainMonitoringViewModel.HeightRefPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (mainMonitoringViewModel.HeightEstPoints.Count > 0 && (altEstBuffer.Last().TimeVal - mainMonitoringViewModel.HeightEstPoints[index].TimeVal) > settingsService.TimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        mainMonitoringViewModel.HeightEstPoints.RemoveAt(0);
                        mainMonitoringViewModel.HeightMeasPoints.RemoveAt(0);
                        mainMonitoringViewModel.HeightRefPoints.RemoveAt(0);
                    }
                }
                mainMonitoringViewModel.HeightEstPoints.AddRange(altEstBuffer);
                mainMonitoringViewModel.HeightMeasPoints.AddRange(altMeasBuffer);
                mainMonitoringViewModel.HeightRefPoints.AddRange(altRefBuffer);
                altEstBuffer.Clear();
                altMeasBuffer.Clear();
                altRefBuffer.Clear();
                altBuffersFull = false;
            }
            else
            {
                // Search which values to remove
                int index = 0; // First index which stays
                while (index < mainMonitoringViewModel.HeightEstPoints.Count && (dataSets.Last().TimeStamp - mainMonitoringViewModel.HeightEstPoints[index].TimeVal) > settingsService.TimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    mainMonitoringViewModel.HeightEstPoints.RemoveAt(0);
                    mainMonitoringViewModel.HeightMeasPoints.RemoveAt(0);
                    mainMonitoringViewModel.HeightRefPoints.RemoveAt(0);
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

                mainMonitoringViewModel.HeightEstPoints.AddRange(newEstPoints);
                mainMonitoringViewModel.HeightMeasPoints.AddRange(newMeasPoints);
                mainMonitoringViewModel.HeightRefPoints.AddRange(newRefPoints);
            }
        }

        private void UpdateNavigation(List<DataSet> dataSets)
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
                UpdateNavigationBuffer(dataSets);
                if (navBuffersFull)
                {
                    // Buffer will delete all current values
                    mainMonitoringViewModel.NavXEstPoints.Clear();
                    mainMonitoringViewModel.NavXMeasPoints.Clear();
                    mainMonitoringViewModel.NavXRefPoints.Clear();

                    mainMonitoringViewModel.NavYEstPoints.Clear();
                    mainMonitoringViewModel.NavYMeasPoints.Clear();
                    mainMonitoringViewModel.NavYRefPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (mainMonitoringViewModel.NavXEstPoints.Count > 0 && (navXEstBuffer.Last().TimeVal - mainMonitoringViewModel.NavXEstPoints[index].TimeVal) > settingsService.TimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        mainMonitoringViewModel.NavXEstPoints.RemoveAt(0);
                        mainMonitoringViewModel.NavXMeasPoints.RemoveAt(0);
                        mainMonitoringViewModel.NavXRefPoints.RemoveAt(0);

                        mainMonitoringViewModel.NavYEstPoints.RemoveAt(0);
                        mainMonitoringViewModel.NavYMeasPoints.RemoveAt(0);
                        mainMonitoringViewModel.NavYRefPoints.RemoveAt(0);
                    }
                }
                mainMonitoringViewModel.NavXEstPoints.AddRange(navXEstBuffer);
                mainMonitoringViewModel.NavXMeasPoints.AddRange(navXMeasBuffer);
                mainMonitoringViewModel.NavXRefPoints.AddRange(navXRefBuffer);

                mainMonitoringViewModel.NavYEstPoints.AddRange(navYEstBuffer);
                mainMonitoringViewModel.NavYMeasPoints.AddRange(navYMeasBuffer);
                mainMonitoringViewModel.NavYRefPoints.AddRange(navYRefBuffer);

                navXEstBuffer.Clear();
                navXMeasBuffer.Clear();
                navXRefBuffer.Clear();

                navYEstBuffer.Clear();
                navYMeasBuffer.Clear();
                navYRefBuffer.Clear();
                navBuffersFull = false;
            }
            else
            {
                // Search which values to remove
                int index = 0; // First index which stays
                while (index < mainMonitoringViewModel.NavXEstPoints.Count && (dataSets.Last().TimeStamp - mainMonitoringViewModel.NavXEstPoints[index].TimeVal) > settingsService.TimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    mainMonitoringViewModel.NavXEstPoints.RemoveAt(0);
                    mainMonitoringViewModel.NavXMeasPoints.RemoveAt(0);
                    mainMonitoringViewModel.NavXRefPoints.RemoveAt(0);

                    mainMonitoringViewModel.NavYEstPoints.RemoveAt(0);
                    mainMonitoringViewModel.NavYMeasPoints.RemoveAt(0);
                    mainMonitoringViewModel.NavYRefPoints.RemoveAt(0);
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

                mainMonitoringViewModel.NavXEstPoints.AddRange(newXEstPoints);
                mainMonitoringViewModel.NavXMeasPoints.AddRange(newXMeasPoints);
                mainMonitoringViewModel.NavXRefPoints.AddRange(newXRefPoints);

                mainMonitoringViewModel.NavYEstPoints.AddRange(newYEstPoints);
                mainMonitoringViewModel.NavYMeasPoints.AddRange(newYMeasPoints);
                mainMonitoringViewModel.NavYRefPoints.AddRange(newYRefPoints);
            }
        }

        private void UpdateAttitudeBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)attRollEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (attRollEstBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.TimeFrame)
                {
                    attBuffersFull = true;

                    attRollEstBuffer.RemoveAt(0);
                    attPitchEstBuffer.RemoveAt(0);
                    attYawEstBuffer.RemoveAt(0);
                    attRollMeasBuffer.RemoveAt(0);
                    attPitchMeasBuffer.RemoveAt(0);
                    attYawMeasBuffer.RemoveAt(0);
                    attRollRefBuffer.RemoveAt(0);
                    attPitchRefBuffer.RemoveAt(0);
                    attYawRefBuffer.RemoveAt(0);

                    firstTime = (float)attRollEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;
                }

                attRollEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Roll));
                attRollMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Roll));
                attRollRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Roll));

                attPitchEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Pitch));
                attPitchMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Pitch));
                attPitchRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Pitch));

                attYawEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Yaw));
                attYawMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Yaw));
                attYawRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Yaw));
            }
        }

        private void UpdateAltitudeBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)altEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (altEstBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.TimeFrame)
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

        private void UpdateNavigationBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)navXEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (navXEstBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.TimeFrame)
                {
                    navBuffersFull = true;
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

        public void Reset()
        {
            mainMonitoringViewModel.RollEstPoints.Clear();
            mainMonitoringViewModel.PitchEstPoints.Clear();
            mainMonitoringViewModel.YawEstPoints.Clear();
            mainMonitoringViewModel.RollMeasPoints.Clear();
            mainMonitoringViewModel.PitchMeasPoints.Clear();
            mainMonitoringViewModel.YawMeasPoints.Clear();
            mainMonitoringViewModel.RollRefPoints.Clear();
            mainMonitoringViewModel.PitchRefPoints.Clear();
            mainMonitoringViewModel.YawRefPoints.Clear();
            attRollEstBuffer.Clear();
            attPitchEstBuffer.Clear();
            attYawEstBuffer.Clear();
            attRollMeasBuffer.Clear();
            attPitchMeasBuffer.Clear();
            attYawMeasBuffer.Clear();
            attRollRefBuffer.Clear();
            attPitchRefBuffer.Clear();
            attYawRefBuffer.Clear();

            mainMonitoringViewModel.HeightEstPoints.Clear();
            mainMonitoringViewModel.HeightMeasPoints.Clear();
            mainMonitoringViewModel.HeightRefPoints.Clear();
            altEstBuffer.Clear();
            altMeasBuffer.Clear();
            altRefBuffer.Clear();

            mainMonitoringViewModel.NavXEstPoints.Clear();
            mainMonitoringViewModel.NavYEstPoints.Clear();
            mainMonitoringViewModel.NavXMeasPoints.Clear();
            mainMonitoringViewModel.NavYMeasPoints.Clear();
            mainMonitoringViewModel.NavXRefPoints.Clear();
            mainMonitoringViewModel.NavYRefPoints.Clear();
            navXEstBuffer.Clear();
            navYEstBuffer.Clear();
            navXMeasBuffer.Clear();
            navYMeasBuffer.Clear();
            navXRefBuffer.Clear();
            navYRefBuffer.Clear();
            attBuffersFull = false;
            altBuffersFull = false;
            navBuffersFull = false;
            attUpdating = false;
            altUpdating = false;
            navUpdating = false;
        }
    }
}