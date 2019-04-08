//******************************************************************************************************
//  Constants.h - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
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

#include "../Common/CommonTypes.h"

namespace GSF {
namespace TimeSeries
{
    // Measurement state flags.
    enum class MeasurementStateFlags : unsigned int
    {
        // Defines normal state.
        Normal = 0x0,
        // Defines bad data state.
        BadData = 0x1,
        // Defines suspect data state.
        SuspectData = 0x2,
        // Defines over range error, i.e., unreasonable high value.
        OverRangeError = 0x4,
        // Defines under range error, i.e., unreasonable low value.
        UnderRangeError = 0x8,
        // Defines alarm for high value.
        AlarmHigh = 0x10,
        // Defines alarm for low value.
        AlarmLow = 0x20,
        // Defines warning for high value.
        WarningHigh = 0x40,
        // Defines warning for low value.
        WarningLow = 0x80,
        // Defines alarm for flat-lined value, i.e., latched value test alarm.
        FlatlineAlarm = 0x100,
        // Defines comparison alarm, i.e., outside threshold of comparison with a real-time value.
        ComparisonAlarm = 0x200,
        // Defines rate-of-change alarm.
        ROCAlarm = 0x400,
        // Defines bad value received.
        ReceivedAsBad = 0x800,
        // Defines calculated value state.
        CalculatedValue = 0x1000,
        // Defines calculation error with the value.
        CalculationError = 0x2000,
        // Defines calculation warning with the value.
        CalculationWarning = 0x4000,
        // Defines reserved quality flag.
        ReservedQualityFlag = 0x8000,
        // Defines bad time state.
        BadTime = 0x10000,
        // Defines suspect time state.
        SuspectTime = 0x20000,
        // Defines late time alarm.
        LateTimeAlarm = 0x40000,
        // Defines future time alarm.
        FutureTimeAlarm = 0x80000,
        // Defines up-sampled state.
        UpSampled = 0x100000,
        // Defines down-sampled state.
        DownSampled = 0x200000,
        // Defines discarded value state.
        DiscardedValue = 0x400000,
        // Defines reserved time flag.
        ReservedTimeFlag = 0x800000,
        // Defines user defined flag 1.
        UserDefinedFlag1 = 0x1000000,
        // Defines user defined flag 2.
        UserDefinedFlag2 = 0x2000000,
        // Defines user defined flag 3.
        UserDefinedFlag3 = 0x4000000,
        // Defines user defined flag 4.
        UserDefinedFlag4 = 0x8000000,
        // Defines user defined flag 5.
        UserDefinedFlag5 = 0x10000000,
        // Defines system error state.
        SystemError = 0x20000000,
        // Defines system warning state.
        SystemWarning = 0x40000000,
        // Defines measurement error flag.
        MeasurementError = 0x80000000
    };

