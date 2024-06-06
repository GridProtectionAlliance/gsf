//******************************************************************************************************
//  MetadataHelpers.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  09/11/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.TimeSeries.Data;
using DeviceRecord = GSF.TimeSeries.Model.Device;
using MeasurementRecord = GSF.TimeSeries.Model.Measurement;
using SignalTypeRecord = GSF.TimeSeries.Model.SignalType;
using HistorianRecord = GSF.TimeSeries.Model.Historian;
using SignalType = GSF.Units.EE.SignalType;

namespace GSF.TimeSeries.Adapters;

/// <summary>
/// Represents functionality to manage device and measurement metadata for adapters.
/// </summary>
public static class MetadataHelpers
{
    /// <summary>
    /// Defines the default measurement table name.
    /// </summary>
    public const string DefaultMeasurementTable = "ActiveMeasurements";

    /// <summary>
    /// Defines the default value for the configuration reload wait timeout.
    /// </summary>
    public const int DefaultConfigurationReloadWaitTimeout = 5000;

    /// <summary>
    /// Defines the default value for the configuration reload wait attempts.
    /// </summary>
    public const int DefaultConfigurationReloadWaitAttempts = 4;

    /// <summary>
    /// Gets ID of parent device, creating or updating record if needed, for the current <see cref="IAdapter"/> instance.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="parentDeviceAcronymTemplate">Parent device acronym template. Use '{0}' to reference <see cref="IAdapter.Name"/>.</param>
    /// <returns>ID of parent device; or, <c>null</c> on failure.</returns>
    public static int? GetParentDevice(this IAdapter instance, string parentDeviceAcronymTemplate = "{0}!RESULTS")
    {
        // Open database connection as defined in configuration file "systemSettings" category
        using AdoDataConnection connection = new("systemSettings");

        TableOperations<DeviceRecord> deviceTable = new(connection);
        string deviceAcronym = string.Format(parentDeviceAcronymTemplate, instance.Name);

        DeviceRecord device = deviceTable.QueryRecordWhere("Acronym = {0}", deviceAcronym) ?? deviceTable.NewRecord();
        int protocolID = connection.ExecuteScalar<int?>("SELECT ID FROM Protocol WHERE Acronym = 'VirtualInput'") ?? 15;

        device.Acronym = deviceAcronym;
        device.Name = deviceAcronym;
        device.ProtocolID = protocolID;
        device.Enabled = true;

        deviceTable.AddNewOrUpdateRecord(device);

        return deviceTable.QueryRecordWhere("Acronym = {0}", deviceAcronym)?.ID;
    }

    /// <summary>
    /// Looks up point tag name from provided <paramref name="signalID"/>.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="signalID"><see cref="Guid"/> signal ID to lookup.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns>Point tag name, if found; otherwise, string representation of provided signal ID.</returns>
    public static string LookupPointTag(this IAdapter instance, Guid signalID, string measurementTable = DefaultMeasurementTable)
    {
        DataRow record = instance.DataSource.LookupMetadata(signalID, measurementTable);
        string pointTag = null;

        if (record is not null)
            pointTag = record["PointTag"].ToString();

        if (string.IsNullOrWhiteSpace(pointTag))
            pointTag = signalID.ToString();

        return pointTag.ToUpper();
    }

    /// <summary>
    /// Looks up signal reference from provided <paramref name="signalID"/>.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="signalID"><see cref="Guid"/> signal ID to lookup.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns>Signal reference name, if found; otherwise, string representation of provided signal ID.</returns>
    public static string LookupSignalReference(this IAdapter instance, Guid signalID, string measurementTable = DefaultMeasurementTable)
    {
        DataRow record = instance.DataSource.LookupMetadata(signalID, measurementTable);
        string signalReference = null;

        if (record is not null)
            signalReference = record["SignalReference"].ToString();

        if (string.IsNullOrWhiteSpace(signalReference))
            signalReference = signalID.ToString();

        return signalReference.ToUpper();
    }

    /// <summary>
    /// Looks up or creates measurement key based on provided <paramref name="signalID"/>.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="signalID"><see cref="Guid"/> signal ID to lookup.</param>
    /// <param name="id">Measurement ID to use for creating new measurement key.</param>
    /// <param name="source">Source name used for meta-data lookup.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns>Found or newly created measurement key. Failure to create key results in <see cref="MeasurementKey.Undefined"/>.</returns>
    /// <remarks>
    /// This is a metadata first lookup operation.
    /// </remarks>
    public static MeasurementKey LookupMeasurementKey(this IAdapter instance, Guid signalID, ulong id, string source = "PPA", string measurementTable = DefaultMeasurementTable)
    {
        DataRow record = instance.DataSource.LookupMetadata(signalID, measurementTable);
        string key = record?["ID"].ToString();

        return string.IsNullOrWhiteSpace(key) ? id == ulong.MaxValue ? MeasurementKey.Undefined : 
                MeasurementKey.CreateOrUpdate(signalID, source, id) : 
                MeasurementKey.LookUpOrCreate(key);
    }

