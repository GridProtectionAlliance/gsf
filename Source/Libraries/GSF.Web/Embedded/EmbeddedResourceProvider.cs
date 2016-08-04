//******************************************************************************************************
//  EmbeddedResourceProvider.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License a
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Hosting;

namespace GSF.Web.Embedded
{
    /// <summary>
    /// Represents a <see cref="VirtualPathProvider"/> that extends an existing provider adding access to embedded resources.
    /// </summary>
    /// <remarks>
    /// To use, add the following to Application_Start function:
    /// <code>
    ///     // Add additional virtual path provider to allow access to embedded resources
    ///     HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourceProvider());
    /// </code>
    /// Then add the namespace paths to the embedded resources for which you need access to the Web.config &lt;system.webServer&gt; section:
    /// <code>
    /// &lt;system.webServer&gt;
    ///   &lt;handlers&gt;
    ///     &lt;add name="EmbeddedResourceHandler-GSF" path="@GSF/Web/Model/Scripts/*" verb="*" type="System.Web.StaticFileHandler" allowPathInfo="true" /&gt;
    ///   &lt;/handlers&gt;
    /// &lt;/system.webServer&gt;
    /// </code>
    /// </remarks>
    public class EmbeddedResourceProvider : VirtualPathProvider
    {
        #region [ Members ]

        // Nested Types

        private class EmbeddedResource : VirtualFile
        {
            public EmbeddedResource(string virtualPath) : base(virtualPath)
            {
            }

            public override Stream Open() => OpenEmbeddedResourceStream(VirtualPath);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets a value that indicates whether a file exists in the virtual file system.
        /// </summary>
        /// <returns>true if the file exists in the virtual file system; otherwise, false.</returns>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <remarks>
        /// Any virtual path that begins with an @ symbol is assumed to be an embedded resource where
        /// the path represents the namespace hierarchy in either slash or dot notation.
        /// </remarks>
        public override bool FileExists(string virtualPath)
        {
            return IsEmbeddedResource(ParseResourceNameFromVirtualPath(virtualPath)) || Previous.FileExists(virtualPath);
        }

        /// <summary>
        /// Gets a file from the virtual file system.
        /// </summary>
        /// <returns>An implementation of the <see cref="VirtualFile"/> class that represents a file in the virtual file system.</returns>
        /// <param name="virtualPath">The path to the virtual file.</param>
        public override VirtualFile GetFile(string virtualPath)
        {
            string resourceName = ParseResourceNameFromVirtualPath(virtualPath);
            return IsEmbeddedResource(resourceName) ? new EmbeddedResource(resourceName) : Previous.GetFile(virtualPath);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly HashSet<string> s_executingAssemblyResources;
        private static readonly HashSet<string> s_entryAssemblyResources;

        // Static Constructor
        static EmbeddedResourceProvider()
        {
            s_executingAssemblyResources = new HashSet<string>(Assembly.GetExecutingAssembly().GetManifestResourceNames(), StringComparer.Ordinal);
            s_entryAssemblyResources = new HashSet<string>(Assembly.GetEntryAssembly()?.GetManifestResourceNames() ?? new[] { "" }, StringComparer.Ordinal);
        }

        // Static Methods
        private static Stream OpenEmbeddedResourceStream(string resourceName)
        {
            try
            {
                // Check for local resource first, then fall back on a resource in source assembly
                return s_executingAssemblyResources.Contains(resourceName) ?
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) :
                    Assembly.GetEntryAssembly()?.GetManifestResourceStream(resourceName);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsEmbeddedResource(string resourceName)
        {
            return s_executingAssemblyResources.Contains(resourceName) || s_entryAssemblyResources.Contains(resourceName);
        }

        private static string ParseResourceNameFromVirtualPath(string virtualPath)
        {
            if (virtualPath.StartsWith("~/@"))
                virtualPath = virtualPath.Substring(3);

            if (virtualPath.StartsWith("/@"))
                virtualPath = virtualPath.Substring(2);

            if (virtualPath.StartsWith("@"))
                virtualPath = virtualPath.Substring(1);

            return virtualPath.Replace('/', '.');
        }

        #endregion
    }
}
