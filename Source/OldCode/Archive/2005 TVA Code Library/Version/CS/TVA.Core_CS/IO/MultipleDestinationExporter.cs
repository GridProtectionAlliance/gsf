//*******************************************************************************************************
//  MultipleDestinationExporter.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/13/2008 - J. Ritchie Carroll
//       Initial version of source generated
//  07/29/2008 - J. Ritchie Carroll
//       Added "Reauthenticate" method to enable user to reconnect to network shares.
//       Added more descriptive status messages to provide more detailed user feedback.
//  09/19/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using TVA.Collections;
using TVA.Configuration;
using TVA.Services;
using TVA.Threading;

namespace TVA.IO
{
    /// <summary>
    /// Handles exporting the same file to multiple destinations that are defined in the configuration file.
    /// Includes feature for network share authentication.
    /// </summary>
    /// <example>
    /// <code>
    /// string xmlData;
    /// MultipleDestinationExporter exporter = new MultipleDestinationExporter();
    /// ExportDestination[] defaultDestinations = new ExportDestination[]
    /// {
    ///     new ExportDestination("\\\\server1\\share\\exportFile.xml", true, "domain", "user1", "password1"),
    ///     new ExportDestination("\\\\server2\\share\\exportFile.xml", true, "domain", "user2", "password2")
    /// };
    ///
    /// // Provide a default set of export destinations to exporter - note that
    /// // actual exports will always be based on entries in configuration file
    /// exporter.Initialize(defaultDestinations);
    ///
    /// // Export data to all defined locations...
    /// exporter.ExportData(xmlData);
    /// </code>
    /// </example>
    /// <remarks>
    /// This class is useful for updating the same file on multiple servers (e.g., load balanced web server).
    /// </remarks>
    [ToolboxBitmap(typeof(MultipleDestinationExporter))]
    public class MultipleDestinationExporter : Component, IServiceComponent, IPersistSettings, ISupportInitialize
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for ExportTimeout property.
        /// </summary>
        public const int DefaultExportTimeout = Timeout.Infinite;

        /// <summary>
        /// Default value for PersistSettings property.
        /// </summary>
        public const bool DefaultPersistSettings = false;

        /// <summary>
        /// Default value for SettingsCategoryName property.
        /// </summary>
        public const string DefaultSettingsCategoryName = "ExportDestinations";

        // Events
        
        /// <summary>
        /// Reports status information from exporter.
        /// </summary>
        [Description("Reports status information from exporter.")]
        public event Action<string> StatusMessage;

        // Fields
        private ExportDestination[] m_exportDestinations;
        private object m_destinationLock;
        private string m_settingsCategoryName;
        private int m_exportTimeout;
        private long m_totalExports;
        private Encoding m_textEncoding;
        private ProcessQueue<byte[]> m_exportQueue;
        private bool m_persistSettings;
        private bool m_previouslyEnabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public MultipleDestinationExporter()
            : this(DefaultSettingsCategoryName, DefaultExportTimeout)
        {
        }

        public MultipleDestinationExporter(string settingsCategoryName, int exportTimeout)
        {
            m_settingsCategoryName = settingsCategoryName;
            m_exportTimeout = exportTimeout;
            m_textEncoding = Encoding.Default; // We use default ANSI page encoding for text based exports...
            m_destinationLock = new object();
        }

