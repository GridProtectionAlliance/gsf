//*******************************************************************************************************
//  RiffHeaderChunk.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/03/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
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
        public const string RiffTypeID = "RIFF";

        // Fields
        private string m_format;

        #endregion

        #region [ Constructors ]

        public RiffHeaderChunk(string format)
            : base(RiffTypeID)
        {
            Format = format;
        }

        /// <summary>Reads a new RIFF header from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed RIFF chunk header.</param>
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

        public new int BinaryLength
        {
            get
            {
                return base.BinaryLength + 4;
            }
        }

        #endregion
    }
}
