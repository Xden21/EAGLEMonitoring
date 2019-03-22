using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;

namespace EAGLEMonitoring.Application.Controllers
{
    [Export]
    public class StatusBarController
    {
        private readonly StatusBarViewModel statusBarViewModel;
        private readonly IGeneralService generalService;
        private readonly IShellService shellService;
        private readonly IDataUpdateService dataUpdateService;

        private DelegateCommand DisconnectCommand;
        private DelegateCommand ResetCommand;
        
        [ImportingConstructor]
        public StatusBarController(StatusBarViewModel statusBarViewModel,
            IGeneralService generalService,
            IShellService shellService,
            IDataUpdateService dataUpdateService)
        {
            this.statusBarViewModel = statusBarViewModel;
            generalService.PropertyChanged += GeneralService_PropertyChanged;
            this.generalService = generalService;
            this.shellService = shellService;
            this.dataUpdateService = dataUpdateService;
            DisconnectCommand = new DelegateCommand(Disconnect_Command);
            ResetCommand = new DelegateCommand(Reset_Command);
        }

        public void Initialize()
        {
            shellService.StatusBar = statusBarViewModel;
            statusBarViewModel.DisconnectCommand = DisconnectCommand;
            statusBarViewModel.Reset = ResetCommand;
        }

        private void Disconnect_Command()
        {
            if(generalService.Disconnect)
            {
                statusBarViewModel.ConnectButtonText = "Disconnect";
                generalService.Disconnect = false;
            }
            else
            {
                statusBarViewModel.ConnectButtonText = "Connect";
                generalService.Disconnect = true;
            }
        }

        private void Reset_Command()
        {
            dataUpdateService.TriggerDataResetEvent();
        }

        private void GeneralService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Connected")
            {
                if (generalService.Connected)
                {
                    statusBarViewModel.Connected = true;
                    statusBarViewModel.ConnectedText = "Connected";
                }
                else
                {
                    statusBarViewModel.Connected = false;
                    statusBarViewModel.ConnectedText = "Disconnected";
                }
            }
            else if(e.PropertyName == "FlightMode")
            {
                switch (generalService.FlightMode)
                {
                    case 0:
                        statusBarViewModel.FlightMode = "MANUAL MODE";
                        break;
                    case 1:
                        statusBarViewModel.FlightMode = "ALTITUDE HOLD";
                        break;
                    case 2:
                        statusBarViewModel.FlightMode = "NAVIGATION MODE";
                        break;
                    case 3:
                        statusBarViewModel.FlightMode = "ERROR";
                        break;
                    default:
                        statusBarViewModel.FlightMode = "UNKNOWN";
                        break;
                }
            }
        }
    }
}
