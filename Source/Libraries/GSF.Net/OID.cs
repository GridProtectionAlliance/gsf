//******************************************************************************************************
//  OID.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/10/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GSF.Net
{
    /// <summary>
    /// Defines global object identifiers used by the Grid Solutions Framework.
    /// </summary>
    public static class OID
    {
        /// <summary>
        /// Root GSF OID (1.3.6.1.4.1.56056). This is the private enterprise number defined for the Grid Protection Alliance.
        /// </summary>
        public static readonly uint[] EnterpriseRoot = { 1, 3, 6, 1, 4, 1, 56056 };

        /// <summary>
        /// Root GSF OID for SNMP objects (1.3.6.1.4.1.56056.1).
        /// </summary>
        public static readonly uint[] SnmpRoot = EnterpriseRoot.Append(1U);

        /// <summary>
        /// Root GSF OID for SNMP statistic values (1.3.6.1.4.1.56056.1.1).
        /// </summary>
        public static readonly uint[] SnmpStatsRoot = SnmpRoot.Append(1U);

        /// <summary>
        /// GSF SNMP OIDs for known statistic sources (1.3.6.1.4.1.56056.1.1.x).
        /// </summary>
        /// <remarks>
        /// Each source category will subdivide specific statistics by signal index.
        /// </remarks>
        public static readonly ReadOnlyDictionary<string, uint[]> SnmpStats = new ReadOnlyDictionary<string, uint[]>(new Dictionary<string, uint[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["System"] = SnmpStatsRoot.Append(1U),
            ["Device"] = SnmpStatsRoot.Append(2U),
            ["InputStream"] = SnmpStatsRoot.Append(3U),
            ["OutputStream"] = SnmpStatsRoot.Append(4U),
            ["Subscriber"] = SnmpStatsRoot.Append(5U),
            ["Publisher"] = SnmpStatsRoot.Append(6U),
            ["Process"] = SnmpStatsRoot.Append(7U),
            ["Downloader"] = SnmpStatsRoot.Append(8U)
        });

        /// <summary>
        /// Root GSF OID for LDAP objects (1.3.6.1.4.1.56056.2).
        /// </summary>
        public static readonly uint[] LdapRoot = EnterpriseRoot.Append(2U);

        /// <summary>
        /// Appends an new OID value to an existing OID value array.
        /// </summary>
        /// <param name="oid">Root OID value array.</param>
        /// <param name="value">New OID value to append.</param>
        /// <returns>Combined OID value array.</returns>
        public static uint[] Append(this uint[] oid, uint value) => oid.Concat(new[] { value }).ToArray();
    }
}
