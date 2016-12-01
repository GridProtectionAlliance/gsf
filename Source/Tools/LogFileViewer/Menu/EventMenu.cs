using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GSF.Diagnostics;
using LogFileViewer.Filters;

namespace LogFileViewer.Menu
{
    public class EventMenu
    {
        private LogMessage m_log;

        public EventMenu(List<LogMessage> selectedLogMessages)
        {
            m_log = selectedLogMessages.First();
        }

        public IEnumerable<Tuple<string, Func<FilterBase>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<FilterBase>>("Is Event and Type", EventAndType),
                       Tuple.Create<string, Func<FilterBase>>("Is Event and Related Type...", EventAndRelatedType),
                       Tuple.Create<string, Func<FilterBase>>("Is Event and Assembly", EventAndAssembly),
                       Tuple.Create<string, Func<FilterBase>>("Is Event", Event),
                   };
        }

        private FilterBase Event()
        {
            return new EventFilter(EventFilter.Mode.EventOnly, m_log.EventName);
        }

        private FilterBase EventAndType()
        {
            return new EventFilter(EventFilter.Mode.WithType, m_log.EventName, m_log.TypeName);
        }

        private FilterBase EventAndRelatedType()
        {
            using (var frm = new RelatedTypesFilter(m_log.EventPublisherDetails.TypeData))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    return new EventFilter(EventFilter.Mode.WithRelatedType, m_log.EventName, frm.SelectedItems);
                }
                return null;
            }
        }

        private FilterBase EventAndAssembly()
        {
            return new EventFilter(EventFilter.Mode.WithAssembly, m_log.EventName, m_log.AssemblyName);
        }





    }
}
