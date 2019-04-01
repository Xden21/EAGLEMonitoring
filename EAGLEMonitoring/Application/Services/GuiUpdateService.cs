using EAGLEMonitoring.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EAGLEMonitoring.Application.Services
{
    [Export(typeof(IGuiUpdateService))]
    public class GuiUpdateService : IGuiUpdateService
    {

        private readonly ISettingsService settingsService;
        private readonly IGeneralService generalService;
        private readonly IDataFetchService dataFetchService;
        private readonly IDataUpdateService dataUpdateService;

        private System.Timers.Timer guiUpdateTimer;

        [ImportingConstructor]
        public GuiUpdateService(ISettingsService settingsService,
            IGeneralService generalService,
            IDataFetchService dataFetchService,
            IDataUpdateService dataUpdateService)
        {
            this.settingsService = settingsService;
            this.generalService = generalService;
            this.dataFetchService = dataFetchService;
            this.dataUpdateService = dataUpdateService;
        }

        public void StartGuiUpdate()
        {
            guiUpdateTimer = new System.Timers.Timer()
            {
                Interval = settingsService.FPS,
                AutoReset = true
            };
            guiUpdateTimer.Elapsed += UpdateData;
            guiUpdateTimer.Start();
        }

        public void StopGuiUpdate()
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
                    lock (dataFetchService.AllDataSets)
                    {
                        dataFetchService.AllDataSets.AddRange(newDataSets);
                    }

                    // Trigger update
                    dataUpdateService.TriggerDataUpdateEvent(newDataSets);
                }
            }
        }
    }
}
