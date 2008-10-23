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
    /// Provides a function signature for methods that produce an amplitude representing the
    /// acoustic pressure of a represented musical timbre for the given time.
    /// </summary>
    /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
    /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
    /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
    /// <param name="sampleRate">Number of samples per second.</param>
    /// <returns>The amplitude of the represented musical timbre at the given time.</returns>
    public delegate double TimbreFunction(double frequency, long sampleIndex, long samplePeriod, int sampleRate);

    /// <summary>
    /// Defines a few timbre functions.
    /// </summary>
    public static class Timbre
	{
        /// <summary>
        /// Computes the angular frequency for the given time.
        /// </summary>
        /// <param name="frequency">Frequency in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The computed angular frequency in radians per second at given time.</returns>
        public static double AngularFrequency(double frequency, long sampleIndex, double sampleRate)
        {
            // 2 PI f t
            //      f = Frequency (Hz)
            //      t = period    (Seconds)

            return (2 * Math.PI * frequency) * (sampleIndex / sampleRate);
        }

        /// <summary>
        /// Generates a pure tone for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a pure tone at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// pure tone of the given frequency for the given time.
        /// </remarks>
        public static double PureTone(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return Math.Sin(AngularFrequency(frequency, sampleIndex, sampleRate));
        }

        /// <summary>
        /// Generates a basic note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a basic note at the given time.</returns>
        /// <remarks>
        /// <para>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// basic note of the given frequency for the given time.
        /// </para>
        /// <para>
        /// </para>
        /// This timbre algorithm combines both the simulated clarinet and odd harmonic series
        /// algoriths to produce a pleasant sounding note.
        /// </remarks>
        public static double BasicNote(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return (Timbre.SimulatedClarinet(frequency, sampleIndex, samplePeriod, sampleRate) + Timbre.OddHarmonicSeries(frequency, sampleIndex, samplePeriod, sampleRate)) / 2;
        }

        /// <summary>
        /// Generates a simulated clarinet note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a simulated clarinet note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// simulated clarinet note of the given frequency for the given time.
        /// </remarks>
        public static double SimulatedClarinet(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            double wt, r1;

            wt = AngularFrequency(frequency, sampleIndex, sampleRate);

            // Simulated Clarinet equation - see http://www.phy.mtu.edu/~suits/clarinet.html
            // s(t) = sin(wt) + 0.75 *      sin(3 * wt) + 0.5 *      sin(5 * wt) + 0.14 *      sin(7 * wt) + 0.5 *      sin(9 * wt) + 0.12 *      sin(11 * wt) + 0.17 *      sin(13 * wt)
            r1 = Math.Sin(wt) + 0.75 * Math.Sin(3 * wt) + 0.5 * Math.Sin(5 * wt) + 0.14 * Math.Sin(7 * wt) + 0.5 * Math.Sin(9 * wt) + 0.12 * Math.Sin(11 * wt) + 0.17 * Math.Sin(13 * wt);

            return r1;
        }

        /// <summary>
        /// Generates a simulated organ note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a simulated clarinet note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a second-order
        /// square wave harmonic series (i.e., Sin(f) + Sin(3f)/3) of the given frequency for the
        /// given time to simulate an organ sound.
        /// </remarks>
        public static double SimulatedOrgan(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return ComputeHarmonicSeries(frequency, sampleIndex, sampleRate, 1, 3);
        }

        /// <summary>
        /// Generates an odd harmonic series for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a simulated clarinet note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of an
        /// odd harmonic series of the given frequency for the given time.
        /// Algorithm: Sin(f) + Sin(3f)/3 + Sin(5f)/5, etc.
        /// </remarks>
        public static double OddHarmonicSeries(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return ComputeHarmonicSeries(frequency, sampleIndex, sampleRate, 1, 25);
        }

        /// <summary>
        /// Generates an even harmonic series for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a simulated clarinet note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of an
        /// even harmonic series of the given frequency for the given time.
        /// Algorithm: Sin(2f) + Sin(4f)/3 + Sin(6f)/5, etc.
        /// </remarks>
        public static double EvenHarmonicSeries(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return ComputeHarmonicSeries(frequency, sampleIndex, sampleRate, 2, 25);
        }

        // Computes a basic harmonic series
        private static double ComputeHarmonicSeries(double frequency, long sampleIndex, double sampleRate, int offset, int order)
        {
            double wt, r1 = 0.0D;

            wt = AngularFrequency(frequency, sampleIndex, sampleRate);

            // Generate harmonic series
            for (int x = offset; x <= order; x += 2)
            {
                r1 += Math.Sin(x * wt) / (x - offset + 1);
            }

            return r1;
        }
    }
}
