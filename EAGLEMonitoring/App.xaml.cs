using EAGLEMonitoring.Application.Controllers;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EAGLEMonitoring
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application, ISingleInstanceApp
    {
        ApplicationController applicationController;
        AggregateCatalog catalog;
        CompositionContainer container;


        private const string Unique = "EAGLE8Monitoring";


        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                try
                {
                    var application = new App();
                    application.Startup += application.OnStartup;
                    application.Exit += application.OnClose;
                    application.InitializeComponent();
                    application.Run();
                    // Allow single instance code to perform cleanup operations
                    SingleInstance<App>.Cleanup();
                }
                catch(Exception ex)
                {
                    Console.Write(ex.ToString());
                    return;
                }
            }

        }

        public void OnStartup(object sender, StartupEventArgs args)
        {
            catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ApplicationController).Assembly));
            container = new CompositionContainer(catalog);
            applicationController = container.GetExportedValue<ApplicationController>();
            applicationController.Initialize();
            applicationController.Run();
        }

        public void OnClose(object sender, ExitEventArgs arg)
        {
            applicationController.Close();
        }

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // ...
            return true;
        }
        #endregion
    }
}
