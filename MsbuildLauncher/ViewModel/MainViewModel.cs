/*
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
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MsbuildLauncher.ViewModel
{
    public class LogOutputEventArgs : EventArgs
    {
        public string Color { get; set; }
        public string Text { get; set; }
    }

    public class MainViewModel : ViewModelBase
    {
        private bool isBuildInProgress;
        public bool IsBuildInProgress
        {
            get { return isBuildInProgress; }
            set { isBuildInProgress = value; OnPropertyChanged("IsBuildInProgress"); }
        }

        private ObservableCollection<string> targetNameList = new ObservableCollection<string>();
        public ObservableCollection<string> TargetNameList
        {
            get { return targetNameList; }
        }

        private ObservableCollection<string> historyPathList = new ObservableCollection<string>();
        public ObservableCollection<string> HistoryPathList
        {
            get { return historyPathList; }
        }

        private string selectedXmlPath;

        public string SelectedXmlPath
        {
            get { return selectedXmlPath; }
            set { selectedXmlPath = value; OnPropertyChanged("SelectedXmlPath"); }
        }

        // FIXME: use action
        public event EventHandler SupposeLogInitialized;
        public event EventHandler<LogOutputEventArgs> SupposeLogOutput;

        private void updateHistory(string xmlPath)
        {
            if (this.HistoryPathList.Contains(xmlPath))
            {
                this.HistoryPathList.Remove(xmlPath);
            }
            this.HistoryPathList.Insert(0, xmlPath);

            for (int i = Properties.Settings.Default.FilePathPreserveCount;
                 i < this.HistoryPathList.Count;
                 i++)
            {
                this.HistoryPathList.RemoveAt(i);
            }
        }

        public void LoadBuildXml(string xmlPath)
        {
            if (SupposeLogInitialized != null)
            {
                SupposeLogInitialized(this, new EventArgs());
            }

            this.TargetNameList.Clear();

            updateHistory(xmlPath);

            List<string> targetNameList = new List<string>();
            try
            {
                var project = new Microsoft.Build.Evaluation.Project(xmlPath);
                foreach (var kvp in project.Targets)
                {
                    targetNameList.Add(kvp.Key);
                }
                project.ProjectCollection.UnloadAllProjects();
            }
            catch (Exception ex)
            {
                // TODO: do not to show a message box directly
                MessageBox.Show("Failed to load msbuild file: \n" + ex.Message, Const.ApplicationName);

                return;
            }

            foreach (var name in targetNameList)
            {
                this.TargetNameList.Add(name);
            }

            this.SelectedXmlPath = xmlPath;
        }

        private BuildContext buildContext = null;
        public void StartBuild(string targetName)
        {
            this.IsBuildInProgress = true;

            if (this.SupposeLogInitialized != null)
            {
                this.SupposeLogInitialized(this, new EventArgs());
            }

            buildContext = new BuildContext();
            buildContext.TargetName = targetName;
            buildContext.XmlPath = this.SelectedXmlPath;
            buildContext.PipeName = ((App) Application.Current).PipeName;

            var task = buildContext.BuildAsync();

            task.ContinueWith((t) =>
            {
                Application.Current.Dispatcher.Invoke(new Action<Exception>((ex1) =>
                {
                    MessageBox.Show("Failed to build: \n" + ex1.Message);
                }), t.Exception.InnerException);
            }, TaskContinuationOptions.OnlyOnFaulted);
            
            task.ContinueWith((t) =>
                {
                    buildContext = null;
                    this.IsBuildInProgress = false;
                });

        }

        public void KillBuild()
        {
            var ctx = buildContext;
            if (ctx != null)
            {
                ctx.Kill();
            }
        }

        public void OutputLog(string text, string color)
        {
            if (SupposeLogOutput != null)
            {
                SupposeLogOutput(this, new LogOutputEventArgs() {Color = color, Text = text});
            }
        }
    }
}
