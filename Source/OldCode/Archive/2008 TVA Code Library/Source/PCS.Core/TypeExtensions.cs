//*******************************************************************************************************
//  TypeExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/18/2008 - James R Carroll
//       Generated original version of source code.
//  10/28/2008 - Pinal C. Patel
//      Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PCS.IO;

namespace PCS
{
    /// <summary>
    /// Extensions to all <see cref="Type"/> objects.
    /// </summary>
    public static class TypeExtensions
    {

        /// <summary>
        /// Gets the root type in the inheritace hierarchy from which the specified type inherits.
        /// </summary>
        /// <param name="type">The System.Type whose root type is to be found.</param>
        /// <returns>The root type in the inheritance hierarchy from which the specified type inherits.</returns>
        /// <remarks>Unless input type is System.Object, the returned type will never be System.Object, even though all types ultimately inherit from it.</remarks>
        public static Type GetRootType(this Type type)
        {
            // Recurse through types until you reach a base type of "System.Object"
            if (type.BaseType != typeof(object)) return GetRootType(type.BaseType);
            return type;
        }

        /// <summary>
        /// Gets a list of publicly visible types from assemblies in the application directory that are related to the 
        /// specified type either by inheritance of the type (if the type is a class) or implementation of the type 
        /// (if the type is an interface).
        /// </summary>
        /// <param name="type">The type being tested.</param>
        /// <returns>List of matching types.</returns>
        /// <remarks><see cref="GetTypes(Type)"/> will exclude abstract types (types that cannot be instantiated).</remarks>
        public static List<Type> LoadImplementations(this Type type)
        {
            return LoadImplementations(type, true);
        }

        /// <summary>
        /// Gets a list of publicly visible types from assemblies in the application directory that are related to the 
        /// specified type either by inheritance of the type (if the type is a class) or implementation of the type 
        /// (if the type is an interface).
        /// </summary>
        /// <param name="type">The type being tested.</param>
        /// <param name="excludeAbsractTypes">true if abstract types are not to be included; otherwise false.</param>
        /// <returns></returns>
        public static List<Type> LoadImplementations(this Type type, bool excludeAbsractTypes)
        {
            return LoadImplementations(type, string.Empty, excludeAbsractTypes);
        }

        public static List<Type> LoadImplementations(this Type type, string binariesDirectory)
        {
            return LoadImplementations(type, binariesDirectory, true);
        }

        public static List<Type> LoadImplementations(this Type type, string binariesDirectory, bool excludeAbsractTypes)
        {
            Assembly asm = null;
            List<Type> types = new List<Type>();

            if (string.IsNullOrEmpty(binariesDirectory))
            {
                switch (Common.GetApplicationType())
                {
                    // The binaries directory is not specified.
                    case ApplicationType.WindowsGui:
                    case ApplicationType.WindowsCui:
                        // Use application install directory for windows applications.
                        binariesDirectory = FilePath.GetAbsolutePath("");
                        break;
                    case ApplicationType.Web:
                        // Use the bin directory for web applications.
                        binariesDirectory = FilePath.GetAbsolutePath("bin");
                        break;
                }
            }

            // Loop through all files in the binaries directory.
            foreach (string bin in Directory.GetFiles(binariesDirectory))
            {
                // Only process DLLs and EXEs.
                if (!(string.Compare(FilePath.GetExtension(bin), ".dll", true) == 0 ||
                      string.Compare(FilePath.GetExtension(bin), ".exe", true) == 0))
                {
                    continue;
                }

                try
                {
                    // Load the assembly in the curent app domain.
                    asm = Assembly.LoadFrom(bin);

                    // Process only the public types in the assembly.
                    foreach (Type asmType in asm.GetExportedTypes())
                    {
                        if (!excludeAbsractTypes || !asmType.IsAbstract)
                        {
                            // Either the current type is not abstract or it's OK to include abstract types.
                            if (type.IsClass && asmType.IsSubclassOf(type))
                            {
                                // The type being tested is a class and current type derives from it.
                                types.Add(asmType);
                            }

                            if (type.IsInterface && asmType.GetInterface(type.Name) != null)
                            {
                                // The type being tested is an interface and current type implements it.
                                types.Add(asmType);
                            }
                        }
                    }
                }
                catch
                {
                    // Absorb any exception thrown while processing the assembly.
                }
            }

            return types;   // Return the matching types found.
        }
    }
}
