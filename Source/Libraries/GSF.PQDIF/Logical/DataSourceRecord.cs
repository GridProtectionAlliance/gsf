//******************************************************************************************************
//  DataSourceRecord.cs - Gbtc
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
        /// Gets the physical structure of the data source record.
        /// </summary>
        public Record PhysicalRecord
        {
            get
            {
                return m_physicalRecord;
            }
        }

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
            set
            {
                ScalarElement vendorIDElement = m_physicalRecord.Body.Collection.GetScalarByTag(VendorIDTag);

                if ((object)vendorIDElement == null)
                {
                    vendorIDElement = new ScalarElement()
                    {
                        TagOfElement = VendorIDTag,
                        TypeOfValue = PhysicalType.Guid
                    };

                    m_physicalRecord.Body.Collection.AddElement(vendorIDElement);
                }

                vendorIDElement.SetGuid(value);
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
            set
            {
                ScalarElement equipmentIDElement = m_physicalRecord.Body.Collection.GetScalarByTag(EquipmentIDTag);

                if ((object)equipmentIDElement == null)
                {
                    equipmentIDElement = new ScalarElement()
                    {
                        TagOfElement = EquipmentIDTag,
                        TypeOfValue = PhysicalType.Guid
                    };

                    m_physicalRecord.Body.Collection.AddElement(equipmentIDElement);
                }

                equipmentIDElement.SetGuid(value);
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
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                VectorElement dataSourceNameElement = m_physicalRecord.Body.Collection.GetVectorByTag(DataSourceNameTag);

                if ((object)dataSourceNameElement == null)
                {
                    dataSourceNameElement = new VectorElement()
                    {
                        TagOfElement = DataSourceNameTag,
                        TypeOfValue = PhysicalType.Char1
                    };

                    m_physicalRecord.Body.Collection.AddElement(dataSourceNameElement);
                }

                dataSourceNameElement.Size = bytes.Length;
                dataSourceNameElement.SetValues(bytes, 0);
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
            set
            {
                VectorElement dataSourceCoordinatesElement = m_physicalRecord.Body.Collection.GetVectorByTag(DataSourceCoordinatesTag);

                if ((object)dataSourceCoordinatesElement == null)
                {
                    dataSourceCoordinatesElement = new VectorElement()
                    {
                        Size = 2,
                        TagOfElement = DataSourceCoordinatesTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(dataSourceCoordinatesElement);
                }

                dataSourceCoordinatesElement.SetUInt4(0, value);
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
            set
            {
                VectorElement dataSourceCoordinatesElement = m_physicalRecord.Body.Collection.GetVectorByTag(DataSourceCoordinatesTag);

                if ((object)dataSourceCoordinatesElement == null)
                {
                    dataSourceCoordinatesElement = new VectorElement()
                    {
                        Size = 2,
                        TagOfElement = DataSourceCoordinatesTag,
                        TypeOfValue = PhysicalType.UnsignedInteger4
                    };

                    m_physicalRecord.Body.Collection.AddElement(dataSourceCoordinatesElement);
                }

                dataSourceCoordinatesElement.SetUInt4(1, value);
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

        #region [ Methods ]

        /// <summary>
        /// Adds a new channel definition to the collection
        /// of channel definitions in this data source record.
        /// </summary>
        public ChannelDefinition AddNewChannelDefinition()
        {
            CollectionElement channelDefinitionsElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelDefinitionsTag);
            CollectionElement channelDefinitionElement = new CollectionElement() { TagOfElement = OneChannelDefinitionTag };
            ChannelDefinition channelDefinition = new ChannelDefinition(channelDefinitionElement, this);

            if ((object)channelDefinitionsElement == null)
            {
                channelDefinitionsElement = new CollectionElement()
                {
                    TagOfElement = ChannelDefinitionsTag
                };

                m_physicalRecord.Body.Collection.AddElement(channelDefinitionsElement);
            }

            channelDefinitionsElement.AddElement(channelDefinitionElement);

            return channelDefinition;
        }

        /// <summary>
        /// Removes the given channel definition from the collection of channel definitions.
        /// </summary>
        /// <param name="channelDefinition">The channel definition to be removed.</param>
        public void Remove(ChannelDefinition channelDefinition)
        {
            CollectionElement channelDefinitionsElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelDefinitionsTag);
            List<CollectionElement> channelDefinitionElements;
            ChannelDefinition definition;

            if ((object)channelDefinitionsElement == null)
                return;

            channelDefinitionElements = channelDefinitionsElement.GetElementsByTag(OneChannelDefinitionTag).Cast<CollectionElement>().ToList();

            foreach (CollectionElement channelDefinitionElement in channelDefinitionElements)
            {
                definition = new ChannelDefinition(channelDefinitionElement, this);

                if (Equals(channelDefinition, definition))
                    channelDefinitionsElement.RemoveElement(channelDefinitionElement);
            }
        }

        /// <summary>
        /// Removes the element identified by the given tag from the record.
        /// </summary>
        /// <param name="tag">The tag of the element to be removed.</param>
        public void RemoveElement(Guid tag)
        {
            m_physicalRecord.Body.Collection.RemoveElementsByTag(tag);
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
