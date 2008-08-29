//*******************************************************************************************************
//  DataCellBase.vb - Data cell base class
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TVA;
using TVA.Interop;
using TVA.Measurements;

namespace PhasorProtocols
{
    /// <summary>This class represents the protocol independent common implementation of a set of phasor related data values that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class DataCellBase : ChannelCellBase, IDataCell
    {
        private IConfigurationCell m_configurationCell;
        private short m_statusFlags;
        private PhasorValueCollection m_phasorValues;
        private IFrequencyValue m_frequencyValue;
        private AnalogValueCollection m_analogValues;
        private DigitalValueCollection m_digitalValues;

        #region " IMeasurement Implementation Members "

        private int m_id;
        private string m_source;
        private MeasurementKey m_key;
        private string m_tagName;
        private long m_ticks;
        private double m_adder;
        private double m_multiplier;

        #endregion

        protected DataCellBase()
        {
        }

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

        protected DataCellBase(IDataFrame parent, bool alignOnDWordBoundary, IConfigurationCell configurationCell, int maximumPhasors, int maximumAnalogs, int maximumDigitals)
            : base(parent, alignOnDWordBoundary)
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
            m_ticks = -1;
            m_multiplier = 1.0D;
        }

        protected DataCellBase(IDataFrame parent, bool alignOnDWordBoundary, int maximumPhasors, int maximumAnalogs, int maximumDigitals, IDataCellParsingState state, byte[] binaryImage, int startIndex)
            : this(parent, alignOnDWordBoundary, state.ConfigurationCell, maximumPhasors, maximumAnalogs, maximumDigitals)
        {
            ParseBinaryImage(state, binaryImage, startIndex);
        }

        protected DataCellBase(IDataFrame parent, bool alignOnDWordBoundary, IConfigurationCell configurationCell, short statusFlags, PhasorValueCollection phasorValues, IFrequencyValue frequencyValue, AnalogValueCollection analogValues, DigitalValueCollection digitalValues)
            : base(parent, alignOnDWordBoundary)
        {
            m_configurationCell = configurationCell;
            m_statusFlags = statusFlags;
            m_phasorValues = phasorValues;
            m_frequencyValue = frequencyValue;
            m_analogValues = analogValues;
            m_digitalValues = digitalValues;
        }

        // Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)

        // Derived classes are expected to expose a Public Sub New(ByVal dataCell As IDataCell)
        protected DataCellBase(IDataCell dataCell)
            : this(dataCell.Parent, dataCell.AlignOnDWordBoundary, dataCell.ConfigurationCell, dataCell.StatusFlags, dataCell.PhasorValues, dataCell.FrequencyValue, dataCell.AnalogValues, dataCell.DigitalValues)
        {
        }

        public virtual new IDataFrame Parent
        {
            get
            {
                return (IDataFrame)base.Parent;
            }
        }

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

        public virtual string StationName
        {
            get
            {
                return m_configurationCell.StationName;
            }
        }

        public virtual string IDLabel
        {
            get
            {
                return m_configurationCell.IDLabel;
            }
        }

        public override ushort IDCode
        {
            get
            {
                return m_configurationCell.IDCode;
            }
            set
            {
                throw (new NotSupportedException("Cannot change IDCode of a data cell, change IDCode is associated configuration cell instead"));
            }
        }

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

        public int CommonStatusFlags
        {
            get
            {
                // Start with lo-word protocol specific flags
                int commonFlags = StatusFlags;

                // Add hi-word protocol independent common flags
                if (!DataIsValid)
                {
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.DataIsValid;
                }
                if (!SynchronizationIsValid)
                {
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.SynchronizationIsValid;
                }
                if (DataSortingType != PhasorProtocols.DataSortingType.ByTimestamp)
                {
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.DataSortingType;
                }
                if (PmuError)
                {
                    commonFlags |= (int)PhasorProtocols.CommonStatusFlags.PmuError;
                }

                return commonFlags;
            }
            set
            {
                // Deriving common states requires clearing of base status flags...
                if (value != -1)
                {
                    StatusFlags = 0;
                }

                // Derive common states via common status flags
                DataIsValid = (value & (int)PhasorProtocols.CommonStatusFlags.DataIsValid) == 0;
                SynchronizationIsValid = (value & (int)PhasorProtocols.CommonStatusFlags.SynchronizationIsValid) == 0;
                DataSortingType = ((value & (int)PhasorProtocols.CommonStatusFlags.DataSortingType) == 0) ? PhasorProtocols.DataSortingType.ByTimestamp : PhasorProtocols.DataSortingType.ByArrival;
                PmuError = (value & (int)PhasorProtocols.CommonStatusFlags.PmuError) > 0;
            }
        }


        public abstract bool DataIsValid
        {
            get;
            set;
        }

