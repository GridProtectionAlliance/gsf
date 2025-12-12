//******************************************************************************************************
//  RollingPhaseEstimator.cs - Gbtc
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
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable GrammarMistakeInComment
// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using GSF.Units;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the output of the phase estimation algorithm.
/// </summary>
internal struct PhaseEstimate
{
    /// <summary>Smoothed frequency estimate in hertz.</summary>
    public double Frequency;

    /// <summary>Rate of change of frequency in Hz/s.</summary>
    public double dFdt;

    /// <summary>Angles in radians, length 6: IA, IB, IC, VA, VB, VC.</summary>
    public Angle[] Angles;

    /// <summary>RMS Magnitudes, length 6: IA, IB, IC, VA, VB, VC.</summary>
    public double[] Magnitudes;
}

/// <summary>
/// Represents a rolling six-phase estimator using a sliding DFT algorithm.
/// </summary>
/// <remarks>
/// <para>
/// This class implements a real-time phasor measurement algorithm suitable for
/// power system applications. It uses a sliding Discrete Fourier Transform (SDFT)
/// approach to efficiently compute phasors at each sample with O(1) complexity.
/// </para>
/// <para>
/// Frequency estimation is performed by tracking the rate of change of phase angle
/// from the reference channel (VA by default). The frequency and ROCOF outputs
/// are smoothed using exponential moving average filters.
/// </para>
/// <para>
/// The algorithm is designed for nominal 50Hz or 60Hz systems sampled at 3000 Hz,
/// but can be configured for other sample rates.
/// </para>
/// </remarks>
internal sealed class RollingPhaseEstimator
{
    #region [ Members ]

    // Constants
    private const int NumChannels = 6;
    private const int ReferenceChannel = 3; // VA used for frequency tracking
    private const double TwoPi = 2.0 * Math.PI;
    private const double NanosecondsPerSecond = 1e9;

    // Channel indices for clarity
    public const int IA = 0;
    public const int IB = 1;
    public const int IC = 2;
    public const int VA = 3;
    public const int VB = 4;
    public const int VC = 5;

    // Fields
    private readonly double m_samplePeriodSeconds;
    private readonly int m_recalculationInterval;

    // Circular sample buffers for each channel
    private readonly double[][] m_sampleBuffers;
    private int m_bufferWriteIndex;

    // DFT twiddle factor for sliding update: e^(j*2π/N)
    private readonly double m_twiddleReal;  // cos(2π/N)
    private readonly double m_twiddleImag;  // sin(2π/N)

    // Accumulated DFT phasors (real and imaginary parts) for each channel
    private readonly double[] m_phasorReal;
    private readonly double[] m_phasorImag;

    // Precomputed cosine/sine tables for full DFT recalculation
    private readonly double[] m_cosTable;
    private readonly double[] m_sinTable;

    // Previous phase angle for frequency calculation (from reference channel)
    private double m_prevPhaseAngle;
    private bool m_hasPrevPhase;

    // Frequency smoothing (exponential moving average)
    private readonly double m_frequencyAlpha;
    private double m_smoothedFrequency;
    private bool m_frequencyInitialized;

    // ROCOF smoothing
    private readonly double m_rocofAlpha;
    private double m_prevSmoothedFrequency;
    private double m_smoothedRocof;
    private bool m_rocofInitialized;

    // Timing
    private long m_prevEpochNs;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingPhaseEstimator"/> class.
    /// </summary>
    /// <param name="sampleRateHz">Sample rate in Hz (e.g., 3000).</param>
    /// <param name="nominalFrequency">Nominal line frequency (50 or 60 Hz).</param>
    /// <param name="targetCycles">Number of cycles in the analysis window (default: 2).</param>
    /// <param name="frequencySmoothingFactor">EMA alpha for frequency smoothing (0-1, default: 0.1).</param>
    /// <param name="rocofSmoothingFactor">EMA alpha for ROCOF smoothing (0-1, default: 0.05).</param>
    /// <param name="recalculationCycles">Number of cycles between full DFT recalculations for numerical stability (default: 10).</param>
    public RollingPhaseEstimator(
        double sampleRateHz,
        LineFrequency nominalFrequency,
        int targetCycles = 2,
        double frequencySmoothingFactor = 0.1,
        double rocofSmoothingFactor = 0.05,
        int recalculationCycles = 10)
    {
        // Validate parameters
        if (sampleRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRateHz), "Sample rate must be positive.");
        
