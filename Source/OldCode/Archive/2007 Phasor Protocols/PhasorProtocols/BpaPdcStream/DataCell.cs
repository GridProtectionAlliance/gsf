//*******************************************************************************************************
//  DataCell.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PCS.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IDataCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class DataCell : DataCellBase
    {
        #region [ Members ]

        // Fields
        private ChannelFlags m_flags;
        private ReservedFlags m_reservedFlags;
        private ushort m_sampleNumber;
        private byte m_dataRate;
        private byte m_pdcBlockPmuCount;
        private bool m_isPdcBlockPmu;
        private bool m_isPdcBlockHeader;
        private int m_pdcBlockLength;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
            : base(parent, configurationCell, false, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            ConfigurationCell configCell = configurationCell as ConfigurationCell;
            bool isPDCBlockSection = false;

            if (configCell != null)
                isPDCBlockSection = configCell.IsPDCBlockSection;

            // Define new parsing state which defines constructors for key data values
            State = new DataCellParsingState(
                configurationCell,
                BpaPdcStream.PhasorValue.CreateNewValue,
                BpaPdcStream.FrequencyValue.CreateNewValue,
                BpaPdcStream.AnalogValue.CreateNewValue,
                BpaPdcStream.DigitalValue.CreateNewValue,
                isPDCBlockSection);
        }

        /// <summary>
        /// Creates a new <see cref="DataCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="DataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        /// <param name="addEmptyValues">If <c>true</c>, adds empty values for each defined configuration cell definition.</param>
        public DataCell(DataFrame parent, ConfigurationCell configurationCell, bool addEmptyValues)
            : this(parent, configurationCell)
        {
            if (addEmptyValues)
            {
                int x;

                // Define needed phasor values
                for (x = 0; x < ConfigurationCell.PhasorDefinitions.Count; x++)
                {
                    PhasorValues.Add(new PhasorValue(this, ConfigurationCell.PhasorDefinitions[x]));
                }

                // Define a frequency and df/dt
                FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition);

                // Define any analog values
                for (x = 0; x < ConfigurationCell.AnalogDefinitions.Count; x++)
                {
                    AnalogValues.Add(new AnalogValue(this, ConfigurationCell.AnalogDefinitions[x]));
                }

                // Define any digital values
                for (x = 0; x < ConfigurationCell.DigitalDefinitions.Count; x++)
                {
                    DigitalValues.Add(new DigitalValue(this, ConfigurationCell.DigitalDefinitions[x]));
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="DataCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data cell
            m_flags = (ChannelFlags)info.GetValue("flags", typeof(ChannelFlags));
            m_reservedFlags = (ReservedFlags)info.GetValue("reservedFlags", typeof(ReservedFlags));
            m_sampleNumber = info.GetUInt16("sampleNumber");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the reference to parent <see cref="DataFrame"/> of this <see cref="DataCell"/>.
        /// </summary>
        public new DataFrame Parent
        {
            get
            {
                return base.Parent as DataFrame;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.
        /// </summary>
        public new ConfigurationCell ConfigurationCell
        {
            get
            {
                return base.ConfigurationCell as ConfigurationCell;
            }
            set
            {
                base.ConfigurationCell = value;
            }
        }

        /// <summary>
        /// Gets or sets channel flags for this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// These are bit flags, use properties to change basic values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
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

        /// <summary>
        /// Gets or sets reserved flags for this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// These are bit flags, use properties to change basic values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
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

        /// <summary>
        /// Gets or sets data rate of this <see cref="DataCell"/>.
        /// </summary>
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

        /// <summary>
        /// Gets or sets <see cref="BpaPdcStream.FormatFlags"/> from <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.
        /// </summary>
        public FormatFlags FormatFlags
        {
            get
            {
                return ConfigurationCell.FormatFlags;
            }
            set
            {
                ConfigurationCell.FormatFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets sample number associated with this <see cref="DataCell"/>.
        /// </summary>
        public ushort SampleNumber
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

        /// <summary>
        /// Gets or sets flag that determines if reserved flag zero is set.
        /// </summary>
        public bool ReservedFlag0IsSet
        {
            get
            {
                return ((m_reservedFlags & ReservedFlags.Reserved0) > 0);
            }
            set
            {
                if (value)
                    m_reservedFlags = m_reservedFlags | ReservedFlags.Reserved0;
                else
                    m_reservedFlags = m_reservedFlags & ~ReservedFlags.Reserved0;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if reserved flag one is set.
        /// </summary>
        public bool ReservedFlag1IsSet
        {
            get
            {
                return ((m_reservedFlags & ReservedFlags.Reserved1) > 0);
            }
            set
            {
                if (value)
                    m_reservedFlags = m_reservedFlags | ReservedFlags.Reserved1;
                else
                    m_reservedFlags = m_reservedFlags & ~ReservedFlags.Reserved1;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get
            {
                return ((m_flags & ChannelFlags.DataIsValid) == 0);
            }
            set
            {
                if (value)
                    m_flags = m_flags & ~ChannelFlags.DataIsValid;
                else
                    m_flags = m_flags | ChannelFlags.DataIsValid;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        public override bool SynchronizationIsValid
        {
            get
            {
                return ((m_flags & ChannelFlags.PMUSynchronized) == 0);
            }
            set
            {
                if (value)
                    m_flags = m_flags & ~ChannelFlags.PMUSynchronized;
                else
                    m_flags = m_flags | ChannelFlags.PMUSynchronized;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
        /// </summary>
        public override DataSortingType DataSortingType
        {
            get
            {
                return (((m_flags & ChannelFlags.DataSortedByArrival) > 0) ? PhasorProtocols.DataSortingType.ByArrival : PhasorProtocols.DataSortingType.ByTimestamp);
            }
            set
            {
                if (value == PhasorProtocols.DataSortingType.ByArrival)
                    m_flags = m_flags | ChannelFlags.DataSortedByArrival;
                else
                    m_flags = m_flags & ~ChannelFlags.DataSortedByArrival;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        public override bool DeviceError
        {
            get
            {
                return ((m_flags & ChannelFlags.TransmissionErrors) > 0);
            }
            set
            {
                if (value)
                    m_flags = m_flags | ChannelFlags.TransmissionErrors;
                else
                    m_flags = m_flags & ~ChannelFlags.TransmissionErrors;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> is using the PDC exchange format.
        /// </summary>
        public bool UsingPDCExchangeFormat
        {
            get
            {
                return ((m_flags & ChannelFlags.PDCExchangeFormat) > 0);
            }
            set
            {
                if (value)
                    m_flags = m_flags | ChannelFlags.PDCExchangeFormat;
                else
                    m_flags = m_flags & ~ChannelFlags.PDCExchangeFormat;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> is using Macrodyne format.
        /// </summary>
        public bool UsingMacrodyneFormat
        {
            get
            {
                return ((m_flags & ChannelFlags.MacrodyneFormat) > 0);
            }
            set
            {
                if (value)
                    m_flags = m_flags | ChannelFlags.MacrodyneFormat;
                else
                    m_flags = m_flags & ~ChannelFlags.MacrodyneFormat;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> is using IEEE format.
        /// </summary>
        public bool UsingIEEEFormat
        {
            get
            {
                return ((m_flags & ChannelFlags.MacrodyneFormat) == 0);
            }
            set
            {
                if (value)
                    m_flags = m_flags & ~ChannelFlags.MacrodyneFormat;
                else
                    m_flags = m_flags | ChannelFlags.MacrodyneFormat;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> data is sorted by timestamp.
        /// </summary>
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
                    m_flags = m_flags & ~ChannelFlags.DataSortedByTimestamp;
                else
                    m_flags = m_flags | ChannelFlags.DataSortedByTimestamp;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp is included with this <see cref="DataCell"/>.
        /// </summary>
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
                    m_flags = m_flags & ~ChannelFlags.TimestampIncluded;
                else
                    m_flags = m_flags | ChannelFlags.TimestampIncluded;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="DataCell"/> is a PDC block PMU.
        /// </summary>
        public bool IsPdcBlockPmu
        {
            get
            {
                return m_isPdcBlockPmu;
            }
        }

        /// <summary>
        /// Gets the PDC block PMU count of this <see cref="DataCell"/>.
        /// </summary>
        public byte PdcBlockPmuCount
        {
            get
            {
                return m_pdcBlockPmuCount;
            }
        }

        /// <summary>
        /// Gets the PDC block length of this <see cref="DataCell"/>.
        /// </summary>
        public int PdcBlockLength
        {
            get
            {
                return m_pdcBlockLength;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                if (m_isPdcBlockPmu)
                    return 2;
                else
                    return 6;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="DataCell"/> object.
        /// </summary>
        /// <remarks>
        /// Although this BPA PDCstream implementation <see cref="DataCell"/> will correctly parse a PDCxchng style
        /// stream, one will not be produced. Only a fully formatted stream will ever be produced.
        /// </remarks>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];

                // Add PDCstream specific image (we don't produce PDCExchangeFormat)
                buffer[0] = (byte)(m_flags & ~ChannelFlags.PDCExchangeFormat);

                if (Parent.ConfigurationFrame.RevisionNumber >= RevisionNumber.Revision2)
                {
                    buffer[1] = (byte)(AnalogValues.Count | (byte)m_reservedFlags);
                    buffer[2] = (byte)(DigitalValues.Count | (byte)FormatFlags);
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

        /// <summary>
        /// Gets the length of the <see cref="DataCellBase.BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                // PDC block headers have no body elements - so we return a zero length
                if (m_isPdcBlockHeader)
                    return 0;
                else
                    return base.BodyLength;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Channel Flags", ChannelFlags.ToString());
                baseAttributes.Add("Reserved Flags", ReservedFlags.ToString());
                baseAttributes.Add("Sample Number", SampleNumber.ToString());
                baseAttributes.Add("Reserved Flag 0 Is Set", ReservedFlag0IsSet.ToString());
                baseAttributes.Add("Reserved Flag 1 Is Set", ReservedFlag1IsSet.ToString());
                baseAttributes.Add("Using PDC Exchange Format", UsingPDCExchangeFormat.ToString());
                baseAttributes.Add("Using Macrodyne Format", UsingMacrodyneFormat.ToString());
                baseAttributes.Add("Using IEEE Format", UsingIEEEFormat.ToString());
                baseAttributes.Add("PMU Parsed From PDC Block", IsPdcBlockPmu.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
        {
            DataCellParsingState state = State as DataCellParsingState;
            RevisionNumber revision = Parent.ConfigurationFrame.RevisionNumber;
            int x, index = startIndex;
            byte analogs = binaryImage[index + 1];
            byte digitals;
            byte phasors;

            // Get data cell flags
            m_flags = (ChannelFlags)binaryImage[index];
            index += 2;

            // Parse PDCstream specific header image
            if (revision >= RevisionNumber.Revision2 && !state.IsPdcBlockPmu)
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

            if (state.IsPdcBlockPmu)
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
                digitals = binaryImage[index];
                phasors = binaryImage[index + 1];
                index += 2;

                if (revision >= RevisionNumber.Revision2)
                {
                    // Strip off IEEE flags
                    FormatFlags = (FormatFlags)digitals & ~FormatFlags.DigitalWordsMask;

                    // Leave digital word count
                    digitals &= (byte)FormatFlags.DigitalWordsMask;
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
                    int parsedLength, cellIndex = state.Index;

                    // Account for channel flags in PDC block header
                    m_pdcBlockLength = 4;

                    for (x = 0; x < m_pdcBlockPmuCount; x++)
                    {
                        if (cellIndex + x < parentFrame.ConfigurationFrame.Cells.Count)
                        {
                            parentFrame.Cells.Add(DataCell.CreateNewCell(parentFrame, parentFrame.State, cellIndex + x, binaryImage, index, out parsedLength));
                            index += parsedLength;
                            m_pdcBlockLength += parsedLength;
                        }
                    }
                }
                else
                {
                    // Parse PMU's sample number
                    m_sampleNumber = EndianOrder.BigEndian.ToUInt16(binaryImage, index);
                    index += 2;
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
                throw new InvalidOperationException("Stream/Config File Mismatch: Phasor value count in stream (" + phasors + ") does not match defined count in configuration file (" + ConfigurationCell.PhasorDefinitions.Count + ") for " + ConfigurationCell.IDLabel);

            // If analog values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            // Dyanmically add analog definitions to configuration cell as needed (they are only defined in data frame of BPA PDCstream)
            if (analogs > ConfigurationCell.AnalogDefinitions.Count)
            {
                for (x = ConfigurationCell.AnalogDefinitions.Count; x < analogs; x++)
                {
                    ConfigurationCell.AnalogDefinitions.Add(new AnalogDefinition(ConfigurationCell, "Analog " + (x + 1), 1, 0.0D));
                }
            }

            // If digital values get a clear definition in INI file at some point, we can validate the number in the stream to the number in the config file...
            // Dyanmically add digital definitions to configuration cell as needed (they are only defined in data frame of BPA PDCstream)
            if (digitals > ConfigurationCell.DigitalDefinitions.Count)
            {
                for (x = ConfigurationCell.DigitalDefinitions.Count; x < digitals; x++)
                {
                    ConfigurationCell.DigitalDefinitions.Add(new DigitalDefinition(ConfigurationCell, "Digital Word " + (x + 1)));
                }
            }

            // Unlike most all other protocols the counts defined for phasors, analogs and digitals in the data frame
            // may not exactly match what's defined in the configuration frame as these values are defined in an external
            // INI file for BPA PDCstream.  As a result, we manually assign the counts to the parsing state so that these
            // will be the counts used to parse values from data frame in the base class ParseBodyImage method
            state.PhasorCount = phasors;
            state.AnalogCount = analogs;
            state.DigitalCount = digitals;

            // Status flags and remaining data elements will parsed by base class in the ParseBodyImage method
            return (index - startIndex);
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            // PDC block headers have no body elements to parse other than children and they will have already been parsed at this point
            if (!m_isPdcBlockHeader)
                return base.ParseBodyImage(binaryImage, startIndex, length);

            return 0;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data cell
            info.AddValue("flags", m_flags, typeof(ChannelFlags));
            info.AddValue("reservedFlags", m_reservedFlags, typeof(ReservedFlags));
            info.AddValue("sampleNumber", m_sampleNumber);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new BPA PDCstream data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new DataCell(parent as IDataFrame, (state as IDataFrameParsingState).ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.Initialize(binaryImage, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}