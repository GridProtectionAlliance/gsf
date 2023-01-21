//******************************************************************************************************
//  SubscriberRightsLookup.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  09/20/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using GSF.Data;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a lookup table that determines whether
    /// a given subscriber has rights to specific signals.
    /// </summary>
    public class SubscriberRightsLookup
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SubscriberRightsLookup"/> class.
        /// </summary>
        /// <param name="dataSource">The source of metadata providing the tables required by the rights logic.</param>
        /// <param name="subscriberID">The ID of the subscriber whose rights are being looked up.</param>
        public SubscriberRightsLookup(DataSet dataSource, Guid subscriberID)
        {
            HasRightsFunc = BuildLookup(dataSource, subscriberID);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the function that determines whether the subscriber has rights to a given signal.
        /// </summary>
        public Func<Guid, bool> HasRightsFunc { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the subscriber has rights to a given signal.
        /// </summary>
        /// <param name="signalID">The ID of the signal.</param>
        /// <returns>True if the subscriber has rights; false otherwise.</returns>
        public bool HasRights(Guid signalID)
        {
            return HasRightsFunc(signalID);
        }

        private Func<Guid, bool> BuildLookup(DataSet dataSource, Guid subscriberID)
        {
            HashSet<Guid> authorizedSignals = new();

            const string filterRegex = @"(ALLOW|DENY)\s+WHERE\s+([^;]*)";

            //==================================================================
            //Check if subscriber is disabled or removed

            // If subscriber has been disabled or removed
            // from the list of valid subscribers,
            // they no longer have rights to any signals
            DataRow subscriber = dataSource.Tables["Subscribers"].Select($"ID = '{subscriberID}' AND Enabled <> 0").FirstOrDefault();

            if (subscriber is null)
                return _ => false;

            //=================================================================
            // Check group implicitly authorized signals

            DataRow[] subscriberMeasurementGroups = dataSource.Tables["SubscriberMeasurementGroups"].Select($"SubscriberID = '{subscriberID}'");

            subscriberMeasurementGroups
                .Join(dataSource.Tables["MeasurementGroups"].Select(),
                    row => row.ConvertField<int>("MeasurementGroupID"),
                    row => row.ConvertField<int>("ID"),
                    (subscriberMeasurementGroup, measurementGroup) =>
                    {
                        bool allowed = subscriberMeasurementGroup.ConvertField<bool>("Allowed");
                        string filter = measurementGroup.ConvertField<string>("FilterExpression");

                        return AdapterBase.ParseInputMeasurementKeys(dataSource, false, filter)
                            .Select(key => new { Allowed = allowed, key.SignalID });
                    })
                .SelectMany(list => list)
                .GroupBy(obj => obj.SignalID)
                .Where(grouping => grouping.All(obj => obj.Allowed))
                .ToList()
                .ForEach(grouping => authorizedSignals.Add(grouping.Key));

            //=================================================================
            //Check implicitly authorized signals

            List<Match> matches = Regex.Matches(subscriber["AccessControlFilter"].ToNonNullString().ReplaceControlCharacters(), filterRegex, RegexOptions.IgnoreCase)
                .Cast<Match>()
                .ToList();

            // Combine individual allow statements into a single measurement filter
            string allowFilter = string.Join(" OR ", matches
                .Where(match => match.Groups[1].Value == "ALLOW")
                .Select(match => $"({match.Groups[2].Value})"));

            // Combine individual deny statements into a single measurement filter
            string denyFilter = string.Join(" OR ", matches
                .Where(match => match.Groups[1].Value == "DENY")
                .Select(match => $"({match.Groups[2].Value})"));

            if (!string.IsNullOrEmpty(allowFilter))
            {
                foreach (DataRow row in dataSource.Tables["ActiveMeasurements"].Select(allowFilter))
                    authorizedSignals.Add(row.ConvertField<Guid>("SignalID"));
            }

            if (!string.IsNullOrEmpty(denyFilter))
            {
                foreach (DataRow row in dataSource.Tables["ActiveMeasurements"].Select(denyFilter))
                    authorizedSignals.Remove(row.ConvertField<Guid>("SignalID"));
            }

            //==================================================================
            //Check explicit group authorizations

            subscriberMeasurementGroups
                .Join(dataSource.Tables["MeasurementGroupMeasurements"].Select(),
                    row => row.ConvertField<int>("MeasurementGroupID"),
                    row => row.ConvertField<int>("MeasurementGroupID"),
                    (subscriberMeasurementGroup, measurementGroupMeasurement) => new
                    {
                        Allowed = subscriberMeasurementGroup.ConvertField<bool>("Allowed"),
                        SignalID = measurementGroupMeasurement.ConvertField<Guid>("SignalID")
                    })
                .GroupBy(obj => obj.SignalID)
                .Select(grouping => new
                {
                    Allowed = grouping.All(obj => obj.Allowed),
                    SignalID = grouping.Key
                })
                .ToList()
                .ForEach(obj =>
                {
                    if (obj.Allowed)
                        authorizedSignals.Add(obj.SignalID);
                    else
                        authorizedSignals.Remove(obj.SignalID);
                });

            //===================================================================
            // Check explicit authorizations

            DataRow[] explicitAuthorizations = dataSource.Tables["SubscriberMeasurements"].Select($"SubscriberID = '{subscriberID}'");

            // Add all explicitly authorized signals to authorizedSignals
            foreach (DataRow explicitAuthorization in explicitAuthorizations)
            {
                if (explicitAuthorization.ConvertField<bool>("Allowed"))
                    authorizedSignals.Add(explicitAuthorization.ConvertField<Guid>("SignalID"));
            }

            // Remove all explicitly unauthorized signals from authorizedSignals
            foreach (DataRow explicitAthorization in explicitAuthorizations)
            {
                if (!explicitAthorization.ConvertField<bool>("Allowed"))
                    authorizedSignals.Remove(explicitAthorization.ConvertField<Guid>("SignalID"));
            }

            return id => authorizedSignals.Contains(id);
        }

        #endregion
    }
}
