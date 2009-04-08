/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll
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

using System.Runtime.Serialization;

namespace System
{
    /// <summary>Represents a standard Unix timetag.</summary>
    public class UnixTimeTag : TimeTagBase
    {
        // Unix dates are measured as the number of seconds since 1/1/1970, so this class calculates this
        // date to get the offset in ticks for later conversion.
        private static long m_unixDateOffsetTicks = (new DateTime(1970, 1, 1, 0, 0, 0)).Ticks;

        /// <summary>
        /// Creates a new <see cref="UnixTimeTag"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected UnixTimeTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>Creates a new <see cref="UnixTimeTag"/>, given number of seconds since 1/1/1970.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1970.</param>
        public UnixTimeTag(double seconds)
            : base(m_unixDateOffsetTicks, seconds)
        {
        }

        /// <summary>Creates a new <see cref="UnixTimeTag"/>, given number of seconds since 1/1/1970.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1970.</param>
        public UnixTimeTag(int seconds)
            : base(m_unixDateOffsetTicks, (double)seconds)
        {
        }

        /// <summary>Creates a new <see cref="UnixTimeTag"/>, given specified <see cref="Ticks"/>.</summary>
        /// <param name="timestamp">Timestamp in <see cref="Ticks"/> to create Unix timetag from (minimum valid date is 1/1/1970).</param>
        public UnixTimeTag(Ticks timestamp)
            : base(m_unixDateOffsetTicks, timestamp)
        {
        }
    }
}