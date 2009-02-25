//*******************************************************************************************************
//  IDigitalValue.cs
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
    /// Represents a protocol independent interface representation of a digital value.
    /// </summary>
    public interface IDigitalValue : IChannelValue<IDigitalDefinition>
    {
        /// <summary>
        /// Gets or sets the integer value (composed of digital bits) that represents this <see cref="IDigitalValue"/>.
        /// </summary>
        int Value { get; set; }
    }
}