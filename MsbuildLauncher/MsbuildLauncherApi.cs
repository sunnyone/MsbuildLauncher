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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MsbuildLauncher.Common;

namespace MsbuildLauncher {
    public class MsbuildLauncherApi : IMsbuildLauncherApi
    {
        public void WriteLog(string text, string color) {
            var mainViewModel = ((App)App.Current).MainViewModel;
            mainViewModel.OutputLog(text, color);
        }

        public string GetXmlPath()
        {
            var mainViewModel = ((App)App.Current).MainViewModel;
            
            if (mainViewModel.BuildContext == null)
                return null;

            return mainViewModel.BuildContext.XmlPath;
        }

        public string GetTargetName()
        {
            var mainViewModel = ((App)App.Current).MainViewModel;

            if (mainViewModel.BuildContext == null)
                return null;

            return mainViewModel.BuildContext.TargetName;
        }

        public List<KeyValuePair<string, string>> GetProperties()
        {
            var mainViewModel = ((App)App.Current).MainViewModel;

            if (mainViewModel.BuildContext == null)
                return null;

            return mainViewModel.BuildContext.PropertyList;
        }
    }
}
