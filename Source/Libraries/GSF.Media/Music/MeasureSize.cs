//******************************************************************************************************
//  MeasureSize.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/29/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/**************************************************************************\
   Copyright © 2009 - J. Ritchie Carroll
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

#endregion

using System;

namespace GSF.Media.Music
{
    /// <summary>
    /// Defines the size of a musical measure as the number of beats per note value.
    /// </summary>
    public class MeasureSize
    {
        #region [ Members ]

        // Fields
        private int m_beats;
        private double m_noteValue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new musical measure defined as the number of beats per note value.
        /// </summary>
        /// <param name="beats">A <see cref="int"/> representing the beats.</param>
        /// <param name="noteValue">A <see cref="double"/> representing the note value.</param>
        public MeasureSize(int beats, double noteValue)
        {
            m_beats = beats;
            m_noteValue = noteValue;
        }

        /// <summary>
        /// Creates a new musical measure defined as the number of beats per note value.
        /// </summary>
        /// <param name="beats">A <see cref="int"/> representing the beats.</param>
        /// <param name="noteValue">A <see cref="NoteValue"/> representing the note value.</param>
        public MeasureSize(int beats, NoteValue noteValue)
        {
            m_beats = beats;
            m_noteValue = noteValue.Duration();
        }

        /// <summary>
        /// Creates a new musical measure defined as the number of beats per note value.
        /// </summary>
        /// <param name="beats">A <see cref="int"/> representing the beats.</param>
        /// <param name="noteValue">A <see cref="NoteValueBritish"/> representing the note value.</param>
        public MeasureSize(int beats, NoteValueBritish noteValue)
        {
            m_beats = beats;
            m_noteValue = noteValue.Duration();
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
        /// Get or sets the relative note value representing the basic pulse of the music.
        /// </summary>
        public double NoteValue
        {
            get
            {
                return m_noteValue;
            }
            set
            {
                m_noteValue = value;
            }
        }

        /// <summary>
        /// Get or sets the note value, expressed in American form, representing the basic pulse of the music.
        /// </summary>
        public NoteValue NamedNoteValue
        {
            get
            {
                return (NoteValue)Note.NamedValueIndex(m_noteValue);
            }
            set
            {
                m_noteValue = value.Duration();
            }
        }

        /// <summary>
        /// Get or sets the note value, expressed in British form, representing the basic pulse of the music.
        /// </summary>
        public NoteValueBritish NamedNoteValueBritish
        {
            get
            {
                return (NoteValueBritish)Note.NamedValueIndex(m_noteValue);
            }
            set
            {
                m_noteValue = value.Duration();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Validates that given note value will fit within this <see cref="MeasureSize"/> for specified beat.
        /// </summary>
        /// <param name="noteValue">Note value (i.e., length) to validate.</param>
        /// <param name="beat">Beat within in measure where note value is trying to fit.</param>
        public void ValidateNoteValueAtBeat(double noteValue, int beat)
        {
            if (beat < 0 || beat > m_beats - 1)
                throw new ArgumentOutOfRangeException(nameof(beat), "Beat must range from 0 to Measure.Beats - 1");

            if (noteValue > (m_beats - beat) * m_noteValue)
                throw new ArgumentOutOfRangeException(nameof(noteValue), "NoteValue is too large to fit within remaining measure");
        }

        /// <summary>
        /// Validates that given note value will fit within this <see cref="MeasureSize"/> for specified beat.
        /// </summary>
        /// <param name="noteValue">Named note value to validate.</param>
        /// <param name="beat">Beat within in measure where note value is trying to fit.</param>
        /// <param name="dots">Dot length extensions to apply to named note value.</param>
        public void ValidateNoteValueAtBeat(NoteValue noteValue, int beat, int dots)
        {
            ValidateNoteValueAtBeat(noteValue.Duration(dots), beat);
        }

        /// <summary>
        /// Validates that given note value will fit within this <see cref="MeasureSize"/> for specified beat.
        /// </summary>
        /// <param name="noteValue">Named note value to validate.</param>
        /// <param name="beat">Beat within in measure where note value is trying to fit.</param>
        /// <param name="dots">Dot length extensions to apply to named note value.</param>
        public void ValidateNoteValueAtBeat(NoteValueBritish noteValue, int beat, int dots)
        {
            ValidateNoteValueAtBeat(noteValue.Duration(dots), beat);
        }

        #endregion
    }
}