using EAGLEMonitoring.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    public interface IDataFetchService
    {
        List<DataSet> UnprocessedDataSets { get; set; }
        
        List<DataSet> AllDataSets { get; set; }

        object DataLock { get; }

        void SaveLoggingToDisk();
    }
}
