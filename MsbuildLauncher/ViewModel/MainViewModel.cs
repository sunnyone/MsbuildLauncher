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
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using MsbuildLauncher.Common;
using MsbuildLauncher.Common.Driver;

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

        private ObservableCollection<PropertyItem> commonPropertyList = new ObservableCollection<PropertyItem>();
        public ObservableCollection<PropertyItem> CommonPropertyList
        {
            get { return commonPropertyList; }
        }

        private ObservableCollection<PropertyItem> filePropertyList = new ObservableCollection<PropertyItem>();
        public ObservableCollection<PropertyItem> FilePropertyList
        {
            get { return filePropertyList; }
        }


        private string selectedXmlPath;

        public string SelectedXmlPath
        {
            get { return selectedXmlPath; }
            set { selectedXmlPath = value; OnPropertyChanged("SelectedXmlPath"); OnPropertyChanged("WindowTitle"); }
        }
        
        [DllImport("shell32.dll")]
        public static extern bool IsUserAnAdmin();

        public string WindowTitle
        {
            get
            {
                var versionFull = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string version = string.Format("{0}.{1}.{2}", versionFull.Major, versionFull.Minor, versionFull.Build);
                string filename = null;
                if (!string.IsNullOrEmpty(this.SelectedXmlPath))
                {
                    filename = System.IO.Path.GetFileName(this.SelectedXmlPath);
                }


                string isAdminString = IsUserAnAdmin() ? " (Administrator)" : "";
                string appVersion = string.Format("{0} version {1}{2}", ThisAssembly.AssemblyProduct, version, isAdminString);

                if (filename != null)
                    return string.Format("{0} - {1}", filename, appVersion);
                else
                    return appVersion;
            }
        }

        public bool IsAdmin
        {
            get { return IsUserAnAdmin(); }
        }

        private string fontName;
        public string FontName
        {
            get { return fontName == null ? SystemFonts.MessageFontFamily.ToString() : fontName; }
            set { fontName = value; OnPropertyChanged("FontName"); }
        }

        private string fontSize;
        public string FontSize
        {
            get { return fontSize == null ? SystemFonts.MessageFontSize.ToString() : fontSize; }
            set { fontSize = value; OnPropertyChanged("FontSize"); }
        }

        // TODO: use action
        public event EventHandler SupposeLogInitialized;
        public event EventHandler<LogOutputEventArgs> SupposeLogOutput;
        public event EventHandler<LogOutputEventArgs> SupposeNotifyError;

        private void supposeNotifyError(string text)
        {
            if (SupposeNotifyError != null)
            {
                SupposeNotifyError(this, new LogOutputEventArgs() { Text = text });
            }
        }

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

        private void clearTargetNameList()
        {
            this.TargetNameList.Clear();
        }

        private void clearCommonProperties()
        {
            this.CommonPropertyList.Clear();
        }

        private void clearFileProperties()
        {
            this.FilePropertyList.Clear();
        }

        public void LoadBuildXml(string xmlPath)
        {
            if (SupposeLogInitialized != null)
            {
                SupposeLogInitialized(this, new EventArgs());
            }

            clearTargetNameList();
            clearCommonProperties();
            clearFileProperties();

            updateHistory(xmlPath);

            try
            {
                using (var driver = new MSBuildDriver())
                {
                    driver.Open(xmlPath);

                    foreach (var targetName in driver.GetTargetNames())
                    {
                        this.TargetNameList.Add(targetName);
                    }

                    PropertyItem[] commonProperties, fileProperties;
                    
                    string[] commonPropertyNames = Properties.Settings.Default.CommonProperties.Split(new char[] { ';' });
                    driver.GetProperties(commonPropertyNames, out commonProperties, out fileProperties);

                    foreach (var propItem in commonProperties)
                        this.CommonPropertyList.Add(propItem);

                    foreach (var propItem in fileProperties)
                        this.FilePropertyList.Add(propItem);
                }
            }
            catch (Exception ex)
            {
                supposeNotifyError("Failed to load msbuild file: \n" + ex.Message);
                return;
            }

            this.SelectedXmlPath = xmlPath;
        }

        public BuildContext BuildContext { get; set; }
        
        public void StartBuild(string targetName)
        {
            this.IsBuildInProgress = true;

            if (this.SupposeLogInitialized != null)
            {
                this.SupposeLogInitialized(this, new EventArgs());
            }

            BuildContext = new BuildContext();
            BuildContext.TargetName = targetName;
            BuildContext.XmlPath = this.SelectedXmlPath;
            BuildContext.PipeName = ((App) Application.Current).PipeName;

            var list = new List<KeyValuePair<string, string>>();
            foreach (var propItem in this.CommonPropertyList.Union(this.FilePropertyList))
            {
                if (!propItem.IsChanged)
                    continue;

                list.Add(new KeyValuePair<string, string>(propItem.Name, propItem.Value));
            }
            BuildContext.PropertyList = list;


            var task = BuildContext.BuildAsync();

            task.ContinueWith((t) =>
            {
                Application.Current.Dispatcher.Invoke(new Action<Exception>((ex1) =>
                {
                    supposeNotifyError("Failed to build: \n" + ex1.Message);
                }), t.Exception.InnerException);
            }, TaskContinuationOptions.OnlyOnFaulted);
            
            task.ContinueWith((t) =>
                {
                    BuildContext = null;
                    this.IsBuildInProgress = false;
                });

        }

        public void KillBuild()
        {
            var ctx = BuildContext;
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
