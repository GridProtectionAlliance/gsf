//*******************************************************************************************************
//  ConnectionParametersBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/26/2007 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent set of extra connection parameters.
    /// </summary>
    /// <remarks>
    /// <para>This class is inherited by subsequent classes to provide protocol specific connection parameters that may be needed to make a connection.</para>
    /// <para>Derived implementations of this class are designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.</para>
    /// </remarks>
    [Serializable()]
    public abstract class ConnectionParametersBase : IConnectionParameters
    {
        /// <summary>
        /// Determines if custom connection parameters are valid.
        /// </summary>
        [Browsable(false)]
        public virtual bool ValuesAreValid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}