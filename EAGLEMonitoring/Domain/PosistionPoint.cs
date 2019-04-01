using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace EAGLEMonitoring.Domain
{
    public class PosistionPoint: Model
    {
        public PosistionPoint(double XPos, double YPos)
        {
            XPosition = XPos;
            YPosition = YPos;
        }

        private double xPosition;

        public double XPosition
        {
            get { return xPosition; }
            set { SetProperty(ref xPosition, value); }
        }

        private double yPosition;

        public double YPosition
        {
            get { return yPosition; }
            set { SetProperty(ref yPosition, value); }
        }
        
    }
}
