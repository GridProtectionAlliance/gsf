//******************************************************************************************************
//  Metadata.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/05/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using InStep.eDNA.EzDNAApiNet;

namespace eDNAAdapters
{
    /// <summary>
    /// Defines a class used to query eDNA meta-data.
    /// </summary>
    public class Metadata
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Channel number field.
        /// </summary>
        public int ChannelNumber = -1;

        /// <summary>
        /// Site field.
        /// </summary>
        public string Site = "*";

        /// <summary>
        /// Service field.
        /// </summary>
        public string Service = "*";

        /// <summary>
        /// Short ID (a.k.a. Point ID) field.
        /// </summary>
        public string ShortID = "*";

        /// <summary>
        /// Long ID field.
        /// </summary>
        public string LongID = "*";

        /// <summary>
        /// Extended ID field.
        /// </summary>
        public string ExtendedID = "*";

        /// <summary>
        /// Description field.
        /// </summary>
        public string Description = "*";

        /// <summary>
        /// Extended description field.
        /// </summary>
        public string ExtendedDescription = "*";

        /// <summary>
        /// Point type field, "AI" or "DI".
        /// </summary>
        public string PointType = "*";

        /// <summary>
        /// Units field.
        /// </summary>
        public string Units = "*";

        /// <summary>
        /// Reference field 1.
        /// </summary>
        public string ReferenceField01 = "*";

        /// <summary>
        /// Reference field 2.
        /// </summary>
        public string ReferenceField02 = "*";

        /// <summary>
        /// Reference field 3.
        /// </summary>
        public string ReferenceField03 = "*";

        /// <summary>
        /// Reference field 4.
        /// </summary>
        public string ReferenceField04 = "*";

        /// <summary>
        /// Reference field 5.
        /// </summary>
        public string ReferenceField05 = "*";

        /// <summary>
        /// Reference field 6.
        /// </summary>
        public string ReferenceField06 = "*";

        /// <summary>
        /// Reference field 7.
        /// </summary>
        public string ReferenceField07 = "*";

        /// <summary>
        /// Reference field 8.
        /// </summary>
        public string ReferenceField08 = "*";

        /// <summary>
        /// Reference field 9.
        /// </summary>
        public string ReferenceField09 = "*";

        /// <summary>
        /// Reference field 10.
        /// </summary>
        public string ReferenceField10 = "*";

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Queries eDNA meta-data for values defined in <paramref name="search"/> values.
        /// </summary>
        /// <param name="search"><see cref="Metadata"/> values to search.</param>
        /// <returns>Values that match search criteria.</returns>
        public static IEnumerable<Metadata> Query(Metadata search)
        {
            // Execute search
            int key, result = Configuration.EzSimpleFindPoints(search.Site, search.Service, search.ShortID, search.LongID,
                search.ExtendedID, search.Description, search.ExtendedDescription, search.PointType, search.Units,
                search.ReferenceField01, search.ReferenceField02, search.ReferenceField03, search.ReferenceField04,
                search.ReferenceField05, search.ReferenceField06, search.ReferenceField07, search.ReferenceField08,
                search.ReferenceField09, search.ReferenceField10, search.ChannelNumber, out key);

            if (result != 0)
            {
                string error;
                Configuration.EzSimpleFindPointsGetLastError(out error);
                throw new EzDNAApiNetException($"Failed to execute eDNA meta-data query: {error}", result);
            }

            try
            {
                // Get search result count
                int count = Configuration.EzSimpleFindPointsSize(key);

                for (int i = 0; i < count; i++)
                {
                    // Create new meta-data record to hold result
                    Metadata record = new Metadata();

                    // Query meta-data record values
                    result = Configuration.EzSimpleFindPointsRec(key, i, out record.Site, out record.Service, out record.ShortID,
                        out record.LongID, out record.ExtendedID, out record.Description, out record.ExtendedDescription,
                        out record.PointType, out record.Units, out record.ReferenceField01, out record.ReferenceField02,
                        out record.ReferenceField03, out record.ReferenceField04, out record.ReferenceField05,
                        out record.ReferenceField06, out record.ReferenceField07, out record.ReferenceField08,
                        out record.ReferenceField09, out record.ReferenceField10, out record.ChannelNumber);

                    if (result != 0)
                    {
                        string error;
                        Configuration.EzSimpleFindPointsGetLastError(out error);
                        throw new EzDNAApiNetException($"Failed to read eDNA meta-data record {i} for key {key}: {error}", result);
                    }

                    yield return record;
                }
            }
            finally
            {
                // Close search handle
                Configuration.EzFindPointsRemoveKey(key);
            }
        }

        #endregion
    }
}