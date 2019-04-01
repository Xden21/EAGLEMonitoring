using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Domain;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Controllers
{
    [Export]
    public class NetworkAccessThreadController
    {
        #region NLog

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructor

        [ImportingConstructor]
        public NetworkAccessThreadController(IGeneralService generalService, ISettingsService settingsService, IDataFetchService dataFetchService)
        {
            this.generalService = generalService;
            this.settingsService = settingsService;
            this.dataFetchService = dataFetchService;
            dataFetchService.UnprocessedDataSets = new List<DataSet>();
            generalService.PropertyChanged += GeneralService_PropertyChanged;
            connecting = false;
#if OFFLINE
            time = 0;
            val = 0;
#endif
        }

        #endregion

        #region Vars & Props

#if OFFLINE
        double time;
        float val;
#endif

        private IGeneralService generalService;
        private ISettingsService settingsService;
        private IDataFetchService dataFetchService;
        private TcpClient connection;
        private NetworkStream stream;
        private Thread NetworkThread;

        private bool connecting;

        #endregion

        #region Methods

        public void TryConnect()
        {
            if (!connecting)
            {
                connecting = true;
                while (!generalService.Connected && !generalService.Disconnect)
                {
                    Connect();
                }
                Logger.Info("Connected");
                connecting = false;
            }
        }

        private bool Connect()
        {
#if !OFFLINE
            try
            {
                connection = new TcpClient(settingsService.InetAddress, settingsService.Port);
                connection.ReceiveTimeout = 100;
                connection.SendTimeout = 100;
                stream = connection.GetStream();
                generalService.Connected = true;
                if (NetworkThread != null && NetworkThread.ThreadState == ThreadState.Running)
                {
                    NetworkThread.Abort();
                }
                NetworkThread = new Thread(NetworkThreadRun);
                NetworkThread.Start();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Connecting Failed");
                Thread.Sleep(200);
                return false;
            }
#else

            generalService.Connected = true;
            if (NetworkThread != null && NetworkThread.ThreadState == ThreadState.Running)
            {
                NetworkThread.Abort();
            }
            NetworkThread = new Thread(NetworkThreadRun);
            NetworkThread.Start();
            return true;
#endif
        }

        public void Disconnect()
        {
            if (generalService.Connected)
            {
#if !OFFLINE
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes("end");
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception)
                { }
                connection.Close();
#endif
                generalService.Connected = false;
                generalService.FlightMode = -1;
                connection = null;
            }
        }
        private void GeneralService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Disconnect")
            {
                if (generalService.Disconnect)
                {
                    Disconnect();
                }
                else
                {
                    ThreadPool.QueueUserWorkItem((info) =>
                    {
                        TryConnect();
                    });
                }
            }
        }

        /**
         * Reads the network as fast as it gets data
         */
        private void NetworkThreadRun()
        {
            while (generalService.Connected)
            {
#if !OFFLINE
                // Send request
                try
                {
                    byte[] sendData = Encoding.UTF8.GetBytes("Start");
                    stream.Write(sendData, 0, sendData.Length);

                    byte[] data;
                    byte[] tempdata = new byte[400];
                    int amountRead = stream.Read(tempdata, 0, 400);
                    data = new byte[amountRead];
                    Array.Copy(tempdata, data, amountRead);

                    DataSet fetchedData = ParseMessage(data);
                    if (fetchedData.ValidInfo)
                    {
                        lock (dataFetchService.DataLock)
                        {
                            dataFetchService.UnprocessedDataSets.Add(fetchedData);
                        }
                    }
                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Network Error");
                    Thread.Sleep(5);
                    if (connection != null && !connection.Connected)
                    {
                        if (!Connect())
                            generalService.Connected = false;
                    }
                    continue;
                }
#else
                val = val + (float)(20.0 / 500.0);
                if (val > 20)
                    val = -20;

                time = time + 1.0 / 100.0;
                DataSet fetchedData = new DataSet
                {
                    ValidInfo = true,
                    FlightMode = 2,
                    TimeStamp = (float)time,
                    AngleEstimate = new EulerAngle { Roll = val, Pitch = val, Yaw = val },
                    AngleMeasured = new EulerAngle { Roll = -val, Pitch = -val, Yaw = -val },
                    AngleReference = new EulerAngle { Roll = val/2, Pitch = val/2, Yaw = val/2 },
                    AngularVelocity = new AngularVelocitySet { XVelocity = val, YVelocity = val, ZVelocity = val },
                    MotorSpeeds = new MotorSpeeds { MotorBL = val, MotorBR = val, MotorFL = val, MotorFR = val },
                    HeightEstimate = val,
                    HeightMeasured = val,
                    HeightReference = val,
                    PositionEstimate = new Position { XPosition = val/20, YPosition = val/40 },
                    PositionMeasured =  new Position { XPosition = val/10, YPosition = -val/20 } ,
                    PositionReference = new Position { XPosition = val/10, YPosition = val/20 },
                    VelocitySet = new VelocitySet { XVelocity=val,YVelocity=val}
                };
                lock (dataFetchService.DataLock)
                {
                    dataFetchService.UnprocessedDataSets.Add(fetchedData);
                }
                Thread.Sleep(10);
#endif
            }
        }

        private DataSet ParseMessage(byte[] data)
        {
            string decodedData = Encoding.UTF8.GetString(data);
            string[] dataArray = decodedData.Split(null);
            bool valid = true;
            int flightMode = 3;
            float timeStamp = -1000;
            EulerAngle AttEstimate = null, AttMeasured = null, AttReference = null;
            AngularVelocitySet angularVelocitySet = null;
            MotorSpeeds motorSpeeds = null;
            float altEstimate = 0, altMeasured = 0, altReference = 0;
            Position navEstimate = null, navMeasured = null, navReference = null;
            VelocitySet navVelocity = null;
            try
            {
                flightMode = (int)Math.Round(float.Parse(dataArray[0]));
                timeStamp = float.Parse(dataArray[1]);
                AttEstimate = new EulerAngle
                {
                    Yaw = float.Parse(dataArray[2]),
                    Pitch = float.Parse(dataArray[3]),
                    Roll = float.Parse(dataArray[4])
                };
                AttMeasured = new EulerAngle
                {
                    Yaw = float.Parse(dataArray[5]),
                    Pitch = float.Parse(dataArray[6]),
                    Roll = float.Parse(dataArray[7])
                };
                AttReference = new EulerAngle
                {
                    Yaw = float.Parse(dataArray[8]),
                    Pitch = float.Parse(dataArray[9]),
                    Roll = float.Parse(dataArray[10])
                };
                angularVelocitySet = new AngularVelocitySet
                {
                    XVelocity = float.Parse(dataArray[11]),
                    YVelocity = float.Parse(dataArray[12]),
                    ZVelocity = float.Parse(dataArray[13])
                };
                motorSpeeds = new MotorSpeeds
                {
                    MotorFL = float.Parse(dataArray[14]),
                    MotorFR = float.Parse(dataArray[15]),
                    MotorBL = float.Parse(dataArray[16]),
                    MotorBR = float.Parse(dataArray[17])
                };
                altEstimate = float.Parse(dataArray[18]);
                altMeasured = float.Parse(dataArray[19]);
                altReference = float.Parse(dataArray[20]);
                navEstimate = new Position
                {
                    XPosition = float.Parse(dataArray[21]),
                    YPosition = float.Parse(dataArray[22])
                };
                navMeasured = new Position
                {
                    XPosition = float.Parse(dataArray[23]),
                    YPosition = float.Parse(dataArray[24])
                };
                navReference = new Position
                {
                    XPosition = float.Parse(dataArray[25]),
                    YPosition = float.Parse(dataArray[26])
                };
                navVelocity = new VelocitySet
                {
                    XVelocity = float.Parse(dataArray[27]),
                    YVelocity = float.Parse(dataArray[28])
                };
            }
            catch (Exception)
            {
                valid = false;
            }
            return new DataSet
            {
                ValidInfo = valid,
                FlightMode = flightMode,
                TimeStamp = timeStamp,
                AngleEstimate = AttEstimate,
                AngleMeasured = AttMeasured,
                AngleReference = AttReference,
                AngularVelocity = angularVelocitySet,
                MotorSpeeds = motorSpeeds,
                HeightEstimate = altEstimate,
                HeightMeasured = altMeasured,
                HeightReference = altReference,
                PositionEstimate = navEstimate,
                PositionMeasured = navMeasured,
                PositionReference = navReference,
                VelocitySet = navVelocity
            };
        }

#endregion
    }
}
