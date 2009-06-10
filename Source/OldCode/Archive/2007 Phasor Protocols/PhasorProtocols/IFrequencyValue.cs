//*******************************************************************************************************
//  IFrequencyValue.cs
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
    /// Represents a protocol independent interface of a frequency value.
    /// </summary>
    public interface IFrequencyValue : IChannelValue<IFrequencyDefinition>
    {
        /// <summary>
        /// Gets or sets the floating point value that represents this <see cref="IFrequencyValue"/>.
        /// </summary>
        double Frequency { get; set; }

        /// <summary>
        /// Gets or sets the floating point value that represents the change in this <see cref="IFrequencyValue"/> over time.
        /// </summary>
        double DfDt { get; set; }

        /// <summary>
        /// Gets or sets the unscaled integer representation of this <see cref="IFrequencyValue"/>.
        /// </summary>
        int UnscaledFrequency { get; set; }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the change in this <see cref="IFrequencyValue"/> over time.
        /// </summary>
        int UnscaledDfDt { get; set; }
    }
}