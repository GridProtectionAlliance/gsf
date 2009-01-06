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
using System.ComponentModel;

//*******************************************************************************************************
//  DigitalDefinitionBase.vb - Digital value definition base class
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
//  02/18/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent definition of a digital value.</summary>
    [CLSCompliant(false), Serializable()]
    public abstract class DigitalDefinitionBase : ChannelDefinitionBase, IDigitalDefinition
    {



        protected DigitalDefinitionBase()
        {
        }

        protected DigitalDefinitionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        protected DigitalDefinitionBase(IConfigurationCell parent)
            : base(parent)
        {


        }

        protected DigitalDefinitionBase(IConfigurationCell parent, int index, string label)
            : base(parent, index, label, 1, 0)
        {


        }

        protected DigitalDefinitionBase(IConfigurationCell parent, byte[] binaryImage, int startIndex)
            : base(parent, binaryImage, startIndex)
        {


        }

        // Derived classes are expected to expose a Public Sub New(ByVal digitalDefinition As IDigitalDefinition)
        protected DigitalDefinitionBase(IConfigurationCell parent, IDigitalDefinition digitalDefinition)
            : this(parent, digitalDefinition.Index, digitalDefinition.Label)
        {


        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DataFormat DataFormat
        {
            get
            {
                return PhasorProtocols.DataFormat.FixedInteger;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override float Offset
        {
            get
            {
                return base.Offset;
            }
            set
            {
                if (value == 0)
                {
                    base.Offset = value;
                }
                else
                {
                    throw (new NotImplementedException("Digital values represent bit flags and thus do not support an offset"));
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override int ScalingFactor
        {
            get
            {
                return base.ScalingFactor;
            }
            set
            {
                if (value == 1)
                {
                    base.ScalingFactor = value;
                }
                else
                {
                    throw (new NotImplementedException("Digital values represent bit flags and thus are not scaled"));
                }
            }
        }

    }
}
