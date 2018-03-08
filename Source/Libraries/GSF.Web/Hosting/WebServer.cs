//******************************************************************************************************
//  WebServer.cs - Gbtc
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
//  05/10/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;
using GSF.IO.Checksums;
using GSF.Reflection;
using GSF.Web.Model;
using GSF.Web.Security;
using Microsoft.Ajax.Utilities;

#pragma warning disable SG0018 // Path traversal prevented by web api controller

namespace GSF.Web.Hosting
{
    /// <summary>
    /// Represents the web server engine for <see cref="WebPageController"/> instances.
    /// </summary>
    public class WebServer
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Exposes execution exceptions that occur in <see cref="WebPageController"/> instances.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExecutionException;

        /// <summary>
        /// Exposes status messages that get reported from <see cref="WebPageController"/> instances.
        /// </summary>
        public event EventHandler<EventArgs<string>> StatusMessage;

        // Fields
        private readonly WebServerOptions m_options;
        private readonly bool m_releaseMode;
        private readonly ConcurrentDictionary<string, long> m_etagCache;
        private readonly ConcurrentDictionary<string, Type> m_handlerTypeCache;
        private readonly SafeFileWatcher m_fileWatcher;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="WebServer"/>.
        /// </summary>
        /// <param name="options">Web server options to use; set to <c>null</c> for defaults.</param>
        /// <param name="razorEngineCS">Razor engine instance for .cshtml templates; uses default instance if not provided.</param>
        /// <param name="razorEngineVB">Razor engine instance for .vbhtml templates; uses default instance if not provided.</param>
        public WebServer(WebServerOptions options = null, IRazorEngine razorEngineCS = null, IRazorEngine razorEngineVB = null)
        {
            m_releaseMode = !AssemblyInfo.EntryAssembly.Debuggable;

            if ((object)options == null)
                options = new WebServerOptions();

            m_options = options;
            RazorEngineCS = razorEngineCS ?? (m_releaseMode ? RazorEngine<CSharp>.Default : RazorEngine<CSharpDebug>.Default as IRazorEngine);
            RazorEngineVB = razorEngineVB ?? (m_releaseMode ? RazorEngine<VisualBasic>.Default : RazorEngine<VisualBasicDebug>.Default as IRazorEngine);
            PagedViewModelTypes = new ConcurrentDictionary<string, Tuple<Type, Type>>(StringComparer.InvariantCultureIgnoreCase);
            m_etagCache = new ConcurrentDictionary<string, long>(StringComparer.InvariantCultureIgnoreCase);
            m_handlerTypeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

            // Validate web root path
            m_options.WebRootPath = FilePath.AddPathSuffix(m_options.WebRootPath ?? RazorEngineCS.TemplatePath);

            m_fileWatcher = new SafeFileWatcher(m_options.WebRootPath)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            m_fileWatcher.Changed += m_fileWatcher_FileChange;
            m_fileWatcher.Deleted += m_fileWatcher_FileChange;
            m_fileWatcher.Renamed += m_fileWatcher_FileChange;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets options in use for this <see cref="WebServer"/>.
        /// </summary>
        public ReadonlyWebServerOptions Options => m_options.Readonly;

        /// <summary>
        /// Gets the C# Razor engine instance used by this <see cref="WebServer"/>.
        /// </summary>
        public IRazorEngine RazorEngineCS { get; }

        /// <summary>
        /// Gets the Visual Basic Razor engine instance used by this <see cref="WebServer"/>.
        /// </summary>
        public IRazorEngine RazorEngineVB { get; }

        /// <summary>
        /// Defines associated page view model types and data hub types for Razor pages, if any.
        /// </summary>
        /// <remarks>
        /// This dictionary associates Razor views based on a paged view model with associated <see cref="Type"/> values.
        /// </remarks>
        public ConcurrentDictionary<string, Tuple<Type, Type>> PagedViewModelTypes { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="WebServer"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="WebServer"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_fileWatcher?.Dispose();
                }
                finally
                {
                    m_disposed = true; // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Renders an HTTP response for a given request.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="pageName">Name of page to render.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        /// <param name="model">Reference to model to use when rendering Razor templates, if any.</param>
        /// <param name="modelType">Type of <paramref name="model"/>, if any.</param>
        /// <param name="database"><see cref="AdoDataConnection"/> to use, if any.</param>
        /// <returns>HTTP response for provided request.</returns>
        public async Task<HttpResponseMessage> RenderResponse(HttpRequestMessage request, string pageName, CancellationToken cancellationToken, object model = null, Type modelType = null, AdoDataConnection database = null)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string content, fileExtension = FilePath.GetExtension(pageName).ToLowerInvariant();
            bool embeddedResource = pageName.StartsWith("@");
            Tuple<Type, Type> pagedViewModelTypes;

            if (embeddedResource)
                pageName = pageName.Substring(1).Replace('/', '.');

            response.RequestMessage = request;

            if (pageName.Equals(m_options.AuthTestPage, StringComparison.OrdinalIgnoreCase))
                return CreateAuthTestResponse(request, response);

            switch (fileExtension)
            {
                case ".cshtml":
                    PagedViewModelTypes.TryGetValue(pageName, out pagedViewModelTypes);
                    content = await new RazorView(embeddedResource ? RazorEngine<CSharpEmbeddedResource>.Default : RazorEngineCS, pageName, model, modelType, pagedViewModelTypes?.Item1, pagedViewModelTypes?.Item2, database, OnExecutionException, Options).ExecuteAsync(request, response, cancellationToken);
                    response.Content = new StringContent(content, Encoding.UTF8, "text/html");
                    break;
                case ".vbhtml":
                    PagedViewModelTypes.TryGetValue(pageName, out pagedViewModelTypes);
                    content = await new RazorView(embeddedResource ? RazorEngine<VisualBasicEmbeddedResource>.Default : RazorEngineVB, pageName, model, modelType, pagedViewModelTypes?.Item1, pagedViewModelTypes?.Item2, database, OnExecutionException, Options).ExecuteAsync(request, response, cancellationToken);
                    response.Content = new StringContent(content, Encoding.UTF8, "text/html");
                    break;
                case ".ashx":
                    await ProcessHTTPHandlerAsync(pageName, embeddedResource, request, response, cancellationToken);
                    break;
                default:
                    string fileName = GetResourceFileName(pageName, embeddedResource);

                    if (m_options.ClientCacheEnabled)
                    {
                        long responseHash;

                        if (!m_etagCache.TryGetValue(fileName, out responseHash))
                        {
                            if (!ResourceExists(fileName, embeddedResource))
                            {
                                response.StatusCode = HttpStatusCode.NotFound;
                                break;
                            }

                            await Task.Run(async () =>
                            {
                                using (Stream source = await OpenResourceAsync(fileName, embeddedResource, cancellationToken))
                                {
                                    // Calculate check-sum for file
                                    const int BufferSize = 32768;
                                    byte[] buffer = new byte[BufferSize];
                                    Crc32 calculatedHash = new Crc32();

                                    int bytesRead = await source.ReadAsync(buffer, 0, BufferSize, cancellationToken);

                                    while (bytesRead > 0)
                                    {
                                        calculatedHash.Update(buffer, 0, bytesRead);
                                        bytesRead = await source.ReadAsync(buffer, 0, BufferSize, cancellationToken);
                                    }

                                    responseHash = calculatedHash.Value;
                                    m_etagCache.TryAdd(fileName, responseHash);

                                    OnStatusMessage($"Cache [{responseHash}] added for file \"{fileName}\"");
                                }
                            }, cancellationToken);
                        }

                        if (PublishResponseContent(request, response, responseHash))
                        {
                            response.Content = new StreamContent(await OpenResourceAsync(fileName, embeddedResource, cancellationToken));
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(pageName));
                        }
                    }
                    else
                    {
                        if (!ResourceExists(fileName, embeddedResource))
                        {
                            response.StatusCode = HttpStatusCode.NotFound;
                            break;
                        }

                        response.Content = new StreamContent(await OpenResourceAsync(fileName, embeddedResource, cancellationToken));
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(pageName));
                    }
                    break;
            }

