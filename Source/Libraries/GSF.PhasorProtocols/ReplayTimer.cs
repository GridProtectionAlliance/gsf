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
/// This class uses a combination of coarse sleeping, yielding, and spinning for
/// accurate wait times with minimal CPU usage. Supports common frame rates, e.g.,
/// 30 or 60, and very high frame rates, e.g., 3000.
/// </remarks>
public sealed class ReplayTimer
{
    private readonly long m_periodTicks;    // Query Performance Counter (QPC) ticks per frame
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
    public void WaitNext()
    {
        m_nextTick += m_periodTicks;

        while (true)
        {
            long now = Stopwatch.GetTimestamp();
            long ticksRemaining = m_nextTick - now;

            if (ticksRemaining <= 0L)
                return;

            // Convert to milliseconds for coarse decisions
            double remaining = ticksRemaining * 1000.0 / Stopwatch.Frequency;

            switch (remaining)
            {
                case >= 2.0D:
                    // Coarse sleep -- 1ms is the finest useful Sleep on Windows
                    Thread.Sleep(1);
                    break;
                case >= 0.3D:
                    // Yield to reduce CPU but avoid oversleeping
                    Thread.Yield();
                    break;
                default:
                    // Just spin to hit sub-millisecond cadence
                    SpinWait sw = new();
                        
                    while (Stopwatch.GetTimestamp() < m_nextTick)
                        sw.SpinOnce();
                        
                    return;
            }
        }
    }
}