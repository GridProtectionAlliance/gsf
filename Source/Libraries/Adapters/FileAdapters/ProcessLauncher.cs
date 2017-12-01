//******************************************************************************************************
//  ProcessLauncher.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/29/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using GSF;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries.Adapters;

namespace FileAdapters
{
    /// <summary>
    /// Represents an adapter that will launch a configured executable process.
    /// </summary>
    /// <remarks>
    /// Unless a credentials are provided to create an authentication context, rights of
    /// any launched executable will be limited to those available to time-series host.
    /// </remarks>
    public class ProcessLauncher : FacileActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="SupportsTemporalProcessing"/> property.
        /// </summary>
        public const bool DefaultSupportsTemporalProcessing = false;

        /// <summary>
        /// Default value for the <see cref="Arguments"/> property.
        /// </summary>
        public const string DefaultArguments = "";

        /// <summary>
        /// Default value for the <see cref="WorkingDirectory"/> property.
        /// </summary>
        public const string DefaultWorkingDirectory = "";

        /// <summary>
        /// Default value for the <see cref="EnvironmentalConnectionString"/> property.
        /// </summary>
        public const string DefaultEnvironmentalConnectionString = "";

        /// <summary>
        /// Default value for the <see cref="CreateNoWindow"/> property.
        /// </summary>
        public const bool DefaultCreateNoWindow = true;

        /// <summary>
        /// Default value for the <see cref="WindowStyle"/> property.
        /// </summary>
        public const ProcessWindowStyle DefaultWindowStyle = ProcessWindowStyle.Normal;

        /// <summary>
        /// Default value for the <see cref="ErrorDialog"/> property.
        /// </summary>
        public const bool DefaultErrorDialog = false;

        /// <summary>
        /// Default value for the <see cref="Domain"/> property.
        /// </summary>
        public const string DefaultDomain = "";

        /// <summary>
        /// Default value for the <see cref="UserName"/> property.
        /// </summary>
        public const string DefaultUserName = "";

        /// <summary>
        /// Default value for the <see cref="Password"/> property.
        /// </summary>
        public const string DefaultPassword = "";

        /// <summary>
        /// Default value for the <see cref="LoadUserProfile"/> property.
        /// </summary>
        public const bool DefaultLoadUserProfile = false;

        /// <summary>
        /// Default value for the <see cref="InitialInputFileName"/> property.
        /// </summary>
        public const string DefaultInitialInputFileName = "";

        /// <summary>
        /// Default value for the <see cref="InitialInputProcessingDelay"/> property.
        /// </summary>
        public const int DefaultInitialInputProcessingDelay = 2000;

        /// <summary>
        /// Default value for the <see cref="RedirectOutputToHostEnvironment"/> property.
        /// </summary>
        public const bool DefaultRedirectOutputToHostEnvironment = true;

        /// <summary>
        /// Default value for the <see cref="RedirectErrorToHostEnvironment"/> property.
        /// </summary>
        public const bool DefaultRedirectErrorToHostEnvironment = true;

        // Fields
        private readonly Process m_process;
        private bool m_supportsTemporalProcessing;
        private long m_inputLinesProcessed;
        private long m_outputLinesProcessed;
        private long m_errorLinesProcessed;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ProcessLauncher"/> class.
        /// </summary>
        public ProcessLauncher()
        {
            m_process = new Process();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter supports temporal processing."),
        DefaultValue(DefaultSupportsTemporalProcessing)]
        public override bool SupportsTemporalProcessing => m_supportsTemporalProcessing;

