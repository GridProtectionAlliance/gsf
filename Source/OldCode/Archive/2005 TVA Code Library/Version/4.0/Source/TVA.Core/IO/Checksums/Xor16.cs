//*******************************************************************************************************
//  Xor16Bit.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/24/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA.IO.Checksums
{
    /// <summary>Calculates word length (16-bit) XOR-based check-sum on specified portion of a buffer.</summary>
    [CLSCompliant(false)]
    public sealed class Xor16 : IChecksum
    {
        #region [ Members ]

        // Fields
        private ushort m_checksum;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the Xor16Bit class.
        /// The checksum starts off with a value of 0.
        /// </summary>
        public Xor16()
        {
            Reset();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the Xor 16-bit checksum computed so far.
        /// </summary>
        public ushort Value
        {
            get
            {
                return m_checksum;
            }
        }

        long IChecksum.Value
        {
            get
            {
                return m_checksum;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Resets the checksum to the initial value.
        /// </summary>
        public void Reset()
        {
            m_checksum = 0;
        }

        /// <summary>
        /// Updates the checksum with a ushort value.
        /// </summary>
        public void Update(ushort value)
        {
            m_checksum ^= value;
        }
        
        void IChecksum.Update(int value)
        {
            Update((ushort)value);
        }

        /// <summary>
        /// Updates the checksum with an array of bytes.
        /// </summary>
        /// <param name="buffer">
        /// The source of the data to update with.
        /// </param>
        public void Update(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            Update(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Updates the checksum with the bytes taken from the array.
        /// </summary>
        /// <param name="buffer">
        /// an array of bytes
        /// </param>
        /// <param name="offset">
        /// the start of the data used for this update
        /// </param>
        /// <param name="count">
        /// the number of bytes to use for this update
        /// </param>
        public void Update(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "cannot be negative");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "cannot be negative");

            if (offset >= buffer.Length)
                throw new ArgumentOutOfRangeException("offset", "not a valid index into buffer");

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count", "exceeds buffer size");

            for (int x = 0; x < count; x += 2)
            {
                m_checksum ^= BitConverter.ToUInt16(buffer, offset + x);
            }
        }

        #endregion
    }
}
