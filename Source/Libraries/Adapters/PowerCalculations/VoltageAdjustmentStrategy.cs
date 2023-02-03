//******************************************************************************************************
//  VoltageAdjustmentStrategy.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/27/2016 - J. Ritchie Carroll
//       Extracted into stand alone class.
//
//******************************************************************************************************

using System.ComponentModel;

namespace PowerCalculations;

/// <summary>
/// Represents the strategy used to adjust voltage values for power
/// calculations based on the nature of the voltage measurement.
/// </summary>
public enum VoltageAdjustmentStrategy
{
    /// <summary>
    /// Factor of 3 adjustment (S=3*V*I)
    /// </summary>
    [Description("Estimated 3-phase line-to-neutral: factor of 3 adjustment (S=3*V*I)")]
    LineToNeutral,

    /// <summary>
    /// Factor of Sqrt(3) adjustment (S=Sqrt(3)*V*I)
    /// </summary>
    [Description("Estimated 3-phase line-to-line: factor of Sqrt(3) adjustment (S=Sqrt(3)*V*I)")]
    LineToLine,

    /// <summary>
    /// Factor of 1 adjustment (S=1*V*I). Same as <see cref="None"/>.
    /// </summary>
    [Description("1-phase line-to-neutral: factor of 1 adjustment (S=1*V*I). Same as None")]
    LineToNeutralSinglePhase,

    /// <summary>
    /// Factor of 1/Sqrt(3) adjustment (S=V*I/Sqrt(3))
    /// </summary>
    [Description("1-phase line-to-line: factor of 1/Sqrt(3) adjustment (S=V*I/Sqrt(3))")]
    LineToLineSinglePhase,

    /// <summary>
    /// No adjustment (S=V*I)
    /// </summary>
    [Description("No adjustment (S=V*I)")]
    None
}