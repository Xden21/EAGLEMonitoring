using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.ViewModels;
using EAGLEMonitoring.Domain;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;

namespace EAGLEMonitoring.Application.Controllers
{
    [Export]
    public class AttitudeMonitoringController
    {
        private readonly AttitudeMonitoringViewModel attitudeMonitoringViewModel;
        private readonly IShellService shellService;
        private readonly IDataUpdateService dataUpdateService;
        private readonly ISettingsService settingsService;
        private readonly IGuiUpdateService guiUpdateService;

        private bool rotLast;
        private bool velLast;
        private bool motorLast;

        private bool rotUpdating;
        private bool velUpdating;
        private bool motUpdating;

        List<LogPoint> attRollEstBuffer;
        List<LogPoint> attRollMeasBuffer;
        List<LogPoint> attRollRefBuffer;

        List<LogPoint> attPitchEstBuffer;
        List<LogPoint> attPitchMeasBuffer;
        List<LogPoint> attPitchRefBuffer;

        List<LogPoint> attYawEstBuffer;
        List<LogPoint> attYawMeasBuffer;
        List<LogPoint> attYawRefBuffer;
        private bool attRotBuffersfull;

        List<LogPoint> attVelXBuffer;
        List<LogPoint> attVelYBuffer;
        List<LogPoint> attVelZBuffer;
        private bool attVelBuffersFull;

        List<LogPoint> motorFLBuffer;
        List<LogPoint> motorFRBuffer;
        List<LogPoint> motorBLBuffer;
        List<LogPoint> motorBRBuffer;
        private bool attMotBuffersFull;

        private DelegateCommand TabChangeCommand;

        private int frameRotCount;
        private int frameVelCount;
        private int frameMotCount;

        [ImportingConstructor]
        public AttitudeMonitoringController(AttitudeMonitoringViewModel attitudeMonitoringViewModel,
            IShellService shellService,
            IDataUpdateService dataUpdateService,
            ISettingsService settingsService,
            IGuiUpdateService guiUpdateService)
        {
            this.attitudeMonitoringViewModel = attitudeMonitoringViewModel;
            this.shellService = shellService;
            this.dataUpdateService = dataUpdateService;
            this.settingsService = settingsService;
            this.guiUpdateService = guiUpdateService;
            dataUpdateService.DataUpdateEvent += DataUpdateEventHandler;
            shellService.PropertyChanged += ShellService_PropertyChanged;

            TabChangeCommand = new DelegateCommand(TabChange_Command);

            attRollEstBuffer = new List<LogPoint>();
            attRollMeasBuffer = new List<LogPoint>();
            attRollRefBuffer = new List<LogPoint>();
            attPitchEstBuffer = new List<LogPoint>();
            attPitchMeasBuffer = new List<LogPoint>();
            attPitchRefBuffer = new List<LogPoint>();
            attYawEstBuffer = new List<LogPoint>();
            attYawMeasBuffer = new List<LogPoint>();
            attYawRefBuffer = new List<LogPoint>();
            attRotBuffersfull = false;

            attVelXBuffer = new List<LogPoint>();
            attVelYBuffer = new List<LogPoint>();
            attVelZBuffer = new List<LogPoint>();
            attVelBuffersFull = false;

            motorFLBuffer = new List<LogPoint>();
            motorFRBuffer = new List<LogPoint>();
            motorBLBuffer = new List<LogPoint>();
            motorBRBuffer = new List<LogPoint>();
            attMotBuffersFull = false;

            rotLast = true;
            rotUpdating = false;
            velUpdating = false;
            motUpdating = false;

            frameRotCount = 0;
            frameVelCount = 0;
            frameMotCount = 0;
        }

