//******************************************************************************************************
//  ExceptionMenu.cs - Gbtc
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
using System.Windows.Forms;
using GSF.Diagnostics;
using LogFileViewer.Filters;

namespace LogFileViewer.Menu
{
    public class ExceptionMenu
    {
        private LogMessage m_log;

        public ExceptionMenu(List<LogMessage> selectedLogMessages)
        {
            m_log = selectedLogMessages.First();
        }

        public IEnumerable<Tuple<string, Func<LogMessageFilter>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<LogMessageFilter>>("Exception", Message),
                       Tuple.Create<string, Func<LogMessageFilter>>("Exception And Event", MessageAndEvent),
                       Tuple.Create<string, Func<LogMessageFilter>>("Exception And Related Type", MessageAndRelatedType),
                       Tuple.Create<string, Func<LogMessageFilter>>("Exception And Assembly", MessageAndAssembly),
                   };
        }

        private LogMessageFilter Message()
        {
            using (var frm = new StringMatchingFilterDialog(new StringMatching(StringMatchingMode.Exact, m_log.ExceptionString)))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    var filter = new LogMessageFilter();
                    filter.ExceptionText = frm.ResultFilter;
                    return filter;
                }
                return null;
            }
        }

        private LogMessageFilter MessageAndEvent()
        {
            using (var frm = new StringMatchingFilterDialog(new StringMatching(StringMatchingMode.Exact, m_log.ExceptionString)))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    var filter = new LogMessageFilter();
                    filter.ExceptionText = frm.ResultFilter;
                    filter.EventName = new StringMatching(StringMatchingMode.Exact, m_log.EventName);
                    return filter;
                }
                return null;
            }
        }

        private LogMessageFilter MessageAndRelatedType()
        {
            using (var frm = new RelatedTypesFilter(m_log.EventPublisherDetails.TypeData))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    using (var frm2 = new StringMatchingFilterDialog(new StringMatching(StringMatchingMode.Exact, m_log.ExceptionString)))
                    {
                        if (frm2.ShowDialog() == DialogResult.OK)
                        {
                            var filter = new LogMessageFilter();
                            filter.RelatedType = new StringMatching(StringMatchingMode.Exact, frm.SelectedItem);
                            filter.ExceptionText = frm2.ResultFilter;
                            return filter;
                        }
                        return null;
                    }
                }
                return null;
            }
        }

        private LogMessageFilter MessageAndAssembly()
        {
            using (var frm = new StringMatchingFilterDialog(new StringMatching(StringMatchingMode.Exact, m_log.ExceptionString)))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    var filter = new LogMessageFilter();
                    filter.ExceptionText = frm.ResultFilter;
                    filter.Assembly = new StringMatching(StringMatchingMode.Exact, m_log.AssemblyName);
                    return filter;
                }
                return null;
            }
        }




    }
}
