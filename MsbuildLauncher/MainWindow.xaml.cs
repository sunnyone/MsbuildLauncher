using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Execution;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Threading;
using MsbuildLauncher.ViewModel;

namespace MsbuildLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;

        }

        private MainViewModel mainViewModel;

        const ConsoleColor defaultConsoleColor = ConsoleColor.White;

        private void loadConfig()
        {
            this.Width = Properties.Settings.Default.WindowWidth;
            this.Height = Properties.Settings.Default.WindowHeight;

            if (Properties.Settings.Default.FilePathHistory != null)
            {
                foreach (string path in Properties.Settings.Default.FilePathHistory)
                {
                    this.comboBoxFilePath.Items.Add(path);
                }
            }
        }

        private void saveConfig()
        {
            Properties.Settings.Default.WindowWidth = this.ActualWidth;
            Properties.Settings.Default.WindowHeight = this.ActualHeight;

            StringCollection col = new StringCollection();
            foreach (object item in this.comboBoxFilePath.Items)
            {
                col.Add((string)item);
            }
            Properties.Settings.Default.FilePathHistory = col;

            Properties.Settings.Default.Save();
        }

        private void initializeLogText()
        {
            this.richTextBoxLog.Document.Blocks.Clear();
            this.richTextBoxLog.Document.Blocks.Add(new Paragraph());
        }

        string selectedXmlPath = null;
        private void loadBuildXmlFromSelectedXmlPath()
        {
            if (selectedXmlPath == null)
            {
                MessageBox.Show("MSBuild file is not selected.", Const.ApplicationName);
                return;
            }

            initializeLogText();
            this.mainViewModel.TargetNameList.Clear();

            List<string> targetNameList = new List<string>();
            try
            {
                var project = new Microsoft.Build.Evaluation.Project(selectedXmlPath);
                foreach (var kvp in project.Targets)
                {
                    targetNameList.Add(kvp.Key);
                }
                project.ProjectCollection.UnloadAllProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load msbuild file: \n" + ex.Message, Const.ApplicationName);

                return;
            }

            foreach (var name in targetNameList)
            {
                this.mainViewModel.TargetNameList.Add(name);
            }
        }

        private void loadXml(string xmlPath)
        {
            this.comboBoxFilePath.SelectionChanged -= comboBoxFilePath_SelectionChanged;

            // refresh all...
            LinkedList<string> pathList = new LinkedList<string>();
            foreach (object item in this.comboBoxFilePath.Items)
            {
                string path = (string)item;
                if (path == null)
                    continue;
                if (path == xmlPath)
                    continue;

                pathList.AddLast(path);
            }
            this.comboBoxFilePath.Items.Clear();

            pathList.AddFirst(xmlPath);

            int i = 0;
            foreach (string path in pathList)
            {
                if (i > Properties.Settings.Default.FilePathPreserveCount)
                    break;

                this.comboBoxFilePath.Items.Add(path);
                i++;
            }
            this.comboBoxFilePath.SelectedIndex = 0;

            selectedXmlPath = xmlPath;
            loadBuildXmlFromSelectedXmlPath();

            this.comboBoxFilePath.SelectionChanged += comboBoxFilePath_SelectionChanged;
        }

        // dirty way...
        private Brush convertColor(string color)
        {
            return (Brush)typeof(Brushes).GetProperty(color).GetValue(null, null);
        }

        public void AppendLogText(string text, string color)
        {
            Span span = new Span();
            span.Foreground = convertColor(color);
            span.Inlines.Add(text);

            Paragraph p = (Paragraph)this.richTextBoxLog.Document.Blocks.Last();
            p.Inlines.Add(span);

            this.richTextBoxLog.ScrollToEnd();
        }

        private System.Diagnostics.Process agentProcess = null;
        private void startBuild(string xmlPath, string targetName)
        {
            this.mainViewModel.IsBuildInProgress = true;

            System.Threading.Tasks.Task.Factory.StartNew(() => {
                try {
                    System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo();
                    si.FileName = "MsbuildLauncherAgent.exe";
                    si.Arguments = String.Format("\"{0}\" \"{1}\" \"{2}\"",
                        ((App)Application.Current).PipeName,
                        xmlPath, targetName);
                    si.CreateNoWindow = true;
                    si.UseShellExecute = false;
                    agentProcess = System.Diagnostics.Process.Start(si);
                    agentProcess.WaitForExit();
                    agentProcess = null;
                } catch (Exception ex) {
                    Dispatcher.Invoke(new Action<Exception>((ex1) => {
                        MessageBox.Show("Failed to build: \n" + ex1.Message);
                    }), ex);
                }

                this.mainViewModel.IsBuildInProgress = false;
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadConfig();

            string filenameAtArgs = ((App)Application.Current).FilenameAtArgs;
            if (filenameAtArgs != null)
            {
                loadXml(filenameAtArgs);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            saveConfig();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames.Length < 1)
            {
                return;
            }

            loadXml(filenames[0]);
        }

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            string fileName;

            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                fileName = dialog.FileName;
            }

            loadXml(fileName);
        }

        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            loadBuildXmlFromSelectedXmlPath();
        }

        private void buttonBuild_Click(object sender, RoutedEventArgs e)
        {
            initializeLogText();
            startBuild(selectedXmlPath, null);
        }

        private void buttonTarget_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            initializeLogText();
            startBuild(selectedXmlPath, (string)button.DataContext);
        }

        private void comboBoxFilePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedXmlPath = (string)this.comboBoxFilePath.SelectedItem;
            loadBuildXmlFromSelectedXmlPath();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            if (agentProcess != null) {
                agentProcess.Kill();
            }
        }
    }
}
