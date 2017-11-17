//******************************************************************************************************
//  Adapter.cs - Gbtc
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
//  04/13/2011 - Aniket Salver
//       Generated original version of source code.
//  04/19/2011 - Mehulbhai Thakkar
//       Added static methods for database operations.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL value and Guid parameter handling for Save() operation.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
{
    #region [ Enumerations ]

    /// <summary>
    /// AdapterType method enumeration.
    /// </summary>
    public enum AdapterType
    {
        /// <summary>
        /// Action Adapter.
        /// </summary>
        /// <remarks>
        /// Use this option to process the incoming data.
        /// </remarks>
        Action,
        /// <summary>
        /// Filter Adapter.
        /// </summary>
        /// <remarks>
        /// Use this option to modify incoming data before routing to other adapters.
        /// </remarks>
        Filter,
        /// <summary>
        /// Input Adapter.
        /// </summary>
        /// <remarks>
        /// Use this option to collect stream data and assign incoming measurements an ID.
        /// </remarks>
        Input,
        /// <summary>
        /// Output Adapter.
        /// </summary>
        /// <remarks>
        /// Use this option to queue up data for archival.
        /// </remarks>
        Output
    }

    #endregion

    /// <summary>
    /// Represents a record of <see cref="Adapter"/> information as defined in the database.
    /// </summary>
    public class Adapter : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_nodeID;
        private int m_id;
        private string m_adapterName;
        private string m_assemblyName;
        private string m_typeName;
        private string m_connectionString;
        private int m_loadOrder;
        private bool m_enabled;
        private string m_nodeName;
        private AdapterType m_adapterType;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = " Adapter node ID is a required field, please select a value.")]
        public Guid NodeID
        {
            get
            {
                return m_nodeID;
            }
            set
            {
                m_nodeID = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> ID
        /// </summary>
        // Field is populated by auto-increment and has no screen interaction, so no validation attributes are applied
        public int ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> AdapterName
        /// </summary>
        [Required(ErrorMessage = " Adapter name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = " Adapter name cannot exceed 200 characters.")]
        [AcronymValidation]
        public string AdapterName
        {
            get
            {
                return m_adapterName;
            }
            set
            {
                m_adapterName = value;
                OnPropertyChanged("AdapterName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> AssemblyName.
        /// </summary>
        [Required(ErrorMessage = " Adapter assembly name is a required field, please provide value.")]
        public string AssemblyName
        {
            get
            {
                return m_assemblyName;
            }
            set
            {
                m_assemblyName = value;
                OnPropertyChanged("AssemblyName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> TypeName.
        /// </summary>
        [Required(ErrorMessage = " Adapter type name is a required field, please provide value.")]
        public string TypeName
        {
            get
            {
                return m_typeName;
            }
            set
            {
                m_typeName = value;
                OnPropertyChanged("TypeName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> ConnectionString.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                OnPropertyChanged("ConnectionString");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> LoadOrder
        /// </summary>
        [Required(ErrorMessage = " Adapter load order is a required field, please provide value.")]
        [DefaultValue(0)]
        public int LoadOrder
        {
            get
            {
                return m_loadOrder;
            }
            set
            {
                m_loadOrder = value;
                OnPropertyChanged("LoadOrder");

            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> Enabled.
        /// </summary>
        [DefaultValue(false)]
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                m_enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> NodeName.
        /// </summary>
        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
            set
            {
                m_nodeName = value;
                OnPropertyChanged("NodeName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> Type.
        /// </summary>
        public AdapterType Type
        {
            get
            {
                return m_adapterType;
            }
            set
            {
                m_adapterType = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> CreatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn
        {
            get
            {
                return m_createdOn;
            }
            set
            {
                m_createdOn = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> CreatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get
            {
                return m_createdBy;
            }
            set
            {
                m_createdBy = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> UpdatedOn.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get
            {
                return m_updatedOn;
            }
            set
            {
                m_updatedOn = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Adapter"/> UpdatedBy.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get
            {
                return m_updatedBy;
            }
            set
            {
                m_updatedBy = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods      

        /// <summary>
        /// Loads <see cref="Adapter"/> IDs as an <see cref="IList{T}"/>.
        /// </summary>        
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="adapterType"><see cref="AdapterType"/> collection to be returned.</param>      
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>  
        /// <returns>Collection of <see cref="Int32"/>.</returns>
        public static IList<int> LoadIDs(AdoDataConnection database, AdapterType adapterType, string sortMember, string sortDirection)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string viewName;

                if (adapterType == AdapterType.Action)
                    viewName = "CustomActionAdapterDetail";
                else if (adapterType == AdapterType.Filter)
                    viewName = "CustomFilterAdapterDetail";
                else if (adapterType == AdapterType.Input)
                    viewName = "CustomInputAdapterDetail";
                else
                    viewName = "CustomOutputAdapterDetail";

                IList<int> adapterList = new List<int>();
                string sortClause = string.Empty;
                DataTable adapterTable;
                string query;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                query = database.ParameterizedQueryString(string.Format("SELECT ID FROM {0} WHERE NodeID = {{0}} {1}", viewName, sortClause), "nodeID");

                adapterTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in adapterTable.Rows)
                {
                    adapterList.Add(row.ConvertField<int>("ID"));
                }

                return adapterList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="Adapter"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>        
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="adapterType"><see cref="AdapterType"/> collection to be returned.</param>     
        /// <param name="keys">Keys of the adapters to be loaded from the database.</param>   
        /// <returns>Collection of <see cref="Adapter"/>.</returns>
        public static ObservableCollection<Adapter> Load(AdoDataConnection database, AdapterType adapterType, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string viewName;

                if (adapterType == AdapterType.Action)
                    viewName = "CustomActionAdapterDetail";
                else if (adapterType == AdapterType.Filter)
                    viewName = "CustomFilterAdapterDetail";
                else if (adapterType == AdapterType.Input)
                    viewName = "CustomInputAdapterDetail";
                else
                    viewName = "CustomOutputAdapterDetail";

                string query;
                string commaSeparatedKeys;

                Adapter[] adapterList = null;
                DataTable adapterTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => "" + key.ToString() + "").Aggregate((str1, str2) => str1 + "," + str2);
                    query = database.ParameterizedQueryString(string.Format("SELECT NodeID, ID, AdapterName, AssemblyName, TypeName, ConnectionString, " +
                        "LoadOrder, Enabled, NodeName FROM {0} WHERE NodeID = {{0}} AND ID IN ({1})", viewName, commaSeparatedKeys), "nodeID");

                    adapterTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());
                    adapterList = new Adapter[adapterTable.Rows.Count];

                    foreach (DataRow row in adapterTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        adapterList[keys.IndexOf(id)] = new Adapter()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            ID = id,
                            AdapterName = row.Field<string>("AdapterName"),
                            AssemblyName = row.Field<string>("AssemblyName"),
                            TypeName = row.Field<string>("TypeName"),
                            ConnectionString = row.Field<string>("ConnectionString"),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            NodeName = row.Field<string>("NodeName"),
                            Type = adapterType
                        };
                    }
                }

                return new ObservableCollection<Adapter>(adapterList ?? new Adapter[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Adapter"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="adapterType">Type of the <see cref="Adapter"/>.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of adapters defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, AdapterType adapterType, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> adapterList = new Dictionary<int, string>();
                if (isOptional)
                    adapterList.Add(0, "Select Adapter");

                string tableName;

                if (adapterType == AdapterType.Action)
                    tableName = "CustomActionAdapter";
                else if (adapterType == AdapterType.Filter)
                    tableName = "CustomFilterAdapter";
                else if (adapterType == AdapterType.Input)
                    tableName = "CustomInputAdapter";
                else
                    tableName = "CustomOutputAdapter";

                string query = database.ParameterizedQueryString("SELECT ID, Name FROM " + tableName + " WHERE Enabled = {0} ORDER BY LoadOrder", "enabled");
                DataTable adapterTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.Bool(true));

                foreach (DataRow row in adapterTable.Rows)
                    adapterList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return adapterList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Adapter"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="adapter">Information about <see cref="Adapter"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Adapter adapter)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string tableName;

                if (adapter.Type == AdapterType.Action)
                    tableName = "CustomActionAdapter";
                else if (adapter.Type == AdapterType.Filter)
                    tableName = "CustomFilterAdapter";
                else if (adapter.Type == AdapterType.Input)
                    tableName = "CustomInputAdapter";
                else
                    tableName = "CustomOutputAdapter";

                if (adapter.ID == 0)
                    database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("INSERT INTO " + tableName + " (NodeID, AdapterName, AssemblyName, TypeName, ConnectionString, LoadOrder, " +
                        "Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})",
                        "nodeID", "adapterName", "assemblyName", "typeName", "connectionString", "loadOrder",
                        "enabled", "updatedBy", "updatedOn", "createdBy", "createdOn"), DefaultTimeout,
                        (adapter.NodeID != Guid.Empty) ? database.Guid(adapter.NodeID) : database.CurrentNodeID(), adapter.AdapterName, adapter.AssemblyName,
                        adapter.TypeName, adapter.ConnectionString.ToNotNull(), adapter.LoadOrder, database.Bool(adapter.Enabled), CommonFunctions.CurrentUser,
                        database.UtcNow, CommonFunctions.CurrentUser,
                        database.UtcNow);
                else
                    database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("UPDATE " + tableName + " SET NodeID = {0}, AdapterName = {1}, AssemblyName = {2}, " +
                        "TypeName = {3}, ConnectionString = {4}, LoadOrder = {5}, Enabled = {6}, UpdatedBy = {7}, " +
                        "UpdatedOn = {8} WHERE ID = {9}", "nodeID", "adapterName", "assemblyName", "typeName", "connectionString",
                        "loadOrder", "enabled", "updatedBy", "updatedOn", "id"), DefaultTimeout, (adapter.NodeID != Guid.Empty) ? database.Guid(adapter.NodeID) : database.CurrentNodeID(),
                        adapter.AdapterName, adapter.AssemblyName, adapter.TypeName, adapter.ConnectionString.ToNotNull(), adapter.LoadOrder, database.Bool(adapter.Enabled),
                        CommonFunctions.CurrentUser, database.UtcNow, adapter.ID);

                return "Adapter information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Adapter"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="adapterType">Type of adapter to determine from which table record to be deleted.</param>
        /// <param name="adapterID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, AdapterType adapterType, int adapterID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                string tableName;

                if (adapterType == AdapterType.Action)
                    tableName = "CustomActionAdapter";
                else if (adapterType == AdapterType.Filter)
                    tableName = "CustomFilterAdapter";
                else if (adapterType == AdapterType.Input)
                    tableName = "CustomInputAdapter";
                else
                    tableName = "CustomOutputAdapter";

                string query = database.ParameterizedQueryString("DELETE FROM " + tableName + " WHERE ID = {0}", "adapterID");
                database.Connection.ExecuteNonQuery(query, DefaultTimeout, adapterID);

                CommonFunctions.SendCommandToService("ReloadConfig");

                return "Adapter deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        #endregion
    }
}
