//******************************************************************************************************
//  MultipleDestinationExporter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/13/2008 - J. Ritchie Carroll
//       Initial version of source generated.
//  07/29/2008 - J. Ritchie Carroll
//       Added "Initialize" method to enable user to reconnect to network shares.
//       Added more descriptive status messages to provide more detailed user feedback.
//  09/19/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/22/2008 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  01/27/2011 - J. Ritchie Carroll
//       Modified internal operation to minimize risk of file dead lock and/or memory overload.
//  09/22/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GSF.Configuration;
using GSF.Threading;

namespace GSF.IO
{
    /// <summary>
    /// Handles the exporting of a file to multiple destinations that are defined in the config file.
    /// </summary>
    /// <remarks>
    /// This class is useful for updating the same file on multiple servers (e.g., load balanced web server).
    /// </remarks>
    /// <example>
    /// This example shows the use <see cref="MultipleDestinationExporter"/> for exporting data to multiple locations:
    /// <code>
    /// using System;
    /// using GSF.IO;
    ///
    /// class Program
    /// {
    ///     static MultipleDestinationExporter s_exporter;
    ///
    ///     static void Main(string[] args)
    ///     {
    ///         s_exporter = new MultipleDestinationExporter();
    ///         s_exporter.Initialized += s_exporter_Initialized;
    ///         ExportDestination[] defaultDestinations = new ExportDestination[] 
    ///         {
    ///             new ExportDestination(@"\\server1\share\exportFile.txt", false, "domain", "user1", "password1"), 
    ///             new ExportDestination(@"\\server2\share\exportFile.txt", false, "domain", "user2", "password2")
    ///         };
    ///         // Initialize with the destinations where data is to be exported.
    ///         s_exporter.Initialize(defaultDestinations);
    ///
    ///         Console.ReadLine();
    ///     }
    ///
    ///     static void s_exporter_Initialized(object sender, EventArgs e)
    ///     {
    ///         // Export data to all defined locations after initialization.
    ///         s_exporter.ExportData("TEST DATA");
    ///     }
    /// }
    /// </code>
    /// This example shows the config file entry that can be used to specify the <see cref="ExportDestination"/> 
    /// used by the <see cref="MultipleDestinationExporter"/> when exporting data:
    /// <code>
    /// <![CDATA[
    /// <exportDestinations>
    ///   <add name="ExportTimeout" value="-1" description="Total allowed time for all exports to execute in milliseconds."
    ///     encrypted="false" />
    ///   <add name="ExportCount" value="2" description="Total number of export files to produce."
    ///     encrypted="false" />
    ///   <add name="ExportDestination1" value="\\server1\share\" description="Root path for export destination. Use UNC path (\\server\share) with no trailing slash for network shares."
    ///     encrypted="false" />
    ///   <add name="ExportDestination1.ConnectToShare" value="True" description="Set to True to attempt authentication to network share."
    ///     encrypted="false" />
    ///   <add name="ExportDestination1.Domain" value="domain" description="Domain used for authentication to network share (computer name for local accounts)."
    ///     encrypted="false" />
    ///   <add name="ExportDestination1.UserName" value="user1" description="User name used for authentication to network share."
    ///     encrypted="false" />
    ///   <add name="ExportDestination1.Password" value="l2qlAwAPihJjoThH+G53BUxzYsIkTE2yNBHLtd1WA3hysDhwDB82ouJb9n35QtG8"
    ///     description="Encrypted password used for authentication to network share."
    ///     encrypted="true" />
    ///   <add name="ExportDestination1.FileName" value="exportFile.txt" description="Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\path\filename.txt)."
    ///     encrypted="false" />
    ///   <add name="ExportDestination2" value="\\server2\share\" description="Root path for export destination. Use UNC path (\\server\share) with no trailing slash for network shares."
    ///     encrypted="false" />
    ///   <add name="ExportDestination2.ConnectToShare" value="True" description="Set to True to attempt authentication to network share."
    ///     encrypted="false" />
    ///   <add name="ExportDestination2.Domain" value="domain" description="Domain used for authentication to network share (computer name for local accounts)."
    ///     encrypted="false" />
    ///   <add name="ExportDestination2.UserName" value="user2" description="User name used for authentication to network share."
    ///     encrypted="false" />
    ///   <add name="ExportDestination2.Password" value="l2qlAwAPihJjoThH+G53BYT6BXHQr13D6Asdibl0rDmlrgRXvJmCwcP8uvkFRHr9"
    ///     description="Encrypted password used for authentication to network share."
    ///     encrypted="true" />
    ///   <add name="ExportDestination2.FileName" value="exportFile.txt" description="Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\path\filename.txt)."
    ///     encrypted="false" />
    /// </exportDestinations>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ExportDestination"/>
    [ToolboxBitmap(typeof(MultipleDestinationExporter))]
    public class MultipleDestinationExporter : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Defines state information for an export.
        /// </summary>
        private sealed class ExportState : IDisposable
        {
            #region [ Members ]

