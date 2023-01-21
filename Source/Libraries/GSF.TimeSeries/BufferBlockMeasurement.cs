//******************************************************************************************************
//  BufferBlockMeasurement.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  01/17/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a byte buffer that can be transported through the system as a <see cref="IMeasurement"/>.
    /// </summary>
    public class BufferBlockMeasurement : Measurement
    {
        #region [ Members ]

        // Members

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="BufferBlockMeasurement"/>.
        /// </summary>
        public BufferBlockMeasurement()
        {
            // Value of measurement should be indeterminate since this a buffer
            Value = double.NaN;
        }

        /// <summary>
        /// Creates a new <see cref="BufferBlockMeasurement"/> from an existing buffer.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public BufferBlockMeasurement(byte[] buffer, int startIndex, int length)
            : this()
        {
            // Validate buffer parameters
            buffer.ValidateParameters(startIndex, length);

            // We don't hold on to source buffer (we don't own it), so we instantiate a new one
            Buffer = new byte[length];

            // Copy buffer contents onto our local buffer
            System.Buffer.BlockCopy(buffer, startIndex, Buffer, 0, length);
            Length = length;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Cached buffer image.
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// Valid length of cached buffer image.
        /// </summary>
        public int Length { get; }

        #endregion
    }
}