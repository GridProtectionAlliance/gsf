//******************************************************************************************************
//  RegistrationRequest.cs - Gbtc
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
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

namespace GSF.ServiceBus
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the intent of the <see cref="RegistrationRequest"/>.
    /// </summary>
    public enum RegistrationType
    {
        /// <summary>
        /// Register to produce <see cref="Message"/>s.
        /// </summary>
        Produce,
        /// <summary>
        /// Register to consume <see cref="Message"/>s.
        /// </summary>
        Consume
    }

    #endregion

    /// <summary>
    /// Represents a request to register with the <see cref="ServiceBusService"/> to produce or consume <see cref="Message"/>s.
    /// </summary>
    public class RegistrationRequest
    {
        #region [ Constructors ]

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="RegistrationType">type</see> of this <see cref="RegistrationRequest"/>.
        /// </summary>
        public RegistrationType RegistrationType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Message.Type"/> of the <see cref="Message"/> this <see cref="RegistrationRequest"/> is for.
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Message.Name"/> of the <see cref="Message"/> this <see cref="RegistrationRequest"/> is for.
        /// </summary>
        public string MessageName { get; set; }

        #endregion
    }
}
