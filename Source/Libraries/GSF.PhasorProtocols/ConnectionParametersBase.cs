//******************************************************************************************************
//  ConnectionParametersBase.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/26/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent set of extra connection parameters.
    /// </summary>
    /// <remarks>
    /// <para>This class is inherited by subsequent classes to provide protocol specific connection parameters that may be needed to make a connection.</para>
    /// <para>Derived implementations of this class are designed to be exposed by a "PropertyGrid" so a UI can request protocol specific connection parameters.</para>
    /// </remarks>
    [Serializable]
    public abstract class ConnectionParametersBase : IConnectionParameters
    {
        /// <summary>
        /// Determines if custom connection parameters are valid.
        /// </summary>
        [Browsable(false)]
        public virtual bool ValuesAreValid => true;

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }
}