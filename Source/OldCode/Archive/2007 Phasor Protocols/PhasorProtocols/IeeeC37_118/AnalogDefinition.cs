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

//*******************************************************************************************************
//  AnalogDefinition.vb - IEEE C37.118 Analog definition
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
        public class AnalogDefinition : AnalogDefinitionBase
        {



            private AnalogType m_type;

            protected AnalogDefinition()
            {
            }

            protected AnalogDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


                // Deserialize analog definition
                m_type = (AnalogType)info.GetValue("type", typeof(AnalogType));

            }

            public AnalogDefinition(ConfigurationCell parent)
                : base(parent)
            {


            }

            public AnalogDefinition(ConfigurationCell parent, byte[] binaryImage, int startIndex)
                : base(parent, binaryImage, startIndex)
            {


            }

            public AnalogDefinition(ConfigurationCell parent, int index, string label)
                : base(parent, index, label, 1, 0)
            {


            }

            public AnalogDefinition(ConfigurationCell parent, IAnalogDefinition analogDefinition)
                : base(parent, analogDefinition)
            {


            }

            internal static IAnalogDefinition CreateNewAnalogDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            {

                return new AnalogDefinition((ConfigurationCell)parent, binaryImage, startIndex);

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

            public AnalogType Type
            {
                get
                {
                    return m_type;
                }
                set
                {
                    m_type = value;
                }
            }

            internal static int ConversionFactorLength
            {
                get
                {
                    return 4;
                }
            }

            internal byte[] ConversionFactorImage
            {
                get
                {
                    byte[] buffer = new byte[ConversionFactorLength];

                    buffer[0] = (byte)m_type;

                    EndianOrder.BigEndian.CopyBuffer(BitConverter.GetBytes(ScalingFactor), 0, buffer, 1, 3);

                    return buffer;
                }
            }

            internal void ParseConversionFactor(byte[] binaryImage, int startIndex)
            {

                byte[] buffer = new byte[4];

                // Get analog type from first byte
                m_type = (AnalogType)binaryImage[startIndex];

                // Last three bytes represent scaling factor
                EndianOrder.BigEndian.CopyBuffer(binaryImage, startIndex + 1, buffer, 0, 3);
                ScalingFactor = BitConverter.ToInt32(buffer, 0);

            }

            public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            {

                base.GetObjectData(info, context);

                // Serialize analog definition
                info.AddValue("type", m_type, typeof(AnalogType));

            }

            public override Dictionary<string, string> Attributes
            {
                get
                {
                    Dictionary<string, string> baseAttributes = base.Attributes;

                    baseAttributes.Add("Analog Type", (int)Type + ": " + Enum.GetName(typeof(AnalogType), Type));

                    return baseAttributes;
                }
            }

        }

    }

}
