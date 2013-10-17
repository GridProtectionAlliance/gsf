//*******************************************************************************************************
//  ProcessQueue.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/07/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  02/12/2006 - J. Ritchie Carroll
//       Added multi-item bulk processing functionality.
//  03/21/2007 - J. Ritchie Carroll
//       Added "ItemsBeingProcessed" property to return current total number of items being processed.
//       Added "Flush" method to allow any remaining items in queue to be processed before shutdown.
//  04/05/2007 - J. Ritchie Carroll
//       Added "RequeueMode" properties to allow users to specify how data gets reinserted back into
//       the list (prefix or suffix) after processing timeouts or exceptions.
//  07/12/2007 - Pinal C. Patel
//       Modified the code for "Flush" method to correctly implement IDisposable interface.
//  08/01/2007 - J. Ritchie Carroll
//       Added some minor optimizations where practical.
//  08/17/2007 - J. Ritchie Carroll
//       Removed IDisposable implementation because of continued flushing errors.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  11/05/2007 - J. Ritchie Carroll
//       Modified flush to complete tasks on calling thread - this avoids errors when timer
//       gets disposed before flush call.
//  02/20/2008 - J. Ritchie Carroll
//       Implemented standard IDisposable pattern.
//  09/11/2008 - J. Ritchie Carroll
//       Converted to C#.
//  11/06/2008 - J. Ritchie Carroll
//       Added CurrentStatistics property to return run-time statistics as a group.
//  02/23/2009 - Josh L. Patterson
//       Edited Code Comments.
//  08/05/2009 - Josh L. Patterson
//       Edited Code Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  01/04/2010 - J. Ritchie Carroll
//       Removed hard thread abort from shutdown which will allow current processing items
//       to complete before terminating thread.
//  06/21/2010 - Stephen C. Wills
//       Modified Dispose to fix potential concurrency issues.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  01/24/2012 - Pinal C. Patel
//       Modified ProcessTimerThreadProc() method to perform null reference check to avoid an exception
//       when the object is being disposed..
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
 Original Software Title: The GSF Open Source Phasor Data Concentrator
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

