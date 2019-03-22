using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;

namespace EAGLEMonitoring.Application.Views
{
    public interface IShellView: IView
    {
        void Show();
    }
}
