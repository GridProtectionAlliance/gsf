//******************************************************************************************************
//  IndependentAdapterManagerHandlers.cs - Gbtc
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
//  02/16/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using GSF.Data;
using GSF.Diagnostics;
using ConnectionStringParser = GSF.Configuration.ConnectionStringParser<GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute>;

namespace GSF.TimeSeries.Adapters
{
    // Common implementation extension handlers for independent adapter collection managers.
    internal static class IndependentAdapterManagerHandlers
    {
        /// <summary>
        /// Handles construction steps for a new <see cref="IIndependentAdapterManager"/> instance.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleConstruct(this IIndependentAdapterManager instance)
        {
            bool needsRouting = true;

            instance.OriginalDataMember = instance.DataMember;
            instance.DataMember = "[internal]";
            instance.Name = $"{instance.GetType().Name} Collection";

            // ReSharper disable SuspiciousTypeConversion.Global
            switch (instance)
            {
                case ActionAdapterCollection actionAdapterCollection:
                    instance.RoutingTables = new RoutingTables { ActionAdapters = actionAdapterCollection };
                    break;
                case OutputAdapterCollection outputAdapterCollection:
                    instance.RoutingTables = new RoutingTables { OutputAdapters = outputAdapterCollection };
                    break;
                case InputAdapterCollection _:
                    needsRouting = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(instance));
            }
            // ReSharper restore SuspiciousTypeConversion.Global

            if (needsRouting)
            {
                instance.RoutingTables.StatusMessage += RoutingTables_StatusMessage;
                instance.RoutingTables.ProcessException += RoutingTables_ProcessException;

                // Make sure routes are recalculated any time measurements are updated
                instance.InputMeasurementKeysUpdated += Instance_InputMeasurementKeysUpdated;
            }

            instance.ConfigurationReloadedWaitHandle = new ManualResetEventSlim();
        }

        /// <summary>
        /// Disposes resources used by the <see cref="IIndependentAdapterManager"/> instance.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleDispose(this IIndependentAdapterManager instance)
        {
            if (instance.RoutingTables is not null)
            {
                instance.InputMeasurementKeysUpdated -= Instance_InputMeasurementKeysUpdated;

                instance.RoutingTables.StatusMessage -= RoutingTables_StatusMessage;
                instance.RoutingTables.ProcessException -= RoutingTables_ProcessException;
                instance.RoutingTables.Dispose();
            }

            if (instance.ConfigurationReloadedWaitHandle is not null)
            {
                instance.ConfigurationReloadedWaitHandle.Set();
                instance.ConfigurationReloadedWaitHandle.Dispose();
            }
        }

