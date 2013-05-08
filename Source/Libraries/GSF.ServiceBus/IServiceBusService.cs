//******************************************************************************************************
//  IServiceBusService.cs - Gbtc
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
//  10/06/2010 - Pinal C. Patel
//       Generated original version of source code.
//  10/26/2010 - Pinal C. Patel
//       Added management operations GetClients(), GetQueues() and GetTopics().
//  10/29/2010 - Pinal C. Patel
//       Made all operations two-way (IsOneWay = false, the default) so OperationContext.RequestContext 
//       is available when operations are invoked so security can be applied to them.
//  02/03/2011 - Pinal C. Patel
//       Added GetLatestMessage() operation that can be used to retrieve the latest message published 
//       to the subscribers of a topic.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.ServiceModel;
using GSF.ServiceModel;

namespace GSF.ServiceBus
{
    /// <summary>
    /// Defines a service bus for event-based messaging between disjoint systems.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed, CallbackContract = typeof(IServiceBusServiceCallback))]
    public interface IServiceBusService : ISelfHostingService
    {
        #region [ Methods ]

        /// <summary>
        /// Registers with the <see cref="ServiceBusService"/> to produce or consume <see cref="Message"/>s.
        /// </summary>
        /// <param name="request">An <see cref="RegistrationRequest"/> containing registration data.</param>
        [OperationContract]
        void Register(RegistrationRequest request);

        /// <summary>
        /// Unregisters a previous registration with the <see cref="ServiceBusService"/> to produce or consume <see cref="Message"/>s
        /// </summary>
        /// <param name="request">The <see cref="RegistrationRequest"/> used when registering.</param>
        [OperationContract]
        void Unregister(RegistrationRequest request);

        /// <summary>
        /// Sends the <paramref name="message"/> to the <see cref="ServiceBusService"/> for distribution amongst its registered consumers.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> that is to be distributed.</param>
        [OperationContract]
        void Publish(Message message);

        /// <summary>
        /// Gets the latest <see cref="Message"/> distributed to the subscribers of the specified <paramref name="topic"/>.
        /// </summary>
        /// <param name="topic">The topic <see cref="RegistrationRequest"/> used when registering.</param>
        /// <returns>The latest <see cref="Message"/> distributed to the <paramref name="topic"/> subscribers.</returns>
        [OperationContract]
        Message GetLatestMessage(RegistrationRequest topic);

        /// <summary>
        /// Gets a list of all clients connected to the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="ClientInfo"/> objects.</returns>
        [OperationContract]
        ICollection<ClientInfo> GetClients();

        /// <summary>
        /// Gets a list of all <see cref="MessageType.Queue"/>s registered on the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="RegistrationInfo"/> objects.</returns>
        [OperationContract]
        ICollection<RegistrationInfo> GetQueues();

        /// <summary>
        /// Gets a list of all <see cref="MessageType.Topic"/>s registered on the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="RegistrationInfo"/> objects.</returns>
        [OperationContract]
        ICollection<RegistrationInfo> GetTopics();

        #endregion
    }
}
