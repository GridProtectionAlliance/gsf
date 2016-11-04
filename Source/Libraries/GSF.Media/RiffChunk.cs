//******************************************************************************************************
//  RiffChunk.cs - Gbtc
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using GSF.Parsing;

namespace GSF.Media
{
    /// <summary>
    /// Represents the type ID and size for a "chunk" in a RIFF media format file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Resource Interchange File Format (RIFF) is a generic meta-format for storing data in tagged chunks.
    /// It was introduced in 1991 by Microsoft and IBM, and was presented by Microsoft as the default format for
    /// Windows 3.1 multimedia files. It is based on Electronic Arts's Interchange File Format, introduced in 1985,
    /// the only difference being that multi-byte integers are in little-endian format, native to the 80x86 processor
    /// series used in IBM PCs, rather than the big-endian format native to the 68k processor series used in Amiga and
    /// Apple Macintosh computers, where IFF files were heavily used. (The specification for AIFF, the big-endian
    /// analogue of RIFF, was published by Apple Computer in 1988.) The Microsoft implementation is mostly known
    /// through file formats like AVI, ANI and WAV, which use the RIFF meta-format as their basis.
    /// </para>
    /// <para>
    /// Some common RIFF file types:
    /// <list type="table">
    /// <listheader>
    ///     <term>File extension</term>
    ///     <description>Description</description>
    /// </listheader>
    /// <item>
    ///     <term>WAV</term>
    ///     <description>Windows audio file</description>
    /// </item>
    /// <item>
    ///     <term>AVI</term>
    ///     <description>Windows audio/video file</description>
    /// </item>
    /// <item>
    ///     <term>ANI</term>
    ///     <description>Animated Windows cursors</description>
    /// </item>
    /// <item>
    ///     <term>RMI</term>
    ///     <description>Windows RIFF MIDI file</description>
    /// </item>
    /// <item>
    ///     <term>CDR</term>
    ///     <description>CorelDRAW vector graphics file</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class RiffChunk : ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// The fixed byte length of a <see cref="RiffChunk"/> instance.
        /// </summary>
        public const int FixedLength = 8;

        // Fields
        private string m_typeID;
        private int m_chunkSize;

        #endregion

        #region [ Constructors ]

        private RiffChunk()
        {
        }

        /// <summary>
        /// Constructor for derived classes used to initialize and validate  <see cref="RiffChunk"/> properties.
        /// </summary>
        /// <param name="preRead">Pre-parsed <see cref="RiffChunk"/> header.</param>
        /// <param name="typeID">Expected type ID.</param>
        protected RiffChunk(RiffChunk preRead, string typeID)
        {
            if (typeID != preRead.TypeID)
                throw new InvalidDataException(string.Format("{0} chunk expected but got {1}, file does not appear to be valid", typeID, preRead.TypeID));

            m_typeID = preRead.TypeID;
            m_chunkSize = preRead.ChunkSize;
        }

        /// <summary>
        /// Constructs a new <see cref="RiffChunk"/> for the given <paramref name="typeID"/>.
        /// </summary>
        /// <param name="typeID">Expected type ID.</param>
        public RiffChunk(string typeID)
        {
            m_typeID = typeID;
        }

        #endregion

        #region [ Properties ]

        /// <summary>Four character text identifer for RIFF chunk.</summary>
        /// <exception cref="ArgumentNullException">TypeID cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">TypeID must be extactly 4 characters in length.</exception>
        public string TypeID
        {
            get
            {
                return m_typeID;
            }
            set
            {
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(value), "TypeID must be exactly 4 characters in length");

                m_typeID = value;
            }
        }

        /// <summary>Size of <see cref="RiffChunk"/>.</summary>
        public virtual int ChunkSize
        {
            get
            {
                return m_chunkSize;
            }
            set
            {
                m_chunkSize = value;
            }
        }

        /// <summary>
        /// Gets the length of a <see cref="RiffChunk"/> consisting of type ID and chunk size (i.e., 8 bytes).
        /// </summary>
        public virtual int BinaryLength
        {
            get
            {
                return FixedLength;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Generates a binary representation of this <see cref="RiffChunk"/> and copies it into the given buffer.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public virtual int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            buffer.ValidateParameters(startIndex, FixedLength);

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(TypeID), 0, buffer, 0, 4);
            Buffer.BlockCopy(LittleEndian.GetBytes(ChunkSize), 0, buffer, 4, 4);

            return FixedLength;
        }

        // This is not currently needed since the RIFF chunks are read using the static ReadNext
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        int ISupportBinaryImage.ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a copy of the <see cref="RiffChunk"/>.
        /// </summary>
        /// <returns>A new copy of the <see cref="RiffChunk"/>.</returns>
        public RiffChunk Clone()
        {
            RiffChunk riffChunk = new RiffChunk(m_typeID);
            riffChunk.ChunkSize = m_chunkSize;
            return riffChunk;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Attempts to read the next RIFF chunk from the <paramref name="source"/> stream.
        /// </summary>
        /// <param name="source">Source stream for next RIFF chunk.</param>
        /// <returns>Next RIFF chunk read from the <paramref name="source"/> stream.</returns>
        /// <exception cref="InvalidOperationException">RIFF chunk too small, media file corrupted.</exception>
        public static RiffChunk ReadNext(Stream source)
        {
            RiffChunk riffChunk = new RiffChunk();
            int length = riffChunk.BinaryLength;

            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            if (bytesRead < length)
                throw new InvalidOperationException("RIFF chunk too small, media file corrupted");

            riffChunk.TypeID = Encoding.ASCII.GetString(buffer, 0, 4);
            riffChunk.ChunkSize = LittleEndian.ToInt32(buffer, 4);

            return riffChunk;
        }

        #endregion
    }
}
