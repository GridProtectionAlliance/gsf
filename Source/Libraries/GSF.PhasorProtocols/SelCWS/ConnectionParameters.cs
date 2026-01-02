//******************************************************************************************************
//  ConnectionParameters.cs - Gbtc
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
//  02/26/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Units.EE;
using static GSF.PhasorProtocols.SelCWS.RollingPhaseEstimator;

namespace GSF.PhasorProtocols.SelCWS;

/// <summary>
/// Represents the extra connection parameters required for a connection to a SEL CWS device.
/// </summary>
/// <remarks>
/// This class is designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.
/// As a result the <see cref="CategoryAttribute"/> and <see cref="DescriptionAttribute"/> elements should be defined for
/// each of the exposed properties.
/// </remarks>
[Serializable]
public class ConnectionParameters : ConnectionParametersBase
{
    #region [ Members ]

    // Constants

    /// <summary>
    /// Default value for <see cref="CalculatePhaseEstimates"/>.
    /// </summary>
    public const bool DefaultCalculatePhaseEstimates = true;

    /// <summary>
    /// Default value for <see cref="RepeatLastCalculatedValueWhenDownSampling"/>.
    /// </summary>
    public const bool DefaultRepeatLastCalculatedValueWhenDownSampling = true;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="ConnectionParameters"/>.
    /// </summary>
    public ConnectionParameters()
    {
        CalculatePhaseEstimates = DefaultCalculatePhaseEstimates;
        NominalFrequency = Common.DefaultNominalFrequency;
        CalculationFrameRate = Common.DefaultFramePerSecond;
        RepeatLastCalculatedValueWhenDownSampling = DefaultRepeatLastCalculatedValueWhenDownSampling;
        ReferenceChannel = DefaultReferenceChannel;
        TargetCycles = DefaultTargetCycles;
        EnableIntervalAveraging = DefaultEnableIntervalAveraging;
        EnablePublishEMA = DefaultEnablePublishEMA;
        PublishAnglesTauSeconds = DefaultPublishAnglesTauSeconds;
        PublishMagnitudesTauSeconds = DefaultPublishMagnitudesTauSeconds;
        PublishFrequencyTauSeconds = DefaultPublishFrequencyTauSeconds;
        PublishRocofTauSeconds = DefaultPublishRocofTauSeconds;
        SampleFrequencyTauSeconds = DefaultSampleFrequencyTauSeconds;
        SampleRocofTauSeconds = DefaultSampleRocofTauSeconds;
        RecalculationCycles = DefaultRecalculationCycles;
    }

    /// <summary>
    /// Creates a new <see cref="ConnectionParameters"/> from serialization parameters.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
    /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
    protected ConnectionParameters(SerializationInfo info, StreamingContext context)
    {
        // Deserialize connection parameters
        CalculatePhaseEstimates = info.GetOrDefault("calculatePhaseEstimates", DefaultCalculatePhaseEstimates);
        NominalFrequency = info.GetOrDefault("nominalFrequency", Common.DefaultNominalFrequency);
        CalculationFrameRate = info.GetOrDefault("calculationFrameRate", Common.DefaultFramePerSecond);
        RepeatLastCalculatedValueWhenDownSampling = info.GetOrDefault("repeatLastCalculatedValueWhenDownSampling", DefaultRepeatLastCalculatedValueWhenDownSampling);
        ReferenceChannel = info.GetOrDefault("referenceChannel", DefaultReferenceChannel);
        TargetCycles = info.GetOrDefault("targetCycles", DefaultTargetCycles);
        EnableIntervalAveraging = info.GetOrDefault("enableIntervalAveraging", DefaultEnableIntervalAveraging);
        EnablePublishEMA = info.GetOrDefault("enablePublishEMA", DefaultEnablePublishEMA);
        PublishAnglesTauSeconds = info.GetOrDefault("publishAnglesTauSeconds", DefaultPublishAnglesTauSeconds);
        PublishMagnitudesTauSeconds = info.GetOrDefault("publishMagnitudesTauSeconds", DefaultPublishMagnitudesTauSeconds);
        PublishFrequencyTauSeconds = info.GetOrDefault("publishFrequencyTauSeconds", DefaultPublishFrequencyTauSeconds);
        PublishRocofTauSeconds = info.GetOrDefault("publishRocofTauSeconds", DefaultPublishRocofTauSeconds);
        SampleFrequencyTauSeconds = info.GetOrDefault("sampleFrequencyTauSeconds", DefaultSampleFrequencyTauSeconds);
        SampleRocofTauSeconds = info.GetOrDefault("sampleRocofTauSeconds", DefaultSampleRocofTauSeconds);
        RecalculationCycles = info.GetOrDefault("recalculationCycles", DefaultRecalculationCycles);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets flag that determines if current and voltage phase estimates, frequency and dF/dt should be
    /// calculated for PoW data.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("Determines if current and voltage phase estimates, frequency and dF/dt should be calculated for PoW data.")]
    [DefaultValue(DefaultCalculatePhaseEstimates)]
    public bool CalculatePhaseEstimates { get; set; }

    /// <summary>
    /// Gets or sets the nominal <see cref="LineFrequency"/> of this SEL CWS device.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("Configured nominal frequency for SEL CWS device.")]
    [DefaultValue(typeof(LineFrequency), "Hz60")]
    public LineFrequency NominalFrequency { get; set; }

    /// <summary>
    /// Gets or sets the configured frame rate for phase estimate calculations.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("Configured frame rate for phase estimate calculations.")]
    [DefaultValue(Common.DefaultFramePerSecond)]
    public ushort CalculationFrameRate { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if last value should be repeated when down-sampling, i.e.,
    /// when <see cref="CalculationFrameRate"/> is less than SEL CWS frame rate (commonly 3000Hz);
    /// otherwise <see cref="Double.NaN"/> will be used.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description(
        "Gets or sets flag that determines if last value should be repeated when down-sampling, i.e.," +
        "when 'CalculationFrameRate' is less than SEL CWS frame rate (commonly 3000Hz);" +
        "otherwise 'NaN' will be used."
    )]
    [DefaultValue(DefaultRepeatLastCalculatedValueWhenDownSampling)]
    public bool RepeatLastCalculatedValueWhenDownSampling { get; set; }

