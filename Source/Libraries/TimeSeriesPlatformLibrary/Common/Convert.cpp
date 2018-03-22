//******************************************************************************************************
//  Convert.cpp - Gbtc
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
//  04/06/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include <ctime>
#include <iomanip>
#include <sstream>
#include <string>
#include "Convert.h"

namespace gsfts = GSF::TimeSeries;

void GSF::TimeSeries::GetUnixTime(gsfts::int64_t ticks, std::time_t& unixSOC, int& milliseconds)
{
	// Unix dates are measured as the number of seconds since 1/1/1970
	const int64_t BaseTimeOffset = 621355968000000000L;

	unixSOC = (std::time_t)((ticks - BaseTimeOffset) / 10000000);
	milliseconds = (int)(ticks / 10000 % 1000);
}

std::size_t GSF::TimeSeries::TicksToString(char* ptr, std::size_t maxsize, std::string format, gsfts::int64_t ticks)
{
	std::time_t fromSeconds;
	int milliseconds;

	GetUnixTime(ticks, fromSeconds, milliseconds);
	
	std::stringstream formatStream;
	std::size_t formatIndex = 0;

	while (formatIndex < format.size())
	{
		char c = format[formatIndex];
		++formatIndex;

		if (c != '%')
		{
			// Not a format specifier
			formatStream << c;
			continue;
		}

		// Check for %f and %t format specifiers and handle them
		// accordingly. All other specifiers get forwarded to strftime
		c = format[formatIndex];
		++formatIndex;

		switch (c)
		{
		case 'f':
		{
			std::stringstream temp;
			temp << std::setw(3) << std::setfill('0') << milliseconds;
			formatStream << temp.str();
			break;
		}

		case 't':
			formatStream << ticks;
			break;

		default:
			formatStream << '%' << c;
			break;
		}
	}

	return std::strftime(ptr, maxsize, formatStream.str().data(), std::localtime(&fromSeconds));
}
