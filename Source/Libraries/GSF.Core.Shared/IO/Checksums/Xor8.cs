//******************************************************************************************************
//  Xor8.cs - Gbtc
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
//  09/24/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.IO.Checksums
{
    /// <summary>Calculates byte length (8-bit) XOR-based check-sum on specified portion of a buffer.</summary>
    public sealed class Xor8
    {
        #region [ Members ]

        // Fields
        private byte m_checksum;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the Xor8Bit class.
        /// The checksum starts off with a value of 0.
        /// </summary>
        public Xor8()
        {
            Reset();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the Xor 8-bit checksum computed so far.
        /// </summary>
        public byte Value
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
        /// Updates the checksum with a byte value.
        /// </summary>
        /// <param name="value">The <see cref="byte"/> value to use for the update.</param>
        public void Update(byte value)
        {
            m_checksum ^= value;
        }

        /// <summary>
        /// Updates the checksum with an array of bytes.
        /// </summary>
        /// <param name="buffer">
        /// The source of the data to update with.
        /// </param>
        public void Update(byte[] buffer)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException(nameof(buffer));

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
            if ((object)buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "cannot be negative");

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "cannot be negative");

            if (offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "not a valid index into buffer");

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count), "exceeds buffer size");

            for (int x = 0; x < count; x++)
            {
                m_checksum ^= buffer[offset + x];
            }
        }

        #endregion
    }
}
