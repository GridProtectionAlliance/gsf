//******************************************************************************************************
//  TSSCDecoder.cpp - Gbtc
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

#include "TSSCDecoder.h"
#include "Constants.h"

using namespace std;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

uint32_t Decode7BitUInt32(const uint8_t* stream, uint32_t& position);
uint64_t Decode7BitUInt64(const uint8_t* stream, uint32_t& position);

TSSCDecoder::TSSCDecoder() :
    m_data(nullptr),
    m_position(0),
    m_lastPosition(0),
    m_prevTimestamp1(0LL),
    m_prevTimestamp2(0LL),
    m_prevTimeDelta1(Int64::MaxValue),
    m_prevTimeDelta2(Int64::MaxValue),
    m_prevTimeDelta3(Int64::MaxValue),
    m_prevTimeDelta4(Int64::MaxValue),    
    m_bitStreamCount(0),
    m_bitStreamCache(0)
{
    m_lastPoint = NewTSSCPointMetadata();
}

void TSSCDecoder::Reset()
{
    m_data = nullptr;
    m_points.clear();
    m_lastPoint = NewTSSCPointMetadata();
    m_position = 0;
    m_lastPosition = 0;
    ClearBitStream();
    m_prevTimeDelta1 = Int64::MaxValue;
    m_prevTimeDelta2 = Int64::MaxValue;
    m_prevTimeDelta3 = Int64::MaxValue;
    m_prevTimeDelta4 = Int64::MaxValue;
    m_prevTimestamp1 = 0LL;
    m_prevTimestamp2 = 0LL;
}

TSSCPointMetadataPtr TSSCDecoder::NewTSSCPointMetadata()
{
    return NewSharedPtr<TSSCPointMetadata>([&,this]() { return ReadBit(); }, [&,this]() { return ReadBits5(); });
}

void TSSCDecoder::SetBuffer(uint8_t* data, uint32_t offset, uint32_t length)
{
    ClearBitStream();
    m_data = data;
    m_position = offset;
    m_lastPosition = length;
}

