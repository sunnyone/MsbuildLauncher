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
using System.ServiceModel;
using MsbuildLauncher.Common;
using MsbuildLauncher.Common.Driver;

namespace MsbuildLauncher.Agent {
    class DriverFeedback : IDriverBuildFeedback
    {
        private IMsbuildLauncherApi launcherApi;
        public DriverFeedback(IMsbuildLauncherApi launcherApi)
        {
            this.launcherApi = launcherApi;
        }

        public void WriteLog(string text, ConsoleColor color)
        {
            launcherApi.WriteLog(text, color.ToString());
        }
    }

    class Program {
        static void Build(IMsbuildLauncherApi launcherApi)
        {
            var driverFeedback = new DriverFeedback(launcherApi);

            string filePath = launcherApi.GetXmlPath();
            string targetName = launcherApi.GetTargetName();
            var propertyList = launcherApi.GetProperties();

            using (var driver = new MSBuildDriver())
            {
                driver.Open(filePath);
                driver.Build(targetName, propertyList.ToArray(), driverFeedback);
            }
        }

        static void Main(string[] args) {
            if (args.Length != 1) {
                throw new ArgumentException("Argument is not valid.");
            }

            string pipeName = args[0];

            ChannelFactory<IMsbuildLauncherApi> channelFactory = null;
            try {
                channelFactory = new ChannelFactory<IMsbuildLauncherApi>(
                                                        new NetNamedPipeBinding(),
                                                        new EndpointAddress("net.pipe://localhost/" + pipeName));
                channelFactory.Open();
                IMsbuildLauncherApi launcherApi = channelFactory.CreateChannel();
                Build(launcherApi);
            } finally {
                if (channelFactory != null) {
                    channelFactory.Close();
                }
            }
        }
    }
}
