//******************************************************************************************************
//  DataSourceRecord.cs - Gbtc
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
//  05/03/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSF.PQDIF.Physical;

namespace GSF.PQDIF.Logical
{
    /// <summary>
    /// Represents a data source record in a PQDIF file. The data source
    /// record contains information about the source of the data in an
    /// <see cref="ObservationRecord"/>.
    /// </summary>
    public class DataSourceRecord
    {
        #region [ Members ]

        // Fields
        private readonly Record m_physicalRecord;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DataSourceRecord"/> class.
        /// </summary>
        /// <param name="physicalRecord">The physical structure of the data source record.</param>
        private DataSourceRecord(Record physicalRecord)
        {
            m_physicalRecord = physicalRecord;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the ID of the vendor of the data source.
        /// </summary>
        public Guid VendorID
        {
            get
            {
                ScalarElement vendorIDElement = m_physicalRecord.Body.Collection.GetScalarByTag(VendorIDTag);

                if ((object)vendorIDElement == null)
                    return Vendor.None;

                return vendorIDElement.GetGuid();
            }
        }

        /// <summary>
        /// Gets the ID of the equipment.
        /// </summary>
        public Guid EquipmentID
        {
            get
            {
                ScalarElement equipmentIDElement = m_physicalRecord.Body.Collection.GetScalarByTag(EquipmentIDTag);

                if ((object)equipmentIDElement == null)
                    return Guid.Empty;

                return equipmentIDElement.GetGuid();
            }
        }

        /// <summary>
        /// Gets the name of the data source.
        /// </summary>
        public string DataSourceName
        {
            get
            {
                VectorElement dataSourceNameElement = m_physicalRecord.Body.Collection.GetVectorByTag(DataSourceNameTag);
                return Encoding.ASCII.GetString(dataSourceNameElement.GetValues()).Trim((char)0);
            }
        }

        /// <summary>
        /// Gets the longitude at which the data source is located.
        /// </summary>
        public uint Longitude
        {
            get
            {
                VectorElement dataSourceCoordinatesElement = m_physicalRecord.Body.Collection.GetVectorByTag(DataSourceCoordinatesTag);

                if ((object)dataSourceCoordinatesElement == null)
                    return uint.MaxValue;

                return dataSourceCoordinatesElement.GetUInt4(0);
            }
        }

        /// <summary>
        /// Gets the latitude at which the device is located.
        /// </summary>
        public uint Latitude
        {
            get
            {
                VectorElement dataSourceCoordinatesElement = m_physicalRecord.Body.Collection.GetVectorByTag(DataSourceCoordinatesTag);

                if ((object)dataSourceCoordinatesElement == null)
                    return uint.MaxValue;

                return dataSourceCoordinatesElement.GetUInt4(1);
            }
        }

        /// <summary>
        /// Gets the definitions for the channels defined in the data source.
        /// </summary>
        public IList<ChannelDefinition> ChannelDefinitions
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetCollectionByTag(ChannelDefinitionsTag)
                    .GetElementsByTag(OneChannelDefinitionTag)
                    .Cast<CollectionElement>()
                    .Select(collection => new ChannelDefinition(collection, this))
                    .ToList();
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Tag that identifies the data source type.
        /// </summary>
        public static readonly Guid DataSourceTypeTag = new Guid("b48d8581-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the vendor ID.
        /// </summary>
        public static readonly Guid VendorIDTag = new Guid("b48d8582-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the equipment ID.
        /// </summary>
        public static readonly Guid EquipmentIDTag = new Guid("b48d8583-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the data source name.
        /// </summary>
        public static readonly Guid DataSourceNameTag = new Guid("b48d8587-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the physical location of the data source.
        /// </summary>
        public static readonly Guid DataSourceCoordinatesTag = new Guid("b48d858b-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the channel definitions collection.
        /// </summary>
        public static readonly Guid ChannelDefinitionsTag = new Guid("b48d858d-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the a single channel definition in the collection.
        /// </summary>
        public static readonly Guid OneChannelDefinitionTag = new Guid("b48d858e-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the time that the data source record becomes effective.
        /// </summary>
        public static readonly Guid EffectiveTag = new Guid("62f28183-f9c4-11cf-9d89-0080c72e70a3");

        // Static Methods

        /// <summary>
        /// Creates a new data source record from the given physical record
        /// if the physical record is of type data source. Returns null if
        /// it is not.
        /// </summary>
        /// <param name="physicalRecord">The physical record used to create the data source record.</param>
        /// <returns>The new data source record, or null if the physical record does not define a data source record.</returns>
        public static DataSourceRecord CreateDataSourceRecord(Record physicalRecord)
        {
            bool isValidDataSourceRecord = physicalRecord.Header.TypeOfRecord == RecordType.DataSource;
            return isValidDataSourceRecord ? new DataSourceRecord(physicalRecord) : null;
        }

        #endregion
        
    }
}
