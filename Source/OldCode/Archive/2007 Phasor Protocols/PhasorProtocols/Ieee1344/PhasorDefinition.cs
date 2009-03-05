//*******************************************************************************************************
//  PhasorDefinition.cs
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

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IPhasorDefinition"/>.
    /// </summary>
    [Serializable()]
    public class PhasorDefinition : PhasorDefinitionBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/>.
        /// </summary>
        protected PhasorDefinition()
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected PhasorDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> using the specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="offset">The offset of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="type">The <see cref="PhasorType"/> of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="voltageReference">The associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).</param>
        public PhasorDefinition(ConfigurationCell parent, string label, uint scale, double offset, PhasorType type, PhasorDefinition voltageReference)
            : base(parent, label, scale, offset, type, voltageReference)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets conversion factor image of this <see cref="PhasorDefinition"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];
                UInt24 scalingFactor = (ScalingValue > UInt24.MaxValue ? UInt24.MaxValue : (UInt24)ScalingValue);

                buffer[0] = (byte)(Type == PhasorType.Voltage ? 0 : 1);

                EndianOrder.BigEndian.CopyBytes(scalingFactor, buffer, 1);

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses conversion factor image from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        internal void ParseConversionFactor(byte[] binaryImage, int startIndex)
        {
            // Get phasor type from first byte
            Type = (binaryImage[startIndex] == 0) ? PhasorType.Voltage : PhasorType.Current;

            // Last three bytes represent scaling factor
            ScalingValue = EndianOrder.BigEndian.ToUInt24(binaryImage, startIndex + 1);
        }

        #endregion

        #region [ Static ]

        // Static Properties

        // Gets length of conversion factor
        internal static int ConversionFactorLength
        {
            get
            {
                return 4;
            }
        }

        // Static Methods

        // Delegate handler to create a new IEEE 1344 phasor definition
        internal static IPhasorDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex)
        {
            IPhasorDefinition phasorDefinition = new PhasorDefinition() { Parent = parent };

            phasorDefinition.Initialize(binaryImage, startIndex, 0);

            return phasorDefinition;
        }

        #endregion
    }
}