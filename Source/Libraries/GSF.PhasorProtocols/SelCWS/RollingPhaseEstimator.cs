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

using System;
using GSF.Units;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.SelCWS;

internal enum TimestampMode
{
    // Center alignment: "phasor-correct"
    Center,
    // End alignment: "stream-instant"
    End
}

internal struct PhaseEstimate
{
    public double Frequency;    // Smoothed frequency estimate in hertz
    public Angle[] Angles;      // Angles, length 6: IA, IB, IC, VA, VB, VC
    public long Timestamp;      // Nanoseconds timestamp aligned per TimestampMode
    public int WindowSamples;   // N used for this estimate
}

/// <summary>
/// Represents a rolling six-phase estimator.
/// </summary>
internal sealed class RollingPhaseEstimator
{
    #region [ Members ]

    // Nested Types

    // Constants
    private const double TwoThirds = 2.0D / 3.0D;
    private const double Half = 0.5D;
    private const double Rt32 = 0.86602540378443864676D; // sqrt(3)/2
    private const double TwoPI = Math.PI * 2.0D;

    // Fields
    private readonly double m_sampleRate;
    private readonly double m_alpha;    // EMA smoothing for ω (0..1)
    private readonly TimestampMode m_timestampMode;

    private readonly int m_targetCycles;

    private int m_windowSamples;    // current window length (samples), cycle-locked
    private int m_cap;              // ring capacity (>= m_windowSamples)
    private int m_head;             // ring head index
    private int m_count;            // number of valid entries in ring

    // Running sums for synchronous detection: Σ x·cosθ, Σ x·sinθ
    private readonly double[] m_sumC = new double[6];
    private readonly double[] m_sumS = new double[6];

    // Ring buffers holding per-sample contributions for quick subtract
    private double[] m_rc0, m_rc1, m_rc2, m_rc3, m_rc4, m_rc5;
    private double[] m_rs0, m_rs1, m_rs2, m_rs3, m_rs4, m_rs5;

    // Frequency tracking
    private double[] m_phase;
    private double m_phiCurrent;    // current unwrapped phase
    private double m_prevTheta = double.NaN;
    private double m_omegaFiltered;
    private bool m_omegaInit;

    // Timestamp calculation
    private readonly double m_samplePeriodNs; // double to avoid overflow
    private long m_lastEpochNs;

    #endregion

    #region [ Constructors ]

    public RollingPhaseEstimator(
        double sampleRateHz,
        LineFrequency nominalFrequency,
        int targetCycles = 5,
        TimestampMode timestampMode = TimestampMode.Center,
        double smoothing = 0.15,
        int maxCyclesForCapacity = 12) // pre-allocate a comfortable upper bound
    {
        if (sampleRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRateHz));

        if (targetCycles is < 2 or > 30)
            throw new ArgumentOutOfRangeException(nameof(targetCycles));

        if (smoothing is <= 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(smoothing));

        if (maxCyclesForCapacity < targetCycles)
            throw new ArgumentOutOfRangeException(nameof(maxCyclesForCapacity));

        m_sampleRate = sampleRateHz;
        m_alpha = smoothing;
        m_timestampMode = timestampMode;
        m_targetCycles = targetCycles;
        m_samplePeriodNs = 1e9 / m_sampleRate;

        // initial window: assume nominal Hz to bootstrap
        int spc0 = (int)Math.Round(m_sampleRate / (int)nominalFrequency);

        // Future: these are sane bounds for 3kS/s (target use case), but should be made adjustable
        m_windowSamples = Clamp(targetCycles * spc0, 60, 2000); 

        m_cap = Clamp(maxCyclesForCapacity * spc0, m_windowSamples, 10000);

        AllocateRings(m_cap);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets the current window length in samples.
    /// </summary>
    public int WindowSamples => m_windowSamples;

