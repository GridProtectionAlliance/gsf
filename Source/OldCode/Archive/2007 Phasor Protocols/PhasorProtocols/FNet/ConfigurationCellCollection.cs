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

//*******************************************************************************************************
//  ConfigurationCellCollection.vb - FNet specific configuration cell collection
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
//  02/08/2007 - J. Ritchie Carroll & Jian (Ryan) Zuo
//       Initial version of source generated
//
//*******************************************************************************************************


namespace PhasorProtocols
{
    namespace FNet
    {
        /// <summary>
        /// Collection of ConfigureCell
        /// </summary>
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