        if (targetCycles < 1)
            throw new ArgumentOutOfRangeException(nameof(targetCycles), "Target cycles must be at least 1.");
        
        if (frequencySmoothingFactor is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(frequencySmoothingFactor), "Smoothing factor must be between 0 and 1.");
        
        if (rocofSmoothingFactor is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(rocofSmoothingFactor), "Smoothing factor must be between 0 and 1.");

        // Store configuration
        SampleRateHz = sampleRateHz;
        NominalFrequencyHz = (double)nominalFrequency;
        m_samplePeriodSeconds = 1.0 / sampleRateHz;
        int samplesPerNominalCycle = (int)Math.Round(sampleRateHz / NominalFrequencyHz);
        WindowSamples = samplesPerNominalCycle * targetCycles;
        m_recalculationInterval = samplesPerNominalCycle * recalculationCycles;

        // Initialize sample buffers
        m_sampleBuffers = new double[NumChannels][];

        for (int ch = 0; ch < NumChannels; ch++)
            m_sampleBuffers[ch] = new double[WindowSamples];

        // Precompute sliding DFT twiddle factor: e^(j*2π*k/N) where k = targetCycles
        // For a window of targetCycles complete cycles, bin k corresponds to the fundamental
        double twiddleAngle = TwoPi * targetCycles / WindowSamples;
        m_twiddleReal = Math.Cos(twiddleAngle);
        m_twiddleImag = Math.Sin(twiddleAngle);

        // Precompute cosine/sine tables for full DFT calculation at bin k = targetCycles
        m_cosTable = new double[WindowSamples];
        m_sinTable = new double[WindowSamples];
        
        for (int n = 0; n < WindowSamples; n++)
        {
            double angle = TwoPi * targetCycles * n / WindowSamples;
            m_cosTable[n] = Math.Cos(angle);
            m_sinTable[n] = Math.Sin(angle);
        }

        // Initialize phasor accumulators
        m_phasorReal = new double[NumChannels];
        m_phasorImag = new double[NumChannels];

        // Initialize frequency tracking
        m_smoothedFrequency = NominalFrequencyHz;
        m_prevSmoothedFrequency = NominalFrequencyHz;
        m_frequencyAlpha = frequencySmoothingFactor;
        m_rocofAlpha = rocofSmoothingFactor;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the configured sample rate in Hz.
    /// </summary>
    public double SampleRateHz { get; }

    /// <summary>
    /// Gets the nominal frequency in Hz.
    /// </summary>
    public double NominalFrequencyHz { get; }

    /// <summary>
    /// Gets the number of samples in the analysis window.
    /// </summary>
    public int WindowSamples { get; }

    /// <summary>
    /// Gets the total number of samples processed.
    /// </summary>
    public long TotalSamplesProcessed { get; private set; }

