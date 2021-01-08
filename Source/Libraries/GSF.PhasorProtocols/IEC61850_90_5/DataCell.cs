//******************************************************************************************************
//  DataCell.cs - Gbtc
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
//  04/19/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols.IEC61850_90_5
{
    /// <summary>
    /// Represents the IEC 61850-90-5 implementation of a <see cref="IDataCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class DataCell : DataCellBase
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
            : base(parent, configurationCell, 0x0000, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Define new parsing state which defines constructors for key data values
            State = new DataCellParsingState(
                configurationCell,
                PhasorValue.CreateNewValue,
                IEC61850_90_5.FrequencyValue.CreateNewValue,
                AnalogValue.CreateNewValue,
                DigitalValue.CreateNewValue);
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
            get => base.Parent as DataFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.
        /// </summary>
        public new ConfigurationCell ConfigurationCell
        {
            get => base.ConfigurationCell as ConfigurationCell;
            set => base.ConfigurationCell = value;
        }

        /// <summary>
        /// Gets or sets status flags for this <see cref="DataCell"/>.
        /// </summary>
        public new StatusFlags StatusFlags
        {
            get => (StatusFlags)(base.StatusFlags & ~(ushort)(StatusFlags.UnlockedTimeMask | StatusFlags.TriggerReasonMask));
            set => base.StatusFlags = (ushort)((base.StatusFlags & (ushort)(StatusFlags.UnlockedTimeMask | StatusFlags.TriggerReasonMask)) | (ushort)value);
        }

        /// <summary>
        /// Gets or sets unlocked time of this <see cref="DataCell"/>.
        /// </summary>
        public UnlockedTime UnlockedTime
        {
            get => (UnlockedTime)(base.StatusFlags & (ushort)StatusFlags.UnlockedTimeMask);
            set
            {
                base.StatusFlags = (ushort)((base.StatusFlags & ~(ushort)StatusFlags.UnlockedTimeMask) | (ushort)value);
                SynchronizationIsValid = value == UnlockedTime.SyncLocked;
            }
        }

        /// <summary>
        /// Gets or sets trigger reason of this <see cref="DataCell"/>.
        /// </summary>
        public TriggerReason TriggerReason
        {
            get => (TriggerReason)(base.StatusFlags & (short)StatusFlags.TriggerReasonMask);
            set
            {
                base.StatusFlags = (ushort)((base.StatusFlags & ~(short)StatusFlags.TriggerReasonMask) | (ushort)value);
                DeviceTriggerDetected = value != TriggerReason.Manual;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get =>
                // TODO: Should data be considered invalid when signature check fails? On my test device this would always be invalid since SHA is not being calculated...
                (StatusFlags & StatusFlags.DataIsValid) == 0;
            set
            {
                if (value)
                    StatusFlags = StatusFlags & ~StatusFlags.DataIsValid;
                else
                    StatusFlags = StatusFlags | StatusFlags.DataIsValid;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        public override bool SynchronizationIsValid
        {
            get =>
                // TODO: Not sure which synchronization flag should take priority here - so using both for now...
                (StatusFlags & StatusFlags.DeviceSynchronizationError) == 0 && Parent.SampleSynchronization > 0;
            set
            {
                if (value)
                    StatusFlags = StatusFlags & ~StatusFlags.DeviceSynchronizationError;
                else
                    StatusFlags = StatusFlags | StatusFlags.DeviceSynchronizationError;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="GSF.PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
        /// </summary>
        public override DataSortingType DataSortingType
        {
            get => (StatusFlags & StatusFlags.DataSortingType) == 0 ? DataSortingType.ByTimestamp : DataSortingType.ByArrival;
            set
            {
                if (value == DataSortingType.ByTimestamp)
                    StatusFlags = StatusFlags & ~StatusFlags.DataSortingType;
                else
                    StatusFlags = StatusFlags | StatusFlags.DataSortingType;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        public override bool DeviceError
        {
            get => (StatusFlags & StatusFlags.DeviceError) > 0;
            set
            {
                if (value)
                    StatusFlags = StatusFlags | StatusFlags.DeviceError;
                else
                    StatusFlags = StatusFlags & ~StatusFlags.DeviceError;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if device trigger is detected for this <see cref="DataCell"/>.
        /// </summary>
        public bool DeviceTriggerDetected
        {
            get => (StatusFlags & StatusFlags.DeviceTriggerDetected) > 0;
            set
            {
                if (value)
                    StatusFlags = StatusFlags | StatusFlags.DeviceTriggerDetected;
                else
                    StatusFlags = StatusFlags & ~StatusFlags.DeviceTriggerDetected;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if configuration change was detected for this <see cref="DataCell"/>.
        /// </summary>
        public bool ConfigurationChangeDetected
        {
            get => (StatusFlags & StatusFlags.ConfigurationChanged) > 0;
            set
            {
                if (value)
                    StatusFlags = StatusFlags | StatusFlags.ConfigurationChanged;
                else
                    StatusFlags = StatusFlags & ~StatusFlags.ConfigurationChanged;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ChannelFrameBase{T}"/> object.
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

        // Delegate handler to create a new IEC 61850-90-5 data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            IDataFrameParsingState parsingState = state as IDataFrameParsingState;
            IConfigurationCell configurationCell = null;

            // With or without an associated configuration, we'll parse the data cell
            if (!(parsingState?.ConfigurationFrame is null))
                configurationCell = parsingState.ConfigurationFrame.Cells[index];

            DataCell dataCell = new DataCell(parent as IDataFrame, configurationCell);

            parsedLength = dataCell.ParseBinaryImage(buffer, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}