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

namespace System.Media.Music
{
    /// <summary>
    /// Defines the relative intensity (i.e., volume) of a musical line.
    /// </summary>
    public enum Dynamic
    {
        /// <summary>No dynamic is defined.</summary>
        Undefined = -1,
        /// <summary>pp - very soft.</summary>
        Pianissimo = 5,
        /// <summary>p - soft.</summary>
        Piano = 10,
        /// <summary>mp - half soft as <see cref="Piano"/>.</summary>
        MezzoPiano = 20,
        /// <summary>mf - half loud as <see cref="Forte"/>.</summary>
        /// <remarks>This is the default dynamic level.</remarks>
        MezzoForte = 40,
        /// <summary>f - loud.</summary>
        Forte = 80,
        /// <summary>ff - very loud.</summary>
        Fortissimo = 90
    }
}
