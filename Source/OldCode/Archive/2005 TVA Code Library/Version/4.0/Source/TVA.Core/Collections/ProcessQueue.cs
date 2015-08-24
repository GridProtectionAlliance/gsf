//*******************************************************************************************************
//  ProcessQueue.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/07/2006 - James R. Carroll
//       Generated original version of source code.
//  02/12/2006 - James R. Carroll
//       Added multi-item bulk processing functionality.
//  03/21/2007 - James R. Carroll
//       Added "ItemsBeingProcessed" property to return current total number of items being processed.
//       Added "Flush" method to allow any remaining items in queue to be processed before shutdown.
//  04/05/2007 - James R. Carroll
//       Added "RequeueMode" properties to allow users to specify how data gets reinserted back into
//       the list (prefix or suffix) after processing timeouts or exceptions.
//  07/12/2007 - Pinal C. Patel
//       Modified the code for "Flush" method to correctly implement IDisposable interface.
//  08/01/2007 - James R. Carroll
//       Added some minor optimizations where practical.
//  08/17/2007 - James R. Carroll
//       Removed IDisposable implementation because of continued flushing errors.
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  11/05/2007 - James R. Carroll
//       Modified flush to complete tasks on calling thread - this avoids errors when timer
//       gets disposed before flush call.
//  02/20/2008 - James R. Carroll
//       Implemented standard IDisposable pattern.
//  09/11/2008 - James R. Carroll
//      Converted to C#
//  11/06/2008 - James R. Carroll
//      Added CurrentStatistics property to return run-time statistics as a group.
//  02/23/2009 - Josh Patterson
//      Edited Code Comments
//  08/05/2009 - Josh Patterson
//      Edited Code Comments
//
//*******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TVA.Units;

namespace TVA.Collections
{
    #region [ Enumerations ]

    /// <summary>Enumeration of possible <see cref="ProcessQueue{T}"/> threading modes.</summary>
    public enum QueueThreadingMode
    {
        /// <summary>Processes several items in the <see cref="ProcessQueue{T}"/> at once on different threads, where processing order is not
        /// important.</summary>
        Asynchronous,
        /// <summary>Processes items in the <see cref="ProcessQueue{T}"/> one at a time on a single thread, where processing order is important.</summary>
        Synchronous
    }

    /// <summary>Enumeration of possible <see cref="ProcessQueue{T}"/> processing styles.</summary>
    public enum QueueProcessingStyle
    {
        /// <summary>Defines <see cref="ProcessQueue{T}"/> processing delegate to process only one item at a time.</summary>
        /// <remarks>This is the typical <see cref="QueueProcessingStyle"/> when the <see cref="QueueThreadingMode"/> is asynchronous.</remarks>
        OneAtATime,
        /// <summary>Defines <see cref="ProcessQueue{T}"/> processing delegate to process all currently available items in the <see cref="ProcessQueue{T}"/>. Items are
        /// passed into delegate as an array.</summary>
        /// <remarks>This is the optimal <see cref="QueueProcessingStyle"/> when the <see cref="QueueThreadingMode"/> is synchronous.</remarks>
        ManyAtOnce
    }

    /// <summary>Enumeration of possible requeue modes.</summary>
    public enum RequeueMode
    {
        /// <summary>Requeues item at the beginning of the <see cref="ProcessQueue{T}"/>.</summary>
        Prefix,
        /// <summary>Requeues item at the end of the <see cref="ProcessQueue{T}"/>.</summary>
        Suffix
    }

    #endregion

    /// <summary>
    /// <para>This class processes a collection of items on independent threads.</para>
    /// <para>The consumer must implement a function to process items.</para>
    /// </summary>
    /// <typeparam name="T">Type of object to process</typeparam>
    /// <remarks>
    /// <para>This class acts as a strongly-typed collection of objects to be processed.</para>
    /// <para>Consumers are expected to create new instances of this class through the static construction functions
    /// (e.g., CreateAsynchronousQueue, CreateSynchronousQueue, etc.)</para>
    /// <para>Note that the <see cref="ProcessQueue{T}"/> will not start processing until the Start method is called.</para>
    /// </remarks>
    public class ProcessQueue<T> : IList<T>, ICollection, IDisposable, IProvideStatus, ISupportLifecycle
    {
        #region [ Members ]

        // Nested Types

        #region [ ProcessThread class ]

        // Limits item processing time, if requested.
        private class ProcessThread
        {
            private ProcessQueue<T> m_parent;
            private Thread m_thread;
            private T m_item;
            private T[] m_items;

            public ProcessThread(ProcessQueue<T> parent, T item)
            {
                m_parent = parent;
                m_item = item;
                m_thread = new Thread(ProcessItem);
                m_thread.Start();
            }

            public ProcessThread(ProcessQueue<T> parent, T[] items)
            {
                m_parent = parent;
                m_items = items;
                m_thread = new Thread(ProcessItems);
                m_thread.Start();
            }

            // Blocks calling thread until specified time has expired.
            public bool WaitUntil(int timeout)
            {
                bool threadComplete = m_thread.Join(timeout);
                if (!threadComplete) m_thread.Abort();
                return threadComplete;
            }

            private void ProcessItem()
            {
                m_parent.ProcessItem(m_item);
            }

            private void ProcessItems()
            {
                m_parent.ProcessItems(m_items);
            }
        }

        #endregion

        // Constants

        /// <summary>Default processing interval (in milliseconds).</summary>
        public const int DefaultProcessInterval = 100;

        /// <summary>Default maximum number of processing threads.</summary>
        public const int DefaultMaximumThreads = 5;

        /// <summary>Default processing timeout (in milliseconds).</summary>
        public const int DefaultProcessTimeout = Timeout.Infinite;

        /// <summary>Default setting for requeuing items on processing timeout.</summary>
        public const bool DefaultRequeueOnTimeout = false;

        /// <summary>Default setting for requeuing mode on processing timeout.</summary>
        public const RequeueMode DefaultRequeueModeOnTimeout = RequeueMode.Prefix;

