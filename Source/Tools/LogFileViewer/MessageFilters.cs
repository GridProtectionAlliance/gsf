//******************************************************************************************************
//  MessageFilters.cs - Gbtc
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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GSF.Diagnostics;

namespace LogFileViewer
{
    internal interface IMessageMatch
    {
        bool IsIncluded(LogMessage log);
        string Description { get; }
        IEnumerable<Tuple<String, Func<bool>>> GetMenuButtons();
        void ToggleResult();
    }

    internal class MatchType : IMessageMatch
    {
        private string m_typeName;
        private bool m_includeIfMatched;

        public MatchType(LogMessage typeName)
        {
            m_typeName = typeName.EventPublisherDetails.TypeName;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName)
                return m_includeIfMatched;
            return !m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include if Type: " + m_typeName;
                else
                    return "Exclude if Type: " + m_typeName;
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                {
                    Tuple.Create<string, Func<bool>>("Include Type", () => { m_includeIfMatched = true; return true; }),
                    Tuple.Create<string, Func<bool>>("Exclude Type", () => { m_includeIfMatched = false; return true;})
                };
        }

        public void ToggleResult()
        {
            m_includeIfMatched = !m_includeIfMatched;
        }
    }

    internal class MatchVerbose : IMessageMatch
    {
        private MessageClass m_class;
        private MessageLevel m_level;
        private bool m_includeIfMatched;

        public MatchVerbose(LogMessage typeName)
        {
            m_class = typeName.Classification;
            m_level = typeName.Level;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.Classification == m_class && log.Level == m_level)
                return m_includeIfMatched;
            return !m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include Level: " + m_class.ToString() + '-' + m_level.ToString();
                else
                    return "Exclude Level: " + m_class.ToString() + '-' + m_level.ToString();
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                {
                    Tuple.Create<string, Func<bool>>("Include Level", () => { m_includeIfMatched = true; return true;}),
                    Tuple.Create<string, Func<bool>>("Exclude Level", () => { m_includeIfMatched = false; return true;})
                };
        }
        public void ToggleResult()
        {
            m_includeIfMatched = !m_includeIfMatched;
        }
    }

    internal class MatchEventName : IMessageMatch
    {
        private string m_typeName;
        private string m_eventName;
        private bool m_includeIfMatched;

        public MatchEventName(LogMessage typeName)
        {
            m_typeName = typeName.EventPublisherDetails.TypeName;
            m_eventName = typeName.EventPublisherDetails.EventName;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName && log.EventPublisherDetails.EventName == m_eventName)
                return m_includeIfMatched;
            return !m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include if Event: " + m_eventName + "(" + m_typeName + ")";
                else
                    return "Exclude if Event: " + m_eventName + "(" + m_typeName + ")";
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                    {
                        Tuple.Create<string, Func<bool>>("Include Event", () => { m_includeIfMatched = true; return true;}),
                        Tuple.Create<string, Func<bool>>("Exclude Event", () => { m_includeIfMatched = false; return true;})
                    };
        }
        public void ToggleResult()
        {
            m_includeIfMatched = !m_includeIfMatched;
        }
    }

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
            m_typeName = typeName.EventPublisherDetails.TypeName;
            m_eventName = typeName.EventPublisherDetails.EventName;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName && log.EventPublisherDetails.EventName == m_eventName)
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
                                                                          using (ErrorFilterText frm = new ErrorFilterText(m_errorText))
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
                                                                           using (ErrorFilterText frm = new ErrorFilterText(m_errorText))
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


    internal class MatchErrorName : IMessageMatch
    {
        private string m_typeName;
        private string m_eventName;
        private bool m_includeIfMatched;
        private string m_errorText;
        private bool m_isContains;
        private bool m_isRegex;
        private Regex m_regex;

        public MatchErrorName(LogMessage typeName)
        {
            m_errorText = typeName.ExceptionString;
            m_typeName = typeName.EventPublisherDetails.TypeName;
            m_eventName = typeName.EventPublisherDetails.EventName;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName && log.EventPublisherDetails.EventName == m_eventName)
            {
                if (m_isRegex)
                    return m_includeIfMatched ^ !m_regex.IsMatch(log.ExceptionString);
                if (m_isContains)
                    return m_includeIfMatched ^ !log.ExceptionString.Contains(m_errorText);
                return m_includeIfMatched ^ !log.ExceptionString.StartsWith(m_errorText);
            }
            return !m_includeIfMatched;
        }

        public string Description
        {
            get
            {
                if (m_includeIfMatched)
                    return "Include Error: " + m_eventName + "(" + m_typeName + ")" + m_errorText;
                else
                    return "Exclude Error: " + m_eventName + "(" + m_typeName + ")" + m_errorText;
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                    {
                        Tuple.Create<string, Func<bool>>("Include Error", () =>
                                                                      {
                                                                          m_includeIfMatched = true;
                                                                           using (ErrorFilterText frm = new ErrorFilterText(m_errorText))
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
                        Tuple.Create<string, Func<bool>>("Exclude Error", () =>
                                                                      {
                                                                          m_includeIfMatched = false;
                                                                           using (ErrorFilterText frm = new ErrorFilterText(m_errorText))
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

    internal class MatchTimestamp : IMessageMatch
    {
        private DateTime m_timestamp;
        private bool m_before;

        public MatchTimestamp(LogMessage typeName)
        {
            m_timestamp = typeName.UtcTime;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (m_before)
            {
                return log.UtcTime >= m_timestamp;
            }
            else
            {
                return log.UtcTime <= m_timestamp;
            }
        }

        public string Description
        {
            get
            {
                if (m_before)
                {
                    return "Exclude if before: " + m_timestamp;
                }
                else
                {
                    return "Exclude if after: " + m_timestamp;
                }
            }
        }

        public IEnumerable<Tuple<string, Func<bool>>> GetMenuButtons()
        {
            return new[]
                    {
                        Tuple.Create<string, Func<bool>>("Exclude Before", () => { m_before = true; return true;}),
                        Tuple.Create<string, Func<bool>>("Exclude After", () => { m_before = false; return true;}),
                        Tuple.Create<string, Func<bool>>("Exclude 5 Minutes Before", () =>{m_before = true; m_timestamp = m_timestamp.AddMinutes(-5);return true;}),
                        Tuple.Create<string, Func<bool>>("Exclude 5 Minutes After", () =>{m_before = false; m_timestamp = m_timestamp.AddMinutes(5);return true;}),
                    };
        }
        public void ToggleResult()
        {
            m_before = !m_before;
        }
    }
}
