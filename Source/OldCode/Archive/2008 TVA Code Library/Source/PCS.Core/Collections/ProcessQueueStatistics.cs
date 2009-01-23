//*******************************************************************************************************
//  ProcessQueueStatistics.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/06/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System.Units;

namespace PCS.Collections
{
    /// <summary>
    /// Represents the current process queue statistics.
    /// </summary>
    public struct ProcessQueueStatistics
    {
        /// <summary>
        /// Gets indicator that the <see cref="ProcessQueue{T}"/> is currently enabled.
        /// </summary>
        public bool IsEnabled;

        /// <summary>
        /// Gets indicator that the <see cref="ProcessQueue{T}"/> is actively processing items.
        /// </summary>
        public bool IsProcessing;

        /// <summary>
        /// Gets the interval, in milliseconds, on which new items begin processing.
        /// </summary>
        public double ProcessingInterval;
        
        /// <summary>
        /// Gets the maximum time, in milliseconds, allowed for processing an item.
        /// </summary>
        public int ProcessTimeout;

        /// <summary>
        /// Gets the current <see cref="QueueThreadingMode"/> for the <see cref="ProcessQueue{T}"/> (i.e., synchronous or asynchronous).
        /// </summary>
        public QueueThreadingMode ThreadingMode;

        /// <summary>
        /// Gets the item <see cref="QueueProcessingStyle"/> for the <see cref="ProcessQueue{T}"/> (i.e., one at a time or many at once).
        /// </summary>
        public QueueProcessingStyle ProcessingStyle;

        /// <summary>
        /// Gets the total amount of time, in seconds, that the process <see cref="ProcessQueue{T}"/> has been active.
        /// </summary>
        public Time RunTime;

        /// <summary>
        /// Gets the current number of active threads.
        /// </summary>
        public int ActiveThreads;

        /// <summary>
        /// Gets the number of elements queued for processing in the <see cref="ProcessQueue{T}"/>.
        /// </summary>
        public int QueueCount;

        /// <summary>
        /// Gets the total number of items currently being processed.
        /// </summary>
        public long ItemsBeingProcessed;

        /// <summary>
        /// Gets the total number of items processed so far.
        /// </summary>
        public long TotalProcessedItems;
    }
}
