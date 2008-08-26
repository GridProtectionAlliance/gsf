using System.Diagnostics;
using System;
//using TVA.Common;
using System.Collections;
using TVA.Interop;
using Microsoft.VisualBasic;
using TVA;
using System.Collections.Generic;
//using TVA.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

//*******************************************************************************************************
//  FrequencyDefinition.vb - PDCstream Frequency definition
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

using System.Diagnostics.CodeAnalysis;

namespace PhasorProtocols
{
    namespace BpaPdcStream
    {

        [CLSCompliant(false), Serializable(), SuppressMessage("Microsoft.Usage", "CA2240")]
        public class FrequencyDefinition : FrequencyDefinitionBase
        {
            private int m_dummy;
            private float m_frequencyOffset;

            protected FrequencyDefinition()
            {
            }

            protected FrequencyDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public FrequencyDefinition(ConfigurationCell parent)
                : base(parent)
            {


            }

            public FrequencyDefinition(ConfigurationCell parent, string entryValue)
                : base(parent)
            {


                string[] entry = entryValue.Split(',');
                FrequencyDefinition defaultFrequency;

                if (parent != null)
                {
                    defaultFrequency = parent.Parent.DefaultFrequency;
                }
                else
                {
                    defaultFrequency = new FrequencyDefinition((ConfigurationCell)null);
                }

                // First entry is an F - we just ignore this
                if (entry.Length > 1)
                {
                    ScalingFactor = int.Parse(entry[1].Trim());
                }
                else
                {
                    ScalingFactor = defaultFrequency.ScalingFactor;
                }
                if (entry.Length > 2)
                {
                    Offset = float.Parse(entry[2].Trim());
                }
                else
                {
                    Offset = defaultFrequency.Offset;
                }
                if (entry.Length > 3)
                {
                    DfDtScalingFactor = int.Parse(entry[3].Trim());
                }
                else
                {
                    DfDtScalingFactor = defaultFrequency.DfDtScalingFactor;
                }
                if (entry.Length > 4)
                {
                    DfDtOffset = float.Parse(entry[4].Trim());
                }
                else
                {
                    DfDtOffset = defaultFrequency.DfDtOffset;
                }
                if (entry.Length > 5)
                {
                    m_dummy = int.Parse(entry[5].Trim());
                }
                else
                {
                    m_dummy = defaultFrequency.m_dummy;
                }
                if (entry.Length > 6)
                {
                    Label = entry[6].Trim();
                }
                else
                {
                    Label = defaultFrequency.Label;
                }

            }

            public FrequencyDefinition(ConfigurationCell parent, IFrequencyDefinition frequencyDefinition)
                : base(parent, frequencyDefinition)
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

            public static string ConfigFileFormat(IFrequencyDefinition definition)
            {
                FrequencyDefinition frequency = definition as FrequencyDefinition;

                if (frequency != null)
                {
                    return "F," + frequency.ScalingFactor + "," + frequency.Offset + "," + frequency.DfDtScalingFactor + "," + frequency.DfDtOffset + "," + frequency.m_dummy + "," + frequency.Label;
                }
                else
                {
                    return "";
                }
            }

            public override float Offset
            {
                get
                {
                    if (Parent == null)
                    {
                        return m_frequencyOffset;
                    }
                    else
                    {
                        return base.Offset;
                    }
                }
                set
                {
                    if (Parent == null)
                    {
                        // Store local value for default frequency definition
                        m_frequencyOffset = value;
                    }
                    else
                    {
                        // Frequency offset is stored as nominal frequency of parent cell
                        if (value >= 60.0F)
                        {
                            Parent.NominalFrequency = LineFrequency.Hz60;
                        }
                        else
                        {
                            Parent.NominalFrequency = LineFrequency.Hz50;
                        }
                    }
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
                    throw (new NotImplementedException("PDCstream does not include frequency definition in descriptor packet - must be defined in external INI file"));
                }
            }
        }
    }
}
