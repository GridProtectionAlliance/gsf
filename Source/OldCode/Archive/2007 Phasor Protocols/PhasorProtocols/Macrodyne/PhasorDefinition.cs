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
//  04/30/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IPhasorDefinition"/>.
    /// </summary>
    [Serializable()]
    public class PhasorDefinition : PhasorDefinitionBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        public PhasorDefinition(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PhasorDefinition"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="ConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="label">The label of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="type">The <see cref="PhasorType"/> of this <see cref="PhasorDefinition"/>.</param>
        /// <param name="voltageReference">The associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).</param>
        public PhasorDefinition(ConfigurationCell parent, string label, PhasorType type, PhasorDefinition voltageReference)
            : base(parent, label, 1, 0.0D, type, voltageReference)
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

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the scale/bit for the <see cref="ChannelDefinitionBase.ScalingValue"/> of this <see cref="PhasorDefinition"/>.
        /// </summary>
        /// <remarks>
        /// We override this for phasor values since Macrodyne transmits unscaled integer values for real and imaginary phasor components.
        /// </remarks>
        public override double ScalePerBit
        {
            get
            {
                return 1.0D;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> parent of this <see cref="PhasorDefinition"/>.
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

        #endregion
    }
}