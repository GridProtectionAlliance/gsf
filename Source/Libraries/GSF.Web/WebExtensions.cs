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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GSF.Collections;
using GSF.IO;
using GSF.Web.Model;
using RazorEngine.Templating;
using HtmlHelper = System.Web.Mvc.HtmlHelper;

namespace GSF.Web
{
    /// <summary>
    /// Defines extension methods useful for web applications.
    /// </summary>
    public static class WebExtensions
    {
        // Nested Types

        // Defines a multi-part stream provider that will parse out files and form data
        private class PostDataStreamProvider : MultipartStreamProvider
        {
            #region [ Members ]

            // Fields
            private readonly PostData m_postData;
            private readonly List<bool> m_isFormData;

            #endregion

            #region [ Constructors ]

            public PostDataStreamProvider()
            {
                m_postData = new PostData();
                m_isFormData = new List<bool>();
            }

            #endregion

            #region [ Properties ]

            // Gets any data passed as part of the multi-part form data
            public PostData PostData => m_postData;

            #endregion

            #region [ Methods ]

            // Gets the stream where to write the body part to. This method is called when a MIME multi-part body part has been parsed
            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                // For form data, Content-Disposition header is a requirement
                ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;

                if ((object)contentDisposition == null)
                    throw new InvalidOperationException("Expected \"Content-Disposition\" header field not found in MIME multi-part body.");

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
                        HttpContent formContent = Contents[i];
                        ContentDispositionHeaderValue contentDisposition = formContent.Headers.ContentDisposition;
                        string fieldName = UnquoteToken(contentDisposition.Name) ?? "";

                        // Read out any form data
                        string fieldValue = await formContent.ReadAsStringAsync();
                        m_postData.FormData.Add(fieldName, fieldValue);
                    }
                    else
                    {
                        m_postData.FileData.Add(Contents[i]);
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

        // Defines a GetMemberBinder implementation to get DynamicViewBag properties by name
        private class MemberBinder : GetMemberBinder
        {
            public MemberBinder(string name) : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) => null;
        }


        // Static Fields
        private static readonly HashSet<string> s_executingAssemblyResources;
        private static readonly HashSet<string> s_callingAssemblyResources;
        private static readonly HashSet<string> s_entryAssemblyResources;

        // Static Constructor
        static WebExtensions()
        {
            s_executingAssemblyResources = new HashSet<string>(Assembly.GetExecutingAssembly().GetManifestResourceNames(), StringComparer.Ordinal);
            s_callingAssemblyResources = new HashSet<string>(Assembly.GetCallingAssembly().GetManifestResourceNames(), StringComparer.Ordinal);
            s_entryAssemblyResources = new HashSet<string>(Assembly.GetEntryAssembly()?.GetManifestResourceNames() ?? new[] { "" }, StringComparer.Ordinal);
        }

        // Static Methods

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
        /// Gets a collection of uploaded post data from an <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request"><see cref="HttpRequestMessage"/> request data that contains form data and/or uploaded files.</param>
        /// <returns>Parsed post data.</returns>
        public static PostData GetPostData(this HttpRequestMessage request)
        {
            // Providing non-async version to simplify processing within Razor script
            Task<PostData> getPostDataTask = GetPostDataAsync(request);

            getPostDataTask.ContinueWith(task =>
            {
                if ((object)task.Exception != null)
                    throw task.Exception;
            },
            TaskContinuationOptions.OnlyOnFaulted);

            return getPostDataTask.Result;
        }

        /// <summary>
        /// Asynchronously gets a collection of uploaded post data from an <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request"><see cref="HttpRequestMessage"/> request data that contains form data and/or uploaded files.</param>
        /// <returns>Parsed post data.</returns>
        public static async Task<PostData> GetPostDataAsync(this HttpRequestMessage request)
        {
            PostDataStreamProvider provider = await request.Content.ReadAsMultipartAsync(new PostDataStreamProvider());
            return provider.PostData;
        }

        /// <summary>
        /// Converts relative URL to an absolute URL.
        /// </summary>
        /// <param name="helper">MVC HTML helper instance.</param>
        /// <param name="url">Relative URL.</param>
        /// <returns>Absolute URL.</returns>
        public static string ToAbsoluteUrl(this HtmlHelper helper, string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            if (HttpContext.Current == null)
                return "";

            if (url.StartsWith("/"))
                url = url.Insert(0, "~");

            if (!url.StartsWith("~/"))
                url = url.Insert(0, "~/");

            Uri uri = HttpContext.Current.Request.Url;
            string port = uri.Port == 80 ? "" : ":" + uri.Port;

            return $"{uri.Scheme}://{uri.Host}{port}{VirtualPathUtility.ToAbsolute(url)}";
        }

