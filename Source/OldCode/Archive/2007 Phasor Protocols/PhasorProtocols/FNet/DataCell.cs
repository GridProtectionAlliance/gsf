//*******************************************************************************************************
//  DataCell.vb - FNet Data Cell
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
//  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using TVA;

namespace PhasorProtocols
{
    namespace FNet
    {

        // This data cell represents what most might call a "field" in table of rows - it is a single unit of data for a specific PMU
        [CLSCompliant(false), Serializable(), SuppressMessage("Microsoft.Usage", "CA2240")]
        public class DataCell : DataCellBase
        {
            private float m_analogValue;

            protected DataCell()
            {
            }

            protected DataCell(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
                : base(parent, false, configurationCell, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
            {


                // Initialize single phasor value and frequency value with an empty value
                PhasorValues.Add(new PhasorValue(this, ConfigurationCell.PhasorDefinitions[0], float.NaN, float.NaN));

                // Initialize frequency and df/dt
                FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition, float.NaN, float.NaN);

            }

            public DataCell(IDataCell dataCell)
                : base(dataCell)
            {


            }

            public DataCell(IDataFrame parent, DataFrameParsingState state, int index, byte[] binaryImage, int startIndex)
                : base(parent, false, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, new DataCellParsingState(state.ConfigurationFrame.Cells[index], null, null, null, null), binaryImage, startIndex)
            {


            }

            internal static IDataCell CreateNewDataCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] binaryImage, int startIndex)
            {

                return new DataCell((IDataFrame)parent, (DataFrameParsingState)state, index, binaryImage, startIndex);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataFrame Parent
            {
                get
                {
                    return (DataFrame)base.Parent;
                }
            }

            public new ConfigurationCell ConfigurationCell
            {
                get
                {
                    return (ConfigurationCell)base.ConfigurationCell;
                }
                set
                {
                    base.ConfigurationCell = value;
                }
            }

            public override bool SynchronizationIsValid
            {
                get
                {
                    return ConfigurationCell.NumberOfSatellites > 0;
                }
                set
                {
                    // We just ignore this value as FNet defines synchronization validity as a derived value based on the number of available satellites
                }
            }

            public override bool DataIsValid
            {
                get
                {
                    return true;
                }
                set
                {
                    // We just ignore this value as FNet defines no flags for data validity
                }
            }

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

            public override bool PmuError
            {
                get
                {
                    return false;
                }
                set
                {
                    // We just ignore this value as FNet defines no flags for data errors
                }
            }

            public string FNetDate
            {
                get
                {
                    return Parent.Timestamp.ToString("MMddyy");
                }
            }

            public string FNetTime
            {
                get
                {
                    return Parent.Timestamp.ToString("HHmmss");
                }
            }

            public float AnalogValue
            {
                get
                {
                    return m_analogValue;
                }
                set
                {
                    m_analogValue = value;
                }
            }

            public string FNetDataString
            {
                get
                {
                    System.Text.StringBuilder dataLine = new StringBuilder();

                    dataLine.Append(Common.StartByte);
                    dataLine.Append(IDCode);
                    dataLine.Append(' ');
                    dataLine.Append(FNetDate);
                    dataLine.Append(' ');
                    dataLine.Append(FNetTime);
                    dataLine.Append(' ');
                    dataLine.Append(Parent.SampleIndex);
                    dataLine.Append(' ');
                    dataLine.Append(m_analogValue);
                    dataLine.Append(' ');
                    dataLine.Append(FrequencyValue.Frequency);
                    dataLine.Append(' ');
                    dataLine.Append(PhasorValues[0].Magnitude);
                    dataLine.Append(' ');
                    dataLine.Append(PhasorValues[0].Angle);
                    dataLine.Append(Common.EndByte);

                    return dataLine.ToString();
                }
            }

            protected override ushort BodyLength
            {
                get
                {
                    return (ushort)FNetDataString.Length;
                }
            }

            protected override byte[] BodyImage
            {
                get
                {
                    return Encoding.ASCII.GetBytes(FNetDataString);
                }
            }

            /// <summary>
            /// Overrides the ParseBodyImage in ChannelCell,phase the image body
            /// </summary>
            /// <remarks>The longitude, latitude and number of satellite at the top of minute in FNET data</remarks>
            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                if (binaryImage[startIndex] != Common.StartByte)
                {
                    throw (new InvalidOperationException("Bad data stream, expected start byte 01 as first byte in FNet frame, got " + binaryImage[startIndex].ToString("x").PadLeft(2, '0').ToUpper()));
                }

                ConfigurationCell configurationCell = (ConfigurationCell)(((IDataCellParsingState)state).ConfigurationCell);

                string[] data;
                int stopByteIndex = 0;

                for (int x = startIndex; x <= binaryImage.Length - 1; x++)
                {
                    if (binaryImage[x] == Common.EndByte)
                    {
                        stopByteIndex = x;
                        break;
                    }
                }

                // Parse FNet data frame into individual fields separated by spaces
                data = TVA.Text.Common.RemoveDuplicateWhiteSpace(Encoding.ASCII.GetString(binaryImage, startIndex + 1, stopByteIndex - startIndex - 1)).Trim().Split(' ');

                // Make sure all the needed data elements exist (could be a bad frame)
                if (data.Length >= 8)
                {
                    // Assign sample index
                    Parent.SampleIndex = Convert.ToInt16(data[Element.SampleIndex]);

                    // Get timestamp of data record
                    Parent.Ticks = configurationCell.TicksOffset + ParseTimestamp(data[Element.Date], data[Element.Time], Parent.SampleIndex, configurationCell.FrameRate);

                    // Parse out first analog value (can be long/lat at top of minute)
                    m_analogValue = Convert.ToSingle(data[Element.Analog]);

                    if (Convert.ToInt32(data[Element.Time].Substring(4, 2)) == 0)
                    {
                        switch (Parent.SampleIndex)
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

                    // Create frequency value
                    FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition, Convert.ToSingle(data[Element.Frequency]), 0);

                    // Create single phasor value
                    PhasorValues.Add(PhasorValue.CreateFromPolarValues(this, ConfigurationCell.PhasorDefinitions[0], Convert.ToSingle(data[Element.Angle]), Convert.ToSingle(data[Element.Voltage])));
                }
                else
                {
                    throw (new InvalidOperationException("Invalid number of data elements encountered in FNET data stream line: \"" + Encoding.ASCII.GetString(binaryImage, startIndex + 1, stopByteIndex - startIndex - 1) + "\".  Got " + data.Length + " elements, expected 8."));
                }

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("FNET Date", FNetDate);
                    baseAttributes.Add("FNET Time", FNetTime);
                    baseAttributes.Add("Analog Value", m_analogValue.ToString());

                    return baseAttributes;
                }
            }

