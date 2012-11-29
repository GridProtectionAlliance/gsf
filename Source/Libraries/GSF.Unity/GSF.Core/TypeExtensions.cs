//******************************************************************************************************
//  TypeExtensions.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/18/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  10/28/2008 - Pinal C. Patel
//       Edited code comments.
//  01/30/2009 - J. Ritchie Carroll
//       Added TryGetAttribute extension.
//  06/30/2009 - Pinal C. Patel
//       Fixed LoadImplementations() to correctly use FilePath.GetFileList().
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/29/2010 - Pinal C. Patel
//       Updated GetRootType() method to include MarshalByRefObject in the root type exclusion list.
//  01/14/2011 - J. Ritchie Carroll
//       Added is numeric type extension.
//  09/22/2011 - J. Ritchie Carroll
//       Added Mono implementation exception regions.
//  10/11/2011 - Pinal C. Patel
//       Updated LoadImplementations() method to add support for attributes.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************



#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using GSF.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GSF
{
    /// <summary>
    /// Extensions to all <see cref="Type"/> objects.
    /// </summary>
    public static class TypeExtensions
    {
        // Native data types that represent numbers
        private static Type[] s_numericTypes = { typeof(SByte), typeof(Byte), typeof(Int16), typeof(UInt16), typeof(Int24), typeof(UInt24), typeof(Int32), typeof(UInt32), typeof(Int64), typeof(UInt64), typeof(Single), typeof(Double), typeof(Decimal) };

        /// <summary>
        /// Determines if the specified type is a native structure that represents a numeric value.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> being tested.</param>
        /// <returns><c>true</c> if the specified type is a native structure that represents a numeric value.</returns>
        /// <remarks>
        /// For this method a boolean value is not considered numeric even though it can be thought of as a bit.
        /// This expression returns <c>true</c> if the type is one of the following:<br/><br/>
        ///     SByte, Byte, Int16, UInt16, Int24, UInt24, Int32, UInt32, Int64, UInt64, Single, Double, Decimal
        /// </remarks>
        public static bool IsNumeric(this Type type)
        {
            return s_numericTypes.Contains(type);
        }

        /// <summary>
        /// Gets the root type in the inheritace hierarchy from which the specified <paramref name="type"/> inherits.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> whose root type is to be found.</param>
        /// <returns>The root type in the inheritance hierarchy from which the specified <paramref name="type"/> inherits.</returns>
        /// <remarks>
        /// Unless input <paramref name="type"/> is <see cref="Object"/> or <see cref="MarshalByRefObject"/>, the returned type will never 
        /// be <see cref="Object"/> or <see cref="MarshalByRefObject"/>, even though all types ultimately inherit from either one of them.
        /// </remarks>
        public static Type GetRootType(this Type type)
        {
            // Recurse through types until you reach a base type of "System.Object" or "System.MarshalByRef".
#if MONO
            if ((object)type.BaseType == null || string.Compare(type.BaseType.FullName, "System.Object") == 0 || string.Compare(type.BaseType.FullName, "System.MarshalByRefObject") == 0)
#else
            if ((object)type.BaseType == null || type.BaseType == typeof(object) || type.BaseType == typeof(MarshalByRefObject))
#endif
                return type;
            else
                return GetRootType(type.BaseType);
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
        /// <param name="excludeAbstractTypes">true to exclude public types that are abstract; otherwise false.</param>
        /// <returns>Public types that implement the specified <paramref name="type"/>.</returns>
        public static List<Type> LoadImplementations(this Type type, bool excludeAbstractTypes)
        {
            return LoadImplementations(type, string.Empty, excludeAbstractTypes);
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
        /// <param name="excludeAbstractTypes">true to exclude public types that are abstract; otherwise false.</param>
        /// <returns>Public types that implement the specified <paramref name="type"/>.</returns>
        public static List<Type> LoadImplementations(this Type type, string binariesDirectory, bool excludeAbstractTypes)
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
                        if (!excludeAbstractTypes || !asmType.IsAbstract)
                        {
                            // Either the current type is not abstract or it's OK to include abstract types.
                            if (type.IsClass && asmType.IsSubclassOf(type))
                            {
                                // The type being tested is a class and current type derives from it.
                                types.Add(asmType);
                            }

                            if (type.IsInterface && (object)asmType.GetInterface(type.Name) != null)
                            {
                                // The type being tested is an interface and current type implements it.
                                types.Add(asmType);
                            }

                            if (type.GetRootType() == typeof(Attribute) && asmType.GetCustomAttributes(type, true).Length > 0)
                            {
                                // The type being tested is an attribute and current type has the attribute.
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
