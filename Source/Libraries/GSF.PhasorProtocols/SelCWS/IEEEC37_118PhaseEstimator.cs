//******************************************************************************************************
//  IEEEC37_118PhaseEstimator.cs - Gbtc
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
//  03/19/2026 - Ritchie Carroll
//       Generated original version of source code in collaboration with ChatGPT implementing the
//       IEEE C37.118-2018 Annex D filter-based phasor estimation algorithm.
//  05/23/2026 - Ritchie Carroll
//       Ported into GSF as a selectable SEL CWS phase-estimation algorithm. Standardized on the
//       VA, VB, VC, IA, IB, IC channel order used by the sliding DFT estimator (which also corrects
//       the voltage positive-sequence channel indexing for frequency/ROCOF estimation).
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
/// Represents a rolling six-phase estimator using the IEEE C37.118-2018 Annex D filter-based
/// phasor estimation algorithm with P-class and M-class support.
/// </summary>
/// <remarks>
/// <para>
/// This class implements the IEEE C37.118-2018 Annex D standard PMU phasor estimation algorithm.
/// It uses precomputed FIR filter coefficients (P-class triangular window or M-class Hamming-windowed
/// sinc) to estimate fundamental-frequency phasors from streaming samples via causal convolution.
/// </para>
/// <para>
/// Frequency estimation is performed per IEEE Annex D.4, Equations D.3 and D.4, using
/// phase-differencing on the voltage positive-sequence phasor. ROCOF is computed as the
/// second difference of the unwrapped positive-sequence phase angle.
/// </para>
/// <para>
/// The estimator operates at the full input sample rate and decimates to the configured report rate
/// by selecting every <c>fs/RR</c>-th sample (dyadic decimation), matching the standard's approach.
/// </para>
/// <para>
/// For P-class filters, an additional magnitude correction per Annex D.6, Equation D.6 is applied
/// to compensate for spectral leakage when the signal frequency deviates from nominal.
/// </para>
/// <para>
/// <b>Outputs:</b> 6 phasors (VA, VB, VC, IA, IB, IC) plus frequency and ROCOF estimates.
/// Internally computes voltage positive-sequence for IEEE Annex D.4 frequency/ROCOF estimation.
/// </para>
/// </remarks>
internal sealed class IEEEC37_118PhaseEstimator : IPhaseEstimator
{
    #region [ Members ]

    // Constants

    // Number of input channels (VA, VB, VC, IA, IB, IC).
    private const int NumInputChannels = 6;
    private const double TwoPI = 2.0 * Math.PI;

    /// <summary>
    /// Default filter class.
    /// </summary>
    public const FilterClass DefaultFilterClass = FilterClass.P;

    // Fields

    // IEEE Annex D FIR filter coefficients (complex-valued), length = FilterOrder + 1
    private readonly double[] m_filterReal;
    private readonly double[] m_filterImag;
    private readonly int m_filterLength;      // = FilterOrder + 1

    // Circular sample buffers for FIR convolution, one per input channel
    // Each buffer has length = m_filterLength
    private readonly double[][] m_sampleBuffers;
    private int m_bufferWriteIndex;

    // Decimation: publish every m_decimationStep-th sample
    private readonly int m_decimationStep;
    private int m_decimationCounter;

    // Group delay in samples (N/2 where N = filter order)
    private readonly int m_groupDelay;

    // Per-sample complex phasor output (real/imag) for each input channel
    private readonly double[] m_phasorReal = new double[NumInputChannels];
    private readonly double[] m_phasorImag = new double[NumInputChannels];

    // Voltage positive-sequence phasor (used internally for frequency/ROCOF estimation)
    private double m_vpReal, m_vpImag;

    // Frequency/ROCOF estimation state (IEEE Annex D.4)
    // Uses a 3-sample history of unwrapped voltage positive-sequence phase
    private readonly double[] m_phaseHistory = new double[3]; // [n-2, n-1, n]
    private int m_phaseHistoryCount;
    private double m_lastUnwrappedPhase;

