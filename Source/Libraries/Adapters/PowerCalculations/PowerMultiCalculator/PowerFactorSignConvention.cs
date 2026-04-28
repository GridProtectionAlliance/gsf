//******************************************************************************************************
//  PowerFactorSignConvention.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/28/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel;

namespace PowerCalculations.PowerMultiCalculator;

/// <summary>
/// Represents the sign convention used when calculating power factor.
/// </summary>
/// <remarks>
/// <para>
/// Power factor magnitude is always defined as <c>|P| / |S|</c> in the range <c>[0, 1]</c>. The sign
/// convention determines how reactive power direction (<c>Q</c>) is encoded into the published value:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Unsigned"/> publishes the magnitude only — values are in <c>[0, 1]</c>.</description></item>
///   <item><description><see cref="Lagging"/> publishes a positive value when reactive power is consumed (inductive, <c>Q &gt; 0</c>)
///       and a negative value when reactive power is supplied (capacitive, <c>Q &lt; 0</c>) — values are in <c>[-1, +1]</c>.</description></item>
///   <item><description><see cref="Leading"/> inverts the <see cref="Lagging"/> convention: positive when capacitive (<c>Q &lt; 0</c>),
///       negative when inductive (<c>Q &gt; 0</c>) — values are in <c>[-1, +1]</c>.</description></item>
/// </list>
/// </remarks>
public enum PowerFactorSignConvention
{
    /// <summary>
    /// Publishes power factor magnitude only, <c>|P| / |S|</c>, range <c>[0, 1]</c>.
    /// </summary>
    [Description("Power factor magnitude only, range [0, 1].")]
    Unsigned,

    /// <summary>
    /// Positive sign indicates a lagging (inductive) load, <c>Q &gt; 0</c>; negative sign indicates a leading (capacitive) load.
    /// </summary>
    [Description("Positive value indicates lagging (inductive) load; negative value indicates leading (capacitive).")]
    Lagging,

    /// <summary>
    /// Positive sign indicates a leading (capacitive) load, <c>Q &lt; 0</c>; negative sign indicates a lagging (inductive) load.
    /// </summary>
    [Description("Positive value indicates leading (capacitive) load; negative value indicates lagging (inductive).")]
    Leading
}
