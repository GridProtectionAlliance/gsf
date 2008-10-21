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
    /// Defines the tempo of song as the total number of note values in one minute.
    /// </summary>
    /// <remarks>
    /// This defined tempo of the music assigns absolute durations to all the
    /// note values within a score.
    /// </remarks>
    public class Tempo
    {
        #region [ Members ]

        // Fields
        private int m_totalNoteValues;
        private int m_noteValue;
        private double m_referenceTime;

        #endregion

        #region [ Constructors ]

        public Tempo(int totalNoteValues, NoteValue noteValue)
        {
            m_totalNoteValues = totalNoteValues;
            m_noteValue = (int)noteValue;
        }

        public Tempo(int totalNoteValues, NoteValueBritish noteValue)
        {
            m_totalNoteValues = totalNoteValues;
            m_noteValue = (int)noteValue;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Total number of reference note values that occur in one minute - thus defining
        /// the tempo for a score.
        /// </summary>
        public int TotalNoteValues
        {
            get
            {
                return m_totalNoteValues;
            }
            set
            {
                m_totalNoteValues = value;
                CalculateReferenceTime();
            }
        }

        /// <summary>Get or sets the note value, expressed in American form, representing the length of the note.</summary>
        public NoteValue NoteValue
        {
            get
            {
                return (NoteValue)m_noteValue;
            }
            set
            {
                m_noteValue = (int)value;
                CalculateReferenceTime();
            }
        }

        /// <summary>Get or sets the note value, expressed in British form, representing the length of the note.</summary>
        public NoteValueBritish NoteValueBritish
        {
            get
            {
                return (NoteValueBritish)m_noteValue;
            }
            set
            {
                m_noteValue = (int)value;
                CalculateReferenceTime();
            }
        }

        /// <summary>
        /// Total time, in seconds, for a single reference note value.  For example, if tempo
        /// is M.M. 120 quarter-notes per minute, then time is referenced in quarter-notes and
        /// this function would return 0.5 seconds.
        /// </summary>
        public double ReferenceTime
        {
            get
            {
                return m_referenceTime;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calculates the actual time duration, in seconds, for the given tempo that the specified
        /// source note value will last. For example, if tempo is M.M. 120 quarter-notes per minute,
        /// then each quarter-note would last a half-second.
        /// </summary>
        /// <param name="source">Source note value.</param>
        /// <param name="dots">Total dotted note length extensions to apply.</param>
        /// <returns>Actual duration of note value in seconds.</returns>
        public double CalculateNoteValueTime(NoteValue source, int dots)
        {
            return m_referenceTime * source.Duration(this.NoteValue, dots);
        }

        /// <summary>
        /// Calculates the actual time duration, in seconds, for the given tempo that the specified
        /// source note value will last. For example, if tempo is M.M. 120 crotchets per minute,
        /// then each crotchet would last a half-second.
        /// </summary>
        /// <param name="source">Source note value.</param>
        /// <param name="dots">Total dotted note length extensions to apply.</param>
        /// <returns>Actual duration of note value in seconds.</returns>
        public double CalculateNoteValueTime(NoteValueBritish source, int dots)
        {
            return m_referenceTime * source.Duration(this.NoteValueBritish, dots);
        }

        private void CalculateReferenceTime()
        {
            m_referenceTime = m_totalNoteValues / 60.0D * this.NoteValue.Duration();
        }

        #endregion
    }
}