bool TSSCDecoder::TryGetMeasurement(uint16_t& id, int64_t& timestamp, uint32_t& quality, float32_t& value)
{
    if (m_position == m_lastPosition && BitStreamIsEmpty())
    {
        ClearBitStream();
        id = 0;
        timestamp = 0;
        quality = 0;
        value = 0.0F;
        return false;
    }

    //Note: since I will not know the incoming pointID. The most recent
    //      measurement received will be the one that contains the 
    //      coding algorithm for this measurement. Since for the more part
    //      measurements generally have some sort of sequence to them, 
    //      this still ends up being a good enough assumption.

    int32_t code = m_lastPoint->ReadCode();

    if (code == TSSCCodeWords::EndOfStream)
    {
        ClearBitStream();
        id = 0;
        timestamp = 0;
        quality = 0;
        value = 0.0F;
        return false;
    }

    if (code <= TSSCCodeWords::PointIDXOR16)
    {
        DecodePointID(code, m_lastPoint);
        code = m_lastPoint->ReadCode();
        
        if (code < TSSCCodeWords::TimeDelta1Forward)
        {
            stringstream errorMessageStream;

            errorMessageStream << "Expecting code >= ";
            errorMessageStream << static_cast<int>(TSSCCodeWords::TimeDelta1Forward);
            errorMessageStream << " Received ";
            errorMessageStream << static_cast<int>(code);
            errorMessageStream << " at position ";
            errorMessageStream << static_cast<int>(m_position);
            errorMessageStream << " with last position ";
            errorMessageStream << static_cast<int>(m_lastPosition);

            throw SubscriberException(errorMessageStream.str());
        }
    }

    id = m_lastPoint->PrevNextPointId1;    
    TSSCPointMetadataPtr nextPoint = id >= m_points.size() ? nullptr : m_points[id];
    
    if (nextPoint == nullptr)
    {
        nextPoint = NewTSSCPointMetadata();

        if (id >= m_points.size())
            m_points.resize(id + 1, nullptr);

        m_points[id] = nextPoint;
        
        nextPoint->PrevNextPointId1 = static_cast<uint16_t>(id + 1);
    }

    if (code <= TSSCCodeWords::TimeXOR7Bit)
    {
        timestamp = DecodeTimestamp(code);
        code = m_lastPoint->ReadCode();

        if (code < TSSCCodeWords::Quality2)
        {
            stringstream errorMessageStream;

            errorMessageStream << "Expecting code >= ";
            errorMessageStream << static_cast<int>(TSSCCodeWords::Quality2);
            errorMessageStream << " Received ";
            errorMessageStream << static_cast<int>(code);
            errorMessageStream << " at position ";
            errorMessageStream << static_cast<int>(m_position);
            errorMessageStream << " with last position ";
            errorMessageStream << static_cast<int>(m_lastPosition);

            throw SubscriberException(errorMessageStream.str());
        }
    }
    else
    {
        timestamp = m_prevTimestamp1;
    }

    if (code <= TSSCCodeWords::Quality7Bit32)
    {
        quality = DecodeQuality(code, nextPoint);
        code = m_lastPoint->ReadCode();
        
        if (code < TSSCCodeWords::Value1)
        {
            stringstream errorMessageStream;

            errorMessageStream << "Expecting code >= ";
            errorMessageStream << static_cast<int>(TSSCCodeWords::Value1);
            errorMessageStream << " Received ";
            errorMessageStream << static_cast<int>(code);
            errorMessageStream << " at position ";
            errorMessageStream << static_cast<int>(m_position);
            errorMessageStream << " with last position ";
            errorMessageStream << static_cast<int>(m_lastPosition);

            throw SubscriberException(errorMessageStream.str());
        }
    }
    else
    {
        quality = nextPoint->PrevQuality1;
    }

    //Since value will almost always change, 
    //This is not put inside a function call.
    uint32_t valueRaw;

    if (code == TSSCCodeWords::Value1)
    {
        valueRaw = nextPoint->PrevValue1;
    }
    else if (code == TSSCCodeWords::Value2)
    {
        valueRaw = nextPoint->PrevValue2;
        nextPoint->PrevValue2 = nextPoint->PrevValue1;
        nextPoint->PrevValue1 = valueRaw;
    }
    else if (code == TSSCCodeWords::Value3)
    {
        valueRaw = nextPoint->PrevValue3;
        nextPoint->PrevValue3 = nextPoint->PrevValue2;
        nextPoint->PrevValue2 = nextPoint->PrevValue1;
        nextPoint->PrevValue1 = valueRaw;
    }
    else if (code == TSSCCodeWords::ValueZero)
    {
        valueRaw = 0;
        nextPoint->PrevValue3 = nextPoint->PrevValue2;
        nextPoint->PrevValue2 = nextPoint->PrevValue1;
        nextPoint->PrevValue1 = valueRaw;
    }
    else
    {
        switch (code)
        {
            case TSSCCodeWords::ValueXOR4:
                valueRaw = static_cast<uint32_t>(ReadBits4()) ^ nextPoint->PrevValue1;
                break;
            case TSSCCodeWords::ValueXOR8:
                valueRaw = static_cast<uint32_t>(m_data[m_position]) ^ nextPoint->PrevValue1;
                m_position++;
                break;
            case TSSCCodeWords::ValueXOR12:
                valueRaw = static_cast<uint32_t>(ReadBits4()) ^ static_cast<uint32_t>(m_data[m_position] << 4) ^ nextPoint->PrevValue1;
                m_position++;
                break;
            case TSSCCodeWords::ValueXOR16:
                valueRaw = static_cast<uint32_t>(m_data[m_position]) ^ static_cast<uint32_t>(m_data[m_position + 1] << 8) ^ nextPoint->PrevValue1;
                m_position += 2;
                break;
            case TSSCCodeWords::ValueXOR20:
                valueRaw = static_cast<uint32_t>(ReadBits4()) ^ static_cast<uint32_t>(m_data[m_position] << 4) ^ static_cast<uint32_t>(m_data[m_position + 1] << 12) ^ nextPoint->PrevValue1;
                m_position += 2;
                break;
            case TSSCCodeWords::ValueXOR24:
                valueRaw = static_cast<uint32_t>(m_data[m_position]) ^ static_cast<uint32_t>(m_data[m_position + 1] << 8) ^ static_cast<uint32_t>(m_data[m_position + 2] << 16) ^ nextPoint->PrevValue1;
                m_position += 3;
                break;
            case TSSCCodeWords::ValueXOR28:
                valueRaw = static_cast<uint32_t>(ReadBits4()) ^ static_cast<uint32_t>(m_data[m_position] << 4) ^ static_cast<uint32_t>(m_data[m_position + 1] << 12) ^ static_cast<uint32_t>(m_data[m_position + 2] << 20) ^ nextPoint->PrevValue1;
                m_position += 3;
                break;
            case TSSCCodeWords::ValueXOR32:
                valueRaw = static_cast<uint32_t>(m_data[m_position]) ^ static_cast<uint32_t>(m_data[m_position + 1] << 8) ^ static_cast<uint32_t>(m_data[m_position + 2] << 16) ^ static_cast<uint32_t>(m_data[m_position + 3] << 24) ^ nextPoint->PrevValue1;
                m_position += 4;
                break;
            default:
                stringstream errorMessageStream;

                errorMessageStream << "Invalid code received ";
                errorMessageStream << static_cast<int>(code);
                errorMessageStream << " at position ";
                errorMessageStream << static_cast<int>(m_position);
                errorMessageStream << " with last position ";
                errorMessageStream << static_cast<int>(m_lastPosition);

                throw SubscriberException(errorMessageStream.str());
        }

        nextPoint->PrevValue3 = nextPoint->PrevValue2;
        nextPoint->PrevValue2 = nextPoint->PrevValue1;
        nextPoint->PrevValue1 = valueRaw;
    }

    value = *reinterpret_cast<float32_t*>(&valueRaw);
    m_lastPoint = nextPoint;

    return true;
}