        ~MultipleDestinationExporter()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets total allowed time for all exports to execute in milliseconds.
        /// </summary>
        /// <remarks>
        /// Set to Timeout.Infinite (-1) for no timeout.
        /// </remarks>
        /// <returns>Total allowed time for all exports to execute in milliseconds.</returns>
        [Category("Settings"), DefaultValue(DefaultExportTimeout), Description("Total allowed time for all exports to execute in milliseconds.")]
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
                {
                    m_exportQueue.ProcessTimeout = value;
                }
            }
        }

        /// <summary>
        /// Currently defined export destinations
        /// </summary>
        [Browsable(false)]
        public ExportDestination[] ExportDestinations
        {
            get
            {
                return m_exportDestinations;
            }
        }

        /// <summary>
        /// Total exports performed.
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
        /// Component status.
        /// </summary>
        [Browsable(false)]
        public string Status
        {
            get
			{
				StringBuilder status = new StringBuilder();

				status.Append("     Configuration section: ");
				status.Append(m_settingsCategoryName);
				status.AppendLine();
				status.Append("       Export destinations: ");
                lock (m_destinationLock)
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

        /// <summary>
        /// Gets or sets the encoding to be used to encode text data being exported.
        /// </summary>
        /// <value>The encoding to be used to encode text data being exported.</value>
        /// <returns>The encoding to be used to encode text data being exported.</returns>
        [Browsable(false)]
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
        /// Component name.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                // We just return the settings category name for unique identification of this component
                return m_settingsCategoryName;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the component settings are to be persisted to the config
        /// file.
        /// </summary>
        /// <returns>True, if the component settings are to be persisted to the config file; otherwise, false.</returns>
        [Category("Persistance"), DefaultValue(DefaultPersistSettings), Description("Indicates whether the component settings are to be persisted to the config file.")]
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
        /// Gets or sets the category name under which the component settings are to be saved in the config file.
        /// </summary>
        /// <returns>The category name under which the component settings are to be saved in the config file.</returns>
        [Category("Persistance"), DefaultValue(DefaultSettingsCategoryName), Description("The category name under which the component settings are to be saved in the config file.")]
        public string SettingsCategoryName
        {
            get
            {
                return m_settingsCategoryName;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_settingsCategoryName = value;
                else
                    throw new ArgumentNullException("SettingsCategoryName");
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
                    if (disposing)
                    {
                        // This method handles all needed dispoable operations
                        Shutdown();
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        // This is all of the needed dispose functionality, but since the class can be re-initialized this is a separate method
        private void Shutdown()
        {
            if (m_exportQueue != null)
            {
                m_exportQueue.ProcessException -= m_exportQueue_ProcessException;
                m_exportQueue.Dispose();
            }
            m_exportQueue = null;

            lock (m_destinationLock)
            {
                // We'll be nice and disconnect network shares when this class is disposed...
                if (m_exportDestinations != null)
                {
                    for (int x = 0; x < m_exportDestinations.Length; x++)
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
                                UpdateStatus(string.Format("Network share disconnect from {0} failed due to exception: {1}", m_exportDestinations[x].Share, ex.Message));
                            }
                        }
                    }
                }

                m_exportDestinations = null;
            }
        }

        /// <summary>
        /// Initializes (or reinitializes) exporter from configuration settings.
        /// </summary>
        /// <remarks>
        /// If not being used as a component (i.e., user creates their own instance of this class), this method
        /// must be called in order to initialize exports.  Event if used as a component this method can be
        /// called at anytime to reintialize the exports with new configuration information.
        /// </remarks>
        public void Initialize()
        {
            Initialize(null);
        }

        /// <summary>
        /// Initializes (or reinitializes) exporter from configuration settings.
        /// </summary>
        /// <param name="defaultDestinations">Provides a default set of export destinations if none exist in configuration settings.</param>
        /// <remarks>
        /// If not being used as a component (i.e., user creates their own instance of this class), this method
        /// must be called in order to initialize exports.  Event if used as a component this method can be
        /// called at anytime to reintialize the exports with new configuration information.
        /// </remarks>
        public void Initialize(ExportDestination[] defaultDestinations)
        {
            // So as to not delay calling thread due to share authentication, we perform initialization on another thread...
#if ThreadTracking
            ManagedThread thread = ManagedThreadPool.QueueUserWorkItem(Initialize, defaultDestinations);
            thread.Name = "TVA.IO.MultipleDestinationExporter.Initialize()";
#else
            ThreadPool.QueueUserWorkItem(Initialize, defaultDestinations);
#endif
        }

        private void Initialize(object state)
        {
            // In case we are reinitializing class, we shutdown any prior queue operations and close any existing network connections...
            Shutdown();

            // Set up a synchronous process queue to handle exports that will limit total export time to export interval
            m_exportQueue = ProcessQueue<byte[]>.CreateSynchronousQueue(WriteExportFiles, 10, m_exportTimeout, false, false);
            m_exportQueue.ProcessException += m_exportQueue_ProcessException;

            lock (m_destinationLock)
            {
                // Retrieve any specified default export destinations
                m_exportDestinations = (ExportDestination[])state;

                // We provide a simple default set of export destinations as a configuration file
                // example if no others are specified
                if (m_exportDestinations == null)
                    m_exportDestinations = new ExportDestination[] { new ExportDestination("C:\\filename.txt", false, "domain", "username", "password") };
            }

            // Load export settings (this loads a new set of destinations in m_exportDestinations)
            LoadSettings();

            // Connect to network shares if necessary
            ExportDestination[] destinations;

            lock (m_destinationLock)
            {
                // Cache a local copy of export destinations to reduce lock time,
                // network share authentication may take some time
                destinations = m_exportDestinations;
            }

            for (int x = 0; x < destinations.Length; x++)
            {
                if (destinations[x].ConnectToShare)
                {
                    // Attempt connection to external network share
                    try
                    {
                        UpdateStatus(string.Format("Attempting network share authentication for user {0}\\{1} to {2}...", destinations[x].Domain, destinations[x].UserName, destinations[x].Share));

                        FilePath.ConnectToNetworkShare(destinations[x].Share, destinations[x].UserName, destinations[x].Password, destinations[x].Domain);

                        UpdateStatus(string.Format("Network share authentication to {0} succeeded.", destinations[x].Share));
                    }
                    catch (Exception ex)
                    {
                        // Something unexpected happened during attempt to connect to network share - so we'll report it...
                        UpdateStatus(string.Format("Network share authentication to {0} failed due to exception: {1}", destinations[x].Share, ex.Message));
                    }
                }
            }

            // Start export queue
            m_exportQueue.Start();
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

        private void WriteExportFiles(byte[] fileData)
        {
            string filename;
            FileStream exportFile;
            ExportDestination[] destinations;

            lock (m_destinationLock)
            {
                // Cache a local copy of export destinations to reduce lock time,
                // exports may take some time
                destinations = m_exportDestinations;
            }

            // Loop through each defined export file
            for (int x = 0; x <= destinations.Length - 1; x++)
            {
                try
                {
                    //  Get next export file name
                    filename = destinations[x].DestinationFile;

                    try
                    {
                        // We'll wait on file lock for up to one second - then give up with IO exception
                        FilePath.WaitForWriteLock(filename, 1);
                    }
                    catch (ThreadAbortException)
                    {
                        // This exception is normal, we'll just rethrow this back up the try stack
                        throw;
                    }
                    catch (FileNotFoundException)
                    {
                        // This would be an expected exception, nothing to do - even if we checked for
                        // this before we called the wait function, another process could have deleted
                        // the file before we attempt a file lock...
                    }

                    // Create a new export file
                    exportFile = File.Create(filename);

                    // Export file data
                    exportFile.Write(fileData, 0, fileData.Length);

                    // Close stream
                    exportFile.Close();

                    // Track successful exports
                    m_totalExports++;
                }
                catch (ThreadAbortException)
                {
                    // This exception is normal, we'll just rethrow this back up the try stack
                    throw;
                }
                catch (Exception ex)
                {
                    // Something unexpected happened during export - we'll report it but keep going, could be
                    // that export destination was offline (not uncommon when system is being rebooted, etc.)
                    UpdateStatus(string.Format("Exception encountered during export for {0}: {1}", destinations[x].DestinationFile, ex.Message));
                }
            }
        }

        private void UpdateStatus(string status)
        {
            if (StatusMessage != null)
                StatusMessage(status);
        }

        private void m_exportQueue_ProcessException(Exception ex)
        {
            // Something unexpected happened during export
            UpdateStatus("Export exception: " + ex.Message);
        }

        public virtual void ProcessStateChanged(string processName, ProcessState newState)
        {
            // This component is not abstractly associated with any particular service process...
        }

        public virtual void ServiceStateChanged(Services.ServiceState newState)
        {
            switch (newState)
            {
                case ServiceState.Paused:
                    m_previouslyEnabled = m_exportQueue.Enabled;
                    m_exportQueue.Enabled = false;
                    break;
                case ServiceState.Resumed:
                    m_exportQueue.Enabled = m_previouslyEnabled;
                    break;
                case ServiceState.Shutdown:
                    Dispose();
                    break;
            }
        }

        public void BeginInit()
        {
            // No prerequisites before the component is initialized.
        }

        public void EndInit()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                Initialize(); // Loads settings and initializes export
            }
        }

        /// <summary>
        /// Loads the component settings from the config file, if present.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                lock (m_destinationLock)
                {
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];

                    // We make sure at least a default set of configuration exists before we start load cycle
                    settings.Add("ExportTimeout", DefaultExportTimeout, "Total allowed time for all exports to execute in milliseconds.");

                    if ((m_exportDestinations != null) && m_exportDestinations.Length > 0)
                    {
                        // Make sure the default configuration variables exist
                        settings.Add("ExportCount", m_exportDestinations.Length.ToString(), "Total number of export files to produce");

                        for (int x = 0; x < m_exportDestinations.Length; x++)
                        {
                            settings.Add(string.Format("ExportDestination{0}", x + 1), m_exportDestinations[x].Share, "Root path for export destination. Use UNC path (\\\\server\\share) with no trailing slash for network shares.");
                            settings.Add(string.Format("ExportDestination{0}.ConnectToShare", x + 1), m_exportDestinations[x].ConnectToShare.ToString(), "Set to True to attempt authentication to network share.", false);
                            settings.Add(string.Format("ExportDestination{0}.Domain", x + 1), m_exportDestinations[x].Domain, "Domain used for authentication to network share (computer name for local accounts).", false);
                            settings.Add(string.Format("ExportDestination{0}.UserName", x + 1), m_exportDestinations[x].UserName, "User name used for authentication to network share.");
                            settings.Add(string.Format("ExportDestination{0}.Password", x + 1), m_exportDestinations[x].Password, "Encrypted password used for authentication to network share.", true);
                            settings.Add(string.Format("ExportDestination{0}.FileName", x + 1), m_exportDestinations[x].FileName, "Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\\path\\filename.txt).");
                        }
                    }

                    // Save updates to config file, if any
                    ConfigurationFile.Current.Save();

                    // Load export destinations
                    if (settings.Count > 0)
                    {
                        string entryRoot;
                        ExportDestination destination;
                        int exportCount = int.Parse(settings["ExportCount"].Value);

                        m_exportTimeout = settings["ExportTimeout"].ValueAs(m_exportTimeout);
                        m_exportDestinations = new ExportDestination[exportCount];

                        for (int x = 0; x < exportCount; x++)
                        {
                            entryRoot = string.Format("ExportDestination{0}", x + 1);

                            // Load export destination from configuration entries
                            destination.DestinationFile = settings[entryRoot].Value + settings[string.Format("{0}.FileName", entryRoot)].Value;
                            destination.ConnectToShare = settings[string.Format("{0}.ConnectToShare", entryRoot)].Value.ParseBoolean();
                            destination.Domain = settings[string.Format("{0}.Domain", entryRoot)].Value;
                            destination.UserName = settings[string.Format("{0}.UserName", entryRoot)].Value;
                            destination.Password = settings[string.Format("{0}.Password", entryRoot)].Value;

                            // Save new export destination
                            m_exportDestinations[x] = destination;
                        }
                    }
                }
            }
            catch
            {
                // Exceptions will occur if the settings are not present in the config file.
            }
        }

        /// <summary>
        /// Saves the component settings to the config file.
        /// </summary>
        public void SaveSettings()
        {
            if (m_persistSettings)
            {
                try
                {
                    lock (m_destinationLock)
                    {
                        CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[m_settingsCategoryName];

                        settings["ExportTimeout"].Value = m_exportTimeout.ToString();

                        if (m_exportDestinations != null && m_exportDestinations.Length > 0)
                        {
                            // Save current export destinations
                            settings["ExportCount"].Value = m_exportDestinations.Length.ToString();

                            for (int x = 0; x < m_exportDestinations.Length; x++)
                            {
                                settings[string.Format("ExportDestination{0}", x + 1)].Value = m_exportDestinations[x].Share;
                                settings[string.Format("ExportDestination{0}.ConnectToShare", x + 1)].Value = m_exportDestinations[x].ConnectToShare.ToString();
                                settings[string.Format("ExportDestination{0}.Domain", x + 1)].Value = m_exportDestinations[x].Domain;
                                settings[string.Format("ExportDestination{0}.UserName", x + 1)].Value = m_exportDestinations[x].UserName;
                                settings[string.Format("ExportDestination{0}.Password", x + 1)].Value = m_exportDestinations[x].Password;
                                settings[string.Format("ExportDestination{0}.FileName", x + 1)].Value = m_exportDestinations[x].FileName;
                            }
                        }

                        // Save updates to config file, if any
                        ConfigurationFile.Current.Save();
                    }
                }
                catch
                {
                    // Exceptions may occur if the settings cannot be saved to the config file.
                }
            }
        }

        #endregion
    }
}