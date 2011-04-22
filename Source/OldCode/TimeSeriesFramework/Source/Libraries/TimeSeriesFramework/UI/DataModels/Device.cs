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
    /// Represents a record of Device information as defined in the database.
    /// </summary>
    public class Device : DataModelBase
    {
        #region [Members]

        //fields
        //private Guid NodeID ;
        private string m_nodeID;
        private int m_id;
        private int? m_parentID;
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

        #region [Properties]

        /// <summary>
        /// Gets or sets <see cref="Device"/> NodeID.
        /// </summary>
        [Required(ErrorMessage = "Device NodeID is a required field, please provide value.")]
        public string NodeID
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
                OnPropertyChanged("ID");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ParentID.
        /// </summary>
        public int? ParentID
        {
            get
            {
                return m_parentID;
            }
            set
            {
                m_parentID = value;
                OnPropertyChanged("ParentId");
            }
        }

        /// <summary>
        ///  Gets or sets <see cref="Device"/> Acronym.
        /// </summary>
        [Required(ErrorMessage = "Device Acronym is a required field, please provide value.")]
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        [Required(ErrorMessage = " Device IsConcentrator is a required field, please provide value.")]
        [DefaultValue(typeof(bool), "0")]
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        [Required(ErrorMessage = "AccessID is required field.")]
        [DefaultValue(typeof(int), "1")]
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                OnPropertyChanged("connectionString");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> TimeZone.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        //Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public int? FramesPerSecond
        {
            get
            {
                return m_framesPerSecond;
            }
            set
            {
                m_framesPerSecond = value;
                OnPropertyChanged("framePerSecond");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> TimeAdjustmentTicks.
        /// </summary>
        [Required(ErrorMessage = "TimeAdjustmentTicks is required field.")]
        [DefaultValue(typeof(long), "0")]
        public long TimeAdjustmentTicks
        {
            get
            {
                return m_timeAdjustmentTicks;
            }
            set
            {
                m_timeAdjustmentTicks = value;
                OnPropertyChanged("TimeAdjustment");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> DataLossInterval.
        /// </summary>
        [Required(ErrorMessage = "DataLossInterval is required field.")]
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
        /// Gets or sets <see cref="Device"/> ContactList.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        [Required(ErrorMessage = "DataLossInterval is required field.")]
        [DefaultValue(typeof(double), "5")]
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
        /// Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        [Required(ErrorMessage = "AllowedParsingExceptions is required field.")]
        [DefaultValue(typeof(int), "10")]
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
        [Required(ErrorMessage = "ParsingExceptionWindow is required field.")]
        [DefaultValue(typeof(double), "5")]
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
        [Required(ErrorMessage = "DelayedConnectionInterval is required field.")]
        [DefaultValue(typeof(double), "5")]
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
        [Required(ErrorMessage = "AllowUseOfCachedConfiguration is required field.")]
        [DefaultValue(typeof(bool), "1")]
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
        [Required(ErrorMessage = "AutoStartDataParsingSequence is required field.")]
        [DefaultValue(typeof(bool), "1")]
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
        [Required(ErrorMessage = "SkipDisableRealTimeData is required field.")]
        [DefaultValue(typeof(bool), "0")]
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
        [Required(ErrorMessage = "SkipDisableRealTimeData is required field.")]
        [DefaultValue(typeof(int), "100000")]
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
        /// Gets or sets <see cref="Device"/> CompanyName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string CompanyName
        {
            get
            {
                return m_companyName;
            }
            set
            {
                m_companyName = value;
                OnPropertyChanged("CompanyName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> CompanyAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string CompanyAcronym
        {
            get
            {
                return m_companyAcronym;
            }
            set
            {
                m_companyAcronym = value;
                OnPropertyChanged("CompanyAcronym");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> HistorianAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="Device"/> VendorDeviceName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string VendorDeviceName
        {
            get
            {
                return m_vendorDeviceName;
            }
            set
            {
                m_vendorDeviceName = value;
                OnPropertyChanged("VendorDeviceName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> VendorAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string VendorAcronym
        {
            get
            {
                return m_vendorAcronym;
            }
            set
            {
                m_vendorAcronym = value;
                OnPropertyChanged("VendorAcronym");

            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ProtocolName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ProtocolName
        {
            get
            {
                return m_protocolName;
            }
            set
            {
                m_protocolName = value;
                OnPropertyChanged("ProtocolName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> InterconnectionName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string InterconnectionName
        {
            get
            {
                return m_interconnectionName;
            }
            set
            {
                m_interconnectionName = value;
                OnPropertyChanged("InterconnectionName");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> NodeName.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string NodeName
        {
            get
            {
                return m_nodeName;
            }
            set
            {
                m_nodeName = value;
                OnPropertyChanged("NodeName");

            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> ParentAcronym.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
        public string ParentAcronym
        {
            get
            {
                return m_parentAcronym;
            }
            set
            {
                m_parentAcronym = value;
                OnPropertyChanged("ParentAcronym");

            }
        }

        /// <summary>
        /// Gets or sets <see cref="Device"/> CreatedOn.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        /// Gets or sets <see cref="Device"/> CreatedBy.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        ///  Gets or sets <see cref="Device"/> UpdatedOn.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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
        ///  Gets or sets <see cref="Device"/> UpdatedBy.
        /// </summary>
        // Field is populated by database via auto-increment and has no screen interaction, so no validation attributes are applied
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

        //Static Methods

        /// <summary>
        /// Loads <see cref="Device"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="nodeID">Id of the <see cref="Node"/> for which <see cref="Device"/> collection is returned.</param>
        /// <param name="parentID">ID of the parent device to filter data.</param>
        /// <returns>Collection of <see cref="Device"/>.</returns>
        public static ObservableCollection<Device> Load(AdoDataConnection database, string nodeID, int parentID = 0)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                ObservableCollection<Device> deviceList = new ObservableCollection<Device>();

                DataTable deviceTable;
                if (parentID > 0)
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "Select * From DeviceDetail Where NodeID = @nodeID AND ParentID = @parentID " +
                        "Order By Acronym", DefaultTimeout, database.IsJetEngine() ? "{" + nodeID + "}" : nodeID, parentID);
                else
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "Select * From DeviceDetail Where NodeID = @nodeID Order By Acronym",
                        DefaultTimeout, database.IsJetEngine() ? "{" + nodeID + "}" : nodeID);

                foreach (DataRow row in deviceTable.Rows)
                {
                    deviceList.Add(new Device()
                    {
                        NodeID = row.Field<object>("NodeID").ToString(),
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
                        CompanyName = row.Field<string>("CompanyName"),
                        CompanyAcronym = row.Field<string>("CompanyAcronym"),
                        HistorianAcronym = row.Field<string>("HistorianAcronym"),
                        VendorDeviceName = row.Field<string>("VendorDeviceName"),
                        VendorAcronym = row.Field<string>("VendorAcronym"),
                        ProtocolName = row.Field<string>("ProtocolName"),
                        InterconnectionName = row.Field<string>("InterconnectionName"),
                        NodeName = row.Field<string>("NodeName"),
                        ParentAcronym = row.Field<string>("ParentAcronym")
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
        /// <param name="nodeID">Id of the <see cref="Node"/> for which <see cref="Device"/> collection is returned.</param>
        /// <param name="deviceType"><see cref="DeviceType"/> to filter data.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>        
        /// <returns><see cref="Dictionary{T1,T2}"/> containing ID and Name of companies defined in the database.</returns>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, string nodeID, DeviceType deviceType = DeviceType.DirectConnected,
            bool isOptional = false)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                Dictionary<int, string> deviceList = new Dictionary<int, string>();
                if (isOptional)
                    deviceList.Add(0, "Select Device");

                DataTable deviceTable;

                if (deviceType == DeviceType.Concentrator)
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM Device WHERE IsConcentrator = @isConcentrator " +
                        "AND NodeID = @nodeID ORDER BY LoadOrder", DefaultTimeout, true, database.IsJetEngine() ? "{" + nodeID + "}" : nodeID);
                else if (deviceType == DeviceType.DirectConnected)
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM Device WHERE IsConcentrator = @isConcentrator " +
                        "AND NodeID = @nodeID ORDER BY LoadOrder", DefaultTimeout, false, database.IsJetEngine() ? "{" + nodeID + "}" : nodeID);
                else
                    deviceTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM Device WHERE " +
                        "NodeID = @nodeID ORDER BY LoadOrder", DefaultTimeout, database.IsJetEngine() ? "{" + nodeID + "}" : nodeID);

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
        /// <param name="isNew">Indicates if save is a new addition or an update to an existing record.</param>
        /// <returns>String, for display use, indicating success.</returns>
        public static string Save(AdoDataConnection database, Device device, bool isNew)
        {
            bool createdConnection = false;
            try
            {
                createdConnection = CreateConnection(ref database);

                if (isNew)
                    database.Connection.ExecuteNonQuery("Insert Into Device (NodeID, ParentID, Acronym, Name, IsConcentrator, CompanyID, HistorianID, AccessID, VendorDeviceID, " +
                    "ProtocolID, Longitude, Latitude, InterconnectionID, ConnectionString, TimeZone, FramesPerSecond, TimeAdjustmentTicks, DataLossInterval, ContactList, " +
                    "MeasuredLines, LoadOrder, Enabled, AllowedParsingExceptions, ParsingExceptionWindow, DelayedConnectionInterval, AllowUseOfCachedConfiguration, " +
                    "AutoStartDataParsingSequence, SkipDisableRealTimeData, MeasurementReportingInterval, UpdatedBy, UpdatedOn, CreatedBy, CreatedOn) Values (@nodeID, " +
                    "@parentID, @acronym, @name, @isConcentrator, @companyID, @historianID, @accessID, @vendorDeviceID, @protocolID, @longitude, @latitude, @interconnectionID, " +
                    "@connectionString, @timezone, @framesPerSecond, @timeAdjustmentTicks, @dataLossInterval, @contactList, @measuredLines, @loadOrder, @enabled, " +
                    "@allowedParsingExceptions, @parsingExceptionWindow, @delayedConnectionInterval, @allowUseOfCachedConfiguration, @autoStartDataParsingSequence, " +
                    "@skipDisableRealTimeData, @measurementReportingInterval, @updatedBy, @updatedOn, @createdBy, @createdOn)", DefaultTimeout, device.NodeID,
                    device.ParentID ?? (object)DBNull.Value, device.Acronym.Replace(" ", "").ToUpper(), device.Name, device.IsConcentrator, device.CompanyID ?? (object)DBNull.Value,
                    device.HistorianID ?? (object)DBNull.Value, device.AccessID, device.VendorDeviceID == null ? (object)DBNull.Value : device.VendorDeviceID == 0 ? (object)DBNull.Value : device.VendorDeviceID,
                    device.ProtocolID ?? (object)DBNull.Value, device.Longitude ?? (object)DBNull.Value, device.Latitude ?? (object)DBNull.Value, device.InterconnectionID ?? (object)DBNull.Value,
                    device.ConnectionString, device.TimeZone, device.FramesPerSecond ?? 30, device.TimeAdjustmentTicks, device.DataLossInterval, device.ContactList, device.MeasuredLines ?? (object)DBNull.Value,
                    device.LoadOrder, device.Enabled, device.AllowedParsingExceptions, device.ParsingExceptionWindow, device.DelayedConnectionInterval, device.AllowUseOfCachedConfiguration,
                    device.AutoStartDataParsingSequence, device.SkipDisableRealTimeData, device.MeasurementReportingInterval, CommonFunctions.CurrentUser,
                    database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, CommonFunctions.CurrentUser, database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow);
                else
                    database.Connection.ExecuteNonQuery("Update Device Set NodeID = @nodeID, ParentID = @parentID, Acronym = @acronym, Name = @name, IsConcentrator = @isConcentrator, " +
                    "CompanyID = @companyID, HistorianID = @historianID, AccessID = @accessID, VendorDeviceID = @vendorDeviceID, ProtocolID = @protocolID, Longitude = @longitude, " +
                    "Latitude = @latitude, InterconnectionID = @interconnectionID, ConnectionString = @connectionString, TimeZone = @timezone, FramesPerSecond = @framesPerSecond, " +
                    "TimeAdjustmentTicks = @timeAdjustmentTicks, DataLossInterval = @dataLossInterval, ContactList = @contactList, MeasuredLines = @measuredLines, " +
                    "LoadOrder = @loadOrder, Enabled = @enabled, AllowedParsingExceptions = @allowedParsingExceptions, ParsingExceptionWindow = @parsingExceptionWindow, " +
                    "DelayedConnectionInterval = @delayedConnectionInterval, AllowUseOfCachedConfiguration = @allowUseOfCachedConfiguration, AutoStartDataParsingSequence " +
                    "= @autoStartDataParsingSequence, SkipDisableRealTimeData = @skipDisableRealTimeData, MeasurementReportingInterval = @measurementReportingInterval, " +
                    "UpdatedBy = @updatedBy, UpdatedOn = @updatedOn WHERE ID = @id", DefaultTimeout, device.NodeID,
                    device.ParentID ?? (object)DBNull.Value, device.Acronym.Replace(" ", "").ToUpper(), device.Name, device.IsConcentrator, device.CompanyID ?? (object)DBNull.Value,
                    device.HistorianID ?? (object)DBNull.Value, device.AccessID, device.VendorDeviceID == null ? (object)DBNull.Value : device.VendorDeviceID == 0 ? (object)DBNull.Value : device.VendorDeviceID,
                    device.ProtocolID ?? (object)DBNull.Value, device.Longitude ?? (object)DBNull.Value, device.Latitude ?? (object)DBNull.Value, device.InterconnectionID ?? (object)DBNull.Value,
                    device.ConnectionString, device.TimeZone, device.FramesPerSecond ?? 30, device.TimeAdjustmentTicks, device.DataLossInterval, device.ContactList, device.MeasuredLines ?? (object)DBNull.Value,
                    device.LoadOrder, device.Enabled, device.AllowedParsingExceptions, device.ParsingExceptionWindow, device.DelayedConnectionInterval, device.AllowUseOfCachedConfiguration,
                    device.AutoStartDataParsingSequence, device.SkipDisableRealTimeData, device.MeasurementReportingInterval, CommonFunctions.CurrentUser,
                    database.IsJetEngine() ? DateTime.UtcNow.Date : DateTime.UtcNow, device.ID);

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
