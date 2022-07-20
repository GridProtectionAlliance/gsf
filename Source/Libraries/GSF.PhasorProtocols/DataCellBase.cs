//******************************************************************************************************
//  DataCellBase.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.TimeSeries;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Protocol independent common status flags enumeration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These flags are expected to exist in the high-word of a double-word flag set such that original word flags remain in-tact
    /// in low-word of double-word flag set.
    /// </para>
    /// <para>
    /// Note that value being stored in a historian using a 32-bit single precision floating number value will only have castable
    /// conversion accuracy back to an unsigned 32-bit integer up to the 24th bit, so any bits used beyond Bit23 will be lossy.
    /// </para>
    /// </remarks>
    [Flags, Serializable]
    public enum CommonStatusFlags : uint
    {
        /// <summary>
        /// Data was modified by a post-processing device, e.g., a PDC (0 for no modifications, 1 when data modified).
        /// </summary>
        DataModified = (uint)Bits.Bit21,
        /// <summary>
        /// Data was discarded from real-time stream due to late arrival (0 when not discarded, 1 when discarded).
        /// </summary>
        DataDiscarded = (uint)Bits.Bit20,
        /// <summary>
        /// Data is valid (0 when device data is valid, 1 when invalid or device is in test mode).
        /// </summary>
        DataIsValid = (uint)Bits.Bit19,
        /// <summary>
        /// Synchronization is valid (0 when in device is in sync, 1 when it is not).
        /// </summary>
        SynchronizationIsValid = (uint)Bits.Bit18,
        /// <summary>
        /// Data sorting type, 0 by timestamp, 1 by arrival.
        /// </summary>
        DataSortingType = (uint)Bits.Bit17,
        /// <summary>
        /// Device error (including configuration error), 0 when no error.
        /// </summary>
        DeviceError = (uint)Bits.Bit16,
        /// <summary>
        /// Reserved bits for future common flags, presently set to 0.
        /// </summary>
        ReservedFlags = (uint)(Bits.Bit22 | Bits.Bit23 | Bits.Bit24 | Bits.Bit25 | Bits.Bit26 | Bits.Bit27 | Bits.Bit28 | Bits.Bit29 | Bits.Bit30 | Bits.Bit31),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (uint)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Represents the protocol independent common implementation of all elements for cells in a <see cref="IDataFrame"/>.
    /// </summary>
    [Serializable]
    public abstract class DataCellBase : ChannelCellBase, IDataCell
    {
        #region [ Members ]

        // Fields
        private IConfigurationCell m_configurationCell;
        private ushort m_statusFlags;
        private bool m_statusAssigned;
        private readonly PhasorValueCollection m_phasorValues;
        private IFrequencyValue m_frequencyValue;
        private readonly AnalogValueCollection m_analogValues;
        private readonly DigitalValueCollection m_digitalValues;
        private bool m_isDiscarded;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCellBase"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCellBase"/>.</param>
        /// <param name="statusFlags">The default status flags to apply to this <see cref="DataCellBase"/>.</param>
        /// <param name="maximumPhasors">Sets the maximum number of phasors for the <see cref="PhasorValues"/> collection.</param>
        /// <param name="maximumAnalogs">Sets the maximum number of phasors for the <see cref="AnalogValues"/> collection.</param>
        /// <param name="maximumDigitals">Sets the maximum number of phasors for the <see cref="DigitalValues"/> collection.</param>
        protected DataCellBase(IDataFrame parent, IConfigurationCell configurationCell, ushort statusFlags, int maximumPhasors, int maximumAnalogs, int maximumDigitals)
            : base(parent, 0)
        {
            m_configurationCell = configurationCell;
            m_statusFlags = statusFlags;
            m_phasorValues = new PhasorValueCollection(maximumPhasors);
            m_analogValues = new AnalogValueCollection(maximumAnalogs);
            m_digitalValues = new DigitalValueCollection(maximumDigitals);
        }

        /// <summary>
        /// Creates a new <see cref="DataCellBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataCellBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data cell values
            m_configurationCell = (IConfigurationCell)info.GetValue("configurationCell", typeof(IConfigurationCell));
            m_statusFlags = info.GetUInt16("statusFlags");
            m_phasorValues = (PhasorValueCollection)info.GetValue("phasorValues", typeof(PhasorValueCollection));
            m_frequencyValue = (IFrequencyValue)info.GetValue("frequencyValue", typeof(IFrequencyValue));
            m_analogValues = (AnalogValueCollection)info.GetValue("analogValues", typeof(AnalogValueCollection));
            m_digitalValues = (DigitalValueCollection)info.GetValue("digitalValues", typeof(DigitalValueCollection));
            m_statusAssigned = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the reference to parent <see cref="IDataFrame"/> of this <see cref="DataCellBase"/>.
        /// </summary>
        public new virtual IDataFrame Parent
        {
            get => base.Parent as IDataFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="IConfigurationCell"/> associated with this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual IConfigurationCell ConfigurationCell
        {
            get => m_configurationCell;
            set => m_configurationCell = value;
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="DataCellBase"/>.
        /// </summary>
        public new virtual IDataCellParsingState State
        {
            get => base.State as IDataCellParsingState;
            set => base.State = value;
        }

        /// <summary>
        /// Gets station name of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual string StationName => 
            m_configurationCell is null ? "" : m_configurationCell.StationName.ToNonNullString();

        /// <summary>
        /// Gets ID label of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual string IDLabel => 
            m_configurationCell is null ? "" : m_configurationCell.IDLabel.ToNonNullString();

        /// <summary>
        /// Gets or sets 16-bit status flags of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual ushort StatusFlags
        {
            get => m_statusFlags;
            set
            {
                m_statusFlags = value;
                m_statusAssigned = true;
            }
        }

        /// <summary>
        /// Gets the numeric ID code for this <see cref="DataCellBase"/>.
        /// </summary>
        /// <remarks>
        /// This value is read-only for <see cref="DataCellBase"/>; assigning a value will throw an exception. Value returned
        /// is the <see cref="IChannelCell.IDCode"/> of the associated <see cref="ConfigurationCell"/>.
        /// </remarks>
        /// <exception cref="NotSupportedException">IDCode of a data cell is read-only, change IDCode is associated configuration cell instead.</exception>
        public override ushort IDCode
        {
            get => m_configurationCell?.IDCode ?? 0;
            set => throw new NotSupportedException("IDCode of a data cell is read-only, change IDCode is associated configuration cell instead");
        }

        /// <summary>
        /// Gets or sets common status flags of this <see cref="DataCellBase"/>.
        /// </summary>
        public uint CommonStatusFlags
        {
            get
            {
                // Start with lo-word protocol specific flags
                uint commonFlags = StatusFlags;

                // Add hi-word protocol independent common flags
                if (!DataIsValid)
                    commonFlags |= (uint)PhasorProtocols.CommonStatusFlags.DataIsValid;

                if (!SynchronizationIsValid)
                    commonFlags |= (uint)PhasorProtocols.CommonStatusFlags.SynchronizationIsValid;

                if (DataSortingType != DataSortingType.ByTimestamp)
                    commonFlags |= (uint)PhasorProtocols.CommonStatusFlags.DataSortingType;

                if (DeviceError)
                    commonFlags |= (uint)PhasorProtocols.CommonStatusFlags.DeviceError;

                if (m_isDiscarded)
                    commonFlags |= (uint)PhasorProtocols.CommonStatusFlags.DataDiscarded;

                if (DataModified)
                    commonFlags |= (uint)PhasorProtocols.CommonStatusFlags.DataModified;

                return commonFlags;
            }
            set
            {
                // Deriving common states requires clearing of base status flags...
                if (value != uint.MaxValue)
                    StatusFlags = 0;

                // Derive common states via common status flags
                DataIsValid = (value & (uint)PhasorProtocols.CommonStatusFlags.DataIsValid) == 0;
                SynchronizationIsValid = (value & (uint)PhasorProtocols.CommonStatusFlags.SynchronizationIsValid) == 0;
                DataSortingType = (value & (uint)PhasorProtocols.CommonStatusFlags.DataSortingType) == 0 ? DataSortingType.ByTimestamp : DataSortingType.ByArrival;
                DeviceError = (value & (uint)PhasorProtocols.CommonStatusFlags.DeviceError) > 0;
                m_isDiscarded = (value & (uint)PhasorProtocols.CommonStatusFlags.DataDiscarded) > 0;
                DataModified = (value & (uint)PhasorProtocols.CommonStatusFlags.DataModified) > 0;
            }
        }

        /// <summary>
        /// Gets flag that determines if all values of this <see cref="DataCellBase"/> have been assigned.
        /// </summary>
        public virtual bool AllValuesAssigned => m_statusAssigned && PhasorValues.AllValuesAssigned && !FrequencyValue.IsEmpty && AnalogValues.AllValuesAssigned && DigitalValues.AllValuesAssigned;

        /// <summary>
        /// Gets <see cref="PhasorValueCollection"/> of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual PhasorValueCollection PhasorValues => m_phasorValues;

        /// <summary>
        /// Gets <see cref="IFrequencyValue"/> of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual IFrequencyValue FrequencyValue
        {
            get => m_frequencyValue;
            set => m_frequencyValue = value;
        }

        /// <summary>
        /// Gets <see cref="AnalogValueCollection"/>of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual AnalogValueCollection AnalogValues => m_analogValues;

        /// <summary>
        /// Gets <see cref="DigitalValueCollection"/>of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual DigitalValueCollection DigitalValues => m_digitalValues;

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCellBase"/> is valid.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract bool DataIsValid { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCellBase"/> is valid based on GPS lock.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract bool SynchronizationIsValid { get; set; }

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.DataSortingType"/> of this <see cref="DataCellBase"/>.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract DataSortingType DataSortingType { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCellBase"/> is reporting an error.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract bool DeviceError { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if data is modified by a post-processing
        /// device, such as a PDC.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public virtual bool DataModified { get; set; }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => 2 + m_phasorValues.BinaryLength + m_frequencyValue.BinaryLength + m_analogValues.BinaryLength + m_digitalValues.BinaryLength;

        /// <summary>
        /// Gets the binary body image of the <see cref="DataCellBase"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];
                int index = 0;

                // Copy in common cell image
                BigEndian.CopyBytes(m_statusFlags, buffer, index);
                index += 2;

                m_phasorValues.CopyImage(buffer, ref index);
                m_frequencyValue.CopyImage(buffer, ref index);
                m_analogValues.CopyImage(buffer, ref index);
                m_digitalValues.CopyImage(buffer, ref index);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataCellBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Station Name", StationName);
                baseAttributes.Add("ID Label", IDLabel);
                baseAttributes.Add("Status Flags", StatusFlags.ToString());
                baseAttributes.Add("Status Flags (Big Endian Bits)", StatusFlags.ToBinaryString());
                baseAttributes.Add("Status Flags (Hexadecimal)", $"0x{StatusFlags:X2}");
                baseAttributes.Add("Data Is Valid", DataIsValid.ToString());
                baseAttributes.Add("Synchronization Is Valid", SynchronizationIsValid.ToString());
                baseAttributes.Add("Data Sorting Type", Enum.GetName(typeof(DataSortingType), DataSortingType));
                baseAttributes.Add("Device Error", DeviceError.ToString());
                baseAttributes.Add("Data Discarded", m_isDiscarded.ToString());
                baseAttributes.Add("Data Modified", DataModified.ToString());
                baseAttributes.Add("Frequency Value", m_frequencyValue?.Frequency.ToString());
                baseAttributes.Add("dF/dt Value", m_frequencyValue?.DfDt.ToString());
                baseAttributes.Add("Total Phasor Values", PhasorValues.Count.ToString());
                baseAttributes.Add("Total Analog Values", AnalogValues.Count.ToString());
                baseAttributes.Add("Total Digital Values", DigitalValues.Count.ToString());
                baseAttributes.Add("All Values Assigned", AllValuesAssigned.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            IDataCellParsingState parsingState = State;
            IPhasorValue phasorValue;
            IAnalogValue analogValue;
            IDigitalValue digitalValue;
            int x, parsedLength, index = startIndex;

            StatusFlags = BigEndian.ToUInt16(buffer, startIndex);
            index += 2;

            // By the very nature of the major phasor protocols supporting the same order of phasors, frequency, df/dt, analog and digitals
            // we are able to "automatically" parse this data out in the data cell base class - BEAUTIFUL!!!

            // Parse out phasor values
            for (x = 0; x < parsingState.PhasorCount; x++)
            {
                phasorValue = parsingState.CreateNewPhasorValue(this, m_configurationCell.PhasorDefinitions[x], buffer, index, out parsedLength);
                m_phasorValues.Add(phasorValue);
                index += parsedLength;
            }

            // Parse out frequency and dF/dt values
            m_frequencyValue = parsingState.CreateNewFrequencyValue(this, m_configurationCell.FrequencyDefinition, buffer, index, out parsedLength);
            index += parsedLength;

            // Parse out analog values
            for (x = 0; x < parsingState.AnalogCount; x++)
            {
                analogValue = parsingState.CreateNewAnalogValue(this, m_configurationCell.AnalogDefinitions[x], buffer, index, out parsedLength);
                m_analogValues.Add(analogValue);
                index += parsedLength;
            }

            // Parse out digital values
            for (x = 0; x < parsingState.DigitalCount; x++)
            {
                digitalValue = parsingState.CreateNewDigitalValue(this, m_configurationCell.DigitalDefinitions[x], buffer, index, out parsedLength);
                m_digitalValues.Add(digitalValue);
                index += parsedLength;
            }

            // Return total parsed length
            return index - startIndex;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize data cell values
            info.AddValue("configurationCell", m_configurationCell, typeof(IConfigurationCell));
            info.AddValue("statusFlags", m_statusFlags);
            info.AddValue("phasorValues", m_phasorValues, typeof(PhasorValueCollection));
            info.AddValue("frequencyValue", m_frequencyValue, typeof(IFrequencyValue));
            info.AddValue("analogValues", m_analogValues, typeof(AnalogValueCollection));
            info.AddValue("digitalValues", m_digitalValues, typeof(DigitalValueCollection));
        }

        // Gets the status flags of the data cell as a measurement value.
        IMeasurement IDataCell.GetStatusFlagsMeasurement()
        {
            return new Measurement()
            {
                Timestamp = Parent.Timestamp,
                Value = CommonStatusFlags
            };
        }

        #endregion
    }
}