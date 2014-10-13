//******************************************************************************************************
//  Alarm.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  02/10/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modifeid Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
{
    /// <summary>
    /// Represents a record of <see cref="Alarm"/> information as defined in the database.
    /// </summary>
    public class Alarm : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_nodeId;
        private int m_id;
        private string m_tagName;
        private Guid m_signalId;
        private Guid? m_associatedMeasurementID;
        private string m_description;
        private int m_severity;
        private int m_operation;
        private double? m_setPoint;
        private double? m_tolerance;
        private double? m_delay;
        private double? m_hysteresis;
        private int m_loadOrder;
        private bool m_enabled;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        private string m_operationDescription;
        private string m_severityName;
        private bool m_createAssociatedMeasurement;
        private bool m_setPointEnabled;
        private bool m_toleranceEnabled;
        private bool m_delayEnabled;
        private bool m_hysteresisEnabled;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Alarm"/> class.
        /// </summary>
        public Alarm()
        {
            CreateAssociatedMeasurement = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> NodeID
        /// </summary>
        [Required(ErrorMessage = " Alarm node ID is a required field, please select a value.")]
        public Guid NodeID
        {
            get
            {
                return m_nodeId;
            }
            set
            {
                m_nodeId = value;
                OnPropertyChanged("NodeID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> ID
        /// </summary>
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
        /// Gets or sets <see cref="Alarm"/> TagName
        /// </summary>
        [Required(ErrorMessage = " Alarm tag name is a required field, please select a value.")]
        [StringLength(200, ErrorMessage = "Alarm tag name cannot exceed 200 characters.")]
        [RegularExpression("^[A-Z0-9-'!'_''.' @#\\$]+$", ErrorMessage = "Only upper case letters, numbers, '!', '-', '@', '#', '_' , '.'and '$' are allowed.")]
        public string TagName
        {
            get
            {
                return m_tagName;
            }
            set
            {
                m_tagName = value;
                OnPropertyChanged("TagName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> SignalID
        /// </summary>
        [Required(ErrorMessage = " Alarm signal ID is a required field, please select a value.")]
        public Guid SignalID
        {
            get
            {
                return m_signalId;
            }
            set
            {
                m_signalId = value;
                OnPropertyChanged("SignalID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> AssociatedMeasurementID
        /// </summary>
        public Guid? AssociatedMeasurementID
        {
            get
            {
                return m_associatedMeasurementID;
            }
            set
            {
                m_associatedMeasurementID = value;
                OnPropertyChanged("AssociatedMeasurementID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> Description
        /// </summary>
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
        /// Gets or sets <see cref="Alarm"/> Severity
        /// </summary>
        [Required(ErrorMessage = " Alarm severity is a required field, please select a value.")]
        public int Severity
        {
            get
            {
                return m_severity;
            }
            set
            {
                m_severity = value;
                OnPropertyChanged("Severity");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> Operation
        /// </summary>
        [Required(ErrorMessage = " Alarm operation is a required field, please select a value.")]
        public int Operation
        {
            get
            {
                return m_operation;
            }
            set
            {
                m_operation = value;
                OnPropertyChanged("Operation");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> SetPoint
        /// </summary>
        public double? SetPoint
        {
            get
            {
                return m_setPoint;
            }
            set
            {
                m_setPoint = value;
                OnPropertyChanged("SetPoint");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> Tolerance
        /// </summary>
        public double? Tolerance
        {
            get
            {
                return m_tolerance;
            }
            set
            {
                m_tolerance = value;
                OnPropertyChanged("Tolerance");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> Delay
        /// </summary>
        public double? Delay
        {
            get
            {
                return m_delay;
            }
            set
            {
                m_delay = value;
                OnPropertyChanged("Delay");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> Hysteresis
        /// </summary>
        public double? Hysteresis
        {
            get
            {
                return m_hysteresis;
            }
            set
            {
                m_hysteresis = value;
                OnPropertyChanged("Hysteresis");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> LoadOrder
        /// </summary>
        [Required(ErrorMessage = " Alarm load order is a required field, please provide value.")]
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
        /// Gets or sets <see cref="Alarm"/> Enabled.
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
        /// Gets or sets <see cref="Alarm"/> CreatedOn.
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
        /// Gets or sets <see cref="Alarm"/> CreatedBy.
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
        /// Gets or sets <see cref="Alarm"/> UpdatedOn.
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
        /// Gets or sets <see cref="Alarm"/> UpdatedBy.
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

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> OperationDescription
        /// </summary>
        public string OperationDescription
        {
            get
            {
                return m_operationDescription;
            }
            set
            {
                m_operationDescription = value;
                OnPropertyChanged("OperationDescription");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> SeverityName
        /// </summary>
        public string SeverityName
        {
            get
            {
                return m_severityName;
            }
            set
            {
                m_severityName = value;
                OnPropertyChanged("SeverityName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> CreateAssociatedMeasurement
        /// </summary>
        public bool CreateAssociatedMeasurement
        {
            get
            {
                return m_createAssociatedMeasurement;
            }
            set
            {
                m_createAssociatedMeasurement = value;
                OnPropertyChanged("CreateAssociatedMeasurement");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> SetPointEnabled
        /// </summary>
        public bool SetPointEnabled
        {
            get
            {
                return m_setPointEnabled;
            }
            set
            {
                m_setPointEnabled = value;
                OnPropertyChanged("SetPointEnabled");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> ToleranceEnabled
        /// </summary>
        public bool ToleranceEnabled
        {
            get
            {
                return m_toleranceEnabled;
            }
            set
            {
                m_toleranceEnabled = value;
                OnPropertyChanged("ToleranceEnabled");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> DelayEnabled
        /// </summary>
        public bool DelayEnabled
        {
            get
            {
                return m_delayEnabled;
            }
            set
            {
                m_delayEnabled = value;
                OnPropertyChanged("DelayEnabled");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Alarm"/> HysteresisEnabled
        /// </summary>
        public bool HysteresisEnabled
        {
            get
            {
                return m_hysteresisEnabled;
            }
            set
            {
                m_hysteresisEnabled = value;
                OnPropertyChanged("HysteresisEnabled");
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads <see cref="Alarm"/> IDs as an <see cref="IList{T}"/>.
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

                IList<int> alarmList = new List<int>();
                string sortClause = string.Empty;
                DataTable adapterTable;
                string query;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                query = database.ParameterizedQueryString(string.Format("SELECT ID FROM Alarm WHERE NodeID = {{0}} {0}", sortClause), "nodeID");

                adapterTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in adapterTable.Rows)
                {
                    alarmList.Add(row.ConvertField<int>("ID"));
                }

                return alarmList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="Alarm"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>        
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the adapters to be loaded from the database.</param>
        /// <returns>Collection of <see cref="Alarm"/>.</returns>
        public static ObservableCollection<Alarm> Load(AdoDataConnection database, IList<int> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                Alarm[] alarmList = null;
                DataTable alarmTable;
                object associatedMeasurementId;
                int id;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => "" + key.ToString() + "").Aggregate((str1, str2) => str1 + "," + str2);
                    query = database.ParameterizedQueryString(string.Format("SELECT NodeID, TagName, ID, SignalID, AssociatedMeasurementID, Description, Severity, Operation, " +
                        "SetPoint, Tolerance, Delay, Hysteresis, LoadOrder, Enabled FROM Alarm WHERE NodeID = {{0}} AND ID IN ({0})", commaSeparatedKeys), "nodeID");

                    alarmTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());
                    alarmList = new Alarm[alarmTable.Rows.Count];

                    foreach (DataRow row in alarmTable.Rows)
                    {
                        id = row.ConvertField<int>("ID");
                        associatedMeasurementId = row.Field<object>("AssociatedMeasurementID");

                        alarmList[keys.IndexOf(id)] = new Alarm()
                        {
                            NodeID = database.Guid(row, "NodeID"),
                            ID = id,
                            TagName = row.Field<string>("TagName"),
                            SignalID = database.Guid(row, "SignalID"),
                            AssociatedMeasurementID = (associatedMeasurementId != null) ? Guid.Parse(associatedMeasurementId.ToString()) : (Guid?)null,
                            Description = row.Field<string>("Description"),
                            Severity = row.ConvertField<int>("Severity"),
                            Operation = row.ConvertField<int>("Operation"),
                            SetPoint = row.ConvertNullableField<double>("SetPoint"),
                            Tolerance = row.ConvertNullableField<double>("Tolerance"),
                            Delay = row.ConvertNullableField<double>("Delay"),
                            Hysteresis = row.ConvertNullableField<double>("Hysteresis"),
                            LoadOrder = row.ConvertField<int>("LoadOrder"),
                            Enabled = row.ConvertField<bool>("Enabled"),
                            CreateAssociatedMeasurement = (associatedMeasurementId != null)
                        };
                    }
                }

                return new ObservableCollection<Alarm>(alarmList ?? new Alarm[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves an <see cref="Alarm"/> information from the database based on query string filter.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="whereClause">Filter clause to append to the SELECT query.</param>
        /// <returns><see cref="Alarm"/> information.</returns>
        public static Alarm GetAlarm(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                DataTable alarmTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM Alarm " + whereClause);

                if (alarmTable.Rows.Count == 0)
                    return null;

                DataRow row = alarmTable.Rows[0];
                object associatedMeasurementId = row.Field<object>("AssociatedMeasurementID");

                Alarm alarm = new Alarm
                {
                    NodeID = database.Guid(row, "NodeID"),
                    ID = row.ConvertField<int>("ID"),
                    TagName = row.Field<string>("TagName"),
                    SignalID = database.Guid(row, "SignalID"),
                    AssociatedMeasurementID = ((object)associatedMeasurementId != null) ? Guid.Parse(associatedMeasurementId.ToString()) : (Guid?)null,
                    Description = row.Field<string>("Description"),
                    Severity = row.ConvertField<int>("Severity"),
                    Operation = row.ConvertField<int>("Operation"),
                    SetPoint = row.ConvertNullableField<double>("SetPoint"),
                    Tolerance = row.ConvertNullableField<double>("Tolerance"),
                    Delay = row.ConvertNullableField<double>("Delay"),
                    Hysteresis = row.ConvertNullableField<double>("Hysteresis"),
                    LoadOrder = row.ConvertField<int>("LoadOrder"),
                    Enabled = row.ConvertField<bool>("Enabled")
                };

                return alarm;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Alarm"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and tag name of alarms defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> alarmList = new Dictionary<int, string>();

                if (isOptional)
                    alarmList.Add(0, "Select Alarm");

                string query = database.ParameterizedQueryString("SELECT ID, TagName FROM Alarm WHERE Enabled = {0} ORDER BY LoadOrder", "enabled");
                DataTable nodeTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.Bool(true));

                foreach (DataRow row in nodeTable.Rows)
                {
                    alarmList[row.ConvertField<int>("ID")] = row.Field<string>("TagName");
                }

                return alarmList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Alarm"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="alarm">Information about <see cref="Alarm"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Alarm alarm)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string updateQuery;
                Alarm createdAlarm = alarm;
                string successMessage = "Alarm information saved successfully";
                object associatedMeasurementId = (alarm.AssociatedMeasurementID != null) ? database.Guid(alarm.AssociatedMeasurementID.Value) : DBNull.Value;

                AlarmMonitor monitor = AlarmMonitor.Default;

                if (alarm.ID == 0)
                {
                    string query = database.ParameterizedQueryString("INSERT INTO Alarm (NodeID, TagName, SignalID, AssociatedMeasurementID, Description, Severity, Operation, SetPoint, Tolerance, Delay, " +
                        "Hysteresis, LoadOrder, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16})",
                        "nodeID", "tagName", "signalId", "associatedMeasurementId", "description", "severity", "operation", "setPoint", "tolerance", "delay",
                        "hysteresis", "loadOrder", "enabled", "updatedBy", "updatedOn", "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, (alarm.NodeID != Guid.Empty) ? database.Guid(alarm.NodeID) : database.CurrentNodeID(),
                        alarm.TagName.ToNotNull(), database.Guid(alarm.SignalID), associatedMeasurementId, alarm.Description.ToNotNull(), alarm.Severity, alarm.Operation, alarm.SetPoint.ToNotNull(),
                        alarm.Tolerance.ToNotNull(), alarm.Delay.ToNotNull(), alarm.Hysteresis.ToNotNull(), alarm.LoadOrder, database.Bool(alarm.Enabled), CommonFunctions.CurrentUser, database.UtcNow,
                        CommonFunctions.CurrentUser, database.UtcNow);

                    createdAlarm = GetAlarm(database, string.Format("WHERE TagName = '{0}'", alarm.TagName));
                }
                else
                {
                    string query = database.ParameterizedQueryString("UPDATE Alarm SET NodeID = {0}, TagName = {1}, SignalID = {2}, AssociatedMeasurementID = {3}, Description = {4}, Severity = {5}, " +
                        "Operation = {6}, SetPoint = {7}, Tolerance = {8}, Delay = {9}, Hysteresis = {10}, LoadOrder = {11}, Enabled = {12}, UpdatedBy = {13}, UpdatedOn = {14} WHERE ID = {15}",
                        "nodeID", "tagName", "signalId", "associatedMeasurementId", "description", "severity", "operation", "setPoint", "tolerance", "delay", "hysteresis",
                        "loadOrder", "enabled", "updatedBy", "updatedOn", "id");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, (alarm.NodeID != Guid.Empty) ? database.Guid(alarm.NodeID) : database.CurrentNodeID(),
                        alarm.TagName, database.Guid(alarm.SignalID), associatedMeasurementId, alarm.Description.ToNotNull(), alarm.Severity, alarm.Operation,
                        alarm.SetPoint.ToNotNull(), alarm.Tolerance.ToNotNull(), alarm.Delay.ToNotNull(), alarm.Hysteresis.ToNotNull(), alarm.LoadOrder, database.Bool(alarm.Enabled),
                        CommonFunctions.CurrentUser, database.UtcNow, alarm.ID);
                }

                updateQuery = database.ParameterizedQueryString("UPDATE Alarm SET AssociatedMeasurementID = {0} WHERE ID = {1}", "associatedMeasurementId", "id");

                if (alarm.CreateAssociatedMeasurement && (object)alarm.AssociatedMeasurementID == null)
                {
                    alarm.AssociatedMeasurementID = CreateAlarmMeasurement(database, createdAlarm);

                    if ((object)alarm.AssociatedMeasurementID != null)
                        database.Connection.ExecuteNonQuery(updateQuery, DefaultTimeout, database.Guid(alarm.AssociatedMeasurementID.Value), createdAlarm.ID);
                    else
                        successMessage += " but failed to create associated measurement";
                }
                else if (!alarm.CreateAssociatedMeasurement && (object)alarm.AssociatedMeasurementID != null)
                {
                    database.Connection.ExecuteNonQuery(updateQuery, DefaultTimeout, DBNull.Value, createdAlarm.ID);
                    DeleteAlarmMeasurement(database, createdAlarm.AssociatedMeasurementID.Value);
                    alarm.AssociatedMeasurementID = null;
                }

                if ((object)monitor != null)
                    monitor.UpdateDefinedAlarms();

                try
                {
                    CommonFunctions.SendCommandToService("ReloadConfig");
                }
                catch (Exception ex)
                {
                    CommonFunctions.LogException(database, "Alarm Save", ex);
                }

                return successMessage;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Alarm"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="alarmId">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int alarmId)
        {
            bool createdConnection = false;
            string query;
            object associatedMeasurementId;

            AlarmMonitor monitor = AlarmMonitor.Default;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                query = database.ParameterizedQueryString("SELECT AssociatedMeasurementID FROM Alarm WHERE ID = {0}", "alarmId");
                associatedMeasurementId = database.Connection.ExecuteScalar(query, DefaultTimeout, alarmId);

                query = database.ParameterizedQueryString("DELETE FROM Alarm WHERE ID = {0}", "alarmId");
                database.Connection.ExecuteNonQuery(query, DefaultTimeout, alarmId);

                if (associatedMeasurementId != null && associatedMeasurementId != DBNull.Value)
                {
                    Guid signalId = Guid.Parse(associatedMeasurementId.ToString());
                    query = database.ParameterizedQueryString("DELETE FROM Measurement WHERE SignalID = {0}", "signalId");
                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, signalId);
                }

                if ((object)monitor != null)
                    monitor.UpdateDefinedAlarms();

                CommonFunctions.SendCommandToService("ReloadConfig");

                return "Alarm deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        // Creates a measurement associated with the given alarm and returns the new measurements signal ID.
        private static Guid? CreateAlarmMeasurement(AdoDataConnection database, Alarm alarm)
        {
            object nodeID;
            Historian historian;
            Measurement alarmMeasurement;
            int signalTypeId;

            try
            {
                nodeID = (alarm.NodeID != Guid.Empty) ? database.Guid(alarm.NodeID) : database.CurrentNodeID();
                historian = Historian.GetHistorian(database, string.Format("WHERE Acronym = 'STAT' AND NodeID = '{0}'", nodeID));
                signalTypeId = Convert.ToInt32(database.Connection.ExecuteScalar("SELECT ID FROM SignalType WHERE Acronym = 'ALRM'", DefaultTimeout));

                alarmMeasurement = new Measurement()
                {
                    HistorianID = historian.ID,
                    PointTag = alarm.TagName,
                    SignalTypeID = signalTypeId,
                    SignalReference = "ALARM!SERVICES-AL" + alarm.ID,
                    Description = "Measurement associated with alarm " + alarm.ID,
                    Internal = true,
                    Enabled = true
                };

                Measurement.Save(database, alarmMeasurement);
                alarmMeasurement = Measurement.GetMeasurement(database, string.Format("WHERE PointTag = '{0}'", alarm.TagName));

                return alarmMeasurement.SignalID;
            }
            catch
            {
                // Return null to indicate measurement
                // was not saved to database
                return null;
            }
        }

        // Deletes a measurement whose signal ID matches the given measurement ID.
        private static void DeleteAlarmMeasurement(AdoDataConnection database, Guid measurementId)
        {
            string query = database.ParameterizedQueryString("DELETE FROM Measurement WHERE SignalID = {0}", "signalId");
            database.Connection.ExecuteNonQuery(query, database.Guid(measurementId));
        }

        #endregion
    }
}
