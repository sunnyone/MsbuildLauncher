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
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using MsbuildLauncher.Common;
using MsbuildLauncher.Common.Driver;

namespace MsbuildLauncher.Agent {
    class Program {
        private static void withChannel(string pipeName, Action<IMsbuildLauncherApi> action)
        {
            ChannelFactory<IMsbuildLauncherApi> channelFactory = null;
            try
            {
                channelFactory = new ChannelFactory<IMsbuildLauncherApi>(
                    AgentCommunicationUtil.CreateBinding(),
                    new EndpointAddress(AgentCommunicationUtil.CreateEndpointUri(pipeName)));
                channelFactory.Open();
                IMsbuildLauncherApi launcherApi = channelFactory.CreateChannel();
                action(launcherApi);
            }
            finally
            {
                if (channelFactory != null)
                {
                    try
                    {
                        channelFactory.Close();
                    }
                    catch
                    {
                        channelFactory.Abort();
                    }
                }
            }
        }

        static void buildWithDriver(IMsbuildLauncherApi launcherApi, IDriverBuildFeedback driverFeedback)
        {
            string filePath = launcherApi.GetXmlPath();
            string targetName = launcherApi.GetTargetName();
            var propertyList = launcherApi.GetProperties();

            using (var driver = DriverDispatcher.CreateDriverByFilename(filePath))
            {
                driver.Open(filePath);
                driver.Build(targetName, propertyList.ToArray(), driverFeedback);
            }
        }

        static void writeLogError(string pipeName, string message) {
            // For DriverFeedback is not available
            withChannel(pipeName, api =>
            {
                api.WriteLog(new List<LogMessage> { new LogMessage() 
                    { Text = message, Color = "Red" }
                });
            });
        }

        static void Build(string pipeName)
        {
            Task driverTask = null, buildTask = null;
            DriverBuildFeedback driverFeedback = null;
            string errorMessage = null;
            withChannel(pipeName, launcherApi =>
            {
                driverFeedback = new DriverBuildFeedback(launcherApi);

                driverTask = driverFeedback.StartAsync(CancellationToken.None);
                buildTask = Task.Factory.StartNew(() => buildWithDriver(launcherApi, driverFeedback));

                Task.WaitAny(driverTask, buildTask);

                if (driverTask.IsCompleted)
                {
                    if (driverTask.IsFaulted)
                    {
                        errorMessage = string.Format("Communication failed with the agent: {0}", driverTask.Exception);
                    }
                }

                if (buildTask.IsCompleted)
                {
                    if (driverFeedback != null)
                    {
                        driverFeedback.Complete();
                        try
                        {
                            driverTask.Wait();
                        }
                        catch
                        {
                            // observe an exception with last Task.WaitAll if nessessary.
                        }
                    }

                    if (buildTask.IsFaulted)
                    {
                        errorMessage = string.Format("Build throws an exception: {0}", buildTask.Exception);
                    }
                }
            });

            // This may throw an exception, it will abort the process.
            if (errorMessage != null)
            {
                writeLogError(pipeName, errorMessage);
            }

            // No way exists to stop buildTask properly. Wait the user cancel the build if nessessary.
            Task.WaitAll(buildTask, driverTask);
        }

        static void Main(string[] args) {
            if (args.Length != 1) {
                throw new ArgumentException("Argument is not valid.");
            }

            string pipeName = args[0];
            Build(pipeName);
        }
    }
}
