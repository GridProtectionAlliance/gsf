using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
//using PhasorProtocols.BpaPdcStream.Common;

//*******************************************************************************************************
//  DataCell.vb - PDCstream PMU Data Cell
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


namespace PCS.PhasorProtocols
{
    namespace BpaPdcStream
    {

        // This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
        [CLSCompliant(false), Serializable()]
        public class DataCell : DataCellBase
        {



            private ChannelFlags m_flags;
            private ReservedFlags m_reservedFlags;
            private short m_sampleNumber;
            private byte m_dataRate;
            private byte m_pdcBlockPmuCount;
            private bool m_isPdcBlockPmu;
            private bool m_isPdcBlockHeader;
            private int m_pdcBlockLength;

            protected DataCell()
            {
            }

            protected DataCell(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize data cell
                m_flags = (ChannelFlags)info.GetValue("flags", typeof(ChannelFlags));
                m_reservedFlags = (ReservedFlags)info.GetValue("reservedFlags", typeof(ReservedFlags));
                m_sampleNumber = info.GetInt16("sampleNumber");

            }

            public DataCell(IDataFrame parent, IConfigurationCell configurationCell, short sampleNumber)
                : base(parent, true, configurationCell, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
            {


                int x;

                m_sampleNumber = sampleNumber;

                // Initialize phasor values and frequency value with an empty value
                for (x = 0; x < ConfigurationCell.PhasorDefinitions.Count; x++)
                {
                    PhasorValues.Add(new PhasorValue(this, ConfigurationCell.PhasorDefinitions[x], float.NaN, float.NaN));
                }

                // Initialize frequency and df/dt
                FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition, float.NaN, float.NaN);

                // Initialize analog values
                for (x = 0; x < ConfigurationCell.AnalogDefinitions.Count; x++)
                {
                    AnalogValues.Add(new AnalogValue(this, ConfigurationCell.AnalogDefinitions[x], float.NaN));
                }

                // Initialize any digital values
                for (x = 0; x < ConfigurationCell.DigitalDefinitions.Count; x++)
                {
                    DigitalValues.Add(new DigitalValue(this, ConfigurationCell.DigitalDefinitions[x], -1));
                }

            }

            public DataCell(IDataCell dataCell)
                : base(dataCell)
            {


            }

            // This constructor satisfies ChannelCellBase class requirement:
            //   ' Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)
            public DataCell(IDataFrame parent, DataFrameParsingState state, int index, byte[] binaryImage, int startIndex)
                : base(parent, true, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, new DataCellParsingState(state.ConfigurationFrame.Cells[index], BpaPdcStream.PhasorValue.CreateNewPhasorValue, BpaPdcStream.FrequencyValue.CreateNewFrequencyValue, BpaPdcStream.AnalogValue.CreateNewAnalogValue, BpaPdcStream.DigitalValue.CreateNewDigitalValue, index), binaryImage, startIndex)
            {


            }

            // This overload allows construction of PMU's that exist within a PDCxchng block
            public DataCell(IDataFrame parent, ConfigurationCell configurationCell, byte[] binaryImage, int startIndex)
                : base(parent, false, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, new DataCellParsingState(configurationCell, BpaPdcStream.PhasorValue.CreateNewPhasorValue, BpaPdcStream.FrequencyValue.CreateNewFrequencyValue, BpaPdcStream.AnalogValue.CreateNewAnalogValue, BpaPdcStream.DigitalValue.CreateNewDigitalValue, true), binaryImage, startIndex)
            {


            }

