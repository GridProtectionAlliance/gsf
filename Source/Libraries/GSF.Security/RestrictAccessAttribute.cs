//******************************************************************************************************
//  RestrictAccessAttribute.cs - Gbtc
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
//  01/06/2011 - Pinal C. Patel
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Threading;

namespace GSF.Security
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be used restrict access to a class when using role-based security.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RestrictAccessAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private string[] m_roles;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictAccessAttribute"/> class.
        /// </summary>
        public RestrictAccessAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictAccessAttribute"/> class.
        /// </summary>
        /// <param name="roles">List of either roles the current thread principal must have in order to have access.</param>
        public RestrictAccessAttribute(params string[] roles)
        {
            m_roles = roles;
        }

        #endregion        

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the list of either roles the current thread principal must have in order to have access.
        /// </summary>
        public string[] Roles
        {
            get
            {
                return m_roles;
            }
            set
            {
                m_roles = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Checks if the current thread principal has at least one of the <see cref="Roles"/> in order to have access.
        /// </summary>
        /// <returns>true if the current thread principal has access, otherwise false.</returns>
        public bool CheckAccess()
        {
            if (m_roles != null)
            {
                // One or more roles have been specified.
                foreach (string role in m_roles)
                {
                    // Check role against principal's role membership.
                    if (Thread.CurrentPrincipal.IsInRole(role))
                        // Principal has membership to the role so allow access.
                        return true;
                }
            }

            return false;
        }

        #endregion
    }
}
