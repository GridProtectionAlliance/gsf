//******************************************************************************************************
//  TimestampMenu.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  12/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF.Diagnostics;
using LogFileViewer.Filters;

namespace LogFileViewer.Menu
{
    public class TimestampMenu
    {
        private DateTime m_minTime;
        private DateTime m_maxTime;

        public TimestampMenu(List<LogMessage> selectedLogMessages)
        {
            var time = selectedLogMessages.OrderBy(x => x.UtcTime);
            m_minTime = time.First().UtcTime;
            m_maxTime = time.Last().UtcTime;
        }

        public IEnumerable<Tuple<string, Func<LogMessageFilter>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<LogMessageFilter>>("Before", ExcludeBefore),
                       Tuple.Create<string, Func<LogMessageFilter>>("After", ExcludeAfter),
                       Tuple.Create<string, Func<LogMessageFilter>>("5 Minutes Before", Exclude5Before),
                       Tuple.Create<string, Func<LogMessageFilter>>("5 Minutes After", Exclude5After),
                       Tuple.Create<string, Func<LogMessageFilter>>("3 Minutes Before And 1 Minute After", ExcludeBeforeAndAfter),
                   };
        }

        private LogMessageFilter ExcludeBefore()
        {
            var filter = new LogMessageFilter();
            filter.TimeFilter = new TimestampMatching(TimestampMatchingMode.Before, m_minTime, m_maxTime);
            return filter;
        }

        private LogMessageFilter ExcludeAfter()
        {
            var filter = new LogMessageFilter();
            filter.TimeFilter = new TimestampMatching(TimestampMatchingMode.After, m_minTime, m_maxTime);
            return filter;
        }

        private LogMessageFilter Exclude5Before()
        {
            var filter = new LogMessageFilter();
            filter.TimeFilter = new TimestampMatching(TimestampMatchingMode.Before, m_minTime.AddMinutes(-5), m_maxTime);
            return filter;
        }

        private LogMessageFilter Exclude5After()
        {
            var filter = new LogMessageFilter();
            filter.TimeFilter = new TimestampMatching(TimestampMatchingMode.After, m_minTime.AddMinutes(5), m_maxTime);
            return filter;
        }

        private LogMessageFilter ExcludeBeforeAndAfter()
        {
            var filter = new LogMessageFilter();
            filter.TimeFilter = new TimestampMatching(TimestampMatchingMode.After, m_minTime.AddMinutes(-3), m_minTime.AddMinutes(1));
            return filter;
        }

    }
}
