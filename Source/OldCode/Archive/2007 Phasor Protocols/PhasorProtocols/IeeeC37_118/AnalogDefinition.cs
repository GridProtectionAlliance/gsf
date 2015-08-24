//*******************************************************************************************************
//  AnalogDefinition.cs
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

namespace TVA.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of an <see cref="IAnalogDefinition"/>.
    /// </summary>
    [Serializable()]
    public class AnalogDefinition : AnalogDefinitionBase
    {
        #region [ Members ]

        // Constants        
        internal const int ConversionFactorLength = 4;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="AnalogDefinition"/>.</param>
        public AnalogDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="scale">The integer scaling value of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="offset">The offset of this <see cref="AnalogDefinition"/>.</param>
        /// <param name="type">The <see cref="AnalogType"/> of this <see cref="AnalogDefinition"/>.</param>
        public AnalogDefinition(ConfigurationCell parent, string label, uint scale, double offset, AnalogType type)
            : base(parent, label, scale, offset, type)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="AnalogDefinition"/>.
        /// </summary>
        public virtual new ConfigurationCell Parent
        {
            get
            {
                return base.Parent as ConfigurationCell;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets conversion factor image of this <see cref="AnalogDefinition"/>.
        /// </summary>
        internal byte[] ConversionFactorImage
        {
            get
            {
                byte[] buffer = new byte[ConversionFactorLength];
                UInt24 scalingFactor = (ScalingValue > UInt24.MaxValue ? UInt24.MaxValue : (UInt24)ScalingValue);

                // Store analog type in first byte
                buffer[0] = (byte)this.AnalogType;

                // Store scaling in last three bytes
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
        internal int ParseConversionFactor(byte[] binaryImage, int startIndex)
        {
            // Get analog type from first byte
            this.AnalogType = (AnalogType)binaryImage[startIndex];

            // Last three bytes represent scaling factor
            ScalingValue = EndianOrder.BigEndian.ToUInt24(binaryImage, startIndex + 1);

            return ConversionFactorLength;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 analog definition
        internal static IAnalogDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IAnalogDefinition analogDefinition = new AnalogDefinition(parent);

            parsedLength = analogDefinition.Initialize(binaryImage, startIndex, 0);

            return analogDefinition;
        }

        #endregion        
    }
}
