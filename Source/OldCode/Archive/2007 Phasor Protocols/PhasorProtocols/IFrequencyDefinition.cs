//*******************************************************************************************************
//  IFrequencyDefinition.cs
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

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of a definition of a <see cref="IFrequencyValue"/>.
    /// </summary>
    public interface IFrequencyDefinition : IChannelDefinition
    {
        /// <summary>
        /// Gets the nominal <see cref="LineFrequency"/> of this <see cref="IFrequencyDefinition"/>.
        /// </summary>
        LineFrequency NominalFrequency { get; }

        /// <summary>
        /// Gets or sets the df/dt offset of this <see cref="IFrequencyDefinition"/>.
        /// </summary>
        double DfDtOffset { get; set; }

        /// <summary>
        /// Gets or sets the df/dt scaling value of this <see cref="IFrequencyDefinition"/>.
        /// </summary>
        uint DfDtScalingValue { get; set; }
    }
}