//******************************************************************************************************
//  Resources.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/30/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Web.Security;

namespace GSF.Web.Shared
{
    /// <summary>
    /// Represents common resources that can be used in a web page.
    /// </summary>
    public static class Resources
    {
        /// <summary>
        /// Defines embedded resource root path.
        /// </summary>
        public const string Root = "/Notifications/@GSF/Web";

        /// <summary>
        /// Gets common set of icon resources that can be included in web page header.
        /// </summary>
        public static readonly string HeaderIcons = $@"
            <link rel=""shortcut icon"" href=""{Root}/Shared/Images/Icons/favicon.ico"" />
            <link rel=""icon"" type=""image/png"" href=""{Root}/Shared/Images/Icons/favicon-196x196.png"" sizes=""196x196"" />
            <link rel=""icon"" type=""image/png"" href=""{Root}/Shared/Images/Icons/favicon-96x96.png"" sizes=""96x96"" />
            <link rel=""icon"" type=""image/png"" href=""{Root}/Shared/Images/Icons/favicon-32x32.png"" sizes=""32x32"" />
            <link rel=""icon"" type=""image/png"" href=""{Root}/Shared/Images/Icons/favicon-16x16.png"" sizes=""16x16"" />
            <link rel=""icon"" type=""image/png"" href=""{Root}/Shared/Images/Icons/favicon-128.png"" sizes=""128x128"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""57x57"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-57x57.png"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""114x114"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-114x114.png"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""72x72"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-72x72.png"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""144x144"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-144x144.png"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""60x60"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-60x60.png"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""120x120"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-120x120.png"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""76x76"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-76x76.png"" />
            <link rel=""apple-touch-icon-precomposed"" sizes=""152x152"" href=""{Root}/Shared/Images/Icons/apple-touch-icon-152x152.png"" />
            <meta name=""msapplication-TileColor"" content=""#AABBAA"" />
            <meta name=""msapplication-TileImage"" content=""{Root}/Shared/Images/Icons/mstile-144x144.png"" />
            <meta name=""msapplication-square70x70logo"" content=""{Root}/Shared/Images/Icons/mstile-70x70.png"" />
            <meta name=""msapplication-square150x150logo"" content=""{Root}/Shared/Images/Icons/mstile-150x150.png"" />
            <meta name=""msapplication-wide310x150logo"" content=""{Root}/Shared/Images/Icons/mstile-310x150.png"" />
            <meta name=""msapplication-square310x310logo"" content=""{Root}/Shared/Images/Icons/mstile-310x310.png""/>
        ".FixForwardSpacing(forceFixed: true);

        static Resources()
        {
            // Generally embedded resources are marked as anonymous, this forces the following to require authentication
            AuthenticationOptions.ResourceRequiresAuthentication("GSF.Web.Shared.Views.UserInfo.cshtml", true);
        }
    }
}
