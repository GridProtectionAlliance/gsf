//******************************************************************************************************
//  ControlFile.cs - Gbtc
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
//  08/01/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GSF.EMAX
{
    /// <summary>
    /// Represents an EMAX control file (i.e., a .CTL file).
    /// </summary>
    public class ControlFile
    {
        #region [ Members ]

        // Fields
        public string FileName;
        public DataSize DataSize;
        public CTL_HEADER Header;
        public CTL_FILE_STRUCT[] FileStructures;
        public SYSTEM_PARAMETERS SystemParameters;
        public SYS_SETTINGS SystemSettings;
        public ANALOG_GROUP AnalogGroup;
        public EVENT_GROUP EventGroup;
        public A_E_RSLTS AnalogEventResults;
        public IDENTSTRING IdentityString;
        public Dictionary<int, ANLG_CHNL_NEW> AnalogChannelSettings;
        public Dictionary<int, EVNT_CHNL_NEW> EventChannelSettings;
        public Dictionary<int, double> ScalingFactors;
        public EVENT_DISPLAY EventDisplay;
        public SENS_RSLTS SensorResults;
        public TPwrRcd PowerRecord;
        public BoardAnalogEventChannels BoardAnalogEventChannels;
        public A_SELECTION AnalogSelection;
        public E_GRP_SELECT EventGroupSelection;
        public PHASOR_GROUPS PhasorGroups;
        public LINE_CONSTANTS LineConstants;
        public LINE_NAMES LineNames;
        public FAULT_LOCATIONS FaultLocations;
        public SEQUENCE_CHANNELS SequenceChannels;
        public BREAKER_TRIP_TIMES BreakerTripTimes;

        private int m_configuredAnalogChannels;
        private readonly List<StructureType> m_parsedSuccesses;
        private readonly List<Tuple<StructureType, Exception>> m_parsedFailures;
        private StructureType m_currentType;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ControlFile"/>.
        /// </summary>
        public ControlFile()
        {
            m_parsedSuccesses = new List<StructureType>();
            m_parsedFailures = new List<Tuple<StructureType, Exception>>();
        }

        /// <summary>
        /// Creates a new <see cref="ControlFile"/> for the specified <paramref name="fileName"/> and attempts to parse.
        /// </summary>
        /// <param name="fileName">Control file name.</param>
        public ControlFile(string fileName)
            : this()
        {
            FileName = fileName;
            Parse();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets configured analog channels.
        /// </summary>
        public int ConfiguredAnalogChannels
        {
            get
            {
                return m_configuredAnalogChannels;
            }
        }

        /// <summary>
        /// Gets the digital channel count for the <see cref="ControlFile"/>.
        /// </summary>
        public int ConfiguredDigitalChannels
        {
            get
            {
                return EventGroupCount * 8;
            }
        }

        /// <summary>
        /// Gets the analog channel count for the <see cref="ControlFile"/>.
        /// </summary>
        public int AnalogChannelCount
        {
            get
            {
                ushort samplesPerSecond = SystemParameters.samples_per_second;
                return m_configuredAnalogChannels * (samplesPerSecond > 5760 ? samplesPerSecond / 5760 : 1);
            }
        }

        /// <summary>
        /// Gets the event group count for the <see cref="ControlFile"/>.
        /// </summary>
        public int EventGroupCount
        {
            get
            {
                if (SystemParameters.samples_per_second > 5760)
                    return 4;

                return 4 * (m_configuredAnalogChannels > 32 ? 2 : 1);
            }
        }

        /// <summary>
        /// Gets <see cref="StructureType"/> instances that were parsed successfully.
        /// </summary>
        public IEnumerable<StructureType> ParsedSuccesses
        {
            get
            {
                return m_parsedSuccesses;
            }
        }

        /// <summary>
        /// Gets <see cref="StructureType"/> instances that failed to parse.
        /// </summary>
        public IEnumerable<Tuple<StructureType, Exception>> ParsedFailures
        {
            get
            {
                return m_parsedFailures;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the <see cref="ControlFile"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">"No EMAX control file name was specified.</exception>
        /// <exception cref="FileNotFoundException">EMAX control file was not found.</exception>
        public void Parse()
        {
            if (string.IsNullOrEmpty(FileName))
                throw new InvalidOperationException("No EMAX control file name was specified.");

            if (!File.Exists(FileName))
                throw new FileNotFoundException(string.Format("EMAX control file {0} not found.", FileName));

            m_parsedSuccesses.Clear();
            m_parsedFailures.Clear();

            byte byteValue;

            using (FileStream stream = File.OpenRead(FileName))
            {
                // Read in header and file structure definitions
                using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true))
                {
                    // Read control header
                    Header = reader.ReadStructure<CTL_HEADER>();

                    // Read byte that defines number of analog channels
                    m_configuredAnalogChannels = BinaryCodedDecimal.Decode(Header.id.LowByte());

                    // Read byte that defines data size (i.e., 12 or 16 bits)
                    byteValue = Header.id.HighByte();

                    if (!Enum.IsDefined(typeof(DataSize), byteValue))
                        throw new InvalidOperationException("Invalid EMAX data size code encountered: 0x" + byteValue.ToString("X").PadLeft(2, '0'));

                    DataSize = (DataSize)byteValue;

                    // Create array of file structures
                    List<CTL_FILE_STRUCT> fileStructures = new List<CTL_FILE_STRUCT>();
                    CTL_FILE_STRUCT fileStructure = new CTL_FILE_STRUCT(reader);

                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                    while (fileStructure.type != StructureType.EndOfStructures)
                    {
                        if (fileStructure.type != StructureType.Unknown)
                            fileStructures.Add(fileStructure);

                        fileStructure = new CTL_FILE_STRUCT(reader);
                    }

                    FileStructures = fileStructures.ToArray();
                }

                // Read in actual file structures
                for (int index = 0; index < FileStructures.Length; index++)
                {
                    CTL_FILE_STRUCT fileStructure = FileStructures[index];

                    if (fileStructure.type == StructureType.Unknown)
                        continue;

                    // Set current type
                    m_currentType = fileStructure.type;

                    // Locate structure in the file
                    stream.Position = fileStructure.offset;

                    // Parse the structure type
                    using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true))
                    {
                        switch (m_currentType)
                        {
                            case StructureType.SYSTEM_PARAMETERS:
                                AttemptParse(() => SystemParameters = reader.ReadStructure<SYSTEM_PARAMETERS>());
                                break;
                            case StructureType.SYS_SETTINGS:
                                AttemptParse(() => SystemSettings = reader.ReadStructure<SYS_SETTINGS>());
                                break;
                            case StructureType.A_E_RSLTS:
                                AttemptParse(() => AnalogEventResults = new A_E_RSLTS(reader, m_configuredAnalogChannels, SystemParameters.analog_groups));
                                break;
                            case StructureType.ANALOG_GROUP:
                                AttemptParse(() => AnalogGroup = new ANALOG_GROUP(reader, m_configuredAnalogChannels));
                                break;
                            case StructureType.EVENT_GROUP:
                                AttemptParse(() => EventGroup = reader.ReadStructure<EVENT_GROUP>());
                                break;
                            case StructureType.ANLG_CHNL_NEW:
                                AttemptParse(() =>
                                {
                                    AnalogChannelSettings = new Dictionary<int, ANLG_CHNL_NEW>();
                                    ScalingFactors = new Dictionary<int, double>();
                                    ANLG_CHNL_NEW settings;

                                    uint nextOffset = (index + 1 < FileStructures.Length) ? FileStructures[index + 1].offset : (uint)stream.Length;
                                    uint length = nextOffset - fileStructure.offset;
                                    Func<ANLG_CHNL_NEW> channelFactory;

                                    if (Marshal.SizeOf<ANLG_CHNL_NEW2>() * ConfiguredAnalogChannels <= length)
                                        channelFactory = () => reader.ReadStructure<ANLG_CHNL_NEW2>().ToAnlgChnlNew();
                                    else
                                        channelFactory = () => reader.ReadStructure<ANLG_CHNL_NEW1>().ToAnlgChnlNew();

                                    for (int i = 0; i < ConfiguredAnalogChannels; i++)
                                    {
                                        settings = channelFactory();
                                        AnalogChannelSettings.Add(settings.ChannelNumber, settings);
                                        ScalingFactors.Add(settings.ChannelNumber, settings.ScalingFactor);
                                    }
                                });
                                break;
                            case StructureType.EVNT_CHNL_NEW:
                                AttemptParse(() =>
                                {
                                    EventChannelSettings = new Dictionary<int, EVNT_CHNL_NEW>();
                                    EVNT_CHNL_NEW settings;

                                    uint nextOffset = (index + 1 < FileStructures.Length) ? FileStructures[index + 1].offset : (uint)stream.Length;
                                    uint length = nextOffset - fileStructure.offset;
                                    Func<EVNT_CHNL_NEW> channelFactory;

                                    if (Marshal.SizeOf<EVNT_CHNL_NEW2>() * ConfiguredDigitalChannels <= length)
                                        channelFactory = () => reader.ReadStructure<EVNT_CHNL_NEW2>().ToEvntChnlNew();
                                    else
                                        channelFactory = () => reader.ReadStructure<EVNT_CHNL_NEW1>().ToEvntChnlNew();

                                    for (int i = 0; i < ConfiguredDigitalChannels; i++)
                                    {
                                        settings = channelFactory();
                                        EventChannelSettings.Add(settings.EventNumber, settings);
                                    }
                                });
                                break;
                            case StructureType.ANLG_CHNLS:
                                // TODO: Add decoder once structure definition is known...
                                m_parsedFailures.Add(new Tuple<StructureType, Exception>(m_currentType, new NotImplementedException()));
                                break;
                            case StructureType.SHORT_HEADER:
                                // TODO: Add decoder once structure definition is known...
                                m_parsedFailures.Add(new Tuple<StructureType, Exception>(m_currentType, new NotImplementedException()));
                                break;
                            case StructureType.FAULT_HEADER:
                                // TODO: Add decoder once structure definition is known...
                                m_parsedFailures.Add(new Tuple<StructureType, Exception>(m_currentType, new NotImplementedException()));
                                break;
                            case StructureType.EVENT_DISPLAY:
                                AttemptParse(() => EventDisplay = new EVENT_DISPLAY(reader, SystemParameters.event_groups));
                                break;
                            case StructureType.IDENTSTRING:
                                AttemptParse(() =>
                                {
                                    IdentityString = reader.ReadStructure<IDENTSTRING>();
                                    IdentityString.value.TruncateRight(IdentityString.length);
                                });
                                break;
                            case StructureType.A_SELECTION:
                                AttemptParse(() => AnalogSelection = new A_SELECTION(reader));
                                break;
                            case StructureType.E_GROUP_SELECT:
                                AttemptParse(() => EventGroupSelection = new E_GRP_SELECT(reader));
                                break;
                            case StructureType.PHASOR_GROUP:
                                AttemptParse(() => PhasorGroups = new PHASOR_GROUPS(reader));
                                break;
                            case StructureType.LINE_CONSTANTS:
                                AttemptParse(() => LineConstants = new LINE_CONSTANTS(reader));
                                break;
                            case StructureType.LINE_NAMES:
                                AttemptParse(() => LineNames = new LINE_NAMES(reader));
                                break;
                            case StructureType.FAULT_LOCATION:
                                AttemptParse(() => FaultLocations = new FAULT_LOCATIONS(reader));
                                break;
                            case StructureType.SENS_RSLTS:
                                AttemptParse(() => SensorResults = reader.ReadStructure<SENS_RSLTS>());
                                break;
                            case StructureType.SEQUENCE_CHANNELS:
                                AttemptParse(() => SequenceChannels = new SEQUENCE_CHANNELS(reader));
                                break;
                            case StructureType.TPwrRcd:
                                AttemptParse(() => PowerRecord = reader.ReadStructure<TPwrRcd>());
                                break;
                            case StructureType.BoardAnalogEventChannels:
                                AttemptParse(() => BoardAnalogEventChannels = reader.ReadStructure<BoardAnalogEventChannels>());
                                break;
                            case StructureType.BREAKER_TRIP_TIMES:
                                AttemptParse(() => BreakerTripTimes = new BREAKER_TRIP_TIMES(reader, m_configuredAnalogChannels, SystemParameters.analog_groups));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        private void AttemptParse(Action parseAction)
        {
            try
            {
                parseAction();
                m_parsedSuccesses.Add(m_currentType);
            }
            catch (Exception ex)
            {
                m_parsedFailures.Add(new Tuple<StructureType, Exception>(m_currentType, ex));
            }
        }

        #endregion

        #region [ Old Code ]

        //private readonly StructureType[] StructureTypeProcessingOrder = 
        //{
        //    StructureType.SYSTEM_PARAMETERS,
        //    StructureType.SYS_SETTINGS,
        //    StructureType.A_E_RSLTS,
        //    StructureType.ANALOG_GROUP,
        //    StructureType.EVENT_GROUP,
        //    StructureType.ANLG_CHNL_NEW,
        //    StructureType.EVNT_CHNL_NEW,
        //    StructureType.ANLG_CHNLS,
        //    StructureType.SHORT_HEADER,
        //    StructureType.FAULT_HEADER,
        //    StructureType.EVENT_DISPLAY,
        //    StructureType.IDENTSTRING,
        //    StructureType.A_SELECTION,
        //    StructureType.E_GROUP_SELECT,
        //    StructureType.PHASOR_GROUP,
        //    StructureType.LINE_CONSTANTS,
        //    StructureType.LINE_NAMES,
        //    StructureType.FAULT_LOCATION,
        //    StructureType.SENS_RSLTS,
        //    StructureType.SEQUENCE_CHANNELS,
        //    StructureType.TPwrRcd,
        //    StructureType.BoardAnalogEventChannels,
        //    StructureType.Unknown,
        //    StructureType.EndOfStructures
        //};

        //Debug.Assert(StructureTypeProcessingOrder.Length == Enum.GetValues(typeof(StructureType)).Length, "StructureTypeProcessOrder is not the same length as StructureType enumeration!");

        //// Sort file structures in desired processing order
        //Func<StructureType, int> getProcessingOrder = structureType => Array.IndexOf(StructureTypeProcessingOrder, structureType);

        //fileStructures.Sort((x, y) =>
        //{
        //    int indexX = getProcessingOrder(x.type);
        //    int indexY = getProcessingOrder(y.type);
        //    return indexX < indexY ? -1 : indexX > indexY ? 1 : 0;
        //});

        #endregion
    }
}