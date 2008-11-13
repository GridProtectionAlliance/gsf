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

//*******************************************************************************************************
//  FrequencyValue.vb - PDCstream Frequency value
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
        public class FrequencyValue : FrequencyValueBase
        {



            protected FrequencyValue()
            {
            }

            protected FrequencyValue(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition, float frequency, float dfdt)
                : base(parent, frequencyDefinition, frequency, dfdt)
            {


            }

            public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition, short unscaledFrequency, short unscaledDfDt)
                : base(parent, frequencyDefinition, unscaledFrequency, unscaledDfDt)
            {


            }

            public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition, byte[] binaryImage, int startIndex)
                : base(parent, frequencyDefinition, binaryImage, startIndex)
            {


            }

            public FrequencyValue(IDataCell parent, IFrequencyDefinition frequencyDefinition, IFrequencyValue frequencyValue)
                : base(parent, frequencyDefinition, frequencyValue)
            {


            }

            internal static IFrequencyValue CreateNewFrequencyValue(IDataCell parent, IFrequencyDefinition definition, byte[] binaryImage, int startIndex)
            {

                return new FrequencyValue(parent, definition, binaryImage, startIndex);

            }

            public override System.Type DerivedType
            {
                get
                {
                    return this.GetType();
                }
            }

            public new DataCell Parent
            {
                get
                {
                    return (DataCell)base.Parent;
                }
            }

            public new FrequencyDefinition Definition
            {
                get
                {
                    return (FrequencyDefinition)base.Definition;
                }
                set
                {
                    base.Definition = value;
                }
            }

            protected override ushort BodyLength
            {
                get
                {
                    // PMUs in PDC block do not include Df/Dt
                    if (Definition.Parent.IsPDCBlockSection)
                    {
                        return (ushort)(base.BodyLength / 2);
                    }
                    else
                    {
                        return base.BodyLength;
                    }
                }
            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {

                // PMUs in PDC block do not include Df/Dt
                if (Definition.Parent.IsPDCBlockSection)
                {
                    if (DataFormat == PhasorProtocols.DataFormat.FixedInteger)
                    {
                        UnscaledFrequency = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                    }
                    else
                    {
                        Frequency = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex);
                    }
                }
                else
                {
                    base.ParseBodyImage(state, binaryImage, startIndex);
                }

            }

            public static ushort CalculateBinaryLength(IFrequencyDefinition definition)
            {

                // The frequency definition will determine the binary length based on data format
                return (new FrequencyValue(null, definition, 0, 0)).BinaryLength;

            }

        }

    }
}
