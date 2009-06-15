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
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using TVA.Units;

namespace PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IPhasorValue"/>.
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
        /// Gets or sets the <see cref="TVA.Units.Angle"/> value (a.k.a., the argument) of this <see cref="PhasorValue"/>, in radians.
        /// </summary>
        public override Angle Angle
        {
            get
            {
                return Angle.FromDegrees(base.Angle.ToDegrees() + Definition.Offset);
            }
            set
            {
                base.Angle = Angle.FromDegrees(value.ToDegrees() - Definition.Offset);
            }
        }

        /// <summary>
        /// Gets or sets the magnitude value (a.k.a., the absolute value or modulus) of this <see cref="PhasorValue"/>.
        /// </summary>
        public override double Magnitude
        {
            get
            {
                return base.Magnitude * PhasorDefinition.CustomConversionFactor(Definition);
            }
            set
            {
                base.Magnitude = value / PhasorDefinition.CustomConversionFactor(Definition);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Calculates binary length of a phasor value based on its definition
        internal static int CalculateBinaryLength(IPhasorDefinition definition)
        {
            // The phasor definition will determine the binary length based on data format
            return (new PhasorValue(null, definition)).BinaryLength;
        }

        // Delegate handler to create a new BPA PDCstream phasor value
        internal static IPhasorValue CreateNewValue(IDataCell parent, IPhasorDefinition definition, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IPhasorValue phasor = new PhasorValue(parent, definition);

            parsedLength = phasor.Initialize(binaryImage, startIndex, 0);

            return phasor;
        }

        #endregion
    }
}