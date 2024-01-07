//******************************************************************************************************
//  PhasorValue.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  08/23/2023 - Timothy Liakh
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.Functions.BuiltIn;
using GSF.TimeSeries;

namespace GrafanaAdapters.DataSources;

/// <summary>
/// Defines a default target for a phasor value.
/// </summary>
public enum PhasorValueTarget
{
    /// <summary>
    /// Target the magnitude components of the phasor value.
    /// </summary>
    Magnitude,
    /// <summary>
    /// Target the angle components of the phasor value.
    /// </summary>
    Angle
}

/// <summary>
/// Represents an individual time-series phasor value from a data source.
/// </summary>
public partial struct PhasorValue
{
    /// <summary>
    /// Query magnitude target, e.g., a point-tag.
    /// </summary>
    public string MagnitudeTarget;

    /// <summary>
    /// Query angle target, e.g., a point-tag.
    /// </summary>
    public string AngleTarget;

    /// <summary>
    /// Queried magnitude value.
    /// </summary>
    public double Magnitude;

    /// <summary>
    /// Queried angle value.
    /// </summary>
    public double Angle;

    /// <summary>
    /// Timestamp, in Unix epoch milliseconds, of queried value.
    /// </summary>
    public double Time;

    /// <summary>
    /// Flags for queried value.
    /// </summary>
    public MeasurementStateFlags Flags;

    /// <summary>
    /// Gets or sets the primary target for the phasor value.
    /// </summary>
    /// <remarks>
    /// This property is used to determine which components of the phasor value to use when using the data
    /// source as an <see cref="IDataSourceValue{T}"/>. This is useful in default function computations that
    /// do not need to operate on both the magnitude and angle components of the phasor value. For example,
    /// see <see cref="Minimum{T}"/> and <see cref="Maximum{T}"/> functions that only operate on magnitudes.
    /// Primary target value defaults to <see cref="PhasorValueTarget.Magnitude"/> but can be overridden to
    /// <see cref="PhasorValueTarget.Angle"/> if the function only needs to operate on the angle components.
    /// See <see cref="WrapAngle{T}"/> for example of using only angle components.
    /// </remarks>
    public PhasorValueTarget PrimaryTarget;
}