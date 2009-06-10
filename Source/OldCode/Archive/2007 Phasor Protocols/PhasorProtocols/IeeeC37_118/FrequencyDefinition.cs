//*******************************************************************************************************
//  FrequencyDefinition.cs
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

namespace PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IFrequencyDefinition"/>.
    /// </summary>
    [Serializable()]
    public class FrequencyDefinition : FrequencyDefinitionBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        public FrequencyDefinition(IConfigurationCell parent)
            : base(parent)
        {
            ScalingValue = 1000;
            DfDtScalingValue = 100;
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="FrequencyDefinition"/>.</param>
        public FrequencyDefinition(ConfigurationCell parent, string label)
            : base(parent, label, 1000, 100, 0.0D)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyDefinition"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="FrequencyDefinition"/>.
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
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="FrequencyDefinition"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                return EndianOrder.BigEndian.GetBytes((ushort)(Parent.NominalFrequency == LineFrequency.Hz50 ? Bits.Bit00 : Bits.Nil));
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
        /// The base implementation assumes that all channel defintions begin with a label as this is
        /// the general case, override functionality if this is not the case.
        /// </remarks>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            Parent.NominalFrequency = ((EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex) & (ushort)Bits.Bit00) > 0) ? LineFrequency.Hz50 : LineFrequency.Hz60;
            return 2;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 frequency definition
        internal static IFrequencyDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IFrequencyDefinition frequencyDefinition = new FrequencyDefinition(parent);

            parsedLength = frequencyDefinition.Initialize(binaryImage, startIndex, 0);

            return frequencyDefinition;
        }

        #endregion
    }
}