using GSF.Units;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GSF.Collections
{
    #region [ Enumerations ]

    /// <summary>
    /// Enumeration of possible <see cref="ProcessQueue{T}"/> threading modes.
    /// </summary>
    public enum QueueThreadingMode
    {
        /// <summary>
        /// Processes several items in the <see cref="ProcessQueue{T}"/> at once on different threads, where processing order is not important.
        /// </summary>
        Asynchronous,
        /// <summary>
        /// Processes items in the <see cref="ProcessQueue{T}"/> one at a time on a single thread, where processing order is important.
        /// </summary>
        Synchronous
    }

    /// <summary>
    /// Enumeration of possible <see cref="ProcessQueue{T}"/> processing styles.
    /// </summary>
    public enum QueueProcessingStyle
    {
        /// <summary>
        /// Defines <see cref="ProcessQueue{T}"/> processing delegate to process only one item at a time.
        /// </summary>
        /// <remarks>
        /// This is the typical <see cref="QueueProcessingStyle"/> when the <see cref="QueueThreadingMode"/> is asynchronous.
        /// </remarks>
        OneAtATime,
        /// <summary>
        /// Defines <see cref="ProcessQueue{T}"/> processing delegate to process all currently available items in the <see cref="ProcessQueue{T}"/>.
        /// Items are passed into delegate as an array.
        /// </summary>
        /// <remarks>
        /// This is the optimal <see cref="QueueProcessingStyle"/> when the <see cref="QueueThreadingMode"/> is synchronous.
        /// </remarks>
        ManyAtOnce
    }

    /// <summary>
    /// Enumeration of possible requeue reasons.
    /// </summary>
    public enum RequeueReason
    {
        /// <summary>
        /// Requeuing item since it cannot be processed at this time.
        /// </summary>
        CannotProcess,
        /// <summary>
        /// Requeuing item due to an exception.
        /// </summary>
        Exception,
        /// <summary>
        /// Requeing item due to timeout.
        /// </summary>
        Timeout
    }

    #endregion

    /// <summary>
    /// Represents a lock-free thread-safe collection of items, based on <see cref="ConcurrentQueue{T}"/>, that get processed on independent threads with a consumer provided function.
    /// </summary>
    /// <typeparam name="T">Type of object to process</typeparam>
    /// <remarks>
    /// <para>This class acts as a strongly-typed collection of objects to be processed.</para>
    /// <para>Consumers are expected to create new instances of this class through the static construction functions (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    /// <para>Note that the <see cref="ProcessQueue{T}"/> will not start processing until the <see cref="ProcessQueue{T}.Start"/> method is called.</para>
    /// </remarks>
    public class ProcessQueue<T> : IProducerConsumerCollection<T>, IProvideStatus, ISupportLifecycle
    {
        #region [ Members ]

        // Nested Types

        // Limits item processing time, if requested.
        private sealed class TemporalTask : IDisposable
        {
            private ProcessQueue<T> m_parent;
            private ManualResetEventSlim m_eventHandle;
            private T m_item;
            private T[] m_items;
            private bool m_disposed;

            private TemporalTask(ProcessQueue<T> parent, T item)
            {
                m_parent = parent;
                m_item = item;
                m_eventHandle = new ManualResetEventSlim(false);
                ThreadPool.QueueUserWorkItem(state => ProcessItem());
            }

            private TemporalTask(ProcessQueue<T> parent, T[] items)
            {
                m_parent = parent;
                m_items = items;
                m_eventHandle = new ManualResetEventSlim(false);
                ThreadPool.QueueUserWorkItem(state => ProcessItems());
            }

            ~TemporalTask()
            {
                Dispose(false);
            }

            private void ProcessItem()
            {
                m_parent.ProcessItem(m_item);
                m_eventHandle.Set();
            }

            private void ProcessItems()
            {
                m_parent.ProcessItems(m_items);
                m_eventHandle.Set();
            }

            // Blocks calling thread until specified process timeout has expired.
            private bool Wait()
            {
                return m_eventHandle.Wait(m_parent.ProcessTimeout);
            }

            void IDisposable.Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if (m_eventHandle != null)
                                m_eventHandle.Dispose();

                            m_eventHandle = null;
                            m_parent = null;
                        }
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.
                    }
                }
            }

            public static bool Process(ProcessQueue<T> parent, T item)
            {
                using (TemporalTask temporalProcess = new TemporalTask(parent, item))
                {
                    return temporalProcess.Wait();
                }
            }

            public static bool Process(ProcessQueue<T> parent, T[] items)
            {
                using (TemporalTask temporalProcess = new TemporalTask(parent, items))
                {
                    return temporalProcess.Wait();
                }
            }
        }

        // Constants

        /// <summary>Default processing interval (in milliseconds).</summary>
        public const int DefaultProcessInterval = 100;

        /// <summary>Default maximum number of processing threads.</summary>
        public const int DefaultMaximumThreads = 5;

        /// <summary>Default processing timeout (in milliseconds).</summary>
        public const int DefaultProcessTimeout = Timeout.Infinite;

        /// <summary>Default setting for requeuing items on processing timeout.</summary>
        public const bool DefaultRequeueOnTimeout = false;

        /// <summary>Default setting for requeuing items on processing exceptions.</summary>
        public const bool DefaultRequeueOnException = false;

        /// <summary>Default real-time processing interval (in milliseconds).</summary>
        public const double RealTimeProcessInterval = 0.0;

        // Delegates

        /// <summary>
        /// Function signature that defines a method to process items one at a time.
        /// </summary>
        /// <param name="item">Item to be processed.</param>
        /// <remarks>
        /// <para>Required unless <see cref="ProcessQueue{T}.ProcessItemsFunction"/> is implemented.</para>
        /// <para>Creates an asynchronous <see cref="ProcessQueue{T}"/> to process individual items - one item at a time - on multiple threads.</para>
        /// </remarks>
        public delegate void ProcessItemFunctionSignature(T item);

        /// <summary>
        /// Function signature that defines a method to process multiple items at once.
        /// </summary>
        /// <param name="items">Items to be processed.</param>
        /// <remarks>
        /// <para>Required unless <see cref="ProcessQueue{T}.ProcessItemFunction"/> is implemented.</para>
        /// <para>Creates an asynchronous <see cref="ProcessQueue{T}"/> to process groups of items simultaneously on multiple threads.</para>
        /// </remarks>
        public delegate void ProcessItemsFunctionSignature(T[] items);

        /// <summary>
        /// Function signature that determines if an item can be currently processed.
        /// </summary>
        /// <param name="item">Item to be checked for processing availablity.</param>
        /// <returns>True, if item can be processed. The default is true.</returns>
        /// <remarks>
        /// <para>Implementation of this function is optional. It is assumed that an item can be processed if this
        /// function is not defined</para>
        /// <para>Items must eventually get to a state where they can be processed, or they will remain in the <see cref="ProcessQueue{T}"/>
        /// indefinitely.</para>
        /// <para>
        /// Note that when this function is implemented and ProcessingStyle = ManyAtOnce (i.e., 
        /// <see cref="ProcessQueue{T}.ProcessItemsFunction"/> is defined), then each item presented for 
        /// processing must evaluate as "CanProcessItem = True" before any items are processed.
        /// </para>
        /// </remarks>
        public delegate bool CanProcessItemFunctionSignature(T item);

        // Events

        /// <summary>
        /// Event that is raised after an item has been successfully processed.
        /// </summary>
        /// <remarks>
        /// <para>Allows custom handling of successfully processed items.</para>
        /// <para>Allows notification when an item has completed processing in the allowed amount of time, if a process
        /// timeout is specified.</para>
        /// <para>Raised only when ProcessingStyle = OneAtATime (i.e., <see cref="ProcessQueue{T}.ProcessItemFunction"/> is defined).</para>
        /// </remarks>
        public event EventHandler<EventArgs<T>> ItemProcessed;

        /// <summary>
        /// Event that is raised after an array of items have been successfully processed.
        /// </summary>
        /// <remarks>
        /// <para>Allows custom handling of successfully processed items.</para>
        /// <para>Allows notification when an item has completed processing in the allowed amount of time, if a process
        /// timeout is specified.</para>
        /// <para>Raised only when when ProcessingStyle = ManyAtOnce (i.e., <see cref="ProcessQueue{T}.ProcessItemsFunction"/> is defined).</para>
        /// </remarks>
        public event EventHandler<EventArgs<T[]>> ItemsProcessed;

        /// <summary>
        /// Event that is raised if an item's processing time exceeds the specified process timeout.
        /// </summary>
        /// <remarks>
        /// <para>Allows custom handling of items that took too long to process.</para>
        /// <para>Raised only when ProcessingStyle = OneAtATime (i.e., <see cref="ProcessQueue{T}.ProcessItemFunction"/> is defined).</para>
        /// </remarks>
        public event EventHandler<EventArgs<T>> ItemTimedOut;

        /// <summary>
        /// Event that is raised if the processing time for an array of items exceeds the specified process timeout.
        /// </summary>
        /// <remarks>
        /// <para>Allows custom handling of items that took too long to process.</para>
        /// <para>Raised only when ProcessingStyle = ManyAtOnce (i.e., <see cref="ProcessQueue{T}.ProcessItemsFunction"/> is defined).</para>
        /// </remarks>
        public event EventHandler<EventArgs<T[]>> ItemsTimedOut;

        /// <summary>
        /// Event that is raised if an exception is encountered while attempting to processing an item in the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <remarks>
        /// Processing will not stop for any exceptions thrown by the user function, but any captured exceptions will
        /// be exposed through this event.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Occurs when the class has been disposed.
        /// </summary>
        public event EventHandler Disposed;

        // Fields
        private ProcessItemFunctionSignature m_processItemFunction;
        private ProcessItemsFunctionSignature m_processItemsFunction;
        private CanProcessItemFunctionSignature m_canProcessItemFunction;

        private IEnumerable<T> m_processQueue;
        private int m_maximumThreads;
        private int m_processTimeout;
        private readonly bool m_processingIsRealTime;

        private bool m_requeueOnTimeout;
        private bool m_requeueOnException;

        private int m_processing;
        private int m_threadCount;
        private bool m_enabled;
        private long m_itemsProcessing;
        private long m_itemsProcessed;
        private long m_startTime;
        private long m_stopTime;
        private string m_name;
        private bool m_disposed;

#if ThreadTracking
        private ManagedThread m_realTimeProcessThread;
#else
        private Thread m_realTimeProcessThread;
