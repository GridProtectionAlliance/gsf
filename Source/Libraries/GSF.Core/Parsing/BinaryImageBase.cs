//******************************************************************************************************
//  BinaryImageBase.cs - Gbtc
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
//  01/06/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/23/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.Parsing
{
    /// <summary>
    /// Defines a base class that represents binary images for parsing or generation in terms of a header, body and footer.
    /// </summary>
    [Serializable]
    public abstract class BinaryImageBase : ISupportBinaryImage
    {
        #region [ Properties ]

        /// <summary>
        /// Gets the length of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is not typically overridden since it is the sum of the header, body and footer lengths.
        /// </remarks>
        public virtual int BinaryLength => HeaderLength + BodyLength + FooterLength;

        /// <summary>
        /// Gets the length of the header portion of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int HeaderLength => 0;

        /// <summary>
        /// Gets the length of the body portion of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int BodyLength => 0;

        /// <summary>
        /// Gets the length of the footer portion of the <see cref="BinaryImageBase"/> object.
        /// </summary>
        /// <remarks>
        /// This property is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int FooterLength => 0;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes object by parsing the specified <paramref name="buffer"/> containing a binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        /// <remarks>
        /// This method is not typically overridden since it is parses the header, body and footer images in sequence.
        /// </remarks>
        public virtual int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            int index = startIndex;

            // Parse out header, body and footer images
            index += ParseHeaderImage(buffer, index, length);
            index += ParseBodyImage(buffer, index, length - (index - startIndex));
            index += ParseFooterImage(buffer, index, length - (index - startIndex));

            return index - startIndex;
        }

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <remarks>
        /// This method is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int ParseHeaderImage(byte[] buffer, int startIndex, int length) => 0;

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <remarks>
        /// This method is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int ParseBodyImage(byte[] buffer, int startIndex, int length) => 0;

        /// <summary>
        /// Parses the binary footer image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <remarks>
        /// This method is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int ParseFooterImage(byte[] buffer, int startIndex, int length) => 0;

        /// <summary>
        /// Generates binary image of the object and copies it into the given buffer, for <see cref="ISupportBinaryImage.BinaryLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        /// <remarks>
        /// This property is not typically overridden since it is the generates the combination of the header, body and footer images.
        /// </remarks>
        public virtual int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            buffer.ValidateParameters(startIndex, BinaryLength);

            int index = startIndex;

            // Generate header, body and footer images
            index += GenerateHeaderImage(buffer, index);
            index += GenerateBodyImage(buffer, index);
            index += GenerateFooterImage(buffer, index);

            return index - startIndex;
        }

        /// <summary>
        /// Generates the binary header image and copies it into the given buffer, for <see cref="HeaderLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// This method is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int GenerateHeaderImage(byte[] buffer, int startIndex) => 0;

        /// <summary>
        /// Generates the binary body image and copies it into the given buffer, for <see cref="BodyLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// This method is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int GenerateBodyImage(byte[] buffer, int startIndex) => 0;

        /// <summary>
        /// Generates the binary footer image and copies it into the given buffer, for <see cref="FooterLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// This method is typically overridden by a specific protocol implementation.
        /// </remarks>
        protected virtual int GenerateFooterImage(byte[] buffer, int startIndex) => 0;

        #endregion
    }
}