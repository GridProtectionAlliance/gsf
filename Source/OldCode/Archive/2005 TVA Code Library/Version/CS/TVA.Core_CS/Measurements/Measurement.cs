using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.ComponentModel;

//*******************************************************************************************************
//  TVA.Measurements.Measurement.vb - Basic measurement implementation
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  This class represents a basic measured value
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/8/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace TVA
{
    namespace Measurements
    {

        /// <summary>Implementation of a basic measured value</summary>
        public class Measurement : IMeasurement
        {



            private int m_id;
            private string m_source;
            private MeasurementKey m_key;
            private string m_tagName;
            private double m_value;
            private double m_adder;
            private double m_multiplier;
            private long m_ticks;
            private bool m_valueQualityIsGood;
            private bool m_timestampQualityIsGood;

            public Measurement()
                : this(-1, null, double.NaN, 0)
            {


            }

            public Measurement(int id, string source)
                : this(id, source, double.NaN, 0.0, 1.0, 0)
            {


            }


            public Measurement(int id, string source, double value, DateTime timestamp)
                : this(id, source, value, timestamp.Ticks)
            {


            }

            public Measurement(int id, string source, double value, long ticks)
                : this(id, source, value, 0.0, 1.0, ticks)
            {


            }

            public Measurement(int id, string source, string tagName, double adder, double multiplier)
                : this(id, source, double.NaN, adder, multiplier, 0)
            {

                m_tagName = tagName;

            }

            public Measurement(int id, string source, double value, double adder, double multiplier, long ticks)
            {

                m_id = id;
                m_source = source;
                m_key = new MeasurementKey(m_id, m_source);
                m_value = value;
                m_adder = adder;
                m_multiplier = multiplier;
                m_ticks = ticks;
                m_valueQualityIsGood = true;
                m_timestampQualityIsGood = true;

            }

            /// <summary>Handy instance reference to self</summary>
            public virtual IMeasurement This
            {
                get
                {
                    return this;
                }
            }

            /// <summary>Creates a copy of the specified measurement</summary>
            public static Measurement Clone(IMeasurement measurementToClone)
            {

                IMeasurement with_1 = measurementToClone;
                return new Measurement(with_1.ID, with_1.Source, with_1.Value, with_1.Adder, with_1.Multiplier, with_1.Ticks);

            }

            /// <summary>Creates a copy of the specified measurement using a new timestamp</summary>
            public static Measurement Clone(IMeasurement measurementToClone, long ticks)
            {

                IMeasurement with_1 = measurementToClone;
                return new Measurement(with_1.ID, with_1.Source, with_1.Value, with_1.Adder, with_1.Multiplier, ticks);

            }

            /// <summary>Creates a copy of the specified measurement using a new value and timestamp</summary>
            public static Measurement Clone(IMeasurement measurementToClone, double value, long ticks)
            {

                IMeasurement with_1 = measurementToClone;
                return new Measurement(with_1.ID, with_1.Source, value, with_1.Adder, with_1.Multiplier, ticks);

            }

            /// <summary>Gets or sets the numeric ID of this measurement</summary>
            /// <remarks>
            /// <para>In most implementations, this will be a required field</para>
            /// <para>Note that this field, in addition to Source, typically creates the primary key for a measurement</para>
            /// </remarks>
            public virtual int ID
            {
                get
                {
                    return m_id;
                }
                set
                {
                    if (m_id != value)
                    {
                        m_id = value;
                        m_key = new MeasurementKey(m_id, m_source);
                    }
                }
            }

            /// <summary>Gets or sets the source of this measurement</summary>
            /// <remarks>
            /// <para>In most implementations, this will be a required field</para>
            /// <para>Note that this field, in addition to ID, typically creates the primary key for a measurement</para>
            /// <para>This value is typically used to track the archive name in which measurement is stored</para>
            /// </remarks>
            public virtual string Source
            {
                get
                {
                    return m_source;
                }
                set
                {
                    if (m_source != value)
                    {
                        m_source = value;
                        m_key = new MeasurementKey(m_id, m_source);
                    }
                }
            }

            /// <summary>Returns the primary key of this measurement</summary>
            public MeasurementKey Key
            {
                get
                {
                    return m_key;
                }
            }

            /// <summary>Gets or sets the text based tag name of this measurement</summary>
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

            /// <summary>Gets or sets the raw measurement value that is not offset by adder and multiplier</summary>
            /// <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier)</returns>
            public double Value
            {
                get
                {
                    return m_value;
                }
                set
                {
                    m_value = value;
                }
            }

            /// <summary>Returns the adjusted numeric value of this measurement</summary>
            /// <remarks>Note that returned value will be offset by adder and multiplier</remarks>
            /// <returns>Value offset by adder and multipler (i.e., Value * Multiplier + Adder)</returns>
            public virtual double AdjustedValue
            {
                get
                {
                    return m_value * m_multiplier + m_adder;
                }
            }

            /// <summary>Defines an offset to add to the measurement value - defaults to zero</summary>
            [DefaultValue(0.0)]
            public double Adder
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

            /// <summary>Defines a mulplicative offset to add to the measurement value - defaults to one</summary>
            [DefaultValue(1.0)]
            public double Multiplier
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

            /// <summary>Gets or sets exact timestamp of the data represented by this measurement</summary>
            /// <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
            public virtual long Ticks
            {
                get
                {
                    return m_ticks;
                }
                set
                {
                    m_ticks = value;
                }
            }

            /// <summary>Date representation of ticks of this measurement</summary>
            public virtual DateTime Timestamp
            {
                get
                {
                    return new DateTime(m_ticks);
                }
            }

            /// <summary>Determines if the quality of the numeric value of this measurement is good</summary>
            public virtual bool ValueQualityIsGood
            {
                get
                {
                    return m_valueQualityIsGood;
                }
                set
                {
                    m_valueQualityIsGood = value;
                }
            }

            /// <summary>Determines if the quality of the timestamp of this measurement is good</summary>
            public virtual bool TimestampQualityIsGood
            {
                get
                {
                    return m_timestampQualityIsGood;
                }
                set
                {
                    m_timestampQualityIsGood = value;
                }
            }

            public override string ToString()
            {

                return ToString(this);

            }

            public static string ToString(IMeasurement measurement)
            {

                if (measurement == null)
                {
                    return "Undefined";
                }
                else
                {
                    string tagName = measurement.TagName;
                    string keyText = measurement.Key.ToString();

                    if (string.IsNullOrEmpty(tagName))
                    {
                        return keyText;
                    }
                    else
                    {
                        return string.Format("{0} [{1}]", tagName, keyText);
                    }
                }

            }

            /// <summary>Returns True if the value of this measurement equals the value of the specified other measurement</summary>
            public bool Equals(IMeasurement other)
            {

                return (CompareTo(other) == 0);

            }

            /// <summary>Returns True if the value of this measurement equals the value of the specified other measurement</summary>
            public override bool Equals(object obj)
            {

                IMeasurement other = obj as IMeasurement;
                if (other != null)
                {
                    return Equals(other);
                }
                throw (new ArgumentException("Object is not a Measurement"));

            }

            /// <summary>This implementation of a basic measurement compares itself by value</summary>
            public int CompareTo(IMeasurement other)
            {

                return m_value.CompareTo(other.Value);

            }

            /// <summary>This implementation of a basic measurement compares itself by value</summary>
            public int CompareTo(object obj)
            {

                IMeasurement other = obj as IMeasurement;
                if (other != null)
                {
                    return CompareTo(other);
                }
                throw (new ArgumentException("Measurement can only be compared with other IMeasurements..."));

            }

            #region " Measurement Operators "

            public static bool operator ==(Measurement measurement1, Measurement measurement2)
            {

                return measurement1.Equals(measurement2);

            }

            public static bool operator !=(Measurement measurement1, Measurement measurement2)
            {

                return !measurement1.Equals(measurement2);

            }

            public static bool operator >(Measurement measurement1, Measurement measurement2)
            {

                return measurement1.CompareTo(measurement2) > 0;

            }

            public static bool operator >=(Measurement measurement1, Measurement measurement2)
            {

                return measurement1.CompareTo(measurement2) >= 0;

            }

            public static bool operator <(Measurement measurement1, Measurement measurement2)
            {

                return measurement1.CompareTo(measurement2) < 0;

            }

            public static bool operator <=(Measurement measurement1, Measurement measurement2)
            {

                return measurement1.CompareTo(measurement2) <= 0;

            }

            #endregion

        }

    }

}