        /// <summary>
        /// Includes an HTTP resource directly into an MVC view page.
        /// </summary>
        /// <param name="helper">MVC HTML helper instance.</param>
        /// <param name="url">URL of resource to include.</param>
        /// <returns>Resolved URL resource as an MvcHtmlString.</returns>
        public static MvcHtmlString IncludeUrl(this HtmlHelper helper, string url)
        {
            if (string.IsNullOrEmpty(url))
                return new MvcHtmlString("");

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using (Stream stream = response?.GetResponseStream())
            {
                if ((object)stream == null)
                    return new MvcHtmlString($"<!-- No response from {url} -->");

                using (StreamReader reader = new StreamReader(stream))
                    return new MvcHtmlString(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Includes an embedded resource directly into an MVC view page.
        /// </summary>
        /// <param name="helper">MVC HTML helper instance.</param>
        /// <param name="resourceName">Resource to include.</param>
        /// <returns>Resource as an MvcHtmlString.</returns>
        public static MvcHtmlString IncludeResouce(this HtmlHelper helper, string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                return new MvcHtmlString("");

            Stream stream = OpenEmbeddedResourceStream(resourceName);

            if ((object)stream == null)
                return new MvcHtmlString("");

            using (StreamReader reader = new StreamReader(stream))
                return new MvcHtmlString(reader.ReadToEnd());
        }

        /// <summary>
        /// Renders a Razor view from an embedded resource.
        /// </summary>
        /// <param name="helper">MVC HTML helper instance.</param>
        /// <param name="resourceName">Embedded resource name.</param>
        /// <param name="modelType">Type of model.</param>
        /// <param name="model">Model instance.</param>
        /// <returns>Rendered resource as an MvcHtmlString.</returns>
        /// <remarks>
        /// If needed by view, define ViewBag.DataContext before rendering resource.
        /// </remarks>
        public static MvcHtmlString RenderResource(this HtmlHelper helper, string resourceName, Type modelType = null, object model = null)
        {
            MvcHtmlString result;
            DynamicViewBag viewBag = new DynamicViewBag(helper.ViewData);
            bool isPost = HttpContext.Current.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase);

            viewBag.AddValue("IsPost", isPost);
            viewBag.AddValue("Request", HttpContext.Current.Items["MS_HttpRequestMessage"] ?? new HttpRequestMessage(isPost ? HttpMethod.Post : HttpMethod.Get, HttpContext.Current.Request.Url));
           
            if (FilePath.GetExtension(resourceName).Equals(".vbhtml", StringComparison.OrdinalIgnoreCase))
                result = new MvcHtmlString(RazorEngine<VisualBasicEmbeddedResource>.Default.RunCompile(resourceName, modelType, model, viewBag));
            else
                result = new MvcHtmlString(RazorEngine<CSharpEmbeddedResource>.Default.RunCompile(resourceName, modelType, model, viewBag));

            // Copy any new or updated view bag elements back to MVC view data dictionary
            foreach (string memberName in viewBag.GetDynamicMemberNames())
            {
                object value;

                viewBag.TryGetMember(new MemberBinder(memberName), out value);

                if (value != null)
                    helper.ViewData[memberName] = value;
            }

            return result;
        }

        /// <summary>
        /// Renders a Razor view from an embedded resource.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="helper">MVC HTML helper instance.</param>
        /// <param name="resourceName">Embedded resource name.</param>
        /// <param name="model">Model instance.</param>
        /// <returns>Rendered resource as an MvcHtmlString.</returns>
        /// <remarks>
        /// If needed by view, define ViewBag.DataContext before rendering resource.
        /// </remarks>
        public static MvcHtmlString RenderResource<T>(this HtmlHelper helper, string resourceName, T model)
        {
            return RenderResource(helper, resourceName, typeof(T), model);
        }

        /// <summary>
        /// Determines if specified embedded resource exists.
        /// </summary>
        /// <param name="resourceName">Fully qualified name of resource to check.</param>
        /// <returns><c>true</c> if resource exists; otherwise, <c>false</c>.</returns>
        public static bool EmbeddedResourceExists(string resourceName)
        {
            return
                s_executingAssemblyResources.Contains(resourceName) ||
                s_callingAssemblyResources.Contains(resourceName) ||
                s_entryAssemblyResources.Contains(resourceName);
        }

        /// <summary>
        /// Opens a stream to an embedded resource.
        /// </summary>
        /// <param name="resourceName">Fully qualified name of resource to open.</param>
        /// <returns>Stream to embedded resource if found; otherwise, <c>null</c>.</returns>
        public static Stream OpenEmbeddedResourceStream(string resourceName)
        {
            if (s_executingAssemblyResources.Contains(resourceName))
                return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            if (s_callingAssemblyResources.Contains(resourceName))
                return Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName);

            if (s_entryAssemblyResources.Contains(resourceName))
                return Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName);

            return null;
        }
    }
}