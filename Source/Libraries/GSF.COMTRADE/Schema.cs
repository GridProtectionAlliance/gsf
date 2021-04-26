//******************************************************************************************************
//  Schema.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  05/18/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using GSF.IO;

namespace GSF.COMTRADE
{
    #region [ Enumerations ]

    /// <summary>
    /// <see cref="Schema"/> file type enumeration.
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// ASCII file type.
        /// </summary>
        Ascii,
        /// <summary>
        /// Binary file type, analog channel size = uint16.
        /// </summary>
        Binary,
        /// <summary>
        /// Binary file type, analog channel size = uint32.
        /// </summary>
        Binary32,
        /// <summary>
        /// Binary file type, analog channel size = float32.
        /// </summary>
        Float32
    }

    /// <summary>
    /// <see cref="Schema"/> time quality indicator code.
    /// </summary>
    public enum TimeQualityIndicatorCode : byte
    {
        /// <summary>
        /// Clock locked, Normal operation.
        /// </summary>
        Locked = 0x0,
        /// <summary>
        /// Clock fault, time not reliable.
        /// </summary>
        Failure = 0xF,
        /// <summary>
        /// Clock unlocked, time within 10^1s.
        /// </summary>
        Unlocked10Seconds = 0xB,
        /// <summary>
        /// Clock unlocked, time within 10^0s.
        /// </summary>
        Unlocked1Second = 0xA,
        /// <summary>
        /// Clock unlocked, time within 10^-1s.
        /// </summary>
        UnlockedPoint1Seconds = 0x9,
        /// <summary>
        /// Clock unlocked, time within 10^-2s.
        /// </summary>
        UnlockedPoint01Seconds = 0x8,
        /// <summary>
        /// Clock unlocked, time within 10^-3s.
        /// </summary>
        UnlockedPoint001Seconds = 0x7,
        /// <summary>
        /// Clock unlocked, time within 10^-4s.
        /// </summary>
        UnlockedPoint0001Seconds = 0x6,
        /// <summary>
        /// Clock unlocked, time within 10^-5s.
        /// </summary>
        UnlockedPoint00001Seconds = 0x5,
        /// <summary>
        /// Clock unlocked, time within 10^-6s.
        /// </summary>
        UnlockedPoint000001Seconds = 0x4,
        /// <summary>
        /// Clock unlocked, time within 10^-7s.
        /// </summary>
        UnlockedPoint0000001Seconds = 0x3,
        /// <summary>
        /// Clock unlocked, time within 10^-8s.
        /// </summary>
        UnlockedPoint00000001Seconds = 0x2,
        /// <summary>
        /// Clock unlocked, time within 10^-9s.
        /// </summary>
        UnlockedPoint000000001Seconds = 0x1
    }
    // 4-bits, 1-nibble

    /// <summary>
    /// <see cref="Schema"/> leap second indicator.
    /// </summary>
    public enum LeapSecondIndicator : byte
    {
        /// <summary>
        /// No leap second adjustment was added during recorded data set.
        /// </summary>
        NoLeapSecondAdjustment = 0,
        /// <summary>
        /// Leap second adjustment was added during recorded data set.
        /// </summary>
        LeapSecondWasAdded = 1,
        /// <summary>
        /// Leap second adjustment was subtracted during recorded data set.
        /// </summary>
        LeapSecondWasSubtracted = 2,
        /// <summary>
        /// Leap second adjustment not capable for recording device.
        /// </summary>
        NoLeapSecondCapacity = 3
    }

    #endregion

    /// <summary>
    /// Represents the schema for a configuration file in the COMTRADE file standard, IEEE Std C37.111-1999/2013.
    /// </summary>
    public class Schema
    {
        #region [ Members ]

        // Fields
        private double m_nominalFrequency;
        private SampleRate[] m_sampleRates;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Schema"/>.
        /// </summary>
        public Schema()
        {
            Version = 1999;
            m_nominalFrequency = 60.0D;
            FileType = FileType.Binary;
            TimeFactor = 1.0D;
            SampleRates = null;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Schema"/> from an existing configuration file name.
        /// </summary>
        /// <param name="fileName">File name of configuration file to parse.</param>
        public Schema(string fileName)
            : this(fileName, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Schema"/> from an existing configuration file name.
        /// </summary>
        /// <param name="fileName">File name of configuration file to parse.</param>
        /// <param name="useRelaxedValidation">Indicates whether to relax validation on the number of line image elements.</param>
        public Schema(string fileName, bool useRelaxedValidation)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Configuration file \"{fileName}\" does not exist.");

            // Cache configuration file name used to create schema
            FileName = fileName;
            IsCombinedFileFormat = HasCFFExtension(FileName);

            string[] lines;

            if (IsCombinedFileFormat)
            {
                // Read out configuration file section
                using (StreamReader fileReader = new StreamReader(FileName))
                {
                    List<string> fileLines = new List<string>();
                    bool firstLine = true;

                    do
                    {
                        string line = fileReader.ReadLine();

                        if (firstLine)
                        {
                            if (!IsFileSectionSeparator(line, out string sectionType) || sectionType != "CFG")
                                throw new InvalidOperationException($"Unexpected file section separator for configuration file type: expected \"--- file type: CFG ---\"{Environment.NewLine}Image = {line}");

                            firstLine = false;
                            continue;
                        }

                        if (line is null || IsFileSectionSeparator(line))
                            break;

                        fileLines.Add(line);
                    }
                    while (true);

                    lines = fileLines.ToArray();
                }
            }
            else
            {
                lines = File.ReadAllLines(fileName);
            }

            int lineNumber = 0;

            // Parse version line
            string[] parts = lines[lineNumber++].Split(',');

            if (parts.Length < 2 || (!useRelaxedValidation && parts.Length != 2 && parts.Length != 3))
                throw new InvalidOperationException($"Unexpected number of line image elements for first configuration file line: {parts.Length} - expected 2 or 3{Environment.NewLine}Image = {lines[lineNumber - 1]}");

            StationName = parts[0].Trim();
            DeviceID = parts[1].Trim();

            if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2]))
                Version = int.Parse(parts[2].Trim());
            else
                Version = 1991;

            // Parse totals line
            parts = lines[lineNumber++].Split(',');

            if (parts.Length < 3 || (!useRelaxedValidation && parts.Length != 3))
                throw new InvalidOperationException($"Unexpected number of line image elements for second configuration file line: {parts.Length} - expected 3{Environment.NewLine}Image = {lines[lineNumber - 1]}");

            int totalChannels = int.Parse(parts[0].Trim());
            int totalAnalogChannels = int.Parse(parts[1].Trim().Split('A')[0]);
            int totalDigitalChannels = int.Parse(parts[2].Trim().Split('D')[0]);

            if (totalChannels != totalAnalogChannels + totalDigitalChannels)
                throw new InvalidOperationException($"Total defined channels must equal the sum of the total number of analog and digital channel definitions.{Environment.NewLine}Image = {lines[lineNumber - 1]}");

            // Cache analog line image definitions - will parse after file type is known
            List<string> analogLineImages = new List<string>();

            for (int i = 0; i < totalAnalogChannels; i++)
                analogLineImages.Add(lines[lineNumber++]);

            // Parse digital definitions
            List<DigitalChannel> digitalChannels = new List<DigitalChannel>();

            for (int i = 0; i < totalDigitalChannels; i++)
                digitalChannels.Add(new DigitalChannel(lines[lineNumber++], Version, useRelaxedValidation));

            DigitalChannels = digitalChannels.ToArray();

            // Parse line frequency
            NominalFrequency = double.Parse(lines[lineNumber++]);

            // Parse total number of sample rates
            int totalSampleRates = int.Parse(lines[lineNumber++]);

            if (totalSampleRates == 0)
                totalSampleRates = 1;

            // Parse each sample rate
            List<SampleRate> sampleRates = new List<SampleRate>();

            for (int i = 0; i < totalSampleRates; i++)
                sampleRates.Add(new SampleRate(lines[lineNumber++], useRelaxedValidation));

            SampleRates = sampleRates.ToArray();

            // Parse timestamps
            StartTime = new Timestamp(lines[lineNumber++]);
            TriggerTime = new Timestamp(lines[lineNumber++]);

            // Parse file type
            Enum.TryParse(lines[lineNumber++], true, out FileType fileType);
            FileType = fileType;

            // Parse analog definitions - we do this after knowing file type to better assign default linear scaling factors
            List<AnalogChannel> analogChannels = new List<AnalogChannel>();
            bool targetFloatingPoint = fileType == FileType.Float32;

            for (int i = 0; i < analogLineImages.Count; i++)
                analogChannels.Add(new AnalogChannel(analogLineImages[i], Version, targetFloatingPoint, useRelaxedValidation));

            AnalogChannels = analogChannels.ToArray();

            // Parse time factor
            TimeFactor = lines.Length < lineNumber ? double.Parse(lines[lineNumber++]) : 1;

            // Parse time information line
            if (lines.Length < lineNumber)
            {
                parts = lines[lineNumber++].Split(',');

                if (parts.Length > 0)
                    TimeCode = new TimeOffset(parts[0].Trim());

                if (parts.Length > 1)
                    LocalCode = new TimeOffset(parts[1].Trim());
            }

            // Parse time state line
            if (lines.Length < lineNumber)
            {
                parts = lines[lineNumber/*++*/].Split(',');

                if (parts.Length > 0)
                    TimeQualityIndicatorCode = (TimeQualityIndicatorCode)byte.Parse(parts[0], NumberStyles.HexNumber);

                if (parts.Length > 1)
                    LeapSecondIndicator = (LeapSecondIndicator)byte.Parse(parts[1]);
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the file name used to generate this schema when constructed with one; otherwise, <c>null</c>.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets flag that determines if <see cref="FileName"/> is a Combined File Format (.cff) file.
        /// </summary>
        public bool IsCombinedFileFormat { get; }

        /// <summary>
        /// Gets or sets free-form station name for this <see cref="Schema"/>.
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// Gets or sets free-form device ID for this <see cref="Schema"/>.
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// Gets or sets version number of the IEEE Std C37.111 used by this <see cref="Schema"/>.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets total number of analog and digital channels of this <see cref="Schema"/>.
        /// </summary>
        public int TotalChannels => TotalAnalogChannels + TotalDigitalChannels;

        /// <summary>
        /// Gets total number of analog channels of this <see cref="Schema"/>.
        /// </summary>
        public int TotalAnalogChannels => AnalogChannels?.Length ?? 0;

        /// <summary>
        /// Gets total number of digital channels of this <see cref="Schema"/>.
        /// </summary>
        public int TotalDigitalChannels => DigitalChannels?.Length ?? 0;

        /// <summary>
        /// Gets or sets analog channels of this <see cref="Schema"/>.
        /// </summary>
        public AnalogChannel[] AnalogChannels { get; set; }

        /// <summary>
        /// Gets or sets digital channels of this <see cref="Schema"/>.
        /// </summary>
        public DigitalChannel[] DigitalChannels { get; set; }

        /// <summary>
        /// Gets or sets nominal frequency of this <see cref="Schema"/>.
        /// </summary>
        public double NominalFrequency
        {
            get => m_nominalFrequency;
            set
            {
                m_nominalFrequency = value;

                // Cascade nominal frequency update to analog channels
                if (!(AnalogChannels is null))
                {
                    foreach (AnalogChannel analogChannel in AnalogChannels)
                        analogChannel.NominalFrequency = m_nominalFrequency;
                }
            }
        }

        /// <summary>
        /// Gets or sets sampling rates of this <see cref="Schema"/>. A file of phasor data will normally be made using a single sampling rate, so this will usually be 1.
        /// </summary>
        public SampleRate[] SampleRates
        {
            get => m_sampleRates;
            set => m_sampleRates = value ?? new[] { new SampleRate { Rate = 0, EndSample = 1 } };
        }

        /// <summary>
        /// Gets total number of sample rates of this <see cref="Schema"/>.
        /// </summary>
        public int TotalSampleRates => m_sampleRates.Length == 1 && m_sampleRates[0].Rate == 0.0D ? 0 : m_sampleRates.Length;

        /// <summary>
        /// Gets total number of samples, i.e., rows per timestamp, as reported by the sample rates.
        /// </summary>
        public long TotalSamples => m_sampleRates.Max(sampleRate => (long)sampleRate.EndSample);

        /// <summary>
        /// Gets total number of channels values, i.e., <c>TotalChannels * TotalSamples</c>.
        /// </summary>
        public long TotalChannelValues => TotalChannels * TotalSamples;

        /// <summary>
        /// Gets or sets start timestamp of this <see cref="Schema"/>.
        /// </summary>
        public Timestamp StartTime { get; set; }

        /// <summary>
        /// Gets or sets trigger timestamp of this <see cref="Schema"/>.
        /// </summary>
        public Timestamp TriggerTime { get; set; }

        /// <summary>
        /// Gets or sets file type of this <see cref="Schema"/>.
        /// </summary>
        public FileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the multiplication factor for the time differential (timestamp) field in this <see cref="Schema"/>.
        /// </summary>
        public double TimeFactor { get; set; }

        /// <summary>
        /// Gets or sets UTC offset for channel timestamps - format HhMM.
        /// </summary>
        public TimeOffset TimeCode { get; set; } = new TimeOffset("0h00");

        /// <summary>
        /// Gets or sets UTC offset for timezone of recording device, or "x" for not applicable - format HhMM.
        /// </summary>
        public TimeOffset LocalCode { get; set; } = new TimeOffset("x");

        /// <summary>
        /// Gets or sets the time quality indicator code for the recorded data set.
        /// </summary>
        public TimeQualityIndicatorCode TimeQualityIndicatorCode { get; set; } = TimeQualityIndicatorCode.Locked;

        /// <summary>
        /// Gets or sets the leap second indicator for the recorded data set.
        /// </summary>
        public LeapSecondIndicator LeapSecondIndicator { get; set; } = LeapSecondIndicator.NoLeapSecondAdjustment;

        /// <summary>
        /// Gets the total digital words for given number of digital values when the file type is binary.
        /// </summary>
        public int DigitalWords => (int)Math.Ceiling(DigitalChannels.Length / 16.0D);

        /// <summary>
        /// Calculates the size of a record, in bytes, when the file type is binary.
        /// </summary>
        public int BinaryRecordLength => 8 + 2 * AnalogChannels.Length + 2 * DigitalWords;

        /// <summary>
        /// Calculates the size of a record, in bytes, when the file type is binary32.
        /// </summary>
        public int Binary32RecordLength => 8 + 4 * AnalogChannels.Length + 2 * DigitalWords;

        /// <summary>
        /// Calculates the size of a record, in bytes, when the file type is float32.
        /// </summary>
        public int Float32RecordLength => 8 + 4 * AnalogChannels.Length + 2 * DigitalWords;

        /// <summary>
        /// Gets the file image of this <see cref="Schema"/>.
        /// </summary>
        [JsonIgnore]
        public string FileImage
        {
            get
            {
                StringBuilder fileImage = new StringBuilder();

                void appendLine(string line)
                {
                    // The standard .NET "Environment.NewLine" constant can just be a line feed on some operating systems,
                    // but the COMTRADE standard requires that end of line markers be both a carriage return and line feed.
                    fileImage.Append(line);
                    fileImage.Append(Writer.CRLF);
                }

                // Write version line
                appendLine($"{StationName},{DeviceID}{(Version >= 1999 ? $",{Version}" : "")}");

                // Write totals line
                appendLine($"{TotalChannels},{TotalAnalogChannels}A,{TotalDigitalChannels}D");

                // Write analog definitions
                for (int i = 0; i < TotalAnalogChannels; i++)
                    appendLine(AnalogChannels[i].ToString());

                // Write digital definitions
                for (int i = 0; i < TotalDigitalChannels; i++)
                    appendLine(DigitalChannels[i].ToString());

                // Write line frequency
                appendLine(NominalFrequency.ToString(CultureInfo.InvariantCulture));

                // Write total number of sample rates (zero signifies no fixed sample rates)
                appendLine(TotalSampleRates.ToString());

                int totalSampleRates = TotalSampleRates;

                if (totalSampleRates == 0)
                    totalSampleRates = 1;

                // Write sample rates
                for (int i = 0; i < totalSampleRates; i++)
                    appendLine(SampleRates[i].ToString());

                // Write timestamps
                appendLine(StartTime.ToString());
                appendLine(TriggerTime.ToString());

                // Write file type
                appendLine(FileType.ToString().ToUpper());

                // Write time factor
                if (Version >= 1999)
                    appendLine(TimeFactor.ToString(CultureInfo.InvariantCulture));

                // Write per data set time info and state
                if (Version >= 2013)
                {
                    appendLine($"{TimeCode},{LocalCode}");
                    appendLine($"{(byte)TimeQualityIndicatorCode:X},{(byte)LeapSecondIndicator}");
                }

                return fileImage.ToString();
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods
        internal static bool HasCFFExtension(string fileName) =>
            !(fileName is null) && string.Equals(FilePath.GetExtension(fileName), ".cff", StringComparison.OrdinalIgnoreCase);

        internal static bool IsFileSectionSeparator(string line) =>
            IsFileSectionSeparator(line, out _, out _);

        internal static bool IsFileSectionSeparator(string line, out string sectionType) =>
            IsFileSectionSeparator(line, out sectionType, out _);

        internal static bool IsFileSectionSeparator(string line, out string sectionType, out long byteCount)
        {
            if (line?.Trim().StartsWith("---") ?? false)
            {
                string[] parts = line.Replace("---", "").Trim().Split(':');

                if (parts.Length >= 2 && string.Equals(parts[0].Trim(), "file type", StringComparison.OrdinalIgnoreCase))
                {
                    sectionType = parts[1].Trim().ToUpperInvariant();

                    if (parts.Length > 2 && sectionType == "DAT BINARY")
                        long.TryParse(parts[2].Trim(), out byteCount);
                    else
                        byteCount = default;

                    return true;
                }
            }

            sectionType = default;
            byteCount = default;
            return false;
        }

        #endregion
    }
}
