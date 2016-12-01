using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GSF.Diagnostics;
using LogFileViewer.Filters;

namespace LogFileViewer.Menu
{
    public class TypeMenu
    {
        private LogMessage m_log;

        public TypeMenu(List<LogMessage> selectedLogMessages)
        {
            m_log = selectedLogMessages.First();
        }

        public IEnumerable<Tuple<string, Func<FilterBase>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<FilterBase>>("Is Type", ExcludeType),
                       Tuple.Create<string, Func<FilterBase>>("Is Type...", ExcludeTypeAdv),
                       Tuple.Create<string, Func<FilterBase>>("Is Assembly", ExcludeAssembly),
                       Tuple.Create<string, Func<FilterBase>>("Is Assembly...", ExcludeAssemblyAdv),
                       Tuple.Create<string, Func<FilterBase>>("Is Related Type", ExcludeRelatedType),
                   };
        }

        private FilterBase ExcludeType()
        {
            return new TypeFilter(TypeFilter.Mode.Type, TypeFilter.MatchMode.Equals, m_log.TypeName);
        }

        private FilterBase ExcludeTypeAdv()
        {
            using (var frm = new ErrorFilterText(m_log.TypeName))
            {
                frm.AllowRegex(false);
                frm.AllowMultiline(false);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    TypeFilter.MatchMode mode = TypeFilter.MatchMode.Equals;
                    if (frm.ContainsText)
                        mode = TypeFilter.MatchMode.Contains;
                    if (frm.StartsWith)
                        mode = TypeFilter.MatchMode.StartsWith;
                    return new TypeFilter(TypeFilter.Mode.Type, mode, frm.ErrorText);
                }
                return null;
            }
        }

        private FilterBase ExcludeRelatedType()
        {
            using (var frm = new RelatedTypesFilter(m_log.EventPublisherDetails.TypeData))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    return new TypeFilter(TypeFilter.Mode.RelatedType, TypeFilter.MatchMode.Equals, frm.SelectedItems);
                }
                return null;
            }
        }

        private FilterBase ExcludeAssembly()
        {
            return new TypeFilter(TypeFilter.Mode.Assembly, TypeFilter.MatchMode.Equals, m_log.AssemblyName);
        }

        private FilterBase ExcludeAssemblyAdv()
        {
            using (var frm = new ErrorFilterText(m_log.AssemblyName))
            {
                frm.AllowRegex(false);
                frm.AllowMultiline(false);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    TypeFilter.MatchMode mode = TypeFilter.MatchMode.Equals;
                    if (frm.ContainsText)
                        mode = TypeFilter.MatchMode.Contains;
                    if (frm.StartsWith)
                        mode = TypeFilter.MatchMode.StartsWith;
                    return new TypeFilter(TypeFilter.Mode.Assembly, mode, frm.ErrorText);
                }
                return null;
            }
        }



    }
}
