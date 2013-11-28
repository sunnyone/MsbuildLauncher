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
using System.ComponentModel;
using System.Diagnostics;
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
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Threading;
using MsbuildLauncher.ViewModel;

namespace MsbuildLauncher
{
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            this.DataContext = mainViewModel;
            this.mainViewModel = mainViewModel;

            mainViewModel.SupposeLogInitialized += new EventHandler(mainViewModel_SupposeLogInitialized);
            mainViewModel.SupposeLogOutput += new EventHandler<LogOutputEventArgs>(mainViewModel_SupposeLogOutput);
            mainViewModel.SupposeNotifyError += new EventHandler<LogOutputEventArgs>(mainViewModel_SupposeNotifyError);
        }


        const ConsoleColor defaultConsoleColor = ConsoleColor.White;

        private void loadConfig()
        {
            this.Width = Properties.Settings.Default.WindowWidth;
            this.Height = Properties.Settings.Default.WindowHeight;
            this.columnDefinitionTarget.Width = new GridLength(Properties.Settings.Default.TargetPaneWidth);
            this.columnDefinitionProperty.Width = new GridLength(Properties.Settings.Default.PropertyPaneWidth);

            if (Properties.Settings.Default.FilePathHistory != null)
            {
                foreach (string path in Properties.Settings.Default.FilePathHistory)
                {
                    this.mainViewModel.HistoryPathList.Add(path);
                }
            }
        }

        private void loadFontConfig()
        {
            this.mainViewModel.FontName = string.IsNullOrWhiteSpace(Properties.Settings.Default.FontName)
                                              ? null
                                              : Properties.Settings.Default.FontName;

            double d;
            string fontSize = Properties.Settings.Default.FontSize ?? "";
            this.mainViewModel.FontSize = Double.TryParse(fontSize, out d)
                                              ? (new FontSizeConverter().ConvertFrom(fontSize + "pt")).ToString()
                                              : null;
        }

        private void saveConfig()
        {
            Properties.Settings.Default.WindowWidth = this.ActualWidth;
            Properties.Settings.Default.WindowHeight = this.ActualHeight;
            Properties.Settings.Default.TargetPaneWidth = this.columnDefinitionTarget.ActualWidth;
            Properties.Settings.Default.PropertyPaneWidth = this.columnDefinitionProperty.ActualWidth;
            
            StringCollection col = new StringCollection();
            foreach (object item in this.comboBoxFilePath.Items)
            {
                if (item == null)
                    continue;

                col.Add((string)item);
            }
            Properties.Settings.Default.FilePathHistory = col;

            Properties.Settings.Default.Save();
        }

        // dirty way...
        private Brush convertColor(string color)
        {
            return (Brush)typeof(Brushes).GetProperty(color).GetValue(null, null);
        }

        private void loadBuildXmlWithSelectionChangedDisabled(string xmlPath)
        {
            this.comboBoxFilePath.SelectionChanged -= comboBoxFilePath_SelectionChanged;

            this.mainViewModel.LoadBuildXml(xmlPath);

            this.comboBoxFilePath.SelectionChanged += comboBoxFilePath_SelectionChanged;
        }

        private void outputLog(string text, string color)
        {
            Span span = new Span();
            span.Foreground = convertColor(color);
            span.Inlines.Add(text);

            Paragraph p = (Paragraph)this.richTextBoxLog.Document.Blocks.Last();
            p.Inlines.Add(span);

            this.richTextBoxLog.ScrollToEnd();
        }

        private void notifyErrorMessage(string text)
        {
            outputLog(text, "Red");
        }

        void mainViewModel_SupposeLogInitialized(object sender, EventArgs e)
        {
            this.richTextBoxLog.Document.Blocks.Clear();
            this.richTextBoxLog.Document.Blocks.Add(new Paragraph());
        }

        void mainViewModel_SupposeLogOutput(object sender, LogOutputEventArgs e)
        {
            outputLog(e.Text, e.Color);
        }

        void mainViewModel_SupposeNotifyError(object sender, LogOutputEventArgs e)
        {
            notifyErrorMessage(e.Text); // e.Color is not set
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadConfig();
            loadFontConfig();
            Properties.Settings.Default.PropertyChanged += (s1, e1) =>
                {
                    loadFontConfig();
                };
            
            string filenameAtArgs = ((App)Application.Current).FilenameAtArgs;
            if (filenameAtArgs != null)
            {
                loadBuildXmlWithSelectionChangedDisabled(filenameAtArgs);
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

            loadBuildXmlWithSelectionChangedDisabled(filenames[0]);
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

            loadBuildXmlWithSelectionChangedDisabled(fileName);
        }

        private bool validateFileSelected()
        {
            if (this.mainViewModel.SelectedXmlPath == null)
            {
                notifyErrorMessage("MSBuild file is not selected.");
                return false;
            }

            return true;
        }

        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            if (!validateFileSelected())
                return;

            loadBuildXmlWithSelectionChangedDisabled(this.mainViewModel.SelectedXmlPath);
        }

        private void buttonBuild_Click(object sender, RoutedEventArgs e)
        {
            if (!validateFileSelected())
                return;

            this.mainViewModel.StartBuild(null);
        }

        private void buttonTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!validateFileSelected())
                return;

            Button button = (Button)sender;
            TargetItemViewModel item = button.DataContext as TargetItemViewModel;
            if (item != null)
                this.mainViewModel.StartBuild(item.Name);
        }

        private void comboBoxFilePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string xmlPath = (string)this.comboBoxFilePath.SelectedItem;
            if (xmlPath == null)
                return;

            loadBuildXmlWithSelectionChangedDisabled(xmlPath);
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.mainViewModel.KillBuild();
        }

        private void buttonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (!validateFileSelected())
                return;

            string exePath = Properties.Settings.Default.EditorPath;
            exePath = Environment.ExpandEnvironmentVariables(exePath);

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exePath;
                psi.Arguments = string.Format("\"{0}\"", this.mainViewModel.SelectedXmlPath);
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                notifyErrorMessage(
                    string.Format("Failed to execute the editor ({0}): {1}", exePath, ex.Message));
            }
        }

        private void buttonSetting_Click(object sender, RoutedEventArgs e)
        {
            var window = new SettingWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        const double NameWidthPercent = 0.35;
        private void scrollViewerProperties_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int nameWidth = (int)(e.NewSize.Width * NameWidthPercent);

            this.propertiesControlCommon.NameWidth = nameWidth;
            this.propertiesControlFile.NameWidth = nameWidth;
        }

        private void ButtonAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo();
                psi.FileName = Environment.GetCommandLineArgs()[0];

                if (!string.IsNullOrEmpty(this.mainViewModel.SelectedXmlPath))
                {
                    psi.Arguments = string.Format("\"{0}\"", this.mainViewModel.SelectedXmlPath);
                }
                psi.Verb = "runas";
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                notifyErrorMessage(string.Format("Failed to re-run this program: {0}", ex.ToString()));
                return;
            }

            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            currentProcess.Kill();
        }
    }
}
