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
//  DataCellCollection.vb - IEEE C37.118 specific data cell collection
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
    namespace IeeeC37_118
    {

        [CLSCompliant(false), Serializable()]
        public class DataCellCollection : PhasorProtocols.DataCellCollection
        {



            public DataCellCollection()
                : base(short.MaxValue, false)
            {


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
