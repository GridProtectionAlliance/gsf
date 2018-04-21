//******************************************************************************************************
//  ChannelDefinition.cs - Gbtc
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
//  05/04/2012 - Stephen C. Wills, Grid Protection Alliance
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSF.PQDIF.Physical;

namespace GSF.PQDIF.Logical
{
    #region [ Enumerations ]

    /// <summary>
    /// Phase types.
    /// </summary>
    public enum Phase : uint
    {
        /// <summary>
        /// Phase is not applicable.
        /// </summary>
        None = 0,

        /// <summary>
        /// A-to-neutral.
        /// </summary>
        AN = 1,

        /// <summary>
        /// B-to-neutral.
        /// </summary>
        BN = 2,

        /// <summary>
        /// C-to-neutral.
        /// </summary>
        CN = 3,

        /// <summary>
        /// Neutral-to-ground.
        /// </summary>
        NG = 4,

        /// <summary>
        /// A-to-B.
        /// </summary>
        AB = 5,

        /// <summary>
        /// B-to-C.
        /// </summary>
        BC = 6,

        /// <summary>
        /// C-to-A.
        /// </summary>
        CA = 7,

        /// <summary>
        /// Residual - the vector or point-on-wave sum of Phases A, B, and C.
        /// Should be zero in a perfectly balanced system.
        /// </summary>
        Residual = 8,

        /// <summary>
        /// Net - the vector or point-on-wave sum of Phases A, B, C and the
        /// Neutral phase. Should be zero in a 4-wire system with no earth
        /// return path.
        /// </summary>
        Net = 9,

        /// <summary>
        /// Positive sequence.
        /// </summary>
        PositiveSequence = 10,

        /// <summary>
        /// Negative sequence.
        /// </summary>
        NegativeSequence = 11,

        /// <summary>
        /// Zero sequence.
        /// </summary>
        ZeroSequence = 12,

        /// <summary>
        /// The value representing a total or other
        /// summarizing value in a multi-phase system.
        /// </summary>
        Total = 13,

        /// <summary>
        /// The value representing average of 3 line-neutral values.
        /// </summary>
        LineToNeutralAverage = 14,

        /// <summary>
        /// The value representing average of 3 line-line values.
        /// </summary>
        LineToLineAverage = 15,

        /// <summary>
        /// The value representing the "worst" of the 3 phases.
        /// </summary>
        Worst = 16,

        /// <summary>
        /// DC Positive.
        /// </summary>
        Plus = 17,

        /// <summary>
        /// DC Negative.
        /// </summary>
        Minus = 18,

        /// <summary>
        /// Generic Phase 1.
        /// </summary>
        General1 = 19,

        /// <summary>
        /// Generic Phase 2.
        /// </summary>
        General2 = 20,

        /// <summary>
        /// Generic Phase 3.
        /// </summary>
        General3 = 21,

        /// <summary>
        /// Generic Phase 4.
        /// </summary>
        General4 = 22,

        /// <summary>
        /// Generic Phase 5.
        /// </summary>
        General5 = 23,

        /// <summary>
        /// Generic Phase 6.
        /// </summary>
        General6 = 24,

        /// <summary>
        /// Generic Phase 7.
        /// </summary>
        General7 = 25,

        /// <summary>
        /// Generic Phase 8.
        /// </summary>
        General8 = 26,

        /// <summary>
        /// Generic Phase 9.
        /// </summary>
        General9 = 27,

        /// <summary>
        /// Generic Phase 10.
        /// </summary>
        General10 = 28,

        /// <summary>
        /// Generic Phase 11.
        /// </summary>
        General11 = 29,

        /// <summary>
        /// Generic Phase 12.
        /// </summary>
        General12 = 30,

        /// <summary>
        /// Generic Phase 13.
        /// </summary>
        General13 = 31,

        /// <summary>
        /// Generic Phase 14.
        /// </summary>
        General14 = 32,

        /// <summary>
        /// Generic Phase 15.
        /// </summary>
        General15 = 33,

        /// <summary>
        /// Generic Phase 16.
        /// </summary>
        General16 = 34
    }

    /// <summary>
    /// Physical quantity under measurement.
    /// </summary>
    public enum QuantityMeasured : uint
    {
        /// <summary>
        /// None or not applicable.
        /// </summary>
        None = 0,

        /// <summary>
        /// Voltage.
        /// </summary>
        Voltage = 1,

        /// <summary>
        /// Current.
        /// </summary>
        Current = 2,

        /// <summary>
        /// Power - includes all data for a quantity or characteristic
        /// derived from multiplying voltage and current components.
        /// </summary>
        Power = 3,

        /// <summary>
        /// Energy - includes all data from an integration of a quantity
        /// or characteristic derived from multiplying voltage and current
        /// components together.
        /// </summary>
        Energy = 4,

        /// <summary>
        /// Temperature.
        /// </summary>
        Temperature = 5,

        /// <summary>
        /// Pressure.
        /// </summary>
        Pressure = 6,

        /// <summary>
        /// Charge.
        /// </summary>
        Charge = 7,