    /// <summary>
    /// Looks up associated device acronym and ID from provided <paramref name="signalID"/>.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="signalID"><see cref="Guid"/> signal ID to lookup.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns>Device acronym and ID tuple, if found; otherwise, string representation of associated point tag and ID of zero.</returns>
    public static (string acronym, int ID) LookupDevice(this IAdapter instance, Guid signalID, string measurementTable = DefaultMeasurementTable)
    {
        DataRow record = instance.DataSource.LookupMetadata(signalID, measurementTable);
        string deviceAcronym = null;
        int deviceID = 0;

        if (record is not null)
        {
            deviceAcronym = record["Device"].ToString();

            // DeviceID from measurement table is a runtime ID, so we look up the actual device ID
            using AdoDataConnection connection = new("systemSettings");
            TableOperations<DeviceRecord> deviceTable = new(connection);
            DeviceRecord device = deviceTable.QueryRecordWhere("Acronym = {0}", deviceAcronym);
            deviceID = device?.ID ?? 0;
        }

        if (string.IsNullOrWhiteSpace(deviceAcronym))
            deviceAcronym = instance.LookupPointTag(signalID, measurementTable);

        return (deviceAcronym.ToUpper().Trim(), deviceID);
    }

    /// <summary>
    /// Determines if <paramref name="signalID"/> exists in local configuration.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="signalID">Signal ID to find.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns><c>true</c>, if <paramref name="signalID"/> is found; otherwise, <c>false</c>.</returns>
    public static bool SignalIDExists(this IAdapter instance, Guid signalID, string measurementTable = DefaultMeasurementTable)
    {
        return instance.DataSource.LookupMetadata(signalID, measurementTable) is not null;
    }

    /// <summary>
    /// Determines if <paramref name="pointTag"/> exists in local configuration.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="pointTag">Point tag to find./</param>
    /// <param name="signalID">Signal ID of measurement with specified point tag.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns><c>true</c>, if <paramref name="pointTag"/> is found; otherwise, <c>false</c>.</returns>
    public static bool PointTagExists(this IAdapter instance, string pointTag, out Guid signalID, string measurementTable = DefaultMeasurementTable)
    {
        DataRow[] rows = instance.DataSource.Tables[measurementTable].Select($"PointTag = '{pointTag}'");

        if (rows.Length != 0 && !string.IsNullOrWhiteSpace(rows[0]["PointTag"].ToString()))
        {
            signalID = Guid.Parse(rows[0]["SignalID"].ToString());
            return true;
        }

        signalID = Guid.Empty;
        return false;
    }

    /// <summary>
    /// Determines if <paramref name="signalReference"/> exists in local configuration.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="signalReference">Point tag to find./</param>
    /// <param name="signalID">Signal ID of measurement with specified point tag.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns><c>true</c>, if <paramref name="signalReference"/> is found; otherwise, <c>false</c>.</returns>
    public static bool SignalReferenceExists(this IAdapter instance, string signalReference, out Guid signalID, string measurementTable = DefaultMeasurementTable)
    {
        DataRow[] rows = instance.DataSource.Tables[measurementTable].Select($"SignalReference = '{signalReference}'");
        
        if (rows.Length != 0 && !string.IsNullOrWhiteSpace(rows[0]["SignalReference"].ToString()))
        {
            signalID = Guid.Parse(rows[0]["SignalID"].ToString());
            return true;
        }

        signalID = Guid.Empty;
        return false;
    }

