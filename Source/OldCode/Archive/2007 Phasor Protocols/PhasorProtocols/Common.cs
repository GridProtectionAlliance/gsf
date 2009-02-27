//*******************************************************************************************************
//  Common.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using PCS;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Common constants and functions for phasor classes.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Typical data stream synchrnonization byte.
        /// </summary>
        public const byte SyncByte = 0xAA;

        /// <summary>
        /// Undefined measurement key.
        /// </summary>
        public static MeasurementKey UndefinedKey = new MeasurementKey(-1, "__");

        /// <summary>
        /// This is a common optimized block copy function for any kind of data.
        /// </summary>
        /// <param name="channel">Source channel with BinaryImage data to copy.</param>
        /// <param name="destination">Destination buffer to hold copied buffer data.</param>
        /// <param name="index">
        /// Index into <paramref name="destination"/> buffer to begin copy. Index is automatically incremented by <see cref="IChannel.BinaryLength"/>.
        /// </param>
        /// <remarks>
        /// This function automatically advances index for convenience.
        /// </remarks>
        public static void CopyImage(this IChannel channel, byte[] destination, ref int index)
        {
            CopyImage(channel.BinaryImage, destination, ref index, channel.BinaryLength);
        }

        /// <summary>
        /// This is a common optimized block copy function for binary data.
        /// </summary>
        /// <param name="source">Source buffer to copy data from.</param>
        /// <param name="destination">Destination buffer to hold copied buffer data.</param>
        /// <param name="index">Index into <paramref name="destination"/> buffer to begin copy.  Index is automatically incremented by <paramref name="length"/>.</param>
        /// <param name="length">Number of bytes to copy from source.</param>
        /// <remarks>
        /// Source index is always zero so hence not requested. This function automatically advances index for convenience.
        /// </remarks>
        public static void CopyImage(this byte[] source, byte[] destination, ref int index, int length)
        {
            if (length > 0)
            {
                Buffer.BlockCopy(source, 0, destination, index, length);
                index += length;
            }
        }

        /// <summary>
        /// Removes duplicate white space, control characters and null from a string.
        /// </summary>
        /// <remarks>
        /// Strings reported from field devices can be full of inconsistencies, this function "cleans-up" the strings for visualization.
        /// </remarks>
        public static string GetValidLabel(this string value)
        {
            return value.RemoveNull().ReplaceControlCharacters().RemoveDuplicateWhiteSpace().Trim();
        }

        /// <summary>
        /// Returns display friendly protocol name.
        /// </summary>
        public static string GetFormattedProtocolName(this PhasorProtocol protocol)
        {
            switch (protocol)
            {
                case PhasorProtocol.IeeeC37_118V1:
                    return "IEEE C37.118-2005";
                case PhasorProtocol.IeeeC37_118D6:
                    return "IEEE C37.118 Draft 6";
                case PhasorProtocol.Ieee1344:
                    return "IEEE 1344-1995";
                case PhasorProtocol.BpaPdcStream:
                    return "BPA PDCstream";
                case PhasorProtocol.FNet:
                    return "Virginia Tech FNET";
                default:
                    return Enum.GetName(typeof(PhasorProtocol), protocol).Replace('_', '.').ToUpper();
            }
        }
    }
}