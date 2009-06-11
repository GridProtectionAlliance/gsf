//*******************************************************************************************************
//  PacketType3.cs
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
//  06/15/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System.Collections.Generic;
using DatAWare.Files;

namespace DatAWare.Packets
{
    /// <summary>
    /// Represents a packet to be used for requesting <see cref="MetadataRecord.Summary"/> for the <see cref="QueryPacketBase.RequestIds"/>.
    /// </summary>
    public class PacketType3 : QueryPacketBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketType3"/> class.
        /// </summary>
        public PacketType3()
            : base(3)
        {
            ProcessHandler = Process;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PacketType3"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="PacketType3"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public PacketType3(byte[] binaryImage, int startIndex, int length)
            : this()
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Processes <see cref="PacketType3"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> object containing the binary images of <see cref="MetadataRecord.Summary"/> for the <see cref="QueryPacketBase.RequestIds"/>.</returns>
        protected virtual IEnumerable<byte[]> Process()
        {
            if (Archive == null)
                return null;

            List<byte[]> reply = new List<byte[]>();
            if (RequestIds.Count == 0 || (RequestIds.Count == 1 && RequestIds[0] == -1))
            {
                // Information for all defined records is requested.
                int id = 0;
                while (true)
                {
                    try
                    {
                        reply.Add(Archive.ReadMetaDataSummary(++id));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else
            {
                // Information for specific records is requested.
                foreach (int id in RequestIds)
                {
                    try
                    {
                        reply.Add(Archive.ReadMetaDataSummary(id));
                    }
                    catch
                    {
                    }
                }
            }
            reply.Add(new MetadataRecord(-1).Summary.BinaryImage);   // To indicate EOT.

            return reply;
        }

        #endregion
    }
}