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

//*******************************************************************************************************
//  DataCellCollection.vb - FNet specific data cell collection
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

        [CLSCompliant(false), Serializable()]
        public class DataCellCollection : PhasorProtocols.DataCellCollection
        {



            public DataCellCollection()
                : base(1, false)
            {

                // IEEE 1344 only supports a single PMU - so there should only be one cell

            }

            protected DataCellCollection(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {


            }

            public void Add(DataCell value)
            {

                base.Add(value);

            }

            public new DataCell this[int index]
            {
                get
                {
                    return (DataCell)base[index];
                }
            }

        }

    }

}
