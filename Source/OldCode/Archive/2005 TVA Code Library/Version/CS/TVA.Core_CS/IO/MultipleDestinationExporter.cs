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
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using TVA.IO;
using TVA.Collections;
using TVA.Configuration;
using TVA.Services;
using TVA.Threading;

namespace TVA.IO
{
    /// <summary>
    /// Handles exporting the same file to multiple destinations that are defined in the configuration file.  Includes feature for network share authentication.
    /// </summary>
    /// <remarks>
    /// This class is useful for updating the same file on multiple servers (e.g., load balanced web server).
    /// </remarks>
    public class MultipleDestinationExporter : IServiceComponent
    {
        #region [ Members ]

        // Constants
        private const string DefaultConfigSection = "ExportDestinations";
        private const int DefaultExportTimeout = Timeout.Infinite;

        // Delegates

        // Events
        public event Action<string> StatusMessage;

        // Fields
        private ExportDestination[] m_exportDestinations;
        private string m_configSection;
        private int m_exportTimeout;
        private long m_totalExports;
        private Encoding m_textEncoding;
        private bool m_disposed;
        private ProcessQueue<byte[]> m_exportQueue;

        #endregion

        #region [ Constructors ]

        public MultipleDestinationExporter()
            : this(new ExportDestination[] { new ExportDestination("C:\\filename.txt", false, "domain", "username", "password") })
        {
        }

        public MultipleDestinationExporter(ExportDestination[] defaultDestinations)
            : this(DefaultConfigSection, DefaultExportTimeout, defaultDestinations)
        {
        }

        public MultipleDestinationExporter(string configSection, int exportTimeout, ExportDestination[] defaultDestinations)
        {
            m_configSection = configSection;
            m_exportTimeout = exportTimeout;
            m_textEncoding = Encoding.Default; // We use default ANSI page encoding for text based exports...

            // So as to not delay class construction due to share authentication, we perform initialization on another thread...
#if ThreadTracking
            ManagedThread thread = ManagedThreadPool.QueueUserWorkItem(Initialize, defaultDestinations);
            thread.Name = "TVA.IO.MultipleDestinationExporter.Initialize()";
#else
            ThreadPool.QueueUserWorkItem(Initialize, defaultDestinations);
#endif
        }

