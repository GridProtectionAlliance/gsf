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
using System.Linq;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Signal index cross reference.
    /// </summary>
    [Serializable]
    public class SignalIndexCache
    {
        #region [ Members ]

        // Fields
        private ConcurrentDictionary<ushort, Tuple<Guid, MeasurementKey>> m_reference;
        private MeasurementKey[] m_unauthorizedKeys;

        [NonSerialized] // SignalID reverse lookup runtime cache
        private ConcurrentDictionary<Guid, ushort> m_signalIDCache;

        [NonSerialized] // MeasurementKey reverse lookup runtime cache
        private ConcurrentDictionary<MeasurementKey, ushort> m_keyCache;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SignalIndexCache"/> instance.
        /// </summary>
        public SignalIndexCache()
        {
            m_reference = new ConcurrentDictionary<ushort, Tuple<Guid, MeasurementKey>>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets integer signal index cross reference dictionary.
        /// </summary>
        public ConcurrentDictionary<ushort, Tuple<Guid, MeasurementKey>> Reference
        {
            get
            {
                return m_reference;
            }
        }

        /// <summary>
        /// Gets reference to array of requested input measurement keys that were authorized.
        /// </summary>
        public MeasurementKey[] AuthorizedKeys
        {
            get
            {
                return m_reference.Select(kvp => kvp.Value.Item2).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets reference to array of requested input measurement keys that were unauthorized.
        /// </summary>
        public MeasurementKey[] UnauthorizedKeys
        {
            get
            {
                return m_unauthorizedKeys;
            }
            set
            {
                m_unauthorizedKeys = value;
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
            if (m_signalIDCache == null)
                m_signalIDCache = new ConcurrentDictionary<Guid, ushort>();

            return m_signalIDCache.GetOrAdd(signalID, id => m_reference.First(kvp => kvp.Value.Item1 == id).Key);
        }

        /// <summary>
        /// Gets runtime signal index for given <see cref="MeasurementKey"/>.
        /// </summary>
        /// <param name="key"><see cref="MeasurementKey"/> used to lookup associated runtime signal index.</param>
        /// <returns>Runtime signal index for given <see cref="MeasurementKey"/> <paramref name="key"/>.</returns>
        public ushort GetSignalIndex(MeasurementKey key)
        {
            if (m_keyCache == null)
                m_keyCache = new ConcurrentDictionary<MeasurementKey, ushort>();

            return m_keyCache.GetOrAdd(key, mk => m_reference.First(kvp => kvp.Value.Item2.Equals(mk)).Key);
        }

        #endregion
    }
}