    /// <summary>
    /// Gets whether the estimator has filled its window and is producing valid estimates.
    /// </summary>
    public bool IsReady => TotalSamplesProcessed >= WindowSamples;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Push one interleaved sample-group (IA, IB, IC, VA, VB, VC) with its epoch nanoseconds.
    /// </summary>
    /// <param name="ia">Current sample for phase A current.</param>
    /// <param name="ib">Current sample for phase B current.</param>
    /// <param name="ic">Current sample for phase C current.</param>
    /// <param name="va">Current sample for phase A voltage.</param>
    /// <param name="vb">Current sample for phase B voltage.</param>
    /// <param name="vc">Current sample for phase C voltage.</param>
    /// <param name="epochNanoseconds">Timestamp in nanoseconds since epoch.</param>
    /// <param name="estimate">Output estimate; valid only when method returns true.</param>
    /// <returns><c>true</c> when an estimate is available (window has filled); otherwise, <c>false</c>.</returns>
    public bool Step(
        double ia, 
        double ib, 
        double ic, 
        double va, 
        double vb, 
        double vc, 
        long epochNanoseconds,
        out PhaseEstimate? estimate)
    {
        // Pack samples for processing
        double[] samples = new double[NumChannels];

        samples[0] = ia;
        samples[1] = ib;
        samples[2] = ic;
        samples[3] = va;
        samples[4] = vb;
        samples[5] = vc;

        // Determine if we're in fill-up mode or sliding mode
        bool isFillingUp = TotalSamplesProcessed < WindowSamples;
        int oldestIndex = m_bufferWriteIndex;

        // Store the new sample in the buffer
        for (int ch = 0; ch < NumChannels; ch++)
        {
            double oldSample = m_sampleBuffers[ch][oldestIndex];
            double newSample = samples[ch];
            m_sampleBuffers[ch][oldestIndex] = newSample;

            if (isFillingUp)
            {
                // During fill-up, add contribution of new sample to DFT at correct phase
                // DFT coefficient for position m_totalSampleCount in window
                int dftIndex = (int)(TotalSamplesProcessed % WindowSamples);
                m_phasorReal[ch] += newSample * m_cosTable[dftIndex];
                m_phasorImag[ch] -= newSample * m_sinTable[dftIndex];
            }
            else
            {
                // Sliding DFT update: X_new = (X_old - x_oldest + x_new) * e^(j*2π/N)
                // First, remove the oldest sample's contribution and add the new sample
                double deltaReal = m_phasorReal[ch] + (newSample - oldSample);
                double deltaImag = m_phasorImag[ch];

                // Then rotate by the twiddle factor
                // Complex multiplication: (a + bi)(c + di) = (ac - bd) + (ad + bc)i
                m_phasorReal[ch] = deltaReal * m_twiddleReal - deltaImag * m_twiddleImag;
                m_phasorImag[ch] = deltaReal * m_twiddleImag + deltaImag * m_twiddleReal;
            }
        }

        // Advance buffer write index
        m_bufferWriteIndex = (m_bufferWriteIndex + 1) % WindowSamples;
        TotalSamplesProcessed++;

        // Periodic full recalculation for numerical stability (only after fill-up)
        // Also recalculate immediately after fill-up to ensure consistency
        bool justFilled = TotalSamplesProcessed == WindowSamples;

        if (!isFillingUp && (justFilled || TotalSamplesProcessed % m_recalculationInterval == 0))
            RecalculateFullDft();

        // Check if window has filled
        if (TotalSamplesProcessed < WindowSamples)
        {
            estimate = null;
            return false;
        }

        // Compute output estimate
        estimate = ComputeEstimate(epochNanoseconds);
        return true;
    }

    /// <summary>
    /// Resets the estimator to its initial state.
    /// </summary>
    public void Reset()
    {
        // Clear sample buffers
        for (int ch = 0; ch < NumChannels; ch++)
            Array.Clear(m_sampleBuffers[ch], 0, WindowSamples);

        // Reset indices and counters
        m_bufferWriteIndex = 0;
        TotalSamplesProcessed = 0;

        // Reset phasor accumulators
        Array.Clear(m_phasorReal, 0, NumChannels);
        Array.Clear(m_phasorImag, 0, NumChannels);

        // Reset frequency tracking state
        m_prevPhaseAngle = 0;
        m_hasPrevPhase = false;
        m_smoothedFrequency = NominalFrequencyHz;
        m_prevSmoothedFrequency = NominalFrequencyHz;
        m_smoothedRocof = 0;
        m_frequencyInitialized = false;
        m_rocofInitialized = false;
        m_prevEpochNs = 0;
    }

    // Performs a full DFT recalculation from the sample buffers for numerical stability.
    private void RecalculateFullDft()
    {
        // The buffer write index points to the oldest sample (next to be overwritten)
        // So we read starting from that index
        int startIndex = m_bufferWriteIndex;

        for (int ch = 0; ch < NumChannels; ch++)
        {
            double sumReal = 0;
            double sumImag = 0;

            for (int i = 0; i < WindowSamples; i++)
            {
                int sampleIndex = (startIndex + i) % WindowSamples;
                double sample = m_sampleBuffers[ch][sampleIndex];

                // DFT at bin k = m_targetCycles: X_k = Σ x[n] * e^(-j*2π*k*n/N)
                // Using pre-computed tables which already incorporate the bin number
                sumReal += sample * m_cosTable[i];
                sumImag -= sample * m_sinTable[i];
            }

            m_phasorReal[ch] = sumReal;
            m_phasorImag[ch] = sumImag;
        }
    }

