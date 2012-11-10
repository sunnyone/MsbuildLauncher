﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace MsbuildLauncher {
    public partial class App : Application {
        public string FilenameAtArgs { get; set; }

        private System.ServiceModel.ServiceHost serviceHost = null;
        public string PipeName { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 0)
            {
                this.FilenameAtArgs = e.Args[0];
            }

            serviceHost = new System.ServiceModel.ServiceHost(typeof(MsbuildLogWriter));

            string pipeName = "MsBuildLauncher-" + Guid.NewGuid().ToString();
            serviceHost.AddServiceEndpoint(typeof(IMsbuildLogWriter), new System.ServiceModel.NetNamedPipeBinding(),
                                           "net.pipe://localhost/" + pipeName);
            PipeName = pipeName;
            serviceHost.Open();
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