void TSSCDecoder::DecodePointID(uint8_t code, const TSSCPointMetadataPtr& lastPoint)
{
    if (code == TSSCCodeWords::PointIDXOR4)
    {
        lastPoint->PrevNextPointId1 ^= static_cast<uint16_t>(ReadBits4());
    }
    else if (code == TSSCCodeWords::PointIDXOR8)
    {
        lastPoint->PrevNextPointId1 ^= static_cast<uint16_t>(m_data[m_position++]);
    }
    else if (code == TSSCCodeWords::PointIDXOR12)
    {
        lastPoint->PrevNextPointId1 ^= static_cast<uint16_t>(ReadBits4());
        lastPoint->PrevNextPointId1 ^= static_cast<uint16_t>(m_data[m_position++] << 4);
    }
    else
    {
        lastPoint->PrevNextPointId1 ^= static_cast<uint16_t>(m_data[m_position++]);
        lastPoint->PrevNextPointId1 ^= static_cast<uint16_t>(m_data[m_position++] << 8);
    }
}

int64_t TSSCDecoder::DecodeTimestamp(uint8_t code)
{
    int64_t timestamp;

    if (code == TSSCCodeWords::TimeDelta1Forward)
    {
        timestamp = m_prevTimestamp1 + m_prevTimeDelta1;
    }
    else if (code == TSSCCodeWords::TimeDelta2Forward)
    {
        timestamp = m_prevTimestamp1 + m_prevTimeDelta2;
    }
    else if (code == TSSCCodeWords::TimeDelta3Forward)
    {
        timestamp = m_prevTimestamp1 + m_prevTimeDelta3;
    }
    else if (code == TSSCCodeWords::TimeDelta4Forward)
    {
        timestamp = m_prevTimestamp1 + m_prevTimeDelta4;
    }
    else if (code == TSSCCodeWords::TimeDelta1Reverse)
    {
        timestamp = m_prevTimestamp1 - m_prevTimeDelta1;
    }
    else if (code == TSSCCodeWords::TimeDelta2Reverse)
    {
        timestamp = m_prevTimestamp1 - m_prevTimeDelta2;
    }
    else if (code == TSSCCodeWords::TimeDelta3Reverse)
    {
        timestamp = m_prevTimestamp1 - m_prevTimeDelta3;
    }
    else if (code == TSSCCodeWords::TimeDelta4Reverse)
    {
        timestamp = m_prevTimestamp1 - m_prevTimeDelta4;
    }
    else if (code == TSSCCodeWords::Timestamp2)
    {
        timestamp = m_prevTimestamp2;
    }
    else
    {
        timestamp = m_prevTimestamp1 ^ static_cast<int64_t>(Decode7BitUInt64(&m_data[0], m_position));
    }

    // Save the smallest delta time
    const int64_t minDelta = abs(m_prevTimestamp1 - timestamp);

    if (minDelta < m_prevTimeDelta4 && minDelta != m_prevTimeDelta1 && minDelta != m_prevTimeDelta2 && minDelta != m_prevTimeDelta3)
    {
        if (minDelta < m_prevTimeDelta1)
        {
            m_prevTimeDelta4 = m_prevTimeDelta3;
            m_prevTimeDelta3 = m_prevTimeDelta2;
            m_prevTimeDelta2 = m_prevTimeDelta1;
            m_prevTimeDelta1 = minDelta;
        }
        else if (minDelta < m_prevTimeDelta2)
        {
            m_prevTimeDelta4 = m_prevTimeDelta3;
            m_prevTimeDelta3 = m_prevTimeDelta2;
            m_prevTimeDelta2 = minDelta;
        }
        else if (minDelta < m_prevTimeDelta3)
        {
            m_prevTimeDelta4 = m_prevTimeDelta3;
            m_prevTimeDelta3 = minDelta;
        }
        else
        {
            m_prevTimeDelta4 = minDelta;
        }
    }

    m_prevTimestamp2 = m_prevTimestamp1;
    m_prevTimestamp1 = timestamp;

    return timestamp;
}

