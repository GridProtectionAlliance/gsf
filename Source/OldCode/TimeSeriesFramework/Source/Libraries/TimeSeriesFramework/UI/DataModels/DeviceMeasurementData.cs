//******************************************************************************************************
//  DeviceMeasurementData.cs - Gbtc
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
//  06/29/2011 - Magdiel Lorenzo
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TVA.Data;
using System.Data;

namespace TimeSeriesFramework.UI.DataModels
{
    /// <summary>
    /// Represents a device measurement data object as defined in the database.
    /// </summary>
    public class DeviceMeasurementData : DataModelBase
    {
        #region [ Members ]

        private int m_id;
        private string m_acronym;
        private string m_name;
        private string m_companyName;
        private bool m_isExpanded;
        private string m_statusColor;
        private bool m_enabled;
        private ObservableCollection<DeviceInfo> m_deviceList;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the ID of the current <see cref="DeviceMeasurementData"/>.
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
        /// Gets or sets the acronym of the current <see cref="DeviceMeasurementData"/>.
        /// </summary>
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
        /// Gets or sets the name of the current <see cref="DeviceMeasurementData"/>.
        /// </summary>
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
        /// Gets or sets the company name of the current <see cref="DeviceMeasurementData"/>.
        /// </summary>
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
        /// Gets or sets whether the current <see cref="DeviceMeasurementData"/> is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return m_isExpanded;
            }
            set
            {
                m_isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        /// <summary>
        /// Gets or sets the status color of the current <see cref="DeviceMeasurementData"/>.
        /// </summary>
        public string StatusColor
        {
            get
            {
                return m_statusColor;
            }
            set
            {
                m_statusColor = value;
                OnPropertyChanged("StatusColor");
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="DeviceMeasurementData"/> is enabled.
        /// </summary>
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
        /// Gets or sets a list of DeviceInfo objects for the current <see cref="DeviceMeasurementData"/>
        /// </summary>
        public ObservableCollection<DeviceInfo> DeviceList
        {
            get
            {
                return m_deviceList;
            }
            set
            {
                m_deviceList = value;
                OnPropertyChanged("DeviceList");
            }
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Retrieves a collection of <see cref="DeviceMeasurementData"/>
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to database</param>
        /// <param name="nodeID">Current node ID</param>
        /// <returns><see cref="ObservableCollection{T}"/> of <see cref="DeviceMeasurementData"/></returns>
        public static ObservableCollection<DeviceMeasurementData> Load(AdoDataConnection database, Guid nodeID)
        {
            bool createdConnection = false;

            try
            {
                createdConnection = CreateConnection(ref database);
                List<DeviceMeasurementData> deviceMeasurementDataList = new List<DeviceMeasurementData>();

                //------------------------------------------
                DataSet resultSet = new DataSet();
                DataTable resultTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym, Name, CompanyName, Enabled FROM DeviceDetail WHERE NodeID = @nodeID AND IsConcentrator = @isConcentrator AND Enabled = @enabled", database.Guid(nodeID), true, true);
                resultTable.TableName = "PdcTable";
                DataRow row = resultTable.NewRow();
                row["ID"] = 0;
                row["Acronym"] = string.Empty;
                row["Name"] = "Devices Connected Directly";
                row["CompanyName"] = string.Empty;
                row["Enabled"] = false;
                resultTable.Rows.Add(row);
                resultSet.Tables.Add(resultTable.Copy());

                //------------------------------------------
                resultTable = database.Connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym, Name, CompanyName, ProtocolName, VendorDeviceName, ParentAcronym, Enabled FROM DeviceDetail WHERE NodeID = @nodeID AND IsConcentrator = @isConcentrator AND Enabled = @enabled ORDER BY Acronym", database.Guid(nodeID), false, true);
                resultTable.TableName = "DeviceTable";
                row = resultTable.NewRow();
                row["ID"] = DBNull.Value;
                row["Acronym"] = "CALCULATED";
                row["Name"] = "CALCULATED MEASUREMENTS";
                row["CompanyName"] = string.Empty;
                row["ProtocolName"] = string.Empty;
                row["VendorDeviceName"] = string.Empty;
                row["ParentAcronym"] = string.Empty;
                row["Enabled"] = false;
                resultSet.Tables.Add(resultTable.Copy());

                //------------------------------------------
                resultTable = database.Connection.RetrieveData(database.AdapterType, "SELECT DeviceID, SignalID, PointID, PointTag, SignalReference, SignalAcronym, Description, SignalName, EngineeringUnits, HistorianAcronym FROM MeasurementDetail WHERE NodeID = @nodeID AND SignalAcronym <> 'STAT' ORDER BY SignalReference", database.Guid(nodeID));
                resultTable.TableName = "MeasurementTable";
                resultSet.Tables.Add(resultTable.Copy());

                if (resultTable.Select("DeviceID IS NULL").Count() > 0)
                {
                    resultSet.Tables["DeviceTable"].Rows.Add(row);
                }

                deviceMeasurementDataList = (from pdc in resultSet.Tables["PdcTable"].AsEnumerable()
                                             select new DeviceMeasurementData()
                                             {
                                                 ID = pdc.Field<int>("ID"),
                                                 Acronym = string.IsNullOrEmpty(pdc.Field<string>("Acronym")) ? "DIRECT CONNECTED" : pdc.Field<string>("Acronym"),
                                                 Name = pdc.Field<string>("Name"),
                                                 CompanyName = pdc.Field<string>("CompanyName"),
                                                 StatusColor = string.IsNullOrEmpty(pdc.Field<string>("Acronym")) ? "Transparent" : "Gray",
                                                 Enabled = Convert.ToBoolean(pdc.Field<object>("Enabled")),
                                                 IsExpanded = false,
                                                 DeviceList = new ObservableCollection<DeviceInfo>((from device in resultSet.Tables["DeviceTable"].AsEnumerable()
                                                                                                    where device.Field<string>("ParentAcronym") == pdc.Field<string>("Acronym")
                                                                                                    select new DeviceInfo()
                                                                                                    {
                                                                                                        ID = device.Field<int?>("ID"),
                                                                                                        Acronym = device.Field<string>("Acronym"),
                                                                                                        Name = device.Field<string>("Name"),
                                                                                                        CompanyName = device.Field<string>("CompanyName"),
                                                                                                        ProtocolName = device.Field<string>("ProtocolName"),
                                                                                                        VendorDeviceName = device.Field<string>("VendorDeviceName"),
                                                                                                        ParentAcronym = string.IsNullOrEmpty(device.Field<string>("ParentAcronym")) ? "DIRECT CONNECTED" : device.Field<string>("ParentAcronym"),
                                                                                                        IsExpanded = false,
                                                                                                        StatusColor = device.Field<int?>("ID") == null ? "Transparent" : "Gray",
                                                                                                        Enabled = Convert.ToBoolean(device.Field<object>("Enabled")),
                                                                                                        MeasurementList = new ObservableCollection<MeasurementInfo>((from measurement in resultSet.Tables["MeasurementTable"].AsEnumerable()
                                                                                                                                                                     where measurement.Field<int?>("DeviceID") == device.Field<int?>("ID")
                                                                                                                                                                     select new MeasurementInfo()
                                                                                                                                                                     {
                                                                                                                                                                         DeviceID = measurement.Field<int?>("DeviceID"),
                                                                                                                                                                         SignalID = measurement.Field<object>("SignalID").ToString(),
                                                                                                                                                                         PointID = measurement.Field<int>("PointID"),
                                                                                                                                                                         PointTag = measurement.Field<string>("PointTag"),
                                                                                                                                                                         SignalReference = measurement.Field<string>("SignalReference"),
                                                                                                                                                                         SignalAcronym = measurement.Field<string>("SignalAcronym"),
                                                                                                                                                                         Description = measurement.Field<string>("description"),
                                                                                                                                                                         SignalName = measurement.Field<string>("SignalName"),
                                                                                                                                                                         EngineeringUnits = measurement.Field<string>("EngineeringUnits"),
                                                                                                                                                                         HistorianAcronym = measurement.Field<string>("HistorianAcronym"),
                                                                                                                                                                         IsExpanded = false,
                                                                                                                                                                         CurrentTimeTag = "N/A",
                                                                                                                                                                         CurrentValue = "--",
                                                                                                                                                                         CurrentQuality = "N/A"
                                                                                                                                                                     }).ToList())
                                                                                                    }).ToList())
                                             }).ToList();

                return new ObservableCollection<DeviceMeasurementData>(deviceMeasurementDataList);

            }
            finally
            {
                if (createdConnection && database != null)
                    database.Dispose();
            }
        }

        #endregion
    }

    /// <summary>
    /// Stores information on a device.
    /// </summary>
    public class DeviceInfo
    {
        public int? ID { get; set; }
        public string Acronym { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string ProtocolName { get; set; }
        public string VendorDeviceName { get; set; }
        public string ParentAcronym { get; set; }
        public bool IsExpanded { get; set; }
        public string StatusColor { get; set; }
        public bool Enabled { get; set; }
        public ObservableCollection<MeasurementInfo> MeasurementList { get; set; }
    }

    /// <summary>
    /// Stores information about a measurement
    /// </summary>
    public class MeasurementInfo
    {
        public int? DeviceID { get; set; }
        public string SignalID { get; set; }
        public int PointID { get; set; }
        public string PointTag { get; set; }
        public string SignalReference { get; set; }
        public string SignalAcronym { get; set; }
        public string Description { get; set; }
        public string SignalName { get; set; }
        public string EngineeringUnits { get; set; }
        public string HistorianAcronym { get; set; }
        public string CurrentTimeTag { get; set; }
        public string CurrentValue { get; set; }
        public string CurrentQuality { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// Class to bind device measurement data to view.
    /// </summary>
    public class DeviceMeasurementDataForBinding
    {
        public ObservableCollection<DeviceMeasurementData> DeviceMeasurementDataList { get; set; }
        public bool IsExpanded { get; set; }
    }
}
