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
//  ConfigurationCellCollection.vb - IEEE 1344 specific configuration cell collection
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
        public class ConfigurationCellCollection : PhasorProtocols.ConfigurationCellCollection
        {



            public ConfigurationCellCollection()
                : base(1, true)
            {

                // IEEE 1344 only supports a single PMU - so there should only be one cell - since there's only one cell, cell lengths will be constant :)

            }

            protected ConfigurationCellCollection(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public void Add(ConfigurationCell value)
            {

                base.Add(value);

            }

            public new ConfigurationCell this[int index]
            {
                get
                {
                    return (ConfigurationCell)base[index];
                }
            }

        }

    }

}
