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
    /// Specifies that this <see cref="System.Type"/> can produce or consume a frame of data represented as a binary image.
    /// </summary>
    /// <remarks>
    /// Related types of protocol data that occur as frames in a stream can implement this interface for automated parsing
    /// via the <see cref="FrameImageParserBase"/> class.
    /// </remarks>
    /// <typeparam name="TTypeIdentifier">Type of the frame identifier.</typeparam>
    public interface ISupportFrameImage<TTypeIdentifier> : ISupportBinaryImage
    {
        /// <summary>
        /// Gets or sets current <see cref="ICommonHeader{TTypeIdentifier}"/>.
        /// </summary>
        /// <remarks>
        /// If used, this will need to be set before call to <see cref="ISupportBinaryImage.Initialize"/>.
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