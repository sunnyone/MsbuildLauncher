using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MsbuildLauncher.Common;

namespace MsbuildLauncher {
    [ServiceBehavior(Namespace="", Name="IMsbuildLogWriter")]
    public class MsbuildLogWriter : MsbuildLauncher.Common.IMsbuildLogWriter
    {
        public void WriteLog(string text, string color) {
            ((MainWindow)App.Current.MainWindow).AppendLogText(text, color);
        }
    }
}