        /// <summary>
        /// Initializes the <see cref="IndependentAdapterManagerExtensions" />.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleInitialize(this IIndependentAdapterManager instance)
        {
            // We don't call base class initialize since it tries to auto-load adapters from the defined
            // data member - instead, the multi-adapter class implementation manages its own adapters
            instance.Initialized = false;

            instance.ParseConnectionString();

            if (instance.ConfigurationReloadWaitTimeout < 0)
                instance.ConfigurationReloadWaitTimeout = 0;
            
            if (instance.InputMeasurementIndexUsedForName < 0 || instance.InputMeasurementIndexUsedForName > instance.PerAdapterInputCount - 1)
                instance.InputMeasurementIndexUsedForName = 0;

            if (instance.SignalTypes is not null && instance.SignalTypes.Length < instance.PerAdapterOutputNames.Count)
            {
                instance.OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Defined {nameof(IIndependentAdapterManager.SignalTypes)} array length for adapter \"{instance.Name}\" does not match {nameof(IIndependentAdapterManager.PerAdapterOutputNames)} array length."));
                return;
            }

            instance.Initialized = true;
        }

        /// <summary>
        /// Sets <see cref="DataSet"/> based data source used to load each <see cref="IAdapter"/>. Updates to this
        /// property will cascade to all adapters in this <see cref="IndependentAdapterManagerExtensions"/> instance.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleUpdateDataSource(this IIndependentAdapterManager instance)
        {
            // Notify any waiting threads that configuration has been reloaded
            instance.ConfigurationReloadedWaitHandle.Set();

            // Update routes for configuration reload
            instance.RoutingTables?.CalculateRoutingTables(null);

            if (instance.AutoReparseConnectionString)
            {
                // Lookup adapter configuration record
                DataRow[] records = instance.DataSource.Tables[instance.OriginalDataMember].Select($"AdapterName = '{instance.Name}'");

                if (records.Length > 0)
                {
                    // Parsing connection string after any updates will parse input measurement keys causing
                    // a request to update measurements routed to adapter. Derived implementations can then
                    // use DataSourceChanged notification to add or remove child adapters as needed.
                    instance.ConnectionString = records[0]["ConnectionString"].ToNonNullString();
                    instance.ParseConnectionString();
                }
                else
                {
                    instance.OnStatusMessage(MessageLevel.Warning, $"Failed to find adapter \"{instance.Name}\" in \"{instance.OriginalDataMember}\" configuration table. Cannot reload connection string parameters.");
                }
            }

            instance.ConfigurationReloaded();
        }

        /// <summary>
        /// Returns the detailed status for the <see cref="IIndependentAdapterManager"/> instance.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static string HandleStatus(this IIndependentAdapterManager instance)
        {
            const int MaxMeasurementsToShow = 10;

            StringBuilder status = new();

            status.AppendLine($"        Point Tag Template: {instance.PointTagTemplate}");
            status.AppendLine($"    Alternate Tag Template: {instance.AlternateTagTemplate}");
            status.AppendLine($" Signal Reference Template: {instance.SignalReferenceTemplate}");
            status.AppendLine($"      Description Template: {instance.DescriptionTemplate}");
            status.AppendLine($"   Device Acronym Template: {instance.ParentDeviceAcronymTemplate}");
            status.AppendLine($"        Output Signal Type: {instance.SignalType}");
            status.AppendLine($"  Target Historian Acronym: {instance.TargetHistorianAcronym}");
            status.AppendLine($"  Source Measurement Table: {instance.SourceMeasurementTable}");
            status.AppendLine($"        Inputs per Adapter: {instance.PerAdapterInputCount:N0}");
            status.AppendLine($" Input Index Used for Name: {instance.InputMeasurementIndexUsedForName:N0}");
            status.AppendLine($"  Output Names per Adapter: {instance.PerAdapterOutputNames.Count:N0}");

            foreach (string outputName in instance.PerAdapterOutputNames)
                status.AppendLine($"                  \"{outputName.TruncateRight(40)}\"");

            status.AppendLine($"Re-parse Connection String: {instance.AutoReparseConnectionString}");
            status.AppendLine($"      Original Data Member: {instance.OriginalDataMember}");
            status.AppendLine($"     Config Reload Timeout: {instance.ConfigurationReloadWaitTimeout:N0} ms");
            status.AppendLine($"    Config Reload Attempts: {instance.ConfigurationReloadWaitAttempts:N0}");
            status.AppendLine($"Database Connection String: {(string.IsNullOrWhiteSpace(instance.DatabaseConnectionString) ? "Using <systemSettings>" : instance.DatabaseConnectionString.TruncateRight(40))}");
            
            if (!string.IsNullOrWhiteSpace(instance.DatabaseConnectionString))
                status.AppendLine($"  Custom Database Provider: {instance.DatabaseProviderString ?? ""}");

            if (instance.OutputMeasurements is not null && instance.OutputMeasurements.Length > instance.OutputMeasurements.Count(m => m.Key == MeasurementKey.Undefined))
            {
                status.AppendLine($"       Output measurements: {instance.OutputMeasurements.Length:N0} defined measurements");
                status.AppendLine();

                for (int i = 0; i < Math.Min(instance.OutputMeasurements.Length, MaxMeasurementsToShow); i++)
                {
                    status.Append(instance.OutputMeasurements[i].ToString().TruncateRight(40).PadLeft(40));
                    status.Append(" ");
                    status.AppendLine(instance.OutputMeasurements[i].ID.ToString());
                }

                if (instance.OutputMeasurements.Length > MaxMeasurementsToShow)
                    status.AppendLine("...".PadLeft(26));

                status.AppendLine();
            }

            if (instance.InputMeasurementKeys is not null && instance.InputMeasurementKeys.Length > instance.InputMeasurementKeys.Count(k => k == MeasurementKey.Undefined))
            {
                status.AppendLine($"        Input measurements: {instance.InputMeasurementKeys.Length:N0} defined measurements");
                status.AppendLine();

                for (int i = 0; i < Math.Min(instance.InputMeasurementKeys.Length, MaxMeasurementsToShow); i++)
                    status.AppendLine(instance.InputMeasurementKeys[i].ToString().TruncateRight(25).CenterText(50));

                if (instance.InputMeasurementKeys.Length > MaxMeasurementsToShow)
                    status.AppendLine("...".CenterText(50));

                status.AppendLine();
            }

            return status.ToString();
        }

        /// <summary>
        /// Parses connection string. Derived classes should override for custom connection string parsing.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleParseConnectionString(this IIndependentAdapterManager instance)
        {
            // Parse all properties marked with ConnectionStringParameterAttribute from provided ConnectionString value
            ConnectionStringParser parser = new();
            parser.ParseConnectionString(instance.ConnectionString, instance);

            // Parse input measurement keys like class was a typical adapter
            if (instance.Settings.TryGetValue(nameof(instance.InputMeasurementKeys), out string setting))
                instance.InputMeasurementKeys = AdapterBase.ParseInputMeasurementKeys(instance.DataSource, true, setting, instance.SourceMeasurementTable);

            // Parse output measurement keys like class was a typical adapter
            if (instance.Settings.TryGetValue(nameof(instance.OutputMeasurements), out setting))
                instance.OutputMeasurements = AdapterBase.ParseOutputMeasurements(instance.DataSource, true, setting, instance.SourceMeasurementTable);
        }
        
        /// <summary>
        /// Validates that an even number of inputs are provided for specified <see cref="IIndependentAdapterManager.PerAdapterInputCount"/>.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleValidateEvenInputCount(this IIndependentAdapterManager instance)
        {
            int remainder = instance.InputMeasurementKeys.Length % instance.PerAdapterInputCount;

            if (remainder == 0)
                return;

            int adjustedCount = instance.InputMeasurementKeys.Length - remainder;
            instance.OnStatusMessage(MessageLevel.Warning, $"Uneven number of inputs provided, adjusting total number of inputs to {adjustedCount:N0}. Expected {instance.PerAdapterInputCount:N0} per adapter, received {instance.InputMeasurementKeys.Length:N0} total measurements, leaving {instance.PerAdapterInputCount - remainder:N0} needed.");
            instance.InputMeasurementKeys = instance.InputMeasurementKeys.Take(adjustedCount).ToArray();
        }

        /// <summary>
        /// Recalculates routing tables.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleRecalculateRoutingTables(this IIndependentAdapterManager instance) => instance.OnInputMeasurementKeysUpdated(); // Requests route recalculation by IonSession

        /// <summary>
        /// Queues a collection of measurements for processing to each <see cref="IAdapter"/> connected to this <see cref="IndependentAdapterManagerExtensions"/>.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        /// <param name="measurements">Measurements to queue for processing.</param>
        public static void HandleQueueMeasurementsForProcessing(this IIndependentAdapterManager instance, IEnumerable<IMeasurement> measurements)
        {
            if (instance.RoutingTables is null)
                return;

            // Pass measurements coming into parent collection adapter to routing tables for individual child adapter distribution
            IList<IMeasurement> measurementList = measurements as IList<IMeasurement> ?? measurements.ToList();
            instance.RoutingTables.InjectMeasurements(instance, new EventArgs<ICollection<IMeasurement>>(measurementList));
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="IndependentAdapterManagerExtensions"/>.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of the <see cref="IndependentAdapterManagerExtensions"/>.</returns>
        public static string HandleGetShortStatus(this IIndependentAdapterManager instance, int maxLength) =>
            instance.Enabled ? 
                $"Processing enabled for {instance.Count:N0} adapters.".CenterText(maxLength) : 
                "Processing not enabled".CenterText(maxLength);

        /// <summary>
        /// Enumerates child adapters.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        public static void HandleEnumerateAdapters(this IIndependentAdapterManager instance)
        {
            StringBuilder enumeratedAdapters = new();
            IAdapter[] adapters = instance.ToArray();

            enumeratedAdapters.AppendLine($"{instance.Name} Indexed Adapter Enumeration - {adapters.Length:N0} Total:\r\n");

            for (int i = 0; i < adapters.Length; i++)
                enumeratedAdapters.AppendLine($"{i,5:N0}: {adapters[i].Name}".TrimWithEllipsisMiddle(79));

            instance.OnStatusMessage(MessageLevel.Info, enumeratedAdapters.ToString());
        }

        /// <summary>
        /// Gets subscriber information for specified client connection.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        /// <param name="adapterIndex">Enumerated index for child adapter.</param>
        public static string HandleGetAdapterStatus(this IIndependentAdapterManager instance, int adapterIndex) => instance[adapterIndex].Status;

        /// <summary>
        /// Gets configured database connection.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        /// <returns>New ADO data connection based on configured settings.</returns>
        public static AdoDataConnection HandleGetConfiguredConnection(this IIndependentAdapterManager instance) => string.IsNullOrWhiteSpace(instance.DatabaseConnectionString) ?
            new AdoDataConnection("systemSettings") :
            new AdoDataConnection(instance.DatabaseConnectionString, instance.DatabaseProviderString);

        /// <summary>
        /// Determines whether the data in the data source has actually changed when receiving a new data source.
        /// </summary>
        /// <param name="instance">Target <see cref="IIndependentAdapterManager"/> instance.</param>
        /// <param name="newDataSource">New data source to check.</param>
        /// <returns><c>true</c> if data source has changed; otherwise, <c>false</c>.</returns>
        public static bool DataSourceChanged(this IIndependentAdapterManager instance, DataSet newDataSource)
        {
            try
            {
                return !DataSetEqualityComparer.Default.Equals(instance.DataSource, newDataSource);
            }
            catch
            {
                // Function is for optimization, reason for failure is irrelevant
                return true;
            }
        }

        // Make sure routes are recalculated any time measurements are updated
        private static void Instance_InputMeasurementKeysUpdated(object sender, EventArgs e)
        {
            if (sender is IIndependentAdapterManager instance)
                instance.RoutingTables?.CalculateRoutingTables(null);
        }

        // Make sure to expose any routing table messages
        private static void RoutingTables_StatusMessage(object sender, EventArgs<string> e)
        {
            if (sender is not RoutingTables routingTables)
                return;

            IIndependentAdapterManager instance =
                routingTables.ActionAdapters as IIndependentAdapterManager ??
                routingTables.OutputAdapters as IIndependentAdapterManager;

            instance?.OnStatusMessage(MessageLevel.Info, e.Argument);
        }

        // Make sure to expose any routing table exceptions
        private static void RoutingTables_ProcessException(object sender, EventArgs<Exception> e)
        {
            if (sender is not RoutingTables routingTables)
                return;

            IIndependentAdapterManager instance =
                routingTables.ActionAdapters as IIndependentAdapterManager ??
                routingTables.OutputAdapters as IIndependentAdapterManager;

            instance?.OnProcessException(MessageLevel.Warning, e.Argument);
        }
    }
}