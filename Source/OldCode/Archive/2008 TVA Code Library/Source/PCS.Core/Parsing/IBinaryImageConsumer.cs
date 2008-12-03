//*******************************************************************************************************
//  IBinaryDataConsumer.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/01/2007 - Pinal C. Patel
//       Original version of source code generated.
//  09/10/2008 - J. Ritchie Carroll
//      Converted to C#.
//  10/28/2008 - Pinal C. Patel
//      Edited code comments.
//
//*******************************************************************************************************

using System;

namespace PCS.Parsing
{
    /// <summary>
    /// Specifies that this <see cref="System.Type"/> can initialize objects from a binary image.
    /// </summary>
    /// <typeparam name="TTypeIdentifier">Type of the identifier.</typeparam>
    public interface IBinaryImageConsumer<TTypeIdentifier>
    {
        /// <summary>
        /// Initializes object from the specified binary image.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initialization.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="binaryImage"/> to be used for initialization.</param>
        /// <param name="length">Valid number of bytes within binary image.</param>
        /// <returns>The number of bytes used for initialization.</returns>
        int Initialize(byte[] binaryImage, int startIndex, int length);

        /// <summary>
        /// Gets or sets current <see cref="ICommonHeader{TTypeIdentifier}"/>.
        /// </summary>
        /// <remarks>
        /// If used, this will need to be set before call to <see cref="Initialize(byte[],int,int)"/>.
        /// </remarks>
        ICommonHeader<TTypeIdentifier> CommonHeader { get; set; }

        /// <summary>
        /// Gets the identifier that can be used for identifying the <see cref="Type"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="TypeID"/> must be unique across all siblings implementing a common <see cref="Type"/> or interface.
        /// </para>
        /// <para>
        /// Output types implement this class so they have a consistently addressable identification property.
        /// </para>
        /// </remarks>
        TTypeIdentifier TypeID { get; }
    }
}