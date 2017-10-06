//******************************************************************************************************
//  UrlHelper.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  08/05/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Web;

namespace GSF.Web.Model
{
    /// <summary>
    /// Defines a URL help class for <see cref="IRazorEngine"/> views.
    /// </summary>
    /// <remarks>
    /// This class provides Razor view compatibility functions so that the same view syntax, e.g.,
    /// @Url.Content(""), can be used in both self-hosted and ASP.NET hosted web sites.
    /// </remarks>
    public class UrlHelper
    {
        #region [ Constructors ]

        // Creates a new URL helper class
        internal UrlHelper()
        {
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a string that contains a content URL.
        /// </summary>
        /// <param name="contentPath">The content path.</param>
        /// <returns>A string that contains a content URL.</returns>
        public string Content(string contentPath)
        {
            if ((object)HttpContext.Current != null)
                return VirtualPathUtility.ToAbsolute(contentPath);

            if (contentPath.StartsWith("~"))
                contentPath = contentPath.Substring(1);

            return contentPath;
        }

        /// <summary>
        /// Encodes special characters in a URL string into character-entity equivalents.
        /// </summary>
        /// <returns>An encoded URL string.</returns>
        /// <param name="url">The text to encode.</param>
        public string Encode(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        #endregion
    }
}
