using System;
using System.Collections.Generic;
using OGE.Core.GSF.Diagnostics.UI;

namespace GSF.Diagnostics.UI
{
    internal interface IMessageMatch
    {
        bool IsIncluded(LogMessage log);
        string Description { get; }
        IEnumerable<Tuple<String, Action>> GetMenuButtons();
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

        public IEnumerable<Tuple<string, Action>> GetMenuButtons()
        {
            return new[]
                {
                    Tuple.Create<string, Action>("Include Type", () => { m_includeIfMatched = true; }),
                    Tuple.Create<string, Action>("Exclude Type", () => { m_includeIfMatched = false; })
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

        public IEnumerable<Tuple<string, Action>> GetMenuButtons()
        {
            return new[]
                {
                    Tuple.Create<string, Action>("Include Level", () => { m_includeIfMatched = true; }),
                    Tuple.Create<string, Action>("Exclude Level", () => { m_includeIfMatched = false; })
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

        public IEnumerable<Tuple<string, Action>> GetMenuButtons()
        {
            return new[]
                    {
                        Tuple.Create<string, Action>("Include Event", () => { m_includeIfMatched = true; }),
                        Tuple.Create<string, Action>("Exclude Event", () => { m_includeIfMatched = false; })
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
        public MatchMessageName(LogMessage typeName)
        {
            m_errorText = typeName.Message;
            m_typeName = typeName.EventPublisherDetails.TypeName;
            m_eventName = typeName.EventPublisherDetails.EventName;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName && log.EventPublisherDetails.EventName == m_eventName)
                return m_includeIfMatched ^ !log.Message.StartsWith(m_errorText);
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

        public IEnumerable<Tuple<string, Action>> GetMenuButtons()
        {
            return new[]
                    {
                        Tuple.Create<string, Action>("Include Message", () =>
                                                                      {
                                                                          m_includeIfMatched = true;
                                                                          using (var frm = new FrmErrorFilterText(m_errorText))
                                                                          {
                                                                              frm.ShowDialog();
                                                                              m_errorText = frm.ErrorText;
                                                                          }

                                                                      }),
                        Tuple.Create<string, Action>("Exclude Message", () =>
                                                                      {
                                                                          m_includeIfMatched = false;
                                                                          using (var frm = new FrmErrorFilterText(m_errorText))
                                                                          {
                                                                              frm.ShowDialog();
                                                                              m_errorText = frm.ErrorText;
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
        public MatchErrorName(LogMessage typeName)
        {
            m_errorText = typeName.ExceptionString;
            m_typeName = typeName.EventPublisherDetails.TypeName;
            m_eventName = typeName.EventPublisherDetails.EventName;
        }

        public bool IsIncluded(LogMessage log)
        {
            if (log.EventPublisherDetails.TypeName == m_typeName && log.EventPublisherDetails.EventName == m_eventName)
                return m_includeIfMatched ^ !log.ExceptionString.StartsWith(m_errorText);
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

        public IEnumerable<Tuple<string, Action>> GetMenuButtons()
        {
            return new[]
                    {
                        Tuple.Create<string, Action>("Include Error", () =>
                                                                      {
                                                                          m_includeIfMatched = true;
                                                                          using (var frm = new FrmErrorFilterText(m_errorText))
                                                                          {
                                                                              frm.ShowDialog();
                                                                              m_errorText = frm.ErrorText;
                                                                          }

                                                                      }),
                        Tuple.Create<string, Action>("Exclude Error", () =>
                                                                      {
                                                                          m_includeIfMatched = false;
                                                                          using (var frm = new FrmErrorFilterText(m_errorText))
                                                                          {
                                                                              frm.ShowDialog();
                                                                              m_errorText = frm.ErrorText;
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

        public IEnumerable<Tuple<string, Action>> GetMenuButtons()
        {
            return new[]
                    {
                        Tuple.Create<string, Action>("Exclude Before", () => { m_before = true; }),
                        Tuple.Create<string, Action>("Exclude After", () => { m_before = false; }),
                        Tuple.Create<string, Action>("Exclude 5 Minutes Before", () =>{m_before = true; m_timestamp = m_timestamp.AddMinutes(-5);}),
                        Tuple.Create<string, Action>("Exclude 5 Minutes After", () =>{m_before = false; m_timestamp = m_timestamp.AddMinutes(5);}),
                    };
        }
        public void ToggleResult()
        {
            m_before = !m_before;
        }
    }
}