            return response;
        }

        private async Task ProcessHTTPHandlerAsync(string pageName, bool embeddedResource, HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string fileName = GetResourceFileName(pageName, embeddedResource);
            Type handlerType;

            if (!m_handlerTypeCache.TryGetValue(fileName, out handlerType))
            {
                if (!ResourceExists(fileName, embeddedResource))
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return;
                }

                using (Stream source = await OpenResourceAsync(fileName, embeddedResource, cancellationToken))
                {
                    string handlerHeader, className;

                    // Parse class name from ASHX handler header parameters
                    using (StreamReader reader = new StreamReader(source))
                        handlerHeader = (await reader.ReadToEndAsync()).RemoveCrLfs().Trim();

                    // Clean up header formatting to make parsing easier
                    handlerHeader = handlerHeader.RemoveDuplicateWhiteSpace().Replace(" =", "=").Replace("= ", "=");

                    string[] tokens = handlerHeader.Split(' ');

                    if (!tokens.Any(token => token.Equals("WebHandler", StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException($"Expected \"WebHandler\" file type not found in ASHX file header: {handlerHeader}");

                    Dictionary<string, string> parameters = handlerHeader.ReplaceCaseInsensitive("WebHandler", "").Replace("<%", "").Replace("%>", "").Replace("@", "").Trim().ParseKeyValuePairs(' ');

                    if (!parameters.TryGetValue("Class", out className))
                        throw new InvalidOperationException($"Missing \"Class\" parameter in ASHX file header: {handlerHeader}");

                    // Remove quotes from class name
                    className = className.Substring(1, className.Length - 2).Trim();

                    handlerType = AssemblyInfo.FindType(className);

                    if (m_handlerTypeCache.TryAdd(fileName, handlerType))
                        OnStatusMessage($"Cached handler type [{handlerType?.FullName}] for file \"{fileName}\"");
                }
            }

            IHostedHttpHandler handler = null;

            if ((object)handlerType != null)
                handler = Activator.CreateInstance(handlerType) as IHostedHttpHandler;

            if ((object)handler == null)
                throw new InvalidOperationException($"Failed to create hosted HTTP handler \"{handlerType?.FullName}\" - make sure class implements IHostedHttpHandler interface.");

            if (m_options.ClientCacheEnabled && handler.UseClientCache)
            {
                if (PublishResponseContent(request, response, handler.GetContentHash(request)))
                    await handler.ProcessRequestAsync(request, response, cancellationToken);
            }
            else
            {
                await handler.ProcessRequestAsync(request, response, cancellationToken);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetResourceFileName(string pageName, bool embeddedResource)
        {
            return embeddedResource ? pageName : FilePath.GetAbsolutePath($"{m_options.WebRootPath}{pageName.Replace('/', Path.DirectorySeparatorChar)}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ResourceExists(string fileName, bool embeddedResource)
        {
            return embeddedResource ? WebExtensions.EmbeddedResourceExists(fileName) : File.Exists(fileName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<Stream> OpenResourceAsync(string fileName, bool embeddedResource, CancellationToken cancellationToken)
        {
            Stream stream = embeddedResource ? WebExtensions.OpenEmbeddedResourceStream(fileName) : File.OpenRead(fileName);

            if (!(m_releaseMode || m_options.UseMinifyInDebug) || !(m_options.MinifyJavascript || m_options.MinifyStyleSheets))
                return stream;

            string extension = FilePath.GetExtension(fileName).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension))
                return stream;

            Minifier minifier = new Minifier();
            Stream minimizedStream = null;

            switch (extension)
            {
                case ".js":
                    if (m_options.MinifyJavascript)
                    {
                        await Task.Run(async () =>
                        {
                            using (StreamReader reader = new StreamReader(stream))
                                minimizedStream = await minifier.MinifyJavaScript(await reader.ReadToEndAsync()).ToStreamAsync();
                        }, cancellationToken);
                    }
                    break;
                case ".css":
                    if (m_options.MinifyStyleSheets)
                    {
                        await Task.Run(async () =>
                        {
                            using (StreamReader reader = new StreamReader(stream))
                                minimizedStream = await minifier.MinifyStyleSheet(await reader.ReadToEndAsync()).ToStreamAsync();
                        }, cancellationToken);
                    }
                    break;
            }

            return minimizedStream ?? stream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool PublishResponseContent(HttpRequestMessage request, HttpResponseMessage response, long responseHash)
        {
            if (!m_options.ClientCacheEnabled)
                return true;

            // See if client's version of cached resource is up to date
            foreach (EntityTagHeaderValue headerValue in request.Headers.IfNoneMatch)
            {
                long requestHash;

                if (long.TryParse(headerValue.Tag?.Substring(1, headerValue.Tag.Length - 2), out requestHash) && responseHash == requestHash)
                {
                    response.StatusCode = HttpStatusCode.NotModified;
                    return false;
                }
            }

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = new TimeSpan(31536000 * TimeSpan.TicksPerSecond)
            };

            response.Headers.ETag = new EntityTagHeaderValue($"\"{responseHash}\"");
            return true;
        }

        private HttpResponseMessage CreateAuthTestResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            response.Content = new StringContent($@"
                <html>
                <head>
                    <title>Authentication Test</title>
                    <link rel=""shortcut icon"" href=""@GSF/Web/Shared/Images/Icons/favicon.ico"" />
                </head>
                <body>
                    {(int)response.StatusCode} ({response.StatusCode}) for user
                    {request.GetRequestContext().Principal?.Identity.Name ?? "Undefined"}
                </body>
                </html>
            ");

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }

        private void OnExecutionException(Exception exception)
        {
            ExecutionException?.Invoke(this, new EventArgs<Exception>(exception));
        }

        private void OnStatusMessage(string message)
        {
            StatusMessage?.Invoke(this, new EventArgs<string>(message));
        }

        private void m_fileWatcher_FileChange(object sender, FileSystemEventArgs e)
        {
            long responseHash;
            Type handlerType;

            if (m_etagCache.TryRemove(e.FullPath, out responseHash))
                OnStatusMessage($"Cache [{responseHash}] cleared for file \"{e.FullPath}\"");

            if (m_handlerTypeCache.TryRemove(e.FullPath, out handlerType))
                OnStatusMessage($"Cleared handler type [{handlerType?.FullName}] from cache for file \"{e.FullPath}\"");
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static WebServer s_defaultServer;
        private static readonly Dictionary<string, WebServer> s_configuredServers;

        // Static Properties

        /// <summary>
        /// Gets default configured web server instance.
        /// </summary>
        public static WebServer Default => s_defaultServer ?? (s_defaultServer = GetConfiguredServer());

        // Static Constructor
        static WebServer()
        {
            s_configuredServers = new Dictionary<string, WebServer>(StringComparer.OrdinalIgnoreCase);
        }

        // Static Methods

        /// <summary>
        /// Gets a shared <see cref="WebServer"/> instance created based on configured options defined in the specified <paramref name="settingsCategory"/>.
        /// </summary>
        /// <param name="settingsCategory">Settings category to use for web server options; defaults to "systemSettings".</param>
        /// <param name="razorEngineCS">Razor engine instance for .cshtml templates; uses default instance if not provided.</param>
        /// <param name="razorEngineVB">Razor engine instance for .vbhtml templates; uses default instance if not provided.</param>
        /// <returns>Shared <see cref="WebServer"/> instance created based on configured options.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        public static WebServer GetConfiguredServer(string settingsCategory = null, IRazorEngine razorEngineCS = null, IRazorEngine razorEngineVB = null)
        {
            lock (typeof(WebServer))
            {
                if (string.IsNullOrWhiteSpace(settingsCategory))
                    settingsCategory = "systemSettings";

                return s_configuredServers.GetOrAdd(settingsCategory, category =>
                {
                    // Get configured web server settings
                    CategorizedSettingsElementCollection settings = ConfigurationFile.Current.Settings[category];

                    settings.Add("WebRootPath", WebServerOptions.DefaultWebRootPath, "The root path for the hosted web server files. Location will be relative to install folder if full path is not specified.");
                    settings.Add("ClientCacheEnabled", WebServerOptions.DefaultClientCacheEnabled, "Determines if cache control is enabled for web server when rendering content to browser clients.");
                    settings.Add("MinifyJavascript", WebServerOptions.DefaultMinifyJavascript, "Determines if minification should be applied to rendered Javascript files.");
                    settings.Add("MinifyStyleSheets", WebServerOptions.DefaultMinifyStyleSheets, "Determines if minification should be applied to rendered CSS files.");
                    settings.Add("UseMinifyInDebug", WebServerOptions.DefaultUseMinifyInDebug, "Determines if minification should be applied when running a Debug build.");
                    settings.Add("SessionToken", SessionHandler.DefaultSessionToken, "Defines the token used for identifying the session ID in cookie headers.");
                    settings.Add("AuthTestPage", AuthenticationOptions.DefaultAuthTestPage, "Defines the page name for the web server to test if a user is authenticated.");

                    WebServerOptions options = new WebServerOptions
                    {
                        WebRootPath = settings["WebRootPath"].ValueAs(WebServerOptions.DefaultWebRootPath),
                        ClientCacheEnabled = settings["ClientCacheEnabled"].ValueAsBoolean(WebServerOptions.DefaultClientCacheEnabled),
                        MinifyJavascript = settings["MinifyJavascript"].ValueAsBoolean(WebServerOptions.DefaultMinifyJavascript),
                        MinifyStyleSheets = settings["MinifyStyleSheets"].ValueAsBoolean(WebServerOptions.DefaultMinifyStyleSheets),
                        UseMinifyInDebug = settings["UseMinifyInDebug"].ValueAsBoolean(WebServerOptions.DefaultUseMinifyInDebug),
                        SessionToken = settings["SessionToken"].ValueAs(SessionHandler.DefaultSessionToken),
                        AuthTestPage = settings["AuthTestPage"].ValueAs(AuthenticationOptions.DefaultAuthTestPage)
                    };

                    return new WebServer(options, razorEngineCS, razorEngineVB);
                });
            }
        }

        #endregion
    }
}