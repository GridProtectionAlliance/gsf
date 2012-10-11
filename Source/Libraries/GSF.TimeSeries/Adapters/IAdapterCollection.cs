//******************************************************************************************************
//  IAdapterCollection.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Data;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents the abstract interface for a collection of adapters.
    /// </summary>
    public interface IAdapterCollection : IAdapter, IList<IAdapter>
    {
        /// <summary>
        /// Gets or sets specific data member (e.g., table name) in <see cref="IAdapter.DataSource"/> used to initialize this <see cref="IAdapterCollection"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="DataMember"/> from <see cref="IAdapter.DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </remarks>
        string DataMember { get; set; }

        /// <summary>
        /// Resets the statistics of this collection.
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
    }
}