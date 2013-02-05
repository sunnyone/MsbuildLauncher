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

            mainViewModel.SupposeLogInitialized += new EventHandler(mainViewModel_SupposeLogInitialized);
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
                    this.mainViewModel.HistoryPathList.Add(path);
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

        // dirty way...
        private Brush convertColor(string color)
        {
            return (Brush)typeof(Brushes).GetProperty(color).GetValue(null, null);
        }

        private void initializeLogText()
        {
            this.richTextBoxLog.Document.Blocks.Clear();
            this.richTextBoxLog.Document.Blocks.Add(new Paragraph());
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

        void mainViewModel_SupposeLogInitialized(object sender, EventArgs e)
        {
            initializeLogText();
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

        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            if (this.mainViewModel.SelectedXmlPath == null)
            {
                MessageBox.Show("MSBuild file is not selected.", Const.ApplicationName);
                return;
            }

            this.mainViewModel.LoadBuildXml(this.mainViewModel.SelectedXmlPath);
        }

        private void buttonBuild_Click(object sender, RoutedEventArgs e)
        {
            this.mainViewModel.StartBuild(null);
        }

        private void buttonTarget_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
