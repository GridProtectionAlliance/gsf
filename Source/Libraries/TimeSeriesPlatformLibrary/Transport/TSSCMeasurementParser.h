//******************************************************************************************************
//  TSSCMeasurementParser.h - Gbtc
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
//  04/11/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __TSSC_MEASUREMENT_PARSER_H
#define __TSSC_MEASUREMENT_PARSER_H

#include "TransportTypes.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class TSSCMeasurementParser;

    // The metadata kept for each pointID.
    class TSSCPointMetadata
    {
    private:
        static const uint8_t CommandStatsLength = 32;

        TSSCMeasurementParser* m_parent;
        uint8_t m_commandStats[CommandStatsLength];
        int32_t m_commandsSentSinceLastChange;

        //Bit codes for the 4 modes of encoding. 
        uint8_t m_mode;

        //(Mode 1 means no prefix.)
        uint8_t m_mode21;

        uint8_t m_mode31;
        uint8_t m_mode301;

        uint8_t m_mode41;
        uint8_t m_mode401;
        uint8_t m_mode4001;

        int32_t m_startupMode;

        void UpdatedCodeStatistics(int32_t code);
        void AdaptCommands();

    public:
        TSSCPointMetadata(TSSCMeasurementParser* parent);

        uint16_t PrevNextPointId1;

        uint32_t PrevQuality1;
        uint32_t PrevQuality2;
        uint32_t PrevValue1;
        uint32_t PrevValue2;
        uint32_t PrevValue3;

        int32_t ReadCode();
    };

    typedef SharedPtr<TSSCPointMetadata> TSSCPointMetadataPtr;

    // Parser for the compact measurement format of the Gateway Exchange Protocol.
    class TSSCMeasurementParser
    {
    private:
        uint8_t* m_data;
        uint32_t m_position;
        uint32_t m_lastPosition;

        int64_t m_prevTimestamp1;
        int64_t m_prevTimestamp2;

        int64_t m_prevTimeDelta1;
        int64_t m_prevTimeDelta2;
        int64_t m_prevTimeDelta3;
        int64_t m_prevTimeDelta4;

        TSSCPointMetadataPtr m_lastPoint;
        std::vector<TSSCPointMetadataPtr> m_points;

        // The number of bits in m_bitStreamCache that are valid. 0 Means the bitstream is empty.
        int32_t m_bitStreamCount;

        // A cache of bits that need to be flushed to m_buffer when full. Bits filled starting from the right moving left.
        int32_t m_bitStreamCache;

        void DecodePointID(uint8_t code, const TSSCPointMetadataPtr& lastPoint);
        int64_t DecodeTimestamp(uint8_t code);
        uint32_t DecodeQuality(uint8_t code, const TSSCPointMetadataPtr& nextPoint);

        bool BitStreamIsEmpty() const;
        void ClearBitStream();

    public:
        // Creates a new instance of the compact measurement parser.
        TSSCMeasurementParser();

        // Resets the TSSC Decoder to the initial state.
        void Reset();

        // Sets the internal buffer to read data from.
        void SetBuffer(uint8_t* data, uint32_t offset, uint32_t length);

        // Reads the next measurement from the stream. If the end of the stream has been encountered, return false.
        bool TryGetMeasurement(uint16_t& id, int64_t& timestamp, uint32_t& quality, float32_t& value);

        int32_t ReadBit();
        int32_t ReadBits4();
        int32_t ReadBits5();
    };
}}}

#endif