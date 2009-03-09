//*******************************************************************************************************
//  DataCellBase.cs
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
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PCS.Measurements;

namespace PCS.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Protocol independent common status flags enumeration.
    /// </summary>
    /// <remarks>
    /// These flags are expected to exist in the hi-word of a double-word flag set such that original word flags remain in-tact
    /// in lo-word of double-word flag set.
    /// </remarks>
    [Flags(), Serializable()]
    public enum CommonStatusFlags
    {
        /// <summary>
        /// Data is valid (0 when PMU data is valid, 1 when invalid or PMU is in test mode).
        /// </summary>
        DataIsValid = Bit.Bit19,
        /// <summary>
        /// Synchronization is valid (0 when in PMU is in sync, 1 when it is not).
        /// </summary>
        SynchronizationIsValid = Bit.Bit18,
        /// <summary>
        /// Data sorting type, 0 by timestamp, 1 by arrival.
        /// </summary>
        DataSortingType = Bit.Bit17,
        /// <summary>
        /// Device error (including configuration error), 0 when no error.
        /// </summary>
        DeviceError = Bit.Bit16,
        /// <summary>
        /// Reserved bits for future common flags, presently set to 0.
        /// </summary>
        ReservedFlags = Bit.Bit20 | Bit.Bit21 | Bit.Bit22 | Bit.Bit23 | Bit.Bit24 | Bit.Bit25 | Bit.Bit26 | Bit.Bit27 | Bit.Bit28 | Bit.Bit29 | Bit.Bit30 | Bit.Bit31
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
        private short m_statusFlags;
        private PhasorValueCollection m_phasorValues;
        private IFrequencyValue m_frequencyValue;
        private AnalogValueCollection m_analogValues;
        private DigitalValueCollection m_digitalValues;

        // IMeasurement implementation fields
        private int m_id;
        private string m_source;
        private MeasurementKey m_key;
        private string m_tagName;
        private Ticks m_timestamp;
        private double m_adder;
        private double m_multiplier;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCellBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCellBase"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCellBase"/>.</param>
        /// <param name="alignOnDWordBoundary">A flag that determines if the <see cref="DataCellBase"/> is aligned on a double-word (i.e., 32-bit) boundry.</param>
        /// <param name="maximumPhasors">Sets the maximum number of phasors for the <see cref="PhasorValues"/> collection.</param>
        /// <param name="maximumAnalogs">Sets the maximum number of phasors for the <see cref="AnalogValues"/> collection.</param>
        /// <param name="maximumDigitals">Sets the maximum number of phasors for the <see cref="DigitalValues"/> collection.</param>
        protected DataCellBase(IDataFrame parent, IConfigurationCell configurationCell, bool alignOnDWordBoundary, int maximumPhasors, int maximumAnalogs, int maximumDigitals)
            : base(parent, alignOnDWordBoundary, 0)
        {
            m_configurationCell = configurationCell;
            m_statusFlags = -1;
            m_phasorValues = new PhasorValueCollection(maximumPhasors);
            m_analogValues = new AnalogValueCollection(maximumAnalogs);
            m_digitalValues = new DigitalValueCollection(maximumDigitals);

            // Initialize IMeasurement members
            m_id = -1;
            m_source = "__";
            m_key = PhasorProtocols.Common.UndefinedKey;
            m_timestamp = -1;
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
            m_statusFlags = info.GetInt16("statusFlags");
            m_phasorValues = (PhasorValueCollection)info.GetValue("phasorValues", typeof(PhasorValueCollection));
            m_frequencyValue = (IFrequencyValue)info.GetValue("frequencyValue", typeof(IFrequencyValue));
            m_analogValues = (AnalogValueCollection)info.GetValue("analogValues", typeof(AnalogValueCollection));
            m_digitalValues = (DigitalValueCollection)info.GetValue("digitalValues", typeof(DigitalValueCollection));
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
                return m_configurationCell.StationName;
            }
        }

        /// <summary>
        /// Gets ID label of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual string IDLabel
        {
            get
            {
                return m_configurationCell.IDLabel;
            }
        }

        /// <summary>
        /// Gets or sets 16-bit status flags of this <see cref="DataCellBase"/>.
        /// </summary>
        public virtual short StatusFlags
        {
            get
            {
                return m_statusFlags;
            }
            set
            {
                m_statusFlags = value;
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
                return m_configurationCell.IDCode;
            }
            set
            {
                throw new NotSupportedException("IDCode of a data cell is read-only, change IDCode is associated configuration cell instead");
            }
        }

        /// <summary>
        /// Gets or sets common status flags of this <see cref="DataCellBase"/>.
        /// </summary>
        public int CommonStatusFlags
        {
            get
            {
                // Start with lo-word protocol specific flags
                int commonFlags = StatusFlags;

                // Add hi-word protocol independent common flags
                if (!DataIsValid)
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.DataIsValid;

                if (!SynchronizationIsValid)
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.SynchronizationIsValid;

                if (DataSortingType != PhasorProtocols.DataSortingType.ByTimestamp)
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.DataSortingType;

                if (DeviceError)
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.DeviceError;

                return commonFlags;
            }
            set
            {
                // Deriving common states requires clearing of base status flags...
                if (value != -1)
                    StatusFlags = 0;

                // Derive common states via common status flags
                DataIsValid = (value & (int)PhasorProtocols.CommonStatusFlags.DataIsValid) == 0;
                SynchronizationIsValid = (value & (int)PhasorProtocols.CommonStatusFlags.SynchronizationIsValid) == 0;
                DataSortingType = ((value & (int)PhasorProtocols.CommonStatusFlags.DataSortingType) == 0) ? PhasorProtocols.DataSortingType.ByTimestamp : PhasorProtocols.DataSortingType.ByArrival;
                DeviceError = (value & (int)PhasorProtocols.CommonStatusFlags.DeviceError) > 0;
            }
        }

        /// <summary>
        /// Gets flag that determines if all values of this <see cref="DataCellBase"/> have been assigned.
        /// </summary>
        public virtual bool AllValuesAssigned
        {
            get
            {
                return (PhasorValues.AllValuesAssigned && !FrequencyValue.IsEmpty && AnalogValues.AllValuesAssigned && DigitalValues.AllValuesAssigned);
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

                baseAttributes.Add("Station Name", StationName);
                baseAttributes.Add("ID Label", IDLabel);
                baseAttributes.Add("Status Flags", StatusFlags.ToString());
                baseAttributes.Add("Data Is Valid", DataIsValid.ToString());
                baseAttributes.Add("Synchronization Is Valid", SynchronizationIsValid.ToString());
                baseAttributes.Add("Data Sorting Type", Enum.GetName(typeof(DataSortingType), DataSortingType));
                baseAttributes.Add("Device Error", DeviceError.ToString());
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
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] binaryImage, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            IDataCellParsingState parsingState = State;
            IPhasorValue phasorValue;
            IAnalogValue analogValue;
            IDigitalValue digitalValue;
            int x, parsedLength, index = startIndex;

            StatusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
            index += 2;

            // By the very nature of the major phasor protocols supporting the same order of phasors, frequency, df/dt, analog and digitals
            // we are able to "automatically" parse this data out in the data cell base class - BEAUTIFUL!!!

            // Parse out phasor values
            for (x = 0; x < parsingState.PhasorCount; x++)
            {
                phasorValue = parsingState.CreateNewPhasorValue(this, m_configurationCell.PhasorDefinitions[x], binaryImage, index, out parsedLength);
                m_phasorValues.Add(phasorValue);
                index += parsedLength;
            }

            // Parse out frequency and df/dt values
            m_frequencyValue = parsingState.CreateNewFrequencyValue(this, m_configurationCell.FrequencyDefinition, binaryImage, index, out parsedLength);
            index += parsedLength;

            // Parse out analog values
            for (x = 0; x < parsingState.AnalogCount; x++)
            {
                analogValue = parsingState.CreateNewAnalogValue(this, m_configurationCell.AnalogDefinitions[x], binaryImage, index, out parsedLength);
                m_analogValues.Add(analogValue);
                index += parsedLength;
            }

            // Parse out digital values
            for (x = 0; x < parsingState.DigitalCount; x++)
            {
                digitalValue = parsingState.CreateNewDigitalValue(this, m_configurationCell.DigitalDefinitions[x], binaryImage, index, out parsedLength);
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
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
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

        #endregion

        #region [ IMeasurement Implementation ]

        // We keep the IMeasurement implementation of the DataCell completely private.  Exposing
        // these properties publically would only stand to add confusion as to where measurements
        // typically come from (i.e., the IDataCell's values) - the only value the cell itself has
        // to offer is the "CommonStatusFlags" property, which we expose below...

        double IMeasurement.Value
        {
            get
            {
                return CommonStatusFlags;
            }
            set
            {
                CommonStatusFlags = (int)value;
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

        Ticks IMeasurement.Timestamp
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

        int IMeasurement.ID
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

        string IMeasurement.Source
        {
            get
            {
                return m_source;
            }
            set
            {
                m_source = value;
            }
        }

        MeasurementKey IMeasurement.Key
        {
            get
            {
                if (m_key.Equals(PhasorProtocols.Common.UndefinedKey))
                {
                    m_key = new MeasurementKey(m_id, m_source);
                }
                return m_key;
            }
        }

        bool IMeasurement.ValueQualityIsGood
        {
            get
            {
                return this.DataIsValid;
            }
            set
            {
                this.DataIsValid = value;
            }
        }

        bool IMeasurement.TimestampQualityIsGood
        {
            get
            {
                return this.SynchronizationIsValid;
            }
            set
            {
                this.SynchronizationIsValid = value;
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

        int IComparable.CompareTo(object obj)
        {
            IMeasurement measurement = obj as IMeasurement;

            if (measurement != null)
                return ((IComparable<IMeasurement>)this).CompareTo(measurement);

            throw new ArgumentException("Measurement can only be compared with other IMeasurements...");
        }

        int IComparable<IMeasurement>.CompareTo(IMeasurement other)
        {
            return ((IMeasurement)this).Value.CompareTo(other.Value);
        }

        bool IEquatable<IMeasurement>.Equals(IMeasurement other)
        {
            return (((IComparable<IMeasurement>)this).CompareTo(other) == 0);
        }

        #endregion
    }
}