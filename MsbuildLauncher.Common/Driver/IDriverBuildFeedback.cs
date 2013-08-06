using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MsbuildLauncher.Common.Driver
{
    public interface IDriverBuildFeedback
    {
        void WriteLog(string text, ConsoleColor color);
    }
}
