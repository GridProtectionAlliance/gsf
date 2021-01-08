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
//  04/27/2009 - J. Ritchie Carroll
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

namespace GSF.PhasorProtocols.SelFastMessage
{
    /// <summary>
    /// Represents the SEL Fast Message implementation of a <see cref="IDataCell"/> that can be sent or received.
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
            : base(parent, configurationCell, (ushort)(StatusFlags.TSOK | StatusFlags.PMDOK), Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
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
        public new uint IDCode => ConfigurationCell.IDCode;

        /// <summary>
        /// Gets or sets status flags for this <see cref="DataCell"/>.
        /// </summary>
        public new StatusFlags StatusFlags
        {
            get => (StatusFlags)base.StatusFlags;
            set => base.StatusFlags = (ushort)value;
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get => (StatusFlags & StatusFlags.PMDOK) > 0;
            set
            {
                if (value)
                    StatusFlags = StatusFlags | StatusFlags.PMDOK;
                else
                    StatusFlags = StatusFlags & ~StatusFlags.PMDOK;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        public override bool SynchronizationIsValid
        {
            get => (StatusFlags & StatusFlags.TSOK) > 0;
            set
            {
                if (value)
                    StatusFlags = StatusFlags | StatusFlags.TSOK;
                else
                    StatusFlags = StatusFlags & ~StatusFlags.TSOK;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="GSF.PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
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
        /// <remarks>SEL Fast Message doesn't define bits for device error.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool DeviceError
        {
            get => false;
            set
            {
                // We just ignore this value as SEL Fast Message defines no flags for data errors
            }
        }

        /// <summary>
        /// Gets <see cref="AnalogValueCollection"/> of this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// SEL Fast Message doesn't define any analog values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override AnalogValueCollection AnalogValues => base.AnalogValues;

        /// <summary>
        /// Gets <see cref="DigitalValueCollection"/> of this <see cref="DataCell"/>.
        /// </summary>
        /// <remarks>
        /// SEL Fast Message doesn't define any digital values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DigitalValueCollection DigitalValues => base.DigitalValues;

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("SEL Status Flags", (int)StatusFlags + ": " + StatusFlags);

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
            ConfigurationCell configCell = ConfigurationCell;
            IPhasorValue phasorValue;
            int x, parsedLength, index = startIndex;

            // Parse out frequency value
            FrequencyValue = SelFastMessage.FrequencyValue.CreateNewValue(this, configCell.FrequencyDefinition, buffer, index, out parsedLength);
            index += parsedLength;

            // Parse out phasor values
            for (x = 0; x < configCell.PhasorDefinitions.Count; x++)
            {
                phasorValue = PhasorValue.CreateNewValue(this, configCell.PhasorDefinitions[x], buffer, index, out parsedLength);
                PhasorValues.Add(phasorValue);
                index += parsedLength;
            }

            // Parse out status flags
            StatusFlags = (StatusFlags)BigEndian.ToUInt16(buffer, index);
            index += 2;

            // Return total parsed length
            return index - startIndex;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new SEL Fast Message data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new DataCell(parent as IDataFrame, (state as IDataFrameParsingState).ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.ParseBinaryImage(buffer, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}