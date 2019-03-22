using EAGLEMonitoring.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace EAGLEMonitoring.Application.Services
{
    [Export(typeof(IShellService))]
    public class ShellService: Model, IShellService
    {
        [ImportingConstructor]
        public ShellService()
        {
            mainVisible = true;
            attVisible = false;
            altVisible = false;
            navVisible = false;
        }

        private MainMonitoringViewModel mainMonitoring;

        public MainMonitoringViewModel MainMonitoring
        {
            get { return mainMonitoring; }
            set { SetProperty(ref mainMonitoring, value); }
        }

        private StatusBarViewModel statusBar;

        public StatusBarViewModel StatusBar
        {
            get { return statusBar; }
            set { SetProperty(ref statusBar, value); }
        }

        private AttitudeMonitoringViewModel attitudeMonitoring;

        public AttitudeMonitoringViewModel AttitudeMonitoring
        {
            get { return attitudeMonitoring; }
            set { SetProperty(ref attitudeMonitoring, value); }
        }

        private AltitudeMonitoringViewModel altitudeMonitoring;

        public AltitudeMonitoringViewModel AltitudeMonitoring
        {
            get { return altitudeMonitoring; }
            set { SetProperty(ref altitudeMonitoring, value); }
        }

        private NavigationMonitoringViewModel navigationMonitoring;

        public NavigationMonitoringViewModel NavigationMonitoring
        {
            get { return navigationMonitoring; }
            set { SetProperty(ref navigationMonitoring, value); }
        }

        private bool mainVisible;

        public bool MainVisible
        {
            get { return mainVisible; }
            set { SetProperty(ref mainVisible, value); }
        }

        private bool attVisible;

        public bool AttVisible
        {
            get { return attVisible; }
            set { SetProperty(ref attVisible, value); }
        }

        private bool altVisible;

        public bool AltVisible
        {
            get { return altVisible; }
            set { SetProperty(ref altVisible, value); }
        }

        private bool navVisible;

        public bool NavVisible
        {
            get { return navVisible; }
            set { SetProperty(ref navVisible, value); }
        }
    }
}
