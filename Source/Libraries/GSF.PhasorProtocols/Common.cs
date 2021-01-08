//******************************************************************************************************
//  Common.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  04/21/2010 - J.Ritchie Carroll
//       Added GetFormattedSignalTypeName signal type enumeration extension.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Soap;
using GSF.Parsing;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Common constants, functions and extensions for phasor classes.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Typical data stream synchronization byte.
        /// </summary>
        public const byte SyncByte = 0xAA;

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
        /// This is a common optimized block copy function for any kind of data.
        /// </summary>
        /// <param name="channel">Source channel with BinaryImage data to copy.</param>
        /// <param name="destination">Destination buffer to hold copied buffer data.</param>
        /// <param name="index">
        /// Index into <paramref name="destination"/> buffer to begin copy. Index is automatically incremented by <see cref="ISupportBinaryImage.BinaryLength"/>.
        /// </param>
        /// <remarks>
        /// This function automatically advances index for convenience.
        /// </remarks>
        public static void CopyImage(this ISupportBinaryImage channel, byte[] destination, ref int index)
        {
            index += channel.GenerateBinaryImage(destination, index);
        }

        /// <summary>
        /// Deserializes a configuration frame from an XML file.
        /// </summary>
        /// <param name="configFileName">Path and file name of XML configuration file.</param>
        /// <returns>Deserialized <see cref="IConfigurationFrame"/>.</returns>
        public static IConfigurationFrame DeserializeConfigurationFrame(string configFileName)
        {
            IConfigurationFrame configFrame;
            FileStream configFile = null;

            try
            {
                configFile = File.Open(configFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                configFrame = DeserializeConfigurationFrame(configFile);
            }
            finally
            {
                configFile?.Close();
            }

            return configFrame;
        }

        /// <summary>
        /// Deserializes a configuration frame from an XML stream.
        /// </summary>
        /// <param name="configStream"><see cref="Stream"/> that contains an XML serialized configuration frame.</param>
        /// <returns>Deserialized <see cref="IConfigurationFrame"/>.</returns>
        public static IConfigurationFrame DeserializeConfigurationFrame(Stream configStream)
        {
            IConfigurationFrame configFrame;
            SoapFormatter xmlSerializer = new SoapFormatter();

            xmlSerializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
            xmlSerializer.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
            xmlSerializer.Binder = Serialization.LegacyBinder;

            //configFrame = xmlSerializer.Deserialize(new MemoryStream(Encoding.Default.GetBytes(xmlFile))) as IConfigurationFrame;
            configFrame = xmlSerializer.Deserialize(configStream) as IConfigurationFrame;

            return configFrame;
        }

        /// <summary>
        /// Removes control characters and null from a string.
        /// </summary>
        /// <param name="value">Source <see cref="String"/> to validate.</param>
        /// <remarks>
        /// Strings reported from field devices can be full of inconsistencies, this function helps clean-up the strings.
        /// </remarks>
        /// <returns><paramref name="value"/> with control characters and nulls removed.</returns>
        public static string GetValidLabel(this string value)
        {
            return value.RemoveNull().ReplaceControlCharacters().Trim();
        }

        /// <summary>
        /// Returns display friendly protocol name.
        /// </summary>
        /// <param name="protocol"><see cref="PhasorProtocol"/> to return display name for.</param>
        /// <returns>Friendly protocol display name for specified phasor <paramref name="protocol"/>.</returns>
        public static string GetFormattedProtocolName(this PhasorProtocol protocol)
        {
            switch (protocol)
            {
                case PhasorProtocol.IEEEC37_118V2:
                    return "IEEE C37.118.2-2011";
                case PhasorProtocol.IEEEC37_118V1:
                    return "IEEE C37.118-2005";
                case PhasorProtocol.IEEEC37_118D6:
                    return "IEEE C37.118 Draft 6";
                case PhasorProtocol.IEEE1344:
                    return "IEEE 1344-1995";
                case PhasorProtocol.BPAPDCstream:
                    return "BPA PDCstream";
                case PhasorProtocol.FNET:
                    return "UTK F-NET";
                case PhasorProtocol.SelFastMessage:
                    return "SEL Fast Message";
                case PhasorProtocol.Macrodyne:
                    return "Macrodyne";
                case PhasorProtocol.IEC61850_90_5:
                    return "IEC 61850-90-5";
                default:
                    return protocol.ToString().Replace('_', '.').ToUpper();
            }
        }
    }
}