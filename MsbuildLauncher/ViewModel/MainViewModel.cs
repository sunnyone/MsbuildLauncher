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
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Text.RegularExpressions;

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
            set { selectedXmlPath = value; OnPropertyChanged("SelectedXmlPath"); }
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


        private void setPropertyInfoToProperyItem(Microsoft.Build.Evaluation.Project project, Microsoft.Build.Evaluation.ProjectProperty prop, PropertyItem propItem)
        {
            propItem.Name = prop.Name;
            propItem.DefaultValue = prop.UnevaluatedValue;
            propItem.Value = prop.UnevaluatedValue;
            propItem.IsChanged = false;
            propItem.IsEnabled = true;

            var q = project.ConditionedProperties.Where(x => x.Key == prop.Name);
            if (q.Any())
            {
                var condProp = q.First();
                propItem.Items = new string[] { prop.UnevaluatedValue }.Union(condProp.Value).ToArray();
            }
        }

        private void loadFileProperties(Microsoft.Build.Evaluation.Project project)
        {
            foreach (var prop in project.Properties) {
                if (prop.IsEnvironmentProperty ||
                    prop.IsGlobalProperty ||
                    prop.IsImported ||
                    prop.IsReservedProperty)
                    continue;

                if (this.CommonPropertyList.Any(x => x.Name == prop.Name))
                    continue;

                var propItem = new PropertyItem();
                setPropertyInfoToProperyItem(project, prop, propItem);

                this.FilePropertyList.Add(propItem);
            }
        }

        private void loadCommonProperties(Microsoft.Build.Evaluation.Project project)
        {
            string[] targets = Properties.Settings.Default.CommonProperties.Split(new char[] { ';' });

            foreach (var propName in targets)
            {
                bool isEnabled = false;

                var propItem = new PropertyItem();
                propItem.Name = propName;

                foreach (var prop in project.Properties)
                {
                    if (propName == prop.Name)
                    {
                        setPropertyInfoToProperyItem(project, prop, propItem);

                        isEnabled = true;
                    }
                }

                propItem.IsEnabled = isEnabled;

                this.CommonPropertyList.Add(propItem);
            }
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
                var project = new Microsoft.Build.Evaluation.Project(xmlPath);
                
                // load target names
                foreach (var kvp in project.Targets)
                {
                    this.TargetNameList.Add(kvp.Key);
                }

                loadCommonProperties(project);
                loadFileProperties(project);
                
                project.ProjectCollection.UnloadAllProjects();
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
