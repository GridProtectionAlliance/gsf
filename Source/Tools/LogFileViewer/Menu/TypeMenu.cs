using System;
//******************************************************************************************************
//  TypeMenu.cs - Gbtc
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

        public IEnumerable<Tuple<string, Func<LogMessageFilter>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<LogMessageFilter>>("Type", ExcludeType),
                       Tuple.Create<string, Func<LogMessageFilter>>("Related Type", ExcludeRelatedType),
                       Tuple.Create<string, Func<LogMessageFilter>>("Assembly", ExcludeAssembly),
                   };
        }

        private LogMessageFilter ExcludeType()
        {
            var filter = new LogMessageFilter();
            filter.Type = new StringMatching(StringMatchingMode.Exact, m_log.TypeName);
            return filter;
        }

        private LogMessageFilter ExcludeRelatedType()
        {
            using (var frm = new RelatedTypesFilter(m_log.EventPublisherDetails.TypeData))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    var filter = new LogMessageFilter();
                    filter.RelatedType = new StringMatching(StringMatchingMode.Exact, frm.SelectedItem);
                    return filter;
                }
                return null;
            }
        }

        private LogMessageFilter ExcludeAssembly()
        {
            var filter = new LogMessageFilter();
            filter.Assembly = new StringMatching(StringMatchingMode.Exact, m_log.AssemblyName);
            return filter;
        }


    }
}
