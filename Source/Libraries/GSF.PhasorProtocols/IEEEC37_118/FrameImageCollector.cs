//******************************************************************************************************
//  FrameImageCollector.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/05/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Collects frame images until a full IEEE C37.118 frame has been received.
    /// </summary>
    public sealed class FrameImageCollector : IDisposable
    {
        #region [ Members ]

        // Fields
        private readonly MemoryStream m_frameQueue;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameImageCollector"/>.
        /// </summary>
        /// <param name="validateFrameCheckSum">Indicates if frames are selected for check-sum validation.</param>
        public FrameImageCollector(bool validateFrameCheckSum)
        {
            m_frameQueue = new MemoryStream();
            ValidateFrameCheckSum = validateFrameCheckSum;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the combined binary image of all the collected frame images.
        /// </summary>
        public byte[] BinaryImage => m_frameQueue.ToArray();

        /// <summary>
        /// Gets the length of the combined binary image of all the collected frame images.
        /// </summary>
        public int BinaryLength => (int)m_frameQueue.Length;

        /// <summary>
        /// Gets the total number frames appended to <see cref="FrameImageCollector"/>.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets flag based on <see cref="CheckSumValidationFrameTypes"/> property that determines if frames are selected for check-sum validation.
        /// </summary>
        public bool ValidateFrameCheckSum { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="FrameImageCollector"/> object.
        /// </summary>
        public void Dispose()
        {
            if (m_disposed)
                return;

            try
            {
                m_frameQueue?.Dispose();
            }
            finally
            {
                m_disposed = true; // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Appends the current frame image to the frame image collection.
        /// </summary>
        /// <param name="buffer">A <see cref="byte"/> array to append to the collection.</param>
        /// <param name="length">An <see cref="int"/> value indicating the number of bytes to read from the <paramref name="buffer"/>.</param>
        /// <param name="offset">An <see cref="int"/> value indicating the offset to read from.</param>
        public void AppendFrameImage(byte[] buffer, int offset, int length)
        {
            // Validate CRC of frame image being appended
            if (ValidateFrameCheckSum && !CommonFrameHeader.ChecksumIsValid(buffer, offset, length))
                throw new InvalidOperationException("Invalid binary image detected - check sum of individual IEEE C37.118 fragmented configuration frame 3 partial frame transmission did not match");

            // Include initial header in new stream
            if (m_frameQueue.Length == 0)
                m_frameQueue.Write(buffer, offset, ConfigurationFrame3.FrameHeaderLength);

            // Skip past header, including CONT_IDX
            offset += ConfigurationFrame3.FrameHeaderLength;

            // Include frame image
            m_frameQueue.Write(buffer, offset, length - ConfigurationFrame3.FrameHeaderLength);

            // Track total frame images
            Count++;
        }

        #endregion
    }
}