    // Publish output buffers
    private readonly Angle[] m_publishAngles = new Angle[NumInputChannels];
    private readonly double[] m_publishMagnitudes = new double[NumInputChannels];

    // Timing tracking
    private readonly double m_samplePeriodSeconds;
    private double m_currentTimeSeconds; // running time counter = sampleIndex / fs

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="IEEEC37_118PhaseEstimator"/> class implementing
    /// the IEEE C37.118-2018 Annex D phasor estimation algorithm.
    /// </summary>
    /// <param name="sampleRateHz">
    /// Input sample rate in Hz (samples per second). Must be &gt; 0 and an integer multiple of
    /// both <paramref name="nominalFrequency"/> and <paramref name="outputRateHz"/>.
    /// </param>
    /// <param name="outputRateHz">
    /// Output report rate in Hz (estimates per second). Must be &gt; 0 and &lt;= <paramref name="sampleRateHz"/>.
    /// The sample rate must be an integer multiple of this value.
    /// </param>
    /// <param name="nominalFrequency">
    /// Nominal line frequency (50Hz or 60Hz). Determines the filter parameters.
    /// </param>
    /// <param name="filterClass">
    /// IEEE C37.118 filter class: <see cref="FilterClass.P"/> for Protection (fast response)
    /// or <see cref="FilterClass.M"/> for Measurement (better out-of-band rejection).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if rates are non-positive, output rate exceeds input rate, or sample rate is not
    /// an integer multiple of the nominal frequency or report rate.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="nominalFrequency"/> and <paramref name="outputRateHz"/> combination
    /// is not supported for M-class filters per IEEE C37.118-2018 Table D.1.
    /// </exception>
    public IEEEC37_118PhaseEstimator(
        double sampleRateHz,
        double outputRateHz,
        LineFrequency nominalFrequency,
        FilterClass filterClass = DefaultFilterClass)
    {
        // Validate rates
        if (sampleRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRateHz), "Input sample rate must be positive.");

        if (outputRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(outputRateHz), "Output rate must be positive.");

        if (outputRateHz > sampleRateHz)
            throw new ArgumentOutOfRangeException(nameof(outputRateHz), "Output rate must be <= input sample rate.");

        double f0 = (double)nominalFrequency;
        double fs = sampleRateHz;
        double rr = outputRateHz;

        // Validate integer-multiple relationships
        double samplesPerCycle = fs / f0;
        double samplesPerReport = fs / rr;

