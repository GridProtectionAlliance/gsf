//******************************************************************************************************
//  SignalIndexCache.cs - Gbtc
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
//  05/15/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TVA;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents a serializable <see cref="Guid"/> signal ID to <see cref="ushort"/> index cross reference.
    /// </summary>
    /// <remarks>
    /// This class is used to create a runtime index to be used for data exchange so that a 16-bit integer
    /// is exchanged in the data packets for signal identification instead of the 128-bit Guid signal ID
    /// to reduce bandwidth required for signal exchange. This means the total number of unique signal
    /// IDs that could be exchanged using this method in a single session is 65,535. This number seems
    /// reasonable for the currently envisioned use cases, however, multiple sessions each with their own
    /// runtime signal index cache could be established if this is a limitation for a given data set.
    /// </remarks>
    [Serializable]
    public class SignalIndexCache
    {
        #region [ Members ]

        // Fields
        private Guid m_subscriberID;

        // Since measurement keys are statically cached as a global system optimization and the keys
        // can be different between two parties exchanging data, the raw measurement key elements are
        // cached and exchanged instead of actual measurement key values
        private ConcurrentDictionary<ushort, Tuple<Guid, string, uint>> m_reference;
        private Guid[] m_unauthorizedSignalIDs;

        [NonSerialized] // SignalID reverse lookup runtime cache (used to speed deserialization)
        private ConcurrentDictionary<Guid, ushort> m_signalIDCache;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SignalIndexCache"/> instance.
        /// </summary>
        public SignalIndexCache()
        {
            m_reference = new ConcurrentDictionary<ushort, Tuple<Guid, string, uint>>();
            m_signalIDCache = new ConcurrentDictionary<Guid, ushort>();
        }

        /// <summary>
        /// Creates a new local system cache from one that was received remotely.
        /// </summary>
        /// <param name="dataSource"><see cref="DataSet"/> based data source used to interpret local measurement keys.</param>
        /// <param name="remoteCache">Deserialized remote signal index cache.</param>
        public SignalIndexCache(DataSet dataSource, SignalIndexCache remoteCache)
        {
            m_subscriberID = remoteCache.SubscriberID;

            // If active measurements are defined, interpret signal cache in context of current measurement key definitions
            if (dataSource != null && dataSource.Tables != null && dataSource.Tables.Contains("ActiveMeasurements"))
            {
                DataTable activeMeasurements = dataSource.Tables["ActiveMeasurements"];
                m_reference = new ConcurrentDictionary<ushort, Tuple<Guid, string, uint>>();

                foreach (KeyValuePair<ushort, Tuple<Guid, string, uint>> signalIndex in remoteCache.Reference)
                {
                    Guid signalID = signalIndex.Value.Item1;
                    DataRow[] filteredRows = activeMeasurements.Select("SignalID = '" + signalID.ToString() + "'");

                    if (filteredRows.Length > 0)
                    {
                        DataRow row = filteredRows[0];
                        MeasurementKey key = MeasurementKey.Parse(row["ID"].ToNonNullString("_:0"), signalID);
                        m_reference.TryAdd(signalIndex.Key, new Tuple<Guid, string, uint>(signalID, key.Source, key.ID));
                    }
                }

                m_unauthorizedSignalIDs = remoteCache.UnauthorizedSignalIDs;
            }
            else
            {
                // Just use remote signal index cache as-is if no local configuration exists
                m_reference = remoteCache.Reference;
                m_unauthorizedSignalIDs = remoteCache.UnauthorizedSignalIDs;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based subscriber ID of this <see cref="SignalIndexCache"/>.
        /// </summary>
        public Guid SubscriberID
        {
            get
            {
                return m_subscriberID;
            }
            set
            {
                m_subscriberID = value;
            }
        }

        /// <summary>
        /// Gets or sets integer signal index cross reference dictionary.
        /// </summary>
        public ConcurrentDictionary<ushort, Tuple<Guid, string, uint>> Reference
        {
            get
            {
                return m_reference;
            }
            set
            {
                m_signalIDCache.Clear();
                m_reference = value;
            }
        }

        /// <summary>
        /// Gets reference to array of requested input measurement signal IDs that were authorized.
        /// </summary>
        public Guid[] AuthorizedSignalIDs
        {
            get
            {
                return m_reference.Select(kvp => kvp.Value.Item1).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets reference to array of requested input measurement signal IDs that were unauthorized.
        /// </summary>
        public Guid[] UnauthorizedSignalIDs
        {
            get
            {
                return m_unauthorizedSignalIDs;
            }
            set
            {
                m_unauthorizedSignalIDs = value;
            }
        }

        /// <summary>
        /// Gets the current maximum integer signal index.
        /// </summary>
        public ushort MaximumIndex
        {
            get
            {
                if (m_reference.Count == 0)
                    return 0;

                return (ushort)(m_reference.Max(kvp => kvp.Key) + 1);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets runtime signal index for given <see cref="Guid"/> signal ID.
        /// </summary>
        /// <param name="signalID"><see cref="Guid"/> signal ID used to lookup associated runtime signal index.</param>
        /// <returns>Runtime signal index for given <see cref="Guid"/> <paramref name="signalID"/>.</returns>
        public ushort GetSignalIndex(Guid signalID)
        {
            ushort index = ushort.MaxValue;

            if (!m_signalIDCache.TryGetValue(signalID, out index))
            {
                foreach (KeyValuePair<ushort, Tuple<Guid, string, uint>> item in m_reference)
                {
                    if (item.Value.Item1 == signalID)
                    {
                        index = item.Key;
                        break;
                    }
                }

                if (index != ushort.MaxValue)
                    m_signalIDCache.TryAdd(signalID, index);
            }

            return index;
        }

        #endregion
    }
}
