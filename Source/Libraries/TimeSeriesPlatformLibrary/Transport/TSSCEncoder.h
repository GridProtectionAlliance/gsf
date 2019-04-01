//******************************************************************************************************
//  TSSCEncoder.h - Gbtc
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

#ifndef __TSSC_ENCODER_H
#define __TSSC_ENCODER_H

#include "TransportTypes.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    // Encoder for the compact measurement format of the Gateway Exchange Protocol.
    class TSSCEncoder
    {
    private:
        static constexpr const uint32_t Bits28 = 0xFFFFFFFU;
        static constexpr const uint32_t Bits24 = 0xFFFFFFU;
        static constexpr const uint32_t Bits20 = 0xFFFFFU;
        static constexpr const uint32_t Bits16 = 0xFFFFU;
        static constexpr const uint32_t Bits12 = 0xFFFU;
        static constexpr const uint32_t Bits8 = 0xFFU;
        static constexpr const uint32_t Bits4 = 0xFU;
        static constexpr const uint32_t Bits0 = 0x0U;

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

        // The position in m_buffer where the bit stream should be flushed, -1 means no bit stream position has been assigned.
        int32_t m_bitStreamBufferIndex;

        // The number of bits in m_bitStreamCache that are valid. 0 Means the bitstream is empty.
        int32_t m_bitStreamCacheBitCount;

        // A cache of bits that need to be flushed to m_buffer when full. Bits filled starting from the right moving left.
        int32_t m_bitStreamCache;

        void WritePointIdChange(uint16_t id);
        void WriteTimestampChange(int64_t timestamp);
        void WriteQualityChange(uint32_t quality, const TSSCPointMetadataPtr& point);

        // Resets the stream so it can be reused. All measurements must be registered again.
        void ClearBitStream();
        void WriteBits(int32_t code, int32_t length);
        void BitStreamFlush();
        void BitStreamEnd();

        TSSCPointMetadataPtr NewTSSCPointMetadata();

    public:
        // Creates a new instance of the TSSC encoder.
        TSSCEncoder();

        // Resets the TSSC Encoder to the initial state. 
        void Reset();

        // Sets the internal buffer to write data to.
        void SetBuffer(uint8_t* data, uint32_t offset, uint32_t length);

        // Finishes the current block and returns position after the last byte written.
        uint32_t FinishBlock();

        // Adds the supplied measurement to the stream. If the stream is full, this method returns false.
        bool TryAddMeasurement(uint16_t id, int64_t timestamp, uint32_t quality, float32_t value);
    };
}}}

#endif