//******************************************************************************************************
//  ClientInfo.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/19/2010 - Pinal C. Patel
//       Generated original version of source code.
//  11/24/2010 - Pinal C. Patel
//       Updated ConnectedAt to use UTC time.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GSF.ServiceBus
{
    /// <summary>
    /// Represents information about a client connected to the <see cref="ServiceBusService"/> to produce/consume <see cref="Message"/>s.
    /// </summary>
    [DataContract]
    public class ClientInfo
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Gets or sets the session identifier of the client.
        /// </summary>
        [DataMember]
        public string SessionId;

        /// <summary>
        /// Gets or sets the UTC <see cref="DateTime"/> when the client connected to the <see cref="ServiceBusService"/>.
        /// </summary>
        [DataMember]
        public DateTime ConnectedAt;

        /// <summary>
        /// Gets or sets the total number of <see cref="Message"/>s produced by the client.
        /// </summary>
        [DataMember]
        public long MessagesProduced;

        /// <summary>
        /// Gets or sets the total number of <see cref="Message"/>s consumed by the client.
        /// </summary>
        [DataMember]
        public long MessagesConsumed;

        /// <summary>
        /// Gets or sets the <see cref="OperationContext"/> object of the client.
        /// </summary>
        public OperationContext OperationContext;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientInfo"/> class.
        /// </summary>
        /// <param name="context">An <see cref="OperationContext"/> object of the client.</param>
        internal ClientInfo(OperationContext context)
        {
            SessionId = context.SessionId;
            ConnectedAt = DateTime.UtcNow;
            OperationContext = context;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines if the specified <see cref="Object"/> is equal to the current <see cref="ClientInfo"/> object.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ClientInfo"/> object.</param>
        /// <returns>true if both <see cref="Object"/>s  are equal; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            ClientInfo other = obj as ClientInfo;
            return (other != null && other.SessionId == this.SessionId);
        }

        /// <summary>
        /// Gets a hash value for the current <see cref="ClientInfo"/> object.
        /// </summary>
        /// <returns>An <see cref="Int32"/> value.</returns>
        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }

        #endregion
    }
}