            /// <summary>
            /// Convert FNET date (mm/dd/yy), time (hh:mm:ss) and subsecond to time ticks
            /// </summary>
            /// <param name="fnetDate"></param>
            /// <param name="fnetTime"></param>
            /// <param name="sampleIndex"></param>
            /// <param name="frameRate"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            private long ParseTimestamp(string fnetDate, string fnetTime, int sampleIndex, int frameRate)
            {

                fnetDate = fnetDate.PadLeft(6, '0');
                fnetTime = fnetTime.PadLeft(6, '0');

                if (sampleIndex == 10)
                {
                    return new DateTime(2000 + Convert.ToInt32(fnetDate.Substring(4, 2)), Convert.ToInt32(fnetDate.Substring(0, 2).Trim()), Convert.ToInt32(fnetDate.Substring(2, 2)), Convert.ToInt32(fnetTime.Substring(0, 2)), Convert.ToInt32(fnetTime.Substring(2, 2)), Convert.ToInt32(fnetTime.Substring(4, 2)), 0).AddSeconds(1.0D).Ticks;
                }
                else
                {
                    return new DateTime(2000 + Convert.ToInt32(fnetDate.Substring(4, 2)), Convert.ToInt32(fnetDate.Substring(0, 2).Trim()), Convert.ToInt32(fnetDate.Substring(2, 2)), Convert.ToInt32(fnetTime.Substring(0, 2)), Convert.ToInt32(fnetTime.Substring(2, 2)), Convert.ToInt32(fnetTime.Substring(4, 2)), Convert.ToInt32((double)sampleIndex / (double)frameRate * 1000.0D)).Ticks;
                }

            }
        }
    }
}
