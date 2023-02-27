//******************************************************************************************************
//  AllAdaptersCollection.cs - Gbtc
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
using System.ComponentModel;
using System.Data;
using GSF.Diagnostics;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a collection of all <see cref="IAdapterCollection"/> implementations (i.e., a collection of <see cref="IAdapterCollection"/>'s).
    /// </summary>
    /// <remarks>
    /// This collection allows all <see cref="IAdapterCollection"/> implementations to be managed as a group.
    /// </remarks>
    public class AllAdaptersCollection : AdapterCollectionBase<IAdapterCollection>
    {
        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="AllAdaptersCollection"/>.
        /// </summary>
        public AllAdaptersCollection() => 
            base.Name = "All Adapters Collection";

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if <see cref="IAdapter"/> implementations are automatically initialized
        /// when they are added to the collection.
        /// </summary>
        /// <remarks>
        /// We don't auto-initialize collections added to the <see cref="AllAdaptersCollection"/> since no data source
        /// will be available when the collections are being created.
        /// </remarks>
        protected override bool AutoInitialize => false;

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source used to load each <see cref="IAdapter"/>.
        /// Updates to this property will cascade to all items in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="AdapterCollectionBase{T}.DataMember"/> from <see cref="AdapterCollectionBase{T}.DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </remarks>
        public override DataSet DataSource
        {
            get => base.DataSource;
            set
            {
                // First remove old adapters that are no longer defined in the configuration
                // so that we do not update the data source of non-existent adapters
                if (Initialized)
                    RemoveOldAdapters(value);

                // Update the data source of all existing adapters
                base.DataSource = value;

                // Add new adapters that weren't
                // previously defined in the configuration
                if (Initialized)
                    AddNewAdapters(value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes each <see cref="IAdapterCollection"/> implementation in this <see cref="AllAdaptersCollection"/>.
        /// </summary>
        public override void Initialize()
        {
            Initialized = false;

            lock (this)
            {
                foreach (IAdapterCollection item in this)
                {
                    try
                    {
                        item.Initialize();
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Warning, ex);
                    }
                }
            }

            Initialized = true;
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IAdapter"/> implementation.</param>
        /// <remarks>
        /// Derived classes should override if more events are defined.
        /// </remarks>
        protected override void InitializeItem(IAdapterCollection item)
        {
            base.InitializeItem(item);
            item.InputMeasurementKeysUpdated += (_, _) => OnInputMeasurementKeysUpdated();
            item.OutputMeasurementsUpdated += (_, _) => OnOutputMeasurementsUpdated();
        }

        /// <summary>
        /// Create newly defined adapters in the adapter collection configurations. 
        /// </summary>
        private void AddNewAdapters(DataSet dataSource)
        {
            List<IAdapterCollection> adapters;

            // Create a local synchronized copy of the items to iterate to avoid nested locks
            lock (this)
            {
                adapters = new List<IAdapterCollection>(this);
            }

            foreach (IAdapterCollection adapterCollection in adapters)
            {
                lock (adapterCollection)
                {
                    string dataMember = adapterCollection.DataMember;

                    if (!dataSource.Tables.Contains(dataMember))
                        continue;

                    // Create newly defined adapters
                    foreach (DataRow adapterRow in dataSource.Tables[dataMember].Rows)
                    {
                        if (adapterCollection.TryGetAdapterByID(uint.Parse(adapterRow[nameof(ID)].ToNonNullString("0")), out _) || !adapterCollection.TryCreateAdapter(adapterRow, out IAdapter adapter))
                            continue;

                        adapterCollection.Add(adapter);
                        OnStatusMessage(MessageLevel.Info, $"[{adapterCollection.Name}] Added new referenced adapter \"{adapter.Name}\" [{adapter.ID}]");
                    }
                }
            }
        }

        /// <summary>
        /// Remove adapters that are no longer present in the adapter collection configurations. 
        /// </summary>
        private void RemoveOldAdapters(DataSet dataSource)
        {
            List<IAdapterCollection> adapters;

            // Create a local synchronized copy of the items to iterate to avoid nested locks
            lock (this)
            {
                adapters = new List<IAdapterCollection>(this);
            }

            foreach (IAdapterCollection adapterCollection in adapters)
            {
                lock (adapterCollection)
                {
                    string dataMember = adapterCollection.DataMember;

                    if (!dataSource.Tables.Contains(dataMember))
                        continue;

                    // Remove adapters that are no longer present in the configuration
                    for (int i = adapterCollection.Count - 1; i >= 0; i--)
                    {
                        IAdapter adapter = adapterCollection[i];
                        DataRow[] adapterRows = dataSource.Tables[dataMember].Select($"ID = {adapter.ID}");

                        if (adapterRows.Length != 0 || adapter.ID == 0)
                            continue;

                        try
                        {
                            adapter.Stop();
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(MessageLevel.Info, new InvalidOperationException($"Exception while stopping adapter {adapter.Name}: {ex.Message}", ex));
                        }

                        adapterCollection.Remove(adapter);
                        OnStatusMessage(MessageLevel.Info, $"[{adapterCollection.Name}] Removed unreferenced adapter \"{adapter.Name}\" [{adapter.ID}]");
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to get any adapter in all collections with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">ID of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <param name="adapterCollection">Adapter collection reference if <paramref name="adapter"/> is found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="id"/> was found; otherwise <c>false</c>.</returns>
        public bool TryGetAnyAdapterByID(uint id, out IAdapter adapter, out IAdapterCollection adapterCollection)
        {
            lock (this)
            {
                foreach (IAdapterCollection collection in this)
                {
                    if (!collection.TryGetAdapterByID(id, out adapter))
                        continue;

                    adapterCollection = collection;
                    return true;
                }
            }

            adapter = null;
            adapterCollection = null;
            return false;
        }

        /// <summary>
        /// Attempts to get any adapter in all collections with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <param name="adapterCollection">Adapter collection reference if <paramref name="adapter"/> is found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="name"/> was found; otherwise <c>false</c>.</returns>
        public bool TryGetAnyAdapterByName(string name, out IAdapter adapter, out IAdapterCollection adapterCollection)
        {
            lock (this)
            {
                foreach (IAdapterCollection collection in this)
                {
                    if (!collection.TryGetAdapterByName(name, out adapter))
                        continue;

                    adapterCollection = collection;
                    return true;
                }
            }

            adapter = null;
            adapterCollection = null;
            return false;
        }

        /// <summary>
        /// Attempts to initialize (or reinitialize) an individual <see cref="IAdapter"/> based on its ID from any collection.
        /// </summary>
        /// <param name="id">The numeric ID associated with the <see cref="IAdapter"/> to be initialized.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// This method traverses all collections looking for an adapter with the specified ID.
        /// </remarks>
        public override bool TryInitializeAdapterByID(uint id)
        {
            lock (this)
            {
                foreach (IAdapterCollection collection in this)
                {
                    if (collection.TryInitializeAdapterByID(id))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method is not implemented in <see cref="AllAdaptersCollection"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryCreateAdapter(DataRow adapterRow, out IAdapterCollection adapter) => 
            throw new NotImplementedException();

        /// <summary>
        /// This method is not implemented in <see cref="AllAdaptersCollection"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryGetAdapterByID(uint id, out IAdapterCollection adapter) => 
            throw new NotImplementedException();

        #endregion
    }
}