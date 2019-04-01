using EAGLEMonitoring.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace EAGLEMonitoring.Application.Services
{
    [Export(typeof(IDataUpdateService))]
    public class DataUpdateService : IDataUpdateService
    {
        private readonly ISettingsService settingsService;
        private readonly IGeneralService generalService;


        [ImportingConstructor]
        public DataUpdateService(ISettingsService settingsService,
            IGeneralService generalService)
        {
            this.settingsService = settingsService;
            this.generalService = generalService;
        }

        public event EventHandler<DataUpdateEventArgs> DataUpdateEvent;
        public event EventHandler ResetDataEvent;

        public void TriggerDataUpdateEvent(List<DataSet> dataSets)
        {
            DataUpdateEvent?.Invoke(this, new DataUpdateEventArgs(dataSets));
        }

        public void TriggerDataResetEvent()
        {
            ResetDataEvent?.Invoke(this, EventArgs.Empty);
        }


    }
}

public class DataUpdateEventArgs : EventArgs
{
    public DataUpdateEventArgs(List<DataSet> dataSets)
    {
        DataSets = dataSets;
    }

    public List<DataSet> DataSets { get; set; }
}
    
