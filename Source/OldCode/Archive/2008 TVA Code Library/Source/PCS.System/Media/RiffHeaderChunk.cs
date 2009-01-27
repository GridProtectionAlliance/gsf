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

using System.IO;
using System.Text;

namespace System.Media
{
    /// <summary>
    /// Represents the header chunk in a RIFF media format file.
    /// </summary>
    public class RiffHeaderChunk : RiffChunk
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Type ID of a RIFF header chunk.
        /// </summary>
        public const string RiffTypeID = "RIFF";

        // Fields
        private string m_format;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new RIFF header chunk for the specified format.
        /// </summary>
        /// <param name="format">RIFF header chunk header format.</param>
        public RiffHeaderChunk(string format)
            : base(RiffTypeID)
        {
            Format = format;
        }

        /// <summary>Reads a new RIFF header from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed <see cref="RiffChunk"/> header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <param name="format">Expected RIFF media format (e.g., "WAVE").</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="format"/> must be extactly 4 characters in length.</exception>
        public RiffHeaderChunk(RiffChunk preRead, Stream source, string format)
            : base(preRead, RiffTypeID)
        {
            Format = format;

            int length = BinaryLength - preRead.BinaryLength;
            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            if (bytesRead < length)
                throw new InvalidOperationException("RIFF format section too small, media file corrupted.");

            // Read and validate format stored in RIFF section
            format = Encoding.ASCII.GetString(buffer, 0, 4);

            if (format != Format)
                throw new InvalidDataException(string.Format("{0} format expected but got {1}, this does not appear to be a valid {0} file", Format, format));

            m_format = format;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets format for RIFF header chunk.
        /// </summary>
        public string Format
        {
            get
            {
                return m_format;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Format");

                if (value.Length != 4)
                    throw new ArgumentOutOfRangeException("Format", "Format must be exactly 4 characters in length");

                m_format = value;
            }
        }

        /// <summary>
        /// Gets binary representation of RIFF header chunk.
        /// </summary>
        public override byte[] BinaryImage
        {
            get
            {
                byte[] binaryImage = new byte[BinaryLength];
                int startIndex = base.BinaryLength;

                Buffer.BlockCopy(base.BinaryImage, 0, binaryImage, 0, startIndex);
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(m_format), 0, binaryImage, startIndex, 4);

                return binaryImage;
            }
        }

        /// <summary>
        /// Gets length of binary representation of RIFF header chunk.
        /// </summary>
        public new int BinaryLength
        {
            get
            {
                return base.BinaryLength + 4;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a cloned instance of this RIFF header chunk.
        /// </summary>
        /// <returns>A cloned instance of this RIFF header chunk.</returns>
        public new RiffHeaderChunk Clone()
        {
            return new RiffHeaderChunk(m_format);
        }

        #endregion
    }
}
