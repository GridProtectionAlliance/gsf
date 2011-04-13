//*******************************************************************************************************
//  InterprocessCache.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/21/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  04/06/2011 - J. Ritchie Carroll
//       Added Flush() method to block current thread and wait for any pending save to complete.
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

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using System.Collections;
using System.IO;
using System.Threading;
using TVA.Collections;
using TVA.Threading;

namespace TVA.IO
{
    /// <summary>
    /// Represents a serialized data cache that can be saved or read from multiple applications using interprocess synchronization.
    /// </summary>
    /// <remarks>
    /// Note that all file data in this class gets serialized to and from memory, as such, the design intention for this class is for
    /// use with smaller data sets such as serialized lists or dictionaries that need interprocess synchronized loading and saving.
    /// </remarks>
    public class InterprocessCache : IDisposable
    {
        #region [ Members ]

        // Constants
        private const int WriteEvent = 0;
        private const int ReadEvent = 1;

        /// <summary>
        /// Default maximum retry attempts allowed for loading <see cref="InterprocessCache"/>.
        /// </summary>
        public const int DefaultMaximumRetryAttempts = 10;

        /// <summary>
        /// Default wait interval, in milliseconds, before retrying load of <see cref="InterprocessCache"/>.
        /// </summary>
        public const double DefaultRetryDelayInterval = 200.0D;

        // Fields
        private string m_fileName;                          // Path and file name of file needing interprocess synchronization
        private byte[] m_fileData;                          // Data loaded or to be saved
        private bool m_autoSave;                            // Flag to auto save when file data has changed
        private InterprocessReaderWriterLock m_fileLock;    // Interprocess reader/writer lock used to synchronize file access
        private ReaderWriterLockSlim m_dataLock;            // Thread level reader/writer lock used to synchronize file data access
        private ManualResetEventSlim m_loadIsReady;         // Wait handle used so that system will wait for file data load
        private ManualResetEventSlim m_saveIsReady;         // Wait handle used so that system will wait for file data save
        private FileSystemWatcher m_fileWatcher;            // Optional file watcher used to reload changes
        private int m_maximumConcurrentLocks;               // Maximum concurrent reader locks allowed
        private int m_maximumRetryAttempts;                 // Maximum retry attempts allowed for loading file
        private BitArray m_retryQueue;                      // Retry event queue
        private System.Timers.Timer m_retryTimer;           // File I/O retry timer
        private long m_lastRetryTime;                       // Time of last retry attempt
        private int m_retryCount;                           // Total number of retries attempted so far
        private bool m_disposed;                            // Class disposed flag

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessCache"/>.
        /// </summary>
        public InterprocessCache()
            : this(InterprocessReaderWriterLock.DefaultMaximumConcurrentLocks)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessCache"/> with the specified number of <paramref name="maximumConcurrentLocks"/>.
        /// </summary>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        public InterprocessCache(int maximumConcurrentLocks)
        {
            // Initialize field values
            m_dataLock = new ReaderWriterLockSlim();
            m_loadIsReady = new ManualResetEventSlim(false);
            m_saveIsReady = new ManualResetEventSlim(true);
            m_maximumConcurrentLocks = maximumConcurrentLocks;
            m_maximumRetryAttempts = DefaultMaximumRetryAttempts;
            m_retryQueue = new BitArray(2);
            m_fileData = new byte[0];

            // Setup retry timer
            m_retryTimer = new System.Timers.Timer();
            m_retryTimer.Elapsed += m_retryTimer_Elapsed;
            m_retryTimer.AutoReset = false;
            m_retryTimer.Interval = DefaultRetryDelayInterval;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="InterprocessCache"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~InterprocessCache()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Path and file name for the cache needing interprocess synchronization.
        /// </summary>
        public virtual string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("FileName", "FileName cannot be null");

                m_fileName = FilePath.GetAbsolutePath(value);

                // Initialize reader/writer lock for given file name
                if (m_fileLock != null)
                    m_fileLock.Dispose();

                m_fileLock = new InterprocessReaderWriterLock(m_fileName, m_maximumConcurrentLocks);
            }
        }

