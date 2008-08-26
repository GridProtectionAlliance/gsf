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
//  DataCellCollection.vb - Data cell collection class
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
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace PhasorProtocols
{
    /// <summary>This class represents the protocol independent collection of the common implementation of a set of phasor related data values that can be sent or received from a PMU.</summary>
    [CLSCompliant(false), Serializable()]
    public class DataCellCollection : ChannelCellCollectionBase<IDataCell>
    {



        protected DataCellCollection()
        {
        }

        protected DataCellCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {


        }

        public DataCellCollection(int maximumCount, bool constantCellLength)
            : base(maximumCount, constantCellLength)
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
