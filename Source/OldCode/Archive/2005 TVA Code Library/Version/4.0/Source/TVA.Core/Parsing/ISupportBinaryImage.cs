//*******************************************************************************************************
//  ISupportBinaryImage.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/04/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA.Parsing
{
    /// <summary>
    /// Specifies that this <see cref="System.Type"/> can support production or consumption of a binary image that represents the object.
    /// </summary>
    public interface ISupportBinaryImage
    {
        /// <summary>
        /// Gets the binary image of the object.
        /// </summary>
        byte[] BinaryImage { get; }

        /// <summary>
        /// Gets the length of the binary image.
        /// </summary>
        /// <remarks>
        /// <see cref="BinaryLength"/> should typically be a constant value but does not have to be.
        /// </remarks>
        int BinaryLength { get; }

        /// <summary>
        /// Initializes object from the specified binary image.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initialization.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="binaryImage"/> to be used for initialization.</param>
        /// <param name="length">Valid number of bytes within binary image.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="binaryImage"/> (i.e., the number of bytes parsed).</returns>
        int Initialize(byte[] binaryImage, int startIndex, int length);
    }
}