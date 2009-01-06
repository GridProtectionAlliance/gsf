//*******************************************************************************************************
//  FrequencyValue.vb - IEEE 1344 Frequency value
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
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
using PCS;
using PCS.Interop;

namespace PCS.PhasorProtocols
{
    namespace Ieee1344
    {
        /// <summary>IEEE 1344 Frequency value</summary>
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

            protected override int BodyLength
            {
                get
                {
                    int length = 0;

                    if (Definition.FrequencyIsAvailable)
                        length += 2;

                    if (Definition.DfDtIsAvailable)
                        length += 2;

                    return length;
                }
            }

            protected override byte[] BodyImage
            {
                get
                {
                    byte[] buffer = new byte[BodyLength];

                    if (Definition.FrequencyIsAvailable)
                    {
                        EndianOrder.BigEndian.CopyBytes(UnscaledFrequency, buffer, 0);
                    }
                    if (Definition.DfDtIsAvailable)
                    {
                        EndianOrder.BigEndian.CopyBytes(UnscaledDfDt, buffer, 2);
                    }

                    return buffer;
                }
            }

            protected override void ParseBodyImage(IChannelParsingState state, byte[] binaryImage, int startIndex)
            {
                // Note that IEEE 1344 only supports scaled integers (no need to worry about floating points)
                if (Definition.FrequencyIsAvailable)
                {
                    UnscaledFrequency = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                    startIndex += 2;
                }

                if (Definition.DfDtIsAvailable)
                {
                    UnscaledDfDt = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
                }
            }
        }
    }
}