            internal static IDataCell CreateNewDataCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] binaryImage, int startIndex)
            {

                return new DataCell((IDataFrame)parent, (DataFrameParsingState)state, index, binaryImage, startIndex);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataFrame Parent
            {
                get
                {
                    return (DataFrame)base.Parent;
                }
            }

            public new ConfigurationCell ConfigurationCell
            {
                get
                {
                    return (ConfigurationCell)base.ConfigurationCell;
                }
                set
                {
                    base.ConfigurationCell = value;
                }
            }

            // Note: this is only the first byte of the channel flag word
            public ChannelFlags ChannelFlags
            {
                get
                {
                    return m_flags;
                }
                set
                {
                    m_flags = value;
                }
            }

            public ReservedFlags ReservedFlags
            {
                get
                {
                    return m_reservedFlags;
                }
                set
                {
                    m_reservedFlags = value;
                }
            }

            public byte DataRate
            {
                get
                {
                    if (Parent.ConfigurationFrame.RevisionNumber >= RevisionNumber.Revision2)
                    {
                        return (byte)Parent.ConfigurationFrame.FrameRate;
                    }
                    else
                    {
                        return m_dataRate;
                    }
                }
                set
                {
                    m_dataRate = value;
                }
            }

            public IEEEFormatFlags IEEEFormatFlags
            {
                get
                {
                    return ConfigurationCell.IEEEFormatFlags;
                }
                set
                {
                    ConfigurationCell.IEEEFormatFlags = value;
                }
            }

            public short SampleNumber
            {
                get
                {
                    return m_sampleNumber;
                }
                set
                {
                    m_sampleNumber = value;
                }
            }

            // These properties make it easier to manage channel flags
            public bool ReservedFlag0IsSet
            {
                get
                {
                    return ((m_reservedFlags & ReservedFlags.Reserved0) > 0);
                }
                set
                {
                    if (value)
                    {
                        m_reservedFlags = m_reservedFlags | ReservedFlags.Reserved0;
                    }
                    else
                    {
                        m_reservedFlags = m_reservedFlags & ~ReservedFlags.Reserved0;
                    }
                }
            }

            public bool ReservedFlag1IsSet
            {
                get
                {
                    return ((m_reservedFlags & ReservedFlags.Reserved1) > 0);
                }
                set
                {
                    if (value)
                    {
                        m_reservedFlags = m_reservedFlags | ReservedFlags.Reserved1;
                    }
                    else
                    {
                        m_reservedFlags = m_reservedFlags & ~ReservedFlags.Reserved1;
                    }
                }
            }

            public override bool DataIsValid
            {
                get
                {
                    return ((m_flags & ChannelFlags.DataIsValid) == 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags & ~ChannelFlags.DataIsValid;
                    }
                    else
                    {
                        m_flags = m_flags | ChannelFlags.DataIsValid;
                    }
                }
            }

            public override bool SynchronizationIsValid
            {
                get
                {
                    return ((m_flags & ChannelFlags.PMUSynchronized) == 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags & ~ChannelFlags.PMUSynchronized;
                    }
                    else
                    {
                        m_flags = m_flags | ChannelFlags.PMUSynchronized;
                    }
                }
            }

            public override DataSortingType DataSortingType
            {
                get
                {
                    return (((m_flags & ChannelFlags.DataSortedByArrival) > 0) ? PhasorProtocols.DataSortingType.ByArrival : PhasorProtocols.DataSortingType.ByTimestamp);
                }
                set
                {
                    if (value == PhasorProtocols.DataSortingType.ByArrival)
                    {
                        m_flags = m_flags | ChannelFlags.DataSortedByArrival;
                    }
                    else
                    {
                        m_flags = m_flags & ~ChannelFlags.DataSortedByArrival;
                    }
                }
            }

            public override bool PmuError
            {
                get
                {
                    return ((m_flags & ChannelFlags.TransmissionErrors) > 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags | ChannelFlags.TransmissionErrors;
                    }
                    else
                    {
                        m_flags = m_flags & ~ChannelFlags.TransmissionErrors;
                    }
                }
            }

            //Public Property TransmissionErrors() As Boolean
            //    Get
            //        Return ((m_flags And ChannelFlags.TransmissionErrors) > 0)
            //    End Get
            //    Set(ByVal value As Boolean)
            //        If value Then
            //            m_flags = m_flags Or ChannelFlags.TransmissionErrors
            //        Else
            //            m_flags = m_flags And Not ChannelFlags.TransmissionErrors
            //        End If
            //    End Set
            //End Property

            //Public Property DataIsSortedByArrival() As Boolean
            //    Get
            //        Return ((m_flags And ChannelFlags.DataSortedByArrival) > 0)
            //    End Get
            //    Set(ByVal value As Boolean)
            //        If value Then
            //            m_flags = m_flags Or ChannelFlags.DataSortedByArrival
            //        Else
            //            m_flags = m_flags And Not ChannelFlags.DataSortedByArrival
            //        End If
            //    End Set
            //End Property

            public bool UsingPDCExchangeFormat
            {
                get
                {
                    return ((m_flags & ChannelFlags.PDCExchangeFormat) > 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags | ChannelFlags.PDCExchangeFormat;
                    }
                    else
                    {
                        m_flags = m_flags & ~ChannelFlags.PDCExchangeFormat;
                    }
                }
            }

            public bool UsingMacrodyneFormat
            {
                get
                {
                    return ((m_flags & ChannelFlags.MacrodyneFormat) > 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags | ChannelFlags.MacrodyneFormat;
                    }
                    else
                    {
                        m_flags = m_flags & ~ChannelFlags.MacrodyneFormat;
                    }
                }
            }

            public bool UsingIEEEFormat
            {
                get
                {
                    return ((m_flags & ChannelFlags.MacrodyneFormat) == 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags & ~ChannelFlags.MacrodyneFormat;
                    }
                    else
                    {
                        m_flags = m_flags | ChannelFlags.MacrodyneFormat;
                    }
                }
            }

            [Obsolete("This bit definition is for obsolete uses that is no longer needed.", false)]
            public bool DataIsSortedByTimestamp
            {
                get
                {
                    return ((m_flags & ChannelFlags.DataSortedByTimestamp) == 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags & ~ChannelFlags.DataSortedByTimestamp;
                    }
                    else
                    {
                        m_flags = m_flags | ChannelFlags.DataSortedByTimestamp;
                    }
                }
            }

            [Obsolete("This bit definition is for obsolete uses that is no longer needed.", false)]
            public bool TimestampIsIncluded
            {
                get
                {
                    return ((m_flags & ChannelFlags.TimestampIncluded) == 0);
                }
                set
                {
                    if (value)
                    {
                        m_flags = m_flags & ~ChannelFlags.TimestampIncluded;
                    }
                    else
                    {
                        m_flags = m_flags | ChannelFlags.TimestampIncluded;
                    }
                }
            }

            public bool IsPdcBlockPmu
            {
                get
                {
                    return m_isPdcBlockPmu;
                }
            }

            public byte PdcBlockPmuCount
            {
                get
                {
                    return m_pdcBlockPmuCount;
                }
            }

            public int PdcBlockLength
            {
                get
                {
                    return m_pdcBlockLength;
                }
            }

            protected override ushort HeaderLength
            {
                get
                {
                    if (m_isPdcBlockPmu)
                    {
                        return 2;
                    }
                    else
                    {
                        return 6;
                    }
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    byte[] buffer = new byte[HeaderLength];

                    // Add PDCstream specific image - note that although this stream will
                    // correctly parse a PDCxchng style stream - we will not produce one.
                    // Only a fully formatted stream will ever be produced
                    buffer[0] = (byte)(m_flags & ~ChannelFlags.PDCExchangeFormat);

                    if (Parent.ConfigurationFrame.RevisionNumber >= RevisionNumber.Revision2)
                    {
                        buffer[1] = (byte)(AnalogValues.Count | (byte)m_reservedFlags);
                        buffer[2] = (byte)(DigitalValues.Count | (byte)IEEEFormatFlags);
                        buffer[3] = (byte)PhasorValues.Count;
                    }
                    else
                    {
                        buffer[1] = m_dataRate;
                        buffer[2] = (byte)DigitalValues.Count;
                        buffer[3] = (byte)PhasorValues.Count;
                    }

                    EndianOrder.BigEndian.CopyBytes(m_sampleNumber, buffer, 4);

                    return buffer;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                DataCellParsingState parsingState = (DataCellParsingState)state;
                RevisionNumber revision = Parent.ConfigurationFrame.RevisionNumber;
                byte analogs = binaryImage[startIndex + 1];
                byte digitals;
                byte phasors;

                // Get data cell flags
                m_flags = (ChannelFlags)binaryImage[startIndex];

                // Parse PDCstream specific header image
                if (revision >= RevisionNumber.Revision2 && !parsingState.IsPdcBlockPmu)
                {
                    // Strip off reserved flags
                    m_reservedFlags = (ReservedFlags)analogs & ~ReservedFlags.AnalogWordsMask;

                    // Leave analog word count
                    analogs &= (byte)ReservedFlags.AnalogWordsMask;
                }
                else
                {
                    // Older revisions didn't allow analogs
                    m_dataRate = analogs;
                    analogs = 0;
                }

                if (parsingState.IsPdcBlockPmu)
                {
                    // PDC Block PMU's contain exactly 2 phasors, 0 analogs and 1 digital
                    phasors = 2;
                    analogs = 0;
                    digitals = 1;
                    m_isPdcBlockPmu = true; // Have to take note of our smaller size for HeaderLength calculation!
                    UsingPDCExchangeFormat = true;
                }
                else
                {
                    // Parse number of digitals and phasors for normal PMU cells
                    digitals = binaryImage[startIndex + 2];
                    phasors = binaryImage[startIndex + 3];

                    if (revision >= RevisionNumber.Revision2)
                    {
                        // Strip off IEEE flags
                        IEEEFormatFlags = (IEEEFormatFlags)digitals & ~IEEEFormatFlags.DigitalWordsMask;

                        // Leave digital word count
                        digitals &= (byte)IEEEFormatFlags.DigitalWordsMask;
                    }

                    // Check for PDC exchange format
                    if (UsingPDCExchangeFormat)
                    {
                        // In cases where we are using PDC exchange the phasor count is the number of PMU's in the PDC block
                        m_pdcBlockPmuCount = phasors;

                        // This PDC block header has no data values of its own (only PMU's) - so we cancel
                        // data parsing for any other elements (see ParseBodyImage override below)
                        m_isPdcBlockHeader = true;

                        // Parse PMU's from PDC block...
                        DataFrame parentFrame = Parent;
                        int index = parsingState.Index;
                        ushort cellLength;

                        // Account for channel flags in PDC block header
                        m_pdcBlockLength = 4;
                        startIndex += 4;

                        for (int x = 0; x < m_pdcBlockPmuCount; x++)
                        {
                            if (index + x < parentFrame.ConfigurationFrame.Cells.Count)
                            {
                                parentFrame.Cells.Add(new DataCell(parentFrame, parentFrame.ConfigurationFrame.Cells[index + x], binaryImage, startIndex));
                                cellLength = parentFrame.Cells[index + x].BinaryLength;
                                startIndex += cellLength;
                                m_pdcBlockLength += cellLength;
                            }
                        }
                    }
                    else
                    {
                        // Parse PMU's sample number
                        m_sampleNumber = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4);
                    }
                }

                // Algorithm Case: Determine best course of action when stream counts don't match counts defined in the
                // external INI based configuration file.  Think about what *will* happen when new data appears in the
                // stream that's not in the config file - you could raise an event notifying consumer about the mismatch
                // instead of raising an exception - could even make a boolean property that would allow either case.
                // The important thing to consider is that to parse the cell images you have to have a defined
                // definition (see base class "Phasors.DataCellBase.ParseBodyImage").  If you have more items defined
                // in the stream than you do in the config file then you won't get the new value, too few items and you
                // don't have enough definitions to correctly interpret the data (that would be bad) - either way the
                // definitions won't line up with the appropriate data value and you won't know which one is missing or
                // added.  I can't change the protocol so this is enough argument to just raise an error for config
                // file/stream mismatch.  So for now we'll just throw an exception and deal with consequences :)
                // Note that this only applies to PDCstream protocol.

                // Addendum: After running this with several protocol implementations I noticed that if a device wasn't
                // reporting, the phasor count dropped to zero even if there were phasors defined in the configuration
                // file, so the only time an exception is thrown is if there are more phasors defined in the the stream
                // than there are defined in the INI file...

                // At least this number of phasors should be already defined in BPA PDCstream configuration file
                if (phasors > ConfigurationCell.PhasorDefinitions.Count)
                {
                    throw (new InvalidOperationException("Stream/Config File Mismatch: Phasor value count in stream (" + phasors + ") does not match defined count in configuration file (" + ConfigurationCell.PhasorDefinitions.Count + ") for " + ConfigurationCell.IDLabel));
                }

                // TODO: Evaluate case of "new PMU added to stream" for which we do not have defined number of phasors...
                // Perhaps some kind of warning would be better than throwing an exception (raise a OnParseWarning event?)
                // This just happened with NYPA:
                        // [P1]: WARNING: NYPA data stream exception: Stream/Config File Mismatch: Phasor
                        // value count in stream (6) does not match defined count in configuration file (0)
                        // for EGC


                // If analog values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
                //If analogWords > ConfigurationCell.AnalogDefinitions.Count Then
                //    Throw New InvalidOperationException("Stream/Config File Mismatch: Analog value count in stream (" analogWords & ") does not match defined count in configuration file (" & ConfigurationCell.AnalogDefinitions.Count & ")")
                //End If

                // If digital values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
                //If digitalWords > ConfigurationCell.DigitalDefinitions.Count Then
                //    Throw New InvalidOperationException("Stream/Config File Mismatch: Digital value count in stream (" digitalWords & ") does not match defined count in configuration file (" & ConfigurationCell.DigitalDefinitions.Count & ")")
                //End If

                // Dyanmically add analog definitions to configuration cell as needed (they are only defined in data frame of BPA PDCstream)
                if (analogs > ConfigurationCell.AnalogDefinitions.Count)
                {
                    for (int x = ConfigurationCell.AnalogDefinitions.Count; x < analogs; x++)
                    {
                        ConfigurationCell.AnalogDefinitions.Add(new BpaPdcStream.AnalogDefinition(ConfigurationCell, x, "Analog " + (x + 1)));
                    }
                }

                // Dyanmically add digital definitions to configuration cell as needed (they are only defined in data frame of BPA PDCstream)
                if (digitals > ConfigurationCell.DigitalDefinitions.Count)
                {
                    for (int x = ConfigurationCell.DigitalDefinitions.Count; x < digitals; x++)
                    {
                        ConfigurationCell.DigitalDefinitions.Add(new BpaPdcStream.DigitalDefinition(ConfigurationCell, x, "Digital Word " + (x + 1)));
                    }
                }

                // Unlike most all other protocols the counts defined for phasors, analogs and digitals in the data frame
                // may not exactly match what's defined in the configuration frame as these values are defined in an external
                // INI file for BPA PDCstream.  As a result, we manually assign the counts to the parsing state so that these
                // will be the counts used to parse values from data frame in the base class ParseBodyImage method
                parsingState.PhasorCount = phasors;
                parsingState.AnalogCount = analogs;
                parsingState.DigitalCount = digitals;

                // Status flags and remaining data elements will parsed by base class in the ParseBodyImage method

            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                // PDC block headers have no body elements to parse other than children and they will have already been parsed at this point
                if (!m_isPdcBlockHeader)
                {
                    base.ParseBodyImage(state, binaryImage, startIndex);
                }

            }

            protected override ushort BodyLength
            {
                get
                {
                    // PDC block headers have no body elements - so we return a zero length
                    if (m_isPdcBlockHeader)
                    {
                        return 0;
                    }
                    else
                    {
                        return base.BodyLength;
                    }
                }
            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize data cell
                info.AddValue("flags", m_flags, typeof(ChannelFlags));
                info.AddValue("reservedFlags", m_reservedFlags, typeof(ReservedFlags));
                info.AddValue("sampleNumber", m_sampleNumber);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Channel Flags", m_flags.ToString());
                    baseAttributes.Add("Reserved Flags", m_reservedFlags.ToString());
                    baseAttributes.Add("Sample Number", m_sampleNumber.ToString());
                    baseAttributes.Add("Reserved Flag 0 Is Set", ReservedFlag0IsSet.ToString());
                    baseAttributes.Add("Reserved Flag 1 Is Set", ReservedFlag1IsSet.ToString());
                    baseAttributes.Add("Using PDC Exchange Format", UsingPDCExchangeFormat.ToString());
                    baseAttributes.Add("Using Macrodyne Format", UsingMacrodyneFormat.ToString());
                    baseAttributes.Add("Using IEEE Format", UsingIEEEFormat.ToString());
                    baseAttributes.Add("Data Sorted By Timestamp (Obsolete)", ((m_flags & ChannelFlags.DataSortedByTimestamp) == 0).ToString());
                    baseAttributes.Add("Timestamp Is Included (Obsolete)", ((m_flags & ChannelFlags.TimestampIncluded) == 0).ToString());
                    baseAttributes.Add("PMU Parsed From PDC Block", m_isPdcBlockPmu.ToString());

                    return baseAttributes;
                }
            }

            // Delegate handler to create a new IEEE C37.118 data cell
            internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] binaryImage, int startIndex, out int parsedLength)
            {
                DataCell dataCell = new DataCell(parent as IDataFrame, (state as IDataFrameParsingState).ConfigurationFrame.Cells[index]);

                parsedLength = dataCell.Initialize(binaryImage, startIndex, 0);

                return dataCell;
            }

        }

    }
}
