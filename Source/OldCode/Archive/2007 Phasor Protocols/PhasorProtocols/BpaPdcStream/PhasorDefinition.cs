//*******************************************************************************************************
//  PhasorDefinition.vb - PDCstream Phasor definition
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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using TVA;

namespace PhasorProtocols
{
    namespace BpaPdcStream
    {
        [CLSCompliant(false), Serializable()]
        public class PhasorDefinition : PhasorDefinitionBase
        {
            private float m_ratio;
            private float m_calFactor;
            private float m_shunt;
            private int m_voltageReferenceIndex;

            protected PhasorDefinition()
            {
            }

            protected PhasorDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                // Deserialize phasor definition
                m_ratio = info.GetSingle("ratio");
                m_calFactor = info.GetSingle("calFactor");
                m_shunt = info.GetSingle("shunt");
                m_voltageReferenceIndex = info.GetInt32("voltageReferenceIndex");
            }

            public PhasorDefinition(ConfigurationCell parent)
                : base(parent)
            {
            }

            public PhasorDefinition(ConfigurationCell parent, int index, string entryValue)
                : base(parent)
            {
                string[] entry = entryValue.Split(',');
                string entryType = entry[0].Trim().Substring(0, 1).ToUpper();
                PhasorDefinition defaultPhasor;

                if (parent != null)
                {
                    ConfigurationFrame configFile = this.Parent.Parent;

                    if (entryType == "V")
                    {
                        Type = PhasorType.Voltage;
                        defaultPhasor = configFile.DefaultPhasorV;
                    }
                    else if (entryType == "I")
                    {
                        Type = PhasorType.Current;
                        defaultPhasor = configFile.DefaultPhasorI;
                    }
                    else
                    {
                        Type = PhasorType.Voltage;
                        defaultPhasor = configFile.DefaultPhasorV;
                    }
                }
                else
                {
                    defaultPhasor = new PhasorDefinition((ConfigurationCell)null);
                }

                if (entry.Length > 1)
                    Ratio = (float)double.Parse(entry[1].Trim());
                else
                    Ratio = defaultPhasor.Ratio;

                if (entry.Length > 2)
                    CalFactor = (float)double.Parse(entry[2].Trim());
                else
                    ConversionFactor = defaultPhasor.ConversionFactor;

                if (entry.Length > 3)
                    Offset = (float)double.Parse(entry[3].Trim());
                else
                    Offset = defaultPhasor.Offset;

                if (entry.Length > 4)
                    Shunt = (float)double.Parse(entry[4].Trim());
                else
                    Shunt = defaultPhasor.Shunt;

                if (entry.Length > 5)
                    VoltageReferenceIndex = (int)double.Parse(entry[5].Trim());
                else
                    VoltageReferenceIndex = defaultPhasor.VoltageReferenceIndex;

                if (entry.Length > 6)
                    Label = entry[6].Trim();
                else
                    Label = defaultPhasor.Label;

                this.Index = index;
            }

            public PhasorDefinition(ConfigurationCell parent, IPhasorDefinition phasorDefinition)
                : base(parent, phasorDefinition)
            {
            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new ConfigurationCell Parent
            {
                get
                {
                    return (ConfigurationCell)base.Parent;
                }
            }

            // BPA PDCstream uses a custom conversion factor (see shared overload below)
            public override float ConversionFactor
            {
                get
                {
                    return 1.0F;
                }
                set
                {
                    // Ignore updates...
                }
            }

            public float Ratio
            {
                get
                {
                    return m_ratio;
                }
                set
                {
                    m_ratio = value;
                }
            }

            public float CalFactor
            {
                get
                {
                    return m_calFactor;
                }
                set
                {
                    m_calFactor = value;
                }
            }

            public float Shunt
            {
                get
                {
                    return m_shunt;
                }
                set
                {
                    m_shunt = value;
                }
            }

            public int VoltageReferenceIndex
            {
                get
                {
                    return m_voltageReferenceIndex;
                }
                set
                {
                    m_voltageReferenceIndex = value;
                }
            }

            public static float CustomConversionFactor(PhasorDefinition phasor)
            {
                if (phasor.Type == PhasorType.Voltage)
                {
                    return phasor.CalFactor * phasor.Ratio;
                }
                else
                {
                    return phasor.CalFactor * phasor.Ratio / phasor.Shunt;
                }
            }

            public static string ConfigFileFormat(IPhasorDefinition definition)
            {
                PhasorDefinition phasor = definition as PhasorDefinition;

                if (phasor != null)
                {
                    System.Text.StringBuilder fileImage = new StringBuilder();

                    switch (phasor.Type)
                    {
                        case PhasorType.Voltage:
                            fileImage.Append('V');
                            break;
                        case PhasorType.Current:
                            fileImage.Append('I');
                            break;
                    }

                    fileImage.Append("," + phasor.Ratio + "," + phasor.CalFactor + "," + phasor.Offset + "," + phasor.Shunt + "," + phasor.VoltageReferenceIndex + "," + phasor.Label);

                    return fileImage.ToString();
                }
                else
                {
                    return "";
                }
            }

            public override int MaximumLabelLength
            {
                get
                {
                    return int.MaxValue;
                }
            }

            protected override ushort BodyLength
            {
                get
                {
                    return 0;
                }
            }

            protected override byte[] BodyImage
            {
                get
                {
                    throw (new NotImplementedException("PDCstream does not include phasor definition in descriptor packet - must be defined in external INI file"));
                }
            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {
                base.GetObjectData(info, context);

                // Serialize phasor definition
                info.AddValue("ratio", m_ratio);
                info.AddValue("calFactor", m_calFactor);
                info.AddValue("shunt", m_shunt);
                info.AddValue("voltageReferenceIndex", m_voltageReferenceIndex);
            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Ratio", m_ratio.ToString());
                    baseAttributes.Add("CalFactor", m_calFactor.ToString());
                    baseAttributes.Add("Shunt", m_shunt.ToString());
                    baseAttributes.Add("Voltage Reference Index", m_voltageReferenceIndex.ToString());

                    return baseAttributes;
                }
            }
        }
    }
}
