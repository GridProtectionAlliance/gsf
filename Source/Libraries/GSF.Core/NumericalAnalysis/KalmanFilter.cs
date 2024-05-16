//******************************************************************************************************
//  KalmanFilter.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  05/16/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

namespace GSF.NumericalAnalysis;

/// <summary>
/// Predicts the next state based on the current estimate.
/// </summary>
public class KalmanFilter
{
    /// <summary>
    /// Default process noise covariance.
    /// </summary>
    public const double DefaultProcessNoise = 1.0E-5D;      // Small process noise

    /// <summary>
    /// Default measurement noise covariance.
    /// </summary>
    public const double DefaultMeasurementNoise = 1.0E-3D;  // Small measurement noise

    /// <summary>
    /// Default estimate error.
    /// </summary>
    public const double DefaultEstimateError = 1.0D;

    private readonly double Q;  // Process noise covariance
    private readonly double R;  // Measurement noise covariance
    private double P;           // Estimate error covariance
    private double K;           // Kalman gain
    private double X;           // Estimated value

    private bool m_initialized;

    /// <summary>
    /// Creates a new Kalman filter.
    /// </summary>
    /// <param name="processNoise">
    /// <para>
    /// Determines how much the system state is expected to change between measurements.
    /// </para>
    /// <para>
    /// Start with a very small value (e.g., 1e-5) and gradually increase it. Too small a value can
    /// make the filter slow to adapt to changes, while too large can make it over-responsive to noise.
    /// </para>
    /// </param>
    /// <param name="measurementNoise">
    /// <para>
    /// Reflects the confidence in the measurements. A lower value gives more weight to the measurements.
    /// </para>
    /// <para>
    /// If your measurements are accurate, set R to a small value (e.g., 1e-3). Increase it if the
    /// measurements are noisy.
    /// </para>
    /// </param>
    /// <param name="estimatedError">
    /// <para>
    /// Represents the initial guess about the error in the state estimate.
    /// </para>
    /// <para>
    /// </para>
    /// Start with a value that reflects the expected variability in the initial state.
    /// </param>
    public KalmanFilter(double processNoise = DefaultProcessNoise, double measurementNoise = DefaultMeasurementNoise, double estimatedError = DefaultProcessNoise)
    {
        Q = processNoise;
        R = measurementNoise;
        P = estimatedError;
        K = 0;
    }

    /// <summary>
    /// Updates the filter with a new measurement.
    /// </summary>
    /// <param name="measurement">New measurement value.</param>
    /// <returns>Predicted next state.</returns>
    public double Update(double measurement)
    {
        if (!m_initialized)
        {
            X = measurement;
            m_initialized = true;
        }

        // Prediction update
        P += Q;

        // Measurement update
        K = P / (P + R);
        X += K * (measurement - X);
        P = (1 - K) * P;

        // Return the updated state prediction
        return X;
    }
}