        /// <summary>
        /// Electrical field.
        /// </summary>
        ElectricalField = 8,

        /// <summary>
        /// Magnetic field.
        /// </summary>
        MagneticField = 9,

        /// <summary>
        /// Velocity.
        /// </summary>
        Velocity = 10,

        /// <summary>
        /// Compass bearing.
        /// </summary>
        Bearing = 11,

        /// <summary>
        /// Applied force, electrical, mechanical etc.
        /// </summary>
        Force = 12,

        /// <summary>
        /// Torque.
        /// </summary>
        Torque = 13,

        /// <summary>
        /// Spatial position.
        /// </summary>
        Position = 14,

        /// <summary>
        /// Flux linkage Weber Turns.
        /// </summary>
        FluxLinkage = 15,

        /// <summary>
        /// Magnetic field density.
        /// </summary>
        FluxDensity = 16,

        /// <summary>
        /// Status data.
        /// </summary>
        Status = 17
    }

    #endregion

    /// <summary>
    /// Represents a channel definition in a PQDIF file. The channel
    /// definition resides within a <see cref="DataSourceRecord"/> and
    /// defines a <see cref="ChannelInstance"/>.
    /// </summary>
    public class ChannelDefinition : IEquatable<ChannelDefinition>
    {
        #region [ Members ]

        // Fields
        private readonly CollectionElement m_physicalStructure;
        private readonly DataSourceRecord m_dataSource;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelDefinition"/> class.
        /// </summary>
        /// <param name="physicalStructure">The collection element which is the physical structure of the channel definition.</param>
        /// <param name="dataSource">The data source in which the channel definition resides.</param>
        public ChannelDefinition(CollectionElement physicalStructure, DataSourceRecord dataSource)
        {
            m_dataSource = dataSource;
            m_physicalStructure = physicalStructure;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the physical structure of the channel definition.
        /// </summary>
        public CollectionElement PhysicalStructure
        {
            get
            {
                return m_physicalStructure;
            }
        }

        /// <summary>
        /// Gets the data source record in which
        /// the channel definition resides.
        /// </summary>
        public DataSourceRecord DataSource
        {
            get
            {
                return m_dataSource;
            }
        }

        /// <summary>
        /// Gets a string identifier for the channel definition.
        /// </summary>
        public string ChannelName
        {
            get
            {
                VectorElement channelNameElement = m_physicalStructure.GetVectorByTag(ChannelNameTag);

                if ((object)channelNameElement == null)
                    return null;

                return Encoding.ASCII.GetString(channelNameElement.GetValues()).Trim((char)0);
            }
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                m_physicalStructure.AddOrUpdateVector(ChannelNameTag, PhysicalType.Char1, bytes);
            }
        }

        /// <summary>
        /// Gets the phase measured by the device.
        /// </summary>
        public Phase Phase
        {
            get
            {
                return (Phase)m_physicalStructure
                    .GetScalarByTag(PhaseIDTag)
                    .GetUInt4();
            }
            set
            {
                ScalarElement phaseIDElement = m_physicalStructure.GetOrAddScalar(PhaseIDTag);
                phaseIDElement.TypeOfValue = PhysicalType.UnsignedInteger4;
                phaseIDElement.SetUInt4((uint)value);
            }
        }

        /// <summary>
        /// Gets the quantity type ID, which specifies how the data
        /// inside instances of this definition should be interpreted.
        /// </summary>
        public Guid QuantityTypeID
        {
            get
            {
                return m_physicalStructure
                    .GetScalarByTag(QuantityTypeIDTag)
                    .GetGuid();
            }
            set
            {
                ScalarElement quantityTypeIDElement = m_physicalStructure.GetOrAddScalar(QuantityTypeIDTag);
                quantityTypeIDElement.TypeOfValue = PhysicalType.Guid;
                quantityTypeIDElement.SetGuid(value);
            }
        }

        /// <summary>
        /// Gets the physical quantity under measurement
        /// (Voltage, Current Power, etc).
        /// </summary>
        public QuantityMeasured QuantityMeasured
        {
            get
            {
                return (QuantityMeasured)m_physicalStructure
                    .GetScalarByTag(QuantityMeasuredIDTag)
                    .GetUInt4();
            }
            set
            {
                ScalarElement quantityMeasuredIDElement = m_physicalStructure.GetOrAddScalar(QuantityMeasuredIDTag);
                quantityMeasuredIDElement.TypeOfValue = PhysicalType.UnsignedInteger4;
                quantityMeasuredIDElement.SetUInt4((uint)value);
            }
        }

