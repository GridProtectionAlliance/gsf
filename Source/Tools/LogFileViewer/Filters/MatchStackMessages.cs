//******************************************************************************************************
//  MatchStackMessages.cs - Gbtc
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
//  11/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GSF.IO;
using GSF.Diagnostics;

namespace LogFileViewer.Filters
{
    internal class MatchStackMessages : IMessageMatch
    {
        private LogStackMessages m_filter;
        private bool m_includeIfMatched;

        public MatchStackMessages(LogMessage typeName)
        {
            m_filter = typeName.InitialStackMessages.ConcatenateWith(typeName.CurrentStackMessages);
        }

        public MatchStackMessages(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_filter = new LogStackMessages(stream);
                    m_includeIfMatched = stream.ReadBoolean();
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public FilterType TypeCode => FilterType.Description;

        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            m_filter.Save(stream);
            stream.Write(m_includeIfMatched);
        }

        public bool IsIncluded(LogMessage log)
        {
            var messages = log.InitialStackMessages.ConcatenateWith(log.CurrentStackMessages);
            if (messages.Count < m_filter.Count)
                return !m_includeIfMatched;

            for (int x = 0; x < m_filter.Count; x++)
            {
                var pair = m_filter[x];

                var lookup = messages[pair.Key];
                if (lookup == null)
                    return !m_includeIfMatched;
                if (lookup != pair.Value)
                    return !m_includeIfMatched;
            }
            return m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include With Stack Details: " + m_filter.ToString();
                else
                    return "Exclude With Stack Details: " + m_filter.ToString();
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<bool>>("Include Key", () =>
                                                                           {
                                                                               m_includeIfMatched = true;
                                                                               using (var frm = new StackDetailsFilter(m_filter))
                                                                               {
                                                                                   if (frm.ShowDialog() == DialogResult.OK)
                                                                                   {
                                                                                       m_filter = frm.SelectedItems;
                                                                                       return true;
                                                                                   }
                                                                                   return false;
                                                                               }

                                                                           }),
                       Tuple.Create<string, Func<bool>>("Include Key", () =>
                                                                           {
                                                                               m_includeIfMatched = false;
                                                                               using (var frm = new StackDetailsFilter(m_filter))
                                                                               {
                                                                                   if (frm.ShowDialog() == DialogResult.OK)
                                                                                   {
                                                                                       m_filter = frm.SelectedItems;
                                                                                       return true;
                                                                                   }
                                                                                   return false;
                                                                               }
                                                                           })
                   };
        }
        public void ToggleResult()
        {
            m_includeIfMatched = !m_includeIfMatched;
        }
    }
}