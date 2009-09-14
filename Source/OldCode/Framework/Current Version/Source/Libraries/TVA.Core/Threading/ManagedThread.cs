//*******************************************************************************************************
//  ManagedThread.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/25/2008 - J. Ritchie Carroll
//       Initial version of source generated.
//  09/11/2008 - J. Ritchie Carroll
//       Converted to C#.
//  08/10/2009 - Josh L. Patterson
//       Edited Comments.
//  9/14/2009 - Stephen C. Wills
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
using System.Threading;
using TVA.Units;

namespace TVA.Threading
{
    #region [ Enumerations ]

    /// <summary>
    /// Managed Thread Types
    /// </summary>
    public enum ThreadType
    {
        /// <summary>Standard thread created with public constructor</summary>
        StandardThread,
        /// <summary>Queued thread added into managed thread pool</summary>
        QueuedThread
    }

    /// <summary>
    /// Managed Thread States
    /// </summary>
    public enum ThreadStatus
    {
        /// <summary>Thread created, not started</summary>
        Unstarted,
        /// <summary>Thread queued for execution</summary>
        Queued,
        /// <summary>Thread start requested, execution pending</summary>
        Started,
        /// <summary>Thread executing</summary>
        Executing,
        /// <summary>Thread completed</summary>
        Completed,
        /// <summary>Thread aborted</summary>
        Aborted
    }

    #endregion

    /// <summary>
    /// Defines a managed thread
    /// </summary>
    /// <remarks>
    /// This class works like any normal thread but provides the benefit of automatic tracking
    /// through the ManagedThreads collection, total thread runtime and the ability to run
    /// the thread in an alternate execution context
    /// </remarks>
    public sealed class ManagedThread
    {
        #region [ Members ]

        // Fields
        private Thread m_thread;
        private ThreadType m_type;
        private ThreadStatus m_status;
        private string m_name;
        private Ticks m_startTime;
        private Ticks m_stopTime;
        private ContextCallback m_ctxCallback;
        private ThreadStart m_tsCallback;
        private ParameterizedThreadStart m_ptsCallback;
        private ExecutionContext m_ctx;
        private object m_state;
        private object m_tag;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the ManagedThread class.
        /// </summary>
        /// <param name="callback">A <see cref="ThreadStart"/> object.</param>
        public ManagedThread(ThreadStart callback)
            : this(ThreadType.StandardThread, callback, null, null)
        {
            m_thread = new Thread(HandleItem);
        }

        /// <summary>
        /// Initializes a new instance of the ManagedThread class, specifying a delegate that allows an object to be passed to the thread when the thread is started.
        /// </summary>
        /// <param name="callback">A <see cref="ParameterizedThreadStart"/> object.</param>
        public ManagedThread(ParameterizedThreadStart callback)
            : this(ThreadType.StandardThread, callback, null, null)
        {
            m_thread = new Thread(HandleItem);
        }

        /// <summary>
        /// Initializes a new instance of the ManagedThread class, specifying a delegate that allows an object to be passed to the thread when the thread is started
        /// and allowing the user to specify an alternate execution context for the thread.
        /// </summary>
        /// <param name="callback">A <see cref="ContextCallback"/> delgate to execute.</param>
        /// <param name="ctx">An <see cref="ExecutionContext"/>.</param>
        public ManagedThread(ContextCallback callback, ExecutionContext ctx)
            : this(ThreadType.StandardThread, callback, null, ctx)
        {
            m_thread = new Thread(HandleItem);
        }

        internal ManagedThread(ThreadType type, ThreadStart callback, object state, ExecutionContext ctx)
        {
            m_type = type;
            m_status = (type == ThreadType.QueuedThread ? ThreadStatus.Queued : ThreadStatus.Unstarted);
            m_tsCallback = callback;
            m_state = state;
            m_ctx = ctx;
        }

        internal ManagedThread(ThreadType type, ParameterizedThreadStart callback, object state, ExecutionContext ctx)
        {
            m_type = type;
            m_status = (type == ThreadType.QueuedThread ? ThreadStatus.Queued : ThreadStatus.Unstarted);
            m_ptsCallback = callback;
            m_state = state;
            m_ctx = ctx;
        }

        internal ManagedThread(ThreadType type, ContextCallback callback, object state, ExecutionContext ctx)
        {
            m_type = type;
            m_status = (type == ThreadType.QueuedThread ? ThreadStatus.Queued : ThreadStatus.Unstarted);
            m_ctxCallback = callback;
            m_state = state;
            m_ctx = ctx;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// An object containing data to be used by the thread's execution method.
        /// </summary>
        public object State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        /// <summary>
        /// An object that allows additional user defined information to be tracked along with this thread.
        /// </summary>
        public object Tag
        {
            get
            {
                return m_tag;
            }
            set
            {
                m_tag = value;
            }
        }

        /// <summary>
        /// Returns the managed thread type (either StandardThread or QueuedThread)
        /// </summary>
        public ThreadType Type
        {
            get
            {
                return m_type;
            }
        }

        /// <summary>
        /// Gets a value containing the curretn status of the current thread.
        /// </summary>
        public ThreadStatus Status
        {
            get
            {
                return m_status;
            }
            internal set
            {
                m_status = value;
            }
        }

        /// <summary>
        /// Gets a value indicating the execution status of the current thread.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return (m_status == ThreadStatus.Started || m_status == ThreadStatus.Executing);
            }
        }

        /// <summary>
        /// Gets or sets the name of the thread.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;

