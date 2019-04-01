using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    public interface ISettingsService
    {
        string InetAddress { get; set; }
        int Port { get; set; }
        int DataAmount { get; set; }
        float TimeFrame { get; set; }
        float BigPlotTimeFrame { get; set; }
        int FPS { get; set; }
        string SaveFolder { get; set; }        
        float TrackingTimeFrame { get; set; }
    }
}
