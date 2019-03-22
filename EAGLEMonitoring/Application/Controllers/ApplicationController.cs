using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.ViewModels;
using EAGLEMonitoring.Domain;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Waf.Applications;

namespace EAGLEMonitoring.Application.Controllers
{
    [Export]
    public class ApplicationController
    {
        private readonly ShellViewModel shellViewModel;
        private readonly IDataFetchService dataFetchService;
        private readonly IDataUpdateService dataUpdateService;
        private readonly IGeneralService generalService;
        private readonly ISettingsService settingsService;
        private readonly IShellService shellService;
        private readonly MainMonitoringController mainMonitoringController;
        private readonly NetworkAccessThreadController networkAccessThreadController;
        private readonly StatusBarController statusBarController;
        private readonly AttitudeMonitoringController attitudeMonitoringController;
        private readonly AltitudeMonitoringController altitudeMonitoringController;
        private readonly NavigationMonitoringController navigationMonitoringController;

        private System.Timers.Timer guiUpdateTimer;

        private DelegateCommand ChangeTabCommand;

        [ImportingConstructor]
        public ApplicationController(ShellViewModel shellViewModel,
            MainMonitoringController mainMonitoringController,
            NetworkAccessThreadController networkAccessThreadController,
            StatusBarController statusBarController,
            AttitudeMonitoringController attitudeMonitoringController,
            AltitudeMonitoringController altitudeMonitoringController,
            NavigationMonitoringController navigationMonitoringController,
            IDataFetchService dataFetchService,
            IDataUpdateService dataUpdateService,
            IGeneralService generalService,
            ISettingsService settingsService,
            IShellService shellService)
        {
            this.shellViewModel = shellViewModel;
            this.mainMonitoringController = mainMonitoringController;
            this.networkAccessThreadController = networkAccessThreadController;
            this.attitudeMonitoringController = attitudeMonitoringController;
            this.altitudeMonitoringController = altitudeMonitoringController;
            this.navigationMonitoringController = navigationMonitoringController;
            this.statusBarController = statusBarController;
            this.dataFetchService = dataFetchService;
            this.dataUpdateService = dataUpdateService;
            this.generalService = generalService;
            this.settingsService = settingsService;
            this.shellService = shellService;
            dataUpdateService.ResetDataEvent += Reset;
            ChangeTabCommand = new DelegateCommand(ChangeTab_Command);
        }

        public void Initialize()
        {
            mainMonitoringController.Initialize();
            statusBarController.Initialize();
            attitudeMonitoringController.Initialize();
            altitudeMonitoringController.Initialize();
            navigationMonitoringController.Initialize();
            SetupCharting();
            shellViewModel.ChangeTab = ChangeTabCommand;
        }

        public void Run()
        {
            shellViewModel.Show();
            mainMonitoringController.InitializePlots();
            attitudeMonitoringController.InitializePlots();
            altitudeMonitoringController.InitializePlots();
            navigationMonitoringController.InitializePlots();
            ThreadPool.QueueUserWorkItem((info) =>
            {
                networkAccessThreadController.TryConnect();
            });
            StartGuiUpdate();
        }

        public void Close()
        {
            networkAccessThreadController.Disconnect();
            StopGuiUpdate();
            dataFetchService.SaveLoggingToDisk();
        }

        private void Reset(object sender, EventArgs args)
        {
            StopGuiUpdate();

            mainMonitoringController.InitializePlots();
            attitudeMonitoringController.InitializePlots();
            altitudeMonitoringController.InitializePlots();
            navigationMonitoringController.InitializePlots();

            StopGuiUpdate();
        }

        private void StartGuiUpdate()
        {
            guiUpdateTimer = new System.Timers.Timer()
            {
                Interval = settingsService.FPS,
                AutoReset = true
            };
            guiUpdateTimer.Elapsed += UpdateData;
            guiUpdateTimer.Start();
        }

        private void StopGuiUpdate()
        {
            if (guiUpdateTimer != null)
            {
                guiUpdateTimer.Stop();
                guiUpdateTimer.Dispose();
                guiUpdateTimer = null;
            }
        }

        private void UpdateData(object sender, ElapsedEventArgs args)
        {
            if (generalService.Connected)
            {
                List<DataSet> newDataSets;
                lock (dataFetchService.DataLock)
                {
                    // Load unprocessed datasets
                    newDataSets = dataFetchService.UnprocessedDataSets;
                    dataFetchService.UnprocessedDataSets = new List<DataSet>();
                }
                // Add data to backup buffer.
                if (newDataSets != null && newDataSets.Count != 0)
                {
                    dataFetchService.AllDataSets.AddRange(newDataSets);

                    // TODO: More data processing?

                    // Trigger update
                    dataUpdateService.TriggerDataUpdateEvent(newDataSets);
                }
            }
        }

      
        private void SetupCharting()
        {
            var mapper = Mappers.Xy<LogPoint>()
                .X(value => value.TimeVal)
                .Y(value => value.Value);
            LiveCharts.Charting.For<LogPoint>(mapper);
        }

        private void ChangeTab_Command(object param)
        {
            int tabnum = int.Parse((string)param);
            switch (tabnum)
            {
                case 0:
                    shellService.MainVisible = true;
                    shellService.AttVisible = false;
                    shellService.AltVisible = false;
                    shellService.NavVisible = false;
                    break;
                case 1:
                    shellService.MainVisible = false;
                    shellService.AttVisible = true;
                    shellService.AltVisible = false;
                    shellService.NavVisible = false;
                    break;
                case 2:
                    shellService.MainVisible = false;
                    shellService.AttVisible = false;
                    shellService.AltVisible = true;
                    shellService.NavVisible = false;
                    break;
                case 3:
                    shellService.MainVisible = false;
                    shellService.AttVisible = false;
                    shellService.AltVisible = false;
                    shellService.NavVisible = true;
                    break;
                default:
                    shellService.MainVisible = false;
                    shellService.AttVisible = false;
                    shellService.AltVisible = false;
                    shellService.NavVisible = false;
                    break;
            }
        }
    }
}

