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
                       Tuple.Create<string, Func<FilterBase>>("Exclude Type", ExcludeType),
                       Tuple.Create<string, Func<FilterBase>>("Exclude Type...", ExcludeTypeAdv),
                       Tuple.Create<string, Func<FilterBase>>("Exclude Assembly", ExcludeAssembly),
                       Tuple.Create<string, Func<FilterBase>>("Exclude Assembly...", ExcludeAssemblyAdv),
                       Tuple.Create<string, Func<FilterBase>>("Exclude Related Type", ExcludeRelatedType),


                       Tuple.Create<string, Func<FilterBase>>("Highlight Type", HighlightType),
                       Tuple.Create<string, Func<FilterBase>>("Highlight Type...", HighlightTypeAdv),
                       Tuple.Create<string, Func<FilterBase>>("Highlight Assembly", HighlightAssembly),
                       Tuple.Create<string, Func<FilterBase>>("Highlight Assembly...", HighlightAssemblyAdv),
                       Tuple.Create<string, Func<FilterBase>>("Highlight Related Type", HighlightRelatedType),

                   };
        }

        private FilterBase ExcludeType()
        {
            return new TypeFilter(TypeFilter.Mode.ExcludeType, TypeFilter.MatchMode.Equals, m_log.TypeName);
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
                    return new TypeFilter(TypeFilter.Mode.ExcludeType, mode, frm.ErrorText);
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
                    return new TypeFilter(TypeFilter.Mode.ExcludeRelatedType, TypeFilter.MatchMode.Equals, frm.SelectedItems);
                }
                return null;
            }
        }

        private FilterBase ExcludeAssembly()
        {
            return new TypeFilter(TypeFilter.Mode.ExcludeAssembly, TypeFilter.MatchMode.Equals, m_log.AssemblyName);
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
                    return new TypeFilter(TypeFilter.Mode.ExcludeAssembly, mode, frm.ErrorText);
                }
                return null;
            }
        }




        private FilterBase HighlightType()
        {
            return new TypeFilter(TypeFilter.Mode.HighlightType, TypeFilter.MatchMode.Equals, m_log.TypeName);
        }

        private FilterBase HighlightTypeAdv()
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
                    return new TypeFilter(TypeFilter.Mode.HighlightType, mode, frm.ErrorText);
                }
                return null;
            }
        }

        private FilterBase HighlightRelatedType()
        {
            using (var frm = new RelatedTypesFilter(m_log.EventPublisherDetails.TypeData))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    return new TypeFilter(TypeFilter.Mode.HighlightRelatedType, TypeFilter.MatchMode.Equals, frm.SelectedItems);
                }
                return null;
            }
        }

        private FilterBase HighlightAssembly()
        {
            return new TypeFilter(TypeFilter.Mode.HighlightAssembly, TypeFilter.MatchMode.Equals, m_log.AssemblyName);
        }

        private FilterBase HighlightAssemblyAdv()
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
                    return new TypeFilter(TypeFilter.Mode.HighlightAssembly, mode, frm.ErrorText);
                }
                return null;
            }
        }




    }
}
