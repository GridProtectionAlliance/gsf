//******************************************************************************************************
//  OIDCUserData.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/24/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;

namespace GSF.Security
{
    /// <summary>
    /// Defines <see cref="UserData"/> for the <see cref="OIDCSecurityProvider"/>.
    /// </summary>
    public class OIDCUserData : UserData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OIDCUserData"/> class.
        /// </summary>
        public OIDCUserData()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OIDCUserData"/> class.
        /// </summary>
        /// <param name="username">User's logon name.</param>
        public OIDCUserData(string username)
        {
            LoginID = username;
            Username = username;
            Groups = new List<string>();
            Roles = new List<string>();

            Initialize();
        }

        /// <summary>
        /// Gets or sets OIDC Nonce value associated with this user.
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TokenResponse"/> associated with this user.
        /// </summary>
        internal TokenResponse Token { get; set; }
    }
}
