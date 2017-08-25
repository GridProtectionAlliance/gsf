//******************************************************************************************************
//  SecurityPrincipal.cs - Gbtc
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
//  03/22/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/03/2010 - Pinal C. Patel
//       Modified IsInRole() to allow for translation of role name before applying security using it.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Security.Principal;

namespace GSF.Security
{
    /// <summary>
    /// A class that implements <see cref="IPrincipal"/> interface to facilitate custom role-based security.
    /// </summary>
    /// <seealso cref="SecurityIdentity"/>
    /// <seealso cref="ISecurityProvider"/>
    public class SecurityPrincipal : IPrincipal
    {
        #region [ Members ]

        // Fields
        private readonly SecurityIdentity m_identity;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPrincipal"/> class.
        /// </summary>
        /// <param name="identity">An <see cref="SecurityIdentity"/> object.</param>
        /// <exception cref="ArgumentNullException">Value specified for <paramref name="identity"/> is null.</exception>
        public SecurityPrincipal(SecurityIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));

            m_identity = identity;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="SecurityIdentity"/> object of the user.
        /// </summary>
        public SecurityIdentity Identity
        {
            get
            {
                return m_identity;
            }
        }

        /// <summary>
        /// Gets the <see cref="IIdentity"/> object of the user.
        /// </summary>
        IIdentity IPrincipal.Identity
        {
            get
            {
                return m_identity;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the user is a member of either of the specified <paramref name="roles"/>.
        /// </summary>
        /// <param name="roles">Comma separated list of roles to check.</param>
        /// <returns>true if the user is a member of either of the specified <paramref name="roles"/>, otherwise false.</returns>
        public bool IsInRole(string roles)
        {
            if (!m_identity.Provider.UserData.IsDefined || !m_identity.Provider.IsUserAuthenticated ||
                m_identity.Provider.UserData.IsDisabled || m_identity.Provider.UserData.IsLockedOut)
            {
                // No need to check user roles.
                return false;
            }

            // Check if user has any one of the specified roles.
            foreach (string role in roles.Split(','))
            {
                if (m_identity.Provider.UserData.Roles.FirstOrDefault(currentRole => (SecurityProviderUtility.IsRegexMatch(m_identity.Provider.TranslateRole(role.Trim()), currentRole))) != null)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
