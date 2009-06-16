//*******************************************************************************************************
//  AssemblyInfo.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  04/29/2005 - Pinal C. Patel
//      Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//      Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Assembly).
//  12/12/2007 - Darrell Zuercher
//      Edited Code Comments.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C# as AssemblyInformation.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using TVA.IO;

namespace TVA.Reflection
{
    /// <summary>Assembly Information Class.</summary>
    public class AssemblyInfo
    {
        #region [ Members ]

        // Fields
        private Assembly m_assemblyInstance;

        #endregion

        #region [ Constructors ]

        /// <summary>Initializes a new instance of the <see cref="AssemblyInfo"/> class.</summary>
        /// <param name="assemblyInstance">An <see cref="Assembly"/> object.</param>
        public AssemblyInfo(Assembly assemblyInstance)
        {
            m_assemblyInstance = assemblyInstance;
        }

        #endregion

        #region [ Properties ]

        /// <summary>Gets the title information of the assembly.</summary>
        /// <returns>The title information of the assembly.</returns>
        public string Title
        {
            get
            {
                return ((AssemblyTitleAttribute)(GetCustomAttribute(typeof(AssemblyTitleAttribute)))).Title;
            }
        }

        /// <summary>Gets the description information of the assembly.</summary>
        /// <returns>The description information of the assembly.</returns>
        public string Description
        {
            get
            {
                return ((AssemblyDescriptionAttribute)(GetCustomAttribute(typeof(AssemblyDescriptionAttribute)))).Description;
            }
        }

        /// <summary>Gets the company name information of the assembly.</summary>
        /// <returns>The company name information of the assembly.</returns>
        public string Company
        {
            get
            {
                return ((AssemblyCompanyAttribute)(GetCustomAttribute(typeof(AssemblyCompanyAttribute)))).Company;
            }
        }

        /// <summary>Gets the product name information of the assembly.</summary>
        /// <returns>The product name information of the assembly.</returns>
        public string Product
        {
            get
            {
                return ((AssemblyProductAttribute)(GetCustomAttribute(typeof(AssemblyProductAttribute)))).Product;
            }
        }

        /// <summary>Gets the copyright information of the assembly.</summary>
        /// <returns>The copyright information of the assembly.</returns>
        public string Copyright
        {
            get
            {
                return ((AssemblyCopyrightAttribute)(GetCustomAttribute(typeof(AssemblyCopyrightAttribute)))).Copyright;
            }
        }

        /// <summary>Gets the trademark information of the assembly.</summary>
        /// <returns>The trademark information of the assembly.</returns>
        public string Trademark
        {
            get
            {
                return ((AssemblyTrademarkAttribute)(GetCustomAttribute(typeof(AssemblyTrademarkAttribute)))).Trademark;
            }
        }

        /// <summary>Gets the configuration information of the assembly.</summary>
        /// <returns>The configuration information of the assembly.</returns>
        public string Configuration
        {
            get
            {
                return ((AssemblyConfigurationAttribute)(GetCustomAttribute(typeof(AssemblyConfigurationAttribute)))).Configuration;
            }
        }

        /// <summary>Gets a boolean value indicating if the assembly has been built as delay-signed.</summary>
        /// <returns>True, if the assembly has been built as delay-signed; otherwise, False.</returns>
        public bool DelaySign
        {
            get
            {
                return ((AssemblyDelaySignAttribute)(GetCustomAttribute(typeof(AssemblyDelaySignAttribute)))).DelaySign;
            }
        }

