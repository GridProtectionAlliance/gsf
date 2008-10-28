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
using System.IO;
using System.Collections.Generic;
using PCS.IO;
using System.Reflection;

namespace PCS
{
    /// <summary>
    /// Extensions to all <see cref="Type"/> objects.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets a list of publicly visible types from assemblies in the application directory that are related to the 
        /// specified type either by inheritance of the type (if the type is a class) or implementation of the type 
        /// (if the type is an interface).
        /// </summary>
        /// <param name="type">The type being tested.</param>
        /// <returns>List of matching types.</returns>
        /// <remarks><see cref="GetTypes(Type)"/> will exclude abstract types (types that cannot be instantiated).</remarks>
        public static List<Type> GetRelatedTypes(this Type type)
        {
            return GetRelatedTypes(type, true);
        }

        /// <summary>
        /// Gets a list of publicly visible types from assemblies in the application directory that are related to the 
        /// specified type either by inheritance of the type (if the type is a class) or implementation of the type 
        /// (if the type is an interface).
        /// </summary>
        /// <param name="type">The type being tested.</param>
        /// <param name="excludeAbsractTypes">true if abstract types are not to be included; otherwise false.</param>
        /// <returns></returns>
        public static List<Type> GetRelatedTypes(this Type type, bool excludeAbsractTypes)
        {
            Assembly asm = null;
            List<Type> types = new List<Type>();
            string binDirectory = FilePath.GetAbsolutePath("");

            if (Common.GetApplicationType() == ApplicationType.Web)
            {
                // In case of a web application, we look in bin directory for assemblies.
                binDirectory = Path.Combine(binDirectory, "bin");
            }

            // Loop through all files in the application directory.
            foreach (string bin in Directory.GetFiles(binDirectory))
            {
                // Only process DLLs and EXEs.
                if (!(string.Compare(Path.GetExtension(bin), ".dll", true) == 0 || 
                      string.Compare(Path.GetExtension(bin), ".exe", true) == 0))
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
