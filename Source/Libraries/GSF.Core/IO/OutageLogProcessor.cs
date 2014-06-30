//******************************************************************************************************
//  OutageLogProcessor.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/27/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Collections;

namespace GSF.IO
{
    /// <summary>
    /// Represents a thread-safe <see cref="OutageLog"/> processor that will operate on each <see cref="Outage"/> with a consumer provided function on independent threads.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is simply a <see cref="ProcessQueue{Outage}"/> that uses a pre-initialized <see cref="OutageLog"/> as its base collection.
    /// </para>
    /// <para>
    /// The <see cref="OutageLogProcessor"/> encapsulates the <see cref="OutageLog"/> in a thread-safe wrapper with full access to list operations.
    /// As a result, operations should generally be executed against the <see cref="OutageLogProcessor"/> rather than the <see cref="OutageLog"/>.
    /// </para>
    /// </remarks>
    public class OutageLogProcessor : ProcessQueue<Outage>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a <see cref="OutageLogProcessor"/> using a pre-initialized <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="outageLog">Pre-initialized <see cref="OutageLog"/> to process.</param>
        /// <param name="processItemFunction">A delegate <see cref="ProcessQueue{Outage}.ProcessItemFunctionSignature"/> that defines a function signature to process a key and value one at a time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether a process should requeue after an exception.</param>
        public OutageLogProcessor(OutageLog outageLog, ProcessItemFunctionSignature processItemFunction, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : this(outageLog, processItemFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Creates a <see cref="OutageLogProcessor"/> using a pre-initialized <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="outageLog">Pre-initialized <see cref="OutageLog"/> to process.</param>
        /// <param name="processItemFunction">A delegate <see cref="ProcessQueue{Outage}.ProcessItemFunctionSignature"/> that defines a function signature to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">Optional delegate <see cref="ProcessQueue{Outage}.CanProcessItemFunctionSignature"/> that determines of a key and value can currently be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether a process should requeue after an exception.</param>
        public OutageLogProcessor(OutageLog outageLog, ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction = null, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : base(processItemFunction, null, canProcessItemFunction, outageLog, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Creates a bulk-item <see cref="OutageLogProcessor"/> using a pre-initialized <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="outageLog">Pre-initialized <see cref="OutageLog"/> to process.</param>
        /// <param name="processItemsFunction">A delegate <see cref="ProcessQueue{Outage}.ProcessItemsFunctionSignature"/> that defines a function signature to process multiple items at once.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether a process should requeue after an exception.</param>
        public OutageLogProcessor(OutageLog outageLog, ProcessItemsFunctionSignature processItemsFunction, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : this(outageLog, processItemsFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        /// <summary>
        /// Creates a bulk-item <see cref="OutageLogProcessor"/> using a pre-initialized <see cref="OutageLog"/>.
        /// </summary>
        /// <param name="outageLog">Pre-initialized <see cref="OutageLog"/> to process.</param>
        /// <param name="processItemsFunction">A delegate <see cref="ProcessQueue{Outage}.ProcessItemsFunctionSignature"/> that defines a function signature to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">Optional delegate <see cref="ProcessQueue{Outage}.CanProcessItemFunctionSignature"/> that determines of a key and value can currently be processed.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="bool"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="bool"/> value that indicates whether a process should requeue after an exception.</param>
        public OutageLogProcessor(OutageLog outageLog, ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction = null, double processInterval = DefaultProcessInterval, int maximumThreads = DefaultMaximumThreads, int processTimeout = DefaultProcessTimeout, bool requeueOnTimeout = DefaultRequeueOnTimeout, bool requeueOnException = DefaultRequeueOnException)
            : base(null, processItemsFunction, canProcessItemFunction, outageLog, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
        }

        #endregion
    }
}