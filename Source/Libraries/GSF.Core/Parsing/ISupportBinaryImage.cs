//******************************************************************************************************
//  ISupportBinaryImage.cs - Gbtc
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
//  12/04/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/22/2011 - J. Ritchie Carroll
//       Converted interface to use a write based image method instead of a property as an optimization.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;

namespace GSF.Parsing
{
    /// <summary>
    /// Specifies that an object can support production or consumption of a binary image that represents the object.
    /// </summary>
    public interface ISupportBinaryImage
    {
        /// <summary>
        /// Gets the length of the binary image.
        /// </summary>
        int BinaryLength
        {
            get;
        }

        /// <summary>
        /// Initializes object by parsing the specified <paramref name="buffer"/> containing a binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> to read from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <remarks>
        /// Implementers should validate <paramref name="startIndex"/> and <paramref name="length"/> against <paramref name="buffer"/> length.
        /// The <see cref="ArrayExtensions.ValidateParameters{T}"/> method can be used to perform this validation.
        /// </remarks>
        int ParseBinaryImage(byte[] buffer, int startIndex, int length);

        /// <summary>
        /// Generates binary image of the object and copies it into the given buffer, for <see cref="BinaryLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// Implementers should validate <paramref name="startIndex"/> and <see cref="BinaryLength"/> against <paramref name="buffer"/> length.
        /// The <see cref="ArrayExtensions.ValidateParameters{T}"/> method can be used to perform this validation.
        /// </remarks>
        int GenerateBinaryImage(byte[] buffer, int startIndex);
    }

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
            if ((object)imageSource == null)
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
            if ((object)imageSource == null)
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
            if ((object)imageSource == null)
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