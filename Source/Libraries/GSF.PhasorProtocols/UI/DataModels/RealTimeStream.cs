﻿//******************************************************************************************************
//  RealTimeData.cs - Gbtc
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
//  07/21/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//  03/06/2012 - J. Ritchie Carroll
//       Added virtual device for subscribed measurements when two nodes share a single database.
//
//******************************************************************************************************
// ReSharper disable ValueParameterNotUsed

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GSF.Data;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.DataModels
{
    /// <summary>
    /// Represents a real-time stream of subscribed data.
    /// </summary>
    public class RealTimeStream : DataModelBase
    {
        #region [ Members ]

        // Fields
        private int m_id;
        private string m_acronym;
        private string m_name;
        private string m_companyName;
        private bool m_enabled;
        private bool m_expanded;
        private string m_statusColor;
        private bool? m_configurationOutOfSync;
        private ObservableCollection<RealTimeDevice> m_deviceList;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RealTimeStream"/> class.
        /// </summary>
        public RealTimeStream()
            : base(false, false)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the ID of the <see cref="RealTimeStream"/>.
        /// </summary>
        public int ID
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Gets or sets the acronym of the current <see cref="RealTimeStream"/>.
        /// </summary>
        public string Acronym
        {
            get => m_acronym;
            set
            {
                m_acronym = value;
                OnPropertyChanged(nameof(Acronym));
            }
        }

        /// <summary>
        /// Gets or sets the name of the current <see cref="RealTimeStream"/>.
        /// </summary>
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets or sets the company name of the current <see cref="RealTimeStream"/>.
        /// </summary>
        public string CompanyName
        {
            get => m_companyName;
            set
            {
                m_companyName = value;
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        /// <summary>
        /// Gets or sets the Expanded flag for the current <see cref="RealTimeStream"/>.
        /// </summary>
        public bool Expanded
        {
            get => m_expanded;
            set
            {
                m_expanded = value;
                OnPropertyChanged(nameof(Expanded));
            }
        }

        /// <summary>
        /// Gets or sets the status color of the current <see cref="RealTimeStream"/>.
        /// </summary>
        public string StatusColor
        {
            get => m_statusColor;
            set
            {
                m_statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncContent));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncToolTip));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncColor));
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if the input stream configuration is out of sync.
        /// </summary>
        public bool ConfigurationOutOfSync
        {
            get => m_configurationOutOfSync ?? false;
            set
            {
                m_configurationOutOfSync = value;
                OnPropertyChanged(nameof(ConfigurationOutOfSync));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncContent));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncToolTip));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncColor));
            }
        }

        /// <summary>
        /// Gets content message for input stream configuration sync state.
        /// </summary>
        public string ConfigurationOutOfSyncContent
        {
            get => DeviceStateUnavailable ? "Loading..." :
                m_configurationOutOfSync is null ? "Checking Config..." :
                    m_configurationOutOfSync.Value ?
                        "Config Out of Sync" :
                        "Config In Sync";
            set { }
        }

        /// <summary>
        /// Gets tool tip for input stream configuration sync state.
        /// </summary>
        public string ConfigurationOutOfSyncToolTip
        {
            get => DeviceStateUnavailable ? "Loading device state..." :
                m_configurationOutOfSync is null ? "Evaluating configuration synchronization state..." :
                    m_configurationOutOfSync.Value ?
                        "Latest received device configuration is not synchronized with host system. Click to resynchronize." :
                        "Latest received device configuration is synchronized with host system.";
            set { }
        }

        /// <summary>
        /// Gets color for input stream configuration sync state.
        /// </summary>
        public Color ConfigurationOutOfSyncColor
        {
            get => DeviceStateUnavailable ? Colors.Gray :
                m_configurationOutOfSync is null ? Colors.Blue :
                m_configurationOutOfSync.Value ?
                    Colors.Red :
                    GreenColor;
            set { }
        }

        /// <summary>
        /// Gets flag that determines if device state is not yet available, e.g., loading.
        /// </summary>
        public bool DeviceStateUnavailable => string.IsNullOrWhiteSpace(StatusColor) || StatusColor is "Gray" or "Transparent";


        /// <summary>
        /// Gets or sets whether the current <see cref="RealTimeStream"/> is enabled.
        /// </summary>
        public bool Enabled
        {
            get => m_enabled;
            set
            {
                m_enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        /// <summary>
        /// Gets or sets collection of <see cref="RealTimeDevice"/>s for the current <see cref="RealTimeStream"/>.
        /// </summary>
        public ObservableCollection<RealTimeDevice> DeviceList
        {
            get => m_deviceList;
            set
            {
                m_deviceList = value;
                OnPropertyChanged(nameof(DeviceList));
            }
        }

        #endregion

        #region [ Static ]

        private const int GroupAccessID = -99999;
        internal static readonly Color GreenColor;

        static RealTimeStream()
        {
            try
            {
                object parsedColor = ColorConverter.ConvertFromString("#FF19C819");
                GreenColor = parsedColor is Color color ? color : Colors.Green;
            }
            catch
            {
                GreenColor = Colors.Green;
            }
        }

        // Static Methods

        /// <summary>
        /// Loads <see cref="RealTimeStream"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="RealTimeStream"/>.</returns>
        public static ObservableCollection<RealTimeStream> Load(AdoDataConnection database)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);

                DataSet resultSet = new();
                resultSet.EnforceConstraints = false;

                // Get PDCs list.
                resultSet.Tables.Add(database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, Acronym, Name, ConnectionString, CompanyName, Enabled FROM DeviceDetail " +
                    "WHERE NodeID = {0} AND IsConcentrator = {1} AND Enabled = {2} ORDER BY Acronym", "nodeID", "isConcentrator", "enabled"), DefaultTimeout, database.CurrentNodeID(), database.Bool(true), database.Bool(true)).Copy());

                resultSet.Tables[0].TableName = "PdcTable";

                // Add a dummy device row in PDC table to associate PMUs which are not PDC and connected directly.
                DataRow row = resultSet.Tables["PdcTable"].NewRow();
                row[nameof(ID)] = 0;
                row[nameof(Acronym)] = string.Empty;
                row[nameof(Name)] = "Devices Connected Directly";
                row[nameof(CompanyName)] = string.Empty;
                row[nameof(Enabled)] = false;
                resultSet.Tables["PdcTable"].Rows.Add(row);

                // Get Non-PDC device list.
                resultSet.Tables.Add(database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, Acronym, Name, AccessID, CompanyName, ProtocolName, VendorDeviceName, " +
                    "ParentAcronym, Enabled FROM DeviceDetail WHERE NodeID = {0} AND IsConcentrator = {1} AND Enabled = {2} ORDER BY Acronym", "nodeID", "isConcentrator", "enabled"),
                    DefaultTimeout, database.CurrentNodeID(), database.Bool(false), database.Bool(true)).Copy());

                resultSet.Tables[1].TableName = "DeviceTable";

                // Get non-statistical measurement list
                resultSet.Tables.Add(database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, DeviceID, SignalID, PointID, PointTag, SignalReference, " +
                    "SignalAcronym, Description, SignalName, EngineeringUnits, HistorianAcronym, Subscribed, Internal FROM MeasurementDetail WHERE NodeID = {0} AND " +
                    "SignalAcronym <> {1} ORDER BY SignalReference", "nodeID", "signalAcronym"), DefaultTimeout, database.CurrentNodeID(), "STAT").Copy());

                resultSet.Tables[2].TableName = "MeasurementTable";

                // Query for any non-statistical measurements that are subscribed via GEP, but are a part of another node in the same database
                // IMPORTANT: Make sure columns in this external node query exactly match those of the previous primary measurement query
                DataTable otherMeasurements = database.Connection.RetrieveData(database.AdapterType, database.ParameterizedQueryString("SELECT ID, 0 AS DeviceID, SignalID, PointID, PointTag, SignalReference, " +
                    "SignalAcronym, Description, SignalName, EngineeringUnits, HistorianAcronym, Subscribed, Internal FROM MeasurementDetail WHERE NodeID <> {0} AND " +
                    "SignalAcronym <> {1} AND Subscribed <> 0 ORDER BY SignalReference", "nodeID", "signalAcronym"), DefaultTimeout, database.CurrentNodeID(), "STAT");

                Dictionary<string, string> parseKeyValuePairs(string connectionString)
                {
                    Dictionary<string, string> settings;

                    try
                    {
                        settings = connectionString.ParseKeyValuePairs();
                    }
                    catch
                    {
                        settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    return settings;
                }

                ObservableCollection<RealTimeStream> realTimeStreamList = new(resultSet.Tables["PdcTable"]
                    .AsEnumerable()
                    .GroupJoin(
                        resultSet.Tables["DeviceTable"]
                            .AsEnumerable()
                            .GroupJoin(
                                resultSet.Tables["MeasurementTable"].AsEnumerable(),
                                device => device.ConvertNullableField<int>(nameof(RealTimeDevice.ID)),
                                measurement => measurement.ConvertNullableField<int>(nameof(RealTimeMeasurement.DeviceID)),
                                (Info, Measurements) => new { Info, Measurements }),
                        pdc => pdc.Field<string>(nameof(Acronym)),
                        device => device.Info.Field<string>(nameof(RealTimeDevice.ParentAcronym)).ToNonNullString(),
                        (pdc, devices) =>
                        {
                            var settings = parseKeyValuePairs(pdc.Field<string>("ConnectionString").ToNonNullString());

                            return new RealTimeStream
                            {
                                ID = pdc.ConvertField<int>(nameof(ID)),
                                Acronym = string.IsNullOrEmpty(pdc.Field<string>(nameof(Acronym))) ? "DIRECT CONNECTED" : pdc.Field<string>(nameof(Acronym)),
                                Name = pdc.Field<string>(nameof(Name)),
                                CompanyName = pdc.Field<string>(nameof(CompanyName)),
                                StatusColor = string.IsNullOrEmpty(pdc.Field<string>(nameof(Acronym))) ? "Transparent" : "Gray",
                                Enabled = Convert.ToBoolean(pdc.Field<object>(nameof(Enabled))),
                                Expanded = false,
                                DeviceList = new ObservableCollection<RealTimeDevice>(devices
                                    .Select(device => new
                                    {
                                        device.Info,
                                        MeasurementList = new ObservableCollection<RealTimeMeasurement>(device.Measurements
                                            .Where(measurement => measurement.ConvertField<bool>("Subscribed") || measurement.ConvertField<bool>("Internal") || (settings.ContainsKey("securityMode") && settings["securityMode"].Equals("None", StringComparison.OrdinalIgnoreCase))) // We will only display measurements which are internal or subscribed to avoid confusion.
                                            .Select(measurement => new RealTimeMeasurement
                                            {
                                                ID = measurement.Field<string>(nameof(ID)),
                                                DeviceID = measurement.ConvertNullableField<int>("DeviceID"),
                                                SignalID = Guid.Parse(measurement.Field<object>("SignalID").ToString()),
                                                PointID = measurement.ConvertField<int>("PointID"),
                                                PointTag = measurement.Field<string>("PointTag"),
                                                SignalReference = measurement.Field<string>("SignalReference"),
                                                Description = measurement.Field<string>("description"),
                                                SignalName = measurement.Field<string>("SignalName"),
                                                SignalAcronym = measurement.Field<string>("SignalAcronym"),
                                                EngineeringUnit = measurement.Field<string>("SignalAcronym") == "FLAG" ? "Hex" : measurement.Field<string>("EngineeringUnits"),
                                                Expanded = false,
                                                Selected = false,
                                                Selectable = measurement.Field<string>("SignalAcronym") == "IPHM" || measurement.Field<string>("SignalAcronym") == "IPHA" || measurement.Field<string>("SignalAcronym") == "VPHM" || measurement.Field<string>("SignalAcronym") == "VPHA" || measurement.Field<string>("SignalAcronym") == "FREQ",
                                                LongTimeTag = "N/A",
                                                TimeTag = "N/A",
                                                Value = "--",
                                                Quality = "N/A",
                                                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
                                            }))
                                    })
                                    .Where(device => device.Info.ConvertField<int>("AccessID") != GroupAccessID || device.MeasurementList.Count > 0)
                                    .Select(device => new RealTimeDevice
                                    {
                                        ID = device.Info.ConvertNullableField<int>(nameof(ID)),
                                        Acronym = device.Info.Field<string>(nameof(Acronym)),
                                        Name = device.Info.Field<string>(nameof(Name)),
                                        ProtocolName = device.Info.Field<string>("ProtocolName"),
                                        VendorDeviceName = device.Info.Field<string>("VendorDeviceName"),
                                        ParentAcronym = string.IsNullOrEmpty(device.Info.Field<string>("ParentAcronym")) ? "DIRECT CONNECTED" : device.Info.Field<string>("ParentAcronym"),
                                        Expanded = false,
                                        StatusColor = device.Info.ConvertNullableField<int>(nameof(ID)) is null ? "Transparent" : "Gray",
                                        Enabled = Convert.ToBoolean(device.Info.Field<object>(nameof(Enabled))),
                                        MeasurementList = device.MeasurementList
                                    }))
                            };
                        }));

                if (otherMeasurements.Rows.Count > 0)
                {
                    // Add subscribed measurements from other nodes
                    realTimeStreamList.Add(new RealTimeStream
                    {
                        ID = 0,
                        Acronym = "SUBSCRIBED",
                        Name = "Subscribed Measurements",
                        CompanyName = string.Empty,
                        StatusColor = "Transparent",
                        Enabled = false,
                        Expanded = false,
                        DeviceList = new ObservableCollection<RealTimeDevice>(otherMeasurements
                            .AsEnumerable()
                            .Where(measurement => measurement.ConvertNullableField<int>("DeviceID") is null)
                            .GroupBy(measurement => GetSourceName(measurement.Field<string>("SignalReference")))
                            .Select(grouping => new RealTimeDevice
                            {
                                ID = 0,
                                Acronym = grouping.Key,
                                Name = grouping.Key,
                                ProtocolName = string.Empty,
                                VendorDeviceName = string.Empty,
                                ParentAcronym = "SUBSCRIBED",
                                Expanded = false,
                                StatusColor = "Gray",
                                Enabled = false,
                                MeasurementList = new ObservableCollection<RealTimeMeasurement>(grouping
                                    .Select(measurement => new RealTimeMeasurement
                                    {
                                        ID = measurement.Field<string>(nameof(ID)),
                                        DeviceID = measurement.ConvertNullableField<int>("DeviceID"),
                                        SignalID = Guid.Parse(measurement.Field<object>("SignalID").ToString()),
                                        PointID = measurement.ConvertField<int>("PointID"),
                                        PointTag = measurement.Field<string>("PointTag"),
                                        SignalReference = measurement.Field<string>("SignalReference"),
                                        Description = measurement.Field<string>("description"),
                                        SignalName = measurement.Field<string>("SignalName"),
                                        SignalAcronym = measurement.Field<string>("SignalAcronym"),
                                        EngineeringUnit = measurement.Field<string>("SignalAcronym") == "FLAG" ? "Hex" : measurement.Field<string>("EngineeringUnits"),
                                        Expanded = false,
                                        Selected = false,
                                        Selectable = measurement.Field<string>("SignalAcronym") == "IPHM" || measurement.Field<string>("SignalAcronym") == "IPHA" || measurement.Field<string>("SignalAcronym") == "VPHM" || measurement.Field<string>("SignalAcronym") == "VPHA" || measurement.Field<string>("SignalAcronym") == "FREQ",
                                        LongTimeTag = "N/A",
                                        TimeTag = "N/A",
                                        Value = "--",
                                        Quality = "N/A",
                                        Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
                                    }))
                            }))
                    });
                }

                if (resultSet.Tables["MeasurementTable"].Select("DeviceID IS NULL").Length > 0)
                {
                    // Add direct connected measurements with no associated device (DeviceID IS NULL)
                    realTimeStreamList.Add(new RealTimeStream
                    {
                        ID = 0,
                        Acronym = "CALCULATED",
                        Name = "Calculated Measurements",
                        CompanyName = string.Empty,
                        StatusColor = "Transparent",
                        Enabled = false,
                        Expanded = false,
                        DeviceList = new ObservableCollection<RealTimeDevice>(resultSet.Tables["MeasurementTable"]
                            .AsEnumerable()
                            .Where(measurement => measurement.ConvertNullableField<int>("DeviceID") is null)
                            .GroupBy(measurement => GetSourceName(measurement.Field<string>("SignalReference")))
                            .Select(grouping => new RealTimeDevice
                            {
                                ID = 0,
                                Acronym = grouping.Key,
                                Name = grouping.Key,
                                ProtocolName = string.Empty,
                                VendorDeviceName = string.Empty,
                                ParentAcronym = "CALCULATED",
                                Expanded = false,
                                StatusColor = "Gray",
                                Enabled = false,
                                MeasurementList = new ObservableCollection<RealTimeMeasurement>(grouping
                                    .Select(measurement => new RealTimeMeasurement
                                    {
                                        ID = measurement.Field<string>(nameof(ID)),
                                        DeviceID = measurement.ConvertNullableField<int>("DeviceID"),
                                        SignalID = Guid.Parse(measurement.Field<object>("SignalID").ToString()),
                                        PointID = measurement.ConvertField<int>("PointID"),
                                        PointTag = measurement.Field<string>("PointTag"),
                                        SignalReference = measurement.Field<string>("SignalReference"),
                                        Description = measurement.Field<string>("description"),
                                        SignalName = measurement.Field<string>("SignalName"),
                                        SignalAcronym = measurement.Field<string>("SignalAcronym"),
                                        EngineeringUnit = measurement.Field<string>("SignalAcronym") == "FLAG" ? "Hex" : measurement.Field<string>("EngineeringUnits"),
                                        Expanded = false,
                                        Selected = false,
                                        Selectable = measurement.Field<string>("SignalAcronym") == "IPHM" || measurement.Field<string>("SignalAcronym") == "IPHA" || measurement.Field<string>("SignalAcronym") == "VPHM" || measurement.Field<string>("SignalAcronym") == "VPHA" || measurement.Field<string>("SignalAcronym") == "FREQ",
                                        LongTimeTag = "N/A",
                                        TimeTag = "N/A",
                                        Value = "--",
                                        Quality = "N/A",
                                        Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
                                    }))
                            }))
                    });
                }

                // Assign parent references for real-time measurements
                foreach (RealTimeStream stream in realTimeStreamList)
                {
                    foreach (RealTimeDevice device in stream.DeviceList)
                    {
                        device.Parent = stream;

                        foreach (RealTimeMeasurement measurement in device.MeasurementList)
                        {
                            measurement.Parent = device;
                        }
                    }
                }

                return realTimeStreamList;
            }
            finally
            {
                if (createdConnection && database is not null)
                    database.Dispose();
            }
        }

        // Calculate text width
        internal static double GetTextWidth(string text)
        {
            // Just making some assumptions about font and size here for rough assertion on size - would be more accurate to pass in actual font info
            Typeface tf = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        #pragma warning disable CS0618
            FormattedText ft = new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, tf, 14.0D, Brushes.Black, null, TextFormattingMode.Display);
        #pragma warning restore CS0618
            return ft.WidthIncludingTrailingWhitespace;

            // Try this for Silverlight if prior doesn't work:
            //TextBlock txtMeasure = new TextBlock();
            //txtMeasure.FontSize = fontSize;
            //txtMeasure.Text = text ?? "";
            //return txtMeasure.ActualWidth;
        }

        /// <summary>
        /// Retrieves <see cref="Dictionary{T1,T2}"/> type collection.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="isOptional">Indicates if selection on UI is optional for this collection.</param>
        /// <returns><see cref="Dictionary{T1,T2}"/> type collection.</returns>
        /// <remarks>This is only a place holder method with no implementation.</remarks>
        public static Dictionary<int, string> GetLookupList(AdoDataConnection database, bool isOptional = false)
        {
            return null;
        }

        /// <summary>
        /// Saves <see cref="RealTimeStream"/> information into the database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="stream">Information about <see cref="RealTimeStream"/></param>
        /// <returns>String, for display use, indicating success.</returns>
        /// <remarks>This is only a place holder method with no implementation.</remarks>
        public static string Save(AdoDataConnection database, RealTimeStream stream)
        {
            return string.Empty;
        }

        /// <summary>
        /// Deletes <see cref="RealTimeStream"/> record from the database.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <param name="streamID">ID of the record to be deleted.</param>
        /// <returns>String, for display use, indicating success.</returns>
        /// <remarks>This is only a place holder method with no implementation.</remarks>
        public static string Delete(AdoDataConnection database, int streamID)
        {
            return string.Empty;
        }

        private static string GetSourceName(string signalReference)
        {
            // Try to parse source name based on properly formatted signal reference (SOURCE-ID)
            int hyphenIndex = signalReference.LastIndexOf('-');

            if (hyphenIndex >= 0)
                return signalReference.Substring(0, hyphenIndex);

            // Try to parse source name from signal reference formatted like point tag (SOURCE:ID).
            // This format may include company name, but should be the same for all points from the same device
            int colonIndex = signalReference.LastIndexOf(':');

            if (colonIndex >= 0)
                return signalReference.Substring(0, colonIndex);

            // Default to '__' if empty
            if (string.IsNullOrWhiteSpace(signalReference))
                return "__";

            // Assume the value itself can be used to categorize the signal
            return signalReference;
        }

        #endregion
    }

    /// <summary>
    /// Represents a real-time stream of device data.
    /// </summary>
    public class RealTimeDevice : DataModelBase
    {
        #region [ Members ]

        // Fields        
        private int? m_id;
        private string m_acronym;
        private string m_name;
        private string m_protocolName;
        private string m_vendorDeviceName;
        private string m_parentAcronym;
        private bool m_expanded;
        private string m_statusColor;
        private bool? m_configurationOutOfSync;
        private bool m_enabled;
        private double m_maximumSignalReferenceWidth = double.NaN;
        private double m_maximumShortSignalReferenceWidth = double.NaN;
        private RealTimeStream m_parent;
        private ObservableCollection<RealTimeMeasurement> m_measurementList;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RealTimeDevice"/> class.
        /// </summary>
        public RealTimeDevice()
            : base(false, false)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets ID of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public int? ID
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Gets or sets Acronym of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public string Acronym
        {
            get => m_acronym;
            set
            {
                m_acronym = value;
                AcronymTruncated = m_acronym.Substring(m_acronym.LastIndexOf("!", StringComparison.Ordinal) + 1);
                OnPropertyChanged(nameof(Acronym));
            }
        }

        /// <summary>
        /// Gets truncated acronym, i.e. removed parent device prefix. This is only for display purpose.
        /// </summary>
        public string AcronymTruncated { get; set; }

        /// <summary>
        /// Gets or sets Name of the <see cref="RealTimeDevice"/>
        /// </summary>
        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets or sets ProtocolName of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public string ProtocolName
        {
            get => m_protocolName;
            set
            {
                m_protocolName = value;
                OnPropertyChanged(nameof(ProtocolName));
            }
        }

        /// <summary>
        /// Gets or sets VendorDeviceName of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public string VendorDeviceName
        {
            get => m_vendorDeviceName;
            set
            {
                m_vendorDeviceName = value;
                OnPropertyChanged(nameof(VendorDeviceName));
            }
        }

        /// <summary>
        /// Gets or sets ParentAcronym of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public string ParentAcronym
        {
            get => m_parentAcronym;
            set
            {
                m_parentAcronym = value;
                OnPropertyChanged(nameof(ParentAcronym));
            }
        }

        /// <summary>
        /// Gets flag that determines if the <see cref="RealTimeDevice"/> is a direct connection.
        /// </summary>
        public bool IsDirectConnectedDevice
        {
            get => ParentAcronym.Equals("DIRECT CONNECTED");
            set { }
        } 

        /// <summary>
        /// Gets or sets Expanded flag of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public bool Expanded
        {
            get => m_expanded;
            set
            {
                m_expanded = value;
                OnPropertyChanged(nameof(Expanded));
            }
        }

        /// <summary>
        /// Gets or sets StatusColor of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public string StatusColor
        {
            get => m_statusColor;
            set
            {
                m_statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncContent));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncToolTip));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncColor));
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if the input stream configuration is out of sync.
        /// </summary>
        public bool ConfigurationOutOfSync
        {
            get => m_configurationOutOfSync ?? false;
            set
            {
                m_configurationOutOfSync = value;
                OnPropertyChanged(nameof(ConfigurationOutOfSync));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncContent));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncToolTip));
                OnPropertyChanged(nameof(ConfigurationOutOfSyncColor));
            }
        }

        /// <summary>
        /// Gets content message for input stream configuration sync state.
        /// </summary>
        public string ConfigurationOutOfSyncContent
        {
            get => DeviceStateUnavailable ? "Loading..." :
                m_configurationOutOfSync is null ? "Checking Config..." : 
                m_configurationOutOfSync.Value ? 
                    "Config Out of Sync" : 
                    "Config In Sync";
            set { }
        }

        /// <summary>
        /// Gets tool tip for input stream configuration sync state.
        /// </summary>
        public string ConfigurationOutOfSyncToolTip
        {
            get => DeviceStateUnavailable ? "Loading device state..." : 
                m_configurationOutOfSync is null ? "Evaluating configuration synchronization state..." : 
                m_configurationOutOfSync.Value ? 
                    "Latest received device configuration is not synchronized with host system. Click to resynchronize." : 
                    "Latest received device configuration is synchronized with host system.";
            set { }
        }

        /// <summary>
        /// Gets color for input stream configuration sync state.
        /// </summary>
        public Color ConfigurationOutOfSyncColor
        {
            get => DeviceStateUnavailable ? Colors.Gray : 
                m_configurationOutOfSync is null ? Colors.Blue: 
                m_configurationOutOfSync.Value ? 
                    Colors.Red :
                    RealTimeStream.GreenColor;
            set { }
        }

        /// <summary>
        /// Gets flag that determines if device state is not yet available, e.g., loading.
        /// </summary>
        public bool DeviceStateUnavailable => string.IsNullOrWhiteSpace(StatusColor) || StatusColor is "Gray" or "Transparent";

        /// <summary>
        /// Gets or sets Enabled flag of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public bool Enabled
        {
            get => m_enabled;
            set
            {
                m_enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        /// <summary>
        /// Gets or sets collection of <see cref="RealTimeMeasurement"/>s of the <see cref="RealTimeDevice"/>.
        /// </summary>
        public ObservableCollection<RealTimeMeasurement> MeasurementList
        {
            get => m_measurementList;
            set
            {
                m_measurementList = value;
                m_maximumSignalReferenceWidth = double.NaN;
                m_maximumShortSignalReferenceWidth = double.NaN;
                OnPropertyChanged(nameof(MeasurementList));
            }
        }

        /// <summary>
        /// Gets maximum width of signal reference column.
        /// </summary>
        public double MaximumSignalReferenceWidth
        {
            get
            {
                if (double.IsNaN(m_maximumSignalReferenceWidth) && m_measurementList is not null)
                    m_maximumSignalReferenceWidth = m_measurementList.Max(rtm => RealTimeStream.GetTextWidth(rtm.SignalReference));

                return m_maximumSignalReferenceWidth;
            }
            set
            {
                m_maximumSignalReferenceWidth = value;
                OnPropertyChanged(nameof(MaximumSignalReferenceWidth));
            }
        }

        /// <summary>
        /// Gets maximum width of short signal reference column.
        /// </summary>
        public double MaximumShortSignalReferenceWidth
        {
            get
            {
                if (double.IsNaN(m_maximumShortSignalReferenceWidth) && m_measurementList is not null)
                    m_maximumShortSignalReferenceWidth = m_measurementList.Max(rtm => RealTimeStream.GetTextWidth(rtm.ShortSignalReference));

                return m_maximumShortSignalReferenceWidth;
            }
            set
            {
                m_maximumShortSignalReferenceWidth = value;
                OnPropertyChanged(nameof(MaximumShortSignalReferenceWidth));
            }
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public RealTimeStream Parent
        {
            get => m_parent;
            set
            {
                m_parent = value;
                OnPropertyChanged(nameof(Parent));
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a real-time stream of Measurement data.
    /// </summary>
    public class RealTimeMeasurement : DataModelBase
    {
        #region [ Members ]

        // Fields

        private int? m_deviceID;
        private Guid m_signalID;
        private string m_id;
        private int m_pointID;
        private string m_pointTag;
        private string m_signalReference;
        private string m_description;
        private string m_signalName;
        private string m_signalAcronym;
        private string m_engineeringUnit;
        private bool m_expanded;
        private bool m_selected;
        private bool m_selectable;
        private string m_longTimeTag;
        private string m_timeTag;
        private string m_value;
        private string m_quality;
        private long m_lastUpdated;
        private SolidColorBrush m_foreground;
        private RealTimeDevice m_parent;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RealTimeMeasurement"/> class.
        /// </summary>
        public RealTimeMeasurement()
            : base(false, false)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets parent <see cref="RealTimeDevice"/>.
        /// </summary>
        public RealTimeDevice Parent
        {
            get => m_parent;
            set
            {
                m_parent = value;
                OnPropertyChanged(nameof(MaximumSignalReferenceWidth));
            }
        }

        /// <summary>
        /// Gets maximum with of signal reference column.
        /// </summary>
        public string MaximumSignalReferenceWidth
        {
            get
            {
                if (m_parent is null)
                    return "Auto";

                double width = m_parent.MaximumSignalReferenceWidth;

                return double.IsNaN(width) ? "Auto" : width.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets maximum with of signal reference column.
        /// </summary>
        public string MaximumShortSignalReferenceWidth
        {
            get
            {
                if (m_parent is null)
                    return "Auto";

                double width = m_parent.MaximumShortSignalReferenceWidth;

                return double.IsNaN(width) ? "Auto" : width.ToString(CultureInfo.InvariantCulture);
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets DeviceID for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public int? DeviceID
        {
            get => m_deviceID;
            set
            {
                m_deviceID = value;
                OnPropertyChanged(nameof(DeviceID));
            }
        }

        /// <summary>
        /// Gets or sets SignalID for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public Guid SignalID
        {
            get => m_signalID;
            set
            {
                m_signalID = value;
                OnPropertyChanged(nameof(SignalID));
            }
        }

        /// <summary>
        /// Gets or sets ID for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public string ID
        {
            get => m_id;
            set
            {
                m_id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        /// <summary>
        /// Gets or sets PointID for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public int PointID
        {
            get => m_pointID;
            set
            {
                m_pointID = value;
                OnPropertyChanged(nameof(PointID));
            }
        }

        /// <summary>
        /// Gets or sets PointTag for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public string PointTag
        {
            get => m_pointTag;
            set
            {
                m_pointTag = value;
                OnPropertyChanged(nameof(PointTag));
            }
        }

        /// <summary>
        /// Gets or sets SignalReference for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public string SignalReference
        {
            get => m_signalReference;
            set
            {
                m_signalReference = value;
                OnPropertyChanged(nameof(SignalReference));
                OnPropertyChanged(nameof(ShortSignalReference));
            }
        }

        /// <summary>
        /// Gets a shortened version of the signal reference.
        /// </summary>
        public string ShortSignalReference => m_signalReference.TrimWithEllipsisMiddle(16);

        /// <summary>
        /// Gets or sets Description for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public string Description
        {
            get => m_description;
            set
            {
                m_description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        /// <summary>
        /// Gets or sets SignalName for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public string SignalName
        {
            get => m_signalName;
            set
            {
                m_signalName = value;
                OnPropertyChanged(nameof(SignalName));
            }
        }

        /// <summary>
        /// Gets or sets SignalAcronym for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public string SignalAcronym
        {
            get => m_signalAcronym;
            set
            {
                m_signalAcronym = value;
                OnPropertyChanged(nameof(SignalAcronym));
            }
        }

        /// <summary>
        /// Gets or sets Engineering Unit for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public string EngineeringUnit
        {
            get => m_engineeringUnit;
            set
            {
                m_engineeringUnit = value;
                OnPropertyChanged(nameof(EngineeringUnit));
                OnPropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// Gets or sets Expanded flag for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public bool Expanded
        {
            get => m_expanded;
            set
            {
                m_expanded = value;
                OnPropertyChanged(nameof(Expanded));
            }
        }

        /// <summary>
        /// Gets or sets Selected flag for <see cref="RealTimeMeasurement"/>.
        /// </summary>
        public bool Selected
        {
            get => m_selected;
            set
            {
                m_selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        /// <summary>
        /// Gets or sets a flag if <see cref="RealTimeMeasurement"/> can be selected on UI or not.
        /// </summary>
        public bool Selectable
        {
            get => m_selectable;
            set
            {
                m_selectable = value;
                OnPropertyChanged(nameof(Selectable));
            }
        }

        /// <summary>
        /// Gets or sets LongTimeTag for <see cref="RealTimeMeasurement"/> data.
        /// </summary>
        public string LongTimeTag
        {
            get => m_longTimeTag;
            set
            {
                m_longTimeTag = value;
                OnPropertyChanged(nameof(LongTimeTag));
            }
        }

        /// <summary>
        /// Gets or sets TimeTag for <see cref="RealTimeMeasurement"/> data.
        /// </summary>
        public string TimeTag
        {
            get => m_timeTag;
            set
            {
                m_timeTag = value;
                OnPropertyChanged(nameof(TimeTag));
            }
        }

        /// <summary>
        /// Gets or sets Value for <see cref="RealTimeMeasurement"/> data.
        /// </summary>
        public string Value
        {
            get
            {
                bool hex = m_engineeringUnit.ToNonNullString().Trim().ToUpperInvariant() == "HEX";

                if (hex && uint.TryParse(m_value, out uint hexValue))
                    return hexValue.ToString("X8");

                return m_value;
            }
            set
            {
                m_value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// Gets or sets Quality for <see cref="RealTimeMeasurement"/> data.
        /// </summary>
        public string Quality
        {
            get => m_quality;
            set
            {
                m_quality = value;
                OnPropertyChanged(nameof(Quality));
            }
        }

        /// <summary>
        /// Gets or sets LastUpdated for <see cref="RealTimeMeasurement"/> data.
        /// </summary>
        public long LastUpdated
        {
            get => m_lastUpdated;
            set
            {
                m_lastUpdated = value;
                OnPropertyChanged(nameof(LastUpdated));
            }
        }

        /// <summary>
        /// Gets or sets Foreground for <see cref="RealTimeMeasurement"/> to display on the screen.
        /// </summary>
        public SolidColorBrush Foreground
        {
            get => m_foreground;
            set
            {
                m_foreground = value;
                OnPropertyChanged(nameof(Foreground));
            }
        }

        #endregion
    }

}
