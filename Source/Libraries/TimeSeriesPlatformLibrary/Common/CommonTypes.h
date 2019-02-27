//******************************************************************************************************
//  CommonTypes.h - Gbtc
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
//  03/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __COMMON_TYPES_H
#define __COMMON_TYPES_H

#include <cstddef>
#include <map>
#include <unordered_map>
#include <boost/any.hpp>
#include <boost/uuid/uuid.hpp>
#include <boost/exception/exception.hpp>
#include <boost/multiprecision/cpp_dec_float.hpp>
#include <boost/thread.hpp>
#include <boost/thread/condition_variable.hpp>
#include <boost/thread/locks.hpp>
#include <boost/asio.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/asio/ip/udp.hpp>
#include <boost/iostreams/filtering_stream.hpp>
#include <boost/iostreams/filter/gzip.hpp>
#include <boost/iostreams/device/array.hpp>

namespace GSF
{
    // Floating-point types
    typedef float_t float32_t;
    typedef double_t float64_t;
    typedef boost::multiprecision::cpp_dec_float_100 decimal_t;

    // DateTime type
    typedef boost::posix_time::ptime datetime_t;

    struct Int8
    {
        static const int8_t MaxValue = static_cast<int8_t>(127);
        static const int8_t MinValue = static_cast<int8_t>(-128);
    };

    struct UInt8
    {
        static const uint8_t MaxValue = static_cast<uint8_t>(255);
        static const uint8_t MinValue = static_cast<uint8_t>(0);
    };

    struct Int16
    {
        static const int16_t MaxValue = static_cast<int16_t>(32767);
        static const int16_t MinValue = static_cast<int16_t>(-32768);
    };

    struct UInt16
    {
        static const uint16_t MaxValue = static_cast<uint16_t>(65535);
        static const uint16_t MinValue = static_cast<uint16_t>(0);
    };

    struct Int32
    {
        static const int32_t MaxValue = static_cast<int32_t>(2147483647);
        static const int32_t MinValue = static_cast<int32_t>(-2147483647) - 1;
    };

    struct UInt32
    {
        static const uint32_t MaxValue = static_cast<uint32_t>(4294967295U);
        static const uint32_t MinValue = static_cast<uint32_t>(0);
    };

    struct Int64
    {
        static const int64_t MaxValue = static_cast<int64_t>(9223372036854775807L);
        static const int64_t MinValue = static_cast<int64_t>(-9223372036854775807L) - 1L;
    };

    struct UInt64
    {
        static const uint64_t MaxValue = static_cast<uint64_t>(18446744073709551615UL);
        static const uint64_t MinValue = static_cast<uint64_t>(0UL);
    };

    struct Decimal
    {
        static const decimal_t MaxValue;
        static const decimal_t MinValue;

        static const decimal_t DotNetMaxValue;
        static const decimal_t DotNetMinValue;
    };

    struct DateTime
    {
        static const datetime_t MaxValue;
        static const datetime_t MinValue;
    };

    struct Ticks
    {
        static const int64_t MaxValue = 3155378975999999999L;       // 12/31/1999 11:59:59.999
        static const int64_t MinValue = 0L;                         // 01/01/0001 00:00:00.000

        static const int64_t UnixBaseOffset = 621355968000000000L;  // 01/01/1970 00:00:00.000
        static const int64_t PTimeBaseOffset = 441481536000000000L; // 01/01/1400 00:00:00.000

        static const int64_t PerSecond = 10000000L;
        static const int64_t PerMillisecond = Ticks::PerSecond / 1000;
        static const int64_t PerMicrosecond = Ticks::PerSecond / 1000000;
        static const int64_t PerMinute = 60L * Ticks::PerSecond;
        static const int64_t PerHour = 60L * Ticks::PerMinute;
        static const int64_t PerDay = 24L * Ticks::PerHour;
    };

    template<class T>
    using SharedPtr = boost::shared_ptr<T>;

    template<class T>
    using EnableSharedThisPtr = boost::enable_shared_from_this<T>;

    template<class T>
    SharedPtr<T> NewSharedPtr()
    {
        return boost::make_shared<T>();
    }

    template<class T, class P1>
    SharedPtr<T> NewSharedPtr(P1 p1)
    {
        return boost::make_shared<T>(p1);
    }

    template<class T, class P1, class P2>
    SharedPtr<T> NewSharedPtr(P1 p1, P2 p2)
    {
        return boost::make_shared<T>(p1, p2);
    }

