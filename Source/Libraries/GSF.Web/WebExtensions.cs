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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using GSF.Collections;

namespace GSF.Web
{
    /// <summary>
    /// Defines extension methods useful for web applications.
    /// </summary>
    public static class WebExtensions
    {
        // Nested Types

        // Defines a multi-part stream provider that will parse out files and form data
        private class FileStreamStreamProvider : MultipartStreamProvider
        {
            #region [ Members ]

            // Fields
            private readonly NameValueCollection m_formData;
            private readonly List<HttpContent> m_fileData;
            private readonly List<bool> m_isFormData;

            #endregion

            #region [ Constructors ]
            public FileStreamStreamProvider(NameValueCollection formData)
            {
                m_formData = formData;
                m_fileData = new List<HttpContent>();
                m_isFormData = new List<bool>();
            }

            #endregion

            #region [ Properties ]

            // Gets any form data passed as part of the multipart form data
            public NameValueCollection FormData => m_formData;

            /// Gets list of uploaded files
            public List<HttpContent> FileData => m_fileData;

            #endregion

            #region [ Methods ]

            // Gets the stream where to write the body part to. This method is called when a MIME multipart body part has been parsed
            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                // For form data, Content-Disposition header is a requirement
                ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;

                if (contentDisposition == null)
                    throw new InvalidOperationException("Expected \"Content-Disposition\" header field not found in MIME multipart body.");

                // Post process this body part as form data if no file name is specified
                m_isFormData.Add(string.IsNullOrEmpty(contentDisposition.FileName));

                return new MemoryStream();
            }

            // Executes the post processing operation reading the non-file contents as form data
            public override async Task ExecutePostProcessingAsync()
            {
                for (int i = 0; i < Contents.Count; i++)
                {
                    if (m_isFormData[i])
                    {
                        // Ignore form data if user did not request it...
                        if (m_formData == null)
                            continue;

                        HttpContent formContent = Contents[i];
                        ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
                        string fieldName = UnquoteToken(contentDisposition.Name) ?? "";

                        // Read out any form data
                        string fieldValue = await formContent.ReadAsStringAsync();
                        FormData.Add(fieldName, fieldValue);
                    }
                    else
                    {
                        m_fileData.Add(Contents[i]);
                    }
                }
            }

            #endregion

            #region [ Static ]

            // Static Methods
            private static string UnquoteToken(string token)
            {
                if (string.IsNullOrWhiteSpace(token))
                    return token;

                if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
                    return token.Substring(1, token.Length - 2);

                return token;
            }

            #endregion
        }

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
        /// Asynchronously gets a collection of uploaded file data from an <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request"><see cref="HttpRequestMessage"/> request data that contains uploaded files.</param>
        /// <param name="formData">Optional form data from <paramref name="request"/> if needed.</param>
        /// <returns></returns>
        public static async Task<IList<HttpContent>> GetFilesAsync(this HttpRequestMessage request, NameValueCollection formData = null)
        {
            FileStreamStreamProvider provider = await request.Content.ReadAsMultipartAsync(new FileStreamStreamProvider(formData));
            return provider.FileData;
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