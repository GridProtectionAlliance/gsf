//******************************************************************************************************
//  Writer.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/19/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GSF.Units.EE;

namespace GSF.COMTRADE
{
    /// <summary>
    /// COMTRADE data and config file writer.
    /// </summary>
    public static class Writer
    {
        /// <summary>
        /// Defines the maximum file size for this COMTRADE implementation.
        /// </summary>
        /// <remarks>
        /// Value represents 256TB, arbitrarily chosen extreme maximum file size.
        /// </remarks>
        public const long MaxFileSize = 281474976710656L;

        private static readonly string MaxByteCountString = new string('0', $"{MaxFileSize}".Length);

        /// <summary>
        /// Defines the maximum COMTRADE end sample number.
        /// </summary>
        public const long MaxEndSample = 9999999999L;

        /// <summary>
        /// Defines a carriage return and line feed constant, i.e., <c>"\r\n"</c>.
        /// </summary>
        /// <remarks>
        /// The standard .NET <see cref="Environment.NewLine"/> constant can just be a line feed, <c>"\n"</c>,
        /// on some operating systems, e.g., Linux, however the COMTRADE standard requires that end of line
        /// markers be both a carriage return and line feed, <c>"\r\n"</c>.
        /// </remarks>
        public const string CRLF = "\r\n";

