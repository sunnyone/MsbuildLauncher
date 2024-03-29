﻿/*
 * Copyright (c) 2013 Yoichi Imai, All rights reserved.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
                try
                {
                    killProcessRecursive(agentProcess);
                }
                catch (Exception ex)
                {
                    var mainViewModel = ((App)App.Current).MainViewModel;
                    mainViewModel.NotifyError("Failed to kill a agent process: " + ex.ToString());
                }
            }
        }

        private void killProcessRecursive(Process process)
        {
            var childProcesses = getChildProcesses(process);
            foreach (var proc in childProcesses)
            {
                killProcessRecursive(proc);
            }
            process.Kill();
        }

        private List<Process> getChildProcesses(Process process)
        {
            var query = string.Format("SELECT * FROM Win32_Process Where ParentProcessID = {0}", process.Id);
            ManagementObjectSearcher mos = new ManagementObjectSearcher(query);

            var children = new List<Process>();
            foreach (ManagementObject mo in mos.Get())
            {
                var pid = Convert.ToInt32(mo["ProcessID"]);

                try
                {
                    var proc = Process.GetProcessById(pid);
                    children.Add(proc);
                }
                catch (ArgumentException)
                {
                    // Specified process may have already exited.
                }
            }

            return children;
        }
    }
}
