//******************************************************************************************************
//  EmbeddedResourcePathProvider.cs - Gbtc
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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using GSF.Configuration;
using GSF.IO;

namespace GSF.Web.Hosting
{
    /// <summary>
    /// A <see cref="System.Web.Hosting.VirtualPathProvider"/> that allows serving
    /// pages from embedded resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ASP.NET retrieves files to serve via the <see cref="System.Web.Hosting.HostingEnvironment"/>.
    /// Rather than opening a file via <see cref="System.IO.File"/>, you ask
    /// the <see cref="System.Web.Hosting.HostingEnvironment"/> for its
    /// <see cref="System.Web.Hosting.HostingEnvironment.VirtualPathProvider"/>
    /// and ask that provider for the file.  The provider will return a
    /// <see cref="System.Web.Hosting.VirtualFile"/> reference that will allow you
    /// to open a stream on the file and use the contents.
    /// </para>
    /// <para>
    /// This implementation of <see cref="System.Web.Hosting.VirtualPathProvider"/>
    /// allows you to serve files to ASP.NET through embedded resources.  Rather
    /// than deploying your web forms, user controls, etc., to the file system,
    /// you can embed the files as resources right in your assembly and deploy
    /// just your assembly.  The <see cref="System.Web.Hosting.VirtualPathProvider"/>
    /// mechanism will take care of the rest.
    /// </para>
    /// <note type="caution">
    /// Most <see cref="System.Web.Hosting.VirtualPathProvider"/> implementations
    /// handle both directories and files.  This implementation handles only files.
    /// As such, if the <see cref="System.Web.Hosting.VirtualPathProvider"/> is
    /// used to enumerate available files (as in directory browsing), files provided
    /// via embedded resource will not be included.
    /// </note>
    /// <para>
    /// To use this <see cref="System.Web.Hosting.VirtualPathProvider"/>, you need
    /// to do four things to your web application.
    /// </para>
    /// <para>
    /// First, you need to set all of the files you want to serve from your assembly
    /// as embedded resources.  By default, web forms and so forth are set as "content"
    /// files; setting them as embedded resources will package them into your assembly.
    /// </para>
    /// <para>
    /// Second, in your <c>AssemblyInfo.cs</c> file (or whichever file you are
    /// declaring your assembly attributes in) you need to add one
    /// <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute"/> for
    /// every file you plan on serving.  This lets the provider know which embedded
    /// resources are available and which are actually resources for other purposes.
    /// Your assembly attributes will look something like this:
    /// </para>
    /// <code lang="C#">
    /// [assembly: EmbeddedResourceFileAttribute("MyNamespace.WebForm1.aspx", "MyNamespace")]
    /// [assembly: EmbeddedResourceFileAttribute("MyNamespace.UserControl1.ascx", "MyNamespace")]
    /// </code>
    /// <para>
    /// Third, you need to register this provider at application startup so ASP.NET
    /// knows to use it.  In your <c>Global.asax</c>, during <c>Application_OnStart</c>,
    /// put the following:
    /// </para>
    /// <code lang="C#">
    /// System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourcePathProvider());
    /// </code>
    /// <para>
    /// Fourth, in your <c>web.config</c> file, you need to set up a configuration
    /// section called <c>embeddedFileAssemblies</c> that lets the provider know
    /// which assemblies should be queried for embedded files.  A sample configuration
    /// section looks like this:
    /// </para>
    /// <code>
    /// &lt;configuration&gt;
    ///   &lt;configSections&gt;
    ///     &lt;section name="embeddedFileAssemblies" type="GSF.Configuration.StringCollectionSectionHandler, GSF.Web.Hosting.EmbeddedResourcePathProvider"/&gt;
    ///   &lt;/configSections&gt;
    ///   &lt;embeddedFileAssemblies&gt;
    ///     &lt;add value="My.Web.Assembly"/&gt;
    ///   &lt;/embeddedFileAssemblies&gt;
    ///   &lt;!-- ... other web.config items ... --&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// <para>
    /// Once you have that set up, you're ready to serve files from embedded resources.
    /// Simply deploy your application without putting the embedded resource files
    /// into the filesystem.  When you visit the embedded locations, the provider
    /// will automatically retrieve the proper embedded resource.
    /// </para>
    /// <para>
    /// File paths are mapped into the application using the
    /// <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute"/>
    /// declarations and the <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider.MapResourceToWebApplication"/>
    /// method.  This allows you to set up your web application as normal in
    /// Visual Studio and the folder structure, which automatically generates
    /// namespaces for your embedded resources, will translate into virtual folders
    /// in the embedded resource "filesystem."
    /// </para>
    /// <para>
    /// By default, files that are embedded as resources will take precedence over
    /// files in the filesystem.  If you would like the files in the filesystem
    /// to take precedence (that is, if you would like to allow the filesystem
    /// to "override" embedded files), you can set a key in the <c>appSettings</c>
    /// section of your <c>web.config</c> file that enables overrides:
    /// </para>
    /// <code>
    /// &lt;configuration&gt;
    ///   &lt;!-- ... other web.config items ... --&gt;
    ///   &lt;appSettings&gt;
    ///     &lt;add key="GSF.Web.Hosting.EmbeddedResourcePathProvider.AllowOverrides" value="true"/&gt;
    ///   &lt;/appSettings&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// <para>
    /// For more information on virtual filesystems in ASP.NET, check out
    /// <see cref="System.Web.Hosting.VirtualPathProvider"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute" />
    /// <seealso cref="System.Web.Hosting.VirtualPathProvider" />
    public class EmbeddedResourcePathProvider : VirtualPathProvider
    {