        /// <summary>
        /// Gets or sets file data for the cache to be saved or that has been loaded.
        /// </summary>
        /// <remarks>
        /// Setting value to <c>null</c> will create a zero-length file.
        /// </remarks>
        public virtual byte[] FileData
        {
            get
            {
                // Calls to this property are blocked until data is available
                WaitForLoad();

                m_dataLock.EnterReadLock();

                try
                {
                    // Make a copy of the file data for external use
                    if (m_fileData != null)
                        return m_fileData.Copy(0, m_fileData.Length);
                }
                finally
                {
                    m_dataLock.ExitReadLock();
                }

                return null;
            }
            set
            {
                if (m_fileName == null)
                    throw new ArgumentNullException("FileName", "FileName property must be defined before setting FileData");

                bool dataChanged = false;

                // If value is null, assume user means zero-length file
                if (value == null)
                    value = new byte[0];

                m_dataLock.EnterWriteLock();

                try
                {
                    if (m_autoSave)
                        dataChanged = (m_fileData.CompareTo(value) != 0);

                    m_fileData = value;
                }
                finally
                {
                    m_dataLock.ExitWriteLock();
                }

                // Initiate save if data has changed
                if (dataChanged)
                {
                    m_saveIsReady.Reset();
                    ThreadPool.QueueUserWorkItem(SynchronizedWrite);
                }
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="InterprocessCache"/> should automatically initiate a save when <see cref="FileData"/> has been updated.
        /// </summary>
        public virtual bool AutoSave
        {
            get
            {
                return m_autoSave;
            }
            set
            {
                m_autoSave = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that enables system to monitor for changes in <see cref="FileName"/> and automatically reload <see cref="FileData"/>.
        /// </summary>
        public virtual bool ReloadOnChange
        {
            get
            {
                return m_fileWatcher != null;
            }
            set
            {
                if (value && m_fileWatcher == null)
                {
                    if (m_fileName == null)
                        throw new ArgumentNullException("FileName", "FileName property must be defined before enabling ReloadOnChange");

                    // Setup file watcher to monitor for external updates
                    m_fileWatcher = new FileSystemWatcher();
                    m_fileWatcher.Path = FilePath.GetDirectoryName(m_fileName);
                    m_fileWatcher.Filter = FilePath.GetFileName(m_fileName);
                    m_fileWatcher.EnableRaisingEvents = true;
                    m_fileWatcher.Changed += m_fileWatcher_Changed;
                }
                else if (!value && m_fileWatcher != null)
                {
                    // Disable file watcher
                    m_fileWatcher.Changed -= m_fileWatcher_Changed;
                    m_fileWatcher.Dispose();
                    m_fileWatcher = null;
                }
            }
        }

        /// <summary>
        /// Gets the maximum concurrent reader locks allowed.
        /// </summary>
        public virtual int MaximumConcurrentLocks
        {
            get
            {
                return m_maximumConcurrentLocks;
            }
        }

        /// <summary>
        /// Maximum retry attempts allowed for loading or saving cache file data.
        /// </summary>
        public virtual int MaximumRetryAttempts
        {
            get
            {
                return m_maximumRetryAttempts;
            }
            set
            {
                m_maximumRetryAttempts = value;
            }
        }

        /// <summary>
        /// Wait interval, in milliseconds, before retrying load or save of cache file data.
        /// </summary>
        public virtual double RetryDelayInterval
        {
            get
            {
                return m_retryTimer.Interval;
            }
            set
            {
                m_retryTimer.Interval = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="InterprocessCache"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="InterprocessCache"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_fileWatcher != null)
                        {
                            m_fileWatcher.Changed -= m_fileWatcher_Changed;
                            m_fileWatcher.Dispose();
                        }
                        m_fileWatcher = null;

                        if (m_retryTimer != null)
                        {
                            m_retryTimer.Elapsed -= m_retryTimer_Elapsed;
                            m_retryTimer.Dispose();
                        }
                        m_retryTimer = null;

                        if (m_loadIsReady != null)
                            m_loadIsReady.Dispose();

                        m_loadIsReady = null;

                        if (m_saveIsReady != null)
                            m_saveIsReady.Dispose();

                        m_saveIsReady = null;

                        if (m_dataLock != null)
                            m_dataLock.Dispose();

                        m_dataLock = null;

                        if (m_fileLock != null)
                            m_fileLock.Dispose();

                        m_fileLock = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Initiates interprocess synchronized cache file save.
        /// </summary>
        public virtual void Save()
        {
            if (m_fileName == null)
                throw new ArgumentNullException("FileName", "FileName is null, cannot initiate save");

            if (m_fileData == null)
                throw new ArgumentNullException("FileData", "FileData is null, cannot initiate save");

            m_saveIsReady.Reset();
            ThreadPool.QueueUserWorkItem(SynchronizedWrite);
        }

        /// <summary>
        /// Initiates interprocess synchronized cache file load.
        /// </summary>
        public virtual void Load()
        {
            if (m_fileName == null)
                throw new ArgumentNullException("FileName", "FileName is null, cannot initiate load");

            m_loadIsReady.Reset();
            ThreadPool.QueueUserWorkItem(SynchronizedRead);
        }

        /// <summary>
        /// Blocks current thread and waits for any pending load to complete; wait time is <c><see cref="RetryDelayInterval"/> * <see cref="MaximumRetryAttempts"/></c>.
        /// </summary>
        public virtual void WaitForLoad()
        {
            WaitForLoad((int)(RetryDelayInterval * MaximumRetryAttempts));
        }

        /// <summary>
        /// Blocks current thread and waits for specified <paramref name="millisecondsTimeout"/> for any pending load to complete.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        public virtual void WaitForLoad(int millisecondsTimeout)
        {
            // Calls to this method are blocked until data is available
            if (!m_loadIsReady.IsSet && !m_loadIsReady.Wait(millisecondsTimeout))
                throw new TimeoutException("Timeout waiting to read data from " + m_fileName);
        }

        /// <summary>
        /// Blocks current thread and waits for any pending save to complete; wait time is <c><see cref="RetryDelayInterval"/> * <see cref="MaximumRetryAttempts"/></c>.
        /// </summary>
        public virtual void WaitForSave()
        {
            WaitForSave((int)(RetryDelayInterval * MaximumRetryAttempts));
        }

        /// <summary>
        /// Blocks current thread and waits for specified <paramref name="millisecondsTimeout"/> for any pending save to complete.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        public virtual void WaitForSave(int millisecondsTimeout)
        {
            // Calls to this method are blocked until data is saved
            if (!m_saveIsReady.IsSet && !m_saveIsReady.Wait(millisecondsTimeout))
                throw new TimeoutException("Timeout waiting to save data to " + m_fileName);
        }

        /// <summary>
        /// Handles serialization of file to disk; virtual method allows customization (e.g., pre-save encryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to serialize data.</param>
        /// <param name="fileData">File data to be serialized.</param>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected virtual void SaveFileData(FileStream fileStream, byte[] fileData)
        {
            fileStream.Write(fileData, 0, fileData.Length);
        }

        /// <summary>
        /// Handles deserialization of file from disk; virtual method allows customization (e.g., pre-load decryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to deserialize data.</param>
        /// <returns>Deserialized file data.</returns>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected virtual byte[] LoadFileData(FileStream fileStream)
        {
            return fileStream.ReadStream();
        }

        /// <summary>
        /// Synchronously writes file data when no reads are active.
        /// </summary>
        private void SynchronizedWrite(object state)
        {
            if (!m_disposed)
            {
                if (m_fileLock.TryEnterWriteLock((int)m_retryTimer.Interval))
                {
                    FileStream fileStream = null;

                    try
                    {
                        fileStream = new FileStream(m_fileName, FileMode.Create, FileAccess.Write, FileShare.None);

                        if (m_dataLock.TryEnterReadLock((int)m_retryTimer.Interval))
                        {
                            try
                            {
                                // Disable file watch notification before update
                                if (m_fileWatcher != null)
                                    m_fileWatcher.EnableRaisingEvents = false;

                                SaveFileData(fileStream, m_fileData);
                                m_saveIsReady.Set();
                            }
                            finally
                            {
                                m_dataLock.ExitReadLock();

                                // Reenable file watch notification
                                if (m_fileWatcher != null)
                                    m_fileWatcher.EnableRaisingEvents = true;
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        RetrySynchronizedEvent(ex, WriteEvent);
                    }
                    finally
                    {
                        m_fileLock.ExitWriteLock();

                        if (fileStream != null)
                            fileStream.Close();
                    }
                }
                else
                {
                    RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire write lock for " + m_fileName), WriteEvent);
                }
            }
        }

        /// <summary>
        /// Synchronously reads file data when no writes are active.
        /// </summary>
        private void SynchronizedRead(object state)
        {
            if (!m_disposed)
            {
                if (File.Exists(m_fileName))
                {
                    if (m_fileLock.TryEnterReadLock((int)m_retryTimer.Interval))
                    {
                        FileStream fileStream = null;

                        try
                        {
                            fileStream = new FileStream(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                            if (m_dataLock.TryEnterWriteLock((int)m_retryTimer.Interval))
                            {
                                try
                                {
                                    m_fileData = LoadFileData(fileStream);
                                }
                                finally
                                {
                                    m_dataLock.ExitWriteLock();
                                }

                                // Release any threads waiting for file data
                                m_loadIsReady.Set();
                            }
                        }
                        catch (IOException ex)
                        {
                            RetrySynchronizedEvent(ex, ReadEvent);
                        }
                        finally
                        {
                            m_fileLock.ExitReadLock();

                            if (fileStream != null)
                                fileStream.Close();
                        }
                    }
                    else
                    {
                        RetrySynchronizedEvent(new TimeoutException("Timeout waiting to acquire read lock for " + m_fileName), ReadEvent);
                    }
                }
                else
                {
                    // File doesn't exist, create ane empty array representing a zero-length file
                    m_fileData = new byte[0];

                    // Release any threads waiting for file data
                    m_loadIsReady.Set();
                }
            }
        }

        /// <summary>
        /// Initiates a retry for specified event type.
        /// </summary>
        /// <param name="ex">Exception causing retry.</param>
        /// <param name="eventType">Event type to retry.</param>
        private void RetrySynchronizedEvent(Exception ex, int eventType)
        {
            // A retry is only being initiating for basic file I/O or locking errors, all other errors will initiate an unhandled
            // exception causing system exit. It would be an error, IMO, for the system to create values then not be able to load
            // them at next run or not be able to use values from last run because file could not be loaded.
            if (!m_disposed)
            {
                // We monitor basic I/O and lock failures occurring in quick succession, we can't allow retry activity to go on forever
                if (DateTime.UtcNow.Ticks - m_lastRetryTime > (long)Ticks.FromMilliseconds(m_retryTimer.Interval * m_maximumRetryAttempts))
                {
                    // Significant time has passed since last retry, so we reset counter
                    m_retryCount = 0;
                    m_lastRetryTime = DateTime.UtcNow.Ticks;
                }
                else
                {
                    m_retryCount++;

                    if (m_retryCount >= m_maximumRetryAttempts)
                        throw new UnauthorizedAccessException("Failed to " + (eventType == WriteEvent ? "write" : "read") + " data to " + m_fileName + " after " + m_maximumRetryAttempts + " attempts: " + ex.Message, ex);
                }

                // Technically the interprocess mutex will handle serialized access to the file, but if the OS or other process
                // not participating with the mutex has the file locked, all we can do is queue up a retry for this event.
                lock (m_retryQueue)
                {
                    m_retryQueue[eventType] = true;
                }
                m_retryTimer.Start();
            }
        }

        /// <summary>
        /// Retries specified serialize or deserialize event in case of file I/O failures.
        /// </summary>
        private void m_retryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!m_disposed)
            {
                WaitCallback callBackEvent = null;

                lock (m_retryQueue)
                {
                    // Reads should always occur first since you may need to load any
                    // newly written data before saving new data: users can override
                    // load and save behavior to "merge" data sets if needed.
                    if (m_retryQueue[ReadEvent])
                    {
                        callBackEvent = SynchronizedRead;
                        m_retryQueue[ReadEvent] = false;
                    }
                    else if (m_retryQueue[WriteEvent])
                    {
                        callBackEvent = SynchronizedWrite;
                        m_retryQueue[WriteEvent] = false;
                    }

                    // If any events remain queued for retry, start timer for next event
                    if (m_retryQueue.Any(true))
                        m_retryTimer.Start();
                }

                if (callBackEvent != null)
                    ThreadPool.QueueUserWorkItem(callBackEvent);
            }
        }

        /// <summary>
        /// Reload file upon external modification.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">An object which provides data for directory events.</param>
        private void m_fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
                Load();
        }

        #endregion
    }
}
