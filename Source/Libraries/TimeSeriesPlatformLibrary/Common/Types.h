//******************************************************************************************************
//  Types.h - Gbtc
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
//  03/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __COMMON_TYPES_H
#define __COMMON_TYPES_H

#include <cstddef>
#include <boost/uuid/uuid.hpp>
#include <boost/exception/exception.hpp>
#include <boost/thread.hpp>
#include <boost/thread/condition_variable.hpp>
#include <boost/thread/locks.hpp>
#include <boost/asio.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/asio/ip/udp.hpp>
#include <boost/iostreams/filtering_stream.hpp>
#include <boost/iostreams/filter/gzip.hpp>
#include <boost/iostreams/device/array.hpp>

using namespace std;

namespace GSF {
namespace TimeSeries
{
    template<class T>
    using SharedPtr = boost::shared_ptr<T>;

    template<class T> SharedPtr<T> NewSharedPtr()
    {
        return boost::make_shared<T>();
    }

    typedef boost::uuids::uuid Guid;
    typedef boost::system::error_code ErrorCode;
    typedef boost::system::system_error SystemError;
    typedef boost::exception Exception;
    typedef boost::thread Thread;
    typedef boost::mutex Mutex;
    typedef boost::condition_variable WaitHandle;
    typedef boost::lock_guard<Mutex> ScopeLock;
    typedef boost::unique_lock<Mutex> UniqueLock;
    typedef boost::asio::ip::address IPAddress;
    typedef boost::asio::ip::tcp::socket TcpSocket;
    typedef boost::asio::ip::udp::socket UdpSocket;
    typedef boost::asio::ip::tcp::resolver DnsResolver;
    typedef boost::iostreams::filtering_streambuf<boost::iostreams::input> Decompressor;
    typedef boost::iostreams::gzip_decompressor GZipStream;

    struct MemoryStream : boost::iostreams::array_source
    {
        MemoryStream(const vector<uint8_t>& buffer) : boost::iostreams::array_source(reinterpret_cast<const char*>(buffer.data()), buffer.size())
        {
        }

        MemoryStream(const uint8_t* buffer, size_t offset, size_t length) : boost::iostreams::array_source(reinterpret_cast<const char*>(buffer + offset), length)
        {
        }
    };

    inline void CopyStream(Decompressor& source, vector<uint8_t>& sink)
    {
        sink.assign(istreambuf_iterator<char>{ &source }, {});
    }

    // Floating-point types
    typedef float float32_t;
    typedef double float64_t;
}}

// Setup standard hash code for Guid
namespace std  // NOLINT
{
    using namespace GSF::TimeSeries;

    template<>
    struct hash<Guid>
    {
        size_t operator () (const Guid& uid) const
        {
            return boost::hash<Guid>()(uid);
        }
    };
}

#endif