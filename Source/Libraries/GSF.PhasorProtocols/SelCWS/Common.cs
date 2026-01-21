//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2025 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

#region [ Enumerations ]

/// <summary>
/// SEL CWS frame types enumeration.
/// </summary>
[Serializable]
public enum FrameType : byte
{
    /// <summary>
    /// Data frame.
    /// </summary>
    DataFrame = 0x00,
    /// <summary>
    /// Configuration frame.
    /// </summary>
    ConfigurationFrame = 0x01
}

/// <summary>
/// Phase channels for SEL CWS PoW analogs.
/// </summary>
public enum PhaseChannel
{ 
    /// <summary>
    /// Phase A current (IA).
    /// </summary>
    IA = 0,
    /// <summary>
    /// Phase B current (IB).
    /// </summary>
    IB = 1,
    /// <summary>
    /// Phase C current (IC).
    /// </summary>
    IC = 2,
    /// <summary>
    /// Phase A voltage (VA).
    /// </summary>
    VA = 3,
    /// <summary>
    /// Phase B voltage (VB).
    /// </summary>
    VB = 4,
    /// <summary>
    /// Phase C voltage (VC).
    /// </summary>
    VC = 5
}

#endregion

/// <summary>
/// Common SEL CWS declarations and functions.
/// </summary>
public static class Common
{
    /// <summary>
    /// SEL CWS data supported version.
    /// </summary>
    public const byte Version = 0x1;

    /// <summary>
    /// Absolute maximum number of possible phasor values that could fit into a data frame.
    /// </summary>
    public const int MaximumPhasorValues = 6;

    /// <summary>
    /// Absolute maximum number of possible analog values that could fit into a data frame.
    /// </summary>
    /// <remarks>SEL CWS doesn't support analog values.</remarks>
    public const int MaximumAnalogValues = 6;

    /// <summary>
    /// Absolute maximum number of possible digital values that could fit into a data frame.
    /// </summary>
    /// <remarks>SEL CWS doesn't support digital values.</remarks>
    public const int MaximumDigitalValues = 0;

    /// <summary>
    /// Default frame rate for SEL CWS devices is 3000 frames per second.
    /// </summary>
    public const ushort DefaultFramePerSecond = 3000;

    /// <summary>
    /// Frames per UDP packet for SEL CWS devices is 50 samples.
    /// </summary>
    public const int FramesPerPacket = 50;

    /// <summary>
    /// Default nominal frequency for SEL CWS devices is 60Hz.
    /// </summary>
    public const LineFrequency DefaultNominalFrequency = LineFrequency.Hz60;
}