        /// <summary>
        /// Gets the name of the quantity.
        /// </summary>
        public string QuantityName
        {
            get
            {
                VectorElement quantityNameElement = m_physicalStructure.GetVectorByTag(QuantityNameTag);

                if ((object)quantityNameElement == null)
                    return null;

                return Encoding.ASCII.GetString(quantityNameElement.GetValues()).Trim((char)0);
            }
            set
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value + (char)0);
                m_physicalStructure.AddOrUpdateVector(QuantityNameTag, PhysicalType.Char1, bytes);
            }
        }

        /// <summary>
        /// Gets the series definitions defined in this channel definition.
        /// </summary>
        public IList<SeriesDefinition> SeriesDefinitions
        {
            get
            {
                return m_physicalStructure.GetCollectionByTag(SeriesDefinitionsTag)
                    .GetElementsByTag(OneSeriesDefinitionTag)
                    .Cast<CollectionElement>()
                    .Select(collection => new SeriesDefinition(collection, this))
                    .ToList();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Adds a new series definition to the collection
        /// of series definitions in this channel definition.
        /// </summary>
        public SeriesDefinition AddNewSeriesDefinition()
        {
            CollectionElement seriesDefinitionElement = new CollectionElement() { TagOfElement = OneSeriesDefinitionTag };
            SeriesDefinition seriesDefinition = new SeriesDefinition(seriesDefinitionElement, this);

            seriesDefinition.ValueTypeID = SeriesValueType.Val;
            seriesDefinition.QuantityUnits = QuantityUnits.None;
            seriesDefinition.QuantityCharacteristicID = QuantityCharacteristic.None;
            seriesDefinition.StorageMethodID = StorageMethods.Values;

            CollectionElement seriesDefinitionsElement = m_physicalStructure.GetCollectionByTag(SeriesDefinitionsTag);

            if ((object)seriesDefinitionsElement == null)
            {
                seriesDefinitionsElement = new CollectionElement() { TagOfElement = SeriesDefinitionsTag };
                m_physicalStructure.AddElement(seriesDefinitionsElement);
            }

            seriesDefinitionsElement.AddElement(seriesDefinitionElement);

            return seriesDefinition;
        }

        /// <summary>
        /// Removes the given series definition from the collection of series definitions.
        /// </summary>
        /// <param name="seriesDefinition">The series definition to be removed.</param>
        public void Remove(SeriesDefinition seriesDefinition)
        {
            CollectionElement seriesDefinitionsElement = m_physicalStructure.GetCollectionByTag(SeriesDefinitionsTag);
            List<CollectionElement> seriesDefinitionElements;
            SeriesDefinition definition;

            if ((object)seriesDefinitionsElement == null)
                return;

            seriesDefinitionElements = seriesDefinitionsElement.GetElementsByTag(OneSeriesDefinitionTag).Cast<CollectionElement>().ToList();

            foreach (CollectionElement seriesDefinitionElement in seriesDefinitionElements)
            {
                definition = new SeriesDefinition(seriesDefinitionElement, this);

                if (Equals(seriesDefinition, definition))
                    seriesDefinitionsElement.RemoveElement(seriesDefinitionElement);
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public bool Equals(ChannelDefinition other)
        {
            if ((object)other == null)
                return false;

            return ReferenceEquals(m_physicalStructure, other.m_physicalStructure);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="ChannelDefinition"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as ChannelDefinition);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current <see cref="ChannelDefinition"/>.</returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return m_physicalStructure.GetHashCode();
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Tag that identifies the channel definition index.
        /// </summary>
        public static readonly Guid ChannelDefinitionIndexTag = new Guid("b48d858f-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the channel name.
        /// </summary>
        public static readonly Guid ChannelNameTag = new Guid("b48d8590-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the phase ID.
        /// </summary>
        public static readonly Guid PhaseIDTag = new Guid("b48d8591-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the quantity type.
        /// </summary>
        public static readonly Guid QuantityTypeIDTag = new Guid("b48d8592-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the quantity measured ID.
        /// </summary>
        public static readonly Guid QuantityMeasuredIDTag = new Guid("c690e872-f755-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the quantity name.
        /// </summary>
        public static readonly Guid QuantityNameTag = new Guid("b48d8595-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies the series definitions collection.
        /// </summary>
        public static readonly Guid SeriesDefinitionsTag = new Guid("b48d8598-f5f5-11cf-9d89-0080c72e70a3");

        /// <summary>
        /// Tag that identifies a single series definition within the collection.
        /// </summary>
        public static readonly Guid OneSeriesDefinitionTag = new Guid("b48d859a-f5f5-11cf-9d89-0080c72e70a3");

        // Static Methods

        /// <summary>
        /// Creates a new channel definition belonging to the given data source record.
        /// </summary>
        /// <param name="dataSourceRecord">The data source record that the new channel definition belongs to.</param>
        /// <returns>The new channel definition.</returns>
        public static ChannelDefinition CreateChannelDefinition(DataSourceRecord dataSourceRecord)
        {
            ChannelDefinition channelDefinition = dataSourceRecord.AddNewChannelDefinition();
            channelDefinition.Phase = Phase.None;
            channelDefinition.QuantityMeasured = QuantityMeasured.None;

            CollectionElement physicalStructure = channelDefinition.PhysicalStructure;
            physicalStructure.AddElement(new CollectionElement() { TagOfElement = SeriesDefinitionsTag });

            return channelDefinition;
        }

        #endregion
    }
}
