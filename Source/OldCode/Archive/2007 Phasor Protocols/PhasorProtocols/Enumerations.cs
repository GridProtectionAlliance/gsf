//*******************************************************************************************************
//  Enumerations.cs
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
//  07/11/2007 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Phasor coordinate formats enumeration.
    /// </summary>
    [Serializable()]
    public enum CoordinateFormat : byte
    {
        /// <summary>
        /// Rectangular coordinate format.
        /// </summary>
        Rectangular,
        /// <summary>
        /// Polar coordinate format.
        /// </summary>
        Polar
    }

    /// <summary>
    /// Composite polar value indicies enumeration.
    /// </summary>
    public enum CompositePhasorValue
    {
        /// <summary>
        /// Composite angle value index.
        /// </summary>
        Angle,
        /// <summary>
        /// Composite magnitude value index.
        /// </summary>
        Magnitude
    }

    /// <summary>
    /// Composite frequency value indicies enumeration.
    /// </summary>
    public enum CompositeFrequencyValue
    {
        /// <summary>
        /// Composite frequency value index.
        /// </summary>
        Frequency,
        /// <summary>
        /// Composite df/ft value index.
        /// </summary>
        DfDt
    }

    /// <summary>
    /// Phasor types enumeration.
    /// </summary>
    [Serializable()]
    public enum PhasorType : byte
    {
        /// <summary>
        /// Voltage phasor.
        /// </summary>
        Voltage,
        /// <summary>
        /// Current phasor.
        /// </summary>
        Current
    }

    /// <summary>
    /// Data transmission formats enumeration.
    /// </summary>
    [Serializable()]
    public enum DataFormat : byte
    {
        /// <summary>
        /// Fixed integer data transmission format.
        /// </summary>
        FixedInteger,
        /// <summary>
        /// Floating point data transmission format.
        /// </summary>
        FloatingPoint
    }

    /// <summary>
    /// Nominal line frequencies enumeration.
    /// </summary>
    [Serializable()]
    public enum LineFrequency : int
    {
        /// <summary>
        /// 50Hz nominal frequency.
        /// </summary>
        Hz50 = 50,
        /// <summary>
        /// 60Hz nominal frequency.
        /// </summary>
        Hz60 = 60
    }

    /// <summary>
    /// Fundamental frame types enumeration.
    /// </summary>
    [Serializable()]
    public enum FundamentalFrameType : int
    {
        /// <summary>
        /// Configuration frame.
        /// </summary>
        ConfigurationFrame,
        /// <summary>
        /// Data frame.
        /// </summary>
        DataFrame,
        /// <summary>
        /// Header frame.
        /// </summary>
        HeaderFrame,
        /// <summary>
        /// Command frame.
        /// </summary>
        CommandFrame,
        /// <summary>
        /// Undetermined frame type.
        /// </summary>
        Undetermined
    }

    /// <summary>
    /// Protocol independent common status flags enumeration.
    /// </summary>
    /// <remarks>
    /// These flags are expected to exist in the hi-word of a double-word flag set such that original word flags remain in-tact
    /// in lo-word of double-word flag set.
    /// </remarks>
    [Flags(), Serializable()]
    public enum CommonStatusFlags : int
    {
        /// <summary>
        /// Data is valid (0 when PMU data is valid, 1 when invalid or PMU is in test mode).
        /// </summary>
        DataIsValid = Bit.Bit19,
        /// <summary>
        /// Synchronization is valid (0 when in PMU is in sync, 1 when it is not).
        /// </summary>
        SynchronizationIsValid = Bit.Bit18,
        /// <summary>
        /// Data sorting type, 0 by timestamp, 1 by arrival.
        /// </summary>
        DataSortingType = Bit.Bit17,
        /// <summary>
        /// PMU error including configuration error, 0 when no error.
        /// </summary>
        PmuError = Bit.Bit16,
        /// <summary>
        /// Reserved bits for future common flags, presently set to 0.
        /// </summary>
        ReservedFlags = Bit.Bit20 | Bit.Bit21 | Bit.Bit22 | Bit.Bit23 | Bit.Bit24 | Bit.Bit25 | Bit.Bit26 | Bit.Bit27 | Bit.Bit28 | Bit.Bit29 | Bit.Bit30 | Bit.Bit31
    }

    /// <summary>
    /// Data sorting types enumeration.
    /// </summary>
    [Serializable()]
    public enum DataSortingType : int
    {
        /// <summary>
        /// Data sorted by timestamp (typical situation).
        /// </summary>
        ByTimestamp,
        /// <summary>
        /// Data sorted by arrival (bad timestamp).
        /// </summary>
        ByArrival
    }

    /// <summary>
    /// Phasor data protocols enumeration.
    /// </summary>
    [Serializable()]
    public enum PhasorProtocol : int
    {
        /// <summary>
        /// IEEE C37.118-2005 protocol.
        /// </summary>
        IeeeC37_118V1,
        /// <summary>
        /// IEEE C37.118, draft 6 protocol.
        /// </summary>
        IeeeC37_118D6,
        /// <summary>
        /// IEEE 1344-1995 protocol.
        /// </summary>
        Ieee1344,
        /// <summary>
        /// BPA PDCstream protocol.
        /// </summary>
        BpaPdcStream,
        /// <summary>
        /// Virgina Tech FNET protocol.
        /// </summary>
        FNet
    }

    /// <summary>
    /// Phasor enabled device commands enumeration.
    /// </summary>
    [Serializable()]
    public enum DeviceCommand : short
    {
        /// <summary>
        /// 0001 Turn off transmission of data frames.
        /// </summary>
        DisableRealTimeData = Bit.Bit0,
        /// <summary>
        /// 0010 Turn on transmission of data frames.
        /// </summary>
        EnableRealTimeData = Bit.Bit1,
        /// <summary>
        /// 0011 Send header file.
        /// </summary>
        SendHeaderFrame = Bit.Bit0 | Bit.Bit1,
        /// <summary>
        /// 0100 Send configuration file 1.
        /// </summary>
        SendConfigurationFrame1 = Bit.Bit2,
        /// <summary>
        /// 0101 Send configuration file 2.
        /// </summary>
        SendConfigurationFrame2 = Bit.Bit0 | Bit.Bit2,
        /// <summary>
        /// 1000 Receive extended frame for IEEE C37.118 / receive reference phasor for IEEE 1344.
        /// </summary>
        ReceiveExtendedFrame = Bit.Bit3,
        /// <summary>
        /// Reserved bits.
        /// </summary>
        ReservedBits = short.MaxValue & ~(Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3)
    }
}