using System;
using System.IO;
using System.Text;

namespace TVA.Media
{
    /// <summary>
    /// Represents the header chunk in a RIFF media format file.
    /// </summary>
    public class RiffHeaderChunk : RiffChunk
    {
        private string m_format;

        public RiffHeaderChunk(string format)
            : base("RIFF")
        {
            Format = format;
        }

        /// <summary>Reads a new RIFF header from the specified stream.</summary>
        /// <param name="source">Source stream to read data from.</param>
        /// <param name="format">Expected RIFF media format (e.g., "WAVE").</param>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="format"/> must be extactly 4 characters in length.</exception>
        public RiffHeaderChunk(RiffChunk preRead, Stream source, string format)
            : base(preRead, "RIFF")
        {
            Format = format;

            int length = BinaryLength - preRead.BinaryLength;
            byte[] buffer = new byte[length];

            int bytesRead = source.Read(buffer, 0, length);

            if (bytesRead < length)
                throw new InvalidOperationException("RIFF format section too small, media file corrupted.");

            Initialize(buffer, 0);
        }

        public override int Initialize(byte[] binaryImage, int startIndex)
        {
            string format = Encoding.ASCII.GetString(binaryImage, startIndex, 4);

            if (format != m_format)
                throw new InvalidDataException(string.Format("{0} format expected but got {1}, this does not appear to be a valid {0} file", m_format, format));

            m_format = format;

            return BinaryLength;
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

        public override int BinaryLength
        {
            get
            {
                return base.BinaryLength + 4;
            }
        }

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
    }
}
