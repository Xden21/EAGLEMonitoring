using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace EAGLEMonitoring.Application.Services
{
    [Export(typeof(IGeneralService))]
    public class GeneralService : Model, IGeneralService
    {
        public GeneralService()
        {
            disconnect = false;
            connected = false;
            flightMode = -1;
        }

        private bool connected;

        public bool Connected
        {
            get { return connected; }
            set {SetProperty(ref connected , value); }
        }

        private int flightMode;

        public int FlightMode
        {
            get { return flightMode; }
            set { SetProperty(ref flightMode, value); }
        }


        private bool disconnect;

        public bool Disconnect
        {
            get { return disconnect; }
            set { SetProperty(ref disconnect, value); }
        }
    }
}
