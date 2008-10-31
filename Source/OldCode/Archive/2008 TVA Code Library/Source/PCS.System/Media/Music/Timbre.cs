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
    /// <returns>The amplitude of the represented musical timbre (a value between zero and one) at the given time.</returns>
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
        /// This timbre algorithm combines the simulated piano and the odd harmonic series
        /// algoriths to produce a pleasant sounding note.
        /// </remarks>
        public static double BasicNote(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return (Timbre.SimulatedPiano(frequency, sampleIndex, samplePeriod, sampleRate) +
                    Timbre.OddHarmonicSeries(frequency, sampleIndex, samplePeriod, sampleRate)) / 2.0D;
        }

        /// <summary>
        /// Generates a simulated piano note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a simulated piano note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// simulated piano note of the given frequency for the given time.
        /// </remarks>
        public static double SimulatedPiano(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return (
                (70.2137233659572D * Math.Cos(AngularFrequency(frequency * 0.72337962962963D, sampleIndex, sampleRate) + 2.15595560508634D)) +
                (32.6386857925095D * Math.Cos(AngularFrequency(frequency * 1.44675925925926D, sampleIndex, sampleRate) - 1.34187877930484D)) +
                (6.46251265801616D * Math.Cos(AngularFrequency(frequency * 2.17013888888889D, sampleIndex, sampleRate) - 2.94209422121373D)) +
                (12.7763287253431D * Math.Cos(AngularFrequency(frequency * 2.89351851851852D, sampleIndex, sampleRate) + 2.43247608525533D)) +
                (26.8756025924165D * Math.Cos(AngularFrequency(frequency * 3.61689814814815D, sampleIndex, sampleRate) + 3.06242942291676D)) +
                (16.7595425274989D * Math.Cos(AngularFrequency(frequency * 4.34027777777778D, sampleIndex, sampleRate) - 1.0881672416834D)) +
                (8.61450310871165D * Math.Cos(AngularFrequency(frequency * 5.06365740740741D, sampleIndex, sampleRate) - 3.01123327342246D)) +
                (6.76189590128027D * Math.Cos(AngularFrequency(frequency * 5.78703703703704D, sampleIndex, sampleRate) + 1.64626744449415D)) +
                (4.11732226561935D * Math.Cos(AngularFrequency(frequency * 6.51041666666667D, sampleIndex, sampleRate) + 0.603920626612006D))) / 185.220116937353D;
        }

        /// <summary>
        /// Generates a simulated guitar note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a simulated guitar note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// simulated guitar note of the given frequency for the given time.
        /// </remarks>
        public static double SimulatedGuitar(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            double scalar = 0.6535;

            return (
                (4992.08306927842D * Math.Cos(AngularFrequency(frequency * scalar * 1.04513052784467D, sampleIndex, sampleRate) - 2.15457792574594D)) +
                (2863.27648044261D * Math.Cos(AngularFrequency(frequency * scalar * 2.3515436876505D, sampleIndex, sampleRate) + 0.574588814876299D)) +
                (2884.59276926048D * Math.Cos(AngularFrequency(frequency * scalar * 3.39667421549516D, sampleIndex, sampleRate) - 1.87865666086637D)) +
                (508.674162219158D * Math.Cos(AngularFrequency(frequency * scalar * 5.74821790314566D, sampleIndex, sampleRate) + 0.95341246662564D)) +
                (256.23136163587D * Math.Cos(AngularFrequency(frequency * scalar * 9.40617475060199D, sampleIndex, sampleRate) + 2.11778291327275D)) +
                (348.457894218248D * Math.Cos(AngularFrequency(frequency * scalar * 10.4513052784467D, sampleIndex, sampleRate) + 0.303677151680579D))) / 11853.3157370548D;

        }

        /// <summary>
        /// Generates a simulated bass guitar note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="sampleIndex">Sample index (represents time anywhere from zero to full length of song).</param>
        /// <param name="samplePeriod">If useful, total period for note in whole samples per second (i.e., seconds of time * <paramref name="sampleRate"/>) over which to compute timbre.</param>
        /// <param name="sampleRate">Number of samples per second.</param>
        /// <returns>The amplitude for a simulated bass guitar note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// simulated bass guitar note of the given frequency for the given time.
        /// </remarks>
        public static double SimulatedBassGuitar(double frequency, long sampleIndex, long samplePeriod, int sampleRate)
        {
            return (
                (16107.474214089D * Math.Cos(AngularFrequency(frequency * 0.485239173642166D, sampleIndex, sampleRate) + 0.878364865481147D)) +
                (14087.0088730627D * Math.Cos(AngularFrequency(frequency * 0.970478347284333D, sampleIndex, sampleRate) - 2.98645760242543D)) +
                (8735.01788701509D * Math.Cos(AngularFrequency(frequency * 1.4557175209265D, sampleIndex, sampleRate) - 0.395796372471435D)) +
                (3973.79454412557D * Math.Cos(AngularFrequency(frequency * 1.94095669456867D, sampleIndex, sampleRate) + 2.23661099705174D)) +
                (1102.84347421078D * Math.Cos(AngularFrequency(frequency * 2.42619586821083D, sampleIndex, sampleRate) - 1.38641414143595D)) +
                (162.235485574335D * Math.Cos(AngularFrequency(frequency * 2.911435041853D, sampleIndex, sampleRate) - 0.458018924851457D))) / 44168.3744780774D;
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
            r1 = (Math.Sin(wt) + 0.75 * Math.Sin(3 * wt) + 0.5 * Math.Sin(5 * wt) + 0.14 * Math.Sin(7 * wt) + 0.5 * Math.Sin(9 * wt) + 0.12 * Math.Sin(11 * wt) + 0.17 * Math.Sin(13 * wt)) / 3.18D;

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
        /// harmonic series approaching a square wave (i.e., Sin(f) + Sin(3f)/3) of the given
        /// frequency for the given time to simulate an organ sound.
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
            return ComputeHarmonicSeries(frequency, sampleIndex, sampleRate, 2, 26);
        }

        // Computes a basic harmonic series
        private static double ComputeHarmonicSeries(double frequency, long sampleIndex, double sampleRate, int offset, int order)
        {
            double wt, r1 = 0.0D, total = 0.0D;
            int divisor;

            wt = AngularFrequency(frequency, sampleIndex, sampleRate);

            // Generate harmonic series
            for (int x = offset; x <= order; x += 2)
            {
                divisor = (x - offset + 1);
                r1 += Math.Sin(x * wt) / divisor;
                total += divisor;
            }

            // Evenly distribute the series between 0 and 1
            return r1 / total;
        }
    }
}
