//*******************************************************************************************************
//  Note.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/07/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Reflection;

namespace System.Media.Music
{
    /// <summary>
    /// Provides a function signature for methods that produce an amplitude representing the
    /// acoustic pressure of a represented musical timbre for the given time.
    /// </summary>
    /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
    /// <param name="time">Time in seconds.</param>
    /// <returns>The amplitude of the represented musical timbre at the given time.</returns>
    public delegate double TimbreFunction(double frequency, double time);

    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// This example creates an in-memory wave file and adds notes to create a basic musical scale:
    /// <code>
    /// using System;
    /// using System.Media;
    /// using System.Media.Music;
    ///
    /// static class Program
    /// {
    ///     /// <summary>
    ///     /// The main entry point for the application.
    ///     /// </summary>
    ///     [STAThread]
    ///     static void Main()
    ///     {
    ///         WaveFile waveFile = new WaveFile(SampleRate.Hz8000, BitsPerSample.Bits16, DataChannels.Mono);            
    ///         TimbreFunction timbre = Note.BasicNote;             // Set the musical timbre
    ///         double amplitude = 0.25D * short.MaxValue;          // Set volume to 25% of maximum
    ///         double seconds = 4.0D;                              // Set length of wave file in seconds
    ///         double samplesPerSecond = waveFile.SampleRate;      // Gets the defined sample rate
    ///         double samplePeriod = seconds * samplesPerSecond;   // Compute total sample period
    ///         int totalNotes = 8;                                 // Total notes to traverse
    ///         string noteID = Note.MiddleC;                       // Start note at middle C
    ///         double frequency = Note.GetNoteFrequency(noteID);   // Get frequency for middle C
    ///         double time;                                        // Time index
    ///
    ///         for (int sample = 0; sample &lt; samplePeriod; sample++)
    ///         {
    ///             // Change notes at even intervals within the sample period
    ///             if (sample &gt; 0 &amp;&amp; (sample % (samplePeriod / totalNotes)) == 0)
    ///             {
    ///                 noteID = Note.GetNextNoteID(noteID, false);
    ///                 frequency = Note.GetNoteFrequency(noteID);
    ///             }
    ///
    ///             // Compute time index of the current sample
    ///             time = sample / samplesPerSecond;
    ///
    ///             waveFile.AddBlock((short)(timbre(frequency, time) * amplitude));
    ///         }
    ///
    ///         waveFile.Play();
    ///
    ///         Console.ReadKey();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class Note
    {
        // Fundamental musical note frequencies (http://www.phy.mtu.edu/~suits/notefreqs.html)
        public const double C0 = 16.35;
        public const double C0S = 17.32;
        public const double D0 = 18.35;
        public const double D0S = 19.45;
        public const double E0 = 20.6;
        public const double F0 = 21.83;
        public const double F0S = 23.12;
        public const double G0 = 24.5;
        public const double G0S = 25.96;
        public const double A0 = 27.5;
        public const double A0S = 29.14;
        public const double B0 = 30.87;
        public const double C1 = 32.7;
        public const double C1S = 34.65;
        public const double D1 = 36.71;
        public const double D1S = 38.89;
        public const double E1 = 41.2;
        public const double F1 = 43.65;
        public const double F1S = 46.25;
        public const double G1 = 49.0;
        public const double G1S = 51.91;
        public const double A1 = 55.0;
        public const double A1S = 58.27;
        public const double B1 = 61.74;
        public const double C2 = 65.41;
        public const double C2S = 69.3;
        public const double D2 = 73.42;
        public const double D2S = 77.78;
        public const double E2 = 82.41;
        public const double F2 = 87.31;
        public const double F2S = 92.5;
        public const double G2 = 98.0;
        public const double G2S = 103.83;
        public const double A2 = 110.0;
        public const double A2S = 116.54;
        public const double B2 = 123.47;
        public const double C3 = 130.81;
        public const double C3S = 138.59;
        public const double D3 = 146.83;
        public const double D3S = 155.56;
        public const double E3 = 164.81;
        public const double F3 = 174.61;
        public const double F3S = 185.0;
        public const double G3 = 196.0;
        public const double G3S = 207.65;
        public const double A3 = 220.0;
        public const double A3S = 233.08;
        public const double B3 = 246.94;
        public const double C4 = 261.63;    // Middle C
        public const double C4S = 277.18;
        public const double D4 = 293.66;
        public const double D4S = 311.13;
        public const double E4 = 329.63;
        public const double F4 = 349.23;
        public const double F4S = 369.99;
        public const double G4 = 392.0;
        public const double G4S = 415.3;
        public const double A4 = 440.0;
        public const double A4S = 466.16;
        public const double B4 = 493.88;
        public const double C5 = 523.25;
        public const double C5S = 554.37;
        public const double D5 = 587.33;
        public const double D5S = 622.25;
        public const double E5 = 659.26;
        public const double F5 = 698.46;
        public const double F5S = 739.99;
        public const double G5 = 783.99;
        public const double G5S = 830.61;
        public const double A5 = 880.0;
        public const double A5S = 932.33;
        public const double B5 = 987.77;
        public const double C6 = 1046.5;
        public const double C6S = 1108.73;
        public const double D6 = 1174.66;
        public const double D6S = 1244.51;
        public const double E6 = 1318.51;
        public const double F6 = 1396.91;
        public const double F6S = 1479.98;
        public const double G6 = 1567.98;
        public const double G6S = 1661.22;
        public const double A6 = 1760.0;
        public const double A6S = 1864.66;
        public const double B6 = 1975.53;
        public const double C7 = 2093.0;
        public const double C7S = 2217.46;
        public const double D7 = 2349.32;
        public const double D7S = 2489.02;
        public const double E7 = 2637.02;
        public const double F7 = 2793.83;
        public const double F7S = 2959.96;
        public const double G7 = 3135.96;
        public const double G7S = 3322.44;
        public const double A7 = 3520.0;
        public const double A7S = 3729.31;
        public const double B7 = 3951.07;
        public const double C8 = 4186.01;
        public const double C8S = 4434.92;
        public const double D8 = 4698.64;
        public const double D8S = 4978.03;

        /// <summary>Note ID for "Middle C"</summary>
        public const string MiddleC = "C4";

        /// <summary>
        /// Gets the specified note frequency.
        /// </summary>
        /// <param name="noteID">ID of the note to retrieve - expected format is "Note + Octave + S?" (e.g., A2 or C5S)</param>
        /// <returns>The specified note.</returns>
        /// <exception cref="ArgumentNullException">noteID is null.</exception>
        /// <exception cref="ArgumentException">Invalid note ID format - expected "Note + Octave + S?" (e.g., A2 or C5S).</exception>
        public static double GetNoteFrequency(string noteID)
        {
            noteID = ValidateNoteID(noteID);
            return GetNoteFrequency(noteID[0], int.Parse(noteID[1].ToString()), noteID.Length > 2 && noteID[2] == 'S' ? true : false);
        }

        /// <summary>
        /// Gets the specified note frequency.
        /// </summary>
        /// <param name="note">Note (A - G) to retrieve.</param>
        /// <param name="octave">Octave of the the note to retrieve (0 - 8).</param>
        /// <param name="sharp">Indicates to get the "sharp" version of the note.</param>
        /// <returns>The specified note.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Notes must be A - G, octaves must be 0 - 8, first note is C0, last note is D8S.</exception>
        /// <exception cref="ArgumentException">Sharps are not defined for notes 'B' and 'E'.</exception>
        public static double GetNoteFrequency(char note, int octave, bool sharp)
        {
            if (note < 'A' || note > 'G')
                throw new ArgumentOutOfRangeException("note", "Note must be A - G");

            if (octave < 0 || octave > 8)
                throw new ArgumentOutOfRangeException("ocatve", "Octave must be between 0 and 8");

            if (octave == 8 && (note < 'C' || note > 'D'))
                throw new ArgumentOutOfRangeException("note", "Maximum note defined for octave 8 is \'D#\'");

            if (sharp && (note == 'B' || note == 'E'))
                throw new ArgumentException("Sharps are not defined for notes \'B\' and \'E\'");

            return (double)typeof(Note).GetField(string.Format("{0}{1}{2}", note, octave, sharp ? "S" : "")).GetRawConstantValue();
        }

        /// <summary>
        /// Gets the next note ID in sequence after the specified note ID.
        /// </summary>
        /// <param name="noteID">ID of the current note - expected format is "Note + Octave + S?" (e.g., A2 or C5S)</param>
        /// <param name="includeSharps">Set to True to include sharp notes in the sequence.</param>
        /// <returns>The specified note.</returns>
        /// <exception cref="ArgumentNullException">noteID is null.</exception>
        /// <exception cref="ArgumentException">Invalid note ID format - expected "Note + Octave + S?" (e.g., A2 or C5S).</exception>
        public static string GetNextNoteID(string noteID, bool includeSharps)
        {
            noteID = ValidateNoteID(noteID);

            char note = noteID[0];
            int octave = int.Parse(noteID[1].ToString());
            bool sharp = (noteID.Length > 2 && noteID[2] == 'S' ? true : false);

            // Transition to next octave after each B note
            if (note == 'B')
                octave++;

            // Include sharp notes if requested
            if (includeSharps && !sharp && note != 'B' && note != 'E')
            {
                sharp = true;
            }
            else
            {
                sharp = false;

                // Transition to next note frequency
                if (note == 'G')
                    note = 'A';
                else
                    note++;
            }

            return string.Format("{0}{1}{2}", note, octave, sharp ? "S" : "");
        }

        private static string ValidateNoteID(string noteID)
        {
            if (noteID == null)
                throw new ArgumentNullException("noteID");

            if (noteID.Length < 2)
                throw new ArgumentException("Invalid note ID format - expected \"Note + Octave + S?\" (e.g., A2 or C5S)");

            return noteID.ToUpper();
        }

        // Timbre functions

        /// <summary>
        /// Generates a pure tone for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="time">Time in seconds.</param>
        /// <returns>The amplitude for a pure tone at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// pure tone of the given frequency for the given time.
        /// </remarks>
        public static double PureTone(double frequency, double time)
        {
            return Math.Sin(AngularFrequency(frequency, time));
        }

        /// <summary>
        /// Generates a basic note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="time">Time in seconds.</param>
        /// <returns>The amplitude for a basic note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// basic note of the given frequency for the given time.
        /// </remarks>
        public static double BasicNote(double frequency, double time)
        {
            double wt, r1, r2;

            wt = AngularFrequency(frequency, time);
            r1 = Math.Sin(wt) + 0.75 * Math.Sin(3 * wt);
            r2 = Math.Sin(wt);

            return r1 + r2;
        }

        /// <summary>
        /// Generates a simulated clarinet note for the given frequency and time.
        /// </summary>
        /// <param name="frequency">Fundamental frequency of the desired note in Hz.</param>
        /// <param name="time">Time in seconds.</param>
        /// <returns>The amplitude for a simulated clarinet note at the given time.</returns>
        /// <remarks>
        /// This method computes an amplitude representing the acoustic pressure of a
        /// simulated clarinet note of the given frequency for the given time.
        /// </remarks>
        public static double SimulatedClarinet(double frequency, double time)
        {
            double wt, r1;

            wt = AngularFrequency(frequency, time);

            // Simulated Clarinet equation
            // s(t) = sin(w1t) + 0.75 *      sin(3 * w1t) + 0.5 *      sin(5 * w1t) + 0.14 *      sin(7 * w1t) + 0.5 *      sin(9 * w1t) + 0.12 *      sin(11 * w1t) + 0.17 *      sin(13 * w1t)
            r1 = Math.Sin(wt) + 0.75 * Math.Sin(3 * wt) + 0.5 * Math.Sin(5 * wt) + 0.14 * Math.Sin(7 * wt) + 0.5 * Math.Sin(9 * wt) + 0.12 * Math.Sin(11 * wt) + 0.17 * Math.Sin(13 * wt);

            return r1;
        }

        /// <summary>
        /// Computes the angular frequency for the given time.
        /// </summary>
        /// <param name="frequency">Frequency in Hz.</param>
        /// <param name="time">Time in seconds.</param>
        /// <returns>The computed angular frequency in radians per second at given time.</returns>
        public static double AngularFrequency(double frequency, double time)
        {
            // 2 PI f t
            //      f = Frequency (Hz)
            //      t = period    (Seconds)

            return (2 * Math.PI * frequency) * time;
        }
    }
}
