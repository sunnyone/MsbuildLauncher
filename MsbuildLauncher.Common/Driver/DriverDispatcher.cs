using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsbuildLauncher.Common.Driver
{
    public class DriverDispatcher
    {
        public static IDriver CreateDriverByFilename(string filePath)
        {
            string extension = (System.IO.Path.GetExtension(filePath) ?? "").ToLowerInvariant();
            if (extension == ".ps1")
                return new PowerShellDriver();

            return new MSBuildDriver();
        }
    }
}
