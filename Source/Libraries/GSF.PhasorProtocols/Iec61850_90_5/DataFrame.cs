//******************************************************************************************************
//  DataFrame.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  04/19/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using GSF.IO;
using GSF.Parsing;
using GSF;

namespace GSF.PhasorProtocols.Iec61850_90_5
{
    /// <summary>
    /// Represents the IEC 61850-90-5 implementation of a <see cref="IDataFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class DataFrame : DataFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
    {
        #region [ Members ]

        // Fields
        private CommonFrameHeader m_frameHeader;
        private string m_msvID;
        private string m_dataSet;
        private ushort m_sampleCount;
        private uint m_configurationRevision;
        private byte m_sampleSynchronization;
        private ushort m_sampleRate;
        private ushort m_idCode;
        private string m_stationName;
        private ConfigurationFrame m_configuration;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an IEC 61850-90-5 data frame.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DataFrame()
            : base(new DataCellCollection(), 0, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataFrame"/> from specified parameters.
        /// </summary>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="DataFrame"/>.</param>
        /// <param name="configurationFrame">The <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEC 61850-90-5 data frame.
        /// </remarks>
        public DataFrame(Ticks timestamp, ConfigurationFrame configurationFrame)
            : base(new DataCellCollection(), timestamp, configurationFrame)
        {
            // Pass timebase along to DataFrame's common header
            if (configurationFrame != null)
                CommonHeader.Timebase = configurationFrame.Timebase;
        }

        /// <summary>
        /// Creates a new <see cref="DataFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data frame
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
            m_msvID = info.GetString("msvID");
            m_dataSet = info.GetString("dataSet");
            m_sampleCount = info.GetUInt16("sampleCount");
            m_configurationRevision = info.GetUInt32("configurationRevision");
            SampleSynchronization = info.GetByte("sampleSynchronization");
            m_sampleRate = info.GetUInt16("sampleRate");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="DataFrame"/>.
        /// </summary>
        public new DataCellCollection Cells
        {
            get
            {
                return base.Cells as DataCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="ConfigurationFrame"/> associated with this <see cref="DataFrame"/>.
        /// </summary>
        public new ConfigurationFrame ConfigurationFrame
        {
            get
            {
                return base.ConfigurationFrame as ConfigurationFrame;
            }
            set
            {
                base.ConfigurationFrame = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="DataFrame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public override Ticks Timestamp
        {
            get
            {
                return CommonHeader.Timestamp;
            }
            set
            {
                // Keep timestamp updates synchrnonized...
                CommonHeader.Timestamp = value;
                base.Timestamp = value;
            }
        }

        /// <summary>
        /// Gets the identifier that is used to identify the IEC 61850-90-5 frame.
        /// </summary>
        public FrameType TypeID
        {
            get
            {
                return Iec61850_90_5.FrameType.DataFrame;
            }
        }

        /// <summary>
        /// Gets or sets IEC 61850-90-5 sample synchronization state.
        /// </summary>
        public byte SampleSynchronization
        {
            get
            {
                return m_sampleSynchronization;
            }
            set
            {
                m_sampleSynchronization = value;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get
            {
                // Make sure frame header exists - using base class timestamp to
                // prevent recursion (m_frameHeader doesn't exist yet)
                if (m_frameHeader == null)
                    m_frameHeader = new CommonFrameHeader(TypeID, base.IDCode, base.Timestamp);

                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;

                if (m_frameHeader != null)
                {
                    State = m_frameHeader.State as IDataFrameParsingState;
                    base.Timestamp = m_frameHeader.Timestamp;
                }
            }
        }

        // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get
            {
                return CommonHeader;
            }
            set
            {
                CommonHeader = value as CommonFrameHeader;
            }
        }

        /// <summary>
        /// Gets the length of the header image.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return CommonHeader.Length;
            }
        }

        // IEC 61850-90-5 implementation currently only supports a read-only mode.
        ///// <summary>
        ///// Gets the binary header image of the <see cref="DataFrame"/> object.
        ///// </summary>
        //protected override byte[] HeaderImage
        //{
        //    get
        //    {
        //        // Make sure to provide proper frame length for use in the common header image
        //        unchecked
        //        {
        //            CommonHeader.FrameLength = (ushort)BinaryLength;
        //        }

        //        return CommonHeader.BinaryImage;
        //    }
        //}

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                CommonHeader.AppendHeaderAttributes(baseAttributes);
                baseAttributes.Add("MSVID", m_msvID);
                baseAttributes.Add("Dataset", m_dataSet);
                baseAttributes.Add("Sample Count", m_sampleCount.ToString());
                baseAttributes.Add("Configuration Revision", m_configurationRevision.ToString());
                baseAttributes.Add("Sample Synchronization", SampleSynchronization + ": " + (SampleSynchronization == 0 ? "Not Synchronized" : "Synchronized"));
                baseAttributes.Add("Sample Rate", m_sampleRate.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            // Overrides base class behavior, ASDUs can generally be parsed even without configuration.
            buffer.ValidateParameters(startIndex, length);

            CommonFrameHeader header = CommonHeader;
            IDataFrameParsingState state = State;
            IConfigurationFrame configurationFrame = state.ConfigurationFrame;

            // Make sure configuration frame gets assigned before parsing begins, if available...
            if (configurationFrame != null)
                ConfigurationFrame = configurationFrame as ConfigurationFrame;

            int tagLength, index = startIndex;

            // Header has already been parsed, skip past it
            index += header.Length;

            // Get reference to configuration frame, if available
            m_configuration = ConfigurationFrame;

            // Parse each ASDU in incoming frame (e.g, DataCell(t-2), DataCell(t-1), DataCell(t))
            for (int i = 0; i < header.AsduCount; i++)
            {
                // Handle redundant ASDU - last one parsed will be newest and exposed normally
                if (i > 0)
                {
                    if (header.ParseRedundantASDUs)
                    {
                        // Create a new data frame to hold redundant ASDU data
                        DataFrame dataFrame = new DataFrame
                        {
                            ConfigurationFrame = m_configuration,
                            CommonHeader = header
                        };

                        // Add a copy of the current data cell to the new frame 
                        foreach (DataCell cell in Cells)
                        {
                            dataFrame.Cells.Add(cell);
                        }

                        // Publish new data frame with redundant ASDU data
                        header.PublishFrame(dataFrame);
                    }

                    // Clear any existing ASDU values from this data frame
                    Cells.Clear();
                }

                // Validate ASDU sequence tag exists and skip past it
                buffer.ValidateTag(SampledValueTag.AsduSequence, ref index);

                // Parse MSVID value
                m_msvID = buffer.ParseStringTag(SampledValueTag.MsvID, ref index);

                // If formatted according to implementation agreement, MSVID value will contain an ID code and station name
                if (!string.IsNullOrWhiteSpace(m_msvID))
                {
                    int underscoreIndex = m_msvID.IndexOf("_");

                    if (underscoreIndex > 0)
                    {
                        if (!ushort.TryParse(m_msvID.Substring(0, underscoreIndex), out m_idCode))
                        {
                            m_idCode = 1;
                            m_stationName = m_msvID;
                        }
                        else
                        {
                            m_stationName = m_msvID.Substring(underscoreIndex + 1);
                        }
                    }
                    else
                    {
                        m_idCode = 1;
                        m_stationName = m_msvID;
                    }
                }
                else
                {
                    m_idCode = 1;
                    m_stationName = "IEC61850Dataset";
                }

                //// Dataset name has been removed as per implemenation agreement
                //// Parse dataset name
                //m_dataSet = buffer.ParseStringTag(SampledValueTag.Dataset, ref index);

                // Parse sample count (for some reason this is coming in as 3 bytes)
                m_sampleCount = buffer.ParseUInt16Tag(SampledValueTag.SmpCnt, ref index);

                // Parse configuration revision (for some reason this is coming in as 5 bytes)
                m_configurationRevision = buffer.ParseUInt32Tag(SampledValueTag.ConfRev, ref index);

                // Parse refresh time
                if ((SampledValueTag)buffer[index] != SampledValueTag.RefrTm)
                    throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[startIndex].ToString("X").PadLeft(2, '0'));

                index++;
                tagLength = buffer.GetTagLength(ref index);

                if (tagLength < 8)
                    throw new InvalidOperationException(string.Format("Unexpected length for \"{0}\" tag: {1}", SampledValueTag.RefrTm, tagLength));

                uint secondOfCentury = EndianOrder.BigEndian.ToUInt32(buffer, index);
                uint fractionOfSecond = EndianOrder.BigEndian.ToUInt32(buffer, index + 4);
                index += 8;

                // Get whole seconds of timestamp
                long timestamp = (new UnixTimeTag((double)secondOfCentury)).ToDateTime().Ticks;

                // Add fraction seconds of timestamp
                decimal fractionalSeconds = (fractionOfSecond & ~Common.TimeQualityFlagsMask) / (decimal)Common.Timebase;
                timestamp += (long)(fractionalSeconds * (decimal)Ticks.PerSecond);

                // Apply parsed timestamp to common header
                header.Timestamp = timestamp;
                header.TimeQualityFlags = (TimeQualityFlags)(fractionOfSecond & Common.TimeQualityFlagsMask);

                // Parse sample synchronization state
                SampleSynchronization = buffer.ParseByteTag(SampledValueTag.SmpSynch, ref index);

                // Parse optional sample rate
                if ((SampledValueTag)buffer[index] == SampledValueTag.SmpRate)
                    m_sampleRate = buffer.ParseUInt16Tag(SampledValueTag.SmpRate, ref index);

                // Validate that next tag is for sample values
                if ((SampledValueTag)buffer[index] != SampledValueTag.Samples)
                    throw new InvalidOperationException("Encountered out-of-sequence or unknown sampled value tag: 0x" + buffer[startIndex].ToString("X").PadLeft(2, '0'));

                index++;
                tagLength = buffer.GetTagLength(ref index);

                // Attempt to derive a configuration if none is defined
                if ((object)m_configuration == null)
                {
                    // If requested, attempt to load configuration from an associated ETR file
                    if (header.UseETRConfiguration)
                        ParseETRConfiguration();

                    // If we still have no configuration, see if a "guess" is requested
                    if ((object)m_configuration == null && header.GuessConfiguration)
                        GuessAtConfiguration(tagLength);
                }

                if ((object)m_configuration == null)
                {
                    // If the configuration is still unavailable, skip past sample values - don't know the details otherwise
                    index += tagLength;
                }
                else
                {
                    // See if sample size validation should by bypassed
                    if (header.IgnoreSampleSizeValidationFailures)
                    {
                        base.ParseBodyImage(buffer, index, length - (index - startIndex));
                        index += tagLength;
                    }
                    else
                    {
                        // Validate that sample size matches current configuration
                        if (tagLength != m_configuration.GetCalculatedSampleLength())
                            throw new InvalidOperationException("Configuration does match data sample size - cannot parse data");

                        // Parse standard synchrophasor sequence
                        index += base.ParseBodyImage(buffer, index, length - (index - startIndex));
                    }
                }

                // Skip past optional sample mod tag, if defined
                if ((SampledValueTag)buffer[startIndex] == SampledValueTag.SmpMod)
                    buffer.ValidateTag(SampledValueTag.SmpMod, ref index);

                // Skip past optional UTC timestamp tag, if defined
                if ((SampledValueTag)buffer[startIndex] == SampledValueTag.UtcTimestamp)
                    buffer.ValidateTag(SampledValueTag.UtcTimestamp, ref index);
            }

            // We're not parsing any of the items remaining in the footer...
            return header.FrameLength;
        }

        // Attempt to parse an associated ETR configuration
        private void ParseETRConfiguration()
        {
            if (!string.IsNullOrWhiteSpace(m_msvID))
            {
                // See if an associated ETR file exists
                string etrFileName = m_msvID + ".etr";
                string etrFilePath = FilePath.GetAbsolutePath(etrFileName);
                bool foundETRFile = File.Exists(etrFilePath);

                if (!foundETRFile)
                {
                    // Also test for ETR in configuration cache folder
                    etrFilePath = FilePath.GetAbsolutePath("ConfigurationCache\\" + etrFileName);
                    foundETRFile = File.Exists(etrFilePath);
                }

                if (foundETRFile)
                {
                    try
                    {
                        StreamReader reader = new StreamReader(etrFilePath);
                        SignalType signalType, lastSignalType = SignalType.NONE;
                        string label;
                        bool statusDefined = false;
                        bool endOfFile = false;
                        int magnitudeSignals = 0;
                        int angleSignals = 0;

                        ConfigurationFrame configFrame = new ConfigurationFrame(Common.Timebase, 1, DateTime.UtcNow.Ticks, m_sampleRate);

                        do
                        {
                            bool badOrder = false;

                            ConfigurationCell configCell = new ConfigurationCell(configFrame, (ushort)(m_idCode + configFrame.Cells.Count), LineFrequency.Hz60)
                            {
                                StationName = m_stationName + (configFrame.Cells.Count + 1)
                            };

                            // Keep parsing records until there are no more...
                            while (ParseNextSampleDefinition(reader, out signalType, out label, out endOfFile))
                            {
                                // If ETR is defining a new device, exit and handle current device
                                if (signalType == SignalType.FLAG && statusDefined)
                                {
                                    badOrder = (lastSignalType != SignalType.DFDT && lastSignalType != SignalType.ALOG && lastSignalType != SignalType.DIGI);
                                    lastSignalType = SignalType.FLAG;
                                    break;
                                }

                                // Validate signal order
                                switch (signalType)
                                {
                                    case SignalType.FLAG:
                                        badOrder = lastSignalType != SignalType.NONE;
                                        statusDefined = true;
                                        break;
                                    case SignalType.VPHM:
                                    case SignalType.IPHM:
                                        badOrder = (lastSignalType != SignalType.FLAG && lastSignalType != SignalType.VPHA && lastSignalType != SignalType.IPHA);
                                        PhasorDefinition phasor = new PhasorDefinition(configCell, label, 1, 0.0D, signalType == SignalType.VPHM ? PhasorType.Voltage : PhasorType.Current, null);
                                        configCell.PhasorDefinitions.Add(phasor);
                                        magnitudeSignals++;
                                        break;
                                    case SignalType.VPHA:
                                        badOrder = lastSignalType != SignalType.VPHM;
                                        angleSignals++;
                                        break;
                                    case SignalType.IPHA:
                                        badOrder = lastSignalType != SignalType.IPHM;
                                        angleSignals++;
                                        break;
                                    case SignalType.FREQ:
                                        badOrder = (lastSignalType != SignalType.VPHA && lastSignalType != SignalType.IPHA);
                                        break;
                                    case SignalType.DFDT:
                                        badOrder = lastSignalType != SignalType.FREQ;
                                        configCell.FrequencyDefinition = new FrequencyDefinition(configCell, "Frequency");
                                        break;
                                    case SignalType.ALOG:
                                        badOrder = (lastSignalType != SignalType.DFDT && lastSignalType != SignalType.ALOG);
                                        AnalogDefinition analog = new AnalogDefinition(configCell, label, 1, 0.0D, AnalogType.SinglePointOnWave);
                                        configCell.AnalogDefinitions.Add(analog);
                                        break;
                                    case SignalType.DIGI:
                                        badOrder = (lastSignalType != SignalType.DFDT && lastSignalType != SignalType.ALOG && lastSignalType != SignalType.DIGI);
                                        DigitalDefinition digital = new DigitalDefinition(configCell, label, 0, 1);
                                        configCell.DigitalDefinitions.Add(digital);
                                        break;
                                    default:
                                        throw new InvalidOperationException("Unxpected signal type enecountered: " + signalType);
                                }

                                lastSignalType = signalType;
                            }

                            if (badOrder)
                                throw new InvalidOperationException(string.Format("Invalid signal order encountered - {0} cannot follow {1}. Standard synchrophasor order is: status flags, one or more phasor magnitude/angle pairs, frequency, dF/dt, optional analogs, optional digitals", signalType, lastSignalType));

                            if (!statusDefined)
                                throw new InvalidOperationException("No status flag signal was defined.");

                            if (configCell.PhasorDefinitions.Count == 0)
                                throw new InvalidOperationException("No phasor magnitude/angle signal pairs were defined.");

                            if (magnitudeSignals != angleSignals)
                                throw new InvalidOperationException("Phasor magnitude/angle signal pair mismatch - there must be a one-to-one defintion between angle and magnitude signals.");

                            if (configCell.FrequencyDefinition == null)
                                throw new InvalidOperationException("No frequency and dF/dt signal pair was defined.");

                            // Add cell to configuration frame
                            configFrame.Cells.Add(configCell);

                            // Reset counters
                            magnitudeSignals = 0;
                            angleSignals = 0;
                        }
                        while (!endOfFile);

                        // Publish configuration frame
                        PublishNewConfigurationFrame(configFrame);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(string.Format("Failed to parse associated ETR configuration \"{0}\": {1}", etrFilePath, ex.Message), ex);
                    }
                }
            }
        }

        // Complex function used to read next signal type and lable from the ETR file...
        // Note that current parsing depends on sample tag name format defined in the IEC 61850-90-5 implementation agreement
        private bool ParseNextSampleDefinition(StreamReader reader, out SignalType signalType, out string label, out bool endOfFile)
        {
            string signalLabel, dataType, signalDetail;
            bool result = false;

            signalType = SignalType.NONE;
            label = null;

            // Attempt to read signal definition and label line
            signalLabel = reader.ReadLine();

            if (signalLabel != null)
            {
                // Attempt to reader data type line
                dataType = reader.ReadLine();

                if (dataType != null)
                {
                    // Clean up data type
                    dataType = dataType.Trim().ToLower();

                    int index = signalLabel.IndexOf("-");

                    // Get defined signal label
                    label = signalLabel.Substring(index + 1).Trim();

                    // See if signal type contains ST
                    index = signalLabel.IndexOf(".ST.");

                    if (index > 0)
                    {
                        // Get detail portion of signal type label
                        signalDetail = signalLabel.Substring(index + 4);

                        // Status or digital value
                        if (signalDetail.StartsWith("Ind1"))
                        {
                            // Status word value
                            signalType = SignalType.FLAG;

                            if (dataType != "i2")
                                throw new InvalidOperationException(string.Format("Invalid data type size {0} specified for signal type {1} parsed from {2}", dataType, signalType, signalLabel));

                            result = true;
                        }
                        else if (signalDetail.StartsWith("Ind2"))
                        {
                            // Digital value
                            signalType = SignalType.DIGI;

                            if (dataType != "i2")
                                throw new InvalidOperationException(string.Format("Invalid data type size {0} specified for signal type {1} parsed from {2}", dataType, signalType, signalLabel));

                            result = true;
                        }
                        else
                        {
                            // Unable to determine signal type
                            throw new InvalidOperationException(string.Format("Unable to determine ETR signal type for {0} ({1})", signalLabel, dataType));
                        }
                    }
                    else
                    {
                        // See if signal type contains MX
                        index = signalLabel.IndexOf(".MX.");

                        if (index > 0)
                        {
                            // Get detail portion of signal type label
                            signalDetail = signalLabel.Substring(index + 4);

                            // Frequency or phasor value
                            if (signalDetail.StartsWith("HzRte"))
                            {
                                // dF/dt value
                                signalType = SignalType.DFDT;

                                if (dataType != "f4")
                                    throw new InvalidOperationException(string.Format("Invalid data type size {0} specified for signal type {1} parsed from {2}", dataType, signalType, signalLabel));

                                result = true;
                            }
                            else if (signalDetail.StartsWith("Hz"))
                            {
                                // Frequency value
                                signalType = SignalType.FREQ;

                                if (dataType != "f4")
                                    throw new InvalidOperationException(string.Format("Invalid data type size {0} specified for signal type {1} parsed from {2}", dataType, signalType, signalLabel));

                                result = true;
                            }
                            else if (signalDetail.StartsWith("PhV") || signalDetail.StartsWith("SeqV"))
                            {
                                if (signalDetail.Contains(".mag."))
                                {
                                    // Voltage phase magnitude
                                    signalType = SignalType.VPHM;
                                }
                                else if (signalDetail.Contains(".ang."))
                                {
                                    // Voltage phase angle
                                    signalType = SignalType.VPHA;
                                }
                                else
                                {
                                    // Unable to determine signal type
                                    throw new InvalidOperationException(string.Format("Unable to determine ETR signal type for {0} ({1})", signalLabel, dataType));
                                }

                                if (dataType != "f4")
                                    throw new InvalidOperationException(string.Format("Invalid data type size {0} specified for signal type {1} parsed from {2}", dataType, signalType, signalLabel));

                                result = true;
                            }
                            else if (signalDetail.StartsWith("SeqA") || signalDetail.StartsWith("A"))
                            {
                                if (signalDetail.Contains(".mag."))
                                {
                                    // Current phase magnitude
                                    signalType = SignalType.IPHM;
                                }
                                else if (signalDetail.Contains(".ang."))
                                {
                                    // Current phase angle
                                    signalType = SignalType.IPHA;
                                }
                                else
                                {
                                    // Unable to determine signal type
                                    throw new InvalidOperationException(string.Format("Unable to determine ETR signal type for {0} ({1})", signalLabel, dataType));
                                }

                                if (dataType != "f4")
                                    throw new InvalidOperationException(string.Format("Invalid data type size {0} specified for signal type {1} parsed from {2}", dataType, signalType, signalLabel));

                                result = true;
                            }
                            else
                            {
                                // Unable to determine signal type
                                throw new InvalidOperationException(string.Format("Unable to determine ETR signal type for {0} ({1})", signalLabel, dataType));
                            }
                        }
                        else
                        {
                            // Assuming anything else is an Analog value
                            signalType = SignalType.ALOG;

                            if (dataType != "f4")
                                throw new InvalidOperationException(string.Format("Invalid data type size {0} specified for assumed analog signal type parsed from {1}", dataType, signalLabel));

                            result = true;
                        }
                    }
                }
            }

            endOfFile = !result;
            return result;
        }

        // Attempt to guess at the configuration
        private void GuessAtConfiguration(int sampleLength)
        {
            // Removed fixed length for 2-byte status, 4-byte frequency and 4-byte dF/dt
            int test = sampleLength - 10;

            // Assume remaining even 8-byte pairs are phasor values (i.e., 4-byte magnitude and 4-byte angle)
            int phasors = test / 8;
            test -= phasors * 8;

            // Assume remaining even 2-byte items are digital values
            int digitals = test / 2;
            test -= digitals * 2;

            // If no bytes remain, we'll assume this distribution as a guess configuration
            if (test == 0)
            {
                // Just assume some details for a configuration frame
                ConfigurationFrame configFrame = new ConfigurationFrame(Common.Timebase, 1, DateTime.UtcNow.Ticks, m_sampleRate);
                ConfigurationCell configCell = new ConfigurationCell(configFrame, m_idCode, LineFrequency.Hz60)
                {
                    StationName = m_stationName
                };

                // Add phasors
                for (int i = 0; i < phasors; i++)
                {
                    PhasorType type = i < phasors / 2 ? PhasorType.Voltage : PhasorType.Current;
                    PhasorDefinition phasor = new PhasorDefinition(configCell, "Phasor " + (i + 1), 1, 0.0D, type, null);
                    configCell.PhasorDefinitions.Add(phasor);
                }

                // Add frequency
                configCell.FrequencyDefinition = new FrequencyDefinition(configCell, "Frequency");

                // Add digitals
                for (int i = 0; i < digitals; i++)
                {
                    DigitalDefinition digital = new DigitalDefinition(configCell, "Digital " + (i + 1), 0, 1);
                    configCell.DigitalDefinitions.Add(digital);
                }

                configFrame.Cells.Add(configCell);
                PublishNewConfigurationFrame(configFrame);
            }
        }

        // Exposes a newly created configuration frame
        private void PublishNewConfigurationFrame(ConfigurationFrame configFrame)
        {
            // Cache new configuration
            m_configuration = configFrame;

            // Cache new associated configuration frame
            ConfigurationFrame = configFrame;

            // Update the frame level parsing state
            DataFrameParsingState parsingState = new DataFrameParsingState(CommonHeader.FrameLength, configFrame, DataCell.CreateNewCell);
            CommonHeader.State = parsingState;
            State = parsingState;

            // Update local associated configuration cells
            for (int i = 0; i < Cells.Count; i++)
            {
                ConfigurationCell configCell = configFrame.Cells[i];

                // Update associated configuration cell
                Cells[i].ConfigurationCell = configCell;

                // Update local parsing state with new configuration info
                Cells[i].State = new DataCellParsingState(
                    configCell,
                    Iec61850_90_5.PhasorValue.CreateNewValue,
                    Iec61850_90_5.FrequencyValue.CreateNewValue,
                    Iec61850_90_5.AnalogValue.CreateNewValue,
                    Iec61850_90_5.DigitalValue.CreateNewValue);
            }

            // Publish the configuration frame to the rest of the system
            CommonHeader.PublishFrame(configFrame);
        }

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            // IEC 61850-90-5 data frame CRC is checked during header parsing
            return true;
        }

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException("Checksum for IEC 61850-90-5 data frames are calculated when header is parsed, see CommonFrameHeader.");
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("msvID", m_msvID);
            info.AddValue("dataSet", m_dataSet);
            info.AddValue("sampleCount", m_sampleCount);
            info.AddValue("configurationRevision", m_configurationRevision);
            info.AddValue("sampleSynchronization", SampleSynchronization);
            info.AddValue("sampleRate", m_sampleRate);
        }

        #endregion
    }
}