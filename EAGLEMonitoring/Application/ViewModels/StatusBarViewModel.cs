using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows.Input;

namespace EAGLEMonitoring.Application.ViewModels
{
    [Export]
    public class StatusBarViewModel : ViewModel<IStatusBarView>
    {
        public IGeneralService GeneralService => generalService;

        [ImportingConstructor]
        public StatusBarViewModel(IStatusBarView view, IGeneralService generalService) : base(view)
        {
            this.generalService = generalService;            
            ConnectedText = "Disconnected";
            ConnectButtonText = "Disconnect";
            FlightMode = "UNKNOWN";
        }

        private readonly IGeneralService generalService;

        private bool connected;

        public bool Connected
        {
            get { return connected; }
            set { SetProperty(ref connected, value); }
        }

        private string connectedText;

        public string ConnectedText
        {
            get { return connectedText; }
            set { SetProperty(ref connectedText, value); }
        }

        private string connectButtonText;

        public string ConnectButtonText
        {
            get { return connectButtonText; }
            set { SetProperty(ref connectButtonText, value); }
        }


        private ICommand disconnectCommand;

        public ICommand DisconnectCommand
        {
            get { return disconnectCommand; }
            set { SetProperty(ref disconnectCommand, value); }
        }

        private ICommand reset;

        public ICommand Reset
        {
            get { return reset; }
            set { SetProperty(ref reset, value); }
        }


        private string flightMode;

        public string FlightMode
        {
            get { return flightMode; }
            set { SetProperty(ref flightMode, value); }
        }

    }
}
