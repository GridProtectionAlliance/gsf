//*******************************************************************************************************
//  Common.vb - Common declarations and functions for phasor classes
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using PCS;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    /// <summary>Common constants and functions for phasor classes</summary>
    [CLSCompliant(false)]
    public static class Common
    {
        /// <summary>Typical data stream synchrnonization byte.</summary>
        public const byte SyncByte = 0xAA;

        /// <summary>Undefined measurement key.</summary>
        public static MeasurementKey UndefinedKey = new MeasurementKey(-1, "__");

        /// <summary>This is a common optimized block copy function for any kind of data.</summary>
        /// <remarks>This function automatically advances index for convenience.</remarks>
        /// <param name="channel">Source channel with BinaryImage data to copy.</param>
        /// <param name="destination">Destination buffer to hold copied buffer data.</param>
        /// <param name="index">Index into <paramref name="destination"/> buffer to begin copy.  Index is automatically incremented by channel's BinaryLength.</param>
        public static void CopyImage(this IChannel channel, byte[] destination, ref int index)
        {
            CopyImage(channel.BinaryImage, destination, ref index, channel.BinaryLength);
        }

        /// <summary>This is a common optimized block copy function for binary data.</summary>
        /// <remarks>
        /// Source index is always zero so hence not requested.
        /// This function automatically advances index for convenience.
        /// </remarks>
        /// <param name="source">Source buffer to copy data from.</param>
        /// <param name="destination">Destination buffer to hold copied buffer data.</param>
        /// <param name="index">Index into <paramref name="destination"/> buffer to begin copy.  Index is automatically incremented by <paramref name="length"/>.</param>
        /// <param name="length">Number of bytes to copy from source.</param>
        public static void CopyImage(this byte[] source, byte[] destination, ref int index, int length)
        {
            if (length > 0)
            {
                Buffer.BlockCopy(source, 0, destination, index, length);
                index += length;
            }
        }

        /// <summary>Removes duplicate white space and control characters from a string.</summary>
        /// <remarks>Strings reported from IED's can be full of inconsistencies, this function "cleans-up" the strings for visualization.</remarks>
        public static string GetValidLabel(string value)
        {
            return value.ReplaceControlCharacters().RemoveDuplicateWhiteSpace().Trim();
        }

        /// <summary>Returns display friendly protocol name.</summary>
        public static string GetFormattedProtocolName(PhasorProtocol protocol)
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