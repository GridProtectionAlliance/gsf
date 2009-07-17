//*******************************************************************************************************
//  AssemblyExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/12/2008 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;

namespace TVA.Reflection
{
    /// <summary>Defines extension functions related to Assemblies.</summary>
    public static class AssemblyExtensions
    {
        /// <summary>Returns only assembly name and version from full assembly name.</summary>
        public static string ShortName(this Assembly assemblyInstance)
        {
            return assemblyInstance.FullName.Split(',')[0];
        }

        /// <summary>Gets the specified embedded resource from the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <param name="resourceName">The full name (including the namespace) of the embedded resource to get.</param>
        /// <returns>The embedded resource.</returns>
        public static Stream GetEmbeddedResource(this Assembly assemblyInstance, string resourceName)
        {
            //Extracts and returns the requested embedded resource.
            return assemblyInstance.GetManifestResourceStream(resourceName);
        }

        /// <summary>Gets the title information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The title information of the assembly.</returns>
        public static string Title(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Title;
        }

        /// <summary>Gets the description information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The description information of the assembly.</returns>
        public static string Description(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Description;
        }

        /// <summary>Gets the company name information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The company name information of the assembly.</returns>
        public static string Company(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Company;
        }

        /// <summary>Gets the product name information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The product name information of the assembly.</returns>
        public static string Product(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Product;
        }

        /// <summary>Gets the copyright information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The copyright information of the assembly.</returns>
        public static string Copyright(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Copyright;
        }

        /// <summary>Gets the trademark information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The trademark information of the assembly.</returns>
        public static string Trademark(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Trademark;
        }

        /// <summary>Gets the configuration information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The configuration information of the assembly.</returns>
        public static string Configuration(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Configuration;
        }

        /// <summary>Gets a boolean value indicating if the assembly has been built as delay-signed.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>True, if the assembly has been built as delay-signed; otherwise, False.</returns>
        public static bool DelaySign(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).DelaySign;
        }

        /// <summary>Gets the version information of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The version information of the assembly</returns>
        public static string InformationalVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).InformationalVersion;
        }

        /// <summary>Gets the name of the file containing the key pair used to generate a strong name for the attributed assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>A string containing the name of the file that contains the key pair.</returns>
        public static string KeyFile(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).KeyFile;
        }

        /// <summary>Gets the culture name of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The culture name of the assembly.</returns>
        public static string CultureName(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).CultureName;
        }

        /// <summary>Gets the assembly version used to instruct the System.Resources.ResourceManager to ask for a particular
        /// version of a satellite assembly to simplify updates of the main assembly of an application.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The satellite contract version of the assembly.</returns>
        public static string SatelliteContractVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).SatelliteContractVersion;
        }

        /// <summary>Gets the string representing the assembly version used to indicate to a COM client that all classes
        /// in the current version of the assembly are compatible with classes in an earlier version of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The string representing the assembly version in MajorVersion.MinorVersion.RevisionNumber.BuildNumber
        /// format.</returns>
        public static string ComCompatibleVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).ComCompatibleVersion;
        }

        /// <summary>Gets a boolean value indicating if the assembly is exposed to COM.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>True, if the assembly is exposed to COM; otherwise, False.</returns>
        public static bool ComVisible(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).ComVisible;
        }

        /// <summary>Gets the assembly GUID that is used as an ID if the assembly is exposed to COM.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The assembly GUID that is used as an ID if the assembly is exposed to COM.</returns>
        public static string Guid(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Guid;
        }

        /// <summary>Gets the string representing the assembly version number in MajorVersion.MinorVersion format.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The string representing the assembly version number in MajorVersion.MinorVersion format.</returns>
        public static string TypeLibVersion(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).TypeLibVersion;
        }

        /// <summary>Gets a boolean value indicating whether the indicated program element is CLS-compliant.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>True, if the program element is CLS-compliant; otherwise, False.</returns>
        public static bool CLSCompliant(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).CLSCompliant;
        }

        /// <summary>Gets a value that indicates whether the runtime will track information during code generation for the
        /// debugger.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>True, if the runtime will track information during code generation for the debugger; otherwise, False.</returns>
        public static bool Debuggable(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).Debuggable;
        }

        /// <summary>Gets the date and time when the assembly was last built.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The date and time when the assembly was last built.</returns>
        public static DateTime BuildDate(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).BuildDate;
        }

        /// <summary>Gets the root namespace of the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The root namespace of the assembly.</returns>
        public static string RootNamespace(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).RootNamespace;
        }

        /// <summary>Gets a name/value collection of assembly attributes exposed by the assembly.</summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>A NameValueCollection of assembly attributes.</returns>
        public static NameValueCollection GetAttributes(this Assembly assemblyInstance)
        {
            return (new AssemblyInfo(assemblyInstance)).GetAttributes();
        }
    }
}
