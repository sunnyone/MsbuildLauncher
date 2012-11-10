using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MsbuildLauncher {
    public class MsbuildLogWriter : IMsbuildLogWriter {
        public void WriteLog(string text, string color) {
            ((MainWindow)App.Current.MainWindow).AppendLogText(text, color);
        }
    }
}
