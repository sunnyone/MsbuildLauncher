using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MsbuildLauncher
{
    [DataContract]
    public class PropertyItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string[] Items { get; set; }

        // for WPF
        public string Value { get; set; }
    }
}
