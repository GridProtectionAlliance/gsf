//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.Units.EE;

namespace GSF.PhasorProtocols.FNET
{
    #region [ Enumerations ]

    /// <summary>
    /// F-NET data elements enumeration structure.
    /// </summary>
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
        /// The maximum practical limit for an F-NET data frame.        
        /// </summary>
        public const int MaximumPracticalFrameSize = 4096;

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