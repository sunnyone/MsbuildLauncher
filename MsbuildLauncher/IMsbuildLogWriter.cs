﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MsbuildLauncher {
    // FIXME: remove the copy
    [ServiceContract]
    public interface IMsbuildLogWriter {
        [OperationContract]
        void WriteLog(string text, string color);
    }
}
