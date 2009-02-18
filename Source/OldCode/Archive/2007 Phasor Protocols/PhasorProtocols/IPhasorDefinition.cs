//*******************************************************************************************************
//  IPhasorDefinition.cs
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
    /// Represents the protocol independent interface of a phasor value definition.
    /// </summary>
    public interface IPhasorDefinition : IChannelDefinition
    {
        /// <summary>
        /// Gets or sets the <see cref="PhasorProtocols.CoordinateFormat"/> of this <see cref="IPhasorDefinition"/>.
        /// </summary>
        CoordinateFormat CoordinateFormat { get; }

        /// <summary>
        /// Gets or sets the <see cref="PhasorType"/> of this <see cref="IPhasorDefinition"/>.
        /// </summary>
        PhasorType Type { get; set; }

        /// <summary>
        /// Gets or sets the associated <see cref="IPhasorDefinition"/> that represents the voltage reference (if any).
        /// </summary>
        /// <remarks>
        /// This only applies to current phasors.
        /// </remarks>
        IPhasorDefinition VoltageReference { get; set; }
    }
}
