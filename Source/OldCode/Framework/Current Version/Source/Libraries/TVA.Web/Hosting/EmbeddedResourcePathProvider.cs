//*******************************************************************************************************
//  EmbeddedResourcePathProvider.cs - Gbtc
//
//  Tennessee Valley Authority, 2010
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/04/2010 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;
using TVA.Configuration;
using TVA.IO;

namespace TVA.Web.Hosting
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
	/// <see cref="TVA.Web.Hosting.EmbeddedResourceFileAttribute"/> for
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
	///     &lt;section name="embeddedFileAssemblies" type="TVA.Configuration.StringCollectionSectionHandler, TVA.Web.Hosting.EmbeddedResourcePathProvider"/&gt;
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
	/// <see cref="TVA.Web.Hosting.EmbeddedResourceFileAttribute"/>
	/// declarations and the <see cref="TVA.Web.Hosting.EmbeddedResourcePathProvider.MapResourceToWebApplication"/>
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
	///     &lt;add key="TVA.Web.Hosting.EmbeddedResourcePathProvider.AllowOverrides" value="true"/&gt;
	///   &lt;/appSettings&gt;
	/// &lt;/configuration&gt;
	/// </code>
	/// <para>
	/// For more information on virtual filesystems in ASP.NET, check out
	/// <see cref="System.Web.Hosting.VirtualPathProvider"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="TVA.Web.Hosting.EmbeddedResourceFileAttribute" />
	/// <seealso cref="System.Web.Hosting.VirtualPathProvider" />
	public class EmbeddedResourcePathProvider : VirtualPathProvider
	{

		#region EmbeddedResourcePathProvider Variables

		#region Constants

		/// <summary>
		/// The standard web application "app relative root" path.
		/// </summary>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		private const string ApplicationRootPath = "~/";

		/// <summary>
		/// The name of the configuration section containing the list of assemblies
		/// that should participate in the virtual filesystem.
		/// </summary>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		public const string ConfigSectionName = "embeddedFileAssemblies";

		/// <summary>
		/// Appsettings key that indicates if filesystem files should override
		/// embedded files.
		/// </summary>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		public const string ConfigKeyAllowOverrides = "TVA.Web.Hosting.EmbeddedResourcePathProvider.AllowOverrides";

		#endregion

		#region Instance

		/// <summary>
		/// Internal storage for the
		/// <see cref="TVA.Web.Hosting.EmbeddedResourcePathProvider.Files" />
		/// property.
		/// </summary>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		private VirtualFileBaseCollection _files = new VirtualFileBaseCollection();

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
		/// key <see cref="TVA.Web.Hosting.EmbeddedResourcePathProvider.ConfigKeyAllowOverrides"/>
		/// to determine if overrides are allowed.  If the key is present, it is
		/// parsed to a <see cref="System.Boolean"/> and that value is returned.
		/// If the key is not present or if any error occurs during the parsing,
		/// <see langword="false" /> is returned.
		/// </para>
		/// </remarks>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		public virtual bool AllowOverrides
		{
			get
			{
				string toParse = System.Configuration.ConfigurationManager.AppSettings[ConfigKeyAllowOverrides];
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
		/// A <see cref="TVA.Web.Hosting.VirtualFileBaseCollection"/>
		/// that contains all of the files served by this provider.
		/// </value>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
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

		/// <summary>
		/// Initializes a new instance of the <see cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" /> class.
		/// </summary>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		public EmbeddedResourcePathProvider() : base() { }

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
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		/// <seealso cref="System.Web.Hosting.VirtualPathProvider.FileExists" />
		public override bool FileExists(string virtualPath)
		{
			if (virtualPath == null)
			{
				throw new ArgumentNullException("virtualPath");
			}
			if (virtualPath.Length == 0)
			{
				throw new ArgumentOutOfRangeException("virtualPath");
			}
			string absolutePath = System.Web.VirtualPathUtility.ToAbsolute(virtualPath);
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
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		/// <seealso cref="System.Web.Hosting.VirtualPathProvider.GetCacheDependency" />
		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			if (virtualPath == null)
			{
				throw new ArgumentNullException("virtualPath");
			}
			if (virtualPath.Length == 0)
			{
				throw new ArgumentOutOfRangeException("virtualPath");
			}
			string absolutePath = System.Web.VirtualPathUtility.ToAbsolute(virtualPath);

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
			CacheDependency primaryDependency = null;
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
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		/// <seealso cref="System.Web.Hosting.VirtualPathProvider.GetFile" />
		public override VirtualFile GetFile(string virtualPath)
		{
			// virtualPath comes in absolute form: /MyApplication/Subfolder/OtherFolder/Control.ascx
			// * ToAppRelative: ~/Subfolder/OtherFolder/Control.ascx
			// * ToAbsolute: /MyApplication/Subfolder/OtherFolder/Control.ascx
			if (virtualPath == null)
			{
				throw new ArgumentNullException("virtualPath");
			}
			if (virtualPath.Length == 0)
			{
				throw new ArgumentOutOfRangeException("virtualPath");
			}

			string absolutePath = System.Web.VirtualPathUtility.ToAbsolute(virtualPath);
			if (this.FileHandledByBaseProvider(absolutePath))
			{
				return base.GetFile(absolutePath);
			}
			else
			{
				return (VirtualFile)this.Files[absolutePath];
			}
		}

		/// <summary>
		/// Initializes the <see cref="System.Web.Hosting.VirtualPathProvider"/> instance.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The set of assemblies configured to provide embedded resource files
		/// are queried for <see cref="TVA.Web.Hosting.EmbeddedResourceFileAttribute"/>
		/// attributes.  For each one present, the associated embedded resource
		/// is added to the virtual filesystem served by this provider.
		/// </para>
		/// </remarks>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		/// <seealso cref="System.Web.Hosting.VirtualPathProvider.Initialize" />
		protected override void Initialize()
		{
			string[] assemblies = FilePath.GetFileList(FilePath.GetAbsolutePath("bin\\*.dll"));
            foreach (string assembly in assemblies)
            {
                this.ProcessEmbeddedFiles(assembly);
            }

			base.Initialize();
		}

		#endregion

		#region Methods

		#region Static

		/// <summary>
		/// Gets the names of the configured assemblies from configuration.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Collections.Specialized.StringCollection"/> with
		/// the names of the configured assemblies that should participate in this
		/// path provider.
		/// </returns>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		protected static string[] GetConfiguredAssemblyNames()
		{
            CategorizedSettingsElement setting = ConfigurationFile.Current.Settings["EmbeddedResourceProvider"]["EmbeddedResourceAssemblies"];
            if (setting == null)
                return new string[] { };
            else
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
		/// this method will process <paramref name="resoucePath" /> as follows:
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
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
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
			int extSeparator = newResourcePath.LastIndexOf(".");

			// Replace all but the last period with a directory separator
			string resourceFilePath = newResourcePath.Substring(0, extSeparator).Replace(".", "/") + newResourcePath.Substring(extSeparator, newResourcePath.Length - extSeparator);

			// Map the path into the web app and return
			string retVal = System.Web.VirtualPathUtility.Combine("~/", resourceFilePath);
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
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		private static void ValidateResourcePath(string paramName, string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("paramName");
			}
			if (path.Length == 0)
			{
				throw new ArgumentOutOfRangeException("paramName");
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

		#region Instance

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
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		private bool FileHandledByBaseProvider(string absolutePath)
		{
			return (this.AllowOverrides && base.FileExists(absolutePath)) ||
							!this.Files.Contains(absolutePath);
		}

		/// <summary>
		/// Reads in the embedded files from an assembly an processes them into
		/// the virtual filesystem.
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
		/// The <paramref name="assmeblyName" /> will be passed to <see cref="System.Reflection.Assembly.Load(string)"/>
		/// so the associated assembly can be processed.  If the assembly is not
		/// found, a <see cref="System.IO.FileNotFoundException"/> is thrown.
		/// </para>
		/// <para>
		/// Once the assembly is retrieved, it is queried for <see cref="TVA.Web.Hosting.EmbeddedResourceFileAttribute"/>
		/// instances.  For each one found, the associated resources are processed
		/// into virtual files that will be stored in
		/// <see cref="TVA.Web.Hosting.EmbeddedResourcePathProvider.Files"/>
		/// for later use.
		/// </para>
		/// </remarks>
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider" />
		/// <seealso cref="TVA.Web.Hosting.EmbeddedResourcePathProvider.Initialize" />
		protected virtual void ProcessEmbeddedFiles(string assemblyName)
		{
            if (string.IsNullOrEmpty(assemblyName))
                throw new ArgumentNullException("assemblyName");

			Assembly assembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(assemblyName));

			// Get the embedded files specified in the assembly; bail early if there aren't any.
			EmbeddedResourceFileAttribute[] attribs = (EmbeddedResourceFileAttribute[])assembly.GetCustomAttributes(typeof(EmbeddedResourceFileAttribute), true);
			if (attribs.Length == 0)
			{
				return;
			}

			// Get the complete set of embedded resource names in the assembly; bail early if there aren't any.
			System.Collections.Generic.List<String> assemblyResourceNames = new System.Collections.Generic.List<string>(assembly.GetManifestResourceNames());
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
					mappedPath = System.Web.VirtualPathUtility.ToAbsolute(MapResourceToWebApplication(attrib.ResourceNamespace, attrib.ResourcePath));
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
