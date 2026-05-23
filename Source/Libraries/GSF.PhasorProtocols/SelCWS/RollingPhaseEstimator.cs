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
//       Generated original version of source code in collaboration with ChatGPT.
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
public readonly ref struct PhaseEstimate
{
    /// <summary>
    /// Gets smoothed frequency estimate in hertz.
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
    /// The data this span references is owned by the <see cref="RollingPhaseEstimator"/> instance,
    /// it is only valid during the scope of the <see cref="PhaseEstimateHandler"/> delegate call.
    /// If you need to retain the data, make a copy.
    /// </remarks>
    public required ReadOnlySpan<Angle> Angles { get; init; }

    /// <summary>
    /// Gets RMS magnitudes, length 6: VA, VB, VC, IA, IB, IC.
    /// </summary>
    /// <remarks>
    /// The data this span references is owned by the <see cref="RollingPhaseEstimator"/> instance,
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

/// <summary>
/// Represents a rolling six-phase estimator using a sliding DFT algorithm with optional smoothing.
/// </summary>
/// <remarks>
/// <para>
/// This class implements a real-time phasor measurement algorithm suitable for power system
/// applications. It uses a Sliding Discrete Fourier Transform (SDFT) approach to efficiently
/// compute phasors at each sample with O(1) complexity.
/// </para>
/// <para>
/// Frequency estimation is performed by tracking the rate of change of phase angle from the
/// reference channel (VA by default). The frequency and ROCOF outputs are smoothed using
/// exponential moving average filters.
/// </para>
/// <para>
/// The algorithm is designed for nominal 50Hz or 60Hz systems sampled at 3000Hz, but can be
/// configured for other sample rates.
/// </para>
/// </remarks>
internal sealed class RollingPhaseEstimator
{
    #region [ Members ]

    // Nested Types
    private struct SamplePhaseEstimate
    {
        public double Frequency;
        public double dFdt;
        public Angle[] Angles;
        public double[] Magnitudes;
    }

    // Constants
    private const int NumChannels = 6;
    private const double TwoPI = 2.0D * Math.PI;
    private const double Sqrt2 = 1.4142135623730951D;
    private const long NanosecondsPerSecond = 1_000_000_000L;

    /// <summary>
    /// Default value for reference channel.
    /// </summary>
    public const PhaseChannel DefaultReferenceChannel = PhaseChannel.VA;
    
    /// <summary>
    /// Default value for number of target cycles.
    /// </summary>
    public const int DefaultTargetCycles = 2;

    /// <summary>
    /// Default value for determining if interval averaging is enabled.
    /// </summary>
    public const bool DefaultEnableIntervalAveraging = true;

    /// <summary>
    /// Default value for determining if EMA publishing is enabled.
    /// </summary>
    public const bool DefaultEnablePublishEMA = true;

    /// <summary>
    /// Default value for EMA time constant τ (seconds) published angles.
    /// </summary>
    public const double DefaultPublishAnglesTauSeconds = 0.35D;

    /// <summary>
    /// Default value for EMA time constant τ (seconds) published RMS magnitudes.
    /// </summary>
    public const double DefaultPublishMagnitudesTauSeconds = 0.50D;

    /// <summary>
    /// Default value for EMA time constant τ (seconds) published frequency.
    /// </summary>
    public const double DefaultPublishFrequencyTauSeconds = 0.75D;

    /// <summary>
    /// Default value for EMA time constant τ (seconds) published ROCOF.
    /// </summary>
    public const double DefaultPublishRocofTauSeconds = 1.50D;

    /// <summary>
    /// Default value for EMA time constant τ (seconds) published for the internal per-sample frequency smoothing.
    /// </summary>
    public const double DefaultSampleFrequencyTauSeconds = 0.15D;

    /// <summary>
    /// Default value for EMA time constant τ (seconds) published for the internal per-sample ROCOF smoothing.
    /// </summary>
    public const double DefaultSampleRocofTauSeconds = 0.30D;

    /// <summary>
    /// Default value for number of recalculation cycles.
    /// </summary>
    public const int DefaultRecalculationCycles = 10;

    /// <summary>
    /// Default maximum gap (in samples) filled by phase-continued synthesis before resynchronizing.
    /// A negative value means "auto" (one full analysis window).
    /// </summary>
    public const int DefaultMaxGapFillSamples = -1;

    // Fields
    private readonly double m_samplePeriodSeconds;
    private readonly int m_recalculationInterval;
    private readonly int m_initialSamplesUntilRecalc;  // countdown to the first recalc after the window fills
    private int m_samplesUntilRecalc;                  // samples remaining until the next full DFT recalc

    // Output decimation / publish gating
    private readonly long m_publishPeriodNs;    // 0 means "publish every sample"
    private long m_nextPublishEpochNs;          // first publish scheduled when window becomes ready
    private bool m_publishScheduleInitialized;  // guards first-time publish-schedule initialization

    // If enabled, we boxcar-average (anti-alias) across each publish interval.
    private readonly bool m_enableIntervalAveraging;
    private double m_lastFrequency;
    private double m_lastRocof;
    private bool m_hasLastSampleEstimate;

    // Optional EMA applied to the *published* stream (after interval averaging).
    private readonly bool m_enablePublishEMA;
    private readonly double m_publishAnglesAlpha;
    private readonly double m_publishMagnitudesAlpha;
    private readonly double m_publishFrequencyAlpha;
    private readonly double m_publishRocofAlpha;

    // Precomputed (1 - α) complements for the published-stream EMA
    private readonly double m_oneMinusPublishAnglesAlpha;
    private readonly double m_oneMinusPublishMagnitudesAlpha;
    private readonly double m_oneMinusPublishFrequencyAlpha;
    private readonly double m_oneMinusPublishRocofAlpha;

    // Circular sample buffers for each channel
    private readonly double[][] m_sampleBuffers;
    private int m_bufferWriteIndex;

    // Reusable per-sample working buffers (avoids allocating Angle[] / double[] every sample)
    private readonly Angle[] m_workingAngles = new Angle[NumChannels];
    private readonly double[] m_workingMagnitudes = new double[NumChannels];

    // Dedicated storage for "sample-and-hold" mode (only used when interval averaging is disabled)
    private readonly Angle[] m_lastAngles = new Angle[NumChannels];
    private readonly double[] m_lastMagnitudes = new double[NumChannels];

    // Publish output buffers (single set for all modes suffices)
    private readonly Angle[] m_publishAngles = new Angle[NumChannels];
    private readonly double[] m_publishMagnitudes = new double[NumChannels];

    // DFT twiddle factor for sliding update at bin k: e^(j*2π*k/N), where k = targetCycles
    private readonly double m_twiddleReal;  // cos(...)
    private readonly double m_twiddleImag;  // sin(...)

    // Accumulated DFT phasors (real and imaginary parts) for each channel
    private readonly double[] m_phasorReal;
    private readonly double[] m_phasorImag;

    // Precomputed cosine/sine tables for full DFT recalculation
    private readonly double[] m_cosTable;
    private readonly double[] m_sinTable;

    // Precomputed RMS magnitude scale factor: sqrt(2) / N
    private readonly double m_magnitudeScale;

    // Precomputed frequency-tracking constants (derived from nominal frequency)
    private readonly double m_minFrequency;     // NominalFrequencyHz * 0.9 (clamp lower bound)
    private readonly double m_maxFrequency;     // NominalFrequencyHz * 1.1 (clamp upper bound)

    // Previous phase angle for frequency calculation (from reference channel)
    private double m_prevPhaseAngle;
    private bool m_hasPrevPhase;

    // Frequency smoothing (exponential moving average) on per-sample instantaneous frequency
    private readonly double m_frequencyAlpha;
    private readonly double m_oneMinusFrequencyAlpha;  // precomputed 1 - α
    private double m_smoothedFrequency;
    private bool m_frequencyInitialized;

    // ROCOF smoothing on per-sample instantaneous ROCOF
    private readonly double m_rocofAlpha;
    private readonly double m_oneMinusRocofAlpha;  // precomputed 1 - α
    private double m_prevSmoothedFrequency;
    private double m_smoothedRocof;
    private bool m_rocofInitialized;

    // Timing
    private long m_prevEpochNs;

    // Gap detection / fill (missing-data handling)
    private readonly double m_samplePeriodNs;   // expected nanoseconds between consecutive input samples
    private readonly int m_maxGapFillSamples;   // max gap (samples) coasted before a resynchronization
    private long m_prevInputEpochNs;            // epoch of the previous Step call (tracked every sample)

    // Interval accumulators (for down-sampling / anti-aliasing)
    private int m_intervalCount;
    private readonly double[] m_intervalAngleCosSum = new double[NumChannels];
    private readonly double[] m_intervalAngleSinSum = new double[NumChannels];
    private readonly double[] m_intervalMagnitudeSum = new double[NumChannels];
    private double m_intervalFreqSum;
    private double m_intervalRocofSum;

    // Published EMA state (wrap-safe for angles)
    private bool m_publishEMAInitialized;
    private readonly double[] m_pubAngleCos = new double[NumChannels];
    private readonly double[] m_pubAngleSin = new double[NumChannels];
    private readonly double[] m_pubMagnitude = new double[NumChannels];
    private double m_pubFrequency;
    private double m_pubRocof;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingPhaseEstimator"/> class with independent
    /// input sample rate and output publish rate, plus smoothing expressed as time constants (τ).
    /// </summary>
    /// <param name="sampleRateHz">
    /// Input sample rate in Hz (PoW samples per second). Must be &gt; 0.
    /// This is the rate at which <see cref="Step"/> is called with new samples.
    /// </param>
    /// <param name="outputRateHz">
    /// Output publish rate in Hz (estimates per second). Must be &gt; 0 and &lt;= <paramref name="sampleRateHz"/>.
    /// Common synchrophasor-friendly values: 30, 60 or 120Hz. Defaults chosen for "calm" 60Hz publish rates;
    /// τ-to-α mapping adapts automatically to other publish rates.
    /// <br/>
    /// Behavioral note:
    /// <see cref="Step"/> may return <c>false</c> even after the estimator is ready, if the current sample does not
    /// land on a publish boundary (i.e., it is not yet time to produce the next output).
    /// </param>
    /// <param name="nominalFrequency">
    /// Nominal line frequency (typically 50Hz or 60Hz). This determines the nominal-cycle length used
    /// for the DFT window and the expected reference-phase progression.
    /// </param>
    /// <param name="referenceChannel">
    /// Reference channel for frequency tracking.
    /// Default: <see cref="PhaseChannel.VA"/>.
    /// </param>
    /// <param name="targetCycles">
    /// Number of nominal cycles contained in the sliding DFT analysis window (default: 2).
    /// Larger values generally reduce noise/jitter (more averaging) but increase latency and reduce step response.
    /// </param>
    /// <param name="enableIntervalAveraging">
    /// Enables interval averaging (boxcar averaging) across each publish interval when down-sampling
    /// (<paramref name="outputRateHz"/> &lt; <paramref name="sampleRateHz"/>).
    /// <br/>
    /// Why this exists:
    /// Down-sampling without an anti-alias / low-pass step will preserve high-rate jitter and can alias higher-frequency
    /// content into the published stream. Interval averaging acts as a simple, cheap low-pass filter that reduces
    /// jitter and improves published stability.
    /// <br/>
    /// Consequences:<br/>
    /// - Improves calmness and reduces aliasing artifacts.<br/>
    /// - Adds a small amount of additional latency (effectively ~½ publish interval group delay).<br/>
    /// Recommended: <c>true</c> (default).
    /// </param>
    /// <param name="enablePublishEMA">
    /// Enables an additional exponential moving average (EMA) applied to the published stream (after interval averaging).
    /// <br/>
    /// Why this exists:<br/>
    /// Interval averaging removes high-rate noise; publish-EMA further reduces remaining jitter and produces a "calm"
    /// display or control signal. This is usually the most intuitive "knob" for operators/consumers because it acts on
    /// the actual output cadence.
    /// <br/>
    /// Consequences:<br/>
    /// - Larger time constants (τ) produce smoother outputs but slower response to real changes.<br/>
    /// - Smaller τ tracks faster but appears noisier.<br/>
    /// Default: <c>true</c>.
    /// </param>
    /// <param name="publishAnglesTauSeconds">
    /// EMA time constant τ (seconds) for published phase angles. Default: 0.35s.
    /// <br/>
    /// Important:<br/>
    /// Angles are circular quantities; this implementation performs wrap-safe smoothing by operating on unit vectors
    /// (cos/sin) rather than naïvely averaging radians. This avoids discontinuities at ±π.
    /// </param>
    /// <param name="publishMagnitudesTauSeconds">
    /// EMA time constant τ (seconds) for published RMS magnitudes. Default: 0.50s.
    /// </param>
    /// <param name="publishFrequencyTauSeconds">
    /// EMA time constant τ (seconds) for published frequency. Default: 0.75s.
    /// </param>
    /// <param name="publishRocofTauSeconds">
    /// EMA time constant τ (seconds) for published ROCOF (dF/dt). Default: 1.50s.
    /// <br/>
    /// Note:<br/>
    /// ROCOF is effectively a derivative signal and is typically much noisier than frequency; it generally benefits from
    /// heavier smoothing (larger τ) than frequency.
    /// </param>
    /// <param name="sampleFrequencyTauSeconds">
    /// EMA time constant τ (seconds) for the internal per-sample frequency smoothing that occurs inside the estimator
    /// before any down-sampling/publish filtering. Default: 0.15s.
    /// <br/>
    /// Guidance:<br/>
    /// When interval averaging + publish EMA are enabled, this can be relatively light. If you disable publish smoothing,
    /// you may want to increase this τ.
    /// </param>
    /// <param name="sampleRocofTauSeconds">
    /// EMA time constant τ (seconds) for the internal per-sample ROCOF smoothing (computed from the internally smoothed
    /// frequency). Default: 0.30s.
    /// </param>
    /// <param name="recalculationCycles">
    /// Number of nominal cycles between full DFT recalculations for numerical stability (default: 10).
    /// Sliding DFT updates are O(1) per sample but can accumulate numerical drift; periodic full recomputation
    /// re-anchors the phasor sums.
    /// </param>
    /// <param name="maxGapFillSamples">
    /// Maximum gap (in input samples) that will be filled by phase-continued synthesis (coasting the last
    /// phasors at the tracked frequency) when the input timestamp cadence indicates dropped samples. Gaps
    /// larger than this trigger a resynchronization (the analysis window is dropped and refilled).
    /// A negative value (default) means "auto" = one full analysis window; <c>0</c> disables coasting
    /// (any gap resynchronizes). Must not exceed twice <see cref="WindowSamples"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if any rate is non-positive, output rate exceeds input rate, targetCycles &lt; 1, or any τ is negative.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <b>Time constant (τ) vs "alpha" (α):</b><br/>
    /// Internally, EMAs use α constrained from 0 to 1. This API exposes τ because it is easier to reason about.
    /// For update period Δt (seconds), α is derived as:
    /// <code>
    /// α = 1 - exp(-Δt / τ)
    /// </code>
    /// Interpretation: after a step change, an EMA will move about 63% toward the new value after ~τ seconds.
    /// </para>
    /// <para>
    /// <b>Two-stage smoothing (when outputRate &lt; inputRate):</b>
    /// <list type="number">
    /// <item>
    /// <description><b>Interval averaging (boxcar)</b> across the publish interval (anti-alias / jitter reduction).</description>
    /// </item>
    /// <item>
    /// <description><b>Publish EMA</b> applied to the down-sampled stream (additional "calmness").</description>
    /// </item>
    /// </list>
    /// This combination yields stable outputs at 60Hz without producing thousands of nearly-identical jittery estimates per second.
    /// </para>
    /// <para>
    /// <b>Knob-tweaking guidance:</b>
    /// <list type="bullet">
    /// <item>
    /// <description>Increase <paramref name="targetCycles"/> to reduce noise/jitter, at the cost of more latency.</description>
    /// </item>
    /// <item>
    /// <description>Increase publish τ values to make outputs calmer, at the cost of slower response.</description>
    /// </item>
    /// <item>
    /// <description>If you disable interval averaging, consider increasing publish τ values to compensate.</description>
    /// </item>
    /// <item>
    /// <description>Keep ROCOF τ larger than frequency τ; ROCOF is inherently noisier.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public RollingPhaseEstimator(
        double sampleRateHz,
        double outputRateHz,
        LineFrequency nominalFrequency,
        PhaseChannel referenceChannel = DefaultReferenceChannel,
        int targetCycles = DefaultTargetCycles,
        bool enableIntervalAveraging = DefaultEnableIntervalAveraging,
        bool enablePublishEMA = DefaultEnablePublishEMA,
        double publishAnglesTauSeconds = DefaultPublishAnglesTauSeconds,
        double publishMagnitudesTauSeconds = DefaultPublishMagnitudesTauSeconds,
        double publishFrequencyTauSeconds = DefaultPublishFrequencyTauSeconds,
        double publishRocofTauSeconds = DefaultPublishRocofTauSeconds,
        double sampleFrequencyTauSeconds = DefaultSampleFrequencyTauSeconds,
        double sampleRocofTauSeconds = DefaultSampleRocofTauSeconds,
        int recalculationCycles = DefaultRecalculationCycles,
        int maxGapFillSamples = DefaultMaxGapFillSamples)
    {
        // Validate rates / window
        if (sampleRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRateHz), "Input sample rate must be positive.");

        if (outputRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(outputRateHz), "Output rate must be positive.");

        if (outputRateHz > sampleRateHz)
            throw new ArgumentOutOfRangeException(nameof(outputRateHz), "Output rate must be <= input sample rate.");

        if (targetCycles < 1)
            throw new ArgumentOutOfRangeException(nameof(targetCycles), "Target cycles must be at least 1.");

        if (recalculationCycles < 1)
            throw new ArgumentOutOfRangeException(nameof(recalculationCycles), "Recalculation cycles must be at least 1.");

        // Validate taus (τ >= 0). τ == 0 means "no smoothing" (alpha = 1).
        if (publishAnglesTauSeconds < 0.0D)
            throw new ArgumentOutOfRangeException(nameof(publishAnglesTauSeconds), "Tau must be >= 0.");

        if (publishMagnitudesTauSeconds < 0.0D)
            throw new ArgumentOutOfRangeException(nameof(publishMagnitudesTauSeconds), "Tau must be >= 0.");

        if (publishFrequencyTauSeconds < 0.0D)
            throw new ArgumentOutOfRangeException(nameof(publishFrequencyTauSeconds), "Tau must be >= 0.");

        if (publishRocofTauSeconds < 0.0D)
            throw new ArgumentOutOfRangeException(nameof(publishRocofTauSeconds), "Tau must be >= 0.");

        if (sampleFrequencyTauSeconds < 0.0D)
            throw new ArgumentOutOfRangeException(nameof(sampleFrequencyTauSeconds), "Tau must be >= 0.");

        if (sampleRocofTauSeconds < 0.0D)
            throw new ArgumentOutOfRangeException(nameof(sampleRocofTauSeconds), "Tau must be >= 0.");

        // Store configuration
        SampleRateHz = sampleRateHz;
        OutputRateHz = outputRateHz;
        NominalFrequencyHz = (double)nominalFrequency;
        ReferenceChannel = referenceChannel;

        // Precompute nominal-derived frequency-tracking constants
        m_minFrequency = NominalFrequencyHz * 0.9D;
        m_maxFrequency = NominalFrequencyHz * 1.1D;

        m_samplePeriodSeconds = 1.0D / sampleRateHz;

        m_publishPeriodNs = outputRateHz >= sampleRateHz ?
            0L : (long)Math.Round(NanosecondsPerSecond / outputRateHz);

        m_enableIntervalAveraging = enableIntervalAveraging && m_publishPeriodNs != 0L;
        m_enablePublishEMA = enablePublishEMA;

        // Convert τ -> α using the appropriate Δt for each EMA
        double publishDt = 1.0D / outputRateHz;     // published stream cadence
        double sampleDt = 1.0D / sampleRateHz;      // per-sample cadence

        m_publishAnglesAlpha = AlphaFromTau(publishDt, publishAnglesTauSeconds);
        m_publishMagnitudesAlpha = AlphaFromTau(publishDt, publishMagnitudesTauSeconds);
        m_publishFrequencyAlpha = AlphaFromTau(publishDt, publishFrequencyTauSeconds);
        m_publishRocofAlpha = AlphaFromTau(publishDt, publishRocofTauSeconds);

        m_frequencyAlpha = AlphaFromTau(sampleDt, sampleFrequencyTauSeconds);
        m_rocofAlpha = AlphaFromTau(sampleDt, sampleRocofTauSeconds);

        // Precompute (1 - α) complements used in the EMA hot paths
        m_oneMinusPublishAnglesAlpha = 1.0D - m_publishAnglesAlpha;
        m_oneMinusPublishMagnitudesAlpha = 1.0D - m_publishMagnitudesAlpha;
        m_oneMinusPublishFrequencyAlpha = 1.0D - m_publishFrequencyAlpha;
        m_oneMinusPublishRocofAlpha = 1.0D - m_publishRocofAlpha;
        m_oneMinusFrequencyAlpha = 1.0D - m_frequencyAlpha;
        m_oneMinusRocofAlpha = 1.0D - m_rocofAlpha;

        int samplesPerNominalCycle = (int)Math.Round(sampleRateHz / NominalFrequencyHz);
        WindowSamples = samplesPerNominalCycle * targetCycles;
        m_recalculationInterval = samplesPerNominalCycle * recalculationCycles;

        // Align the first recalc countdown so recalcs land on the same absolute cadence a modulo would
        m_initialSamplesUntilRecalc = m_recalculationInterval - WindowSamples % m_recalculationInterval;

        // Gap-fill bound: validate for reasonability against the window, then resolve "auto" (negative) to one window
        if (maxGapFillSamples > 2 * WindowSamples)
            throw new ArgumentOutOfRangeException(nameof(maxGapFillSamples), "Max gap fill samples should not exceed twice the window size.");

        m_maxGapFillSamples = maxGapFillSamples < 0 ? WindowSamples : maxGapFillSamples;
        m_samplePeriodNs = NanosecondsPerSecond / sampleRateHz;

        // Precompute RMS magnitude scale factor from DFT magnitude.
        // For a pure sinusoid: |X| = N * A / 2 (A = peak amplitude); RMS = A / sqrt(2), so RMS = sqrt(2) * |X| / N.
        m_magnitudeScale = Sqrt2 / WindowSamples;

        // Initialize sample buffers
        m_sampleBuffers = new double[NumChannels][];

        for (int ch = 0; ch < NumChannels; ch++)
            m_sampleBuffers[ch] = new double[WindowSamples];

        // DFT twiddle factor for sliding update at bin k: e^(j*2π*k/N), where k = targetCycles
        double twiddleAngle = TwoPI * targetCycles / WindowSamples;
        m_twiddleReal = Math.Cos(twiddleAngle);
        m_twiddleImag = Math.Sin(twiddleAngle);

        // Precompute cos/sin tables for full DFT calculation at bin k = targetCycles
        m_cosTable = new double[WindowSamples];
        m_sinTable = new double[WindowSamples];

        for (int i = 0; i < WindowSamples; i++)
        {
            double angle = TwoPI * targetCycles * i / WindowSamples;
            m_cosTable[i] = Math.Cos(angle);
            m_sinTable[i] = Math.Sin(angle);
        }

        // Initialize phasor accumulators
        m_phasorReal = new double[NumChannels];
        m_phasorImag = new double[NumChannels];

        // Initialize frequency tracking (internal)
        m_smoothedFrequency = NominalFrequencyHz;
        m_prevSmoothedFrequency = NominalFrequencyHz;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the configured sample rate in Hz.
    /// </summary>
    public double SampleRateHz { get; }

    /// <summary>
    /// Gets the configured output publish rate in Hz.
    /// </summary>
    public double OutputRateHz { get; }

    /// <summary>
    /// Gets the nominal frequency in Hz.
    /// </summary>
    public double NominalFrequencyHz { get; }

    /// <summary>
    /// Gets the reference channel index used for frequency tracking (typically VA).
    /// </summary>
    public PhaseChannel ReferenceChannel { get; }

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
    /// <c>true</c> when an estimate is available (window has filled) and it is time to 
    /// publish at target <see cref="OutputRateHz"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The data referenced in the span-based properties of the <see cref="PhaseEstimate"/>
    /// parameter provided to the <see cref="PhaseEstimateHandler"/> delegate is owned by the
    /// <see cref="RollingPhaseEstimator"/> instance, it is only valid during the scope of the
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

        // Missing-data handling: infer dropped samples from the input timestamp cadence, then either
        // coast across a small gap (phase-continued synthesis) or resynchronize across a large one.
        if (m_prevInputEpochNs > 0L)
        {
            long deltaNs = epochNanoseconds - m_prevInputEpochNs;

            if (deltaNs <= 0L)
            {
                // Backward or duplicate timestamp. Unexpected for GPS-synced sources, so the sample is ignored.
                // NOTE: if rewound/looped streams become a real case, treat a backward jump as a resync instead.
                return false;
            }

            // Samples missing between the previous input and this one (rounding absorbs minor timing jitter).
            double missing = Math.Round(deltaNs / m_samplePeriodNs) - 1.0D;

            if (missing > 0.0D)
            {
                if (IsReady && missing <= m_maxGapFillSamples)
                {
                    CoastFillGap((int)missing);

                    // Re-anchor frequency tracking to the post-coast state so the upcoming real sample yields a
                    // clean single-sample phase delta (a multi-sample delta would wrap and corrupt the estimate).
                    int referenceChannel = (int)ReferenceChannel;
                    m_prevPhaseAngle = Math.Atan2(m_phasorImag[referenceChannel], m_phasorReal[referenceChannel]);
                    m_prevEpochNs = epochNanoseconds - (long)m_samplePeriodNs;
                }
                else
                {
                    Resync();
                }
            }
        }

        // Apply the real sample to the buffers and SDFT
        Span<double> samples = stackalloc double[NumChannels] { va, vb, vc, ia, ib, ic };
        AdvanceOneSample(samples);

        m_prevInputEpochNs = epochNanoseconds;

        // Check if window has filled
        if (TotalSamplesProcessed < WindowSamples)
            return false;

        // Compute per-sample estimate (this is the "high-rate" stream)
        SamplePhaseEstimate sampleEstimate = ComputeEstimate(epochNanoseconds);

        if (m_publishPeriodNs == 0L)
        {
            // Full-rate publishing: publish cadence equals sample cadence (so publish EMA is well-defined here)
            sampleEstimate.Angles.AsSpan().CopyTo(m_publishAngles);
            sampleEstimate.Magnitudes.AsSpan().CopyTo(m_publishMagnitudes);

            double freq = sampleEstimate.Frequency;
            double rocof = sampleEstimate.dFdt;

            if (m_enablePublishEMA)
                ApplyPublishEMA(m_publishAngles, m_publishMagnitudes, ref freq, ref rocof);

            phaseEstimateHandler(new PhaseEstimate
            {
                Frequency = freq,
                dFdt = rocof,
                Angles = m_publishAngles,
                Magnitudes = m_publishMagnitudes
            });

            return true;
        }

        if (!m_enableIntervalAveraging)
            StoreLastSample(sampleEstimate);

        // Initialize publish schedule the first time we become ready
        if (!m_publishScheduleInitialized)
        {
            m_nextPublishEpochNs = AlignToNextBoundary(epochNanoseconds, m_publishPeriodNs);
            m_publishScheduleInitialized = true;
        }

        // Accumulate all per-sample estimates within a publish interval.
        // This acts as an anti-alias low-pass when outputRate < sampleRate.
        if (m_enableIntervalAveraging)
            AccumulateInterval(sampleEstimate);

        // Not time to publish yet
        if (epochNanoseconds < m_nextPublishEpochNs)
            return false;

        // Time to publish: produce a down-sampled estimate
        PhaseEstimate estimate = ProduceIntervalEstimate();

        // Advance next publish time
        if (m_publishPeriodNs > 0L)
        {
            // Handle possible gaps safely
            long behind = epochNanoseconds - m_nextPublishEpochNs;
            long steps = behind >= 0L ? behind / m_publishPeriodNs + 1L : 1L;
            m_nextPublishEpochNs += steps * m_publishPeriodNs;
        }

        phaseEstimateHandler(estimate);
        return true;
    }

    // Applies one sample-group to the circular buffers and the sliding DFT (no estimate / no publish).
    // Shared by real input samples and synthesized gap-fill samples so the window stays contiguous.
    private void AdvanceOneSample(ReadOnlySpan<double> samples)
    {
        bool isFillingUp = TotalSamplesProcessed < WindowSamples;
        int oldestIndex = m_bufferWriteIndex;

        for (int ch = 0; ch < NumChannels; ch++)
        {
            double oldSample = m_sampleBuffers[ch][oldestIndex];
            double newSample = samples[ch];

            m_sampleBuffers[ch][oldestIndex] = newSample;

            if (isFillingUp)
            {
                // During fill-up, add contribution of new sample to DFT at correct phase
                int dftIndex = (int)(TotalSamplesProcessed % WindowSamples);
                m_phasorReal[ch] += newSample * m_cosTable[dftIndex];
                m_phasorImag[ch] -= newSample * m_sinTable[dftIndex];
            }
            else
            {
                // Sliding DFT update: X_new = (X_old - x_oldest + x_new) * e^(j*2π/N)
                double unrotatedReal = m_phasorReal[ch] + (newSample - oldSample);
                double unrotatedImag = m_phasorImag[ch];

                m_phasorReal[ch] = unrotatedReal * m_twiddleReal - unrotatedImag * m_twiddleImag;
                m_phasorImag[ch] = unrotatedReal * m_twiddleImag + unrotatedImag * m_twiddleReal;
            }
        }

        // Advance buffer write index (compare/reset avoids a per-sample modulo)
        if (++m_bufferWriteIndex >= WindowSamples)
            m_bufferWriteIndex = 0;

        TotalSamplesProcessed++;

        // Periodic full recalculation for numerical stability: immediately after fill-up, then on a fixed
        // sample countdown aligned to m_recalculationInterval (avoids a per-sample modulo).
        bool justFilled = TotalSamplesProcessed == WindowSamples;

        if (justFilled)
        {
            m_samplesUntilRecalc = m_initialSamplesUntilRecalc;
            RecalculateFullDft();
        }
        else if (!isFillingUp && --m_samplesUntilRecalc == 0)
        {
            m_samplesUntilRecalc = m_recalculationInterval;
            RecalculateFullDft();
        }
    }

    // Fills a detected gap of "missing" samples by synthesizing a phase-continued sinusoid for each
    // channel from the current phasor state, feeding them through the SDFT so the window stays
    // temporally contiguous. Only valid when IsReady (a settled phasor exists to extrapolate from).
    private void CoastFillGap(int missing)
    {
        // Per-sample phase advance at the tracked frequency: ω = 2π·f/fs.
        double omega = TwoPI * m_smoothedFrequency * m_samplePeriodSeconds;

        // Reconstruct each channel's raw-waveform argument at the last real sample from its phasor.
        // For a tone at bin k, the phasor angle ψ relates to the newest sample's cosine argument by
        // rawArg = ψ + ω·(N-1), and peak amplitude A = 2·|X|/N.
        double windowOffset = omega * (WindowSamples - 1);

        Span<double> amplitude = stackalloc double[NumChannels];
        Span<double> rawArg0 = stackalloc double[NumChannels];

        for (int ch = 0; ch < NumChannels; ch++)
        {
            double re = m_phasorReal[ch];
            double im = m_phasorImag[ch];

            amplitude[ch] = 2.0D * Math.Sqrt(re * re + im * im) / WindowSamples;
            rawArg0[ch] = Math.Atan2(im, re) + windowOffset;
        }

        Span<double> synthesized = stackalloc double[NumChannels];

        for (int k = 1; k <= missing; k++)
        {
            for (int ch = 0; ch < NumChannels; ch++)
                synthesized[ch] = amplitude[ch] * Math.Cos(rawArg0[ch] + omega * k);

            AdvanceOneSample(synthesized);
        }
    }

    // Resynchronizes after a gap too large to coast: drops the in-flight SDFT/window and frequency
    // state so the window refills from fresh contiguous samples. Published EMA state and the publish
    // schedule are intentionally preserved so consumers see continuity while the window refills.
    // NOTE: to instead snap outputs to the resynchronized values, also call ResetPublishEMA() here.
    private void Resync()
    {
        for (int ch = 0; ch < NumChannels; ch++)
            Array.Clear(m_sampleBuffers[ch], 0, WindowSamples);

        m_bufferWriteIndex = 0;
        TotalSamplesProcessed = 0L;

        Array.Clear(m_phasorReal, 0, NumChannels);
        Array.Clear(m_phasorImag, 0, NumChannels);

        m_prevPhaseAngle = 0.0D;
        m_hasPrevPhase = false;
        m_hasLastSampleEstimate = false;
        m_smoothedFrequency = NominalFrequencyHz;
        m_prevSmoothedFrequency = NominalFrequencyHz;
        m_smoothedRocof = 0.0D;
        m_frequencyInitialized = false;
        m_rocofInitialized = false;
        m_prevEpochNs = 0L;

        ResetInterval();
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
        TotalSamplesProcessed = 0L;

        // Reset phasor accumulators
        Array.Clear(m_phasorReal, 0, NumChannels);
        Array.Clear(m_phasorImag, 0, NumChannels);

        // Reset frequency tracking state
        m_prevPhaseAngle = 0.0D;
        m_hasPrevPhase = false;
        m_hasLastSampleEstimate = false;
        m_smoothedFrequency = NominalFrequencyHz;
        m_prevSmoothedFrequency = NominalFrequencyHz;
        m_smoothedRocof = 0.0D;
        m_frequencyInitialized = false;
        m_rocofInitialized = false;
        m_prevEpochNs = 0L;
        m_prevInputEpochNs = 0L;
        m_nextPublishEpochNs = 0L;
        m_publishScheduleInitialized = false;

        ResetInterval();
        ResetPublishEMA();
    }

    private void ResetInterval()
    {
        m_intervalCount = 0;

        Array.Clear(m_intervalAngleCosSum, 0, NumChannels);
        Array.Clear(m_intervalAngleSinSum, 0, NumChannels);
        Array.Clear(m_intervalMagnitudeSum, 0, NumChannels);

        m_intervalFreqSum = 0.0D;
        m_intervalRocofSum = 0.0D;
    }

    private void ResetPublishEMA()
    {
        m_publishEMAInitialized = false;

        Array.Clear(m_pubAngleCos, 0, NumChannels);
        Array.Clear(m_pubAngleSin, 0, NumChannels);
        Array.Clear(m_pubMagnitude, 0, NumChannels);

        m_pubFrequency = 0.0D;
        m_pubRocof = 0.0D;
    }

    // Performs a full DFT recalculation from the sample buffers for numerical stability.
    private void RecalculateFullDft()
    {
        // The buffer write index points to the oldest sample (next to be overwritten)
        // So we read starting from that index
        int startIndex = m_bufferWriteIndex;

        for (int ch = 0; ch < NumChannels; ch++)
        {
            double sumReal = 0.0D;
            double sumImag = 0.0D;

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

    private void AccumulateInterval(in SamplePhaseEstimate estimate)
    {
        m_intervalCount++;

        // Angles: accumulate unit vectors to avoid wrap issues
        for (int ch = 0; ch < NumChannels; ch++)
        {
            double angle = estimate.Angles[ch];
            m_intervalAngleCosSum[ch] += Math.Cos(angle);
            m_intervalAngleSinSum[ch] += Math.Sin(angle);
            m_intervalMagnitudeSum[ch] += estimate.Magnitudes[ch];
        }

        m_intervalFreqSum += estimate.Frequency;
        m_intervalRocofSum += estimate.dFdt;
    }

    // Do not call ProduceIntervalEstimate twice without another interval fill,
    // otherwise you will be smoothing already-smoothed values
    private PhaseEstimate ProduceIntervalEstimate()
    {
        Angle[] angles;
        double[] magnitudes;
        double freq;
        double rocof;

        // If interval averaging is disabled, publish the most recent per-sample estimate ("sample-and-hold").
        // This avoids pretending to "average" while the knob says we shouldn’t.
        if (!m_enableIntervalAveraging)
        {
            if (!m_hasLastSampleEstimate)
                throw new InvalidOperationException("No sample estimate available"); // Unexpected

            // Copy scalars to locals to not mutate the stored last sample
            freq = m_lastFrequency;
            rocof = m_lastRocof;

            // Copy held sample into publish buffers so ApplyPublishEMA can safely write back.
            m_lastAngles.AsSpan().CopyTo(m_publishAngles);
            m_lastMagnitudes.AsSpan().CopyTo(m_publishMagnitudes);

            angles = m_publishAngles;
            magnitudes = m_publishMagnitudes;

            if (m_enablePublishEMA)
                ApplyPublishEMA(angles, magnitudes, ref freq, ref rocof);

            return new PhaseEstimate
            {
                Frequency = freq,
                dFdt = rocof,
                Angles = angles,
                Magnitudes = magnitudes
            };
        }

        // Interval-averaged publish path (anti-aliasing low-pass)
        int intervalCount = m_intervalCount > 0 ? m_intervalCount : 1;

        angles = m_publishAngles;
        magnitudes = m_publishMagnitudes;

        for (int ch = 0; ch < NumChannels; ch++)
        {
            double meanAngle = Math.Atan2(m_intervalAngleSinSum[ch], m_intervalAngleCosSum[ch]);
            angles[ch] = NormalizeAngle(meanAngle);
            magnitudes[ch] = m_intervalMagnitudeSum[ch] / intervalCount;
        }

        freq = m_intervalFreqSum / intervalCount;
        rocof = m_intervalRocofSum / intervalCount;

        ResetInterval();

        if (m_enablePublishEMA)
            ApplyPublishEMA(angles, magnitudes, ref freq, ref rocof);

        return new PhaseEstimate
        {
            Frequency = freq,
            dFdt = rocof,
            Angles = angles,
            Magnitudes = magnitudes
        };
    }

    private void ApplyPublishEMA(Angle[] angles, double[] magnitudes, ref double freq, ref double rocof)
    {
        if (m_publishEMAInitialized)
        {
            // Angles: EMA on unit vectors (wrap-safe)
            for (int ch = 0; ch < NumChannels; ch++)
            {
                double angle = angles[ch];
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);

                m_pubAngleCos[ch] = m_publishAnglesAlpha * cos + m_oneMinusPublishAnglesAlpha * m_pubAngleCos[ch];
                m_pubAngleSin[ch] = m_publishAnglesAlpha * sin + m_oneMinusPublishAnglesAlpha * m_pubAngleSin[ch];

                m_pubMagnitude[ch] = m_publishMagnitudesAlpha * magnitudes[ch] + m_oneMinusPublishMagnitudesAlpha * m_pubMagnitude[ch];
            }

            m_pubFrequency = m_publishFrequencyAlpha * freq + m_oneMinusPublishFrequencyAlpha * m_pubFrequency;
            m_pubRocof = m_publishRocofAlpha * rocof + m_oneMinusPublishRocofAlpha * m_pubRocof;
        }
        else
        {
            for (int ch = 0; ch < NumChannels; ch++)
            {
                double angle = angles[ch];
                m_pubAngleCos[ch] = Math.Cos(angle);
                m_pubAngleSin[ch] = Math.Sin(angle);
                m_pubMagnitude[ch] = magnitudes[ch];
            }

            m_pubFrequency = freq;
            m_pubRocof = rocof;
            m_publishEMAInitialized = true;
        }

        // Write back smoothed values
        for (int ch = 0; ch < NumChannels; ch++)
        {
            double angle = Math.Atan2(m_pubAngleSin[ch], m_pubAngleCos[ch]);
            angles[ch] = NormalizeAngle(angle);
            magnitudes[ch] = m_pubMagnitude[ch];
        }

        freq = m_pubFrequency;
        rocof = m_pubRocof;
    }

    // Computes the per-sample phase estimate from current phasor values.
    private SamplePhaseEstimate ComputeEstimate(long epochNanoseconds)
    {
        Angle[] angles = m_workingAngles;
        double[] magnitudes = m_workingMagnitudes;
        int referenceChannel = (int)ReferenceChannel;

        // Reference angle for phase normalization (typically VA); also reused below as the reference
        // channel's current phase for frequency tracking, which avoids a duplicate Atan2 evaluation.
        double referenceAngle = Math.Atan2(m_phasorImag[referenceChannel], m_phasorReal[referenceChannel]);

        for (int ch = 0; ch < NumChannels; ch++)
        {
            double real = m_phasorReal[ch];
            double imag = m_phasorImag[ch];

            // Calculate magnitude (RMS)
            magnitudes[ch] = Math.Sqrt(real * real + imag * imag) * m_magnitudeScale;

            // Calculate angle relative to reference (VA)
            double rawAngle = Math.Atan2(imag, real);
            angles[ch] = NormalizeAngle(rawAngle - referenceAngle);
        }

        // Calculate time delta since last sample
        double deltaTimeSeconds = m_samplePeriodSeconds;

        if (m_prevEpochNs > 0)
        {
            double measuredDelta = (epochNanoseconds - m_prevEpochNs) / (double)NanosecondsPerSecond;

            if (measuredDelta is > 0.0D and < 1.0D)
                deltaTimeSeconds = measuredDelta;
        }

        // Frequency from reference channel phase progression (reuses referenceAngle as current phase).
        // FUTURE: Track frequency from the positive-sequence component instead of a single reference
        // channel, so a single-channel fault (e.g., a VA PT failure) does not degrade the frequency /
        // ROCOF estimate while the remaining phases are still healthy.
        double instantaneousFrequency = NominalFrequencyHz;

        if (m_hasPrevPhase)
        {
            double deltaPhase = NormalizeAngle(referenceAngle - m_prevPhaseAngle);

            // Instantaneous frequency from phase progression. The full form is
            // NominalFrequencyHz + (deltaPhase - 2π·fNom·dt) / (2π·dt), which cancels exactly to
            // deltaPhase / (2π·dt) — fewer ops and avoids adding/subtracting the large nominal term.
            instantaneousFrequency = deltaPhase / (TwoPI * deltaTimeSeconds);

            // Clamp instantaneous frequency to ±10% of nominal to reject transient / bad-data excursions.
            // This saturates silently, so it effectively acts as a data-quality guard. Surfacing that as an
            // output quality flag is protocol-dependent: SEL CWS currently carries no quality flags, so any
            // quality indication would have to be expressed through the existing base-model quality fields.
            // FUTURE: Revisit quality reporting if/when SEL CWS adds native quality flags.
            instantaneousFrequency = Math.Max(m_minFrequency, Math.Min(m_maxFrequency, instantaneousFrequency));
        }

        m_prevPhaseAngle = referenceAngle;
        m_hasPrevPhase = true;

        // Smooth frequency
        if (m_frequencyInitialized)
        {
            m_smoothedFrequency = m_frequencyAlpha * instantaneousFrequency + m_oneMinusFrequencyAlpha * m_smoothedFrequency;
        }
        else
        {
            m_smoothedFrequency = instantaneousFrequency;
            m_frequencyInitialized = true;
        }

        // ROCOF
        double rocof = 0.0D;

        if (m_rocofInitialized)
        {
            double instantaneousRocof = (m_smoothedFrequency - m_prevSmoothedFrequency) / deltaTimeSeconds;
            m_smoothedRocof = m_rocofAlpha * instantaneousRocof + m_oneMinusRocofAlpha * m_smoothedRocof;
            rocof = m_smoothedRocof;
        }
        else
        {
            m_rocofInitialized = true;
        }

        m_prevSmoothedFrequency = m_smoothedFrequency;
        m_prevEpochNs = epochNanoseconds;

        // Arrays are reused; callers must treat results as ephemeral per Step call
        return new SamplePhaseEstimate
        {
            Frequency = m_smoothedFrequency,
            dFdt = rocof,
            Angles = angles,
            Magnitudes = magnitudes
        };
    }

    // Normalizes an angle to the range [-π, π]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Angle NormalizeAngle(double angle)
    {
        return new Angle(angle).ToRange(-Math.PI, false);
    }

    // Converts a time constant (τ) to an EMA alpha (α) for a given update period (Δt)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double AlphaFromTau(double dtSeconds, double tauSeconds)
    {
        // τ == 0 => α == 1 (no smoothing; output follows input immediately)
        if (tauSeconds <= 0.0D)
            return 1.0D;

        // Numerically stable for typical dt/tau ratios; dt and tau are positive here
        return 1.0D - Math.Exp(-dtSeconds / tauSeconds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long AlignToNextBoundary(long timestampNs, long periodNs)
    {
        long remainder = timestampNs % periodNs;
        return remainder == 0 ? timestampNs : timestampNs + (periodNs - remainder);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StoreLastSample(in SamplePhaseEstimate estimate)
    {
        estimate.Angles.AsSpan().CopyTo(m_lastAngles);
        estimate.Magnitudes.AsSpan().CopyTo(m_lastMagnitudes);
        m_lastFrequency = estimate.Frequency;
        m_lastRocof = estimate.dFdt;
        m_hasLastSampleEstimate = true;
    }

    #endregion
}
