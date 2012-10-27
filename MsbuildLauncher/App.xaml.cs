using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace MsbuildLauncher {
    public partial class App : Application {
        public string FilenameAtArgs { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 0)
            {
                this.FilenameAtArgs = e.Args[0];
            }
        }
    }
}
