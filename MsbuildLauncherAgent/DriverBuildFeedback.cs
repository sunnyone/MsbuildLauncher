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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MsbuildLauncher.Common;
using MsbuildLauncher.Common.Driver;

namespace MsbuildLauncher.Agent
{
    class DriverBuildFeedback : IDriverBuildFeedback
    {
        private IMsbuildLauncherApi launcherApi;
        public DriverBuildFeedback(IMsbuildLauncherApi launcherApi)
        {
            this.launcherApi = launcherApi;
        }

        public void WriteLog(string text, ConsoleColor color)
        {
            logQueue.Add(new LogMessage() { Text = text, Color = color.ToString() });
        }

        public void Complete()
        {
            logQueue.CompleteAdding();
        }

        private static readonly TimeSpan flushInterval = TimeSpan.FromMilliseconds(20);

        private BlockingCollection<LogMessage> logQueue = new BlockingCollection<LogMessage>(); 
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => start(cancellationToken), cancellationToken);
        }

        private void start(CancellationToken cancellationToken)
        {
            var logBuffer = new List<LogMessage>();
            var nextFlush = DateTime.MinValue;

            LogMessage logMessage;
            while (!logQueue.IsAddingCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var timeoutMs = Math.Max(0, (int)(DateTime.UtcNow - nextFlush).TotalMilliseconds);

                if (logQueue.TryTake(out logMessage, timeoutMs, cancellationToken))
                {
                    logBuffer.Add(logMessage);
                }

                if (nextFlush < DateTime.UtcNow)
                {
                    if (logBuffer.Any())
                    {
                        launcherApi.WriteLog(logBuffer);
                        logBuffer.Clear();
                    }

                    nextFlush = DateTime.UtcNow + flushInterval;
                }
            }

            // Flush remained log messages
            while (logQueue.TryTake(out logMessage))
            {
                logBuffer.Add(logMessage);
            }

            if (logBuffer.Any())
            {
                launcherApi.WriteLog(logBuffer);
            }
        }
    }
}
