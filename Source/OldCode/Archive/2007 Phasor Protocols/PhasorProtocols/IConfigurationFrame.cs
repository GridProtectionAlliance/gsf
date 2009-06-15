//*******************************************************************************************************
//  IConfigurationFrame.cs
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
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using TVA;

namespace PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any kind of configuration frame that contains
    /// a collection of <see cref="IConfigurationCell"/> objects.
    /// </summary>
    public interface IConfigurationFrame : IChannelFrame
    {
        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="IConfigurationFrame"/>.
        /// </summary>
        new ConfigurationCellCollection Cells { get; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="IConfigurationFrame"/>.
        /// </summary>
        new IConfigurationFrameParsingState State { get; set; }

            /// <summary>
        /// Gets or sets defined frame rate of this <see cref="IConfigurationFrame"/>.
        /// </summary>
        ushort FrameRate { get; set; }

        /// <summary>
        /// Gets the defined <see cref="Ticks"/> per frame of this <see cref="IConfigurationFrame"/>.
        /// </summary>
        decimal TicksPerFrame { get; }

        /// <summary>
        /// Sets a new nominal <see cref="LineFrequency"/> for all <see cref="IFrequencyDefinition"/> elements of each <see cref="IConfigurationCell"/> in the <see cref="Cells"/> collection.
        /// </summary>
        /// <param name="value">New nominal <see cref="LineFrequency"/> for <see cref="IFrequencyDefinition"/> elements.</param>
        void SetNominalFrequency(LineFrequency value);
    }
}