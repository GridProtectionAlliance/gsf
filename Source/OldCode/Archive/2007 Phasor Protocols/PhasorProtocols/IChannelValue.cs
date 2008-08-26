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
using TVA.Measurements;

//*******************************************************************************************************
//  IChannelValue.vb - Channel data value interface
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


namespace PhasorProtocols
{
    /// <summary>This interface represents a protocol independent representation of any kind of data value.</summary>
    [CLSCompliant(false)]
    public interface IChannelValue<T> : IChannel, ISerializable where T : IChannelDefinition
    {


        IDataCell Parent
        {
            get;
        }

        T Definition
        {
            get;
            set;
        }

        DataFormat DataFormat
        {
            get;
        }

        string Label
        {
            get;
        }

        /// <summary>Composite measurements of channel value</summary>
        /// <remarks>
        /// Because derived value classes may consist of more than one measured value,
        /// we use the composite value properties to abstractly expose each value
        /// </remarks>
        float this[int index]
        {
            get;
            set;
        }

        /// <summary>Total number of composite measurements exposed by the channel value</summary>
        /// <remarks>
        /// Because derived value classes may consist of more than one measured value,
        /// we use the composite value properties to abstractly expose each value
        /// </remarks>
        int CompositeValueCount
        {
            get;
        }

        bool IsEmpty
        {
            get;
        }

        IMeasurement[] Measurements
        {
            get;
        }

    }
}
