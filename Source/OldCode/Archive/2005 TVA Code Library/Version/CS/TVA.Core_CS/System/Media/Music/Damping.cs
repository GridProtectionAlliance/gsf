/**************************************************************************\
   Copyright (c) 2008 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

using System;

namespace System.Media.Music
{
    /// <summary>
    /// Provides a function signature for methods that damp an amplitude representing a
    /// lowering of the acoustic pressure over time.
    /// </summary>
    /// <param name="sample">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
    /// <param name="samplePeriod">Total period, in whole samples per second (i.e., time * SamplesPerSecond), over which to perform damping.</param>
    /// <returns>Scaling factor used to damp an amplitude at the given sample index.</returns>
    public delegate double DampingFunction(long sample, long samplePeriod);
    
    public static class Damping
	{
        /// <summary>
        /// Produces a damping signature that represents no damping over time.
        /// </summary>
        /// <param name="sample">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., time * SamplesPerSecond), over which to perform damping.</param>
        /// <returns>Returns a scalar of 1.0 regardless to time.</returns>
        /// <remarks>
        /// Zero damped sounds would be produced by synthetic sources such as an electronic keyboard.
        /// </remarks>
        public static double Zero(long sample, long samplePeriod)
        {
            return 1.0D;
        }

        /// <summary>
        /// Produces a natural damping curve similar to that of a piano - slowly damping over
        /// time until the key is released at which point the string is quickly damped.
        /// </summary>
        /// <param name="sample">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., time * SamplesPerSecond), over which to perform damping.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        /// <remarks>
        /// This damping algorithm combines both the linear and logarithmic damping algoriths to
        /// produce a more natural damping curve.
        /// </remarks>
        public static double Natural(long sample, long samplePeriod)
        {
            return (Logarithmic(sample, samplePeriod) + Linear(sample, samplePeriod)) / 2;
        }
        /// <summary>
        /// Produces a logarithmic damping curve - slowly damping with a sharp end from 1 to 0 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sample">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., time * SamplesPerSecond), over which to perform damping.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        public static double Logarithmic(long sample, long samplePeriod)
        {
            return Math.Log10(samplePeriod - sample) / Math.Log10(samplePeriod);
        }

        /// <summary>
        /// Produces a linear damping curve - damping with a perfect slope from 1 to 0 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sample">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., time * SamplesPerSecond), over which to perform damping.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        public static double Linear(long sample, long samplePeriod)
        {
            return (samplePeriod - sample) * (1.0D / samplePeriod);
        }
    }
}
