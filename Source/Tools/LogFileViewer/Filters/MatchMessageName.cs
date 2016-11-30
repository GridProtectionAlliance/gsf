//******************************************************************************************************
//  MatchMessageName.cs - Gbtc
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
    internal class MatchMessageName : IMessageMatch
    {
        private string m_typeName;
        private string m_eventName;
        private bool m_includeIfMatched;
        private string m_errorText;

        private bool m_isContains;
        private bool m_isRegex;
        private Regex m_regex;

        public MatchMessageName(LogMessage typeName)
        {
            m_errorText = typeName.Message;
            m_typeName = typeName.TypeName;
            m_eventName = typeName.EventName;
        }

        public MatchMessageName(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    m_typeName = stream.ReadString();
                    m_eventName = stream.ReadString();
                    m_includeIfMatched = stream.ReadBoolean();
                    m_errorText = stream.ReadString();
                    m_isContains = stream.ReadBoolean();
                    m_isRegex = stream.ReadBoolean();
                    if (m_isRegex)
                        m_regex = new Regex(m_errorText);
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public FilterType TypeCode => FilterType.Description;


        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write(m_typeName);
            stream.Write(m_eventName);
            stream.Write(m_includeIfMatched);
            stream.Write(m_errorText);
            stream.Write(m_isContains);
            stream.Write(m_isRegex);
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.TypeName == m_typeName && log.EventName == m_eventName)
            {
                if (m_isRegex)
                    return m_includeIfMatched ^ !m_regex.IsMatch(log.Message);
                if (m_isContains)
                    return m_includeIfMatched ^ !log.Message.Contains(m_errorText);
                return m_includeIfMatched ^ !log.Message.StartsWith(m_errorText);
            }
            return !m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include Message: " + m_eventName + "(" + m_typeName + ")" + m_errorText;
                else
                    return "Exclude Message: " + m_eventName + "(" + m_typeName + ")" + m_errorText;
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                   {
                       Tuple.Create<string, Func<bool>>("Include Message", () =>
                                                                           {
                                                                               m_includeIfMatched = true;
                                                                               using (var frm = new ErrorFilterText(m_errorText))
                                                                               {
                                                                                   if (frm.ShowDialog() == DialogResult.OK)
                                                                                   {
                                                                                       m_isContains = frm.rdoContains.Checked;
                                                                                       m_isRegex = frm.rdoRegex.Checked;
                                                                                       m_errorText = frm.ErrorText;
                                                                                       if (m_isRegex)
                                                                                       {
                                                                                           m_regex = new Regex(m_errorText);
                                                                                       }
                                                                                       return true;
                                                                                   }
                                                                                   return false;
                                                                               }

                                                                           }),
                       Tuple.Create<string, Func<bool>>("Exclude Message", () =>
                                                                           {
                                                                               m_includeIfMatched = false;
                                                                               using (var frm = new ErrorFilterText(m_errorText))
                                                                               {
                                                                                   if (frm.ShowDialog() == DialogResult.OK)
                                                                                   {
                                                                                       m_isContains = frm.rdoContains.Checked;
                                                                                       m_isRegex = frm.rdoRegex.Checked;
                                                                                       m_errorText = frm.ErrorText;
                                                                                       if (m_isRegex)
                                                                                       {
                                                                                           m_regex = new Regex(m_errorText);
                                                                                       }
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