        private void ShellService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "AttVisible")
            {
                if(shellService.AttVisible)
                {
                    attitudeMonitoringViewModel.RotVis = rotLast;
                    attitudeMonitoringViewModel.VelVis = velLast;
                    attitudeMonitoringViewModel.MotorVis = motorLast;
                }
                else
                {
                    rotLast = attitudeMonitoringViewModel.RotVis;
                    velLast = attitudeMonitoringViewModel.VelVis;
                    motorLast = attitudeMonitoringViewModel.MotorVis;
                    attitudeMonitoringViewModel.RotVis = false;
                    attitudeMonitoringViewModel.VelVis = false;
                    attitudeMonitoringViewModel.MotorVis = false;
                }
            }
        }

        public void Initialize()
        {
            shellService.AttitudeMonitoring = attitudeMonitoringViewModel;
            attitudeMonitoringViewModel.TabChange = TabChangeCommand;
        }
            
        private void DataUpdateEventHandler(object sender, DataUpdateEventArgs args)
        {
            if (args.DataSets == null || args.DataSets.Count == 0)
                return; // No updating to be done
            if (attitudeMonitoringViewModel.RotVis)
            {
                if (rotUpdating || frameRotCount > 0)
                {
                    frameRotCount--;
                    UpdateRotBuffer(args.DataSets);                    
                }
                else
                {
                    rotUpdating = true;
                    frameRotCount = settingsService.FPS / 20;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateAttitudeRot(args.DataSets);
                        rotUpdating = false;
                    });
                }
            }
            else
            {
                UpdateRotBuffer(args.DataSets);
            }
            if (attitudeMonitoringViewModel.VelVis)
            {
                if (velUpdating || frameVelCount > 0)
                {
                    frameVelCount--;
                    UpdateVelBuffer(args.DataSets);
                }
                else
                {
                    velUpdating = true;
                    frameVelCount = settingsService.FPS / 20;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateAttitudeVel(args.DataSets);
                        velUpdating = false;
                    });
                }
            }
            else
            {
                UpdateVelBuffer(args.DataSets);
            }
            if (attitudeMonitoringViewModel.MotorVis)
            {
                if (motUpdating || frameMotCount > 0)
                {
                    frameMotCount--;
                    UpdateMotorBuffer(args.DataSets);
                }
                else
                {
                    motUpdating = true;
                    frameMotCount = settingsService.FPS / 20;
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        UpdateAttitudeMotors(args.DataSets);
                        motUpdating = false;
                    });
                }
            }
            else
            {
                UpdateMotorBuffer(args.DataSets);
            }
        }
                       
        private void UpdateAttitudeRot(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (attRollEstBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                return;
            }

            if (addBuffer)
            {
                UpdateRotBuffer(dataSets);
                if (attRotBuffersfull)
                {
                    attitudeMonitoringViewModel.RollEstPoints.Clear();
                    attitudeMonitoringViewModel.RollMeasPoints.Clear();
                    attitudeMonitoringViewModel.RollRefPoints.Clear();

                    attitudeMonitoringViewModel.PitchEstPoints.Clear();
                    attitudeMonitoringViewModel.PitchMeasPoints.Clear();
                    attitudeMonitoringViewModel.PitchRefPoints.Clear();

                    attitudeMonitoringViewModel.YawEstPoints.Clear();
                    attitudeMonitoringViewModel.YawMeasPoints.Clear();
                    attitudeMonitoringViewModel.YawRefPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (attitudeMonitoringViewModel.RollEstPoints.Count > 0 && (attRollEstBuffer.Last().TimeVal - attitudeMonitoringViewModel.RollEstPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        attitudeMonitoringViewModel.RollEstPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.RollMeasPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.RollRefPoints.RemoveAt(0);

                        attitudeMonitoringViewModel.PitchEstPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.PitchMeasPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.PitchRefPoints.RemoveAt(0);

                        attitudeMonitoringViewModel.YawEstPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.YawMeasPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.YawRefPoints.RemoveAt(0);
                    }
                    attitudeMonitoringViewModel.RollEstPoints.AddRange(attRollEstBuffer);
                    attitudeMonitoringViewModel.RollMeasPoints.AddRange(attRollMeasBuffer);
                    attitudeMonitoringViewModel.RollRefPoints.AddRange(attRollRefBuffer);

                    attitudeMonitoringViewModel.PitchEstPoints.AddRange(attPitchEstBuffer);
                    attitudeMonitoringViewModel.PitchMeasPoints.AddRange(attPitchMeasBuffer);
                    attitudeMonitoringViewModel.PitchRefPoints.AddRange(attPitchRefBuffer);

                    attitudeMonitoringViewModel.YawEstPoints.AddRange(attYawEstBuffer);
                    attitudeMonitoringViewModel.YawMeasPoints.AddRange(attYawMeasBuffer);
                    attitudeMonitoringViewModel.YawRefPoints.AddRange(attYawRefBuffer);

                    attRollEstBuffer.Clear();
                    attRollMeasBuffer.Clear();
                    attRollRefBuffer.Clear();

                    attPitchEstBuffer.Clear();
                    attPitchMeasBuffer.Clear();
                    attPitchRefBuffer.Clear();

                    attYawEstBuffer.Clear();
                    attYawMeasBuffer.Clear();
                    attYawRefBuffer.Clear();
                    attRotBuffersfull = false;
                }
            }
            else
            {
                int index = 0; // First index which stays
                while (index < attitudeMonitoringViewModel.RollEstPoints.Count && (dataSets.Last().TimeStamp - attitudeMonitoringViewModel.RollEstPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    attitudeMonitoringViewModel.RollEstPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.RollMeasPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.RollRefPoints.RemoveAt(0);

                    attitudeMonitoringViewModel.PitchEstPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.PitchMeasPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.PitchRefPoints.RemoveAt(0);

                    attitudeMonitoringViewModel.YawEstPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.YawMeasPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.YawRefPoints.RemoveAt(0);
                }
                List<LogPoint> newRollEstPoints = new List<LogPoint>();
                List<LogPoint> newRollMeasPoints = new List<LogPoint>();
                List<LogPoint> newRollRefPoints = new List<LogPoint>();

                List<LogPoint> newPitchEstPoints = new List<LogPoint>();
                List<LogPoint> newPitchMeasPoints = new List<LogPoint>();
                List<LogPoint> newPitchRefPoints = new List<LogPoint>();

                List<LogPoint> newYawEstPoints = new List<LogPoint>();
                List<LogPoint> newYawMeasPoints = new List<LogPoint>();
                List<LogPoint> newYawRefPoints = new List<LogPoint>();

                foreach (DataSet dataSet in dataSets)
                {
                    newRollEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Roll));
                    newRollMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Roll));
                    newRollRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Roll));

                    newPitchEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Pitch));
                    newPitchMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Pitch));
                    newPitchRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Pitch));

                    newYawEstPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Yaw));
                    newYawMeasPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Yaw));
                    newYawRefPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Yaw));
                }

                attitudeMonitoringViewModel.RollEstPoints.AddRange(newRollEstPoints);
                attitudeMonitoringViewModel.RollMeasPoints.AddRange(newRollMeasPoints);
                attitudeMonitoringViewModel.RollRefPoints.AddRange(newRollRefPoints);

                attitudeMonitoringViewModel.PitchEstPoints.AddRange(newPitchEstPoints);
                attitudeMonitoringViewModel.PitchMeasPoints.AddRange(newPitchMeasPoints);
                attitudeMonitoringViewModel.PitchRefPoints.AddRange(newPitchRefPoints);

                attitudeMonitoringViewModel.YawEstPoints.AddRange(newYawEstPoints);
                attitudeMonitoringViewModel.YawMeasPoints.AddRange(newYawMeasPoints);
                attitudeMonitoringViewModel.YawRefPoints.AddRange(newYawRefPoints);
            }
        }

        private void UpdateAttitudeVel(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (attVelXBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                return;
            }

            if (addBuffer)
            {
                UpdateVelBuffer(dataSets);
                if (attVelBuffersFull)
                {
                    // Buffer will delete all current values
                    attitudeMonitoringViewModel.VelXPoints.Clear();
                    attitudeMonitoringViewModel.VelYPoints.Clear();
                    attitudeMonitoringViewModel.VelZPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (attitudeMonitoringViewModel.VelXPoints.Count > 0 && (attVelXBuffer.Last().TimeVal - attitudeMonitoringViewModel.VelXPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        attitudeMonitoringViewModel.VelXPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.VelYPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.VelZPoints.RemoveAt(0);
                    }
                }
                attitudeMonitoringViewModel.VelXPoints.AddRange(attVelXBuffer);
                attitudeMonitoringViewModel.VelYPoints.AddRange(attVelYBuffer);
                attitudeMonitoringViewModel.VelZPoints.AddRange(attVelZBuffer);
                attVelXBuffer.Clear();
                attVelYBuffer.Clear();
                attVelZBuffer.Clear();
                attVelBuffersFull = false;
            }
            else
            {
                // Search which values to remove
                int index = 0; // First index which stays
                while (index < attitudeMonitoringViewModel.VelXPoints.Count && (dataSets.Last().TimeStamp - attitudeMonitoringViewModel.VelXPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    attitudeMonitoringViewModel.VelXPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.VelXPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.VelXPoints.RemoveAt(0);
                }
                List<LogPoint> newXPoints = new List<LogPoint>();
                List<LogPoint> newYPoints = new List<LogPoint>();
                List<LogPoint> newZPoints = new List<LogPoint>();

                foreach (DataSet dataSet in dataSets)
                {
                    newXPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.XVelocity));
                    newYPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.YVelocity));
                    newZPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.ZVelocity));
                }

                attitudeMonitoringViewModel.VelXPoints.AddRange(newXPoints);
                attitudeMonitoringViewModel.VelYPoints.AddRange(newYPoints);
                attitudeMonitoringViewModel.VelZPoints.AddRange(newZPoints);
            }
        }

        private void UpdateAttitudeMotors(List<DataSet> dataSets)
        {
            bool addBuffer;
            if (motorFLBuffer.Count > 0)
                addBuffer = true;
            else
                addBuffer = false;

            if (dataSets == null || dataSets.Count == 0)
            {
                return;
            }

            if (addBuffer)
            {
                UpdateMotorBuffer(dataSets);
                if (attMotBuffersFull)
                {
                    // Buffer will delete all current values
                    attitudeMonitoringViewModel.MotorFLPoints.Clear();
                    attitudeMonitoringViewModel.MotorFRPoints.Clear();
                    attitudeMonitoringViewModel.MotorBLPoints.Clear();
                    attitudeMonitoringViewModel.MotorBRPoints.Clear();
                }
                else
                {
                    int index = 0;
                    while (attitudeMonitoringViewModel.MotorFLPoints.Count > 0 && (motorFLBuffer.Last().TimeVal - attitudeMonitoringViewModel.MotorFLPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                        index++;

                    for (int i = 0; i < index; i++)
                    {
                        attitudeMonitoringViewModel.MotorFLPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.MotorFRPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.MotorBLPoints.RemoveAt(0);
                        attitudeMonitoringViewModel.MotorBRPoints.RemoveAt(0);
                    }
                }
                attitudeMonitoringViewModel.MotorFLPoints.AddRange(motorFLBuffer);
                attitudeMonitoringViewModel.MotorFRPoints.AddRange(motorFRBuffer);
                attitudeMonitoringViewModel.MotorBLPoints.AddRange(motorBLBuffer);
                attitudeMonitoringViewModel.MotorBRPoints.AddRange(motorBRBuffer);
                motorFLBuffer.Clear();
                motorFRBuffer.Clear();
                motorBLBuffer.Clear();
                motorBRBuffer.Clear();
                attMotBuffersFull = false;
            }
            else
            {
                // Search which values to remove
                int index = 0; // First index which stays
                while (index < attitudeMonitoringViewModel.MotorFLPoints.Count && (dataSets.Last().TimeStamp - attitudeMonitoringViewModel.MotorFLPoints[index].TimeVal) > settingsService.BigPlotTimeFrame)
                    index++;

                for (int i = 0; i < index; i++)
                {
                    attitudeMonitoringViewModel.MotorFLPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.MotorFRPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.MotorBLPoints.RemoveAt(0);
                    attitudeMonitoringViewModel.MotorBRPoints.RemoveAt(0);
                }
                List<LogPoint> newFLPoints = new List<LogPoint>();
                List<LogPoint> newFRPoints = new List<LogPoint>();
                List<LogPoint> newBLPoints = new List<LogPoint>();
                List<LogPoint> newBRPoints = new List<LogPoint>();

                foreach (DataSet dataSet in dataSets)
                {
                    newFLPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorFL));
                    newFRPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorFR));
                    newBLPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorBL));
                    newBRPoints.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorBR));
                }

                attitudeMonitoringViewModel.MotorFLPoints.AddRange(newFLPoints);
                attitudeMonitoringViewModel.MotorFRPoints.AddRange(newFRPoints);
                attitudeMonitoringViewModel.MotorBLPoints.AddRange(newBLPoints);
                attitudeMonitoringViewModel.MotorBRPoints.AddRange(newBRPoints);
            }
        }

        private void UpdateRotBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)attRollEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (attRollEstBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.BigPlotTimeFrame)
                {
                    attRotBuffersfull = true;
                    attRollEstBuffer.RemoveAt(0);
                    attPitchEstBuffer.RemoveAt(0);
                    attYawEstBuffer.RemoveAt(0);
                    attRollMeasBuffer.RemoveAt(0);
                    attPitchMeasBuffer.RemoveAt(0);
                    attYawMeasBuffer.RemoveAt(0);
                    attRollRefBuffer.RemoveAt(0);
                    attPitchRefBuffer.RemoveAt(0);
                    attYawRefBuffer.RemoveAt(0);

                    firstTime = (float)attRollEstBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;
                }

                attRollEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Roll));
                attRollMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Roll));
                attRollRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Roll));

                attPitchEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Pitch));
                attPitchMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Pitch));
                attPitchRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Pitch));

                attYawEstBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleEstimate.Yaw));
                attYawMeasBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleMeasured.Yaw));
                attYawRefBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngleReference.Yaw));
            }
        }

        private void UpdateVelBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)attVelXBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (attVelXBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.BigPlotTimeFrame)
                {
                    attVelBuffersFull = true;
                    attVelXBuffer.RemoveAt(0);
                    attVelYBuffer.RemoveAt(0);
                    attVelZBuffer.RemoveAt(0);

                    firstTime = (float)attVelXBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;
                }
                attVelXBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.XVelocity));
                attVelYBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.YVelocity));
                attVelZBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.AngularVelocity.ZVelocity));
            }
        }

        private void UpdateMotorBuffer(List<DataSet> dataSets)
        {
            if (dataSets == null || dataSets.Count == 0)
                return; // No updating to be done

            float firstTime = (float)motorFLBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;

            foreach (DataSet dataSet in dataSets)
            {
                if (firstTime != 0 && (motorFLBuffer.Count > 0) && (dataSet.TimeStamp - firstTime) > settingsService.BigPlotTimeFrame)
                {
                    attMotBuffersFull = true;
                    motorFLBuffer.RemoveAt(0);
                    motorFRBuffer.RemoveAt(0);
                    motorBLBuffer.RemoveAt(0);
                    motorBRBuffer.RemoveAt(0);

                    firstTime = (float)motorFLBuffer.DefaultIfEmpty(new LogPoint(0, 0)).First().TimeVal;
                }
                motorFLBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorFL));
                motorFRBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorFR));
                motorBLBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorBL));
                motorBRBuffer.Add(new LogPoint(dataSet.TimeStamp, dataSet.MotorSpeeds.MotorBR));
            }
        }

        private void TabChange_Command(object param)
        {
            guiUpdateService.StopGuiUpdate();
            Thread.Sleep(50);
            int tabnum = int.Parse((string)param);

            switch (tabnum)
            {
                case 0:
                    attitudeMonitoringViewModel.RotVis = true;
                    attitudeMonitoringViewModel.VelVis = false;
                    attitudeMonitoringViewModel.MotorVis = false;
                    break;
                case 1:
                    attitudeMonitoringViewModel.RotVis = false;
                    attitudeMonitoringViewModel.VelVis = true;
                    attitudeMonitoringViewModel.MotorVis = false;
                    break;
                case 2:
                    attitudeMonitoringViewModel.RotVis = false;
                    attitudeMonitoringViewModel.VelVis = false;
                    attitudeMonitoringViewModel.MotorVis = true;
                    break;
                default:
                    break;
            }
            Thread.Sleep(50);
            guiUpdateService.StartGuiUpdate();
        }
        
        public void Reset()
        {
            attitudeMonitoringViewModel.RollEstPoints.Clear();
            attitudeMonitoringViewModel.PitchEstPoints.Clear();
            attitudeMonitoringViewModel.YawEstPoints.Clear();
            attitudeMonitoringViewModel.RollMeasPoints.Clear();
            attitudeMonitoringViewModel.PitchMeasPoints.Clear();
            attitudeMonitoringViewModel.YawMeasPoints.Clear();
            attitudeMonitoringViewModel.RollRefPoints.Clear();
            attitudeMonitoringViewModel.PitchRefPoints.Clear();
            attitudeMonitoringViewModel.YawRefPoints.Clear();
            attitudeMonitoringViewModel.VelXPoints.Clear();
            attitudeMonitoringViewModel.VelYPoints.Clear();
            attitudeMonitoringViewModel.VelZPoints.Clear();
            attitudeMonitoringViewModel.MotorFLPoints.Clear();
            attitudeMonitoringViewModel.MotorFRPoints.Clear();
            attitudeMonitoringViewModel.MotorBLPoints.Clear();
            attitudeMonitoringViewModel.MotorBRPoints.Clear();

            attRollEstBuffer = new List<LogPoint>();
            attRollMeasBuffer = new List<LogPoint>();
            attRollRefBuffer = new List<LogPoint>();
            attPitchEstBuffer = new List<LogPoint>();
            attPitchMeasBuffer = new List<LogPoint>();
            attPitchRefBuffer = new List<LogPoint>();
            attYawEstBuffer = new List<LogPoint>();
            attYawMeasBuffer = new List<LogPoint>();
            attYawRefBuffer = new List<LogPoint>();
            attRotBuffersfull = false;

            attVelXBuffer = new List<LogPoint>();
            attVelYBuffer = new List<LogPoint>();
            attVelZBuffer = new List<LogPoint>();
            attVelBuffersFull = false;

            motorFLBuffer = new List<LogPoint>();
            motorFRBuffer = new List<LogPoint>();
            motorBLBuffer = new List<LogPoint>();
            motorBRBuffer = new List<LogPoint>();
            attMotBuffersFull = false;

            rotLast = true;
            rotUpdating = false;
            velUpdating = false;
            motUpdating = false;

            frameRotCount = 0;
            frameVelCount = 0;
            frameMotCount = 0;
        }
    }
}
