using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;

namespace MsbuildLauncher.Common.Driver
{
    public class MSBuildDriver : IDriver
    {
        Project project;
        public void Open(string filePath)
        {
            project = new Microsoft.Build.Evaluation.Project(filePath);
        }

        public void Dispose()
        {
            project.ProjectCollection.UnloadAllProjects();
        }

        public string[] GetTargetNames()
        {
            return project.Targets.Select(kvp => kvp.Key).ToArray();
        }

        private PropertyItem createPropertyItem(Microsoft.Build.Evaluation.Project project, Microsoft.Build.Evaluation.ProjectProperty prop)
        {
            var propItem = new PropertyItem()
                {
                    Name = prop.Name,
                    DefaultValue = prop.UnevaluatedValue,
                    Value = prop.UnevaluatedValue,
                    IsChanged = false,
                    IsEnabled = true
                };

            var q = project.ConditionedProperties.Where(x => x.Key == prop.Name);
            if (q.Any())
            {
                var condProp = q.First();
                propItem.Items = new string[] { prop.UnevaluatedValue }.Union(condProp.Value).ToArray();
            }

            return propItem;
        }
        
        public void GetProperties(string[] commonPropertyNames, out PropertyItem[] commonProperties, out PropertyItem[] fileProperties)
        {
            commonProperties =
                commonPropertyNames.Select(name =>
                    {
                        var prop = project.Properties.Where(x => x.Name == name).FirstOrDefault();
                        if (prop == null)
                            return new PropertyItem() {Name = name, IsEnabled = false};
                        else
                            return createPropertyItem(project, prop);
                    }).ToArray();

            fileProperties =
                project.Properties
                       .Where(prop =>
                           !(prop.IsEnvironmentProperty || prop.IsGlobalProperty || prop.IsImported ||
                             prop.IsReservedProperty))
                       .Where(prop => !commonPropertyNames.Any(name => prop.Name == name))
                       .Select(prop => createPropertyItem(project, prop))
                       .ToArray();
        }

        public void Build(string targetName, KeyValuePair<string, string>[] properties, IDriverBuildFeedback feedback)
        {
            ConsoleColor lastColor = ConsoleColor.White;

            var consoleLogger = new Microsoft.Build.Logging.ConsoleLogger(LoggerVerbosity.Normal,
                (text) =>
                {
                    feedback.WriteLog(text, lastColor);
                },
                (c) => { lastColor = c; }, // set color
                () => { lastColor = ConsoleColor.White; }); // reset color

            ILogger[] loggers = new ILogger[] { consoleLogger };

            foreach (var prop in properties)
                project.SetGlobalProperty(prop.Key, prop.Value);

            if (string.IsNullOrEmpty(targetName))
                project.Build(loggers);
            else
                project.Build(targetName, loggers);
        }
    }
}
