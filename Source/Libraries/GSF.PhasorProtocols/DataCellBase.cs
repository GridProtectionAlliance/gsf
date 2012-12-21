//******************************************************************************************************
//  DataCellBase.cs - Gbtc
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
using GSF;

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
    [Flags(), Serializable()]
    public enum CommonStatusFlags : uint
    {
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
        ReservedFlags = (uint)(Bits.Bit21 | Bits.Bit22 | Bits.Bit23 | Bits.Bit24 | Bits.Bit25 | Bits.Bit26 | Bits.Bit27 | Bits.Bit28 | Bits.Bit29 | Bits.Bit30 | Bits.Bit31),
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (uint)Bits.Nil
    }

    #endregion

    /// <summary>
    /// Represents the protocol independent common implementation of all elements for cells in a <see cref="IDataFrame"/>.
    /// </summary>
    [Serializable()]
    public abstract class DataCellBase : ChannelCellBase, IDataCell
    {
        #region [ Members ]

        // Fields
        private IConfigurationCell m_configurationCell;
        private ushort m_statusFlags;
        private bool m_statusAssigned;
        private PhasorValueCollection m_phasorValues;
        private IFrequencyValue m_frequencyValue;
        private AnalogValueCollection m_analogValues;
        private DigitalValueCollection m_digitalValues;

        // IMeasurement implementation fields
        private MeasurementKey m_key;
        private Guid m_id;
        private MeasurementStateFlags m_stateFlags;
        private string m_tagName;
        private Ticks m_timestamp;
        private Ticks m_receivedTimestamp;
        private Ticks m_publishedTimestamp;
        private double m_adder;
        private double m_multiplier;
        private bool m_isDiscarded;
        private MeasurementValueFilterFunction m_measurementValueFilter;

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

            // Initialize IMeasurement members
            m_key = GSF.PhasorProtocols.Common.UndefinedKey;
            m_stateFlags = MeasurementStateFlags.Normal;
            m_receivedTimestamp = PrecisionTimer.UtcNow.Ticks;
            m_timestamp = -1;
            m_adder = 0.0D;
            m_multiplier = 1.0D;
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
        public virtual new IDataFrame Parent
        {
            get
            {
                return base.Parent as IDataFrame;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IConfigurationCell"/> associated with this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual IConfigurationCell ConfigurationCell
        {
            get
            {
                return m_configurationCell;
            }
            set
            {
                m_configurationCell = value;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual new IDataCellParsingState State
        {
            get
            {
                return base.State as IDataCellParsingState;
            }
            set
            {
                base.State = value;
            }
        }

        /// <summary>
        /// Gets station name of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual string StationName
        {
            get
            {
                if ((object)m_configurationCell != null)
                    return m_configurationCell.StationName.ToNonNullString();

                return "";
            }
        }

        /// <summary>
        /// Gets ID label of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual string IDLabel
        {
            get
            {
                if ((object)m_configurationCell != null)
                    return m_configurationCell.IDLabel.ToNonNullString();

                return "";
            }
        }

        /// <summary>
        /// Gets or sets 16-bit status flags of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual ushort StatusFlags
        {
            get
            {
                return m_statusFlags;
            }
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
            get
            {
                if ((object)m_configurationCell != null)
                    return m_configurationCell.IDCode;

                return 0;
            }
            set
            {
                throw new NotSupportedException("IDCode of a data cell is read-only, change IDCode is associated configuration cell instead");
            }
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
                    commonFlags |= (uint)GSF.PhasorProtocols.CommonStatusFlags.DataIsValid;

                if (!SynchronizationIsValid)
                    commonFlags |= (uint)GSF.PhasorProtocols.CommonStatusFlags.SynchronizationIsValid;

                if (DataSortingType != GSF.PhasorProtocols.DataSortingType.ByTimestamp)
                    commonFlags |= (uint)GSF.PhasorProtocols.CommonStatusFlags.DataSortingType;

                if (DeviceError)
                    commonFlags |= (uint)GSF.PhasorProtocols.CommonStatusFlags.DeviceError;

                if (m_isDiscarded)
                    commonFlags |= (uint)GSF.PhasorProtocols.CommonStatusFlags.DataDiscarded;

                return commonFlags;
            }
            set
            {
                // Deriving common states requires clearing of base status flags...
                if (value != uint.MaxValue)
                    StatusFlags = 0;

                // Derive common states via common status flags
                DataIsValid = (value & (uint)GSF.PhasorProtocols.CommonStatusFlags.DataIsValid) == 0;
                SynchronizationIsValid = (value & (uint)GSF.PhasorProtocols.CommonStatusFlags.SynchronizationIsValid) == 0;
                DataSortingType = ((value & (uint)GSF.PhasorProtocols.CommonStatusFlags.DataSortingType) == 0) ? GSF.PhasorProtocols.DataSortingType.ByTimestamp : GSF.PhasorProtocols.DataSortingType.ByArrival;
                DeviceError = ((value & (uint)GSF.PhasorProtocols.CommonStatusFlags.DeviceError) > 0);
                m_isDiscarded = ((value & (uint)GSF.PhasorProtocols.CommonStatusFlags.DataDiscarded) > 0);
            }
        }

        /// <summary>
        /// Gets flag that determines if all values of this <see cref="DataCellBase"/> have been assigned.
        /// </summary>
        public virtual bool AllValuesAssigned
        {
            get
            {
                return m_statusAssigned && PhasorValues.AllValuesAssigned && !FrequencyValue.IsEmpty && AnalogValues.AllValuesAssigned && DigitalValues.AllValuesAssigned;
            }
        }

        /// <summary>
        /// Gets <see cref="PhasorValueCollection"/> of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual PhasorValueCollection PhasorValues
        {
            get
            {
                return m_phasorValues;
            }
        }

        /// <summary>
        /// Gets <see cref="IFrequencyValue"/> of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual IFrequencyValue FrequencyValue
        {
            get
            {
                return m_frequencyValue;
            }
            set
            {
                m_frequencyValue = value;
            }
        }

        /// <summary>
        /// Gets <see cref="AnalogValueCollection"/>of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual AnalogValueCollection AnalogValues
        {
            get
            {
                return m_analogValues;
            }
        }

        /// <summary>
        /// Gets <see cref="DigitalValueCollection"/>of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual DigitalValueCollection DigitalValues
        {
            get
            {
                return m_digitalValues;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCellBase"/> is valid.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract bool DataIsValid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCellBase"/> is valid based on GPS lock.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract bool SynchronizationIsValid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets <see cref="GSF.PhasorProtocols.DataSortingType"/> of this <see cref="DataCellBase"/>.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract DataSortingType DataSortingType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCellBase"/> is reporting an error.
        /// </summary>
        /// <remarks>
        /// This value is used to abstractly assign the protocol independent set of <see cref="CommonStatusFlags"/>.
        /// </remarks>
        public abstract bool DeviceError
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 2 + m_phasorValues.BinaryLength + m_frequencyValue.BinaryLength + m_analogValues.BinaryLength + m_digitalValues.BinaryLength;
            }
        }

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
                EndianOrder.BigEndian.CopyBytes(m_statusFlags, buffer, index);
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
                byte[] valueBytes = BitConverter.GetBytes(StatusFlags);

                baseAttributes.Add("Station Name", StationName);
                baseAttributes.Add("ID Label", IDLabel);
                baseAttributes.Add("Status Flags", StatusFlags.ToString());
                baseAttributes.Add("Status Flags (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(valueBytes));
                baseAttributes.Add("Status Flags (Hexadecimal)", "0x" + ByteEncoding.Hexadecimal.GetString(valueBytes));
                baseAttributes.Add("Data Is Valid", DataIsValid.ToString());
                baseAttributes.Add("Synchronization Is Valid", SynchronizationIsValid.ToString());
                baseAttributes.Add("Data Sorting Type", Enum.GetName(typeof(DataSortingType), DataSortingType));
                baseAttributes.Add("Device Error", DeviceError.ToString());
                baseAttributes.Add("Data Discarded", m_isDiscarded.ToString());
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

            StatusFlags = EndianOrder.BigEndian.ToUInt16(buffer, startIndex);
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

            // Parse out frequency and df/dt values
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
            return (index - startIndex);
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

        /// <summary>
        /// Gets the string respresentation of this <see cref="DataCellBase"/>.
        /// </summary>
        /// <returns>String respresentation of this <see cref="DataCellBase"/>.</returns>
        public override string ToString()
        {
            string stationName = StationName;
            string measurementID = null;

            if (m_key.ID != uint.MaxValue && m_key.Source != "__")
                measurementID = Measurement.ToString(this);

            if (!string.IsNullOrWhiteSpace(stationName))
            {
                if (measurementID != null)
                    stationName += " [" + measurementID + "]";

                return stationName;
            }

            if (measurementID != null)
                return measurementID;

            return base.ToString();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the <see cref="DataCellBase"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the <see cref="DataCellBase"/>.</param>
        /// <returns>true if the specified object is equal to the <see cref="DataCellBase"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            ITimeSeriesValue measurement = obj as ITimeSeriesValue;

            // If comparing to another measurment, use hash code for equality
            if (measurement != null)
                return ((ITimeSeriesValue)this).Equals(measurement);

            // Otherwise use default equality comparison
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for the <see cref="DataCellBase"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="DataCellBase"/>.</returns>
        /// <remarks>Hash code based on value of measurement key associated with the <see cref="DataCellBase"/>.</remarks>
        public override int GetHashCode()
        {
            return ((ITimeSeriesValue)this).GetHashCode();
        }

        #endregion

        #region [ IMeasurement Implementation ]

        // We keep the IMeasurement implementation of the DataCell completely private.  Exposing
        // these properties publically would only stand to add confusion as to where measurements
        // typically come from (i.e., the IDataCell's values) - the only value the cell itself has
        // to offer is the "CommonStatusFlags" property, which we expose below...

        BigBinaryValue ITimeSeriesValue.Value
        {
            get
            {
                return CommonStatusFlags;
            }
            set
            {
                switch (value.TypeCode)
                {
                    case TypeCode.Byte:
                        CommonStatusFlags = (uint)(Byte)value;
                        break;
                    case TypeCode.SByte:
                        CommonStatusFlags = (uint)(SByte)value;
                        break;
                    case TypeCode.Int16:
                        CommonStatusFlags = (uint)(Int16)value;
                        break;
                    case TypeCode.UInt16:
                        CommonStatusFlags = (UInt16)value;
                        break;
                    case TypeCode.Int32:
                        CommonStatusFlags = (uint)(Int32)value;
                        break;
                    case TypeCode.UInt32:
                        CommonStatusFlags = (UInt32)value;
                        break;
                    case TypeCode.Int64:
                        CommonStatusFlags = (uint)(Int64)value;
                        break;
                    case TypeCode.UInt64:
                        CommonStatusFlags = (uint)(UInt64)value;
                        break;
                    case TypeCode.Single:
                        CommonStatusFlags = (uint)(Single)value;
                        break;
                    case TypeCode.Double:
                        CommonStatusFlags = (uint)(Double)value;
                        break;
                    //case TypeCode.Boolean:
                    //    break;
                    //case TypeCode.Char:
                    //    break;
                    //case TypeCode.DateTime:
                    //    break;
                    //case TypeCode.Decimal:
                    //    break;
                    //case TypeCode.String:
                    //    m_value = double.Parse(value);
                    //    break;
                    default:
                        CommonStatusFlags = value;
                        break;
                }
            }
        }

        double ITimeSeriesValue<double>.Value
        {
            get
            {
                return CommonStatusFlags;
            }
            set
            {
                CommonStatusFlags = (uint)value;
            }
        }

        // The only "measured value" a data cell exposes is its "StatusFlags"
        double IMeasurement.AdjustedValue
        {
            get
            {
                return (double)CommonStatusFlags * m_multiplier + m_adder;
            }
        }

        // I don't imagine you would want offsets for status flags - but this may yet be handy for
        // "forcing" a particular set of quality flags to come through the system (M=0, A=New Flags)
        double IMeasurement.Adder
        {
            get
            {
                return m_adder;
            }
            set
            {
                m_adder = value;
            }
        }

        double IMeasurement.Multiplier
        {
            get
            {
                return m_multiplier;
            }
            set
            {
                m_multiplier = value;
            }
        }

        Ticks ITimeSeriesValue.Timestamp
        {
            get
            {
                if (m_timestamp == -1)
                    m_timestamp = Parent.Timestamp;

                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        Ticks IMeasurement.ReceivedTimestamp
        {
            get
            {
                return m_receivedTimestamp;
            }
            set
            {
                m_receivedTimestamp = value;
            }
        }

        Ticks IMeasurement.PublishedTimestamp
        {
            get
            {
                return m_publishedTimestamp;
            }
            set
            {
                m_publishedTimestamp = value;
            }
        }

        MeasurementKey IMeasurement.Key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;
            }
        }

        Guid ITimeSeriesValue.ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        MeasurementValueFilterFunction IMeasurement.MeasurementValueFilter
        {
            get
            {
                // If measurement user has assigned another filter for this measurement,
                // we'll use it instead
                if (m_measurementValueFilter != null)
                    return m_measurementValueFilter;

                // Otherwise, status flags are digital in nature, so we return a majority item filter
                return Measurement.MajorityValueFilter;
            }
            set
            {
                m_measurementValueFilter = value;
            }
        }

        MeasurementStateFlags IMeasurement.StateFlags
        {
            get
            {
                // The quality of the status flags "measurement" is always assumed to be good since it consists
                // of the flags that make up the actual quality of the incoming device data, as a result this
                // property will always return true so as to not affect archived data quality
                return m_stateFlags & ~MeasurementStateFlags.BadTime & ~MeasurementStateFlags.BadData;
            }
            set
            {
                m_stateFlags = value;

                // Updates to data quality are applied to status flags
                this.DataIsValid = (m_stateFlags & MeasurementStateFlags.BadData) == 0;

                // Updates to time quality are applied to status flags
                this.SynchronizationIsValid = (m_stateFlags & MeasurementStateFlags.BadTime) == 0;
            }
        }
        string IMeasurement.TagName
        {
            get
            {
                return m_tagName;
            }
            set
            {
                m_tagName = value;
            }
        }

        int ITimeSeriesValue.GetHashCode()
        {
            return m_id.GetHashCode();
        }

        int IComparable.CompareTo(object obj)
        {
            IMeasurement measurement = obj as IMeasurement;

            if (measurement != null)
                return (this as IComparable<IMeasurement>).CompareTo(measurement);

            throw new ArgumentException("Measurement can only be compared with other IMeasurements...");
        }

        int IComparable<ITimeSeriesValue>.CompareTo(ITimeSeriesValue other)
        {
            return (this as ITimeSeriesValue).GetHashCode().CompareTo(other.GetHashCode());
        }

        bool IEquatable<ITimeSeriesValue>.Equals(ITimeSeriesValue other)
        {
            return ((this as IComparable<ITimeSeriesValue>).CompareTo(other) == 0);
        }

        #endregion
    }
}