            // Fields
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new <see cref="ExportState"/>.
            /// </summary>
            public ExportState()
            {
                WaitHandle = new ManualResetEventSlim(false);
            }

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="ExportState"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~ExportState()
            {
                Dispose(false);
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets the source file name for the <see cref="ExportState"/>.
            /// </summary>
            public string SourceFileName
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the destination file name for the <see cref="ExportState"/>.
            /// </summary>
            public string DestinationFileName
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the event wait handle for the <see cref="ExportState"/>.
            /// </summary>
            public ManualResetEventSlim WaitHandle
            {
                get;
            }

            /// <summary>
            /// Gets or sets a flag that is used to determine if export process has timed out.
            /// </summary>
            public bool Timeout
            {
                get;
                set;
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases all the resources used by the <see cref="ExportState"/> object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="ExportState"/> object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<WaitHandle>k__BackingField", Justification = "FxCop needs to do a better job recognizing null propagation :-p")]
            private void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                            WaitHandle?.Dispose();
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.
                    }
                }
            }

            #endregion
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="ExportTimeout"/> property.
        /// </summary>
        public const int DefaultExportTimeout = Timeout.Infinite;

        /// <summary>
        /// Specifies the default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = true;

        /// <summary>
        /// Specifies the default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ExportDestinations";

        /// <summary>
        /// Specifies the default value for the <see cref="MaximumRetryAttempts"/> property.
        /// </summary>
        public const int DefaultMaximumRetryAttempts = 4; // That is 4 retries plus the original attempt for a total of 5 attempts

        /// <summary>
        /// Specifies the default value for the <see cref="RetryDelayInterval"/> property.
        /// </summary>
        public const int DefaultRetryDelayInterval = 1000;

        // Events

        /// <summary>
        /// Occurs when the <see cref="MultipleDestinationExporter"/> object has been initialized.
        /// </summary>
        [Description("Occurs when the MultipleDestinationExporter object has been initialized.")]
        public event EventHandler Initialized;

