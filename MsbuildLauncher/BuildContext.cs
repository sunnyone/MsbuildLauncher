using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MsbuildLauncher
{
    public class BuildContext
    {
        public string XmlPath { get; set; }
        public string TargetName { get; set; }
        public string PipeName { get; set; }
        public List<KeyValuePair<string, string>> PropertyList { get; set; }
 
        private System.Diagnostics.Process agentProcess = null;

        public System.Threading.Tasks.Task BuildAsync()
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo();
                si.FileName = "MsbuildLauncher.Agent.exe";
                si.Arguments = String.Format("\"{0}\"", PipeName);
                si.CreateNoWindow = true;
                si.UseShellExecute = false;
                agentProcess = System.Diagnostics.Process.Start(si);
                agentProcess.WaitForExit();
                agentProcess = null;
            });
        }

        public void Kill()
        {
            if (agentProcess != null)
            {
                agentProcess.Kill();
            }
        }
    }
}
