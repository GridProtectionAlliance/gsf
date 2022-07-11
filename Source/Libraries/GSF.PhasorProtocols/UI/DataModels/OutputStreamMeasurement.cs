//******************************************************************************************************
//  OutputStreamMeasurement.cs - Gbtc
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
//   08/22/2011 - Aniket Salver
//       Generated original version of source code.
//   09/16/2011 - Mehulbhai P Thakkar
//       Modified static methods to fix bugs.
//   09/19/2011 - Mehulbhai P Thakkar
//       Added OnPropertyChanged() on all properties to reflect changes on UI.
//       Fixed Load() and GetLookupList() static methods.
//   09/21/2011 - Aniket Salver
//       Fixed issue, which helps in enabling the save button  on the screen.
//   09/15/2012 - Aniket Salver 
//       Added paging and sorting technique. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.Data;
using GSF.TimeSeries.UI;
using Measurement = GSF.TimeSeries.UI.DataModels.Measurement;

namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="OutputStreamMeasurement"/> information as defined in the database.
    /// </summary>
    public class OutputStreamMeasurement : DataModelBase
    {
        #region [ Members ]

        private Guid m_nodeID;
        private int m_adapterID;
        private int m_id;
        private int? m_historianID;
        private int m_pointID;
        private string m_signalReference;
        private string m_sourcePointTag;
        private string m_historianAcronym;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s NodeID.
        /// </summary>		
        public Guid NodeID
        {
            get => m_nodeID;
            set
            {
                m_nodeID = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s AdapterID.
        /// </summary>
        public int AdapterID
        {
            get => m_adapterID;
            set
            {
                m_adapterID = value;
                OnPropertyChanged("AdapterID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s ID.
        /// </summary>
        public int ID
        {
            get => m_id;
            set => m_id = value;
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s HistorianID.
        /// </summary>
        public int? HistorianID
        {
            get => m_historianID;
            set
            {
                m_historianID = value;
                OnPropertyChanged("HistorianID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s PointID.
        /// </summary>		
        public int PointID
        {
            get => m_pointID;
            set
            {
                m_pointID = value;
                OnPropertyChanged("PointID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s SignalReference.
        /// </summary>
        [StringLength(200, ErrorMessage = "OutputStreamMeasurement SignalReference cannot exceed 200 characters.")]
        [Required(ErrorMessage = "OutputStreamMeasurement SignalReference is a required field, please provide value.")]
        public string SignalReference
        {
            get => m_signalReference;
            set
            {
                m_signalReference = value;
                OnPropertyChanged("SignalReference");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s SourcePointTag.
        /// </summary>        
        public string SourcePointTag => m_sourcePointTag;

        /// <summary>
        /// Gets or sets the current <see cref="OutputStreamMeasurement"/>'s HistorianAcronym.
        /// </summary>
        public string HistorianAcronym => m_historianAcronym;

        /// <summary>
        /// Gets or sets the Date or Time the current <see cref="OutputStreamMeasurement"/> was created on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime CreatedOn
        {
            get => m_createdOn;
            set => m_createdOn = value;
        }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamMeasurement"/> was created by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string CreatedBy
        {
            get => m_createdBy;
            set => m_createdBy = value;
        }

        /// <summary>
        /// Gets or sets the Date or Time when the current <see cref="OutputStreamMeasurement"/> was updated on.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public DateTime UpdatedOn
        {
            get => m_updatedOn;
            set => m_updatedOn = value;
        }

        /// <summary>
        /// Gets or sets who the current <see cref="OutputStreamMeasurement"/> was updated by.
        /// </summary>
        // Field is populated by trigger and has no screen interaction, so no validation attributes are applied
        public string UpdatedBy
        {
            get => m_updatedBy;
            set => m_updatedBy = value;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// LoadKeys <see cref="OutputStreamMeasurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the <see cref="Device"/> to filter data.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Phasor"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database, int outputStreamID, string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                IList<int> outputStreamMeasurementList = new List<int>();

                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = $"ORDER BY {sortMember} {sortDirection}";

                DataTable outputStreamMeasurementTable = database.Connection.RetrieveData(database.AdapterType, $"SELECT ID From OutputStreamMeasurementDetail where AdapterID = {outputStreamID} {sortClause}");

                foreach (DataRow row in outputStreamMeasurementTable.Rows)
                {
                    outputStreamMeasurementList.Add(row.ConvertField<int>("ID"));
                }

                return outputStreamMeasurementList;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="OutputStreamMeasurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the measurement to be loaded from the database</param>
        /// <returns>Collection of <see cref="OutputStreamMeasurement"/>.</returns>
        public static ObservableCollection<OutputStreamMeasurement> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                OutputStreamMeasurement[] outputStreamMeasurementList = null;
                DataTable outputStreamMeasurementTable;
                int id;

                if (keys is not null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => $"{key}").Aggregate((str1, str2) => $"{str1},{str2}");
                    query = $"SELECT NodeID, AdapterID, ID, HistorianID, PointID, SignalReference, SourcePointTag, HistorianAcronym FROM OutputStreamMeasurementDetail WHERE ID IN ({commaSeparatedKeys})";

                    outputStreamMeasurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
                    outputStreamMeasurementList = new OutputStreamMeasurement[outputStreamMeasurementTable.Rows.Count];

                    foreach (DataRow row in outputStreamMeasurementTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");

                        outputStreamMeasurementList[keys.IndexOf(id)] = new OutputStreamMeasurement()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            AdapterID = row.ConvertField<int>("AdapterID"),
                            ID = id,
                            HistorianID = row.ConvertNullableField<int>("HistorianID"),
                            PointID = row.ConvertField<int>("PointID"),
                            SignalReference = row.Field<string>("SignalReference"),
                            m_sourcePointTag = row.Field<string>("SourcePointTag"),
                            m_historianAcronym = row.Field<string>("HistorianAcronym")
                        };
                    }
                }

                return new ObservableCollection<OutputStreamMeasurement>(outputStreamMeasurementList ?? Array.Empty<OutputStreamMeasurement>());
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="OutputStreamMeasurement"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <param name="outputStreamID">ID of the output stream to filter data.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and NodeID of OutputStreamMeasurement defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, int outputStreamID, bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> OutputStreamMeasurementList = new();

                if (isOptional)
                    OutputStreamMeasurementList.Add(0, "Select OutputStreamMeasurement");

                string query = database.ParameterizedQueryString("SELECT PointID, SignalReference FROM OutputStreamMeasurement WHERE AdapterID = {0} ORDER BY LoadOrder", "adapterID");
                
                DataTable OutputStreamMeasurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, outputStreamID);

                foreach (DataRow row in OutputStreamMeasurementTable.Rows)
                    OutputStreamMeasurementList[row.ConvertField<int>("PointID")] = row.Field<string>("SignalReference");

                return OutputStreamMeasurementList;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="OutputStreamMeasurement"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamMeasurement">Information about <see cref="OutputStreamMeasurement"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, OutputStreamMeasurement outputStreamMeasurement)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (outputStreamMeasurement.ID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO OutputStreamMeasurement (NodeID, AdapterID, HistorianID, PointID, SignalReference, " +
                        " UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                        "nodeID", "adapterID", "historianID", "pointID", "signalReference", "updatedBy", "updatedOn", "createdBy",
                        "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, outputStreamMeasurement.NodeID == Guid.Empty ? database.CurrentNodeID() : database.Guid(outputStreamMeasurement.NodeID),
                        outputStreamMeasurement.AdapterID, outputStreamMeasurement.HistorianID.ToNotNull(), outputStreamMeasurement.PointID, outputStreamMeasurement.SignalReference,
                        CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE OutputStreamMeasurement SET NodeID = {0}, AdapterID = {1}, HistorianID = {2}, PointID = {3}, " +
                        "SignalReference = {4}, UpdatedBy = {5}, UpdatedOn = {6} WHERE ID = {7}", "nodeID", "adapterID",
                        "historianID", "pointID", "signalReference", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, database.Guid(outputStreamMeasurement.NodeID), outputStreamMeasurement.AdapterID,
                        outputStreamMeasurement.HistorianID.ToNotNull(), outputStreamMeasurement.PointID, outputStreamMeasurement.SignalReference,
                        CommonFunctions.CurrentUser, database.UtcNow, outputStreamMeasurement.ID);
                }

                return "OutputStreamMeasurement information saved successfully";
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="OutputStreamMeasurement"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="OutputStreamMeasurementID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int OutputStreamMeasurementID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM OutputStreamMeasurement WHERE ID = {0}", "outputStreamMeasurementID"), DefaultTimeout, OutputStreamMeasurementID);

                return "OutputStreamMeasurement deleted successfully";
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Creates and saves new <see cref="OutputStreamMeasurement"/> into the database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the output stream.</param>
        /// <param name="measurements">Collection of measurements to be added.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string AddMeasurements(AdoDataConnection database, int outputStreamID, ObservableCollection<Measurement> measurements)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                foreach (Measurement measurement in measurements)
                {
                    OutputStreamMeasurement outputStreamMeasurement = new()
                    {
                        NodeID = CommonFunctions.CurrentNodeID(),
                        AdapterID = outputStreamID,
                        HistorianID = measurement.HistorianID,
                        PointID = measurement.PointID,
                        SignalReference = measurement.SignalReference
                    };

                    Save(database, outputStreamMeasurement);
                }

                return "Output stream measurements added successfully.";
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        #endregion
    }
}
