using EAGLEMonitoring.Application.Services;
using EAGLEMonitoring.Application.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows.Input;

namespace EAGLEMonitoring.Application.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        #region Constructor

        [ImportingConstructor]
        public ShellViewModel(IShellView view, IShellService shellService) : base(view)
        {
            this.shellService = shellService;
        }

        #endregion

        #region Vars & Properties

        private IShellService shellService;

        public IShellService ShellService
        {
            get { return shellService; }
            set { SetProperty(ref shellService, value); }
        }
                
        private ICommand changeTab;

        public ICommand ChangeTab
        {
            get { return changeTab; }
            set { SetProperty(ref changeTab, value); }
        }

        #endregion

        #region Methods

        public void Show()
        {
            ViewCore.Show();
        }

        #endregion

    }
}
