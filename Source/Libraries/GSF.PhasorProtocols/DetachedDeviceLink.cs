//******************************************************************************************************
//  DetachedDeviceLink.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/27/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Defines helper functions for managing connection string based parent / child device linkage used
    /// by concentrator style device connections whose child devices have been "detached" from the data
    /// model, i.e., child device records with a null <c>ParentID</c> so that the children appear as
    /// standalone devices while stream parsing still maps data to them through the parent connection.
    /// </summary>
    /// <remarks>
    /// A detached child device carries <c>parentID=&lt;parent Device.ID&gt;</c> (and typically
    /// <c>stationName=&lt;Device.Name&gt;</c>) in its connection string as a proxy for the database
    /// <c>ParentID</c> field. The parent concentrator device carries <c>detachedChildren=true</c> along
    /// with <c>databaseID=&lt;its own Device.ID&gt;</c> so the runtime can match children to the parent
    /// without requiring database access, e.g., when operating from cached configuration.
    /// </remarks>
    public static class DetachedDeviceLink
    {
        /// <summary>
        /// Connection string key, applied to a detached child device, whose value is the database ID of
        /// its parent concentrator device. Acts as a proxy for the database <c>ParentID</c> field.
        /// </summary>
        public const string ParentIDKey = "parentID";

        /// <summary>
        /// Connection string key, applied to a detached child device, whose value is the station name,
        /// i.e., <c>Device.Name</c>, of the child device. Used to preserve label based cell mapping since
        /// detached children are not exposed through the runtime input stream devices table.
        /// </summary>
        public const string StationNameKey = "stationName";

        /// <summary>
        /// Connection string key, applied to a parent concentrator device, that flags the connection as
        /// having detached, i.e., standalone modeled, child devices.
        /// </summary>
        public const string DetachedChildrenKey = "detachedChildren";

        /// <summary>
        /// Connection string key, applied to a parent concentrator device, whose value is the database ID
        /// of the parent device itself. Allows the runtime to match detached children, whose
        /// <see cref="ParentIDKey"/> values are database IDs, without requiring database access.
        /// </summary>
        public const string DatabaseIDKey = "databaseID";

        /// <summary>
        /// Gets the raw parent identifier, i.e., the <see cref="ParentIDKey"/> value, defined in the
        /// specified connection string, if any.
        /// </summary>
        /// <param name="connectionString">Connection string to check.</param>
        /// <returns>Defined parent identifier; otherwise, <c>null</c> when key is not defined.</returns>
        /// <remarks>
        /// Value is normally the numeric database ID of the parent device, but a device acronym is
        /// accepted as a manual configuration fallback.
        /// </remarks>
        public static string GetParentIdentifier(string connectionString)
        {
            Dictionary<string, string> settings = SafeParseKeyValuePairs(connectionString);

            if (settings is not null && settings.TryGetValue(ParentIDKey, out string value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();

            return null;
        }

        /// <summary>
        /// Attempts to parse the numeric parent device database ID defined in the specified connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to check.</param>
        /// <param name="parentID">Parsed parent device database ID, when defined.</param>
        /// <returns><c>true</c> if a numeric <see cref="ParentIDKey"/> value was defined; otherwise, <c>false</c>.</returns>
        public static bool TryParseParentID(string connectionString, out int parentID)
        {
            string identifier = GetParentIdentifier(connectionString);

            if (!string.IsNullOrWhiteSpace(identifier) && int.TryParse(identifier, out parentID))
                return true;

            parentID = 0;
            return false;
        }

        /// <summary>
        /// Gets the effective parent device database ID for a device, i.e., the defined database
        /// <paramref name="parentID"/> field value, when not <c>null</c>, falling back on any numeric
        /// <see cref="ParentIDKey"/> value defined in the device <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="parentID">Database <c>ParentID</c> field value for the device.</param>
        /// <param name="connectionString">Connection string for the device.</param>
        /// <returns>Effective parent device database ID; otherwise, <c>null</c> when device has no parent.</returns>
        public static int? GetEffectiveParentID(int? parentID, string connectionString)
        {
            if (parentID is not null)
                return parentID;

            return TryParseParentID(connectionString, out int detachedParentID) ? detachedParentID : null;
        }

        /// <summary>
        /// Gets flag that determines if the specified connection string identifies a detached child
        /// device, i.e., connection string defines a <see cref="ParentIDKey"/> value.
        /// </summary>
        /// <param name="connectionString">Connection string to check.</param>
        /// <returns><c>true</c> if connection string identifies a detached child device; otherwise, <c>false</c>.</returns>
        public static bool IsDetachedChild(string connectionString) =>
            GetParentIdentifier(connectionString) is not null;

        /// <summary>
        /// Gets the station name, i.e., the <see cref="StationNameKey"/> value, defined in the specified
        /// connection string, if any.
        /// </summary>
        /// <param name="connectionString">Connection string to check.</param>
        /// <returns>Defined station name; otherwise, <c>null</c> when key is not defined.</returns>
        public static string GetStationName(string connectionString)
        {
            Dictionary<string, string> settings = SafeParseKeyValuePairs(connectionString);

            if (settings is not null && settings.TryGetValue(StationNameKey, out string value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();

            return null;
        }

        /// <summary>
        /// Gets flag that determines if the specified connection string identifies a parent concentrator
        /// device with detached, i.e., standalone modeled, child devices.
        /// </summary>
        /// <param name="connectionString">Connection string to check.</param>
        /// <returns><c>true</c> if connection string defines <see cref="DetachedChildrenKey"/> as <c>true</c>; otherwise, <c>false</c>.</returns>
        public static bool HasDetachedChildren(string connectionString)
        {
            Dictionary<string, string> settings = SafeParseKeyValuePairs(connectionString);
            return settings is not null && settings.TryGetValue(DetachedChildrenKey, out string value) && value.ParseBoolean();
        }

        /// <summary>
        /// Gets the device database ID, i.e., the <see cref="DatabaseIDKey"/> value, defined in the
        /// specified connection string, if any.
        /// </summary>
        /// <param name="connectionString">Connection string to check.</param>
        /// <returns>Defined device database ID; otherwise, zero when key is not defined.</returns>
        public static int GetDatabaseID(string connectionString)
        {
            Dictionary<string, string> settings = SafeParseKeyValuePairs(connectionString);

            if (settings is not null && settings.TryGetValue(DatabaseIDKey, out string value) && int.TryParse(value, out int databaseID))
                return databaseID;

            return 0;
        }

        /// <summary>
        /// Builds a connection string for a detached child device that links the child to its parent
        /// concentrator device.
        /// </summary>
        /// <param name="parentID">Database ID of the parent concentrator device.</param>
        /// <param name="stationName">Station name, i.e., <c>Device.Name</c>, of the child device.</param>
        /// <returns>Connection string for a detached child device.</returns>
        public static string BuildChildConnectionString(int parentID, string stationName)
        {
            Dictionary<string, string> settings = new()
            {
                [ParentIDKey] = parentID.ToString()
            };

            if (!string.IsNullOrWhiteSpace(stationName))
                settings[StationNameKey] = stationName.Trim();

            return settings.JoinKeyValuePairs();
        }

        /// <summary>
        /// Removes any detached child linkage keys, i.e., <see cref="ParentIDKey"/> and
        /// <see cref="StationNameKey"/>, from the specified connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to update.</param>
        /// <returns>Updated connection string.</returns>
        public static string ClearChildLink(string connectionString)
        {
            Dictionary<string, string> settings = SafeParseKeyValuePairs(connectionString);

            if (settings is null)
                return connectionString;

            bool removedParentID = settings.Remove(ParentIDKey);
            bool removedStationName = settings.Remove(StationNameKey);

            if (!removedParentID && !removedStationName)
                return connectionString;

            return settings.JoinKeyValuePairs();
        }

        /// <summary>
        /// Adds or removes the detached children flag, i.e., <see cref="DetachedChildrenKey"/> along with
        /// its companion <see cref="DatabaseIDKey"/> stamp, on the specified parent concentrator device
        /// connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to update.</param>
        /// <param name="enabled">Flag that determines if parent has detached child devices.</param>
        /// <param name="databaseID">Database ID of the parent concentrator device itself.</param>
        /// <returns>Updated connection string.</returns>
        public static string SetDetachedChildren(string connectionString, bool enabled, int databaseID)
        {
            Dictionary<string, string> settings = SafeParseKeyValuePairs(connectionString) ?? new Dictionary<string, string>();

            if (enabled)
            {
                settings[DetachedChildrenKey] = "true";
                settings[DatabaseIDKey] = databaseID.ToString();
            }
            else
            {
                settings.Remove(DetachedChildrenKey);
                settings.Remove(DatabaseIDKey);
            }

            return settings.JoinKeyValuePairs();
        }

        // Attempts to parse connection string key / value pairs, returning null when parse fails
        private static Dictionary<string, string> SafeParseKeyValuePairs(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            try
            {
                return connectionString.ParseKeyValuePairs();
            }
            catch
            {
                return null;
            }
        }
    }
}
