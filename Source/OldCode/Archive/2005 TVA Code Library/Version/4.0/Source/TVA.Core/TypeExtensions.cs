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
//  01/30/2009 - James R Carroll
//      Added TryGetAttribute extension.
//  06/30/2009 - Pinal C. Patel
//      Fixed LoadImplementations() to correctly use FilePath.GetFileList().
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using TVA.IO;

namespace TVA
{
    /// <summary>
    /// Extensions to all <see cref="Type"/> objects.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the root type in the inheritace hierarchy from which the specified type inherits.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> whose root type is to be found.</param>
        /// <returns>The root type in the inheritance hierarchy from which the specified type inherits.</returns>
        /// <remarks>
        /// Unless input type is <see cref="Object"/>, the returned type will never be <see cref="Object"/>, 
        /// even though all types ultimately inherit from it.
        /// </remarks>
        public static Type GetRootType(this Type type)
        {
            // Recurse through types until you reach a base type of "System.Object"
            if (type.BaseType != typeof(object)) return GetRootType(type.BaseType);
            return type;
        }

        /// <summary>
        /// Attempts to get the specified <paramref name="Attribute"/> from a <see cref="Type"/>, returning <c>true</c> if it does.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> object over which to search attributes.</param>
        /// <param name="attribute">The <see cref="Attribute"/> that was found, if any.</param>
        /// <returns><c>true</c> if <paramref name="Attribute"/> was found; otherwise <c>false</c>.</returns>
        /// <typeparam name="TAttribute"><see cref="Type"/> of <see cref="Attribute"/> to attempt to retrieve.</typeparam>
        public static bool TryGetAttribute<TAttribute>(this Type type, out TAttribute attribute)
            where TAttribute : Attribute
        {
            object[] attributes = type.GetCustomAttributes(typeof(TAttribute), true);

            if (attributes.Length > 0)
            {
                attribute = attributes[0] as TAttribute;
                return true;
            }

            attribute = null;
            return false;
        }

        /// <summary>
        /// Loads public types from assemblies in the application binaries directory that implement the specified 
        /// <paramref name="type"/> either through class inheritance or interface implementation.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> that must be implemented by the public types.</param>
        /// <returns>Public types that implement the specified <paramref name="type"/>.</returns>
        public static List<Type> LoadImplementations(this Type type)
        {
            return LoadImplementations(type, true);
        }

        /// <summary>
        /// Loads public types from assemblies in the application binaries directory that implement the specified 
        /// <paramref name="type"/> either through class inheritance or interface implementation.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> that must be implemented by the public types.</param>
        /// <param name="excludeAbsractTypes">true to exclude public types that are abstract; otherwise false.</param>
        /// <returns>Public types that implement the specified <paramref name="type"/>.</returns>
        public static List<Type> LoadImplementations(this Type type, bool excludeAbsractTypes)
        {
            return LoadImplementations(type, string.Empty, excludeAbsractTypes);
        }

        /// <summary>
        /// Loads public types from assemblies in the specified <paramref name="binariesDirectory"/> that implement 
        /// the specified <paramref name="type"/> either through class inheritance or interface implementation.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> that must be implemented by the public types.</param>
        /// <param name="binariesDirectory">The directory containing the assemblies to be processed.</param>
        /// <returns>Public types that implement the specified <paramref name="type"/>.</returns>
        public static List<Type> LoadImplementations(this Type type, string binariesDirectory)
        {
            return LoadImplementations(type, binariesDirectory, true);
        }

        /// <summary>
        /// Loads public types from assemblies in the specified <paramref name="binariesDirectory"/> that implement 
        /// the specified <paramref name="type"/> either through class inheritance or interface implementation.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> that must be implemented by the public types.</param>
        /// <param name="binariesDirectory">The directory containing the assemblies to be processed.</param>
        /// <param name="excludeAbsractTypes">true to exclude public types that are abstract; otherwise false.</param>
        /// <returns>Public types that implement the specified <paramref name="type"/>.</returns>
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
                        binariesDirectory = FilePath.GetAbsolutePath("*.*");
                        break;
                    case ApplicationType.Web:
                        // Use the bin directory for web applications.
                        binariesDirectory = FilePath.GetAbsolutePath("bin\\*.*");
                        break;
                }
            }

            // Loop through all files in the binaries directory.
            foreach (string bin in FilePath.GetFileList(binariesDirectory))
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