        ~MultipleDestinationExporter()
        {
            Dispose(true);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Defines name of configuration section used to store settings.
        /// </summary>
        public string ConfigSection
        {
            get
            {
                return m_configSection;
            }
            set
            {
                m_configSection = value;
            }
        }

        /// <summary>
        /// Total allowed time for all exports to execute in milliseconds.
        /// </summary>
        /// <remarks>
        /// Set to Timeout.Infinite (-1) for no timeout.
        /// </remarks>
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
        public string Status
        {
            get
			{
				StringBuilder status = new StringBuilder();

				status.Append("     Configuration Section: ");
				status.Append(m_configSection);
				status.AppendLine();
				status.Append("       Export destinations: ");
				status.Append(m_exportDestinations.ToDelimitedString(','));
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
        public string Name
        {
            get
            {
                return m_configSection;
            }
        }

        #endregion

        #region [ Methods ]

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                    Shutdown();
            }

            m_disposed = true;
        }

        private void Shutdown()
        {
            if (m_exportQueue != null)
            {
                m_exportQueue.ProcessException -= m_exportQueue_ProcessException;
                m_exportQueue.Dispose();
            }
            m_exportQueue = null;

            // We'll be nice and disconnect network shares when this class is disposed...
            if (m_exportDestinations != null)
            {
                for (int x = 0; x <= m_exportDestinations.Length - 1; x++)
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

        private void Initialize(object state)
        {
            ExportDestination[] defaultDestinations = (ExportDestination[])state;

            // In case we are reinitializing class (e.g., user requested to reauthenticate), we shutdown
            // any prior queue operations and close any existing network connections...
            Shutdown();

            // Set up a synchronous process queue to handle exports that will limit total export time to export interval
            m_exportQueue = ProcessQueue<byte[]>.CreateSynchronousQueue(WriteExportFiles, 10, m_exportTimeout, false, false);
            m_exportQueue.ProcessException += m_exportQueue_ProcessException;

            CategorizedSettingsElementCollection settings = TVA.Configuration.Common.CategorizedSettings(m_configSection);

            if ((defaultDestinations != null) && defaultDestinations.Length > 0)
            {
                // Make sure the default configuration variables exist
                settings.Add("ExportCount", defaultDestinations.Length.ToString(), "Total number of export files to produce");

                for (int x = 0; x <= defaultDestinations.Length - 1; x++)
                {
                    settings.Add(string.Format("ExportDestination{0}", x + 1), defaultDestinations[x].Share, "Root path for export destination. Use UNC path (\\\\server\\share) with no trailing slash for network shares.");
                    settings.Add(string.Format("ExportDestination{0}.ConnectToShare", x + 1), defaultDestinations[x].ConnectToShare.ToString(), "Set to True to attempt authentication to network share.", false);
                    settings.Add(string.Format("ExportDestination{0}.Domain", x + 1), defaultDestinations[x].Domain, "Domain used for authentication to network share (computer name for local accounts).", false);
                    settings.Add(string.Format("ExportDestination{0}.UserName", x + 1), defaultDestinations[x].UserName, "User name used for authentication to network share.");
                    settings.Add(string.Format("ExportDestination{0}.Password", x + 1), defaultDestinations[x].Password, "Encrypted password used for authentication to network share.", true);
                    settings.Add(string.Format("ExportDestination{0}.FileName", x + 1), defaultDestinations[x].FileName, "Path and file name of data export (do not include drive letter or UNC share). Prefix with slash when using UNC paths (\\path\\filename.txt).");
                }

                // Save updates to config file, if any
                TVA.Configuration.Common.SaveSettings();
            }

            // Load needed settings
            string entryRoot;
            ExportDestination destination;
            int exportCount = int.Parse(settings["ExportCount"].Value);

            m_exportDestinations = new ExportDestination[exportCount];

            for (int x = 0; x <= exportCount - 1; x++)
            {
                entryRoot = string.Format("ExportDestination{0}", x + 1);

                // Load export destination from configuration entries
                destination.DestinationFile = settings[entryRoot].Value + settings[string.Format("{0}.FileName", entryRoot)].Value;
                destination.ConnectToShare = settings[string.Format("{0}.ConnectToShare", entryRoot)].Value.ParseBoolean();
                destination.Domain = settings[string.Format("{0}.Domain", entryRoot)].Value;
                destination.UserName = settings[string.Format("{0}.UserName", entryRoot)].Value;
                destination.Password = settings[string.Format("{0}.Password", entryRoot)].Value;

                if (destination.ConnectToShare)
                {
                    // Attempt connection to external network share
                    try
                    {
                        UpdateStatus(string.Format("Attempting network share authentication for user {0}\\{1} to {2}...", destination.Domain, destination.UserName, destination.Share));

                        FilePath.ConnectToNetworkShare(destination.Share, destination.UserName, destination.Password, destination.Domain);

                        UpdateStatus(string.Format("Network share authentication to {0} succeeded.", destination.Share));
                    }
                    catch (Exception ex)
                    {
                        // Something unexpected happened during attempt to connect to network share - so we'll report it...
                        UpdateStatus(string.Format("Network share authentication to {0} failed due to exception: {1}", destination.Share, ex.Message));
                    }
                }

                m_exportDestinations[x] = destination;
            }

            m_exportQueue.Start();
        }

        public void Reauthenticate()
        {
            Initialize(null);
        }

        public void Reauthenticate(ExportDestination[] defaultDestinations)
        {
            Initialize(defaultDestinations);
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

            // Loop through each defined export file
            for (int x = 0; x <= m_exportDestinations.Length - 1; x++)
            {
                try
                {
                    //  Get next export file name
                    filename = m_exportDestinations[x].DestinationFile;

                    try
                    {
                        // We'll wait on file lock for up to one second - then give up with IO exception
                        FilePath.WaitForWriteLock(filename, 1);
                    }
                    catch (ThreadAbortException ex)
                    {
                        // This exception is normal, we'll just rethrow this back up the try stack
                        throw ex;
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
                catch (ThreadAbortException ex)
                {
                    // This exception is normal, we'll just rethrow this back up the try stack
                    throw ex;
                }
                catch (Exception ex)
                {
                    // Something unexpected happened during export - we'll report it but keep going, could be
                    // that export destination was offline (not uncommon when system is being rebooted, etc.)
                    UpdateStatus(string.Format("Exception encountered during export for {0}: {1}", m_exportDestinations[x].DestinationFile, ex.Message));
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

        public void ProcessStateChanged(string processName, ProcessState newState)
        {
            // This component is not abstractly associated with any particular service process...
        }

        public void ServiceStateChanged(Services.ServiceState newState)
        {
            if (newState == ServiceState.Paused)
            {
                if (m_exportQueue != null)
                    m_exportQueue.Stop();
            }
            else if (newState == ServiceState.Resumed)
            {
                if (m_exportQueue != null)
                    m_exportQueue.Start();
            }
            else if (newState == ServiceState.Shutdown)
            {
                Dispose();
            }
        }

        #endregion
    }
}