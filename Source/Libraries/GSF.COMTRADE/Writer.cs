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
        public static Schema CreateSchema(IEnumerable<ChannelMetadata> metadata, string stationName, string deviceID, Ticks dataStartTime, int sampleCount, int version = 1999, FileType fileType = FileType.Binary, double timeFactor = 1.0D, double samplingRate = 30.0D, double nominalFrequency = 60.0D, bool includeFracSecDefinition = true)
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
            bool targetFloatingPoint = fileType == FileType.Float32;

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

            // Add meta data for selected points sorted analogs followed by status flags then digitals
            foreach (ChannelMetadata record in metadata.OrderBy(m => m, ChannelMetadataSorter.Default))
            {
                if (record.IsDigital)
                {
                    // Every synchrophasor digital is 16-bits
                    for (int i = 0; i < 16; i++)
                    {
                        digitalChannels.Add(new DigitalChannel(schema.Version)
                        {
                            Index = digitalIndex++,
                            Name = record.Name,
                            PhaseID = "B" + i.ToString("X")
                        });
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
        /// Creates a new COMTRADE configuration <see cref="Schema"/>.
        /// </summary>
        /// <param name="metadata">Schema <see cref="ChannelMetadata"/> records.</param>
        /// <param name="stationName">Station name for the schema.</param>
        /// <param name="deviceID">Device ID for the schema.</param>
        /// <param name="dataStartTime">Data start time.</param>
        /// <param name="sampleCount">Total data samples (i.e., total number of rows).</param>
        /// <param name="isBinary">Determines if data file should be binary or ASCII - defaults to <c>true</c> for binary.</param>
        /// <param name="timeFactor">Time factor to use in schema - defaults to 1000.</param>
        /// <param name="samplingRate">Desired sampling rate - defaults to 33.3333Hz.</param>
        /// <param name="nominalFrequency">Nominal frequency - defaults to 60Hz.</param>
        /// <param name="includeFracSecDefinition">Determines if the FRACSEC word digital definitions should be included - defaults to <c>true</c>.</param>
        /// <returns>New COMTRADE configuration <see cref="Schema"/>.</returns>
        /// <remarks>
        /// This function is primarily intended to create a configuration based on synchrophasor data
        /// (see Annex H: Schema for Phasor Data 2150 Using the COMTRADE File Standard in IEEE C37.111-2010),
        /// it may be necessary to manually create a schema object for other COMTRADE needs. You can call
        /// the <see cref="Schema.FileImage"/> property to return a string that can be written to a file
        /// that will be the contents of the configuration file.
        /// </remarks>
        [Obsolete("Switch to constructor overload that specifies a schema version and enumeration value to use for file type - this constructor may be removed from future builds", false)]
        public static Schema CreateSchema(IEnumerable<ChannelMetadata> metadata, string stationName, string deviceID, Ticks dataStartTime, int sampleCount, bool isBinary = true, double timeFactor = 1.0D, double samplingRate = 30.0D, double nominalFrequency = 60.0D, bool includeFracSecDefinition = true)
        {
            return CreateSchema(metadata, stationName, deviceID, dataStartTime, sampleCount, 1999, isBinary ? FileType.Binary : FileType.Ascii, timeFactor, samplingRate, nominalFrequency, includeFracSecDefinition);
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

                output.Write(LittleEndian.GetBytes((ushort)value), 0, 2);
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

                    output.Write(LittleEndian.GetBytes((uint)value), 0, 4);
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

                    output.Write(LittleEndian.GetBytes((float)value), 0, 4);
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
