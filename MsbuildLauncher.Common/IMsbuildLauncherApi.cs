using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MsbuildLauncher.Common {
    [ServiceContract(ConfigurationName = "MsbuildLauncher.IMsbuildLauncherApi")]
    public interface IMsbuildLauncherApi
    {
        [OperationContract]
        string GetXmlPath();

        [OperationContract]
        string GetTargetName();

        [OperationContract]
        List<KeyValuePair<string, string>> GetProperties();

        [OperationContract]
        void WriteLog(string text, string color);
    }
}
