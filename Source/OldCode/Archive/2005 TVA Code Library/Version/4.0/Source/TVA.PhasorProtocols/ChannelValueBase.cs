//*******************************************************************************************************
//  ChannelValueBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/07/2005 - James R. Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using TVA.Measurements;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent representation of any kind of data value.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
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
        /// Gets total number of composite values that this <see cref="ChannelValueBase{T}"/> provides.
        /// </summary>
        public abstract int CompositeValueCount { get; }

        /// <summary>
        /// Gets the composite values of this <see cref="ChannelValueBase{T}"/> as an array of <see cref="IMeasurement"/> values.
        /// </summary>
        public virtual IMeasurement[] Measurements
        {
            get
            {
                // Create a measurement instance for each composite value the derived channel value exposes
                if (m_measurements == null)
                {
                    m_measurements = new IMeasurement[CompositeValueCount];

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
                int compositeValues = CompositeValueCount;

                baseAttributes.Add("Label", Label);
                baseAttributes.Add("Data Format", (int)DataFormat + ": " + DataFormat);
                baseAttributes.Add("Is Empty", IsEmpty.ToString());
                baseAttributes.Add("Total Composite Values", compositeValues.ToString());

                for (int x = 0; x < compositeValues; x++)
                {
                    baseAttributes.Add("     Composite Value " + x, " => " + GetCompositeValue(x).ToString());
                }

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the specified composite value of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        /// <remarks>
        /// Some <see cref="ChannelValueBase{T}"/> implementations can contain more than one value, this method is used to abstractly expose each value.
        /// </remarks>
        /// <returns>A <see cref="Double"/> representing the composite value.</returns>
        public abstract double GetCompositeValue(int index);

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize channel value
            info.AddValue("parent", m_parent, typeof(IDataCell));
            info.AddValue("definition", m_definition, typeof(T));
        }

        #endregion
    }
}