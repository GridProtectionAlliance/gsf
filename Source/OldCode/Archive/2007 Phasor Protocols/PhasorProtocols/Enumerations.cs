//*******************************************************************************************************
//  Enumerations.vb - Global enumerations for this namespace
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/11/2007 - J. Ritchie Carroll
//       Moved all namespace level enumerations into "Enumerations.vb" file
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using TVA;
using TVA.Interop;

namespace PhasorProtocols
{
    /// <summary>Phasor coordinate formats</summary>
    [Serializable()]
    public enum CoordinateFormat : byte
    {
        Rectangular,
        Polar
    }

    /// <summary>Composite polar values</summary>
    [SuppressMessage("Microsoft.Performance", "CA1815")]
    public struct CompositePhasorValue
    {
        public const int Angle = 0;
        public const int Magnitude = 1;
    }

    /// <summary>Composite frequency values</summary>
    [SuppressMessage("Microsoft.Performance", "CA1815")]
    public struct CompositeFrequencyValue
    {
        public const int Frequency = 0;
        public const int DfDt = 1;
    }

    /// <summary>Phasor types</summary>
    [Serializable()]
    public enum PhasorType : byte
    {
        Voltage,
        Current
    }

    /// <summary>Data transmission formats</summary>
    [Serializable()]
    public enum DataFormat : byte
    {
        FixedInteger,
        FloatingPoint
    }

    /// <summary>Nominal line frequencies</summary>
    [Serializable()]
    public enum LineFrequency : int
    {
        Hz50 = 50,
        Hz60 = 60
    }

    /// <summary>Fundamental frame types</summary>
    [Serializable()]
    public enum FundamentalFrameType : int
    {
        ConfigurationFrame,
        DataFrame,
        HeaderFrame,
        CommandFrame,
        Undetermined
    }

    /// <summary>Protocol independent common flag set</summary>
    /// <remarks>These flags are expected to exist in the hi-word of a double-word flag set - this way original word flags remain in-tact</remarks>
    [Flags(), Serializable()]
    public enum CommonStatusFlags : int
    {
        /// <summary>Data is valid (0 when PMU data is valid, 1 when invalid or PMU is in test mode)</summary>
        DataIsValid = Bit.Bit19,
        /// <summary>Synchronization is valid (0 when in PMU is in sync, 1 when it is not)</summary>
        SynchronizationIsValid = Bit.Bit18,
        /// <summary>Data sorting type, 0 by timestamp, 1 by arrival</summary>
        DataSortingType = Bit.Bit17,
        /// <summary>PMU error including configuration error, 0 when no error</summary>
        PmuError = Bit.Bit16,
        /// <summary>Reserved bits for future common flags, presently set to 0</summary>
        ReservedFlags = Bit.Bit20 | Bit.Bit21 | Bit.Bit22 | Bit.Bit23 | Bit.Bit24 | Bit.Bit25 | Bit.Bit26 | Bit.Bit27 | Bit.Bit28 | Bit.Bit29 | Bit.Bit30 | Bit.Bit31
    }

    /// <summary>Data sorting types</summary>
    [Serializable()]
    public enum DataSortingType : int
    {
        /// <summary>Data sorted by timestamp (typical situation)</summary>
        ByTimestamp,
        /// <summary>Data sorted by arrival (bad timestamp)</summary>
        ByArrival
    }

    /// <summary>Phasor data protocols</summary>
    [Serializable()]
    public enum PhasorProtocol : int
    {
        IeeeC37_118V1,
        IeeeC37_118D6,
        Ieee1344,
        BpaPdcStream,
        FNet
    }

    /// <summary>Phasor enabled device commands</summary>
    [Serializable()]
    public enum DeviceCommand : short
    {
        /// <summary>0001 Turn off transmission of data frames</summary>
        DisableRealTimeData = Bit.Bit0,
        /// <summary>0010 Turn on transmission of data frames</summary>
        EnableRealTimeData = Bit.Bit1,
        /// <summary>0011 Send header file</summary>
        SendHeaderFrame = Bit.Bit0 | Bit.Bit1,
        /// <summary>0100 Send configuration file 1</summary>
        SendConfigurationFrame1 = Bit.Bit2,
        /// <summary>0101 Send configuration file 2</summary>
        SendConfigurationFrame2 = Bit.Bit0 | Bit.Bit2,
        /// <summary>1000 Receive extended frame for IEEE C37.118 / receive reference phasor for IEEE 1344</summary>
        ReceiveExtendedFrame = Bit.Bit3,
        /// <summary>Reserved bits</summary>
        ReservedBits = short.MaxValue & ~(Bit.Bit0 | Bit.Bit1 | Bit.Bit2 | Bit.Bit3)
    }
}