        #region EmbeddedResourcePathProvider Variables

        #region Constants

        /// <summary>
        /// The standard web application "app relative root" path.
        /// </summary>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        private const string ApplicationRootPath = "~/";

        /// <summary>
        /// The name of the configuration section containing the list of assemblies
        /// that should participate in the virtual filesystem.
        /// </summary>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        public const string ConfigSectionName = "embeddedFileAssemblies";

        /// <summary>
        /// Appsettings key that indicates if filesystem files should override
        /// embedded files.
        /// </summary>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        public const string ConfigKeyAllowOverrides = "GSF.Web.Hosting.EmbeddedResourcePathProvider.AllowOverrides";

        #endregion

        #region Instance

        /// <summary>
        /// Internal storage for the
        /// <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider.Files" />
        /// property.
        /// </summary>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        private readonly VirtualFileBaseCollection _files = new VirtualFileBaseCollection();

        #endregion

        #endregion



        #region EmbeddedResourcePathProvider Properties

        /// <summary>
        /// Gets a value indicating if embedded files can be overridden.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if embedded files can be overridden by other
        /// files (e.g., if filesystem files override embedded files); <see langword="false" />
        /// if not.  Defaults to <see langword="false" />.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property uses the <see cref="System.Configuration.ConfigurationManager.AppSettings"/>
        /// key <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider.ConfigKeyAllowOverrides"/>
        /// to determine if overrides are allowed.  If the key is present, it is
        /// parsed to a <see cref="System.Boolean"/> and that value is returned.
        /// If the key is not present or if any error occurs during the parsing,
        /// <see langword="false" /> is returned.
        /// </para>
        /// </remarks>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        public virtual bool AllowOverrides
        {
            get
            {
                string toParse = ConfigurationManager.AppSettings[ConfigKeyAllowOverrides];
                if (String.IsNullOrEmpty(toParse))
                {
                    return false;
                }
                bool retVal;
                bool.TryParse(toParse, out retVal);
                return retVal;
            }
        }

        /// <summary>
        /// Gets the collection of files served by this provider.
        /// </summary>
        /// <value>
        /// A <see cref="GSF.Web.Hosting.VirtualFileBaseCollection"/>
        /// that contains all of the files served by this provider.
        /// </value>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        public virtual VirtualFileBaseCollection Files
        {
            get
            {
                return _files;
            }
        }

        #endregion



        #region EmbeddedResourcePathProvider Implementation

        #region Constructors

        #endregion

        #region Overrides

