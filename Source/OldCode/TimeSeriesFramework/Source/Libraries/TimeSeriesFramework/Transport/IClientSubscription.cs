//******************************************************************************************************
//  IClientSubscription.cs - Gbtc
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
//  06/24/2011 - Ritchie
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using TimeSeriesFramework.Adapters;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents a common set of interfaces for a client adapter subscription to the <see cref="DataPublisher"/>.
    /// </summary>
    public interface IClientSubscription : IActionAdapter
    {
        /// <summary>
        /// Gets the <see cref="Guid"/> client TCP connection identifier of this <see cref="IClientSubscription"/>.
        /// </summary>
        Guid ClientID
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> based subscriber ID of this <see cref="IClientSubscription"/>.
        /// </summary>
        Guid SubscriberID
        {
            get;
        }

        /// <summary>
        /// Gets the current signal index cache of this <see cref="IClientSubscription"/>.
        /// </summary>
        SignalIndexCache SignalIndexCache
        {
            get;
        }

        /// <summary>
        /// Gets or sets flag that determines if the compact measurement format should be used in data packets of this <see cref="IClientSubscription"/>.
        /// </summary>
        bool UseCompactMeasurementFormat
        {
            get;
            set;
        }
    }
}
