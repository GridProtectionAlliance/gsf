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
//       Added "Initialize" method to enable user to reconnect to network shares.
//       Added more descriptive status messages to provide more detailed user feedback.
//  09/19/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
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
    /// <remarks>
    /// This class is useful for updating the same file on multiple servers (e.g., load balanced web server).
    /// </remarks>
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
    [ToolboxBitmap(typeof(MultipleDestinationExporter))]
    public class MultipleDestinationExporter : Component, ISupportLifecycle, ISupportInitialize, IStatusProvider, IPersistSettings
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="ExportTimeout"/> property.
        /// </summary>
        public const int DefaultExportTimeout = Timeout.Infinite;

        /// <summary>
        /// Default value for the <see cref="PersistSettings"/> property.
        /// </summary>
        public const bool DefaultPersistSettings = true;

        /// <summary>
        /// Default value for the <see cref="SettingsCategory"/> property.
        /// </summary>
        public const string DefaultSettingsCategory = "ExportDestinations";

        // Events

        /// <summary>
        /// Occurs to report status information from exporter.
        /// </summary>
        [Description("Occurs to report status information from exporter.")]
        public event Action<string> StatusMessage;

        // Fields

        private int m_exportTimeout;
        private bool m_persistSettings;
        private string m_settingsCategory;
        private long m_totalExports;
        private Encoding m_textEncoding;
        private ProcessQueue<byte[]> m_exportQueue;
        private object m_destinationLock;
        private ExportDestination[] m_exportDestinations;
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
        /// <param name="settingsCategory"></param>
        /// <param name="exportTimeout">The total allowed time in milliseconds for all exports to execute.</param>
        public MultipleDestinationExporter(string settingsCategory, int exportTimeout)
        {
            m_exportTimeout = exportTimeout;
            m_settingsCategory = settingsCategory;
            m_persistSettings = DefaultPersistSettings;
            m_textEncoding = Encoding.Default; // We use default ANSI page encoding for text based exports...
            m_destinationLock = new object();
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
                {
                    m_exportQueue.ProcessTimeout = value;
                }
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
        /// <exception cref="ArgumentNullException">The value being set is null or empty string.</exception>
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
                    throw (new ArgumentNullException());

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
        /// <remarks>
        /// <see cref="Enabled"/> property is not be set by user-code directly.
        /// </remarks>
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
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
        /// Gets a list of currently defined export destinations.
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
        /// Performs necessary operations before the <see cref="MultipleDestinationExporter"/> object properties are initialized.
        /// </summary>
        /// <remarks>
        /// <see cref="BeginInit()"/> should never be called by user-code directly. This method exists solely for use 
        /// by the designer if the <see cref="MultipleDestinationExporter"/> object is consumed through the designer surface of the IDE.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void BeginInit()
        {
            // Nothing needs to be done before component is initialized.
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
                Initialize();
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
                lock (m_destinationLock)
                {
                    settings["ExportTimeout", true].Update(m_exportTimeout, "Total allowed time for all exports to execute in milliseconds.");

                    if ((m_exportDestinations != null) && m_exportDestinations.Length > 0)
                    {
                        settings["ExportCount", true].Update(m_exportDestinations.Length, "Total number of export files to produce");
                        for (int x = 0; x < m_exportDestinations.Length; x++)
                        {
                            settings[string.Format("ExportDestination{0}", x + 1), true].Update(m_exportDestinations[x].Share, "Root path for export destination. Use UNC path (\\\\server\\share) with no trailing slash for network shares.");
                            settings[string.Format("ExportDestination{0}.ConnectToShare", x + 1), true].Update(m_exportDestinations[x].ConnectToShare, "Set to True to attempt authentication to network share.");
                            settings[string.Format("ExportDestination{0}.Domain", x + 1), true].Update(m_exportDestinations[x].Domain, "Domain used for authentication to network share (computer name for local accounts).");
                            settings[string.Format("ExportDestination{0}.UserName", x + 1), true].Update(m_exportDestinations[x].UserName, "User name used for authentication to network share.");
                            settings[string.Format("ExportDestination{0}.Password", x + 1), true].Update(m_exportDestinations[x].Password, "Encrypted password used for authentication to network share.", true);
                            settings[string.Format("ExportDestination{0}.FileName", x + 1), true].Update(m_exportDestinations[x].FileName, "Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\\path\\filename.txt).");
                        }
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
                string entryRoot;
                ExportDestination destination;
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[m_settingsCategory];

                m_exportTimeout = settings["ExportTimeout", true].ValueAs(m_exportTimeout);
                m_exportDestinations = new ExportDestination[settings["ExportCount", true].ValueAsInt32()];
                for (int x = 0; x < m_exportDestinations.Length; x++)
                {
                    entryRoot = string.Format("ExportDestination{0}", x + 1);

                    // Load export destination from configuration entries
                    destination.DestinationFile = settings[entryRoot, true].ValueAsString() + settings[string.Format("{0}.FileName", entryRoot), true].ValueAsString();
                    destination.ConnectToShare = settings[string.Format("{0}.ConnectToShare", entryRoot), true].ValueAsBoolean();
                    destination.Domain = settings[string.Format("{0}.Domain", entryRoot), true].ValueAsString();
                    destination.UserName = settings[string.Format("{0}.UserName", entryRoot), true].ValueAsString();
                    destination.Password = settings[string.Format("{0}.Password", entryRoot), true].ValueAsString();

                    // Save new export destination
                    m_exportDestinations[x] = destination;
                }
            }
        }

        protected void OnStatusMessage(string status)
        {
            if (StatusMessage != null)
                StatusMessage(status);
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
                        OnStatusMessage(string.Format("Attempting network share authentication for user {0}\\{1} to {2}...", destinations[x].Domain, destinations[x].UserName, destinations[x].Share));

                        FilePath.ConnectToNetworkShare(destinations[x].Share, destinations[x].UserName, destinations[x].Password, destinations[x].Domain);

                        OnStatusMessage(string.Format("Network share authentication to {0} succeeded.", destinations[x].Share));
                    }
                    catch (Exception ex)
                    {
                        // Something unexpected happened during attempt to connect to network share - so we'll report it...
                        OnStatusMessage(string.Format("Network share authentication to {0} failed due to exception: {1}", destinations[x].Share, ex.Message));
                    }
                }
            }

            // Start export queue
            m_exportQueue.Start();
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
                                OnStatusMessage(string.Format("Network share disconnect from {0} failed due to exception: {1}", m_exportDestinations[x].Share, ex.Message));
                            }
                        }
                    }
                }

                m_exportDestinations = null;
            }
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
                    OnStatusMessage(string.Format("Exception encountered during export for {0}: {1}", destinations[x].DestinationFile, ex.Message));
                }
            }
        }

        private void m_exportQueue_ProcessException(Exception ex)
        {
            // Something unexpected happened during export
            OnStatusMessage("Export exception: " + ex.Message);
        }

        #endregion
    }
}