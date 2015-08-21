//*******************************************************************************************************
//  IAdapterCollection.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/11/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents the abstract interface for a collection of adapters.
    /// </summary>
    [CLSCompliant(false)]
    public interface IAdapterCollection : IAdapter, ICollection<IAdapter>
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
        /// Attempts to get the adapter with the specified <paramref name="ID"/>.
        /// </summary>
        /// <param name="ID">ID of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="ID"/> was found; otherwise <c>false</c>.</returns>
        bool TryGetAdapterByID(uint ID, out IAdapter adapter);

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