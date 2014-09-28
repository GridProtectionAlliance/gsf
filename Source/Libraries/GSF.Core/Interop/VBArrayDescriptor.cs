//******************************************************************************************************
//  VBArrayDescriptor.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/07/2007 - Pinal C. Patel
//       Original version of source code generated.
//       Generated original version of source code.
//  09/10/2008 - J. Ritchie Carroll
//       Converted to C#.
//  11/04/2008 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/23/2011 - J. Ritchie Carroll
//       Updated to support new ISupportBinaryImage buffer optimizations.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.Parsing;

namespace GSF.Interop
{
    /// <summary>
    /// Represents an old style Visual Basic array descriptor.
    /// </summary>
    /// <remarks>
    /// This class is used to mimic the binary array descriptor used when an array is serialized
    /// into a file using older Visual Basic applications (VB 6 and prior), this way old VB apps
    /// can still deserialize an array stored in a file written by a .NET application and vice versa.
    /// </remarks>
    public sealed class VBArrayDescriptor : ISupportBinaryImage
    {
        #region [ Members ]

        // Nested Types
        private class DimensionDescriptor
        {
            public const int BinaryLength = 8;

            public readonly int Length;
            public readonly int LowerBound;

            public DimensionDescriptor(int length, int lowerBound)
            {
                Length = length;
                LowerBound = lowerBound;
            }
        }

        // Fields
        private readonly List<DimensionDescriptor> m_arrayDimensionDescriptors;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="VBArrayDescriptor"/> class.
        /// </summary>
        /// <param name="arrayLengths">Length of array per dimension.</param>
        /// <param name="arrayLowerBounds">Lower bound of array per dimension.</param>
        public VBArrayDescriptor(int[] arrayLengths, int[] arrayLowerBounds)
        {
            if (arrayLengths.Length == arrayLowerBounds.Length)
            {
                m_arrayDimensionDescriptors = new List<DimensionDescriptor>();

                for (int i = 0; i < arrayLengths.Length; i++)
                {
                    m_arrayDimensionDescriptors.Add(new DimensionDescriptor(arrayLengths[i], arrayLowerBounds[i]));
                }
            }
            else
            {
                throw new ArgumentException("Number of lengths and lower bounds must be the same");
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the length of serialized <see cref="VBArrayDescriptor"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return 2 + 8 * m_arrayDimensionDescriptors.Count;
            }
        }

        #endregion

        #region [ Methods ]

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
        public int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            int length = BinaryLength;
            buffer.ValidateParameters(startIndex, length);

            Buffer.BlockCopy(BitConverter.GetBytes(m_arrayDimensionDescriptors.Count), 0, buffer, startIndex, 2);

            for (int i = 0; i < m_arrayDimensionDescriptors.Count; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(m_arrayDimensionDescriptors[i].Length), 0, buffer, startIndex + (i * DimensionDescriptor.BinaryLength) + 2, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(m_arrayDimensionDescriptors[i].LowerBound), 0, buffer, startIndex + (i * DimensionDescriptor.BinaryLength) + 6, 4);
            }

            return length;
        }

        // Currently not supporting initialization from binary image
        int ISupportBinaryImage.ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Returns a <see cref="VBArrayDescriptor"/> object for a one dimensional array with zero-based index.
        /// </summary>
        /// <param name="arrayLength">Length of the array.</param>
        /// <returns>A <see cref="VBArrayDescriptor"/> object.</returns>
        public static VBArrayDescriptor ZeroBasedOneDimensionalArray(int arrayLength)
        {
            return new VBArrayDescriptor(new[] { arrayLength }, new[] { 0 });
        }

        /// <summary>
        /// Returns a <see cref="VBArrayDescriptor"/> object for a one dimensional array with one-based index.
        /// </summary>
        /// <param name="arrayLength">Length of the array.</param>
        /// <returns>A <see cref="VBArrayDescriptor"/> object.</returns>
        public static VBArrayDescriptor OneBasedOneDimensionalArray(int arrayLength)
        {
            return new VBArrayDescriptor(new[] { arrayLength }, new[] { 1 });
        }

        /// <summary>
        /// Returns a <see cref="VBArrayDescriptor"/> object for a two dimensional array with zero-based index.
        /// </summary>
        /// <param name="dimensionOneLength">Length of array in dimension one.</param>
        /// <param name="dimensionTwoLength">Length of array in dimension two.</param>
        /// <returns>A <see cref="VBArrayDescriptor"/> object.</returns>
        public static VBArrayDescriptor ZeroBasedTwoDimensionalArray(int dimensionOneLength, int dimensionTwoLength)
        {
            return new VBArrayDescriptor(new[] { dimensionOneLength, dimensionTwoLength }, new[] { 0, 0 });
        }

        /// <summary>
        /// Returns a <see cref="VBArrayDescriptor"/> object for a two dimensional array with one-based index.
        /// </summary>
        /// <param name="dimensionOneLength">Length of array in dimension one.</param>
        /// <param name="dimensionTwoLength">Length of array in dimension two.</param>
        /// <returns>A <see cref="VBArrayDescriptor"/> object.</returns>
        public static VBArrayDescriptor OneBasedTwoDimensionalArray(int dimensionOneLength, int dimensionTwoLength)
        {
            return new VBArrayDescriptor(new[] { dimensionOneLength, dimensionTwoLength }, new[] { 1, 1 });
        }

        #endregion
    }
}