    MeasurementStateFlags operator &(MeasurementStateFlags lhs, MeasurementStateFlags rhs);
    MeasurementStateFlags operator ^(MeasurementStateFlags lhs, MeasurementStateFlags rhs);
    MeasurementStateFlags operator ~(MeasurementStateFlags rhs);
    MeasurementStateFlags& operator |=(MeasurementStateFlags &lhs, MeasurementStateFlags rhs);
    MeasurementStateFlags& operator &=(MeasurementStateFlags &lhs, MeasurementStateFlags rhs);
    MeasurementStateFlags& operator ^=(MeasurementStateFlags &lhs, MeasurementStateFlags rhs);
}}

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    // Common constants.
    struct Common
    {
        static const size_t MaxPacketSize = 32768U;
        static const uint32_t PayloadHeaderSize = 8U;
        static const uint32_t ResponseHeaderSize = 6U;
    };

    // DataPublisher data packet flags.
    struct DataPacketFlags
    {
        // Determines if data packet is synchronized. Bit set = synchronized, bit clear = unsynchronized.
        static const uint8_t Synchronized = 0x01;
        // Determines if serialized measurement is compact. Bit set = compact, bit clear = full fidelity.
        static const uint8_t Compact = 0x02;
        // Determines which cipher index to use when encrypting data packet. Bit set = use odd cipher index (i.e., 1), bit clear = use even cipher index (i.e., 0).
        static const uint8_t CipherIndex = 0x04;
        // Determines if data packet payload is compressed. Bit set = payload compressed, bit clear = payload normal.
        static const uint8_t Compressed = 0x08;
        // Determines if the compressed data payload is in little-endian order. Bit set = little-endian order compression, bit clear = big-endian order compression.
        static const uint8_t LittleEndianCompression = 0x10;
        // No flags set. This would represent unsynchronized, full fidelity measurement data packets.
        static const uint8_t NoFlags = 0x00;
    };

    // Server commands received by DataPublisher and sent by DataSubscriber.
    struct ServerCommand
    {
        // Solicited server commands will receive a ServerResponse.Succeeded or ServerResponse.Failed response
        // code along with an associated success or failure message. Message type for successful responses will
        // be based on server command - for example, server response for a successful MetaDataRefresh command
        // will return a serialized DataSet of the available server metadata. Message type for failed responses
        // will always be a string of text representing the error message.

        // Authenticate command. Deprecated - use TLS instead.
        static const uint8_t Authenticate = 0x00;
        // Meta data refresh command. Requests that server send an updated set of metadata so client can refresh its point list.
        static const uint8_t MetadataRefresh = 0x01;
        // Subscribe command. Requests a subscription of streaming data from server based on connection string that follows.
        static const uint8_t Subscribe = 0x02;
        // Unsubscribe command. Requests that server stop sending streaming data to the client and cancel the current subscription.
        static const uint8_t Unsubscribe = 0x03;
        // Rotate cipher keys. Manually requests that server send a new set of cipher keys for data packet encryption.
        static const uint8_t RotateCipherKeys = 0x04;
        // Update processing interval. Manually requests server to update the processing interval with the following specified value.
        static const uint8_t UpdateProcessingInterval = 0x05;
        // Define operational modes for subscriber connection. As soon as connection is established, requests that server set operational modes that affect how the subscriber and publisher will communicate.
        static const uint8_t DefineOperationalModes = 0x06;
        // Confirm receipt of a notification. This message is sent in response to ServerResponse.Notify.
        static const uint8_t ConfirmNotification = 0x07;
        // Confirm receipt of a buffer block measurement. This message is sent in response to ServerResponse.BufferBlock.
        static const uint8_t ConfirmBufferBlock = 0x08;
        // Codes for handling user-defined commands.
        static const uint8_t UserCommand00 = 0xD0;
        static const uint8_t UserCommand01 = 0xD1;
        static const uint8_t UserCommand02 = 0xD2;
        static const uint8_t UserCommand03 = 0xD3;
        static const uint8_t UserCommand04 = 0xD4;
        static const uint8_t UserCommand05 = 0xD5;
        static const uint8_t UserCommand06 = 0xD6;
        static const uint8_t UserCommand07 = 0xD7;
        static const uint8_t UserCommand08 = 0xD8;
        static const uint8_t UserCommand09 = 0xD9;
        static const uint8_t UserCommand10 = 0xDA;
        static const uint8_t UserCommand11 = 0xDB;
        static const uint8_t UserCommand12 = 0xDC;
        static const uint8_t UserCommand13 = 0xDD;
        static const uint8_t UserCommand14 = 0xDE;
        static const uint8_t UserCommand15 = 0xDF;
    };

    // Although the server commands and responses will be on two different paths, the response enumeration values
    // are defined as distinct from the command values to make it easier to identify codes from a wire analysis.

    // Server responses sent by DataPublisher and received by DataSubscriber.
    struct ServerResponse
    {
        // Command succeeded response. Informs client that its solicited server command succeeded, original command and success message follow.
        static const uint8_t Succeeded = 0x80;
        // Command failed response. Informs client that its solicited server command failed, original command and failure message follow.
        static const uint8_t Failed = 0x81;
        // Data packet response. Unsolicited response informs client that a data packet follows.
        static const uint8_t DataPacket = 0x82;
        // Update signal index cache response. Unsolicited response requests that client update its runtime signal index cache with the one that follows.
        static const uint8_t UpdateSignalIndexCache = 0x83;
        // Update runtime base-timestamp offsets response. Unsolicited response requests that client update its runtime base-timestamp offsets with those that follow.
        static const uint8_t UpdateBaseTimes = 0x84;
        // Update runtime cipher keys response. Response, solicited or unsolicited, requests that client update its runtime data cipher keys with those that follow.
        static const uint8_t UpdateCipherKeys = 0x85;
        // Data start time response packet. Unsolicited response provides the start time of data being processed from the first measurement.
        static const uint8_t DataStartTime = 0x86;
        // Processing complete notification. Unsolicited response provides notification that input processing has completed, typically via temporal constraint.
        static const uint8_t ProcessingComplete = 0x87;
        // Buffer block response. Unsolicited response informs client that a raw buffer block follows.
        static const uint8_t BufferBlock = 0x88;
        // Notify response. Unsolicited response provides a notification message to the client.
        static const uint8_t Notify = 0x89;
        // Configuration changed response. Unsolicited response provides a notification that the publisher's source configuration has changed and that client may want to request a meta-data refresh.
        static const uint8_t ConfigurationChanged = 0x8A;
        // Codes for handling user-defined responses.
        static const uint8_t UserResponse00 = 0xE0;
        static const uint8_t UserResponse01 = 0xE1;
        static const uint8_t UserResponse02 = 0xE2;
        static const uint8_t UserResponse03 = 0xE3;
        static const uint8_t UserResponse04 = 0xE4;
        static const uint8_t UserResponse05 = 0xE5;
        static const uint8_t UserResponse06 = 0xE6;
        static const uint8_t UserResponse07 = 0xE7;
        static const uint8_t UserResponse08 = 0xE8;
        static const uint8_t UserResponse09 = 0xE9;
        static const uint8_t UserResponse10 = 0xEA;
        static const uint8_t UserResponse11 = 0xEB;
        static const uint8_t UserResponse12 = 0xEC;
        static const uint8_t UserResponse13 = 0xED;
        static const uint8_t UserResponse14 = 0xEE;
        static const uint8_t UserResponse15 = 0xEF;
        // No operation keep-alive ping. The command channel can remain quiet for some time, this command allows a period test of client connectivity.
        static const uint8_t NoOP = 0xFF;
    };

    // Operational modes are sent from a subscriber to a publisher to request operational behaviors for the
    // connection, as a result the operation modes must be sent before any other command. The publisher may
    // silently refuse some requests (e.g., compression) based on its configuration. Operational modes only
    // apply to fundamental protocol control.

    // Operational modes that affect how DataPublisher and DataSubscriber communicate.
    struct OperationalModes
    {
        // Mask to get version number of protocol. Version number is currently set to 0.
        static const uint32_t VersionMask = 0x0000001F;
        // Mask to get mode of compression. GZip and TSSC compression are the only modes currently supported. Remaining bits are reserved for future compression modes.
        static const uint32_t CompressionModeMask = 0x000000E0;
        // Mask to get character encoding used when exchanging messages between publisher and subscriber.
        static const uint32_t EncodingMask = 0x00000300;
        // Determines type of serialization to use when exchanging signal index cache and metadata. Bit set = common serialization format, bit clear is deprecated.
        static const uint32_t UseCommonSerializationFormat = 0x01000000;
        // Determines whether external measurements are exchanged during metadata synchronization. Bit set = external measurements are exchanged, bit clear = no external measurements are exchanged.
        static const uint32_t ReceiveExternalMetadata = 0x02000000;
        // Determines whether internal measurements are exchanged during metadata synchronization. Bit set = internal measurements are exchanged, bit clear = no internal measurements are exchanged.
        static const uint32_t ReceiveInternalMetadata = 0x04000000;
        // Determines whether payload data is compressed when exchanging between publisher and subscriber. Bit set = compress, bit clear = no compression.
        static const uint32_t CompressPayloadData = 0x20000000;
        // Determines whether the signal index cache is compressed when exchanging between publisher and subscriber. Bit set = compress, bit clear = no compression.
        static const uint32_t CompressSignalIndexCache = 0x40000000;
        // Determines whether metadata is compressed when exchanging between publisher and subscriber. Bit set = compress, bit clear = no compression.
        static const uint32_t CompressMetadata = 0x80000000;
        // No flags set.
        static const uint32_t NoFlags = 0x00000000;
    };

    // Operational modes are sent from a subscriber to a publisher to request operational behaviors for the
    // connection, as a result the operation modes must be sent before any other command. The publisher may
    // silently refuse some requests (e.g., compression) based on its configuration. Operational modes only
    // apply to fundamental protocol control.

    // Enumeration for character encodings supported by the Gateway Exchange Protocol.
    struct OperationalEncoding
    {
        // UTF-16, little endian
        static const uint32_t Unicode = 0x00000000;
        // UTF-16, big endian
        static const uint32_t BigEndianUnicode = 0x00000100;
        // UTF-8
        static const uint32_t UTF8 = 0x00000200;
        // ANSI
        static const uint32_t ANSI = 0x00000300;
    };

    // Enumeration for compression modes supported by the Gateway Exchange Protocol.
    struct CompressionModes
    {
        // GZip compression
        static const uint32_t GZip = 0x00000020;
        // TSSC compression
        static const uint32_t TSSC = 0x00000040;
        // No compression
        static const uint32_t None = 0x00000000;
    };

    // Security modes used by the DataPublisher to secure data sent over the command channel.
    enum class SecurityMode
    {
        // No security.
        None,
        // Transport Layer Security.
        TLS,
        // Pre-shared key. Deprecated - use TLS instead.
        Gateway
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