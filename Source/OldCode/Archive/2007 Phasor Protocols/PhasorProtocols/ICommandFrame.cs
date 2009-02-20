//*******************************************************************************************************
//  ICommandFrame.cs
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

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent interface representation of any kind of command frame that contains
    /// a collection of <see cref="ICommandCell"/> objects.
    /// </summary>
    public interface ICommandFrame : IChannelFrame<ICommandFrame>
    {
        /// <summary>
        /// Gets reference to the <see cref="CommandCellCollection"/> for this <see cref="ICommandFrame"/>.
        /// </summary>
        new CommandCellCollection Cells { get; }

        /// <summary>
        /// Gets or sets <see cref="DeviceCommand"/> for this <see cref="ICommandFrame"/>.
        /// </summary>
        DeviceCommand Command { get; set; }

        /// <summary>
        /// Gets or sets extended binary image data for this <see cref="ICommandFrame"/>.
        /// </summary>
        byte[] ExtendedData { get; set; }
    }
}
