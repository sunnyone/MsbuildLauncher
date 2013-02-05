using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MsbuildLauncher.Common;

namespace MsbuildLauncher {
    public class MsbuildLauncherApi : IMsbuildLauncherApi
    {
        public void WriteLog(string text, string color) {
            ((App)App.Current).MainViewModel.OutputLog(text, color);
        }
    }
}
