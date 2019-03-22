using EAGLEMonitoring.Application.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EAGLEMonitoring.Presentation.Views
{
    /// <summary>
    /// Interaction logic for StatusBarView.xaml
    /// </summary>
    [Export(typeof(IStatusBarView))]
    public partial class StatusBarView : UserControl, IStatusBarView
    {
        public StatusBarView()
        {
            InitializeComponent();
        }
    }
}
