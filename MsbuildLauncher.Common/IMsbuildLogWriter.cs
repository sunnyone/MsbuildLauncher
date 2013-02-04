using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MsbuildLauncher.Common {
    [ServiceContract(ConfigurationName = "MsbuildLauncher.IMsbuildLogWriter")]
    public interface IMsbuildLogWriter {
        [OperationContract]
        void WriteLog(string text, string color);
    }
}
