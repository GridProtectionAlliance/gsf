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

using System;

namespace PCS.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Phasor enabled device commands enumeration.
    /// </summary>
    [Serializable()]
    public enum DeviceCommand : ushort
    {
        /// <summary>
        /// 0001 Turn off transmission of data frames.
        /// </summary>
        DisableRealTimeData = (ushort)Bits.Bit00,
        /// <summary>
        /// 0010 Turn on transmission of data frames.
        /// </summary>
        EnableRealTimeData = (ushort)Bits.Bit01,
        /// <summary>
        /// 0011 Send header file.
        /// </summary>
        SendHeaderFrame = (ushort)Bits.Bit00 | (ushort)Bits.Bit01,
        /// <summary>
        /// 0100 Send configuration file 1.
        /// </summary>
        SendConfigurationFrame1 = (ushort)Bits.Bit02,
        /// <summary>
        /// 0101 Send configuration file 2.
        /// </summary>
        SendConfigurationFrame2 = (ushort)Bits.Bit00 | (ushort)Bits.Bit02,
        /// <summary>
        /// 1000 Receive extended frame for IEEE C37.118 / receive reference phasor for IEEE 1344.
        /// </summary>
        ReceiveExtendedFrame = (ushort)Bits.Bit03,
        /// <summary>
        /// Reserved bits.
        /// </summary>
        ReservedBits = ushort.MaxValue & ~((ushort)Bits.Bit00 | (ushort)Bits.Bit01 | (ushort)Bits.Bit02 | (ushort)Bits.Bit03)
    }

    #endregion

    /// <summary>
    /// Represents a protocol independent interface representation of any kind of command frame that contains
    /// a collection of <see cref="ICommandCell"/> objects.
    /// </summary>
    public interface ICommandFrame : IChannelFrame<ICommandCell>
    {
        /// <summary>
        /// Gets reference to the <see cref="CommandCellCollection"/> for this <see cref="ICommandFrame"/>.
        /// </summary>
        new CommandCellCollection Cells { get; }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="ICommandFrame"/>.
        /// </summary>
        new ICommandFrameParsingState State { get; set; }
    
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