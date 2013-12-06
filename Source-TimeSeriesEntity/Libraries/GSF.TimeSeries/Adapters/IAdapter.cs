//******************************************************************************************************
//  IAdapter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  11/01/2013 - Stephen C. Wills
//       Updated to process time-series entities.
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
        #region [ Members ]

        // Events

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
        /// Event is raised when <see cref="InputSignalIDs"/> are updated.
        /// </summary>
        event EventHandler InputSignalIDsUpdated;

        /// <summary>
        /// Event is raised when <see cref="OutputSignalIDs"/> are updated.
        /// </summary>
        event EventHandler OutputSignalIDsUpdated;

        /// <summary>
        /// Event is raised when adapter is aware of a configuration change.
        /// </summary>
        event EventHandler ConfigurationChanged;

        /// <summary>
        /// This event is raised if there are any time-series entities being discarded during processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the enumeration of <see cref="ITimeSeriesEntity"/> objects that are being discarded during processing.
        /// </remarks>
        event EventHandler<EventArgs<IEnumerable<ITimeSeriesEntity>>> EntitiesDiscarded;

        #endregion

        #region [ Properties ]

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
        /// Gets or sets the maximum amount of time the adapter is expected to take during initialization,
        /// after which the system will issue a warning that the adapter is taking too long to initialize.
        /// </summary>
        /// <remarks>
        /// Implementers should use value <see cref="Timeout.Infinite"/> to disable the warning.
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
        /// Gets or sets the collection of signal IDs the adapter wishes to receive as input.
        /// </summary>
        /// <remarks>
        /// It is expected that that this value will never return null.
        /// </remarks>
        ISet<Guid> InputSignalIDs
        {
            get;
        }

        /// <summary>
        /// Gets or sets the collection of signal IDs the adapter plans to create as output.
        /// </summary>
        /// <remarks>
        /// It is expected that that this value will never return null.
        /// </remarks>
        ISet<Guid> OutputSignalIDs
        {
            get;
        }

        /// <summary>
        /// Gets the total number of time-series entities processed thus far by the <see cref="IAdapter"/>.
        /// </summary>
        long ProcessedEntities
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

        #endregion

        #region [ Methods ]

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

        #endregion
    }

    /// <summary>
    /// Defines static extension functions for <see cref="IAdapter"/> implementations.
    /// </summary>
    public static class AdapterExtensions
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
        /// Attempts to parse an individual signal ID from the specified <paramref name="parameterName"/> out of the <see cref="IAdapter.ConnectionString"/>.
        /// </summary>
        /// <param name="adapter">The <see cref="IAdapter"/> used for source configuration.</param>
        /// <param name="parameterName">Parameter name for the signal ID expected in the <see cref="IAdapter.ConnectionString"/>.</param>
        /// <param name="signalID">The returned <see cref="Guid"/> based signal ID if successfully parsed.</param>
        /// <returns><c>true</c> if signal ID was successfully parsed; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This function is useful when output signals need to be individually specified, e.g., in a calculation, where the outputs are
        /// identified as connection string parameters.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="adapter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><see cref="IAdapter.Settings"/> is <c>null</c>.</exception>
        public static bool TryParseSignalID(this IAdapter adapter, string parameterName, out Guid signalID)
        {
            if ((object)adapter == null)
                throw new ArgumentNullException("adapter");

            return AdapterBase.TryParseSignalID(adapter.DataSource, adapter.Settings, parameterName, out signalID);
        }

        /// <summary>
        /// Attempts to lookup meta-data associated with the specified <paramref name="signalID"/>.
        /// </summary>
        /// <param name="adapter">Adapter to get meta-data for.</param>
        /// <param name="signalID">The <see cref="Guid"/> for the signal to look up the meta-data.</param>
        /// <param name="row"><see cref="DataRow"/> of meta-data associated with the specified <paramref name="signalID"/>.</param>
        /// <param name="measurementTable">Measurement table name to search for meta-data.</param>
        /// <returns><c>true</c> if meta-data record for <paramref name="signalID"/> was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="adapter"/> is <c>null</c>.</exception>
        public static bool TryGetMetadata(this IAdapter adapter, Guid signalID, out DataRow row, string measurementTable = "ActiveMeasurements")
        {
            if ((object)adapter == null)
                throw new ArgumentNullException("adapter");

            return AdapterBase.TryGetMetadata(adapter.DataSource, signalID, out row, measurementTable);
        }

        /// <summary>
        /// Gets the all available meta-data for the set of input or output signals associated with an adapter.
        /// </summary>
        /// <param name="adapter">Adapter to get meta-data for.</param>
        /// <param name="measurementTable">Measurement table name to search for meta-data.</param>
        /// <returns>Available meta-data for a set of input or output signals associated with an adapter.</returns>
        public static Dictionary<Guid, DataRow> GetMetadata(this IAdapter adapter, string measurementTable = "ActiveMeasurements")
        {
            if ((object)adapter == null)
                throw new ArgumentNullException("adapter");

            return GetMetadata(adapter.InputSignalIDs.Union(adapter.OutputSignalIDs), adapter.DataSource, measurementTable);
        }

        /// <summary>
        /// Gets the all available meta-data for a set of signal IDs.
        /// </summary>
        /// <param name="signalIDs">Set of signalIDs to return meta-data for.</param>
        /// <param name="dataSource"><see cref="DataSet"/> containing meta-data to be searched.</param>
        /// <param name="measurementTable">Measurement table name to search for meta-data.</param>
        /// <returns>Available meta-data for a set of input or output signals associated with an adapter.</returns>
        public static Dictionary<Guid, DataRow> GetMetadata(this IEnumerable<Guid> signalIDs, DataSet dataSource, string measurementTable = "ActiveMeasurements")
        {
            if ((object)signalIDs == null)
                throw new ArgumentNullException("signalIDs");

            Dictionary<Guid, DataRow> metadata = new Dictionary<Guid, DataRow>();
            DataRow row;

            foreach (Guid signalID in signalIDs)
            {
                if (AdapterBase.TryGetMetadata(dataSource, signalID, out row))
                    metadata.Add(signalID, row);
            }

            return metadata;
        }

        /// <summary>
        /// Attempts to retrieve the minimum needed number of entities from the frame (as specified by <paramref name="minimumToGet"/>).
        /// </summary>
        /// <param name="adapter">Adapter needing to entity data.</param>
        /// <param name="frame">Source frame for the entities</param>
        /// <param name="entities">Return array of entities</param>
        /// <param name="minimumToGet">Minimum number of input signals required for adapter.  Set to -1 to require all.</param>
        /// <returns><c>true</c> if minimum needed number of entities were returned in entity array; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// This function will only return the minimum needed number of entities, no more.  If you want to use
        /// all available entities in your adapter, just use <see cref="IFrame.Entities"/> directly.
        /// </para>
        /// <para>
        /// Note that the entities array parameter will be created if the reference is null, otherwise if caller creates
        /// array it must be sized to <paramref name="minimumToGet"/>
        /// </para>
        /// </remarks>
        public static bool TryGetMinimumNeededEntities<T>(this IAdapter adapter, IFrame frame, int minimumToGet, ref T[] entities) where T : class, ITimeSeriesEntity
        {
            if ((object)adapter == null)
                throw new ArgumentNullException("adapter");

            if ((object)frame == null)
                throw new ArgumentNullException("frame");

            IDictionary<Guid, ITimeSeriesEntity> frameEntities = frame.Entities;
            ISet<Guid> inputSignals = adapter.InputSignalIDs;
            int index = 0;

            if (minimumToGet == -1)
                minimumToGet = inputSignals.Count();

            if ((object)entities == null || entities.Length < minimumToGet)
                entities = new T[minimumToGet];

            if (inputSignals.Count == 0)
            {
                // No input signals are defined, just get first set of signals in this frame
                foreach (ITimeSeriesEntity entity in frameEntities.Values)
                {
                    entities[index++] = entity as T;

                    if (index == minimumToGet)
                        break;
                }
            }
            else
            {
                // Loop through all input signals to see if they exist in this frame
                ITimeSeriesEntity entity;

                foreach (Guid signalID in inputSignals)
                {
                    if (frameEntities.TryGetValue(signalID, out entity))
                    {
                        entities[index++] = entity as T;

                        if (index == minimumToGet)
                            break;
                    }
                }
            }

            return (index == minimumToGet);
        }
    }
}