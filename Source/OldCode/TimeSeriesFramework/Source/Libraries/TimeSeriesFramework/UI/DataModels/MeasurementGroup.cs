//******************************************************************************************************
//  MeasuremnetGroup.cs - Gbtc
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
//  05/11/2011 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="MeasurementGroup"/> information as defined in the database.
    /// </summary>
    public class MeasurementGroup : DataModelBase
    {
        #region[Members]

        //Fields
        private Guid m_nodeID;
        private int m_ID;
        private string m_name;
        private string m_description;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region[properties]

        /// <summary>
        /// Gets or sets the <see cref="MeasurementGroup"/> NodeID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
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
        /// Gets or sets the <see cref="MeasurementGroup"/> ID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
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
        /// Gets or sets <see cref="MeasurementGroup"/> Name.
        /// </summary>
        [Required(ErrorMessage = "MeasurementGroup name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "MeasurementGroup Name cannot exceed 100 characters.")]
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
        /// Gets or sets <see cref="MeasurementGroup"/>DescriptionText.
        /// </summary>
        // Field is populated by database via trigger and has no screen interaction, so no validation attributes are applied
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
                OnPropertyChanged("DescriptionText");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementGroup"/> CreatedOn.
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
        /// Gets or sets <see cref="MeasurementGroup"/> CreatedBy.
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
        /// Gets or sets <see cref="MeasurementGroup"/> UpdatedOn.
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
        /// Gets or sets <see cref="MeasurementGroup"/> UpdatedBy.
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
        /// Loads <see cref="MeasurementGroup"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="MeasurementGroup"/>.</returns>
        public static ObservableCollection<MeasurementGroup> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<MeasurementGroup> MeasurementGroupList = new ObservableCollection<MeasurementGroup>();
                DataTable MeasurementGroupTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, ID, Name, Description, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn FROM MeasurementGroup ORDER BY ID");

                foreach (DataRow row in MeasurementGroupTable.Rows)
                {
                    MeasurementGroupList.Add(new MeasurementGroup()
                    {
                        NodeID = database.Guid(row, "NodeID"),
                        ID = row.Field<int>("ID"),
                        Name = row.Field<string>("Name"),
                        Description = row.Field<string>("Description"),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn")
                    });
                }

                return MeasurementGroupList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="MeasurementGroup"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of MeasurementGroups defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = true)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> MeasurementGroupList = new Dictionary<int, string>();
                if (isOptional)
                    MeasurementGroupList.Add(0, "Select MeasurementGroup");

                DataTable MeasurementGroupTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM MeasurementGroup ORDER BY SourceIndex");

                foreach (DataRow row in MeasurementGroupTable.Rows)
                    MeasurementGroupList[row.Field<int>("ID")] = row.Field<string>("Name");

                return MeasurementGroupList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="MeasurementGroup"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementGroup">Information about <see cref="MeasurementGroup"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, MeasurementGroup measurementGroup)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (measurementGroup.ID == 0)
                    database.Connection.ExecuteNonQuery("INSERT INTO MeasurementGroup (NodeID, ID, Name, Description, CreatedBy, CreatedOn) " +
                        "VALUES (@nodeID, @iD, @name, @description, @createdBy, @createdOn)", DefaultTimeout, measurementGroup.NodeID, measurementGroup.ID,
                        measurementGroup.Name, measurementGroup.Description, CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE MeasurementGroup SET NodeID = @nodeID, ID = @iD, Name = @name, Description = @description, " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, measurementGroup.NodeID, measurementGroup.ID, measurementGroup.Name, measurementGroup.Description,
                         CommonFunctions.CurrentUser, database.UtcNow(), measurementGroup.ID);

                return "MeasurementGroup information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="MeasurementGroup"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="MeasurementGroupID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int MeasurementGroupID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM MeasurementGroup WHERE ID = @MeasurementGroupID", DefaultTimeout, MeasurementGroupID);

                return "MeasurementGroup deleted successfully";
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

