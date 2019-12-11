//******************************************************************************************************
//  ISupportBinaryImageExtensions.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  12/04/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;

namespace GSF.Parsing
{
    /// <summary>
    /// Defines extension functions related to <see cref="ISupportBinaryImage"/> implementations.
    /// </summary>
    public static class ISupportBinaryImageExtensions
    {
        /// <summary>
        /// Returns a binary image of an object that implements <see cref="ISupportBinaryImage"/>.
        /// </summary>
        /// <param name="imageSource"><see cref="ISupportBinaryImage"/> source.</param>
        /// <returns>A binary image of an object that implements <see cref="ISupportBinaryImage"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageSource"/> cannot be null.</exception>
        /// <remarks>
        /// This is a convenience method. It is often optimal to use <see cref="ISupportBinaryImage.GenerateBinaryImage"/>
        /// directly using a common buffer instead of always allocating new buffers.
        /// </remarks>
        public static byte[] BinaryImage(this ISupportBinaryImage imageSource)
        {
            if (imageSource == null)
                throw new ArgumentNullException(nameof(imageSource));

            byte[] buffer = new byte[imageSource.BinaryLength];

            imageSource.GenerateBinaryImage(buffer, 0);

            return buffer;
        }

        /// <summary>
        /// Copies binary image of object that implements <see cref="ISupportBinaryImage"/> to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="imageSource"><see cref="ISupportBinaryImage"/> source.</param>
        /// <param name="stream">Destination <see cref="Stream"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageSource"/> cannot be null.</exception>
        public static void CopyBinaryImageToStream(this ISupportBinaryImage imageSource, Stream stream)
        {
            if (imageSource == null)
                throw new ArgumentNullException(nameof(imageSource));

            int length = imageSource.BinaryLength;
            byte[] buffer = new byte[length];

            // Copy generated binary image to buffer
            int writeCount = imageSource.GenerateBinaryImage(buffer, 0);

            // Write buffer bytes to stream, if any were generated
            if (writeCount > 0)
                stream.Write(buffer, 0, writeCount);
        }

        /// <summary>
        /// Parses binary image of object that implements <see cref="ISupportBinaryImage"/> from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="imageSource"><see cref="ISupportBinaryImage"/> source.</param>
        /// <param name="stream">Source <see cref="Stream"/>.</param>
        /// <returns>The number of bytes parsed from the <paramref name="stream"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="imageSource"/> cannot be null.</exception>
        public static int ParseBinaryImageFromStream(this ISupportBinaryImage imageSource, Stream stream)
        {
            if (imageSource == null)
                throw new ArgumentNullException(nameof(imageSource));

            int length = imageSource.BinaryLength;
            byte[] buffer = new byte[length];

            // Read buffer bytes from stream
            int readCount = stream.Read(buffer, 0, length);

            // Parse binary image from buffer bytes read from stream
            return imageSource.ParseBinaryImage(buffer, 0, readCount);
        }
    }
}