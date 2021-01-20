//******************************************************************************************************
//  ConfigurationFrame.cs - Gbtc
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
//  04/30/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using GSF.Interop;
using GSF.IO.Checksums;
using GSF.Parsing;
using GSF.Reflection;

namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationFrame : ConfigurationFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
    {
        #region [ Members ]

        /// <summary>
        /// Default voltage phasor INI based configuration entry.
        /// </summary>
        public const string DefaultVoltagePhasorEntry = "V,4500.0,0.0060573,0,0,500,Default 500kV";

        /// <summary>
        /// Default current phasor INI based configuration entry.
        /// </summary>
        public const string DefaultCurrentPhasorEntry = "I,600.00,0.000040382,0,1,1,Default Current";

        /// <summary>
        /// Default frequency INI based configuration entry.
        /// </summary>
        public const string DefaultFrequencyEntry = "F,1000,60,1000,0,0,Frequency";

        // Events

        /// <summary>
        /// Occurs when the Macrodyne INI based configuration file has been reloaded.
        /// </summary>
        public event EventHandler ConfigurationFileReloaded;

        // Fields
        private CommonFrameHeader m_frameHeader;
        private readonly IniFile m_iniFile;
        private ConfigurationCellCollection m_configurationFileCells;
        private OnlineDataFormatFlags m_onlineDataFormatFlags;

    #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse a Macrodyne configuration frame.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigurationFrame()
            : base(0, new ConfigurationCellCollection(), 0, 0)
        {
            // We just assume current timestamp for configuration frames since they don't provide one
            Timestamp = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <param name="onlineDataFormatFlags">Online data format flags to use in this Macrodyne <see cref="ConfigurationFrame"/>.</param>
        /// <param name="unitID">8 character unit ID to use in this Macrodyne <see cref="ConfigurationFrame"/>.</param>
        /// <param name="configurationFileName">INI configuration file name, if specified.</param>
        /// <param name="deviceLabel">INI section name.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate a Macrodyne configuration frame.
        /// </remarks>
        public ConfigurationFrame(OnlineDataFormatFlags onlineDataFormatFlags, string unitID, string configurationFileName = null, string deviceLabel = null)
            : base(0, new ConfigurationCellCollection(), 0, 0)
        {
            m_onlineDataFormatFlags = onlineDataFormatFlags;
            StationName = unitID;

            ConfigurationCell configCell = new ConfigurationCell(this, deviceLabel);

            // Macrodyne protocol sends data for one device
            Cells.Add(configCell);

            if (!string.IsNullOrEmpty(configurationFileName))
                m_iniFile = new IniFile(configurationFileName);

            Refresh();
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration frame
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
            m_onlineDataFormatFlags = (OnlineDataFormatFlags)info.GetValue("onlineDataFormatFlags", typeof(OnlineDataFormatFlags));

            try
            {
                m_iniFile = new IniFile(info.GetString("configurationFileName"));
            }
            catch (SerializationException)
            {
                m_iniFile = null;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells => base.Cells as ConfigurationCellCollection;

        /// <summary>
        /// Gets the identifier that is used to identify the Macrodyne frame.
        /// </summary>
        public FrameType TypeID => Macrodyne.FrameType.ConfigurationFrame;

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            // Make sure frame header exists
            get => m_frameHeader ?? (m_frameHeader = new CommonFrameHeader());
            set
            {
                m_frameHeader = value;

                if (m_frameHeader is null)
                    return;

                if (m_frameHeader.State is ConfigurationFrameParsingState parsingState)
                {
                    State = parsingState;

                    // Cache station name for use when cell gets parsed
                    StationName = parsingState.HeaderFrame.HeaderData;
                }

                if (m_frameHeader.ProtocolVersion == ProtocolVersion.G)
                    TimestampIncluded = true;
            }
        }

        // This interface implementation satisfies ISupportFrameImage<int>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get => CommonHeader;
            set => CommonHeader = value as CommonFrameHeader;
        }

        /// <summary>
        /// Gets or sets the numeric ID code for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override ushort IDCode
        {
            get
            {
                if (Cells.Count > 0)
                    return Cells[0].IDCode;

                return base.IDCode;
            }
            set => base.IDCode = value;
        }

        /// <summary>
        /// Gets the INI based configuration file name of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public string ConfigurationFileName => m_iniFile.FileName;

        /// <summary>
        /// Gets the default voltage phasor definition.
        /// </summary>
        public PhasorDefinition DefaultPhasorV { get; private set; }

        /// <summary>
        /// Gets the default current phasor definition.
        /// </summary>
        public PhasorDefinition DefaultPhasorI { get; private set; }

        /// <summary>
        /// Gets the default frequency definition.
        /// </summary>
        public FrequencyDefinition DefaultFrequency { get; private set; }

        /// <summary>
        /// Gets station name retrieved from header frame.
        /// </summary>
        public string StationName { get; private set; }

        /// <summary>
        /// Gets or sets the Macrodyne <see cref="Macrodyne.OnlineDataFormatFlags"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public OnlineDataFormatFlags OnlineDataFormatFlags
        {
            get => m_onlineDataFormatFlags;
            set => m_onlineDataFormatFlags = value;
        }

        /// <summary>
        /// Gets phasor count derived from <see cref="OnlineDataFormatFlags"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public int PhasorCount
        {
            get
            {
                int count = 1;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor2Enabled) == OnlineDataFormatFlags.Phasor2Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor3Enabled) == OnlineDataFormatFlags.Phasor3Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor4Enabled) == OnlineDataFormatFlags.Phasor4Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor5Enabled) == OnlineDataFormatFlags.Phasor5Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor6Enabled) == OnlineDataFormatFlags.Phasor6Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor7Enabled) == OnlineDataFormatFlags.Phasor7Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor8Enabled) == OnlineDataFormatFlags.Phasor8Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor9Enabled) == OnlineDataFormatFlags.Phasor9Enabled)
                    count++;

                if ((m_onlineDataFormatFlags & OnlineDataFormatFlags.Phasor10Enabled) == OnlineDataFormatFlags.Phasor10Enabled)
                    count++;

                return count;
            }
        }

        /// <summary>
        /// Gets flag that determines if status 2 flags are included in ON-LINE data.
        /// </summary>
        public bool Status2Included => (m_onlineDataFormatFlags & OnlineDataFormatFlags.Status2ByteEnabled) == OnlineDataFormatFlags.Status2ByteEnabled;

        /// <summary>
        /// Gets flag that determines if timestamp is included in ON-LINE data.
        /// </summary>
        public bool TimestampIncluded
        {
            get => (m_onlineDataFormatFlags & OnlineDataFormatFlags.TimestampEnabled) == OnlineDataFormatFlags.TimestampEnabled;
            internal set
            {
                if (value)
                    m_onlineDataFormatFlags |= OnlineDataFormatFlags.TimestampEnabled;
                else
                    m_onlineDataFormatFlags &= ~OnlineDataFormatFlags.TimestampEnabled;
            }
        }

        /// <summary>
        /// Gets flag that determines if reference phasor is included in ON-LINE data.
        /// </summary>
        public bool ReferenceIncluded => (m_onlineDataFormatFlags & OnlineDataFormatFlags.ReferenceEnabled) == OnlineDataFormatFlags.ReferenceEnabled;

        /// <summary>
        /// Gets flag that determines if Digital 1 is included in ON-LINE data.
        /// </summary>
        public bool Digital1Included => (m_onlineDataFormatFlags & OnlineDataFormatFlags.Digital1Enabled) == OnlineDataFormatFlags.Digital1Enabled;

        /// <summary>
        /// Gets flag that determines if Digital 2 is included in ON-LINE data.
        /// </summary>
        public bool Digital2Included => (m_onlineDataFormatFlags & OnlineDataFormatFlags.Digital2Enabled) == OnlineDataFormatFlags.Digital2Enabled;

        /// <summary>
        /// Gets length of data frame based on enabled streaming data.
        /// </summary>
        public ushort DataFrameLength => (ushort)(7 + (CommonHeader.ProtocolVersion == ProtocolVersion.M ? 2 : 0) + PhasorCount * 4 + (Status2Included ? 1 : 0) + (TimestampIncluded ? 6 : 0) + (ReferenceIncluded ? 6 : 0) + (Digital1Included ? 2 : 0) + (Digital2Included ? 2 : 0));

        /// <summary>
        /// Gets the length of the <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <remarks>
        /// This property is overridden so the length can be extended to include a 1-byte checksum.
        /// </remarks>
        public override int BinaryLength
        {
            get
            {
                // We override normal binary length so we can modify length to include 1-byte checksum.
                // Also, if frame length was parsed from stream header - we use that length
                // instead of the calculated length...
                if (ParsedBinaryLength > 0)
                    return ParsedBinaryLength;

                // Subtract one byte for Macrodyne 1-byte CRC
                return base.BinaryLength - 1;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength =>
            // Common header plus two bytes for on-line data format flags
            CommonFrameHeader.FixedLength + 2;

        /// <summary>
        /// Gets the binary header image of the <see cref="DataFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];

                buffer.BlockCopy(0, CommonFrameHeader.FixedLength);
                BigEndian.CopyBytes((ushort)m_onlineDataFormatFlags, buffer, 2);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("ON-LINE Data Format Flags", $"{(byte)OnlineDataFormatFlags}: {OnlineDataFormatFlags}");

                if (!(m_iniFile is null))
                    baseAttributes.Add("Configuration File Name", m_iniFile.FileName);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reload Macrodyne INI based configuration file.
        /// </summary>
        public void Refresh()
        {
            if (m_iniFile is null)
                return;

            // The only time we need an access lock is when we reload the config file...
            lock (m_iniFile)
            {
                if (File.Exists(m_iniFile.FileName))
                {
                    ConfigurationCell pmuCell;
                    int phasorCount, pmuCount, x, y;

                    DefaultPhasorV = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorV", DefaultVoltagePhasorEntry]);
                    DefaultPhasorI = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorI", DefaultCurrentPhasorEntry]);
                    DefaultFrequency = new FrequencyDefinition(null, m_iniFile["DEFAULT", "Frequency", DefaultFrequencyEntry]);
                    FrameRate = ushort.Parse(m_iniFile["CONFIG", "SampleRate", "30"]);

                    // We read all cells in the config file into their own configuration cell collection - cells parsed
                    // from the configuration frame will be mapped to their associated config file cell by ID label
                    // when the configuration cell is parsed from the configuration frame
                    if (m_configurationFileCells is null)
                        m_configurationFileCells = new ConfigurationCellCollection(int.MaxValue);

                    m_configurationFileCells.Clear();

                    // Load phasor data for each section in config file...
                    foreach (string section in m_iniFile.GetSectionNames())
                    {
                        if (section.Length > 0)
                        {
                            // Make sure this is not a special section
                            if (!string.Equals(section, "DEFAULT", StringComparison.OrdinalIgnoreCase) && !string.Equals(section, "CONFIG", StringComparison.OrdinalIgnoreCase))
                            {
                                // Create new PMU entry structure from config file settings...
                                phasorCount = int.Parse(m_iniFile[section, "NumberPhasors", "0"]);

                                // Check for PDC code
                                int pdcID = int.Parse(m_iniFile[section, "PDC", "-1"]);

                                if (pdcID == -1)
                                {
                                    // No PDC entry exists, assume this is a PMU
                                    pmuCell = new ConfigurationCell(this);
                                    pmuCell.IDCode = ushort.Parse(m_iniFile[section, "PMU", Cells.Count.ToString()]);
                                    pmuCell.SectionEntry = section; // This will automatically assign ID label as first 4 digits of section
                                    pmuCell.StationName = m_iniFile[section, "Name", section];

                                    pmuCell.PhasorDefinitions.Clear();

                                    for (x = 0; x < phasorCount; x++)
                                    {
                                        pmuCell.PhasorDefinitions.Add(new PhasorDefinition(pmuCell, x + 1, m_iniFile[section, $"Phasor{(x + 1)}", DefaultVoltagePhasorEntry]));
                                    }

                                    pmuCell.FrequencyDefinition = new FrequencyDefinition(pmuCell, m_iniFile[section, "Frequency", DefaultFrequencyEntry]);
                                    m_configurationFileCells.Add(pmuCell);
                                }
                                else
                                {
                                    // This is a PDC, need to define one virtual entry for each PMU
                                    pmuCount = int.Parse(m_iniFile[section, "NumberPMUs", "0"]);

                                    for (x = 0; x < pmuCount; x++)
                                    {
                                        // Create a new PMU cell for each PDC entry that exists
                                        pmuCell = new ConfigurationCell(this);

                                        // For BPA INI files, PMUs tradionally have an ID number indexed starting at zero or one - so we multiply
                                        // ID by 1000 and add index to attempt to create a fairly unique ID to help optimize downstream parsing
                                        pmuCell.IDCode = unchecked((ushort)(pdcID * 1000 + x));
                                        pmuCell.SectionEntry = $"{section}pmu{x}"; // This will automatically assign ID label as first 4 digits of section
                                        pmuCell.StationName = $"{m_iniFile[section, "Name", section]} - Device {x + 1}";

                                        pmuCell.PhasorDefinitions.Clear();

                                        for (y = 0; y < 2; y++)
                                        {
                                            pmuCell.PhasorDefinitions.Add(new PhasorDefinition(pmuCell, y + 1, m_iniFile[section, $"Phasor{(x * 2 + y + 1)}", DefaultVoltagePhasorEntry]));
                                        }

                                        pmuCell.FrequencyDefinition = new FrequencyDefinition(pmuCell, m_iniFile[section, "Frequency", DefaultFrequencyEntry]);
                                        m_configurationFileCells.Add(pmuCell);
                                    }
                                }
                            }
                        }
                    }

                    // Associate single Macrodyne cell with its associated cell hopefully defined in INI file
                    if (m_configurationFileCells.Count > 0 && !(Cells is null) && Cells.Count > 0)
                    {
                        ConfigurationCell configurationFileCell = null;

                        // Assign INI file cell associating by section entry
                        ConfigurationCell cell = Cells[0];

                        // Attempt to associate this configuration cell with information read from external INI based configuration file
                        m_configurationFileCells.TryGetBySectionEntry(cell.SectionEntry, ref configurationFileCell);
                        cell.ConfigurationFileCell = configurationFileCell;
                        m_onlineDataFormatFlags = Common.GetFormatFlagsFromPhasorCount(cell.PhasorDefinitions.Count);
                        StationName = cell.StationName;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Macrodyne config file \"{m_iniFile.FileName}\" does not exist.");
                }
            }

            // In case other classes want to know, we send out a notification that the config file has been reloaded (make sure
            // you do this after the write lock has been released to avoid possible dead-lock situations)
            ConfigurationFileReloaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overridden so INI file can be loaded after binary image has been parsed as well as adjusting CRC length.
        /// </remarks>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            int parsedLength = base.ParseBinaryImage(buffer, startIndex, length);

            // Load INI file image and associate parsed cells to cells in configuration file...
            Refresh();

            return parsedLength;
        }

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            // We already parsed the frame header, so we just skip past it...
            startIndex += CommonFrameHeader.FixedLength;

            // Parse on -line data format
            m_onlineDataFormatFlags = (OnlineDataFormatFlags)BigEndian.ToUInt16(buffer, startIndex);

            return CommonFrameHeader.FixedLength + 2;
        }

        /// <summary>
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// Default implementation expects 2-byte big-endian ordered checksum. So we override method since checksum
        /// in Macrodyne is a single byte.
        /// </remarks>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            int sumLength = BinaryLength - 2;
            return buffer[startIndex + BinaryLength - 1] == CalculateChecksum(buffer, startIndex + 1, sumLength);
        }

        /// <summary>
        /// Appends checksum onto <paramref name="buffer"/> starting at <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="buffer">Buffer image on which to append checksum.</param>
        /// <param name="startIndex">Index into <paramref name="buffer"/> where checksum should be appended.</param>
        /// <remarks>
        /// Default implementation encodes checksum in big-endian order and expects buffer size large enough to accomodate
        /// 2-byte checksum representation. We override this method since checksum in Macrodyne is a single byte.
        /// </remarks>
        protected override void AppendChecksum(byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)CalculateChecksum(buffer, 1, startIndex);
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
            // Macrodyne uses 8-bit Xor checksum for frames
            return buffer.Xor8Checksum(offset, length);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("onlineDataFormatFlags", m_onlineDataFormatFlags, typeof(OnlineDataFormatFlags));
            info.AddValue("configurationFileName", m_iniFile is null ? null : m_iniFile.FileName);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets a generated INI configuration file image.
        /// </summary>
        public static string GetIniFileImage(IConfigurationFrame configFrame)
        {
            StringBuilder fileImage = new StringBuilder();

            fileImage.AppendLine($"; BPA PDCstream Style IniFile for Macrodyne Configuration {configFrame.IDCode}");
            fileImage.AppendLine($"; Auto-generated on {DateTime.Now}");
            fileImage.AppendLine($";    Assembly: {AssemblyInfo.ExecutingAssembly.Name}");
            fileImage.AppendLine($";    Compiled: {File.GetLastWriteTime(AssemblyInfo.ExecutingAssembly.Location)}");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";");
            fileImage.AppendLine("; Format:");
            fileImage.AppendLine(";   Each Column in data file is given a bracketed identifier, numbered in the order it");
            fileImage.AppendLine(";   appears in the data file, and identified by data type ( PMU, PDC, or other)");
            fileImage.AppendLine(";     PMU designates column data format from a single PMU");
            fileImage.AppendLine(";     PDC designates column data format from another PDC which is somewhat different from a single PMU");
            fileImage.AppendLine(";   Default gives default values for a processing algorithm in case quantities are omitted");
            fileImage.AppendLine(";   Name= gives the overall station name for print labels");
            fileImage.AppendLine(";   NumberPhasors= :  for PMU data, gives the number of phasors contained in column");
            fileImage.AppendLine(";                     for PDC data, gives the number of PMUs data included in the column");
            fileImage.AppendLine(";                     Note - for PDC data, there will be 2 phasors & 1 freq per PMU");
            fileImage.AppendLine(";   Quantities within the column are listed by PhasorI=, Frequency=, etc");
            fileImage.AppendLine(";   Each quantity has 7 comma separated fields followed by an optional comment");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";   Phasor entry format:  Type, Ratio, Cal Factor, Offset, Shunt, VoltageRef/Class, Label  ;Comments");
            fileImage.AppendLine(";    Type:       Type of measurement, V=voltage, I=current, N=don\'t care, single ASCII character");
            fileImage.AppendLine(";    Ratio:      PT/CT ratio N:1 where N is a floating point number");
            fileImage.AppendLine(";    Cal Factor: Conversion factor between integer in file and secondary volts, floating point");
            fileImage.AppendLine(";    Offset:     Phase Offset to correct for phase angle measurement errors or differences, floating point");
            fileImage.AppendLine(";    Shunt:      Current- shunt resistence in ohms, or the equivalent ratio for aux CTs, floating point");
            fileImage.AppendLine(";                Voltage- empty, not used");
            fileImage.AppendLine(";    VoltageRef: Current- phasor number (1-10) of voltage phasor to use for power calculation, integer");
            fileImage.AppendLine(";                Voltage- voltage class, standard l-l voltages, 500, 230, 115, etc, integer");
            fileImage.AppendLine(";    Label:      Phasor quantity label for print label, text");
            fileImage.AppendLine(";    Comments:   All text after the semicolon on a line are optional comments not for processing");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";   Voltage Magnitude = MAG(Real,Imaginary) * CalFactor * PTR (line-neutral)");
            fileImage.AppendLine(";   Current Magnitude = MAG(Real,Imaginary) * CalFactor * CTR / Shunt (phase current)");
            fileImage.AppendLine(";   Phase Angle = ATAN(Imaginary/Real) + Phase Offset (usually degrees)");
            fileImage.AppendLine(";     Note: Usually phase Offset is 0, but is sometimes required for comparing measurements");
            fileImage.AppendLine(";           from different systems or through transformer banks");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";   Frequency entry format:  scale, offset, dF/dt scale, dF/dt offset, dummy, label  ;Comments");
            fileImage.AppendLine(";   Frequency = Number / scale + offset");
            fileImage.AppendLine(";   dF/dt = Number / (dF/dt scale) + (dF/dt offset)");
            fileImage.AppendLine(";");
            fileImage.AppendLine(";");

            fileImage.AppendLine("[DEFAULT]");
            fileImage.AppendLine($"PhasorV={DefaultVoltagePhasorEntry}"); //PhasorDefinition.ConfigFileFormat(DefaultPhasorV));
            fileImage.AppendLine($"PhasorI={DefaultCurrentPhasorEntry}"); //PhasorDefinition.ConfigFileFormat(DefaultPhasorI));
            fileImage.AppendLine($"Frequency={DefaultFrequencyEntry}");   //FrequencyDefinition.ConfigFileFormat(DefaultFrequency));
            fileImage.AppendLine();

            fileImage.AppendLine("[CONFIG]");
            fileImage.AppendLine($"SampleRate={configFrame.FrameRate}");
            fileImage.AppendLine($"NumberOfPMUs={configFrame.Cells.Count}");
            fileImage.AppendLine();

            for (int x = 0; x < configFrame.Cells.Count; x++)
            {
                fileImage.AppendLine($"[{configFrame.Cells[x].IDLabel}]");
                fileImage.AppendLine($"Name={configFrame.Cells[x].StationName}");
                fileImage.AppendLine($"PMU={x}");
                fileImage.AppendLine($"NumberPhasors={configFrame.Cells[x].PhasorDefinitions.Count}");
                for (int y = 0; y < configFrame.Cells[x].PhasorDefinitions.Count; y++)
                {
                    fileImage.AppendLine($"Phasor{(y + 1)}={PhasorDefinition.ConfigFileFormat(configFrame.Cells[x].PhasorDefinitions[y])}");
                }
                fileImage.AppendLine($"Frequency={FrequencyDefinition.ConfigFileFormat(configFrame.Cells[x].FrequencyDefinition)}");
                fileImage.AppendLine();
            }

            return fileImage.ToString();
        }

        #endregion
    }
}