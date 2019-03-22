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

        private DelegateCommand ChartUpdated;

        private bool initializing = false;
        private bool isVisible;
        private bool drawn;

        #region Buffers

        List<LogPoint> attRollEstBuffer;
        List<LogPoint> attRollMeasBuffer;
        List<LogPoint> attRollRefBuffer;

        List<LogPoint> attPitchEstBuffer;
        List<LogPoint> attPitchMeasBuffer;
        List<LogPoint> attPitchRefBuffer;

        List<LogPoint> attYawEstBuffer;
        List<LogPoint> attYawMeasBuffer;
        List<LogPoint> attYawRefBuffer;

        List<LogPoint> altEstBuffer;
        List<LogPoint> altMeasBuffer;
        List<LogPoint> altRefBuffer;

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
            ChartUpdated = new DelegateCommand(ChartUpdatedCommand);
            generalService.PropertyChanged += GeneralService_PropertyChanged;
            shellService.PropertyChanged += ShellService_PropertyChanged;
            isVisible = true;
            drawn = true;

            attRollEstBuffer = new List<LogPoint>();
            attRollMeasBuffer = new List<LogPoint>();
            attRollRefBuffer = new List<LogPoint>();
            attPitchEstBuffer = new List<LogPoint>();
            attPitchMeasBuffer = new List<LogPoint>();
            attPitchRefBuffer = new List<LogPoint>();
            attYawEstBuffer = new List<LogPoint>();
            attYawMeasBuffer = new List<LogPoint>();
            attYawRefBuffer = new List<LogPoint>();

            altEstBuffer = new List<LogPoint>();
            altMeasBuffer = new List<LogPoint>();
            altRefBuffer = new List<LogPoint>();

            navXEstBuffer = new List<LogPoint>();
            navXMeasBuffer = new List<LogPoint>();
            navXRefBuffer = new List<LogPoint>();
            navYEstBuffer = new List<LogPoint>();
            navYMeasBuffer = new List<LogPoint>();
            navYRefBuffer = new List<LogPoint>();
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
                    drawn = false;
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
            mainMonitoringViewModel.ChartUpdated = ChartUpdated;
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
                    mainMonitoringViewModel.AltMode = generalService.FlightMode != 0 && generalService.FlightMode != 3;
                }
            }
            else if (e.PropertyName == "NavOverride")
            {
                if (mainMonitoringViewModel.NavOverride)
                    mainMonitoringViewModel.NavMode = true;
                else
                {
                    mainMonitoringViewModel.NavMode = generalService.FlightMode != 0 && generalService.FlightMode != 1 && generalService.FlightMode != 3;
                }
            }
        }

        public void InitializePlots()
        {
            initializing = true;

            InitializeAttitude();
            InitializeAltitude();
            InitializeNavigation();
        }

        private void DataUpdateEventHandler(object sender, DataUpdateEventArgs args)
        {
            if (args.DataSets == null || args.DataSets.Count == 0)
                return; // No updating to be done

            generalService.FlightMode = args.DataSets.Last().FlightMode; //Set flight mode to most up to date value

            if (isVisible)
            {
                bool addBuffer;
                if (drawn)
                    addBuffer = false;
                else
                    addBuffer = true;
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    UpdateAttitude(args.DataSets, addBuffer);
                });
                if (mainMonitoringViewModel.AltMode || mainMonitoringViewModel.AltOverride)
                {
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateAltitude(args.DataSets, addBuffer);
                    });
                }
                if (mainMonitoringViewModel.NavMode || mainMonitoringViewModel.NavOverride)
                {
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateNavigation(args.DataSets, addBuffer);
                    });
                }
                if (!drawn)
                    drawn = true;
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


        private void InitializeAttitude()
        {
            SeriesCollection RollCollection = new SeriesCollection();
            SeriesCollection PitchCollection = new SeriesCollection();
            SeriesCollection YawCollection = new SeriesCollection();

            mainMonitoringViewModel.AttRollSeries = RollCollection;
            mainMonitoringViewModel.AttPitchSeries = PitchCollection;
            mainMonitoringViewModel.AttYawSeries = YawCollection;

            RollCollection.Add(new GLineSeries()
            {
                Title = "Roll Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            RollCollection.Add(new GLineSeries()
            {
                Title = "Roll Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            RollCollection.Add(new GLineSeries()
            {
                Title = "Roll Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });

            // Make line series for pitch
            PitchCollection.Add(new GLineSeries()
            {
                Title = "Pitch Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            PitchCollection.Add(new GLineSeries()
            {
                Title = "Pitch Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            PitchCollection.Add(new GLineSeries()
            {
                Title = "Pitch Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });

            // Make line series for yaw
            YawCollection.Add(new GLineSeries()
            {
                Title = "Yaw Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            YawCollection.Add(new GLineSeries()
            {
                Title = "Yaw Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            YawCollection.Add(new GLineSeries()
            {
                Title = "Yaw Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });

        }

        private void InitializeAltitude()
        {
            SeriesCollection AltSeries = new SeriesCollection();

            // Make line series for altitude
            AltSeries.Add(new GLineSeries()
            {
                Title = "Height Estimate",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            AltSeries.Add(new GLineSeries()
            {
                Title = "Height Measured",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });
            AltSeries.Add(new GLineSeries()
            {
                Title = "Height Reference",
                LineSmoothness = 1,
                Fill = System.Windows.Media.Brushes.Transparent,
                Values = new GearedValues<LogPoint>().WithQuality(Quality.Medium),
                PointGeometry = null
            });

            mainMonitoringViewModel.AltSeries = AltSeries;
        }

        private void InitializeNavigation()
        {
            SeriesCollection NavXCollection = new SeriesCollection();
            SeriesCollection NavYCollection = new SeriesCollection();

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

            mainMonitoringViewModel.NavXSeries = NavXCollection;
            mainMonitoringViewModel.NavYSeries = NavYCollection;
        }

        private void UpdateAttitude(List<DataSet> dataSets, bool addBuffer)
        {
            if (dataSets == null || dataSets.Count == 0)
            {
                if (!addBuffer)
                    return;
                else if (attRollEstBuffer == null || attRollEstBuffer.Count == 0)
                    return;
            }

            float newLastTime;
            if (dataSets == null || dataSets.Count == 0)
                newLastTime = (float)attRollEstBuffer.Last().TimeVal;
            else
                newLastTime = dataSets.Last().TimeStamp;

            GearedValues<LogPoint> RollPointsEst = (GearedValues<LogPoint>)mainMonitoringViewModel.AttRollSeries[0].Values;
            GearedValues<LogPoint> RollPointsMeas = (GearedValues<LogPoint>)mainMonitoringViewModel.AttRollSeries[1].Values;
            GearedValues<LogPoint> RollPointRef = (GearedValues<LogPoint>)mainMonitoringViewModel.AttRollSeries[2].Values;

            GearedValues<LogPoint> PitchPointsEst = (GearedValues<LogPoint>)mainMonitoringViewModel.AttPitchSeries[0].Values;
            GearedValues<LogPoint> PitchPointsMeas = (GearedValues<LogPoint>)mainMonitoringViewModel.AttPitchSeries[1].Values;
            GearedValues<LogPoint> PitchPointRef = (GearedValues<LogPoint>)mainMonitoringViewModel.AttPitchSeries[2].Values;

            GearedValues<LogPoint> YawPointsEst = (GearedValues<LogPoint>)mainMonitoringViewModel.AttYawSeries[0].Values;
            GearedValues<LogPoint> YawPointsMeas = (GearedValues<LogPoint>)mainMonitoringViewModel.AttYawSeries[1].Values;
            GearedValues<LogPoint> YawPointRef = (GearedValues<LogPoint>)mainMonitoringViewModel.AttYawSeries[2].Values;

            double firstTimeValue = RollPointsEst.DefaultIfEmpty(new LogPoint(0, 0)).FirstOrDefault().TimeVal;

            if ((newLastTime - firstTimeValue) > settingsService.TimeFrame)
            {
                // Remove points out of frame
                double timeToCut = newLastTime - firstTimeValue - settingsService.TimeFrame;
                int index = 0;
                while (index < RollPointsEst.Count && RollPointsEst[index].TimeVal < (firstTimeValue + timeToCut))
                {
                    index++;
                }
                for (int i = index - 1; i >= 0; i--)
                {
                    // Each plot has the same x values
                    RollPointsEst.RemoveAt(i);
                    RollPointsMeas.RemoveAt(i);
                    RollPointRef.RemoveAt(i);

                    PitchPointsEst.RemoveAt(i);
                    PitchPointsMeas.RemoveAt(i);
                    PitchPointRef.RemoveAt(i);

                    YawPointsEst.RemoveAt(i);
                    YawPointsMeas.RemoveAt(i);
                    YawPointRef.RemoveAt(i);
                }
            }
            List<LogPoint> newRollEstPoints;
            List<LogPoint> newRollMeasPoints;
            List<LogPoint> newRollRefPoints;

            List<LogPoint> newPitchEstPoints;
            List<LogPoint> newPitchMeasPoints;
            List<LogPoint> newPitchRefPoints;

            List<LogPoint> newYawEstPoints;
            List<LogPoint> newYawMeasPoints;
            List<LogPoint> newYawRefPoints;

            if (addBuffer)
            {
                newRollEstPoints = attRollEstBuffer;
                newRollMeasPoints = attRollMeasBuffer;
                newRollRefPoints = attRollRefBuffer;

                newPitchEstPoints = attPitchEstBuffer;
                newPitchMeasPoints = attPitchMeasBuffer;
                newPitchRefPoints = attPitchRefBuffer;

                newYawEstPoints = attYawEstBuffer;
                newYawMeasPoints = attYawMeasBuffer;
                newYawRefPoints = attYawRefBuffer;

                // Reset Buffers
                attRollRefBuffer = new List<LogPoint>();
                attRollMeasBuffer = new List<LogPoint>();
                attRollMeasBuffer = new List<LogPoint>();
                attPitchEstBuffer = new List<LogPoint>();
                attPitchMeasBuffer = new List<LogPoint>();
                attPitchRefBuffer = new List<LogPoint>();
                attYawEstBuffer = new List<LogPoint>();
                attYawMeasBuffer = new List<LogPoint>();
                attYawRefBuffer = new List<LogPoint>();
            }
            else
            {
                newRollEstPoints = new List<LogPoint>();
                newRollMeasPoints = new List<LogPoint>();
                newRollRefPoints = new List<LogPoint>();

                newPitchEstPoints = new List<LogPoint>();
                newPitchMeasPoints = new List<LogPoint>();
                newPitchRefPoints = new List<LogPoint>();

                newYawEstPoints = new List<LogPoint>();
                newYawMeasPoints = new List<LogPoint>();
                newYawRefPoints = new List<LogPoint>();
            }


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
            lock (RollPointsEst)
            {
                RollPointsEst.AddRange(newRollEstPoints);
            }
            lock (RollPointsMeas)
            {
                RollPointsMeas.AddRange(newRollMeasPoints);
            }
            lock (RollPointRef)
            {
                RollPointRef.AddRange(newRollRefPoints);
            }

            lock (PitchPointsEst)
            {
                PitchPointsEst.AddRange(newPitchEstPoints);
            }
            lock (PitchPointsMeas)
            {
                PitchPointsMeas.AddRange(newPitchMeasPoints);
            }
            lock (PitchPointRef)
            {
                PitchPointRef.AddRange(newPitchRefPoints);
            }

            lock (YawPointsEst)
            {
                YawPointsEst.AddRange(newYawEstPoints);
            }
            lock (YawPointsMeas)
            {
                YawPointsMeas.AddRange(newYawMeasPoints);
            }
            lock (YawPointRef)
            {
                YawPointRef.AddRange(newYawRefPoints);
            }
        }

        private void UpdateAltitude(List<DataSet> dataSets, bool addBuffer)
        {
            if (dataSets == null || dataSets.Count == 0)
            {
                if (!addBuffer)
                    return;
                else if (altEstBuffer == null || altEstBuffer.Count == 0)
                    return;
            }

            float newLastTime;
            if (dataSets == null || dataSets.Count == 0)
                newLastTime = (float)altEstBuffer.Last().TimeVal;
            else
                newLastTime = dataSets.Last().TimeStamp;

            GearedValues<LogPoint> AltEstPoints = (GearedValues<LogPoint>)mainMonitoringViewModel.AltSeries[0].Values;
            GearedValues<LogPoint> AltMeasPoints = (GearedValues<LogPoint>)mainMonitoringViewModel.AltSeries[1].Values;
            GearedValues<LogPoint> AltRefPoints = (GearedValues<LogPoint>)mainMonitoringViewModel.AltSeries[2].Values;

            double firstTimeValue = AltEstPoints.DefaultIfEmpty(new LogPoint(0, 0)).FirstOrDefault().TimeVal;

            if ((newLastTime - firstTimeValue) > settingsService.TimeFrame)
            {
                // Remove points out of frame
                double timeToCut = newLastTime - firstTimeValue - settingsService.TimeFrame;
                int index = 0;
                while (index < AltEstPoints.Count && AltEstPoints[index].TimeVal < (firstTimeValue + timeToCut))
                {
                    index++;
                }
                for (int i = index - 1; i >= 0; i--)
                {
                    // Each plot has the same x values
                    AltEstPoints.RemoveAt(i);
                    AltMeasPoints.RemoveAt(i);
                    AltRefPoints.RemoveAt(i);
                }
            }

            List<LogPoint> newAltEstPoints;
            List<LogPoint> newAltMeasPoints;
            List<LogPoint> newAltRefPoints;

            if (addBuffer)
            {
                newAltEstPoints = altEstBuffer;
                newAltMeasPoints = altMeasBuffer;
                newAltRefPoints = altRefBuffer;

                altEstBuffer = new List<LogPoint>();
                altMeasBuffer = new List<LogPoint>();
                altRefBuffer = new List<LogPoint>();
            }
            else
            {
                newAltEstPoints = new List<LogPoint>();
                newAltMeasPoints = new List<LogPoint>();
                newAltRefPoints = new List<LogPoint>();
            }

            foreach (DataSet dataSet in dataSets)
            {
                newAltEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightEstimate));
                newAltMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightMeasured));
                newAltRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightReference));
            }

            lock (AltEstPoints)
            {
                AltEstPoints.AddRange(newAltEstPoints);
            }
            lock (AltMeasPoints)
            {
                AltMeasPoints.AddRange(newAltMeasPoints);
            }
            lock (AltRefPoints)
            {
                AltRefPoints.AddRange(newAltRefPoints);
            }

        }

        private void UpdateNavigation(List<DataSet> dataSets, bool addBuffer)
        {
            if (dataSets == null || dataSets.Count == 0)
            {
                if (!addBuffer)
                    return;
                else if (navXEstBuffer == null || navXEstBuffer.Count == 0)
                    return;
            }

            float newLastTime;
            if (dataSets == null || dataSets.Count == 0)
                newLastTime = (float)navXEstBuffer.Last().TimeVal;
            else
                newLastTime = dataSets.Last().TimeStamp;

            GearedValues<LogPoint> XCoorEst = (GearedValues<LogPoint>)mainMonitoringViewModel.NavXSeries[0].Values;
            GearedValues<LogPoint> XCoorMeas = (GearedValues<LogPoint>)mainMonitoringViewModel.NavXSeries[1].Values;
            GearedValues<LogPoint> XCoorRef = (GearedValues<LogPoint>)mainMonitoringViewModel.NavXSeries[2].Values;

            GearedValues<LogPoint> YCoorEst = (GearedValues<LogPoint>)mainMonitoringViewModel.NavYSeries[0].Values;
            GearedValues<LogPoint> YCoorMeas = (GearedValues<LogPoint>)mainMonitoringViewModel.NavYSeries[1].Values;
            GearedValues<LogPoint> YCoorRef = (GearedValues<LogPoint>)mainMonitoringViewModel.NavYSeries[2].Values;

            double firstTimeValue = XCoorEst.DefaultIfEmpty(new LogPoint(0, 0)).FirstOrDefault().TimeVal;

            if ((newLastTime - firstTimeValue) > settingsService.TimeFrame)
            {
                // Remove points out of frame
                double timeToCut = newLastTime - firstTimeValue - settingsService.TimeFrame;
                int index = 0;
                while (index < XCoorEst.Count && XCoorEst[index].TimeVal < (firstTimeValue + timeToCut))
                {
                    index++;
                }
                for (int i = index - 1; i >= 0; i--)
                {
                    // Each plot has the same x values
                    XCoorEst.RemoveAt(i);
                    XCoorMeas.RemoveAt(i);
                    XCoorRef.RemoveAt(i);

                    YCoorEst.RemoveAt(i);
                    YCoorMeas.RemoveAt(i);
                    YCoorRef.RemoveAt(i);
                }
            }
            List<LogPoint> newXCoorEstPoints;
            List<LogPoint> newXCoorMeasPoints;
            List<LogPoint> newXCoorRefPoints;

            List<LogPoint> newYCoorEstPoints;
            List<LogPoint> newYCoorMeasPoints;
            List<LogPoint> newYCoorRefPoints;

            if (addBuffer)
            {
                newXCoorEstPoints = navXEstBuffer;
                newXCoorMeasPoints = navXMeasBuffer;
                newXCoorRefPoints = navXRefBuffer;

                newYCoorEstPoints = navYEstBuffer;
                newYCoorMeasPoints = navYMeasBuffer;
                newYCoorRefPoints = navYRefBuffer;

                navXEstBuffer = new List<LogPoint>();
                navXMeasBuffer = new List<LogPoint>();
                navXRefBuffer = new List<LogPoint>();
                navYEstBuffer = new List<LogPoint>();
                navYMeasBuffer = new List<LogPoint>();
                navYRefBuffer = new List<LogPoint>();
            }
            else
            {
                newXCoorEstPoints = new List<LogPoint>();
                newXCoorMeasPoints = new List<LogPoint>();
                newXCoorRefPoints = new List<LogPoint>();
                newYCoorEstPoints = new List<LogPoint>();
                newYCoorMeasPoints = new List<LogPoint>();
                newYCoorRefPoints = new List<LogPoint>();
            }

            foreach (DataSet dataSet in dataSets)
            {
                newXCoorEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.XPosition));
                newXCoorMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.XPosition));
                newXCoorRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.XPosition));

                newYCoorEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.YPosition));
                newYCoorMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.YPosition));
                newYCoorRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.YPosition));
            }

            lock (XCoorEst)
            {
                XCoorEst.AddRange(newXCoorEstPoints);
            }
            lock (XCoorMeas)
            {
                XCoorMeas.AddRange(newXCoorMeasPoints);
            }
            lock (XCoorRef)
            {
                XCoorRef.AddRange(newXCoorRefPoints);
            }

            lock (YCoorEst)
            {
                YCoorEst.AddRange(newYCoorEstPoints);
            }
            lock (YCoorMeas)
            {
                YCoorMeas.AddRange(newYCoorMeasPoints);
            }
            lock (YCoorRef)
            {
                YCoorRef.AddRange(newYCoorRefPoints);
            }

        }

        private void UpdateAttitudeBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done
            foreach (DataSet dataSet in dataSets)
            {
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
            foreach (DataSet dataSet in dataSets)
            {
                altEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightEstimate));
                altMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightMeasured));
                altRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.HeightReference));
            }
        }

        private void UpdateNavigationBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done
            foreach (DataSet dataSet in dataSets)
            {
                navXEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.XPosition));
                navXMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.XPosition));
                navXRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.XPosition));

                navYEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionEstimate.YPosition));
                navYMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionMeasured.YPosition));
                navYRefBuffer
.Add(new LogPoint(dataSet.TimeStamp, dataSet.PositionReference.YPosition));
            }
        }

        private void ChartUpdatedCommand()
        {
            if (initializing)
            {
                initializing = false;
                ((LineSeries)mainMonitoringViewModel.AttRollSeries[1]).Visibility = System.Windows.Visibility.Hidden;
                ((LineSeries)mainMonitoringViewModel.AttPitchSeries[1]).Visibility = System.Windows.Visibility.Hidden;
                ((LineSeries)mainMonitoringViewModel.AttYawSeries[1]).Visibility = System.Windows.Visibility.Hidden;
                ((LineSeries)mainMonitoringViewModel.AltSeries[1]).Visibility = System.Windows.Visibility.Hidden;
                ((LineSeries)mainMonitoringViewModel.NavXSeries[1]).Visibility = System.Windows.Visibility.Hidden;
                ((LineSeries)mainMonitoringViewModel.NavYSeries[1]).Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
