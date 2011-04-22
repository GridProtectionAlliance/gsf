//******************************************************************************************************
//  Historian.cs - Gbtc
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
//  04/07/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  04/13/2011 - Mehulbhai P Thakkar
//       Added static methods for database operations.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a record of company information as defined in the database.
    /// </summary>
    public class Historian : DataModelBase
    {
        #region [ Members ]

        // Fields
        private string m_nodeId;
        private int m_ID;
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
        [StringLength(36, ErrorMessage = "Historian node ID cannot exceed 36 characters.")]
        public string NodeId
        {
            get
            {
                return m_nodeId;
            }
            set
            {
                m_nodeId = value;
                OnPropertyChanged("NodeId");
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
                return m_ID;
            }
            set
            {
                m_ID = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Acronym.
        /// </summary>
        [Required(ErrorMessage = "Historian acronym is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "Historian acronym cannot exceed 50 characters.")]
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
        [StringLength(100, ErrorMessage = "Historian name cannot exceed 100 characters")]
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
        [DefaultValue(typeof(bool), "true")]
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
        // No validation attributes are applied because of database design.
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
        [DefaultValue(typeof(int), "0")]
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
        [DefaultValue(typeof(bool), "false")]
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
        [Required(ErrorMessage = "Historian Measurement Reporting Interval is a required field, please provide value.")]
        [DefaultValue(typeof(int), "100000")]
        public int MeasurementReportingInterval
        {
            get
            {
                return m_measurementReportingInterval;
            }
            set
            {
                m_measurementReportingInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Historian" />'s Node Name.
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
        /// Loads <see cref="Historian"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="nodeID">Id of the <see cref="Node"/> for which <see cref="Historian"/> collection is returned.</param>
        /// <returns>Collection of <see cref="Historian"/>.</returns>
        public static ObservableCollection<Historian> Load(AdoDataConnection database, string nodeID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (database.IsJetEngine())
                    nodeID = "{" + nodeID + "}";

                ObservableCollection<Historian> historianList = new ObservableCollection<Historian>();
                DataTable historianTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, ID, Acronym, Name, AssemblyName, TypeName, " +
                    "ConnectionString, IsLocal, Description, LoadOrder, Enabled, MeasurementReportingInterval, NodeName FROM HistorianDetail " +
                    "WHERE NodeID = @nodeID ORDER BY LoadOrder", DefaultTimeout, nodeID);

                foreach (DataRow row in historianTable.Rows)
                {
                    historianList.Add(new Historian()
                        {
                            NodeId = row.Field<string>("NodeID"),
                            ID = row.Field<int>("ID"),
                            Acronym = row.Field<string>("Acronym"),
                            Name = row.Field<string>("Name"),
                            AssemblyName = row.Field<string>("AssemblyName"),
                            TypeName = row.Field<string>("TypeName"),
                            ConnectionString = row.Field<string>("ConnectionString"),
                            IsLocal = Convert.ToBoolean(row.Field<object>("IsLocal")),
                            Description = row.Field<string>("Description"),
                            LoadOrder = row.Field<int>("LoadOrder"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            MeasurementReportingInterval = row.Field<int>("MeasurementReportingInterval"),
                            NodeName = row.Field<string>("NodeName")
                        });
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

                DataTable historianTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM Historian WHERE Enabled = @enabled " +
                    "ORDER BY LoadOrder", DefaultTimeout, true);

                foreach (DataRow row in historianTable.Rows)
                    historianList[row.Field<int>("ID")] = row.Field<string>("Acronym");

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
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Historian historian, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                if (isNew)
                    database.Connection.ExecuteNonQuery("INSERT INTO Historian (NodeID, Acronym, Name, AssemblyName, TypeName, ConnectionString, IsLocal, MeasurementReportingInterval, " +
                        "Description, LoadOrder, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES (@nodeID, @acronym, @name, @assemblyName, @typeName, @connectionString, " +
                        "@isLocal, @measurementReportingInterval, @description, @loadOrder, @enabled, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, historian.NodeId,
                        historian.Acronym.Replace(" ", "").ToUpper(), historian.Name, historian.AssemblyName, historian.TypeName, historian.ConnectionString, historian.IsLocal,
                        historian.MeasurementReportingInterval, historian.Description, historian.LoadOrder, historian.Enabled, CommonFunctions.CurrentUser,
                        database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, CommonFunctions.CurrentUser,
                        database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow);
                else
                    database.Connection.ExecuteNonQuery("UPDATE Historian SET NodeID = @nodeID, Acronym = @acronym, Name = @name, AssemblyName = @assemblyName, TypeName = @typeName, " +
                        "ConnectionString = @connectionString, IsLocal = @isLocal, MeasurementReportingInterval = @measurementReportingInterval, Description = @description, " +
                        "LoadOrder = @loadOrder, Enabled = @enabled, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, historian.NodeId, historian.Acronym.Replace(" ", "").ToUpper(),
                        historian.Name, historian.AssemblyName, historian.TypeName, historian.ConnectionString, historian.IsLocal, historian.MeasurementReportingInterval,
                        historian.Description, historian.LoadOrder, historian.Enabled, CommonFunctions.CurrentUser,
                        database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, historian.ID);

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

                database.Connection.ExecuteNonQuery("DELETE FROM Historian WHERE ID = @historianID", DefaultTimeout, historianID);

                return "Historian deleted successfully";
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
