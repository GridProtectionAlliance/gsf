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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GSF;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Statistics;
using GSF.Units;

// ReSharper disable AssignNullToNotNullAttribute
namespace FileAdapters;

/// <summary>
/// Represents an adapter that will launch a configured executable process.
/// </summary>
/// <remarks>
/// Unless credentials are provided to create an authentication context, rights of
/// any launched executable will be limited to those available to time-series host.
/// </remarks>
[Description("Process Launcher: Launches a configured executable process on initialization")]
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
    /// Default value for the <see cref="EnvironmentalVariables"/> property.
    /// </summary>
    public const string DefaultEnvironmentalVariables = "";

    /// <summary>
    /// Default value for the <see cref="CreateNoWindow"/> property.
    /// </summary>
    public const bool DefaultCreateNoWindow = true;

    /// <summary>
    /// Default value for the <see cref="WindowStyle"/> property.
    /// </summary>
    public const string DefaultWindowStyle = nameof(WindowStyle.Normal);

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

    /// <summary>
    /// Default value for the <see cref="ProcessOutputAsLogMessages"/> property.
    /// </summary>
    public const bool DefaultProcessOutputAsLogMessages = false;

    /// <summary>
    /// Default value for the <see cref="LogMessageTextExpression"/> property.
    /// </summary>
    public const string DefaultLogMessageTextExpression = @"(?<=.*msg\s*\=\s*\"")[^\""]*(?=\"")";

    /// <summary>
    /// Default value for the <see cref="LogMessageLevelExpression"/> property.
    /// </summary>
    public const string DefaultLogMessageLevelExpression = @"(?<=.*lvl\s*\=\s*)[^\s]*(?=\s|$)";

    /// <summary>
    /// Default value for the <see cref="LogMessageLevelMappings"/> property.
    /// </summary>
    public const string DefaultLogMessageLevelMappings = "info=Info; warn=Waning; error=Error; critical=Critical; debug=Debug";

    /// <summary>
    /// Default value for the <see cref="ForceKillOnDispose"/> property.
    /// </summary>
    public const bool DefaultForceKillOnDispose = true;

    /// <summary>
    /// Default value for the <see cref="TrackProcessStatistics"/> property.
    /// </summary>
    public const bool DefaultTrackProcessStatistics = true;

    // Fields
    private readonly Process m_process;
    private readonly Dictionary<string, MessageLevel> m_messageLevelMap;
    private readonly ProcessUtilizationCalculator m_processUtilizationCalculator;
    private readonly ChildProcessManager m_childProcessManager;
    private Regex m_logMessageTextExpression;
    private Regex m_logMessageLevelExpression;
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
        m_messageLevelMap = new Dictionary<string, MessageLevel>(StringComparer.OrdinalIgnoreCase);
        m_processUtilizationCalculator = new ProcessUtilizationCalculator();

        // In Windows environments, make sure child processes can be terminated if parent is terminated - even if termination is not graceful
        if (!Common.IsPosixEnvironment)
            m_childProcessManager = new ChildProcessManager();
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the flag indicating if this adapter supports temporal processing.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the flag indicating if this adapter supports temporal processing.")]
    [DefaultValue(DefaultSupportsTemporalProcessing)]
    public override bool SupportsTemporalProcessing => m_supportsTemporalProcessing;

    /// <summary>
    /// Gets or sets the path and filename of executable to launch as a new process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the path and filename of executable to launch as a new process.")]
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets any command line arguments to use when launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define any command line arguments to use when launching the process.")]
    [DefaultValue(DefaultArguments)]
    public string Arguments { get; set; } = DefaultArguments;

    /// <summary>
    /// Gets or sets working directory to use when launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define working directory to use when launching the process. Defaults to path of executable to launch.")]
    [DefaultValue(DefaultWorkingDirectory)]
    public string WorkingDirectory { get; set; } = DefaultWorkingDirectory;

    /// <summary>
    /// Gets or sets any needed environmental variables that should be set or updated before launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define any needed environmental variables that should be set or updated before launching the process. Use connection string format to define variables, for example: variable1=value1; variable2=value2")]
    [DefaultValue(DefaultEnvironmentalVariables)]
    public string EnvironmentalVariables { get; set; } = DefaultEnvironmentalVariables;

    /// <summary>
    /// Gets or sets flag that determines if a new window should be created when launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if a new window should be created when launching the process. This property is only applicable when adapter hosting environment can interact with a user interface.")]
    [DefaultValue(DefaultCreateNoWindow)]
    public bool CreateNoWindow { get; set; } = DefaultCreateNoWindow;

    /// <summary>
    /// Gets or sets window style to use when launching the process if creating a window.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define window style to use when launching the process if creating a window. This property is only applicable when adapter hosting environment can interact with a user interface.")]
    [DefaultValue(typeof(ProcessWindowStyle), DefaultWindowStyle)]
    public ProcessWindowStyle WindowStyle { get; set; } = (ProcessWindowStyle)Enum.Parse(typeof(ProcessWindowStyle), DefaultWindowStyle);

    /// <summary>
    /// Gets or sets flag that determines if an error dialog should be displayed if the process cannot be launched.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if an error dialog should be displayed if the process cannot be launched. This property is only applicable when adapter hosting environment can interact with a user interface.")]
    [DefaultValue(DefaultErrorDialog)]
    public bool ErrorDialog { get; set; } = DefaultErrorDialog;

    /// <summary>
    /// Gets or sets the user domain to use when creating an authentication context before launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the user domain to use when creating an authentication context before launching the process. Leave blank to use authentication context of hosting environment.")]
    [DefaultValue(DefaultDomain)]
    public string Domain { get; set; } = DefaultDomain;

    /// <summary>
    /// Gets or sets the user name to use when creating an authentication context before launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the user name to use when creating an authentication context before launching the process. Leave blank to use authentication context of hosting environment.")]
    [DefaultValue(DefaultUserName)]
    public string UserName { get; set; } = DefaultUserName;

    /// <summary>
    /// Gets or sets the user password to use when creating an authentication context before launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the user password to use when creating an authentication context before launching the process. Leave blank to use authentication context of hosting environment.")]
    [DefaultValue(DefaultPassword)]
    public string Password { get; set; } = DefaultPassword;

    /// <summary>
    /// Gets or sets flag that determines if Windows user profile should be loaded from the registry before launching the process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if Windows user profile should be loaded from the registry before launching the process.")]
    [DefaultValue(DefaultLoadUserProfile)]
    public bool LoadUserProfile { get; set; } = DefaultLoadUserProfile;

    /// <summary>
    /// Gets or sets filename that contains text to use as standard input into process after it has been launched.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define filename that contains text to use as input into process after it has been launched. UTF-8 encoding assumed.")]
    [DefaultValue(DefaultInitialInputFileName)]
    public string InitialInputFileName { get; set; } = DefaultInitialInputFileName;

    /// <summary>
    /// Gets or sets the processing delay, in milliseconds, before text defined in the <see cref="InitialInputFileName"/> is sent to the launched process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the processing delay, in milliseconds, before text defined in the InitialInputFileName is sent to the launched process.")]
    [DefaultValue(DefaultInitialInputProcessingDelay)]
    public int InitialInputProcessingDelay { get; set; } = DefaultInitialInputProcessingDelay;

    /// <summary>
    /// Gets or sets flag that determines if process standard output should be redirected to the hosting environment.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if process standard output should be redirected to the hosting environment.")]
    [DefaultValue(DefaultRedirectOutputToHostEnvironment)]
    public bool RedirectOutputToHostEnvironment { get; set; } = DefaultRedirectOutputToHostEnvironment;

    /// <summary>
    /// Gets or sets flag that determines if process standard error output should be redirected to the hosting environment.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if process standard error output should be redirected to the hosting environment.")]
    [DefaultValue(DefaultRedirectErrorToHostEnvironment)]
    public bool RedirectErrorToHostEnvironment { get; set; } = DefaultRedirectErrorToHostEnvironment;

    /// <summary>
    /// Gets or sets flag that determines if redirected output should be attempted to be parsed as formatted log messages, e.g.: lvl=info msg="log message".
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if redirected output should be attempted to be parsed as formatted log messages, e.g.: lvl=info msg=\"log message\".")]
    [DefaultValue(DefaultProcessOutputAsLogMessages)]
    public bool ProcessOutputAsLogMessages { get; set; } = DefaultProcessOutputAsLogMessages;

    /// <summary>
    /// Gets or sets regular expression used to find log message text when redirected output is processed as formatted log messages.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define regular expression used to find log message text when redirected output is processed as formatted log messages. Multiple matches will be joined as a single string.")]
    [DefaultValue(DefaultLogMessageTextExpression)]
    public string LogMessageTextExpression { get; set; } = DefaultLogMessageTextExpression;

    /// <summary>
    /// Gets or sets regular expression used to find log message level when redirected output is processed as formatted log messages.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define regular expression used to find log message level when redirected output is processed as formatted log messages.")]
    [DefaultValue(DefaultLogMessageLevelExpression)]
    public string LogMessageLevelExpression { get; set; } = DefaultLogMessageLevelExpression;

    /// <summary>
    /// Gets or sets log level mappings to use when redirected output is processed as formatted log messages.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define log level mappings to use when redirected output is processed as formatted log messages. Use connection string format as \"log_level_name=GSF.Diagnostics.MessageLevel\", for example: info=Info; warn=Waning; error=Error; critical=Critical; debug=Debug")]
    [DefaultValue(DefaultLogMessageLevelMappings)]
    public string LogMessageLevelMappings { get; set; } = DefaultLogMessageLevelMappings;

    /// <summary>
    /// Gets or sets flag that determines if launched process should be forcibly terminated when adapter is disposed.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if launched process should be forcibly terminated when adapter is disposed.")]
    [DefaultValue(DefaultForceKillOnDispose)]
    public bool ForceKillOnDispose { get; set; } = DefaultForceKillOnDispose;

    /// <summary>
    /// Gets or sets the interval over which to calculate lunched process utilization.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the interval over which to update the lunched process CPU utilization. Set to 0 to disable utilization calculations.")]
    [DefaultValue(ProcessUtilizationCalculator.DefaultUpdateInterval)]
    public int UtilizationUpdateInterval { get; set; } = ProcessUtilizationCalculator.DefaultUpdateInterval;

    /// <summary>
    /// Gets or sets flag that determines if statistics should be tracked for launched process.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if statistics should be tracked for launched process.")]
    [DefaultValue(DefaultTrackProcessStatistics)]
    public bool TrackProcessStatistics { get; set; } = DefaultTrackProcessStatistics;

    #region [ Hidden Properties ]

    // The following common adapter properties are marked as never browse by an
    // editor since the properties are not used by the ProcessLauncher adapter

    /// <summary>
    /// Property hidden - not used by <see cref="ProcessLauncher"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new MeasurementKey[] InputMeasurementKeys
    {
        get => base.InputMeasurementKeys;
        set => base.InputMeasurementKeys = value;
    }

    /// <summary>
    /// Property hidden - not used by <see cref="ProcessLauncher"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new IMeasurement[] OutputMeasurements
    {
        get => base.OutputMeasurements;
        set => base.OutputMeasurements = value;
    }

    /// <summary>
    /// Property hidden - not used by <see cref="ProcessLauncher"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new int FramesPerSecond
    {
        get => base.FramesPerSecond;
        set => base.FramesPerSecond = value;
    }

    /// <summary>
    /// Property hidden - not used by <see cref="ProcessLauncher"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new double LagTime
    {
        get => base.LagTime;
        set => base.LagTime = value;
    }

    /// <summary>
    /// Property hidden - not used by <see cref="ProcessLauncher"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new double LeadTime
    {
        get => base.LeadTime;
        set => base.LeadTime = value;
    }

    #endregion

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
            StringBuilder status = new();

            if (string.IsNullOrEmpty(WorkingDirectory))
                WorkingDirectory = Directory.GetCurrentDirectory();

            status.Append(base.Status);
            status.AppendLine($"       Executable filename: {FilePath.TrimFileName(FileName, 51)}");
            status.AppendLine($"         Working directory: {FilePath.TrimFileName(WorkingDirectory, 51)}");

            if (Initialized)
            {
                try
                {
                    FileVersionInfo version = m_process.MainModule!.FileVersionInfo;

                    if (!string.IsNullOrWhiteSpace(version.FileVersion))
                        status.AppendLine($"   Executable file version: {version.FileVersion}");

                    if (!string.IsNullOrWhiteSpace(version.FileDescription))
                        status.AppendLine($"    Executable description: {version.FileDescription}");

                    if (!string.IsNullOrWhiteSpace(version.Comments))
                        status.AppendLine($"       Executable comments: {version.Comments}");

                    if (!string.IsNullOrWhiteSpace(version.CompanyName))
                        status.AppendLine($"   Executable company name: {version.CompanyName}");

                    if (!string.IsNullOrWhiteSpace(version.LegalCopyright))
                        status.AppendLine($"      Executable copyright: {version.LegalCopyright}");

                    if (!string.IsNullOrWhiteSpace(version.Language))
                        status.AppendLine($"       Executable language: {version.Language}");

                    if (!string.IsNullOrWhiteSpace(version.OriginalFilename))
                        status.AppendLine($"  Executable original name: {version.OriginalFilename}");

                    if (!string.IsNullOrWhiteSpace(version.InternalName))
                        status.AppendLine($"  Executable internal name: {version.InternalName}");

                    if (!string.IsNullOrWhiteSpace(version.ProductName))
                        status.AppendLine($"   Executable product name: {version.ProductName}");

                    if (!string.IsNullOrWhiteSpace(version.ProductVersion))
                        status.AppendLine($"Executable product version: {version.ProductVersion}");

                    status.AppendLine($" Executable is debug build: {version.IsDebug}");
                }
                catch
                {
                    status.AppendLine("       Version information: [unavailable]");
                }

                try
                {
                    if (m_process.HasExited)
                    {
                        status.AppendLine("        Process has exited: True");
                        status.AppendLine($"         Process exit code: {m_process.ExitCode}");
                        status.AppendLine($"         Process exit time: {m_process.ExitTime:yyyy-MM-dd HH:mm:ss.fff}");
                        status.AppendLine($"      Total processor time: {m_process.TotalProcessorTime.ToElapsedTimeString()}");
                        status.AppendLine($"            Total run-time: {(m_process.ExitTime - ((DateTime)StartTime).ToLocalTime()).ToElapsedTimeString()}");
                    }
                    else
                    {
                        status.AppendLine($"     Process is responding: {m_process.Responding}");
                        status.AppendLine($"              Process name: {m_process.ProcessName}");
                        status.AppendLine($"        Process start time: {m_process.StartTime:yyyy-MM-dd HH:mm:ss.fff}");
                        status.AppendLine($"             OS Process ID: {m_process.Id}");
                        status.AppendLine($"     Process base priority: {m_process.BasePriority}");
                        status.AppendLine($"      Process thread count: {m_process.Threads.Count:N0}");
                        status.AppendLine($"      Process handle count: {m_process.HandleCount:N0}");
                        status.AppendLine($"      Process memory usage: {SI2.ToScaledString(m_process.WorkingSet64, 2, "B")} (working set)");

                        if (m_processUtilizationCalculator.UpdateInterval > 0)
                            status.AppendLine($"       Process utilization: {m_processUtilizationCalculator.Utilization:##0.0%}");                            

                        status.AppendLine($"      Total processor time: {m_process.TotalProcessorTime.ToElapsedTimeString()}");
                        status.AppendLine($"            Total run-time: {(DateTime.Now - m_process.StartTime).ToElapsedTimeString()}");
                    }
                }
                catch
                {
                    status.AppendLine("             Process state: [unavailable]");
                }
            }

            status.AppendLine($"    Initial input filename: {FilePath.TrimFileName(InitialInputFileName, 51)}");
            status.AppendLine($" CPU usage update interval: {(m_processUtilizationCalculator.UpdateInterval > 0 ? $"{m_processUtilizationCalculator.UpdateInterval:N0}ms" : "Disabled")}");
            status.AppendLine($"     Input lines processed: {m_inputLinesProcessed:N0}");
            status.AppendLine($"    Output lines processed: {m_outputLinesProcessed:N0}");
            status.AppendLine($"     Error lines processed: {m_errorLinesProcessed:N0}");
            status.AppendLine($" Interpret output for logs: {ProcessOutputAsLogMessages}");
            status.AppendLine($"   Forcing kill on dispose: {ForceKillOnDispose}");
            status.AppendLine($"       Tracking statistics: {TrackProcessStatistics}");

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]
                
    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ProcessLauncher"/> object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        try
        {
            if (!disposing)
                return;

            TerminateProcess(true);

            m_process.Dispose();
            m_process.OutputDataReceived -= ProcessOutputDataReceived;
            m_process.ErrorDataReceived -= ProcessErrorDataReceived;

            m_processUtilizationCalculator.Dispose();
        }
        finally
        {
            m_disposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    /// <summary>
    /// Initializes <see cref="ProcessLauncher"/>.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        Dictionary<string, string> settings = Settings;

        ProcessStartInfo startInfo = m_process.StartInfo;

        // Load required parameters
        if (settings.TryGetValue(nameof(FileName), out string setting))
        {
            setting = FilePath.GetAbsolutePath(setting.Trim());

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
        m_supportsTemporalProcessing = settings.TryGetValue(nameof(SupportsTemporalProcessing), out setting) && setting.ParseBoolean();

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
        else
        {
            WorkingDirectory = FilePath.GetDirectoryName(FileName);
            startInfo.WorkingDirectory = WorkingDirectory;
        }

        if (settings.TryGetValue(nameof(EnvironmentalVariables), out setting))
        {
            foreach (KeyValuePair<string, string> item in setting.ParseKeyValuePairs())
                startInfo.Environment[item.Key] = item.Value;
        }

        // Note that it's possible that time-series framework is being hosted by an application
        // running in a Window making many of the following process start properties relevant.
        // Even when hosted as a service, the user may start the service logging on using the
        // local system account and select to allow the service interact with the desktop.
        startInfo.CreateNoWindow = !settings.TryGetValue(nameof(CreateNoWindow), out setting) || setting.ParseBoolean();

        if (settings.TryGetValue(nameof(WindowStyle), out setting) && Enum.TryParse(setting, true, out ProcessWindowStyle windowStyle))
            startInfo.WindowStyle = windowStyle;
        else
            startInfo.WindowStyle = (ProcessWindowStyle)Enum.Parse(typeof(ProcessWindowStyle), DefaultWindowStyle);

        startInfo.ErrorDialog = settings.TryGetValue(nameof(ErrorDialog), out setting) && setting.ParseBoolean();

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
            setting = FilePath.GetAbsolutePath(setting.Trim());

            if (File.Exists(setting))
                InitialInputFileName = setting;
            else
                throw new FileNotFoundException($"Cannot launch process: specified initial input filename \"{setting}\" does not exist.");
        }

        if (settings.TryGetValue(nameof(InitialInputProcessingDelay), out setting) && int.TryParse(setting, out int initialInputProcessingDelay) && initialInputProcessingDelay > -1)
            InitialInputProcessingDelay = initialInputProcessingDelay;

        if (settings.TryGetValue(nameof(RedirectOutputToHostEnvironment), out setting))
            RedirectOutputToHostEnvironment = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(RedirectErrorToHostEnvironment), out setting))
            RedirectErrorToHostEnvironment = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(UtilizationUpdateInterval), out setting) && int.TryParse(setting, out int utilizationCalculationInterval))
            UtilizationUpdateInterval = utilizationCalculationInterval;

        startInfo.RedirectStandardOutput = RedirectOutputToHostEnvironment;
        startInfo.RedirectStandardError = RedirectErrorToHostEnvironment;
        startInfo.RedirectStandardInput = true;
        startInfo.UseShellExecute = false;

        m_process.EnableRaisingEvents = true;
        m_process.OutputDataReceived += ProcessOutputDataReceived;
        m_process.ErrorDataReceived += ProcessErrorDataReceived;

        if (settings.TryGetValue(nameof(ProcessOutputAsLogMessages), out setting))
            ProcessOutputAsLogMessages = setting.ParseBoolean();

        if (ProcessOutputAsLogMessages)
        {
            if (settings.TryGetValue(nameof(LogMessageTextExpression), out setting) && setting.Length > 0)
                LogMessageTextExpression = setting;

            m_logMessageTextExpression = new Regex(LogMessageTextExpression, RegexOptions.Compiled);

            if (settings.TryGetValue(nameof(LogMessageLevelExpression), out setting) && setting.Length > 0)
                LogMessageLevelExpression = setting;

            m_logMessageLevelExpression = new Regex(LogMessageLevelExpression, RegexOptions.Compiled);

            if (settings.TryGetValue(nameof(LogMessageLevelMappings), out setting))
                LogMessageLevelMappings = setting;

            foreach (KeyValuePair<string, string> item in LogMessageLevelMappings.ParseKeyValuePairs())
            {
                if (Enum.TryParse(item.Value, true, out MessageLevel level))
                    m_messageLevelMap[item.Key] = level;
            }
        }

        if (settings.TryGetValue(nameof(ForceKillOnDispose), out setting))
            ForceKillOnDispose = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(TrackProcessStatistics), out setting))
            TrackProcessStatistics = setting.ParseBoolean();

        m_process.Start();

        if (ForceKillOnDispose)
            m_childProcessManager?.AddProcess(m_process);

        if (RedirectOutputToHostEnvironment)
            m_process.BeginOutputReadLine();

        if (RedirectErrorToHostEnvironment)
            m_process.BeginErrorReadLine();

        m_processUtilizationCalculator.UpdateInterval = UtilizationUpdateInterval;
        m_processUtilizationCalculator.Initialize(m_process);

        // Register launched process with the statistics engine
        if (TrackProcessStatistics)
            StatisticsEngine.Register(this, "Process", "PROC");

        if (string.IsNullOrEmpty(InitialInputFileName))
            return;

        // Send any defined initial input to launched application
        new Action(() =>
        {
            try
            {
                using StreamReader reader = File.OpenText(InitialInputFileName);

                while (reader.ReadLine() is {} line)
                    Input(line);
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed while sending text from \"{InitialInputFileName}\" to launched process standard input: {ex.Message}", ex));
            }
        })
        .DelayAndExecute(InitialInputProcessingDelay);
    }

    /// <summary>
    /// Gets a short one-line status of this <see cref="AdapterBase"/>.
    /// </summary>
    /// <param name="maxLength">Maximum number of available characters for display.</param>
    /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
    public override string GetShortStatus(int maxLength)
    {
        string filename = FilePath.GetFileName(FileName);

        if (!Initialized)
            return $"\"{filename}\" process has not started...".CenterText(maxLength);

        try
        {
            if (m_process.HasExited)
                return $"\"{filename}\" process exited with {m_process.ExitCode}.".CenterText(maxLength);

            // Ignoring this check on Mono since it seems to report incorrect state for Responding property
            // TODO: Will need to verify if this is still the case with .NET Core
            if (!Common.IsPosixEnvironment && !m_process.Responding)
                return $"\"{filename}\" process is not responding...".CenterText(maxLength);

            string utilization = m_processUtilizationCalculator.UpdateInterval > 0 ? $" at {m_processUtilizationCalculator.Utilization:##0.0%}" : "";
            return $"\"{filename}\" process running{utilization} for {(DateTime.Now - m_process.StartTime).ToElapsedTimeString()}.".CenterText(maxLength);
        }
        catch
        {
            return $"\"{filename}\" process state could not be determined...".CenterText(maxLength);
        }
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

    /// <summary>
    /// Stops the launched process.
    /// </summary>
    [AdapterCommand("Stops the launched process.")]
    public void Kill()
    {
        TerminateProcess(false);
    }

    private void TerminateProcess(bool disposing)
    {
        if (ForceKillOnDispose)
        {
            try
            {
                m_process.Kill();

                // Kill any child processes that were launched by the process by disposing of the child process manager
                m_childProcessManager?.Dispose();
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Exception encountered while attempting to stop process launched from \"{FileName}\": {ex.Message}", ex));
            }
        }
        else
        {
            if (!disposing)
                m_process.Kill();
        }
    }

    private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        MessageLevel level = MessageLevel.Info;
        string status = ProcessOutputAsLogMessages ? ParseLogMessage(e.Data, out level) : e.Data;

        if (string.IsNullOrEmpty(status))
            return;

        OnStatusMessage(level, status);
        m_outputLinesProcessed++;
    }

    private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        MessageLevel level = MessageLevel.Error;
        string status = ProcessOutputAsLogMessages ? ParseLogMessage(e.Data, out level) : e.Data;

        if (string.IsNullOrEmpty(status))
            return;

        OnStatusMessage(level, status);
        m_errorLinesProcessed++;
    }

    private string ParseLogMessage(string message, out MessageLevel level)
    {
        level = MessageLevel.NA;

        if (string.IsNullOrWhiteSpace(message))
            return "";

        // Parse log message level
        if (m_logMessageLevelExpression is not null)
        {
            Match match = m_logMessageLevelExpression.Match(message);

            if (match.Success)
            {
                if (!m_messageLevelMap.TryGetValue(match.Value, out level))
                    level = MessageLevel.NA;
            }
        }

        // Parse log message text
        if (m_logMessageTextExpression is not null)
        {
            MatchCollection matches = m_logMessageTextExpression.Matches(message);

            if (matches.Count > 0)
                return string.Join(" ", matches.Cast<Match>().Select(match => match.Value));
        }

        return message;
    }

    #endregion

    #region [ Static ]

    // Static Methods

    // ReSharper disable UnusedMember.Local
    // ReSharper disable UnusedParameter.Local

    private static double GetProcessStatistic_CPUUsage(object source, string arguments)
    {
        double statistic = double.NaN;

        if (source is ProcessLauncher { m_disposed: false, m_process.HasExited: false } launcher)
            statistic = launcher.m_processUtilizationCalculator.Utilization;

        return statistic;
    }

    private static double GetProcessStatistic_MemoryUsage(object source, string arguments)
    {
        double statistic = double.NaN;

        if (source is ProcessLauncher { m_disposed: false, m_process.HasExited: false } launcher)
            statistic = launcher.m_process.WorkingSet64 / (double)SI2.Mega;

        return statistic;
    }

    private static double GetProcessStatistic_UpTime(object source, string arguments)
    {
        double statistic = double.NaN;

        if (source is ProcessLauncher { m_disposed: false, m_process.HasExited: false } launcher)
            statistic = (DateTime.Now - launcher.m_process.StartTime).TotalSeconds;

        return statistic;
    }

    // ReSharper restore UnusedMember.Local
    // ReSharper restore UnusedParameter.Local

    #endregion
}