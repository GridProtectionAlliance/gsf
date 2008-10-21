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
using System.Media.Music;

namespace System.Media.Sound
{
    /// <summary>Dual Tone Multi-Frequency Class.</summary>
    /// <example>
    /// This example generates some familiar dual tone multi-frequency sounds
    /// and plays them over the computer's speakers:
    /// <code>
    /// using System;
    /// using System.Media;
    /// using System.Media.Sound;
    ///
    /// static class Program
    /// {
    ///     static void Main()
    ///     {
    ///         WaveFile waveFile = new WaveFile(SampleRate.Hz8000, BitsPerSample.Bits16, DataChannels.Mono);
    ///         double volume = 0.25D;  // Set volume of tones to 25% of maximum
    ///         DTMF tone;
    ///
    ///         // Get the dial tone dual-frequencies
    ///         tone = DTMF.DialTone;
    ///
    ///         // Change the duration of the dial-tone to 3 seconds
    ///         tone.Duration = 3.0D;
    ///
    ///         // Generate a dial tone
    ///         DTMF.Generate(waveFile, tone, volume);
    ///
    ///         // Generate a busy-signal tone, repeat four times
    ///         DTMF.Generate(waveFile, DTMF.BusySignal, volume, 4);
    ///
    ///         // Generate an off-the-hook tone, repeat eight times
    ///         DTMF.Generate(waveFile, DTMF.OffHook, volume, 8);
    ///
    ///         // Get the EBS Alert dual-frequencies
    ///         tone = DTMF.EmergencyBroadcastSystemAlert;
    ///
    ///         // The official duration of an EBS Alert is 22.5 seconds, but
    ///         // the noise is rather annoying - so we set it to 4 seconds
    ///         tone.Duration = 4.0D;
    ///
    ///         // Generate the EBS Alert noise
    ///         DTMF.Generate(waveFile, tone, volume);
    ///
    ///         // Play all the generated tones
    ///         waveFile.Play();
    ///
    ///         Console.ReadKey();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class DTMF
    {
        #region [ Members ]

        // Fields
        public double LowFrequency;
        public double HighFrequency;
        public double Duration;

        #endregion

        #region [ Constructors ]

        public DTMF()
        {
        }

        public DTMF(double lowFrequency, double highFrequency, double duration)
        {
            LowFrequency = lowFrequency;
            HighFrequency = highFrequency;
            Duration = duration;
        }

        #endregion

        #region [ Static ]

        // Static Properties

        /// <summary>Gets the <see cref="DTMF"/> instance representing a telephone dial tone.</summary>
        public static DTMF DialTone
        {
            get
            {
                return new DTMF(350.0D, 440.0D, 1.0D);
            }
        }

        /// <summary>Gets the <see cref="DTMF"/> instances representing a telephone off-the-hook signal.</summary>
        public static DTMF[] OffHook
        {
            get
            {
                return new DTMF[] { new DTMF(1400.0D, 2450.0D, 0.1D), new DTMF(2060.0D, 2600.0D, 0.1D) };
            }
        }

        /// <summary>Gets the <see cref="DTMF"/> instances representing a telephone busy signal.</summary>
        public static DTMF[] BusySignal
        {
            get
            {
                return new DTMF[] { new DTMF(480.0D, 620.0D, 0.5D), new DTMF(0.0D, 0.0D, 0.5D) };
            }
        }

        /// <summary>Gets the <see cref="DTMF"/> instance representing the Emergency Broadcast System alert tone.</summary>
        /// <remarks>The official duration of an EBS Alert is 22.5 seconds.</remarks>
        public static DTMF EmergencyBroadcastSystemAlert
        {
            get
            {
                return new DTMF(853.0D, 960.0D, 22.5D);
            }
        }

        // Static Methods

        /// <summary>
        /// Computes a dual-tone multi-frequency sound for the given <see cref="DTMF"/> information and time.
        /// </summary>
        /// <param name="tone">Instance of the <see cref="DTMF"/> specifying the duration as well as the low and high frequencies of the dual-tone.</param>
        /// <param name="time">Time in seconds.</param>
        /// <returns>The amplitude for the dual-tone at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// <see cref="DTMF"/> of the given frequency for the given time.
        /// </remarks>
        public static double ComputeFrequencies(DTMF tone, double time)
        {
            return Note.PureTone(tone.LowFrequency, time) + Note.PureTone(tone.HighFrequency, time);
        }

        /// <summary>
        /// Generates the specified dual-tone multi-frequency storing it in the specified <see cref="WaveFile"/>.
        /// </summary>
        /// <param name="destination"><see cref="WaveFile"/> used to store generated dual-tone multi-frequencies.</param>
        /// <param name="tone">Dual-tone multi-frequency to generate.</param>
        /// <param name="volume">Volume of generated dual-tones as a percentage (0 to 1).</param>
        public static void Generate(WaveFile destination, DTMF tone, double volume)
        {
            Generate(destination, new DTMF[] { tone }, volume, 1);
        }

        /// <summary>
        /// Generates the specified dual-tone multi-frequency <paramref name="repeatCount"/> times storing it in the specified <see cref="WaveFile"/>.
        /// </summary>
        /// <param name="destination"><see cref="WaveFile"/> used to store generated dual-tone multi-frequencies.</param>
        /// <param name="tone">Dual-tone multi-frequency to generate.</param>
        /// <param name="volume">Volume of generated dual-tones as a percentage (0 to 1).</param>
        /// <param name="repeatCount">Number of times to repeat the tone.</param>
        public static void Generate(WaveFile destination, DTMF tone, double volume, int repeatCount)
        {
            Generate(destination, new DTMF[] { tone }, volume, repeatCount);
        }

        /// <summary>
        /// Generates a single instance of each of the specified dual-tone multi-frequencies storing them in the specified <see cref="WaveFile"/>.
        /// </summary>
        /// <param name="destination"><see cref="WaveFile"/> used to store generated dual-tone multi-frequencies.</param>
        /// <param name="tones">Dual-tone multi-frequencies to generate.</param>
        /// <param name="volume">Volume of generated dual-tones as a percentage (0 to 1).</param>
        public static void Generate(WaveFile destination, DTMF[] tones, double volume)
        {
            Generate(destination, tones, volume, 1);
        }

        /// <summary>
        /// Generates the specified dual-tone multi-frequencies <paramref name="repeatCount"/> times storing them in the specified <see cref="WaveFile"/>.
        /// </summary>
        /// <param name="destination"><see cref="WaveFile"/> used to store generated dual-tone multi-frequencies.</param>
        /// <param name="tones">Dual-tone multi-frequencies to generate.</param>
        /// <param name="volume">Volume of generated dual-tones as a percentage (0 to 1).</param>
        /// <param name="repeatCount">Number of times to repeat each tone.</param>
        /// <exception cref="ArgumentOutOfRangeException">Value must be expressed as a fractional percentage between zero and one.</exception>
        /// <exception cref="InvalidOperationException"><see cref="DTMF"/> only generated for <see cref="WaveFile"/> with a sample rate of 8, 16, 24, 32 or 64 bits per sample.</exception>
        public static void Generate(WaveFile destination, DTMF[] tones, double volume, int repeatCount)
        {
            if (volume < 0.0D || volume > 1.0D)
                throw new ArgumentOutOfRangeException("volume", "Value must be expressed as a fractional percentage between zero and one.");

            short bitsPerSample = destination.BitsPerSample;
            double sampleRate = destination.SampleRate;
            double amplitude, sample;
            LittleBinaryValue[] samples;

            // Calculate sample amplitude factor for given bit size
            switch (bitsPerSample)
            {
                case 8:
                    amplitude = SByte.MaxValue * volume;
                    break;
                case 16:
                    amplitude = Int16.MaxValue * volume;
                    break;
                case 24:
                    amplitude = Int24.MaxValue * volume;
                    break;
                case 32:
                    amplitude = Int32.MaxValue * volume;
                    break;
                case 64:
                    amplitude = Int64.MaxValue * volume;
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Cannot generate DTMF for {0} bits per sample - must be 8, 16, 24, 32 or 64", bitsPerSample));
            }

            // Iterate through each repeat count
            for (int x = 0; x < repeatCount; x++)
            {
                // Interate through each tone
                foreach (DTMF tone in tones)
                {
                    // Iterate through each sample for total DTMF duration
                    for (int y = 0; y < tone.Duration * sampleRate; y++)
                    {
                        // Compute frequencies of DTMF at given time
                        sample = DTMF.ComputeFrequencies(tone, y / sampleRate) * amplitude;

                        // Create a new sample block
                        samples = new LittleBinaryValue[destination.Channels];

                        // Iterate through each channel in WaveFile
                        for (int z = 0; z < destination.Channels; z++)
                        {
                            // Cast sample to appropriate data type based on bit size
                            switch (bitsPerSample)
                            {
                                case 8: // Bytes are unsigned and need 128 byte offset
                                    samples[z] = (Byte)(sample + 128);
                                    break;
                                case 16:
                                    samples[z] = (Int16)sample;
                                    break;
                                case 24:
                                    samples[z] = (Int24)sample;
                                    break;
                                case 32:
                                    samples[z] = (Int32)sample;
                                    break;
                                case 64:
                                    samples[z] = (Int64)sample;
                                    break;
                                default:
                                    break;
                            }
                        }

                        // Add sample block to WaveFile
                        destination.AddBlock(samples);
                    }
                }
            }
        }

        #endregion
    }
}