        /// <summary>
        /// Creates a new COMTRADE configuration <see cref="Schema"/>.
        /// </summary>
        /// <param name="metadata">Schema <see cref="ChannelMetadata"/> records.</param>
        /// <param name="stationName">Station name for the schema.</param>
        /// <param name="deviceID">Device ID for the schema.</param>
        /// <param name="dataStartTime">Data start time.</param>
        /// <param name="sampleCount">Total data samples (i.e., total number of rows).</param>
        /// <param name="version">Target schema version - defaults to 1999.</param>
        /// <param name="fileType">Determines the data file type for the schema.</param>
        /// <param name="timeFactor">Time factor to use in schema - defaults to 1000.</param>
        /// <param name="samplingRate">Desired sampling rate - defaults to 33.3333Hz.</param>
        /// <param name="nominalFrequency">Nominal frequency - defaults to 60Hz.</param>
        /// <param name="includeFracSecDefinition">Determines if the FRACSEC word digital definitions should be included - defaults to <c>true</c>.</param>
        /// <returns>New COMTRADE configuration <see cref="Schema"/>.</returns>
        /// <remarks>
        /// <para>
        /// This function is primarily intended to create a configuration based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually create a schema object for other COMTRADE needs. You can call
        /// the <see cref="Schema.FileImage"/> property to return a string that can be written to a file
        /// that will be the contents of the configuration file.
        /// </para>
        /// <para>
        /// Linear scaling factors for analog channels, i.e., adders and multipliers, will be set to reasonable
        /// values based on the channel type. These should be adjusted as needed based on actual channel value
        /// ranges. Note that for <see cref="FileType.Float32"/> the multiplier will be <c>1.0</c> and the adder
        /// will be <c>0.0</c> for all analog values.
        /// </para>
        /// </remarks>
        public static Schema CreateSchema(IEnumerable<ChannelMetadata> metadata, string stationName, string deviceID, Ticks dataStartTime, long sampleCount, int version = 1999, FileType fileType = FileType.Binary, double timeFactor = 1.0D, double samplingRate = 30.0D, double nominalFrequency = 60.0D, bool includeFracSecDefinition = true)
        {
            Schema schema = new Schema
            {
                StationName = stationName,
                DeviceID = deviceID,
                Version = version
            };

            SampleRate samplingFrequency = new SampleRate
            {
                Rate = samplingRate,
                EndSample = sampleCount
            };

            schema.SampleRates = new[] { samplingFrequency };

            Timestamp startTime;
            startTime.Value = dataStartTime;
            schema.StartTime = startTime;
            schema.TriggerTime = startTime;

            schema.FileType = fileType;
            schema.TimeFactor = timeFactor;

            List<AnalogChannel> analogChannels = new List<AnalogChannel>();
            List<DigitalChannel> digitalChannels = new List<DigitalChannel>();
            bool targetFloatingPoint = fileType == FileType.Ascii || fileType == FileType.Float32;

            int analogIndex = 1;
            int digitalIndex = 1;

            if (includeFracSecDefinition)
            {
                // Add default time quality digitals for IEEE C37.118 FRACSEC word. Note that these flags, as
                // defined in Annex H of the IEEE C37.111-2010 standard, assume full export was all from one
                // source device. This a poor assumption since data can be exported from historical data for any
                // number of points which could have come from any number of devices all with different FRACSEC
                // values. Regardless there is only one FRACSEC definition defined and, if included, it must
                // come as the first set of digitals in the COMTRADE configuration.
                for (int i = 0; i < 4; i++)
                {
                    digitalChannels.Add(new DigitalChannel(schema.Version)
                    {
                        Index = digitalIndex,
                        Name = "TQ_CNT" + i,
                        PhaseID = "T" + digitalIndex++
                    });
                }

                digitalChannels.Add(new DigitalChannel(schema.Version)
                {
                    Index = digitalIndex,
                    Name = "TQ_LSPND",
                    PhaseID = "T" + digitalIndex++
                });


                digitalChannels.Add(new DigitalChannel(schema.Version)
                {
                    Index = digitalIndex,
                    Name = "TQ_LSOCC",
                    PhaseID = "T" + digitalIndex++
                });

                digitalChannels.Add(new DigitalChannel(schema.Version)
                {
                    Index = digitalIndex,
                    Name = "TQ_LSDIR",
                    PhaseID = "T" + digitalIndex++
                });

                digitalChannels.Add(new DigitalChannel(schema.Version)
                {
                    Index = digitalIndex,
                    Name = "RSV",
                    PhaseID = "T" + digitalIndex++
                });

                for (int i = 1; i < 9; i++)
                {
                    digitalChannels.Add(new DigitalChannel(schema.Version)
                    {
                        Index = digitalIndex,
                        Name = "RESV" + i,
                        PhaseID = "T" + digitalIndex++
                    });
                }
            }

            // Add meta data for selected points sorted by analogs followed by status flags then digitals
            foreach (ChannelMetadata record in metadata.OrderBy(m => m, ChannelMetadataSorter.Default))
            {
                if (record.IsDigital)
                {
                    switch (record.SignalType)
                    {
                        case SignalType.FLAG: // Status flags
                            // Add synchrophasor status flag specific digitals
                            int statusIndex = 0;

                            for (int i = 1; i < 5; i++)
                            {
                                digitalChannels.Add(new DigitalChannel(schema.Version)
                                {
                                    Index = digitalIndex++,
                                    Name = record.Name + ":TRG" + i,
                                    PhaseID = "S" + statusIndex++.ToString("X")
                                });
                            }

                            for (int i = 1; i < 3; i++)
                            {
                                digitalChannels.Add(new DigitalChannel(schema.Version)
                                {
                                    Index = digitalIndex++,
                                    Name = record.Name + ":UNLK" + i,
                                    PhaseID = "S" + statusIndex++.ToString("X")
                                });
                            }

                            for (int i = 1; i < 5; i++)
                            {
                                digitalChannels.Add(new DigitalChannel(schema.Version)
                                {
                                    Index = digitalIndex++,
                                    Name = record.Name + ":SEC" + i,
                                    PhaseID = "S" + statusIndex++.ToString("X")
                                });
                            }

                            digitalChannels.Add(new DigitalChannel(schema.Version)
                            {
                                Index = digitalIndex++,
                                Name = record.Name + ":CFGCH",
                                PhaseID = "S" + statusIndex++.ToString("X")
                            });

                            digitalChannels.Add(new DigitalChannel(schema.Version)
                            {
                                Index = digitalIndex++,
                                Name = record.Name + ":PMUTR",
                                PhaseID = "S" + statusIndex++.ToString("X")
                            });

                            digitalChannels.Add(new DigitalChannel(schema.Version)
                            {
                                Index = digitalIndex++,
                                Name = record.Name + ":SORT",
                                PhaseID = "S" + statusIndex++.ToString("X")
                            });

                            digitalChannels.Add(new DigitalChannel(schema.Version)
                            {
                                Index = digitalIndex++,
                                Name = record.Name + ":SYNC",
                                PhaseID = "S" + statusIndex++.ToString("X")
                            });

                            digitalChannels.Add(new DigitalChannel(schema.Version)
                            {
                                Index = digitalIndex++,
                                Name = record.Name + ":PMUERR",
                                PhaseID = "S" + statusIndex++.ToString("X")
                            });

                            digitalChannels.Add(new DigitalChannel(schema.Version)
                            {
                                Index = digitalIndex++,
                                Name = record.Name + ":DTVLD",
                                PhaseID = "S" + statusIndex.ToString("X")
                            });
                            break;
                        default:
                            // Every synchrophasor digital is 16-bits
                            for (int i = 0; i < 16; i++)
                            {
                                digitalChannels.Add(new DigitalChannel(schema.Version)
                                {
                                    Index = digitalIndex++,
                                    Name = record.Name,
                                    PhaseID = "B" + i.ToString("X"),
                                    CircuitComponent = record.CircuitComponent
                                });
                            }
                            break;
                    }
                }
                else
                {
                    switch (record.SignalType)
                    {
                        case SignalType.IPHM: // Current Magnitude
                            analogChannels.Add(new AnalogChannel(schema.Version, targetFloatingPoint)
                            {
                                Index = analogIndex++,
                                Name = record.Name,
                                Units = record.Units ?? "A",
                                PhaseID = "Pm",
                                CircuitComponent = record.CircuitComponent,
                                Multiplier = targetFloatingPoint ? 1.0D : AnalogChannel.DefaultCurrentMagnitudeMultiplier
                            });
                            break;
                        case SignalType.VPHM: // Voltage Magnitude
                            analogChannels.Add(new AnalogChannel(schema.Version, targetFloatingPoint)
                            {
                                Index = analogIndex++,
                                Name = record.Name,
                                Units = record.Units ?? "V",
                                PhaseID = "Pm",
                                CircuitComponent = record.CircuitComponent,
                                Multiplier = targetFloatingPoint ? 1.0D : AnalogChannel.DefaultVoltageMagnitudeMultiplier
                            });
                            break;
                        case SignalType.IPHA: // Current Phase Angle
                        case SignalType.VPHA: // Voltage Phase Angle
                            analogChannels.Add(new AnalogChannel(schema.Version, targetFloatingPoint)
                            {
                                Index = analogIndex++,
                                Name = record.Name,
                                Units = record.Units ?? "Rads",
                                PhaseID = "Pa",
                                CircuitComponent = record.CircuitComponent,
                                Multiplier = targetFloatingPoint ? 1.0D : AnalogChannel.DefaultPhaseAngleMultiplier
                            });
                            break;
                        case SignalType.FREQ: // Frequency
                            analogChannels.Add(new AnalogChannel(schema.Version, targetFloatingPoint)
                            {
                                Index = analogIndex++,
                                Name = record.Name,
                                Units = record.Units ?? "Hz",
                                PhaseID = "F",
                                CircuitComponent = record.CircuitComponent,
                                Adder = targetFloatingPoint ? 0.0D : nominalFrequency,
                                Multiplier = targetFloatingPoint ? 1.0D : AnalogChannel.DefaultFrequencyMultiplier
                            });
                            break;
                        case SignalType.DFDT: // Frequency Delta (dF/dt)
                            analogChannels.Add(new AnalogChannel(schema.Version, targetFloatingPoint)
                            {
                                Index = analogIndex++,
                                Name = record.Name,
                                Units = record.Units ?? "Hz/s",
                                PhaseID = "dF",
                                CircuitComponent = record.CircuitComponent,
                                Multiplier = targetFloatingPoint ? 1.0D : AnalogChannel.DefaultDfDtMultiplier
                            });
                            break;
                        default:     // All other signals assumed to be analog values
                            analogChannels.Add(new AnalogChannel(schema.Version, targetFloatingPoint)
                            {
                                Index = analogIndex++,
                                Name = record.Name,
                                PhaseID = "",
                                Units = record.Units,
                                CircuitComponent = record.CircuitComponent,
                                Multiplier = targetFloatingPoint ? 1.0D : AnalogChannel.DefaultAnalogMultipler
                            });
                            break;
                    }
                }
            }

            schema.AnalogChannels = analogChannels.ToArray();
            schema.DigitalChannels = digitalChannels.ToArray();
            schema.NominalFrequency = nominalFrequency;

            return schema;
        }

