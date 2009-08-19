//*******************************************************************************************************
//  PacketCommonHeader.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/21/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using TVA.Parsing;

namespace TVA.Historian.Packets
{
    /// <summary>
    /// Represents the common header information that is present in the binary image of all <see cref="Type"/>s that implement the <see cref="IPacket"/> interface.
    /// </summary>
    public class PacketCommonHeader : CommonHeaderBase<short>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketCommonHeader"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="PacketCommonHeader"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
         public PacketCommonHeader(byte[] binaryImage, int startIndex, int length)
         {
             if (length > 1)
                 TypeID = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex);
             else
                 throw new InvalidOperationException("Binary image is malformed.");
         }
    }
}
