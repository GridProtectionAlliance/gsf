//******************************************************************************************************
//  ReplayTimer.cs - Gbtc
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
//  12/29/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.Threading;

namespace GSF.PhasorProtocols;

/// <summary>
/// Replay timer class used to pace frame publication for file-based playback.
/// </summary>
/// <remarks>
/// <para>
/// Uses absolute deadline advancement to maintain consistent inter-frame intervals.
/// For longer intervals, e.g., 30 FPS / ~33ms, a single bulk <see cref="Thread.Sleep(int)"/>
/// is used for most of the wait, followed by yield/spin for the final approach — minimizing
/// OS scheduler jitter compared to many individual <c>Thread.Sleep(1)</c> calls.
/// </para>
/// <para>
/// Also supports very high frame rates, e.g., 3000 FPS, where the yield/spin path
/// naturally dominates due to sub-millisecond intervals.
/// </para>
/// </remarks>
public sealed class ReplayTimer
{
    // Guard-band in milliseconds: we sleep until this much
    // time remains, then yield/spin to the exact deadline
    private const double SleepGuardBand = 2.0D;

    private readonly long m_periodTicks;    // Query Performance Counter (QPC) ticks per frame
    private readonly long m_guardBandTicks; // QPC ticks for the guard-band
    private long m_nextTick;                // Next scheduled QPC tick (not equal to DateTime ticks)

    /// <summary>
    /// Creates a new <see cref="ReplayTimer"/> instance.
    /// </summary>
    /// <param name="definedFrameRate">The defined frame rate in frames per second.</param>
    public ReplayTimer(int definedFrameRate)
    {
        if (definedFrameRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(definedFrameRate));

        m_periodTicks = (long)Math.Round(Stopwatch.Frequency / (double)definedFrameRate);
        m_guardBandTicks = (long)Math.Round(Stopwatch.Frequency * SleepGuardBand / 1000.0D);
        m_nextTick = Stopwatch.GetTimestamp();

        DefinedFrameRate = definedFrameRate;
    }

    /// <summary>
    /// Gets the defined frame rate for this timer.
    /// </summary>
    public int DefinedFrameRate { get; }

    /// <summary>
    /// Blocks until the next scheduled frame rate interval.
    /// </summary>
    /// <remarks>
    /// The deadline advances by exactly one period from the current deadline (absolute
    /// cadence) to keep inter-frame intervals consistent. If the deadline has already
    /// fallen behind by more than one full period, it resets to "now + period" to
    /// prevent a burst of catch-up frames.
    /// </remarks>
    public void WaitNext()
    {
        // Advance deadline absolutely from previous deadline to maintain
        // a steady cadence regardless of per-frame processing jitter
        long nextTick = m_nextTick + m_periodTicks;

        // If the advanced deadline is already in the past, we've fallen
        // behind by more than a full period — reset to relative mode to
        // avoid a burst of catch-up frames
        if (Stopwatch.GetTimestamp() >= nextTick)
            nextTick = Stopwatch.GetTimestamp() + m_periodTicks;

        m_nextTick = nextTick;

        // Fast exit if already past deadline (e.g., processing took longer than interval)
        if (Stopwatch.GetTimestamp() >= nextTick)
            return;

        long ticksRemaining = nextTick - Stopwatch.GetTimestamp();

        // For longer waits, sleep in one bulk call leaving only a small
        // guard-band for spin/yield — this dramatically reduces the number
        // of OS scheduler interactions compared to repeated Sleep(1) calls
        if (ticksRemaining > m_guardBandTicks)
        {
            long sleepTicks = ticksRemaining - m_guardBandTicks;
            int sleepMs = (int)(sleepTicks * 1000L / Stopwatch.Frequency);

            if (sleepMs > 0)
                Thread.Sleep(sleepMs);
        }

        // Yield / spin for the remaining guard-band to hit precise deadline
        while (true)
        {
            long now = Stopwatch.GetTimestamp();

            if (now >= nextTick)
                return;

            double remaining = (nextTick - now) * 1000.0 / Stopwatch.Frequency;

            if (remaining >= 0.3D)
            {
                // Yield to reduce CPU but avoid oversleeping
                Thread.Yield();
            }
            else
            {
                // Spin to hit sub-millisecond cadence
                SpinWait sw = new();

                while (Stopwatch.GetTimestamp() < nextTick)
                    sw.SpinOnce();

                return;
            }
        }
    }
}