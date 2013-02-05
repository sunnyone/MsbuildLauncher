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
            var mainViewModel = ((App)App.Current).MainViewModel;
            mainViewModel.OutputLog(text, color);
        }

        public string GetXmlPath()
        {
            var mainViewModel = ((App)App.Current).MainViewModel;
            
            if (mainViewModel.BuildContext == null)
                return null;

            return mainViewModel.BuildContext.XmlPath;
        }

        public string GetTargetName()
        {
            var mainViewModel = ((App)App.Current).MainViewModel;

            if (mainViewModel.BuildContext == null)
                return null;

            return mainViewModel.BuildContext.TargetName;
        }
    }
}