#endif

        private System.Timers.Timer m_processTimer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="ProcessQueue{T}"/> based on the generic List(Of T) class.
        /// </summary>
        /// <param name="processItemFunction">Delegate that defines a method to process one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate that determines if an item can currently be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        protected ProcessQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
            : this(processItemFunction, null, canProcessItemFunction, new ConcurrentQueue<T>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Creates a bulk item <see cref="ProcessQueue{T}"/> based on the generic List(Of T) class.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate that determines if an item can currently be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        protected ProcessQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
            : this(null, processItemsFunction, canProcessItemFunction, new ConcurrentQueue<T>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Allows derived classes to define their own instance, if desired.
        /// </summary>
        /// <param name="processItemFunction">Delegate that defines a method to process one item at a time.</param>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate that determines if an item can currently be processed.</param>
        /// <param name="processQueue">A storage list for items to be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        protected ProcessQueue(ProcessItemFunctionSignature processItemFunction, ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, IEnumerable<T> processQueue, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            m_processItemFunction = processItemFunction;    // Defining this function creates a ProcessingStyle = OneAtATime process queue
            m_processItemsFunction = processItemsFunction;  // Defining this function creates a ProcessingStyle = ManyAtOnce process queue
            m_canProcessItemFunction = canProcessItemFunction;
            m_processQueue = processQueue;
            m_maximumThreads = maximumThreads;
            m_processTimeout = processTimeout;
            m_requeueOnTimeout = requeueOnTimeout;
            m_requeueOnException = requeueOnException;

            if (processInterval == RealTimeProcessInterval)
            {
                // Instantiates process queue for real-time item processing
                m_processingIsRealTime = true;
                m_maximumThreads = 1;
            }
            else
            {
                // Instantiates process queue for intervaled item processing
                m_processTimer = new System.Timers.Timer();
                m_processTimer.Elapsed += ProcessTimerThreadProc;
                m_processTimer.Interval = processInterval;
                m_processTimer.AutoReset = true;
                m_processTimer.Enabled = false;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ProcessQueue{T}"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ProcessQueue()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the user function for processing individual items in the <see cref="ProcessQueue{T}"/> one at a time.
        /// </summary>
        /// <remarks>
        /// <para>Cannot be defined simultaneously with <see cref="ProcessQueue{T}.ProcessItemsFunction"/>.</para>
        /// <para>A <see cref="ProcessQueue{T}"/> must be defined to process a single item at a time or many items at once.</para>
        /// <para>Implementation makes ProcessingStyle = OneAtATime.</para>
        /// </remarks>
        public virtual ProcessItemFunctionSignature ProcessItemFunction
        {
            get
            {
                return m_processItemFunction;
            }
            set
            {
                if ((object)value != null)
                {
                    m_processItemFunction = value;
                    m_processItemsFunction = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the user function for processing multiple items in the <see cref="ProcessQueue{T}"/> at once.
        /// </summary>
        /// <remarks>
        /// <para>This function and <see cref="ProcessQueue{T}.ProcessItemFunction"/> cannot be defined at the same time</para>
        /// <para>A <see cref="ProcessQueue{T}"/> must be defined to process a single item at a time or many items at once</para>
        /// <para>Implementation of this function makes ProcessingStyle = ManyAtOnce</para>
        /// </remarks>
        public virtual ProcessItemsFunctionSignature ProcessItemsFunction
        {
            get
            {
                return m_processItemsFunction;
            }
            set
            {
                if ((object)value != null)
                {
                    m_processItemsFunction = value;
                    m_processItemFunction = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the user function determining if an item is ready to be processed.
        /// </summary>
        public virtual CanProcessItemFunctionSignature CanProcessItemFunction
        {
            get
            {
                return m_canProcessItemFunction;
            }
            set
            {
                m_canProcessItemFunction = value;
            }
        }

        /// <summary>
        /// Gets indicator that items will be processed in real-time.
        /// </summary>
        public virtual bool ProcessingIsRealTime
        {
            get
            {
                return m_processingIsRealTime;
            }
        }

        /// <summary>
        /// Gets the current <see cref="QueueThreadingMode"/> for the <see cref="ProcessQueue{T}"/> (i.e., synchronous or asynchronous).
        /// </summary>
        /// <remarks>
        /// <para>The maximum number of processing threads determines the <see cref="QueueThreadingMode"/>.</para>
        /// <para>If the maximum threads are set to one, item processing will be synchronous
        /// (i.e., ThreadingMode = Synchronous).</para>
        /// <para>If the maximum threads are more than one, item processing will be asynchronous
        /// (i.e., ThreadingMode = Asynchronous).</para>
        /// <para>
        /// Note that for asynchronous <see cref="ProcessQueue{T}"/>, the processing interval will control how many threads are spawned
        /// at once. If items are processed faster than the specified processing interval, only one process thread
        /// will ever be spawned at a time. To ensure multiple threads are utilized to <see cref="ProcessQueue{T}"/> items, lower
        /// the process interval (minimum process interval is 1 millisecond).
        /// </para>
        /// </remarks>
        public virtual QueueThreadingMode ThreadingMode
        {
            get
            {
                if (m_maximumThreads > 1)
                    return QueueThreadingMode.Asynchronous;

                return QueueThreadingMode.Synchronous;
            }
        }

        /// <summary>
        /// Gets the item <see cref="QueueProcessingStyle"/> for the <see cref="ProcessQueue{T}"/> (i.e., one at a time or many at once).
        /// </summary>
        /// <returns>
        /// <para>OneAtATime, if the <see cref="ProcessQueue{T}.ProcessItemFunction"/> is implemented.</para>
        /// <para>ManyAtOnce, if the <see cref="ProcessQueue{T}.ProcessItemsFunction"/> is implemented.</para>
        /// </returns>
        /// <remarks>
        /// <para>The implemented item processing function determines the <see cref="QueueProcessingStyle"/>.</para>
        /// <para>
        /// If the <see cref="QueueProcessingStyle"/> is ManyAtOnce, all available items in the <see cref="ProcessQueue{T}"/> are presented for processing
        /// at each processing interval. If you expect items to be processed in the order in which they were received, make
        /// sure you use a synchronous <see cref="ProcessQueue{T}"/>. Real-time <see cref="ProcessQueue{T}"/> are inherently synchronous.
        /// </para>
        /// </remarks>
        public virtual QueueProcessingStyle ProcessingStyle
        {
            get
            {
                if ((object)m_processItemFunction == null)
                    return QueueProcessingStyle.ManyAtOnce;

                return QueueProcessingStyle.OneAtATime;
            }
        }

        /// <summary>
        /// Gets or sets the interval, in milliseconds, on which new items begin processing.
        /// </summary>
        public virtual double ProcessInterval
        {
            get
            {
                if (m_processingIsRealTime)
                    return RealTimeProcessInterval;

                return m_processTimer.Interval;
            }
            set
            {
                if (m_processingIsRealTime)
                    throw new InvalidOperationException("Cannot change process interval when " + Name + " is configured for real-time processing");

                m_processTimer.Interval = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of threads to process simultaneously.
        /// </summary>
        /// <value>Sets the maximum number of processing threads.</value>
        /// <returns>Maximum number of processing threads.</returns>
        /// <remarks>If MaximumThreads is set to one, item processing will be synchronous (i.e., ThreadingMode = Synchronous)</remarks>
        public virtual int MaximumThreads
        {
            get
            {
                return m_maximumThreads;
            }
            set
            {
                if (m_processingIsRealTime)
                    throw new InvalidOperationException("Cannot change the maximum number of threads when " + Name + " is configured for real-time processing");

                m_maximumThreads = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum time, in milliseconds, allowed for processing an item.
        /// </summary>
        /// <value>Sets the maximum number of milliseconds allowed to process an item.</value>
        /// <returns>The maximum number of milliseconds allowed to process an item.</returns>
        /// <remarks>Set to Timeout.Infinite (i.e., -1) to allow processing to take as long as needed.</remarks>
        public virtual int ProcessTimeout
        {
            get
            {
                return m_processTimeout;
            }
            set
            {
                m_processTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not to automatically place an item back into the <see cref="ProcessQueue{T}"/> if the processing times out.
        /// </summary>
        /// <remarks>Ignored if the ProcessTimeout is set to Timeout.Infinite (i.e., -1).</remarks>
        public virtual bool RequeueOnTimeout
        {
            get
            {
                return m_requeueOnTimeout;
            }
            set
            {
                m_requeueOnTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not to automatically place an item back into the <see cref="ProcessQueue{T}"/> if an exception occurs
        /// while processing.
        /// </summary>
        public virtual bool RequeueOnException
        {
            get
            {
                return m_requeueOnException;
            }
            set
            {
                m_requeueOnException = value;
            }
        }

        /// <summary>
        /// Gets or sets indicator that the <see cref="ProcessQueue{T}"/> is currently enabled.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (m_enabled && !value)
                    Stop();
                else if (!m_enabled && value)
                    Start();
            }
        }

        /// <summary>
        /// Gets indicator that the <see cref="ProcessQueue{T}"/> is actively processing items.
        /// </summary>
        public bool IsProcessing
        {
            get
            {
                if (m_processingIsRealTime)
                    return ((object)m_realTimeProcessThread != null);

                lock (m_processTimer)
                {
                    // Enabled flag changes are always in a critical section to ensure all items will be processed
                    return m_processTimer.Enabled;
                }
            }
        }

        /// <summary>
        /// Gets the total number of items currently being processed.
        /// </summary>
        /// <returns>Total number of items currently being processed.</returns>
        public long ItemsBeingProcessed
        {
            get
            {
                return m_itemsProcessing;
            }
        }

        /// <summary>
        /// Gets the total number of items processed so far.
        /// </summary>
        /// <returns>Total number of items processed so far.</returns>
        public long TotalProcessedItems
        {
            get
            {
                return m_itemsProcessed;
            }
        }

        /// <summary>
        /// Gets the current number of active threads.
        /// </summary>
        /// <returns>Current number of active threads.</returns>
        public int ThreadCount
        {
            get
            {
                return m_threadCount;
            }
        }

        /// <summary>
        /// Gets the total amount of time, in seconds, that the <see cref="ProcessQueue{T}"/> has been active.
        /// </summary>
        public virtual Time RunTime
        {
            get
            {
                Ticks processingTime = 0;

                if (m_startTime > 0)
                {
                    if (m_stopTime > 0)
                        processingTime = m_stopTime - m_startTime;
                    else
                        processingTime = DateTime.Now.Ticks - m_startTime;
                }

                if (processingTime < 0)
                    processingTime = 0;

                return processingTime.ToSeconds();
            }
        }

        /// <summary>
        /// Gets or sets name for this <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <remarks>
        /// This name is used for class identification in strings (e.g., used in error messages).
        /// </remarks>
        public virtual string Name
        {
            get
            {
                if (string.IsNullOrEmpty(m_name))
                    m_name = this.GetType().Name;

                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>Gets the number of elements actually contained in the <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>The number of elements actually contained in the <see cref="ProcessQueue{T}"/>.</returns>
        public virtual int Count
        {
            get
            {
                return m_processQueue.Count();
            }
        }

        /// <summary>Gets a value indicating whether access to the <see cref="ProcessQueue{T}"/> is synchronized (thread safe).  Always returns true for <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>true, <see cref="ProcessQueue{T}"/> is always synchronized (thread safe).</returns>
        /// <remarks>The <see cref="ProcessQueue{T}"/> is effectively "synchronized" since all functions SyncLock operations internally.</remarks>
        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the current run-time statistics of the <see cref="ProcessQueue{T}"/> as a single group of values.
        /// </summary>
        public virtual ProcessQueueStatistics CurrentStatistics
        {
            get
            {
                ProcessQueueStatistics statistics;

                statistics.IsEnabled = m_enabled;
                statistics.IsProcessing = IsProcessing;
                statistics.ProcessingInterval = ProcessInterval;
                statistics.ProcessingStyle = ProcessingStyle;
                statistics.ProcessTimeout = m_processTimeout;
                statistics.ThreadingMode = ThreadingMode;
                statistics.ActiveThreads = m_threadCount;
                statistics.ItemsBeingProcessed = m_itemsProcessing;
                statistics.TotalProcessedItems = m_itemsProcessed;
                statistics.QueueCount = Count;
                statistics.RunTime = RunTime;

                return statistics;
            }
        }

        /// <summary>
        /// Gets current status of <see cref="ProcessQueue{T}"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append("       Queue processing is: ");
                status.Append(m_enabled ? "Enabled" : "Disabled");
                status.AppendLine();
                status.Append("  Current processing state: ");
                status.Append(IsProcessing ? "Executing" : "Idle");
                status.AppendLine();
                status.Append("       Processing interval: ");
                if (m_processingIsRealTime)
                {
                    status.Append("Real-time");
                }
                else
                {
                    status.Append(ProcessInterval);
                    status.Append(" milliseconds");
                }
                status.AppendLine();
                status.Append("        Processing timeout: ");
                if (m_processTimeout == Timeout.Infinite)
                {
                    status.Append("Infinite");
                }
                else
                {
                    status.Append(m_processTimeout);
                    status.Append(" milliseconds");
                }
                status.AppendLine();
                status.Append("      Queue threading mode: ");
                if (ThreadingMode == QueueThreadingMode.Asynchronous)
                {
                    status.Append("Asynchronous - ");
                    status.Append(m_maximumThreads);
                    status.Append(" maximum threads");
                }
                else
                {
                    status.Append("Synchronous");
                }
                status.AppendLine();
                status.Append("    Queue processing style: ");
                status.Append(ProcessingStyle == QueueProcessingStyle.OneAtATime ? "One at a time" : "Many at once");
                status.AppendLine();
                status.Append("    Total process run time: ");
                status.Append(RunTime.ToString());
                status.AppendLine();
                status.Append("      Total active threads: ");
                status.Append(m_threadCount);
                status.AppendLine();
                status.Append("   Queued items to process: ");
                status.Append(Count);
                status.AppendLine();
                status.Append("     Items being processed: ");
                status.Append(m_itemsProcessing);
                status.AppendLine();
                status.Append("     Total items processed: ");
                status.Append(m_itemsProcessed);
                status.AppendLine();

                return status.ToString();
            }
        }

        /// <summary>
        /// Allows derived classes to access the interfaced internal <see cref="ProcessQueue{T}"/> directly.
        /// </summary>
        protected IEnumerable<T> InternalQueue
        {
            get
            {
                return m_processQueue;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ProcessQueue{T}"/>. 
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return m_processQueue;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ProcessQueue{T}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProcessQueue{T}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // Must stop thread, otherwise your app will keep running :)
                    Stop();

                    if (disposing)
                    {
                        if ((object)m_processTimer != null)
                        {
                            m_processTimer.Elapsed -= ProcessTimerThreadProc;
                            m_processTimer.Dispose();
                        }
                        m_processTimer = null;
                        m_processQueue = null;
                        m_processItemFunction = null;
                        m_processItemsFunction = null;
                        m_canProcessItemFunction = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.

                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <param name="item">The item to add to the <see cref="ProcessQueue{T}"/>.</param>
        public virtual void Add(T item)
        {
            ConcurrentQueue<T> queue = m_processQueue as ConcurrentQueue<T>;

            if ((object)queue != null)
            {
                queue.Enqueue(item);

                if (m_enabled && m_processingIsRealTime && Interlocked.CompareExchange(ref m_processing, 1, 0) == 0)
                {
                    if (m_processQueue.Any())
                        ThreadPool.QueueUserWorkItem(RealTimeDataProcessingLoop);
                    else
                        Interlocked.Exchange(ref m_processing, 0);
                }

                DataAdded();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Adds the specified <paramref name="items"/> to the <see name="ProcessQueue{T}"/>.
        /// </summary>
        /// <param name="items">The elements to be added to the <see name="ProcessQueue{T}"/>.</param>
        public virtual void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <returns>True, if item is found in the <see cref="ProcessQueue{T}"/>; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        public virtual bool Contains(T item)
        {
            return m_processQueue.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ProcessQueue{T}"/> to an <see cref="System.Array"/>, starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="System.Array"/> that is the destination of the elements copied from the <see cref="ProcessQueue{T}"/>.
        /// The array must have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public virtual void CopyTo(T[] array, int index)
        {
            m_processQueue.ToList().CopyTo(array, index);
        }

        /// <summary>
        /// Copies the elements contained in the <see cref="ProcessQueue{T}"/> to a new array. 
        /// </summary>
        /// <returns>A new array containing the elements copied from the <see cref="ProcessQueue{T}"/>.</returns>
        public virtual T[] ToArray()
        {
            return m_processQueue.ToArray();
        }

        /// <summary>
        /// Attempts to add an object to the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ProcessQueue{T}"/>.</param>
        /// <returns><c>true</c> if the object was successfully added; otherwise, <c>false</c>.</returns>
        public virtual bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        /// <summary>
        /// Attempts to remove and return an object from the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <param name="item">When this method returns, if the object was removed and returned successfully, item contains the removed object. If no object was available to be removed, the value is unspecified.</param>
        /// <returns><c>true</c> if an object was removed and returned successfully; otherwise, <c>false</c>.</returns>
        public virtual bool TryTake(out T item)
        {
            ConcurrentQueue<T> queue = m_processQueue as ConcurrentQueue<T>;

            if ((object)queue != null)
                return queue.TryDequeue(out item);

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Attempts to remove and return all objects from the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <param name="items">When this method returns, if any objects were removed and returned successfully, item array contains the removed objects. If no object was available to be removed, the value is null.</param>
        /// <returns><c>true</c> if any objects were removed and returned successfully; otherwise, <c>false</c>.</returns>
        public virtual bool TryTake(out T[] items)
        {
            ConcurrentQueue<T> queue = m_processQueue as ConcurrentQueue<T>;

            if ((object)queue != null)
            {
                T item;
                List<T> taken = new List<T>();

                while (queue.TryDequeue(out item))
                {
                    taken.Add(item);
                }

                if (taken.Count > 0)
                {
                    items = taken.ToArray();
                    return true;
                }

                items = null;
                return false;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="ProcessList{T}"/>.</returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return m_processQueue.GetEnumerator();
        }

        /// <summary>
        /// Removes all elements from the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        public virtual void Clear()
        {
            ConcurrentQueue<T> queue = m_processQueue as ConcurrentQueue<T>;

            if ((object)queue != null)
            {
                T result;

                while (queue.TryDequeue(out result))
                {
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Starts item processing.
        /// </summary>
        public virtual void Start()
        {
            m_enabled = true;
            m_threadCount = 0;
            m_itemsProcessed = 0;
            m_stopTime = 0;
            m_startTime = DateTime.Now.Ticks;

            // Note that real-time queues have their main thread running continually, but for
            // intervaled queues, processing occurs only when data is available to be processed.
            if (m_processingIsRealTime)
            {
                // Start real-time processing thread
#if ThreadTracking
                m_realTimeProcessThread = new ManagedThread(RealTimeThreadProc);
                m_realTimeProcessThread.Name = "TVA.Collections.ProcessQueue.RealTimeThreadProc() [" + Name + "]";
#else
                m_realTimeProcessThread = new Thread(RealTimeThreadProc);
#endif

                m_realTimeProcessThread.IsBackground = true;
                m_realTimeProcessThread.Start();
            }
            else
            {
                // Start intervaled processing, if there items in the queue
                lock (m_processTimer)
                {
                    // Enabled flag changes are always in a critical section to ensure all items will be processed
                    m_processTimer.Enabled = Count > 0;
                }
            }
        }

        void ISupportLifecycle.Initialize()
        {
            // Enabled property handles check for redundant calls...
            Enabled = true;
        }

        /// <summary>
        /// Stops item processing.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;

            if (m_processingIsRealTime)
            {
                // Remove reference to process thread - it will stop gracefully after it has finished processing
                // current set of items since enabled is false...
                m_realTimeProcessThread = null;
            }
            else
            {
                // Stops intervaled processing, if active.
                if ((object)m_processTimer != null)
                {
                    lock (m_processTimer)
                    {
                        // Enabled flag changes are always in a critical section to ensure all items will be processed
                        m_processTimer.Enabled = false;
                    }
                }
            }

            m_stopTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Blocks the current thread, if the <see cref="ProcessQueue{T}"/> is active (i.e., user has called "Start" method), until all items
        /// in <see cref="ProcessQueue{T}"/> are processed, and then stops the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Begins processing items as quickly as possible, regardless of currently defined process interval, until all
        /// items in the <see cref="ProcessQueue{T}"/> have been processed. Stops the <see cref="ProcessQueue{T}"/> when this function ends.
        /// This method is typically called on shutdown to make sure any remaining queued items get processed before the
        /// <see cref="ProcessQueue{T}"/> is destructed.
        /// </para>
        /// <para>
        /// It is possible for items to be added to the <see cref="ProcessQueue{T}"/> while the flush is executing. The flush will continue to
        /// process items as quickly as possible until the <see cref="ProcessQueue{T}"/> is empty. Unless the user stops queueing items to be
        /// processed, the flush call may never return (not a happy situtation on shutdown). For this reason, during this
        /// function call, requeueing of items on exception or process timeout is temporarily disabled.
        /// </para>
        /// <para>
        /// The <see cref="ProcessQueue{T}"/> does not clear queue prior to destruction. If the user fails to call this method before the
        /// class is destructed, there may be items that remain unprocessed in the <see cref="ProcessQueue{T}"/>.
        /// </para>
        /// </remarks>
        public virtual void Flush()
        {
            bool enabled = m_enabled;

            // Stop all queue processing...
            Stop();

            if (enabled)
            {
                bool originalRequeueOnTimeout = m_requeueOnTimeout;
                bool originalRequeueOnException = m_requeueOnException;

                // We must disable requeueing of items or this method could continue indefinitely.
                m_requeueOnTimeout = false;
                m_requeueOnException = false;

                // Only waits around if there is something to process.
                while (Count > 0)
                {
                    // Create a real-time processing loop that will process remaining items as quickly as possible.
                    while (m_processQueue.Any())
                    {
                        if ((object)m_processItemsFunction == null)
                        {
                            // Processes one item at a time.
                            ProcessNextItem();
                        }
                        else
                        {
                            // Processes multiple items at once.
                            ProcessNextItems();
                        }
                    }
                }

                // Just in case user continues to use queue after flush, this restores original states.
                m_requeueOnTimeout = originalRequeueOnTimeout;
                m_requeueOnException = originalRequeueOnException;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_processQueue).GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            m_processQueue.ToArray().CopyTo(array, index);
        }

        #region [ Item Processing Functions ]

        /// <summary>
        /// Raises the base class <see cref="ItemProcessed"/> event.
        /// </summary>
        /// <remarks>
        /// Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate
        /// as needed.
        /// </remarks>
        /// <param name="item">A generic type T to be passed to ItemProcessed.</param>
        protected virtual void OnItemProcessed(T item)
        {
            if ((object)ItemProcessed != null)
                ItemProcessed(this, new EventArgs<T>(item));
        }

        /// <summary>
        /// Raises the base class <see cref="ItemsProcessed"/> event.
        /// </summary>
        /// <remarks>
        /// Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate
        /// as needed.
        /// </remarks>
        /// <param name="items">An array of generic type T to be passed to ItemsProcessed.</param>
        protected virtual void OnItemsProcessed(T[] items)
        {
            if ((object)ItemsProcessed != null)
                ItemsProcessed(this, new EventArgs<T[]>(items));
        }

        /// <summary>
        /// Raises the base class <see cref="ItemTimedOut"/> event.
        /// </summary>
        /// <remarks>
        /// Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate
        /// as needed.
        /// </remarks>
        /// <param name="item">A generic type T to be passed to ItemProcessed.</param>
        protected virtual void OnItemTimedOut(T item)
        {
            if ((object)ItemTimedOut != null)
                ItemTimedOut(this, new EventArgs<T>(item));
        }

        /// <summary>
        /// Raises the base class <see cref="ItemsTimedOut"/> event.
        /// </summary>
        /// <remarks>
        /// Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate
        /// as needed.
        /// </remarks>
        /// <param name="items">An array of generic type T to be passed to ItemsProcessed.</param>
        protected virtual void OnItemsTimedOut(T[] items)
        {
            if ((object)ItemsTimedOut != null)
                ItemsTimedOut(this, new EventArgs<T[]>(items));
        }

        /// <summary>
        /// Raises the base class <see cref="ProcessException"/> event.
        /// </summary>
        /// <remarks>
        /// Derived classes cannot raise events of their base classes, so we expose event wrapper methods to accomodate
        /// as needed.
        /// </remarks>
        /// <param name="ex"><see cref="Exception"/> to be passed to ProcessException.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Notifies a class that data was added, so it can begin processing data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Derived classes *must* make sure to call this method after data gets added, so that the
        /// process timer can be enabled for intervaled <see cref="ProcessQueue{T}"/> and data processing can begin.
        /// </para>
        /// <para>
        /// To make sure items in the <see cref="ProcessQueue{T}"/> always get processed, this function is expected to be
        /// invoked from within a SyncLock of the exposed SyncRoot (i.e., InternalList).
        /// </para>
        /// </remarks>
        protected virtual void DataAdded()
        {
            // For queues that are not processing in real-time, we start the intervaled process timer
            // when data is added, if it's not running already
            if (!m_processingIsRealTime)
            {
                lock (m_processTimer)
                {
                    // Enabled flag changes are always in a critical section to ensure all items will be processed
                    if (m_enabled && !m_processTimer.Enabled)
                        m_processTimer.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Determines if an item can be processed.
        /// </summary>
        /// <values>True, if user provided no implementation for the CanProcessItemFunction.</values>
        /// <remarks>
        /// <para>
        /// Use this function instead of invoking the CanProcessItemFunction pointer
        /// directly, since implementation of this delegate is optional.
        /// </para>
        /// </remarks>
        /// <param name="item">The item T to process.</param>
        /// <returns>A <see cref="Boolean"/> value indicating whether it can process the item or not.</returns>
        protected virtual bool CanProcessItem(T item)
        {
            // If user provided no implementation for this function or function failed, we assume item can be processed.
            if ((object)m_canProcessItemFunction == null)
                return true;

            try
            {
                // When user function is provided, we call it to determine if item state allows processing at this time.
                return m_canProcessItemFunction(item);
            }
            catch (ThreadAbortException)
            {
                // Rethrow thread abort so calling method can respond appropriately
                throw;
            }
            catch (Exception ex)
            {
                // Processing will not stop for any errors thrown by the user function, but errors will be reported.
                OnProcessException(ex);
            }

            // Assuming processing must go on if the user function fails
            return true;
        }

        /// <summary>
        /// Determines if all items can be processed.
        /// </summary>
        /// <values>True, if user provided no implementation for the CanProcessItemFunction.</values>
        /// <remarks>
        /// <para>
        /// Use this function instead of invoking the CanProcessItemFunction pointer
        /// directly, since implementation of this delegate is optional.
        /// </para>
        /// </remarks>
        /// <param name="items">An array of items of type T.</param>
        /// <returns>A <see cref="Boolean"/> value indicating whether the process queue can process the items.</returns>
        protected virtual bool CanProcessItems(T[] items)
        {
            // If user provided no implementation for this function or function failed, we assume item can be processed.
            if ((object)m_canProcessItemFunction == null)
                return true;

            // Otherwise we call user function for each item to determine if all items are ready for processing.
            bool allItemsCanBeProcessed = true;

            foreach (T item in items)
            {
                if (!CanProcessItem(item))
                {
                    allItemsCanBeProcessed = false;
                    break;
                }
            }

            return allItemsCanBeProcessed;
        }

        /// <summary>
        /// Requeues item into <see cref="ProcessQueue{T}"/> according to specified requeue reason.
        /// </summary>
        /// <param name="item">A generic item of type T to be requeued.</param>
        /// <param name="reason">The reason the object is being requeued.</param>
        protected virtual void RequeueItem(T item, RequeueReason reason)
        {
            Add(item);
        }

        /// <summary>
        /// Requeues items into <see cref="ProcessQueue{T}"/> according to specified requeue reason.
        /// </summary>
        /// <param name="items">Array of type T to be requeued.</param>
        /// <param name="reason">The reason the object is being requeued.</param>
        protected virtual void RequeueItems(T[] items, RequeueReason reason)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Handles standard processing of a single item. 
        /// </summary>
        /// <param name="item">A generic item of type T to be processed.</param>
        private void ProcessItem(T item)
        {
            try
            {
                // Invokes user function to process item.
                m_processItemFunction(item);
                Interlocked.Increment(ref m_itemsProcessed);

                // Notifies consumers of successfully processed items.
                OnItemProcessed(item);
            }
            catch (ThreadAbortException)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw;
            }
            catch (Exception ex)
            {
                // Requeues item on processing exception, if requested.
                if (m_requeueOnException)
                    RequeueItem(item, RequeueReason.Exception);

                // Processing will not stop for any errors thrown by the user function, but errors will be reported.
                OnProcessException(ex);
            }
        }

        /// <summary>
        /// Handles standard processing of multiple items.
        /// </summary>
        /// <param name="items">Array of type T.</param>
        private void ProcessItems(T[] items)
        {
            try
            {
                // Invokes user function to process items.
                m_processItemsFunction(items);
                Interlocked.Add(ref m_itemsProcessed, items.Length);

                // Notifies consumers of successfully processed items.
                OnItemsProcessed(items);
            }
            catch (ThreadAbortException)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw;
            }
            catch (Exception ex)
            {
                // Requeues items on processing exception, if requested.
                if (m_requeueOnException)
                    RequeueItems(items, RequeueReason.Exception);

                // Processing will not stop for any errors thrown by the user function, but errors will be reported.
                OnProcessException(ex);
            }
        }

        /// <summary>
        /// Creates a real-time thread for processing items. 
        /// </summary>
        private void RealTimeThreadProc()
        {
            int processing;
            int sleepTime = 1;
            long noWorkSleeps = 0L;

            // Creates a real-time processing loop that will start item processing as quickly as possible.
            while (m_enabled)
            {
                processing = Interlocked.CompareExchange(ref m_processing, 1, 0);

                // Kick start processing when items exist that are not currently being processed
                if (processing == 0 && m_processQueue.Any())
                {
                    sleepTime = 1;
                    noWorkSleeps = 0L;
                    ThreadPool.QueueUserWorkItem(RealTimeDataProcessingLoop);
                }
                else
                {
                    // If the processing flag was set but no items were found in the process queue,
                    // the asynchronous loop was never spawned so we need to clear the processing flag
                    if (processing == 0)
                        Interlocked.Exchange(ref m_processing, 0);

                    // Vary sleep time based on how often kick start is being processed, up to one second for very idle queues
                    if (noWorkSleeps > 1000L)
                        sleepTime = 1000;   // It will take well over 1.5 minutes of no work before sleeping for 1 second
                    else if (noWorkSleeps > 100L)
                        sleepTime = 100;    // It will take at least one second of no work before sleeping for 100ms
                    else if (noWorkSleeps > 5L)
                        sleepTime = 10;     // It will take at least 5ms of no work before sleeping for 10ms

                    noWorkSleeps++;

                    // Wait around for more items to process
                    Thread.Sleep(sleepTime);
                }
            }
        }

        // Creates a real-time loop for processing data that runs as long as there is data to process
        private void RealTimeDataProcessingLoop(object state)
        {
            if ((object)m_processItemsFunction == null)
                ProcessNextItem();
            else
                ProcessNextItems();

            if (m_enabled && m_processQueue.Any())
                ThreadPool.QueueUserWorkItem(RealTimeDataProcessingLoop);
            else
                Interlocked.Exchange(ref m_processing, 0);
        }

        /// <summary>
        /// Processes queued items on an interval.
        /// </summary>
        /// <param name="sender">The sender object of the item.</param>
        /// <param name="e">Arguments for the elapsed event.</param>
        private void ProcessTimerThreadProc(object sender, System.Timers.ElapsedEventArgs e)
        {
            // The system timer creates an intervaled processing loop such that if an existing item processing
            // call hasn't completed before next interval, multiple processing calls will be spawned thereby
            // distributing item processing across multiple threads as needed.
            if ((object)m_processItemsFunction == null)
            {
                // Process one item at a time.
                ProcessNextItem();
            }
            else
            {
                // Process multiple items at once.
                ProcessNextItems();
            }

            if ((object)m_processTimer != null)
            {
                // Stop the process timer if there is no more data to process.
                lock (m_processTimer)
                {
                    // Enabled flag changes are always in a critical section to ensure all items will be processed
                    if (!m_processQueue.Any())
                        m_processTimer.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Processes next item in queue, one at a time (i.e., ProcessingStyle = OneAtATime). 
        /// </summary>
        private void ProcessNextItem()
        {
            T nextItem = default(T);
            bool processingItem = false;

            try
            {
                // Get the next item to be processed if the number of current process threads is less
                // than the maximum allowable number of process threads.
                if (m_threadCount < m_maximumThreads && TryTake(out nextItem))
                {
                    // Call optional user function to see if we can process this item.
                    if (CanProcessItem(nextItem))
                    {
                        Interlocked.Increment(ref m_threadCount);
                        Interlocked.Increment(ref m_itemsProcessing);
                        processingItem = true;
                    }
                    else
                    {
                        // If item state is not ready for processing, all we can do is requeue.
                        RequeueItem(nextItem, RequeueReason.CannotProcess);
                    }
                }

                if (processingItem)
                {
                    if (m_processTimeout == Timeout.Infinite)
                    {
                        // If an item is in the queue to process, and the process queue was not set up with a process
                        // timeout, we use the current thread (i.e., the timer event or real-time thread) to process the
                        // next item taking as long as we need for it to complete. For timer events, the next item in
                        // the queue will begin processing even if this item is not completed, but no more than the
                        // specified number of maximum threads will ever be spawned at once.
                        ProcessItem(nextItem);
                    }
                    else
                    {
                        // If an item is in the queue to process, with a specified process timeout, a new thread is
                        // created to handle the processing. The timer event or real-time thread that invoked this method
                        // is already a new thread, so the only reason to create another thread is to implement the
                        // process timeout if the process takes too long to run. This is done by joining the current
                        // thread (which will block it) until the specified interval has passed or the process thread
                        // completes, whichever comes first. This is a safe operation since the current thread
                        // (i.e., the timer event or real-time thread) was already an independent thread and will not
                        // block any other processing, including another timer event.
                        if (!TemporalTask.Process(this, nextItem))
                        {
                            // Notify user of process timeout, in case they want to do anything special.
                            OnItemTimedOut(nextItem);

                            // Requeues item on processing timeout, if requested.
                            if (m_requeueOnTimeout)
                                RequeueItem(nextItem, RequeueReason.Timeout);
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw;
            }
            catch (Exception ex)
            {
                // Processing will not stop for any errors encountered here, but errors will be reported.
                OnProcessException(ex);
            }
            finally
            {
                // Decrements thread count, if item was retrieved for processing.
                if (processingItem)
                {
                    Interlocked.Decrement(ref m_threadCount);
                    Interlocked.Decrement(ref m_itemsProcessing);
                }
            }
        }

        /// <summary>
        /// Processes next items in an array of items as a group (i.e., ProcessingStyle = ManyAtOnce).
        /// </summary>
        private void ProcessNextItems()
        {
            T[] nextItems = null;
            bool processingItems = false;

            try
            {
                // Get next items to be processed if the number of current process threads is less
                // than the maximum allowable number of process threads.
                if (m_threadCount < m_maximumThreads && TryTake(out nextItems))
                {
                    // Call optional user function to see if these items can be processed.
                    if (CanProcessItems(nextItems))
                    {
                        Interlocked.Increment(ref m_threadCount);
                        Interlocked.Add(ref m_itemsProcessing, nextItems.Length);
                        processingItems = true;
                    }
                    else
                    {
                        // If item state is not ready for processing, all we can do is requeue.
                        RequeueItems(nextItems, RequeueReason.CannotProcess);
                    }
                }

                if (processingItems)
                {
                    if (m_processTimeout == Timeout.Infinite)
                    {
                        // If items are in the queue to process, and the process queue was not set up with a process
                        // timeout, the current thread (i.e., the timer event or real-time thread) is used to process the
                        // next items taking as long as necessary to complete. For timer events, any new items available
                        // in the queue will be processed, even if the current items have not completed, but no more than
                        // the specified number of maximum threads will ever be spawned at once.
                        ProcessItems(nextItems);
                    }
                    else
                    {
                        // If items are in the queue to process, and a process timeout was specified, a new thread is
                        // created to handle the processing. The timer event or real-time thread that invoked this method
                        // is already a new thread, so the only reason to create another thread is to implement the
                        // process timeout if the process takes too long to run. We do this by joining the current thread
                        // (which will block it) until the specified interval has passed or the process thread completes,
                        // whichever comes first. This is a safe operation, since the current thread (i.e., the timer
                        // event or real-time thread) was already an independent thread and will not block any other
                        // processing, including another timer event.
                        if (!TemporalTask.Process(this, nextItems))
                        {
                            // Notify the user of the process timeout, in case they want to do anything special.
                            OnItemsTimedOut(nextItems);

                            // Requeues items on processing timeout, if requested.
                            if (m_requeueOnTimeout)
                                RequeueItems(nextItems, RequeueReason.Timeout);
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw;
            }
            catch (Exception ex)
            {
                // Processing will not stop for any errors encountered here, but errors will be reported.
                OnProcessException(ex);
            }
            finally
            {
                // Decrements thread count, if items were retrieved for processing.
                if (processingItems)
                {
                    Interlocked.Decrement(ref m_threadCount);
                    Interlocked.Add(ref m_itemsProcessing, -nextItems.Length);
                }
            }
        }

        #endregion

        #endregion

        #region [ Static ]

        #region [ Single-Item Processing Constructors ]

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100, MaximumThreads = 5,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateAsynchronousQueue(processItemFunction, null, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100, MaximumThreads = 5,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemFunction, null, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessQueue{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateAsynchronousQueue(processItemFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous <see cref="ProcessQueue{T}"/> using  specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessQueue<T>(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessQueue{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateSynchronousQueue(processItemFunction, null, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessQueue{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessQueue{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateSynchronousQueue(processItemFunction, null, processInterval, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous <see cref="ProcessQueue{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessQueue<T>(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessQueue{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateRealTimeQueue(processItemFunction, null, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessQueue{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessQueue{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateRealTimeQueue(processItemFunction, null, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time <see cref="ProcessQueue{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate that processes one item at a time.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessQueue<T>(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        #endregion

        #region [ Multi-Item Processing Constructors ]

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessQueue{T}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessQueue{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous, bulk item <see cref="ProcessQueue{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value that determines the maximum number of threads used to process items.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessQueue<T>(processItemsFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessQueue{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateSynchronousQueue(processItemsFunction, null, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessQueue{T}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateSynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessQueue{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateSynchronousQueue(processItemsFunction, null, processInterval, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous, bulk item <see cref="ProcessQueue{T}"/> (i.e., single process thread) using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessQueue<T>(processItemsFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessQueue{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateRealTimeQueue(processItemsFunction, null, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessQueue{T}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateRealTimeQueue(processItemsFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessQueue{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateRealTimeQueue(processItemsFunction, null, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time, bulk item <see cref="ProcessQueue{T}"/> using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate that defines a method to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Delegate which determines whether an item can be processed.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>A ProcessQueue object based on type T.</returns>
        public static ProcessQueue<T> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new ProcessQueue<T>(processItemsFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        #endregion

        #endregion
    }
}