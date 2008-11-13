//*******************************************************************************************************
//  ConfigurationCell.vb - FNet Cconfiguration cell
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
using PCS;
using PCS.Interop;

namespace PCS.PhasorProtocols
{
    namespace FNet
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationCell : ConfigurationCellBase
        {



            private long m_ticksOffset;
            private float m_longitude;
            private float m_latitude;
            private int m_numberOfSatellites = 1; // We'll initially assume synchronization is good until told otherwise

            protected ConfigurationCell()
            {
            }

            /// <summary>
            /// Deserialize the configuration cell. Retrieve the Longitude, Latitude and the number of satellite
            /// </summary>
            protected ConfigurationCell(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize configuration cell
                m_ticksOffset = info.GetInt64("ticksOffset");
                m_longitude = info.GetSingle("longitude");
                m_latitude = info.GetSingle("latitude");
                m_numberOfSatellites = info.GetInt32("numberOfSatellites");

            }

            public ConfigurationCell(ConfigurationFrame parent, LineFrequency nominalFrequency, long ticksOffset)
                : base(parent, false, 0, nominalFrequency, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
            {

                m_ticksOffset = ticksOffset;

            }

            public ConfigurationCell(IConfigurationCell configurationCell)
                : base(configurationCell)
            {


            }

            // FNet supports no configuration frame in the data stream - so there will be nothing to parse

            //' This constructor satisfies ChannelCellBase class requirement:
            //'   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)
            //Public Sub New(ByVal parent As IConfigurationFrame, ByVal state As IConfigurationFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)

            //    '' We pass in defaults for id code and nominal frequency since these will be parsed out later
            //    'MyBase.New(parent, False, MaximumPhasorValues, MaximumAnalogValues, MaximumDigitalValues, _
            //    '    New ConfigurationCellParsingState( _
            //    '        AddressOf FNet.PhasorDefinition.CreateNewPhasorDefinition, _
            //    '        AddressOf FNet.FrequencyDefinition.CreateNewFrequencyDefinition, _
            //    '        Nothing, _
            //    '        Nothing), _
            //    '    binaryImage, startIndex)

            //End Sub

            //Friend Shared Function CreateNewConfigurationCell(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState(Of IConfigurationCell), ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int) As IConfigurationCell

            //    Return New ConfigurationCell(parent, state, index, binaryImage, startIndex)

            //End Function

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationFrame Parent
            {
                get
                {
                    return (ConfigurationFrame)base.Parent;
                }
            }

            // FNet protocol only allows one device, so we share ID code with parent frame...
            public override ushort IDCode
            {
                get
                {
                    return Parent.IDCode;
                }
                set
                {
                    Parent.IDCode = value;
                }
            }

            public long TicksOffset
            {
                get
                {
                    return m_ticksOffset;
                }
                set
                {
                    m_ticksOffset = value;
                }
            }

            public float Longitude
            {
                get
                {
                    return m_longitude;
                }
                set
                {
                    m_longitude = value;
                }
            }

            public float Latitude
            {
                get
                {
                    return m_latitude;
                }
                set
                {
                    m_latitude = value;
                }
            }

            public int NumberOfSatellites
            {
                get
                {
                    return m_numberOfSatellites;
                }
                set
                {
                    m_numberOfSatellites = value;
                }
            }

            public override int MaximumStationNameLength
            {
                get
                {
                    // The station name is defined external to the protocol, so there is no set limit
                    return int.MaxValue;
                }
            }

            // FNet only supports floating point data
            public override DataFormat PhasorDataFormat
            {
                get
                {
                    return DataFormat.FloatingPoint;
                }
                set
                {
                    if (value != DataFormat.FloatingPoint)
                    {
                        throw (new NotSupportedException("FNet only supports floating point data"));
                    }
                }
            }

            public override CoordinateFormat PhasorCoordinateFormat
            {
                get
                {
                    return CoordinateFormat.Polar;
                }
                set
                {
                    if (value != CoordinateFormat.Polar)
                    {
                        throw (new NotSupportedException("FNet only supports polar coordinates"));
                    }
                }
            }

            public override DataFormat FrequencyDataFormat
            {
                get
                {
                    return DataFormat.FloatingPoint;
                }
                set
                {
                    if (value != DataFormat.FloatingPoint)
                    {
                        throw (new NotSupportedException("FNet only supports floating point data"));
                    }
                }
            }

            public override DataFormat AnalogDataFormat
            {
                get
                {
                    return DataFormat.FloatingPoint;
                }
                set
                {
                    if (value != DataFormat.FloatingPoint)
                    {
                        throw (new NotSupportedException("FNet only supports floating point data"));
                    }
                }
            }

            /// <summary>
            /// Serialize the parameters of Longitude, Latitude and numberOfSatellite
            /// </summary>
            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize configuration cell
                info.AddValue("ticksOffset", m_ticksOffset);
                info.AddValue("longitude", m_longitude);
                info.AddValue("latitude", m_latitude);
                info.AddValue("numberOfSatellites", m_numberOfSatellites);

            }

            /// <summary>
            /// Retrieve the attribute include Longitude, Latitude and NumberOfSatellite
            /// </summary>
            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Time Offset (ticks)", m_ticksOffset.ToString());
                    baseAttributes.Add("Time Offset (seconds)", ((double)m_ticksOffset / (double)TVA.DateTime.Common.TicksPerSecond).ToString());
                    baseAttributes.Add("Longitude", Longitude.ToString());
                    baseAttributes.Add("Latitude", Latitude.ToString());
                    baseAttributes.Add("Number of Satellites", NumberOfSatellites.ToString());

                    return baseAttributes;
                }
            }
        }
    }
}
