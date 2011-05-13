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
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL handling for Save() operation.
//  05/13/2011 - Aniket Salver
//                  Modified the way Guid is retrived from the Data Base.
//  05/13/2011 - Mehulbhai P Thakkar
//       Modified static methods to filter data by device.
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
    /// Represents a record of <see cref="Measurement"/> information as defined in the database.
    /// </summary>
    public class Measurement : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_signalID;
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
        private bool m_subscribed;
        private bool m_internal;
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
        private string m_id;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Signal ID.
        /// </summary>
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
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Device ID.
        /// </summary>
        // Because of database design, no validation attributes are applied.
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
        [Required(ErrorMessage = "Measurement point tag is a required field, please provide value.")]
        [StringLength(200, ErrorMessage = "Measurement point tag cannot exceed 200 characters.")]
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
        [StringLength(200, ErrorMessage = "Measurement alternate tag cannot exceed 200 characters.")]
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
        [Required(ErrorMessage = "Measurement signal reference is a required field, please provide value.")]
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
        [DefaultValue(false)]
        public bool Subscribed
        {
            get
            {
                return m_subscribed;
            }
            set
            {
                m_subscribed = value;
                OnPropertyChanged("Subscribed");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Measurement"/> is enabled.
        /// </summary>
        [DefaultValue(false)]
        public bool Internal
        {
            get
            {
                return m_internal;
            }
            set
            {
                m_internal = value;
                OnPropertyChanged("Internal");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="Measurement"/> is enabled.
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
        /// Gets the current <see cref="Measurement"/>'s Historian Acronym.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string HistorianAcronym
        {
            get
            {
                return m_historianAcronym;
            }
        }

        /// <summary>
        /// Gets the current <see cref="Measurement"/>'s Device Acronym.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string DeviceAcronym
        {
            get
            {
                return m_deviceAcronym;
            }
        }

        /// <summary>
        /// Gets the current <see cref="Measurement"/>'s Frames Per Second.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public int? FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
        }

        /// <summary>
        /// Gets the current <see cref="Measurement"/>'s Signal Name.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string SignalName
        {
            get
            {
                return m_signalName;
            }
        }

        /// <summary>
        /// Gets the current <see cref="Measurement"/>'s Signal Acronym.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string SignalAcronym
        {
            get
            {
                return m_signalAcronym;
            }
        }

        /// <summary>
        /// Gets the current <see cref="Measurement"/>'s Signal Suffix.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string SignalSuffix
        {
            get
            {
                return m_signalSuffix;
            }
        }

        /// <summary>
        /// Gets the current <see cref="Measurement"/>'s Phasor Label.
        /// </summary>
        // Because of database design, no validation attributes are supplied
        public string PhasorLabel
        {
            get
            {
                return m_phasorLabel;
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
            }
        }

        /// <summary>
        /// Gets ID of the <see cref="Measurement"/>.
        /// </summary>
        // Field is populated via view, so no validation attributes are applied.
        public string ID
        {
            get
            {
                return m_id;
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Loads <see cref="Measurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the <see cref="Device"/> to filter data.</param>
        /// <returns>Collection of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> Load(AdoDataConnection database, int deviceID = 0)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();
                DataTable measurementTable;

                if (deviceID > 0)
                    measurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM MeasurementDetail WHERE " +
                        "DeviceID = @deviceID ORDER BY PointID", DefaultTimeout, deviceID);
                else
                    measurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM MeasurementDetail WHERE " +
                        "NodeID = @nodeID ORDER BY PointTag", DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in measurementTable.Rows)
                {
                    measurementList.Add(new Measurement()
                    {
                        SignalID = database.Guid(row, "SignalID"),
                        HistorianID = row.Field<int?>("HistorianID"),
                        PointID = row.Field<int>("PointID"),
                        DeviceID = row.Field<int?>("DeviceID"),
                        PointTag = row.Field<string>("PointTag"),
                        AlternateTag = row.Field<string>("AlternateTag"),
                        SignalTypeID = row.Field<int>("SignalTypeID"),
                        PhasorSourceIndex = row.Field<int?>("PhasorSourceIndex"),
                        SignalReference = row.Field<string>("SignalReference"),
                        Adder = row.Field<double>("Adder"),
                        Multiplier = row.Field<double>("Multiplier"),
                        Description = row.Field<string>("Description"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_historianAcronym = row.Field<string>("HistorianAcronym"),
                        m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                        m_signalName = row.Field<string>("SignalName"),
                        m_signalAcronym = row.Field<string>("SignalAcronym"),
                        m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                        m_phasorLabel = row.Field<string>("PhasorLabel"),
                        m_framesPerSecond = row.Field<int?>("FramesPerSecond"),
                        m_id = row.Field<string>("ID")
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
        /// <returns><see cref="Dictionary{T1,T2}"/> containing PointID and SignalID of measurements defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> measurementList = new Dictionary<int, string>();
                if (isOptional)
                    measurementList.Add(0, "Select Measurement");

                DataTable measurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT PointID, PointTag FROM Measurement ORDER BY PointID");

                foreach (DataRow row in measurementTable.Rows)
                    measurementList[row.Field<int>("PointID")] = row.Field<string>("PointTag");

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
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Measurement measurement)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (measurement.PointID == 0)
                    database.Connection.ExecuteNonQuery("INSERT INTO Measurement (HistorianID, DeviceID, PointTag, AlternateTag, SignalTypeID, PhasorSourceIndex, " +
                        "SignalReference, Adder, Multiplier, Subscribed, Internal, Description, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES (@historianID, @deviceID, " +
                        "@pointTag, @alternateTag, @signalTypeID, @phasorSourceIndex, @signalReference, @adder, @multiplier, @description, @enabled, @updatedBy, " +
                        "@updatedOn, @createdBy, @createdOn)", DefaultTimeout, measurement.HistorianID.ToNotNull(), measurement.DeviceID.ToNotNull(), measurement.PointTag,
                        measurement.AlternateTag.ToNotNull(), measurement.SignalTypeID, measurement.PhasorSourceIndex.ToNotNull(), measurement.SignalReference,
                        measurement.Adder, measurement.Multiplier, measurement.Description.ToNotNull(), measurement.Subscribed, measurement.Internal, measurement.Enabled, CommonFunctions.CurrentUser,
                        database.UtcNow(), CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("Update Measurement Set HistorianID = @historianID, DeviceID = @deviceID, PointTag = @pointTag, " +
                        "AlternateTag = @alternateTag, SignalTypeID = @signalTypeID, PhasorSourceIndex = @phasorSourceIndex, SignalReference = @signalReference, " +
                        "Adder = @adder, Multiplier = @multiplier, Description = @description, Subscribed = @subscribed, Internal = @internal, Enabled = @enabled, UpdatedBy = @updatedBy, UpdatedOn = @updatedOn " +
                        "Where PointID = @pointID", DefaultTimeout, measurement.HistorianID.ToNotNull(), measurement.DeviceID.ToNotNull(), measurement.PointTag,
                        measurement.AlternateTag.ToNotNull(), measurement.SignalTypeID, measurement.PhasorSourceIndex.ToNotNull(), measurement.SignalReference,
                        measurement.Adder, measurement.Multiplier, measurement.Description.ToNotNull(), measurement.Subscribed, measurement.Internal, measurement.Enabled, CommonFunctions.CurrentUser, database.UtcNow(),
                        measurement.PointID);

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
        /// <param name="pointID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int pointID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM Measurement WHERE PointID = @pointID", DefaultTimeout, pointID);

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
