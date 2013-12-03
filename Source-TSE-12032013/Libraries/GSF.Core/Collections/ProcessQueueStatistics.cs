//******************************************************************************************************
//  ProcessQueueStatistics.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/06/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.Units;

namespace GSF.Collections
{
    /// <summary>
    /// Represents the statistics of a <see cref="ProcessQueue{T}"/>.
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
