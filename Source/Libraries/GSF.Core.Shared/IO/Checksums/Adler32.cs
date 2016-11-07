//******************************************************************************************************
//  Adler32.cs - Gbtc
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
//  04/11/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.IO.Checksums
{
    /// <summary>
    /// Generates an Adler-32 checksum calculation.
    /// </summary>
    public sealed class Adler32
    {
        #region [ Members ]

        // Constants
        private const uint AdlerMod = 65521u;
        private const int MaxLoops = 5552;

        // Fields
        private uint m_a;
        private uint m_b;

        private int m_loopCount;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Adler32"/> class.
        /// </summary>
        public Adler32()
        {
            Reset();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the Adler-32 data checksum computed so far.
        /// </summary>
        public uint Value
        {
            get
            {
                ApplyMod();
                return m_a | (m_b << 16);
            }
            set
            {
                m_a = value & 0xFFFF;
                m_b = value >> 16;
                ApplyMod();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Resets the Adler-32 data checksum as if no update was ever called.
        /// </summary>
        public void Reset()
        {
            m_a = 1;
            m_b = 0;
            m_loopCount = 0;
        }

        /// <summary>
        /// Updates the checksum with the byte value.
        /// </summary>
        /// <param name="value">The <see cref="byte"/> value to use for the update.</param>
        public void Update(byte value)
        {
            if (m_loopCount >= MaxLoops)
                ApplyMod();

            m_a += value;
            m_b += m_a;
            m_loopCount++;
        }

        /// <summary>
        /// Updates the checksum with the bytes taken from the array.
        /// </summary>
        /// <param name="buffer">buffer an array of bytes</param>
        public void Update(byte[] buffer)
        {
            Update(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Adds the byte array to the data checksum.
        /// </summary>
        /// <param name="buffer">The buffer which contains the data</param>
        /// <param name="offset">The offset in the buffer where the data starts</param>
        /// <param name="count">The number of data bytes to update the checksum with.</param>
        public void Update(byte[] buffer, int offset, int count)
        {
            int remainingLoops;
            int numLoops;
            int i = 0;

            while (i < count)
            {
                if (m_loopCount >= MaxLoops)
                    ApplyMod();

                remainingLoops = MaxLoops - m_loopCount;
                numLoops = Math.Min(remainingLoops, count - i);

                for (int loopCtr = 0; loopCtr < numLoops; loopCtr++)
                {
                    m_a += buffer[offset + i];
                    m_b += m_a;
                    i++;
                }

                m_loopCount += numLoops;
            }
        }

        // Applies the modulus operation
        // to the checksum components.
        private void ApplyMod()
        {
            m_a %= AdlerMod;
            m_b %= AdlerMod;
            m_loopCount = 0;
        }

        #endregion
    }
}
