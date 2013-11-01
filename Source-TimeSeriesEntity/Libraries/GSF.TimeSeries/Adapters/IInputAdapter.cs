//******************************************************************************************************
//  IInputAdapter.cs - Gbtc
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
    /// Represents the abstract interface for any incoming adapter.
    /// </summary>
    public interface IInputAdapter : IAdapter
    {
        /// <summary>
        /// Provides new time-series entities from the input adapter.
        /// </summary>
        event EventHandler<RoutingEventArgs> NewEntities;

        /// <summary>
        /// Indicates to the host that processing for the input adapter has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        event EventHandler ProcessingComplete;

        /// <summary>
        /// Gets or sets <see cref="MeasurementKey.Source"/> values used to filter output signals.
        /// </summary>
        /// <remarks>
        /// This allows an adapter to associate itself with entire collections of signals based on the source of the measurement keys.
        /// Set to <c>null</c> apply no filter.
        /// </remarks>
        string[] OutputSourceIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets output signal IDs that are requested by other adapters based on what adapter says it can provide.
        /// </summary>
        ISet<Guid> RequestedOutputSignals
        {
            get;
            set;
        }
    }
}
