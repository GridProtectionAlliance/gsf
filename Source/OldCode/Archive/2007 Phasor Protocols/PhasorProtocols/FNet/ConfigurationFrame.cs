using System.Diagnostics;
using System;
//using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
//using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using PhasorProtocols.Common;
//using PCS.IO.Compression.Common;

//*******************************************************************************************************
//  ConfigurationFrame.vb - FNet Configuration Frame
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


namespace PCS.PhasorProtocols
{
    namespace FNet
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationFrame : ConfigurationFrameBase
        {



            protected ConfigurationFrame()
            {
            }

            protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public ConfigurationFrame(ushort idCode, long ticks, short frameRate, LineFrequency nominalFrequency, string stationName, long ticksOffset)
                : base(idCode, new ConfigurationCellCollection(), ticks, frameRate)
            {

                ConfigurationCell configCell = new ConfigurationCell(this, nominalFrequency, ticksOffset);

                // FNet protocol sends data for one device
                Cells.Add(configCell);

                // Assign station name
                if (string.IsNullOrEmpty(stationName))
                {
                    configCell.StationName = "FNET Unit-" + idCode;
                }
                else
                {
                    configCell.StationName = stationName;
                }

                // Add a single frequency definition
                configCell.FrequencyDefinition = new FrequencyDefinition(configCell);

                // Add a single phasor definition
                PhasorDefinition phasorDefinition = new PhasorDefinition(configCell);

                phasorDefinition.Label = "120V Phasor";
                phasorDefinition.Type = PhasorType.Voltage;

                configCell.PhasorDefinitions.Add(phasorDefinition);
            }

            // FNet supports no configuration frame in the data stream - so there will be nothing to parse
            //Public Sub New(ByVal binaryImage As Byte(), ByVal startIndex As int)

            //    MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, 0, _
            //            AddressOf FNet.ConfigurationCell.CreateNewConfigurationCell), binaryImage, startIndex)

            //End Sub

            public ConfigurationFrame(IConfigurationFrame configurationFrame)
                : base(configurationFrame)
            {


            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationCellCollection Cells
            {
                get
                {
                    return (ConfigurationCellCollection)base.Cells;
                }
            }

            // Since FNet only supports a single device there will only be one cell, so we just share ticks offset,
            // nominal frequency, longitude, latitude and number of satellites with our only child and expose the
            // value at the parent level for convience
            public long TicksOffset
            {
                get
                {
                    return Cells[0].TicksOffset;
                }
                set
                {
                    Cells[0].TicksOffset = value;
                }
            }

            public LineFrequency NominalFrequency
            {
                get
                {
                    return Cells[0].NominalFrequency;
                }
                set
                {
                    Cells[0].NominalFrequency = value;
                }
            }

            public float Longitude
            {
                get
                {
                    return Cells[0].Longitude;
                }
                set
                {
                    Cells[0].Longitude = value;
                }
            }

            public float Latitude
            {
                get
                {
                    return Cells[0].Latitude;
                }
                set
                {
                    Cells[0].Latitude = value;
                }
            }

            public int NumberOfSatellites
            {
                get
                {
                    return Cells[0].NumberOfSatellites;
                }
                set
                {
                    Cells[0].NumberOfSatellites = value;
                }
            }

        }

    }
}
