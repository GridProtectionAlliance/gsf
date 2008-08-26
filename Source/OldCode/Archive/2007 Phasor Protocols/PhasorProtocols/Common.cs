//*******************************************************************************************************
//  Common.vb - Common declarations and functions for phasor classes
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
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
using TVA;
using TVA.Interop;
using TVA.Measurements;

namespace PhasorProtocols
{
    /// <summary>Common constants and functions for phasor classes</summary>
    [CLSCompliant(false)]
    public sealed class Common
    {
        private Common()
        {
            // This class contains only global functions and is not meant to be instantiated
        }

        /// <summary>Typical data stream synchrnonization byte</summary>
        public const byte SyncByte = 0xAA;

        /// <summary>Undefined measurement key</summary>
        public static MeasurementKey UndefinedKey = new MeasurementKey(-1, "__");

        /// <summary>This is a common optimized block copy function for any kind of data</summary>
        /// <remarks>This function automatically advances index for convenience</remarks>
        public static void CopyImage(IChannel channel, byte[] buffer, ref int index)
        {
            CopyImage(channel.BinaryImage, buffer, ref index, channel.BinaryLength);
        }

        /// <summary>This is a common optimized block copy function for binary data</summary>
        /// <remarks>This function automatically advances index for convenience</remarks>
        public static void CopyImage(byte[] source, byte[] buffer, ref int index, int length)
        {
            if (length > 0)
            {
                Buffer.BlockCopy(source, 0, buffer, index, length);
                index += length;
            }
        }

        /// <summary>Removes duplicate white space and control characters from a string</summary>
        /// <remarks>Strings reported from IED's can be full of inconsistencies, this function "cleans-up" the strings for visualization</remarks>
        public static string GetValidLabel(string value)
        {
            return TVA.Text.Common.RemoveDuplicateWhiteSpace(TVA.Text.Common.ReplaceControlCharacters(value, ' ')).Trim();
        }

        /// <summary>Returns display friendly protocol name</summary>
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