    /// <summary>
    /// Gets the current filtered frequency estimate in hertz.
    /// </summary>
    /// <remarks>
    /// Returns <c>NaN</c> if window not yet full.
    /// </remarks>
    public double CurrentFrequency => m_omegaInit ? m_omegaFiltered / TwoPI : double.NaN;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Push one interleaved sample-group (IA,IB,IC,VA,VB,VC) with its epoch nanoseconds.
    /// Returns <c>true</c> when an estimate is available, i.e., window has filled.
    /// </summary>
    public bool Step(double ia, double ib, double ic, double va, double vb, double vc, long epochNanoseconds, out PhaseEstimate estimate)
    {
        estimate = default;
        m_lastEpochNs = epochNanoseconds;

        // --- Voltage space-vector angle θ (Clarke αβ) ---
        double alpha = TwoThirds * (va - Half * vb - Half * vc);
        double beta = TwoThirds * (Rt32 * (vb - vc));
        double theta = Math.Atan2(beta, alpha);

        // --- Unwrap phase and maintain global ϕ(k) ---
        if (double.IsNaN(m_prevTheta))
        {
            // first sample: initialize unwrapped phase
            m_phiCurrent = 0.0D;
        }
        else
        {
            double dtheta = theta - m_prevTheta;

            switch (dtheta)
            {
                case > Math.PI:
                    dtheta -= TwoPI;
                    break;
                case < -Math.PI:
                    dtheta += TwoPI;
                    break;
            }

            m_phiCurrent += dtheta; // global unwrapped phase
        }

        m_prevTheta = theta;

        double c = Math.Cos(theta);
        double s = Math.Sin(theta);

        // --- Contributions for this sample ---
        double c0 = ia * c, s0 = ia * s;
        double c1 = ib * c, s1 = ib * s;
        double c2 = ic * c, s2 = ic * s;
        double c3 = va * c, s3 = va * s;
        double c4 = vb * c, s4 = vb * s;
        double c5 = vc * c, s5 = vc * s;

        // --- If window is full, pop oldest (subtract its contributions) ---
        if (m_count == m_windowSamples)
        {
            int idx = m_head;
            SubtractOld(idx);
            m_head = (m_head + 1) % m_cap;
            m_count--; // will re-increment below
        }

        // --- Ensure capacity in case of later window growth ---
        if (m_count == m_cap)
            GrowCapacity(m_cap * 2);

        // --- Push new contributions and phase into ring ---
        int write = (m_head + m_count) % m_cap;

        m_rc0[write] = c0; m_rs0[write] = s0; m_sumC[0] += c0; m_sumS[0] += s0;
        m_rc1[write] = c1; m_rs1[write] = s1; m_sumC[1] += c1; m_sumS[1] += s1;
        m_rc2[write] = c2; m_rs2[write] = s2; m_sumC[2] += c2; m_sumS[2] += s2;
        m_rc3[write] = c3; m_rs3[write] = s3; m_sumC[3] += c3; m_sumS[3] += s3;
        m_rc4[write] = c4; m_rs4[write] = s4; m_sumC[4] += c4; m_sumS[4] += s4;
        m_rc5[write] = c5; m_rs5[write] = s5; m_sumC[5] += c5; m_sumS[5] += s5;

        m_phase[write] = m_phiCurrent;

        m_count++;

        // --- Update frequency estimate from whole-window phase change ---
        if (m_count >= 2)
        {
            int oldest = m_head;
            int newest = (m_head + m_count - 1) % m_cap;

            double dphi = m_phase[newest] - m_phase[oldest];
            double totalTimeSec = (m_count - 1) / m_sampleRate;

            if (totalTimeSec > 0.0D)
            {
                double omegaRaw = dphi / totalTimeSec; // rad/s over window

                if (m_omegaInit)
                {
                    m_omegaFiltered = (1.0D - m_alpha) * m_omegaFiltered + m_alpha * omegaRaw;
                }
                else
                {
                    m_omegaFiltered = omegaRaw;
                    m_omegaInit = true;
                }
            }
        }

        // --- Cycle-lock window length to current f̂ (after updating ω) ---
        if (m_omegaInit)
            RetuneWindow();

        // Not enough samples yet?
        if (m_count < m_windowSamples)
            return false;

        // --- Build result ---
        double frequency = m_omegaFiltered / TwoPI;

        Angle[] angles = new Angle[6];

        for (int k = 0; k < 6; k++)
        {
            Angle phi = Math.Atan2(-m_sumS[k], m_sumC[k]); // radians
            angles[k] = phi.ToRange(-Math.PI, false).ToDegrees();
        }

        long timestamp = m_timestampMode == TimestampMode.End
            ? m_lastEpochNs
            : CenterTimestampNs();

        estimate = new PhaseEstimate
        {
            Frequency = frequency,
            Angles = angles,    // IA, IB, IC, VA, VB, VC
            Timestamp = timestamp,
            WindowSamples = m_windowSamples
        };

        return true;
    }