    template<class T, class P1, class P2, class P3>
    SharedPtr<T> NewSharedPtr(P1 p1, P2 p2, P3 p3)
    {
        return boost::make_shared<T>(p1, p2, p3);
    }

    template<class T, class P1, class P2, class P3, class P4>
    SharedPtr<T> NewSharedPtr(P1 p1, P2 p2, P3 p3, P4 p4)
    {
        return boost::make_shared<T>(p1, p2, p3, p4);
    }

    template<class T, class P1, class P2, class P3, class P4, class P5>
    SharedPtr<T> NewSharedPtr(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
    {
        return boost::make_shared<T>(p1, p2, p3, p4, p5);
    }

    template<class T, class S>
    SharedPtr<T> CastSharedPtr(SharedPtr<S> const& source)
    {
        return boost::dynamic_pointer_cast<T>(source);
    }

    typedef boost::any Object;
    typedef boost::uuids::uuid Guid;
    typedef boost::posix_time::time_duration TimeSpan;
    typedef boost::system::error_code ErrorCode;
    typedef boost::system::system_error SystemError;
    typedef boost::exception Exception;
    typedef boost::thread Thread;
    typedef boost::mutex Mutex;
    typedef boost::condition_variable WaitHandle;
    typedef boost::lock_guard<Mutex> ScopeLock;
    typedef boost::unique_lock<Mutex> UniqueLock;
    typedef boost::asio::io_context IOContext;
    typedef boost::asio::io_context::strand Strand;
    typedef boost::asio::ip::address IPAddress;
    typedef boost::asio::ip::tcp::socket TcpSocket;
    typedef boost::asio::ip::udp::socket UdpSocket;
    typedef boost::asio::ip::tcp::acceptor TcpAcceptor;
    typedef boost::asio::ip::tcp::endpoint TcpEndPoint;
    typedef boost::asio::ip::tcp::resolver DnsResolver;
    typedef boost::iostreams::filtering_streambuf<boost::iostreams::input> StreamBuffer;
    typedef boost::iostreams::gzip_decompressor GZipDecompressor;
    typedef boost::iostreams::gzip_compressor GZipCompressor;

    // Empty types
    struct Empty
    {
        static const std::string String;
        static const GSF::datetime_t DateTime;
        static const GSF::Guid Guid;
        static const GSF::Object Object;
        static const GSF::IPAddress IPAddress;
    };

    // std::unordered_map string hasher
    struct StringHash : std::unary_function<std::string, size_t>
    {
        size_t operator()(const std::string& value) const;
    };

    // std::unordered_map string equality tester
    struct StringEqual : std::binary_function<std::string, std::string, bool>
    {
        bool operator()(const std::string& left, const std::string& right) const;
    };

    template<class T>
    using StringMap = std::unordered_map<std::string, T, GSF::StringHash, GSF::StringEqual>;

    // std::map string comparer
    struct StringComparer : std::binary_function<std::string, std::string, bool>
    {
        bool operator()(const std::string& left, const std::string& right) const;
    };

    template<class T>
    using SortedStringMap = std::map<std::string, T, GSF::StringComparer>;

    template<class TKey, class TValue>
    bool TryGetValue(const std::map<TKey, TValue>& dictionary, const TKey& key, TValue& value, const TValue& defaultValue)
    {
        auto iterator = dictionary.find(key);

        if (iterator != dictionary.end())
        {
            value = iterator->second;
            return true;
        }

        value = defaultValue;
        return false;
    }

    template<class TKey, class TValue>
    bool TryGetValue(const std::unordered_map<TKey, TValue>& dictionary, const TKey& key, TValue& value, const TValue& defaultValue)
    {
        auto iterator = dictionary.find(key);

        if (iterator != dictionary.end())
        {
            value = iterator->second;
            return true;
        }

        value = defaultValue;
        return false;
    }

    template<class TValue>
    bool TryGetValue(const StringMap<TValue>& dictionary, const std::string& key, TValue& value, const TValue& defaultValue)
    {
        auto iterator = dictionary.find(key);

        if (iterator != dictionary.end())
        {
            value = iterator->second;
            return true;
        }

        value = defaultValue;
        return false;
    }

    inline bool TryGetValue(const StringMap<std::string>& dictionary, const std::string& key, std::string& value)
    {
        return TryGetValue<std::string>(dictionary, key, value, Empty::String);
    }

    template<class T>
    T Cast(const Object& source)
    {
        return boost::any_cast<T>(source);
    }

    struct MemoryStream : boost::iostreams::array_source
    {
        MemoryStream(const std::vector<uint8_t>& buffer) : boost::iostreams::array_source(reinterpret_cast<const char*>(buffer.data()), buffer.size())
        {
        }

        MemoryStream(const uint8_t* buffer, uint32_t offset, uint32_t length) : boost::iostreams::array_source(reinterpret_cast<const char*>(buffer + offset), length)
        {
        }
    };

    template<class T, class TElem = char>
    void CopyStream(T* source, std::vector<uint8_t>& sink)
    {
        std::istreambuf_iterator<TElem> it{ source };
        std::istreambuf_iterator<TElem> eos{};

        for (; it != eos; ++it)
            sink.push_back(static_cast<uint8_t>(*it));
    }

    template<class T, class TElem = char>
    void CopyStream(T& source, std::vector<uint8_t>& sink)
    {
        std::istreambuf_iterator<TElem> it{ source };
        std::istreambuf_iterator<TElem> eos{};

        for (; it != eos; ++it)
            sink.push_back(static_cast<uint8_t>(*it));
    }

    template<class T>
    static uint32_t WriteBytes(std::vector<uint8_t>& buffer, const T& value)
    {
        static const int32_t length = sizeof(T);
        const uint8_t* bytes = reinterpret_cast<const uint8_t*>(&value);

        for (int32_t i = 0; i < length; i++)
            buffer.push_back(bytes[i]);

        return length;
    }

    uint32_t WriteBytes(std::vector<uint8_t>& buffer, const uint8_t* source, const uint32_t offset, const uint32_t length);
    uint32_t WriteBytes(std::vector<uint8_t>& buffer, const std::vector<uint8_t>& source);
    uint32_t WriteBytes(std::vector<uint8_t>& buffer, const Guid& value);

    Guid NewGuid();

    // Handy string functions (boost wrappers)
    bool IsEqual(const std::string& left, const std::string& right, bool ignoreCase = true);
    bool StartsWith(const std::string& value, const std::string& findValue, bool ignoreCase = true);
    bool EndsWith(const std::string& value, const std::string& findValue, bool ignoreCase = true);
    bool Contains(const std::string& value, const std::string& findValue, bool ignoreCase = true);
    int32_t Count(const std::string& value, const std::string& findValue, bool ignoreCase = true);
    int32_t Compare(const std::string& leftValue, const std::string& rightValue, bool ignoreCase = true);
    int32_t IndexOf(const std::string& value, const std::string& findValue, bool ignoreCase = true);
    int32_t IndexOf(const std::string& value, const std::string& findValue, int32_t index, bool ignoreCase = true);
    int32_t LastIndexOf(const std::string& value, const std::string& findValue, bool ignoreCase = true);
    std::vector<std::string> Split(const std::string& value, const std::string& delimiterValue, bool ignoreCase = true);
    std::string Split(const std::string& value, const std::string& delimiterValue, int32_t index, bool ignoreCase = true);
    std::string Replace(const std::string& value, const std::string& findValue, const std::string& replaceValue, bool ignoreCase = true);
    std::string ToUpper(const std::string& value);
    std::string ToLower(const std::string& value);
    std::string Trim(const std::string& value);
    std::string Trim(const std::string& value, const std::string& trimValues);
    std::string TrimRight(const std::string& value);
    std::string TrimRight(const std::string& value, const std::string& trimValues);
    std::string TrimLeft(const std::string& value);
    std::string TrimLeft(const std::string& value, const std::string& trimValues);
    std::string PadLeft(const std::string& value, uint32_t count, char padChar);
    std::string PadRight(const std::string& value, uint32_t count, char padChar);

    #ifndef strcpy_s
    #define strcpy_s(dest, size, src) strncpy(dest, src, size)
    #endif

    // Handy date/time functions (boost wrappers)
    enum class TimeInterval
    {
        Year,
        Month,
        DayOfYear,
        Day,
        Week,
        WeekDay,
        Hour,
        Minute,
        Second,
        Millisecond
    };

    datetime_t DateAdd(const datetime_t& value, int32_t addValue, TimeInterval interval);
    int32_t DateDiff(const datetime_t& startTime, const datetime_t& endTime, TimeInterval interval);
    int32_t DatePart(const datetime_t& value, TimeInterval interval);
    datetime_t Now();
    datetime_t UtcNow();
}

// Setup standard hash code for Guid
namespace std  // NOLINT
{
    template<>
    struct hash<GSF::Guid>
    {
        size_t operator () (const GSF::Guid& uid) const
        {
            return boost::hash<GSF::Guid>()(uid);
        }
    };
}

#endif