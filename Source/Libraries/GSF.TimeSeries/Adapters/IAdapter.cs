//******************************************************************************************************
//  IAdapter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the abstract interface for any adapter.
    /// </summary>
    public interface IAdapter : ISupportLifecycle, IProvideStatus
    {
        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Implementations of this interface are expected to capture any exceptions that might be thrown by
        /// user code in any processing to prevent third-party code from causing an unhandled exception
        /// in the host.  Errors are reported via this event so host administrators will be aware of the exception.
        /// Any needed connection cycle to data adapter should be restarted when an exception is encountered.
        /// </para>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Event is raised when <see cref="InputMeasurementKeys"/> are updated.
        /// </summary>
        event EventHandler InputMeasurementKeysUpdated;

        /// <summary>
        /// Event is raised when <see cref="OutputMeasurements"/> are updated.
        /// </summary>
        event EventHandler OutputMeasurementsUpdated;

        /// <summary>
        /// Event is raised when adapter is aware of a configuration change.
        /// </summary>
        event EventHandler ConfigurationChanged;

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to <see cref="IAdapter"/>.
        /// </summary>
        DataSet DataSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets key/value pair connection information specific to <see cref="IAdapter"/>.
        /// </summary>
        string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets connection info for adapter, if any.
        /// </summary>
        /// <remarks>
        /// For example, this could return IP or host name of source connection.
        /// </remarks>
        string ConnectionInfo
        {
            get;
        }

        /// <summary>
        /// Gets settings <see cref="Dictionary{TKey,TValue}"/> parsed when <see cref="ConnectionString"/> was assigned.
        /// </summary>
        Dictionary<string, string> Settings
        {
            get;
        }

        /// <summary>
        /// Gets or sets name of this <see cref="IAdapter"/>.
        /// </summary>
        new string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the numeric ID associated with this <see cref="IAdapter"/>.
        /// </summary>
        uint ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag indicating if the adapter has been initialized successfully.
        /// </summary>
        /// <remarks>
        /// Implementers only need to track this value.
        /// </remarks>
        bool Initialized
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets maximum time system will wait during <see cref="Start"/> for initialization.
        /// </summary>
        /// <remarks>
        /// Implementers should use value <see cref="Timeout.Infinite"/> to wait indefinitely.
        /// </remarks>
        int InitializationTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag indicating if adapter should automatically start or connect on demand.
        /// </summary>
        bool AutoStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets primary keys of input measurements the adapter expects.
        /// </summary>
        MeasurementKey[] InputMeasurementKeys
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets output measurements that the adapter will produce, if any.
        /// </summary>
        IMeasurement[] OutputMeasurements
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the total number of measurements processed thus far by the <see cref="IAdapter"/>.
        /// </summary>
        long ProcessedMeasurements
        {
            get;
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        bool SupportsTemporalProcessing
        {
            get;
        }

        /// <summary>
        /// Gets the start time temporal processing constraint defined by call to <see cref="SetTemporalConstraint"/>.
        /// </summary>
        /// <remarks>
        /// This value will be <see cref="DateTime.MinValue"/> when start time constraint is not set - meaning the adapter
        /// is processing data in real-time.
        /// </remarks>
        DateTime StartTimeConstraint
        {
            get;
        }

        /// <summary>
        /// Gets the stop time temporal processing constraint defined by call to <see cref="SetTemporalConstraint"/>.
        /// </summary>
        /// <remarks>
        /// This value will be <see cref="DateTime.MaxValue"/> when stop time constraint is not set - meaning the adapter
        /// is processing data in real-time.
        /// </remarks>
        DateTime StopTimeConstraint
        {
            get;
        }

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        int ProcessingInterval
        {
            get;
            set;
        }

        /// <summary>
        ///  Starts the adapter, if it is not already running.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the adapter.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets a short one-line adapter status.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current adapter status.</returns>
        string GetShortStatus(int maxLength);

        /// <summary>
        /// Defines a temporal processing constraint for the adapter.
        /// </summary>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint.</param>
        /// <remarks>
        /// <para>
        /// This method defines a temporal processing constraint for an adapter, i.e., the start and stop time over which an
        /// adapter will process data. Actual implementation of the constraint will be adapter specific. Implementations
        /// should be able to dynamically handle multiple calls to this function with new constraints. Passing in <c>null</c>
        /// for the <paramref name="startTime"/> and <paramref name="stopTime"/> should cancel the temporal constraint and
        /// return the adapter to standard / real-time operation.
        /// </para>
        /// <para>
        /// The <paramref name="startTime"/> and <paramref name="stopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        void SetTemporalConstraint(string startTime, string stopTime, string constraintParameters);
    }

    /// <summary>
    /// Defines static extension functions for <see cref="IAdapter"/> implementations.
    /// </summary>
    public static class IAdapterExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if <see cref="IAdapter"/> has a temporal constraint defined, i.e., either
        /// <see cref="IAdapter.StartTimeConstraint"/> or <see cref="IAdapter.StopTimeConstraint"/> is not
        /// set to its default value.
        /// </summary>
        /// <param name="adapter"><see cref="IAdapter"/> instance to test.</param>
        /// <returns><c>true</c> if <see cref="IAdapter"/> has a temporal constraint defined.</returns>
        public static bool TemporalConstraintIsDefined(this IAdapter adapter)
        {
            return (adapter.StartTimeConstraint != DateTime.MinValue || adapter.StopTimeConstraint != DateTime.MaxValue);
        }

        /// <summary>
        /// Returns the <see cref="MeasurementKey"/> values of the <see cref="IAdapter"/> input measurements.
        /// </summary>
        /// <param name="adapter"><see cref="IAdapter"/> instance input measurements to convert.</param>
        /// <returns><see cref="MeasurementKey"/> values of the <see cref="IAdapter"/> input measurements.</returns>
        public static MeasurementKey[] InputMeasurementKeys(this IAdapter adapter)
        {
            return adapter.InputMeasurementKeys ?? new MeasurementKey[0];
        }

        /// <summary>
        /// Returns the <see cref="MeasurementKey"/> values of the <see cref="IAdapter"/> output measurements.
        /// </summary>
        /// <param name="adapter"><see cref="IAdapter"/> instance output measurements to convert.</param>
        /// <returns><see cref="MeasurementKey"/> values of the <see cref="IAdapter"/> output measurements.</returns>
        public static MeasurementKey[] OutputMeasurementKeys(this IAdapter adapter)
        {
            return adapter.OutputMeasurements.MeasurementKeys().ToArray();
        }

        /// <summary>
        /// Gets a distinct list of input measurement keys for all of the provided adapters.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="IAdapter"/>.</typeparam>
        /// <param name="adapters">Source <see cref="IAdapter"/> enumeration.</param>
        /// <returns>Distinct list of input measurement keys for all of the provided adapters.</returns>
        public static MeasurementKey[] InputMeasurementKeys<T>(this IEnumerable<T> adapters) where T : IAdapter
        {
            List<MeasurementKey> inputMeasurementKeys = new List<MeasurementKey>();

            foreach (T adapter in adapters)
            {
                MeasurementKey[] adapterInputMeasurementKeys = adapter.InputMeasurementKeys;

                if (adapterInputMeasurementKeys != null && adapterInputMeasurementKeys.Length > 0)
                    inputMeasurementKeys.AddRange(adapterInputMeasurementKeys);
            }

            return inputMeasurementKeys.Distinct().ToArray();
        }

        /// <summary>
        /// Gets a distinct list of output measurement keys for all of the provided adapters.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="IAdapter"/>.</typeparam>
        /// <param name="adapters">Source <see cref="IAdapter"/> enumeration.</param>
        /// <returns>Distinct list of output measurement keys for all of the provided adapters.</returns>
        public static MeasurementKey[] OutputMeasurementKeys<T>(this IEnumerable<T> adapters) where T : IAdapter
        {
            List<MeasurementKey> outputMeasurementKeys = new List<MeasurementKey>();

            foreach (T adapter in adapters)
            {
                IEnumerable<MeasurementKey> adapterOutputMeasurementKeys = adapter.OutputMeasurementKeys();

                if ((object)adapterOutputMeasurementKeys != null && adapterOutputMeasurementKeys.Any())
                    outputMeasurementKeys.AddRange(adapterOutputMeasurementKeys);
            }

            return outputMeasurementKeys.Distinct().ToArray();
        }
    }
}