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
            serviceHost.AddServiceEndpoint(typeof(IMsbuildLauncherApi),
                AgentCommunicationUtil.CreateBinding(), AgentCommunicationUtil.CreateEndpointUri(pipeName));
            PipeName = pipeName;
            serviceHost.Open();

            this.MainViewModel = new MainViewModel();

            var mainWindow = new MainWindow(this.MainViewModel);

            try
            {
                string jsonPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    Const.CommonPropertiesFilename);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to load " + Const.CommonPropertiesFilename + ": " + ex.ToString());
            }

            this.MainWindow = mainWindow;
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
