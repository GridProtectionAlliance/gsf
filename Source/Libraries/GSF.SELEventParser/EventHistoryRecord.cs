//******************************************************************************************************
//  Event.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/19/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using GSF.Collections;

namespace GSF.SELEventParser
{
    public class EventHistoryRecord
    {
        #region [ Members ]

        // Nested Types
        private class Token
        {
            public string Text;
            public int StartIndex;
            public int EndIndex;

            public double Distance(Token t)
            {
                return Math.Abs(StartIndex - t.StartIndex)
                    + Math.Abs(EndIndex - t.EndIndex);
            }

            public Token JoinWith(Token t)
            {
                Text = Text + " " + t.Text;
                EndIndex = StartIndex + Text.Length - 1;
                return this;
            }
        }

        // Fields
        private int m_eventNumber;
        private DateTime m_time;
        private string m_eventType;
        private double m_faultLocation;
        private double m_current;
        private double m_frequency;
        private int m_group;
        private int m_shot;
        private string m_targets;

        #endregion

        #region [ Properties ]

        public int EventNumber
        {
            get
            {
                return m_eventNumber;
            }
            set
            {
                m_eventNumber = value;
            }
        }

        public DateTime Time
        {
            get
            {
                return m_time;
            }
            set
            {
                m_time = value;
            }
        }

        public string EventType
        {
            get
            {
                return m_eventType;
            }
            set
            {
                m_eventType = value;
            }
        }

        public double FaultLocation
        {
            get
            {
                return m_faultLocation;
            }
            set
            {
                m_faultLocation = value;
            }
        }

        public double Current
        {
            get
            {
                return m_current;
            }
            set
            {
                m_current = value;
            }
        }

        public double Frequency
        {
            get
            {
                return m_frequency;
            }
            set
            {
                m_frequency = value;
            }
        }

        public int Group
        {
            get
            {
                return m_group;
            }
            set
            {
                m_group = value;
            }
        }

        public int Shot
        {
            get
            {
                return m_shot;
            }
            set
            {
                m_shot = value;
            }
        }

        public string Targets
        {
            get
            {
                return m_targets;
            }
            set
            {
                m_targets = value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        public static List<EventHistoryRecord> ParseRecords(string[] lines, ref int index)
        {
            List<EventHistoryRecord> histories = new List<EventHistoryRecord>();
            EventHistoryRecord eventHistory;
            string currentLine;

            List<Token> tokens;
            List<Token> headers;
            Dictionary<Token, Token> fields;
            Token fieldHeader;
            Token field;

            int eventNumber;
            DateTime dateTime;
            double faultLocation;
            double current;
            double frequency;
            int group;
            int shot;

            string date;
            string time;

            // Parse header
            headers = Split(lines[index++]);

            // Skip to the next nonblank line
            EventFile.SkipBlanks(lines, ref index);

            while (index < lines.Length)
            {
                currentLine = lines[index];

                // Empty line indicates end of event histories
                if (string.IsNullOrWhiteSpace(currentLine))
                    break;

                // Create a new event history record
                eventHistory = new EventHistoryRecord();

                // Parse fields
                tokens = Split(currentLine);

                // Initialize date and time variables
                date = null;
                time = null;

                fields = new Dictionary<Token, Token>();

                foreach (Token token in tokens)
                {
                    fieldHeader = headers.MinBy(header => token.Distance(header));
                    fields.AddOrUpdate(fieldHeader, token, (key, value) => value.JoinWith(token));
                }

                foreach (Token header in headers)
                {
                    if (fields.TryGetValue(header, out field))
                    {

                        switch (header.Text.ToUpper())
                        {
                            case "#":
                            case "REC_NUM":
                                // Parse the field as an event number
                                if (int.TryParse(field.Text, out eventNumber))
                                    eventHistory.EventNumber = eventNumber;

                                break;

                            case "DATE":
                                // Parse the field as a date value
                                date = field.Text;

                                // If both date and time have been provided, parse them as a DateTime
                                if ((object)time != null && EventFile.TryParseDateTime(string.Format("{0} {1}", date, time), out dateTime))
                                    eventHistory.Time = dateTime;

                                break;

                            case "TIME":
                                // Parse the field as a time value
                                time = field.Text;

                                // If both date and time have been provided, parse them as a DateTime
                                if ((object)date != null && EventFile.TryParseDateTime(string.Format("{0} {1}", date, time), out dateTime))
                                    eventHistory.Time = dateTime;

                                break;

                            case "EVENT":
                                // Parse the field as an event type
                                eventHistory.EventType = field.Text;
                                break;

                            case "LOCAT":
                            case "LOCATION":
                                // Parse the field as a fault location value
                                if (double.TryParse(field.Text, out faultLocation))
                                    eventHistory.FaultLocation = faultLocation;

                                break;

                            case "CURR":
                                // Parse the field as a current magnitude
                                if (double.TryParse(field.Text, out current))
                                    eventHistory.Current = current;

                                break;

                            case "FREQ":
                                // Parse the field as a frequency value
                                if (double.TryParse(field.Text, out frequency))
                                    eventHistory.Frequency = frequency;

                                break;

                            case "GRP":
                            case "GROUP":
                                // Parse the field as a group number
                                if (int.TryParse(field.Text, out group))
                                    eventHistory.Group = group;

                                break;

                            case "SHOT":
                                // Parse the field as a shot number
                                if (int.TryParse(field.Text, out shot))
                                    eventHistory.Shot = shot;

                                break;

                            case "TARGETS":
                                // Parse the field as targets
                                eventHistory.Targets = field.Text;
                                break;
                        }
                    }
                }

                // Add history record to the list of histories
                histories.Add(eventHistory);

                // Advance to the next line
                index++;
            }

            return histories;
        }

        private static List<Token> Split(string line)
        {
            List<Token> tokens = new List<Token>();
            int startIndex = -1;

            for (int i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    if (startIndex < 0)
                        startIndex = i;
                }
                else
                {
                    if (startIndex >= 0)
                    {
                        tokens.Add(new Token()
                        {
                            Text = line.Substring(startIndex, i - startIndex),
                            StartIndex = startIndex,
                            EndIndex = i - 1
                        });

                        startIndex = -1;
                    }
                }
            }

            if (startIndex >= 0)
            {
                tokens.Add(new Token()
                {
                    Text = line.Substring(startIndex),
                    StartIndex = startIndex,
                    EndIndex = line.Length - 1
                });
            }

            return tokens;
        }

        #endregion
    }
}