        /// <summary>
        /// Occurs when status information for the <see cref="MultipleDestinationExporter"/> object is being reported.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the status message being reported by the <see cref="MultipleDestinationExporter"/>.
        /// </remarks>
        [Description("Occurs when status information for the MultipleDestinationExporter object is being reported.")]
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        [Description("Occurs when an exception occurs in the MultipleDestinationExporter.")]
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private int m_exportTimeout;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private long m_totalExports;
        private long m_failedExportAttempts;
        private volatile byte[] m_fileData;
        private Encoding m_textEncoding;
        private List<ExportDestination> m_exportDestinations;
        private readonly object m_exportDestinationsLock;
        private readonly LongSynchronizedOperation m_exportOperation;
        private int m_maximumRetryAttempts;
        private int m_retryDelayInterval;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleDestinationExporter"/> class.
        /// </summary>
        public MultipleDestinationExporter()
            : this(DefaultSettingsCategory, DefaultExportTimeout)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleDestinationExporter"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="MultipleDestinationExporter"/>.</param>
        public MultipleDestinationExporter(IContainer container)
            : this()
        {
            if ((object)container != null)
                container.Add(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleDestinationExporter"/> class.
        /// </summary>
        /// <param name="settingsCategory">The config file settings category under which the export destinations are defined.</param>
        /// <param name="exportTimeout">The total allowed time in milliseconds for each export to execute.</param>
        public MultipleDestinationExporter(string settingsCategory, int exportTimeout)
        {
            m_exportTimeout = exportTimeout;
            m_settingsCategory = settingsCategory;
            m_persistSettings = DefaultPersistSettings;
            m_maximumRetryAttempts = DefaultMaximumRetryAttempts;
            m_retryDelayInterval = DefaultRetryDelayInterval;
            m_textEncoding = Encoding.Default; // We use default ANSI page encoding for text based exports...
            m_exportDestinationsLock = new object();
            m_exportOperation = new LongSynchronizedOperation(ExecuteExports, OnProcessException)
            {
                IsBackground = true
            };            
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the total allowed time in milliseconds for each export to execute.
        /// </summary>
        /// <remarks>
        /// Set to Timeout.Infinite (-1) for no timeout.
        /// </remarks>
        [Category("Settings"), DefaultValue(DefaultExportTimeout), Description("Total allowed time for each export to execute, in milliseconds. Set to -1 for no specific timeout.")]
        public int ExportTimeout
        {
            get
            {
                return m_exportTimeout;
            }
            set
            {
                m_exportTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of retries that will be attempted during an export if the export fails.
        /// </summary>
        /// <remarks>
        /// Total file export attempts = 1 + <see cref="MaximumRetryAttempts"/>. Set to zero to only attempt export once.
        /// </remarks>
        [Category("Settings"), DefaultValue(DefaultMaximumRetryAttempts), Description("Maximum number of retries that will be attempted during an export if the export fails. Set to zero to only attempt export once.")]
        public int MaximumRetryAttempts
        {
            get
            {
                return m_maximumRetryAttempts;
            }
            set
            {
                m_maximumRetryAttempts = value;

                if (m_maximumRetryAttempts < 0)
                    m_maximumRetryAttempts = 0;
            }
        }

        /// <summary>
        /// Gets or sets the interval to wait, in milliseconds, before retrying an export if the export fails.
        /// </summary>
        [Category("Settings"), DefaultValue(DefaultRetryDelayInterval), Description("Interval to wait, in milliseconds, before retrying an export if the export fails.")]
        public int RetryDelayInterval
        {
            get
            {
                return m_retryDelayInterval;
            }
            set
            {
                m_retryDelayInterval = value;

                if (m_retryDelayInterval <= 0)
                    m_retryDelayInterval = DefaultRetryDelayInterval;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="MultipleDestinationExporter"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistence"), DefaultValue(DefaultPersistSettings), Description("Indicates whether the settings of MultipleDestinationExporter object are to be saved to the config file.")]
        public bool PersistSettings
        {
            get
            {
                return m_persistSettings;
            }
            set
            {
                m_persistSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the category under which the settings of <see cref="MultipleDestinationExporter"/> object are to be saved
        /// to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is null or empty string.</exception>
        [Category("Persistence"), DefaultValue(DefaultSettingsCategory), Description("Category under which the settings of MultipleDestinationExporter object are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used to encode text data being exported.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get
            {
                return m_textEncoding;
            }
            set
            {
                if ((object)value == null)
                {
                    m_textEncoding = Encoding.Default;
                }
                else
                {
                    m_textEncoding = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="MultipleDestinationExporter"/> object is currently enabled.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the object has been disposed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed => m_disposed;

        /// <summary>
        /// Gets the total number exports performed successfully.
        /// </summary>
        [Browsable(false)]
        public long TotalExports => m_totalExports;

        /// <summary>
        /// Gets a list of currently defined <see cref="ExportDestination"/>.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Initialize(IEnumerable{ExportDestination})"/> method to change the export destination collection.
        /// </remarks>
        [Category("Settings"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Description("Gets a list of all the defined export destinations to be used by the MultipleDestinationExporter.")]
        public ReadOnlyCollection<ExportDestination> ExportDestinations
        {
            get
            {
                lock (m_exportDestinationsLock)
                {
                    if ((object)m_exportDestinations != null)
                        return new ReadOnlyCollection<ExportDestination>(m_exportDestinations);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="MultipleDestinationExporter"/> object.
        /// </summary>
        [Browsable(false)]
        public string Name => m_settingsCategory;

        /// <summary>
        /// Gets the descriptive status of the <see cref="MultipleDestinationExporter"/> object.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("     Configuration section: ");
                status.Append(m_settingsCategory);
                status.AppendLine();
                status.Append("            Export enabled: ");
                status.Append(m_enabled);
                status.AppendLine();
                status.Append("       Temporary file path: ");
                status.Append(FilePath.TrimFileName(Path.GetTempPath(), 51));
                status.AppendLine();
                status.AppendLine("       Export destinations: ");
                status.AppendLine();

                lock (m_exportDestinationsLock)
                {
                    int count = 1;

                    foreach (ExportDestination export in m_exportDestinations)
                    {
                        status.AppendFormat("         {0}: {1}\r\n", count.ToString().PadLeft(2, '0'), FilePath.TrimFileName(export.DestinationFile, 65));
                        count++;
                    }
                }

                status.AppendLine();
                status.Append("       File export timeout: ");
                status.Append(m_exportTimeout == Timeout.Infinite ? "Infinite" : m_exportTimeout + " milliseconds");
                status.AppendLine();
                status.Append("    Maximum retry attempts: ");
                status.Append(m_maximumRetryAttempts);
                status.AppendLine();
                status.Append("      Retry delay interval: ");
                status.Append(m_retryDelayInterval.ToString() + " milliseconds");
                status.AppendLine();
                status.Append("    Failed export attempts: ");
                status.Append(m_failedExportAttempts);
                status.AppendLine();
                status.Append("      Total exports so far: ");
                status.Append(m_totalExports);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MultipleDestinationExporter"/> object and optionally releases the managed resources.
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
                        // This will be done only when the object is disposed by calling Dispose().
                        Shutdown();
                        SaveSettings();
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
        /// Performs necessary operations before the <see cref="MultipleDestinationExporter"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="MultipleDestinationExporter"/> object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            if (!DesignMode)
            {
                try
                {
                    // Nothing needs to be done before component is initialized.
                }
                catch
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Performs necessary operations after the <see cref="MultipleDestinationExporter"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="EndInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="MultipleDestinationExporter"/> object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void EndInit()
        {
            if (!DesignMode)
            {
                try
                {
                    Initialize();
                }
                catch
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="MultipleDestinationExporter"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                settings.Clear();
                settings["ExportTimeout", true].Update(m_exportTimeout, "Total allowed time for each export to execute, in milliseconds. Set to -1 for no specific timeout.");
                settings["MaximumRetryAttempts", true].Update(m_maximumRetryAttempts, "Maximum number of retries that will be attempted during an export if the export fails. Set to zero to only attempt export once.");
                settings["RetryDelayInterval", true].Update(m_retryDelayInterval, "Interval to wait, in milliseconds, before retrying an export if the export fails.");

                lock (m_exportDestinationsLock)
                {
                    settings["ExportCount", true].Update(m_exportDestinations.Count, "Total number of export files to produce.");
                    for (int x = 0; x < m_exportDestinations.Count; x++)
                    {
                        settings[$"ExportDestination{x + 1}", true].Update(m_exportDestinations[x].Share, "Root path for export destination. Use UNC path (\\\\server\\share) with no trailing slash for network shares.");
                        settings[$"ExportDestination{x + 1}.ConnectToShare", true].Update(m_exportDestinations[x].ConnectToShare, "Set to True to attempt authentication to network share.");
                        settings[$"ExportDestination{x + 1}.Domain", true].Update(m_exportDestinations[x].Domain, "Domain used for authentication to network share (computer name for local accounts).");
                        settings[$"ExportDestination{x + 1}.UserName", true].Update(m_exportDestinations[x].UserName, "User name used for authentication to network share.");
                        settings[$"ExportDestination{x + 1}.Password", true].Update(m_exportDestinations[x].Password, "Encrypted password used for authentication to network share.", true);
                        settings[$"ExportDestination{x + 1}.FileName", true].Update(m_exportDestinations[x].FileName, "Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\\path\\filename.txt).");
                    }
                }

                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="MultipleDestinationExporter"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new ConfigurationErrorsException("SettingsCategory property has not been set");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                if (settings.Count == 0)
                    return;    // Don't proceed if export destinations don't exist in config file.

                m_exportTimeout = settings["ExportTimeout", true].ValueAs(m_exportTimeout);
                m_maximumRetryAttempts = settings["MaximumRetryAttempts", true].ValueAs(m_maximumRetryAttempts);
                m_retryDelayInterval = settings["RetryDelayInterval", true].ValueAs(m_retryDelayInterval);
                int count = settings["ExportCount", true].ValueAsInt32();

                lock (m_exportDestinationsLock)
                {
                    m_exportDestinations = new List<ExportDestination>(count);

                    for (int x = 0; x < count; x++)
                    {
                        string entryRoot = $"ExportDestination{x + 1}";

                        // Load export destination from configuration entries
                        ExportDestination destination = new ExportDestination();

                        destination.DestinationFile = settings[entryRoot, true].ValueAsString() + settings[$"{entryRoot}.FileName", true].ValueAsString();
                        destination.ConnectToShare = settings[$"{entryRoot}.ConnectToShare", true].ValueAsBoolean();
                        destination.Domain = settings[$"{entryRoot}.Domain", true].ValueAsString();
                        destination.UserName = settings[$"{entryRoot}.UserName", true].ValueAsString();
                        destination.Password = settings[$"{entryRoot}.Password", true].ValueAsString();

                        // Save new export destination if destination file name has been defined and is valid
                        if (FilePath.IsValidFileName(destination.DestinationFile))
                            m_exportDestinations.Add(destination);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes (or reinitializes) <see cref="MultipleDestinationExporter"/> from configuration settings.
        /// </summary>
        /// <remarks>
        /// If not being used as a component (i.e., user creates their own instance of this class), this method
        /// must be called in order to initialize exports.  Event if used as a component this method can be
        /// called at anytime to reinitialize the exports with new configuration information.
        /// </remarks>
        public void Initialize()
        {
            // We provide a simple default set of export destinations since no others are specified.
            Initialize(new[] { new ExportDestination(Common.IsPosixEnvironment ? "/usr/share/filename.txt" : "C:\\filename.txt", false, "domain", "username", "password") });
        }

        /// <summary>
        /// Initializes (or reinitializes) <see cref="MultipleDestinationExporter"/> from configuration settings.
        /// </summary>
        /// <param name="defaultDestinations">Provides a default set of export destinations if none exist in configuration settings.</param>
        /// <remarks>
        /// If not being used as a component (i.e., user creates their own instance of this class), this method
        /// must be called in order to initialize exports.  Even if used as a component this method can be
        /// called at anytime to reinitialize the exports with new configuration information.
        /// </remarks>
        public void Initialize(IEnumerable<ExportDestination> defaultDestinations)
        {
            // So as to not delay calling thread due to share authentication, we perform initialization on another thread...
            Thread initializeThread = new Thread(InitializeExporter)
            {
                IsBackground = true
            };

            initializeThread.Start(defaultDestinations.ToList());
        }

        private void InitializeExporter(object state)
        {
            // In case we are reinitializing class, we shutdown any prior queue operations and close any existing network connections...
            Shutdown();

            // Retrieve any specified default export destinations
            lock (m_exportDestinationsLock)
            {
                m_exportDestinations = state as List<ExportDestination>;

                if ((object)m_exportDestinations == null)
                    m_exportDestinations = new List<ExportDestination>();
            }

            // Load export destinations from the config file - if nothing is in config file yet,
            // the default settings (passed in via state) will be used instead. Consumers
            // wishing to dynamically change export settings in code will need to make sure
            // PersistSettings is false in order to load specified code settings instead of
            // those that may be saved in the configuration file
            LoadSettings();

            ExportDestination[] destinations;

            lock (m_exportDestinationsLock)
            {
                // Cache a local copy of export destinations to reduce lock time,
                // network share authentication may take some time
                destinations = m_exportDestinations.ToArray();
            }

            for (int x = 0; x < destinations.Length; x++)
            {
                // Connect to network shares if necessary
                if (destinations[x].ConnectToShare)
                {
                    if (Common.IsPosixEnvironment)
                    {
                        OnStatusMessage("Network share authentication not available under POSIX environment...");
                    }
                    else
                    {
                        // Attempt connection to external network share
                        try
                        {
                            OnStatusMessage("Attempting network share authentication for user {0}\\{1} to {2}...", destinations[x].Domain, destinations[x].UserName, destinations[x].Share);

                            FilePath.ConnectToNetworkShare(destinations[x].Share, destinations[x].UserName, destinations[x].Password, destinations[x].Domain);

                            OnStatusMessage("Network share authentication to {0} succeeded.", destinations[x].Share);
                        }
                        catch (Exception ex)
                        {
                            // Something unexpected happened during attempt to connect to network share - so we'll report it...
                            OnProcessException(new IOException($"Network share authentication to {destinations[x].Share} failed due to exception: {ex.Message}", ex));
                        }
                    }
                }
            }

            m_enabled = true;

            // Notify that initialization is complete.
            OnInitialized();
        }

        // This is all of the needed dispose functionality, but since the class can be re-initialized this is a separate method
        private void Shutdown()
        {
            m_enabled = false;

            lock (m_exportDestinationsLock)
            {
                if ((object)m_exportDestinations != null)
                {
                    // We'll be nice and disconnect network shares when this class is disposed...
                    for (int x = 0; x < m_exportDestinations.Count; x++)
                    {
                        if (m_exportDestinations[x].ConnectToShare && !Common.IsPosixEnvironment)
                        {
                            try
                            {
                                FilePath.DisconnectFromNetworkShare(m_exportDestinations[x].Share);
                            }
                            catch (Exception ex)
                            {
                                // Something unexpected happened during attempt to disconnect from network share - so we'll report it...
                                OnProcessException(new IOException($"Network share disconnect from {m_exportDestinations[x].Share} failed due to exception: {ex.Message}", ex));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="Initialized"/> event.
        /// </summary>
        protected virtual void OnInitialized()
        {
            if ((object)Initialized != null)
                Initialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">Status message to report.</param>
        /// <param name="args"><see cref="string.Format(string,object[])"/> parameters used for status message.</param>
        protected virtual void OnStatusMessage(string status, params object[] args)
        {
            if ((object)StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(string.Format(status, args)));
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Start multiple file export.
        /// </summary>
        /// <param name="fileData">Text based data to export to each destination.</param>
        /// <remarks>
        /// This is assumed to be the full content of the file to export. This class does not queue data since
        /// the export is not intended to append to an existing file but rather replace an existing one.
        /// </remarks>
        public void ExportData(string fileData)
        {
            ExportData(m_textEncoding.GetBytes(fileData));
        }

        /// <summary>
        /// Start multiple file export.
        /// </summary>
        /// <param name="fileData">Binary data to export to each destination.</param>
        /// <remarks>
        /// This is assumed to be the full content of the file to export. This class does not queue data since
        /// the export is not intended to append to an existing file but rather replace an existing one.
        /// </remarks>
        public void ExportData(byte[] fileData)
        {
            if (m_enabled)
            {
                // Ensure that only one export will be queued and exporting at once
                m_fileData = fileData;
                m_exportOperation.RunOnceAsync();
            }
            else
            {
                throw new InvalidOperationException("Export failed: exporter is not currently enabled.");
            }
        }       

        private void ExecuteExports()
        {
            byte[] fileData = m_fileData;

            if (m_enabled && (object)fileData != null && m_exportDestinations.Count > 0)
            {
                string fileName = null;
                ExportState[] exportStates = null;
                ExportDestination[] destinations;

                try
                {
                    //  Get a temporary file name
                    fileName = Path.GetTempFileName();

                    // Export data to the temporary file
                    File.WriteAllBytes(fileName, fileData);

                    lock (m_exportDestinationsLock)
                    {
                        // Cache a local copy of export destinations to reduce lock time
                        destinations = m_exportDestinations.ToArray();
                    }

                    // Define a new export state for each export destination
                    exportStates = new ExportState[destinations.Length];

                    for (int i = 0; i < exportStates.Length; i++)
                    {
                        exportStates[i] = new ExportState
                        {
                            SourceFileName = fileName,
                            DestinationFileName = destinations[i].DestinationFile
                        };
                    }

                    // Spool threads to attempt copy of export files
                    for (int i = 0; i < destinations.Length; i++)
                        ThreadPool.QueueUserWorkItem(CopyFileToDestination, exportStates[i]);

                    // Wait for exports to complete - even if user specifies to wait indefinitely spooled copy routines
                    // will eventually return since there is a specified maximum retry count
                    if (!exportStates.Select(exportState => exportState.WaitHandle).WaitAll(m_exportTimeout))
                    {
                        // Exports failed to complete in specified allowed time, set timeout flag for each export state
                        Array.ForEach(exportStates, exportState => exportState.Timeout = true);
                        OnStatusMessage("Timed out attempting export, waited for {0}.", Ticks.FromMilliseconds(m_exportTimeout).ToElapsedTimeString(2).ToLower());
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException($"Exception encountered during export preparation: {ex.Message}", ex));
                }
                finally
                {
                    // Dispose the export state wait handles
                    if ((object)exportStates != null)
                    {
                        foreach (ExportState exportState in exportStates)
                            exportState.Dispose();
                    }

                    // Delete the temporary file - wait for the specified retry time in case the export threads may still be trying
                    // their last copy attempt. This is important if the timeouts are synchronized and there is one more export
                    // about to be attempted before the timeout flag is checked.
                    new Action(() => DeleteTemporaryFile(fileName)).DelayAndExecute(m_retryDelayInterval);
                }
            }
        }

        [SuppressMessage("Gendarme.Rules.Exceptions", "DoNotDestroyStackTraceRule")]
        private void CopyFileToDestination(object state)
        {
            ExportState exportState = null;
            Exception exportException = null;
            int failedExportCount = 0;

            try
            {
                exportState = state as ExportState;

                if ((object)exportState != null)
                {
                    // File copy may fail if destination is locked, so we setup to retry this operation
                    // waiting the specified period between attempts
                    for (int attempt = 0; attempt < 1 + m_maximumRetryAttempts; attempt++)
                    {
                        try
                        {
                            // Attempt to copy file to destination, overwriting if it already exists
                            File.Copy(exportState.SourceFileName, exportState.DestinationFileName, true);
                        }
                        catch (Exception ex)
                        {
                            // Stack exception history to provide a full inner exception failure log for each export attempt
                            if ((object)exportException == null)
                                exportException = ex;
                            else
                                exportException = new IOException($"Attempt {attempt + 1} exception: {ex.Message}", exportException);

                            failedExportCount++;

                            // Abort retry attempts if export has timed out or maximum exports have been attempted
                            if (!m_enabled || exportState.Timeout || attempt >= m_maximumRetryAttempts)
                                throw exportException;

                            Thread.Sleep(m_retryDelayInterval);
                        }
                    }

                    // Track successful exports
                    m_totalExports++;
                }
            }
            catch (Exception ex)
            {
                string destinationFileName = null;
                bool timeout = false;

                if ((object)exportState != null)
                {
                    destinationFileName = exportState.DestinationFileName;
                    timeout = exportState.Timeout;
                }

                OnProcessException(new InvalidOperationException($"Export attempt aborted {(timeout ? "due to timeout with" : "after")} {failedExportCount} exception{(failedExportCount > 1 ? "s" : "")} for \"{destinationFileName.ToNonNullString("[undefined]")}\" - {ex.Message}", ex));
            }
            finally
            {
                // Release waiting thread
                exportState?.WaitHandle?.Set();

                // Track total number of failed export attempts
                Interlocked.Add(ref m_failedExportAttempts, failedExportCount);
            }
        }

        private void DeleteTemporaryFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            try
            {
                // Delete the temporary file
                if (File.Exists(filename))
                    File.Delete(filename);
            }
            catch (Exception ex)
            {
                // Although errors are not expected from deleting the temporary file, we report any that may occur
                OnProcessException(new InvalidOperationException($"Exception encountered while trying to remove temporary file: {ex.Message}", ex));
            }
        }

        #endregion
    }
}