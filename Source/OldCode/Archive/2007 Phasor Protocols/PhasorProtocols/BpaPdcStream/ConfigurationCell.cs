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
//using PhasorProtocols.BpaPdcStream.Common;

//*******************************************************************************************************
//  ConfigurationCell.vb - PDCstream PMU configuration cell
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
    namespace BpaPdcStream
    {

        [CLSCompliant(false), Serializable()]
        public class ConfigurationCell : ConfigurationCellBase
        {



            private ConfigurationCell m_configurationFileCell;
            private string m_sectionEntry;
            private IEEEFormatFlags m_ieeeFormatFlags;
            private ushort m_offset;
            private short m_reserved;

            protected ConfigurationCell()
            {
            }

            protected ConfigurationCell(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize configuration cell
                m_sectionEntry = info.GetString("sectionEntry");
                m_ieeeFormatFlags = (IEEEFormatFlags)info.GetValue("IEEEFormatFlags", typeof(IEEEFormatFlags));
                m_offset = info.GetUInt16("offset");
                m_reserved = info.GetInt16("reserved");

            }

            public ConfigurationCell(IConfigurationFrame parent, ushort idCode, LineFrequency nominalFrequency)
                : base(parent, true, idCode, nominalFrequency, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
            {


            }

            public ConfigurationCell(IConfigurationCell configurationCell)
                : base(configurationCell)
            {


            }

            // This constructor satisfies ChannelCellBase class requirement:
            //   Final dervived classes must expose Public Sub New(ByVal parent As IChannelFrame, ByVal state As IChannelFrameParsingState, ByVal index As int, ByVal binaryImage As Byte(), ByVal startIndex As int)
            public ConfigurationCell(IConfigurationFrame parent, IConfigurationFrameParsingState state, int index, byte[] binaryImage, int startIndex)
                : base(parent, true, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, null, binaryImage, startIndex)
            {

                // We don't pass in a ConfigurationCellParsingState here because it is not needed for PDCstream (see ParseBodyImage below)

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

            public ConfigurationCell ConfigurationFileCell
            {
                get
                {
                    return m_configurationFileCell;
                }
                set
                {
                    m_configurationFileCell = value;
                }
            }

            // We use phasor definitions, station name, ID code and frequency definition of associated configuration cell that was read from INI file
            public override PhasorDefinitionCollection PhasorDefinitions
            {
                get
                {
                    if (m_configurationFileCell == null)
                    {
                        return base.PhasorDefinitions;
                    }
                    else
                    {
                        return m_configurationFileCell.PhasorDefinitions;
                    }
                }
            }

            public override string StationName
            {
                get
                {
                    if (m_configurationFileCell == null)
                    {
                        return base.StationName;
                    }
                    else
                    {
                        return m_configurationFileCell.StationName;
                    }
                }
                set
                {
                    if (m_configurationFileCell == null)
                    {
                        base.StationName = value;
                    }
                    else
                    {
                        m_configurationFileCell.StationName = value;
                    }
                }
            }

            public override int IDLabelLength
            {
                get
                {
                    // BPA PDCstream ID label length is 4 characters - max!
                    return 4;
                }
            }

            public string SectionEntry
            {
                get
                {
                    if (m_configurationFileCell == null)
                    {
                        return m_sectionEntry;
                    }
                    else
                    {
                        return m_configurationFileCell.SectionEntry;
                    }
                }
                set
                {
                    m_sectionEntry = value.Trim();

                    // Get ID label as substring of section entry
                    if (!string.IsNullOrEmpty(m_sectionEntry))
                    {
                        if (m_sectionEntry.Length > base.IDLabelLength)
                        {
                            base.IDLabel = m_sectionEntry.Substring(0, base.IDLabelLength);
                        }
                        else
                        {
                            base.IDLabel = m_sectionEntry;
                        }
                    }
                }
            }

            public bool IsPDCBlockSection
            {
                get
                {
                    if (m_configurationFileCell == null)
                    {
                        if (string.IsNullOrEmpty(m_sectionEntry))
                        {
                            return false;
                        }
                        else
                        {
                            return (m_sectionEntry.Length > IDLabelLength);
                        }
                    }
                    else
                    {
                        return m_configurationFileCell.IsPDCBlockSection;
                    }
                }
            }

            public override ushort IDCode
            {
                get
                {
                    if (m_configurationFileCell == null)
                    {
                        return base.IDCode;
                    }
                    else
                    {
                        return m_configurationFileCell.IDCode;
                    }
                }
                set
                {
                    if (m_configurationFileCell == null)
                    {
                        base.IDCode = value;
                    }
                    else
                    {
                        m_configurationFileCell.IDCode = value;
                    }
                }
            }

            public override IFrequencyDefinition FrequencyDefinition
            {
                get
                {
                    if (m_configurationFileCell == null)
                    {
                        return base.FrequencyDefinition;
                    }
                    else
                    {
                        return m_configurationFileCell.FrequencyDefinition;
                    }
                }
                set
                {
                    if (m_configurationFileCell == null)
                    {
                        base.FrequencyDefinition = value;
                    }
                    else
                    {
                        m_configurationFileCell.FrequencyDefinition = value;
                    }
                }
            }

            public override LineFrequency NominalFrequency
            {
                get
                {
                    if (m_configurationFileCell == null)
                    {
                        return base.NominalFrequency;
                    }
                    else
                    {
                        return m_configurationFileCell.NominalFrequency;
                    }
                }
                set
                {
                    if (m_configurationFileCell == null)
                    {
                        base.NominalFrequency = value;
                    }
                    else
                    {
                        m_configurationFileCell.NominalFrequency = value;
                    }
                }
            }

            public override int MaximumStationNameLength
            {
                get
                {
                    // The station name in the PDCstream is read from an INI file, so there is no set limit
                    return int.MaxValue;
                }
            }

            // The PDCstream descriptor maintains offsets for cell data in data packet
            public ushort Offset
            {
                get
                {
                    return m_offset;
                }
                set
                {
                    m_offset = value;
                }
            }

            public short Reserved
            {
                get
                {
                    return m_reserved;
                }
                set
                {
                    m_reserved = value;
                }
            }

            // These flags are defined in the data cell in the BPA PDCstream
            public IEEEFormatFlags IEEEFormatFlags
            {
                get
                {
                    return m_ieeeFormatFlags;
                }
                set
                {
                    m_ieeeFormatFlags = value;
                }
            }

            public override CoordinateFormat PhasorCoordinateFormat
            {
                get
                {
                    return (((m_ieeeFormatFlags & IEEEFormatFlags.Coordinates) > 0) ? CoordinateFormat.Polar : CoordinateFormat.Rectangular);
                }
                set
                {
                    if (value == CoordinateFormat.Polar)
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags | IEEEFormatFlags.Coordinates;
                    }
                    else
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags & ~IEEEFormatFlags.Coordinates;
                    }
                }
            }

            public override DataFormat PhasorDataFormat
            {
                get
                {
                    return (((m_ieeeFormatFlags & IEEEFormatFlags.Phasors) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
                }
                set
                {
                    if (value == DataFormat.FloatingPoint)
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags | IEEEFormatFlags.Phasors;
                    }
                    else
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags & ~IEEEFormatFlags.Phasors;
                    }
                }
            }

            public override DataFormat FrequencyDataFormat
            {
                get
                {
                    return (((m_ieeeFormatFlags & IEEEFormatFlags.Frequency) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
                }
                set
                {
                    if (value == DataFormat.FloatingPoint)
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags | IEEEFormatFlags.Frequency;
                    }
                    else
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags & ~IEEEFormatFlags.Frequency;
                    }
                }
            }

            public override DataFormat AnalogDataFormat
            {
                get
                {
                    return (((m_ieeeFormatFlags & IEEEFormatFlags.Analog) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
                }
                set
                {
                    if (value == DataFormat.FloatingPoint)
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags | IEEEFormatFlags.Analog;
                    }
                    else
                    {
                        m_ieeeFormatFlags = m_ieeeFormatFlags & ~IEEEFormatFlags.Analog;
                    }
                }
            }

            // The descriptor cell broadcasted by PDCstream only includes PMUID and offset, all
            // other metadata is defined in an external INI based configuration file - so we
            // override the base class image implementations which attempt to generate and
            // parse data based on a common nature of configuration frames
            protected override ushort HeaderLength
            {
                get
                {
                    return 0;
                }
            }

            protected override byte[] HeaderImage
            {
                get
                {
                    return null;
                }
            }

            protected override void ParseHeaderImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                // BPA PDC stream doesn't use standard configuration cell header like IEEE do - so we override this function to do nothing

            }

            protected override ushort BodyLength
            {
                get
                {
                    return 8;
                }
            }

            protected override byte[] BodyImage
            {
                get
                {
                    byte[] buffer = new byte[BodyLength];
                    int index = 0;

                    PhasorProtocols.Common.CopyImage(IDLabelImage, buffer, ref index, IDLabelLength); // PMUID
                    EndianOrder.BigEndian.CopyBytes(Reserved, buffer, index); // Reserved
                    EndianOrder.BigEndian.CopyBytes(Offset, buffer, index + 2); // Offset

                    return buffer;
                }
            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                IDLabel = Encoding.ASCII.GetString(binaryImage, startIndex, 4);
                Reserved = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 4);
                Offset = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 6);

            }

            protected override ushort FooterLength
            {
                get
                {
                    return 0;
                }
            }

            protected override byte[] FooterImage
            {
                get
                {
                    return null;
                }
            }

            protected override void ParseFooterImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                // BPA PDC stream doesn't use standard configuration cell footer like IEEE do - so we override this function to do nothing

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize configuration cell
                info.AddValue("sectionEntry", SectionEntry);
                info.AddValue("IEEEFormatFlags", m_ieeeFormatFlags, typeof(IEEEFormatFlags));
                info.AddValue("offset", m_offset);
                info.AddValue("reserved", m_reserved);

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("INI File Section Entry", SectionEntry);
                    baseAttributes.Add("Offset", m_offset.ToString());
                    baseAttributes.Add("Reserved", m_reserved.ToString());

                    return baseAttributes;
                }
            }

        }

    }
}
