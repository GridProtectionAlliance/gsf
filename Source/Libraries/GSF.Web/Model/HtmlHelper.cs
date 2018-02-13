//******************************************************************************************************
//  HtmlHelper.cs - Gbtc
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
//  08/04/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using GSF.IO;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace GSF.Web.Model
{
    /// <summary>
    /// Defines an HTML helper class for <see cref="IRazorEngine"/> views.
    /// </summary>
    /// <remarks>
    /// This class provides Razor view compatibility functions so that the same view syntax, e.g.,
    /// @Html.Raw(""), can be used in both self-hosted and ASP.NET hosted web sites.
    /// </remarks>
    public class HtmlHelper
    {
        #region [ Members ]

        // Fields
        private readonly TemplateBase m_parent;

        #endregion

        #region [ Constructors ]

        // Creates a new HTML helper class associated with parent template
        internal HtmlHelper(TemplateBase parent)
        {
            m_parent = parent;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Generates a hidden form field (anti-forgery token) that is validated when the form is submitted.
        /// </summary>
        /// <returns>The generated form field (anti-forgery token).</returns>
        /// <remarks>
        /// The anti-forgery token can be used to help protect your application against cross-site request forgery.
        /// To use this feature, call the AntiForgeryToken method from a form and add the
        /// <see cref="ValidateAntiForgeryTokenAttribute"/> attribute to the action method that you want to protect.
        /// </remarks>
        public IEncodedString AntiForgeryToken() => new RawString(AntiForgery.GetHtml().ToString());

        /// <summary>
        /// Generates an anti-forgery token that can be manually added to a "RequestVerificationToken" HTTP header,
        /// e.g., from within an AJAX request.
        /// </summary>
        /// <returns>Anti-forgery token to be added as the "RequestVerificationToken" HTTP header value.</returns>
        public string AntiForgeryTokenHeader()
        {
            string cookieToken, formToken;

            AntiForgery.GetTokens(null, out cookieToken, out formToken);

            return $"{cookieToken}:{formToken}";
        }

        /// <summary>
        /// Returns a string value that is not HTML encoded.
        /// </summary>
        /// <param name="value">Raw string value to return.</param>
        /// <returns>Raw string value that is not HTML encoded.</returns>
        public IEncodedString Raw(string value) => new RawString(value);

        /// <summary>
        /// Includes the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <param name="modelType"></param>
        /// <returns>The template writer helper.</returns>
        public TemplateWriter Partial(string name, object model = null, Type modelType = null)
        {
            return m_parent.Include(name, model, modelType);
        }

        /// <summary>
        /// Converts relative URL to an absolute URL.
        /// </summary>
        /// <param name="url">Relative URL.</param>
        /// <param name="request">Request message.</param>
        /// <returns>Absolute URL.</returns>
        /// <remarks>
        /// If HttpRequestMessage is not specified, function will attempt to find message in ViewBag.Request.
        /// </remarks>
        public string ToAbsoluteUrl(string url, HttpRequestMessage request = null)
        {
            if (string.IsNullOrEmpty(url))
                return "";

            if ((object)request == null)
                request = m_parent.ViewBag.Request;

            if ((object)request == null)
                throw new ArgumentNullException(nameof(request));

            Uri uri = new Uri(request.RequestUri.AbsoluteUri.Replace(request.RequestUri.PathAndQuery, ""));
            string port = uri.Port == 80 ? "" : ":" + uri.Port;

            return $"{uri.Scheme}://{uri.Host}{port}{VirtualPathUtility.ToAbsolute(url)}";
        }

        /// <summary>
        /// Includes an HTTP resource directly into view page.
        /// </summary>
        /// <param name="url">URL of resource to include.</param>
        /// <returns>Resolved URL resource as an encoded string.</returns>
        public IEncodedString IncludeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return new RawString("");

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Credentials = CredentialCache.DefaultCredentials;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            using (Stream stream = response?.GetResponseStream())
            {
                if ((object)stream == null)
                    return new RawString($"<!-- No response from {url} -->");

                using (StreamReader reader = new StreamReader(stream))
                    return new RawString(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Includes an embedded resource directly into view page.
        /// </summary>
        /// <param name="resourceName">Resource to include.</param>
        /// <returns>Resource as an encoded string.</returns>
        public IEncodedString IncludeResource(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                return new RawString("");

            Stream stream = WebExtensions.OpenEmbeddedResourceStream(resourceName);

            if ((object)stream == null)
                return new RawString("");

            using (StreamReader reader = new StreamReader(stream))
                return new RawString(reader.ReadToEnd());
        }

        /// <summary>
        /// Renders a Razor view from an embedded resource.
        /// </summary>
        /// <param name="resourceName">Embedded resource name.</param>
        /// <param name="modelType">Type of model.</param>
        /// <param name="model">Model instance.</param>
        /// <returns>Rendered resource as an encoded string.</returns>
        public IEncodedString RenderResource(string resourceName, Type modelType = null, object model = null)
        {
            if (FilePath.GetExtension(resourceName).Equals(".vbhtml", StringComparison.OrdinalIgnoreCase))
                return new RawString(RazorEngine<VisualBasicEmbeddedResource>.Default.RunCompile(resourceName, modelType, model, m_parent.ViewBag as DynamicViewBag));

            return new RawString(RazorEngine<CSharpEmbeddedResource>.Default.RunCompile(resourceName, modelType, model, m_parent.ViewBag as DynamicViewBag));
        }

        /// <summary>
        /// Renders a Razor view from an embedded resource.
        /// </summary>
        /// <typeparam name="T">Type of model.</typeparam>
        /// <param name="resourceName">Embedded resource name.</param>
        /// <param name="model">Model instance.</param>
        /// <returns>Rendered resource as an encoded string.</returns>
        public IEncodedString RenderResource<T>(string resourceName, T model)
        {
            return RenderResource(resourceName, typeof(T), model);
        }

        #endregion
    }
}
