//******************************************************************************************************
//  CalculatedMeasurement.cs - Gbtc
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
//  04/11/2011 - Aniket Salver
//       Generated original version of source code.
//  04/21/2011 - Mehulbhai P Thakkar
//       Added static methods for database operations.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL value and Guid parameter handling for Save() operation.
//  05/12/2011 - Aniket Salver
//                  Modified the way Guid is retrived from the Data Base.
//  05/13/2011 - Mehulbhai P Thakkar
//       Added regular expression validator for Acronym.
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
    /// Represents a record of <see cref="CalculatedMeasurement"/> information as defined in the database.
    /// </summary>
    public class CalculatedMeasurement : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_nodeID;
        private int m_id;
        private string m_acronym;
        private string m_name;
        private string m_typeName;
        private string m_assemblyName;
        private string m_connectionString;
        private string m_configSection;
        private string m_outputMeasurements;
        private string m_inputMeasurements;
        private int m_minimumMeasurementsToUse;
        private int m_framesPerSecond;
        private double m_lagTime;
        private double m_leadTime;
        private bool m_useLocalClockAsRealTime;
        private bool m_allowSortsByArrival;
        private int m_loadOrder;
        private bool m_enabled;
        private bool m_ignoreBadTimeStamps;
        private int m_timeResolution;
        private bool m_allowPreemptivePublishing;
        private string m_downsamplingMethod;
        private string m_nodeName;
        private bool m_performTimestampReasonabilityCheck;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="CalculatedMeasurement"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = "Please select a node ID.")]
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> ID.
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "Calculated measurement acronym is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Calculated measurement acronym cannot exceed 200 characters.")]
        [RegularExpression("^[A-Z0-9-'!'_]+$", ErrorMessage = "Only upper case letters, numbers, '!', '-' and '_' are allowed.")]
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> Name.
        /// </summary>
        [StringLength(200, ErrorMessage = "Calculated measurement name cannot exceed 200 characters.")]
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "Calculated measurement type name is a required field, please provide value.")]
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> AssemblyName.
        /// </summary>
        [Required(ErrorMessage = "Calculated measurement assembly name is a required field, please provide value.")]
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
        /// Gets or sets <see cref="CalculatedMeasurement"/> ConnectionString.
        /// </summary>
        // Because of database design, no validation attributes are applied.
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> ConfigSection.
        /// </summary>
        [StringLength(200, ErrorMessage = "Config Section cannot exceed 200 characters.")]
        public string ConfigSection
        {
            get
            {
                return m_configSection;
            }
            set
            {
                m_configSection = value;
                OnPropertyChanged("ConfigSection");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="CalculatedMeasurement"/> OutputMeasurements.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public string OutputMeasurements
        {
            get
            {
                return m_outputMeasurements;
            }
            set
            {
                m_outputMeasurements = value;
                OnPropertyChanged("OutputMeasurements");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> InputMeasurements.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public string InputMeasurements
        {
            get
            {
                return m_inputMeasurements;
            }
            set
            {
                m_inputMeasurements = value;
                OnPropertyChanged("InputMeasurements");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> MinimumMeasurementsToUse.
        /// </summary>
        [Required(ErrorMessage = " Calculated measurement minimum measurements to use is a required field, please provide value.")]
        [DefaultValue(-1)]
        public int MinimumMeasurementsToUse
        {
            get
            {
                return m_minimumMeasurementsToUse;
            }
            set
            {
                m_minimumMeasurementsToUse = value;
                OnPropertyChanged("MinimumMeasurementsToUse");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> FramesPerSecond.
        /// </summary>
        [Required(ErrorMessage = " Calculated measurement frames per second is a required field, please provide value.")]
        [DefaultValue(30)]
        public int FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                m_framesPerSecond = value;
                OnPropertyChanged("FramesPerSecond");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> LagTime.
        /// </summary>
        [Required(ErrorMessage = "Calculated measurement lag time is a required field, please provide value.")]
        [DefaultValue(3.0D)]
        public double LagTime
        {
            get
            {
                return m_lagTime;
            }
            set
            {
                m_lagTime = value;
                OnPropertyChanged("LagTime");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> LeadTime.
        /// </summary>
        [Required(ErrorMessage = "Calculated measurement lead time is a required field, please provide value.")]
        [DefaultValue(1.0D)]
        public double LeadTime
        {
            get
            {
                return m_leadTime;
            }
            set
            {
                m_leadTime = value;
                OnPropertyChanged("LeadTime");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> UseLocalClockAsRealTime.
        /// </summary>
        [DefaultValue(false)]
        public bool UseLocalClockAsRealTime
        {
            get
            {
                return m_useLocalClockAsRealTime;
            }
            set
            {
                m_useLocalClockAsRealTime = value;
                OnPropertyChanged("UseLocalAsRealTime");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> AllowSortsByArrival.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowSortsByArrival
        {
            get
            {
                return m_allowSortsByArrival;
            }
            set
            {
                m_allowSortsByArrival = value;
                OnPropertyChanged("AllowSortsByArrival");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "Calculated measurement load order is a required field, please provide value.")]
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> Enabled
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> IgnoreBadTimeStamps
        /// </summary>
        [DefaultValue(false)]
        public bool IgnoreBadTimeStamps
        {
            get
            {
                return m_ignoreBadTimeStamps;
            }
            set
            {
                m_ignoreBadTimeStamps = value;
                OnPropertyChanged("IgnoreBadtimeStamps");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> TimeResolution
        /// </summary>
        [Required(ErrorMessage = "Calculated measurement time resoulution is a required field, please provide value.")]
        [DefaultValue(10000)]
        public int TimeResolution
        {
            get
            {
                return m_timeResolution;
            }
            set
            {
                m_timeResolution = value;
                OnPropertyChanged("TimeResolution");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> AllowPreemptivePublishing
        /// </summary>
        [DefaultValue(true)]
        public bool AllowPreemptivePublishing
        {
            get
            {
                return m_allowPreemptivePublishing;
            }
            set
            {
                m_allowPreemptivePublishing = value;
                OnPropertyChanged("AllowPreemptivePublishing");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> DownsamplingMethod
        /// </summary>
        [StringLength(15, ErrorMessage = "Calculated measurement name cannot exceed 15 characters.")]
        [DefaultValue("LastReceived")]
        public string DownsamplingMethod
        {
            get
            {
                return m_downsamplingMethod;
            }
            set
            {
                m_downsamplingMethod = value;
                OnPropertyChanged("DownsamplingMethod");
            }
        }

        /// <summary>
        ///  Gets <see cref="CalculatedMeasurement"/> NodeName
        /// </summary>
        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="CalculatedMeasurement"/> PerformTimestampReasonabilityCheck
        /// </summary>
        [DefaultValue(true)]
        public bool PerformTimestampReasonabilityCheck
        {
            get
            {
                return m_performTimestampReasonabilityCheck;
            }
            set
            {
                m_performTimestampReasonabilityCheck = value;
                OnPropertyChanged("PerformTimestampReasonabilityCheck");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="CalculatedMeasurement"/> CreatedOn
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
        /// Gets or sets <see cref="CalculatedMeasurement"/> CreatedBy
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> UpdatedOn
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
        ///  Gets or sets <see cref="CalculatedMeasurement"/> UpdatedBy
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
        /// Loads <see cref="Company"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>        
        /// <returns>Collection of <see cref="CalculatedMeasurement"/>.</returns>
        public static ObservableCollection<CalculatedMeasurement> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<CalculatedMeasurement> calculatedMeasurementList = new ObservableCollection<CalculatedMeasurement>();
                DataTable calculatedMeasurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT NodeID, ID, Acronym, Name, AssemblyName, " +
                    "TypeName, ConnectionString, ConfigSection, InputMeasurements, OutputMeasurements, MinimumMeasurementsToUse, FramesPerSecond, LagTime, " +
                    "LeadTime, UseLocalClockAsRealTime, AllowSortsByArrival, LoadOrder, Enabled, IgnoreBadTimeStamps, TimeResolution, AllowPreemptivePublishing, " +
                    "DownSamplingMethod, NodeName, PerformTimestampReasonabilityCheck From CalculatedMeasurementDetail WHERE NodeID = @nodeID ORDER BY LoadOrder",
                    DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in calculatedMeasurementTable.Rows)
                {
                    calculatedMeasurementList.Add(new CalculatedMeasurement()
                    {
                        NodeID = database.Guid(row, "NodeID"),
                        ID = row.Field<int>("ID"),
                        Acronym = row.Field<string>("Acronym"),
                        Name = row.Field<string>("Name"),
                        AssemblyName = row.Field<string>("AssemblyName"),
                        TypeName = row.Field<string>("TypeName"),
                        ConnectionString = row.Field<string>("ConnectionString"),
                        ConfigSection = row.Field<string>("ConfigSection"),
                        InputMeasurements = row.Field<string>("InputMeasurements"),
                        OutputMeasurements = row.Field<string>("OutputMeasurements"),
                        MinimumMeasurementsToUse = row.Field<int>("MinimumMeasurementsToUse"),
                        FramesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                        LagTime = row.Field<double>("LagTime"),
                        LeadTime = row.Field<double>("LeadTime"),
                        UseLocalClockAsRealTime = Convert.ToBoolean(row.Field<object>("UseLocalClockAsRealTime")),
                        AllowSortsByArrival = Convert.ToBoolean(row.Field<object>("AllowSortsByArrival")),
                        LoadOrder = row.Field<int>("LoadOrder"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        IgnoreBadTimeStamps = Convert.ToBoolean(row.Field<object>("IgnoreBadTimeStamps")),
                        TimeResolution = Convert.ToInt32(row.Field<object>("TimeResolution")),
                        AllowPreemptivePublishing = Convert.ToBoolean(row.Field<object>("AllowPreemptivePublishing")),
                        DownsamplingMethod = row.Field<string>("DownSamplingMethod"),
                        m_nodeName = row.Field<string>("NodeName"),
                        PerformTimestampReasonabilityCheck = Convert.ToBoolean(row.Field<object>("PerformTimestampReasonabilityCheck"))
                    });
                }

                return calculatedMeasurementList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Company"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of companies defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> calculatedMeasurementList = new Dictionary<int, string>();

                if (isOptional)
                    calculatedMeasurementList.Add(0, "Select CalculatedMeasurement");

                DataTable calculatedMeasurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Name FROM CalculatedMeasurement ORDER BY LoadOrder");

                foreach (DataRow row in calculatedMeasurementTable.Rows)
                    calculatedMeasurementList[row.Field<int>("ID")] = row.Field<string>("Name");

                return calculatedMeasurementList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Company"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="calculatedMeasurement">Information about <see cref="CalculatedMeasurement"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, CalculatedMeasurement calculatedMeasurement)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (calculatedMeasurement.ID == 0)
                    database.Connection.ExecuteNonQuery("INSERT INTO CalculatedMeasurement (NodeID, Acronym, Name, AssemblyName, TypeName, ConnectionString, " +
                        "ConfigSection, InputMeasurements, OutputMeasurements, MinimumMeasurementsToUse, FramesPerSecond, LagTime, LeadTime, UseLocalClockAsRealTime, " +
                        "AllowSortsByArrival, LoadOrder, Enabled, IgnoreBadTimeStamps, TimeResolution, AllowPreemptivePublishing, DownsamplingMethod, " +
                        "PerformTimestampReasonabilityCheck, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@nodeID, @acronym, @name, @assemblyName, " +
                        "@typeName, @connectionString, @configSection, @inputMeasurements, @outputMeasurements, @minimumMeasurementsToUse, @framesPerSecond, " +
                        "@lagTime, @leadTime, @useLocalClockAsRealTime, @allowSortsByArrival, @loadOrder, @enabled, @ignoreBadTimeStamps, @timeResolution, " +
                        "@allowPreemptivePublishing, @downsamplingMethod, @performTimestampReasonabilityCheck, @updatedBy, @updatedOn, @createdBy, @createdOn)",
                        DefaultTimeout, database.Guid(calculatedMeasurement.NodeID), calculatedMeasurement.Acronym.Replace(" ", "").ToUpper(), calculatedMeasurement.Name.ToNotNull(),
                        calculatedMeasurement.AssemblyName, calculatedMeasurement.TypeName, calculatedMeasurement.ConnectionString.ToNotNull(), calculatedMeasurement.ConfigSection,
                        calculatedMeasurement.InputMeasurements.ToNotNull(), calculatedMeasurement.OutputMeasurements.ToNotNull(), calculatedMeasurement.MinimumMeasurementsToUse,
                        calculatedMeasurement.FramesPerSecond, calculatedMeasurement.LagTime, calculatedMeasurement.LeadTime, calculatedMeasurement.UseLocalClockAsRealTime,
                        calculatedMeasurement.AllowSortsByArrival, calculatedMeasurement.LoadOrder, calculatedMeasurement.Enabled, calculatedMeasurement.IgnoreBadTimeStamps,
                        calculatedMeasurement.TimeResolution, calculatedMeasurement.AllowPreemptivePublishing, calculatedMeasurement.DownsamplingMethod,
                        calculatedMeasurement.PerformTimestampReasonabilityCheck, CommonFunctions.CurrentUser, database.UtcNow(),
                        CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE CalculatedMeasurement SET NodeID = @nodeID, Acronym = @acronym, Name = @name, AssemblyName = @assemblyName, " +
                        "TypeName = @typeName, ConnectionString = @connectionString, ConfigSection = @configSection, InputMeasurements = @inputMeasurements, " +
                        "OutputMeasurements = @outputMeasurements, MinimumMeasurementsToUse = @minimumMeasurementsToUse, FramesPerSecond = @framesPerSecond, " +
                        "LagTime = @lagTime, LeadTime = @leadTime, UseLocalClockAsRealTime = @useLocalClockAsRealTime, AllowSortsByArrival = @allowSortsByArrival, " +
                        "LoadOrder = @loadOrder, Enabled = @enabled, IgnoreBadTimeStamps = @ignoreBadTimeStamps, TimeResolution = @timeResolution, AllowPreemptivePublishing " +
                        "= @allowPreemptivePublishing, DownsamplingMethod = @downsamplingMethod, PerformTimestampReasonabilityCheck = @performTimestampReasonabilityCheck, " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, database.Guid(calculatedMeasurement.NodeID),
                        calculatedMeasurement.Acronym.Replace(" ", "").ToUpper(), calculatedMeasurement.Name.ToNotNull(), calculatedMeasurement.AssemblyName,
                        calculatedMeasurement.TypeName, calculatedMeasurement.ConnectionString.ToNotNull(), calculatedMeasurement.ConfigSection, calculatedMeasurement.InputMeasurements.ToNotNull(),
                        calculatedMeasurement.OutputMeasurements.ToNotNull(), calculatedMeasurement.MinimumMeasurementsToUse, calculatedMeasurement.FramesPerSecond,
                        calculatedMeasurement.LagTime, calculatedMeasurement.LeadTime, calculatedMeasurement.UseLocalClockAsRealTime, calculatedMeasurement.AllowSortsByArrival,
                        calculatedMeasurement.LoadOrder, calculatedMeasurement.Enabled, calculatedMeasurement.IgnoreBadTimeStamps, calculatedMeasurement.TimeResolution,
                        calculatedMeasurement.AllowPreemptivePublishing, calculatedMeasurement.DownsamplingMethod, calculatedMeasurement.PerformTimestampReasonabilityCheck,
                        CommonFunctions.CurrentUser, database.UtcNow(), calculatedMeasurement.ID);

                return "Calculated measurement information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Company"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="calculatedMeasurementID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int calculatedMeasurementID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM CalculatedMeasurement WHERE ID = @calculatedMeasurementID", DefaultTimeout, calculatedMeasurementID);

                return "Calculated measurement deleted successfully";
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
