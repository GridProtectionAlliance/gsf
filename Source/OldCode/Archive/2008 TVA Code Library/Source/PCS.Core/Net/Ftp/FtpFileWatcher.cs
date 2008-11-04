//*******************************************************************************************************
//  FileWatcher.cs
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
    [ToolboxBitmap(typeof(FileWatcher)), DefaultProperty("Server"), DefaultEvent("FileAdded"), Description("Monitors for file changes over an FTP session")]
    public class FileWatcher : Component
    {
        #region [ Members ]

        // Events
        public event Action<FtpFile> FileAdded;
        public event Action<FtpFile> FileDeleted;
        public event Action<string> Status;
        public event Action<string> InternalSessionCommand;
        public event Action<string> InternalSessionResponse;

        // Fields
        protected Session m_session;
        protected string m_username;
        protected string m_password;
        protected string m_watchDirectory;
        protected System.Timers.Timer m_watchTimer;
        protected System.Timers.Timer m_restartTimer;
        protected List<FtpFile> m_currentFiles;
        protected List<FtpFile> m_newFiles;
        private bool m_enabled;
        private bool m_notifyOnComplete;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public FileWatcher()
        {
            m_enabled = true;
            m_notifyOnComplete = true;
            m_currentFiles = new List<FtpFile>();
            m_newFiles = new List<FtpFile>();

            m_session = new Session(false);
            m_session.CommandSent += Session_CommandSent;
            m_session.ResponseReceived += Session_ResponseReceived;

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

        public FileWatcher(bool caseInsensitive, bool notifyOnComplete)
            : this()
        {
            m_session.CaseInsensitive = caseInsensitive;
            m_notifyOnComplete = notifyOnComplete;
        }

        ~FileWatcher()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

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

        [Browsable(true), Category("Configuration"), Description("Set to True to not be case sensitive with FTP file names."), DefaultValue(false)]
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
        /// Sets flag for notification time: set to True to only notify when a file is finished uploading, set to False to get an immediate notification when a new file is detected.
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
                            m_session.CommandSent -= Session_CommandSent;
                            m_session.ResponseReceived -= Session_ResponseReceived;
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

        public virtual void Close()
        {
            m_currentFiles.Clear();
            m_newFiles.Clear();
            m_watchTimer.Enabled = false;
            m_restartTimer.Enabled = false;
            CloseSession();
        }

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

                if (Status != null)
                    Status("[" + DateTime.Now + "] FTP file watcher connected to \"ftp://" + m_username + "@" + m_session.Server + "\"");

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
                if (Status != null)
                    Status("[" + DateTime.Now + "] FTP file watcher failed to connect to \"ftp://" + m_username + "@" + m_session.Server + "\" - trying again in 10 seconds..." + "\r\n" + "\t" + "Exception: " + ex.Message);

                RestartConnectCycle();
            }
        }

        public virtual Session NewDirectorySession()
        {
            // This method is just for convenience.  We can't allow the end user to use the
            // actual internal directory for sending files or other work because it is
            // constantly being refreshed/used etc., so we instead create a new FTP Session
            // based on the current internal session and watch directory information
            Session newSession = new Session(m_session.CaseInsensitive);

            newSession.Server = m_session.Server;
            newSession.Connect(m_username, m_password);
            newSession.SetCurrentDirectory(m_watchDirectory);

            return newSession;
        }

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

        private void ConnectToWatchDirectory()
        {
            if (m_session.IsConnected)
            {
                m_session.SetCurrentDirectory(m_watchDirectory);

                if (m_watchDirectory.Length > 0)
                {
                    if (Status != null)
                        Status("[" + DateTime.Now + "] FTP file watcher monitoring directory \"" + m_watchDirectory + "\"");
                }
                else
                {
                    if (Status != null)
                        Status("[" + DateTime.Now + "] No FTP file watcher directory specified - monitoring initial folder");
                }
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

                if (Status != null)
                    Status("[" + DateTime.Now + "] FTP file watcher is no longer connected to server \"" + m_session.Server + "\" - restarting connect cycle." + "\r\n" + "\t" + "Exception: " + ex.Message);
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

                                    if (FileAdded != null)
                                        FileAdded(currentFile);
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

                            if (FileAdded != null)
                                FileAdded(currentFile);
                        }
                    }

                    // Check for removed files
                    for (x = 0; x <= m_currentFiles.Count - 1; x++)
                    {
                        if (m_session.CurrentDirectory.FindFile(m_currentFiles[x].Name) == null)
                        {
                            removedFiles.Add(x);
                            if (FileDeleted != null)
                                FileDeleted(m_currentFiles[x]);
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

                    if (Status != null)
                        Status("[" + DateTime.Now + "] FTP file watcher is no longer connected to server \"" + m_session.Server + "\" - restarting connect cycle.");
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

        private void Session_CommandSent(string Command)
        {
            if (InternalSessionCommand != null)
                InternalSessionCommand(Command);
        }

        private void Session_ResponseReceived(string Response)
        {
            if (InternalSessionResponse != null)
                InternalSessionResponse(Response);
        }

        #endregion
    }
}