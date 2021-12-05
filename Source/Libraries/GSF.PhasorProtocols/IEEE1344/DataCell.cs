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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.IEEE1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IDataCell"/> that can be sent or received.
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
                IEEE1344.FrequencyValue.CreateNewValue,
                null, // IEEE 1344 doesn't define analogs
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
            if (!addEmptyValues)
                return;

            // Define needed phasor values
            foreach (IPhasorDefinition phasorDefinition in configurationCell.PhasorDefinitions)
                PhasorValues.Add(new PhasorValue(this, phasorDefinition));

            // Define a frequency and df/dt
            FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition);

            // Define any digital values
            foreach (IDigitalDefinition digitalDefinition in configurationCell.DigitalDefinitions)
                DigitalValues.Add(new DigitalValue(this, digitalDefinition));
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
        /// Gets the numeric ID code for this <see cref="DataCell"/>.
        /// </summary>
        public new ulong IDCode => ConfigurationCell.IDCode;

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get => (StatusFlags & (ushort)Bits.Bit14) == 0;
            set
            {
                if (value)
                    StatusFlags = (ushort)(StatusFlags & ~(ushort)Bits.Bit14);
                else
                    StatusFlags = (ushort)(StatusFlags | (ushort)Bits.Bit14);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        public override bool SynchronizationIsValid
        {
            get => (StatusFlags & (ushort)Bits.Bit15) == 0;
            set
            {
                if (value)
                    StatusFlags = (ushort)(StatusFlags & ~(ushort)Bits.Bit15);
                else
                    StatusFlags = (ushort)(StatusFlags | (ushort)Bits.Bit15);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
        /// </summary>
        public override DataSortingType DataSortingType
        {
            get => SynchronizationIsValid ? DataSortingType.ByTimestamp : DataSortingType.ByArrival;
            set
            {
                // We just ignore this value as we have defined data sorting type as a derived value based on synchronization validity
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        /// <remarks>IEEE 1344 doesn't define bits for device error.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool DeviceError
        {
            get => false;
            set
            {
                // We just ignore this value as IEEE 1344 defines no flags for data errors
            }
        }

        /// <summary>
        /// Gets or sets trigger status of this <see cref="DataCell"/>.
        /// </summary>
        public TriggerStatus TriggerStatus
        {
            get => (TriggerStatus)(StatusFlags & Common.TriggerMask);
            set => StatusFlags = (ushort)((StatusFlags & ~Common.TriggerMask) | (ushort)value);
        }

        /// <summary>
        /// Gets <see cref="AnalogValueCollection"/> of this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// IEEE 1344 doesn't define any analog values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override AnalogValueCollection AnalogValues => base.AnalogValues;

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Trigger Status", $"{(int)TriggerStatus}: {TriggerStatus}");

                return baseAttributes;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE 1344 data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new(parent as IDataFrame, (state as IDataFrameParsingState)?.ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.ParseBinaryImage(buffer, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}