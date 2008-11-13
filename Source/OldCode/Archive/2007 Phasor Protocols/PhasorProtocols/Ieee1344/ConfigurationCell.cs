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
//using PhasorProtocols.Ieee1344.Common;

//*******************************************************************************************************
//  ConfigurationCell.vb - IEEE 1344 Cconfiguration cell
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
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PCS.PhasorProtocols
{
    namespace Ieee1344
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationCell : ConfigurationCellBase
        {



            private CoordinateFormat m_coordinateFormat;
            private short m_statusFlags;

            protected ConfigurationCell()
            {
            }

            protected ConfigurationCell(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize configuration cell
                m_coordinateFormat = (CoordinateFormat)info.GetValue("coordinateFormat", typeof(CoordinateFormat));
                m_statusFlags = info.GetInt16("statusFlags");

            }

            public ConfigurationCell(ConfigurationFrame parent, LineFrequency nominalFrequency)
                : base(parent, false, 0, nominalFrequency, PhasorProtocols.Ieee1344.Common.MaximumPhasorValues, PhasorProtocols.Ieee1344.Common.MaximumAnalogValues, PhasorProtocols.Ieee1344.Common.MaximumDigitalValues)
            {


            }

            public ConfigurationCell(IConfigurationCell configurationCell)
                : base(configurationCell)
            {


            }

            // This constructor satisfies ChannelCellBase class requirement:
            //   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)
            public ConfigurationCell(IConfigurationFrame parent, IConfigurationFrameParsingState state, int index, byte[] binaryImage, int startIndex)
                : base(parent, false, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, new ConfigurationCellParsingState(Ieee1344.PhasorDefinition.CreateNewPhasorDefinition, Ieee1344.FrequencyDefinition.CreateNewFrequencyDefinition, null, Ieee1344.DigitalDefinition.CreateNewDigitalDefinition), binaryImage, startIndex)
            {

                // We pass in defaults for id code and nominal frequency since these will be parsed out later

            }

            internal static IConfigurationCell CreateNewConfigurationCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] binaryImage, int startIndex)
            {

                return new ConfigurationCell((IConfigurationFrame)parent, (IConfigurationFrameParsingState)state, index, binaryImage, startIndex);

            }

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

            public short StatusFlags
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

            public new ulong IDCode
            {
                get
                {
                    // IEEE 1344 only allows one PMU, so we share ID code with parent frame...
                    return Parent.IDCode;
                }
                set
                {
                    Parent.IDCode = value;

                    // Base classes constrain maximum value to 65535
                    if (value > ushort.MaxValue)
                    {
                        base.IDCode = ushort.MaxValue;
                    }
                    else
                    {
                        base.IDCode = (ushort)value;
                    }
                }
            }

            public bool SynchronizationIsValid
            {
                get
                {
                    return (StatusFlags & Bit.Bit15) == 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = (short)(StatusFlags & ~Bit.Bit15);
                    }
                    else
                    {
                        StatusFlags = (short)(StatusFlags | Bit.Bit15);
                    }
                }
            }

            public bool DataIsValid
            {
                get
                {
                    return (StatusFlags & Bit.Bit14) == 0;
                }
                set
                {
                    if (value)
                    {
                        StatusFlags = (short)(StatusFlags & ~Bit.Bit14);
                    }
                    else
                    {
                        StatusFlags = (short)(StatusFlags | Bit.Bit14);
                    }
                }
            }

            public TriggerStatus TriggerStatus
            {
                get
                {
                    return (TriggerStatus)(StatusFlags & PhasorProtocols.Ieee1344.Common.TriggerMask);
                }
                set
                {
                    StatusFlags = (short)((StatusFlags & ~Common.TriggerMask) | (ushort)value);
                }
            }

            // IEEE 1344 only supports scaled data
            public override DataFormat PhasorDataFormat
            {
                get
                {
                    return DataFormat.FixedInteger;
                }
                set
                {
                    if (value != DataFormat.FixedInteger)
                    {
                        throw (new NotSupportedException("IEEE 1344 only supports scaled data"));
                    }
                }
            }

            public override CoordinateFormat PhasorCoordinateFormat
            {
                get
                {
                    return m_coordinateFormat;
                }
                set
                {
                    m_coordinateFormat = value;
                }
            }

            public override DataFormat FrequencyDataFormat
            {
                get
                {
                    return DataFormat.FixedInteger;
                }
                set
                {
                    if (value != DataFormat.FixedInteger)
                    {
                        throw (new NotSupportedException("IEEE 1344 only supports scaled data"));
                    }
                }
            }

            public override DataFormat AnalogDataFormat
            {
                get
                {
                    return DataFormat.FixedInteger;
                }
                set
                {
                    if (value != DataFormat.FixedInteger)
                    {
                        throw (new NotSupportedException("IEEE 1344 only supports scaled data"));
                    }
                }
            }

            protected override ushort HeaderLength
            {
                get
                {
                    return (ushort)(base.HeaderLength + 14);
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    byte[] buffer = new byte[HeaderLength];
                    int index = 0;

                    EndianOrder.BigEndian.CopyBytes(m_statusFlags, buffer, index);

                    // Copy in station name
                    index += 2;
                    PhasorProtocols.Common.CopyImage(base.HeaderImage, buffer, ref index, base.HeaderLength);

                    EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index);
                    EndianOrder.BigEndian.CopyBytes((short)PhasorDefinitions.Count, buffer, index + 8);
                    EndianOrder.BigEndian.CopyBytes((short)DigitalDefinitions.Count, buffer, index + 10);

                    return buffer;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                IConfigurationCellParsingState parsingState = (IConfigurationCellParsingState)state;

                m_statusFlags = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                startIndex += 2;

                // Parse out station name
                base.ParseHeaderImage(state, binaryImage, startIndex);
                startIndex += base.HeaderLength;

                IDCode = EndianOrder.BigEndian.ToUInt64(binaryImage, startIndex);

                parsingState.PhasorCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8);
                parsingState.DigitalCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 10);

            }

            protected override ushort FooterLength
            {
                get
                {
                    return (ushort)(base.FooterLength + PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength);
                }
            }

            protected override byte[] FooterImage
            {
                get
                {
                    byte[] buffer = new byte[FooterLength];
                    int x;
                    int index = 0;

                    // Include conversion factors in configuration cell footer
                    for (x = 0; x <= PhasorDefinitions.Count - 1; x++)
                    {
                        PhasorProtocols.Common.CopyImage(((PhasorDefinition)(PhasorDefinitions[x])).ConversionFactorImage, buffer, ref index, PhasorDefinition.ConversionFactorLength);
                    }

                    for (x = 0; x <= DigitalDefinitions.Count - 1; x++)
                    {
                        PhasorProtocols.Common.CopyImage(((DigitalDefinition)(DigitalDefinitions[x])).ConversionFactorImage, buffer, ref index, DigitalDefinition.ConversionFactorLength);
                    }

                    // Include nominal frequency
                    PhasorProtocols.Common.CopyImage(base.FooterImage, buffer, ref index, base.FooterLength);

                    return buffer;
                }
            }

            protected override void ParseFooterImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                int x;

                // Parse conversion factors from configuration cell footer
                for (x = 0; x <= PhasorDefinitions.Count - 1; x++)
                {
                    ((PhasorDefinition)(PhasorDefinitions[x])).ParseConversionFactor(binaryImage, startIndex);
                    startIndex += PhasorDefinition.ConversionFactorLength;
                }

                for (x = 0; x <= DigitalDefinitions.Count - 1; x++)
                {
                    ((DigitalDefinition)(DigitalDefinitions[x])).ParseConversionFactor(binaryImage, startIndex);
                    startIndex += DigitalDefinition.ConversionFactorLength;
                }

                // Parse nominal frequency
                base.ParseFooterImage(state, binaryImage, startIndex);

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize configuration cell
                info.AddValue("coordinateFormat", m_coordinateFormat, typeof(CoordinateFormat));
                info.AddValue("statusFlags", m_statusFlags);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Status Flags", StatusFlags.ToString());
                    baseAttributes.Add("Synchronization Is Valid", SynchronizationIsValid.ToString());
                    baseAttributes.Add("Data Is Valid", DataIsValid.ToString());
                    baseAttributes.Add("Trigger Status", (int)TriggerStatus + ": " + TriggerStatus);

                    return baseAttributes;
                }
            }

        }

    }
}
