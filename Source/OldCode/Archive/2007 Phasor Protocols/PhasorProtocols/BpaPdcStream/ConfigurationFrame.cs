//*******************************************************************************************************
//  ConfigurationFrame.vb - PDCstream Configuration Frame / File
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Threading;
using PCS;
using PCS.Interop;
using PCS.Reflection;
using PCS.IO.Checksums;

namespace PCS.PhasorProtocols
{
    namespace BpaPdcStream
    {

        // Note that it is expected that the end user will typically create only one instance of this class per INI file for use by any
        // number of different threads and a request can be made at anytime to "reload" the config file, so we make sure all publically
        // accessible methods in the class make proper use of the internal reader-writer lock.  This also allows end user to place a
        // file-watcher on the INI file so class can "reload" config file when it's updated...
        [CLSCompliant(false), Serializable()]
        public class ConfigurationFrame : ConfigurationFrameBase, ICommonFrameHeader
        {
            private ReaderWriterLock m_readWriteLock;
            private IniFile m_iniFile;
            private ConfigurationCellCollection m_configurationFileCells;
            private PhasorDefinition m_defaultPhasorV;
            private PhasorDefinition m_defaultPhasorI;
            private FrequencyDefinition m_defaultFrequency;
            private ushort m_rowLength;
            private short m_packetsPerSample;
            private StreamType m_streamType;
            private RevisionNumber m_revisionNumber;

            public delegate void ConfigFileReloadedEventHandler();
            public event ConfigFileReloadedEventHandler ConfigFileReloaded;

            public const string DefaultVoltagePhasorEntry = "V,4500.0,0.0060573,0,0,500,Default 500kV";
            public const string DefaultCurrentPhasorEntry = "I,600.00,0.000040382,0,1,1.0,Default Current";
            public const string DefaultFrequencyEntry = "F,1000,60,1000,0,0,Frequency";

            protected ConfigurationFrame()
            {
            }

            protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize configuration frame
                m_packetsPerSample = info.GetInt16("packetsPerSample");
                m_streamType = (StreamType)info.GetValue("streamType", typeof(StreamType));
                m_revisionNumber = (RevisionNumber)info.GetValue("revisionNumber", typeof(RevisionNumber));
                m_iniFile = new IniFile(info.GetString("configurationFileName"));
                m_readWriteLock = new ReaderWriterLock();
                Refresh(false);

            }

            public ConfigurationFrame(string configurationFileName)
                : base(new ConfigurationCellCollection())
            {


                m_iniFile = new IniFile(configurationFileName);
                m_readWriteLock = new ReaderWriterLock();
                m_packetsPerSample = 1;
                Refresh(false);

            }

            // If you are going to create multiple data packets, you can use this constructor
            // Note that this only starts becoming necessary if you start hitting data size
            // limits imposed by the nature of the transport protocol...
            public ConfigurationFrame(string configurationFileName, short packetsPerSample)
                : this(configurationFileName)
            {

                m_packetsPerSample = packetsPerSample;

            }

