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
        private MainViewModel mainViewModel;
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();

            this.DataContext = mainViewModel;
            this.mainViewModel = mainViewModel;

            mainViewModel.SupposeLogInitialized += new EventHandler(mainViewModel_SupposeLogInitialized);
            mainViewModel.SupposeLogOutput += new EventHandler<LogOutputEventArgs>(mainViewModel_SupposeLogOutput);
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

        private void saveConfig()
        {
            Properties.Settings.Default.WindowWidth = this.ActualWidth;
            Properties.Settings.Default.WindowHeight = this.ActualHeight;
            Properties.Settings.Default.TargetPaneWidth = this.columnDefinitionTarget.ActualWidth;
            Properties.Settings.Default.PropertyPaneWidth = this.columnDefinitionProperty.ActualWidth;

            StringCollection col = new StringCollection();
            foreach (object item in this.comboBoxFilePath.Items)
            {
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

        void mainViewModel_SupposeLogInitialized(object sender, EventArgs e)
        {
            this.richTextBoxLog.Document.Blocks.Clear();
            this.richTextBoxLog.Document.Blocks.Add(new Paragraph());
        }

        void mainViewModel_SupposeLogOutput(object sender, LogOutputEventArgs e)
        {
            Span span = new Span();
            span.Foreground = convertColor(e.Color);
            span.Inlines.Add(e.Text);

            Paragraph p = (Paragraph)this.richTextBoxLog.Document.Blocks.Last();
            p.Inlines.Add(span);

            this.richTextBoxLog.ScrollToEnd();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadConfig();

            string filenameAtArgs = ((App)Application.Current).FilenameAtArgs;
            if (filenameAtArgs != null)
            {
                this.mainViewModel.LoadBuildXml(filenameAtArgs);
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

            this.mainViewModel.LoadBuildXml(filenames[0]);
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

            this.mainViewModel.LoadBuildXml(fileName);
        }

        private bool validateFileSelected()
        {
            if (this.mainViewModel.SelectedXmlPath == null)
            {
                MessageBox.Show("MSBuild file is not selected.", Const.ApplicationName);
                return false;
            }

            return true;
        }

        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            if (!validateFileSelected())
                return;

            this.mainViewModel.LoadBuildXml(this.mainViewModel.SelectedXmlPath);
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
            this.mainViewModel.StartBuild((string)button.DataContext);
        }

        private void comboBoxFilePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.comboBoxFilePath.SelectionChanged -= comboBoxFilePath_SelectionChanged;

            this.mainViewModel.LoadBuildXml((string)this.comboBoxFilePath.SelectedItem);

            this.comboBoxFilePath.SelectedIndex = 0;
            this.comboBoxFilePath.SelectionChanged += comboBoxFilePath_SelectionChanged;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.mainViewModel.KillBuild();
        }

        private void buttonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (!validateFileSelected())
                return;

            string exePath = FileAssocUtil.GetAssocExecutable(".xml");
            if (exePath == null)
            {
                MessageBox.Show("Failed to get the program for .xml");
                return;
            }

            System.Diagnostics.ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exePath;
            psi.Arguments = string.Format("\"{0}\"", this.mainViewModel.SelectedXmlPath);
            System.Diagnostics.Process.Start(psi);
        }
    }
}