        /// <summary>
        /// Creates a new Combined File Format (.cff) COMTRADE file stream.
        /// </summary>
        /// <param name="fileName">Target file name. Must have ".cff" extension.</param>
        /// <param name="schema">Schema of file stream.</param>
        /// <param name="infLines">Lines of "INF" section to write to stream, if any.</param>
        /// <param name="hdrLines">Lines of "HDR" section to write to stream, if any.</param>
        /// <param name="encoding">Target encoding; <c>null</c> value will default to UTF-8 (no BOM).</param>
        /// <returns>New file stream for Combined File Format (.cff) COMTRADE file, ready to write at data section.</returns>
        public static FileStream CreateCFFStream(string fileName, Schema schema, string[] infLines = null, string[] hdrLines = null, Encoding encoding = null) => 
            CreateCFFStream(fileName, schema, infLines, hdrLines, encoding, out _);

        /// <summary>
        /// Creates a new Combined File Format (.cff) COMTRADE file stream targeted for ASCII.
        /// </summary>
        /// <param name="fileName">Target file name. Must have ".cff" extension.</param>
        /// <param name="schema">Schema of file stream.</param>
        /// <param name="infLines">Lines of "INF" section to write to stream, if any.</param>
        /// <param name="hdrLines">Lines of "HDR" section to write to stream, if any.</param>
        /// <param name="encoding">Target encoding; <c>null</c> value will default to UTF-8 (no BOM).</param>
        /// <returns>New stream writer for Combined File Format (.cff) COMTRADE file, ready to write at data section.</returns>
        /// <remarks>
        /// For COMTRADE versions greater than 2001, any use of the term ASCII also inherently implies Unicode UTF-8.
        /// When then <paramref name="encoding"/> parameter is <c>null</c>, the default, UTF-8 encoding will be used
        /// for text writes. If ASCII encoding needs to be enforced for backwards compatibility reasons, then the
        /// <paramref name="encoding"/> parameter will need to be set to <see cref="Encoding.ASCII"/>.
        /// </remarks>
        public static StreamWriter CreateCFFStreamAscii(string fileName, Schema schema, string[] infLines = null, string[] hdrLines = null, Encoding encoding = null)
        {
            if (schema.FileType != FileType.Ascii)
                throw new ArgumentException($"Cannot create ASCII CFF stream using schema targeted for {schema.FileType.ToString().ToUpperInvariant()}", nameof(schema));

            CreateCFFStream(fileName, schema, infLines, hdrLines, encoding, out StreamWriter writer);
            return writer;
        }

