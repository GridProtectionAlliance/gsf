//******************************************************************************************************
//  RiffHeaderChunk.cs - Gbtc
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
using System.IO;
using System.Text;
using GSF.Parsing;

namespace GSF.Media
{
    /// <summary>
    /// Represents the header chunk in a RIFF media format file.
    /// </summary>
    public class RiffHeaderChunk : RiffChunk, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Type ID of a RIFF header chunk.
        /// </summary>
        public const string RiffTypeID = "RIFF";

        /// <summary>
        /// The fixed byte length of a <see cref="RiffChunk"/> instance.
        /// </summary>
        public new const int FixedLength = RiffChunk.FixedLength + 4;

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
            if (string.IsNullOrWhiteSpace(format))
                throw new ArgumentNullException("value");

            if (format.Length != 4)
                throw new ArgumentOutOfRangeException("value", "Format must be exactly 4 characters in length");

            m_format = format;
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
            if (string.IsNullOrWhiteSpace(format))
                throw new ArgumentNullException("value");

            if (format.Length != 4)
                throw new ArgumentOutOfRangeException("value", "Format must be exactly 4 characters in length");

            byte[] buffer = new byte[4];

            int bytesRead = source.Read(buffer, 0, 4);

            if (bytesRead < 4)
                throw new InvalidOperationException("RIFF format section too small, media file corrupted");

            // Read format stored in RIFF section
            m_format = Encoding.ASCII.GetString(buffer, 0, 4);

            if (m_format != format)
                throw new InvalidDataException(string.Format("{0} format expected but got {1}, this does not appear to be a valid {0} file", format, m_format));
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
                if ((object)value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(value), "Format must be exactly 4 characters in length");

                m_format = value;
            }
        }

        /// <summary>
        /// Gets length of binary representation of RIFF header chunk.
        /// </summary>
        public new int BinaryLength
        {
            get
            {
                return FixedLength;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Generates binary image of the object and copies it into the given buffer.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            buffer.ValidateParameters(startIndex, FixedLength);

            startIndex += base.GenerateBinaryImage(buffer, startIndex);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(m_format), 0, buffer, startIndex, 4);

            return FixedLength;
        }

        // This is not currently needed since the RIFF header chunk is initialized during construction
        int ISupportBinaryImage.ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

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
