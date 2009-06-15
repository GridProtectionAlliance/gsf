//*******************************************************************************************************
//  PhasorValue.cs
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
//  04/27/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using TVA;
using TVA.Units;

namespace PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="IPhasorValue"/>.
    /// </summary>
    [Serializable()]
    public class PhasorValue : PhasorValueBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="IPhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        public PhasorValue(IDataCell parent, IPhasorDefinition phasorDefinition)
            : base(parent, phasorDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        /// <param name="real">The real value of this <see cref="PhasorValue"/>.</param>
        /// <param name="imaginary">The imaginary value of this <see cref="PhasorValue"/>.</param>
        public PhasorValue(DataCell parent, PhasorDefinition phasorDefinition, double real, double imaginary)
            : base(parent, phasorDefinition, real, imaginary)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.</param>
        /// <param name="phasorDefinition">The <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.</param>
        /// <param name="angle">The <see cref="TVA.Units.Angle"/> value (a.k.a., the argument) of this <see cref="PhasorValue"/>, in radians.</param>
        /// <param name="magnitude">The magnitude value (a.k.a., the absolute value or modulus) of this <see cref="PhasorValue"/>.</param>
        public PhasorValue(DataCell parent, PhasorDefinition phasorDefinition, Angle angle, double magnitude)
            : base(parent, phasorDefinition, angle, magnitude)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorValue"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]
        
        /// <summary>
        /// Gets or sets the <see cref="DataCell"/> parent of this <see cref="PhasorValue"/>.
        /// </summary>
        public virtual new DataCell Parent
        {
            get
            {
                return base.Parent as DataCell;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PhasorDefinition"/> associated with this <see cref="PhasorValue"/>.
        /// </summary>
        public virtual new PhasorDefinition Definition
        {
            get
            {
                return base.Definition as PhasorDefinition;
            }
            set
            {
                base.Definition = value;
            }
        }
        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="PhasorValueBase"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[8];

                EndianOrder.BigEndian.CopyBytes((float)Magnitude, buffer, 0);
                EndianOrder.BigEndian.CopyBytes((float)Angle.ToDegrees(), buffer, 4);

                return buffer;
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
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            // Parse magnitude and angle in degrees
            Magnitude = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
            Angle = Angle.FromDegrees(EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4));

            return 8;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new SEL Fast Message phasor value
        internal static IPhasorValue CreateNewValue(IDataCell parent, IPhasorDefinition definition, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IPhasorValue phasor = new PhasorValue(parent, definition);

            parsedLength = phasor.Initialize(binaryImage, startIndex, 0);

            return phasor;
        }

        #endregion
    }
}