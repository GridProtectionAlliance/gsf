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

#ifndef __TYPES_H
#define __TYPES_H

#include <boost/uuid/uuid.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/asio/ip/udp.hpp>

namespace GSF {
namespace TimeSeries
{
	typedef boost::uuids::uuid Guid;
	typedef boost::asio::ip::tcp::socket TcpSocket;
	typedef boost::asio::ip::udp::socket UdpSocket;

	// Signed integer types
	typedef signed char		int8_t;
	typedef short			int16_t;
	typedef int				int32_t;
	typedef long			int64_t;

	// Unsigned integer types
	typedef unsigned char	uint8_t;
	typedef unsigned short	uint16_t;
	typedef unsigned int	uint32_t;
	typedef unsigned long	uint64_t;

	// Floating-point types
	typedef float			float32_t;
	typedef double			float64_t;
}}

#endif