        if (Math.Abs(samplesPerCycle - Math.Round(samplesPerCycle)) > 1e-9)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRateHz), sampleRateHz,
                $"Sample rate {sampleRateHz:N0} Hz must be an integer multiple of the nominal frequency {f0:N0} Hz.");
        }

        if (Math.Abs(samplesPerReport - Math.Round(samplesPerReport)) > 1e-9)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sampleRateHz), sampleRateHz,
                $"Sample rate {sampleRateHz:N0} Hz must be an integer multiple of the report rate {outputRateHz:N0} Hz.");
        }

        // Store configuration
        SampleRateHz = sampleRateHz;
        OutputRateHz = outputRateHz;
        NominalFrequencyHz = f0;
        Class = filterClass;

        m_samplePeriodSeconds = 1.0 / fs;
        m_decimationStep = (int)Math.Round(samplesPerReport);
        m_decimationCounter = 0;

        // Build IEEE Annex D filter coefficients
        BuildFilter(fs, f0, rr, filterClass, out m_filterReal, out m_filterImag, out int filterOrder);
        m_filterLength = filterOrder + 1;
        m_groupDelay = filterOrder / 2;

        // The window of samples needed before valid output = filter length
        // (filter startup transient, first FilterOrder samples produce NaN per IEEE)
        WindowSamples = m_filterLength;

        // Initialize circular sample buffers (one per input channel)
        m_sampleBuffers = new double[NumInputChannels][];

        for (int ch = 0; ch < NumInputChannels; ch++)
            m_sampleBuffers[ch] = new double[m_filterLength];
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the configured sample rate in Hz.
    /// </summary>
    public double SampleRateHz { get; }

    /// <summary>
    /// Gets the configured output report rate in Hz.
    /// </summary>
    public double OutputRateHz { get; }

    /// <summary>
    /// Gets the nominal frequency in Hz.
    /// </summary>
    public double NominalFrequencyHz { get; }

    /// <summary>
    /// Gets the IEEE C37.118 filter class (P or M).
    /// </summary>
    public FilterClass Class { get; }

    /// <summary>
    /// Gets the number of samples required before valid output is produced (filter startup transient).
    /// </summary>
    public int WindowSamples { get; }

    /// <summary>
    /// Gets the total number of samples processed.
    /// </summary>
    public long TotalSamplesProcessed { get; private set; }

    /// <summary>
    /// Gets whether the estimator has filled its filter window and is producing valid estimates.
    /// </summary>
    public bool IsReady => TotalSamplesProcessed >= WindowSamples;

    /// <summary>
    /// Gets the filter order (N), the number of FIR filter taps minus one.
    /// </summary>
    public int FilterOrder => m_filterLength - 1;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Push one interleaved sample-group (VA, VB, VC, IA, IB, IC) with its epoch nanoseconds.
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
    /// <c>true</c> when an estimate is available (filter has filled) and it is time to
    /// publish at target <see cref="OutputRateHz"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The data referenced in the span-based properties of the <see cref="PhaseEstimate"/>
    /// parameter provided to the <see cref="PhaseEstimateHandler"/> delegate is owned by the
    /// <see cref="IEEEC37_118PhaseEstimator"/> instance, it is only valid during the scope of the
    /// delegate call. If you need to retain the data, make a copy.
    /// </remarks>
    public bool Step(
        double va,
        double vb,
        double vc,
        double ia,
        double ib,
        double ic,
        long epochNanoseconds,
        PhaseEstimateHandler phaseEstimateHandler)
    {
        if (phaseEstimateHandler is null)
            throw new ArgumentNullException(nameof(phaseEstimateHandler));

        Span<double> samples = stackalloc double[NumInputChannels] { va, vb, vc, ia, ib, ic };

        // Push new samples into circular buffers
        for (int ch = 0; ch < NumInputChannels; ch++)
            m_sampleBuffers[ch][m_bufferWriteIndex] = samples[ch];

        m_bufferWriteIndex = (m_bufferWriteIndex + 1) % m_filterLength;
        TotalSamplesProcessed++;
        m_currentTimeSeconds = (TotalSamplesProcessed - 1) / SampleRateHz;

        // Not enough samples to produce valid output yet (filter startup transient)
        if (TotalSamplesProcessed < WindowSamples)
            return false;

        // Compute FIR convolution for each input channel → complex phasor
        ComputePhasors();

        // Apply group delay compensation by adjusting the effective time
        double effectiveTime = m_currentTimeSeconds - m_groupDelay * m_samplePeriodSeconds;

        // Extract magnitude and angle per IEEE Annex D.2/D.3
        ExtractMagnitudesAndAngles(effectiveTime);

        // Compute voltage positive-sequence phasor (needed for frequency/ROCOF estimation)
        ComputeVoltagePositiveSequence();

        // Compute frequency and ROCOF from voltage positive-sequence phase (IEEE Annex D.4)
        ComputeFrequencyAndRocof(out double frequency, out double rocof);

        // P-class magnitude correction (IEEE Annex D.6, Equation D.6)
        if (Class == FilterClass.P)
            ApplyPClassMagnitudeCorrection(frequency);

        // Decimation: only publish at report-rate boundaries
        m_decimationCounter++;

        if (m_decimationCounter < m_decimationStep)
            return false;

        m_decimationCounter = 0;

        // Publish the estimate
        phaseEstimateHandler(new PhaseEstimate
        {
            Frequency = frequency,
            dFdt = rocof,
            Angles = m_publishAngles,
            Magnitudes = m_publishMagnitudes
        });

        return true;
    }

    /// <summary>
    /// Resets the estimator to its initial state.
    /// </summary>
    public void Reset()
    {
        // Clear sample buffers
        for (int ch = 0; ch < NumInputChannels; ch++)
            Array.Clear(m_sampleBuffers[ch], 0, m_filterLength);

        m_bufferWriteIndex = 0;
        TotalSamplesProcessed = 0L;
        m_decimationCounter = 0;
        m_currentTimeSeconds = 0.0D;

        // Reset phasor outputs
        Array.Clear(m_phasorReal, 0, NumInputChannels);
        Array.Clear(m_phasorImag, 0, NumInputChannels);
        m_vpReal = m_vpImag = 0.0D;

        // Reset frequency/ROCOF state
        Array.Clear(m_phaseHistory, 0, 3);
        m_phaseHistoryCount = 0;
        m_lastUnwrappedPhase = 0.0D;

        // Reset output buffers
        Array.Clear(m_publishAngles, 0, NumInputChannels);
        Array.Clear(m_publishMagnitudes, 0, NumInputChannels);
    }

    /// <summary>
    /// Computes FIR convolution for each input channel, producing complex phasor outputs.
    /// </summary>
    /// <remarks>
    /// Implements IEEE C37.118-2018 Annex D.2, Equation D.1: X = Σ filter[k] * x[n-k]
    /// The filter coefficients are pre-reversed (flipped) during construction so we can
    /// simply dot-product with the buffer in forward order.
    /// </remarks>
    private void ComputePhasors()
    {
        for (int ch = 0; ch < NumInputChannels; ch++)
        {
            double sumReal = 0.0;
            double sumImag = 0.0;
            double[] buffer = m_sampleBuffers[ch];

            // The buffer write index points to where the NEXT sample will go,
            // so the oldest sample is at m_bufferWriteIndex.
            // Filter coefficients were pre-reversed during construction.
            for (int k = 0; k < m_filterLength; k++)
            {
                int index = (m_bufferWriteIndex + k) % m_filterLength;
                double sample = buffer[index];

                // Complex multiply: (filterReal + j*filterImag) * sample
                sumReal += m_filterReal[k] * sample;
                sumImag += m_filterImag[k] * sample;
            }

            m_phasorReal[ch] = sumReal;
            m_phasorImag[ch] = sumImag;
        }
    }

    /// <summary>
    /// Extracts magnitude and angle from complex phasors per IEEE Annex D.2/D.3.
    /// </summary>
    /// <param name="effectiveTime">Time in seconds with group delay compensation applied.</param>
    private void ExtractMagnitudesAndAngles(double effectiveTime)
    {
        double w0 = TwoPI * NominalFrequencyHz;

        for (int ch = 0; ch < NumInputChannels; ch++)
        {
            double real = m_phasorReal[ch];
            double imag = m_phasorImag[ch];

            // Magnitude = |X|
            double magnitude = Math.Sqrt(real * real + imag * imag);

            // Angle = wrapToPi(angle(X) - t * w0) per IEEE __estimate
            double rawAngle = Math.Atan2(imag, real);
            Angle angle = NormalizeAngle(rawAngle - effectiveTime * w0);

            m_publishMagnitudes[ch] = magnitude;
            m_publishAngles[ch] = angle;
        }
    }

    /// <summary>
    /// Computes the voltage positive-sequence phasor for frequency/ROCOF estimation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Per IEEE C37.118-2018: Xp = (1/3)(Xa + α·Xb + α²·Xc)
    /// where α = e^(j·2π/3) = -0.5 + j·√3/2
    /// </para>
    /// <para>
    /// Uses the derotated phasors (with nominal frequency rotation removed), matching the Python
    /// reference implementation where <c>__estimate</c> removes the <c>t*w0</c> rotation before
    /// positive-sequence computation. Using the raw FIR output (which still rotates at w0) would
    /// cause the frequency formula <c>F = dφ/dt/(4π·dt) + f0</c> to double-count f0.
    /// </para>
    /// </remarks>
    private void ComputeVoltagePositiveSequence()
    {
        // α = -0.5 + j·√3/2
        const double alphaReal = -0.5D;
        double alphaImag = Math.Sqrt(3.0D) / 2.0D;

        // α² = -0.5 - j·√3/2
        const double alpha2Real = -0.5D;
        double alpha2Imag = -Math.Sqrt(3.0D) / 2.0D;

        // Voltage positive sequence: Vp = (1/3)(Va + α·Vb + α²·Vc)
        const int VA = (int)PhaseChannel.VA;
        const int VB = (int)PhaseChannel.VB;
        const int VC = (int)PhaseChannel.VC;

        // Reconstruct derotated complex phasors from published magnitudes and angles
        // (which have the nominal frequency rotation already removed in ExtractMagnitudesAndAngles).
        // This matches the Python __estimate output: XM*cos(XA) + j*XM*sin(XA)
        double vaAngle = (double)m_publishAngles[VA];
        double vaReal = m_publishMagnitudes[VA] * Math.Cos(vaAngle);
        double vaImag = m_publishMagnitudes[VA] * Math.Sin(vaAngle);

        double vbAngle = (double)m_publishAngles[VB];
        double vbReal = m_publishMagnitudes[VB] * Math.Cos(vbAngle);
        double vbImag = m_publishMagnitudes[VB] * Math.Sin(vbAngle);

        double vcAngle = (double)m_publishAngles[VC];
        double vcReal = m_publishMagnitudes[VC] * Math.Cos(vcAngle);
        double vcImag = m_publishMagnitudes[VC] * Math.Sin(vcAngle);

        // α·Vb (complex multiply)
        double aVbReal = alphaReal * vbReal - alphaImag * vbImag;
        double aVbImag = alphaReal * vbImag + alphaImag * vbReal;

        // α²·Vc (complex multiply)
        double a2VcReal = alpha2Real * vcReal - alpha2Imag * vcImag;
        double a2VcImag = alpha2Real * vcImag + alpha2Imag * vcReal;

        m_vpReal = (vaReal + aVbReal + a2VcReal) / 3.0;
        m_vpImag = (vaImag + aVbImag + a2VcImag) / 3.0;
    }

    /// <summary>
    /// Computes frequency and ROCOF from the voltage positive-sequence phasor phase
    /// per IEEE C37.118-2018 Annex D.4, Equations D.3 and D.4.
    /// </summary>
    /// <param name="frequency">Output frequency estimate in Hz.</param>
    /// <param name="rocof">Output ROCOF estimate in Hz/s.</param>
    private void ComputeFrequencyAndRocof(out double frequency, out double rocof)
    {
        double currentPhase = Math.Atan2(m_vpImag, m_vpReal);

        // Unwrap phase relative to last unwrapped value
        if (m_phaseHistoryCount > 0)
        {
            double wrapped = NormalizeAngle(currentPhase - m_lastUnwrappedPhase);
            currentPhase = m_lastUnwrappedPhase + wrapped;
        }

        m_lastUnwrappedPhase = currentPhase;

        // Shift phase history: [n-2] = [n-1], [n-1] = [n], [n] = currentPhase
        m_phaseHistory[0] = m_phaseHistory[1];
        m_phaseHistory[1] = m_phaseHistory[2];
        m_phaseHistory[2] = currentPhase;
        m_phaseHistoryCount = Math.Min(m_phaseHistoryCount + 1, 3);

        double dt = m_samplePeriodSeconds;

        switch (m_phaseHistoryCount)
        {
            // IEEE Annex D.4, Equation D.3:
            // F = lfilter([1, 0, -1], 1, unwrap(angle(Xp))) / (4*pi*1/fs) + f0
            // Equivalent to: F[n] = (phase[n] - phase[n-2]) / (4*pi * dt) + f0
            case >= 3:
                frequency = (m_phaseHistory[2] - m_phaseHistory[0]) / (4.0D * Math.PI * dt) + NominalFrequencyHz;

                // IEEE Annex D.4, Equation D.4:
                // DF = lfilter([1, -2, 1], 1, unwrap(angle(Xp))) / (2*pi / fs^2)
                // Equivalent to: DF[n] = (phase[n] - 2*phase[n-1] + phase[n-2]) / (2*pi * dt^2)
                rocof = (m_phaseHistory[2] - 2.0D * m_phaseHistory[1] + m_phaseHistory[0]) / (TwoPI * dt * dt);
                break;
            case 2:
                // Partial: can compute a first-order frequency estimate
                frequency = (m_phaseHistory[2] - m_phaseHistory[1]) / (TwoPI * dt) + NominalFrequencyHz;
                rocof = 0.0;
                break;
            default:
                frequency = NominalFrequencyHz;
                rocof = 0.0;
                break;
        }
    }

    /// <summary>
    /// Applies P-class magnitude correction per IEEE C37.118-2018 Annex D.6, Equation D.6.
    /// </summary>
    /// <param name="frequency">Current frequency estimate in Hz.</param>
    private void ApplyPClassMagnitudeCorrection(double frequency)
    {
        // XM_corrected = XM / sin(π * (f0 + 1.625*(f0-F)) / (2*f0))
        double f0 = NominalFrequencyHz;
        double correctionArg = Math.PI * (f0 + 1.625D * (f0 - frequency)) / (2.0D * f0);
        double sinVal = Math.Sin(correctionArg);

        // Avoid division by zero or near-zero
        if (Math.Abs(sinVal) < 1e-10D)
            return;

        double correctionFactor = 1.0 / sinVal;

        // Apply correction to all output phasors
        for (int i = 0; i < NumInputChannels; i++)
            m_publishMagnitudes[i] *= correctionFactor;
    }

    /// <summary>
    /// Normalizes angle by wrapping to the range [-π, π].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Angle NormalizeAngle(double angle)
    {
        return new Angle(angle).ToRange(-Math.PI, false);
    }

    #endregion

    #region [ Static ]

    /// <summary>
    /// Builds IEEE C37.118-2018 Annex D FIR filter coefficients.
    /// </summary>
    /// <param name="fs">Sample rate in Hz.</param>
    /// <param name="f0">Nominal frequency in Hz.</param>
    /// <param name="rr">Report rate in Hz.</param>
    /// <param name="filterClass">Filter class (P or M).</param>
    /// <param name="filterReal">Output: real parts of filter coefficients (pre-reversed for convolution).</param>
    /// <param name="filterImag">Output: imaginary parts of filter coefficients (pre-reversed for convolution).</param>
    /// <param name="filterOrder">Output: filter order N (number of taps = N+1).</param>
    private static void BuildFilter(
        double fs, double f0, double rr, FilterClass filterClass,
        out double[] filterReal, out double[] filterImag, out int filterOrder)
    {
        double w0 = TwoPI * f0;
        int N;
        double[] w; // Window function values

        if (filterClass == FilterClass.P)
        {
            // IEEE Annex D.6, Equation D.5: Triangular window
            N = (int)((fs / f0 - 1) * 2);
            w = new double[N + 1];

            for (int i = 0; i <= N; i++)
            {
                double k = i - N / 2.0D;
                w[i] = 1.0D - 2.0D / (N + 2) * Math.Abs(k);
            }
        }
        else // M-class
        {
            // IEEE Annex D.7, Table D.1: lookup (Ffr, N) for valid (f0, RR) combinations
            (double Ffr, N) = GetMClassParameters(f0, rr);

            // IEEE Annex D.7, Equation D.7: Hamming-windowed sinc
            w = new double[N + 1];
            double[] hamming = BuildHammingWindow(N + 1);

            for (int i = 0; i <= N; i++)
            {
                double k = i - N / 2.0D;
                double sincArg = TwoPI * (2.0D * Ffr) / fs * k;

                double sincVal;

                if (Math.Abs(sincArg) < 1e-15D)
                    sincVal = 1.0D;
                else
                    sincVal = Math.Sin(sincArg) / sincArg;

                w[i] = sincVal * hamming[i];
            }
        }

        filterOrder = N;

        // Compute filter normalization gain: G = sum(w)
        double G = 0.0D;

        for (int i = 0; i <= N; i++)
            G += w[i];

        // Build DFT kernel: E[k] = exp(-j * k / fs * w0) for k from -N/2 to N/2
        // Then flip it: filter = sqrt(2)/G * exp(j*N/2/fs*w0) * w * flip(E)
        // Combined: filter[i] = sqrt(2)/G * exp(j*N/2/fs*w0) * w[i] * exp(j*(i-N/2)/fs*w0)
        //         = sqrt(2)/G * w[i] * exp(j*((N/2) + (i-N/2))/fs*w0)
        //         = sqrt(2)/G * w[i] * exp(j*i/fs*w0)

        // Phase offset: exp(j*N/2/fs*w0)
        // flip(E[k]) where E[k] = exp(-j*(k-N/2)/fs*w0), flipped: E_flip[i] = exp(-j*(N/2-i)/fs*w0) = exp(j*(i-N/2)/fs*w0)
        // So: filter[i] = sqrt(2)/G * exp(j*N/2/fs*w0) * w[i] * exp(j*(i-N/2)/fs*w0)
        //               = sqrt(2)/G * w[i] * exp(j*i/fs*w0)

        double scale = Math.Sqrt(2.0D) / G;
        filterReal = new double[N + 1];
        filterImag = new double[N + 1];

        // Build filter coefficients (already in convolution order matching lfilter behavior)
        for (int i = 0; i <= N; i++)
        {
            double phase = i / fs * w0;
            double coeff = scale * w[i];
            filterReal[i] = coeff * Math.Cos(phase);
            filterImag[i] = coeff * Math.Sin(phase);
        }

        // Reverse the filter for dot-product convolution (so we can index forward through the circular buffer)
        Array.Reverse(filterReal);
        Array.Reverse(filterImag);
    }

    /// <summary>
    /// Gets M-class filter parameters (Ffr, N) from IEEE C37.118-2018 Table D.1.
    /// </summary>
    private static (double Ffr, int N) GetMClassParameters(double f0, double rr)
    {
        int f0Int = (int)Math.Round(f0);
        int rrInt = (int)Math.Round(rr);

        return f0Int switch
        {
            (int)LineFrequency.Hz50 => rrInt switch
            {
                10 => (1.779D, 806),
                25 => (4.355D, 338),
                50 => (7.75D, 142),
                100 => (14.1D, 66),
                _ => throw new ArgumentException($"M-class filter at 50Hz nominal only supports report rates: 10, 25, 50, 100. Got: {rr}", nameof(rr))
            },
            (int)LineFrequency.Hz60 => rrInt switch
            {
                10 => (1.78D, 968),
                12 => (2.125D, 816),
                15 => (2.64D, 662),
                20 => (3.5D, 502),
                30 => (5.02D, 306),
                60 => (8.19D, 164),
                120 => (16.25D, 70),
                _ => throw new ArgumentException($"M-class filter at 60Hz nominal only supports report rates: 10, 12, 15, 20, 30, 60, 120. Got: {rr}", nameof(rr))
            },
            _ => throw new ArgumentException($"M-class filter only supports nominal frequencies of 50 or 60 Hz. Got: {f0}", nameof(f0))
        };
    }

    /// <summary>
    /// Builds a Hamming window of the specified length.
    /// </summary>
    /// <param name="length">Window length.</param>
    /// <returns>Hamming window coefficients.</returns>
    private static double[] BuildHammingWindow(int length)
    {
        double[] window = new double[length];

        for (int i = 0; i < length; i++)
            window[i] = 0.54D - 0.46D * Math.Cos(TwoPI * i / (length - 1));

        return window;
    }

    #endregion
}
