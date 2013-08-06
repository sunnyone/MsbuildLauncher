using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsbuildLauncher.Common.Driver
{
    public interface IDriver : IDisposable
    {
        void Open(string filePath);
        string[] GetTargetNames();
        void GetProperties(string[] commonPropertyNames, out PropertyItem[] commonProperties, out PropertyItem[] fileProperties);
        void Build(string targetName, KeyValuePair<string, string>[] properties, IDriverBuildFeedback feedback);
    }
}
