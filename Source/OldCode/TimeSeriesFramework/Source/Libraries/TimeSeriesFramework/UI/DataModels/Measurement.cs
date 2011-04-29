//******************************************************************************************************
//  Measurement.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
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
    /// Represents a record of measurement information as defined in the database.
    /// </summary>
    public class Measurement : DataModelBase
    {
        #region [ Members ]

        //Fields
        private string m_signalID;
        private int? m_historianID;
        private int m_pointID;
        private int? m_deviceID;
        private string m_pointTag;
        private string m_alternateTag;
        private int m_signalTypeID;
        private int? m_phasorSourceIndex;
        private string m_signalReference;
        private double m_adder;
        private double m_multiplier;
        private string m_description;
        private bool m_enabled;
        private string m_historianAcronym;
        private string m_deviceAcronym;
        private int? m_framesPerSecond;
        private string m_signalName;
        private string m_signalAcronym;
        private string m_signalSuffix;
        private string m_phasorLabel;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;
        private string m_ID;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Signal ID.
        /// </summary>
        [StringLength(36, ErrorMessage = " Measurement SignalID cannot exceed 36 characters.")]
        public string SignalID
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
        /// Gets or sets the current <see cref="Measurement"/>'s Historian ID.
        /// </summary>
        // Because of database design, no validation attributes are applied
        public int? HistorianID
        {
            get
            {
                return m_historianID;
            }
            set
            {
                m_historianID = value;
                OnPropertyChanged("HistorianID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Point ID.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied.
        public int PointID
        {
            get
            {
                return m_pointID;
            }
            set
            {
                m_pointID = value;
                OnPropertyChanged("PointID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Device ID.
        /// </summary>
        // Because of database design, no validation attributes are applied
        public int? DeviceID
        {
            get
            {
                return m_deviceID;
            }
            set
            {
                m_deviceID = value;
                OnPropertyChanged("DeviceID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Point Tag.
        /// </summary>
        [Required(ErrorMessage = "Measurement Point Tag is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "Measurement Point Tag cannot exceed 50 characters.")]
        public string PointTag
        {
            get
            {
                return m_pointTag;
            }
            set
            {
                m_pointTag = value;
                OnPropertyChanged("PointTag");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Alternate Tag.
        /// </summary>
        [StringLength(50, ErrorMessage = "Measurement alternate tag cannot exceed 50 characters.")]
        public string AlternateTag
        {
            get
            {
                return m_alternateTag;
            }
            set
            {
                m_alternateTag = value;
                OnPropertyChanged("AlternateTag");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Signal Type ID.
        /// </summary>
        [Required(ErrorMessage = "Measurement signal type ID is a required field, please provide value.")]
        public int SignalTypeID
        {
            get
            {
                return m_signalTypeID;
            }
            set
            {
                m_signalTypeID = value;
                OnPropertyChanged("SignalTypeID");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Phasor Source Index.
        /// </summary>
        // Because of database design, no validation attributes are applied
        public int? PhasorSourceIndex
        {
            get
            {
                return m_phasorSourceIndex;
            }
            set
            {
                m_phasorSourceIndex = value;
                OnPropertyChanged("PhasorSourceIndex");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Signal Reference.
        /// </summary>
        [Required(ErrorMessage = "Measurement Signal Reference is a required field, please provide value.")]
        public string SignalReference
        {
            get
            {
                return m_signalReference;
            }
            set
            {
                m_signalReference = value;
                OnPropertyChanged("SignalReference");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Adder.
        /// </summary>
        [DefaultValue(typeof(double), "0.0")]
        public double Adder
        {
            get
            {
                return m_adder;
            }
            set
            {
                m_adder = value;
                OnPropertyChanged("Adder");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Multiplier.
        /// </summary>
        [DefaultValue(typeof(double), "1.0")]
        public double Multiplier
        {
            get
            {
                return m_multiplier;
            }
            set
            {
                m_multiplier = value;
                OnPropertyChanged("Multiplier");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Description.
        /// </summary>
        // Because of database design, no validation attributes are applied
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
        /// Gets or sets whether the current <see cref="Measurement"/> is enabled.
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
        /// Gets or sets the current <see cref="Measurement"/>'s Historian Acronym.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string HistorianAcronym
        {
            get
            {
                return m_historianAcronym;
            }
            set
            {
                m_historianAcronym = value;
                OnPropertyChanged("HistorianAcronym");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Device Acronym.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string DeviceAcronym
        {
            get
            {
                return m_deviceAcronym;
            }
            set
            {
                m_deviceAcronym = value;
                OnPropertyChanged("DeviceAcronym");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Frames Per Second.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public int? FramesPerSecond
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
        /// Gets or sets the current <see cref="Measurement"/>'s Signal Name.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string SignalName
        {
            get
            {
                return m_signalName;
            }
            set
            {
                m_signalName = value;
                OnPropertyChanged("SignalName");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Signal Acronym.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string SignalAcronym
        {
            get
            {
                return m_signalAcronym;
            }
            set
            {
                m_signalAcronym = value;
                OnPropertyChanged("SignalAcronym");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Signal Suffix.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string SignalSuffix
        {
            get
            {
                return m_signalSuffix;
            }
            set
            {
                m_signalSuffix = value;
                OnPropertyChanged("SignalSuffix");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Phasor Label.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string PhasorLabel
        {
            get
            {
                return m_phasorLabel;
            }
            set
            {
                m_phasorLabel = value;
                OnPropertyChanged("PhasorLabel");
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Measurement"/> was Created.
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
                OnPropertyChanged("CreatedOn");
            }
        }

        /// <summary>
        /// Gets or sets who the current <see cref="Measurement"/> was created by.
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
                OnPropertyChanged("CreatedBy");
            }
        }

        /// <summary>
        /// Gets or sets when the current <see cref="Measurement"/> updated.
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
                OnPropertyChanged("UpdatedOn");
            }
        }

        /// <summary>
        /// Gets or sets who the current <see cref="Measurement"/> was updated by.
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
                OnPropertyChanged("UpdatedBy");
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Loads <see cref="Measurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();
                DataTable measurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT SignalID, HistorianID, PointID, DeviceID, PointTag, AlternateTag, SignalTypeID, PhasorSourceIndex, SignalReference, Adder, Multiplier, Description, Enabled, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy " +
                    "FROM Measurement ORDER BY PointID");

                foreach (DataRow row in measurementTable.Rows)
                {
                    measurementList.Add(new Measurement()
                    {
                        SignalID = row.Field<string>("SignalID"),
                        HistorianID = row.Field<int>("HistorianID"),
                        PointID = row.Field<int>("PointID"),
                        DeviceID = row.Field<int>("DeviceID"),
                        PointTag = row.Field<string>("PointTag"),
                        AlternateTag = row.Field<string>("AlternateTag"),
                        SignalTypeID = row.Field<int>("SignalTypeID"),
                        PhasorSourceIndex = row.Field<int>("PhasorSourceIndex"),
                        SignalReference = row.Field<string>("SignalReference"),
                        Adder = row.Field<double>("Adder"),
                        Multiplier = row.Field<double>("Multiplier"),
                        Description = row.Field<string>("Description"),
                        Enabled = row.Field<bool>("Enabled"),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        CreatedBy = row.Field<string>("CreatedBy"),
                        UpdatedOn = row.Field<DateTime>("UpdatedOn"),
                        UpdatedBy = row.Field<string>("UpdatedBy")
                    });
                }

                return measurementList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Measurement"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns>Dictionary<int, string> containing PointID and SignalID of measurements defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> measurementList = new Dictionary<int, string>();
                if (isOptional)
                    measurementList.Add(0, "Select Measurement");

                DataTable measurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT PointID, SignalID FROM Measurement ORDER BY PointID");

                foreach (DataRow row in measurementTable.Rows)
                    measurementList[row.Field<int>("PointID")] = row.Field<string>("SignalID");

                return measurementList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Measurement"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurement">Information about <see cref="Measurement"/>.</param>
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Measurement measurement, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                if (isNew)
                    database.Connection.ExecuteNonQuery("INSERT INTO Company (SignalID, HistorianID, DeviceID, PointTag, AlternateTag, SignalTypeID, PhasorSourceIndex, SignalReference, Adder, Multiplier, Description, Enabled, CreatedBy, CreatedOn) " +
                        "VALUES (@signalID, @historianID, @pointTag, @alternateTag, @signalTypeID, @phasorSourceIndex, @signalReference, @adder, @multiplier, @description, @enabled, @createdBy, @createdOn)", DefaultTimeout, measurement.SignalID, measurement.HistorianID, measurement.DeviceID, measurement.PointTag, measurement.AlternateTag,
                        measurement.SignalTypeID, measurement.PhasorSourceIndex, measurement.SignalReference, measurement.Adder, measurement.Multiplier, measurement.Description, measurement.Enabled,
                        CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow);
                else
                    database.Connection.ExecuteNonQuery("UPDATE Company SET HistorianID = @historianID, DeviceID = @deviceID, PointTag = @pointTag,  AlternateTag = @alternateTag, SignalTypeID = @signalTypeID, PhasorSourceIndex = @phasorSourceIndex, SignalReference = @signalReference, Adder = @adder, Multiplier = @multiplier, Description = @description, Enabled = @enabled" +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE SignalID = @signalID", DefaultTimeout, measurement.HistorianID, measurement.DeviceID, measurement.PointTag, measurement.AlternateTag, measurement.SignalTypeID, measurement.PhasorSourceIndex, measurement.SignalReference, measurement.Adder, measurement.Multiplier, measurement.Description, measurement.Enabled, CommonFunctions.CurrentUser,
                        database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, measurement.PointID);

                return "Measurement information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Measurement"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, string measurementID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM Measurement WHERE SignalID = @measurementID", DefaultTimeout, measurementID);

                return "Measurement deleted successfully";
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
