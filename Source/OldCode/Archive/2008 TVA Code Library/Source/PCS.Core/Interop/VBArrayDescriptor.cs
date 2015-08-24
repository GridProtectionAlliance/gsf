//*******************************************************************************************************
//  VBArrayDescriptor.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/07/2007 - Pinal C. Patel
//       Original version of source code generated.
//       Generated original version of source code.
//  09/10/2008 - J. Ritchie Carroll
//       Converted to C#.
//  11/04/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using PCS.Parsing;

namespace PCS.Interop
{
    /// <summary>
    /// Old Style Visual Basic Array Descriptor
    /// </summary>
    /// <remarks>
    /// This class is used to mimic the binary array descriptor used when an array is serialized
    /// into a file using older Visual Basic applications (VB 6 and prior), this way old VB apps
    /// can still deserialize an array stored in a file written by a .NET application and vice versa.
    /// </remarks>
    public class VBArrayDescriptor : ISupportBinaryImage
    {
        #region [ Members ]

        // Nested Types
        private class DimensionDescriptor
        {
            public const int BinaryLength = 8;

            public int Length;
            public int LowerBound;

            public DimensionDescriptor(int length, int lowerBound)
            {
                Length = length;
                LowerBound = lowerBound;
            }
        }

        // Fields
        private List<DimensionDescriptor> m_arrayDimensionDescriptors;

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
                throw new ArgumentException("Number of lengths and lower bounds must be the same.");
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a serialized version of <see cref="VBArrayDescriptor"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[this.BinaryLength];

                Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors.Count), 0, image, 0, 2);

                for (int i = 0; i < m_arrayDimensionDescriptors.Count; i++)
                {
                    Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors[i].Length), 0, image, (i * DimensionDescriptor.BinaryLength) + 2, 4);
                    Array.Copy(BitConverter.GetBytes(m_arrayDimensionDescriptors[i].LowerBound), 0, image, (i * DimensionDescriptor.BinaryLength) + 6, 4);
                }

                return image;
            }
        }

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

        // Currently not supporting initialization from binary image
        int ISupportBinaryImage.Initialize(byte[] binaryImage, int startIndex, int length)
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
            return new VBArrayDescriptor(new int[] { arrayLength }, new int[] { 0 });
        }

        /// <summary>
        /// Returns a <see cref="VBArrayDescriptor"/> object for a one dimensional array with one-based index.
        /// </summary>
        /// <param name="arrayLength">Length of the array.</param>
        /// <returns>A <see cref="VBArrayDescriptor"/> object.</returns>
        public static VBArrayDescriptor OneBasedOneDimensionalArray(int arrayLength)
        {
            return new VBArrayDescriptor(new int[] { arrayLength }, new int[] { 1 });
        }

        /// <summary>
        /// Returns a <see cref="VBArrayDescriptor"/> object for a two dimensional array with zero-based index.
        /// </summary>
        /// <param name="dimensionOneLength">Length of array in dimension one.</param>
        /// <param name="dimensionTwoLength">Length of array in dimension two.</param>
        /// <returns>A <see cref="VBArrayDescriptor"/> object.</returns>
        public static VBArrayDescriptor ZeroBasedTwoDimensionalArray(int dimensionOneLength, int dimensionTwoLength)
        {
            return new VBArrayDescriptor(new int[] { dimensionOneLength, dimensionTwoLength }, new int[] { 0, 0 });
        }

        /// <summary>
        /// Returns a <see cref="VBArrayDescriptor"/> object for a two dimensional array with one-based index.
        /// </summary>
        /// <param name="dimensionOneLength">Length of array in dimension one.</param>
        /// <param name="dimensionTwoLength">Length of array in dimension two.</param>
        /// <returns>A <see cref="VBArrayDescriptor"/> object.</returns>
        public static VBArrayDescriptor OneBasedTwoDimensionalArray(int dimensionOneLength, int dimensionTwoLength)
        {
            return new VBArrayDescriptor(new int[] { dimensionOneLength, dimensionTwoLength }, new int[] { 1, 1 });
        }

        #endregion
    }
}