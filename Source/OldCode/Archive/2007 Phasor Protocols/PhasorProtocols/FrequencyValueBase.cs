//*******************************************************************************************************
//  FrequencyValueBase.cs
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
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PCS;
using PCS.Interop;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent representation of a frequency and df/dt value.
    /// </summary>
    [Serializable()]
    public abstract class FrequencyValueBase : ChannelValueBase<IFrequencyDefinition>, IFrequencyValue
    {
        #region [ Members ]

        // Fields
        private double m_frequency;
        private double m_dfdt;
        private bool m_frequencyAssigned;
        private bool m_dfdtAssigned;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyValueBase"/>.
        /// </summary>
        protected FrequencyValueBase()
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyValueBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize frequency value
            m_frequency = info.GetDouble("frequency");
            m_dfdt = info.GetDouble("dfdt");

            m_frequencyAssigned = true;
            m_dfdtAssigned = true;
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyValueBase"/> from the specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="FrequencyValueBase"/>.</param>
        /// <param name="frequencyDefinition">The <see cref="IFrequencyDefinition"/> associated with this <see cref="FrequencyValueBase"/>.</param>
        /// <param name="frequency">The floating point value that represents this <see cref="FrequencyValueBase"/>.</param>
        /// <param name="dfdt">The floating point value that represents the change in this <see cref="FrequencyValueBase"/> over time.</param>
        protected FrequencyValueBase(IDataCell parent, IFrequencyDefinition frequencyDefinition, double frequency, double dfdt)
            : base(parent, frequencyDefinition)
        {
            m_frequency = frequency;
            m_dfdt = dfdt;

            m_frequencyAssigned = !double.IsNaN(frequency);
            m_dfdtAssigned = !double.IsNaN(dfdt);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the floating point value that represents this <see cref="FrequencyValueBase"/>.
        /// </summary>
        public virtual double Frequency
        {
            get
            {
                return m_frequency;
            }
            set
            {
                m_frequency = value;
                m_frequencyAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the floating point value that represents the change in this <see cref="FrequencyValueBase"/> over time.
        /// </summary>
        public virtual double DfDt
        {
            get
            {
                return m_dfdt;
            }
            set
            {
                m_dfdt = value;
                m_dfdtAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of this <see cref="FrequencyValueBase"/>.
        /// </summary>
        public virtual int UnscaledFrequency
        {
            get
            {
                unchecked
                {
                    return (int)((m_frequency - Definition.Offset) * Definition.ScalingValue);
                }
            }
            set
            {
                m_frequency = value / (double)Definition.ScalingValue + Definition.Offset;
                m_frequencyAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the change in this <see cref="FrequencyValueBase"/> over time.
        /// </summary>
        public virtual int UnscaledDfDt
        {
            get
            {
                unchecked
                {
                    return (int)((m_dfdt - Definition.DfDtOffset) * Definition.DfDtScalingValue);
                }
            }
            set
            {
                m_dfdt = value / (double)Definition.DfDtScalingValue + Definition.DfDtOffset;
                m_dfdtAssigned = true;
            }
        }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="FrequencyValueBase"/> have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        public override bool IsEmpty
        {
            get
            {
                return (!m_frequencyAssigned || !m_dfdtAssigned);
            }
        }

        /// <summary>
        /// Gets the composite values of this <see cref="FrequencyValueBase"/>.
        /// </summary>
        public override double[] CompositeValues
        {
            get
            {
                return new double[] { m_frequency, m_dfdt };
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override int BodyLength
        {
            get
            {
                if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                    return 4;
                else
                    return 8;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="FrequencyValueBase"/> object.
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

                // Had to make a descision on usage versus typical protocol implementation when
                // exposing values as double / int when protocols typically use float / short for
                // transmission. Exposing values as double / int makes class more versatile by
                // allowing future protocol implementations to support higher resolution values
                // simply by overriding BodyLength, BodyImage and ParseBodyImage. Exposing class
                // values as double / int runs the risk of providing values that are outside the
                // data type limitations, hence the unchecked section below. However, risk should
                // be low in typical usage scenarios since values being transmitted via a generated
                // image were likely parsed previously from a binary image with the same constraints.
                unchecked
                {
                    if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                    {
                        EndianOrder.BigEndian.CopyBytes((short)UnscaledFrequency, buffer, 0);
                        EndianOrder.BigEndian.CopyBytes((short)UnscaledDfDt, buffer, 2);
                    }
                    else
                    {
                        EndianOrder.BigEndian.CopyBytes((float)m_frequency, buffer, 0);
                        EndianOrder.BigEndian.CopyBytes((float)m_dfdt, buffer, 4);
                    }
                }

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="FrequencyValueBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frequency Value", Frequency.ToString());
                baseAttributes.Add("df/dt Value", DfDt.ToString());
                baseAttributes.Add("Unscaled Frequency Value", UnscaledFrequency.ToString());
                baseAttributes.Add("Unscaled df/dt Value", UnscaledDfDt.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
            {
                UnscaledFrequency = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                UnscaledDfDt = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2);
                
                return 4;
            }
            else
            {
                m_frequency = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
                m_dfdt = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4);

                m_frequencyAssigned = true;
                m_dfdtAssigned = true;

                return 8;
            }
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize frequency value
            info.AddValue("frequency", m_frequency);
            info.AddValue("dfdt", m_dfdt);
        }

        #endregion
    }
}