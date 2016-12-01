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

        public IEnumerable<Tuple<string, Func<FilterBase>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<FilterBase>>("Exclude Before", ExcludeBefore),
                       Tuple.Create<string, Func<FilterBase>>("Exclude After", ExcludeAfter),
                       Tuple.Create<string, Func<FilterBase>>("Exclude 5 Minutes Before", Exclude5Before),
                       Tuple.Create<string, Func<FilterBase>>("Exclude 5 Minutes After", Exclude5After),
                       Tuple.Create<string, Func<FilterBase>>("Exclude 3 Minutes Before And 1 Minute After", ExcludeBeforeAndAfter),
                       Tuple.Create<string, Func<FilterBase>>("Highlight 3 Minutes Before And 1 Minute After", HighlightBeforeAndAfter),
                   };
        }

        private FilterBase ExcludeBefore()
        {
            return new TimestampFilter(m_minTime, m_maxTime, TimestampFilter.Mode.ExcludeBefore);
        }

        private FilterBase ExcludeAfter()
        {
            return new TimestampFilter(m_minTime, m_maxTime, TimestampFilter.Mode.ExcludeAfter);
        }

        private FilterBase Exclude5Before()
        {
            return new TimestampFilter(m_minTime.AddMinutes(-5), m_maxTime, TimestampFilter.Mode.ExcludeBefore);
        }

        private FilterBase Exclude5After()
        {
            return new TimestampFilter(m_minTime.AddMinutes(5), m_maxTime, TimestampFilter.Mode.ExcludeAfter);
        }

        private FilterBase ExcludeBeforeAndAfter()
        {
            return new TimestampFilter(m_minTime.AddMinutes(-3), m_maxTime.AddMinutes(1), TimestampFilter.Mode.ExcludeOutside);
        }

        private FilterBase HighlightBeforeAndAfter()
        {
            return new TimestampFilter(m_minTime.AddMinutes(-3), m_maxTime.AddMinutes(1), TimestampFilter.Mode.HighlightInside);
        }

    }
}
