//******************************************************************************************************
//  PhaseEstimate.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  11/04/2025 - Ritchie Carroll
//       Generated original version of source code in collaboration with ChatGPT.
//  05/23/2026 - Ritchie Carroll
//       Extracted shared phase-estimate result type and handler so multiple phase-estimation
//       algorithms (sliding DFT and IEEE C37.118-2018 Annex D) can produce a common output.
//
//******************************************************************************************************
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

using System;
using GSF.Units;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the output of a SEL CWS phase estimation algorithm.
/// </summary>
/// <remarks>
/// This is the common result type produced by every <see cref="IPhaseEstimator"/> implementation,
/// regardless of the underlying estimation algorithm.
/// </remarks>
public readonly ref struct PhaseEstimate
{
    /// <summary>
    /// Gets frequency estimate in hertz.
    /// </summary>
    public required double Frequency { get; init; }

    /// <summary>
    /// Gets rate of change of frequency (ROCOF) in Hz/s.
    /// </summary>
    public required double dFdt { get; init; }

    /// <summary>
    /// Gets angles in radians, length 6: VA, VB, VC, IA, IB, IC.
    /// </summary>
    /// <remarks>
    /// The data this span references is owned by the phase estimator instance,
    /// it is only valid during the scope of the <see cref="PhaseEstimateHandler"/> delegate call.
    /// If you need to retain the data, make a copy.
    /// </remarks>
    public required ReadOnlySpan<Angle> Angles { get; init; }

    /// <summary>
    /// Gets RMS magnitudes, length 6: VA, VB, VC, IA, IB, IC.
    /// </summary>
    /// <remarks>
    /// The data this span references is owned by the phase estimator instance,
    /// it is only valid during the scope of the <see cref="PhaseEstimateHandler"/> delegate call.
    /// If you need to retain the data, make a copy.
    /// </remarks>
    public required ReadOnlySpan<double> Magnitudes { get; init; }
}

/// <summary>
/// Delegate for handling a <see cref="PhaseEstimate"/>.
/// </summary>
/// <param name="estimate">Phase estimate.</param>
public delegate void PhaseEstimateHandler(in PhaseEstimate estimate);