                if (m_type == ThreadType.StandardThread)
                    m_thread.Name = value;
            }
        }

        /// <summary>
        /// Get the time, in ticks, that the thread started executing
        /// </summary>
        public Ticks StartTime
        {
            get
            {
                return m_startTime;
            }
        }

        /// <summary>
        /// Get the time, in ticks, that the thread finished executing
        /// </summary>
        public Ticks StopTime
        {
            get
            {
                return m_stopTime;
            }
        }

        /// <summary>
        /// Gets the total amount of time, in seconds, that the managed thread has been active.
        /// </summary>
        public Time RunTime
        {
            get
            {
                Ticks processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                        processingTime = m_stopTime - m_startTime;
                    else
                        processingTime = DateTime.UtcNow.Ticks - m_startTime;
                }

                if (processingTime < 0)
                    processingTime = 0;

                return processingTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of a thread.
        /// </summary>
        /// <returns>One of the ThreadPriority values. The default value is Normal.</returns>
        /// <remarks>
        /// Changing of this value is only available to standard threads - you can't change the priorty of queued threads since they are already
        /// allocated and owned by the .NET thread pool.
        /// </remarks>
        public ThreadPriority Priority
        {
            get
            {
                if (m_type == ThreadType.QueuedThread)
                {
                    return ThreadPriority.Normal;
                }
                else
                {
                    return m_thread.Priority;
                }
            }
            set
            {
                if (m_type == ThreadType.QueuedThread)
                    throw new InvalidOperationException("Cannot change priority of a thread that was queued into thread pool.");

                m_thread.Priority = value;
            }
        }

        internal Thread Thread
        {
            get
            {
                return m_thread;
            }
            set
            {
                m_thread = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, to begin the process of terminating the thread. Calling this method usually terminates the thread.
        /// </summary>
        public void Abort()
        {
            ManagedThreads.Cancel(this, true, null);
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, to begin the process of terminating the thread. Calling this method usually terminates the thread.
        /// </summary>
        /// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted.</param>
        public void Abort(object stateInfo)
        {
            ManagedThreads.Cancel(this, true, stateInfo);
        }

        /// <summary>
        /// Causes a thread to be scheduled for execution.
        /// </summary>
        public void Start()
        {
            if (m_type == ThreadType.QueuedThread)
                throw new InvalidOperationException("Cannot manually start a thread that was queued into thread pool.");

            ManagedThreads.Add(this);

            m_thread.Start();
        }

        /// <summary>
        /// Causes a thread to be scheduled for execution.
        /// </summary>
        /// <param name="parameter">An object that contains data to be used by the method the thread executes.</param>
        public void Start(object parameter)
        {
            if (m_type == ThreadType.QueuedThread)
                throw new InvalidOperationException("Cannot manually start a thread that was queued into thread pool.");

            m_state = parameter;

            ManagedThreads.Add(this);

            m_thread.Start();
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        /// <remarks>
        /// This is only available for standard threads - queued threads don't have an associated thread until they are executing.
        /// </remarks>
        public void Join()
        {
            if (m_type == ThreadType.QueuedThread)
                throw new InvalidOperationException("Cannot join a thread that was queued into thread pool.");

            if (!IsAlive)
                throw new InvalidOperationException("Cannot join a thread that has not been started.");

            m_thread.Join();
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for the thread to terminate. </param>
        /// <returns>true if the thread has terminated; false if the thread has not terminated after the amount of time specified by the millisecondsTimeout parameter has elapsed.</returns>
        /// <remarks>
        /// This is only available for standard threads - queued threads don't have an associated thread until they are executing.
        /// </remarks>
        public bool Join(int millisecondsTimeout)
        {
            if (m_type == ThreadType.QueuedThread)
                throw new InvalidOperationException("Cannot join a thread that was queued into thread pool.");

            if (!IsAlive)
                throw new InvalidOperationException("Cannot join a thread that has not been started.");

            return m_thread.Join(millisecondsTimeout);
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        /// <param name="timeout">A TimeSpan set to the amount of time to wait for the thread to terminate. </param>
        /// <returns>true if the thread terminated; false if the thread has not terminated after the amount of time specified by the timeout parameter has elapsed.</returns>
        /// <remarks>
        /// This is only available for standard threads - queued threads don't have an associated thread until they are executing.
        /// </remarks>
        public bool Join(TimeSpan timeout)
        {
            return Join((int)timeout.TotalMilliseconds);
        }

        internal void HandleItem()
        {
            // Set start state
            m_startTime = DateTime.UtcNow.Ticks;
            m_status = ThreadStatus.Executing;

            try
            {
                // Invoke the user's call back function
                if (m_ctx == null)
                {
                    if (m_tsCallback != null)
                    {
                        m_tsCallback.Invoke();
                    }
                    else if (m_ptsCallback != null)
                    {
                        m_ptsCallback.Invoke(m_state);
                    }
                    else
                    {
                        m_ctxCallback.Invoke(m_state);
                    }
                }
                else
                {
                    // If user specified an alternate execution context, we invoke
                    // their delegate under that context
                    ExecutionContext.Run(m_ctx, m_ctxCallback, m_state);
                }
            }
            finally
            {
                // Set finish state
                if (m_status == ThreadStatus.Executing)
                {
                    m_status = ThreadStatus.Completed;
                }
                m_stopTime = DateTime.UtcNow.Ticks;

                ManagedThreads.Remove(this);
            }
        }

        #endregion
   }
}