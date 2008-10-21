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

namespace System.Media.Music
{
    /// <summary>
    /// Defines the size of a musical measure as the number of beats per note value.
    /// </summary>
    public class MeasureSize
    {
        #region [ Members ]

        // Fields
        private int m_beats;
        private int m_noteValue;
        private double m_duration;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new musical measure defined as the number of beats per note value.
        /// </summary>
        public MeasureSize(int beats, NoteValue noteValue)
        {
            m_beats = beats;
            m_noteValue = (int)noteValue;
            m_duration = noteValue.Duration();
        }

        /// <summary>
        /// Creates a new musical measure defined as the number of beats per note value.
        /// </summary>
        public MeasureSize(int beats, NoteValueBritish noteValue)
        {
            m_beats = beats;
            m_noteValue = (int)noteValue;
            m_duration = noteValue.Duration();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of beats per measure.
        /// </summary>
        public int Beats
        {
            get
            {
                return m_beats;
            }
            set
            {
                m_beats = value;
            }
        }

        /// <summary>
        /// Get or sets the note value, expressed in American form, representing the basic pulse of the music.
        /// </summary>
        public NoteValue NoteValue
        {
            get
            {
                return (NoteValue)m_noteValue;
            }
            set
            {
                m_noteValue = (int)value;
            }
        }

        /// <summary>
        /// Get or sets the note value, expressed in British form, representing the basic pulse of the music.
        /// </summary>
        public NoteValueBritish NoteValueBritish
        {
            get
            {
                return (NoteValueBritish)m_noteValue;
            }
            set
            {
                m_noteValue = (int)value;
            }
        }

        #endregion

        #region [ Methods ]

        public void ValidateNoteValueAtBeat(NoteValue noteValue, int beat)
        {
            ValidateNoteValueAtBeat(noteValue.Duration(), beat);
        }

        public void ValidateNoteValueAtBeat(NoteValueBritish noteValue, int beat)
        {
            ValidateNoteValueAtBeat(noteValue.Duration(), beat);
        }

        private void ValidateNoteValueAtBeat(double duration, int beat)
        {
            if (beat < 0 || beat > m_beats - 1)
                throw new ArgumentOutOfRangeException("beats", "Beat must range from 0 to Measure.Beats - 1");

            if (duration > (m_beats - beat) * m_duration)
                throw new ArgumentOutOfRangeException("noteValue", "NoteValue is too large to fit within remaining measure");
        }

        #endregion
    }
}