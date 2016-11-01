//******************************************************************************************************
//  EVEFile.cs - Gbtc
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
//  11/05/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GSF.Collections;

namespace GSF.SELEventParser
{
    public class EventFile
    {
        #region [ Members ]

        // Fields
        private List<object> m_sections;

        #endregion

        #region [ Constructors ]

        public EventFile()
        {
            m_sections = new List<object>();
        }

        #endregion

        #region [ Properties ]

        public List<EventReport> EventReports
        {
            get
            {
                return m_sections.OfType<EventReport>().ToList();
            }
        }

        public List<EventHistory> EventHistories
        {
            get
            {
                return m_sections.OfType<EventHistory>().ToList();
            }
        }

        public List<CommaSeparatedEventReport> CommaSeparatedEventReports
        {
            get
            {
                return m_sections.OfType<CommaSeparatedEventReport>().ToList();
            }
        }

        #endregion

        #region [ Methods ]

        public void Add(EventReport eventReport)
        {
            m_sections.Add(eventReport);
        }

        public bool Remove(EventReport eventReport)
        {
            return m_sections.Remove(eventReport);
        }

        public void Add(EventHistory eventHistory)
        {
            m_sections.Add(eventHistory);
        }

        public bool Remove(EventHistory eventHistory)
        {
            return m_sections.Remove(eventHistory);
        }

        public void Add(CommaSeparatedEventReport commaSeparatedEventReport)
        {
            m_sections.Add(commaSeparatedEventReport);
        }

        public bool Remove(CommaSeparatedEventReport commaSeparatedEventReport)
        {
            return m_sections.Remove(commaSeparatedEventReport);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        public static EventFile Parse(string filename)
        {
            string[] lineSeparators = { "\r\n", "\n\r", "\r", "\n" };

            EventFile parsedFile = new EventFile();
            string fileText = File.ReadAllText(filename);

            int firstLineSeparatorIndex = lineSeparators.Select(separator => fileText.IndexOf(separator)).Where(index => index >= 0).Min();
            string lineSeparator = lineSeparators.First(separator => fileText.IndexOf(separator) == firstLineSeparatorIndex);

            string[] lines = fileText
                .Split(new string[] { lineSeparator }, StringSplitOptions.None)
                .Select(line => line.RemoveControlCharacters())
                .ToArray();

            int lineIndex = 0;

            string command;
            EventReport parsedEventReport;
            EventHistory parsedEventHistory;
            CommaSeparatedEventReport parsedCommaSeparatedEventReport;

            while (lineIndex < lines.Length)
            {
                // Parse next command from the file
                command = ParseCommand(lines, ref lineIndex);

                // Skip to the next nonblank line
                SkipBlanks(lines, ref lineIndex);

                if (command.ToUpper().Contains("EVE"))
                {
                    parsedEventReport = EventReport.Parse(lines, ref lineIndex);
                    parsedEventReport.Command = command;
                    parsedFile.Add(parsedEventReport);
                }
                else if (command.ToUpper().Contains("CEV"))
                {
                    parsedCommaSeparatedEventReport = CommaSeparatedEventReport.Parse(lines, ref lineIndex);
                    parsedCommaSeparatedEventReport.Command = (command.ToUpper().Contains("FID") ? command.Split('\"')[0] : command);
                    parsedFile.Add(parsedCommaSeparatedEventReport);
                }
                else if (command.ToUpper().Contains("HIS"))
                {
                    parsedEventHistory = EventHistory.Parse(lines, ref lineIndex);
                    parsedEventHistory.Command = command;
                    parsedFile.Add(parsedEventHistory);
                }

                // Skip to the next nonblank line
                SkipBlanks(lines, ref lineIndex);
            }

            return parsedFile;
        }

        private static string ParseCommand(string[] lines, ref int index)
        {
            StringBuilder command = new StringBuilder();
            string currentLine;
            int commandIndex;

            // Scan forward to the start of the next command
            if (index > 0)
            {
                while (index < lines.Length)
                {
                    currentLine = lines[index];

                    if (currentLine.StartsWith("=>") || currentLine.StartsWith("=>>"))
                        break;

                    index++;
                }
            }

            commandIndex = index;

            // Parse consecutive command lines as one single command
            while (commandIndex < lines.Length)
            {
                currentLine = lines[commandIndex++];

                // Skip over blank lines
                if (string.IsNullOrWhiteSpace(currentLine))
                    continue;

                // If this line isn't the first line in the file and it doesn't
                // start with => or =>>, it is not part of the command
                if (commandIndex > 1 && !currentLine.StartsWith("=>") && !currentLine.StartsWith("=>>"))
                    break;

                // Append the command portion of the line to the command
                command.Append(currentLine.TrimStart('=', '>'));

                // Advance index to the line after this one
                index = commandIndex;
            }

            return command.ToString();
        }

        public static void SkipBlanks(string[] lines, ref int index)
        {
            while (index < lines.Length && string.IsNullOrWhiteSpace(lines[index]))
                index++;
        }

        public static bool TryParseDateTime(string dateTimeString, out DateTime dateTime)
        {
            return DateTime.TryParse(dateTimeString, out dateTime)
                || DateTime.TryParseExact(dateTimeString, new string[] { "y/M/d H:mm:ss.fff", "y/M/d H:mm:ss" }, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out dateTime);
        }

        #endregion
    }
}
