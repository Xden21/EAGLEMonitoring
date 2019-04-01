using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAGLEMonitoring.Application.Services
{
    public interface IGuiUpdateService
    {
        void StartGuiUpdate();
        void StopGuiUpdate();
    }
}
