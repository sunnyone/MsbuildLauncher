using System;
using System.ServiceModel;
using Microsoft.Build.Framework;
using MsbuildLauncher.Common;

namespace MsbuildLauncher.Agent {
    class Program {
        // FIXME: duplicated
        const ConsoleColor defaultConsoleColor = ConsoleColor.White;

        static void Build(IMsbuildLauncherApi launcherApi) {
            ConsoleColor lastColor = ConsoleColor.White;

            string xmlPath = launcherApi.GetXmlPath();
            string targetName = launcherApi.GetTargetName();

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
