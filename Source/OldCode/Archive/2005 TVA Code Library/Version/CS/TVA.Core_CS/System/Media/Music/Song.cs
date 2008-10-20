/**************************************************************************\
   Copyright (c) 2008, James Ritchie Carroll
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
        private Tempo m_tempo;
        private List<Note> m_noteQueue;
        private double m_dynamic;
        private int m_beat;
        private long m_totalBeats;
        private long m_time;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new song with a 3/4 measure size, a tempo of 120 quarter-notes per minute,
        /// mezzo-forte prevailing dynamic level, using a basic note timbre and standard CD-quality
        /// settings for the underlying sound file.
        /// </summary>
        public Song()
        {
            m_measureSize = new MeasureSize(3, NoteValue.Quarter);
            m_tempo = new Tempo(120, NoteValue.Quarter);
            m_dynamic = (double)Dynamic.MezzoForte / 100.0D;
            m_timbre = Note.BasicNote;
            m_noteQueue = new List<Note>();
        }

        /// <summary>
        /// Creates a new song with a 3/4 measure size, a tempo of 120 quarter-notes per minute,
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
            m_tempo = new Tempo(120, NoteValue.Quarter);
            m_dynamic = (double)Dynamic.MezzoForte / 100.0D;
            m_timbre = Note.BasicNote;
            m_noteQueue = new List<Note>();
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
        /// of the added notes (i.e., the instrument), this timbre function will be
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

        /// <summary>
        /// Returns the index of the current beat (0 to Measure.Beats - 1) within the current measure.
        /// </summary>
        public int Beat
        {
            get
            {
                return m_beat;
            }
        }

        /// <summary>
        /// Returns the index of the current measure.
        /// </summary>
        /// <remarks>
        /// This is calculated based on the total number of beats added so far, note that
        /// if the beats per measure is changed - the offset of the measure will change
        /// based on this value.
        /// </remarks>
        public long MeasureIndex
        {
            get
            {
                return (long)Math.Truncate(m_totalBeats / (double)m_measureSize.Beats);
            }
        }

        /// <summary>
        /// Returns the total number of beats added to the song so far.
        /// </summary>
        public long TotalBeats
        {
            get
            {
                return m_totalBeats;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Add a series of notes, all to be played at the specified beat in the current measure.
        /// </summary>
        /// <param name="notes">Notes to add.</param>
        /// <remarks>
        /// Beat within the measure will be automatically advanced - you should add all needed
        /// notes at the current beat during this call.
        /// </remarks>
        public void AddNotes(params Note[] notes)
        {
            // Increment beat, start new measure if needed
            if (++m_beat == m_measureSize.Beats)
                m_beat = 0;

            // Track total number of beats processed so far
            m_totalBeats++;

            // Because of the way the note values are enumerated, the note value with
            // the smallest index will actually be the one with the longest length
            NoteValue maxLength = notes.Min(note => note.NoteValue);

            foreach (Note note in notes)
            {
                // Validate note length for remaining time in measure
                m_measureSize.ValidateNoteValueAtBeat(note.NoteValue, m_beat);

                // Calculate note value times for each note to be added
                note.CalculateNoteValueTime(m_tempo);

                // Note the beat at which this note was added
                note.Beat = m_beat;
            }

            // Add notes to note queue
            m_noteQueue.AddRange(notes);

            // Create a loop over all notes for this beat
            for (int x = 0; x < m_noteQueue.Count; x++)
            {
                
            }
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        public void AddRest(NoteValue restLength)
        {
            AddNotes(new Note(0.0D, restLength));
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        public void AddRest(NoteValueBritish restLength)
        {
            AddNotes(new Note(0.0D, restLength));
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

        #endregion
    }
}
