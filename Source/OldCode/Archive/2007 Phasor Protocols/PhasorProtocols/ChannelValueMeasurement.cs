//*******************************************************************************************************
//  ChannelValueMeasurement.cs
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
using System.Collections.Generic;
using PCS;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents an <see cref="IMeasurement"/> implementation for composite values of a given <see cref="IChannelValue{T}"/>.
    /// </summary>
    internal class ChannelValueMeasurement<T> : IMeasurement where T : IChannelDefinition
    {
        #region [ Members ]

        // Fields
        private IChannelValue<T> m_parent;
        private int m_id;
        private string m_source;
        private MeasurementKey m_key;
        private string m_tagName;
        private Ticks m_ticks;
        private int m_valueIndex;
        private double m_adder;
        private double m_multiplier;
        private int m_dataQualityIsGood;
        private int m_timeQualityIsGood;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="ChannelValueMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the new measurement.</param>
        /// <param name="source">Source name of the new measurement.</param>
        public ChannelValueMeasurement(IChannelValue<T> parent, int valueIndex)
        {
            m_parent = parent;
            m_valueIndex = valueIndex;
            m_id = -1;
            m_source = "__";
            m_key = Common.UndefinedKey;
            m_ticks = -1;
            m_multiplier = 1.0D;
            m_dataQualityIsGood = -1;
            m_timeQualityIsGood = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.
        /// </summary>
        protected IChannelValue<T> Parent
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
        /// Gets or sets the numeric ID of this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to <see cref="Source"/>, typically creates the primary key for a <see cref="ChannelValueMeasurement{T}"/>.</para>
        /// </remarks>
        public virtual int ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets the source of this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most implementations, this will be a required field.</para>
        /// <para>Note that this field, in addition to <see cref="ID"/>, typically creates the primary key for a <see cref="ChannelValueMeasurement{T}"/>.</para>
        /// <para>This value is typically used to track the archive name in which measurement is stored.</para>
        /// </remarks>
        public virtual string Source
        {
            get
            {
                return m_source;
            }
            set
            {
                m_source = value;
            }
        }

        /// <summary>
        /// Gets the primary key (a <see cref="MeasurementKey"/>, of this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        public virtual MeasurementKey Key
        {
            get
            {
                if (m_key.Equals(Common.UndefinedKey))
                    m_key = new MeasurementKey(m_id, m_source);

                return m_key;
            }
        }

        /// <summary>
        /// Gets or sets the text based tag name of this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        public virtual string TagName
        {
            get
            {
                return m_tagName;
            }
            set
            {
                m_tagName = value;
            }
        }

        /// <summary>
        /// Gets or sets index into the <see cref="IChannelValue{T}.CompositeValues"/> that this measurement derives its value from.
        /// </summary>
        public virtual int ValueIndex
        {
            get
            {
                return m_valueIndex;
            }
            set
            {
                m_valueIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the raw measurement value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="ChannelValueMeasurement{T}"/> (i.e., value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>).</returns>
        public virtual double Value
        {
            get
            {
                return m_parent.CompositeValues[m_valueIndex];
            }
            set
            {
                m_parent.CompositeValues[m_valueIndex] = value;
            }
        }

        /// <summary>
        /// Gets the adjusted numeric value of this measurement, taking into account the specified <see cref="Adder"/> and <see cref="Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// Note that returned value will be offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </remarks>
        /// <returns><see cref="Value"/> offset by <see cref="Adder"/> and <see cref="Multiplier"/> (i.e., <c><see cref="Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>).</returns>
        public virtual double AdjustedValue
        {
            get
            {
                return m_parent.CompositeValues[m_valueIndex] * m_multiplier + m_adder;
            }
        }

        /// <summary>
        /// Gets or sets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        public virtual double Adder
        {
            get
            {
                return m_adder;
            }
            set
            {
                m_adder = value;
            }
        }

        /// <summary>
        /// Defines a mulplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        public virtual double Multiplier
        {
            get
            {
                return m_multiplier;
            }
            set
            {
                m_multiplier = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value determining if the quality of the timestamp of this <see cref="ChannelValueMeasurement{T}"/> is good.
        /// </summary>
        /// <remarks>This value returns timestamp quality of parent data cell unless assigned an alternate value</remarks>
        public virtual bool TimestampQualityIsGood
        {
            get
            {
                if (m_timeQualityIsGood == -1)
                    return (m_parent.Parent.SynchronizationIsValid && Timestamp != -1);

                return (m_timeQualityIsGood != 0);
            }
            set
            {
                m_timeQualityIsGood = value ? 1 : 0;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value determining if the quality of the numeric value of this <see cref="ChannelValueMeasurement{T}"/> is good.
        /// </summary>
        /// <remarks>This value returns data quality of parent data cell unless assigned an alternate value</remarks>
        public virtual bool ValueQualityIsGood
        {
            get
            {
                if (m_dataQualityIsGood == -1)
                    return m_parent.Parent.DataIsValid;

                return (m_dataQualityIsGood != 0);
            }
            set
            {
                m_dataQualityIsGood = value ? 1 : 0;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <remarks>
        /// This value returns timestamp of parent data cell unless assigned an alternate value.<br/>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public virtual Ticks Timestamp
        {
            get
            {
                if (m_ticks == -1)
                    m_ticks = m_parent.Parent.Parent.Timestamp;

                return m_ticks;
            }
            set
            {
                m_ticks = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="ChannelValueMeasurement{T}"/>.</returns>
        public override string ToString()
        {
            return Measurement.ToString(this);
        }

        /// <summary>
        /// Determines whether the specified <see cref="IMeasurement"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <param name="other">The <see cref="IMeasurement"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>
        /// true if the specified <see cref="IMeasurement"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(IMeasurement other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        public override bool Equals(object obj)
        {
            IMeasurement other = obj as IMeasurement;

            if (other != null)
                return Equals(other);

            throw new ArgumentException("Object is not a Measurement");
        }

        /// <summary>
        /// Compares the <see cref="ChannelValueMeasurement{T}"/> with an <see cref="IMeasurement"/>.
        /// </summary>
        /// <param name="other">The <see cref="IMeasurement"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>This implementation of a basic measurement compares itself by value.</remarks>
        public int CompareTo(IMeasurement other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Compares the <see cref="ChannelValueMeasurement{T}"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        /// <remarks>This implementation of a basic measurement compares itself by value.</remarks>
        public int CompareTo(object obj)
        {
            IMeasurement other = obj as IMeasurement;

            if (other != null)
                return CompareTo(other);

            throw new ArgumentException("Measurement can only be compared with other IMeasurements...");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ChannelValueMeasurement{T}"/>.</returns>
        /// <remarks>Hash code based on value of measurement.</remarks>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #endregion
    }
}