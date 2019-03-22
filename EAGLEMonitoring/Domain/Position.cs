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
    public class Position: Model
    {
        public Position()
        {

        }

        private float xPosistion;

        public float XPosition
        {
            get { return xPosistion; }
            set { SetProperty(ref xPosistion, value); }
        }

        private float yPosition;

        public float YPosition
        {
            get { return yPosition; }
            set { SetProperty(ref yPosition, value); }
        }

        public override string ToString()
        {
            return XPosition.ToString() + ";" + YPosition.ToString() + ";";
        }
    }
}
