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
using System.Media;
using System.Linq;
using System.Collections.Generic;

namespace System.Media.Music
{
    /// <summary>
    /// Allows creation of a synthesized musical score storing the resultant song in an
    /// in-memory wave file for play back or saving music to disk.
    /// </summary>
    public class Song : WaveFile
    {
        #region [ Members ]

        // Fields
        private MeasureSize m_measureSize;
        private TimbreFunction m_timbre;
        private DampingFunction m_damping;
        private Tempo m_tempo;
        private List<Note> m_noteQueue;
        private double m_dynamic;
        private double m_interNoteDelay;
        private long m_currentSample;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new song with a 3/4 measure size, a tempo of 240 quarter-notes per minute,
        /// mezzo-forte prevailing dynamic level, using a basic note timbre and standard CD-quality
        /// settings for the underlying sound file.
        /// </summary>
        public Song()
        {
            m_measureSize = new MeasureSize(3, NoteValue.Quarter);
            m_tempo = new Tempo(240, NoteValue.Quarter);
            m_dynamic = (double)Dynamic.MezzoForte / 100.0D;
            m_timbre = Note.BasicNote;
            m_damping = Note.NaturalDamping;
            m_noteQueue = new List<Note>();
            m_interNoteDelay = 0.01D;
        }

        /// <summary>
        /// Creates a new song with a 3/4 measure size, a tempo of 240 quarter-notes per minute,
        /// mezzo-forte prevailing dynamic level, using a basic note timbre and the specified
        /// audio format settings.
        /// </summary>
        /// <param name="sampleRate">Desired sample rate</param>
        /// <param name="bitsPerSample">Desired bits-per-sample</param>
            /// <param name="channels">Desired data channels</param>
        public Song(SampleRate sampleRate, BitsPerSample bitsPerSample, DataChannels channels)
            : base(sampleRate, bitsPerSample, channels)
        {
            m_measureSize = new MeasureSize(3, NoteValue.Quarter);
            m_tempo = new Tempo(240, NoteValue.Quarter);
            m_dynamic = (double)Dynamic.MezzoForte / 100.0D;
            m_timbre = Note.BasicNote;
            m_damping = Note.NaturalDamping;
            m_noteQueue = new List<Note>();
            m_interNoteDelay = 0.01D;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets current measure size or defines a new measure size for the song.
        /// </summary>
        public MeasureSize MeasureSize
        {
            get
            {
                return m_measureSize;
            }
            set
            {
                m_measureSize = value;
            }
        }

        /// <summary>
        /// Gets current tempo or defines a new tempo for the song.
        /// </summary>
        public Tempo Tempo
        {
            get
            {
                return m_tempo;
            }
            set
            {
                m_tempo = value;
            }
        }

        /// <summary>
        /// Gets or sets the default tibre function used to synthesize the sounds
        /// of the added notes (i.e., the instrument). This timbre function will be
        /// used if no other function is specified when adding notes.
        /// </summary>
        public TimbreFunction Timbre
        {
            get
            {
                return m_timbre;
            }
            set
            {
                m_timbre = value;
            }
        }

        /// <summary>
        /// Gets or sets the default damping function used to lower the sound volume
        /// of the added notes over time. This damping function will be used if no
        /// other function is specified when adding notes.
        /// </summary>
        public DampingFunction Damping
        {
            get
            {
                return m_damping;
            }
            set
            {
                m_damping = value;
            }
        }

        /// <summary>
        /// Gets or sets the prevailing dynamic (i.e., volume) for the song.  Individual notes
        /// can choose to override this dynamic.
        /// </summary>
        public Dynamic Dynamic
        {
            get
            {
                // Dynamic can be custom, so return closest match...
                int dynamic = (int)m_dynamic * 100;

                if (dynamic <= (int)Dynamic.Pianissimo)
                {
                    return Dynamic.Pianissimo;
                }
                else if (dynamic <= (int)Dynamic.Piano)
                {
                    return Dynamic.Piano;
                }
                else if (dynamic <= (int)Dynamic.MezzoPiano)
                {
                    return Dynamic.MezzoPiano;
                }
                else if (dynamic <= (int)Dynamic.MezzoForte)
                {
                    return Dynamic.MezzoForte;
                }
                else if (dynamic <= (int)Dynamic.Forte)
                {
                    return Dynamic.Forte;
                }
                else
                {
                    return Dynamic.Fortissimo;
                }
            }
            set
            {
                if (value == Dynamic.Undefined)
                    value = Dynamic.MezzoForte;

                m_dynamic = (double)value / 100.0D;
            }
        }

        /// <summary>
        /// Gets or sets the prevailing dynamic (i.e., volume) expressed as percentage in the range
        /// of 0 to 1 for the song. Individual notes can choose to override this dynamic.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Value must be expressed as a fractional percentage between zero and one.
        /// </exception>
        public double CustomDynamic
        {
            get
            {
                return m_dynamic;
            }
            set
            {
                if (value != -1.0D && (value < 0.0D || value > 1.0D))
                    throw new ArgumentOutOfRangeException("CustomDynamic", "Value must be expressed as a fractional percentage between zero and one.");

                if (value == -1.0D)
                    m_dynamic = (double)Dynamic.MezzoForte / 100.0D;
                else
                    m_dynamic = value;
            }
        }

        /// <summary>Defines the rest time, in seconds, to pause between notes.</summary>
        public double InterNoteDelay
        {
            get
            {
                return m_interNoteDelay;
            }
            set
            {
                m_interNoteDelay = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Add a predefined phrase of notes to the song.
        /// </summary>
        /// <param name="phrase">Phrase to add.</param>
        public void AddPhrase(Phrase phrase)
        {
            foreach (Note[] notes in phrase.Notes)
            {
                AddNotes(notes);
            }
        }

        /// <summary>
        /// Add a series of notes to the song.
        /// </summary>
        /// <param name="notes">Notes to add.</param>
        public void AddNotes(params Note[] notes)
        {
            double samplesPerSecond = SampleRate;
            long samplePeriod;

            // Calculate note value times for each note to be added            
            foreach (Note note in notes)
            {
                note.CalculateNoteValueTime(m_tempo);
                note.StartTime = m_currentSample / samplesPerSecond;
                note.EndTime = note.StartTime + note.NoteValueTime - (1 / samplesPerSecond);
            }

            // Assign sample period to note with shortest duration - all other notes
            // will remain in queue until they have completed their run
            samplePeriod = (long)(notes.Min(note => note.NoteValueTime) * samplesPerSecond);

            // Add notes to note queue
            m_noteQueue.AddRange(notes);

            // Add queued notes to song for given sample period
            AddQueuedNotesToSong(samplePeriod);
        }

        /// <summary>
        /// Called when there are no more notes to add.
        /// </summary>
        /// <remarks>
        /// This flushes the remaining queued notes into the song allowing them to run their remaining time.
        /// </remarks>
        public void Finish()
        {
            if (m_noteQueue.Count > 0)
            {
                double samplesPerSecond = SampleRate;
                long samplePeriod;

                // Assign sample period to note with longest duration - this makes sure all remaining
                // queued notes will complete their run
                samplePeriod = (long)(m_noteQueue.Max(note => note.NoteValueTime) * samplesPerSecond);

                // Add queued notes to song for given sample period
                AddQueuedNotesToSong(samplePeriod);
            }
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        public void AddRest(NoteValue restLength)
        {
            AddNotes(new Note { NoteValue = restLength });
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        public void AddRest(NoteValueBritish restLength)
        {
            AddNotes(new Note { NoteValueBritish = restLength });
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        /// <param name="dots">Total dotted note length extensions to apply.</param>
        public void AddRest(NoteValue restLength, int dots)
        {
            AddNotes(new Note { NoteValue = restLength, Dots = dots });
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        /// <param name="dots">Total dotted note length extensions to apply.</param>
        public void AddRest(NoteValueBritish restLength, int dots)
        {
            AddNotes(new Note { NoteValueBritish = restLength, Dots = dots });
        }

        /// <summary>
        /// Starts crescendo dynamic over the range of the specified number of beats.
        /// </summary>
        /// <param name="totalBeats">Total number of beats overwhich to gradually increase volume.</param>
        /// <param name="endDynamic">Desired volume when crescendo is complete.</param>
        /// <remarks>
        /// Current <see cref="Dynamic"/> is the starting dynamic which should be less than <paramref name="endDynamic"/>.
        /// </remarks>
        public void SetCrescendoDynamic(int totalBeats, Dynamic endDynamic)
        {
            // TODO: implement crescendo dynamic
        }

        /// <summary>
        /// Starts diminuendo dynamic over the range of the specified number of beats.
        /// </summary>
        /// <param name="totalBeats">Total number of beats overwhich to gradually decrease volume.</param>
        /// <param name="endDynamic">Desired volume when diminuendo is complete.</param>
        /// <remarks>
        /// Current <see cref="Dynamic"/> is the starting dynamic which should be greater than <paramref name="endDynamic"/>.
        /// </remarks>
        public void SetDiminuendoDynamic(int totalBeats, Dynamic endDynamic)
        {
            // TODO: implement diminuendo dynamic
        }

        private void AddQueuedNotesToSong(long samplePeriod)
        {
            List<int> completedNotes = new List<int>();
            TimbreFunction timbre;
            DampingFunction damping;
            double samplesPerSecond = SampleRate;
            double sample, dynamic, time;
            Note note;

            for (long sampleIndex = m_currentSample; sampleIndex < m_currentSample + samplePeriod + (long)(m_interNoteDelay * samplesPerSecond); sampleIndex++)
            {
                // Compute time index in seconds of the current sample
                time = sampleIndex / samplesPerSecond;

                // Create summation of all notes at given time
                sample = 0.0D;

                for (int index = 0; index < m_noteQueue.Count; index++)
                {
                    if (!completedNotes.Contains(index))
                    {
                        note = m_noteQueue[index];

                        if (time < note.EndTime)
                        {
                            // Get note dynamic
                            dynamic = (note.Dynamic == Dynamic.Undefined ? m_dynamic : note.CustomDynamic);

                            // Get timbre function
                            timbre = (note.Timbre == null ? m_timbre : timbre = note.Timbre);

                            // Get damping function
                            damping = (note.Damping == null ?  m_damping : note.Damping);

                            // Generate note at given time
                            sample += timbre(note.Frequency, time) * (dynamic * damping(sampleIndex - m_currentSample, samplePeriod));
                        }
                        else
                        {
                            completedNotes.Add(index);
                        }
                    }
                }

                AddSample(sample * AmplitudeScalar);
            }

            m_currentSample += samplePeriod;

            // Remove completed notes from queue - removal is in reverse sorted
            // order to preserve indices
            completedNotes.Sort();
            completedNotes.Reverse();

            for (int x = 0; x < completedNotes.Count; x++)
            {
                m_noteQueue.RemoveAt(completedNotes[x]);
            }
        }

        #endregion
    }
}
