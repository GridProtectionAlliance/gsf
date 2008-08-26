using System.Diagnostics;
using System;
////using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
////using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using PhasorProtocols.Common;
//using PhasorProtocols.IeeeC37_118.Common;

//*******************************************************************************************************
//  ConfigurationCell.vb - IEEE C37.118 Configuration cell
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


namespace PhasorProtocols
{
    namespace IeeeC37_118
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationCell : ConfigurationCellBase
        {



            private FormatFlags m_formatFlags;

            protected ConfigurationCell()
            {
            }

            protected ConfigurationCell(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize configuration cell
                m_formatFlags = (FormatFlags)info.GetValue("formatFlags", typeof(FormatFlags));

            }

            public ConfigurationCell(ConfigurationFrame parent, ushort idCode, LineFrequency nominalFrequency)
                : base(parent, false, idCode, nominalFrequency, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
            {


            }

            public ConfigurationCell(IConfigurationCell configurationCell)
                : base(configurationCell)
            {


            }

            // This constructor satisfies ChannelCellBase class requirement:
            //   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)
            public ConfigurationCell(IConfigurationFrame parent, IConfigurationFrameParsingState state, int index, byte[] binaryImage, int startIndex)
                : base(parent, false, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, new ConfigurationCellParsingState(PhasorDefinition.CreateNewPhasorDefinition, IeeeC37_118.FrequencyDefinition.CreateNewFrequencyDefinition, AnalogDefinition.CreateNewAnalogDefinition, DigitalDefinition.CreateNewDigitalDefinition), binaryImage, startIndex)
            {

                // We pass in defaults for id code and nominal frequency since these will be parsed out later

            }

            internal static IConfigurationCell CreateNewConfigurationCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] binaryImage, int startIndex)
            {

                return (IConfigurationCell)new ConfigurationCell((IConfigurationFrame)parent, (IConfigurationFrameParsingState)state, index, binaryImage, startIndex);

            }

            public new ConfigurationFrame Parent
            {
                get
                {
                    return (ConfigurationFrame)base.Parent;
                }
            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public FormatFlags FormatFlags
            {
                get
                {
                    return m_formatFlags;
                }
                set
                {
                    m_formatFlags = value;
                }
            }

            public override DataFormat PhasorDataFormat
            {
                get
                {
                    return (((m_formatFlags & FormatFlags.Phasors) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
                }
                set
                {
                    if (value == DataFormat.FloatingPoint)
                    {
                        m_formatFlags = m_formatFlags | FormatFlags.Phasors;
                    }
                    else
                    {
                        m_formatFlags = m_formatFlags & ~FormatFlags.Phasors;
                    }
                }
            }

            public override CoordinateFormat PhasorCoordinateFormat
            {
                get
                {
                    return (((m_formatFlags & FormatFlags.Coordinates) > 0) ? CoordinateFormat.Polar : CoordinateFormat.Rectangular);
                }
                set
                {
                    if (value == CoordinateFormat.Polar)
                    {
                        m_formatFlags = m_formatFlags | FormatFlags.Coordinates;
                    }
                    else
                    {
                        m_formatFlags = m_formatFlags & ~FormatFlags.Coordinates;
                    }
                }
            }

            public override DataFormat FrequencyDataFormat
            {
                get
                {
                    return (((m_formatFlags & FormatFlags.Frequency) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
                }
                set
                {
                    if (value == DataFormat.FloatingPoint)
                    {
                        m_formatFlags = m_formatFlags | FormatFlags.Frequency;
                    }
                    else
                    {
                        m_formatFlags = m_formatFlags & ~FormatFlags.Frequency;
                    }
                }
            }

            public override DataFormat AnalogDataFormat
            {
                get
                {
                    return (((m_formatFlags & FormatFlags.Analog) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
                }
                set
                {
                    if (value == DataFormat.FloatingPoint)
                    {
                        m_formatFlags = m_formatFlags | FormatFlags.Analog;
                    }
                    else
                    {
                        m_formatFlags = m_formatFlags & ~FormatFlags.Analog;
                    }
                }
            }

            protected override ushort HeaderLength
            {
                get
                {
                    return (ushort)(base.HeaderLength + 10);
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    byte[] buffer = new byte[HeaderLength];
                    int index = 0;

                    PhasorProtocols.Common.CopyImage(base.HeaderImage, buffer, ref index, base.HeaderLength);
                    EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index);
                    EndianOrder.BigEndian.CopyBytes((short)m_formatFlags, buffer, index + 2);
                    EndianOrder.BigEndian.CopyBytes((short)PhasorDefinitions.Count, buffer, index + 4);
                    EndianOrder.BigEndian.CopyBytes((short)AnalogDefinitions.Count, buffer, index + 6);
                    EndianOrder.BigEndian.CopyBytes((short)DigitalDefinitions.Count, buffer, index + 8);

                    return buffer;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                IConfigurationCellParsingState parsingState = (IConfigurationCellParsingState)state;

                // Parse out station name
                base.ParseHeaderImage(state, binaryImage, startIndex);
                startIndex += base.HeaderLength;

                IDCode = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex);
                m_formatFlags = (FormatFlags)EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2);

                parsingState.PhasorCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4);
                parsingState.AnalogCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6);
                parsingState.DigitalCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 8);

            }

            protected override ushort FooterLength
            {
                get
                {
                    return (ushort)(base.FooterLength + PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + AnalogDefinitions.Count * AnalogDefinition.ConversionFactorLength + DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength + (Parent.DraftRevision > DraftRevision.Draft6 ? 2 : 0));
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

                    for (x = 0; x <= AnalogDefinitions.Count - 1; x++)
                    {
                        PhasorProtocols.Common.CopyImage(((AnalogDefinition)(AnalogDefinitions[x])).ConversionFactorImage, buffer, ref index, AnalogDefinition.ConversionFactorLength);
                    }

                    for (x = 0; x <= DigitalDefinitions.Count - 1; x++)
                    {
                        PhasorProtocols.Common.CopyImage(((DigitalDefinition)(DigitalDefinitions[x])).ConversionFactorImage, buffer, ref index, DigitalDefinition.ConversionFactorLength);
                    }

                    // Include nominal frequency
                    PhasorProtocols.Common.CopyImage(base.FooterImage, buffer, ref index, base.FooterLength);

                    // Include configuration count (new for version 7.0)
                    if (Parent.DraftRevision > DraftRevision.Draft6)
                    {
                        EndianOrder.BigEndian.CopyBytes(RevisionCount, buffer, index);
                    }

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

                for (x = 0; x <= AnalogDefinitions.Count - 1; x++)
                {
                    ((AnalogDefinition)(AnalogDefinitions[x])).ParseConversionFactor(binaryImage, startIndex);
                    startIndex += AnalogDefinition.ConversionFactorLength;
                }

                for (x = 0; x <= DigitalDefinitions.Count - 1; x++)
                {
                    ((DigitalDefinition)(DigitalDefinitions[x])).ParseConversionFactor(binaryImage, startIndex);
                    startIndex += DigitalDefinition.ConversionFactorLength;
                }

                // Parse nominal frequency
                base.ParseFooterImage(state, binaryImage, startIndex);

                // Get configuration count (new for version 7.0)
                if (Parent.DraftRevision > DraftRevision.Draft6)
                {
                    RevisionCount = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex);
                }

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize configuration cell
                info.AddValue("formatFlags", m_formatFlags, typeof(FormatFlags));

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Format Flags", (int)m_formatFlags + ": " + Enum.GetName(typeof(FormatFlags), m_formatFlags));

                    return baseAttributes;
                }
            }

        }

    }
}
