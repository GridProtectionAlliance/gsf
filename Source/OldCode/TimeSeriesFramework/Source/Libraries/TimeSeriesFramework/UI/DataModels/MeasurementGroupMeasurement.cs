//******************************************************************************************************
//  MeasurementGroupMeasurement.cs - Gbtc
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
//  05/12/2011 - Aniket Salver
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using TVA.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="MeasurementGroupMeasurement"/> information as defined in the database.
    /// </summary>
    class MeasurementGroupMeasurement : DataModelBase
    {
        #region[Members]

        //Fields
        private Guid m_nodeID;
        private int m_measurementGroupID;
        private Guid m_signalID;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region[Properties]

        /// <summary>
        /// Gets or sets the <see cref="MeasurementGroupMeasurement"/> NodeID.
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
        /// Gets or sets the <see cref="MeasurementGroupMeasurement"/> MeasurementGroupID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
        public int MeasurementGroupID
        {
            get
            {
                return m_measurementGroupID;
            }
            set
            {
                m_measurementGroupID = value;
                OnPropertyChanged("MeasurementGroupID");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MeasurementGroupMeasurement"/> SignalID.
        /// </summary>
        // Field is populated by database via auto-increment, so no validation attributes are applied.
        public Guid SignalID
        {
            get
            {
                return m_signalID;
            }
            set
            {
                m_signalID = value;
                OnPropertyChanged("SignalID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementGroupMeasurement"/> CreatedOn.
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
        /// Gets or sets <see cref="MeasurementGroupMeasurement"/> CreatedBy.
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
        /// Gets or sets <see cref="MeasurementGroupMeasurement"/> UpdatedOn.
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
        /// Gets or sets <see cref="MeasurementGroupMeasurement"/> UpdatedBy.
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
        /// Loads <see cref="MeasurementGroupMeasurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="MeasurementGroupMeasurement"/>.</returns>
        public static ObservableCollection<MeasurementGroupMeasurement> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<MeasurementGroupMeasurement> MeasurementGroupMeasurementList = new ObservableCollection<MeasurementGroupMeasurement>();
                DataTable MeasurementGroupMeasurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, MeasurementGroupID , SignalID, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn FROM MeasurementGroupMeasurement ORDER BY NodeID");

                foreach (DataRow row in MeasurementGroupMeasurementTable.Rows)
                {
                    MeasurementGroupMeasurementList.Add(new MeasurementGroupMeasurement()
                    {
                        NodeID = database.Guid(row, "NodeID"),
                        MeasurementGroupID = row.Field<int>("MeasurementGroupID"),
                        SignalID = database.Guid(row, "SignalID"),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn")
                    });
                }

                return MeasurementGroupMeasurementList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="MeasurementGroupMeasurement"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing NodeID and Name of MeasurementGroupMeasurements defined in the database.</returns>
        public static object GetLookupList(AdoDataConnection database, bool isOptional = true)
        {
            return null;
            //bool createdConnection = false;

            //try
            //{
            //    createdConnection = CreateConnection(ref database);

            //    Dictionary<Guid, string> MeasurementGroupMeasurementList = new Dictionary<Guid, string>();
            //    if (isOptional)
            //        MeasurementGroupMeasurementList.Add(Guid.Empty , "Select MeasurementGroupMeasurement");

            //    DataTable MeasurementGroupMeasurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, SignalID  FROM MeasurementGroupMeasurement ORDER BY MeasurementGroupID ");

            //    foreach (DataRow row in MeasurementGroupMeasurementTable.Rows)
            //        MeasurementGroupMeasurementList[database.Guid(row, "NodeID")] = database.Guid(row, "SignaleID")row.Field<string>("SignalID");

            //    return MeasurementGroupMeasurementList;
            //}
            //finally
            //{
            //    if (createdConnection && database != null)
            //        database.Dispose();
            //}
        }

        /// <summary>
        /// Saves <see cref="MeasurementGroupMeasurement"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementGroupMeasurement">Information about <see cref="MeasurementGroupMeasurement"/>.</param>        
        /// <returns>string, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, MeasurementGroupMeasurement measurementGroupMeasurement)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (measurementGroupMeasurement.NodeID == Guid.Empty)
                    database.Connection.ExecuteNonQuery("INSERT INTO MeasurementGroupMeasurement (NodeID, MeasurementGroupID, SignalID, CreatedBy, CreatedOn) " +
                        "VALUES (@nodeID, @measurementGroupID, @signalID, @createdBy, @createdOn)", DefaultTimeout, measurementGroupMeasurement.NodeID, measurementGroupMeasurement.MeasurementGroupID,
                        measurementGroupMeasurement.SignalID, CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE MeasurementGroupMeasurement SET NodeID = @nodeID, MeasurementGroupID = @measurementGroupID, SignalID = @SignalID, " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, measurementGroupMeasurement.NodeID, measurementGroupMeasurement.MeasurementGroupID, measurementGroupMeasurement.SignalID,
                         CommonFunctions.CurrentUser, database.UtcNow(), measurementGroupMeasurement.NodeID);

                return "MeasurementGroupMeasurement information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="MeasurementGroupMeasurement"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="MeasurementGroupMeasurementNodeID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, string MeasurementGroupMeasurementNodeID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM MeasurementGroupMeasurement WHERE ID = @MeasurementGroupMeasurementID", DefaultTimeout, MeasurementGroupMeasurementNodeID);

                return "MeasurementGroupMeasurement deleted successfully";
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
