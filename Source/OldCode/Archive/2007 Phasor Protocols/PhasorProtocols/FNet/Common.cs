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
//  02/08/2007 - James R Carroll & Jian (Ryan) Zuo
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;

namespace PCS.PhasorProtocols.FNet
{
    #region [ Enumerations ]

    /// <summary>
    /// F-NET data elements enumeration structure.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815")]
    internal struct Element
    {
        // This is a structure to avoid having to cast when using elements as an index
        public const int UnitID = 0;
        public const int Date = 1;
        public const int Time = 2;
        public const int SampleIndex = 3;
        public const int Analog = 4;
        public const int Frequency = 5;
        public const int Voltage = 6;
        public const int Angle = 7;
    }

    #endregion

    /// <summary>
    /// Common F-NET declarations and functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// F-NET data frame start byte.
        /// </summary>
        public const byte StartByte = 0x1;

        /// <summary>
        /// F-NET data frame start byte.
        /// </summary>
        public const byte EndByte = 0x0;

        /// <summary>
        /// Absolute maximum number of possible phasor values that could fit into a data frame.
        /// </summary>
        public const int MaximumPhasorValues = 1;

        /// <summary>
        /// Absolute maximum number of possible analog values that could fit into a data frame.
        /// </summary>
        /// <remarks>F-NET doesn't support analog values.</remarks>
        public const int MaximumAnalogValues = 0;

        /// <summary>
        /// Absolute maximum number of possible digital values that could fit into a data frame.
        /// </summary>
        /// <remarks>F-NET doesn't support digital values.</remarks>
        public const int MaximumDigitalValues = 0;

        /// <summary>
        /// Default frame rate for F-NET devices is 10 frames per second.
        /// </summary>
        public const ushort DefaultFrameRate = 10;

        /// <summary>
        /// Default nominal frequency for F-NET devices is 60Hz.
        /// </summary>
        public const LineFrequency DefaultNominalFrequency = LineFrequency.Hz60;

        /// <summary>
        /// Default real-time ticks offset for F-NET.
        /// </summary>
        public const long DefaultTimeOffset = 110000000;

        /// <summary>
        /// Default F-NET station name.
        /// </summary>
        public const string DefaultStationName = "F-NET Unit";
    }
}