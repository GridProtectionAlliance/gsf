//******************************************************************************************************
//  AnalogSection.cs - Gbtc
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
using System.Linq;
using System.Text.RegularExpressions;

namespace GSF.SELEventParser
{
    public class AnalogSection
    {
        #region [ Members ]

        // Fields
        private Channel<DateTime> m_timeChannel;
        private List<Channel<double>> m_analogChannels;
        private List<Channel<bool?>> m_digitalChannels;

        #endregion

        #region [ Constructors ]

        public AnalogSection()
        {
            m_timeChannel = new Channel<DateTime>() { Name = "Time" };
            m_analogChannels = new List<Channel<double>>();
            m_digitalChannels = new List<Channel<bool?>>();
        }

        #endregion

        #region [ Properties ]

        public Channel<DateTime> TimeChannel
        {
            get
            {
                return m_timeChannel;
            }
            set
            {
                m_timeChannel = value;
            }
        }

        public List<Channel<double>> AnalogChannels
        {
            get
            {
                return m_analogChannels;
            }
            set
            {
                m_analogChannels = value;
            }
        }

        public List<Channel<bool?>> DigitalChannels
        {
            get
            {
                return m_digitalChannels;
            }
            set
            {
                m_digitalChannels = value;
            }
        }

        #endregion

        #region [ Methods ]

        public Channel<double> GetAnalogChannel(string name)
        {
            return m_analogChannels.FirstOrDefault(channel => channel.Name == name);
        }

        public Channel<bool?> GetDigitalChannel(string name)
        {
            return m_digitalChannels.FirstOrDefault(channel => channel.Name == name);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        public static AnalogSection Parse(DateTime eventTime, string[] lines, ref int index)
        {
            return Parse(eventTime, 60.0D, lines, ref index);
        }

        public static AnalogSection Parse(DateTime eventTime, double systemFrequency, string[] lines, ref int index)
        {
            const string CycleHeader = @"^\[\d+\]$";

            AnalogSection analogSection = new AnalogSection();

            if (lines.Length < index)
                return analogSection;

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
                    .Replace("-", " -")
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
            timePerSample = TimeSpan.TicksPerSecond / (long)(systemFrequency * firstCycleCount);

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

        #endregion
    }
}
