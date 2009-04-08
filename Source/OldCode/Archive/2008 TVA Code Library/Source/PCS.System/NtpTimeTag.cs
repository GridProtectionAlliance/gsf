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
    /// <summary>Standard Network Time Protocol Timetag.</summary>
    public class NtpTimeTag : TimeTagBase
    {
        // NTP dates are measured as the number of seconds since 1/1/1900, so we calculate this
        // date to get offset in ticks for later conversion.
        private static long m_ntpDateOffsetTicks = (new DateTime(1900, 1, 1, 0, 0, 0)).Ticks;

        /// <summary>
        /// Creates a new <see cref="NtpTimeTag"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected NtpTimeTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given number of seconds since 1/1/1900.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1900.</param>
        public NtpTimeTag(double seconds)
            : base(m_ntpDateOffsetTicks, seconds)
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given number of seconds since 1/1/1900.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1900.</param>
        public NtpTimeTag(int seconds)
            : base(m_ntpDateOffsetTicks, (double)seconds)
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given number of seconds and fractional seconds since 1/1/1900.</summary>
        /// <param name="seconds">Number of seconds since 1/1/1900.</param>
        /// <param name="fractionalSeconds">Fractional seconds.</param>
        [CLSCompliant(false)]
        public NtpTimeTag(int seconds, uint fractionalSeconds)
            : base(m_ntpDateOffsetTicks, seconds + (fractionalSeconds / (double)uint.MaxValue))
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given 64-bit NTP timestamp.</summary>
        /// <param name="timestamp">NTP timestamp containing number of seconds since 1/1/1900 in hi-word and fractional seconds in lo-word.</param>
        public NtpTimeTag(long timestamp)
            : this((int)(((ulong)timestamp >> 32) & 0xffffffffL), (uint)(timestamp & 0xffffffffL))
        {
        }

        /// <summary>Creates a new <see cref="NtpTimeTag"/>, given specified <see cref="Ticks"/>.</summary>
        /// <param name="timestamp">Timestamp in <see cref="Ticks"/> to create Unix timetag from (minimum valid date is 1/1/1900).</param>
        public NtpTimeTag(Ticks timestamp)
            : base(m_ntpDateOffsetTicks, timestamp)
        {
        }
    }
}