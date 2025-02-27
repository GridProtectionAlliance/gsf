﻿//******************************************************************************************************
//  MetaData.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/11/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GSF;
using GSF.TimeSeries;

namespace GEPDataExtractor
{
    public class MeasurementDetail
    {
        public long PointID { get; set; }
        public string PointTag { get; set; }
        public Guid SignalID { get; set; }
        public string DeviceName { get; set; }
        public string SignalAcronym { get; set; }
        public string Description { get; set; }

        public MeasurementDetail(DataRow row)
        {
            MeasurementKey.TryParse(row["ID"].ToString(), out MeasurementKey measurementKey);
            PointID = unchecked((long)measurementKey.ID);
            PointTag = row["PointTag"].ToString();
            SignalID = Guid.Parse(row["SignalID"].ToString());
            DeviceName = row["DeviceAcronym"].ToString();
            SignalAcronym = row["SignalAcronym"].ToString();
            Description = row["Description"].ToString();
        }
    }

    public class DeviceDetail
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public string Missing { get; set; }
        public string BadData { get; set; }
        public string BadTime { get; set; }
        public string Description { get; set; }
        public bool Concentrator { get; set; }
        public string Company { get; set; }
        public string VendorName { get; set; }
        public string VendorDevice { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public Guid UniqueID { get; set; }

        public DeviceDetail(DataRow row)
        {
            Selected = true;
            Name = row["Acronym"].ToString();
            Missing = " -- % ";
            BadData = " -- % ";
            BadTime = " -- % ";
            Description = $"{row["Name"]} {row["ParentAcronym"]} {row["OriginalSource"]}";
            Concentrator = row["IsConcentrator"].ToString().ParseBoolean();
            Company = row["CompanyAcronym"].ToString();
            VendorName = row["VendorAcronym"].ToString();
            VendorDevice = row["VendorDeviceName"].ToString();
            Longitude = decimal.Parse(row["Longitude"].ToNonNullNorWhiteSpace("0.0"));
            Latitude = decimal.Parse(row["Latitude"].ToNonNullNorWhiteSpace("0.0"));
            UniqueID = Guid.Parse(row["UniqueID"].ToString());
        }
    }

    public class DeviceStats
    {
        public DeviceDetail Device;
        public long BadDataCount;
        public long BadTimeCount;
        public long MissingDataCount;
        public long Total;
    }

    public class DeviceDetailComparer : IComparer<DeviceDetail>
    {
        private readonly IComparer m_comparer;
        private readonly PropertyInfo m_property;
        private readonly SortOrder m_sortOrder;

        public DeviceDetailComparer(string propertyName, SortOrder sortOrder)
        {
            m_property = typeof(DeviceDetail).GetProperty(propertyName);

            if (m_property is null)
                throw new ArgumentException($"Property name {propertyName} not found", nameof(propertyName));

            Type comparerType = typeof(Comparer<>).MakeGenericType(m_property.PropertyType);
            PropertyInfo defaultComparer = comparerType.GetProperty("Default");

            if (defaultComparer is null)
                throw new InvalidOperationException($"Failed to find \"Default\" property for \"Comparer<{m_property.PropertyType.Name}>\"");

            m_comparer = defaultComparer.GetValue(null) as IComparer;            
            m_sortOrder = sortOrder;
        }

        public int Compare(DeviceDetail left, DeviceDetail right)
        {
            object leftValue = m_property.GetValue(left);
            object rightValue = m_property.GetValue(right);

            return m_sortOrder == SortOrder.Descending ? 
                m_comparer.Compare(rightValue, leftValue) : 
                m_comparer.Compare(leftValue, rightValue);
        }
    }

    public class Metadata
    {
        public DataTable MeasurementTable;
        public DataTable DeviceTable;
        public DataTable PhasorTable;

        public List<MeasurementDetail> Measurements;
        public List<DeviceDetail> Devices;

        public Metadata(Settings settings)
        {
            Measurements = new List<MeasurementDetail>();
            Devices = new List<DeviceDetail>();

            // Do the following on button click or missing configuration, etc:

            // Note that openHistorian internal publisher controls how many tables / fields to send as meta-data to subscribers (user controllable),
            // as a result, not all fields in associated database views will be available. Below are the default SELECT filters the publisher
            // will apply to the "MeasurementDetail", "DeviceDetail" and "PhasorDetail" database views:

            // "SELECT NodeID, UniqueID, OriginalSource, IsConcentrator, Acronym, Name, AccessID, ParentAcronym, ProtocolName, FramesPerSecond, CompanyAcronym, VendorAcronym, VendorDeviceName, Longitude, Latitude, InterconnectionName, ContactList, Enabled, UpdatedOn FROM DeviceDetail WHERE IsConcentrator = 0;" +
            // "SELECT DeviceAcronym, ID, SignalID, PointTag, SignalReference, SignalAcronym, PhasorSourceIndex, Description, Internal, Enabled, UpdatedOn FROM MeasurementDetail;" +
            // "SELECT ID, DeviceAcronym, Label, Type, Phase, DestinationPhasorID, SourceIndex, UpdatedOn FROM PhasorDetail;" +
            // "SELECT VersionNumber FROM SchemaVersion";

            // SELECT Internal, DeviceAcronym, DeviceName, SignalAcronym, ID, SignalID, PointTag, SignalReference, Description, Enabled FROM MeasurementDetail
            // SELECT DeviceAcronym, Label, Type, Phase, SourceIndex FROM PhasorDetail

            MeasurementTable = null;
            DeviceTable = null;

            try
            {
                DataSet metadata = MetadataRetriever.GetMetadata($"server={settings.HostAddress}; port={settings.Port}; interface=0.0.0.0");

                // Reference meta-data tables
                MeasurementTable = metadata.Tables["MeasurementDetail"];
                DeviceTable = metadata.Tables["DeviceDetail"];
                PhasorTable = metadata.Tables["PhasorDetail"];

                MeasurementTable.Columns.Add("Phase", typeof(string));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception retrieving meta-data: " + ex.Message);
            }

            if (MeasurementTable is not null)
            {
                // Load measurement records
                foreach (DataRow row in MeasurementTable.Select("SignalAcronym <> 'STAT' and SignalAcronym <> 'DIGI'"))
                {
                    if (PhasorTable is not null && row["SignalAcronym"].ToString().Contains("PH") && !string.IsNullOrWhiteSpace(row["DeviceAcronym"].ToString()) && !string.IsNullOrWhiteSpace(row["PhasorSourceIndex"].ToString()))
                    {
                        DataRow[] rows = PhasorTable.Select($"DeviceAcronym = '{row["DeviceAcronym"]}' AND SourceIndex = {row["PhasorSourceIndex"]}");

                        if (rows.Length > 0)
                            row["Phase"] = rows[0]["Phase"];
                    }

                    Measurements.Add(new MeasurementDetail(row));
                }
            }

            if (DeviceTable is not null)
            {
                // Load device records
                foreach (DataRow row in DeviceTable.Rows)
                    Devices.Add(new DeviceDetail(row));
            }
        }
    }
}
