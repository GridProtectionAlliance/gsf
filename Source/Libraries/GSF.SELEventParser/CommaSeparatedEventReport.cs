//******************************************************************************************************
//  StringExtensions.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  11/01/2016 - Billy Ernest
//       Created original version of source code modeled after "Event Report" class
//  08/04/2017 - F. Russell Robertson
//       Re-factored class to utilize new StringParser Class in GSF, new GSF string extensions and Log4Net
//		 Added SectionsDefinions Class and support for new ByteSum class
//  08/30/2018 - F. Russell Robertson
//      Fixed settings parsing error found by LG&E for SEL 421 produced file with complete re-write of settings block parser
//         -- As before, error persists that for the SEL devices produce multi-line values for keys, ONLY the first line of values is captured.
//         -- Now only one settings region for settings key:value pairs.  No duplication in keys have been found.
//      Added options to not process digital data or settings to reduce parsing time for those only interested in analog data
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GSF.Parsing;

namespace GSF.SELEventParser
{
    public class CommaSeparatedEventReport
    {
        #region [ Members ]

        // Nested Types

        public struct SectionDefinition
        {
            public string Name;
            public int StartLine;
            public int Length;

            public SectionDefinition(string Name, int StartLine, int Length)
            {
                this.StartLine = StartLine;
                this.Length = Length;
                this.Name = Name;
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(Name))
                    return "";

                return string.Format("{0}: StartLine = {1}, Length = {2}", Name, StartLine, Length);
            }
        }

        // Fields
        private const string m_triggerFieldName = "TRIG";   //this is the field name that is the trigger and must separate the analog and digital data
        private const string DoubleQuote = "\"";

        #endregion

        #region [ Properties ]

        public Dictionary<string, string> Settings { get; set; }
        public Header Header { get; set; }
        public Firmware Firmware { get; set; }
        public AnalogSection AnalogSection { get; set; }
        public double FrequencyAverage { get; set; }
        public double FrequencyNominal { get; set; } = 60D;
        public double SamplesPerCycleAnalog { get; set; }
        public double SamplesPerCycleDigital { get; set; }
        public double NumberOfCycles { get; set; }
        public string Event { get; set; }
        public int TriggerIndex { get; set; }
        public int InitialReadingIndex { get; set; }
        public string[] SettingsRegions { get; set; }
        public int ExpectedAnalogCount { get; set; }
        public int ExpectedDigitalCount { get; set; }
        public int ExpectedDataRecordValueCount { get; set; }
        public int ExpectedSampleCount { get; set; }
        public bool ProcessSettings { get; set; } = false;
        public bool ProcessDigitals { get; set; } = false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns the value for a specific setting group and key
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="valueKey"></param>
        /// <param name="value"></param>
        public string GetSettingValue(string groupName, string valueKey)
        {
            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(valueKey))
                return string.Empty;

            valueKey = string.Concat(groupName.Trim(), ":", valueKey.Trim());

            if (Settings.ContainsKey(valueKey))
                return Settings[valueKey];

