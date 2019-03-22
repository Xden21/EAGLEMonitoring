using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    [Export(typeof(ISettingsService))]
    public class SettingsService: ISettingsService
    {
        private string inetAddress;
        private int port;


        public string InetAddress
        {
            get { return inetAddress; }
            set { inetAddress = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        private int dataAmount;

        public int DataAmount
        {
            get { return dataAmount; }
            set { dataAmount = value; }
        }

        private float timeFrame;

        public float TimeFrame
        {
            get { return timeFrame; }
            set { timeFrame = value; }
        }

        private int fps;

        public int FPS
        {
            get { return fps; }
            set { fps = value; }
        }

        private string saveFolder;

        public string SaveFolder
        {
            get { return saveFolder; }
            set { saveFolder = value; }
        }


        [ImportingConstructor]
        public SettingsService()
        {
            ReadSettings();
        }

        private void ReadSettings()
        {
            InetAddress = Properties.Settings.Default.InetAddress;
            Port = Properties.Settings.Default.Port;
            DataAmount = Properties.Settings.Default.DataAmount;
            TimeFrame = Properties.Settings.Default.TimeFrame;
            fps = Properties.Settings.Default.FPS;
            saveFolder = Properties.Settings.Default.LogSaveFolder;
        }
    }
}
