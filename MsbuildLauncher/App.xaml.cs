using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using MsbuildLauncher.Common;
using MsbuildLauncher.ViewModel;

namespace MsbuildLauncher {
    public partial class App : Application {
        public string FilenameAtArgs { get; set; }

        private System.ServiceModel.ServiceHost serviceHost = null;
        public string PipeName { get; set; }

        public MainViewModel MainViewModel { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 0)
            {
                this.FilenameAtArgs = e.Args[0];
            }

            serviceHost = new System.ServiceModel.ServiceHost(typeof(MsbuildLauncherApi));

            string pipeName = "MsBuildLauncher-" + Guid.NewGuid().ToString();
            serviceHost.AddServiceEndpoint(typeof(IMsbuildLauncherApi), new System.ServiceModel.NetNamedPipeBinding(),
                                           "net.pipe://localhost/" + pipeName);
            PipeName = pipeName;
            serviceHost.Open();

            this.MainViewModel = new MainViewModel();
            this.MainWindow = new MainWindow(this.MainViewModel);
            this.MainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            if (serviceHost != null) {
                try {
                    serviceHost.Close();
                } catch {

                }
            }
        }
    }
}
