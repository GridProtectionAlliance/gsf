//*******************************************************************************************************
//  FtpFileWatcher.cs
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
//  05/22/2003 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace PCS.Net.Ftp
{
    /// <summary>
    /// FTP File Watcher
    /// </summary>
    /// <remarks>
    /// Monitors for file changes over an FTP session.
    /// </remarks>
    [ToolboxBitmap(typeof(FtpFileWatcher)), DefaultProperty("Server"), DefaultEvent("FileAdded"), Description("Monitors for file changes over an FTP session")]
    public class FtpFileWatcher : Component
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Raised when new file is added to monitored FTP directory.
        /// </summary>
        /// <remarks>
        /// <see cref="System.EventArgs{T}.Argument"/> is reference to newly added file.
        /// </remarks>
        public event EventHandler<EventArgs<FtpFile>> FileAdded;

        /// <summary>
        /// Raised when file is deleted from monitored FTP directory.
        /// </summary>
        /// <remarks>
        /// <see cref="System.EventArgs{T}.Argument"/> is reference to file that was removed.
        /// </remarks>
        public event EventHandler<EventArgs<FtpFile>> FileDeleted;

        /// <summary>
        /// Raised when new status messages come from the FTP file watcher.
        /// </summary>
        /// <remarks>
        /// <see cref="System.EventArgs{T}.Argument"/> is status message from FTP file watcher.
        /// </remarks>
        public event EventHandler<EventArgs<string>> Status;

        /// <summary>
        /// Raised when FTP command has been sent.
        /// </summary>
        /// <remarks>
        /// <see cref="System.EventArgs{T}.Argument"/> is sent FTP command.
        /// </remarks>
        public event EventHandler<EventArgs<string>> CommandSent;

        /// <summary>
        /// Raised when FTP response has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="System.EventArgs{T}.Argument"/> is received FTP response.
        /// </remarks>
        public event EventHandler<EventArgs<string>> ResponseReceived;

        // Fields
        private FtpClient m_session;
        private string m_username;
        private string m_password;
        private string m_watchDirectory;
        private System.Timers.Timer m_watchTimer;
        private System.Timers.Timer m_restartTimer;
        private List<FtpFile> m_currentFiles;
        private List<FtpFile> m_newFiles;
        private bool m_enabled;
        private bool m_notifyOnComplete;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new FTP file watcher using the default settings.
        /// </summary>
        public FtpFileWatcher()
        {
            m_enabled = true;
            m_notifyOnComplete = true;
            m_currentFiles = new List<FtpFile>();
            m_newFiles = new List<FtpFile>();

            m_session = new FtpClient(false);
            m_session.CommandSent += OnCommandSent;
            m_session.ResponseReceived += OnResponseReceived;

            // Define a timer to watch for new files
            m_watchTimer = new System.Timers.Timer();
            m_watchTimer.Elapsed += WatchTimer_Elapsed;
            m_watchTimer.AutoReset = false;
            m_watchTimer.Interval = 5000;
            m_watchTimer.Enabled = false;

            // Define a timer for FTP connection in case of availability failures
            m_restartTimer = new System.Timers.Timer();
            m_restartTimer.Elapsed += RestartTimer_Elapsed;
            m_restartTimer.AutoReset = false;
            m_restartTimer.Interval = 10000;
            m_restartTimer.Enabled = false;
        }

        /// <summary>
        /// Constructs a new FTP file watcher using the specified settings.
        /// </summary>
        /// <param name="caseInsensitive">Set to true to not be case sensitive with FTP file and directory names.</param>
        /// <param name="notifyOnComplete">Set to true to notify after file has completed uploading -or- set to false for immediate notification of new file.</param>
        public FtpFileWatcher(bool caseInsensitive, bool notifyOnComplete)
            : this()
        {
            m_session.CaseInsensitive = caseInsensitive;
            m_notifyOnComplete = notifyOnComplete;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="FtpFileWatcher"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~FtpFileWatcher()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets FTP server name (DNS name or IP) to watch.
        /// </summary>
        /// <remarks>
        /// FTP server name should not be prefixed with FTP://.
        /// </remarks>
        [Browsable(true), Category("Configuration"), Description("Specify FTP server name (do not prefix with ftp://).")]
        public virtual string Server
        {
            get
            {
                return m_session.Server;
            }
            set
            {
                m_session.Server = value;
            }
        }

        /// <summary>
        /// Gets or sets FTP server port to use, defaults to 21.
        /// </summary>
        /// <remarks>
        /// This only needs to be changed if the FTP server is established on a non-standard port number.
        /// </remarks>
        [Browsable(true), Category("Configuration"), Description("Specify FTP server port, if needed."), DefaultValue(21)]
        public int Port
        {
            get
            {
                return m_session.Port;
            }
            set
            {
                m_session.Port = value;
            }
        }

        /// <summary>
        /// Gets or sets FTP case sensitivity of file and directory names.
        /// </summary>
        /// <remarks>
        /// Set to true to not be case sensitive with FTP file and directory names.
        /// </remarks>
        [Browsable(true), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file and directory names."), DefaultValue(false)]
        public bool CaseInsensitive
        {
            get
            {
                return m_session.CaseInsensitive;
            }
            set
            {
                m_session.CaseInsensitive = value;
            }
        }

        /// <summary>
        /// Gets or sets interval, in seconds, to scan for file changes on monitored FTP directory.
        /// </summary>
        /// <remarks>
        /// Specify interval in seconds to poll FTP directory for file changes.
        /// </remarks>
        [Browsable(true), Category("Configuration"), Description("Specify interval in seconds to poll FTP directory for file changes."), DefaultValue(5)]
        public virtual int WatchInterval
        {
            get
            {
                return (int)(m_watchTimer.Interval / 1000);
            }
            set
            {
                m_watchTimer.Enabled = false;
                m_watchTimer.Interval = value * 1000;
                m_watchTimer.Enabled = m_enabled;
            }
        }

        /// <summary>
        /// Gets or sets name of FTP directory name to monitor. Leave blank to monitor initial FTP session directory.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Specify FTP directory to monitor.  Leave blank to monitor initial FTP session directory."), DefaultValue("")]
        public virtual string Directory
        {
            get
            {
                return m_watchDirectory;
            }
            set
            {
                m_watchDirectory = value;
                ConnectToWatchDirectory();
                Reset();
            }
        }

        /// <summary>
        /// Sets flag for notification time. Set to true to only notify when a file is finished uploading, set to False to get an immediate notification when a new file is detected.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Set to True to only be notified of new FTP files when upload is complete.  This monitors file size changes at each WatchInterval."), DefaultValue(true)]
        public virtual bool NotifyOnComplete
        {
            get
            {
                return m_notifyOnComplete;
            }
            set
            {
                m_notifyOnComplete = value;
                Reset();
            }
        }

        /// <summary>
        /// Gets or sets enabled state of the <see cref="FtpFileWatcher"/> object.
        /// </summary>
        [Browsable(true), Category("Configuration"), Description("Determines if FTP file watcher is enabled."), DefaultValue(true)]
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
                Reset();
            }
        }

        /// <summary>
        /// Returns true if FTP file watcher session is connected.
        /// </summary>
        [Browsable(false)]
        public virtual bool IsConnected
        {
            get
            {
                return m_session.IsConnected;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FtpFileWatcher"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!m_disposed)
                {
                    if (disposing)
                    {
                        Close();

                        if (m_session != null)
                        {
                            m_session.CommandSent -= OnCommandSent;
                            m_session.ResponseReceived -= OnResponseReceived;
                            m_session.Dispose();
                        }
                        m_session = null;

                        if (m_watchTimer != null)
                        {
                            m_watchTimer.Elapsed -= WatchTimer_Elapsed;
                            m_watchTimer.Dispose();
                        }
                        m_watchTimer = null;

                        if (m_restartTimer != null)
                        {
                            m_restartTimer.Elapsed -= RestartTimer_Elapsed;
                            m_restartTimer.Dispose();
                        }
                        m_restartTimer = null;
                    }
                }

                m_disposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Closes FTP session and clears resources used by the <see cref="FtpFileWatcher"/>.
        /// </summary>
        public virtual void Close()
        {
            m_currentFiles.Clear();
            m_newFiles.Clear();
            m_watchTimer.Enabled = false;
            m_restartTimer.Enabled = false;
            CloseSession();
        }

        /// <summary>
        /// Connects to FTP server and enables file watching if <see cref="Enabled"/> is true.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public virtual void Connect(string userName, string password)
        {
            if (!string.IsNullOrEmpty(userName))
                m_username = userName;

            if (!string.IsNullOrEmpty(password))
                m_password = password;

            try
            {
                // Attempt to connect to FTP server
                m_session.Connect(m_username, m_password);

                OnStatus("FTP file watcher connected to \"ftp://" + m_username + "@" + m_session.Server + "\"");

                ConnectToWatchDirectory();
                m_watchTimer.Enabled = m_enabled;

                // FTP servers can be fickle creatues, so after a successful connection we setup the
                // restart timer to reconnect every thirty minutes whether we need to or not :)
                m_restartTimer.Interval = 1800000;
                m_restartTimer.Enabled = true;
            }
            catch (FtpExceptionBase ex)
            {
                // If this fails, we'll try again in a moment.  The FTP server may be down...
                OnStatus("FTP file watcher failed to connect to \"ftp://" + m_username + "@" + m_session.Server + "\" - trying again in 10 seconds..." + "\r\n" + "\t" + "Exception: " + ex.Message);
                RestartConnectCycle();
            }
        }

        /// <summary>
        /// Clones FTP session used by file watcher so it can be used for other purposes.
        /// </summary>
        /// <returns>New connected FTP session matching settings defined for FTP file watcher.</returns>
        public virtual FtpClient CloneFtpSession()
        {
            // This method is just for convenience.  We can't allow the end user to use the
            // actual internal directory for sending files or other work because it is
            // constantly being refreshed/used etc., so we instead create a new FTP Session
            // based on the current internal session and watch directory information
            FtpClient newSession = new FtpClient(m_session.CaseInsensitive);

            newSession.Server = m_session.Server;
            newSession.Connect(m_username, m_password);
            newSession.SetCurrentDirectory(m_watchDirectory);

            return newSession;
        }

        /// <summary>
        /// Resets and restarts FTP session used by FTP file watcher.
        /// </summary>
        public virtual void Reset()
        {
            m_restartTimer.Enabled = false;
            m_watchTimer.Enabled = false;

            m_currentFiles.Clear();
            m_newFiles.Clear();

            m_watchTimer.Enabled = m_enabled;

            if (!m_session.IsConnected)
                RestartConnectCycle();
        }

        /// <summary>
        /// Raises <see cref="Status"/> event.
        /// </summary>
        protected void OnStatus(string status)
        {
            if (Status != null)
                Status(this, new EventArgs<string>("[" + DateTime.Now + "] " + status));
        }

        /// <summary>
        /// Raises <see cref="FileAdded"/> event.
        /// </summary>
        protected void OnFileAdded(FtpFile file)
        {
            if (FileAdded != null)
                FileAdded(this, new EventArgs<FtpFile>(file));
        }

        /// <summary>
        /// Raises <see cref="FileDeleted"/> event.
        /// </summary>
        protected void OnFileDeleted(FtpFile file)
        {
            if (FileDeleted != null)
                FileDeleted(this, new EventArgs<FtpFile>(file));
        }

        /// <summary>
        /// Raises <see cref="CommandSent"/> event.
        /// </summary>
        protected void OnCommandSent(string command)
        {
            if (CommandSent != null)
                CommandSent(this, new EventArgs<string>(command));
        }

        private void OnCommandSent(object sender, EventArgs<string> e)
        {
            OnCommandSent(e.Argument);
        }

        /// <summary>
        /// Raises <see cref="ResponseReceived"/> event.
        /// </summary>
        protected void OnResponseReceived(string response)
        {
            if (ResponseReceived != null)
                ResponseReceived(this, new EventArgs<string>(response));
        }

        private void OnResponseReceived(object sender, EventArgs<string> e)
        {
            OnCommandSent(e.Argument);
        }

        private void ConnectToWatchDirectory()
        {
            if (m_session.IsConnected)
            {
                m_session.SetCurrentDirectory(m_watchDirectory);

                if (m_watchDirectory.Length > 0)
                    OnStatus("FTP file watcher monitoring directory \"" + m_watchDirectory + "\"");
                else
                    OnStatus("No FTP file watcher directory specified - monitoring initial folder");
            }
        }

        // This method is synchronized in case user sets watch interval too tight...
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void WatchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // We attempt to access the FTP Session and refresh the current directory, if this fails
            // we are going to restart the connect cycle
            try
            {
                // Refresh the file listing for the current directory
                m_session.CurrentDirectory.Refresh();
            }
            catch (FtpExceptionBase ex)
            {
                RestartConnectCycle();
                OnStatus("FTP file watcher is no longer connected to server \"" + m_session.Server + "\" - restarting connect cycle." + "\r\n" + "\t" + "Exception: " + ex.Message);
            }

            if (m_session != null)
            {
                if (m_session.IsConnected)
                {
                    FtpFile newFile;
                    Dictionary<string, FtpFile>.ValueCollection.Enumerator currentFiles = m_session.CurrentDirectory.Files.GetEnumerator();
                    List<int> removedFiles = new List<int>();
                    int x, index;

                    // Check for new files
                    foreach (FtpFile currentFile in m_session.CurrentDirectory.Files)
                    {
                        if (m_notifyOnComplete)
                        {
                            // See if any new files are finished downloading
                            index = m_newFiles.BinarySearch(currentFile);

                            if (index >= 0)
                            {
                                newFile = m_newFiles[index];

                                if (newFile.Size == currentFile.Size)
                                {
                                    // File size has not changed since last directory refresh, so we will
                                    // notify user of new file...
                                    m_currentFiles.Add(currentFile);
                                    m_currentFiles.Sort();
                                    m_newFiles.RemoveAt(index);
                                    OnFileAdded(currentFile);
                                }
                                else
                                {
                                    newFile.Size = currentFile.Size;
                                }
                            }
                            else if (m_currentFiles.BinarySearch(currentFile) < 0)
                            {
                                m_newFiles.Add(currentFile);
                                m_newFiles.Sort();
                            }
                        }
                        else if (m_currentFiles.BinarySearch(currentFile) < 0)
                        {
                            // If user wants an immediate notification of new files, we'll give it to them...
                            m_currentFiles.Add(currentFile);
                            m_currentFiles.Sort();
                            OnFileAdded(currentFile);
                        }
                    }

                    // Check for removed files
                    for (x = 0; x <= m_currentFiles.Count - 1; x++)
                    {
                        if (m_session.CurrentDirectory.FindFile(m_currentFiles[x].Name) == null)
                        {
                            removedFiles.Add(x);
                            OnFileDeleted(m_currentFiles[x]);
                        }
                    }

                    // Remove files that have been deleted
                    if (removedFiles.Count > 0)
                    {
                        removedFiles.Sort();

                        // We remove items in desc order to maintain index integrity
                        for (x = removedFiles.Count - 1; x >= 0; x--)
                        {
                            m_currentFiles.RemoveAt(removedFiles[x]);
                        }

                        removedFiles.Clear();
                    }

                    m_watchTimer.Enabled = m_enabled;
                }
                else
                {
                    RestartConnectCycle();
                    OnStatus("FTP file watcher is no longer connected to server \"" + m_session.Server + "\" - restarting connect cycle.");
                }
            }
        }

        private void CloseSession()
        {
            try
            {
                m_session.Close();
            }
            catch
            {
            }
        }

        private void RestartConnectCycle()
        {
            m_restartTimer.Enabled = false;
            m_restartTimer.Interval = 10000;
            m_restartTimer.Enabled = true;
        }

        private void RestartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Attempt to close the FTP Session if it is open...
            CloseSession();

            // Try to reestablish connection
            m_watchTimer.Enabled = false;
            Connect(null, null);
        }

        #endregion
    }
}