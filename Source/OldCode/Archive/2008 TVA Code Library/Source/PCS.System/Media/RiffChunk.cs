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
using System.IO;
using System.Text;

namespace System.Media
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
    public class RiffChunk
    {
        #region [ Members ]

        // Fields
        private string m_typeID;
        private int m_chunkSize;

        #endregion

        #region [ Constructors ]

        private RiffChunk()
        {
        }

        protected RiffChunk(RiffChunk preRead, string typeID)
        {
            if (typeID != preRead.TypeID)
                throw new InvalidDataException(string.Format("{0} chunk expected but got {1}, file does not appear to be valid.", typeID, preRead.TypeID));

            m_typeID = preRead.TypeID;
            m_chunkSize = preRead.ChunkSize;
        }

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
                if (value == null)
                    throw new ArgumentNullException("TypeID");

                if (value.Length != 4)
                    throw new ArgumentOutOfRangeException("TypeID", "TypeID must be exactly 4 characters in length");

                m_typeID = value;
            }
        }

        /// <summary>Size of RIFF chunk.</summary>
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
        /// Returns a binary representation of this RIFF chunk.
        /// </summary>
        public virtual byte[] BinaryImage
        {
            get
            {
                byte[] binaryImage = new byte[BinaryLength];

                Buffer.BlockCopy(Encoding.ASCII.GetBytes(TypeID), 0, binaryImage, 0, 4);
                Buffer.BlockCopy(EndianOrder.LittleEndian.GetBytes(ChunkSize), 0, binaryImage, 4, 4);

                return binaryImage;
            }
        }

        /// <summary>
        /// Gets the length of a RIFF chunk consisting of type ID and chunk size (i.e., 8 bytes).
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return 8;
            }
        }

        #endregion

        #region [ Methods ]

        public RiffChunk Clone()
        {
            RiffChunk riffChunk = new RiffChunk(m_typeID);
            riffChunk.ChunkSize = m_chunkSize;
            return riffChunk;
        }

        #endregion

        #region [ Static ]

        // Static Methods
        public static RiffChunk ReadNext(Stream source)
        {
            RiffChunk riffChunk = new RiffChunk();
            int length = riffChunk.BinaryLength;

            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            if (bytesRead < length)
                throw new InvalidOperationException("RIFF chunk too small, media file corrupted.");

            riffChunk.TypeID = Encoding.ASCII.GetString(buffer, 0, 4);
            riffChunk.ChunkSize = EndianOrder.LittleEndian.ToInt32(buffer, 4);

            return riffChunk;
        }

        #endregion
    }
}
