//******************************************************************************************************
//  RoutingEventArgs.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  10/21/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Routing
{
    /// <summary>
    /// Arguments for routing events which route time-series signals to their destinations.
    /// </summary>
    public class RoutingEventArgs : EventArgs
    {
        #region [ Members ]

        // Fields
        private ICollection<ITimeSeriesEntity> m_timeSeriesEntities; 
        private IDictionary<Guid, ICollection<SignalRoute>> m_signalRoutesLookup;
        private IDictionary<IAdapter, ICollection<SignalRoute>> m_adapterRoutes;
        private int m_cacheVersion;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the collection of time-series entities to be routed.
        /// </summary>
        public ICollection<ITimeSeriesEntity> TimeSeriesEntities
        {
            get
            {
                return m_timeSeriesEntities;
            }
            set
            {
                m_timeSeriesEntities = value;
            }
        }

        /// <summary>
        /// Gets the lookup table for signal routes.
        /// </summary>
        public IDictionary<Guid, ICollection<SignalRoute>> SignalRoutesLookup
        {
            get
            {
                return m_signalRoutesLookup;
            }
        }

        /// <summary>
        /// Gets the lookup table for adapters' generic routes.
        /// </summary>
        public IDictionary<IAdapter, ICollection<SignalRoute>> AdapterRoutes
        {
            get
            {
                return m_adapterRoutes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the lookup tables, and also clears
        /// them out when the cache version changes.
        /// </summary>
        /// <param name="cacheVersion">
        /// The version of the cache which determines whether to
        ///  maintain the current lookup tables or clear them out.
        /// </param>
        public void Initialize(int cacheVersion)
        {
            if ((object)m_timeSeriesEntities == null)
                m_timeSeriesEntities = new List<ITimeSeriesEntity>();

            if ((object)m_signalRoutesLookup == null)
                m_signalRoutesLookup = new Dictionary<Guid, ICollection<SignalRoute>>();

            if ((object)m_adapterRoutes == null)
                m_adapterRoutes = new Dictionary<IAdapter, ICollection<SignalRoute>>();

            if (cacheVersion != m_cacheVersion)
            {
                m_signalRoutesLookup.Clear();
                m_adapterRoutes.Clear();
                m_cacheVersion = cacheVersion;
            }
        }

        #endregion
    }
}
