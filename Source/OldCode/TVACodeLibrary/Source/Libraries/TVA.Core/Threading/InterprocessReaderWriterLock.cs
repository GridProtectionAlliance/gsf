//*******************************************************************************************************
//  InterprocessReaderWriterLock.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/21/2011 - Ritchie
//       Generated original version of source code.
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
using System.Threading;

namespace TVA.Threading
{
    /// <summary>
    /// Represents an interprocess reader/writer lock using <see cref="Semaphore"/> and <see cref="Mutex"/> native locking mechanisms.
    /// </summary>
    public class InterprocessReaderWriterLock : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default maximum concurrent locks allowed for <see cref="InterprocessReaderWriterLock"/>.
        /// </summary>
        public const int DefaultMaximumConcurrentLocks = 10;

        // Fields
        private Mutex m_semaphoreLock;          // Mutex used to synchronize access to Semaphore
        private Semaphore m_concurrencyLock;    // Semaphore used for reader/writer lock on consumer object
        private int m_maximumConcurrentLocks;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessReaderWriterLock"/> associated with the specified
        /// <paramref name="name"/> that identifies a source object needing concurrency locking.
        /// </summary>
        /// <param name="name">Identifiying name of source object needing concurrency locking (e.g., a path and file name).</param>
        public InterprocessReaderWriterLock(string name) : this(name, DefaultMaximumConcurrentLocks)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InterprocessReaderWriterLock"/> associated with the specified
        /// <paramref name="name"/> that identifies a source object needing concurrency locking.
        /// </summary>
        /// <param name="name">Identifiying name of source object needing concurrency locking (e.g., a path and file name).</param>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        /// <remarks>
        /// If more reader locks are requested than the <paramref name="maximumConcurrentLocks"/>, excess reader locks will simply
        /// wait until a lock is available (i.e., one of the existing reads completes).
        /// </remarks>
        public InterprocessReaderWriterLock(string name, int maximumConcurrentLocks)
        {
            m_maximumConcurrentLocks = maximumConcurrentLocks;
            m_semaphoreLock = InterprocessLock.GetNamedMutex(name);
            m_concurrencyLock = InterprocessLock.GetNamedSemaphore(name, m_maximumConcurrentLocks);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="InterprocessReaderWriterLock"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~InterprocessReaderWriterLock()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the maximum concurrent reader locks allowed.
        /// </summary>
        public int MaximumConcurrentLocks
        {
            get
            {
                return m_maximumConcurrentLocks;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="InterprocessReaderWriterLock"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="InterprocessReaderWriterLock"/> object and optionally releases the managed resources.
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
                        if (m_concurrencyLock != null)
                            m_concurrencyLock.Close();
                        m_concurrencyLock = null;

                        if (m_semaphoreLock != null)
                            m_semaphoreLock.Close();
                        m_semaphoreLock = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Tries to enter the lock in read mode.
        /// </summary>
        /// <remarks>
        /// Upon successful aquistion of a read lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitReadLock"/>.
        /// One <see cref="ExitReadLock"/> should be called for each <see cref="EnterReadLock"/> or <see cref="TryEnterReadLock"/>.
        /// </remarks>
        public void EnterReadLock()
        {
            TryEnterReadLock(Timeout.Infinite);
        }

        /// <summary>
        /// Tries to enter the lock in write mode.
        /// </summary>
        /// <remarks>
        /// Upon successful aquistion of a write lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitWriteLock"/>.
        /// One <see cref="ExitWriteLock"/> should be called for each <see cref="EnterWriteLock"/> or <see cref="TryEnterWriteLock"/>.
        /// </remarks>
        public void EnterWriteLock()
        {
            TryEnterWriteLock(Timeout.Infinite);
        }

        /// <summary>
        /// Exits read mode and returns the prior read lock count.
        /// </summary>
        /// <remarks>
        /// Upon successful aquistion of a read lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitReadLock"/>.
        /// One <see cref="ExitReadLock"/> should be called for each <see cref="EnterReadLock"/> or <see cref="TryEnterReadLock"/>.
        /// </remarks>
        public int ExitReadLock()
        {
            // Release the semaphore lock and restore the slot
            return m_concurrencyLock.Release();
        }

        /// <summary>
        /// Exits write mode.
        /// </summary>
        /// <remarks>
        /// Upon successful aquistion of a write lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitWriteLock"/>.
        /// One <see cref="ExitWriteLock"/> should be called for each <see cref="EnterWriteLock"/> or <see cref="TryEnterWriteLock"/>.
        /// </remarks>
        public void ExitWriteLock()
        {
            // Release semaphore synchronization mutex lock
            m_semaphoreLock.ReleaseMutex();
        }

        /// <summary>
        /// Tries to enter the lock in read mode, with an optional time-out.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 (<see cref="Timeout.Infinite"/>) to wait indefinitely.</param>
        /// <returns><c>true</c> if the calling thread entered read mode, otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Upon successful aquistion of a read lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitReadLock"/>.
        /// One <see cref="ExitReadLock"/> should be called for each <see cref="EnterReadLock"/> or <see cref="TryEnterReadLock"/>.
        /// </remarks>
        public bool TryEnterReadLock(int millisecondsTimeout)
        {
            bool success = false;

            try
            {
                // Wait for system level mutex lock to synchronize access to semaphore
                m_semaphoreLock.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                // Abnormal application terminations can leave a mutex abandoned, in
                // this case we now own the mutex so we just ignore the exception
            }

            try
            {
                // Wait for a semaphore slot to become available
                success = m_concurrencyLock.WaitOne(millisecondsTimeout);
            }
            finally
            {
                // Release mutex so others can access the semaphore
                m_semaphoreLock.ReleaseMutex();
            }

            return success;
        }

        /// <summary>
        /// Tries to enter the lock in write mode, with an optional time-out.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 (<see cref="Timeout.Infinite"/>) to wait indefinitely.</param>
        /// <returns><c>true</c> if the calling thread entered write mode, otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Upon successful aquistion of a write lock, use the <c>finally</c> block of a <c>try/finally</c> statement to call <see cref="ExitWriteLock"/>.
        /// One <see cref="ExitWriteLock"/> should be called for each <see cref="EnterWriteLock"/> or <see cref="TryEnterWriteLock"/>.
        /// </remarks>
        public bool TryEnterWriteLock(int millisecondsTimeout)
        {
            bool success = false;
            long startTime;

            try
            {
                // Wait for system level mutex lock to synchronize access to semaphore
                m_semaphoreLock.WaitOne();
            }
            catch (AbandonedMutexException)
            {
                // Abnormal application terminations can leave a mutex abandoned, in
                // this case we now own the mutex so we just ignore the exception
            }

            try
            {
                // At this point no other threads can aquire read or write access since we own the mutex.
                // Other threads may be busy reading, so we wait until all semaphore slots become available.
                // The only way to get a semaphore slot count is to execute a successful wait and release.
                startTime = DateTime.UtcNow.Ticks;

                if (success = m_concurrencyLock.WaitOne(millisecondsTimeout))
                {
                    int count = m_concurrencyLock.Release();
                    int adjustedTimeout = millisecondsTimeout;

                    // After a successful wait and release the returned semaphore slot count will be -1 of the
                    // actual count since we owned one slot after a successful wait.
                    while (success && count != m_maximumConcurrentLocks - 1)
                    {
                        // Sleep to allow any remaining reads to complete
                        Thread.Sleep(1);
                        
                        // Continue to adjust remaining time to accomodate user specified millisecond timeout
                        if (millisecondsTimeout > 0)
                            adjustedTimeout = millisecondsTimeout - (int)Ticks.ToMilliseconds(DateTime.UtcNow.Ticks - startTime);

                        if (adjustedTimeout < 0)
                            adjustedTimeout = 0;

                        if (success = m_concurrencyLock.WaitOne(adjustedTimeout))
                            count = m_concurrencyLock.Release();
                    }
                }
            }
            finally
            {
                // If lock failed, release mutex so others can access the semaphore
                if (!success)
                    m_semaphoreLock.ReleaseMutex();
            }

            // Successfully entering write lock leaves state of semphore locking mutex "owned", it is critical that
            // consumer call ExitWriteLock upon completion of write action to release mutex regardless of whether
            // their code succeeds or fails, that is, consumer should use the finally clause of a "try/finally"
            // expression to ExitWriteLock.
            return success;
        }

        #endregion
    }
}
