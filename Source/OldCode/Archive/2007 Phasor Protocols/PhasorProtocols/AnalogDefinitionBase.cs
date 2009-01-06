//*******************************************************************************************************
//  AnalogDefinitionBase.cs
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
//  02/18/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// This class represents the common implementation of the protocol independent definition of an analog value.
    /// </summary>
    [CLSCompliant(false), Serializable()]
    public abstract class AnalogDefinitionBase : ChannelDefinitionBase, IAnalogDefinition
    {
        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        protected AnalogDefinitionBase()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected AnalogDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> using the specified parameters.
        /// </summary>
        protected AnalogDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> using the specified parameters.
        /// </summary>
        protected AnalogDefinitionBase(IConfigurationCell parent, int index, string label, int scale, float offset)
            : base(parent, index, label, scale, offset)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AnalogDefinitionBase"/> copied from the specified <see cref="IAnalogDefinition"/> object.
        /// </summary>
        protected AnalogDefinitionBase(IConfigurationCell parent, IAnalogDefinition analogDefinition)
            : this(parent, analogDefinition.Index, analogDefinition.Label, analogDefinition.ScalingFactor, analogDefinition.Offset)
        {
        }

        /// <summary>
        /// Gets the <see cref="PhasorProtocols.DataFormat"/> for the <see cref="AnalogDefinitionBase"/>.
        /// </summary>
        public override DataFormat DataFormat
        {
            get
            {
                return Parent.AnalogDataFormat;
            }
        }
    }
}
