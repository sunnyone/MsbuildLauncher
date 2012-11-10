using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Microsoft.Build.Framework;

namespace MsbuildLauncherAgent {
    class Program {
        // FIXME: duplicated
        const ConsoleColor defaultConsoleColor = ConsoleColor.White;

        static void Build(string xmlPath, string targetName, IMsbuildLogWriter logWriter) {
            ConsoleColor lastColor = ConsoleColor.White;

            var proj = new Microsoft.Build.Evaluation.Project(xmlPath);
            var consoleLogger = new Microsoft.Build.Logging.ConsoleLogger(LoggerVerbosity.Normal,
                (text) => {
                    logWriter.WriteLog(text, lastColor.ToString());
                },
                (c) => { lastColor = c; }, // set color
                () => { lastColor = defaultConsoleColor; }); // reset color

            ILogger[] loggers = new ILogger[] { consoleLogger };
            proj.Build(targetName, loggers);
            proj.ProjectCollection.UnloadAllProjects();
        }


        static void Main(string[] args) {
            if (args.Length != 3) {
                throw new ArgumentException("Argument is not valid.");
            }

            string pipeName = args[0];
            string xmlPath = args[1];
            string targetName = args[2];

            ChannelFactory<IMsbuildLogWriter> channelFactory = null;
            try {
                channelFactory = new ChannelFactory<IMsbuildLogWriter>(
                                                        new NetNamedPipeBinding(),
                                                        new EndpointAddress("net.pipe://localhost/" + pipeName));
                channelFactory.Open();
                IMsbuildLogWriter logWriter = channelFactory.CreateChannel();
                Build(xmlPath, targetName, logWriter);
            } finally {
                if (channelFactory != null) {
                    channelFactory.Close();
                }
            }
        }
    }
}