        /// <summary>Gets the version information of the assembly.</summary>
        /// <returns>The version information of the assembly</returns>
        public string InformationalVersion
        {
            get
            {
                return ((AssemblyInformationalVersionAttribute)(GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)))).InformationalVersion;
            }
        }

        /// <summary>Gets the name of the file containing the key pair used to generate a strong name for the attributed
        /// assembly.</summary>
        /// <returns>A string containing the name of the file that contains the key pair.</returns>
        public string KeyFile
        {
            get
            {
                return ((AssemblyKeyFileAttribute)(GetCustomAttribute(typeof(AssemblyKeyFileAttribute)))).KeyFile;
            }
        }

        /// <summary>Gets the culture name of the assembly.</summary>
        /// <returns>The culture name of the assembly.</returns>
        public string CultureName
        {
            get
            {
                return ((NeutralResourcesLanguageAttribute)(GetCustomAttribute(typeof(NeutralResourcesLanguageAttribute)))).CultureName;
            }
        }

        /// <summary>Gets the assembly version used to instruct the System.Resources.ResourceManager to ask for a particular
        /// version of a satellite assembly to simplify updates of the main assembly of an application.</summary>
        public string SatelliteContractVersion
        {
            get
            {
                return ((SatelliteContractVersionAttribute)(GetCustomAttribute(typeof(SatelliteContractVersionAttribute)))).Version;
            }
        }

        /// <summary>Gets the string representing the assembly version used to indicate to a COM client that all classes
        /// in the current version of the assembly are compatible with classes in an earlier version of the assembly.</summary>
        /// <returns>The string representing the assembly version in MajorVersion.MinorVersion.RevisionNumber.BuildNumber
        /// format.</returns>
        public string ComCompatibleVersion
        {
            get
            {
                ComCompatibleVersionAttribute comVersionAttribute = ((ComCompatibleVersionAttribute)(GetCustomAttribute(typeof(ComCompatibleVersionAttribute))));
                return comVersionAttribute.MajorVersion.ToString() + "." + comVersionAttribute.MinorVersion.ToString() + "." + comVersionAttribute.RevisionNumber.ToString() + "." + comVersionAttribute.BuildNumber.ToString();
            }
        }

        /// <summary>Gets a boolean value indicating if the assembly is exposed to COM.</summary>
        /// <returns>True, if the assembly is exposed to COM; otherwise, False.</returns>
        public bool ComVisible
        {
            get
            {
                return ((ComVisibleAttribute)(GetCustomAttribute(typeof(ComVisibleAttribute)))).Value;
            }
        }

        /// <summary>Gets the assembly GUID that is used as an ID if the assembly is exposed to COM.</summary>
        /// <returns>The assembly GUID that is used as an ID if the assembly is exposed to COM.</returns>
        public string Guid
        {
            get
            {
                return ((GuidAttribute)(GetCustomAttribute(typeof(GuidAttribute)))).Value;
            }
        }

        /// <summary>Gets the string representing the assembly version number in MajorVersion.MinorVersion format.</summary>
        /// <returns>The string representing the assembly version number in MajorVersion.MinorVersion format.</returns>
        public string TypeLibVersion
        {
            get
            {
                TypeLibVersionAttribute versionAttribute = ((TypeLibVersionAttribute)(GetCustomAttribute(typeof(TypeLibVersionAttribute))));
                return versionAttribute.MajorVersion.ToString() + "." + versionAttribute.MinorVersion.ToString();
            }
        }

        /// <summary>Gets a boolean value indicating whether the indicated program element is CLS-compliant.</summary>
        /// <returns>True, if the program element is CLS-compliant; otherwise, False.</returns>
        public bool CLSCompliant
        {
            get
            {
                return ((CLSCompliantAttribute)(GetCustomAttribute(typeof(CLSCompliantAttribute)))).IsCompliant;
            }
        }

        /// <summary>Gets a value that indicates whether the runtime will track information during code generation for the
        /// debugger.</summary>
        /// <returns>True, if the runtime will track information during code generation for the debugger; otherwise, False.</returns>
        public bool Debuggable
        {
            get
            {
                return ((DebuggableAttribute)(GetCustomAttribute(typeof(DebuggableAttribute)))).IsJITTrackingEnabled;
            }
        }

        /// <summary>Gets the path or UNC location of the loaded file that contains the manifest.</summary>
        /// <returns>The location of the loaded file that contains the manifest.</returns>
        public string Location
        {
            get
            {
                return m_assemblyInstance.Location.ToLower();
            }
        }

        /// <summary>Gets the location of the assembly as specified originally; for example, in a
        /// AssemblyName object.</summary>
        /// <returns>The location of the assembly as specified originally.</returns>
        public string CodeBase
        {
            get
            {
                return m_assemblyInstance.CodeBase.Replace("file:///", "").ToLower();
            }
        }

        /// <summary>Gets the display name of the assembly.</summary>
        /// <returns>The display name of the assembly.</returns>
        public string FullName
        {
            get
            {
                return m_assemblyInstance.FullName;
            }
        }

        /// <summary>Gets the simple, unencrypted name of the assembly.</summary>
        /// <returns>A string that is the simple, unencrypted name of the assembly.</returns>
        public string Name
        {
            get
            {
                return m_assemblyInstance.GetName().Name;
            }
        }

        /// <summary>Gets the major, minor, revision, and build numbers of the assembly.</summary>
        /// <returns>A System.Version object representing the major, minor, revision, and build numbers of the assembly.</returns>
        public Version Version
        {
            get
            {
                return m_assemblyInstance.GetName().Version;
            }
        }

        /// <summary>Gets the string representing the version of the common language runtime (CLR) saved in the file
        /// containing the manifest.</summary>
        /// <returns>The string representing the CLR version folder name. This is not a full path.</returns>
        public string ImageRuntimeVersion
        {
            get
            {
                return m_assemblyInstance.ImageRuntimeVersion;
            }
        }

        /// <summary>Gets a boolean value indicating whether the assembly was loaded from the global assembly cache.</summary>
        /// <returns>True, if the assembly was loaded from the global assembly cache; otherwise, False.</returns>
        public bool GACLoaded
        {
            get
            {
                return m_assemblyInstance.GlobalAssemblyCache;
            }
        }

        /// <summary>Gets the date and time when the assembly was last built.</summary>
        /// <returns>The date and time when the assembly was last built.</returns>
        public DateTime BuildDate
        {
            get
            {
                return File.GetLastWriteTime(m_assemblyInstance.Location);
            }
        }

        /// <summary>Gets the root namespace of the assembly.</summary>
        /// <returns>The root namespace of the assembly.</returns>
        public string RootNamespace
        {
            get
            {
                return m_assemblyInstance.GetExportedTypes()[0].Namespace;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>Gets a collection of assembly attributes exposed by the assembly.</summary>
        /// <returns>A System.Specialized.KeyValueCollection of assembly attributes.</returns>
        public NameValueCollection GetAttributes()
        {
            NameValueCollection assemblyAttributes = new NameValueCollection();

            //Add some values that are not in AssemblyInfo.
            assemblyAttributes.Add("Full Name", FullName);
            assemblyAttributes.Add("Name", Name);
            assemblyAttributes.Add("Version", Version.ToString());
            assemblyAttributes.Add("Image Runtime Version", ImageRuntimeVersion);
            assemblyAttributes.Add("Build Date", BuildDate.ToString());
            assemblyAttributes.Add("Location", Location);
            assemblyAttributes.Add("Code Base", CodeBase);
            assemblyAttributes.Add("GAC Loaded", GACLoaded.ToString());

            //Add all attributes available from AssemblyInfo.
            foreach (object assemblyAttribute in m_assemblyInstance.GetCustomAttributes(false))
            {
                if (assemblyAttribute is AssemblyTitleAttribute)
                {
                    assemblyAttributes.Add("Title", Title);
                }
                else if (assemblyAttribute is AssemblyDescriptionAttribute)
                {
                    assemblyAttributes.Add("Description", Description);
                }
                else if (assemblyAttribute is AssemblyCompanyAttribute)
                {
                    assemblyAttributes.Add("Company", Company);
                }
                else if (assemblyAttribute is AssemblyProductAttribute)
                {
                    assemblyAttributes.Add("Product", Product);
                }
                else if (assemblyAttribute is AssemblyCopyrightAttribute)
                {
                    assemblyAttributes.Add("Copyright", Copyright);
                }
                else if (assemblyAttribute is AssemblyTrademarkAttribute)
                {
                    assemblyAttributes.Add("Trademark", Trademark);
                }
                else if (assemblyAttribute is AssemblyConfigurationAttribute)
                {
                    assemblyAttributes.Add("Configuration", Configuration);
                }
                else if (assemblyAttribute is AssemblyDelaySignAttribute)
                {
                    assemblyAttributes.Add("Delay Sign", DelaySign.ToString());
                }
                else if (assemblyAttribute is AssemblyInformationalVersionAttribute)
                {
                    assemblyAttributes.Add("Informational Version", InformationalVersion);
                }
                else if (assemblyAttribute is AssemblyKeyFileAttribute)
                {
                    assemblyAttributes.Add("Key File", KeyFile);
                }
                else if (assemblyAttribute is NeutralResourcesLanguageAttribute)
                {
                    assemblyAttributes.Add("Culture Name", CultureName);
                }
                else if (assemblyAttribute is SatelliteContractVersionAttribute)
                {
                    assemblyAttributes.Add("Satellite Contract Version", SatelliteContractVersion);
                }
                else if (assemblyAttribute is ComCompatibleVersionAttribute)
                {
                    assemblyAttributes.Add("Com Compatible Version", ComCompatibleVersion);
                }
                else if (assemblyAttribute is ComVisibleAttribute)
                {
                    assemblyAttributes.Add("Com Visible", ComVisible.ToString());
                }
                else if (assemblyAttribute is GuidAttribute)
                {
                    assemblyAttributes.Add("Guid", Guid);
                }
                else if (assemblyAttribute is TypeLibVersionAttribute)
                {
                    assemblyAttributes.Add("Type Lib Version", TypeLibVersion);
                }
                else if (assemblyAttribute is CLSCompliantAttribute)
                {
                    assemblyAttributes.Add("CLS Compliant", CLSCompliant.ToString());
                }
                else if (assemblyAttribute is DebuggableAttribute)
                {
                    assemblyAttributes.Add("Debuggable", Debuggable.ToString());
                }
            }

            return assemblyAttributes;
        }

        /// <summary>Gets the specified assembly attribute if it is exposed by the assembly.</summary>
        /// <param name="attributeType">Type of the attribute to get.</param>
        /// <returns>The assembly attribute.</returns>
        public object GetCustomAttribute(Type attributeType)
        {
            //Returns the requested assembly attribute.
            object[] assemblyAttributes = m_assemblyInstance.GetCustomAttributes(attributeType, false);

            if (assemblyAttributes.Length >= 1)
                return assemblyAttributes[0];
            else
                throw new ApplicationException("Assembly does not expose this attribute");
        }

        /// <summary>Gets the specified embedded resource from the assembly.</summary>
        /// <param name="resourceName">The full name (including the namespace) of the embedded resource to get.</param>
        /// <returns>The embedded resource.</returns>
        public Stream GetEmbeddedResource(string resourceName)
        {
            //Extracts and returns the requested embedded resource.
            return m_assemblyInstance.GetEmbeddedResource(resourceName);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static AssemblyInfo m_callingAssembly;
        private static AssemblyInfo m_entryAssembly;
        private static AssemblyInfo m_executingAssembly;
        private static Dictionary<string, Assembly> m_assemblyCache;
        private static bool m_addedResolver;

        // Static Properties

        /// <summary>Gets the <see cref="AssemblyInfo"/> object of the assembly that invoked the currently executing method.</summary>
        public static AssemblyInfo CallingAssembly
        {
            get
            {
                if (m_callingAssembly == null)
                {
                    // We have to find the calling assembly of the caller.
                    StackTrace trace = new StackTrace();
                    Assembly caller = Assembly.GetCallingAssembly();
                    Assembly current = Assembly.GetExecutingAssembly();
                    foreach (StackFrame frame in trace.GetFrames())
                    {
                        Assembly assembly = Assembly.GetAssembly(frame.GetMethod().DeclaringType);
                        if (assembly != caller && assembly != current)
                        {
                            // Assembly is neither the current assembly or the calling assembly.
                            m_callingAssembly = new AssemblyInfo(assembly);
                            break;
                        }
                    }
                }

                return m_callingAssembly;
            }
        }

        /// <summary>Gets the <see cref="AssemblyInfo"/> object of the process executable in the default application domain.</summary>
        public static AssemblyInfo EntryAssembly
        {
            get
            {
                if (m_entryAssembly == null)
                    m_entryAssembly = new AssemblyInfo(Assembly.GetEntryAssembly());

                return m_entryAssembly;
            }
        }

        /// <summary>Gets the <see cref="AssemblyInfo"/> object of the assembly that contains the code that is currently executing.</summary>
        public static AssemblyInfo ExecutingAssembly
        {
            get
            {
                if (m_executingAssembly == null) 
                    // Caller's assembly will be the executing assembly for the caller.
                    m_executingAssembly = new AssemblyInfo(Assembly.GetCallingAssembly());

                return m_executingAssembly;
            }
        }

        // Static Methods

        /// <summary>Loads the specified assembly that is embedded as a resource in the assembly.</summary>
        /// <param name="assemblyName">Name of the assembly to load.</param>
        /// <remarks>This cannot be used to load TVA.Core itself.</remarks>
        public static void LoadAssemblyFromResource(string assemblyName)
        {
            // Hooks into assembly resolve event for current domain so it can load assembly from embedded resource.
            if (!m_addedResolver)
            {
                AppDomain.CurrentDomain.AssemblyResolve += new System.ResolveEventHandler(ResolveAssemblyFromResource);
                m_addedResolver = true;
            }

            // Loads the assembly (This will invoke event that will resolve assembly from resource.).
            AppDomain.CurrentDomain.Load(assemblyName);
        }

        private static Assembly ResolveAssemblyFromResource(object sender, ResolveEventArgs e)
        {
            Assembly resourceAssembly;
            string shortName = e.Name.Split(',')[0];

            if (m_assemblyCache == null) m_assemblyCache = new Dictionary<string, Assembly>();
            resourceAssembly = m_assemblyCache[shortName];

            if (resourceAssembly == null)
            {
                // Loops through all of the resources in the executing assembly.
                foreach (string name in Assembly.GetEntryAssembly().GetManifestResourceNames())
                {
                    // Sees if the embedded resource name matches the assembly it is trying to load.
                    if (string.Compare(FilePath.GetFileNameWithoutExtension(name), EntryAssembly.RootNamespace + "." + shortName, true) == 0)
                    {
                        // If so, loads embedded resource assembly into a binary buffer.
                        System.IO.Stream resourceStream = Assembly.GetEntryAssembly().GetManifestResourceStream(name);
                        byte[] buffer = new byte[resourceStream.Length];
                        resourceStream.Read(buffer, 0, (int)resourceStream.Length);
                        resourceStream.Close();

                        // Loads assembly from binary buffer.
                        resourceAssembly = Assembly.Load(buffer);
                        m_assemblyCache.Add(shortName, resourceAssembly);
                        break;
                    }
                }
            }

            return resourceAssembly;
        }

        #endregion
    }
}