    /// <summary>
    /// Gets or sets the reference channel for frequency tracking.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("Reference channel for frequency tracking.")]
    [DefaultValue(typeof(PhaseChannel), "VA")]
    public PhaseChannel ReferenceChannel { get; set; }

    /// <summary>
    /// Gets or sets the number of nominal cycles contained in the sliding DFT analysis window.
    /// </summary>
    /// <remarks>
    /// Larger values generally reduce noise/jitter (more averaging) but increase latency and reduce step response.
    /// </remarks>
    [Category("Phase Estimation Parameters")]
    [Description("Number of nominal cycles contained in the sliding DFT analysis window. Larger values generally reduce noise/jitter (more averaging) but increase latency and reduce step response.")]
    [DefaultValue(DefaultTargetCycles)]
    public int TargetCycles { get; set; }

    /// <summary>
    /// Gets or sets a flag that determines if interval averaging (boxcar averaging) is enabled across each publish interval when down-sampling.
    /// </summary>
    /// <remarks>
    /// Down-sampling without an anti-alias / low-pass step will preserve high-rate jitter and can alias higher-frequency
    /// content into the published stream. Interval averaging acts as a simple, cheap low-pass filter that reduces
    /// jitter and improves published stability.
    /// </remarks>
    [Category("Phase Estimation Parameters")]
    [Description("Enables interval averaging (boxcar averaging) across each publish interval when down-sampling. Down-sampling without an anti-alias / low-pass step will preserve high-rate jitter and can alias higher-frequency content into the published stream. Interval averaging acts as a simple, cheap low-pass filter that reduces jitter and improves published stability.")]
    [DefaultValue(DefaultEnableIntervalAveraging)]
    public bool EnableIntervalAveraging { get; set; }

    /// <summary>
    /// Gets or sets a flag that determines if an additional exponential moving average (EMA) is applied to the published stream (after interval averaging).
    /// </summary>
    /// <remarks>
    /// Interval averaging removes high-rate noise; publish-EMA further reduces remaining jitter and produces a "calm"
    /// display or control signal. This is usually the most intuitive "knob" for operators/consumers because it acts on
    /// the actual output cadence.
    /// </remarks>
    [Category("Phase Estimation Parameters")]
    [Description("Enables an additional exponential moving average (EMA) applied to the published stream (after interval averaging). Interval averaging removes high-rate noise; publish-EMA further reduces remaining jitter and produces a calm display or control signal.")]
    [DefaultValue(DefaultEnablePublishEMA)]
    public bool EnablePublishEMA { get; set; }

    /// <summary>
    /// Gets or sets the EMA time constant τ (seconds) for published phase angles.
    /// </summary>
    /// <remarks>
    /// Angles are circular quantities; this implementation performs wrap-safe smoothing by operating on unit vectors
    /// (cos/sin) rather than naïvely averaging radians. This avoids discontinuities at ±π.
    /// </remarks>
    [Category("Phase Estimation Parameters")]
    [Description("EMA time constant τ (seconds) for published phase angles. Angles are circular quantities; this implementation performs wrap-safe smoothing by operating on unit vectors (cos/sin) rather than naïvely averaging radians.")]
    [DefaultValue(DefaultPublishAnglesTauSeconds)]
    public double PublishAnglesTauSeconds { get; set; }

