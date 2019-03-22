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
    public class VelocitySet: Model
    {
        public VelocitySet()
        {

        }

        private float xVelocity;

        public float XVelocity
        {
            get { return xVelocity; }
            set { SetProperty(ref xVelocity, value); }
        }

        private float yVelocity;

        public float YVelocity
        {
            get { return yVelocity; }
            set { SetProperty(ref yVelocity, value); }
        }

        public override string ToString()
        {
            return XVelocity.ToString() + ";" + YVelocity.ToString() + ";";
        }
    }
}
