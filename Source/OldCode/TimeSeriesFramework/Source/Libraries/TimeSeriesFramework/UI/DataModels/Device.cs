//******************************************************************************************************
//  Device.cs - Gbtc
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
    #region [ Enumerations ]

    /// <summary>
    /// Device type enumeration.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// All types of devices
        /// </summary>
        /// <remarks>
        /// Use this option to return all types of devices.
        /// </remarks>
        All,
        /// <summary>
        /// Concentrator devices such as PDC.
        /// </summary>
        /// <remarks>
        /// Use this option just to return concentrating devices.
        /// </remarks>
        Concentrator,
        /// <summary>
        /// Direct connected devices such as field PMUs.
        /// </summary>
        /// <remarks>
        /// Use this option to return directly connected devices.
        /// </remarks>
        DirectConnected
    }

    #endregion

    /// <summary>
    /// Represents a record of <see cref="Device"/> information as defined in the database.
    /// </summary>
    public class Device : DataModelBase
    {
        #region [ Members ]

        // Fields
        private Guid m_nodeID;
        private int m_id;
        private int? m_parentID;
        private Guid m_uniqueID; 
        private string m_acronym;
        private string m_name;
        private bool m_isConcentrator;
        private int? m_companyID;
        private int? m_historianID;
        private int m_accessID;
        private int? m_vendorDeviceID;
        private int? m_protocolID;
        private decimal? m_longitude;
        private decimal? m_latitude;
        private int? m_interconnectionID;
        private string m_connectionString;
        private string m_timeZone;
        private int? m_framesPerSecond;
        private long m_timeAdjustmentTicks;
        private double m_dataLossInterval;
        private bool m_connectOnDemand;   
        private string m_contactList;
        private int? m_measuredLines;
        private int m_loadOrder;
        private bool m_enabled;
        private int m_allowedParsingExceptions;
        private double m_parsingExceptionWindow;
        private double m_delayedConnectionInterval;
        private bool m_allowUseOfCachedConfiguration;
        private bool m_autoStartDataParsingSequence;
        private bool m_skipDisableRealTimeData;
        private int m_measurementReportingInterval;
        private string m_companyName;
        private string m_companyAcronym;
        private string m_historianAcronym;
        private string m_vendorDeviceName;
        private string m_vendorAcronym;
        private string m_protocolName;
        private string m_interconnectionName;
        private string m_nodeName;
        private string m_parentAcronym;
        private DateTime m_createdOn;
        private string m_createdBy;
        private DateTime m_updatedOn;
        private string m_updatedBy;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="Device"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = "Device node ID is a required field, please provide value.")]
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
        /// Gets or sets <see cref="Device"/> ID.
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
        /// Gets or sets <see cref="Device"/> ParentID.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public int? ParentID
        {
            get
            {
                return m_parentID;
            }
            set
            {
                m_parentID = value;
                OnPropertyChanged("ParentID");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "Device acronym is a required field, please provide value.")]
        [StringLength(50, ErrorMessage = "Device Acronym cannot exceed 50 characters.")]
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
        /// Gets or sets <see cref="Device"/>  Name.
        /// </summary>
        [StringLength(200, ErrorMessage = "Device Name cannot exceed 200 characters.")]
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
        ///  Gets or sets <see cref="Device"/> IsConcenttrator.
        /// </summary>
        [DefaultValue(false)]
        public bool IsConcentrator
        {
            get
            {
                return m_isConcentrator;
            }
            set
            {
                m_isConcentrator = value;
                OnPropertyChanged("IsConcentrator ");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> CompanyID.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public int? CompanyID
        {
            get
            {
                return m_companyID;
            }
            set
            {
                m_companyID = value;
                OnPropertyChanged("CompanyID");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> HistrianID.
        /// </summary>
        // Because of database design, no validation attributes are applied.
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
        ///  Gets or sets <see cref="Device"/> AccessID.
        /// </summary>
        [Required(ErrorMessage = "Device access ID is a required field, please provide value.")]
        [DefaultValue(0)]
        public int AccessID
        {
            get
            {
                return m_accessID;
            }
            set
            {
                m_accessID = value;
                OnPropertyChanged("AccessID");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> VendorDeviceID.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public int? VendorDeviceID
        {
            get
            {
                return m_vendorDeviceID;
            }
            set
            {
                m_vendorDeviceID = value;
                OnPropertyChanged("VendorDeviceID");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> protocolID.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public int? ProtocolID
        {
            get
            {
                return m_protocolID;
            }
            set
            {
                m_protocolID = value;
                OnPropertyChanged("ProtocolID");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> Longitude.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public decimal? Longitude
        {
            get
            {
                return m_longitude;
            }
            set
            {
                m_longitude = value;
                OnPropertyChanged("Longitude");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> Latitude.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public decimal? Latitude
        {
            get
            {
                return m_latitude;
            }
            set
            {
                m_latitude = value;
                OnPropertyChanged("Latitude");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> InterconnectionID.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public int? InterconnectionID
        {
            get
            {
                return m_interconnectionID;
            }
            set
            {
                m_interconnectionID = value;
                OnPropertyChanged("InterconnectionID");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> ConnectionString.
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
        /// Gets or sets <see cref="Device"/> TimeZone.
        /// </summary>
        [StringLength(200, ErrorMessage = "Device Time Zone cannot exceed 200 characters.")]
        public string TimeZone
        {
            get
            {
                return m_timeZone;
            }
            set
            {
                m_timeZone = value;
                OnPropertyChanged("TimeZone");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> FramesPerSecond.
        /// </summary>
        [DefaultValue(30)]
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
        /// Gets or sets <see cref="Device"/> TimeAdjustmentTicks.
        /// </summary>
        [Required(ErrorMessage = "Device time adjustment ticks is a required field, please provide value.")]
        [DefaultValue(0L)]
        public long TimeAdjustmentTicks
        {
            get
            {
                return m_timeAdjustmentTicks;
            }
            set
            {
                m_timeAdjustmentTicks = value;
                OnPropertyChanged("TimeAdjustmentTicks");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> DataLossInterval.
        /// </summary>
        [Required(ErrorMessage = "Device data loss interval is a required field, please provide value.")]
        [DefaultValue(typeof(double), "5")]
        public double DataLossInterval
        {
            get
            {
                return m_dataLossInterval;
            }
            set
            {
                m_dataLossInterval = value;
                OnPropertyChanged("DataLossInterval");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/>ConnectOnDemand.
        /// </summary>
        [Required(ErrorMessage = "ConnectOnDemand is a required field, please provide value.")]
        [DefaultValue(typeof(int), "1")]
        public bool ConnectOnDemand
        {
            get
            {
                return m_connectOnDemand;
            }
            set
            {
                m_connectOnDemand = value;
                OnPropertyChanged("ConnectOnDemand");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ContactList.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public string ContactList
        {
            get
            {
                return m_contactList;
            }
            set
            {
                m_contactList = value;
                OnPropertyChanged("ContactList");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> MeasuredLines.
        /// </summary>
        // Because of database design, no validation attributes are applied.
        public int? MeasuredLines
        {
            get
            {
                return m_measuredLines;
            }
            set
            {
                m_measuredLines = value;
                OnPropertyChanged("MeasuredLines");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> LoadOrder.
        /// </summary>
        [Required(ErrorMessage = "Device load order is a required field, please provide value.")]
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
        /// Gets or sets <see cref="Device"/> Enabled.
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
        /// Gets or sets <see cref="Device"/> AllowedParsingExceptions.
        /// </summary>
        [Required(ErrorMessage = "Device allowed parsing exceptions is a required field, please provide value.")]
        [DefaultValue(10)]
        public int AllowedParsingExceptions
        {
            get
            {
                return m_allowedParsingExceptions;
            }
            set
            {
                m_allowedParsingExceptions = value;
                OnPropertyChanged("AllowedParsingExceptions");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ParsingExceptionWindow.
        /// </summary>
        [Required(ErrorMessage = "Device parsing exception window is a required field, please provide value.")]
        [DefaultValue(5.0D)]
        public double ParsingExceptionWindow
        {
            get
            {
                return m_parsingExceptionWindow;
            }
            set
            {
                m_parsingExceptionWindow = value;
                OnPropertyChanged("ParsingExceptionWindow");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> DelayedConnectionInterval.
        /// </summary>
        [Required(ErrorMessage = "Device delayed connection interval is a required field, please provide value.")]
        [DefaultValue(5.0D)]
        public double DelayedConnectionInterval
        {
            get
            {
                return m_delayedConnectionInterval;
            }
            set
            {
                m_delayedConnectionInterval = value;
                OnPropertyChanged("DelayedConnectionInterval");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> AllowUseOfCachedConfiguration.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowUseOfCachedConfiguration
        {
            get
            {
                return m_allowUseOfCachedConfiguration;
            }
            set
            {
                m_allowUseOfCachedConfiguration = value;
                OnPropertyChanged("AllowUseOfCachedConfiguration");

            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> AutoStartDataParsingSequence.
        /// </summary>
        [DefaultValue(true)]
        public bool AutoStartDataParsingSequence
        {
            get
            {
                return m_autoStartDataParsingSequence;
            }
            set
            {
                m_autoStartDataParsingSequence = value;
                OnPropertyChanged("AutoStartDataParsingSequence");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> SkipDisableRealTimeData.
        /// </summary>
        [DefaultValue(false)]
        public bool SkipDisableRealTimeData
        {
            get
            {
                return m_skipDisableRealTimeData;
            }
            set
            {
                m_skipDisableRealTimeData = value;
                OnPropertyChanged("SkipDisableRealTimeData");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> MeasurementReportingInterval.
        /// </summary>
        [Required(ErrorMessage = "Device measurement reporting interval is a required field, please provide value.")]
        [DefaultValue(100000)]
        public int MeasurementReportingInterval
        {
            get
            {
                return m_measurementReportingInterval;
            }
            set
            {
                m_measurementReportingInterval = value;
                OnPropertyChanged("MeasurementReportingInterval");
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> CompanyName.
        /// </summary>
        public string CompanyName
        {
            get
            {
                return m_companyName;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> CompanyAcronym.
        /// </summary>
        public string CompanyAcronym
        {
            get
            {
                return m_companyAcronym;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> HistorianAcronym.
        /// </summary>
        public string HistorianAcronym
        {
            get
            {
                return m_historianAcronym;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> VendorDeviceName.
        /// </summary>
        public string VendorDeviceName
        {
            get
            {
                return m_vendorDeviceName;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> VendorAcronym.
        /// </summary>
        public string VendorAcronym
        {
            get
            {
                return m_vendorAcronym;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> ProtocolName.
        /// </summary>
        public string ProtocolName
        {
            get
            {
                return m_protocolName;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> InterconnectionName.
        /// </summary>
        public string InterconnectionName
        {
            get
            {
                return m_interconnectionName;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> NodeName.
        /// </summary>
        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
        }

        /// <summary>
        /// Gets <see cref="Device"/> ParentAcronym.
        /// </summary>
        public string ParentAcronym
        {
            get
            {
                return m_parentAcronym;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> CreatedOn.
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
        /// Gets or sets <see cref="Device"/> CreatedBy.
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
        ///  Gets or sets <see cref="Device"/> UpdatedOn.
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
        ///  Gets or sets <see cref="Device"/> UpdatedBy.
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
        /// Loads <see cref="Device"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>        
        /// <param name="parentID">ID of the parent device to filter data.</param>
        /// <returns>Collection of <see cref="Device"/>.</returns>
        public static ObservableCollection<Device> Load(AdoDataConnection database, int parentID = 0)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Device> deviceList = new ObservableCollection<Device>();
                DataTable deviceTable;

                if (parentID > 0)
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM DeviceDetail WHERE NodeID = @nodeID AND ParentID = @parentID " +
                        "ORDER BY Acronym", DefaultTimeout, database.CurrentNodeID(), parentID);
                else
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM DeviceDetail WHERE NodeID = @nodeID ORDER BY Acronym",
                        DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in deviceTable.Rows)
                {
                    deviceList.Add(new Device()
                    {
                        NodeID = Guid.Parse(row.Field<string>("NodeID")),
                        ID = row.Field<int>("ID"),
                        ParentID = row.Field<int?>("ParentID"),
                        Acronym = row.Field<string>("Acronym"),
                        Name = row.Field<string>("Name"),
                        IsConcentrator = Convert.ToBoolean(row.Field<object>("IsConcentrator")),
                        CompanyID = row.Field<int?>("CompanyID"),
                        HistorianID = row.Field<int?>("HistorianID"),
                        AccessID = row.Field<int>("AccessID"),
                        VendorDeviceID = row.Field<int?>("VendorDeviceID"),
                        ProtocolID = row.Field<int?>("ProtocolID"),
                        Longitude = row.Field<decimal?>("Longitude"),
                        Latitude = row.Field<decimal?>("Latitude"),
                        InterconnectionID = row.Field<int?>("InterconnectionID"),
                        ConnectionString = row.Field<string>("ConnectionString"),
                        TimeZone = row.Field<string>("TimeZone"),
                        FramesPerSecond = Convert.ToInt32(row.Field<object>("FramesPerSecond") ?? 30),
                        TimeAdjustmentTicks = Convert.ToInt64(row.Field<object>("TimeAdjustmentTicks")),
                        DataLossInterval = row.Field<double>("DataLossInterval"),
                        ContactList = row.Field<string>("ContactList"),
                        MeasuredLines = row.Field<int?>("MeasuredLines"),
                        LoadOrder = row.Field<int>("LoadOrder"),
                        Enabled = Convert.ToBoolean(row.Field<object>("Enabled")),
                        CreatedOn = row.Field<DateTime>("CreatedOn"),
                        AllowedParsingExceptions = Convert.ToInt32(row.Field<object>("AllowedParsingExceptions")),
                        ParsingExceptionWindow = row.Field<double>("ParsingExceptionWindow"),
                        DelayedConnectionInterval = row.Field<double>("DelayedConnectionInterval"),
                        AllowUseOfCachedConfiguration = Convert.ToBoolean(row.Field<object>("AllowUseOfCachedConfiguration")),
                        AutoStartDataParsingSequence = Convert.ToBoolean(row.Field<object>("AutoStartDataParsingSequence")),
                        SkipDisableRealTimeData = Convert.ToBoolean(row.Field<object>("SkipDisableRealTimeData")),
                        MeasurementReportingInterval = Convert.ToInt32(row.Field<object>("MeasurementReportingInterval")),
                        ConnectOnDemand =  Convert.ToBoolean(row.Field<object>("ConnectOnDemand")),
                        m_companyName = row.Field<string>("CompanyName"),
                        m_companyAcronym = row.Field<string>("CompanyAcronym"),
                        m_historianAcronym = row.Field<string>("HistorianAcronym"),
                        m_vendorDeviceName = row.Field<string>("VendorDeviceName"),
                        m_vendorAcronym = row.Field<string>("VendorAcronym"),
                        m_protocolName = row.Field<string>("ProtocolName"),
                        m_interconnectionName = row.Field<string>("InterconnectionName"),
                        m_nodeName = row.Field<string>("NodeName"),
                        m_parentAcronym = row.Field<string>("ParentAcronym")
                    });
                }

                return deviceList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{T1,T2}"/> style list of <see cref="Device"/> information.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>        
        /// <param name="deviceType"><see cref="DeviceType"/> to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>        
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of companies defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, DeviceType deviceType = DeviceType.DirectConnected, bool isOptional = false)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> deviceList = new Dictionary<int, string>();
                DataTable deviceTable;

                if (isOptional)
                    deviceList.Add(0, "Select Device");

                if (deviceType == DeviceType.Concentrator)
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM Device WHERE IsConcentrator = @isConcentrator " +
                        "AND NodeID = @nodeID ORDER BY LoadOrder", DefaultTimeout, true, database.CurrentNodeID());
                else if (deviceType == DeviceType.DirectConnected)
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM Device WHERE IsConcentrator = @isConcentrator " +
                        "AND NodeID = @nodeID ORDER BY LoadOrder", DefaultTimeout, false, database.CurrentNodeID());
                else
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM Device WHERE " +
                        "NodeID = @nodeID ORDER BY LoadOrder", DefaultTimeout, database.CurrentNodeID());

                foreach (DataRow row in deviceTable.Rows)
                    deviceList[row.Field<int>("ID")] = row.Field<string>("Acronym");

                return deviceList;
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Saves <see cref="Device"/> information to database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="device">Information about <see cref="Device"/>.</param>        
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Device device)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                if (device.ID == 0)
                    database.Connection.ExecuteNonQuery("INSERT INTO Device (NodeID, ParentID, Acronym, Name, IsConcentrator, CompanyID, HistorianID, AccessID, VendorDeviceID, " +
                        "ProtocolID, Longitude, Latitude, InterconnectionID, ConnectionString, TimeZone, FramesPerSecond, TimeAdjustmentTicks, DataLossInterval, ContactList, " +
                        "MeasuredLines, LoadOrder, Enabled, AllowedParsingExceptions, ParsingExceptionWindow, DelayedConnectionInterval, AllowUseOfCachedConfiguration, " +
                        "AutoStartDataParsingSequence, SkipDisableRealTimeData, MeasurementReportingInterval, ConnectOndemand, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@nodeID, " +
                        "@parentID, @acronym, @name, @isConcentrator, @companyID, @historianID, @accessID, @vendorDeviceID, @protocolID, @longitude, @latitude, @interconnectionID, " +
                        "@connectionString, @timezone, @framesPerSecond, @timeAdjustmentTicks, @dataLossInterval, @contactList, @measuredLines, @loadOrder, @enabled, " +
                        "@allowedParsingExceptions, @parsingExceptionWindow, @delayedConnectionInterval, @allowUseOfCachedConfiguration, @autoStartDataParsingSequence, @connectionOndemand" +
                        "@skipDisableRealTimeData, @measurementReportingInterval, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, database.Guid(device.NodeID),
                        device.ParentID.ToNotNull(), device.Acronym.Replace(" ", "").ToUpper(), device.Name, device.IsConcentrator, device.CompanyID.ToNotNull(),
                        device.HistorianID.ToNotNull(), device.AccessID, device.VendorDeviceID.ToNotNull(),
                        device.ProtocolID.ToNotNull(), device.Longitude.ToNotNull(), device.Latitude.ToNotNull(), device.InterconnectionID.ToNotNull(),
                        device.ConnectionString, device.TimeZone, device.FramesPerSecond ?? 30, device.TimeAdjustmentTicks, device.DataLossInterval, device.ContactList, device.MeasuredLines.ToNotNull(),
                        device.LoadOrder, device.Enabled, device.AllowedParsingExceptions, device.ParsingExceptionWindow, device.DelayedConnectionInterval, device.AllowUseOfCachedConfiguration,device.ConnectOnDemand,
                        device.AutoStartDataParsingSequence, device.SkipDisableRealTimeData, device.MeasurementReportingInterval, CommonFunctions.CurrentUser,
                        database.UtcNow(), CommonFunctions.CurrentUser, database.UtcNow());
                else
                    database.Connection.ExecuteNonQuery("UPDATE Device SET NodeID = @nodeID, ParentID = @parentID, Acronym = @acronym, Name = @name, IsConcentrator = @isConcentrator, " +
                        "CompanyID = @companyID, HistorianID = @historianID, AccessID = @accessID, VendorDeviceID = @vendorDeviceID, ProtocolID = @protocolID, Longitude = @longitude, " +
                        "Latitude = @latitude, InterconnectionID = @interconnectionID, ConnectionString = @connectionString, TimeZone = @timezone, FramesPerSecond = @framesPerSecond, " +
                        "TimeAdjustmentTicks = @timeAdjustmentTicks, DataLossInterval = @dataLossInterval, ContactList = @contactList, MeasuredLines = @measuredLines, " +
                        "LoadOrder = @loadOrder, Enabled = @enabled, AllowedParsingExceptions = @allowedParsingExceptions, ParsingExceptionWindow = @parsingExceptionWindow, " +
                        "DelayedConnectionInterval = @delayedConnectionInterval, AllowUseOfCachedConfiguration = @allowUseOfCachedConfiguration, AutoStartDataParsingSequence " +
                        "= @autoStartDataParsingSequence, SkipDisableRealTimeData = @skipDisableRealTimeData, MeasurementReportingInterval = @measurementReportingInterval, ConnectOnDemand = @ConnectOnDemand " +
                        "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, database.Guid(device.NodeID),
                        device.ParentID.ToNotNull(), device.Acronym.Replace(" ", "").ToUpper(), device.Name, device.IsConcentrator, device.CompanyID.ToNotNull(),
                        device.HistorianID.ToNotNull(), device.AccessID, device.VendorDeviceID.ToNotNull(),
                        device.ProtocolID.ToNotNull(), device.Longitude.ToNotNull(), device.Latitude.ToNotNull(), device.InterconnectionID.ToNotNull(),
                        device.ConnectionString, device.TimeZone, device.FramesPerSecond ?? 30, device.TimeAdjustmentTicks, device.DataLossInterval, device.ContactList, device.MeasuredLines.ToNotNull(),
                        device.LoadOrder, device.Enabled, device.AllowedParsingExceptions, device.ConnectOnDemand, device.ParsingExceptionWindow, device.DelayedConnectionInterval, device.AllowUseOfCachedConfiguration,
                        device.AutoStartDataParsingSequence, device.SkipDisableRealTimeData, device.MeasurementReportingInterval, CommonFunctions.CurrentUser,
                        database.UtcNow(), device.ID);

                return "Device information saved successfully";
            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        /// <summary>
        /// Deletes specified <see cref="Device"/> record from database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="deviceID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Delete(AdoDataConnection database, int deviceID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                // Setup current user context for any delete triggers
                CommonFunctions.SetCurrentUserContext(database);

                database.Connection.ExecuteNonQuery("DELETE FROM Device WHERE ID = @deviceID", DefaultTimeout, deviceID);

                return "Device deleted successfully";
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
