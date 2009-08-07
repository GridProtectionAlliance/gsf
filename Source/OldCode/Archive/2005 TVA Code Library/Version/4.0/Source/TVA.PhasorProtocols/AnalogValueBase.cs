//*******************************************************************************************************
//  AnalogValueBase.cs
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
//  02/18/2005 - James R. Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh Patterson
//      Edited Comments
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent representation of an analog value.
    /// </summary>
    [Serializable()]
    public abstract class AnalogValueBase : ChannelValueBase<IAnalogDefinition>, IAnalogValue
    {
        #region [ Members ]

        // Fields
        private double m_value;
        private bool m_valueAssigned;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AnalogValueBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="AnalogValueBase"/>.</param>
        /// <param name="analogDefinition">The <see cref="IAnalogDefinition"/> associated with this <see cref="AnalogValueBase"/>.</param>
        protected AnalogValueBase(IDataCell parent, IAnalogDefinition analogDefinition)
            : base(parent, analogDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogValueBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="AnalogValueBase"/>.</param>
        /// <param name="analogDefinition">The <see cref="IAnalogDefinition"/> associated with this <see cref="AnalogValueBase"/>.</param>
        /// <param name="value">The floating point value that represents this <see cref="AnalogValueBase"/>.</param>
        protected AnalogValueBase(IDataCell parent, IAnalogDefinition analogDefinition, double value)
            : base(parent, analogDefinition)
        {
            m_value = value;
            m_valueAssigned = !double.IsNaN(value);
        }

        /// <summary>
        /// Creates a new <see cref="AnalogValueBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize analog value
            m_value = info.GetDouble("value");
            m_valueAssigned = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the floating point value that represents this <see cref="AnalogValueBase"/>.
        /// </summary>
        public virtual double Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                m_valueAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the integer representation of this <see cref="AnalogValueBase"/> value.
        /// </summary>
        public virtual int IntegerValue
        {
            get
            {
                unchecked
                {
                    return (int)m_value;
                }
            }
            set
            {
                m_value = value;
                m_valueAssigned = true;
            }
        }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="AnalogValueBase"/> have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        public override bool IsEmpty
        {
            get
            {
                return !m_valueAssigned;
            }
        }

        /// <summary>
        /// Gets total number of composite values that this <see cref="AnalogValueBase"/> provides.
        /// </summary>
        public override int CompositeValueCount
        {
            get
            {
                return 1;
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
                    return 2;
                else
                    return 4;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="AnalogValueBase"/> object.
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
                // simply by overriding BodyLength, BodyImage and ParseBodyImage. However, exposing
                // the double / int values runs the risk of providing values that are outside the
                // data type limitations, hence the unchecked section below. Risk should generally
                // be low in typical usage scenarios since values being transmitted via a generated
                // image were likely parsed previously from a binary image with the same constraints.
                unchecked
                {
                    if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                        EndianOrder.BigEndian.CopyBytes((short)m_value, buffer, 0);
                    else
                        EndianOrder.BigEndian.CopyBytes((float)m_value, buffer, 0);
                }

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="AnalogValueBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Analog Value", Value.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the specified composite value of this <see cref="AnalogValueBase"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        /// <remarks>
        /// Some <see cref="ChannelValueBase{T}"/> implementations can contain more than one value, this method is used to abstractly expose each value.
        /// </remarks>
        /// <returns>A composite value as <see cref="Double"/>.</returns>
        public override double GetCompositeValue(int index)
        {
            if (index == 0)
                return m_value;
            else
                throw new ArgumentOutOfRangeException("index", "Invalid composite index requested");
        }

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
                m_value = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                m_valueAssigned = true;
                return 2;
            }
            else
            {
                m_value = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
                m_valueAssigned = true;
                return 4;
            }
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize analog value
            info.AddValue("value", m_value);
        }

        #endregion
    }
}