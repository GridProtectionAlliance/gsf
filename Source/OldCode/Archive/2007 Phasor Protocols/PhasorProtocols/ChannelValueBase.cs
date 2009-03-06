//*******************************************************************************************************
//  ChannelValueBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/07/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent representation of any kind of data value.
    /// </summary>
    [Serializable()]
    public abstract class ChannelValueBase<T> : ChannelBase, IChannelValue<T> where T : IChannelDefinition
    {
        #region [ Members ]

        // Fields
        private IDataCell m_parent;
        private T m_definition;
        private IMeasurement[] m_measurements;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelValueBase{T}"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="ChannelValueBase{T}"/>.</param>
        /// <param name="channelDefinition">The <see cref="IChannelDefinition"/> associated with this <see cref="ChannelValueBase{T}"/>.</param>
        protected ChannelValueBase(IDataCell parent, T channelDefinition)
        {
            m_parent = parent;
            m_definition = channelDefinition;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelValueBase{T}"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelValueBase(SerializationInfo info, StreamingContext context)
        {
            // Deserialize channel value
            m_parent = (IDataCell)info.GetValue("parent", typeof(IDataCell));
            m_definition = (T)info.GetValue("definition", typeof(T));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IDataCell"/> parent of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual IDataCell Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IChannelDefinition"/> associated with this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual T Definition
        {
            get
            {
                return m_definition;
            }
            set
            {
                m_definition = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual DataFormat DataFormat
        {
            get
            {
                return m_definition.DataFormat;
            }
        }

        /// <summary>
        /// Gets text based label of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual string Label
        {
            get
            {
                return m_definition.Label;
            }
        }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="ChannelValueBase{T}"/> have been assigned a value.
        /// </summary>
        /// <returns><c>true</c>, if no composite values have been assigned a value; otherwise, <c>false</c>.</returns>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Gets the composite values of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Some <see cref="ChannelValueBase{T}"/> implementations can contain more than one value, this property is used to abstractly expose each value.
        /// </remarks>
        public abstract double[] CompositeValues { get; }

        /// <summary>
        /// Gets the <see cref="CompositeValues"/> of this <see cref="ChannelValueBase{T}"/> as an array of <see cref="IMeasurement"/> values.
        /// </summary>
        public virtual IMeasurement[] Measurements
        {
            get
            {
                // Create a measurement instance for each composite value the derived channel value exposes
                if (m_measurements == null)
                {
                    m_measurements = new IMeasurement[CompositeValues.Length];

                    for (int x = 0; x < m_measurements.Length; x++)
                    {
                        // ChannelValueMeasurement dynamically accesses CompositeValues[x] for its value
                        m_measurements[x] = new ChannelValueMeasurement<T>(this, x);
                    }
                }

                return m_measurements;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ChannelValueBase{T}"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;
                double[] compositeValues = CompositeValues;

                baseAttributes.Add("Label", Label);
                baseAttributes.Add("Data Format", (int)DataFormat + ": " + DataFormat);
                baseAttributes.Add("Is Empty", IsEmpty.ToString());
                baseAttributes.Add("Total Composite Values", compositeValues.Length.ToString());

                for (int x = 0; x < compositeValues.Length; x++)
                {
                    baseAttributes.Add("     Composite Value " + x, compositeValues[x].ToString());
                }

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize channel value
            info.AddValue("parent", m_parent, typeof(IDataCell));
            info.AddValue("definition", m_definition, typeof(T));
        }

        #endregion
    }
}