    /// <summary>
    /// Gets measurement record, creating it if needed.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="currentDeviceID">Device ID associated with current adapter, or zero if none.</param>
    /// <param name="pointTag">Point tag of measurement.</param>
    /// <param name="alternateTag">Alternate tag of measurement.</param>
    /// <param name="signalReference">Signal reference of measurement.</param>
    /// <param name="description">Description of measurement.</param>
    /// <param name="signalType">Signal type of measurement.</param>
    /// <param name="targetHistorianAcronym">Acronym of target historian for measurement.</param>
    /// <param name="sendChangedNotification">Flag that determines if host system should be notified of configuration changes.</param>
    /// <returns>Measurement record.</returns>
    public static MeasurementRecord GetMeasurementRecord(this IAdapter instance, int? currentDeviceID, string pointTag, string alternateTag, string signalReference, string description, SignalType signalType = SignalType.CALC, string targetHistorianAcronym = "PPA", bool sendChangedNotification = false)
    {
        // Open database connection as defined in configuration file "systemSettings" category
        using AdoDataConnection connection = new("systemSettings");

        TableOperations<DeviceRecord> deviceTable = new(connection);
        TableOperations<MeasurementRecord> measurementTable = new(connection);
        TableOperations<HistorianRecord> historianTable = new(connection);
        TableOperations<SignalTypeRecord> signalTypeTable = new(connection);

        // Lookup target device ID
        int? deviceID = currentDeviceID > 0 ? currentDeviceID : deviceTable.QueryRecordWhere("Acronym = {0}", instance.Name)?.ID;

        // Lookup target historian ID
        int? historianID = historianTable.QueryRecordWhere("Acronym = {0}", targetHistorianAcronym)?.ID;

        // Lookup signal type ID
        int signalTypeID = signalTypeTable.QueryRecordWhere("Acronym = {0}", signalType.ToString())?.ID ?? 1;

        // Lookup measurement record by point tag, creating a new record if one does not exist
        MeasurementRecord measurement = measurementTable.QueryRecordWhere("SignalReference = {0}", signalReference) ?? measurementTable.NewRecord();

        // Update record fields
        measurement.DeviceID = deviceID;
        measurement.HistorianID = historianID;
        measurement.PointTag = pointTag;
        measurement.AlternateTag = alternateTag;
        measurement.SignalReference = signalReference;
        measurement.SignalTypeID = signalTypeID;
        measurement.Description = description;

        // Save record updates
        measurementTable.AddNewOrUpdateRecord(measurement);

        // Re-query new records to get any database assigned information, e.g., unique Guid-based signal ID
        if (measurement.PointID == 0)
            measurement = measurementTable.QueryRecordWhere("SignalReference = {0}", signalReference);

        // Notify host system of configuration changes
        if (sendChangedNotification)
            instance.OnConfigurationChanged();

        return measurement;
    }

    /// <summary>
    /// Notifies host system of configuration changes.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    public static void OnConfigurationChanged(this IAdapter instance)
    {
        try
        {
            Type baseType = instance switch
            {
                AdapterBase => typeof(AdapterBase),
                ActionAdapterBase => typeof(ActionAdapterBase),
                _ => null
            };

            if (baseType is null)
                throw new NotSupportedException($"Call support for '{nameof(OnConfigurationChanged)}' not implemented for type '{instance.GetType().Name}'.");

            MethodInfo onConfigurationChanged = baseType.GetMethod("OnConfigurationChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            onConfigurationChanged?.Invoke(instance, null);
        }
        catch (Exception ex)
        {
            Logger.SwallowException(ex);
        }
    }

    /// <summary>
    /// Waits for <paramref name="signalIDs"/> to be loaded in system configuration.
    /// </summary>
    /// <param name="instance">Target <see cref="IAdapter"/> instance.</param>
    /// <param name="configurationReloadedWaitHandle">Wait handle to signal when configuration has been reloaded.</param>
    /// <param name="signalIDs">Signal IDs to wait for.</param>
    /// <param name="configurationReloadWaitTimeout">Configuration reload wait timeout.</param>
    /// <param name="configurationReloadWaitAttempts">Configuration reload wait attempts.</param>
    /// <param name="measurementTable">Measurement table name used for meta-data lookup.</param>
    /// <returns><c>true</c> if <paramref name="signalIDs"/> are loaded; otherwise, <c>false</c>.</returns>
    public static bool WaitForSignalsToLoad(this IAdapter instance, ManualResetEventSlim configurationReloadedWaitHandle, IReadOnlyList<Guid> signalIDs, int configurationReloadWaitTimeout = DefaultConfigurationReloadWaitTimeout, int configurationReloadWaitAttempts = DefaultConfigurationReloadWaitAttempts, string measurementTable = DefaultMeasurementTable)
    {
        int attempts = 0;
        bool signalsLoaded = false;

        while (attempts++ < configurationReloadWaitAttempts)
        {
            configurationReloadedWaitHandle.Reset();

            if (signalIDs.All(signalID => instance.SignalIDExists(signalID, measurementTable)))
            {
                signalsLoaded = true;
                break;
            }

            configurationReloadedWaitHandle.Wait(configurationReloadWaitTimeout);
        }

        return signalsLoaded;
    }

    /// <summary>
    /// Gets string representation of elapsed wait time for <paramref name="configurationReloadWaitTimeout"/> and <paramref name="configurationReloadWaitAttempts"/>.
    /// </summary>
    /// <param name="configurationReloadWaitTimeout">Configuration reload wait timeout.</param>
    /// <param name="configurationReloadWaitAttempts">Configuration reload wait attempts.</param>
    /// <returns>String representation of elapsed wait time.</returns>
    public static string ElapsedWaitTimeString(int configurationReloadWaitTimeout = DefaultConfigurationReloadWaitTimeout, int configurationReloadWaitAttempts = DefaultConfigurationReloadWaitAttempts)
    {
        return Ticks.FromMilliseconds(configurationReloadWaitTimeout * configurationReloadWaitAttempts).ToElapsedTimeString(3);
    }
}