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
//  AnalogDefinitionBase.vb - Analog value definition base class
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
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent definition of an analog value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class AnalogDefinitionBase : ChannelDefinitionBase, IAnalogDefinition
    {



        protected AnalogDefinitionBase()
        {
        }

        protected AnalogDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        protected AnalogDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {


        }

        protected AnalogDefinitionBase(IConfigurationCell parent, int index, string label, int scale, float offset)
            : base(parent, index, label, scale, offset)
        {


        }

        protected AnalogDefinitionBase(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            : base(parent, binaryImage, startIndex)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal analogDefinition As IAnalogDefinition)
        protected AnalogDefinitionBase(IConfigurationCell parent, IAnalogDefinition analogDefinition)
            : this(parent, analogDefinition.Index, analogDefinition.Label, analogDefinition.ScalingFactor, analogDefinition.Offset)
        {


        }

        public override DataFormat DataFormat
        {
            get
            {
                return Parent.AnalogDataFormat;
            }
        }

    }

}
