//******************************************************************************************************
//  EVEFile.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
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
using System.Text.RegularExpressions;
using GSF.Collections;

namespace GSF.SELEventParser
{
    public class EventFile
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

            while (lineIndex < lines.Length)
            {
                // Parse next command from the file
                command = ParseCommand(lines, ref lineIndex);

                // Skip to the next nonblank line
                SkipBlanks(lines, ref lineIndex);

                if (command.ToUpper().Contains("EVE"))
                {
                    parsedEventReport = ParseEventReport(lines, ref lineIndex);
                    parsedEventReport.Command = command;
                    parsedFile.Add(parsedEventReport);
                }
                else if (command.ToUpper().Contains("HIS"))
                {
                    parsedEventHistory = ParseEventHistory(lines, ref lineIndex);
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

        private static EventReport ParseEventReport(string[] lines, ref int index)
        {
            EventReport eventReport = new EventReport();
            int firmwareIndex;

            // Parse the report header
            eventReport.Header = ParseHeader(lines, ref index);

            // Skip to the next nonblank line
            SkipBlanks(lines, ref index);

            // Get the index of the line where
            // the firmware information is located
            firmwareIndex = index;

            // Parse the firmware and event number
            eventReport.Firmware = ParseFirmware(lines, ref firmwareIndex);
            eventReport.EventNumber = ParseEventNumber(lines, ref index);
            index = Math.Max(firmwareIndex, index);

            // Skip to the next nonblank line
            SkipBlanks(lines, ref index);

            // Parse the analog section of the report
            eventReport.AnalogSection = ParseAnalogSection(eventReport.Header.EventTime, lines, ref index);

            return eventReport;
        }

        private static EventHistory ParseEventHistory(string[] lines, ref int index)
        {
            EventHistory eventHistory = new EventHistory();

            // Parse the report header
            eventHistory.Header = ParseHeader(lines, ref index);

            // Skip to the next nonblank line
            SkipBlanks(lines, ref index);

            // Parse event histories
            eventHistory.Histories = ParseEventHistoryRecords(lines, ref index);

            return eventHistory;
        }

        private static Header ParseHeader(string[] lines, ref int index)
        {
            const string HeaderLine1 = @"(\S.*)\s+Date:\s+(\S+)\s+Time:\s+(\S+)";
            const string HeaderLine2 = @"(\S.*)(?:\s+Serial Number: (\d+))?";

            Header header = new Header();

            Match regexMatch;
            DateTime eventTime;
            string eventTimeString;
            int serialNumber;

            if (index < lines.Length)
            {
                // Apply regex match to get information contained on first line of header
                regexMatch = Regex.Match(lines[index], HeaderLine1);

                if (regexMatch.Success)
                {
                    // Get relay ID from line 1
                    header.RelayID = regexMatch.Groups[1].Value.Trim();

                    // Build date/time string for parsing
                    eventTimeString = string.Format("{0} {1}", regexMatch.Groups[2].Value, regexMatch.Groups[3].Value);

                    // Get event time from line 1
                    if (TryParseDateTime(eventTimeString, out eventTime))
                        header.EventTime = eventTime;

                    // Advance to the next line
                    index++;

                    if (index < lines.Length)
                    {
                        // Apply regex match to get information contained on second line of header
                        regexMatch = Regex.Match(lines[index], HeaderLine2);

                        if (regexMatch.Success)
                        {
                            // Get station ID and serial number from line 2
                            header.StationID = regexMatch.Groups[1].Value.Trim();

                            if (int.TryParse(regexMatch.Groups[2].Value, out serialNumber))
                                header.SerialNumber = serialNumber;

                            // Advance to the next line
                            index++;
                        }
                    }
                }
            }

            return header;
        }

        private static Firmware ParseFirmware(string[] lines, ref int index)
        {
            const string FirmwareIDRegex = @"FID=(\S+)";
            const string ChecksumRegex = @"CID=(?:0x)?(\S+)";

            Firmware firmware = new Firmware();
            Match firmwareIDMatch;
            Match checksumMatch;

            // Firmware ID and checksum are on the same line --
            // match both regular expressions with the first line
            firmwareIDMatch = Regex.Match(lines[index], FirmwareIDRegex);
            checksumMatch = Regex.Match(lines[index], ChecksumRegex);

            // Get the firmware ID
            if (firmwareIDMatch.Success)
                firmware.ID = firmwareIDMatch.Groups[1].Value;

            // Get the firmware checksum
            if (checksumMatch.Success)
                firmware.Checksum = Convert.ToInt32(checksumMatch.Groups[1].Value, 16);

            // If either match was a success, advance to the next line
            if (firmwareIDMatch.Success || checksumMatch.Success)
                index++;

            return firmware;
        }

        private static int ParseEventNumber(string[] lines, ref int index)
        {
            const string EventNumberRegex = @"Event Number = (\d+)";

            // Match the regular expression to get the event number
            Match eventNumberMatch = Regex.Match(lines[index], EventNumberRegex);

            if (eventNumberMatch.Success)
            {
                // Match was successful so
                // advance to the next line
                index++;

                // Get the event number
                return Convert.ToInt32(eventNumberMatch.Groups[1].Value);
            }

            // Unable to get event number
            // so default to zero
            return 0;
        }

        private static AnalogSection ParseAnalogSection(DateTime eventTime, string[] lines, ref int index)
        {
            const string CycleHeader = @"^\[\d+\]$";

            AnalogSection analogSection = new AnalogSection();

            int headerLineIndex;
            int firstDataLineIndex;
            int dataLineIndex;
            int triggerLineIndex;

            string[] headers = null;
            string[] fields = null;
            int analogEndIndex = -1;

            double[] analogs;
            double analog = 0.0;

            Cycle<DateTime> currentTimeCycle = null;
            List<Cycle<double>> currentCycles = null;

            string currentLine;
            int sampleCount = 0;
            int eventSample = -1;

            int firstCycleCount;
            long timePerSample;

            // Scan forward to the first line of data
            firstDataLineIndex = index;

            while (firstDataLineIndex < lines.Length)
            {
                fields = Regex.Split(lines[firstDataLineIndex], @"\s|>|\*").Where(s => s != string.Empty).ToArray();

                if (fields.Length > 0 && double.TryParse(fields[0], out analog))
                    break;

                firstDataLineIndex++;
            }

            if (firstDataLineIndex >= lines.Length)
                return analogSection;

            // Scan backward to find the header line
            headerLineIndex = firstDataLineIndex - 1;

            while (headerLineIndex >= index)
            {
                headers = lines[headerLineIndex].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                if (headers.Length == fields.Length)
                    break;

                headerLineIndex--;
            }

            if (headerLineIndex < index)
                return analogSection;
            
            // Scan forward to either the trigger or the
            // largest current, whichever comes first,
            // and use it to determine the analogEndIndex
            triggerLineIndex = firstDataLineIndex;

            while (firstDataLineIndex < lines.Length)
            {
                currentLine = lines[triggerLineIndex++];

                // If the current line is empty or it matches the cycle header, skip it
                if (string.IsNullOrWhiteSpace(currentLine) || Regex.IsMatch(currentLine, CycleHeader))
                    continue;

                // If the length of the current line is not within one character
                // of the length of the first data line, assume we have reached
                // the end of the section and stop scanning lines
                if (Math.Abs(currentLine.Length - lines[firstDataLineIndex].Length) > 1)
                    break;

                // Check if this is the trigger row
                analogEndIndex = currentLine.IndexOf('>');

                if (analogEndIndex >= 0)
                    break;

                // Check if this is the largest current row
                analogEndIndex = currentLine.IndexOf('*');

                if (analogEndIndex >= 0)
                    break;
            }

            // If analogEndIndex is valid, parse the header row again
            if (analogEndIndex >= 0 && analogEndIndex < lines[headerLineIndex].Length)
                headers = lines[headerLineIndex].Remove(analogEndIndex).Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            // Generate analog channels from header row
            analogSection.AnalogChannels = headers
                .Select(header => new Channel<double>() { Name = header })
                .ToList();

            // Scan through the lines of data
            dataLineIndex = firstDataLineIndex;

            while (dataLineIndex < lines.Length)
            {
                currentLine = lines[dataLineIndex++];

                // Empty lines or cycle headers indicate start of next cycle
                if (string.IsNullOrWhiteSpace(currentLine) || Regex.IsMatch(currentLine, CycleHeader))
                {
                    // Two empty lines in a row indicates the end of the section
                    if ((object)currentCycles == null)
                        break;

                    currentCycles = null;
                    continue;
                }

                // If the line does not indicate the start of a cycle,
                // check the length to make sure we are still in the analog section
                if (Math.Abs(currentLine.Length - lines[firstDataLineIndex].Length) > 1)
                    break;

                // Parse this line as a line of data
                if (analogEndIndex >= 0 && analogEndIndex < currentLine.Length)
                    currentLine = currentLine.Remove(analogEndIndex);

                analogs = currentLine
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                    .TakeWhile(field => double.TryParse(field, out analog))
                    .Select(field => analog)
                    .ToArray();

                // Remove analog channels whose values cannot be parsed as doubles
                while (analogSection.AnalogChannels.Count > analogs.Length)
                    analogSection.AnalogChannels.RemoveAt(analogSection.AnalogChannels.Count - 1);

                // Ensure the existence of a list of cycles to hold the analog values
                if ((object)currentCycles == null)
                {
                    currentCycles = analogSection.AnalogChannels.Select(channel => new Cycle<double>()).ToList();

                    for (int i = 0; i < currentCycles.Count; i++)
                        analogSection.AnalogChannels[i].Cycles.Add(currentCycles[i]);
                }

                // Add the analogs to their respective cycles
                for (int i = 0; i < analogs.Length && i < currentCycles.Count; i++)
                    currentCycles[i].Samples.Add(analogs[i]);

                // Determine whether this line represents the sample that triggered the event
                if (currentLine.Contains('>') || (eventSample == -1 && currentLine.Contains('*')))
                    eventSample = sampleCount;

                sampleCount++;
                index = dataLineIndex;
            }

            // If we did not find any samples marked with the
            // event time, assume it is the first sample
            if (eventSample < 0)
                eventSample = 0;

            // Determine the time per sample, in ticks
            firstCycleCount = analogSection.AnalogChannels.First().Cycles.First().Samples.Count;
            timePerSample = TimeSpan.TicksPerSecond / (60L * firstCycleCount);

            for (int i = 0; i < sampleCount; i++)
            {
                if ((i % firstCycleCount) == 0)
                {
                    currentTimeCycle = new Cycle<DateTime>();
                    analogSection.TimeChannel.Cycles.Add(currentTimeCycle);
                }

                // Null reference not possible since 0 % firstCycleCount is 0
                currentTimeCycle.Samples.Add(eventTime + TimeSpan.FromTicks(timePerSample * (i - eventSample)));
            }

            return analogSection;
        }

        private static List<EventHistoryRecord> ParseEventHistoryRecords(string[] lines, ref int index)
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
            SkipBlanks(lines, ref index);

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
                                if ((object)time != null && TryParseDateTime(string.Format("{0} {1}", date, time), out dateTime))
                                    eventHistory.Time = dateTime;

                                break;

                            case "TIME":
                                // Parse the field as a time value
                                time = field.Text;

                                // If both date and time have been provided, parse them as a DateTime
                                if ((object)date != null && TryParseDateTime(string.Format("{0} {1}", date, time), out dateTime))
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

        private static void SkipBlanks(string[] lines, ref int index)
        {
            while (index < lines.Length && string.IsNullOrWhiteSpace(lines[index]))
                index++;
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

        private static bool TryParseDateTime(string dateTimeString, out DateTime dateTime)
        {
            return DateTime.TryParse(dateTimeString, out dateTime)
                || DateTime.TryParseExact(dateTimeString, new string[] { "y/M/d H:mm:ss.fff", "y/M/d H:mm:ss" }, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out dateTime);
        }

        #endregion
    }
}
