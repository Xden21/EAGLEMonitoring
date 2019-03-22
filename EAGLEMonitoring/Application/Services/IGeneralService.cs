using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    public interface IGeneralService
    {
        event PropertyChangedEventHandler PropertyChanged;
        bool Disconnect { get; set; }
        bool Connected { get; set; }
        int FlightMode { get; set; }
        
    }
}
