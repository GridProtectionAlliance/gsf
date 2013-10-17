//******************************************************************************************************
//  IServiceBusServiceCallback.cs - Gbtc
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
//  10/29/2010 - Pinal C. Patel
//       Renamed MessageReceived to ProcessMessage so Silverlight generated async proxy has 
//       ProcessMessageReceived instead of MessageReceivedReceived.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.ServiceModel;

namespace GSF.ServiceBus
{
    /// <summary>
    /// Defines a callback contract that must be implemented by clients of <see cref="ServiceBusService"/> for receiving <see cref="Message"/>s.
    /// </summary>
    public interface IServiceBusServiceCallback
    {
        #region [ Methods ]

        /// <summary>
        /// Invoked when a new <see cref="Message"/> is received from the <see cref="ServiceBusService"/>.
        /// </summary>
        /// <param name="message"><see cref="Message"/> received from the <see cref="ServiceBusService"/>.</param>
        [OperationContract(IsOneWay = true)]
        void ProcessMessage(Message message);

        #endregion
    }
}
