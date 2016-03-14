//******************************************************************************************************
//  WebExtensions.cs - Gbtc
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
//  02/17/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using GSF.Collections;

namespace GSF.Web
{
    /// <summary>
    /// Defines extension methods useful for web applications.
    /// </summary>
    public static class WebExtensions
    {
        /// <summary>
        /// Performs JavaScript encoding on given string.
        /// </summary>
        /// <param name="text">The string to be encoded.</param>
        public static string JavaScriptEncode(this string text)
        {
            return HttpUtility.JavaScriptStringEncode(text.ToNonNullString());
        }

        /// <summary>
        /// Performs HTML encoding on the given string.
        /// </summary>
        /// <param name="text">The string to be encoded.</param>
        public static string HtmlEncode(this string text)
        {
            return HttpUtility.HtmlEncode(text.ToNonNullString());
        }

        /// <summary>
        /// Performs URL encoding on the given string.
        /// </summary>
        /// <param name="text">The string to be encoded.</param>
        public static string UrlEncode(this string text)
        {
            return HttpUtility.UrlEncode(text.ToNonNullString());
        }

        /// <summary>
        /// Gets query parameters for current request message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Dictionary<string, string> QueryParameters(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Corrects script alignment with a desired number of forward spaces.
        /// </summary>
        /// <param name="script">Script text.</param>
        /// <param name="spaces">Desired forward spaces.</param>
        /// <returns>Script with corrected forward alignment.</returns>
        public static string FixForwardSpacing(this string script, int spaces = 4)
        {
            Tuple<string, int>[] linesAndLengths = script
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(line => new Tuple<string, int>(line, line.Length - line.TrimStart(' ').Length))
                .ToArray();

            int minLength = linesAndLengths
                .Select(lineAndLength => (int?)lineAndLength.Item2)
                .Where(length => length.GetValueOrDefault() > 0)
                .Min() ?? 0;

            string forwardSpacing = new string(' ', spaces);

            return linesAndLengths
                .Select(lineAndLength => lineAndLength.Item2 > 0 ?
                    $"{forwardSpacing}{(lineAndLength.Item2 > minLength ? lineAndLength.Item1.Substring(minLength) : lineAndLength.Item1)}" :
                    lineAndLength.Item1.ToNonNullNorEmptyString())
                .ToDelimitedString(Environment.NewLine);
        }
    }
}