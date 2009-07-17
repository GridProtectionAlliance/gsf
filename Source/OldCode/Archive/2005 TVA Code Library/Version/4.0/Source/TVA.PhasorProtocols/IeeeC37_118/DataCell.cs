//*******************************************************************************************************
//  DataCell.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TVA.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IDataCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class DataCell : DataCellBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
            : base(parent, configurationCell, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Define new parsing state which defines constructors for key data values
            State = new DataCellParsingState(
                configurationCell,
                IeeeC37_118.PhasorValue.CreateNewValue,
                IeeeC37_118.FrequencyValue.CreateNewValue,
                IeeeC37_118.AnalogValue.CreateNewValue,
                IeeeC37_118.DigitalValue.CreateNewValue);
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

                // Define any analog values
                for (x = 0; x < configurationCell.AnalogDefinitions.Count; x++)
                {
                    AnalogValues.Add(new AnalogValue(this, configurationCell.AnalogDefinitions[x]));
                }

                // Define any digital values
                for (x = 0; x < configurationCell.DigitalDefinitions.Count; x++)
                {
                    DigitalValues.Add(new DigitalValue(this, configurationCell.DigitalDefinitions[x]));
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
        /// Gets or sets status flags for this <see cref="DataCell"/>.
        /// </summary>
        public new StatusFlags StatusFlags
        {
            get
            {
                return (StatusFlags)(base.StatusFlags & ~(ushort)(StatusFlags.UnlockedTimeMask | StatusFlags.TriggerReasonMask));
            }
            set
            {
                base.StatusFlags = (ushort)((base.StatusFlags & (ushort)(StatusFlags.UnlockedTimeMask | StatusFlags.TriggerReasonMask)) | (ushort)value);
            }
        }

        /// <summary>
        /// Gets or sets unlocked time of this <see cref="DataCell"/>.
        /// </summary>
        public UnlockedTime UnlockedTime
        {
            get
            {
                return (UnlockedTime)(base.StatusFlags & (ushort)StatusFlags.UnlockedTimeMask);
            }
            set
            {
                base.StatusFlags = (ushort)((base.StatusFlags & ~(ushort)StatusFlags.UnlockedTimeMask) | (ushort)value);
                SynchronizationIsValid = (value == IeeeC37_118.UnlockedTime.SyncLocked);
            }
        }

        /// <summary>
        /// Gets or sets trigger reason of this <see cref="DataCell"/>.
        /// </summary>
        public TriggerReason TriggerReason
        {
            get
            {
                return (TriggerReason)(base.StatusFlags & (short)StatusFlags.TriggerReasonMask);
            }
            set
            {
                base.StatusFlags = (ushort)((base.StatusFlags & ~(short)StatusFlags.TriggerReasonMask) | (ushort)value);
                DeviceTriggerDetected = (value != IeeeC37_118.TriggerReason.Manual);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get
            {
                return (StatusFlags & IeeeC37_118.StatusFlags.DataIsValid) == 0;
            }
            set
            {
                if (value)
                    StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.DataIsValid;
                else
                    StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.DataIsValid;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        public override bool SynchronizationIsValid
        {
            get
            {
                return (StatusFlags & IeeeC37_118.StatusFlags.DeviceSynchronizationError) == 0;
            }
            set
            {
                if (value)
                    StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.DeviceSynchronizationError;
                else
                    StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.DeviceSynchronizationError;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
        /// </summary>
        public override DataSortingType DataSortingType
        {
            get
            {
                return (((StatusFlags & IeeeC37_118.StatusFlags.DataSortingType) == 0) ? PhasorProtocols.DataSortingType.ByTimestamp : PhasorProtocols.DataSortingType.ByArrival);
            }
            set
            {
                if (value == PhasorProtocols.DataSortingType.ByTimestamp)
                    StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.DataSortingType;
                else
                    StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.DataSortingType;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        public override bool DeviceError
        {
            get
            {
                return (StatusFlags & IeeeC37_118.StatusFlags.DeviceError) > 0;
            }
            set
            {
                if (value)
                    StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.DeviceError;
                else
                    StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.DeviceError;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if device trigger is detected for this <see cref="DataCell"/>.
        /// </summary>
        public bool DeviceTriggerDetected
        {
            get
            {
                return (StatusFlags & IeeeC37_118.StatusFlags.DeviceTriggerDetected) > 0;
            }
            set
            {
                if (value)
                    StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.DeviceTriggerDetected;
                else
                    StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.DeviceTriggerDetected;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if configuration change was detected for this <see cref="DataCell"/>.
        /// </summary>
        public bool ConfigurationChangeDetected
        {
            get
            {
                return (StatusFlags & IeeeC37_118.StatusFlags.ConfigurationChanged) > 0;
            }
            set
            {
                if (value)
                    StatusFlags = StatusFlags | IeeeC37_118.StatusFlags.ConfigurationChanged;
                else
                    StatusFlags = StatusFlags & ~IeeeC37_118.StatusFlags.ConfigurationChanged;
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

                baseAttributes.Add("Unlocked Time", (int)UnlockedTime + ": " + UnlockedTime);
                baseAttributes.Add("Device Trigger Detected", DeviceTriggerDetected.ToString());
                baseAttributes.Add("Trigger Reason", (int)TriggerReason + ": " + TriggerReason);
                baseAttributes.Add("Configuration Change Detected", ConfigurationChangeDetected.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new DataCell(parent as IDataFrame, (state as IDataFrameParsingState).ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.Initialize(binaryImage, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}