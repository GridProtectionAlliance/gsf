//*******************************************************************************************************
//  Common.cs
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
//  04/26/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.PhasorProtocols.SelFastMessage
{
    #region [ Enumerations ]

    /// <summary>
    /// SEL Fast Message PMDATA setting frame size enumeration.
    /// </summary>
    [Serializable()]
    public enum FrameSize : byte
    {
        /// <summary>
        /// Positive sequence voltage phasor.
        /// </summary>
        V1 = 0x28,
        /// <summary>
        /// Three-phase and positive sequence voltage phasors.
        /// </summary>
        V = 0x40,
        /// <summary>
        /// Three-phase and positive sequence voltage and current phasors.
        /// </summary>
        A = 0x60
    }

    /// <summary>
    /// SEL Fast Message status word flags enumeration.
    /// </summary>
    [Flags(), Serializable()]
    public enum StatusFlags : ushort
    {
        /// <summary>
        /// Time synchronization OK, 1 when OK.
        /// </summary>
        TSOK = (ushort)Bits.Bit00,
        /// <summary>
        /// Phasor measurement data OK, 1 when OK.
        /// </summary>
        PMDOK = (ushort)Bits.Bit01,
        /// <summary>
        /// User programmable bit 1.
        /// </summary>
        PSV51 = (ushort)Bits.Bit02,
        /// <summary>
        /// User programmable bit 2.
        /// </summary>
        PSV52 = (ushort)Bits.Bit03,
        /// <summary>
        /// User programmable bit 3.
        /// </summary>
        PSV53 = (ushort)Bits.Bit04,
        /// <summary>
        /// User programmable bit 4.
        /// </summary>
        PSV54 = (ushort)Bits.Bit05,
        /// <summary>
        /// User programmable bit 5.
        /// </summary>
        PSV55 = (ushort)Bits.Bit06,
        /// <summary>
        /// User programmable bit 6.
        /// </summary>
        PSV56 = (ushort)Bits.Bit07,
        /// <summary>
        /// User programmable bit 7.
        /// </summary>
        PSV57 = (ushort)Bits.Bit08,
        /// <summary>
        /// User programmable bit 8.
        /// </summary>
        PSV58 = (ushort)Bits.Bit09,
        /// <summary>
        /// User programmable bit 9.
        /// </summary>
        PSV59 = (ushort)Bits.Bit10,
        /// <summary>
        /// User programmable bit 10.
        /// </summary>
        PSV60 = (ushort)Bits.Bit11,
        /// <summary>
        /// User programmable bit 11.
        /// </summary>
        PSV61 = (ushort)Bits.Bit12,
        /// <summary>
        /// User programmable bit 12.
        /// </summary>
        PSV62 = (ushort)Bits.Bit13,
        /// <summary>
        /// User programmable bit 13.
        /// </summary>
        PSV63 = (ushort)Bits.Bit14,
        /// <summary>
        /// User programmable bit 14.
        /// </summary>
        PSV64 = (ushort)Bits.Bit15,
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (ushort)Bits.Nil
    }

    /// <summary>
    /// SEL Fast Message frame rate enumeration.
    /// </summary>
    [Serializable()]
    public enum MessagePeriod : ushort
    {
        /// <summary>
        /// Default message rate, 20 messages per second.
        /// </summary>
        DefaultRate = 0x0000,
        /// <summary>
        /// 20 messages per second.
        /// </summary>
        TwentyPerSecond = 0x0005,
        /// <summary>
        /// 10 messages per second.
        /// </summary>
        TenPerSecond = 0x000A,
        /// <summary>
        /// 5 messages per second.
        /// </summary>
        FivePerSecond = 0x0016,
        /// <summary>
        /// 4 messages per second.
        /// </summary>
        FourPerSecond = 0x0019,
        /// <summary>
        /// 2 messages per second.
        /// </summary>
        TwoPerSecond = 0x0032,
        /// <summary>
        /// 1 packet per second, or 60 messages per minute.
        /// </summary>
        OnePerSecond = 0x0064,
        /// <summary>
        /// 30 messages per minute, or 1/2 packet per second.
        /// </summary>
        ThirtyPerMinute = 0x00C8,
        /// <summary>
        /// 20 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        TwentyPerMinute = 0x012C,
        /// <summary>
        /// 15 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        FifteenPerMinute = 0x0190,
        /// <summary>
        /// 12 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        TwelvePerMinute = 0x01F4,
        /// <summary>
        /// 10 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        TenPerMinute = 0x0258,
        /// <summary>
        /// 6 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        SixPerMinute = 0x03E8,
        /// <summary>
        /// 4 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        FourPerMinute = 0x05DC,
        /// <summary>
        /// 3 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        ThreePerMinute = 0x07D0,
        /// <summary>
        /// 2 messages per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        TwoPerMinute = 0x0BB8,
        /// <summary>
        /// 1 message per minute.
        /// </summary>
        /// <remarks>Only supported by SEL 300 series relays.</remarks>
        OnePerMinute = 0x1770
    }

    /// <summary>
    /// SEL Fast Message device commands enumeration.
    /// </summary>
    [Serializable()]
    public enum DeviceCommand : byte
    {
        /// <summary>
        /// Enable unsolicited write messages.
        /// </summary>
        EnableUnsolicitedMessages = 0x01,
        /// <summary>
        /// Disable unsolicited write messages.
        /// </summary>
        DisableUnsolicitedMessages = 0x02,
        /// <summary>
        /// Undefined command.
        /// </summary>
        Undefined = 0xFF
    }

    #endregion

    /// <summary>
    /// Common SEL Fast Message declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// SEL Fast message data frame header byte 1.
        /// </summary>
        public const byte HeaderByte1 = 0xA5;
        
        /// <summary>
        /// SEL Fast message data frame header byte 2.
        /// </summary>
        public const byte HeaderByte2 = 0x46;

        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumPhasorValues = 8;

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumAnalogValues = 0;

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        public const ushort MaximumDigitalValues = 0;

        /// <summary>
        /// Absolute maximum data length (in bytes) that could fit into any frame.
        /// </summary>
        public const ushort MaximumDataLength = (ushort)FrameSize.A;

        /// <summary>
        /// Absolute maximum number of bytes of extended data that could fit into a command frame.
        /// </summary>
        public const ushort MaximumExtendedDataLength = 0;
    }
}