        /// <summary>
        /// Gets or sets the path and filename of executable to launch as a new process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the path and filename of executable to launch as a new process.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets any command line arguments to use when launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define any command line arguments to use when launching the process."),
        DefaultValue(DefaultArguments)]
        public string Arguments { get; set; } = DefaultArguments;

        /// <summary>
        /// Gets or sets working directory to use when launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define working directory to use when launching the process. Defaults to current working directory as set by hosting environment."),
        DefaultValue(DefaultWorkingDirectory)]
        public string WorkingDirectory { get; set; } = DefaultWorkingDirectory;

        /// <summary>
        /// Gets or sets any needed environmental variables that should be set or updated before launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define any needed environmental variables that should be set or updated before launching the process. Example connection string format: variable1=value1; variable2=value2"),
        DefaultValue(DefaultEnvironmentalConnectionString)]
        public string EnvironmentalConnectionString { get; set; } = DefaultEnvironmentalConnectionString;

        /// <summary>
        /// Gets or sets flag that determines if a new window should be created when launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define flag that determines if a new window should be created when launching the process. This property is only applicable when adapter hosting environment can interact with a desktop user interface."),
        DefaultValue(DefaultCreateNoWindow)]
        public bool CreateNoWindow { get; set; } = DefaultCreateNoWindow;

        /// <summary>
        /// Gets or sets window style to use when launching the process if creating a window.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define window style to use when launching the process if creating a window. This property is only applicable when adapter hosting environment can interact with a desktop user interface."),
        DefaultValue(DefaultWindowStyle)]
        public ProcessWindowStyle WindowStyle { get; set; } = DefaultWindowStyle;

        /// <summary>
        /// Gets or sets flag that determines if an error dialog should be displayed if the process cannot be launched.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define flag that determines if an error dialog should be displayed if the process cannot be launched. This property is only applicable when adapter hosting environment can interact with a desktop user interface."),
        DefaultValue(DefaultErrorDialog)]
        public bool ErrorDialog { get; set; } = DefaultErrorDialog;

        /// <summary>
        /// Gets or sets the user domain to use when creating an authentication context before launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the user domain to use when creating an authentication context before launching the process. Leave blank to use authentication context of hosting environment."),
        DefaultValue(DefaultDomain)]
        public string Domain { get; set; } = DefaultDomain;

        /// <summary>
        /// Gets or sets the user name to use when creating an authentication context before launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the user name to use when creating an authentication context before launching the process. Leave blank to use authentication context of hosting environment."),
        DefaultValue(DefaultUserName)]
        public string UserName { get; set; } = DefaultUserName;

        /// <summary>
        /// Gets or sets the user password to use when creating an authentication context before launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the user password to use when creating an authentication context before launching the process. Leave blank to use authentication context of hosting environment."),
        DefaultValue(DefaultPassword)]
        public string Password { get; set; } = DefaultPassword;

        /// <summary>
        /// Gets or sets flag that determines if Windows user profile should be loaded from the registry before launching the process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define flag that determines if Windows user profile should be loaded from the registry before launching the process."),
        DefaultValue(DefaultLoadUserProfile)]
        public bool LoadUserProfile { get; set; } = DefaultLoadUserProfile;

        /// <summary>
        /// Gets or sets filename that contains text to use as standard input into process after it has been launched.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define filename that contains text to use as input into process after it has been launched. UTF-8 encoding assumed."),
        DefaultValue(DefaultInitialInputFileName)]
        public string InitialInputFileName { get; set; } = DefaultInitialInputFileName;

        /// <summary>
        /// Gets or sets the processing delay, in milliseconds, before text defined in the <see cref="InitialInputFileName"/> is sent to the launched process.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the processing delay, in milliseconds, before text defined in the InitialInputFileName is sent to the launched process."),
        DefaultValue(DefaultInitialInputProcessingDelay)]
        public int InitialInputProcessingDelay { get; set; } = DefaultInitialInputProcessingDelay;

        /// <summary>
        /// Gets or sets flag that determines if process standard output should be redirected to the hosting environment.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define flag that determines if process standard output should be redirected to the hosting environment."),
        DefaultValue(DefaultRedirectOutputToHostEnvironment)]
        public bool RedirectOutputToHostEnvironment { get; set; } = DefaultRedirectOutputToHostEnvironment;

        /// <summary>
        /// Gets or sets flag that determines if process standard error output should be redirected to the hosting environment.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define flag that determines if process standard error output should be redirected to the hosting environment."),
        DefaultValue(DefaultRedirectErrorToHostEnvironment)]
        public bool RedirectErrorToHostEnvironment { get; set; } = DefaultRedirectErrorToHostEnvironment;

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                if (string.IsNullOrEmpty(WorkingDirectory))
                    WorkingDirectory = Directory.GetCurrentDirectory();

                status.Append(base.Status);
                status.AppendLine($"       Executable filename: {FilePath.TrimFileName(FileName, 51)}");

                try
                {
                    FileVersionInfo version = m_process.MainModule.FileVersionInfo;

                    status.AppendLine($"   Executable file version: {version.FileVersion}");

                    if (!string.IsNullOrWhiteSpace(version.FileDescription))
                        status.AppendLine($"    Executable description: {version.FileDescription}");

                    if (!string.IsNullOrWhiteSpace(version.Comments))
                        status.AppendLine($"       Executable comments: {version.Comments}");

                    if (!string.IsNullOrWhiteSpace(version.CompanyName))
                        status.AppendLine($"   Executable company name: {version.CompanyName}");

                    if (!string.IsNullOrWhiteSpace(version.LegalCopyright))
                        status.AppendLine($"      Executable copyright: {version.LegalCopyright}");

                    status.AppendLine($" Executable is debug build: {version.IsDebug}");
                    status.AppendLine($"       Executable language: {version.Language}");

                    if (!string.IsNullOrWhiteSpace(version.OriginalFilename))
                        status.AppendLine($"  Executable original name: {version.OriginalFilename}");

                    if (!string.IsNullOrWhiteSpace(version.InternalName))
                        status.AppendLine($"  Executable internal name: {version.InternalName}");

                    if (!string.IsNullOrWhiteSpace(version.ProductName))
                        status.AppendLine($"   Executable product name: {version.ProductName}");

                    if (!string.IsNullOrWhiteSpace(version.ProductVersion))
                        status.AppendLine($"Executable product version: {version.ProductVersion}");
                }
                catch
                {
                    status.AppendLine("       Version information: [unavailable]");
                }

                status.AppendLine($"         Working directory: {FilePath.TrimFileName(WorkingDirectory, 51)}");
                status.AppendLine($"              Process name: {m_process.ProcessName}");
                status.AppendLine($"             OS Process ID: {m_process.Id}");
                status.AppendLine($"        Process start time: {m_process.StartTime:yyyy-MM-dd HH:mm:ss.fff}");
                status.AppendLine($"     Process is responding: {m_process.Responding}");
                status.AppendLine($"     Process base priority: {m_process.BasePriority}");
                status.AppendLine($"        Process has exited: {m_process.HasExited}");

                if (m_process.HasExited)
                {
                    status.AppendLine($"         Process exit code: {m_process.ExitCode}");
                    status.AppendLine($"         Process exit time: {m_process.ExitTime:yyyy-MM-dd HH:mm:ss.fff}");
                    status.AppendLine($"      Total processor time: {m_process.TotalProcessorTime.ToElapsedTimeString()}");
                    status.AppendLine($"            Total run-time: {(m_process.ExitTime - m_process.StartTime).ToElapsedTimeString()}");
                }
                else
                {
                    status.AppendLine($"      Process thread count: {m_process.Threads:N0}");
                    status.AppendLine($"      Process handle count: {m_process.HandleCount:N0}");
                    status.AppendLine($"      Total processor time: {m_process.TotalProcessorTime.ToElapsedTimeString()}");
                    status.AppendLine($"            Total run-time: {(DateTime.Now - m_process.StartTime).ToElapsedTimeString()}");
                }

                status.AppendLine($"    Initial input filename: {FilePath.TrimFileName(InitialInputFileName, 51)}");
                status.AppendLine($"     Input lines processed: {m_inputLinesProcessed:N0}");
                status.AppendLine($"    Output lines processed: {m_outputLinesProcessed:N0}");
                status.AppendLine($"     Error lines processed: {m_errorLinesProcessed:N0}");

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]
                
        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProcessLauncher"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {			
                    // This will be done regardless of whether the object is finalized or disposed.
                    
                    if (disposing)
                    {
                        try
                        {
                            m_process.Dispose();
                        }
                        catch
                        {
                            try
                            {
                                if (!m_process.HasExited)
                                    m_process.Kill();
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to stop process launched \"{FileName}\": {ex.Message}", ex));
                            }
                        }
                        finally
                        {
                            m_process.OutputDataReceived -= ProcessOutputDataReceived;
                            m_process.ErrorDataReceived -= ProcessErrorDataReceived;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Initializes <see cref="ProcessLauncher"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            ProcessWindowStyle windowStyle;
            int initialInputProcessingDelay;
            string setting;

            ProcessStartInfo startInfo = new ProcessStartInfo();

            // Load required parameters
            if (settings.TryGetValue(nameof(FileName), out setting))
            {
                setting = setting.Trim();

                if (File.Exists(setting))
                {
                    FileName = setting;
                    startInfo.FileName = FileName;
                }
                else
                {
                    throw new FileNotFoundException($"Cannot launch process: specified executable path and filename \"{setting}\" does not exist.");
                }
            }
            else
            {
                throw new ArgumentException($"Cannot launch process: required \"{nameof(FileName)}\" parameter is missing from connection string.");
            }

            // Load optional parameters
            if (settings.TryGetValue(nameof(SupportsTemporalProcessing), out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = DefaultSupportsTemporalProcessing;

            // Note that it's possible that time-series framework is being hosted by an application
            // running in a Window making many of the following process start properties relevant.
            // Even when hosted as a service, the user may start the service log on using the
            // local system account and select to allow the service interact with the desktop.
            if (settings.TryGetValue(nameof(Arguments), out setting) && setting.Length > 0)
                startInfo.Arguments = setting;

            if (settings.TryGetValue(nameof(WorkingDirectory), out setting))
            {
                setting = setting.Trim();

                if (Directory.Exists(setting))
                {
                    WorkingDirectory = setting;
                    startInfo.WorkingDirectory = WorkingDirectory;
                }
                else
                {
                    throw new DirectoryNotFoundException($"Cannot launch process: specified working directory \"{setting}\" does not exist.");
                }
            }

            if (settings.TryGetValue(nameof(EnvironmentalConnectionString), out setting))
            {
                foreach (KeyValuePair<string, string> item in setting.ParseKeyValuePairs())
                    startInfo.Environment[item.Key] = item.Value;
            }

            if (settings.TryGetValue(nameof(CreateNoWindow), out setting))
                startInfo.CreateNoWindow = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(WindowStyle), out setting) && Enum.TryParse(setting, true, out windowStyle))
                startInfo.WindowStyle = windowStyle;

            if (settings.TryGetValue(nameof(ErrorDialog), out setting))
                startInfo.ErrorDialog = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(Domain), out setting) && setting.Length > 0)
                startInfo.Domain = setting;

            if (settings.TryGetValue(nameof(UserName), out setting) && setting.Length > 0)
                startInfo.UserName = setting;

            if (settings.TryGetValue(nameof(Password), out setting) && setting.Length > 0)
                startInfo.Password = setting.ToSecureString();

            if (settings.TryGetValue(nameof(LoadUserProfile), out setting))
                startInfo.LoadUserProfile = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(InitialInputFileName), out setting))
            {
                setting = setting.Trim();

                if (File.Exists(setting))
                    InitialInputFileName = setting;
                else
                    throw new FileNotFoundException($"Cannot launch process: specified initial input filename \"{setting}\" does not exist.");
            }

            if (settings.TryGetValue(nameof(InitialInputProcessingDelay), out setting) && int.TryParse(setting, out initialInputProcessingDelay) && initialInputProcessingDelay > -1)
                InitialInputProcessingDelay = initialInputProcessingDelay;

            if (settings.TryGetValue(nameof(RedirectOutputToHostEnvironment), out setting))
            {
                RedirectOutputToHostEnvironment = setting.ParseBoolean();
                startInfo.RedirectStandardOutput = RedirectOutputToHostEnvironment;
            }

            if (settings.TryGetValue(nameof(RedirectErrorToHostEnvironment), out setting))
            {
                RedirectErrorToHostEnvironment = setting.ParseBoolean();
                startInfo.RedirectStandardError = true;
            }

            startInfo.RedirectStandardInput = true;

            m_process.StartInfo = startInfo;
            m_process.EnableRaisingEvents = true;
            m_process.OutputDataReceived += ProcessOutputDataReceived;
            m_process.ErrorDataReceived += ProcessErrorDataReceived;

            if (!m_process.Start())
                throw new InvalidOperationException($"Failed to launch process from \"{FileName}\".");

            if (!string.IsNullOrEmpty(InitialInputFileName))
            {
                new Action(() =>
                {
                    try
                    {
                        using (StreamReader reader = File.OpenText(InitialInputFileName))
                        {
                            string line;

                            while ((object)(line = reader.ReadLine()) != null)
                                Input(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed while sending text from \"{InitialInputFileName}\" to launched process standard input: {ex.Message}", ex));
                    }                    
                })
                .DelayAndExecute(InitialInputProcessingDelay);
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if (m_process.HasExited)
                return $"Process launched from \"{FileName}\" exited with code {m_process.ExitCode}.".CenterText(maxLength);
            
            if (m_process.Responding)
                return $"Process launched from \"{FileName}\" has been running for {(DateTime.Now - m_process.StartTime).ToElapsedTimeString()}.".CenterText(maxLength);

            return $"Process launched from \"{FileName}\" is not responding...".CenterText(maxLength);
        }

        /// <summary>
        /// Sends specified value as input to launched process, new line will be automatically appended to text.
        /// </summary>
        /// <param name="value">Input string to send to launched process.</param>
        [AdapterCommand("Sends specified value as input to launched process, new line will be automatically appended to text.")]
        public void Input(string value)
        {           
            m_process.StandardInput.WriteLine(value);
            m_process.StandardInput.Flush();
            m_inputLinesProcessed++;
        }

        /// <summary>
        /// Clears any cached information associated with the launched process.
        /// </summary>
        [AdapterCommand("Clears any cached information associated with the launched process.")]
        public void Refresh()
        {
            m_process.Refresh();
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, e.Data);
            m_outputLinesProcessed++;
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnStatusMessage(MessageLevel.Error, e.Data);
            m_errorLinesProcessed++;
        }

        #endregion
    }
}