    private void RetuneWindow()
    {
        // clamp f̂ to [30, 90] Hz to be sane
        double fhat = Math.Max(30.0D, Math.Min(90.0D, m_omegaFiltered / TwoPI));
        int spc = (int)Math.Round(m_sampleRate / fhat);
        int desired = Clamp(m_targetCycles * spc, 90, 4000);

        if (desired == m_windowSamples)
            return;

        m_windowSamples = desired;

        // If window shrank, drop oldest until count <= window
        while (m_count > m_windowSamples)
        {
            SubtractOld(m_head);
            m_head = (m_head + 1) % m_cap;
            m_count--;
        }

        // If window grew, just wait for natural filling. Ensure capacity.
        if (m_cap < m_windowSamples)
            GrowCapacity(Math.Max(m_windowSamples, m_cap * 2));
    }

    private void SubtractOld(int idx)
    {
        m_sumC[0] -= m_rc0[idx]; m_sumS[0] -= m_rs0[idx];
        m_sumC[1] -= m_rc1[idx]; m_sumS[1] -= m_rs1[idx];
        m_sumC[2] -= m_rc2[idx]; m_sumS[2] -= m_rs2[idx];
        m_sumC[3] -= m_rc3[idx]; m_sumS[3] -= m_rs3[idx];
        m_sumC[4] -= m_rc4[idx]; m_sumS[4] -= m_rs4[idx];
        m_sumC[5] -= m_rc5[idx]; m_sumS[5] -= m_rs5[idx];
    }

    private long CenterTimestampNs()
    {
        // center offset = ((N-1)/2) * Ts
        double half = (m_count - 1) * 0.5D * m_samplePeriodNs;
        double t = m_lastEpochNs - half;

        // round to nearest integer ns
        return (long)Math.Round(t);
    }

    private static int Clamp(int v, int lo, int hi)
    {
        return v < lo ? lo : v > hi ? hi : v;
    }

    private void AllocateRings(int capacity)
    {
        m_rc0 = new double[capacity]; m_rs0 = new double[capacity];
        m_rc1 = new double[capacity]; m_rs1 = new double[capacity];
        m_rc2 = new double[capacity]; m_rs2 = new double[capacity];
        m_rc3 = new double[capacity]; m_rs3 = new double[capacity];
        m_rc4 = new double[capacity]; m_rs4 = new double[capacity];
        m_rc5 = new double[capacity]; m_rs5 = new double[capacity];

        m_phase = new double[capacity];
    }

    private void GrowCapacity(int newCap)
    {
        // Copy current ring into new contiguous [0.._count)
        double[] nc = new double[newCap]; double[] ns = new double[newCap];
        CopyRing(m_rc0, nc); m_rc0 = nc; CopyRing(m_rs0, ns); m_rs0 = ns;

        nc = new double[newCap]; ns = new double[newCap];
        CopyRing(m_rc1, nc); m_rc1 = nc; CopyRing(m_rs1, ns); m_rs1 = ns;

        nc = new double[newCap]; ns = new double[newCap];
        CopyRing(m_rc2, nc); m_rc2 = nc; CopyRing(m_rs2, ns); m_rs2 = ns;

        nc = new double[newCap]; ns = new double[newCap];
        CopyRing(m_rc3, nc); m_rc3 = nc; CopyRing(m_rs3, ns); m_rs3 = ns;

        nc = new double[newCap]; ns = new double[newCap];
        CopyRing(m_rc4, nc); m_rc4 = nc; CopyRing(m_rs4, ns); m_rs4 = ns;

        nc = new double[newCap]; ns = new double[newCap];
        CopyRing(m_rc5, nc); m_rc5 = nc; CopyRing(m_rs5, ns); m_rs5 = ns;

        // phase ring
        double[] pNew = new double[newCap];
        CopyRing(m_phase, pNew);
        m_phase = pNew;

        m_cap = newCap;
        m_head = 0; // reset ring indexing (we’ve packed at start)
    }

    private void CopyRing(double[] src, double[] dst)
    {
        if (m_count == 0)
            return;

        int tailCount = Math.Min(m_count, src.Length - m_head);
        Array.Copy(src, m_head, dst, 0, tailCount);

        if (m_count > tailCount)
            Array.Copy(src, 0, dst, tailCount, m_count - tailCount);
    }

    // Future: change target cycles on the fly
    //public void SetTargetCycles(int cycles)
    //{
    //    if (cycles is < 2 or > 30)
    //        throw new ArgumentOutOfRangeException(nameof(cycles));

    //    m_targetCycles = cycles;

    //    // Re-tune immediately with current f̂ if available
    //    if (m_omegaInit)
    //        RetuneWindow();
    //}

    #endregion
}