        /// <summary>Default setting for requeuing items on processing exceptions.</summary>
        public const bool DefaultRequeueOnException = false;

        /// <summary>Default setting for requeuing mode on processing exceptions.</summary>
        public const RequeueMode DefaultRequeueModeOnException = RequeueMode.Prefix;

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

        // Fields
        private ProcessItemFunctionSignature m_processItemFunction;
        private ProcessItemsFunctionSignature m_processItemsFunction;
        private CanProcessItemFunctionSignature m_canProcessItemFunction;

        private IList<T> m_processQueue;
        private int m_maximumThreads;
        private int m_processTimeout;

        private bool m_requeueOnTimeout;
        private bool m_requeueOnException;
        private RequeueMode m_requeueModeOnTimeout;
        private RequeueMode m_requeueModeOnException;

        private bool m_processingIsRealTime;
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

        private ThreadPriority m_realTimeProcessThreadPriority;
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
            : this(processItemFunction, null, canProcessItemFunction, new List<T>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
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
            : this(null, processItemsFunction, canProcessItemFunction, new List<T>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Allows derived classes to define their own IList instance, if desired.
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
        protected ProcessQueue(ProcessItemFunctionSignature processItemFunction, ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, IList<T> processQueue, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            m_processItemFunction = processItemFunction;    // Defining this function creates a ProcessingStyle = OneAtATime process queue
            m_processItemsFunction = processItemsFunction;  // Defining this function creates a ProcessingStyle = ManyAtOnce process queue
            m_canProcessItemFunction = canProcessItemFunction;
            m_processQueue = processQueue;
            m_maximumThreads = maximumThreads;
            m_processTimeout = processTimeout;
            m_requeueOnTimeout = requeueOnTimeout;
            m_requeueModeOnTimeout = DefaultRequeueModeOnTimeout;
            m_requeueOnException = requeueOnException;
            m_requeueModeOnException = DefaultRequeueModeOnException;
            m_realTimeProcessThreadPriority = ThreadPriority.Highest;

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
                if (value != null)
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
                if (value != null)
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
                {
                    return QueueThreadingMode.Asynchronous;
                }
                else
                {
                    return QueueThreadingMode.Synchronous;
                }
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
                if (m_processItemFunction == null)
                {
                    return QueueProcessingStyle.ManyAtOnce;
                }
                else
                {
                    return QueueProcessingStyle.OneAtATime;
                }
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
                {
                    return RealTimeProcessInterval;
                }
                else
                {
                    return m_processTimer.Interval;
                }
            }
            set
            {
                if (m_processingIsRealTime)
                {
                    throw new InvalidOperationException("Cannot change process interval when " + Name + " is configured for real-time processing");
                }
                else
                {
                    m_processTimer.Interval = value;
                }
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
                {
                    throw new InvalidOperationException("Cannot change the maximum number of threads when " + Name + " is configured for real-time processing");
                }
                else
                {
                    m_maximumThreads = value;
                }
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
        /// Gets or sets the mode of insertion used (prefix or suffix) when at item is placed back into the <see cref="ProcessQueue{T}"/>
        /// after processing times out.
        /// </summary>
        /// <remarks>Only relevant when RequeueOnTimeout = True.</remarks>
        public virtual RequeueMode RequeueModeOnTimeout
        {
            get
            {
                return m_requeueModeOnTimeout;
            }
            set
            {
                m_requeueModeOnTimeout = value;
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
        /// Gets or sets the mode of insertion used (prefix or suffix) when at item is placed back into the
        /// <see cref="ProcessQueue{T}"/> after an exception occurs while processing.
        /// </summary>
        /// <remarks>Only relevant when RequeueOnException = True.</remarks>
        public virtual RequeueMode RequeueModeOnException
        {
            get
            {
                return m_requeueModeOnException;
            }
            set
            {
                m_requeueModeOnException = value;
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
                {
                    return (m_realTimeProcessThread != null);
                }
                else
                {
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
        /// Gets or sets adjustment of real-time process thread priority.
        /// </summary>
        /// <remarks>
        /// <para>Only affects real-time <see cref="ProcessQueue{T}"/>.</para>
        /// <para>Only takes effect when set before calling the "Start" method.</para>
        /// </remarks>
        public ThreadPriority RealTimeProcessThreadPriority
        {
            get
            {
                return m_realTimeProcessThreadPriority;
            }
            set
            {
                m_realTimeProcessThreadPriority = value;
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

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than
        /// <see cref="ProcessQueue{T}"/> length. </exception>
        public virtual T this[int index]
        {
            get
            {
                lock (m_processQueue)
                {
                    return m_processQueue[index];
                }
            }
            set
            {
                lock (m_processQueue)
                {
                    m_processQueue[index] = value;
                    DataAdded();
                }
            }
        }

        /// <summary>Gets the number of elements actually contained in the <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>The number of elements actually contained in the <see cref="ProcessQueue{T}"/>.</returns>
        public virtual int Count
        {
            get
            {
                lock (m_processQueue)
                {
                    return m_processQueue.Count;
                }
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="ProcessQueue{T}"/> is read-only.</summary>
        /// <returns>True, if the <see cref="ProcessQueue{T}"/> is read-only; otherwise, false. In the default implementation, this property
        /// always returns false.</returns>
        public virtual bool IsReadOnly
        {
            get
            {
                return m_processQueue.IsReadOnly;
            }
        }

        /// <summary>Returns reference to internal IList that should be used to synchronize access to the <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>Reference to internal IList that should be used to synchronize access to the <see cref="ProcessQueue{T}"/>.</returns>
        /// <remarks>
        /// <para>
        /// Note that all the methods of this class are already individually synchronized; however, to safely enumerate through each
        /// <see cref="ProcessQueue{T}"/> element (i.e., to make sure <see cref="ProcessQueue{T}"/> elements do not change during enumeration),
        /// derived classes and end users should perform their own synchronization by implementing a SyncLock using this SyncRoot property.
        /// </para>
        /// <para>
        /// We return a typed object for synchronization as an optimization. Returning a generic object requires that
        /// SyncLock implementations validate that the referenced object is not a value type at run time.
        /// </para>
        /// </remarks>
        public IList<T> SyncRoot
        {
            get
            {
                return m_processQueue;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return m_processQueue;
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
        protected IList<T> InternalList
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
                        if (m_processTimer != null)
                        {
                            m_processTimer.Elapsed -= ProcessTimerThreadProc;
                            m_processTimer.Dispose();
                        }
                        m_processTimer = null;
                        if (m_processQueue != null)
                        {
                            m_processQueue.Clear();
                        }
                        m_processQueue = null;
                        m_processItemFunction = null;
                        m_processItemsFunction = null;
                        m_canProcessItemFunction = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
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
                m_realTimeProcessThread = new Thread(new ThreadStart(RealTimeThreadProc));
#endif

                m_realTimeProcessThread.Priority = m_realTimeProcessThreadPriority;
                m_realTimeProcessThread.Start();
            }
            else
            {
                // Start intervaled processing, if there items in the queue
                m_processTimer.Enabled = Count > 0;
            }
        }

        void ISupportLifecycle.Initialize()
        {
            // Enabled property handles check for redundant calls...
            this.Enabled = true;
        }

        /// <summary>
        /// Stops item processing.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;

            if (m_processingIsRealTime)
            {
                // Stops real-time processing thread.
                if (m_realTimeProcessThread != null)
                    m_realTimeProcessThread.Abort();

                m_realTimeProcessThread = null;
            }
            else
            {
                // Stops intervaled processing, if active.
                if (m_processTimer != null)
                    m_processTimer.Enabled = false;
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
                    lock (m_processQueue)
                    {
                        while (m_processQueue.Count > 0)
                        {
                            if (m_processItemsFunction == null)
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
                }

                // Just in case user continues to use queue after flush, this restores original states.
                m_requeueOnTimeout = originalRequeueOnTimeout;
                m_requeueOnException = originalRequeueOnException;
            }
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
            if (ItemProcessed != null)
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
            if (ItemsProcessed != null)
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
            if (ItemTimedOut != null)
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
            if (ItemsTimedOut != null)
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
            if (ProcessException != null)
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
        /// invoked from within a SyncLock of the exposed SyncRoot (i.e., m_processQueue).
        /// </para>
        /// </remarks>
        protected virtual void DataAdded()
        {
            // For queues that are not processing in real-time, we start the intervaled process timer
            // when data is added, if it's not running already
            if (!m_processingIsRealTime && !m_processTimer.Enabled)
                m_processTimer.Enabled = m_enabled;
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
            if (m_canProcessItemFunction == null)
            {
                // If user provided no implementation for this function or function failed, we assume item can be processed.
                return true;
            }
            else
            {
                try
                {
                    // When user function is provided, we call it to determine if item should be processed at this time.
                    return m_canProcessItemFunction(item);
                }
                catch (ThreadAbortException ex)
                {
                    // Rethrow thread abort so calling method can respond appropriately
                    throw ex;
                }
                catch (Exception ex)
                {
                    // Processing will not stop for any errors thrown by the user function, but errors will be reported.
                    OnProcessException(ex);
                }

                // Assuming processing must go on if the user function fails
                return true;
            }
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
            if (m_canProcessItemFunction == null)
            {
                // If user provided no implementation for this function or function failed, we assume item can be processed.
                return true;
            }
            else
            {
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
        }

        /// <summary>
        /// Requeues item into <see cref="ProcessQueue{T}"/> according to specified requeue mode.
        /// </summary>
        /// <param name="item">A generic item of type T to be requeued.</param>
        /// <param name="mode">The method in which to requeue the object.</param>
        protected virtual void RequeueItem(T item, RequeueMode mode)
        {
            if (mode == RequeueMode.Prefix)
            {
                Insert(0, item);
            }
            else
            {
                Add(item);
            }
        }

        /// <summary>
        /// Requeues items into <see cref="ProcessQueue{T}"/> according to specified requeue mode.
        /// </summary>
        /// <param name="items">Array of type T.</param>
        /// <param name="mode">The method in which to requeue the object.</param>
        protected virtual void RequeueItems(T[] items, RequeueMode mode)
        {
            if (mode == RequeueMode.Prefix)
            {
                InsertRange(0, items);
            }
            else
            {
                AddRange(items);
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
            catch (ThreadAbortException ex)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw ex;
            }
            catch (Exception ex)
            {
                // Requeues item on processing exception, if requested.
                if (m_requeueOnException)
                    RequeueItem(item, m_requeueModeOnException);

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
            catch (ThreadAbortException ex)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw ex;
            }
            catch (Exception ex)
            {
                // Requeues items on processing exception, if requested.
                if (m_requeueOnException)
                    RequeueItems(items, m_requeueModeOnException);

                // Processing will not stop for any errors thrown by the user function, but errors will be reported.
                OnProcessException(ex);
            }
        }

        
        /// <summary>
        /// Creates a real-time thread for processing items. 
        /// </summary>
        private void RealTimeThreadProc()
        {
            // Creates a real-time processing loop that will process items as quickly as possible.
            while (true)
            {
                if (m_processItemsFunction == null)
                {
                    // Processes one item at a time.
                    ProcessNextItem();
                }
                else
                {
                    // Processes multiple items at once.
                    ProcessNextItems();
                }

                // Sleeps the thread between each loop to help minimize CPU loading.
                Thread.Sleep(1);
            }
        }

        
        /// <summary>
        /// Processes queued items on an interval.
        /// </summary>
        /// <param name="sender">The sender object of the item.</param>
        /// <param name="e">Arguments for the elapsed event.</param>
        private void ProcessTimerThreadProc(object sender, System.Timers.ElapsedEventArgs e)
        {
            // The system timer creates an intervaled processing loop that distributes item processing across multiple threads if needed
            if (m_processItemsFunction == null)
            {
                // Processes one item at a time.
                ProcessNextItem();
            }
            else
            {
                // Processes multiple items at once.
                ProcessNextItems();
            }

            lock (m_processQueue)
            {
                // Stops the process timer if there is no more data to process.
                if (m_processQueue.Count == 0)
                    m_processTimer.Enabled = false;
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
                // Handles all queue operations for getting next item in a single synchronous operation.
                // We keep work to be done here down to a mimimum amount of time
                lock (m_processQueue)
                {
                    // Retrieves the next item to be processed if the number of current process threads is less
                    // than the maximum allowable number of process threads.
                    if (m_processQueue.Count > 0 && m_threadCount < m_maximumThreads)
                    {
                        // Retrieves first item to be processed.
                        nextItem = m_processQueue[0];

                        // Calls optional user function to see if we should process this item.
                        if (CanProcessItem(nextItem))
                        {
                            Interlocked.Increment(ref m_threadCount);

                            // Removes the item about to be processed from the queue.
                            m_processQueue.RemoveAt(0);

                            processingItem = true;
                            Interlocked.Increment(ref m_itemsProcessing);
                        }
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
                        ProcessThread processThread = new ProcessThread(this, nextItem);

                        if (!processThread.WaitUntil(m_processTimeout))
                        {
                            // Notify user of process timeout, in case they want to do anything special.
                            OnItemTimedOut(nextItem);

                            // Requeues item on processing timeout, if requested.
                            if (m_requeueOnTimeout)
                                RequeueItem(nextItem, m_requeueModeOnTimeout);
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw ex;
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
                // Handles all queue operations for getting next items in a single synchronous operation.
                // We keep work to be done here down to a mimimum amount of time.
                lock (m_processQueue)
                {
                    // Gets next items to be processed, if the number of current process threads is less
                    // than the maximum allowable number of process threads.
                    if (m_processQueue.Count > 0 && m_threadCount < m_maximumThreads)
                    {
                        // Retrieves items to be processed.
                        nextItems = ToArray();

                        // Calls optional user function to see if these items should be processed.
                        if (CanProcessItems(nextItems))
                        {
                            Interlocked.Increment(ref m_threadCount);

                            // Clears all items from the queue
                            m_processQueue.Clear();

                            processingItems = true;
                            Interlocked.Add(ref m_itemsProcessing, nextItems.Length);
                        }
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
                        ProcessThread processThread = new ProcessThread(this, nextItems);

                        if (!processThread.WaitUntil(m_processTimeout))
                        {
                            // Notify the user of the process timeout, in case they want to do anything special.
                            OnItemsTimedOut(nextItems);

                            // Requeues items on processing timeout, if requested.
                            if (m_requeueOnTimeout)
                                RequeueItems(nextItems, m_requeueModeOnTimeout);
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                // Rethrows thread abort, so calling method can respond appropriately.
                throw ex;
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

        #region [ Handy List(Of T) Functions Implementation ]

        // The internal list is declared as an IList(Of T). Derived classes (e.g., KeyedProcessQueue) can use their own
        // list implementation for process functionality. However, the regular List(Of T) provides many handy functions
        // that are not required to be exposed by the IList(Of T) interface. So, if the implemented list is a List(Of T),
        // we'll expose this native functionality; otherwise, we implement it for you. Yeah, you'll thank me one day.

        // Note: All List(Of T) implementations should be synchronized, as necessary.

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="ProcessQueue{T}"/>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        /// </param>
        /// <exception cref="ArgumentNullException">collection is null.</exception>
        public virtual void AddRange(IEnumerable<T> collection)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature if process queue is not a List(Of T).
                    if (collection == null)
                        throw new ArgumentNullException("collection", "collection is null");

                    foreach (T item in collection)
                    {
                        m_processQueue.Add(item);
                    }
                }
                else
                {
                    // Otherwise, we'll call native implementation.
                    processQueue.AddRange(collection);
                }

                DataAdded();
            }
        }

        ///	<summary>
        /// Searches the entire sorted <see cref="ProcessQueue{T}"/>, using a binary search algorithm, for an element using the
        /// default comparer and returns the zero-based index of the element.
        /// </summary>
        /// <remarks>
        /// <see cref="ProcessQueue{T}"/> must be sorted in order for this function to return an accurate result.
        /// </remarks>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="ProcessQueue{T}"/>, if item is found; otherwise, a negative number that is the
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        /// the bitwise complement of count.
        /// </returns>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual int BinarySearch(T item)
        {
            return BinarySearch(0, m_processQueue.Count, item, null);
        }

        ///	<summary>
        /// Searches the entire sorted <see cref="ProcessQueue{T}"/>, using a binary search algorithm, for an element using the
        /// specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <remarks>
        /// <see cref="ProcessQueue{T}"/> must be sorted in order for this function to return an accurate result.
        /// </remarks>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="ProcessQueue{T}"/>, if item is found; otherwise, a negative number that is the
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        /// the bitwise complement of count.
        /// </returns>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual int BinarySearch(T item, IComparer<T> comparer)
        {
            return BinarySearch(0, m_processQueue.Count, item, comparer);
        }

        ///	<summary>
        /// Searches a range of elements in the sorted <see cref="ProcessQueue{T}"/>, using a binary search algorithm, for an
        /// element using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <remarks>
        /// <see cref="ProcessQueue{T}"/> must be sorted in order for this function to return an accurate result.
        /// </remarks>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- null to use
        /// the default comparer: Generic.Comparer(Of T).Default</param>
        /// <returns>
        /// The zero-based index of item in the sorted <see cref="ProcessQueue{T}"/>, if item is found; otherwise, a negative number that is the
        /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
        /// the bitwise complement of count.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>
        /// -or- count is less than 0 -or- startIndex and count do not specify a valid section in the <see cref="ProcessQueue{T}"/></exception>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    int foundIndex = -1;
                    int startIndex = index;
                    int stopIndex = index + count - 1;
                    int currentIndex;
                    int result;

                    // Validates start and stop index.
                    if (startIndex < 0 || count < 0 || stopIndex > m_processQueue.Count - 1)
                        throw new ArgumentOutOfRangeException("index", "index and/or count is outside the range of valid indexes for the queue");

                    if (comparer == null) comparer = Comparer<T>.Default;

                    if (count > 0)
                    {
                        while (true)
                        {
                            // Finds next mid point.
                            currentIndex = startIndex + (stopIndex - startIndex) / 2;

                            // Compares item at mid-point
                            result = comparer.Compare(item, m_processQueue[currentIndex]);

                            if (result == 0)
                            {
                                // For a found item, returns located index.
                                foundIndex = currentIndex;
                                break;
                            }
                            else if (startIndex == stopIndex)
                            {
                                // Met in the middle and didn't find match, so we are finished,
                                foundIndex = startIndex ^ -1;
                                break;
                            }
                            else if (result > 0)
                            {
                                if (currentIndex < count - 1)
                                {
                                    // Item is beyond current item, so we start search at next item.
                                    startIndex = currentIndex + 1;
                                }
                                else
                                {
                                    // Looked to the end and did not find match, so we are finished.
                                    foundIndex = (count - 1) ^ -1;
                                    break;
                                }
                            }
                            else
                            {
                                if (currentIndex > 0)
                                {
                                    // Item is before current item, so we will stop search at current item.
                                    // Note that because of the way the math works, you do not stop at the
                                    // prior item, as you might guess. It can cause you to skip an item.
                                    stopIndex = currentIndex;
                                }
                                else
                                {
                                    // Looked to the top and did not find match, so we are finished.
                                    foundIndex = 0 ^ -1;
                                    break;
                                }
                            }
                        }
                    }

                    return foundIndex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.BinarySearch(index, count, item, comparer);
                }
            }
        }

        /// <summary>Converts the elements in the current <see cref="ProcessQueue{T}"/> to another type, and returns a <see cref="ProcessQueue{T}"/> containing the
        /// converted elements.</summary>
        /// <returns>A generic list of the target type containing the converted elements from the current <see cref="ProcessQueue{T}"/>.</returns>
        /// <param name="converter">A Converter delegate that converts each element from one type to another type.</param>
        /// <exception cref="ArgumentNullException">converter is null.</exception>
        /// <typeparam name="TOutput">The generic type used.</typeparam>
        public virtual List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (converter == null)
                        throw new ArgumentNullException("converter", "converter is null");

                    List<TOutput> result = new List<TOutput>();

                    foreach (T item in m_processQueue)
                    {
                        result.Add(converter(item));
                    }

                    return result;
                }
                else
                {
                    // Otherwise, we will call native implementation
                    return processQueue.ConvertAll(converter);
                }
            }
        }

        /// <summary>Determines whether the <see cref="ProcessQueue{T}"/> contains elements that match the conditions defined by the specified
        /// predicate.</summary>
        /// <returns>True, if the <see cref="ProcessQueue{T}"/> contains one or more elements that match the conditions defined by the specified
        /// predicate; otherwise, false.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual bool Exists(Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (match == null)
                        throw new ArgumentNullException("match", "match is null");

                    bool found = false;

                    for (int x = 0; x < m_processQueue.Count; x++)
                    {
                        if (match(m_processQueue[x]))
                        {
                            found = true;
                            break;
                        }
                    }

                    return found;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.Exists(match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the first occurrence within the entire <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>The first element that matches the conditions defined by the specified predicate, if found;
        /// otherwise, the default value for type T.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual T Find(Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (match == null)
                        throw new ArgumentNullException("match", "match is null");

                    T foundItem = default(T);
                    int foundIndex = FindIndex(match);

                    if (foundIndex >= 0)
                        foundItem = m_processQueue[foundIndex];

                    return foundItem;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.Find(match);
                }
            }
        }

        /// <summary>Retrieves all elements that match the conditions defined by the specified predicate.</summary>
        /// <returns>A generic list containing all elements that match the conditions defined by the specified predicate,
        /// if found; otherwise, an empty list.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual List<T> FindAll(Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (match == null)
                        throw new ArgumentNullException("match", "match is null");

                    List<T> foundItems = new List<T>();

                    foreach (T item in m_processQueue)
                    {
                        if (match(item))
                            foundItems.Add(item);
                    }

                    return foundItems;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.FindAll(match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the first occurrence within the range of elements in the <see cref="ProcessQueue{T}"/> that extends from the
        /// specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, m_processQueue.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the first occurrence within the range of elements in the <see cref="ProcessQueue{T}"/> that extends from the
        /// specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, m_processQueue.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the first occurrence within the range of elements in the <see cref="ProcessQueue{T}"/> that extends from the
        /// specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="startIndex">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>
        /// -or- count is less than 0 -or- startIndex and count do not specify a valid section in the <see cref="ProcessQueue{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (startIndex < 0 || count < 0 || startIndex + count > m_processQueue.Count)
                        throw new ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue");

                    if (match == null)
                        throw new ArgumentNullException("match", "match is null");

                    int foundindex = -1;

                    for (int x = startIndex; x < startIndex + count; x++)
                    {
                        if (match(m_processQueue[x]))
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.FindIndex(startIndex, count, match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type T.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual T FindLast(Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature if process queue is not a List(Of T)
                    if (match == null)
                        throw (new ArgumentNullException("match", "match is null"));

                    T foundItem = default(T);
                    int foundIndex = FindLastIndex(match);

                    if (foundIndex >= 0)
                        foundItem = m_processQueue[foundIndex];

                    return foundItem;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.FindLast(match);
                }
            }
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the last occurrence within the entire <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(0, m_processQueue.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the last occurrence within the range of elements in the <see cref="ProcessQueue{T}"/> that extends from the
        /// first element to the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, m_processQueue.Count, match);
        }

        /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns
        /// the zero-based index of the last occurrence within the range of elements in the <see cref="ProcessQueue{T}"/> that contains the
        /// specified number of elements and ends at the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by
        /// match, if found; otherwise, –1.</returns>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="match">The Predicate delegate that defines the conditions of the element to search for.</param>
        /// <exception cref="ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>
        /// -or- count is less than 0 -or- startIndex and count do not specify a valid section in the <see cref="ProcessQueue{T}"/>.</exception>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (startIndex < 0 || count < 0 || startIndex + count > m_processQueue.Count)
                        throw new ArgumentOutOfRangeException("startIndex", "startIndex and/or count is outside the range of valid indexes for the queue");

                    if (match == null)
                        throw new ArgumentNullException("match", "match is null");

                    int foundindex = -1;

                    for (int x = startIndex + count - 1; x >= startIndex; x--)
                    {
                        if (match(m_processQueue[x]))
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.FindLastIndex(startIndex, count, match);
                }
            }
        }

        /// <summary>Performs the specified action on each element of the <see cref="ProcessQueue{T}"/>.</summary>
        /// <param name="action">The Action delegate to perform on each element of the <see cref="ProcessQueue{T}"/>.</param>
        /// <exception cref="ArgumentNullException">action is null.</exception>
        public virtual void ForEach(Action<T> action)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (action == null)
                        throw new ArgumentNullException("action", "action is null");

                    foreach (T item in m_processQueue)
                    {
                        action(item);
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processQueue.ForEach(action);
                }
            }
        }

        /// <summary>Creates a shallow copy of a range of elements in the source <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>A shallow copy of a range of elements in the source <see cref="ProcessQueue{T}"/>.</returns>
        /// <param name="count">The number of elements in the range.</param>
        /// <param name="index">The zero-based <see cref="ProcessQueue{T}"/> index at which the range starts.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        /// <exception cref="ArgumentException">index and count do not denote a valid range of elements in the <see cref="ProcessQueue{T}"/>.</exception>
        public virtual List<T> GetRange(int index, int count)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index + count > m_processQueue.Count)
                        throw new ArgumentException("Index and count do not denote a valid range of elements in the queue");

                    if (index < 0 || count < 0)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    List<T> items = new List<T>();

                    for (int x = index; x < index + count; x++)
                    {
                        items.Add(m_processQueue[x]);
                    }

                    return items;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.GetRange(index, count);
                }
            }
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within
        /// the range of elements in the <see cref="ProcessQueue{T}"/> that extends from the specified index to the last element.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="ProcessQueue{T}"/> that
        /// extends from index to the last element, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>.</exception>
        public virtual int IndexOf(T item, int index)
        {
            return IndexOf(item, index, m_processQueue.Count);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within
        /// the range of elements in the <see cref="ProcessQueue{T}"/> that starts at the specified index and contains the specified number of
        /// elements.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the range of elements in the <see cref="ProcessQueue{T}"/> that
        /// starts at index and contains count number of elements, if found; otherwise, –1.</returns>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>
        /// -or- count is less than 0 -or- index and count do not specify a valid section in the <see cref="ProcessQueue{T}"/>.</exception>
        public virtual int IndexOf(T item, int index, int count)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || count < 0 || index + count > m_processQueue.Count)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    int foundindex = -1;
                    Comparer<T> comparer = Comparer<T>.Default;

                    for (int x = index; x < index + count; x++)
                    {
                        if (comparer.Compare(item, m_processQueue[x]) == 0)
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.IndexOf(item, index, count);
                }
            }
        }

        /// <summary>Inserts the elements of a collection into the <see cref="ProcessQueue{T}"/> at the specified index.</summary>
        /// <param name="collection">The collection whose elements should be inserted into the <see cref="ProcessQueue{T}"/>. The collection
        /// itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than <see cref="ProcessQueue{T}"/> length.</exception>
        /// <exception cref="ArgumentNullException">collection is null.</exception>
        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || index > m_processQueue.Count - 1)
                        throw new ArgumentOutOfRangeException("index", "index is outside the range of valid indexes for the queue");

                    if (collection == null)
                        throw new ArgumentNullException("collection", "collection is null");

                    foreach (T item in collection)
                    {
                        m_processQueue.Insert(index, item);
                        index++;
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processQueue.InsertRange(index, collection);
                }

                DataAdded();
            }
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
        /// entire <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the entire the <see cref="ProcessQueue{T}"/>, if found;
        /// otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        public virtual int LastIndexOf(T item)
        {
            return LastIndexOf(item, 0, m_processQueue.Count);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
        /// range of elements in the <see cref="ProcessQueue{T}"/> that extends from the first element to the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the <see cref="ProcessQueue{T}"/> that
        /// extends from the first element to index, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/>. </exception>
        public virtual int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, m_processQueue.Count);
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
        /// range of elements in the <see cref="ProcessQueue{T}"/> that contains the specified number of elements and ends at the specified index.</summary>
        /// <returns>The zero-based index of the last occurrence of item within the range of elements in the <see cref="ProcessQueue{T}"/> that
        /// contains count number of elements and ends at index, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="ProcessQueue{T}"/> -or-
        /// count is less than 0 -or- index and count do not specify a valid section in the <see cref="ProcessQueue{T}"/>.</exception>
        public virtual int LastIndexOf(T item, int index, int count)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || count < 0 || index + count > m_processQueue.Count)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    int foundindex = -1;
                    Comparer<T> comparer = Comparer<T>.Default;

                    for (int x = index + count - 1; x >= index; x--)
                    {
                        if (comparer.Compare(item, m_processQueue[x]) == 0)
                        {
                            foundindex = x;
                            break;
                        }
                    }

                    return foundindex;
                }
                else
                {
                    // Otherwise, we'll call native implementation.
                    return processQueue.LastIndexOf(item, index, count);
                }
            }
        }

        /// <summary>Removes the all the elements that match the conditions defined by the specified predicate.</summary>
        /// <returns>The number of elements removed from the <see cref="ProcessQueue{T}"/>.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual int RemoveAll(Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (match == null)
                        throw new ArgumentNullException("match", "match is null");

                    int removedItems = 0;

                    // Process removal from the bottom up to maintain proper index access
                    for (int x = m_processQueue.Count - 1; x >= 0; x--)
                    {
                        if (match(m_processQueue[x]))
                        {
                            m_processQueue.RemoveAt(x);
                            removedItems++;
                        }
                    }

                    return removedItems;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.RemoveAll(match);
                }
            }
        }

        /// <summary>Removes a range of elements from the <see cref="ProcessQueue{T}"/>.</summary>
        /// <param name="count">The number of elements to remove.</param>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        /// <exception cref="ArgumentException">index and count do not denote a valid range of elements in the <see cref="ProcessQueue{T}"/>.</exception>
        public virtual void RemoveRange(int index, int count)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index < 0 || count < 0 || index + count > m_processQueue.Count)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    for (int x = index + count - 1; x >= index; x--)
                    {
                        m_processQueue.RemoveAt(x);
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processQueue.RemoveRange(index, count);
                }
            }
        }

        /// <summary>Reverses the order of the elements in the entire <see cref="ProcessQueue{T}"/>.</summary>
        public virtual void Reverse()
        {
            Reverse(0, m_processQueue.Count);
        }

        /// <summary>Reverses the order of the elements in the specified range.</summary>
        /// <param name="count">The number of elements in the range to reverse.</param>
        /// <param name="index">The zero-based starting index of the range to reverse.</param>
        /// <exception cref="ArgumentException">index and count do not denote a valid range of elements in the <see cref="ProcessQueue{T}"/>. </exception>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        public virtual void Reverse(int index, int count)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (index + count > m_processQueue.Count)
                        throw new ArgumentException("Index and count do not denote a valid range of elements in the queue");

                    if (index < 0 || count < 0)
                        throw new ArgumentOutOfRangeException("index", "Index and/or count is outside the range of valid indexes for the queue");

                    T item;
                    int stopIndex = index + count - 1;

                    for (int x = index; x < (index + count) / 2; x++)
                    {
                        if (x < stopIndex)
                        {
                            // Swaps items top to bottom to reverse order.
                            item = m_processQueue[x];
                            m_processQueue[x] = m_processQueue[stopIndex];
                            m_processQueue[stopIndex] = item;
                            stopIndex--;
                        }
                    }
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processQueue.Reverse(index, count);
                }
            }
        }

        /// <summary>Sorts the elements in the entire <see cref="ProcessQueue{T}"/>, using the default comparer.</summary>
        ///	<exception cref="InvalidOperationException">The default comparer, Generic.Comparer.Default, cannot find an
        /// implementation of the IComparable generic interface or the IComparable interface for type T.</exception>
        public virtual void Sort()
        {
            Sort(0, m_processQueue.Count, null);
        }

        /// <summary>Sorts the elements in the entire <see cref="ProcessQueue{T}"/>, using the specified comparer.</summary>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use
        /// the default comparer: Generic.Comparer.Default.</param>
        /// <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For
        /// example, comparer might not return 0 when comparing an item with itself.</exception>
        ///	<exception cref="InvalidOperationException">the comparer is null and the default comparer,
        /// Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the
        /// IComparable interface for type T.</exception>
        public virtual void Sort(IComparer<T> comparer)
        {
            Sort(0, m_processQueue.Count, comparer);
        }

        /// <summary>Sorts the elements in a range of elements in the <see cref="ProcessQueue{T}"/>, using the specified comparer.</summary>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements, or null to use
        /// the default comparer: Generic.Comparer.Default.</param>
        /// <exception cref="ArgumentException">The implementation of comparer caused an error during the sort. For
        /// example, comparer might not return 0 when comparing an item with itself.</exception>
        ///	<exception cref="InvalidOperationException">the comparer is null and the default comparer,
        /// Generic.Comparer.Default, cannot find an implementation of the IComparable generic interface or the
        /// IComparable interface for type T.</exception>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- count is less than 0.</exception>
        public virtual void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (comparer == null)
                        comparer = Comparer<T>.Default;

                    // This sort implementation is a little harsh, but the normal process queue uses List(Of T) and the
                    // keyed process queue is based on a sorted list anyway (i.e., no sorting needed); so, this alternate
                    // sort implementation exists for any future derived process queue possibly based on a non List(Of T)
                    // queue and will at least ensure that the function will perform as expected.
                    T[] items = ToArray();
                    Array.Sort<T>(items, index, count, comparer);
                    m_processQueue.Clear();
                    AddRange(items);
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    processQueue.Sort(index, count, comparer);
                }
            }
        }

        /// <summary>Sorts the elements in the entire <see cref="ProcessQueue{T}"/>, using the specified comparison.</summary>
        /// <param name="comparison">The comparison to use when comparing elements.</param>
        /// <exception cref="ArgumentException">The implementation of comparison caused an error during the sort. For
        /// example, comparison might not return 0 when comparing an item with itself.</exception>
        /// <exception cref="ArgumentNullException">comparison is null.</exception>
        public virtual void Sort(Comparison<T> comparison)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (comparison == null)
                        throw new ArgumentNullException("comparison", "comparison is null");

                    // This sort implementation is a little harsh, but the normal process queue uses List(Of T) and the
                    // keyed process queue is based on a sorted list anyway (i.e., no sorting needed); so, this alternate
                    // sort implementation exists for any future derived process queue possibly based on a non-List(Of T)
                    // queue and will at least ensure that the function will perform as expected. Maybe some clever
                    // programmer will come behind me and add some "Linq-y" expression that will magically do this...
                    T[] items = ToArray();
                    Array.Sort<T>(items, comparison);
                    m_processQueue.Clear();
                    AddRange(items);
                }
                else
                {
                    // Otherwise we'll call native implementation
                    processQueue.Sort(comparison);
                }
            }
        }

        /// <summary>Copies the elements of the <see cref="ProcessQueue{T}"/> to a new array.</summary>
        /// <returns>An array containing copies of the elements of the <see cref="ProcessQueue{T}"/>.</returns>
        public virtual T[] ToArray()
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    T[] items = new T[m_processQueue.Count];

                    for (int x = 0; x < m_processQueue.Count; x++)
                    {
                        items[x] = m_processQueue[x];
                    }

                    return items;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.ToArray();
                }
            }
        }

        /// <summary>Determines whether every element in the <see cref="ProcessQueue{T}"/> matches the conditions defined by the specified
        /// predicate.</summary>
        /// <returns>True, if every element in the <see cref="ProcessQueue{T}"/> matches the conditions defined by the specified predicate;
        /// otherwise, false. If the <see cref="ProcessQueue{T}"/> has no elements, the return value is true.</returns>
        /// <param name="match">The Predicate delegate that defines the conditions to check against the elements.</param>
        /// <exception cref="ArgumentNullException">match is null.</exception>
        public virtual bool TrueForAll(Predicate<T> match)
        {
            lock (m_processQueue)
            {
                List<T> processQueue = m_processQueue as List<T>;

                if (processQueue == null)
                {
                    // We manually implement this feature, if process queue is not a List(Of T).
                    if (match == null)
                        throw (new ArgumentNullException("match", "match is null"));

                    bool allTrue = true;

                    foreach (T item in m_processQueue)
                    {
                        if (!match(item))
                        {
                            allTrue = false;
                            break;
                        }
                    }

                    return allTrue;
                }
                else
                {
                    // Otherwise, we will call native implementation.
                    return processQueue.TrueForAll(match);
                }
            }
        }

        #endregion

        #region [ Handy Queue Functions Implementation ]

        // Note: All queue function implementations should be synchronized, as necessary.

        /// <summary>Inserts an item onto the top of the <see cref="ProcessQueue{T}"/>.</summary>
        /// <param name="item">The item to push onto the <see cref="ProcessQueue{T}"/>.</param>
        public virtual void Push(T item)
        {
            lock (m_processQueue)
            {
                m_processQueue.Insert(0, item);
                DataAdded();
            }
        }

        /// <summary>Removes the first item from the <see cref="ProcessQueue{T}"/>, and returns its value.</summary>
        /// <exception cref="IndexOutOfRangeException">There are no items in the <see cref="ProcessQueue{T}"/>.</exception>
        /// <returns>An object of generic type T.</returns>
        public virtual T Pop()
        {
            lock (m_processQueue)
            {
                if (m_processQueue.Count > 0)
                {
                    T poppedItem = m_processQueue[0];
                    m_processQueue.RemoveAt(0);
                    return poppedItem;
                }
                else
                {
                    throw new IndexOutOfRangeException("The " + Name + " is empty");
                }
            }
        }

        /// <summary>Removes the last item from the <see cref="ProcessQueue{T}"/>, and returns its value. (It's a dirty job, but someone has to do it.)</summary>
        /// <returns>An object of generic type T.</returns>
        /// <exception cref="IndexOutOfRangeException">There are no items in the <see cref="ProcessQueue{T}"/>.</exception>
        public virtual T Poop()
        {
            lock (m_processQueue)
            {
                if (m_processQueue.Count > 0)
                {
                    int lastIndex = m_processQueue.Count - 1;
                    T poopedItem = m_processQueue[lastIndex];
                    m_processQueue.RemoveAt(lastIndex);
                    return poopedItem;
                }
                else
                {
                    throw new IndexOutOfRangeException("The " + Name + " is empty");
                }
            }
        }

        #endregion

        #region [ Generic IList(Of T) Implementation ]

        // Note: All IList(Of T) implementations should be synchronized, as necessary.

        /// <summary>Adds an item to the <see cref="ProcessQueue{T}"/>.</summary>
        /// <param name="item">The item to add to the <see cref="ProcessQueue{T}"/>.</param>
        public virtual void Add(T item)
        {
            lock (m_processQueue)
            {
                m_processQueue.Add(item);
                DataAdded();
            }
        }

        /// <summary>Inserts an element into the <see cref="ProcessQueue{T}"/> at the specified index.</summary>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than <see cref="ProcessQueue{T}"/> length.</exception>
        public virtual void Insert(int index, T item)
        {
            lock (m_processQueue)
            {
                m_processQueue.Insert(index, item);
                DataAdded();
            }
        }

        /// <summary>Copies the entire <see cref="ProcessQueue{T}"/> to a compatible one-dimensional array, starting at the beginning of the
        /// target array.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from <see cref="ProcessQueue{T}"/>. The
        /// array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentException">arrayIndex is equal to or greater than the length of array -or- the
        /// number of elements in the source <see cref="ProcessQueue{T}"/> is greater than the available space from arrayIndex to the end of the
        /// destination array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            lock (m_processQueue)
            {
                m_processQueue.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>Returns an enumerator that iterates through the <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>An enumerator for the <see cref="ProcessQueue{T}"/>.</returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return m_processQueue.GetEnumerator();
        }

        /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within
        /// the entire <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="ProcessQueue{T}"/>, if found; otherwise, –1.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        public virtual int IndexOf(T item)
        {
            lock (m_processQueue)
            {
                return m_processQueue.IndexOf(item);
            }
        }


        /// <summary>Removes all elements from the <see cref="ProcessQueue{T}"/>.</summary>
        public virtual void Clear()
        {
            lock (m_processQueue)
            {
                m_processQueue.Clear();
            }
        }

        /// <summary>Determines whether an element is in the <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>True, if item is found in the <see cref="ProcessQueue{T}"/>; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        public virtual bool Contains(T item)
        {
            lock (m_processQueue)
            {
                return m_processQueue.Contains(item);
            }
        }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="ProcessQueue{T}"/>.</summary>
        /// <returns>True, if item is successfully removed; otherwise, false. This method also returns false if item was
        /// not found in the <see cref="ProcessQueue{T}"/>.</returns>
        /// <param name="item">The object to remove from the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        public virtual bool Remove(T item)
        {
            lock (m_processQueue)
            {
                return m_processQueue.Remove(item);
            }
        }

        /// <summary>Removes the element at the specified index of the <see cref="ProcessQueue{T}"/>.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than
        /// <see cref="ProcessQueue{T}"/> length.</exception>
        public virtual void RemoveAt(int index)
        {
            lock (m_processQueue)
            {
                m_processQueue.RemoveAt(index);
            }
        }

        /// <summary>
        /// Gets an enumerator of all items within the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_processQueue).GetEnumerator();
        }

        /// <summary>
        /// Copies the elements of the <see cref="ProcessQueue{T}"/> to an <see cref="System.Array"/>, starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="System.Array"/> that is the destination of the elements 
        /// copied from the <see cref="ProcessQueue{T}"/>. The array must have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            CopyTo(array, index);
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