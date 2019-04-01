using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.ViewModels;
using EAGLEMonitoring.Domain;
using LiveCharts.Configurations;
using NLog;
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
        #region NLog

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        private readonly ShellViewModel shellViewModel;
        private readonly IDataFetchService dataFetchService;
        private readonly IDataUpdateService dataUpdateService;
        private readonly IGeneralService generalService;
        private readonly ISettingsService settingsService;
        private readonly IShellService shellService;
        private readonly IGuiUpdateService guiUpdateService;
        private readonly MainMonitoringController mainMonitoringController;
        private readonly NetworkAccessThreadController networkAccessThreadController;
        private readonly StatusBarController statusBarController;
        private readonly AttitudeMonitoringController attitudeMonitoringController;
        private readonly AltitudeMonitoringController altitudeMonitoringController;
        private readonly NavigationMonitoringController navigationMonitoringController;


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
            IGuiUpdateService guiUpdateService,
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
            this.guiUpdateService = guiUpdateService;
            dataUpdateService.ResetDataEvent += Reset;
            ChangeTabCommand = new DelegateCommand(ChangeTab_Command);
        }

        public void Initialize()
        {
            Logger.Info("Initializing");
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
            Logger.Info("Start");
            shellViewModel.Show();
            ThreadPool.QueueUserWorkItem((info) =>
            {
                networkAccessThreadController.TryConnect();
            });
            guiUpdateService.StartGuiUpdate();
        }

        public void Close()
        {
            Logger.Info("Finishing");
            networkAccessThreadController.Disconnect();
            guiUpdateService.StopGuiUpdate();
            dataFetchService.SaveLoggingToDisk();
        }

        private void Reset(object sender, EventArgs args)
        {
            Logger.Info("Reset GUI");
            guiUpdateService.StopGuiUpdate();

            //Reset
            networkAccessThreadController.Disconnect();
            mainMonitoringController.Reset();
            altitudeMonitoringController.Reset();
            attitudeMonitoringController.Reset();
            navigationMonitoringController.Reset();

            ThreadPool.QueueUserWorkItem((info) =>
            {
                networkAccessThreadController.TryConnect();
            });
            guiUpdateService.StartGuiUpdate();
        }

       
      
        private void SetupCharting()
        {
            var mapper = Mappers.Xy<LogPoint>()
                .X(value => value.TimeVal)
                .Y(value => value.Value);
            LiveCharts.Charting.For<LogPoint>(mapper);

            var mapperTracking = Mappers.Xy<PosistionPoint>()
                .X(value => value.XPosition)
                .Y(value => value.YPosition);
            LiveCharts.Charting.For<PosistionPoint>(mapperTracking);
        }

        private void ChangeTab_Command(object param)
        {
            guiUpdateService.StopGuiUpdate();
            Thread.Sleep(50);
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
            Thread.Sleep(50);
            guiUpdateService.StartGuiUpdate();
        }
    }
}

