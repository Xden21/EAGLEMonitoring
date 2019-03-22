using EAGLEMonitoring.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    public interface IShellService
    {
        event PropertyChangedEventHandler PropertyChanged;
        MainMonitoringViewModel MainMonitoring { get; set; }
        StatusBarViewModel StatusBar { get; set; }
        AttitudeMonitoringViewModel AttitudeMonitoring { get; set; }
        AltitudeMonitoringViewModel AltitudeMonitoring { get; set; }
        NavigationMonitoringViewModel NavigationMonitoring { get; set; }

        bool MainVisible { get; set; }
        bool AttVisible { get; set; }
        bool AltVisible { get; set; }
        bool NavVisible { get; set; }
    }
}
