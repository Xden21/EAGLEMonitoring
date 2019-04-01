using EAGLEMonitoring.Domain;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    [Export(typeof(IDataFetchService))]
    public class DataFetchService: IDataFetchService
    {
        #region NLog

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        private readonly ISettingsService settingsService;
        private readonly IDataUpdateService dataUpdateService;

        #region Constructor

        [ImportingConstructor]
        public DataFetchService(ISettingsService settingsService, IDataUpdateService dataUpdateService)
        {
            this.settingsService = settingsService;
            this.dataUpdateService = dataUpdateService;
            dataUpdateService.ResetDataEvent += ResetEventHandler;
            dataLock = new object();
            AllDataSets = new List<DataSet>();
        }

        /*
         * List of datasets that haven't been processed.
         */
        public List<DataSet> UnprocessedDataSets { get; set; }

        /*
         * List of all received datasets
         */
        public List<DataSet> AllDataSets { get; set; }

        private object dataLock;

        public object DataLock { get => dataLock; }
        #endregion

        #region Methods

        public void SaveLoggingToDisk()
        {
            try
            {

                if (AllDataSets.Count != 0)
                {
                    StringBuilder builder = new StringBuilder("Mode;time;yaw estimate; pitch estimate; roll estimate;yaw measured; pitch measured; roll measured;yaw desired; pitch desired; roll desired; x-axis; y-axis; z-axis; motor1;motor2;motor3;motor4; altitude estimate ; altitude measured; altitude desired ; x-pos estimate ; y-pos estimate ; x-pos measured ; y-pos measured ; x-pos target ; y-pos target ; x-vel estimate ; y-vel estimate\n");
                    lock (AllDataSets)
                    {
                        foreach (DataSet set in AllDataSets)
                        {
                            builder.Append(set.ToString());
                        }
                    }
                    string time = DateTime.Now.ToString("ddMMyy-HHmmss");

                    System.IO.File.WriteAllText(settingsService.SaveFolder + @"\AllLoggerData" + time + ".csv", builder.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Save Failed");
            }
        }
        
        public void ResetEventHandler(object sender, EventArgs args)
        {
            SaveLoggingToDisk();
            AllDataSets = new List<DataSet>();            
        }

        #endregion
    }
}
