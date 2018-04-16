//******************************************************************************************************
//  TSSCMeasurementParser.cpp - Gbtc
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

#include "TSSCMeasurementParser.h"
#include "Constants.h"

using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

uint32_t Decode7BitUInt32(const uint8_t* stream, volatile int32_t& position);
uint64_t Decode7BitUInt64(const uint8_t* stream, volatile int32_t& position);

TSSCPointMetadata::TSSCPointMetadata(TSSCMeasurementParser* parent) :
    m_parent(parent),
    m_commandsSentSinceLastChange(0),
    m_mode(4),
    m_mode21(0),
    m_mode31(0),
    m_mode301(0),
    m_mode41(TSSCCodeWords::Value1),
    m_mode401(TSSCCodeWords::Value2),
    m_mode4001(TSSCCodeWords::Value3),
    m_startupMode(0),
    PrevNextPointId1(0),
    PrevQuality1(0),
    PrevQuality2(0),
    PrevValue1(0),
    PrevValue2(0),
    PrevValue3(0)
{
}

int32_t TSSCPointMetadata::ReadCode()
{
    int32_t code;

    switch (m_mode)
    {
        case 1:
            code = m_parent->ReadBits5();
            break;
        case 2:
            if (m_parent->ReadBit() == 1)
            {
                code = m_mode21;
            }
            else
            {
                code = m_parent->ReadBits5();
            }
            break;
        case 3:
            if (m_parent->ReadBit() == 1)
            {
                code = m_mode31;
            }
            else if (m_parent->ReadBit() == 1)
            {
                code = m_mode301;
            }
            else
            {
                code = m_parent->ReadBits5();
            }
            break;
        case 4:
            if (m_parent->ReadBit() == 1)
            {
                code = m_mode41;
            }
            else if (m_parent->ReadBit() == 1)
            {
                code = m_mode401;
            }
            else if (m_parent->ReadBit() == 1)
            {
                code = m_mode4001;
            }
            else
            {
                code = m_parent->ReadBits5();
            }
            break;
        default:
            throw SubscriberException("Unsupported compression mode");
    }

    UpdatedCodeStatistics(code);
    return code;
}

void TSSCPointMetadata::UpdatedCodeStatistics(int32_t code)
{
    m_commandsSentSinceLastChange++;
    m_commandStats[code]++;

    if (m_startupMode == 0 && m_commandsSentSinceLastChange > 5)
    {
        m_startupMode++;
        AdaptCommands();
    }
    else if (m_startupMode == 1 && m_commandsSentSinceLastChange > 20)
    {
        m_startupMode++;
        AdaptCommands();
    }
    else if (m_startupMode == 2 && m_commandsSentSinceLastChange > 100)
    {
        AdaptCommands();
    }
}

void TSSCPointMetadata::AdaptCommands()
{
    uint8_t code1 = 0;
    int32_t count1 = 0;

    uint8_t code2 = 1;
    int32_t count2 = 0;

    uint8_t code3 = 2;
    int32_t count3 = 0;

    int32_t total = 0;

    for (int32_t i = 0; i < CommandStatsLength; i++)
    {
        const int32_t count = m_commandStats[i];
        m_commandStats[i] = 0;

        total += count;

        if (count > count3)
        {
            if (count > count1)
            {
                code3 = code2;
                count3 = count2;

                code2 = code1;
                count2 = count1;

                code1 = static_cast<uint8_t>(i);
                count1 = count;
            }
            else if (count > count2)
            {
                code3 = code2;
                count3 = count2;

                code2 = static_cast<uint8_t>(i);
                count2 = count;
            }
            else
            {
                code3 = static_cast<uint8_t>(i);
                count3 = count;
            }
        }
    }

    const int32_t mode1Size = total * 5;
    const int32_t mode2Size = count1 * 1 + (total - count1) * 6;
    const int32_t mode3Size = count1 * 1 + count2 * 2 + (total - count1 - count2) * 7;
    const int32_t mode4Size = count1 * 1 + count2 * 2 + count3 * 3 + (total - count1 - count2 - count3) * 8;

    int32_t minSize = Int32::MaxValue;

    minSize = min(minSize, mode1Size);
    minSize = min(minSize, mode2Size);
    minSize = min(minSize, mode3Size);
    minSize = min(minSize, mode4Size);

    if (minSize == mode1Size)
    {
        m_mode = 1;
    }
    else if (minSize == mode2Size)
    {
        m_mode = 2;
        m_mode21 = code1;
    }
    else if (minSize == mode3Size)
    {
        m_mode = 3;
        m_mode31 = code1;
        m_mode301 = code2;
    }
    else if (minSize == mode4Size)
    {
        m_mode = 4;
        m_mode41 = code1;
        m_mode401 = code2;
        m_mode4001 = code3;
    }
    else
    {
        throw SubscriberException("Coding Error");
    }

    m_commandsSentSinceLastChange = 0;
}

TSSCMeasurementParser::TSSCMeasurementParser() :
    m_data(nullptr),
    m_position(0),
    m_lastPosition(0),
    m_prevTimestamp1(0L),
    m_prevTimestamp2(0L),
    m_prevTimeDelta1(Int64::MaxValue),
    m_prevTimeDelta2(Int64::MaxValue),
    m_prevTimeDelta3(Int64::MaxValue),
    m_prevTimeDelta4(Int64::MaxValue),
    m_bitStreamCount(0),
    m_bitStreamCache(0)
{
    m_lastPoint = NewSharedPtr<TSSCPointMetadata>(this);
}