        private static FileStream CreateCFFStream(string fileName, Schema schema, string[] infLines, string[] hdrLines, Encoding encoding, out StreamWriter writer)
        {
            if (fileName is null)
                throw new ArgumentNullException(nameof(fileName));

            if (!Schema.HasCFFExtension(fileName))
                throw new ArgumentException("Specified file name is not using standard Combined File Format COMTRADE file extension: \".cff\".", nameof(fileName));

            FileStream stream = File.Create(fileName);

            CreateCFFStream(stream, schema, infLines, hdrLines, encoding, out writer);

            return stream;
        }

        /// <summary>
        /// Creates a new Combined File Format (.cff) COMTRADE file stream.
        /// </summary>
        /// <param name="stream">Target stream.</param>
        /// <param name="schema">Schema of file stream.</param>
        /// <param name="infLines">Lines of "INF" section to write to stream, if any.</param>
        /// <param name="hdrLines">Lines of "HDR" section to write to stream, if any.</param>
        /// <param name="encoding">Target encoding; <c>null</c> value will default to UTF-8 (no BOM).</param>
        public static void CreateCFFStream(Stream stream, Schema schema, string[] infLines = null, string[] hdrLines = null, Encoding encoding = null) =>
            CreateCFFStream(stream, schema, infLines, hdrLines, encoding, out _);

        /// <summary>
        /// Creates a new Combined File Format (.cff) COMTRADE file stream targeted for ASCII.
        /// </summary>
        /// <param name="stream">Target stream.</param>
        /// <param name="schema">Schema of file stream.</param>
        /// <param name="infLines">Lines of "INF" section to write to stream, if any.</param>
        /// <param name="hdrLines">Lines of "HDR" section to write to stream, if any.</param>
        /// <param name="encoding">Target encoding; <c>null</c> value will default to UTF-8 (no BOM).</param>
        /// <returns>New stream writer for Combined File Format (.cff) COMTRADE file, ready to write at data section.</returns>
        /// <remarks>
        /// For COMTRADE versions greater than 2001, any use of the term ASCII also inherently implies Unicode UTF-8.
        /// When then <paramref name="encoding"/> parameter is <c>null</c>, the default, UTF-8 encoding will be used
        /// for text writes. If ASCII encoding needs to be enforced for backwards compatibility reasons, then the
        /// <paramref name="encoding"/> parameter will need to be set to <see cref="Encoding.ASCII"/>.
        /// </remarks>
        public static StreamWriter CreateCFFStreamAscii(Stream stream, Schema schema, string[] infLines = null, string[] hdrLines = null, Encoding encoding = null)
        {
            if (schema.FileType != FileType.Ascii)
                throw new ArgumentException($"Cannot create ASCII CFF stream using schema targeted for {schema.FileType.ToString().ToUpperInvariant()}", nameof(schema));

            CreateCFFStream(stream, schema, infLines, hdrLines, encoding, out StreamWriter writer);
            return writer;
        }

