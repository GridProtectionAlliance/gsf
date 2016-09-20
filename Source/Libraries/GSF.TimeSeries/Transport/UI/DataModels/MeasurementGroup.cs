//******************************************************************************************************
//  MeasuremnetGroup.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  05/11/2011 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using GSF.Data;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;

namespace GSF.TimeSeries.Transport.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="MeasurementGroup"/> information as defined in the database.
    /// </summary>
    public class MeasurementGroup : DataModelBase
    {
        #region [ Members ]

        //Fields
        private Guid m_nodeID;
        private int m_id;
        private string m_name;
        private string m_description;
        private string m_filterExpression;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

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
                return m_id;
            }
            set
            {
                m_id = value;
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementGroup"/> Name.
        /// </summary>
        [Required(ErrorMessage = "MeasurementGroup name is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "MeasurementGroup Name cannot exceed 200 characters.")]
        [DefaultValue("Add New Group")]
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
                OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementGroup"/> FilterExpression text.
        /// </summary>
        public string FilterExpression
        {
            get
            {
                return m_filterExpression;
            }
            set
            {
                m_filterExpression = value;
                OnPropertyChanged("FilterExpression");
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
            ObservableCollection<MeasurementGroup> measurementGroupList;
            DataTable measurementGroupTable;
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);
                measurementGroupList = new ObservableCollection<MeasurementGroup>();
                query = database.ParameterizedQueryString("SELECT NodeID, ID, Name, Description, FilterExpression FROM MeasurementGroup WHERE NodeID = {0} ORDER BY Name", "nodeID");
                measurementGroupTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in measurementGroupTable.Rows)
                {
                    measurementGroupList.Add(new MeasurementGroup()
                    {
                        NodeID = database.Guid(row, "NodeID"),
                        ID = row.ConvertField<int>("ID"),
                        Name = row.Field<string>("Name"),
                        Description = row.Field<object>("Description").ToNonNullString(),
                        FilterExpression = row.Field<object>("FilterExpression").ToNonNullString(),
                    });
                }

                measurementGroupList.Insert(0, new MeasurementGroup());

                return measurementGroupList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves <see cref="MeasurementGroup"/> information for the group with the given ID.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="groupID">The ID of the measurement group to be retrieved.</param>
        /// <returns>Measurement group with the given ID.</returns>
        public static MeasurementGroup GetMeasurementGroup(AdoDataConnection database, int groupID)
        {
            DataTable measurementGroupTable;
            bool createdConnection = false;
            DataRow row;

            try
            {
                createdConnection = CreateConnection(ref database);
                measurementGroupTable = database.RetrieveData(DefaultTimeout, "SELECT * FROM MeasurementGroup WHERE ID = {0}", groupID);

                if (measurementGroupTable.Rows.Count == 0)
                    return null;

                row = measurementGroupTable.Rows[0];

                MeasurementGroup measurementGroup = new MeasurementGroup()
                {
                    NodeID = database.Guid(row, "NodeID"),
                    ID = row.ConvertField<int>("ID"),
                    Name = row.Field<string>("Name"),
                    Description = row.Field<object>("Description").ToNonNullString(),
                    FilterExpression = row.Field<object>("FilterExpression").ToNonNullString(),
                };

                return measurementGroup;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Dictionary{T1,T2}"/> style list of <see cref="MeasurementGroup"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of <see cref="MeasurementGroup"/>s defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = true)
        {
            Dictionary<int, string> measurementGroupList;
            DataTable measurementGroupTable;
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);
                measurementGroupList = new Dictionary<int, string>();

                if (isOptional)
                    measurementGroupList.Add(0, "Select MeasurementGroup");

                query = database.ParameterizedQueryString("SELECT ID, Name FROM MeasurementGroup WHERE NodeID = {0} ORDER BY SourceIndex", "nodeID");
                measurementGroupTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in measurementGroupTable.Rows)
                    measurementGroupList[row.ConvertField<int>("ID")] = row.Field<string>("Name");

                return measurementGroupList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Measurement"/> assigned to <see cref="MeasurementGroup"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementGroupId">ID of the <see cref="MeasurementGroup"/> to filter data.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing SignalID and PointTag of <see cref="Measurement"/>s assigned to <see cref="MeasurementGroup"/>.</returns>
        public static Dictionary<Guid, string> GetCurrentMeasurements(AdoDataConnection database, int measurementGroupId)
        {
            Dictionary<Guid, string> currentMeasurements;
            DataTable currentMeasurementTable;
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);
                currentMeasurements = new Dictionary<Guid, string>();
                query = database.ParameterizedQueryString("SELECT * FROM MeasurementGroupMeasDetail WHERE MeasurementGroupID = {0} ORDER BY PointID", "measurementGroupID");
                currentMeasurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, measurementGroupId);

                foreach (DataRow row in currentMeasurementTable.Rows)
                    currentMeasurements[database.Guid(row, "SignalID")] = row.Field<string>("PointTag");

                return currentMeasurements;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves <see cref="ObservableCollection{T}"/> style list of <see cref="Measurement"/> not yet assigned to <see cref="MeasurementGroup"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementGroupId">ID of the <see cref="MeasurementGroup"/> to filter data.</param>
        /// <returns><see cref="ObservableCollection{T}"/> style list of <see cref="Measurement"/> not yet assigned to <see cref="MeasurementGroup"/>.</returns>
        public static ObservableCollection<Measurement> GetPossibleMeasurements(AdoDataConnection database, int measurementGroupId)
        {
            return Measurement.GetMeasurementsByGroup(database, measurementGroupId);
        }

        /// <summary>
        /// Method to add <see cref="Measurement"/>s to <see cref="MeasurementGroup"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementGroupID">ID of the <see cref="MeasurementGroup"/> to add <see cref="Measurement"/> to.</param>
        /// <param name="measurementsToBeAdded">List of <see cref="Measurement"/> signal ids to be added.</param>
        /// <returns>string, indicating success for UI display.</returns>
        public static int AddMeasurements(AdoDataConnection database, int measurementGroupID, ICollection<Guid> measurementsToBeAdded)
        {
            string QueryFormat =
                "INSERT INTO MeasurementGroupMeasurement (NodeID, MeasurementGroupID, SignalID) " +
                "SELECT {0}, {1}, {2} " +
                "WHERE (SELECT COUNT(*) FROM MeasurementGroupMeasurement WHERE MeasurementGroupID = {1} AND SignalID = {2}) = 0";

            bool createdConnection = false;
            int rowsAffected = 0;

            try
            {
                createdConnection = CreateConnection(ref database);

                foreach (Guid id in measurementsToBeAdded)
                    rowsAffected += database.ExecuteNonQuery(DefaultTimeout, QueryFormat, CommonFunctions.CurrentNodeID(), measurementGroupID, id);

                return rowsAffected;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Method to remove <see cref="Measurement"/>s from <see cref="MeasurementGroup"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementGroupID">ID of the <see cref="MeasurementGroup"/> to remove <see cref="Measurement"/> from.</param>
        /// <param name="measurementsToBeRemoved">List of <see cref="Measurement"/> signal ids to be removed.</param>
        /// <returns>string, indicating success for UI display.</returns>
        public static string RemoveMeasurements(AdoDataConnection database, int measurementGroupID, List<Guid> measurementsToBeRemoved)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                foreach (Guid id in measurementsToBeRemoved)
                {
                    query = database.ParameterizedQueryString("DELETE FROM MeasurementGroupMeasurement WHERE MeasurementGroupID = {0} AND SignalID = {1}", "measurementGroupID", "signalID");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, measurementGroupID, database.Guid(id));
                }

                return "Measurements deleted from group successfully";
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
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (measurementGroup.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO MeasurementGroup (NodeID, Name, Description, FilterExpression, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) " +
                        "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", "nodeID", "name", "description", "filterExpression", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.CurrentNodeID(), measurementGroup.Name, measurementGroup.Description.ToNotNull(),
                        measurementGroup.FilterExpression.ToNotNull(), CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE MeasurementGroup SET NodeID = {0}, Name = {1}, Description = {2}, FilterExpression = {3}, " +
                        "UpdatedBy = {4}, UpdatedOn = {5} WHERE ID = {6}", "nodeID", "name", "description", "filterExpression", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.CurrentNodeID(), measurementGroup.Name, measurementGroup.Description.ToNotNull(),
                        measurementGroup.FilterExpression.ToNotNull(), CommonFunctions.CurrentUser, database.UtcNow, measurementGroup.ID);
                }

                return "Measurement group information saved successfully";
            }
            catch (Exception ex)
            {
                return ex.Message;
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
        /// <param name="measurementGroupID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int measurementGroupID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM MeasurementGroup WHERE ID = {0}", "measurementGroupID"), DefaultTimeout, measurementGroupID);

                return "Measurement group deleted successfully";
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

