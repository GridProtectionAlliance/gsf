//******************************************************************************************************
//  Guid.h - Gbtc
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

#include <boost/uuid/uuid.hpp>
#include <boost/exception/exception.hpp>
#include <boost/thread.hpp>
#include <boost/thread/condition_variable.hpp>
#include <boost/thread/mutex.hpp>
#include <boost/thread/locks.hpp>
#include <boost/asio.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/asio/ip/udp.hpp>

using namespace std;

namespace GSF {
namespace TimeSeries
{
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

	// Floating-point types
	typedef float float32_t;
	typedef double float64_t;
}}

#endif