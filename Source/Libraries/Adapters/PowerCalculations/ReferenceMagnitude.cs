//******************************************************************************************************
//  ReferenceMagnitude.cs - Gbtc
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
//  11/22/2006 - J. Ritchie Carroll
//       Initial version of source generated
//  12/23/2009 - Jian R. Zuo
//       Converted code to C#;
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for magnitude calculation.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF.TimeSeries;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations;

/// <summary>
/// Calculates an average magnitude associated with a composed reference angle.
/// </summary>
[Description("Reference Magnitude: Calculates an average magnitude associated with a composed reference angle")]
public class ReferenceMagnitude : CalculatedMeasurementBase
{
    #region [ Members ]

    // Fields
    private double m_referenceMagnitude;

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Returns the detailed status of the <see cref="ReferenceMagnitude"/> calculator.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.AppendLine($" Last calculated magnitude: {m_referenceMagnitude}");
            status.Append(base.Status);

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Initializes the <see cref="ReferenceMagnitude"/> calculator.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        // Validate input measurements
        List<MeasurementKey> validInputMeasurementKeys = new();

        for (int i = 0; i < InputMeasurementKeys.Length; i++)
        {
            SignalType keyType = InputMeasurementKeyTypes[i];

            // Make sure measurement key type is a phase magnitude
            if (keyType is SignalType.VPHM or SignalType.IPHM)
                validInputMeasurementKeys.Add(InputMeasurementKeys[i]);
        }

        if (validInputMeasurementKeys.Count == 0)
            throw new InvalidOperationException("No valid phase magnitudes were specified as inputs to the reference magnitude calculator.");

        if (InputMeasurementKeyTypes.Count(s => s == SignalType.VPHM) > 0 && InputMeasurementKeyTypes.Count(s => s == SignalType.IPHM) > 0)
            throw new InvalidOperationException("A mixture of voltage and current phase magnitudes were specified as inputs to the reference magnitude calculator - you must specify one or the other: only voltage phase magnitudes or only current phase magnitudes.");

        // Make sure only phase magnitudes are used as input
        InputMeasurementKeys = validInputMeasurementKeys.ToArray();

        // Validate output measurements
        if (OutputMeasurements.Length < 1)
            throw new InvalidOperationException("An output measurement was not specified for the reference magnitude calculator - one measurement is expected to represent the \"Calculated Reference Magnitude\" value.");
    }

    /// <summary>
    /// Calculates the average reference magnitude.
    /// </summary>
    /// <param name="frame">Single frame of measurement data within a one second sample.</param>
    /// <param name="index">Index of frame within the one second sample.</param>
    protected override void PublishFrame(IFrame frame, int index)
    {
        if (frame.Measurements.Count > 0)
        {
            // Calculate the average magnitude
            m_referenceMagnitude = frame.Measurements.Values.Select(m => m.AdjustedValue).Average();

            // Provide calculated measurement for external consumption
            OnNewMeasurements(new IMeasurement[] { Measurement.Clone(OutputMeasurements[0], m_referenceMagnitude, frame.Timestamp) });
        }
        else
        {
            m_referenceMagnitude = 0.0D;
        }
    }

    #endregion
}