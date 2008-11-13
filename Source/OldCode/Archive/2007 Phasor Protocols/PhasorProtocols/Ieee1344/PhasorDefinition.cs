using System.Diagnostics;
using System;
////using PCS.Common;
using System.Collections;
using PCS.Interop;
using Microsoft.VisualBasic;
using PCS;
using System.Collections.Generic;
////using PCS.Interop.Bit;
using System.Linq;
using System.Runtime.Serialization;

//*******************************************************************************************************
//  PhasorDefinition.vb - Phasor definition
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
        public class PhasorDefinition : PhasorDefinitionBase
        {



            protected PhasorDefinition()
            {
            }

            protected PhasorDefinition(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public PhasorDefinition(ConfigurationCell parent)
                : base(parent)
            {


            }

            public PhasorDefinition(ConfigurationCell parent, int index, string label, int scale, float offset, PhasorType type, PhasorDefinition voltageReference)
                : base(parent, index, label, scale, offset, type, voltageReference)
            {


            }

            public PhasorDefinition(ConfigurationCell parent, byte[] binaryImage, int startIndex)
                : base(parent, binaryImage, startIndex)
            {


            }

            public PhasorDefinition(ConfigurationCell parent, IPhasorDefinition phasorDefinition)
                : base(parent, phasorDefinition)
            {


            }

            internal static IPhasorDefinition CreateNewPhasorDefinition(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            {

                return new PhasorDefinition((ConfigurationCell)parent, binaryImage, startIndex);

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

                    buffer[0] = (byte)(Type == PhasorType.Voltage ? 0 : 1);

                    EndianOrder.BigEndian.CopyBuffer(BitConverter.GetBytes(ScalingFactor), 0, buffer, 1, 3);

                    return buffer;
                }
            }

            internal void ParseConversionFactor(byte[] binaryImage, int startIndex)
            {

                byte[] buffer = new byte[4];

                // Get phasor type from first byte
                Type = (binaryImage[startIndex] == 0) ? PhasorType.Voltage : PhasorType.Current;

                // Last three bytes represent scaling factor
                EndianOrder.BigEndian.CopyBuffer(binaryImage, startIndex + 1, buffer, 0, 3);
                ScalingFactor = BitConverter.ToInt32(buffer, 0);

            }

        }

    }

}
