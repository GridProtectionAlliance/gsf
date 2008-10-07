//*******************************************************************************************************
//  RiffChunk.cs
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

using System.IO;
using System.Text;

namespace System
{
    /// <summary>
    /// Represents the type ID and size for a "chunk" in a RIFF media format file.
    /// </summary>
    /// <remarks>
    /// The RIFF media format is Microsoft's implementation of the Interchange File Format (IFF)
    /// originally developed Electronic Arts in 1985.  The primary difference is that RIFF uses
    /// little-endian byte order for integer encoding.
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
