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
//  04/30/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IDataCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class DataCell : DataCellBase
    {
        #region [ Members ]

        // Fields
        private byte m_status1Flags;
        private byte m_status2Flags;
        private ClockStatusFlags m_clockStatusFlags;
        private ushort m_sampleNumber;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
            : base(parent, configurationCell, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
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
                for (x = 0; x < configurationCell.PhasorDefinitions.Count; x++)
                {
                    PhasorValues.Add(new PhasorValue(this, configurationCell.PhasorDefinitions[x]));
                }

                // Define a frequency and df/dt
                FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition);
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
            m_status1Flags = info.GetByte("status1Flags");
            m_status2Flags = info.GetByte("status2Flags");
            m_clockStatusFlags = (ClockStatusFlags)info.GetValue("clockStatusFlags", typeof(ClockStatusFlags));
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
        /// Gets the numeric ID code for this <see cref="DataCell"/>.
        /// </summary>
        public new uint IDCode
        {
            get
            {
                return ConfigurationCell.IDCode;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Macrodyne.ClockStatusFlags"/> for this <see cref="DataCell"/>.
        /// </summary>
        public ClockStatusFlags ClockStatusFlags
        {
            get
            {
                return m_clockStatusFlags;
            }
            set
            {
                m_clockStatusFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets status 1 flags for this <see cref="DataCell"/>.
        /// </summary>
        public StatusFlags Status1Flags
        {
            get
            {
                return (StatusFlags)m_status1Flags;
            }
            set
            {
                m_status1Flags = (byte)value;
                base.StatusFlags = Word.MakeWord(m_status1Flags, m_status2Flags);
            }
        }

        /// <summary>
        /// Gets or sets status 2 flags for this <see cref="DataCell"/>.
        /// </summary>
        public byte Status2Flags
        {
            get
            {
                return m_status2Flags;
            }
            set
            {
                m_status2Flags = value;
                base.StatusFlags = Word.MakeWord(m_status1Flags, m_status2Flags);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Macrodyne.TriggerReason"/> for this <see cref="DataCell"/>.
        /// </summary>
        public TriggerReason TriggerReason
        {
            get
            {
                return (TriggerReason)(m_status2Flags & (byte)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02));
            }
            set
            {
                m_status2Flags = (byte)((m_status2Flags & ~(byte)(Bits.Bit00 | Bits.Bit01 | Bits.Bit02)) | (byte)value);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Macrodyne.GpsStatus"/> for this <see cref="DataCell"/>.
        /// </summary>
        public GpsStatus GpsStatus
        {
            get
            {
                return (GpsStatus)(m_status2Flags & (byte)(Bits.Bit03 | Bits.Bit04));
            }
            set
            {
                m_status2Flags = (byte)((m_status2Flags & ~(byte)(Bits.Bit03 | Bits.Bit04)) | (byte)value);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get
            {
                return !((Status1Flags & Macrodyne.StatusFlags.OperationalLimitReached) > 0);
            }
            set
            {
                if (value)
                    Status1Flags = Status1Flags & ~Macrodyne.StatusFlags.OperationalLimitReached;
                else
                    Status1Flags = Status1Flags | Macrodyne.StatusFlags.OperationalLimitReached;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        public override bool SynchronizationIsValid
        {
            get
            {
                return !((Status1Flags & Macrodyne.StatusFlags.TimeError) > 0);
            }
            set
            {
                if (value)
                    Status1Flags = Status1Flags & ~Macrodyne.StatusFlags.TimeError;
                else
                    Status1Flags = Status1Flags | Macrodyne.StatusFlags.TimeError;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
        /// </summary>
        public override DataSortingType DataSortingType
        {
            get
            {
                return (SynchronizationIsValid ? PhasorProtocols.DataSortingType.ByTimestamp : PhasorProtocols.DataSortingType.ByArrival);
            }
            set
            {
                // We just ignore this value as we have defined data sorting type as a derived value based on synchronization validity
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        /// <remarks>Macrodyne doesn't define bits for device error.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool DeviceError
        {
            get
            {
                return false;
            }
            set
            {
                // We just ignore this value as Macrodyne defines no flags for data errors
            }
        }

        /// <summary>
        /// Gets <see cref="AnalogValueCollection"/> of this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// Macrodyne doesn't define any analog values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override AnalogValueCollection AnalogValues
        {
            get
            {
                return base.AnalogValues;
            }
        }

        /// <summary>
        /// Gets <see cref="DigitalValueCollection"/> of this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// Macrodyne doesn't define any digital values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DigitalValueCollection DigitalValues
        {
            get
            {
                return base.DigitalValues;
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

                baseAttributes.Add("Status 1 Flags", (int)Status1Flags + ": " + Status1Flags);
                baseAttributes.Add("Status 2 Flags", Status2Flags.ToString());
                baseAttributes.Add("Clock Status Flags", (int)ClockStatusFlags + ": " + ClockStatusFlags);
                baseAttributes.Add("GPS Status", (int)GpsStatus + ": " + GpsStatus);
                baseAttributes.Add("Trigger Reason", (int)TriggerReason + ": " + TriggerReason);

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
            ConfigurationCell configCell = ConfigurationCell;
            ConfigurationFrame configFrame = configCell.Parent;
            IPhasorValue phasorValue;
            IDigitalValue digitalValue;
            int x, parsedLength, index = startIndex;

            m_status1Flags = binaryImage[index];
            index++;

            // Parse out status 2 flags
            if (configFrame.Status2Included)
            {
                m_status2Flags = binaryImage[index];
                index++;
            }
            else
                m_status2Flags = 0;

            base.StatusFlags = Word.MakeWord(m_status1Flags, m_status2Flags);

            // Parse out time tag
            if (configFrame.TimestampIncluded)
            {
                m_clockStatusFlags = (ClockStatusFlags)binaryImage[index];
                index += 1;

                ushort day = BinaryCodedDecimal.Decode(EndianOrder.BigEndian.ToUInt16(binaryImage, index));
                byte hours = BinaryCodedDecimal.Decode(binaryImage[index + 2]);
                byte minutes = BinaryCodedDecimal.Decode(binaryImage[index + 3]);
                byte seconds = BinaryCodedDecimal.Decode(binaryImage[index + 4]);

                m_sampleNumber = EndianOrder.BigEndian.ToUInt16(binaryImage, index + 5);
                index += 7;

                // TODO: Think about how to handle year change with floating clock...
                // Calculate timestamp
                Parent.Timestamp = new DateTime(DateTime.UtcNow.Year, 1, 1).AddDays(day - 1).AddHours(hours).AddMinutes(minutes).AddSeconds(seconds + m_sampleNumber / 719.0D);
            }
            else
            {
                Parent.Timestamp = DateTime.UtcNow.Ticks;
                SynchronizationIsValid = false;  // TODO: Handle "assigned value" - change flags?

                m_sampleNumber = EndianOrder.BigEndian.ToUInt16(binaryImage, index);
                index += 2;
            }

            // Parse out first five phasor values
            for (x = 0; x < TVA.Common.Min(configCell.PhasorDefinitions.Count, 5); x++)
            {
                phasorValue = Macrodyne.PhasorValue.CreateNewValue(this, configCell.PhasorDefinitions[x], binaryImage, index, out parsedLength);
                PhasorValues.Add(phasorValue);
                index += parsedLength;
            }

            // Parse out frequency value
            FrequencyValue = Macrodyne.FrequencyValue.CreateNewValue(this, configCell.FrequencyDefinition, binaryImage, index, out parsedLength);
            index += parsedLength;

            if (configFrame.ReferenceIncluded)
            {
                // TODO: Parse reference phasor...
                index += 3;
            }

            if (configFrame.Digital1Included)
            {
                digitalValue = DigitalValue.CreateNewValue(this, configCell.DigitalDefinitions[0], binaryImage, index, out parsedLength);
                DigitalValues.Add(digitalValue);
                index += parsedLength;
            }

            // Parse out next five phasor values
            for (x = 5; x < configCell.PhasorDefinitions.Count; x++)
            {
                phasorValue = Macrodyne.PhasorValue.CreateNewValue(this, configCell.PhasorDefinitions[x], binaryImage, index, out parsedLength);
                PhasorValues.Add(phasorValue);
                index += parsedLength;
            }

            if (configFrame.Digital2Included)
            {
                digitalValue = DigitalValue.CreateNewValue(this, configCell.DigitalDefinitions[configCell.DigitalDefinitions.Count - 1], binaryImage, index, out parsedLength);
                DigitalValues.Add(digitalValue);
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

            // Serialize data cell
            info.AddValue("status1Flags", m_status1Flags);
            info.AddValue("status2Flags", m_status2Flags);
            info.AddValue("clockStatusFlags", m_clockStatusFlags, typeof(ClockStatusFlags));
            info.AddValue("sampleNumber", m_sampleNumber);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new Macrodyne data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new DataCell(parent as IDataFrame, (state as IDataFrameParsingState).ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.Initialize(binaryImage, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}