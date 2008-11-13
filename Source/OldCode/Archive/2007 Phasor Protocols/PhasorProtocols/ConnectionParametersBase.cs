//*******************************************************************************************************
//  ConnectionParametersBase.vb - Connection parameters base class
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
//  2/26/2007 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>This class represents the common implementation of the protocol independent connection parameters base class.</summary>
    /// <remarks>
    /// <para>This class is inherited by subsequent classes to provide protocol specific connection parameters that may be needed to make a connection.</para>
    /// <para>Derived implementations of this class are designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.</para>
    /// </remarks>
    [Serializable()]
    public abstract class ConnectionParametersBase : IConnectionParameters
    {
        /// <summary>Returns True if all connection parameters are valid</summary>
        [Browsable(false)]
        public virtual bool ValuesAreValid
        {
            get
            {
                return true;
            }
        }

        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}
