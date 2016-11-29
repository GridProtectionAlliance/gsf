//******************************************************************************************************
//  AssemblyExtensions.cs - Gbtc
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
//  09/12/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/28/2010 - Pinal C. Patel
//       Removed Debuggable extension method since its value source is no longer present in AssemblyInfo.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using GSF.Diagnostics;

namespace GSF.Reflection
{
    /// <summary>
    /// Defines extension functions related to Assemblies.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Returns only assembly name and version from full assembly name.
        /// </summary>
        /// <param name="assemblyInstance">An <see cref="Assembly"/> to get the short name of.</param>
        /// <returns>The assembly name and version from the full assembly name.</returns>
        public static string ShortName(this Assembly assemblyInstance)
        {
            return assemblyInstance.FullName.Split(',')[0];
        }

        /// <summary>
        /// Gets the specified embedded resource from the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <param name="resourceName">The full name (including the namespace) of the embedded resource to get.</param>
        /// <returns>The embedded resource.</returns>
        public static Stream GetEmbeddedResource(this Assembly assemblyInstance, string resourceName)
        {
            //Extracts and returns the requested embedded resource.
            return assemblyInstance.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Gets the title information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The title information of the assembly.</returns>
        public static string Title(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Title;
        }

        /// <summary>
        /// Gets the description information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The description information of the assembly.</returns>
        public static string Description(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Description;
        }

        /// <summary>
        /// Gets the company name information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The company name information of the assembly.</returns>
        public static string Company(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Company;
        }

        /// <summary>
        /// Gets the product name information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The product name information of the assembly.</returns>
        public static string Product(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Product;
        }

        /// <summary>
        /// Gets the copyright information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The copyright information of the assembly.</returns>
        public static string Copyright(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Copyright;
        }

        /// <summary>
        /// Gets the trademark information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The trademark information of the assembly.</returns>
        public static string Trademark(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Trademark;
        }

        /// <summary>
        /// Gets the configuration information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The configuration information of the assembly.</returns>
        public static string Configuration(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Configuration;
        }

        /// <summary>
        /// Gets a boolean value indicating if the assembly has been built as delay-signed.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns><c>true</c>, if the assembly has been built as delay-signed; otherwise, <c>false</c>.</returns>
        public static bool DelaySign(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).DelaySign;
        }

        /// <summary>
        /// Gets the version information of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The version information of the assembly</returns>
        public static string InformationalVersion(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).InformationalVersion;
        }

        /// <summary>
        /// Gets the name of the file containing the key pair used to generate a strong name for the attributed assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>A string containing the name of the file that contains the key pair.</returns>
        public static string KeyFile(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).KeyFile;
        }

        /// <summary>
        /// Gets the culture name of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The culture name of the assembly.</returns>
        public static string CultureName(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).CultureName;
        }

        /// <summary>
        /// Gets the assembly version used to instruct the <see cref="System.Resources.ResourceManager"/> to ask for
        /// a particular version of a satellite assembly to simplify updates of the main assembly of an application.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The satellite contract version of the assembly.</returns>
        public static string SatelliteContractVersion(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).SatelliteContractVersion;
        }

        /// <summary>
        /// Gets the string representing the assembly version used to indicate to a COM client that all classes in the
        /// current version of the assembly are compatible with classes in an earlier version of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The string representing the assembly version in MajorVersion.MinorVersion.RevisionNumber.BuildNumber format.</returns>
        public static string ComCompatibleVersion(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).ComCompatibleVersion;
        }

        /// <summary>
        /// Gets a boolean value indicating if the assembly is exposed to COM.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns><c>true</c>, if the assembly is exposed to COM; otherwise, <c>false</c>.</returns>
        public static bool ComVisible(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).ComVisible;
        }

        /// <summary>
        /// Gets a boolean value indicating if the <see cref="Assembly"/> was built in debug mode.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns><c>true</c>, if the assembly was built in debug mode; otherwise, <c>false</c>.</returns>
        public static bool Debuggable(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).ComVisible;
        }

        /// <summary>
        /// Gets the assembly GUID that is used as an ID if the assembly is exposed to COM.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The assembly GUID that is used as an ID if the assembly is exposed to COM.</returns>
        public static string Guid(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).Guid;
        }

        /// <summary>
        /// Gets the string representing the assembly version number in MajorVersion.MinorVersion format.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The string representing the assembly version number in MajorVersion.MinorVersion format.</returns>
        public static string TypeLibVersion(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).TypeLibVersion;
        }

        /// <summary>
        /// Gets a boolean value indicating whether the indicated program element is CLS-compliant.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns><c>true</c>, if the program element is CLS-compliant; otherwise, <c>false</c>.</returns>
        public static bool CLSCompliant(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).CLSCompliant;
        }

        /// <summary>
        /// Gets the date and time when the assembly was last built.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The date and time when the assembly was last built.</returns>
        public static DateTime BuildDate(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).BuildDate;
        }

        /// <summary>
        /// Gets the root namespace of the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>The root namespace of the assembly.</returns>
        public static string RootNamespace(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).RootNamespace;
        }

        /// <summary>
        /// Gets a name/value collection of assembly attributes exposed by the assembly.
        /// </summary>
        /// <param name="assemblyInstance">Source assembly.</param>
        /// <returns>A NameValueCollection of assembly attributes.</returns>
        public static NameValueCollection GetAttributes(this Assembly assemblyInstance)
        {
            return new AssemblyInfo(assemblyInstance).GetAttributes();
        }

        /// <summary>
        /// Recursively attempts to load all assemblies referenced from the given assembly.
        /// </summary>
        /// <param name="assemblyInstance">The assembly whose references are to be loaded.</param>
        /// <returns><c>true</c> if the references were successfully loaded; <c>false</c> otherwise.</returns>
        /// <remarks>
        /// If an object is created from a type that is loaded from an assembly, and if that
        /// assembly's references fail to load during instantiation, an exception may be thrown
        /// from both the constructor and the finalizer of the object that was instantiated.
        /// This method allows us to ensure that all referenced assemblies can be loaded
        /// before attempting to instantiate a type from that assembly.
        /// </remarks>
        public static bool TryLoadAllReferences(this Assembly assemblyInstance)
        {
            return TryLoadAllReferences(assemblyInstance, new HashSet<string>(AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetName().ToString())));
        }

        // Recursively attempts to load all assemblies referenced from the given assembly.
        private static bool TryLoadAllReferences(Assembly assembly, ISet<string> validNames)
        {
            try
            {
                Assembly referencedAssembly;

                // Base case: all referenced assemblies' names are present in the set of valid names
                IEnumerable<AssemblyName> referencedAssemblyNames = assembly.GetReferencedAssemblies()
                    .Where(referencedAssemblyName => !validNames.Contains(referencedAssemblyName.ToString()));

                // Load each referenced assembly and recursively load their references as well
                foreach (AssemblyName referencedAssemblyName in referencedAssemblyNames)
                {
                    referencedAssembly = Assembly.Load(referencedAssemblyName);
                    validNames.Add(referencedAssemblyName.ToString());

                    if (!TryLoadAllReferences(referencedAssembly, validNames))
                        return false;
                }

                // All referenced assemblies loaded successfully
                return true;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex, "AssemblyExtensions.cs TryLoadAllReferences: Failed to load for some reason.");
                // Error loading a referenced assembly
                return false;
            }
        }
    }
}