uint32_t TSSCDecoder::DecodeQuality(uint8_t code, const TSSCPointMetadataPtr& nextPoint)
{
    uint32_t quality;

    if (code == TSSCCodeWords::Quality2)
    {
        quality = nextPoint->PrevQuality2;
    }
    else
    {
        quality = Decode7BitUInt32(&m_data[0], m_position);
    }

    nextPoint->PrevQuality2 = nextPoint->PrevQuality1;
    nextPoint->PrevQuality1 = quality;

    return quality;
}

bool TSSCDecoder::BitStreamIsEmpty() const
{
    return m_bitStreamCount == 0;
}

void TSSCDecoder::ClearBitStream()
{
    m_bitStreamCount = 0;
    m_bitStreamCache = 0;
}

int32_t TSSCDecoder::ReadBit()
{
    if (m_bitStreamCount == 0)
    {
        m_bitStreamCount = 8;
        m_bitStreamCache = static_cast<int32_t>(m_data[m_position++]);
    }

    m_bitStreamCount--;
    
    return (m_bitStreamCache >> m_bitStreamCount) & 1;
}

int32_t TSSCDecoder::ReadBits4()
{
    return ReadBit() << 3 | ReadBit() << 2 | ReadBit() << 1 | ReadBit();;
}

int32_t TSSCDecoder::ReadBits5()
{
    return ReadBit() << 4 | ReadBit() << 3 | ReadBit() << 2 | ReadBit() << 1 | ReadBit();;
}

uint32_t Decode7BitUInt32(const uint8_t* stream, uint32_t& position)
{
    stream += position;    
    uint32_t value = *stream;
    
    if (value < 128)
    {
        position++;
        return value;
    }
    
    value ^= (static_cast<uint32_t>(stream[1]) << 7);
    
    if (value < 16384)
    {
        position += 2;
        return value ^ 0x80;
    }
    
    value ^= (static_cast<uint32_t>(stream[2]) << 14);
    
    if (value < 2097152)
    {
        position += 3;
        return value ^ 0x4080;
    }
    
    value ^= (static_cast<uint32_t>(stream[3]) << 21);
    
    if (value < 268435456)
    {
        position += 4;
        return value ^ 0x204080;
    }
    
    value ^= (static_cast<uint32_t>(stream[4])  << 28);
    position += 5;
    
    return value ^ 0x10204080;
}

uint64_t Decode7BitUInt64(const uint8_t* stream, uint32_t& position)
{
    stream += position;
    uint64_t value = *stream;

    if (value < 128UL)
    {
        ++position;
        return value;
    }
    
    value ^= (static_cast<uint64_t>(stream[1]) << 7);
    
    if (value < 16384UL)
    {
        position += 2;
        return value ^ 0x80UL;
    }
    
    value ^= (static_cast<uint64_t>(stream[2]) << 14);
    
    if (value < 2097152UL)
    {
        position += 3;
        return value ^ 0x4080UL;
    }
    
    value ^= (static_cast<uint64_t>(stream[3]) << 21);
    
    if (value < 268435456UL)
    {
        position += 4;
        return value ^ 0x204080UL;
    }
    
    value ^= (static_cast<uint64_t>(stream[4]) << 28);
    
    if (value < 34359738368UL)
    {
        position += 5;
        return value ^ 0x10204080UL;
    }
    
    value ^= (static_cast<uint64_t>(stream[5]) << 35);
    
    if (value < 4398046511104UL)
    {
        position += 6;
        return value ^ 0x810204080UL;
    }
    
    value ^= (static_cast<uint64_t>(stream[6]) << 42);
    
    if (value < 562949953421312UL)
    {
        position += 7;
        return value ^ 0x40810204080UL;
    }
    
    value ^= (static_cast<uint64_t>(stream[7]) << 49);
    
    if (value < 72057594037927936UL)
    {
        position += 8;
        return value ^ 0x2040810204080UL;
    }
    
    value ^= (static_cast<uint64_t>(stream[8]) << 56);    
    position += 9;

    return value ^ 0x102040810204080UL;
}