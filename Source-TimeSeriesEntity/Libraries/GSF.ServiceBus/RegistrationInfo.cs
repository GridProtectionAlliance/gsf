//******************************************************************************************************
//  RegistrationInfo.cs - Gbtc
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
//  02/03/2011 - Pinal C. Patel
//       Added LatestMessage field to save the very latest message distributed out to subscribers.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace GSF.ServiceBus
{
    /// <summary>
    /// Represents information about a registration with the <see cref="ServiceBusService"/> to produce/consume <see cref="Message"/>s.
    /// </summary>
    [DataContract]
    public class RegistrationInfo : IDisposable
    {
        #region [ Members ]

        // Fields

        private bool m_disposed;

        /// <summary>
        /// Gets or sets the type for <see cref="Message"/>s being produced/consumed.
        /// </summary>
        [DataMember]
        public MessageType MessageType;

        /// <summary>
        /// Gets or sets the name for <see cref="Message"/>s being produced/consumed. 
        /// </summary>
        [DataMember]
        public string MessageName;

        /// <summary>
        /// Gets or sets the total number of <see cref="Message"/>s received.
        /// </summary>
        [DataMember]
        public long MessagesReceived;

        /// <summary>
        /// Gets or sets the total number of <see cref="Message"/>s distributed.
        /// </summary>
        [DataMember]
        public long MessagesProcessed;

        /// <summary>
        /// Gets or sets the list of clients producing the <see cref="Message"/>s.
        /// </summary>
        [DataMember]
        public List<ClientInfo> Producers;

        /// <summary>
        /// Gets or sets the list of clients consuming the <see cref="Message"/>s.
        /// </summary>
        [DataMember]
        public List<ClientInfo> Consumers;

        /// <summary>
        /// Gets or sets the latest <see cref="Message"/> distributed to the subscribers.
        /// </summary>
        public Message LatestMessage;

        /// <summary>
        /// Gets the <see cref="ReaderWriterLockSlim"/> to be used for synchronized access to <see cref="Producers"/>.
        /// </summary>
        public readonly ReaderWriterLockSlim ProducersLock;

        /// <summary>
        /// Gets the <see cref="ReaderWriterLockSlim"/> to be used for synchronized access to <see cref="Consumers"/>.
        /// </summary>
        public readonly ReaderWriterLockSlim ConsumersLock;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
        /// </summary>
        /// <param name="request">An <see cref="RegistrationRequest"/> object.</param>
        internal RegistrationInfo(RegistrationRequest request)
        {
            MessageType = request.MessageType;
            MessageName = request.MessageName;
            Producers = new List<ClientInfo>();
            Consumers = new List<ClientInfo>();
            ProducersLock = new ReaderWriterLockSlim();
            ConsumersLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="RegistrationInfo"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~RegistrationInfo()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="RegistrationInfo"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="RegistrationInfo"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (ProducersLock != null)
                            ProducersLock.Dispose();

                        if (ConsumersLock != null)
                            ConsumersLock.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion
    }
}
