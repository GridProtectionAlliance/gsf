//*******************************************************************************************************
//  FtpFileWatcher.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/22/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace TVA.Net.Ftp
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
        /// <see cref="EventArgs{T}.Argument"/> is reference to newly added file.
        /// </remarks>
        public event EventHandler<EventArgs<FtpFile>> FileAdded;

        /// <summary>
        /// Raised when file is deleted from monitored FTP directory.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is reference to file that was removed.
        /// </remarks>
        public event EventHandler<EventArgs<FtpFile>> FileDeleted;

        /// <summary>
        /// Raised when new status messages come from the FTP file watcher.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is status message from FTP file watcher.
        /// </remarks>
        public event EventHandler<EventArgs<string>> Status;

        /// <summary>
        /// Raised when FTP command has been sent.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is sent FTP command.
        /// </remarks>
        public event EventHandler<EventArgs<string>> CommandSent;

        /// <summary>
        /// Raised when FTP response has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is received FTP response.
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
            : base()
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
        /// Initializes a new instance of the <see cref="FtpFileWatcher"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="FtpFileWatcher"/>.</param>
        public FtpFileWatcher(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
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
        /// <param name="userName">A <see cref="String"/> value as the username.</param>
        /// <param name="password">A <see cref="String"/> value as the password.</param>
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
        /// <param name="status">A <see cref="String"/> status message.</param>
        protected void OnStatus(string status)
        {
            if (Status != null)
                Status(this, new EventArgs<string>("[" + DateTime.Now + "] " + status));
        }

        /// <summary>
        /// Raises <see cref="FileAdded"/> event.
        /// </summary>
        /// <param name="file">A <see cref="FtpFile"/> file.</param>
        protected void OnFileAdded(FtpFile file)
        {
            if (FileAdded != null)
                FileAdded(this, new EventArgs<FtpFile>(file));
        }

        /// <summary>
        /// Raises <see cref="FileDeleted"/> event.
        /// </summary>
        /// <param name="file">A <see cref="FtpFile"/> file.</param>
        protected void OnFileDeleted(FtpFile file)
        {
            if (FileDeleted != null)
                FileDeleted(this, new EventArgs<FtpFile>(file));
        }

        /// <summary>
        /// Raises <see cref="CommandSent"/> event.
        /// </summary>
        /// <param name="command">A <see cref="String"/> command.</param>
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
        /// <param name="response">A <see cref="String"/> response.</param>
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
                    //Dictionary<string, FtpFile>.ValueCollection.Enumerator currentFiles = m_session.CurrentDirectory.Files.GetEnumerator();
                    FtpFile newFile;
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
                    for (x = 0; x < m_currentFiles.Count; x++)
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