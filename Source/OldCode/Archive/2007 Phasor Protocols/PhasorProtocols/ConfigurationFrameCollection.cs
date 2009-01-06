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
//  ConfigurationFrameCollection.vb - Configuration frame collection class
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


// <summary>This class represents the protocol independent collection of the common implementation of a configuration frame that can be sent or received from a PMU.</summary>
namespace PCS.PhasorProtocols
{
    [CLSCompliant(false), Serializable()]
    public class ConfigurationFrameCollection : ChannelFrameCollectionBase<IConfigurationFrame>
    {



        protected ConfigurationFrameCollection()
        {
        }

        protected ConfigurationFrameCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        public ConfigurationFrameCollection(int maximumCount)
            : base(maximumCount)
        {


        }

        public override Type DerivedType
        {
            get
            {
                return this.GetType();
            }
        }

    }
}
