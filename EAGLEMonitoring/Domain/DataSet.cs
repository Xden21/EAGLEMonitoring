using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace EAGLEMonitoring.Domain
{
    [Export]
    public class DataSet: Model
    {
        public DataSet()
        { }

        private bool validInfo;

        public bool ValidInfo
        {
            get { return validInfo; }
            set { SetProperty(ref validInfo, value); }
        }


        private int flightMode;

        public int FlightMode
        {
            get { return flightMode; }
            set { SetProperty(ref flightMode, value); }
        }

        private float timeStamp;

        public float TimeStamp
        {
            get { return timeStamp; }
            set { SetProperty(ref timeStamp, value); }
        }


        private EulerAngle angleEstimate;

        public EulerAngle AngleEstimate
        {
            get { return angleEstimate; }
            set { SetProperty(ref angleEstimate, value); }
        }

        private EulerAngle angleMeasured;   

        public EulerAngle AngleMeasured
        {
            get { return angleMeasured; }
            set { SetProperty(ref angleMeasured, value); }
        }

        private EulerAngle angleReference;

        public EulerAngle AngleReference
        {
            get { return angleReference; }
            set { SetProperty(ref angleReference, value); }
        }

        private AngularVelocitySet angularVelocity;

        public AngularVelocitySet AngularVelocity
        {
            get { return angularVelocity; }
            set { SetProperty(ref angularVelocity, value); }
        }

        private MotorSpeeds motorSpeeds;

        public MotorSpeeds MotorSpeeds
        {
            get { return motorSpeeds; }
            set { SetProperty(ref motorSpeeds, value); }
        }

        private double heightEstimate;

        public double HeightEstimate
        {
            get { return heightEstimate; }
            set { SetProperty(ref heightEstimate, value); }
        }

        private double heightMeasured;

        public double HeightMeasured
        {
            get { return heightMeasured; }
            set { SetProperty(ref heightMeasured, value); }
        }

        private double heightReference;

        public double HeightReference
        {
            get { return heightReference; }
            set { SetProperty(ref heightReference, value); }
        }

        private Position positionEstimate;

        public Position PositionEstimate
        {
            get { return positionEstimate; }
            set { SetProperty(ref positionEstimate, value); }
        }

        private Position positionMeasured;

        public Position PositionMeasured
        {
            get { return positionMeasured; }
            set { SetProperty(ref positionMeasured, value); }
        }

        private Position positionReference;

        public Position PositionReference
        {
            get { return positionReference; }
            set { SetProperty(ref positionReference, value); }
        }

        private VelocitySet velocitySet;

        public VelocitySet VelocitySet
        {
            get { return velocitySet; }
            set { SetProperty(ref velocitySet, value); }
        }


        public override string ToString()
        {
            return FlightMode.ToString() + ";" + TimeStamp.ToString() + ";" + AngleEstimate.ToString() + AngleMeasured.ToString() + AngleReference.ToString() + AngularVelocity.ToString() +
                HeightEstimate.ToString() + ";" + HeightMeasured.ToString() + ";" + HeightReference.ToString() + ";"
                + PositionEstimate.ToString() + ";" + PositionMeasured.ToString() + ";"+ PositionReference.ToString() + ";" + VelocitySet.ToString() + "\n";
        }
    }
}
