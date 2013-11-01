//******************************************************************************************************
//  IActionAdapter.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  11/01/2013 - Stephen C. Wills
//       Updated to process time-series entities.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.TimeSeries.Routing;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the abstract interface for any action adapter.
    /// </summary>
    public interface IActionAdapter : IAdapter
    {
        /// <summary>
        /// Provides new time-series entities from action adapter.
        /// </summary>
        event EventHandler<RoutingEventArgs> NewEntities;

        /// <summary>
        /// Event is raised every five seconds allowing host to track total number of unprocessed entities.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Implementations of this interface are expected to report current queue size of unprocessed
        /// time-series entities so that if queue size reaches an unhealthy threshold, host can take action.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is total number of unprocessed entities.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<int>> UnprocessedEntities;

        /// <summary>
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on input demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is set up to connect on demand. In the case of respecting auto-start input demands,
        /// as an example, this would be <c>false</c> for an action adapter that calculated measurements, but <c>true</c> for an action adapter used to archive inputs.
        /// </remarks>
        bool RespectInputDemands
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag indicating if action adapter should respect auto-start requests based on output demands.
        /// </summary>
        /// <remarks>
        /// Action adapters are in the curious position of being able to both consume and produce points, as such the user needs to be able to control how their
        /// adapter will behave concerning routing demands when the adapter is setup to connect on demand. In the case of respecting auto-start output demands,
        /// as an example, this would be <c>true</c> for an action adapter that calculated measurements, but <c>false</c> for an action adapter used to archive inputs.
        /// </remarks>
        bool RespectOutputDemands
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter input signals.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of signals based on the source of the measurement keys.
        /// Set to <c>null</c> to apply no filter.
        /// </remarks>
        string[] InputSourceIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter output signals.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of signals based on the source of the measurement keys.
        /// Set to <c>null</c> to apply no filter.
        /// </remarks>
        string[] OutputSourceIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets input signals that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        ISet<Guid> RequestedInputSignals
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets output signals that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        ISet<Guid> RequestedOutputSignals
        {
            get;
            set;
        }

        /// <summary>
        /// Queues entities for processing.
        /// </summary>
        /// <param name="entities">Collection of entities to queue for processing.</param>
        void QueueEntitiesForProcessing(IEnumerable<ITimeSeriesEntity> entities);
    }
}