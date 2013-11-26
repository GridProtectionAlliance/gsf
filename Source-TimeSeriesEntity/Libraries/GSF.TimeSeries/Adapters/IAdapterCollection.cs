//******************************************************************************************************
//  IAdapterCollection.cs - Gbtc
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

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the abstract interface for a collection of adapters.
    /// </summary>
    public interface IAdapterCollection : IList<IAdapter>, ISupportLifecycle, IProvideStatus
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </para>
        /// <para>
        /// EventHander sender object will be represent source adapter or this collection.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </para>
        /// <para>
        /// Implementations of this interface are expected to capture any exceptions that might be thrown by
        /// user code in any processing to prevent third-party code from causing an unhandled exception
        /// in the host.  Errors are reported via this event so host administrators will be aware of the exception.
        /// Any needed connection cycle to data adapter should be restarted when an exception is encountered.
        /// </para>
        /// <para>
        /// EventHander sender object will be represent source adapter or this collection.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<Exception>> ProcessException;

        /// <summary>
        /// Event is raised when <see cref="InputSignals"/> are updated in any of the adapters in the collection.
        /// </summary>
        /// <remarks>
        /// EventHander sender object will be represent source adapter.
        /// </remarks>
        event EventHandler InputSignalsUpdated;

        /// <summary>
        /// Event is raised when <see cref="OutputSignals"/> are updated in any of the adapters in the collection.
        /// </summary>
        /// <remarks>
        /// EventHander sender object will be represent source adapter.
        /// </remarks>
        event EventHandler OutputSignalsUpdated;

        /// <summary>
        /// Event is raised when an adapter is aware of a configuration change.
        /// </summary>
        /// <remarks>
        /// EventHander sender object will be represent source adapter.
        /// </remarks>
        event EventHandler ConfigurationChanged;

        /// <summary>
        /// This event is raised if there are any time-series entities being discarded during processing in any of the adapters in the collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T}.Argument"/> is the enumeration of <see cref="ITimeSeriesEntity"/> objects that are being discarded during processing.
        /// </para>
        /// <para>
        /// EventHander sender object will be represent source adapter.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs<IEnumerable<ITimeSeriesEntity>>> EntitiesDiscarded;

        /// <summary>
        /// Event is raised when this <see cref="IAdapterCollection"/> is disposed or an <see cref="IAdapter"/> in the collection is disposed.
        /// </summary>
        /// <remarks>
        /// EventHander sender object will be represent source adapter or this collection.
        /// </remarks>
        event EventHandler Disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source used to load each <see cref="IAdapter"/>.
        /// Updates to this property will cascade to all items in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be strings.
        /// </remarks>
        DataSet DataSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets specific data member (e.g., table name) in <see cref="IAdapter.DataSource"/> used to initialize this <see cref="IAdapterCollection"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be strings.
        /// </remarks>
        string DataMember
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the cumulative set of signals that adapters in this collection wishes to receive as input.
        /// </summary>
        /// <remarks>
        /// It is expected that that this value will never return null.
        /// </remarks>
        ISet<Guid> InputSignals
        {
            get;
        }

        /// <summary>
        /// Gets the cumulative set of signals that adapters in this collection plan to create as output.
        /// </summary>
        /// <remarks>
        /// It is expected that that this value will never return null.
        /// </remarks>
        ISet<Guid> OutputSignals
        {
            get;
        }

        /// <summary>
        /// Gets or sets name of this <see cref="IAdapterCollection"/>.
        /// </summary>
        new string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter collection and applies this interval to each adapter.
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
        /// Gets the total number of measurements processed thus far by each <see cref="IAdapter"/> implementation
        /// in the <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        long ProcessedEntities
        {
            get;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the the adapter collection.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Resets the statistics of this adapter collection.
        /// </summary>
        void ResetStatistics();

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">ID of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="id"/> was found; otherwise <c>false</c>.</returns>
        bool TryGetAdapterByID(uint id, out IAdapter adapter);

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="name"/> was found; otherwise <c>false</c>.</returns>
        bool TryGetAdapterByName(string name, out IAdapter adapter);

        /// <summary>
        /// Attempts to create an <see cref="IAdapter"/> from the specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="adapterRow"><see cref="DataRow"/> containing item information to initialize.</param>
        /// <param name="adapter">Initialized adapter if successful; otherwise null.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        bool TryCreateAdapter(DataRow adapterRow, out IAdapter adapter);

        /// <summary>
        /// Attempts to initialize (or reinitialize) an individual <see cref="IAdapter"/> based on its ID.
        /// </summary>
        /// <param name="id">The numeric ID associated with the <see cref="IAdapter"/> to be initialized.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        bool TryInitializeAdapterByID(uint id);

        #endregion
    }
}