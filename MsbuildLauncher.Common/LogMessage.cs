using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MsbuildLauncher.Common
{
    [DataContract]
    public class LogMessage
    {
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Color { get; set; }
    }
}
