//*******************************************************************************************************
//  ChannelValueMeasurement.vb - Channel data value measurement class
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  3/7/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using PCS;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the protocol independent representation of any kind of data value as an abstract measurement.</summary>
    internal class ChannelValueMeasurement<T> : IMeasurement where T : IChannelDefinition
    {
        private IChannelValue<T> m_parent;
        private int m_id;
        private string m_source;
        private MeasurementKey m_key;
        private string m_tagName;
        private long m_ticks;
        private int m_valueIndex;
        private double m_adder;
        private double m_multiplier;
        private int m_dataQualityIsGood;
        private int m_timeQualityIsGood;

        protected ChannelValueMeasurement()
        {
        }

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

        public virtual MeasurementKey Key
        {
            get
            {
                if (m_key.Equals(Common.UndefinedKey))
                {
                    m_key = new MeasurementKey(m_id, m_source);
                }
                return m_key;
            }
        }

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

        public virtual double Value
        {
            get
            {
                return m_parent[m_valueIndex];
            }
            set
            {
                m_parent[m_valueIndex] = (float)value;
            }
        }

        public virtual double AdjustedValue
        {
            get
            {
                return m_parent[m_valueIndex] * m_multiplier + m_adder;
            }
        }

        /// <summary>Defines an offset to add to the measurement value</summary>
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

        /// <summary>Defines a mulplicative offset to add to the measurement value</summary>
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

        /// <summary>Determines if the quality of the timestamp of this measurement is good</summary>
        /// <remarks>This value returns timestamp quality of parent data cell unless assigned an alternate value</remarks>
        public virtual bool TimestampQualityIsGood
        {
            get
            {
                if (m_timeQualityIsGood == -1)
                {
                    return (m_parent.Parent.SynchronizationIsValid && Ticks != -1);
                }
                return (m_timeQualityIsGood != 0);
            }
            set
            {
                m_timeQualityIsGood = value ? 1 : 0;
            }
        }

        /// <summary>Determines if the quality of the numeric value of this measurement is good</summary>
        /// <remarks>This value returns data quality of parent data cell unless assigned an alternate value</remarks>
        public virtual bool ValueQualityIsGood
        {
            get
            {
                if (m_dataQualityIsGood == -1)
                {
                    return m_parent.Parent.DataIsValid;
                }
                return (m_dataQualityIsGood != 0);
            }
            set
            {
                m_dataQualityIsGood = value ? 1 : 0;
            }
        }

        /// <summary>Gets or sets exact timestamp of the data represented by this measurement</summary>
        /// <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
        public virtual long Ticks
        {
            get
            {
                if (m_ticks == -1)
                {
                    m_ticks = m_parent.Parent.Parent.Ticks;
                }
                return m_ticks;
            }
            set
            {
                m_ticks = value;
            }
        }

        public virtual DateTime Timestamp
        {
            get
            {
                long ticks = this.Ticks;

                if (ticks == -1)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    return new DateTime(ticks);
                }
            }
        }

        /// <summary>This implementation of a basic measurement compares itself by value</summary>
        public virtual int CompareTo(object obj)
        {
            IMeasurement measurement = obj as IMeasurement;

            if (measurement != null) return CompareTo(measurement);

            throw (new ArgumentException(m_parent.DerivedType.Name + " measurement can only be compared with other IMeasurements..."));
        }

        /// <summary>This implementation of a basic measurement compares itself by value</summary>
        public virtual int CompareTo(IMeasurement other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>Returns True if the value of this measurement equals the value of the specified other measurement</summary>
        public virtual bool Equals(IMeasurement other)
        {
            return (CompareTo(other) == 0);
        }
    }
}
