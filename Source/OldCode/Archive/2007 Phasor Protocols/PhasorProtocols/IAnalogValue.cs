//*******************************************************************************************************
//  IAnalogValue.cs
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

namespace PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of an analog value.
    /// </summary>
    public interface IAnalogValue : IChannelValue<IAnalogDefinition>
    {
        /// <summary>
        /// Gets or sets the floating point value that represents this <see cref="IAnalogValue"/>.
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// Gets or sets the integer representation of this <see cref="IAnalogValue"/> value.
        /// </summary>
        int IntegerValue { get; set; }
    }
}