    /// <summary>
    /// Gets or sets the EMA time constant τ (seconds) for published RMS magnitudes.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("EMA time constant τ (seconds) for published RMS magnitudes.")]
    [DefaultValue(DefaultPublishMagnitudesTauSeconds)]
    public double PublishMagnitudesTauSeconds { get; set; }

    /// <summary>
    /// Gets or sets the EMA time constant τ (seconds) for published frequency.
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("EMA time constant τ (seconds) for published frequency.")]
    [DefaultValue(DefaultPublishFrequencyTauSeconds)]
    public double PublishFrequencyTauSeconds { get; set; }

    /// <summary>
    /// Gets or sets the EMA time constant τ (seconds) for published ROCOF (dF/dt).
    /// </summary>
    /// <remarks>
    /// ROCOF is effectively a derivative signal and is typically much noisier than frequency; it generally benefits from
    /// heavier smoothing (larger τ) than frequency.
    /// </remarks>
    [Category("Phase Estimation Parameters")]
    [Description("EMA time constant τ (seconds) for published ROCOF (dF/dt). ROCOF is effectively a derivative signal and is typically much noisier than frequency; it generally benefits from heavier smoothing (larger τ) than frequency.")]
    [DefaultValue(DefaultPublishRocofTauSeconds)]
    public double PublishRocofTauSeconds { get; set; }

    /// <summary>
    /// Gets or sets the EMA time constant τ (seconds) for the internal per-sample frequency smoothing that occurs inside the estimator before any down-sampling/publish filtering.
    /// </summary>
    /// <remarks>
    /// When interval averaging + publish EMA are enabled, this can be relatively light. If you disable publish smoothing,
    /// you may want to increase this τ.
    /// </remarks>
    [Category("Phase Estimation Parameters")]
    [Description("EMA time constant τ (seconds) for the internal per-sample frequency smoothing that occurs inside the estimator before any down-sampling/publish filtering. When interval averaging + publish EMA are enabled, this can be relatively light.")]
    [DefaultValue(DefaultSampleFrequencyTauSeconds)]
    public double SampleFrequencyTauSeconds { get; set; }

    /// <summary>
    /// Gets or sets the EMA time constant τ (seconds) for the internal per-sample ROCOF smoothing (computed from the internally smoothed frequency).
    /// </summary>
    [Category("Phase Estimation Parameters")]
    [Description("EMA time constant τ (seconds) for the internal per-sample ROCOF smoothing (computed from the internally smoothed frequency).")]
    [DefaultValue(DefaultSampleRocofTauSeconds)]
    public double SampleRocofTauSeconds { get; set; }

    /// <summary>
    /// Gets or sets the number of nominal cycles between full DFT recalculations for numerical stability.
    /// </summary>
    /// <remarks>
    /// Sliding DFT updates are O(1) per sample but can accumulate numerical drift; periodic full recomputation
    /// re-anchors the phasor sums.
    /// </remarks>
    [Category("Phase Estimation Parameters")]
    [Description("Number of nominal cycles between full DFT recalculations for numerical stability. Sliding DFT updates are O(1) per sample but can accumulate numerical drift; periodic full recomputation re-anchors the phasor sums.")]
    [DefaultValue(DefaultRecalculationCycles)]
    public int RecalculationCycles { get; set; }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        // Serialize connection parameters
        info.AddValue("calculatePhaseEstimates", CalculatePhaseEstimates);
        info.AddValue("nominalFrequency", NominalFrequency, typeof(LineFrequency));
        info.AddValue("calculationFrameRate", CalculationFrameRate);
        info.AddValue("repeatLastCalculatedValueWhenDownSampling", RepeatLastCalculatedValueWhenDownSampling);
        info.AddValue("referenceChannel", ReferenceChannel, typeof(PhaseChannel));
        info.AddValue("targetCycles", TargetCycles);
        info.AddValue("enableIntervalAveraging", EnableIntervalAveraging);
        info.AddValue("enablePublishEMA", EnablePublishEMA);
        info.AddValue("publishAnglesTauSeconds", PublishAnglesTauSeconds);
        info.AddValue("publishMagnitudesTauSeconds", PublishMagnitudesTauSeconds);
        info.AddValue("publishFrequencyTauSeconds", PublishFrequencyTauSeconds);
        info.AddValue("publishRocofTauSeconds", PublishRocofTauSeconds);
        info.AddValue("sampleFrequencyTauSeconds", SampleFrequencyTauSeconds);
        info.AddValue("sampleRocofTauSeconds", SampleRocofTauSeconds);
        info.AddValue("recalculationCycles", RecalculationCycles);
    }

    #endregion
}