        /// <summary>
        /// Gets a value that indicates whether a file exists in the virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns><see langword="true" /> if the file exists in the virtual file system; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        /// <para>
        /// This override checks to see if the embedded resource file exists
        /// in memory.  If so, this method will return <see langword="true" />.
        /// If not, it returns the value from the <see cref="System.Web.Hosting.VirtualPathProvider.Previous"/>
        /// virtual path provider.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="virtualPath" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="virtualPath" /> is <see cref="System.String.Empty" />.
        /// </exception>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        /// <seealso cref="System.Web.Hosting.VirtualPathProvider.FileExists" />
        public override bool FileExists(string virtualPath)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException(nameof(virtualPath));
            }
            if (virtualPath.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(virtualPath));
            }
            string absolutePath = VirtualPathUtility.ToAbsolute(virtualPath);
            if (this.Files.Contains(absolutePath))
            {
                return true;
            }
            return base.FileExists(absolutePath);
        }

        /// <summary>
        /// Creates a cache dependency based on the specified virtual paths.
        /// </summary>
        /// <param name="virtualPath">The path to the primary virtual resource.</param>
        /// <param name="virtualPathDependencies">An array of paths to other resources required by the primary virtual resource.</param>
        /// <param name="utcStart">The UTC time at which the virtual resources were read.</param>
        /// <returns>A <see cref="System.Web.Caching.CacheDependency"/> object for the specified virtual resources.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="virtualPath" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="virtualPath" /> is <see cref="System.String.Empty" />.
        /// </exception>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        /// <seealso cref="System.Web.Hosting.VirtualPathProvider.GetCacheDependency" />
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (virtualPath == null)
            {
                throw new ArgumentNullException(nameof(virtualPath));
            }
            if (virtualPath.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(virtualPath));
            }
            string absolutePath = VirtualPathUtility.ToAbsolute(virtualPath);

            // Lazy initialize the return value so we can return null if needed
            AggregateCacheDependency retVal = null;

            // Handle chained dependencies
            if (virtualPathDependencies != null)
            {
                foreach (string virtualPathDependency in virtualPathDependencies)
                {
                    CacheDependency dependencyToAdd = this.GetCacheDependency(virtualPathDependency, null, utcStart);
                    if (dependencyToAdd == null)
                    {
                        // Ignore items that have no dependency
                        continue;
                    }

                    if (retVal == null)
                    {
                        retVal = new AggregateCacheDependency();
                    }
                    retVal.Add(dependencyToAdd);
                }
            }

            // Handle the primary file
            CacheDependency primaryDependency;
            if (this.FileHandledByBaseProvider(absolutePath))
            {
                primaryDependency = base.GetCacheDependency(absolutePath, null, utcStart);
            }
            else
            {
                primaryDependency = new CacheDependency(((EmbeddedResourceVirtualFile)this.Files[absolutePath]).ContainingAssembly.Location, utcStart);
            }

            if (primaryDependency != null)
            {
                if (retVal == null)
                {
                    retVal = new AggregateCacheDependency();
                }
                retVal.Add(primaryDependency);
            }

            return retVal;
        }

        /// <summary>
        /// Gets a virtual file from the virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>A descendent of the <see cref="System.Web.Hosting.VirtualFile"/> class that represents a file in the virtual file system.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="virtualPath" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="virtualPath" /> is <see cref="System.String.Empty" />.
        /// </exception>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        /// <seealso cref="System.Web.Hosting.VirtualPathProvider.GetFile" />
        public override VirtualFile GetFile(string virtualPath)
        {
            // virtualPath comes in absolute form: /MyApplication/Subfolder/OtherFolder/Control.ascx
            // * ToAppRelative: ~/Subfolder/OtherFolder/Control.ascx
            // * ToAbsolute: /MyApplication/Subfolder/OtherFolder/Control.ascx
            if (virtualPath == null)
            {
                throw new ArgumentNullException(nameof(virtualPath));
            }
            if (virtualPath.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(virtualPath));
            }

            string absolutePath = VirtualPathUtility.ToAbsolute(virtualPath);

            if (this.FileHandledByBaseProvider(absolutePath))
                return base.GetFile(absolutePath);

            return (VirtualFile)this.Files[absolutePath];
        }

        /// <summary>
        /// Initializes the <see cref="System.Web.Hosting.VirtualPathProvider"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The set of assemblies configured to provide embedded resource files
        /// are queried for <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute"/>
        /// attributes.  For each one present, the associated embedded resource
        /// is added to the virtual file system served by this provider.
        /// </para>
        /// </remarks>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        /// <seealso cref="System.Web.Hosting.VirtualPathProvider.Initialize()" />
        protected override void Initialize()
        {
            string[] assemblies = FilePath.GetFileList(FilePath.GetAbsolutePath(string.Format("bin{0}*.dll", Path.DirectorySeparatorChar)));

            foreach (string assembly in assemblies)
            {
                try
                {
                    this.ProcessEmbeddedFiles(assembly);
                }
                // Thrown when there's a non-.NET dll in bin.  Since that's a valid case, we ignore it.
                catch (BadImageFormatException) { }
            }

            base.Initialize();
        }

        #endregion

        #region [ Methods ]

        #region [ Static ]

        /// <summary>
        /// Gets the names of the configured assemblies from configuration.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Collections.Specialized.StringCollection"/> with
        /// the names of the configured assemblies that should participate in this
        /// path provider.
        /// </returns>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        protected static string[] GetConfiguredAssemblyNames()
        {
            CategorizedSettingsElement setting = ConfigurationFile.Current.Settings["EmbeddedResourceProvider"]["EmbeddedResourceAssemblies"];

            if ((object)setting == null)
                return new string[] { };

            return setting.Value.Split(',');
        }

        /// <summary>
        /// Maps an embedded resource ID into a web application relative path (~/path).
        /// </summary>
        /// <param name="baseNamespace">
        /// The base namespace of the resource to map.
        /// </param>
        /// <param name="resourcePath">
        /// The fully qualified embedded resource path to map.
        /// </param>
        /// <returns>The mapped path of the resource into the web application.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="baseNamespace" /> is stripped from the front of the
        /// <paramref name="resourcePath" /> and all but the last period in the remaining
        /// <paramref name="resourcePath" /> is replaced with the directory separator character
        /// ('/').  Finally, that path is mapped into a web application relative path.
        /// </para>
        /// <para>
        /// The filename being mapped must have an extension associated with it, and that
        /// extension may not have a period in it.  Only one period will be kept in the
        /// mapped filename - others will be assumed to be directory separators.  If a filename
        /// has multiple extensions (i.e., <c>My.Custom.config</c>), it will not map properly -
        /// it will end up being <c>~/My/Custom.config</c>.
        /// </para>
        /// <para>
        /// If <paramref name="baseNamespace" /> does not occur at the start of the
        /// <paramref name="resourcePath" />, an <see cref="System.InvalidOperationException"/>
        /// is thrown.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// Given a <paramref name="baseNamespace" /> of <c>MyNamespace</c>,
        /// this method will process <paramref name="resourcePath" /> as follows:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term><paramref name="resourcePath" /> value</term>
        /// <description>Mapping in Web App</description>
        /// </listheader>
        /// <item>
        /// <term><c>MyNamespace.Config.MyFile.config</c></term>
        /// <description><c>~/Config/MyFile.config</c></description>
        /// </item>
        /// <item>
        /// <term><c>MyNamespace.MyPage.aspx</c></term>
        /// <description><c>~/MyPage.aspx</c></description>
        /// </item>
        /// </list>
        /// </example>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="baseNamespace" /> or <paramref name="resourcePath" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="baseNamespace" /> or <paramref name="resourcePath" />:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Is <see cref="System.String.Empty"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Start or end with period.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Contain two or more periods together (like <c>MyNamespace..MySubnamespace</c>).
        /// </description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        protected static string MapResourceToWebApplication(string baseNamespace, string resourcePath)
        {
            // Validate parameters
            ValidateResourcePath("baseNamespace", baseNamespace);
            ValidateResourcePath("resourcePath", resourcePath);

            // Ensure that the base namespace (with the period delimiter) appear in the resource path
            if (resourcePath.IndexOf(baseNamespace + ".") != 0)
            {
                throw new InvalidOperationException("Base resource namespace must appear at the start of the embedded resource path.");
            }

            // Remove the base namespace from the resource path
            string newResourcePath = resourcePath.Remove(0, baseNamespace.Length + 1);

            // Find the last period - that's the file extension
            int extSeparator = newResourcePath.LastIndexOf(".", StringComparison.Ordinal);

            // Replace all but the last period with a directory separator
            string resourceFilePath = newResourcePath.Substring(0, extSeparator).Replace(".", "/") + newResourcePath.Substring(extSeparator, newResourcePath.Length - extSeparator);

            // Map the path into the web app and return
            string retVal = VirtualPathUtility.Combine("~/", resourceFilePath);
            return retVal;
        }

        /// <summary>
        /// Validates an embedded resource path or namespace.
        /// </summary>
        /// <param name="paramName">The name of the parameter being validated.</param>
        /// <param name="path">The path/namespace to validate.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="path" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="path" />:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Is <see cref="System.String.Empty"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Start or end with period.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Contain two or more periods together (like <c>MyNamespace..MySubnamespace</c>).
        /// </description>
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        private static void ValidateResourcePath(string paramName, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }
            if (path.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(paramName));
            }
            if (path.StartsWith(".") || path.EndsWith("."))
            {
                throw new ArgumentOutOfRangeException(paramName, path, paramName + " may not start or end with a period.");
            }
            if (path.IndexOf("..") >= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, path, paramName + " may not contain two or more periods together.");
            }
        }

        #endregion

        #region [ Instance ]

        /// <summary>
        /// Determines if a file should be handled by the base provider or if
        /// it should be handled by this provider.
        /// </summary>
        /// <param name="absolutePath">The absolute path to the file to check.</param>
        /// <returns>
        /// <see langword="true" /> if processing of the file at
        /// <paramref name="absolutePath" /> should be done by the base provider;
        /// <see langword="false" /> if this provider should handle it.
        /// </returns>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        private bool FileHandledByBaseProvider(string absolutePath)
        {
            return (this.AllowOverrides && base.FileExists(absolutePath)) ||
                            !this.Files.Contains(absolutePath);
        }

        /// <summary>
        /// Reads in the embedded files from an assembly an processes them into
        /// the virtual file system.
        /// </summary>
        /// <param name="assemblyName">The name of the <see cref="System.Reflection.Assembly"/> to load and process.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="assemblyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="assemblyName" /> is <see cref="System.String.Empty" />.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the <see cref="System.Reflection.Assembly"/> indicated by
        /// <paramref name="assemblyName" /> is not found.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="assemblyName" /> will be passed to <see cref="System.Reflection.Assembly.Load(string)"/>
        /// so the associated assembly can be processed.  If the assembly is not
        /// found, a <see cref="System.IO.FileNotFoundException"/> is thrown.
        /// </para>
        /// <para>
        /// Once the assembly is retrieved, it is queried for <see cref="GSF.Web.Hosting.EmbeddedResourceFileAttribute"/>
        /// instances.  For each one found, the associated resources are processed
        /// into virtual files that will be stored in
        /// <see cref="GSF.Web.Hosting.EmbeddedResourcePathProvider.Files"/>
        /// for later use.
        /// </para>
        /// </remarks>
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider" />
        /// <seealso cref="GSF.Web.Hosting.EmbeddedResourcePathProvider.Initialize" />
        protected virtual void ProcessEmbeddedFiles(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                throw new ArgumentNullException(nameof(assemblyName));

            Assembly assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(assemblyName));

            // Get the embedded files specified in the assembly; bail early if there aren't any.
            EmbeddedResourceFileAttribute[] attribs = (EmbeddedResourceFileAttribute[])assembly.GetCustomAttributes(typeof(EmbeddedResourceFileAttribute), true);
            if (attribs.Length == 0)
            {
                return;
            }

            // Get the complete set of embedded resource names in the assembly; bail early if there aren't any.
            List<String> assemblyResourceNames = new List<string>(assembly.GetManifestResourceNames());
            if (assemblyResourceNames.Count == 0)
            {
                return;
            }

            foreach (EmbeddedResourceFileAttribute attrib in attribs)
            {
                // Ensure the resource specified actually exists in the assembly
                if (!assemblyResourceNames.Contains(attrib.ResourcePath))
                {
                    continue;
                }

                // Map the path into the web application
                string mappedPath;
                try
                {
                    mappedPath = VirtualPathUtility.ToAbsolute(MapResourceToWebApplication(attrib.ResourceNamespace, attrib.ResourcePath));
                }
                catch (ArgumentNullException)
                {
                    continue;
                }
                catch (ArgumentOutOfRangeException)
                {
                    continue;
                }

                // Create the file and ensure it's unique
                EmbeddedResourceVirtualFile file = new EmbeddedResourceVirtualFile(mappedPath, assembly, attrib.ResourcePath);
                if (this.Files.Contains(file.VirtualPath))
                {
                    continue;
                }

                // The file is unique; add it to the filesystem
                this.Files.Add(file);
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