            return string.Empty;
        }

        /// <summary>
        /// Returns the value for a specific setting group and key
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="valueKey"></param>
        /// <param name="value"></param>
        public string GetSettingValue(string fullKey)
        {
            if (string.IsNullOrEmpty(fullKey))
                return string.Empty;

            if (Settings.ContainsKey(fullKey))
                return Settings[fullKey];

            return string.Empty;
        }

        #endregion

        #region [ Static ]

        // Static Fields

        public static event EventHandler<EventArgs<string>> DebugMessage;

        // Static Methods

        /// <summary>
        /// Parses CEV files.
        /// </summary>
        /// <param name="lines">The string array of SEL.cev lines to process</param>
        /// <param name="processSettings">Set to TRUE to process settings block</param>
        /// <param name="processDigitals">Set to TRUE to process digital values block</param>
        /// <param name="fileIdentifier">For error logging, an identifier of the file being processed -- typically the filename.</param>
        /// <param name="maxFileDuration">Set to a positive value limit the number of data records processed.</param>
        /// <remarks>Removed lineIndex since file must be processed in sequence. </remarks>
        /// <returns>Data model representing the comma separated event report.</returns>
        public static CommaSeparatedEventReport Parse(string[] lines, bool processDigitals = true, bool processSettings = false, string fileIdentifier = "", double maxFileDuration = 0.0D)
        {
            //OnDebugMessage(string.Format("Parsing SEL CEV file: {0}", fileIdentifier));

            if (lines == null || lines.Length == 0)
            {
                OnDebugMessage(string.Format("SEL CEV Parse aborted.  Nothing to do. Sent line array from file {0} is null or empty.", fileIdentifier));
                return null;
            }

            CommaSeparatedEventReport cSER = new CommaSeparatedEventReport();
            cSER.Firmware = new Firmware();
            cSER.Header = new Header();

            cSER.ProcessSettings = processSettings;
            cSER.ProcessDigitals = processDigitals;

            int lineIndex = 0;      //relative to the first line in the file
            string inString = string.Empty;
            int headerRecordNumber = 1;

            string[] headerFields = null;
            string[] lastHeaderFields = null;

            //------------------------------------------------  THE HEADER BLOCK --------------------------------------------------------

            //Header Section -- 7 records expected
            //It's reasonable to assume that for a file to be valid it must contain the correct number of headers in the proper order
            //Returns null if header is significantly malformed.
            //However, it will try to survive bad bytesum checks

            while (lineIndex < lines.Length)
            {
                headerFields = StringParser.ParseStandardCSV(lines[lineIndex]);
                if (headerFields != null && headerFields[0].ToUpper().Contains("FID"))
                    break;
                lineIndex++;
            }
            if (lineIndex >= lines.Length)
            {
                OnDebugMessage(string.Format("No SEL CEV data found. Nothing to do processing file {0} of length {1}", fileIdentifier, lines.Length.ToString()));
                return null;
            }

            while (headerRecordNumber < 8)
            {
                inString = lines[lineIndex].Trim();

                if (!string.IsNullOrEmpty(inString))
                {
                    ByteSum byteSum = new ByteSum();

                    headerFields = StringParser.ParseStandardCSV(inString);

                    switch (headerRecordNumber)
                    {
                        case 1:
                            //field names for headerRecord 2 -- already verified that field 0 contains 'FID'
                            byteSum.Check(inString, fileIdentifier);

                            if (!byteSum.Match)  //moved inside case switch due to case 7
                                OnDebugMessage(string.Format("ByteSum check failed for header record {0} in SEL CEV file: {1}", headerRecordNumber, fileIdentifier));

                            if (headerFields.Length < 2)
                                OnDebugMessage(string.Format("Processing SEL CEV header record 1 for SEL CEV file: {0}  Expected at least 2 fields and {1} were found.", fileIdentifier, headerFields.Length));

                            headerRecordNumber++;
                            //lastHeaderFields = headerFields;
                            break;

                        case 2:
                            //The FID and Firmware Version Number
                            byteSum.Check(inString, fileIdentifier);

                            if (!byteSum.Match)  //moved inside case switch due to case 7
                                OnDebugMessage(string.Format("ByteSum check failed for header record {0} in SEL CEV file: {1}", headerRecordNumber, fileIdentifier));

                            if (headerFields.Length < 2)
                                OnDebugMessage(string.Format("Processing SEL CEV  header record 2 for SEL CEV file: {0}  Expected at least 2 fields and {1} were found.", fileIdentifier, headerFields.Length));

                            if (!headerFields[0].Contains("SEL"))
                            {
                                OnDebugMessage(string.Format("Processing field 0 for header record 2 for file {0}  Expected string to contain 'SEL' and '{1}' was found.", fileIdentifier, headerFields[0]));
                                OnDebugMessage(string.Format("Processing TERMINATED for SEL CEV file: {0}", fileIdentifier));
                                return null;
                            }

                            if (headerFields.Length > 2)
                                cSER.Firmware.ID = headerFields[1].Trim();

                            headerRecordNumber++;
                            //lastHeaderFields = headerFields;
                            break;

                        case 3:
                            //The headers for the date data
                            byteSum.Check(inString, fileIdentifier);

                            if (!byteSum.Match)  //moved inside case switch due to case 7
                                OnDebugMessage(string.Format("ByteSum check failed for header record {0} in SEL CEV file: {1}", headerRecordNumber, fileIdentifier));

                            string[] expectedFieldNames3 = { "MONTH", "DAY", "YEAR", "HOUR", "MIN", "SEC", "MSEC" };

                            if (!StringParser.ExpectedFieldNamesMatch(expectedFieldNames3, headerFields, true, 6))
                            {
                                OnDebugMessage("Processing SEL CEV header record 3, field names for date header. The expected values for the date labels did not match.");
                                OnDebugMessage(string.Format("Processing TERMINATED for SEL CEV file: {0}", fileIdentifier));
                                return null;
                            }

                            headerRecordNumber++;
                            //lastHeaderFields = headerFields;
                            break;

                        case 4:
                            //The file date values
                            byteSum.Check(inString, fileIdentifier);

                            if (!byteSum.Match)  //moved inside case switch due to case 7
                                OnDebugMessage(string.Format("ByteSum check failed for header record {0} in SEL CEV file: {1}", headerRecordNumber, fileIdentifier));

                            if (headerFields.Length < 6)
                                OnDebugMessage(string.Format("Processing SEL CEV header record 4 for SEL CEV file: {0}  Expected at least 6 fields and {1} were found.", fileIdentifier, headerFields.Length));
                            else
                            {
                                int[] values;

                                if (!TryConvertInt32(headerFields, out values, headerFields.Length - 1))
                                {
                                    OnDebugMessage(string.Format("One or more date fields in header record 4 did not parse to integers.  Event time not set in SEL CEV File: {0}", fileIdentifier));
                                }
                                else
                                {
                                    cSER.Header.EventTime = new DateTime(values[2], values[0], values[1], values[3], values[4], values[5]);
                                    if (cSER.Header.EventTime.CompareTo(Convert.ToDateTime("01/01/2000")) < 0)
                                        OnDebugMessage(string.Format("The event time of {0} is prior to January 1, 2000.", cSER.Header.EventTime.ToShortDateString()));
                                }
                            }

                            headerRecordNumber++;
                            //lastHeaderFields = headerFields;
                            break;

                        case 5:
                            //The headers for the summary data - fields can appear in any order
                            byteSum.Check(inString, fileIdentifier);

                            if (!byteSum.Match)  //moved inside case switch due to case 7
                                OnDebugMessage(string.Format("ByteSum check failed for header record {0} in SEL CEV file: {1}", headerRecordNumber, fileIdentifier));

                            if (StringParser.FindIndex("FREQ", headerFields) < 0 || StringParser.FindIndex("EVENT", headerFields) < 0)  //just check a couple
                            {
                                OnDebugMessage("Processing header record 5 and the minimum expected values of 'FREQ' and 'EVENT' were not found.");
                                OnDebugMessage(string.Format("Processing TERMINATED for SEL CEV file: {0}", fileIdentifier));
                                return null;
                            }

                            headerRecordNumber++;
                            lastHeaderFields = headerFields;
                            break;

                        case 6:
                            //The summary data  - no try-parse data tests performed since there is confirmation of the availability of specific fields
                            byteSum.Check(inString, fileIdentifier);

                            if (!byteSum.Match)  //moved inside case switch due to case 7
                                OnDebugMessage(string.Format("ByteSum check failed for header record {0} in SEL CEV file: {1}", headerRecordNumber, fileIdentifier));

                            if (headerFields.Length != lastHeaderFields.Length)
                            {
                                OnDebugMessage(string.Format("Processing header record 6 -- expected {0} values and {1} were found", lastHeaderFields.Length, headerFields.Length));
                                OnDebugMessage(string.Format("Processing TERMINATED for SEL CEV file: {0}", fileIdentifier));
                                return null;
                            }

                            //For completeness, not needed
                            cSER.Header.SerialNumber = 0;
                            cSER.Header.RelayID = "";
                            cSER.Header.StationID = "";

                            //set key class properties
                            //nominal frequency is based on average found.

                            cSER.FrequencyAverage = Convert.ToDouble(headerFields[StringParser.FindIndex("FREQ", lastHeaderFields)]);
                            if (cSER.FrequencyAverage > 48D && cSER.FrequencyAverage < 52D)
                                cSER.FrequencyNominal = 50D;
                            else
                                cSER.FrequencyNominal = 60D;
                            cSER.Event = headerFields[StringParser.FindIndex("EVENT", lastHeaderFields)];

                            int labelIndex = StringParser.FindIndex("SAM/CYC_A", lastHeaderFields);
                            if (labelIndex > 0)
                                cSER.SamplesPerCycleAnalog = Convert.ToDouble(headerFields[labelIndex]);
                            labelIndex = StringParser.FindIndex("SAM/CYC_D", lastHeaderFields);
                            if (labelIndex > 0)
                                cSER.SamplesPerCycleDigital = Convert.ToDouble(headerFields[labelIndex]);
                            labelIndex = StringParser.FindIndex("NUM_OF_CYC", lastHeaderFields);
                            if (labelIndex > 0)
                                cSER.NumberOfCycles = Convert.ToDouble(headerFields[labelIndex]);
                            labelIndex = StringParser.FindIndex("NUM_CH_A", lastHeaderFields);
                            if (labelIndex > 0)
                                cSER.ExpectedAnalogCount = Convert.ToInt32(headerFields[labelIndex]);
                            labelIndex = StringParser.FindIndex("NUM_CH_D", lastHeaderFields);
                            if (labelIndex > 0)
                                cSER.ExpectedDigitalCount = Convert.ToInt32(headerFields[labelIndex]);

                            headerRecordNumber++;
                            lastHeaderFields = headerFields;
                            break;

                        case 7:
                            //The header the data records - assume valid if all three phase currents are present
                            //Note for some files, this record spans multiple lines, will keep reading lines until the quotes match.

                            StringBuilder sb = new StringBuilder(lines[lineIndex]);

                            //find all the data field names - spanning multiple lines
                            while (sb.ToString().CharCount('\"') % 2 == 1)
                            {
                                sb.Append(lines[++lineIndex]);
                                if (lineIndex >= lines.Length)
                                {
                                    OnDebugMessage(string.Format("Only partial CEV header data found. Processing of SEL CEV file: {0} aborted at line {1}", fileIdentifier, lineIndex.ToString()));
                                    return null;
                                }
                            }

                            headerFields = StringParser.ParseStandardCSV(sb.ToString().Trim());
                            byteSum.Check(inString, fileIdentifier);

                            if (!byteSum.Match)  //moved inside case switch due to case 7
                                OnDebugMessage(string.Format("ByteSum check failed for header record {0} in SEL CEV file: {1}", headerRecordNumber, fileIdentifier));

                            if (StringParser.FindIndex("IA", headerFields, false, true) < 0 || StringParser.FindIndex("IB", headerFields, false, true) < 0 ||
                                StringParser.FindIndex("IC", headerFields, false, true) < 0)
                            {
                                OnDebugMessage("Processing header record 7, the field names for the data records, and did not find the minimum set of 'IA', 'IB' and 'IC'");
                                OnDebugMessage(string.Format("Processing TERMINATED for SEL CEV file: {0}", fileIdentifier));
                                return null;
                            }

                            headerRecordNumber++;
                            lastHeaderFields = headerFields;
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    OnDebugMessage(string.Format("Unexpected empty header reader in advance of header record number {0} for SEL CEV file: {1}", headerRecordNumber, fileIdentifier));
                }

                lineIndex++;
                if (lineIndex >= lines.Length)
                {
                    OnDebugMessage(string.Format("Only partial CEV header data found. Processing of SEL CEV file: {0} aborted at line {1}", fileIdentifier, lineIndex.ToString()));
                    return null;
                }
            }

            cSER.InitialReadingIndex = lineIndex;

            //determine the number of analog data fields based on the position of "TRIG" (the trigger field name) and setup up Analog Section
            int triggerFieldPosition = Array.FindIndex(headerFields, x => x.ToUpper().Contains(m_triggerFieldName));
            if (triggerFieldPosition < 0)  //not found
            {
                OnDebugMessage(string.Format("Processing header record 8, the field names for data, searching for {0} as the analog/digital data field separator.  It was not found within the values of {1}.",
                                    m_triggerFieldName, headerFields.ToString()));

                OnDebugMessage(string.Format("Processing TERMINATED for SEL CEV file: {0}", fileIdentifier));
                return null;
            }

            if (headerFields.Length < triggerFieldPosition + 2)  //too few field names past separator
            {
                OnDebugMessage(string.Format("Processing header record 8, the field names for data, too few field names found past {0} (the analog/digital data field separator) within the values of {1}.",
                                   m_triggerFieldName, headerFields.ToString()));

                OnDebugMessage(string.Format("Processing TERMINATED for SEL CEV file: {0}", fileIdentifier));
                return null;
            }

            cSER.AnalogSection = new AnalogSection();

            //loop through the expected analog fields, add all the fields but "TRIG" (the trigger field name)

            if (cSER.ExpectedAnalogCount <= 0)
                cSER.ExpectedAnalogCount = triggerFieldPosition;

            //expected value count = analogs + trigger + digitals + bytesum (analogs plus 3)
            cSER.ExpectedDataRecordValueCount = cSER.ExpectedAnalogCount + 3;

            //for speed, the scaling factors, if any are used, are pre-positioned
            double[] scalingFactors = new double[cSER.ExpectedAnalogCount];
            bool scalingRequired = false;
            for (int fieldIndex = 0; fieldIndex < triggerFieldPosition; fieldIndex++)
            {
                cSER.AnalogSection.AnalogChannels.Add(new Channel<double>());
                cSER.AnalogSection.AnalogChannels[fieldIndex].Name = headerFields[fieldIndex];
                if (headerFields[fieldIndex].ToUpper().Contains("KV"))
                {
                    scalingFactors[fieldIndex] = 1000D;
                    scalingRequired = true;
                }
                else
                    scalingFactors[fieldIndex] = 1D;
            }

            //loop through the digital channels
            int digitalChannelCount = 0;
            foreach (string channel in headerFields[triggerFieldPosition + 1].QuoteUnwrap().RemoveDuplicateWhiteSpace().Trim().Split(' '))
            {
                cSER.AnalogSection.DigitalChannels.Add(new Channel<bool?>());
                cSER.AnalogSection.DigitalChannels[cSER.AnalogSection.DigitalChannels.Count - 1].Name = channel;
                digitalChannelCount++;
            }

            if (cSER.ExpectedDigitalCount <= 0)
                cSER.ExpectedDigitalCount = digitalChannelCount;
            else if (digitalChannelCount != cSER.ExpectedDigitalCount)
                OnDebugMessage(string.Format("Processing SEL CEV header record 8, the field names for data, the {0} digital channel names found does not match the expected number of {1}",
                  cSER.ExpectedDigitalCount, digitalChannelCount));

            //find the trigger record within the data section, Carry on if none found.
            int triggerIndexRelative = 0;   //relative to the first data line
            for (lineIndex = cSER.InitialReadingIndex; lineIndex < lines.Length; lineIndex++)
            {
                if (string.IsNullOrEmpty(lines[lineIndex]) || lines[lineIndex].Trim().Length == 0)
                {
                    OnDebugMessage(string.Format("Null or empty data record was found at line {0} in file {1} and was skipped in the determination of the trigger record time.", lineIndex, fileIdentifier));
                    //this condition logged at Info level later
                    continue;  //skip this line to be consistent with data parsing logic.
                }

                string[] s = lines[lineIndex].Split(',');                 //use the split function for speed
                if (s.Length > triggerFieldPosition && s[triggerFieldPosition].Trim().Length > 0)
                {
                    cSER.TriggerIndex = triggerIndexRelative;
                    break;
                }
                if (s.Length > 0 && s[0].ToUpper().Contains("SETTINGS"))  //we're done with data and no trigger was found.
                {
                    triggerIndexRelative = 0;
                    OnDebugMessage(string.Format("No trigger index found in SEL CEV file: {0}", fileIdentifier));
                    break;
                }
                ++triggerIndexRelative;
            }

            if (lineIndex >= lines.Length)  //we've looped through all the lines, no trigger && no SETTINGS
            {
                OnDebugMessage(string.Format("No SETTINGS line terminator and No trigger index found in SEL CEV file: {0}", fileIdentifier));
                //this condition logged at the Info level later
                triggerIndexRelative = 0;
            }

            //Log significant info about the file
            //OnDebugMessage(string.Format("Found {0} analog channels and {1} digital channels to process within the SEL CEV file: {2} with an event time of {3} and a relative trigger index of {4}",
            //   commaSeparatedEventReport.AnalogSection.AnalogChannels.Count, commaSeparatedEventReport.AnalogSection.DigitalChannels.Count, fileIdentifier,
            //   commaSeparatedEventReport.Header.EventTime.ToLongDateString(), triggerIndexRelative.ToString()));


            int timeStepTicks = Convert.ToInt32(Math.Round(10000000.0 / cSER.FrequencyNominal / cSER.SamplesPerCycleAnalog));
            //Time (in ticks) is relative to the trigger line (record).
            //Negative in advance of the trigger record, Zero at the trigger record, Positive following the trigger recored.
            int lineTicks = -1 * triggerIndexRelative * timeStepTicks;

            //Log significant time-based info
            //OnDebugMessage(string.Format("Starting line tics: {0} Incremental tics per record: {1}", lineTicks, timeStepTicks));

            //set data record limit
            int dataRecordLimit = (int)Math.Round(maxFileDuration * cSER.FrequencyNominal * cSER.SamplesPerCycleAnalog);

            if (dataRecordLimit > 0)
                dataRecordLimit = ((lines.Length - cSER.InitialReadingIndex) > dataRecordLimit) ? dataRecordLimit : lines.Length - cSER.InitialReadingIndex;
            else
                dataRecordLimit = lines.Length - cSER.InitialReadingIndex;

            //------------------------------------------------  THE DATA BLOCK --------------------------------------------------------
            //Now loop through the lines to get the data
            //Empty lines are ignored (i.e., time is not incremented) [OnDebugMessage]
            //For radically malformed lines time is incremented and all analogs are set to NaN and digitals set to null [OnDebugMessage]
            //Data field order and type are set by the header and do not vary with the data region

            int dataRecordCount = 0;
            for (lineIndex = cSER.InitialReadingIndex; lineIndex < cSER.InitialReadingIndex + dataRecordLimit; lineIndex++)
            {
                string[] data = StringParser.ParseStandardCSV(lines[lineIndex]);
                dataRecordCount++;

                if (data == null || data.Length == 0)
                {
                    OnDebugMessage(string.Format("Data record {0} in SEL CEV file: {1} was empty and was skipped.", dataRecordCount, fileIdentifier));
                    continue; //get next line
                }

                if (data.Length > 0 && data[0].ToUpper().Contains("SETTINGS"))  //we're done with the data
                    break;

                //increment time
                cSER.AnalogSection.TimeChannel.Samples.Add(cSER.Header.EventTime.AddTicks(lineTicks));
                lineTicks += timeStepTicks;

                if (data.Length != cSER.ExpectedDataRecordValueCount)
                {
                    OnDebugMessage(string.Format("Data record {0} in SEL CEV file: {1} did not contain the anticipated values.", dataRecordCount, fileIdentifier));

                    //let's try to survive it.
                    foreach (var analogChannel in cSER.AnalogSection.AnalogChannels)
                    {
                        analogChannel.Samples.Add(Double.NaN);    //what are the consequences here??
                    }
                    foreach (Channel<bool?> channel in cSER.AnalogSection.DigitalChannels)
                    {
                        channel.Samples.Add(null);
                    }
                    continue;  //get next line
                }

                //check bytesum
                ByteSum byteSum = new ByteSum();

                byteSum.Check(lines[lineIndex], fileIdentifier);

                if (!byteSum.Match)
                {
                    OnDebugMessage(string.Format("Byte sum does not match for data record {0} in SEL CEV file {1}. This record processed as if it is valid.", dataRecordCount, fileIdentifier));
                }

                //LOAD ANALOG DATA (overall record tests above are sufficient to verify expected number of values)
                int channelIndex = 0;
                foreach (var analogChannel in cSER.AnalogSection.AnalogChannels)
                {
                    double value = 0D;
                    if (!double.TryParse(data[channelIndex], out value))
                        analogChannel.Samples.Add(Double.NaN);
                    else if (scalingRequired)
                        analogChannel.Samples.Add(Convert.ToDouble(data[channelIndex]) * scalingFactors[channelIndex]);
                    else
                        analogChannel.Samples.Add(Convert.ToDouble(data[channelIndex]));

                    channelIndex++;

                    //analogChannel.Samples.Add(Convert.ToDouble(lines[lineIndex].Split(',')[channelIndex]) * (lineFields[channelIndex++].ToUpper().Contains("KV") ? 1000 : 1));
                }

                if (cSER.ProcessDigitals)
                {

                    //LOAD DIGITAL DATA
                    char[] hexDigitals = data[cSER.AnalogSection.AnalogChannels.Count + 1].QuoteUnwrap().Trim().ToCharArray(); //digitals always on the other side of "TRIG"

                    if (hexDigitals.Length == 0)
                        continue;

                    if (hexDigitals.Length * 4 < cSER.AnalogSection.DigitalChannels.Count)
                    {
                        OnDebugMessage(string.Format("The expected {0} digital channels were not found for data record {1} in SEL CEV file {2}.  {3} were found.  Setting digitals to null and continuing.",
                            hexDigitals.Length * 4, dataRecordCount, fileIdentifier, cSER.AnalogSection.DigitalChannels.Count));
                        foreach (Channel<bool?> channel in cSER.AnalogSection.DigitalChannels)
                        {
                            channel.Samples.Add(null);
                        }
                        continue;  //get next line
                    }

                    channelIndex = 0;
                    int hexCharIndex = 0;
                    if (cSER.AnalogSection.DigitalChannels.Count > 0)
                    {
                        foreach (Channel<bool?> channel in cSER.AnalogSection.DigitalChannels)  //loop through the channels and add the values
                        {
                            hexCharIndex = channelIndex / 4;
                            if (hexDigitals[hexCharIndex].IsHex())
                            {
                                BitArray ba = hexDigitals[hexCharIndex].ConvertHexToBitArray();
                                //OnDebugMessage(string.Format("dig channel:{0} hex:{1}, position:{2}, value:{3}", channelIndex, hexDigitals[hexCharIndex], channelIndex % 4, ba[channelIndex % 4].ToString()));  //validation of correct digital logic

                                channel.Samples.Add(ba[channelIndex % 4]);
                            }
                            else
                                channel.Samples.Add(null);

                            channelIndex++;

                        }
                    }
                }

            }

            cSER.ExpectedSampleCount = dataRecordCount;
            //OnDebugMessage(string.Format("Successfully processed {0} data records in SEL CEV file: {1}", dataRecordCount, fileIdentifier));

            //------------------------------  END DATA BLOCK -----------------------------------------

            //Directed to not process settings or lines.Length busted.
            if (!cSER.ProcessSettings || lineIndex > lines.Length)
            {
                return cSER;
            }

            //advance to 'SETTINGS' if we're not there already
            if (!lines[lineIndex].Contains("SETTINGS"))
            {
                while (lineIndex < lines.Length)
                {
                    string[] temp = StringParser.ParseStandardCSV(lines[lineIndex]);
                    if (temp != null && string.Equals(temp[0].ToUpper(), "SETTINGS"))
                        break;
                    lineIndex++;
                }
            }

            if (lineIndex >= lines.Length)  //we've looped through all the lines no settings found to add
            {
                OnDebugMessage(string.Format("No settings were found following the SETTINGS line terminator was found at end of data section in SEL CEV file: {0}", fileIdentifier));
                return cSER;
            }


            //------------------------------  SETTINGS BLOCK -----------------------------------------

            List<SectionDefinition> settingsRegions = new List<SectionDefinition>();
            string sectionName = "Settings";

            //verify that settings are within quotes -- start line
            int startSettingsLine = -1;
            while (lineIndex < lines.Length)
            {
                if (lines[lineIndex].Trim().Equals(DoubleQuote))
                {
                    startSettingsLine = lineIndex;
                    lineIndex++;
                    break;
                }
                lineIndex++;
            }

            int endSettingsLine = -1;
            //verify that settings are within quotes; i.e,. look for the next quote character
            while (lineIndex < lines.Length)
            {
                if (lines[lineIndex].IndexOf(DoubleQuote) > -1)
                {
                    endSettingsLine = lineIndex;
                    break;
                }
                lineIndex++;
            }

            if (startSettingsLine < 0 || endSettingsLine < 0)
            {
                OnDebugMessage(string.Format("The settings block in the CEV file as malformed.  Processing of this section skipped: {0}", fileIdentifier));
                return cSER;
            }

            lineIndex = startSettingsLine;

            //Settings format differs strongly by SEL make and model
            //Data structure accommodates multiple settings Regions.  However, for now all data is placed in the single region called "settings"
            //Parses the key-value pairs and builds a dictionary from them without regard to headings
            //For test data sets provided, have yet to see a key repeated in a file regardless of the number of sections
            //Keys and values are assumed to be on the same line and are separated by the 2 character string ":="
            // (note: for some SEL devices, value strings are multi-line and this parser does not yet handle this case)

            settingsRegions.Add(new SectionDefinition(sectionName, startSettingsLine, endSettingsLine - startSettingsLine));
            Dictionary<string, string> settingValues = new Dictionary<string, string>();
            string[] regions = new string[1];
            regions[0] = sectionName;

            while (lineIndex <= endSettingsLine)  //there may be content in the last settings line
            {
                if (lines[lineIndex].Length < 2 || lines[lineIndex].IndexOf(":=") == -1)  //skip it
                {
                    lineIndex++;
                    continue;
                }

                int[] keySeparaterIndicies = StringParser.IndicesOfToken(lines[lineIndex], ":=");


                if (keySeparaterIndicies == null || keySeparaterIndicies.Length == 0)
                {
                    lineIndex++;
                    continue;
                }

                for (int j = 0; j < keySeparaterIndicies.Length; j++)
                {
                    int index = keySeparaterIndicies[j];

                    string key = PreviousToken(lines[lineIndex], index);
                    if (string.IsNullOrEmpty(key))
                        continue;

                    string value = string.Empty;

                    if (j < keySeparaterIndicies.Length - 1 && lines[lineIndex].Length > index + 3)
                            value = NextToken(lines[lineIndex], index + 2);
                    else
                        value = lines[lineIndex].Substring(index + 2).Trim();

                    if (string.IsNullOrEmpty(value))
                        value = "0.0";

                    if (settingValues.ContainsKey(key))
                        OnDebugMessage(string.Format("Settings already contains key:{0}", key));
                    else
                        settingValues.Add(key, value);
                }         
                lineIndex++;
            }

            cSER.SettingsRegions = regions;
            cSER.Settings = settingValues;
            return cSER;
        }

        /// <summary>
        /// Gets the settings token in the CEV file that follows the index provided
        /// </summary>
        /// <param name="inString">the text string for this line</param>
        /// <param name="startIndex">the provided index</param>
        /// <returns></returns>
        private static string NextToken(string inString, int startIndex)
        {
            if (string.IsNullOrEmpty(inString))
                return string.Empty;

            if (startIndex < 0 || startIndex > inString.Length - 1)
                return string.Empty;

            int tokenStartIndex = -1;
            int tokenLength = -1;

            bool haveToken = false;
            for (int i = startIndex; i < inString.Length; i++)
            {
                if (inString[i] <= 32 || inString[i] == ':')
                {
                    if (haveToken)
                    {
                        //done
                        tokenLength = i - tokenStartIndex;
                        break;
                    }
                    continue;
                }

                if (!haveToken)
                {
                    tokenStartIndex = i;
                    haveToken = true;
                }
            }

            //there is no next token
            if (tokenStartIndex == -1)
                return string.Empty;

            if (tokenLength == -1)
                tokenLength = inString.Length - tokenStartIndex;

            return inString.Substring(tokenStartIndex, tokenLength);
        }

        /// <summary>
        /// Gets the settings token in the CEV file previous to the provided index
        /// </summary>
        /// <param name="inString">The string for this line</param>
        /// <param name="startIndex">the provided index</param>
        /// <returns></returns>
        private static string PreviousToken(string inString, int startIndex)
        {
            if (string.IsNullOrEmpty(inString))
                return string.Empty;

            if (startIndex < 0 || startIndex > inString.Length - 1)
                return string.Empty;

            inString = inString.Reverse();

            startIndex = inString.Length - startIndex;
            string value = NextToken(inString, startIndex);

            return value.Reverse();
        }

        private static bool TryConvertInt32(string[] data, out int[] values, int length = 0)
        {
            values = null;
            bool allPassed = true;

            if (data == null || data.Length == 0)
                return false;

            if (length <= 0 || length > data.Length)
                length = data.Length;
            values = new int[length];

            for (int i = 0; i < length; i++)
            {
                int value;

                if (int.TryParse(data[i], out value))
                {
                    values[i] = value;
                }
                else
                {
                    allPassed = false;
                    values[i] = 0;
                }
            }

            return allPassed;
        }

        private static void OnDebugMessage(string message)
        {
            DebugMessage?.Invoke(null, new EventArgs<string>(message));
        }

        #endregion
    }
}
