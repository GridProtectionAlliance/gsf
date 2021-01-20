//******************************************************************************************************
//  FrameImageCollector.cs - Gbtc
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
//  12/30/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;

namespace GSF.PhasorProtocols.IEEE1344
{
    /// <summary>
    /// Collects frame images until a full IEEE 1344 frame has been received.
    /// </summary>
    public class FrameImageCollector
    {
        #region [ Members ]

        // Fields
        private readonly MemoryStream m_frameQueue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrameImageCollector"/>.
        /// </summary>
        public FrameImageCollector()
        {
            // As an optimization in context of usage, we don't implement IDisposable for
            // this class just for the memory stream since its Close method (i.e., Dispose)
            // does nothing (reflect it and look for yourself). Additionally, we go ahead
            // and suppress the finalizer for this stream to reduce that overhead too.
            m_frameQueue = new MemoryStream();
            GC.SuppressFinalize(m_frameQueue);
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

    #endregion

        #region [ Methods ]

        /// <summary>
        /// Appends the current frame image to the frame image collection.
        /// </summary>
        /// <param name="buffer">A <see cref="byte"/> array to append to the collection.</param>
        /// <param name="length">An <see cref="int"/> value indicating the number of bytes to read from the <paramref name="buffer"/>.</param>
        /// <param name="offset">An <see cref="int"/> value indicating the offset to read from.</param>
        public void AppendFrameImage(byte[] buffer, int offset, int length)
        {
            // Validate CRC of frame image being appended
            if (!CommonFrameHeader.ChecksumIsValid(buffer, offset, length))
                throw new InvalidOperationException("Invalid binary image detected - check sum of individual IEEE 1344 interleaved frame transmission did not match");

            // Include initial header in new stream...
            if (m_frameQueue.Length == 0)
                m_frameQueue.Write(buffer, offset, CommonFrameHeader.FixedLength);

            // Skip past header
            offset += CommonFrameHeader.FixedLength;

            // Include frame image
            m_frameQueue.Write(buffer, offset, length - CommonFrameHeader.FixedLength);

            // Track total frame images
            Count++;
        }

        #endregion
    }
}