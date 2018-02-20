//******************************************************************************************************
//  AntiForgeryConfig.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  02/15/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

// Derived from AspNetWebStack (https://github.com/aspnet/AspNetWebStack) 
// Copyright (c) .NET Foundation. All rights reserved.
// See NOTICE.txt file in Source folder for more information.

#endregion

using System.ComponentModel;

namespace GSF.Web.Security
{
    /// <summary>
    /// Provides programmatic configuration for the anti-forgery token system.
    /// </summary>
    public static class AntiForgeryConfig
    {
        internal const string AntiForgeryTokenFieldName = "__RequestVerificationToken";

        private static string s_cookieName;

        /// <summary>
        /// Specifies an object that can provide additional data to put into all
        /// generated tokens and that can validate additional data in incoming
        /// tokens.
        /// </summary>
        public static IAntiForgeryAdditionalDataProvider AdditionalDataProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the name of the cookie that is used by the anti-forgery
        /// system.
        /// </summary>
        /// <remarks>
        /// If an explicit name is not provided, the system will automatically
        /// generate a name.
        /// </remarks>
        public static string CookieName
        {
            get
            {
                return s_cookieName ?? (s_cookieName = AntiForgeryTokenFieldName);
            }
            set
            {
                s_cookieName = value;
            }
        }

        /// <summary>
        /// Specifies whether SSL is required for the anti-forgery system
        /// to operate. If this setting is 'true' and a non-SSL request
        /// comes into the system, all anti-forgery APIs will fail.
        /// </summary>
        public static bool RequireSsl
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether to suppress the generation of X-Frame-Options header which
        /// is used to prevent ClickJacking. By default, the X-Frame-Options header is
        /// generated with the value SAMEORIGIN. If this setting is <c>true</c>, the
        /// X-Frame-Options header will not be generated for the response.
        /// </summary>
        public static bool SuppressXFrameOptionsHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies whether the anti-forgery system should skip checking
        /// for conditions that might indicate misuse of the system. Please
        /// use caution when setting this switch, as improper use could open
        /// security holes in the application.
        /// </summary>
        /// <remarks>
        /// Setting this switch will disable several checks, including:
        /// - Identity.IsAuthenticated = true without Identity.Name being set
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool SuppressIdentityHeuristicChecks
        {
            get;
            set;
        }
    }

    internal sealed class ReadOnlyAntiForgeryConfig
    {
        public IAntiForgeryAdditionalDataProvider AdditionalDataProvider => AntiForgeryConfig.AdditionalDataProvider;

        public string CookieName => AntiForgeryConfig.CookieName;

        public string FormFieldName => AntiForgeryConfig.AntiForgeryTokenFieldName;

        public bool RequireSSL => AntiForgeryConfig.RequireSsl;

        public bool SuppressIdentityHeuristicChecks => AntiForgeryConfig.SuppressIdentityHeuristicChecks;

        public bool SuppressXFrameOptionsHeader => AntiForgeryConfig.SuppressXFrameOptionsHeader;
    }
}