        public abstract bool SynchronizationIsValid
        {
            get;
            set;
        }

        public abstract DataSortingType DataSortingType
        {
            get;
            set;
        }

        public abstract bool PmuError
        {
            get;
            set;
        }

        public virtual bool AllValuesAssigned
        {
            get
            {
                return (PhasorValues.AllValuesAssigned && (!FrequencyValue.IsEmpty) && AnalogValues.AllValuesAssigned && DigitalValues.AllValuesAssigned);
            }
        }

        public virtual PhasorValueCollection PhasorValues
        {
            get
            {
                return m_phasorValues;
            }
        }

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

        public virtual AnalogValueCollection AnalogValues
        {
            get
            {
                return m_analogValues;
            }
        }

        public virtual DigitalValueCollection DigitalValues
        {
            get
            {
                return m_digitalValues;
            }
        }

        protected override ushort BodyLength
        {
            get
            {
                return (ushort)(2 + m_phasorValues.BinaryLength + m_frequencyValue.BinaryLength + m_analogValues.BinaryLength + m_digitalValues.BinaryLength);
            }
        }

        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];
                int index = 0;

                // Copy in common cell image
                EndianOrder.BigEndian.CopyBytes(m_statusFlags, buffer, index);
                index += 2;

                PhasorProtocols.Common.CopyImage(m_phasorValues, buffer, ref index);
                PhasorProtocols.Common.CopyImage(m_frequencyValue, buffer, ref index);
                PhasorProtocols.Common.CopyImage(m_analogValues, buffer, ref index);
                PhasorProtocols.Common.CopyImage(m_digitalValues, buffer, ref index);

                return buffer;
            }
        }

        protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
        {
            IDataCellParsingState parsingState = (IDataCellParsingState)state;
            int x;

            StatusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
            startIndex += 2;

            // By the very nature of the three protocols supporting the same order of phasors, frequency, dfreq, analog and digitals
            // we are able to "automatically" parse this data out in the data cell base class - BEAUTIFUL!!!

            // Parse out phasor values
            for (x = 0; x <= parsingState.PhasorCount - 1; x++)
            {
                m_phasorValues.Add(parsingState.CreateNewPhasorValueFunction(this, m_configurationCell.PhasorDefinitions[x], binaryImage, startIndex));
                startIndex += m_phasorValues[x].BinaryLength;
            }

            // Parse out frequency and df/dt values
            m_frequencyValue = parsingState.CreateNewFrequencyValueFunction(this, m_configurationCell.FrequencyDefinition, binaryImage, startIndex);
            startIndex += m_frequencyValue.BinaryLength;

            // Parse out analog values
            for (x = 0; x <= parsingState.AnalogCount - 1; x++)
            {
                m_analogValues.Add(parsingState.CreateNewAnalogValueFunction(this, m_configurationCell.AnalogDefinitions[x], binaryImage, startIndex));
                startIndex += m_analogValues[x].BinaryLength;
            }

            // Parse out digital values
            for (x = 0; x <= parsingState.DigitalCount - 1; x++)
            {
                m_digitalValues.Add(parsingState.CreateNewDigitalValueFunction(this, m_configurationCell.DigitalDefinitions[x], binaryImage, startIndex));
                startIndex += m_digitalValues[x].BinaryLength;
            }
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
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
                baseAttributes.Add("PMU Error", PmuError.ToString());
                baseAttributes.Add("Total Phasor Values", PhasorValues.Count.ToString());
                baseAttributes.Add("Total Analog Values", AnalogValues.Count.ToString());
                baseAttributes.Add("Total Digital Values", DigitalValues.Count.ToString());
                baseAttributes.Add("All Values Assigned", AllValuesAssigned.ToString());

                return baseAttributes;
            }
        }

        #region " IMeasurement Implementation "

        // We keep the IMeasurement implementation of the DataCell completely private.  Exposing
        // these properties publically would only stand to add confusion as to where measurements
        // typically come from (i.e., the IDataCell's values) - the only value the cell itself has
        // to offer is the "CommonStatusFlags" property, which we expose below

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

        long IMeasurement.Ticks
        {
            get
            {
                if (m_ticks == -1)
                {
                    m_ticks = Parent.Ticks;
                }
                return m_ticks;
            }
            set
            {
                m_ticks = value;
            }
        }

        DateTime IMeasurement.Timestamp
        {
            get
            {
                long ticks = ((IMeasurement)this).Ticks;

                if (ticks == -1)
                {
                    return DateTime.MinValue;
                }
                else
                {
                    return new DateTime(ticks);
                }
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
            {
                return ((IComparable<IMeasurement>)this).CompareTo(measurement);
            }

            throw (new ArgumentException(DerivedType.Name + " measurement can only be compared with other IMeasurements..."));

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