void TSSCMeasurementParser::Reset()
{
    m_points.clear();
    m_lastPoint = NewSharedPtr<TSSCPointMetadata>(this);
    m_data = nullptr;
    m_position = 0;
    m_lastPosition = 0;
    ClearBitStream();
    m_prevTimeDelta1 = Int64::MaxValue;
    m_prevTimeDelta2 = Int64::MaxValue;
    m_prevTimeDelta3 = Int64::MaxValue;
    m_prevTimeDelta4 = Int64::MaxValue;
    m_prevTimestamp1 = 0L;
    m_prevTimestamp2 = 0L;
}

void TSSCMeasurementParser::SetBuffer(const uint8_t* data, const int32_t offset, const int32_t length)
{
    ClearBitStream();
    m_data = data;
    m_position = offset;
    m_lastPosition = length;
}

bool TSSCMeasurementParser::TryGetMeasurement(uint16_t& id, int64_t& timestamp, uint32_t& quality, float_t& value)
{
    if (m_position == m_lastPosition && BitStreamIsEmpty())
    {
        ClearBitStream();
        id = 0;
        timestamp = 0;
        quality = 0;
        value = 0;
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
        value = 0;
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
        nextPoint = NewSharedPtr<TSSCPointMetadata>(this);
        
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

                errorMessageStream << "Invalid code received  ";
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

    value = *reinterpret_cast<float_t*>(&valueRaw);
    m_lastPoint = nextPoint;

    return true;
}

void TSSCMeasurementParser::DecodePointID(uint8_t code, const TSSCPointMetadataPtr& lastPoint)
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

int64_t TSSCMeasurementParser::DecodeTimestamp(uint8_t code)
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
        timestamp = m_prevTimestamp1 ^ static_cast<int64_t>(Decode7BitUInt64(m_data, m_position));
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

uint32_t TSSCMeasurementParser::DecodeQuality(uint8_t code, const TSSCPointMetadataPtr& nextPoint)
{
    uint32_t quality;

    if (code == TSSCCodeWords::Quality2)
    {
        quality = nextPoint->PrevQuality2;
    }
    else
    {
        quality = Decode7BitUInt32(m_data, m_position);
    }

    nextPoint->PrevQuality2 = nextPoint->PrevQuality1;
    nextPoint->PrevQuality1 = quality;

    return quality;
}

bool TSSCMeasurementParser::BitStreamIsEmpty() const
{
    return m_bitStreamCount == 0;
}

void TSSCMeasurementParser::ClearBitStream()
{
    m_bitStreamCount = 0;
    m_bitStreamCache = 0;
}

int32_t TSSCMeasurementParser::ReadBit()
{
    if (m_bitStreamCount == 0)
    {
        m_bitStreamCount = 8;
        m_bitStreamCache = static_cast<int32_t>(m_data[m_position++]);
    }

    m_bitStreamCount--;
    
    return (m_bitStreamCache >> m_bitStreamCount) & 1;
}

int32_t TSSCMeasurementParser::ReadBits4()
{
    int32_t bits = ReadBit() << 3;

    bits |= ReadBit() << 2;
    bits |= ReadBit() << 1;
    bits |= ReadBit();

    return bits;
}

int32_t TSSCMeasurementParser::ReadBits5()
{
    int32_t bits = ReadBit() << 4;

    bits |= ReadBit() << 3;
    bits |= ReadBit() << 2;
    bits |= ReadBit() << 1;
    bits |= ReadBit();

    return bits;
}

uint32_t Decode7BitUInt32(const uint8_t* stream, volatile int32_t& position)
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

uint64_t Decode7BitUInt64(const uint8_t* stream, volatile int32_t& position)
{
    stream += position;
    uint64_t value = *stream;

    if (value < 128UL)
    {
        position++;
        return value;
    }
    
    value ^= (static_cast<int64_t>(stream[1]) << 7);
    
    if (value < 16384UL)
    {
        position += 2;
        return value ^ 0x80UL;
    }
    
    value ^= (static_cast<int64_t>(stream[2]) << 14);
    
    if (value < 2097152UL)
    {
        position += 3;
        return value ^ 0x4080UL;
    }
    
    value ^= (static_cast<int64_t>(stream[3]) << 21);
    
    if (value < 268435456UL)
    {
        position += 4;
        return value ^ 0x204080UL;
    }
    
    value ^= (static_cast<int64_t>(stream[4]) << 28);
    
    if (value < 34359738368UL)
    {
        position += 5;
        return value ^ 0x10204080UL;
    }
    
    value ^= (static_cast<int64_t>(stream[5]) << 35);
    
    if (value < 4398046511104UL)
    {
        position += 6;
        return value ^ 0x810204080UL;
    }
    
    value ^= (static_cast<int64_t>(stream[6]) << 42);
    
    if (value < 562949953421312UL)
    {
        position += 7;
        return value ^ 0x40810204080UL;
    }
    
    value ^= (static_cast<int64_t>(stream[7]) << 49);
    
    if (value < 72057594037927936UL)
    {
        position += 8;
        return value ^ 0x2040810204080UL;
    }
    
    value ^= (static_cast<int64_t>(stream[8]) << 56);    
    position += 9;

    return value ^ 0x102040810204080UL;
}