            public ConfigurationFrame(ICommonFrameHeader parsedFrameHeader, string configurationFileName, byte[] binaryImage, int startIndex)
                : base(new ConfigurationFrameParsingState(new ConfigurationCellCollection(), parsedFrameHeader.FrameLength,
                    BpaPdcStream.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)
            {


                CommonFrameHeader.Clone(parsedFrameHeader, this);

                m_iniFile = new IniFile(configurationFileName);
                m_readWriteLock = new ReaderWriterLock();
                m_packetsPerSample = 1;

                Refresh(true);

            }

            public ConfigurationFrame(IConfigurationFrame configurationFrame)
                : base(configurationFrame)
            {


            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationCellCollection Cells
            {
                get
                {
                    return (ConfigurationCellCollection)base.Cells;
                }
            }

            public ConfigurationCellCollection ConfigurationFileCells
            {
                get
                {
                    return m_configurationFileCells;
                }
            }

            public StreamType StreamType
            {
                get
                {
                    return m_streamType;
                }
                set
                {
                    m_streamType = value;
                }
            }

            public RevisionNumber RevisionNumber
            {
                get
                {
                    return m_revisionNumber;
                }
                set
                {
                    m_revisionNumber = value;
                }
            }

            public short SampleNumber
            {
                get
                {
                    return -1;
                }
                set
                {
                    // Sample number is readonly for configuration frames - we don't throw an exception here if someone attempts to change
                    // the packet number on a configuration frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                    // but we don't do anything with the value either.
                }
            }

            public byte PacketNumber
            {
                get
                {
                    return Common.DescriptorPacketFlag;
                }
                set
                {
                    // Packet number is readonly for configuration frames - we don't throw an exception here if someone attempts to change
                    // the packet number on a configuration frame (e.g., the CommonFrameHeader.Clone method will attempt to copy this property)
                    // but we don't do anything with the value either.
                }
            }

            public FrameType FrameType
            {
                get
                {
                    return BpaPdcStream.FrameType.DataFrame;
                }
            }

            FundamentalFrameType ICommonFrameHeader.FundamentalFrameType
            {
                get
                {
                    return base.FundamentalFrameType;
                }
            }

            public short WordCount
            {
                get
                {
                    return (short)(base.BinaryLength / 2);
                }
                set
                {
                    base.ParsedBinaryLength = (ushort)(value * 2);
                }
            }

            public ushort FrameLength
            {
                get
                {
                    return base.BinaryLength;
                }
            }

            public void Refresh()
            {

                Refresh(false);

            }

            private void Refresh(bool refreshCausedByFrameParse)
            {

                // The only time we need a write lock is when we reload the config file...
                m_readWriteLock.AcquireWriterLock(-1);

                try
                {
                    if (File.Exists(m_iniFile.FileName))
                    {
                        ConfigurationCell pmuCell;
                        int x;
                        int phasorCount;

                        m_defaultPhasorV = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorV", DefaultVoltagePhasorEntry]);
                        m_defaultPhasorI = new PhasorDefinition(null, 0, m_iniFile["DEFAULT", "PhasorI", DefaultCurrentPhasorEntry]);
                        m_defaultFrequency = new FrequencyDefinition(null, m_iniFile["DEFAULT", "Frequency", DefaultFrequencyEntry]);
                        FrameRate = Convert.ToInt16(m_iniFile["CONFIG", "SampleRate", "30"]);

                        // We read all cells in the config file into their own configuration cell collection - cells parsed
                        // from the configuration frame will be mapped to their associated config file cell by ID label
                        // when the configuration cell is parsed from the configuration frame
                        if (m_configurationFileCells == null)
                        {
                            m_configurationFileCells = new ConfigurationCellCollection();
                        }
                        m_configurationFileCells.Clear();

                        // Load phasor data for each section in config file...
                        foreach (string section in m_iniFile.SectionNames)
                        {
                            if (section.Length > 0)
                            {
                                // Make sure this is not a special section
                                if (string.Compare(section, "DEFAULT", true) != 0 && string.Compare(section, "CONFIG", true) != 0)
                                {
                                    // Create new PMU entry structure from config file settings...
                                    phasorCount = Convert.ToInt32(m_iniFile[section, "NumberPhasors", "0"]);

                                    pmuCell = new ConfigurationCell(this, 0, LineFrequency.Hz60);

                                    pmuCell.SectionEntry = section; // This will automatically assign ID label as first 4 digits of section
                                    pmuCell.StationName = m_iniFile[section, "Name", section];
                                    pmuCell.IDCode = Convert.ToUInt16(m_iniFile[section, "PMU", Cells.Count.ToString()]);

                                    for (x = 0; x <= phasorCount - 1; x++)
                                    {
                                        pmuCell.PhasorDefinitions.Add(new PhasorDefinition(pmuCell, x + 1, m_iniFile[section, "Phasor" + (x + 1), DefaultVoltagePhasorEntry]));
                                    }

                                    pmuCell.FrequencyDefinition = new FrequencyDefinition(pmuCell, m_iniFile[section, "Frequency", DefaultFrequencyEntry]);

                                    m_configurationFileCells.Add(pmuCell);
                                }
                            }
                        }

                        // Associate parsed cells with cells defined in INI file
                        if (m_configurationFileCells.Count > 0 && (Cells != null))
                        {
                            ConfigurationCell configurationFileCell = null;
                            IConfigurationCell configurationCell = null;

                            if (refreshCausedByFrameParse)
                            {
                                // Create a new configuration cell collection that will account for PDC block cells
                                ConfigurationCellCollection cellCollection = new ConfigurationCellCollection();
                                ConfigurationCell cell;

                                // For freshly parsed configuration frames we'll have no PMU's in configuration
                                // frame for PDCxchng blocks - so we'll need to dynamically create them
                                for (x = 0; x <= Cells.Count - 1; x++)
                                {
                                    // Get current configuration cell
                                    cell = Cells[x];

                                    // Lookup INI file configuration cell by ID label
                                    m_configurationFileCells.TryGetByIDLabel(cell.IDLabel, ref configurationCell);
                                    configurationFileCell = (ConfigurationCell)configurationCell;

                                    if (configurationFileCell == null)
                                    {
                                        // Couldn't find associated INI file cell - just append the parsed cell to the collection
                                        cellCollection.Add(cell);
                                    }
                                    else
                                    {
                                        if (configurationFileCell.IsPDCBlockSection)
                                        {
                                            // This looks like a PDC block section - so we'll keep adding cells for each defined PMU in the PDC block
                                            int index = 0;

                                            do
                                            {
                                                // Lookup PMU by section name
                                                m_configurationFileCells.TryGetBySectionEntry(cell.IDLabel + "pmu" + index, ref configurationFileCell);

                                                // Add PDC block PMU configuration cell to the collection
                                                if (configurationFileCell != null)
                                                {
                                                    cellCollection.Add(configurationFileCell);
                                                }
                                                index++;
                                            } while (!(configurationFileCell == null));
                                        }
                                        else
                                        {
                                            // As far as we can tell from INI file, this is just a regular PMU
                                            cell.ConfigurationFileCell = configurationFileCell;
                                            cellCollection.Add(cell);
                                        }
                                    }
                                }

                                // Assign "new" cell collection which will include PMU's from defined PDC blocks
                                Cells.Clear();
                                Cells.AddRange(cellCollection);
                            }
                            else
                            {
                                // For simple INI file updates, we just re-assign INI file cells associating by section entry
                                foreach (ConfigurationCell cell in Cells)
                                {
                                    // Attempt to associate this configuration cell with information read from external INI based configuration file
                                    m_configurationFileCells.TryGetBySectionEntry(cell.SectionEntry, ref configurationFileCell);
                                    cell.ConfigurationFileCell = configurationFileCell;
                                }
                            }
                        }
                    }
                    else
                    {
                        throw (new InvalidOperationException("PDC config file \"" + m_iniFile.FileName + "\" does not exist."));
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    m_readWriteLock.ReleaseWriterLock();
                }

                // In case other classes want to know, we send out a notification that the config file has been reloaded (make sure
                // you do this after the write lock has been released to avoid possible dead-lock situations)
                if (ConfigFileReloaded != null)
                    ConfigFileReloaded();

            }

            public short PacketsPerSample
            {
                get
                {
                    return m_packetsPerSample;
                }
                set
                {
                    m_packetsPerSample = value;
                }
            }

            public string ConfigurationFileName
            {
                get
                {
                    m_readWriteLock.AcquireReaderLock(-1);

                    try
                    {
                        return m_iniFile.FileName;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        m_readWriteLock.ReleaseReaderLock();
                    }
                }
            }

            public PhasorDefinition DefaultPhasorV
            {
                get
                {
                    m_readWriteLock.AcquireReaderLock(-1);

                    try
                    {
                        return m_defaultPhasorV;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        m_readWriteLock.ReleaseReaderLock();
                    }
                }
            }

            public PhasorDefinition DefaultPhasorI
            {
                get
                {
                    m_readWriteLock.AcquireReaderLock(-1);

                    try
                    {
                        return m_defaultPhasorI;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        m_readWriteLock.ReleaseReaderLock();
                    }
                }
            }

            public FrequencyDefinition DefaultFrequency
            {
                get
                {
                    m_readWriteLock.AcquireReaderLock(-1);

                    try
                    {
                        return m_defaultFrequency;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        m_readWriteLock.ReleaseReaderLock();
                    }
                }
            }

            public string IniFileImage
            {
                get
                {
                    m_readWriteLock.AcquireReaderLock(-1);

                    try
                    {
                        System.Text.StringBuilder fileImage = new StringBuilder();
                        fileImage.Append("; File - " + m_iniFile.FileName + Environment.NewLine);
                        fileImage.Append("; Auto-generated on " + DateTime.Now + Environment.NewLine);
                        fileImage.Append(";    Assembly: " + AssemblyInfo.ExecutingAssembly.Name + Environment.NewLine);
                        fileImage.Append(";    Compiled: " + File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location) + Environment.NewLine);
                        fileImage.Append(";" + Environment.NewLine);
                        fileImage.Append(";" + Environment.NewLine);
                        fileImage.Append("; Format:" + Environment.NewLine);
                        fileImage.Append(";   Each Column in data file is given a bracketed identifier, numbered in the order it" + Environment.NewLine);
                        fileImage.Append(";   appears in the data file, and identified by data type ( PMU, PDC, or other)" + Environment.NewLine);
                        fileImage.Append(";     PMU designates column data format from a single PMU" + Environment.NewLine);
                        fileImage.Append(";     PDC designates column data format from another PDC which is somewhat different from a single PMU" + Environment.NewLine);
                        fileImage.Append(";   Default gives default values for a processing algorithm in case quantities are omitted" + Environment.NewLine);
                        fileImage.Append(";   Name= gives the overall station name for print labels" + Environment.NewLine);
                        fileImage.Append(";   NumberPhasors= :  for PMU data, gives the number of phasors contained in column" + Environment.NewLine);
                        fileImage.Append(";                     for PDC data, gives the number of PMUs data included in the column" + Environment.NewLine);
                        fileImage.Append(";                     Note - for PDC data, there will be 2 phasors & 1 freq per PMU" + Environment.NewLine);
                        fileImage.Append(";   Quantities within the column are listed by PhasorI=, Frequency=, etc" + Environment.NewLine);
                        fileImage.Append(";   Each quantity has 7 comma separated fields followed by an optional comment" + Environment.NewLine);
                        fileImage.Append(";" + Environment.NewLine);
                        fileImage.Append(";   Phasor entry format:  Type, Ratio, Cal Factor, Offset, Shunt, VoltageRef/Class, Label  ;Comments" + Environment.NewLine);
                        fileImage.Append(";    Type:       Type of measurement, V=voltage, I=current, N=don\'t care, single ASCII character" + Environment.NewLine);
                        fileImage.Append(";    Ratio:      PT/CT ratio N:1 where N is a floating point number" + Environment.NewLine);
                        fileImage.Append(";    Cal Factor: Conversion factor between integer in file and secondary volts, floating point" + Environment.NewLine);
                        fileImage.Append(";    Offset:     Phase Offset to correct for phase angle measurement errors or differences, floating point" + Environment.NewLine);
                        fileImage.Append(";    Shunt:      Current- shunt resistence in ohms, or the equivalent ratio for aux CTs, floating point" + Environment.NewLine);
                        fileImage.Append(";                Voltage- empty, not used" + Environment.NewLine);
                        fileImage.Append(";    VoltageRef: Current- phasor number (1-10) of voltage phasor to use for power calculation, integer" + Environment.NewLine);
                        fileImage.Append(";                Voltage- voltage class, standard l-l voltages, 500, 230, 115, etc, integer" + Environment.NewLine);
                        fileImage.Append(";    Label:      Phasor quantity label for print label, text" + Environment.NewLine);
                        fileImage.Append(";    Comments:   All text after the semicolon on a line are optional comments not for processing" + Environment.NewLine);
                        fileImage.Append(";" + Environment.NewLine);
                        fileImage.Append(";   Voltage Magnitude = MAG(Real,Imaginary) * CalFactor * PTR (line-neutral)" + Environment.NewLine);
                        fileImage.Append(";   Current Magnitude = MAG(Real,Imaginary) * CalFactor * CTR / Shunt (phase current)" + Environment.NewLine);
                        fileImage.Append(";   Phase Angle = ATAN(Imaginary/Real) + Phase Offset (usually degrees)" + Environment.NewLine);
                        fileImage.Append(";     Note: Usually phase Offset is 0, but is sometimes required for comparing measurements" + Environment.NewLine);
                        fileImage.Append(";           from different systems or through transformer banks" + Environment.NewLine);
                        fileImage.Append(";" + Environment.NewLine);
                        fileImage.Append(";   Frequency entry format:  scale, offset, dF/dt scale, dF/dt offset, dummy, label  ;Comments" + Environment.NewLine);
                        fileImage.Append(";   Frequency = Number / scale + offset" + Environment.NewLine);
                        fileImage.Append(";   dF/dt = Number / (dF/dt scale) + (dF/dt offset)" + Environment.NewLine);
                        fileImage.Append(";" + Environment.NewLine);
                        fileImage.Append(";" + Environment.NewLine);

                        fileImage.Append("[DEFAULT]" + Environment.NewLine);
                        fileImage.Append("PhasorV=" + PhasorDefinition.ConfigFileFormat(DefaultPhasorV) + Environment.NewLine);
                        fileImage.Append("PhasorI=" + PhasorDefinition.ConfigFileFormat(DefaultPhasorI) + Environment.NewLine);
                        fileImage.Append("Frequency=" + FrequencyDefinition.ConfigFileFormat(DefaultFrequency) + Environment.NewLine);
                        fileImage.AppendLine();

                        fileImage.Append("[CONFIG]" + Environment.NewLine);
                        fileImage.Append("SampleRate=" + FrameRate + Environment.NewLine);
                        fileImage.Append("NumberOfPMUs=" + Cells.Count + Environment.NewLine);
                        fileImage.AppendLine();

                        for (int x = 0; x <= Cells.Count - 1; x++)
                        {
                            fileImage.Append("[" + Cells[x].IDLabel + "]" + Environment.NewLine);
                            fileImage.Append("Name=" + Cells[x].StationName + Environment.NewLine);
                            fileImage.Append("PMU=" + x + Environment.NewLine);
                            fileImage.Append("NumberPhasors=" + Cells[x].PhasorDefinitions.Count + Environment.NewLine);
                            for (int y = 0; y <= Cells[x].PhasorDefinitions.Count - 1; y++)
                            {
                                fileImage.Append("Phasor" + (y + 1) + "=" + PhasorDefinition.ConfigFileFormat(Cells[x].PhasorDefinitions[y]) + Environment.NewLine);
                            }
                            fileImage.Append("Frequency=" + FrequencyDefinition.ConfigFileFormat(Cells[x].FrequencyDefinition) + Environment.NewLine);
                            fileImage.AppendLine();
                        }

                        return fileImage.ToString();
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        m_readWriteLock.ReleaseReaderLock();
                    }
                }
            }

            // RowLength property calculates cell offsets - so it must be called before
            // accessing cell offsets - this happens automatically since HeaderImage is
            // called before base class BodyImage which just gets Cells.BinaryImage
            public ushort RowLength(bool recalculate)
            {
                if (m_rowLength == 0 || recalculate)
                {
                    m_rowLength = 0;

                    for (int x = 0; x <= Cells.Count - 1; x++)
                    {
                        ConfigurationCell cell = Cells[x];

                        cell.Offset = m_rowLength;

                        m_rowLength += (ushort)(12 + FrequencyValue.CalculateBinaryLength(cell.FrequencyDefinition));

                        for (int y = 0; y <= cell.PhasorDefinitions.Count - 1; y++)
                        {
                            m_rowLength += (ushort)PhasorValue.CalculateBinaryLength(cell.PhasorDefinitions[y]);
                        }
                    }
                }

                return m_rowLength;
            }

            [CLSCompliant(false)]
            protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
            {
                // PDCstream uses a 16-bit XOR based check sum
                return buffer.Xor16CheckSum(offset, length);
            }

            protected override void AppendChecksum(byte[] buffer, int startIndex)
            {
                // Oddly enough, check sum for frames in BPA PDC stream is little-endian
                EndianOrder.LittleEndian.CopyBytes(CalculateChecksum(buffer, 0, startIndex), buffer, startIndex);
            }

            protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
            {
                int sumLength = (int)(BinaryLength - 2);
                return EndianOrder.LittleEndian.ToUInt16(buffer, startIndex + sumLength) == CalculateChecksum(buffer, startIndex, sumLength);

            }

            protected override ushort HeaderLength
            {
                get
                {
                    return 16;
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    byte[] buffer = new byte[HeaderLength];

                    // Common in common frame header portion of header image
                    System.Buffer.BlockCopy(CommonFrameHeader.BinaryImage(this), 0, buffer, 0, CommonFrameHeader.BinaryLength);

                    buffer[4] = (byte)StreamType;
                    buffer[5] = (byte)RevisionNumber;
                    EndianOrder.BigEndian.CopyBytes(FrameRate, buffer, 6);
                    EndianOrder.BigEndian.CopyBytes(RowLength(true), buffer, 8); // <-- Important: This step calculates all PMU row offsets!
                    EndianOrder.BigEndian.CopyBytes(PacketsPerSample, buffer, 12);
                    EndianOrder.BigEndian.CopyBytes((short)Cells.Count, buffer, 14);

                    return buffer;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                // We parse the PDC stream specific header image here...
                IConfigurationFrameParsingState parsingState = (IConfigurationFrameParsingState)state;

                // Only need to parse what wan't already parsed in common frame header
                StreamType = (StreamType)binaryImage[startIndex + 4];
                RevisionNumber = (RevisionNumber)binaryImage[startIndex + 5];
                FrameRate = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6);
                m_rowLength = (ushort)EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 8);
                PacketsPerSample = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 12);

                parsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 14);

                // The data that's in the data stream will take precedence over what's in the
                // in the configuration file.  The configuration file may define more PMU's than
                // are in the stream - in my opinion that's OK - it's when you have PMU's in the
                // stream that aren't defined in the INI file that you'll have trouble...

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize configuration frame
                info.AddValue("packetsPerSample", m_packetsPerSample);
                info.AddValue("streamType", m_streamType, typeof(StreamType));
                info.AddValue("revisionNumber", m_revisionNumber, typeof(RevisionNumber));
                info.AddValue("configurationFileName", m_iniFile.FileName);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    if (m_iniFile != null)
                    {
                        baseAttributes.Add("Configuration File Name", m_iniFile.FileName);
                    }
                    baseAttributes.Add("Packet Number", Common.DescriptorPacketFlag.ToString());
                    baseAttributes.Add("Stream Type", (int)m_streamType + ": " + m_streamType);
                    baseAttributes.Add("Revision Number", (int)m_revisionNumber + ": " + m_revisionNumber);
                    baseAttributes.Add("Packets Per Sample", m_packetsPerSample.ToString());

                    return baseAttributes;
                }
            }

        }

    }
}