    // Computes the phase estimate from current phasor values.
    private PhaseEstimate ComputeEstimate(long epochNanoseconds)
    {
        Angle[] angles = new Angle[NumChannels];
        double[] magnitudes = new double[NumChannels];

        // Scaling factor for RMS magnitude from DFT
        // For a pure sinusoid: DFT magnitude = N * A / 2, where A is peak amplitude
        // RMS = A / sqrt(2), so RMS = (2 * |X|) / (N * sqrt(2)) = sqrt(2) * |X| / N
        double magnitudeScale = Math.Sqrt(2.0) / WindowSamples;

        // Reference angle for phase normalization (typically VA)
        double referenceAngle = Math.Atan2(m_phasorImag[ReferenceChannel], m_phasorReal[ReferenceChannel]);

        for (int ch = 0; ch < NumChannels; ch++)
        {
            double real = m_phasorReal[ch];
            double imag = m_phasorImag[ch];

            // Calculate magnitude (RMS)
            double magnitude = Math.Sqrt(real * real + imag * imag) * magnitudeScale;
            magnitudes[ch] = magnitude;

            // Calculate angle relative to reference (VA)
            double rawAngle = Math.Atan2(imag, real);
            double relativeAngle = NormalizeAngle(rawAngle - referenceAngle);
            angles[ch] = new Angle(relativeAngle);
        }

        // Calculate time delta since last sample
        double deltaTimeSeconds = m_samplePeriodSeconds; // Default to one sample period
        if (m_prevEpochNs > 0)
        {
            double measuredDelta = (epochNanoseconds - m_prevEpochNs) / NanosecondsPerSecond;
            
            if (measuredDelta is > 0 and < 1.0) // Sanity check
                deltaTimeSeconds = measuredDelta;
        }

        // Calculate frequency from reference channel phase progression
        // The sliding DFT output phase changes by 2π*k/N per sample for a signal at bin k
        // For our nominal frequency, this corresponds to 2π*f_nom*dt
        double currentPhase = Math.Atan2(m_phasorImag[ReferenceChannel], m_phasorReal[ReferenceChannel]);
        double instantaneousFrequency = NominalFrequencyHz;

        if (m_hasPrevPhase)
        {
            // Phase change (unwrapped)
            double deltaPhase = NormalizeAngle(currentPhase - m_prevPhaseAngle);

            // Expected phase change for nominal frequency
            double expectedDeltaPhase = TwoPi * NominalFrequencyHz * deltaTimeSeconds;

            // Frequency deviation
            double phaseDeviation = deltaPhase - expectedDeltaPhase;
            double frequencyDeviation = phaseDeviation / (TwoPi * deltaTimeSeconds);
            instantaneousFrequency = NominalFrequencyHz + frequencyDeviation;

            // Clamp to reasonable range (±10% of nominal)
            double minFreq = NominalFrequencyHz * 0.9;
            double maxFreq = NominalFrequencyHz * 1.1;
            instantaneousFrequency = Math.Max(minFreq, Math.Min(maxFreq, instantaneousFrequency));
        }

        // Update previous phase for next iteration
        m_prevPhaseAngle = currentPhase;
        m_hasPrevPhase = true;

        // Apply exponential smoothing to frequency
        if (!m_frequencyInitialized)
        {
            m_smoothedFrequency = instantaneousFrequency;
            m_frequencyInitialized = true;
        }
        else
        {
            m_smoothedFrequency = m_frequencyAlpha * instantaneousFrequency +
                                  (1.0 - m_frequencyAlpha) * m_smoothedFrequency;
        }

        // Calculate ROCOF (rate of change of frequency)
        double rocof = 0;

        if (m_rocofInitialized)
        {
            // ROCOF from smoothed frequency change
            double instantaneousRocof = (m_smoothedFrequency - m_prevSmoothedFrequency) / deltaTimeSeconds;

            // Smooth ROCOF
            m_smoothedRocof = m_rocofAlpha * instantaneousRocof + (1.0 - m_rocofAlpha) * m_smoothedRocof;
            rocof = m_smoothedRocof;
        }
        else
        {
            m_rocofInitialized = true;
        }

        m_prevSmoothedFrequency = m_smoothedFrequency;
        m_prevEpochNs = epochNanoseconds;

        return new PhaseEstimate
        {
            Frequency = m_smoothedFrequency,
            dFdt = rocof,
            Angles = angles,
            Magnitudes = magnitudes
        };
    }

    /// <summary>
    /// Normalizes an angle to the range [-π, π].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double NormalizeAngle(double angle)
    {
        while (angle > Math.PI)
            angle -= TwoPi;
        while (angle < -Math.PI)
            angle += TwoPi;
        return angle;
    }

    #endregion
}
