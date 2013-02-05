using System;
using System.ServiceModel;
using Microsoft.Build.Framework;
using MsbuildLauncher.Common;

namespace MsbuildLauncher.Agent {
    class Program {
        // FIXME: duplicated
        const ConsoleColor defaultConsoleColor = ConsoleColor.White;

        static void Build(string xmlPath, string targetName, IMsbuildLauncherApi launcherApi) {
            ConsoleColor lastColor = ConsoleColor.White;

            var proj = new Microsoft.Build.Evaluation.Project(xmlPath);
            var consoleLogger = new Microsoft.Build.Logging.ConsoleLogger(LoggerVerbosity.Normal,
                (text) => {
                    launcherApi.WriteLog(text, lastColor.ToString());
                },
                (c) => { lastColor = c; }, // set color
                () => { lastColor = defaultConsoleColor; }); // reset color

            ILogger[] loggers = new ILogger[] { consoleLogger };
            
            if (string.IsNullOrEmpty(targetName))
            {
                proj.Build(loggers);
            }
            else
            {
                proj.Build(targetName, loggers);
            }

            proj.ProjectCollection.UnloadAllProjects();
        }


        static void Main(string[] args) {
            if (args.Length != 3) {
                throw new ArgumentException("Argument is not valid.");
            }

            string pipeName = args[0];
            string xmlPath = args[1];
            string targetName = args[2];

            ChannelFactory<IMsbuildLauncherApi> channelFactory = null;
            try {
                channelFactory = new ChannelFactory<IMsbuildLauncherApi>(
                                                        new NetNamedPipeBinding(),
                                                        new EndpointAddress("net.pipe://localhost/" + pipeName));
                channelFactory.Open();
                IMsbuildLauncherApi launcherApi = channelFactory.CreateChannel();
                Build(xmlPath, targetName, launcherApi);
            } finally {
                if (channelFactory != null) {
                    channelFactory.Close();
                }
            }
        }
    }
}
