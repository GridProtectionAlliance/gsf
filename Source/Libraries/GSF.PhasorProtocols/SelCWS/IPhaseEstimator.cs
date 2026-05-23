//******************************************************************************************************
//  IPhaseEstimator.cs - Gbtc
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
//  05/23/2026 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Defines a SEL CWS six-phase estimator that consumes point-on-wave sample groups and produces
/// <see cref="PhaseEstimate"/> results.
/// </summary>
/// <remarks>
/// Implementations represent interchangeable phasor estimation algorithms (see
/// <see cref="PhaseEstimationAlgorithm"/>) selected via the SEL CWS connection parameters.
/// </remarks>
internal interface IPhaseEstimator
{
    /// <summary>
    /// Pushes one interleaved sample-group (VA, VB, VC, IA, IB, IC) with its epoch nanoseconds.
    /// </summary>
    /// <param name="va">Current sample for phase A voltage.</param>
    /// <param name="vb">Current sample for phase B voltage.</param>
    /// <param name="vc">Current sample for phase C voltage.</param>
    /// <param name="ia">Current sample for phase A current.</param>
    /// <param name="ib">Current sample for phase B current.</param>
    /// <param name="ic">Current sample for phase C current.</param>
    /// <param name="epochNanoseconds">Timestamp in nanoseconds since epoch.</param>
    /// <param name="phaseEstimateHandler">Handler for phase estimate result.</param>
    /// <returns>
    /// <c>true</c> when an estimate is available, and it is time to publish at the target output rate;
    /// otherwise, <c>false</c>.
    /// </returns>
    bool Step(
        double va,
        double vb,
        double vc,
        double ia,
        double ib,
        double ic,
        long epochNanoseconds,
        PhaseEstimateHandler phaseEstimateHandler);

    /// <summary>
    /// Resets the estimator to its initial state.
    /// </summary>
    void Reset();
}
