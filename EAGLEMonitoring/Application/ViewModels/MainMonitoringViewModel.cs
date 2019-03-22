using EAGLEMonitoring.Application.Views;
using LiveCharts;
using LiveCharts.Wpf;
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
    public class MainMonitoringViewModel : ViewModel<IMainMonitoringView>
    {
        #region Constructor

        [ImportingConstructor]
        public MainMonitoringViewModel(IMainMonitoringView view) : base(view)
        {
            Formatter = value => value.ToString("0.###");
            AttEnable = true;
            AltMode = true;
            NavMode = true;
        }

        #endregion

        #region Vars & Props

        private SeriesCollection attRollSeries;

        public SeriesCollection AttRollSeries
        {
            get { return attRollSeries; }
            set { SetProperty(ref attRollSeries, value); }
        }

        private SeriesCollection attPitchSeries;

        public SeriesCollection AttPitchSeries
        {
            get { return attPitchSeries; }
            set { SetProperty(ref attPitchSeries, value); }
        }

        private SeriesCollection attYawSeries;

        public SeriesCollection AttYawSeries
        {
            get { return attYawSeries; }
            set { SetProperty(ref attYawSeries, value); }
        }

        private SeriesCollection altSeries;

        public SeriesCollection AltSeries
        {
            get { return altSeries; }
            set { SetProperty(ref altSeries, value); }
        }

        private SeriesCollection navXSeries;

        public SeriesCollection NavXSeries
        {
            get { return navXSeries; }
            set { SetProperty(ref navXSeries, value); }
        }

        private SeriesCollection navYSeries;

        public SeriesCollection NavYSeries
        {
            get { return navYSeries; }
            set { SetProperty(ref navYSeries, value); }
        }

        private bool altMode;

        public bool AltMode
        {
            get { return altMode; }
            set { SetProperty(ref altMode, value); }
        }

        private bool navMode;

        public bool NavMode
        {
            get { return navMode; }
            set { SetProperty(ref navMode, value); }
        }

        private bool altOverride;

        public bool AltOverride
        {
            get { return altOverride; }
            set { SetProperty(ref altOverride, value); }
        }

        private bool navOverride;

        public bool NavOverride
        {
            get { return navOverride; }
            set { SetProperty(ref navOverride, value); }
        }

        private ICommand toggleVisibilityCommand;

        public ICommand ToggleVisibilityCommand
        {
            get { return toggleVisibilityCommand; }
            set { SetProperty(ref toggleVisibilityCommand, value); }
        }

        private LineSeries selectedSeries;

        public LineSeries SelectedSeries
        {
            get { return selectedSeries; }
            set { SetProperty(ref selectedSeries, value); }
        }

        private ICommand chartUpdated;

        public ICommand ChartUpdated
        {
            get { return chartUpdated; }
            set { SetProperty(ref chartUpdated, value); }
        }

        private bool attEnable;

        public bool AttEnable
        {
            get { return attEnable; }
            set { SetProperty(ref attEnable, value); }
        }

        public Func<double, string> Formatter { get; set; }

        #endregion

        #region Methods

        #endregion
    }
}
