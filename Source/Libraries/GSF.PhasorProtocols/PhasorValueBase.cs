//******************************************************************************************************
//  PhasorValueBase.cs - Gbtc
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.TimeSeries;
using GSF.Units;
using GSF.Units.EE;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Composite polar value indices enumeration.
    /// </summary>
    public enum CompositePhasorValue
    {
        /// <summary>
        /// Composite angle value index.
        /// </summary>
        Angle,
        /// <summary>
        /// Composite magnitude value index.
        /// </summary>
        Magnitude
    }

    #endregion

    /// <summary>
    /// Represents the common implementation of the protocol independent representation of a phasor value.
    /// </summary>
    [Serializable]
    public abstract class PhasorValueBase : ChannelValueBase<IPhasorDefinition>, IPhasorValue
    {
        #region [ Members ]

        // Fields
        private ComplexNumber m_phasor;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorValueBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="PhasorValueBase"/>.</param>
        /// <param name="phasorDefinition">The <see cref="IPhasorDefinition"/> associated with this <see cref="PhasorValueBase"/>.</param>
        protected PhasorValueBase(IDataCell parent, IPhasorDefinition phasorDefinition)
            : base(parent, phasorDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValueBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="PhasorValueBase"/>.</param>
        /// <param name="phasorDefinition">The <see cref="IPhasorDefinition"/> associated with this <see cref="PhasorValueBase"/>.</param>
        /// <param name="real">The real value of this <see cref="PhasorValueBase"/>.</param>
        /// <param name="imaginary">The imaginary value of this <see cref="PhasorValueBase"/>.</param>
        protected PhasorValueBase(IDataCell parent, IPhasorDefinition phasorDefinition, double real, double imaginary)
            : base(parent, phasorDefinition)
        {
            m_phasor.Real = real;
            m_phasor.Imaginary = imaginary;
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValueBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="PhasorValueBase"/>.</param>
        /// <param name="phasorDefinition">The <see cref="IPhasorDefinition"/> associated with this <see cref="PhasorValueBase"/>.</param>
        /// <param name="angle">The <see cref="GSF.Units.Angle"/> value (a.k.a., the argument) of this <see cref="PhasorValueBase"/>, in radians.</param>
        /// <param name="magnitude">The magnitude value (a.k.a., the absolute value or modulus) of this <see cref="PhasorValueBase"/>.</param>
        protected PhasorValueBase(IDataCell parent, IPhasorDefinition phasorDefinition, Angle angle, double magnitude)
            : base(parent, phasorDefinition)
        {
            m_phasor.Angle = angle;
            m_phasor.Magnitude = magnitude;
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValueBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize phasor value
            m_phasor.Real = info.GetDouble("real");
            m_phasor.Imaginary = info.GetDouble("imaginary");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="GSF.PhasorProtocols.CoordinateFormat"/> of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual CoordinateFormat CoordinateFormat => Definition.CoordinateFormat;

        /// <summary>
        /// Gets the <see cref="GSF.PhasorProtocols.AngleFormat"/> of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual AngleFormat AngleFormat => Definition.AngleFormat;

        /// <summary>
        /// Gets the <see cref="PhasorType"/> of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual PhasorType Type => Definition.PhasorType;

        /// <summary>
        /// Gets <see cref="GSF.Units.EE.Phasor"/> value from this <see cref="IPhasorValue"/>.
        /// </summary>
        public Phasor Phasor => new Phasor(Type, m_phasor);

        /// <summary>
        /// Gets or sets the <see cref="GSF.Units.Angle"/> value (a.k.a., the argument) of this <see cref="PhasorValueBase"/>, in radians.
        /// </summary>
        public virtual Angle Angle
        {
            get
            {
                return m_phasor.Angle;
            }
            set
            {
                m_phasor.Angle = value;
            }
        }

        /// <summary>
        /// Gets or sets the magnitude value (a.k.a., the absolute value or modulus) of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual double Magnitude
        {
            get
            {
                return m_phasor.Magnitude;
            }
            set
            {
                m_phasor.Magnitude = value;
            }
        }

        /// <summary>
        /// Gets or sets the real value of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual double Real
        {
            get
            {
                return m_phasor.Real;
            }
            set
            {
                m_phasor.Real = value;
            }
        }

        /// <summary>
        /// Gets or sets the imaginary value of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual double Imaginary
        {
            get
            {
                return m_phasor.Imaginary;
            }
            set
            {
                m_phasor.Imaginary = value;
            }
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the real value of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual int UnscaledReal
        {
            get
            {
                unchecked
                {
                    return (int)(m_phasor.Real / Definition.ConversionFactor);
                }
            }
            set
            {
                m_phasor.Real = value * Definition.ConversionFactor;
            }
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the imaginary value of this <see cref="PhasorValueBase"/>.
        /// </summary>
        public virtual int UnscaledImaginary
        {
            get
            {
                unchecked
                {
                    return (int)(m_phasor.Imaginary / Definition.ConversionFactor);
                }
            }
            set
            {
                m_phasor.Imaginary = value * Definition.ConversionFactor;
            }
        }

        /// <summary>
        /// Gets total number of composite values that this <see cref="PhasorValueBase"/> provides.
        /// </summary>
        public override int CompositeValueCount => 2;

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="PhasorValueBase"/> have been assigned a value.
        /// </summary>
        public override bool IsEmpty => m_phasor.NoneAssigned;

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override int BodyLength => DataFormat == DataFormat.FixedInteger ? 4 : 8;

        /// <summary>
        /// Gets the binary body image of the <see cref="PhasorValueBase"/> object.
        /// </summary>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                // Had to make a decision on usage versus typical protocol implementation when
                // exposing values as double / int when protocols typically use float / short for
                // transmission. Exposing values as double / int makes class more versatile by
                // allowing future protocol implementations to support higher resolution values
                // simply by overriding BodyLength, BodyImage and ParseBodyImage. However, exposing
                // the double / int values runs the risk of providing values that are outside the
                // data type limitations, hence the unchecked section below. Risk should generally
                // be low in typical usage scenarios since values being transmitted via a generated
                // image were likely parsed previously from a binary image with the same constraints.
                unchecked
                {
                    if (CoordinateFormat == CoordinateFormat.Rectangular)
                    {
                        if (DataFormat == DataFormat.FixedInteger)
                        {
                            BigEndian.CopyBytes((short)UnscaledReal, buffer, 0);
                            BigEndian.CopyBytes((short)UnscaledImaginary, buffer, 2);
                        }
                        else
                        {
                            BigEndian.CopyBytes((float)m_phasor.Real, buffer, 0);
                            BigEndian.CopyBytes((float)m_phasor.Imaginary, buffer, 4);
                        }
                    }
                    else
                    {
                        if (DataFormat == DataFormat.FixedInteger)
                        {
                            BigEndian.CopyBytes((ushort)(m_phasor.Magnitude / Definition.ConversionFactor), buffer, 0);

                            if (AngleFormat == AngleFormat.Radians)
                                BigEndian.CopyBytes((short)(m_phasor.Angle * 10000.0D), buffer, 2);
                            else
                                BigEndian.CopyBytes((short)(m_phasor.Angle.ToDegrees() * 10000.0D), buffer, 2);
                        }
                        else
                        {
                            BigEndian.CopyBytes((float)m_phasor.Magnitude, buffer, 0);

                            if (AngleFormat == AngleFormat.Radians)
                                BigEndian.CopyBytes((float)m_phasor.Angle, buffer, 4);
                            else
                                BigEndian.CopyBytes((float)m_phasor.Angle.ToDegrees(), buffer, 4);
                        }
                    }
                }

                return buffer;
            }
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for this <see cref="PhasorValueBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Phasor Type", (int)Type + ": " + Type);
                baseAttributes.Add("Angle Value", Angle.ToDegrees() + "°");
                baseAttributes.Add("Magnitude Value", Magnitude.ToString());
                baseAttributes.Add("Real Value", Real.ToString());
                baseAttributes.Add("Imaginary Value", Imaginary.ToString());
                baseAttributes.Add("Unscaled Real Value", UnscaledReal.ToString());
                baseAttributes.Add("Unscaled Imaginary Value", UnscaledImaginary.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the specified composite value of this <see cref="PhasorValueBase"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        /// <remarks>
        /// Some <see cref="ChannelValueBase{T}"/> implementations can contain more than one value, this method is used to abstractly expose each value.
        /// </remarks>
        /// <returns>A <see cref="double"/> representing the composite value.</returns>
        public override double GetCompositeValue(int index)
        {
            switch (index)
            {
                case (int)CompositePhasorValue.Angle:
                    return Angle.ToDegrees();
                case (int)CompositePhasorValue.Magnitude:
                    return Magnitude;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Invalid composite index requested");
            }
        }

        /// <summary>
        /// Creates a new <see cref="IMeasurement"/> value for specified composite value for this <see cref="PhasorValueBase"/>.
        /// </summary>
        /// <param name="valueIndex">Composite value index for which to derive new <see cref="IMeasurement"/> value.</param>
        /// <returns>New <see cref="IMeasurement"/> value for specified composite value for this <see cref="PhasorValueBase"/>.</returns>
        protected override IMeasurement CreateMeasurement(int valueIndex)
        {
            IMeasurement measurement = base.CreateMeasurement(valueIndex);

            if (valueIndex == (int)CompositePhasorValue.Angle)
            {
                double adjustedValue = measurement.AdjustedValue;

                if (adjustedValue <= -180 || adjustedValue > 180)
                {
                    adjustedValue = Angle.FromDegrees(adjustedValue).ToRange(-Math.PI, false).ToDegrees();
                    measurement.Value = (adjustedValue - measurement.Adder) / measurement.Multiplier;
                }
            }

            return measurement;
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            if (DataFormat == DataFormat.FixedInteger)
            {
                if (CoordinateFormat == CoordinateFormat.Rectangular)
                {
                    // Parse from fixed-integer, rectangular
                    UnscaledReal = BigEndian.ToInt16(buffer, startIndex);
                    UnscaledImaginary = BigEndian.ToInt16(buffer, startIndex + 2);
                }
                else
                {
                    // Parse from fixed-integer, polar
                    m_phasor.Magnitude = BigEndian.ToUInt16(buffer, startIndex) * Definition.ConversionFactor;

                    if (AngleFormat == AngleFormat.Radians)
                        m_phasor.Angle = BigEndian.ToInt16(buffer, startIndex + 2) / 10000.0D;
                    else
                        m_phasor.Angle = Angle.FromDegrees(BigEndian.ToInt16(buffer, startIndex + 2) / 10000.0D);
                }

                return 4;
            }

            if (CoordinateFormat == CoordinateFormat.Rectangular)
            {
                // Parse from single-precision floating-point, rectangular
                m_phasor.Real = BigEndian.ToSingle(buffer, startIndex);
                m_phasor.Imaginary = BigEndian.ToSingle(buffer, startIndex + 4);
            }
            else
            {
                // Parse from single-precision floating-point, polar
                m_phasor.Magnitude = BigEndian.ToSingle(buffer, startIndex);

                if (AngleFormat == AngleFormat.Radians)
                    m_phasor.Angle = BigEndian.ToSingle(buffer, startIndex + 4);
                else
                    m_phasor.Angle = Angle.FromDegrees(BigEndian.ToSingle(buffer, startIndex + 4));
            }

            return 8;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize phasor value
            info.AddValue("real", m_phasor.Real);
            info.AddValue("imaginary", m_phasor.Imaginary);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Calculates watts from imaginary and real components of a voltage and current phasor.
        /// </summary>
        /// <param name="voltage">Voltage phasor.</param>
        /// <param name="current">Current phasor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="voltage"/> and <paramref name="current"/> must not be null.</exception>
        /// <returns>Calculated watts from imaginary and real components of specified <paramref name="voltage"/> and <paramref name="current"/> phasors.</returns>
        public static Power CalculatePower(IPhasorValue voltage, IPhasorValue current)
        {
            if (voltage == null)
                throw new ArgumentNullException(nameof(voltage), "No voltage specified");

            if (current == null)
                throw new ArgumentNullException(nameof(current), "No current specified");

            return Phasor.CalculateActivePower(voltage.Phasor, current.Phasor);
        }

        /// <summary>
        /// Calculates vars (total volt-amperes of reactive power) from imaginary and real components of a voltage and current phasor.
        /// </summary>
        /// <param name="voltage">Voltage phasor.</param>
        /// <param name="current">Current phasor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="voltage"/> and <paramref name="current"/> must not be null.</exception>
        /// <remarks>
        /// Although the <see cref="Power"/> units class technically represents watts (i.e., real power) and vars (i.e., imaginary power)
        /// are properly expressed in volt-amperes reactive (VAr), the calculated result is still a representation of power and therefore
        /// the <see cref="Power"/> units class is used to express the return value leaving the consumer to properly apply the needed
        /// engineering units for display purposes.
        /// </remarks>
        /// <returns>Calculated vars (total volt-amperes of reactive power) from imaginary and real components of specified <paramref name="voltage"/> and <paramref name="current"/> phasors.</returns>
        public static Power CalculateVars(IPhasorValue voltage, IPhasorValue current)
        {
            if (voltage == null)
                throw new ArgumentNullException(nameof(voltage), "No voltage specified");

            if (current == null)
                throw new ArgumentNullException(nameof(current), "No current specified");

            return Phasor.CalculateReactivePower(voltage.Phasor, current.Phasor);
        }

        #endregion
    }
}