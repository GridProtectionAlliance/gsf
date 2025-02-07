//******************************************************************************************************
//  Measurement.cs - Gbtc
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
//  04/08/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//  05/02/2011 - J. Ritchie Carroll
//       Updated for coding consistency.
//  05/03/2011 - Mehulbhai P Thakkar
//       Guid field related changes as well as static functions update.
//  05/05/2011 - Mehulbhai P Thakkar
//       Added NULL handling for Save() operation.
//  05/13/2011 - Aniket Salver
//       Modified the way Guid is retrieved from the Data Base.
//  05/13/2011 - Mehulbhai P Thakkar
//       Modified static methods to filter data by device.
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
using GSF.Data;

namespace GSF.TimeSeries.UI.DataModels
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
        private string m_companyAcronym;
        private string m_companyName;
        private bool m_selected;  //This is added for the SelectMeasurement user control to provide check boxes.       

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Measurement"/> class.
        /// </summary>
        public Measurement()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Measurement"/> class.
        /// </summary>
        /// <param name="loadDefaults">
        /// Determines whether to load default values into the properties using reflection.
        /// </param>
        public Measurement(bool loadDefaults)
            : base(loadDefaults, false)
        {
        }

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
                if ((object)value != null && value.Length > 200)
                    m_pointTag = value.Substring(0, 200);
                else
                    m_pointTag = value;
                OnPropertyChanged("PointTag");
            }
        }

        /// <summary>
        /// Gets or sets the current <see cref="Measurement"/>'s Alternate Tag.
        /// </summary>        
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
        [DefaultValue(true)]
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

        /// <summary>
        /// Gets <see cref="Measurement"/> company acronym.
        /// </summary>
        // Field is populated via view, so no validation attributes are applied.
        public string CompanyAcronym
        {
            get
            {
                return m_companyAcronym;
            }
        }

        /// <summary>
        /// Gets <see cref="Measurement"/> company name.
        /// </summary>
        // Field is populated via view, so no validation attributes are applied.
        public string CompanyName
        {
            get
            {
                return m_companyName;
            }
        }

        /// <summary>
        /// Gets or sets selected flag for <see cref="Measurement"/>.
        /// </summary>
        public bool Selected
        {
            get
            {
                return m_selected;
            }
            set
            {
                m_selected = value;
                OnPropertyChanged("Selected");
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Loads <see cref="Measurement"/> signal IDs as an <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="filterExpression">SQL expression by which to filter the data coming from the database.</param>
        /// <param name="sortMember">The field to sort by.</param>
        /// <param name="sortDirection"><c>ASC</c> or <c>DESC</c> for ascending or descending respectively.</param>
        /// <returns>Collection of <see cref="Guid"/>.</returns>
        /// <remarks>
        /// This method does not validate <paramref name="filterExpression"/> for SQL injection.
        /// Developers should validate their inputs before entering a filter expression.
        /// </remarks>
        public static List<Guid> LoadSignalIDs(AdoDataConnection database, string filterExpression = "", string sortMember = "", string sortDirection = "")
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                List<Guid> signalIDList = new List<Guid>();
                DataTable measurementTable;

                string query;
                string sortClause = string.Empty;

                if (!string.IsNullOrEmpty(sortMember))
                    sortClause = string.Format("ORDER BY {0} {1}", sortMember, sortDirection);

                if (!string.IsNullOrEmpty(filterExpression))
                    query = string.Format("SELECT SignalID FROM MeasurementDetail WHERE ({0}) {1}", filterExpression, sortClause);
                else
                    query = string.Format("SELECT SignalID FROM MeasurementDetail {0}", sortClause);

                measurementTable = database.Connection.RetrieveData(database.AdapterType, query);

                foreach (DataRow row in measurementTable.Rows)
                {
                    signalIDList.Add(database.Guid(row, "SignalID"));
                }

                return signalIDList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="Measurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="keys">Keys of the measurements to be loaded from the database.</param>
        /// <returns>Collection of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> LoadFromKeys(AdoDataConnection database, List<Guid> keys)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                string query;
                string commaSeparatedKeys;

                Measurement[] measurementList = null;
                DataTable measurementTable;
                Guid signalID;

                if ((object)keys != null && keys.Count > 0)
                {
                    commaSeparatedKeys = keys.Select(key => "'" + key.ToString() + "'").Aggregate((str1, str2) => str1 + "," + str2);
                    query = string.Format("SELECT * FROM MeasurementDetail WHERE SignalID IN ({0})", commaSeparatedKeys);
                    measurementTable = database.Connection.RetrieveData(database.AdapterType, query);
                    measurementList = new Measurement[measurementTable.Rows.Count];

                    foreach (DataRow row in measurementTable.Rows)
                    {
                        signalID = database.Guid(row, "SignalID");

                        measurementList[keys.IndexOf(signalID)] = new Measurement(false)
                        {
                            SignalID = signalID,
                            HistorianID = row.ConvertNullableField<int>("HistorianID"),
                            PointID = row.ConvertField<int>("PointID"),
                            DeviceID = row.ConvertNullableField<int>("DeviceID"),
                            PointTag = row.Field<string>("PointTag"),
                            AlternateTag = row.Field<string>("AlternateTag"),
                            SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                            PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                            SignalReference = row.Field<string>("SignalReference"),
                            Adder = row.ConvertField<double>("Adder"),
                            Multiplier = row.ConvertField<double>("Multiplier"),
                            Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                            Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                            Description = row.Field<string>("Description"),
                            Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                            m_historianAcronym = row.Field<string>("HistorianAcronym"),
                            m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                            m_signalName = row.Field<string>("SignalName"),
                            m_signalAcronym = row.Field<string>("SignalAcronym"),
                            m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                            m_phasorLabel = row.Field<string>("PhasorLabel"),
                            m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                            m_id = row.Field<string>("ID"),
                            m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                            m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                            Selected = false
                        };
                    }
                }

                return new ObservableCollection<Measurement>(measurementList ?? new Measurement[0]);
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads <see cref="Measurement"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the Device to filter data.</param>
        /// <param name="filterByInternalFlag">boolean flag to indicate if only non internal data requested.</param>
        /// <param name="includeInternal">boolean flag to indicate if internal measurements are included.</param>
        /// <returns>Collection of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> Load(AdoDataConnection database, int deviceID = 0, bool filterByInternalFlag = false, bool includeInternal = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();
                DataTable measurementTable;
                string query;

                if (filterByInternalFlag)
                {
                    if (deviceID > 0)
                    {
                        if (includeInternal)
                        {
                            query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE DeviceID = {0} AND " +
                                "Subscribed = {1} ORDER BY PointID", "deviceID", "subscribed");

                            measurementTable = database.Connection.RetrieveData(database.AdapterType, query,
                                DefaultTimeout, deviceID, database.Bool(false));
                        }
                        else
                        {
                            query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE DeviceID = {0} AND Internal = {1} AND " +
                                "Subscribed = {2} ORDER BY PointID", "deviceID", "internal", "subscribed");

                            measurementTable = database.Connection.RetrieveData(database.AdapterType, query,
                                DefaultTimeout, deviceID, database.Bool(false), database.Bool(false));
                        }
                    }
                    else
                    {
                        if (includeInternal)
                        {
                            query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE " +
                                "Subscribed = {0} ORDER BY PointTag", "subscribed");

                            measurementTable = database.Connection.RetrieveData(database.AdapterType, query,
                                DefaultTimeout, database.Bool(false));
                        }
                        else
                        {
                            query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE Internal = {0} AND " +
                                "Subscribed = {1} ORDER BY PointTag", "internal", "subscribed");

                            measurementTable = database.Connection.RetrieveData(database.AdapterType, query,
                                DefaultTimeout, database.Bool(false), database.Bool(false));
                        }
                    }
                }
                else
                {
                    if (deviceID > 0)
                    {
                        query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE DeviceID = {0} ORDER BY PointID", "deviceID");
                        measurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, deviceID);
                    }
                    else
                    {
                        query = "SELECT * FROM MeasurementDetail ORDER BY PointTag";
                        measurementTable = database.Connection.RetrieveData(database.AdapterType, query);
                    }
                }

                foreach (DataRow row in measurementTable.Rows)
                {
                    measurementList.Add(new Measurement(false)
                    {
                        SignalID = database.Guid(row, "SignalID"),
                        HistorianID = row.ConvertNullableField<int>("HistorianID"),
                        PointID = row.ConvertField<int>("PointID"),
                        DeviceID = row.ConvertNullableField<int>("DeviceID"),
                        PointTag = row.Field<string>("PointTag"),
                        AlternateTag = row.Field<string>("AlternateTag"),
                        SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                        PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                        SignalReference = row.Field<string>("SignalReference"),
                        Adder = row.ConvertField<double>("Adder"),
                        Multiplier = row.ConvertField<double>("Multiplier"),
                        Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                        Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                        Description = row.Field<string>("Description"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_historianAcronym = row.Field<string>("HistorianAcronym"),
                        m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                        m_signalName = row.Field<string>("SignalName"),
                        m_signalAcronym = row.Field<string>("SignalAcronym"),
                        m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                        m_phasorLabel = row.Field<string>("PhasorLabel"),
                        m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                        m_id = row.Field<string>("ID"),
                        m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                        m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                        Selected = false
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
        /// Loads information about <see cref="Measurement"/> assigned to MeasurementGroup as <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurementGroupID">ID of the MeasurementGroup to filter data.</param>
        /// <returns>Collection of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> GetMeasurementsByGroup(AdoDataConnection database, int measurementGroupID)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                if (measurementGroupID == 0)
                    return Load(database);

                ObservableCollection<Measurement> possibleMeasurements = new ObservableCollection<Measurement>();

                string query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE SignalID NOT IN " +
                    "(SELECT SignalID FROM MeasurementGroupMeasurement WHERE MeasurementGroupID = {0}) ORDER BY PointTag", "measurementGroupID");

                DataTable possibleMeasurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, measurementGroupID);

                foreach (DataRow row in possibleMeasurementTable.Rows)
                {
                    possibleMeasurements.Add(new Measurement
                    {
                        SignalID = database.Guid(row, "SignalID"),
                        HistorianID = row.ConvertNullableField<int>("HistorianID"),
                        PointID = row.ConvertField<int>("PointID"),
                        DeviceID = row.ConvertNullableField<int>("DeviceID"),
                        PointTag = row.Field<string>("PointTag"),
                        AlternateTag = row.Field<string>("AlternateTag"),
                        SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                        PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                        SignalReference = row.Field<string>("SignalReference"),
                        Adder = row.ConvertField<double>("Adder"),
                        Multiplier = row.ConvertField<double>("Multiplier"),
                        Description = row.Field<string>("Description"),
                        Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                        Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_historianAcronym = row.Field<string>("HistorianAcronym"),
                        m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                        m_signalName = row.Field<string>("SignalName"),
                        m_signalAcronym = row.Field<string>("SignalAcronym"),
                        m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                        m_phasorLabel = row.Field<string>("PhasorLabel"),
                        m_id = row.Field<string>("ID"),
                        m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                        m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                        Selected = false
                    });
                }

                return possibleMeasurements;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Loads information about <see cref="Measurement"/> assigned to Subscriber as <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="subscriberID">ID of the Subscriber to filter data.</param>
        /// <returns>Collection of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> GetMeasurementsBySubscriber(AdoDataConnection database, Guid subscriberID)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                if (subscriberID == Guid.Empty)
                    return Load(database);

                ObservableCollection<Measurement> possibleMeasurements = new ObservableCollection<Measurement>();

                string query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE SignalID NOT IN " +
                    "(SELECT SignalID FROM SubscriberMeasurement WHERE SubscriberID = {0}) ORDER BY PointTag", "subscriberID");

                DataTable possibleMeasurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.Guid(subscriberID));

                foreach (DataRow row in possibleMeasurementTable.Rows)
                {
                    possibleMeasurements.Add(new Measurement
                    {
                        SignalID = database.Guid(row, "SignalID"),
                        HistorianID = row.ConvertNullableField<int>("HistorianID"),
                        PointID = row.ConvertField<int>("PointID"),
                        DeviceID = row.ConvertNullableField<int>("DeviceID"),
                        PointTag = row.Field<string>("PointTag"),
                        AlternateTag = row.Field<string>("AlternateTag"),
                        SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                        PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                        SignalReference = row.Field<string>("SignalReference"),
                        Adder = row.ConvertField<double>("Adder"),
                        Multiplier = row.ConvertField<double>("Multiplier"),
                        Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                        Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                        Description = row.Field<string>("Description"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_historianAcronym = row.Field<string>("HistorianAcronym"),
                        m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                        m_signalName = row.Field<string>("SignalName"),
                        m_signalAcronym = row.Field<string>("SignalAcronym"),
                        m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                        m_phasorLabel = row.Field<string>("PhasorLabel"),
                        m_id = row.Field<string>("ID"),
                        m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                        m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                        Selected = false
                    });
                }

                return possibleMeasurements;
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
        /// <param name="subscribedOnly">boolean flag to indicate if only subscribed measurements to be returned.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> containing PointID and SignalID of measurements defined in the database.</returns>
        public static Dictionary<Guid, string> GetLookupList(AdoDataConnection database, bool isOptional = false, bool subscribedOnly = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<Guid, string> measurementList = new Dictionary<Guid, string>();
                if (isOptional)
                    measurementList.Add(Guid.Empty, "Select Measurement");

                DataTable measurementTable;
                string query;

                if (subscribedOnly)
                {
                    // If subscribedOnly is set then return only those measurements which are not internal and subscribed flag is set to true.
                    query = database.ParameterizedQueryString("SELECT SignalID, PointTag FROM MeasurementDetail WHERE " +
                        "NodeID = {0} AND Internal = {1} AND Subscribed = {2} ORDER BY PointID", "nodeID", "internal", "subscribed");

                    measurementTable = database.Connection.RetrieveData(database.AdapterType, query,
                        DefaultTimeout, database.CurrentNodeID(), database.Bool(false), database.Bool(true));
                }
                else
                {
                    query = database.ParameterizedQueryString("SELECT SignalID, PointTag FROM MeasurementDetail WHERE NodeID = {0} ORDER BY PointID", "nodeID");
                    measurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());
                }

                foreach (DataRow row in measurementTable.Rows)
                    measurementList[database.Guid(row, "SignalID")] = row.Field<string>("PointTag");

                return measurementList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves only subscribed <see cref="Measurement"/> collection.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns><see cref="ObservableCollection{T1}"/> type list of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> GetSubscribedMeasurements(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();
                DataTable measurementTable;
                string query;

                query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE " +
                    "Subscribed = {0} ORDER BY PointTag", "subscribed");

                measurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.Bool(true));

                foreach (DataRow row in measurementTable.Rows)
                {
                    measurementList.Add(new Measurement
                    {
                        SignalID = database.Guid(row, "SignalID"),
                        HistorianID = row.ConvertNullableField<int>("HistorianID"),
                        PointID = row.ConvertField<int>("PointID"),
                        DeviceID = row.ConvertNullableField<int>("DeviceID"),
                        PointTag = row.Field<string>("PointTag"),
                        AlternateTag = row.Field<string>("AlternateTag"),
                        SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                        PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                        SignalReference = row.Field<string>("SignalReference"),
                        Adder = row.ConvertField<double>("Adder"),
                        Multiplier = row.ConvertField<double>("Multiplier"),
                        Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                        Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                        Description = row.Field<string>("Description"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_historianAcronym = row.Field<string>("HistorianAcronym"),
                        m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                        m_signalName = row.Field<string>("SignalName"),
                        m_signalAcronym = row.Field<string>("SignalAcronym"),
                        m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                        m_phasorLabel = row.Field<string>("PhasorLabel"),
                        m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                        m_id = row.Field<string>("ID"),
                        m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                        m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                        Selected = false
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
        /// Saves <see cref="Measurement"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="measurement">Information about <see cref="Measurement"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Measurement measurement)
        {
            bool createdConnection = false;
            string query;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (measurement.PointID == 0)
                {
                    query = database.ParameterizedQueryString("INSERT INTO Measurement (HistorianID, DeviceID, PointTag, AlternateTag, SignalTypeID, PhasorSourceIndex, " +
                        "SignalReference, Adder, Multiplier, Subscribed, Internal, Description, Enabled, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) VALUES ({0}, {1}, {2}, " +
                        "{3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16})", "historianID", "deviceID", "pointTag", "alternateTag", "signalTypeID",
                        "phasorSourceIndex", "signalReference", "adder", "multiplier", "subscribed", "internal", "description", "enabled", "updatedBy", "updatedOn",
                        "createdBy", "createdOn");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, measurement.HistorianID.ToNotNull(), measurement.DeviceID.ToNotNull(), measurement.PointTag,
                        measurement.AlternateTag.ToNotNull(), measurement.SignalTypeID, measurement.PhasorSourceIndex ?? measurement.PhasorSourceIndex.ToNotNull(), measurement.SignalReference,
                        measurement.Adder, measurement.Multiplier, database.Bool(measurement.Subscribed), database.Bool(measurement.Internal), measurement.Description.ToNotNull(),
                        database.Bool(measurement.Enabled), CommonFunctions.CurrentUser, database.UtcNow, CommonFunctions.CurrentUser, database.UtcNow);
                }
                else
                {
                    query = database.ParameterizedQueryString("UPDATE Measurement SET HistorianID = {0}, DeviceID = {1}, PointTag = {2}, AlternateTag = {3}, " +
                        "SignalTypeID = {4}, PhasorSourceIndex = {5}, SignalReference = {6}, Adder = {7}, Multiplier = {8}, Description = {9}, Subscribed = {10}, " +
                        "Internal = {11}, Enabled = {12}, UpdatedBy = {13}, UpdatedOn = {14} WHERE PointID = {15}", "historianID", "deviceID", "pointTag",
                        "alternateTag", "signalTypeID", "phasorSourceINdex", "signalReference", "adder", "multiplier", "description", "subscribed", "internal",
                        "enabled", "updatedBy", "updatedOn", "pointID");

                    database.Connection.ExecuteNonQuery(query, DefaultTimeout, measurement.HistorianID.ToNotNull(), measurement.DeviceID.ToNotNull(), measurement.PointTag,
                        measurement.AlternateTag.ToNotNull(), measurement.SignalTypeID, measurement.PhasorSourceIndex ?? measurement.PhasorSourceIndex.ToNotNull(), measurement.SignalReference,
                        measurement.Adder, measurement.Multiplier, measurement.Description.ToNotNull(), database.Bool(measurement.Subscribed), database.Bool(measurement.Internal),
                        database.Bool(measurement.Enabled), CommonFunctions.CurrentUser, database.UtcNow, measurement.PointID);
                }

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
        /// <param name="signalID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, Guid signalID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM Measurement WHERE SignalID = {0}", "signalID"), DefaultTimeout, database.Guid(signalID));

                return "Measurement deleted successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Measurement"/> information from the database based on the signal ID of the measurement.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="signalID">Signal ID of the measurement.</param>
        /// <returns><see cref="Measurement"/> information.</returns>
        public static Measurement GetMeasurement(AdoDataConnection database, Guid signalID)
        {
            bool createdConnection = false;
            DataTable measurementTable;
            DataRow row;

            try
            {
                createdConnection = CreateConnection(ref database);
                measurementTable = database.RetrieveData(DefaultTimeout, "SELECT * FROM MeasurementDetail WHERE SignalID = {0}", signalID);

                if (measurementTable.Rows.Count == 0)
                    return null;

                row = measurementTable.Rows[0];

                Measurement measurement = new Measurement()
                {
                    SignalID = database.Guid(row, "SignalID"),
                    HistorianID = row.ConvertNullableField<int>("HistorianID"),
                    PointID = row.ConvertField<int>("PointID"),
                    DeviceID = row.ConvertNullableField<int>("DeviceID"),
                    PointTag = row.Field<string>("PointTag"),
                    AlternateTag = row.Field<string>("AlternateTag"),
                    SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                    PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                    SignalReference = row.Field<string>("SignalReference"),
                    Adder = row.ConvertField<double>("Adder"),
                    Multiplier = row.ConvertField<double>("Multiplier"),
                    Description = row.Field<string>("Description"),
                    Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                    m_historianAcronym = row.Field<string>("HistorianAcronym"),
                    m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                    m_signalName = row.Field<string>("SignalName"),
                    m_signalAcronym = row.Field<string>("SignalAcronym"),
                    m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                    m_phasorLabel = row.Field<string>("PhasorLabel"),
                    m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                    m_id = row.Field<string>("ID"),
                    m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                    m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                    Selected = false
                };

                return measurement;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Measurement"/> information from the database based on query string filter.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="whereClause"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns><see cref="Measurement"/> information.</returns>
        public static Measurement GetMeasurement(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;
            DataTable measurementTable;
            DataRow row;

            try
            {
                createdConnection = CreateConnection(ref database);
                measurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM MeasurementDetail " + whereClause);

                if (measurementTable.Rows.Count == 0)
                    return null;

                row = measurementTable.Rows[0];

                Measurement measurement = new Measurement
                {
                    SignalID = database.Guid(row, "SignalID"),
                    HistorianID = row.ConvertNullableField<int>("HistorianID"),
                    PointID = row.ConvertField<int>("PointID"),
                    DeviceID = row.ConvertNullableField<int>("DeviceID"),
                    PointTag = row.Field<string>("PointTag"),
                    AlternateTag = row.Field<string>("AlternateTag"),
                    SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                    PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                    SignalReference = row.Field<string>("SignalReference"),
                    Adder = row.ConvertField<double>("Adder"),
                    Multiplier = row.ConvertField<double>("Multiplier"),
                    Description = row.Field<string>("Description"),
                    Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                    m_historianAcronym = row.Field<string>("HistorianAcronym"),
                    m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                    m_signalName = row.Field<string>("SignalName"),
                    m_signalAcronym = row.Field<string>("SignalAcronym"),
                    m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                    m_phasorLabel = row.Field<string>("PhasorLabel"),
                    m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                    m_id = row.Field<string>("ID"),
                    m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                    m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                    Selected = false
                };

                return measurement;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Retrieves <see cref="ObservableCollection{T}"/> of <see cref="Measurement"/> based on the whereClause.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="whereClause"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns><see cref="ObservableCollection{T}"/> type collection of <see cref="Measurement"/>.</returns>
        public static ObservableCollection<Measurement> GetMeasurements(AdoDataConnection database, string whereClause)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();
                DataTable measurementTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM MeasurementDetail " + whereClause);
                foreach (DataRow row in measurementTable.Rows)
                {
                    measurementList.Add(new Measurement
                    {
                        SignalID = database.Guid(row, "SignalID"),
                        HistorianID = row.ConvertNullableField<int>("HistorianID"),
                        PointID = row.ConvertField<int>("PointID"),
                        DeviceID = row.ConvertNullableField<int>("DeviceID"),
                        PointTag = row.Field<string>("PointTag"),
                        AlternateTag = row.Field<string>("AlternateTag"),
                        SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                        PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                        SignalReference = row.Field<string>("SignalReference"),
                        Adder = row.ConvertField<double>("Adder"),
                        Multiplier = row.ConvertField<double>("Multiplier"),
                        Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                        Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                        Description = row.Field<string>("Description"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        m_historianAcronym = row.Field<string>("HistorianAcronym"),
                        m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                        m_signalName = row.Field<string>("SignalName"),
                        m_signalAcronym = row.Field<string>("SignalAcronym"),
                        m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                        m_phasorLabel = row.Field<string>("PhasorLabel"),
                        m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                        m_id = row.Field<string>("ID"),
                        m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                        m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                        Selected = false
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
        /// Retrieves unassigned measurements for output stream.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamID">ID of the output stream to filter data.</param>
        /// <returns><see cref="ObservableCollection{T}"/> type collection of Measurements.</returns>
        public static ObservableCollection<Measurement> GetNewOutputStreamMeasurements(AdoDataConnection database, int outputStreamID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();
                string query = database.ParameterizedQueryString("SELECT * FROM MeasurementDetail WHERE NodeID = {0} AND PointID NOT IN (SELECT PointID FROM OutputStreamMeasurement WHERE AdapterID = {1}) " +
                    "ORDER BY SignalReference", "nodeID", "outputStreamID");
                DataTable measurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID(), outputStreamID);

                foreach (DataRow row in measurementTable.Rows)
                {
                    if (row.Field<string>("SignalAcronym") != "STAT")
                    {
                        measurementList.Add(new Measurement
                        {
                            SignalID = database.Guid(row, "SignalID"),
                            HistorianID = row.ConvertNullableField<int>("HistorianID"),
                            PointID = row.ConvertField<int>("PointID"),
                            DeviceID = row.ConvertNullableField<int>("DeviceID"),
                            PointTag = row.Field<string>("PointTag"),
                            AlternateTag = row.Field<string>("AlternateTag"),
                            SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                            PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                            SignalReference = row.Field<string>("SignalReference"),
                            Adder = row.ConvertField<double>("Adder"),
                            Multiplier = row.ConvertField<double>("Multiplier"),
                            Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                            Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                            Description = row.Field<string>("Description"),
                            Enabled = row.ConvertField<bool>("Enabled"),
                            m_historianAcronym = row.Field<string>("HistorianAcronym"),
                            m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                            m_signalName = row.Field<string>("SignalName"),
                            m_signalAcronym = row.Field<string>("SignalAcronym"),
                            m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                            m_phasorLabel = row.Field<string>("PhasorLabel"),
                            m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                            m_id = row.Field<string>("ID"),
                            m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                            m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                            Selected = false
                        });
                    }
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
        /// Retrieves statistic measurements for a device.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the device to filter data.</param>
        /// <returns><see cref="ObservableCollection{T}"/> type collection of Measurement.</returns>
        public static ObservableCollection<Measurement> GetInputStatisticMeasurements(AdoDataConnection database, int deviceID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();
                string query;
                DataTable measurementTable;

                if (deviceID == 0)
                {
                    query = database.ParameterizedQueryString("SELECT * FROM StatisticMeasurement WHERE NodeID = {0} AND DeviceID > 0 ORDER BY SignalReference", "nodeID");
                    measurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());
                }
                else
                {
                    query = database.ParameterizedQueryString("SELECT * FROM StatisticMeasurement WHERE NodeID = {0} AND DeviceID = {1} ORDER BY SignalReference", "nodeID", "deviceID");
                    measurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID(), deviceID);
                }

                foreach (DataRow row in measurementTable.Rows)
                {
                    if (row.Field<string>("SignalAcronym") == "STAT") // Just one more filter.
                    {
                        measurementList.Add(new Measurement
                            {
                                SignalID = database.Guid(row, "SignalID"),
                                HistorianID = row.ConvertNullableField<int>("HistorianID"),
                                PointID = row.ConvertField<int>("PointID"),
                                DeviceID = row.ConvertNullableField<int>("DeviceID"),
                                PointTag = row.Field<string>("PointTag"),
                                AlternateTag = row.Field<string>("AlternateTag"),
                                SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                                PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                                SignalReference = row.Field<string>("SignalReference"),
                                Adder = row.ConvertField<double>("Adder"),
                                Multiplier = row.ConvertField<double>("Multiplier"),
                                Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                                Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                                Description = row.Field<string>("Description"),
                                Enabled = row.ConvertField<bool>("Enabled"),
                                m_historianAcronym = row.Field<string>("HistorianAcronym"),
                                m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                                m_signalName = row.Field<string>("SignalName"),
                                m_signalAcronym = row.Field<string>("SignalAcronym"),
                                m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                                m_phasorLabel = row.Field<string>("PhasorLabel"),
                                m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                                m_id = row.Field<string>("ID"),
                                m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                                m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                                Selected = false
                            });
                    }
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
        /// Retrieves statistic measurements for output stream.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="outputStreamAcronym">Acronym of the output stream to filter data.</param>
        /// <returns><see cref="ObservableCollection{T}"/> type collection of Measurement.</returns>
        public static ObservableCollection<Measurement> GetOutputStatisticMeasurements(AdoDataConnection database, string outputStreamAcronym)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                ObservableCollection<Measurement> measurementList = new ObservableCollection<Measurement>();

                string query = database.ParameterizedQueryString("SELECT * FROM StatisticMeasurement WHERE NodeID = {0} AND DeviceID IS NULL ORDER BY SignalReference", "nodeID");
                DataTable measurementTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in measurementTable.Rows)
                {
                    if (row.Field<string>("SignalAcronym") == "STAT") // Just one more filter.
                    {
                        bool continueProcess = false;

                        if (!string.IsNullOrEmpty(outputStreamAcronym))
                        {
                            if (row.Field<string>("SignalReference").StartsWith(outputStreamAcronym + "!OS"))
                                continueProcess = true;
                        }
                        else
                        {
                            continueProcess = true;
                        }

                        if (continueProcess)
                        {
                            measurementList.Add(new Measurement
                            {
                                SignalID = database.Guid(row, "SignalID"),
                                HistorianID = row.ConvertNullableField<int>("HistorianID"),
                                PointID = row.ConvertField<int>("PointID"),
                                DeviceID = row.ConvertNullableField<int>("DeviceID"),
                                PointTag = row.Field<string>("PointTag"),
                                AlternateTag = row.Field<string>("AlternateTag"),
                                SignalTypeID = row.ConvertField<int>("SignalTypeID"),
                                PhasorSourceIndex = row.ConvertNullableField<int>("PhasorSourceIndex"),
                                SignalReference = row.Field<string>("SignalReference"),
                                Adder = row.ConvertField<double>("Adder"),
                                Multiplier = row.ConvertField<double>("Multiplier"),
                                Internal = Convert.ToBoolean(row.Field<object>("Internal")),
                                Subscribed = Convert.ToBoolean(row.Field<object>("Subscribed")),
                                Description = row.Field<string>("Description"),
                                Enabled = row.ConvertField<bool>("Enabled"),
                                m_historianAcronym = row.Field<string>("HistorianAcronym"),
                                m_deviceAcronym = row.Field<object>("DeviceAcronym") == null ? string.Empty : row.Field<string>("DeviceAcronym"),
                                m_signalName = row.Field<string>("SignalName"),
                                m_signalAcronym = row.Field<string>("SignalAcronym"),
                                m_signalSuffix = row.Field<string>("SignalTypeSuffix"),
                                m_phasorLabel = row.Field<string>("PhasorLabel"),
                                m_framesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                                m_id = row.Field<string>("ID"),
                                m_companyAcronym = row.Field<object>("CompanyAcronym") == null ? string.Empty : row.Field<string>("CompanyAcronym"),
                                m_companyName = row.Field<object>("CompanyName") == null ? string.Empty : row.Field<string>("CompanyName"),
                                Selected = false
                            });
                        }
                    }
                }

                return measurementList;
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
