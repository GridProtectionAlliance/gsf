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
    /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
    /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
    /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
    /// <returns>Scaling factor in the range of zero to one used to damp an amplitude at the given sample index.</returns>
    public delegate double DampingFunction(long sampleIndex, long samplePeriod, int sampleRate);

    /// <summary>
    /// Defines a few damping functions.
    /// </summary>
    public static class Damping
	{
        /// <summary>
        /// Produces a natural damping curve very similar to that of a string based instrument - strong at
        /// first and damping quickly over time from 1 to 0 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
        /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        /// <remarks>
        /// This damping algorithm combines both the logarithmic and linear damping algoriths to
        /// produce a very natural damping curve.
        /// </remarks>
        public static double Natural(long sampleIndex, long samplePeriod, int sampleRate)
        {
            return (Logarithmic(sampleIndex, samplePeriod, sampleRate) + 0.25D * Linear(sampleIndex, samplePeriod, sampleRate)) / 1.25D;
        }

        /// <summary>
        /// Produces a logarithmic damping curve - strong at first and damping quickly over time from 1 to 0 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
        /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        /// <remarks>
        /// This damping would be similar to that of a note produced by a string based instrument.
        /// </remarks>
        public static double Logarithmic(long sampleIndex, long samplePeriod, int sampleRate)
        {
            return 1 - Math.Log10(sampleIndex + 1) / Math.Log10(samplePeriod);
        }

        /// <summary>
        /// Produces an inverse logarithmic damping curve - slowly damping with a sharp end from 1 to 0 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
        /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        /// <remarks>
        /// This damping would be similar to that of a note produced on an electronic keyboard or a breath based instrument.
        /// </remarks>
        public static double InverseLogarithmic(long sampleIndex, long samplePeriod, int sampleRate)
        {
            return Math.Log10(samplePeriod - sampleIndex) / Math.Log10(samplePeriod);
        }

        /// <summary>
        /// Produces a linear damping curve - damping with a perfect slope from 1 to 0 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
        /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        public static double Linear(long sampleIndex, long samplePeriod, int sampleRate)
        {
            return (samplePeriod - sampleIndex) * (1.0D / samplePeriod);
        }

        /// <summary>
        /// Produces a reverse linear damping curve - damping with a perfect slope from 0 to 1 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
        /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        /// <remarks>This is just used for an interesting note effect.</remarks>
        public static double ReverseLinear(long sampleIndex, long samplePeriod, int sampleRate)
        {
            return sampleIndex * (1.0D / samplePeriod);
        }

        /// <summary>
        /// Produces a sinusoidal damping curve oscillating from 1 to 0 to 1 over the <paramref name="samplePeriod"/>.
        /// </summary>
        /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
        /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
        /// <returns>Scaling factor used to damp an amplitude at the given time.</returns>
        /// <remarks>This is just used for an interesting note effect.</remarks>
        public static double Sinusoidal(long sampleIndex, long samplePeriod, int sampleRate)
        {
            return Math.Sin(2.0D * Math.PI * (sampleIndex / (double)samplePeriod));
        }

        /// <summary>
        /// Produces a damping signature that represents no damping over time.
        /// </summary>
        /// <param name="sampleIndex">Sample index (0 to <paramref name="samplePeriod"/> - 1).</param>
        /// <param name="samplePeriod">Total period, in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>), over which to perform damping.</param>
        /// <param name="sampleRate">Number of samples per second, if useful for calculation.</param>
        /// <returns>Returns a scalar of 1.0 regardless to time.</returns>
        /// <remarks>
        /// Zero damped sounds would be produced by synthetic sources such as an electronic keyboard.
        /// </remarks>
        public static double Zero(long sampleIndex, long samplePeriod, int sampleRate)
        {
            return 1.0D;
        }
    }
}
