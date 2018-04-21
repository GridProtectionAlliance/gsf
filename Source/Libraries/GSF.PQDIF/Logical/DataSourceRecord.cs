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
    /// Types of data sources.
    /// </summary>
    public static class DataSourceType
    {
        /// <summary>
        /// The ID for data source type Measure.
        /// </summary>
        public static readonly Guid Measure = new Guid("e6b51730-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for data source type Manual.
        /// </summary>
        public static readonly Guid Manual = new Guid("e6b51731-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for data source type Simulate.
        /// </summary>
        public static readonly Guid Simulate = new Guid("e6b51732-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for data source type Benchmark.
        /// </summary>
        public static readonly Guid Benchmark = new Guid("e6b51733-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// The ID for data source type Debug.
        /// </summary>
        public static readonly Guid Debug = new Guid("e6b51734-f747-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Gets information about the data source type identified by the given ID.
        /// </summary>
        /// <param name="dataSourceTypeID">Globally unique identifier for the data source type.</param>
        /// <returns>The information about the data source type.</returns>
        public static Identifier GetInfo(Guid dataSourceTypeID)
        {
            Identifier identifier;
            return DataSourceTypeLookup.TryGetValue(dataSourceTypeID, out identifier) ? identifier : null;
        }

        /// <summary>
        /// Converts the given data source type ID to a string containing the name of the data source type.
        /// </summary>
        /// <param name="dataSourceTypeID">The ID of the data source type to be converted to a string.</param>
        /// <returns>A string containing the name of the data source type with the given ID.</returns>
        public static string ToString(Guid dataSourceTypeID)
        {
            return GetInfo(dataSourceTypeID)?.Name ?? dataSourceTypeID.ToString();
        }

        private static Dictionary<Guid, Identifier> DataSourceTypeLookup
        {
            get
            {
                Tag dataSourceTypeTag = Tag.GetTag(DataSourceRecord.VendorIDTag);

                if (s_dataSourceTypeTag != dataSourceTypeTag)
                {
                    s_dataSourceTypeLookup = dataSourceTypeTag.ValidIdentifiers.ToDictionary(id => Guid.Parse(id.Value));
                    s_dataSourceTypeTag = dataSourceTypeTag;
                }

                return s_dataSourceTypeLookup;
            }
        }

        private static Tag s_dataSourceTypeTag;
        private static Dictionary<Guid, Identifier> s_dataSourceTypeLookup;
    }

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
        /// Gets or sets the ID of the type of the data source.
        /// </summary>
        public Guid DataSourceTypeID
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetScalarByTag(DataSourceTypeIDTag)
                    .GetGuid();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement dataSourceTypeIDElement = collectionElement.GetOrAddScalar(DataSourceTypeIDTag);
                dataSourceTypeIDElement.TypeOfValue = PhysicalType.Guid;
                dataSourceTypeIDElement.SetGuid(value);
            }
        }

        /// <summary>
        /// Gets or sets the ID of the vendor of the data source.
        /// </summary>
        public Guid VendorID
        {
            get
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement vendorIDElement = collectionElement.GetScalarByTag(VendorIDTag);

                if ((object)vendorIDElement == null)
                    return Vendor.None;

                return vendorIDElement.GetGuid();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement vendorIDElement = collectionElement.GetOrAddScalar(VendorIDTag);
                vendorIDElement.TypeOfValue = PhysicalType.Guid;
                vendorIDElement.SetGuid(value);
            }
        }

        /// <summary>
        /// Gets or sets the ID of the equipment.
        /// </summary>
        public Guid EquipmentID
        {
            get
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement equipmentIDElement = collectionElement.GetScalarByTag(EquipmentIDTag);

                if ((object)equipmentIDElement == null)
                    return Guid.Empty;

                return equipmentIDElement.GetGuid();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement equipmentIDElement = collectionElement.GetOrAddScalar(EquipmentIDTag);
                equipmentIDElement.TypeOfValue = PhysicalType.Guid;
                equipmentIDElement.SetGuid(value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the data source.
        /// </summary>
        public string DataSourceName
        {
            get
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                VectorElement dataSourceNameElement = collectionElement.GetVectorByTag(DataSourceNameTag);
                return Encoding.ASCII.GetString(dataSourceNameElement.GetValues()).Trim((char)0);
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                collectionElement.AddOrUpdateVector(DataSourceNameTag, PhysicalType.Char1, bytes);
            }
        }

        /// <summary>
        /// Gets or sets the longitude at which the data source is located.
        /// </summary>
        public uint Longitude
        {
            get
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                VectorElement dataSourceCoordinatesElement = collectionElement.GetVectorByTag(DataSourceCoordinatesTag);

                if ((object)dataSourceCoordinatesElement == null)
                    return uint.MaxValue;

                return dataSourceCoordinatesElement.GetUInt4(0);
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                VectorElement dataSourceCoordinatesElement = collectionElement.GetOrAddVector(DataSourceCoordinatesTag);
                dataSourceCoordinatesElement.TypeOfValue = PhysicalType.UnsignedInteger4;
                dataSourceCoordinatesElement.Size = 2;
                dataSourceCoordinatesElement.SetUInt4(0, value);
            }
        }

        /// <summary>
        /// Gets or sets the latitude at which the device is located.
        /// </summary>
        public uint Latitude
        {
            get
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                VectorElement dataSourceCoordinatesElement = collectionElement.GetVectorByTag(DataSourceCoordinatesTag);

                if ((object)dataSourceCoordinatesElement == null)
                    return uint.MaxValue;

                return dataSourceCoordinatesElement.GetUInt4(1);
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                VectorElement dataSourceCoordinatesElement = collectionElement.GetOrAddVector(DataSourceCoordinatesTag);
                dataSourceCoordinatesElement.TypeOfValue = PhysicalType.UnsignedInteger4;
                dataSourceCoordinatesElement.Size = 2;
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

        /// <summary>
        /// Gets or sets the time that this data source record became effective.
        /// </summary>
        public DateTime Effective
        {
            get
            {
                return m_physicalRecord.Body.Collection
                    .GetScalarByTag(EffectiveTag)
                    .GetTimestamp();
            }
            set
            {
                CollectionElement collectionElement = m_physicalRecord.Body.Collection;
                ScalarElement effectiveElement = collectionElement.GetOrAddScalar(EffectiveTag);
                effectiveElement.TypeOfValue = PhysicalType.Timestamp;
                effectiveElement.SetTimestamp(value);
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
            CollectionElement channelDefinitionElement = new CollectionElement() { TagOfElement = OneChannelDefinitionTag };
            ChannelDefinition channelDefinition = new ChannelDefinition(channelDefinitionElement, this);

            channelDefinition.Phase = Phase.None;
            channelDefinition.QuantityMeasured = QuantityMeasured.None;
            channelDefinitionElement.AddElement(new CollectionElement() { TagOfElement = ChannelDefinition.SeriesDefinitionsTag });

            CollectionElement channelDefinitionsElement = m_physicalRecord.Body.Collection.GetCollectionByTag(ChannelDefinitionsTag);

            if ((object)channelDefinitionsElement == null)
            {
                channelDefinitionsElement = new CollectionElement() { TagOfElement = ChannelDefinitionsTag };
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
        public static readonly Guid DataSourceTypeIDTag = new Guid("b48d8581-f5f5-11cf-9d89-0080c72e70a3");

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
        /// Creates a new data source record from scratch.
        /// </summary>
        /// <param name="dataSourceName">The name of the data source to be created.</param>
        /// <returns>The new data source record.</returns>
        public static DataSourceRecord CreateDataSourceRecord(string dataSourceName)
        {
            Guid recordTypeTag = Record.GetTypeAsTag(RecordType.DataSource);
            Record physicalRecord = new Record(recordTypeTag);
            DataSourceRecord dataSourceRecord = new DataSourceRecord(physicalRecord);

            DateTime now = DateTime.UtcNow;
            dataSourceRecord.DataSourceTypeID = DataSourceType.Simulate;
            dataSourceRecord.DataSourceName = dataSourceName;
            dataSourceRecord.Effective = now;

            CollectionElement bodyElement = physicalRecord.Body.Collection;
            bodyElement.AddElement(new CollectionElement() { TagOfElement = ChannelDefinitionsTag });

            return dataSourceRecord;
        }

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
