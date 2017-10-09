//******************************************************************************************************
//  Historian.cs - Gbtc
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
//  04/07/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  04/13/2011 - Mehulbhai P Thakkar
//       Added static methods for database operations.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/12/2011 - Aniket Salver
//                  Modified the way Guid is retrived from the Data Base.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added regular expression validator for Acronym.
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
    /// <summary>
    /// Represents a record of <see cref="Historian"/> information as defined in the database.
    /// </summary>
    public class Historian : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_nodeID;
        private int m_id;
        private string m_acronym;
        private string m_name;
        private string m_assemblyName;
        private string m_typeName;
        private string m_connectionString;
        private bool m_isLocal;
        private string m_description;
        private int m_loadOrder;
        private bool m_enabled;
        private int m_measurementReportingInterval;
        private string m_nodeName;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Node ID.
        /// </summary>
        [Required(ErrorMessage = "Historian Node ID is a required field, please provide a value.")]
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
        /// Gets or sets the current <see cref="Historian" />'s ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets the current <see cref="Historian" />'s Acronym.
        /// </summary>
        [Required(ErrorMessage = "Historian acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Historian acronym cannot exceed 200 characters.")]
        [AcronymValidation]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
                OnPropertyChanged("Acronym");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Name.
        /// </summary>
        [StringLength(200, ErrorMessage = "Historian name cannot exceed 200 characters")]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Assembly Name.
        /// </summary>
        // No validation attributes are applied because of database design.
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
        /// Gets or sets the current <see cref="Historian" />'s Type Name.
        /// </summary>
        // No validation attributes are applied because of database design.
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
        /// Gets or sets the current <see cref="Historian" />'s Connection String.
        /// </summary>
        // No validation attributes are applied because of database design.
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
        /// Gets or sets whether the current <see cref="Historian" /> is local.
        /// </summary>
        [DefaultValue(true)]
        public bool IsLocal
        {
            get
            {
                return m_isLocal;
            }
            set
            {
                m_isLocal = value;
                OnPropertyChanged("IsLocal");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Description.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Load Order.
        /// </summary>
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
        /// Gets or sets whether the current <see cref="Historian" /> is enabled.
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
        /// Gets or sets the current <see cref="Historian" />'s Measurement Reporting Interval.
        /// </summary>
        [Required(ErrorMessage = "Historian measurement reporting interval is a required field, please provide value.")]
        [DefaultValue(100000)]
        public int MeasurementReportingInterval
        {
            get
            {
                return m_measurementReportingInterval;
            }
            set
            {
                m_measurementReportingInterval = value;
                OnPropertyChanged("MeasurementReportingInterval");
            }
        }

        /// <summary>
        /// Gets the current <see cref="Historian" />'s Node Name.
        /// </summary>
        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Historian" /> was created.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets who the current <see cref="Historian" /> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets when the current <see cref="Historian" /> was updated.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets who the current <see cref="Historian" /> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
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
        /// Loads <see cref="Historian"/> IDs as an <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Int32"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> historianList = new List<int>();
                string sortClause = string.Empty;
                DataTable historianTable;
                string query;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                query = database.ParameterizedQueryString(string.Format("SELECT ID FROM HistorianDetail WHERE NodeID = {{0}} {0}", sortClause), "nodeID");
                historianTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in historianTable.Rows)
                {
                    historianList.Add(row.ConvertField<int>("ID"));
                }

                return historianList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="Historian"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys to load, if any.</param>
        /// <returns>Collection of <see cref="Historian"/>.</returns>
        public static ObservableCollection<Historian> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                Historian[] historianList = null;
                DataTable historianTable;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => key.ToString()).Aggregate((str1, str2) => str1 + "," + str2);
                    query = database.ParameterizedQueryString(string.Format("SELECT NodeID, ID, Acronym, Name, AssemblyName, TypeName, " +
                        "ConnectionString, IsLocal, Description, LoadOrder, Enabled, MeasurementReportingInterval, NodeName" +
                        " FROM HistorianDetail WHERE NodeID = {{0}} AND ID IN ({0})", commaSeparatedKeys), "nodeID");

                    historianTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());
                    historianList = new Historian[historianTable.Rows.Count];

                    foreach (DataRow row in historianTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        historianList[keys.IndexOf(id)] = new Historian()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            ID = id,
                            Acronym = row.Field<string>("Acronym"),
                            Name = row.Field<string>("Name"),
                            AssemblyName = row.Field<string>("AssemblyName"),
                            TypeName = row.Field<string>("TypeName"),
                            ConnectionString = row.Field<string>("ConnectionString"),
                            IsLocal = Convert.ToBoolean(row.Field<object>("IsLocal")),
                            Description = row.Field<string>("Description"),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            MeasurementReportingInterval = row.ConvertField<int>("MeasurementReportingInterval"),
                            m_nodeName = row.Field<string>("NodeName")
                        };
                    }
                }

                return new ObservableCollection<Historian>(historianList ?? new Historian[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Historian"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <param name="includeStatHistorian">Indicates if statistical historian included in the collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of historians defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false, bool includeStatHistorian = true)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> historianList = new Dictionary<int, string>();
                if (isOptional)
                    historianList.Add(0, "Select Historian");

                string query = database.ParameterizedQueryString("SELECT ID, Acronym FROM Historian WHERE Enabled = {0} AND NodeID = {1} ORDER BY LoadOrder", "enabled", "nodeID");
                DataTable historianTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.Bool(true), database.CurrentNodeID());

                foreach (DataRow row in historianTable.Rows)
                {
                    if (!includeStatHistorian)
                    {
                        if (row.Field<string>("Acronym").ToUpper() != "STAT")
                            historianList[row.ConvertField<int>("ID")] = row.Field<string>("Acronym");
                    }
                    else
                        historianList[row.ConvertField<int>("ID")] = row.Field<string>("Acronym");
                }

                return historianList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Historian"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="historian">Infomration about <see cref="Historian"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Historian historian)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (historian.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO Historian (NodeID, Acronym, Name, AssemblyName, TypeName, ConnectionString, IsLocal, " +
                        "MeasurementReportingInterval, Description, LoadOrder, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, " +
                        "{5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14})", "nodeID", "acronym", "name", "assemblyName", "typeName", "connectionString", "isLocal",
                        "measurementReportingInterval", "description", "loadOrder", "enabled", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, (historian.NodeID != Guid.Empty) ? database.Guid(historian.NodeID) : database.CurrentNodeID(),
                        historian.Acronym.Replace(" ", "").ToUpper(), historian.Name.ToNotNull(), historian.AssemblyName.ToNotNull(),
                        historian.TypeName.ToNotNull(), historian.ConnectionString.ToNotNull(), database.Bool(historian.IsLocal), historian.MeasurementReportingInterval,
                        historian.Description.ToNotNull(), historian.LoadOrder, database.Bool(historian.Enabled), CommonFunctions.CurrentUser, database.UtcNow,
                        CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE Historian SET NodeID = {0}, Acronym = {1}, Name = {2}, AssemblyName = {3}, TypeName = {4}, " +
                        "ConnectionString = {5}, IsLocal = {6}, MeasurementReportingInterval = {7}, Description = {8}, LoadOrder = {9}, Enabled = {10}, " +
                        "UpdatedBy = {11}, UpdatedOn = {12} WHERE ID = {13}", "nodeID", "acronym", "name", "assemblyName", "typeName", "connectionString",
                        "isLocal", "measurementReportingInterval", "description", "loadOrder", "enabled", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, (historian.NodeID != Guid.Empty) ? database.Guid(historian.NodeID) : database.CurrentNodeID(),
                        historian.Acronym.Replace(" ", "").ToUpper(), historian.Name.ToNotNull(), historian.AssemblyName.ToNotNull(), historian.TypeName.ToNotNull(),
                        historian.ConnectionString.ToNotNull(), database.Bool(historian.IsLocal), historian.MeasurementReportingInterval, historian.Description.ToNotNull(),
                        historian.LoadOrder, database.Bool(historian.Enabled), CommonFunctions.CurrentUser, database.UtcNow, historian.ID);
                }

                return "Historian information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Historian"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="historianID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int historianID)
        {
            bool createdConnection = false;

            try
            {
                if (database == null)
                {
                    database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                    createdConnection = true;
                }

                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Historian WHERE ID = {0}", "historianID"), DefaultTimeout, historianID);

                CommonFunctions.SendCommandToService("ReloadConfig");

                return "Historian deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Historian"/> information from the database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="whereClause">Filter expression to filter data.</param>
        /// <returns><see cref="Historian"/> information.</returns>
        public static Historian GetHistorian(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                DataTable historianTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM HistorianDetail " + whereClause);

                if (historianTable.Rows.Count == 0)
                    return null;

                DataRow row = historianTable.Rows[0];

                Historian historian = new Historian
                    {
                        NodeID = database.Guid(row, "NodeID"),
                        ID = row.ConvertField<int>("ID"),
                        Acronym = row.Field<string>("Acronym"),
                        Name = row.Field<string>("Name"),
                        AssemblyName = row.Field<string>("AssemblyName"),
                        TypeName = row.Field<string>("TypeName"),
                        ConnectionString = row.Field<string>("ConnectionString"),
                        IsLocal = Convert.ToBoolean(row.Field<object>("IsLocal")),
                        Description = row.Field<string>("Description"),
                        LoadOrder = row.ConvertField<int>("LoadOrder"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        MeasurementReportingInterval = row.ConvertField<int>("MeasurementReportingInterval"),
                        m_nodeName = row.Field<string>("NodeName")
                    };

                return historian;
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
