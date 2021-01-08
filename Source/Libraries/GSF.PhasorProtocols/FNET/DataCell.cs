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
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
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
using System.Text;
using GSF.Units;

namespace GSF.PhasorProtocols.FNET
{
    /// <summary>
    /// Represents the F-NET implementation of a <see cref="IDataCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class DataCell : DataCellBase
    {
        #region [ Members ]

        // Fields
        private double m_analogValue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
            : base(parent, configurationCell, 0x0000, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Initialize single phasor value and frequency value with an empty value
            PhasorValues.Add(new PhasorValue(this, configurationCell.PhasorDefinitions[0]));

            // Initialize frequency and df/dt
            FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition);
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
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get => true;
            set
            {
                // We just ignore updates to this value; F-NET defines no flags to determine if data is valid
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        /// <remarks>
        /// F-NET defines synchronization validity as a derived value based on the number of available satellites, i.e.,
        /// synchronization is valid if number of visible sattellites is greater than zero.
        /// </remarks>
        public override bool SynchronizationIsValid
        {
            get => ConfigurationCell.NumberOfSatellites > 0;
            set
            {
                // We just ignore updates to this value; F-NET defines synchronization validity as a derived value based on the number of available satellites
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
                // We just ignore updates to this value; data sorting type has been defined as a derived value based on synchronization validity
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        /// <remarks>F-NET doesn't define any flags for device errors.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool DeviceError
        {
            get => false;
            set
            {
                // We just ignore updates to this value; F-NET defines no flags for data errors
            }
        }

        /// <summary>
        /// Gets date in F-NET format.
        /// </summary>
        public string FNetDate => ((DateTime)Parent.Timestamp).ToString("MMddyy");

        /// <summary>
        /// Gets time in F-NET format.
        /// </summary>
        public string FNetTime => ((DateTime)Parent.Timestamp).ToString("HHmmss");

        /// <summary>
        /// Gets or sets analog value for F-NET data row.
        /// </summary>
        public double AnalogValue
        {
            get => m_analogValue;
            set => m_analogValue = value;
        }

        /// <summary>
        /// Gets image of this <see cref="DataCell"/> as a F-NET formatted data row.
        /// </summary>
        public string FNetDataRow
        {
            get
            {
                StringBuilder dataRow = new StringBuilder();

                dataRow.Append(Common.StartByte);
                dataRow.Append(IDCode);
                dataRow.Append(' ');
                dataRow.Append(FNetDate);
                dataRow.Append(' ');
                dataRow.Append(FNetTime);
                dataRow.Append(' ');
                dataRow.Append(Parent.SampleIndex);
                dataRow.Append(' ');
                dataRow.Append(m_analogValue);
                dataRow.Append(' ');
                dataRow.Append(FrequencyValue.Frequency);
                dataRow.Append(' ');
                dataRow.Append(PhasorValues[0].Magnitude);
                dataRow.Append(' ');
                dataRow.Append(PhasorValues[0].Angle);
                dataRow.Append(Common.EndByte);

                return dataRow.ToString();
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => FNetDataRow.Length;

        /// <summary>
        /// Gets the binary body image of the <see cref="DataCell"/> object.
        /// </summary>
        protected override byte[] BodyImage => Encoding.ASCII.GetBytes(FNetDataRow);

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("F-NET Date", FNetDate);
                baseAttributes.Add("F-NET Time", FNetTime);
                baseAttributes.Add("Analog Value", m_analogValue.ToString());

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
        /// <remarks>
        /// The longitude, latitude and number of satellites arrive at the top of minute in F-NET data as the analog
        /// data in a single row, each on their own row, as sample 1, 2, and 3 respectively.
        /// </remarks>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            DataFrame parent = Parent;
            CommonFrameHeader commonHeader = parent.CommonHeader;
            string[] data = commonHeader.DataElements;
            ConfigurationCell configurationCell = ConfigurationCell;
            uint sampleIndex;

            // Attempt to parse sample index
            if (uint.TryParse(data[Element.SampleIndex], out sampleIndex))
            {
                parent.SampleIndex = sampleIndex;

                // Get timestamp of data record
                parent.Timestamp = configurationCell.TimeOffset + ParseTimestamp(data[Element.Date], data[Element.Time], parent.SampleIndex, configurationCell.FrameRate);

                // Parse out first analog value (can be long/lat at top of minute)
                m_analogValue = double.Parse(data[Element.Analog]);

                if (data[Element.Time].Length >= 7 && int.Parse(data[Element.Time].Substring(4, 2)) == 0)
                {
                    switch (parent.SampleIndex)
                    {
                        case 1:
                            configurationCell.Latitude = m_analogValue;
                            break;
                        case 2:
                            configurationCell.Longitude = m_analogValue;
                            break;
                        case 3:
                            configurationCell.NumberOfSatellites = (int)m_analogValue;
                            break;
                    }
                }

                // Update (or create) frequency value
                double frequency = double.Parse(data[Element.Frequency]);

                if (FrequencyValue is null)
                    FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition as FrequencyDefinition, frequency, 0.0D);
                else
                    FrequencyValue.Frequency = frequency;

                // Update (or create) phasor value
                Angle angle = double.Parse(data[Element.Angle]);
                double magnitude = double.Parse(data[Element.Voltage]);
                PhasorValue phasor = null;

                if (PhasorValues.Count > 0)
                    phasor = PhasorValues[0] as PhasorValue;

                if (phasor is null)
                {
                    phasor = new PhasorValue(this, configurationCell.PhasorDefinitions[0] as PhasorDefinition, angle, magnitude);
                    PhasorValues.Add(phasor);
                }
                else
                {
                    phasor.Angle = angle;
                    phasor.Magnitude = magnitude;
                }
            }

            return commonHeader.ParsedLength;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Converts F-NET date (mm/dd/yy), time (hh:mm:ss) and subsecond to time in ticks
        internal static Ticks ParseTimestamp(string fnetDate, string fnetTime, uint sampleIndex, int frameRate)
        {
            fnetDate = fnetDate.PadLeft(6, '0');
            fnetTime = fnetTime.PadLeft(6, '0');

            if (sampleIndex == 10)
                return new DateTime(2000 + int.Parse(fnetDate.Substring(4, 2)), int.Parse(fnetDate.Substring(0, 2).Trim()), int.Parse(fnetDate.Substring(2, 2)), int.Parse(fnetTime.Substring(0, 2)), int.Parse(fnetTime.Substring(2, 2)), int.Parse(fnetTime.Substring(4, 2)), 0).AddSeconds(1.0D).Ticks;

            return new DateTime(2000 + int.Parse(fnetDate.Substring(4, 2)), int.Parse(fnetDate.Substring(0, 2).Trim()), int.Parse(fnetDate.Substring(2, 2)), int.Parse(fnetTime.Substring(0, 2)), int.Parse(fnetTime.Substring(2, 2)), int.Parse(fnetTime.Substring(4, 2)), (int)(sampleIndex / (double)frameRate * 1000.0D) % 1000).Ticks;
        }

        // Delegate handler to create a new F-NET data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new DataCell(parent as IDataFrame, (state as IDataFrameParsingState).ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.ParseBinaryImage(buffer, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}