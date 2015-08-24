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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace TVA.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of an <see cref="IAnalogDefinition"/>.
    /// </summary>
    [Serializable()]
    public class AnalogDefinition : AnalogDefinitionBase
    {
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
        /// Gets the maximum length of the <see cref="ChannelDefinitionBase.Label"/> of this <see cref="AnalogDefinition"/>.
        /// </summary>
        public override int MaximumLabelLength
        {
            get
            {
                return 256;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="AnalogDefinition"/> object.
        /// </summary>
        /// <remarks>
        /// BPA PDCstream does not include analog definition in descriptor packet.  Only a count of available values is defined in the data frame.
        /// </remarks>
        protected override byte[] BodyImage
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new BPA PDCstream analog definition
        internal static IAnalogDefinition CreateNewDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            IAnalogDefinition analogDefinition = new AnalogDefinition(parent);

            parsedLength = analogDefinition.Initialize(binaryImage, startIndex, 0);

            return analogDefinition;
        }

        #endregion
    }
}