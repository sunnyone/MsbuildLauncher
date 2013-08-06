using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;

namespace MsbuildLauncher.Common.Driver
{
    class MsbuildLauncherPSHost : PSHost
    {
        private MSBuildLauncherPSHostUserInterface userInterface;
        public MsbuildLauncherPSHost(MSBuildLauncherPSHostUserInterface userInterface)
        {
            this.userInterface = userInterface;
        }

        private CultureInfo originalCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return originalCultureInfo; }
        }
        private CultureInfo originalUICultureInfo = System.Threading.Thread.CurrentThread.CurrentUICulture;
        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return originalUICultureInfo; }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        private Guid instanceId = Guid.NewGuid();
        public override Guid InstanceId
        {
            get { return instanceId; }
        }

        public override string Name
        {
            get { return "MsbuildLauncherPSHost"; }
        }

        public override void NotifyBeginApplication()
        {
            throw new NotImplementedException();
        }

        public override void NotifyEndApplication()
        {
            throw new NotImplementedException();
        }

        public override void SetShouldExit(int exitCode)
        {
            throw new NotImplementedException();
        }

        public override PSHostUserInterface UI
        {
            get { return userInterface; }
        }

        public override Version Version
        {
            get { return new Version(ThisAssembly.AssemblyVersion); }
        }
    }

    class MSBuildLauncherPSHostUserInterface : PSHostUserInterface
    {
        private IDriverBuildFeedback driverBuildFeedback;
        private MSBuildLauncherPSHostRawUserInterface rawUI;
        public MSBuildLauncherPSHostUserInterface(IDriverBuildFeedback driverBuildFeedback)
        {
            this.driverBuildFeedback = driverBuildFeedback;
            rawUI = new MSBuildLauncherPSHostRawUserInterface();
        }

        public override Dictionary<string, System.Management.Automation.PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException();
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName, System.Management.Automation.PSCredentialTypes allowedCredentialTypes, System.Management.Automation.PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return rawUI; }
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            throw new NotImplementedException();
        }

        public override void Write(string value)
        {
            throw new NotImplementedException();
        }

        public override void WriteDebugLine(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteErrorLine(string value)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string value)
        {
            throw new NotImplementedException();
        }

        public override void WriteProgress(long sourceId, System.Management.Automation.ProgressRecord record)
        {
            throw new NotImplementedException();
        }

        public override void WriteVerboseLine(string message)
        {
            throw new NotImplementedException();
        }

        public override void WriteWarningLine(string message)
        {
            throw new NotImplementedException();
        }
    }

    class MSBuildLauncherPSHostRawUserInterface : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Size BufferSize
        {
            get { return new Size(int.MaxValue, int.MaxValue); }
            set
            {
                // DO NOTHING
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int CursorSize
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void FlushInputBuffer()
        {
            throw new NotImplementedException();
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override bool KeyAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        public override Coordinates WindowPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Size WindowSize
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string WindowTitle
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    class PowerShellDriver : IDriver
    {
        private string fileFullPath;

        public void Open(string filePath)
        {
            this.fileFullPath = System.IO.Path.GetFullPath(filePath);
        }

        public void Dispose()
        {
        }

        public string[] GetTargetNames()
        {
            return new string[0];
        }

        public void GetProperties(string[] commonPropertyNames, out PropertyItem[] commonProperties, out PropertyItem[] fileProperties)
        {
            commonProperties = new PropertyItem[0];
            fileProperties = new PropertyItem[0];
        }

        public void Build(string targetName, KeyValuePair<string, string>[] properties, IDriverBuildFeedback feedback)
        {
            var ui = new MSBuildLauncherPSHostUserInterface(feedback);
            var pshost = new MsbuildLauncherPSHost(ui);

            try
            {
                using (var runspace = RunspaceFactory.CreateRunspace(pshost))
                {
                    runspace.Open();

                    runspace.CreatePipeline("Set-ExecutionPolicy Unrestricted -Scope Process").Invoke();

                    var pipeline = runspace.CreatePipeline(this.fileFullPath);
                    //pipeline.Commands.AddScript("Out-String");

                    foreach (var obj in pipeline.Invoke())
                    {
                        feedback.WriteLog(obj.ToString() + "\n", ConsoleColor.White);
                    }
                }
            }
            catch (Exception ex)
            {
                feedback.WriteLog("An error occured: " + ex.ToString(), ConsoleColor.Red);
            }
        }

    }
}
