//*******************************************************************************************************
//  MultipleDestinationExporter.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/13/2008 - James R. Carroll
//       Initial version of source generated
//  07/29/2008 - James R. Carroll
//       Added "Initialize" method to enable user to reconnect to network shares.
//       Added more descriptive status messages to provide more detailed user feedback.
//  09/19/2008 - James R. Carroll
//       Converted to C#.
//  10/22/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TVA.Collections;
using TVA.Configuration;

namespace TVA.IO
{
    /// <summary>
    /// Handles the exporting of a file to multiple destinations that are defined in the config file.
    /// </summary>
    /// <remarks>
    /// This class is useful for updating the same file on multiple servers (e.g., load balanced web server).
    /// </remarks>
    /// <seealso cref="ExportDestination"/>
    /// <example>
    /// This example shows the use <see cref="MultipleDestinationExporter"/> for exporting data to multiple locations:
    /// <code>
    /// using System;
    /// using TVA.IO;
    ///
    /// class Program
    /// {
    ///     static MultipleDestinationExporter m_exporter;
    ///
    ///     static void Main(string[] args)
    ///     {
    ///         m_exporter = new MultipleDestinationExporter();
    ///         m_exporter.Initialized += m_exporter_Initialized;
    ///         ExportDestination[] defaultDestinations = new ExportDestination[] 
    ///         {
    ///             new ExportDestination(@"\\server1\share\exportFile.txt", false, "domain", "user1", "password1"), 
    ///             new ExportDestination(@"\\server2\share\exportFile.txt", false, "domain", "user2", "password2")
    ///         };
    ///         // Initialize with the destinations where data is to be exported.
    ///         m_exporter.Initialize(defaultDestinations);
    ///
    ///         Console.ReadLine();
    ///     }
    ///
    ///     static void m_exporter_Initialized(object sender, EventArgs e)
    ///     {
    ///         // Export data to all defined locations after initialization.
    ///         m_exporter.ExportData("TEST DATA");
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
    [ToolboxBitmap(typeof(MultipleDestinationExporter))]
    public class MultipleDestinationExporter : Component, ISupportLifecycle, ISupportInitialize, IProvideStatus, IPersistSettings
    {
        #region [ Members ]

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

        // Fields

