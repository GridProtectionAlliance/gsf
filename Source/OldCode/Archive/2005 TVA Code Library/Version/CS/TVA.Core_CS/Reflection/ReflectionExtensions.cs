//*******************************************************************************************************
//  TVA.Reflection.Extensions.vb - Defines extension functions related to Assemblies
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/12/2008 - J. Ritchie Carroll
//      Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Reflection;
using System.Collections.Specialized;

namespace TVA.Reflection
{
    public static class ReflectionExtensions
    {
        /// <summary>Returns only assembly name and version from full assembly name.</summary>
        public static string ShortName(this Assembly assemblyInstance)
        {
            return assemblyInstance.FullName.Split(',')[0];
        }

        /// <summary>Gets the specified embedded resource from the assembly.</summary>
        /// <param name="resourceName">The full name (including the namespace) of the embedded resource to get.</param>
        /// <returns>The embedded resource.</returns>
        public static Stream GetEmbeddedResource(this Assembly assemblyInstance, string resourceName)
        {
            //Extracts and returns the requested embedded resource.
            return assemblyInstance.GetManifestResourceStream(resourceName);
        }

        /// <summary>Gets the title information of the assembly.</summary>
        /// <returns>The title information of the assembly.</returns>
        public static string Title(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Title;
        }

        /// <summary>Gets the description information of the assembly.</summary>
        /// <returns>The description information of the assembly.</returns>
        public static string Description(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Description;
        }

        /// <summary>Gets the company name information of the assembly.</summary>
        /// <returns>The company name information of the assembly.</returns>
        public static string Company(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Company;
        }

        /// <summary>Gets the product name information of the assembly.</summary>
        /// <returns>The product name information of the assembly.</returns>
        public static string Product(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Product;
        }

        /// <summary>Gets the copyright information of the assembly.</summary>
        /// <returns>The copyright information of the assembly.</returns>
        public static string Copyright(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Copyright;
        }

        /// <summary>Gets the trademark information of the assembly.</summary>
        /// <returns>The trademark information of the assembly.</returns>
        public static string Trademark(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Trademark;
        }

        /// <summary>Gets the configuration information of the assembly.</summary>
        /// <returns>The configuration information of the assembly.</returns>
        public static string Configuration(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Configuration;
        }

        /// <summary>Gets a boolean value indicating if the assembly has been built as delay-signed.</summary>
        /// <returns>True, if the assembly has been built as delay-signed; otherwise, False.</returns>
        public static bool DelaySign(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).DelaySign;
        }

        /// <summary>Gets the version information of the assembly.</summary>
        /// <returns>The version information of the assembly</returns>
        public static string InformationalVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).InformationalVersion;
        }

        /// <summary>Gets the name of the file containing the key pair used to generate a strong name for the attributed
        /// assembly.</summary>
        /// <returns>A string containing the name of the file that contains the key pair.</returns>
        public static string KeyFile(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).KeyFile;
        }

        /// <summary>Gets the culture name of the assembly.</summary>
        /// <returns>The culture name of the assembly.</returns>
        public static string CultureName(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).CultureName;
        }

        /// <summary>Gets the assembly version used to instruct the System.Resources.ResourceManager to ask for a particular
        /// version of a satellite assembly to simplify updates of the main assembly of an application.</summary>
        public static string SatelliteContractVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).SatelliteContractVersion;
        }

        /// <summary>Gets the string representing the assembly version used to indicate to a COM client that all classes
        /// in the current version of the assembly are compatible with classes in an earlier version of the assembly.</summary>
        /// <returns>The string representing the assembly version in MajorVersion.MinorVersion.RevisionNumber.BuildNumber
        /// format.</returns>
        public static string ComCompatibleVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).ComCompatibleVersion;
        }

        /// <summary>Gets a boolean value indicating if the assembly is exposed to COM.</summary>
        /// <returns>True, if the assembly is exposed to COM; otherwise, False.</returns>
        public static bool ComVisible(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).ComVisible;
        }

        /// <summary>Gets the assembly GUID that is used as an ID if the assembly is exposed to COM.</summary>
        /// <returns>The assembly GUID that is used as an ID if the assembly is exposed to COM.</returns>
        public static string Guid(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Guid;
        }

        /// <summary>Gets the string representing the assembly version number in MajorVersion.MinorVersion format.</summary>
        /// <returns>The string representing the assembly version number in MajorVersion.MinorVersion format.</returns>
        public static string TypeLibVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).TypeLibVersion;
        }

        /// <summary>Gets a boolean value indicating whether the indicated program element is CLS-compliant.</summary>
        /// <returns>True, if the program element is CLS-compliant; otherwise, False.</returns>
        public static bool CLSCompliant(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).CLSCompliant;
        }

        /// <summary>Gets a value that indicates whether the runtime will track information during code generation for the
        /// debugger.</summary>
        /// <returns>True, if the runtime will track information during code generation for the debugger; otherwise, False.</returns>
        public static bool Debuggable(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).Debuggable;
        }

        /// <summary>Gets the date and time when the assembly was last built.</summary>
        /// <returns>The date and time when the assembly was last built.</returns>
        public static DateTime BuildDate(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).BuildDate;
        }

        /// <summary>Gets the root namespace of the assembly.</summary>
        /// <returns>The root namespace of the assembly.</returns>
        public static string RootNamespace(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).RootNamespace;
        }

        /// <summary>Gets a name/value collection of assembly attributes exposed by the assembly.</summary>
        /// <returns>A NameValueCollection of assembly attributes.</returns>
        public static NameValueCollection GetAttributes(this Assembly assemblyInstance)
        {
            return (new AssemblyInformation(assemblyInstance)).GetAttributes();
        }
    }
}
