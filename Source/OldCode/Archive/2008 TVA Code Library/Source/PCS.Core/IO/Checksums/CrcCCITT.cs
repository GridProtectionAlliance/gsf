//*******************************************************************************************************
//  CrcCCITT.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/25/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace PCS.IO.Checksums 
{
	/// <summary>Generates a 16-bit CRC-CCITT checksum.</summary>
    /// <remarks>
    /// This is a non-table based 16-bit CRC popular for modem protocols defined for use by the
    /// Consultative Committee on International Telegraphy and Telephony (CCITT) 
    /// </remarks>
    public sealed class CrcCCITT : IChecksum
	{
        #region [ Members ]

        // Constants
        const ushort CrcSeed = 0xFFFF;

        // Fields
        private ushort crc; // The crc data checksum so far.

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the Crc16 class.
        /// The checksum starts off with a value of 0xFFFF.
        /// </summary>
        public CrcCCITT()
        {
            Reset();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the CRC16 data checksum computed so far.
        /// </summary>
        [CLSCompliant(false)]
        public ushort Value
        {
            get
            {
                return crc;
            }
            set
            {
                crc = value;
            }
        }

        long IChecksum.Value
        {
            get
            {
                return (long)crc;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Resets the CRC-CCITT data checksum as if no update was ever called.
        /// </summary>
        public void Reset()
        {
            crc = CrcSeed;
        }

        /// <summary>
        /// Updates the checksum with the int bval.
        /// </summary>
        public void Update(byte value)
        {
            Update(new byte[] { value });
        }

        void IChecksum.Update(int value)
        {
            Update((byte)value);
        }

        /// <summary>
        /// Updates the checksum with the bytes taken from the array.
        /// </summary>
        /// <param name="buffer">
        /// buffer an array of bytes
        /// </param>
        public void Update(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            Update(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Adds the byte array to the data checksum.
        /// </summary>
        /// <param name = "buffer">
        /// The buffer which contains the data
        /// </param>
        /// <param name = "offset">
        /// The offset in the buffer where the data starts
        /// </param>
        /// <param name = "count">
        /// The number of data bytes to update the CRC with.
        /// </param>
        public void Update(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero");

            if (offset < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            
            ushort temp, quick;

            for (int x = 0; x < count; x++)
            {
                temp = (ushort)((crc >> 8) ^ buffer[x + offset]);
                crc <<= 8;
                quick = (ushort)(temp ^ (temp >> 4));
                crc ^= quick;
                quick <<= 5;
                crc ^= quick;
                quick <<= 7;
                crc ^= quick;
            }
        }

        #endregion
	}
}