        private int m_exportTimeout;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private long m_totalExports;
        private Encoding m_textEncoding;
        private List<ExportDestination> m_exportDestinations;
        private ProcessQueue<byte[]> m_exportQueue;
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
            if (container != null)
                container.Add(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleDestinationExporter"/> class.
        /// </summary>
        /// <param name="settingsCategory">The config file settings category under which the export destinations are defined.</param>
        /// <param name="exportTimeout">The total allowed time in milliseconds for all exports to execute.</param>
        public MultipleDestinationExporter(string settingsCategory, int exportTimeout)
            : base()
        {
            m_exportTimeout = exportTimeout;
            m_settingsCategory = settingsCategory;
            m_persistSettings = DefaultPersistSettings;
            m_textEncoding = Encoding.Default; // We use default ANSI page encoding for text based exports...
            m_exportDestinations = new List<ExportDestination>();

            // Set up a synchronous process queue to handle exports that will limit total export time to export interval
            m_exportQueue = ProcessQueue<byte[]>.CreateSynchronousQueue(WriteExportFiles, 10, m_exportTimeout, false, false);
            m_exportQueue.ProcessException += ProcessExceptionHandler;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the total allowed time in milliseconds for all exports to execute.
        /// </summary>
        /// <remarks>
        /// Set to Timeout.Infinite (-1) for no timeout.
        /// </remarks>
        [Category("Settings"),
        DefaultValue(DefaultExportTimeout),
        Description("Total allowed time in milliseconds for all exports to execute.")]
        public int ExportTimeout
        {
            get
            {
                return m_exportTimeout;
            }
            set
            {
                m_exportTimeout = value;
                if (m_exportQueue != null)
                    m_exportQueue.ProcessTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the settings of <see cref="MultipleDestinationExporter"/> object are 
        /// to be saved to the config file.
        /// </summary>
        [Category("Persistance"),
        DefaultValue(DefaultPersistSettings),
        Description("Indicates whether the settings of MultipleDestinationExporter object are to be saved to the config file.")]
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
        [Category("Persistance"),
        DefaultValue(DefaultSettingsCategory),
        Description("Category under which the settings of MultipleDestinationExporter object are to be saved to the config file if the PersistSettings property is set to true.")]
        public string SettingsCategory
        {
            get
            {
                return m_settingsCategory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw (new ArgumentNullException("value"));

                m_settingsCategory = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> to be used to encode text data being exported.
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Encoding TextEncoding
        {
            get
            {
                return m_textEncoding;
            }
            set
            {
                if (value == null)
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
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Enabled
        {
            get
            {
                return m_exportQueue.Enabled;
            }
            set
            {
                m_exportQueue.Enabled = value;
            }
        }

        /// <summary>
        /// Gets the total number exports performed successfully.
        /// </summary>
        [Browsable(false)]
        public long TotalExports
        {
            get
            {
                return m_totalExports;
            }
        }

        /// <summary>
        /// Gets a list of currently defined <see cref="ExportDestination"/>.
        /// </summary>
        /// <remarks>
        /// Thread-safety Warning: Due to the asynchronous nature of <see cref="MultipleDestinationExporter"/>, a lock must be 
        /// obtained on <see cref="ExportDestinations"/> before accessing it.
        /// </remarks>
        [Category("Settings"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Description("Gets a list of all the defined export destinations to be used by the MultipleDestinationExporter.")]
        public List<ExportDestination> ExportDestinations
        {
            get
            {
                return m_exportDestinations;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="MultipleDestinationExporter"/> object.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                // We just return the settings category name for unique identification of this component
                return m_settingsCategory;
            }
        }

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
                status.Append("       Export destinations: ");
                lock (m_exportDestinations)
                {
                    status.Append(m_exportDestinations.ToDelimitedString(','));
                }
                status.AppendLine();
                status.Append(" Cumulative export timeout: ");
                status.Append(m_exportTimeout == Timeout.Infinite ? "Infinite" : m_exportTimeout + " milliseconds");
                status.AppendLine();
                status.Append("      Total exports so far: ");
                status.Append(m_totalExports);
                status.AppendLine();

                if (m_exportQueue != null)
                    status.Append(m_exportQueue.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes (or reinitializes) <see cref="MultipleDestinationExporter"/> from configuration settings.
        /// </summary>
        /// <remarks>
        /// If not being used as a component (i.e., user creates their own instance of this class), this method
        /// must be called in order to initialize exports.  Event if used as a component this method can be
        /// called at anytime to reintialize the exports with new configuration information.
        /// </remarks>
        public void Initialize()
        {
            // We provide a simple default set of export destinations since no others are specified.
            Initialize(new ExportDestination[] { new ExportDestination("C:\\filename.txt", false, "domain", "username", "password") });
        }

        /// <summary>
        /// Initializes (or reinitializes) <see cref="MultipleDestinationExporter"/> from configuration settings.
        /// </summary>
        /// <param name="defaultDestinations">Provides a default set of export destinations if none exist in configuration settings.</param>
        /// <remarks>
        /// If not being used as a component (i.e., user creates their own instance of this class), this method
        /// must be called in order to initialize exports.  Even if used as a component this method can be
        /// called at anytime to reintialize the exports with new configuration information.
        /// </remarks>
        public void Initialize(IEnumerable<ExportDestination> defaultDestinations)
        {
            // So as to not delay calling thread due to share authentication, we perform initialization on another thread...
#if ThreadTracking
            ManagedThread thread = ManagedThreadPool.QueueUserWorkItem(Initialize, defaultDestinations.ToList());
            thread.Name = "TVA.IO.MultipleDestinationExporter.Initialize()";
#else
            ThreadPool.QueueUserWorkItem(Initialize, defaultDestinations.ToList());
#endif
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
            try
            {
                // Nothing needs to be done before component is initialized.
            }
            catch (Exception)
            {
                // Prevent the IDE from crashing when component is in design mode.
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
                catch (Exception)
                {
                    // Prevent the IDE from crashing when component is in design mode.
                }
            }
        }

        /// <summary>
        /// Saves settings for the <see cref="MultipleDestinationExporter"/> object to the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];
                settings.Clear();
                settings["ExportTimeout", true].Update(m_exportTimeout, "Total allowed time for all exports to execute in milliseconds.");
                lock (m_exportDestinations)
                {
                    settings["ExportCount", true].Update(m_exportDestinations.Count, "Total number of export files to produce.");
                    for (int x = 0; x < m_exportDestinations.Count; x++)
                    {
                        settings[string.Format("ExportDestination{0}", x + 1), true].Update(m_exportDestinations[x].Share, "Root path for export destination. Use UNC path (\\\\server\\share) with no trailing slash for network shares.");
                        settings[string.Format("ExportDestination{0}.ConnectToShare", x + 1), true].Update(m_exportDestinations[x].ConnectToShare, "Set to True to attempt authentication to network share.");
                        settings[string.Format("ExportDestination{0}.Domain", x + 1), true].Update(m_exportDestinations[x].Domain, "Domain used for authentication to network share (computer name for local accounts).");
                        settings[string.Format("ExportDestination{0}.UserName", x + 1), true].Update(m_exportDestinations[x].UserName, "User name used for authentication to network share.");
                        settings[string.Format("ExportDestination{0}.Password", x + 1), true].Update(m_exportDestinations[x].Password, "Encrypted password used for authentication to network share.", true);
                        settings[string.Format("ExportDestination{0}.FileName", x + 1), true].Update(m_exportDestinations[x].FileName, "Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\\path\\filename.txt).");
                    }
                }
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved settings for the <see cref="MultipleDestinationExporter"/> object from the config file if the <see cref="PersistSettings"/> 
        /// property is set to true.
        /// </summary>        
        public void LoadSettings()
        {
            if (m_persistSettings)
            {
                // Ensure that settings category is specified.
                if (string.IsNullOrEmpty(m_settingsCategory))
                    throw new InvalidOperationException("SettingsCategory property has not been set.");

                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                if (settings.Count == 0) return;    // Don't proceed if export destinations don't exist in config file.

                string entryRoot;
                int count;

                ExportDestination destination;
                m_exportTimeout = settings["ExportTimeout", true].ValueAs(m_exportTimeout);
                count = settings["ExportCount", true].ValueAsInt32();
                m_exportDestinations = new List<ExportDestination>(count);

                lock (m_exportDestinations)
                {
                    for (int x = 0; x < count; x++)
                    {
                        entryRoot = string.Format("ExportDestination{0}", x + 1);

                        // Load export destination from configuration entries
                        destination = new ExportDestination();
                        destination.DestinationFile = settings[entryRoot, true].ValueAsString() + settings[string.Format("{0}.FileName", entryRoot), true].ValueAsString();
                        destination.ConnectToShare = settings[string.Format("{0}.ConnectToShare", entryRoot), true].ValueAsBoolean();
                        destination.Domain = settings[string.Format("{0}.Domain", entryRoot), true].ValueAsString();
                        destination.UserName = settings[string.Format("{0}.UserName", entryRoot), true].ValueAsString();
                        destination.Password = settings[string.Format("{0}.Password", entryRoot), true].ValueAsString();

                        // Save new export destination
                        m_exportDestinations.Add(destination);
                    }
                }
            }
        }

        /// <summary>
        /// Start multiple file export.
        /// </summary>
        /// <param name="fileData">Text based data to export to each destination.</param>
        public void ExportData(string fileData)
        {
            // Queue data for export - multiple exports may take some time, so we do this on another thread...
            if (m_exportQueue != null)
                m_exportQueue.Add(m_textEncoding.GetBytes(fileData));
        }

        /// <summary>
        /// Start multiple file export.
        /// </summary>
        /// <param name="fileData">Binary data to export to each destination.</param>
        public void ExportData(byte[] fileData)
        {
            // Queue data for export - multiple exports may take some time, so we do this on another thread...
            if (m_exportQueue != null)
                m_exportQueue.Add(fileData);
        }

        /// <summary>
        /// Raises the <see cref="Initialized"/> event.
        /// </summary>
        protected virtual void OnInitialized()
        {
            if (Initialized != null)
                Initialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">Status message to report.</param>
        /// <param name="args"><see cref="string.Format(string,object[])"/> parameters used for status message.</param>
        protected virtual void OnStatusMessage(string status, params object[] args)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(string.Format(status, args)));
        }

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

                        if (m_exportQueue != null)
                            m_exportQueue.ProcessException -= ProcessExceptionHandler;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        private void Initialize(object state)
        {
            // In case we are reinitializing class, we shutdown any prior queue operations and close any existing network connections...
            Shutdown();

            // Retrieve any specified default export destinations
            lock (m_exportDestinations)
            {
                m_exportDestinations = state as List<ExportDestination>;
            }

            // Load export destinations from the config file - if nothing is in config file yet,
            // the default settings (passed in via state) will be used instead
            LoadSettings();

            List<ExportDestination> destinations;

            lock (m_exportDestinations)
            {
                // Cache a local copy of export destinations to reduce lock time,
                // network share authentication may take some time
                destinations = new List<ExportDestination>(m_exportDestinations);
            }

            for (int x = 0; x < destinations.Count; x++)
            {
                // Connect to network shares if necessary
                if (destinations[x].ConnectToShare)
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
                        OnStatusMessage("Network share authentication to {0} failed due to exception: {1}", destinations[x].Share, ex.Message);
                    }
                }
            }

            m_exportQueue.Start();          // Start export queue.
            OnInitialized();                // Notify that initialization is complete.
        }

        // This is all of the needed dispose functionality, but since the class can be re-initialized this is a separate method
        private void Shutdown()
        {
            if (m_exportQueue != null)
            {
                m_exportQueue.Stop();
                m_exportQueue.Clear();
            }

            if (m_exportDestinations != null)
            {
                lock (m_exportDestinations)
                {
                    // We'll be nice and disconnect network shares when this class is disposed...
                    for (int x = 0; x < m_exportDestinations.Count; x++)
                    {
                        if (m_exportDestinations[x].ConnectToShare)
                        {
                            try
                            {
                                FilePath.DisconnectFromNetworkShare(m_exportDestinations[x].Share);
                            }
                            catch (Exception ex)
                            {
                                // Something unexpected happened during attempt to disconnect from network share - so we'll report it...
                                OnStatusMessage("Network share disconnect from {0} failed due to exception: {1}", m_exportDestinations[x].Share, ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void WriteExportFiles(byte[] fileData)
        {
            string filename;
            List<ExportDestination> destinations;

            lock (m_exportDestinations)
            {
                // Cache a local copy of export destinations to reduce lock time,
                // exports may take some time
                destinations = new List<ExportDestination>(m_exportDestinations);
            }

            // Loop through each defined export file
            for (int x = 0; x < destinations.Count; x++)
            {
                try
                {
                    //  Get next export file name
                    filename = destinations[x].DestinationFile;

                    if (File.Exists(filename))
                        FilePath.WaitForWriteLock(filename, 1); // Wait for a lock on the file.

                    File.WriteAllBytes(filename, fileData);     // Export data to the file.
                    m_totalExports++;                           // Track successful exports.
                }
                catch (ThreadAbortException)
                {
                    throw;  // This exception is normal, we'll just rethrow this back up the try stack
                }
                catch (Exception ex)
                {
                    // Something unexpected happened during export - we'll report it but keep going, could be
                    // that export destination was offline (not uncommon when system is being rebooted, etc.)
                    OnStatusMessage("Exception encountered during export for {0}: {1}", destinations[x].DestinationFile, ex.Message);
                }
            }
        }

        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            // Something unexpected happened during export
            OnStatusMessage("Export exception: {0}", e.Argument.Message);
        }

        #endregion
    }
}