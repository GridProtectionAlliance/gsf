//******************************************************************************************************
//  Measurement.cs - Gbtc
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
//  11/2/2015 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace PowerCalculations.PowerMultiCalculator;

/// <summary>
/// Simple model class for measurements
/// </summary>
public class PowerMeasurement
{
    #region [ Properties ]

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Signal ID
    /// </summary>
    public Guid SignalID { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Point Tag
    /// </summary>
    public string PointTag { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Adder
    /// </summary>
    public int Adder { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Multiplier
    /// </summary>
    public int Multiplier { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Device ID
    /// </summary>
    public int DeviceID { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Historian ID
    /// </summary>
    public int? HistorianID { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Signal Type ID
    /// </summary>
    public int SignalTypeID { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s Enabled flag
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PowerMeasurement"/>'s signal reference
    /// </summary>
    public string SignalReference { get; set; }

    #endregion
}