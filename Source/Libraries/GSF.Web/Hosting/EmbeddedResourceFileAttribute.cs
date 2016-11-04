//******************************************************************************************************
//  EmbeddedResourceFileAttribute.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/04/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text.RegularExpressions;

namespace GSF.Web.Hosting
{
	/// <summary>
	/// Attribute indicating the location of an embedded resource that should be
	/// served by the <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This attribute is used by the <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider"/>
	/// module/path provider to retrieve the list of embedded resources from an assembly
	/// that should be considered a part of the virtual filesystem.
	/// </para>
	/// </remarks>
	/// <example>
	/// <para>
	/// Below is an example of what it might look like to embed a web form and a
	/// user control in an assembly to be served up with the <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider"/>:
	/// </para>
	/// <code lang="C#">
	/// [assembly: EmbeddedResourceFileAttribute("MyNamespace.WebForm1.aspx", "MyNamespace")]
	/// [assembly: EmbeddedResourceFileAttribute("MyNamespace.UserControl1.ascx", "MyNamespace")]
	/// </code>
	/// </example>
	/// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider"/>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class EmbeddedResourceFileAttribute : Attribute
	{
        #region [ Members ]

        // Fields

        /// <summary>
        /// Internal storage for the
        /// <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute.ResourceNamespace" />
        /// property.
        /// </summary>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
        private readonly string m_resourceNamespace;

        /// <summary>
        /// Internal storage for the
        /// <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute.ResourcePath" />
        /// property.
        /// </summary>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
        private readonly string m_resourcePath;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" /> class.
        /// </summary>
        /// <param name="resourcePath">The path to the embedded resource.  Used to get the resource as a stream from the assembly.</param>
        /// <param name="resourceNamespace">The namespace the resource is in.  This will generally be removed from the full resource path to calculate the "application path" for the embedded resource.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="resourcePath" /> or <paramref name="resourceNamespace" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="resourcePath" /> is <see cref="System.String.Empty" />
        /// or if it only consists of periods and/or spaces.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Both <paramref name="resourcePath" /> and <paramref name="resourceNamespace" />
        /// will be processed to have leading and trailing periods and spaces removed.
        /// If <paramref name="resourcePath" /> ends up being empty, an
        /// <see cref="System.ArgumentOutOfRangeException"/> is thrown.  No exception
        /// is thrown if <paramref name="resourceNamespace" /> turns out empty.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// If the <paramref name="resourcePath" /> is <c>RootNS.SubNS.AppRoot.Folder.File.aspx</c>
        /// and the <paramref name="resourceNamespace" /> is <c>RootNS.SubNS.AppRoot</c>,
        /// the virtual "path" to the embedded file will be <c>Folder.File.aspx</c>
        /// (which will be converted by the <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider"/>
        /// to <c>~/Folder/File.aspx</c>).
        /// </para>
        /// </example>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
        public EmbeddedResourceFileAttribute(string resourcePath, string resourceNamespace)
        {
            if (resourcePath == null)
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }
            if (resourcePath.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resourcePath));
            }
            if (resourceNamespace == null)
            {
                throw new ArgumentNullException(nameof(resourceNamespace));
            }

            m_resourcePath = RemoveMalformedEndChars(resourcePath);
            if (m_resourcePath.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resourcePath), resourcePath, "The resource path is invalid for mapping.");
            }
            m_resourceNamespace = RemoveMalformedEndChars(resourceNamespace);
            if (m_resourceNamespace.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resourceNamespace), resourceNamespace, "The resource namespace is invalid for mapping.");
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the namespace for the embedded resource.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> with the embedded resource namespace.
        /// </value>
        /// <remarks>
        /// <para>
        /// This namespace will be removed from the full <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute.ResourcePath"/>
        /// to create the virtual application path for the resource.
        /// </para>
        /// </remarks>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
        public string ResourceNamespace
        {
            get
            {
                return m_resourceNamespace;
            }
        }

        /// <summary>
        /// Gets the path to the embedded resource.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> with the full path to an embedded resource in
        /// the associated assembly.
        /// </value>
        /// <remarks>
        /// <para>
        /// This path will be used to retrieve the resource from the assembly and serve
        /// it up.
        /// </para>
        /// </remarks>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
        public string ResourcePath
        {
            get
            {
                return m_resourcePath;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Removes leading and trailing dots and spaces from a string.
        /// </summary>
        /// <param name="toFix">The <see cref="System.String"/> to be cleaned up.</param>
        /// <returns>A version of <paramref name="toFix" /> with leading and trailing dots and spaces removed.</returns>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
        private static string RemoveMalformedEndChars(string toFix)
        {
            return MalformedEndCharExpression.Replace(toFix, "$1");
        }

        #endregion

        #region [ Static ]

        // Static Fields

        /// <summary>
        /// Regular expression indicating the characters that can't start or end a resource.
        /// </summary>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute.RemoveMalformedEndChars" />
        private static readonly Regex MalformedEndCharExpression = new Regex("^[. ]*(.*?)[. ]*$", RegexOptions.Compiled);

        #endregion
	}
}