        private static void CreateCFFStream(Stream stream, Schema schema, string[] infLines, string[] hdrLines, Encoding encoding, out StreamWriter writer)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            if (schema is null)
                throw new ArgumentNullException(nameof(schema));

            if (schema.Version < 2013)
                throw new ArgumentException("Minimum COMTRADE version for a Combined File Format (.cff) file is 2013", nameof(schema));

            writer = new StreamWriter(stream, encoding ?? new UTF8Encoding(false)) { NewLine = CRLF };

            writer.WriteLine("--- file type: CFG ---");
            writer.WriteLine(schema.FileImage);

            writer.WriteLine("--- file type: INF ---");
            writer.WriteLine(string.Join(CRLF, infLines ?? Array.Empty<string>()));

            writer.WriteLine("--- file type: HDR ---");
            writer.WriteLine(string.Join(CRLF, hdrLines ?? Array.Empty<string>()));

            // Reserve space for binary byte count
            writer.WriteLine($"--- file type: DAT {(schema.FileType == FileType.Ascii ? "ASCII" : $"BINARY: {MaxByteCountString}")} ---");

            // Do not dispose writer as this will dispose base stream
            writer.Flush();
        }

        /// <summary>
        /// Updates a Combined File Format (.cff) COMTRADE file stream with a final end sample number.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <param name="endSample">End sample value.</param>
        /// <param name="rateIndex">Zero-based rate index to update.</param>
        /// <param name="encoding">Target encoding; <c>null</c> value will default to UTF-8 (no BOM).</param>
        public static void UpdateCFFEndSample(Stream stream, long endSample, int rateIndex = 0, Encoding encoding = null)
        {
            if (endSample > MaxEndSample)
                throw new ArgumentOutOfRangeException(nameof(endSample), $"Max end sample for COMTRADE is {MaxEndSample:N0}");

            if (rateIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(rateIndex), "Rate index cannot be a negative value");

            stream.Position = 0;

            // Do not dispose stream reader - this would dispose base stream
            StreamReader fileReader = new StreamReader(stream);
            Encoding utf8 = new UTF8Encoding(false);
            string lastLine = null;
            long position = 0;

            string readLine()
            {
                if (lastLine?.Length > 0)
                    position += utf8.GetBytes(lastLine).Length + 2;

                string nextLine = fileReader.ReadLine();

                if (nextLine is null)
                    throw new InvalidOperationException($"Unexpected end of configuration section");

                return lastLine = nextLine;
            }

            // Read configuration section separator
            string line = readLine();

            if (!Schema.IsFileSectionSeparator(line, out string sectionType) || sectionType != "CFG")
                throw new InvalidOperationException($"Unexpected file section separator for configuration file type: expected \"--- file type: CFG ---\"{Environment.NewLine}Image = {line}");

            // Skip Version line
            readLine();

            // Parse totals line
            string[] parts = readLine().Split(',');

            if (parts.Length < 3)
                throw new InvalidOperationException($"Unexpected number of line image elements for second configuration file line: {parts.Length} - expected 3{Environment.NewLine}Image = {line}");

            int totalAnalogChannels = int.Parse(parts[1].Trim().Split('A')[0]);
            int totalDigitalChannels = int.Parse(parts[2].Trim().Split('D')[0]);

            // Skip analog definitions
            for (int i = 0; i < totalAnalogChannels; i++)
                readLine();

            // Skip digital definitions
            for (int i = 0; i < totalDigitalChannels; i++)
                readLine();

            // Skip line frequency
            readLine();

            // Parse total number of sample rates
            int totalSampleRates = int.Parse(readLine());

            if (totalSampleRates == 0)
                totalSampleRates = 1;

            if (rateIndex > totalSampleRates - 1)
                throw new ArgumentOutOfRangeException(nameof(rateIndex), $"Rate index {rateIndex:N0} exceeds available sampling rates: {totalSampleRates:N0}");

            // Skip to target rate index
            for (int i = 0; i < rateIndex; i++)
                readLine();

            // Parse target sample rate
            line = readLine();
            SampleRate sampleRate = new SampleRate(line) { EndSample = endSample };

            // Write updated sample rate
            byte[] bytes = (encoding ?? utf8).GetBytes(sampleRate.ToString().PadRight(line.Length));
            stream.Position = position;
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Updates a Combined File Format (.cff) COMTRADE file stream with a final binary byte count.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <param name="byteCount">Binary byte count.</param>
        /// <param name="encoding">Target encoding; <c>null</c> value will default to UTF-8 (no BOM).</param>
        public static void UpdateCFFStreamBinaryByteCount(Stream stream, long byteCount, Encoding encoding = null)
        {
            if (byteCount > MaxFileSize)
                throw new ArgumentOutOfRangeException(nameof(byteCount), $"Max byte count currently set to 256TB ({MaxFileSize:N0} bytes)");

            stream.Position = 0;

            // Scan ahead to data section (do not dispose stream reader - this would dispose base stream)
            StreamReader fileReader = new StreamReader(stream);
            Encoding utf8 = new UTF8Encoding(false);
            long position = 0;

            do
            {
                string line = fileReader.ReadLine();

                if (line is null)
                    break;

                if (Schema.IsFileSectionSeparator(line, out string sectionType, out _) && sectionType == "DAT BINARY")
                {
                    byte[] bytes = (encoding ?? utf8).GetBytes($"{byteCount} ---".PadRight($"{MaxByteCountString} ---".Length));
                    stream.Position = position + "--- file type: DAT BINARY: ".Length;
                    stream.Write(bytes, 0, bytes.Length);
                    break;
                }

                position += utf8.GetBytes(line).Length + 2;
            }
            while (true);
        }

        /// <summary>
        /// Writes next COMTRADE record in ASCII format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="values">Values to write - 16-bit digitals should exist as a word in an individual double value, method will write out bits.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>true</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE ASCII data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordAscii(StreamWriter output, Schema schema, Ticks timestamp, double[] values, uint sample, bool injectFracSecValue = true, ushort fracSecValue = 0x0000)
        {
            if (schema.FileType != FileType.Ascii)
                throw new ArgumentException($"Cannot write ASCII record to schema targeted for {schema.FileType.ToString().ToUpperInvariant()}", nameof(schema));

            // Make timestamp relative to beginning of file
            timestamp -= schema.StartTime.Value;

            uint microseconds = (uint)(timestamp.ToMicroseconds() / schema.TimeFactor);
            StringBuilder line = new StringBuilder();
            bool isFirstDigital = true;

            line.Append(sample);
            line.Append(',');
            line.Append(microseconds);

            for (int i = 0; i < values.Length; i++)
            {
                double value = values[i];

                if (i < schema.AnalogChannels.Length)
                {
                    value -= schema.AnalogChannels[i].Adder;
                    value /= schema.AnalogChannels[i].Multiplier;

                    line.Append(',');
                    line.Append(value.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    if (isFirstDigital)
                    {
                        // Handle automatic injection of IEEE C37.118 FRACSEC digital value if requested
                        isFirstDigital = false;

                        if (injectFracSecValue)
                        {
                            for (int j = 0; j < 16; j++)
                            {
                                line.Append(',');
                                line.Append(fracSecValue.CheckBits(BitExtensions.BitVal(j)) ? 1 : 0);
                            }
                        }
                    }

                    ushort digitalWord = (ushort)value;

                    for (int j = 0; j < 16; j++)
                    {
                        line.Append(',');
                        line.Append(digitalWord.CheckBits(BitExtensions.BitVal(j)) ? 1 : 0);
                    }
                }
            }

            // Make sure FRACSEC values are injected
            if (isFirstDigital && injectFracSecValue)
            {
                for (int j = 0; j < 16; j++)
                {
                    line.Append(',');
                    line.Append(fracSecValue.CheckBits(BitExtensions.BitVal(j)) ? 1 : 0);
                }
            }

            output.WriteLine(line.ToString());
        }

        /// <summary>
        /// Writes next COMTRADE record in binary format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="values">Values to write - 16-bit digitals should exist as a word in an individual double value.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>true</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE binary data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordBinary(Stream output, Schema schema, Ticks timestamp, double[] values, uint sample, bool injectFracSecValue = true, ushort fracSecValue = 0x0000)
        {
            if (schema.FileType != FileType.Binary)
                throw new ArgumentException($"Cannot write BINARY record to schema targeted for {schema.FileType.ToString().ToUpperInvariant()}", nameof(schema));

            // Make timestamp relative to beginning of file
            timestamp -= schema.StartTime.Value;

            uint microseconds = (uint)(timestamp.ToMicroseconds() / schema.TimeFactor);
            bool isFirstDigital = true;

            output.Write(LittleEndian.GetBytes(sample), 0, 4);
            output.Write(LittleEndian.GetBytes(microseconds), 0, 4);

            for (int i = 0; i < values.Length; i++)
            {
                double value = values[i];

                if (i < schema.AnalogChannels.Length)
                {
                    value -= schema.AnalogChannels[i].Adder;
                    value /= schema.AnalogChannels[i].Multiplier;
                }
                else if (isFirstDigital)
                {
                    // Handle automatic injection of IEEE C37.118 FRACSEC digital value if requested
                    isFirstDigital = false;

                    if (injectFracSecValue)
                        output.Write(LittleEndian.GetBytes(fracSecValue), 0, 2);
                }

                output.Write(LittleEndian.GetBytes((ushort)value), 0, 2); // 16-bit binary integer values
            }

            // Make sure FRACSEC values are injected
            if (isFirstDigital && injectFracSecValue)
                output.Write(LittleEndian.GetBytes(fracSecValue), 0, 2);
        }

        /// <summary>
        /// Writes next COMTRADE record in binary32 format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="values">Values to write - 16-bit digitals should exist as a word in an individual double value.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>true</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE binary32 data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordBinary32(Stream output, Schema schema, Ticks timestamp, double[] values, uint sample, bool injectFracSecValue = true, ushort fracSecValue = 0x0000)
        {
            if (schema.FileType != FileType.Binary32)
                throw new ArgumentException($"Cannot write BINARY32 record to schema targeted for {schema.FileType.ToString().ToUpperInvariant()}", nameof(schema));

            // Make timestamp relative to beginning of file
            timestamp -= schema.StartTime.Value;

            uint microseconds = (uint)(timestamp.ToMicroseconds() / schema.TimeFactor);
            bool isFirstDigital = true;

            output.Write(LittleEndian.GetBytes(sample), 0, 4);
            output.Write(LittleEndian.GetBytes(microseconds), 0, 4);

            for (int i = 0; i < values.Length; i++)
            {
                double value = values[i];

                if (i < schema.AnalogChannels.Length)
                {
                    value -= schema.AnalogChannels[i].Adder;
                    value /= schema.AnalogChannels[i].Multiplier;

                    output.Write(LittleEndian.GetBytes((uint)value), 0, 4); // 32-bit binary integer values
                }
                else
                {
                    if (isFirstDigital)
                    {
                        // Handle automatic injection of IEEE C37.118 FRACSEC digital value if requested
                        isFirstDigital = false;

                        if (injectFracSecValue)
                            output.Write(LittleEndian.GetBytes(fracSecValue), 0, 2);
                    }

                    output.Write(LittleEndian.GetBytes((ushort)value), 0, 2);
                }
            }

            // Make sure FRACSEC values are injected
            if (isFirstDigital && injectFracSecValue)
                output.Write(LittleEndian.GetBytes(fracSecValue), 0, 2);
        }

        /// <summary>
        /// Writes next COMTRADE record in float32 format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="values">Values to write - 16-bit digitals should exist as a word in an individual double value.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>true</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE float32 data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordFloat32(Stream output, Schema schema, Ticks timestamp, double[] values, uint sample, bool injectFracSecValue = true, ushort fracSecValue = 0x0000)
        {
            if (schema.FileType != FileType.Float32)
                throw new ArgumentException($"Cannot write FLOAT32 record to schema targeted for {schema.FileType.ToString().ToUpperInvariant()}", nameof(schema));

            // Make timestamp relative to beginning of file
            timestamp -= schema.StartTime.Value;

            uint microseconds = (uint)(timestamp.ToMicroseconds() / schema.TimeFactor);
            bool isFirstDigital = true;

            output.Write(LittleEndian.GetBytes(sample), 0, 4);
            output.Write(LittleEndian.GetBytes(microseconds), 0, 4);

            for (int i = 0; i < values.Length; i++)
            {
                double value = values[i];

                if (i < schema.AnalogChannels.Length)
                {
                    value -= schema.AnalogChannels[i].Adder;
                    value /= schema.AnalogChannels[i].Multiplier;

                    output.Write(LittleEndian.GetBytes((float)value), 0, 4); // 32-bit binary float values
                }
                else
                {
                    if (isFirstDigital)
                    {
                        // Handle automatic injection of IEEE C37.118 FRACSEC digital value if requested
                        isFirstDigital = false;

                        if (injectFracSecValue)
                            output.Write(LittleEndian.GetBytes(fracSecValue), 0, 2);
                    }

                    output.Write(LittleEndian.GetBytes((ushort)value), 0, 2);
                }
            }

            // Make sure FRACSEC values are injected
            if (isFirstDigital && injectFracSecValue)
                output.Write(LittleEndian.GetBytes(fracSecValue), 0, 2);
        }

        /// <summary>
        /// Writes next COMTRADE record in ASCII format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="analogValues">Values to write for analog channels.</param>
        /// <param name="digitalValues">Values to write for digital channels.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>false</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE ASCII data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordAscii(StreamWriter output, Schema schema, Ticks timestamp, double[] analogValues, bool[] digitalValues, uint sample, bool injectFracSecValue = false, ushort fracSecValue = 0x0000)
        {
            double[] values = analogValues
                .Concat(digitalValues.Select(b => b ? 1.0D : 0.0D))
                .ToArray();

            WriteNextRecordAscii(output, schema, timestamp, values, sample, injectFracSecValue, fracSecValue);
        }

        /// <summary>
        /// Writes next COMTRADE record in binary format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="analogValues">Values to write for analog channels.</param>
        /// <param name="digitalValues">Values to write for digital channels.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>false</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE binary data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordBinary(Stream output, Schema schema, Ticks timestamp, double[] analogValues, bool[] digitalValues, uint sample, bool injectFracSecValue = false, ushort fracSecValue = 0x0000)
        {
            double[] digitalWords = GroupByWord(digitalValues);
            double[] values = analogValues.Concat(digitalWords).ToArray();
            WriteNextRecordBinary(output, schema, timestamp, values, sample, injectFracSecValue, fracSecValue);
        }

        /// <summary>
        /// Writes next COMTRADE record in binary32 format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="analogValues">Values to write for analog channels.</param>
        /// <param name="digitalValues">Values to write for digital channels.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>false</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE binary32 data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordBinary32(Stream output, Schema schema, Ticks timestamp, double[] analogValues, bool[] digitalValues, uint sample, bool injectFracSecValue = false, ushort fracSecValue = 0x0000)
        {
            double[] digitalWords = GroupByWord(digitalValues);
            double[] values = analogValues.Concat(digitalWords).ToArray();
            WriteNextRecordBinary32(output, schema, timestamp, values, sample, injectFracSecValue, fracSecValue);
        }

        /// <summary>
        /// Writes next COMTRADE record in float32 format.
        /// </summary>
        /// <param name="output">Destination stream.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="timestamp">Record timestamp (implicitly castable as <see cref="DateTime"/>).</param>
        /// <param name="analogValues">Values to write for analog channels.</param>
        /// <param name="digitalValues">Values to write for digital channels.</param>
        /// <param name="sample">User incremented sample index.</param>
        /// <param name="injectFracSecValue">Determines if FRACSEC value should be automatically injected into stream as first digital - defaults to <c>false</c>.</param>
        /// <param name="fracSecValue">FRACSEC value to inject into output stream - defaults to 0x0000.</param>
        /// <remarks>
        /// This function is primarily intended to write COMTRADE float32 data records based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually write records for other COMTRADE needs (e.g., non 16-bit digitals).
        /// </remarks>
        public static void WriteNextRecordFloat32(Stream output, Schema schema, Ticks timestamp, double[] analogValues, bool[] digitalValues, uint sample, bool injectFracSecValue = false, ushort fracSecValue = 0x0000)
        {
            double[] digitalWords = GroupByWord(digitalValues);
            double[] values = analogValues.Concat(digitalWords).ToArray();
            WriteNextRecordFloat32(output, schema, timestamp, values, sample, injectFracSecValue, fracSecValue);
        }

        private static double[] GroupByWord(bool[] digitalValues)
        {
            int index = 0;

            return digitalValues
                .Select(b => b ? 1 : 0)
                .GroupBy(word => (index++) / 16)
                .Select(grouping => grouping.Select((bit, i) => bit << i))
                .Select(grouping => grouping.Aggregate((ushort)0, (word, bit) => (ushort)(word | bit)))
                .Select(word => (double)word)
                .ToArray();
        }
    }
}
