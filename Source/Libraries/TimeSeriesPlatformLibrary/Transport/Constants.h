//******************************************************************************************************
//  Constants.h - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/29/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __CONSTANTS_H
#define __CONSTANTS_H

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    struct DataPacketFlags
    {
        static const uint8_t Synchronized = 0x01;
        static const uint8_t Compact = 0x02;
        static const uint8_t CipherIndex = 0x04;
        static const uint8_t Compressed = 0x08;
        static const uint8_t NoFlags = 0x00;
    };

    struct ServerCommand
    {
        static const uint8_t Authenticate = 0x00;
        static const uint8_t MetadataRefresh = 0x01;
        static const uint8_t Subscribe = 0x02;
        static const uint8_t Unsubscribe = 0x03;
        static const uint8_t RotateCipherKeys = 0x04;
        static const uint8_t UpdateProcessingInterval = 0x05;
        static const uint8_t DefineOperationalModes = 0x06;
        static const uint8_t ConfirmNotification = 0x07;
        static const uint8_t ConfirmBufferBlock = 0x08;
        static const uint8_t PublishCommandMeasurements = 0x09;
    };

    struct ServerResponse
    {
        static const uint8_t Succeeded = 0x80;
        static const uint8_t Failed = 0x81;
        static const uint8_t DataPacket = 0x82;
        static const uint8_t UpdateSignalIndexCache = 0x83;
        static const uint8_t UpdateBaseTimes = 0x84;
        static const uint8_t UpdateCipherKeys = 0x85;
        static const uint8_t DataStartTime = 0x86;
        static const uint8_t ProcessingComplete = 0x87;
        static const uint8_t BufferBlock = 0x88;
        static const uint8_t Notify = 0x89;
        static const uint8_t ConfigurationChanged = 0x8A;
        static const uint8_t NoOP = 0xFF;
    };

    struct OperationalModes
    {
        static const uint32_t VersionMask = 0x0000001F;
        static const uint32_t CompressionModeMask = 0x000000E0;
        static const uint32_t EncodingMask = 0x00000300;
        static const uint32_t UseCommonSerializationFormat = 0x01000000;
        static const uint32_t ReceiveExternalMetadata = 0x02000000;
        static const uint32_t ReceiveInternalMetadata = 0x04000000;
        static const uint32_t CompressPayloadData = 0x20000000;
        static const uint32_t CompressSignalIndexCache = 0x40000000;
        static const uint32_t CompressMetadata = 0x80000000;
        static const uint32_t NoFlags = 0x00000000;
    };

    struct OperationalEncoding
    {
        static const uint32_t Unicode = 0x00000000;
        static const uint32_t BigEndianUnicode = 0x00000100;
        static const uint32_t UTF8 = 0x00000200;
        static const uint32_t ANSI = 0x00000300;
    };

    struct CompressionModes
    {
        static const uint32_t GZip = 0x00000020;
        static const uint32_t TSSC = 0x00000040;
        static const uint32_t None = 0x00000000;
    };

    // The encoding commands supported by TSSC
    struct TSSCCodeWords
    {
        static const uint8_t EndOfStream = 0;

        static const uint8_t PointIDXOR4 = 1;
        static const uint8_t PointIDXOR8 = 2;
        static const uint8_t PointIDXOR12 = 3;
        static const uint8_t PointIDXOR16 = 4;

        static const uint8_t TimeDelta1Forward = 5;
        static const uint8_t TimeDelta2Forward = 6;
        static const uint8_t TimeDelta3Forward = 7;
        static const uint8_t TimeDelta4Forward = 8;
        static const uint8_t TimeDelta1Reverse = 9;
        static const uint8_t TimeDelta2Reverse = 10;
        static const uint8_t TimeDelta3Reverse = 11;
        static const uint8_t TimeDelta4Reverse = 12;
        static const uint8_t Timestamp2 = 13;
        static const uint8_t TimeXOR7Bit = 14;

        static const uint8_t Quality2 = 15;
        static const uint8_t Quality7Bit32 = 16;

        static const uint8_t Value1 = 17;
        static const uint8_t Value2 = 18;
        static const uint8_t Value3 = 19;
        static const uint8_t ValueZero = 20;
        static const uint8_t ValueXOR4 = 21;
        static const uint8_t ValueXOR8 = 22;
        static const uint8_t ValueXOR12 = 23;
        static const uint8_t ValueXOR16 = 24;
        static const uint8_t ValueXOR20 = 25;
        static const uint8_t ValueXOR24 = 26;
        static const uint8_t ValueXOR28 = 27;
        static const uint8_t ValueXOR32 = 28;
    };
}}}

#endif