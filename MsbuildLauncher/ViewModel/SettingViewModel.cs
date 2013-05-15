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

namespace MsbuildLauncher.ViewModel
{
    class SettingViewModel : ViewModelBase
    {
        public List<string> FontNameList { get; set; }

        public SettingViewModel()
        {
            var ifc = new System.Drawing.Text.InstalledFontCollection();

            FontNameList = ifc.Families.Select(x => x.Name).ToList();
        }

        private string editorPath;
        public string EditorPath
        {
            get { return editorPath; }
            set { editorPath = value; OnPropertyChanged("EditorPath"); }
        }

        private string commonProperties;
        public string CommonProperties
        {
            get { return commonProperties; }
            set { commonProperties = value; OnPropertyChanged("CommonProperties"); }
        }

        private string fontName;
        public string FontName
        {
            get { return fontName; }
            set { fontName = value; OnPropertyChanged("FontName"); }
        }

        private string fontSize;
        public string FontSize
        {
            get { return fontSize; }
            set { fontSize = value; OnPropertyChanged("FontSize"); }
        }

        public void LoadSettings()
        {
            this.EditorPath = Properties.Settings.Default.EditorPath;
            this.CommonProperties = Properties.Settings.Default.CommonProperties;
            this.FontName = Properties.Settings.Default.FontName;
            this.FontSize = Properties.Settings.Default.FontSize;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.EditorPath = this.EditorPath;
            Properties.Settings.Default.CommonProperties = this.CommonProperties;
            Properties.Settings.Default.FontName = this.FontName;
            Properties.Settings.Default.FontSize = this.FontSize;

            Properties.Settings.Default.Save();
        }
    }
}
