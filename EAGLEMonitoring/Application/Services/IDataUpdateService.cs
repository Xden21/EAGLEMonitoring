using EAGLEMonitoring.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    public interface IDataUpdateService
    {
        event EventHandler<DataUpdateEventArgs> DataUpdateEvent;
        event EventHandler ResetDataEvent;

        void TriggerDataUpdateEvent(List<DataSet> dataSets);
        void TriggerDataResetEvent();
    }
}
