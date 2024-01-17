//******************************************************************************************************
//  TypeExtensions.cs - Gbtc
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
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  06/30/2009 - Pinal C. Patel
//       Update LoadImplementations to use FQN when checking if a Type implements a specific interface
//       to avoid namespace collisions.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GSF;

/// <summary>
/// Extensions to all <see cref="Type"/> objects.
/// </summary>
public static partial class TypeExtensions
{
    // Native data types that represent numbers
    private static readonly Type[] s_numericTypes = 
    { 
        typeof(sbyte), 
        typeof(byte), 
        typeof(short), 
        typeof(ushort), 
        typeof(Int24), 
        typeof(UInt24), 
        typeof(int), 
        typeof(uint), 
        typeof(long), 
        typeof(ulong), 
        typeof(float), 
        typeof(double), 
        typeof(decimal)
    };

    private static readonly Regex s_validCSharpIdentifierRegex = new(@"[\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Nd}\p{Pc}\p{Mn}\p{Mc}\p{Cf}]", RegexOptions.Compiled);

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
    /// Gets the friendly class name of the provided type, trimming generic parameters.
    /// </summary>
    /// <param name="type">Type to get friendly class name for.</param>
    /// <returns>Friendly class name of the provided type, or <see cref="string.Empty"/> if <paramref name="type"/> is <c>null</c>.</returns>
    public static string GetFriendlyClassName(this Type type)
    {
        string name = type?.FullName;

        if (string.IsNullOrEmpty(name))
            return string.Empty;

        int length = name.Length;
        int indexOfBracket = name.IndexOf('[');
        int indexOfComma = name.IndexOf(',');

        if (indexOfBracket >= 0)
            length = Math.Min(indexOfBracket, length);

        if (indexOfComma >= 0)
            length = Math.Min(indexOfComma, length);

        name = name.Substring(0, length).Trim();

        return name;
    }

    /// <summary>
    /// Gets the root type in the inheritance hierarchy from which the specified <paramref name="type"/> inherits.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> whose root type is to be found.</param>
    /// <returns>The root type in the inheritance hierarchy from which the specified <paramref name="type"/> inherits.</returns>
    /// <remarks>
    /// Unless input <paramref name="type"/> is <see cref="object"/> or <see cref="MarshalByRefObject"/>, the returned type will never 
    /// be <see cref="object"/> or <see cref="MarshalByRefObject"/>, even though all types ultimately inherit from either one of them.
    /// </remarks>
    public static Type GetRootType(this Type type)
    {
        while (true)
        {
            // Recurse through types until you reach a base type of "System.Object" or "System.MarshalByRef".
            if (type.BaseType is null || type.BaseType == typeof(object) || type.BaseType == typeof(MarshalByRefObject))
                return type;

            type = type.BaseType;
        }
    }

    /// <summary>
    /// Gets a C#-compatible proper type name, resolving generic type names using reflection with no backticks (`), for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> whose name is to be resolved.</param>
    /// <param name="includeNamespaces">Flag that indicates if namespaces should be included in the type name.</param>
    /// <returns>
    /// A C#-compatible proper type name, resolving generic type names using reflection with no backticks (`), for the specified <paramref name="type"/>.
    /// </returns>
    /// <remarks>
    /// This method will return a C#-compatible proper type name, resolving generic type names using reflection, which creates a valid,
    /// useable type name versus what <see cref="Type.FullName"/> returns. For example, <see cref="Type.FullName"/> will return something like:
    /// <c>System.Collections.Generic.List`1[[System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]</c>
    /// with a backtick (`) to indicate a generic type and noisy assembly info. For the same type, this method would instead return:
    /// <c>System.Collections.Generic.List&lt;System.String&gt;</c> which is a valid, usable C# type name. You can also set the
    /// <paramref name="includeNamespaces"/> parameter to <c>false</c> to remove namespaces from the type name, which would yield:
    /// <c>List&lt;String&gt;</c> for the above example.
    /// </remarks>
    public static string GetReflectedTypeName(this Type type, bool includeNamespaces = true)
    {
        return type.GetReflectedTypeName(includeNamespaces, null, null);
    }

    private static string GetReflectedTypeName(this Type type, bool includeNamespaces, Stack<Type> genericArgs, StringBuilder arrayBrackets)
    {
        StringBuilder code = new();
        Type declaringType = type.DeclaringType;
        bool arrayBracketsWasNull = arrayBrackets == null;
            
        genericArgs ??= new Stack<Type>(type.GetGenericArguments());

        int currentTypeGenericArgsCount = genericArgs.Count;
            
        if (declaringType != null)
            currentTypeGenericArgsCount -= declaringType.GetGenericArguments().Length;

        Type[] currentTypeGenericArgs = new Type[currentTypeGenericArgsCount];
            
        for (int i = currentTypeGenericArgsCount - 1; i >= 0; i--)
            currentTypeGenericArgs[i] = genericArgs.Pop();

        if (declaringType != null)
            code.Append(declaringType.GetReflectedTypeName(includeNamespaces, genericArgs, null)).Append('.');

        if (type.IsArray)
        {
            arrayBrackets ??= new StringBuilder();
            arrayBrackets.Append('[');
            arrayBrackets.Append(',', type.GetArrayRank() - 1);
            arrayBrackets.Append(']');

            Type elementType = type.GetElementType();
            code.Insert(0, elementType.GetReflectedTypeName(includeNamespaces, null, arrayBrackets));
        }
        else
        {
            static bool isValidCSharpIdentifier(char identifier) =>
                s_validCSharpIdentifierRegex.IsMatch(identifier.ToString());

            code.Append(new string(type.Name.TakeWhile(isValidCSharpIdentifier).ToArray()));

            if (currentTypeGenericArgsCount > 0)
            {
                code.Append('<');
                    
                for (int i = 0; i < currentTypeGenericArgsCount; i++)
                {
                    code.Append(currentTypeGenericArgs[i].GetReflectedTypeName(includeNamespaces, null, null));

                    if (i < currentTypeGenericArgsCount - 1)
                        code.Append(',');
                }
                    
                code.Append('>');
            }

            if (declaringType == null && !string.IsNullOrEmpty(type.Namespace) && includeNamespaces)
                code.Insert(0, '.').Insert(0, type.Namespace);
        }

        if (arrayBracketsWasNull && arrayBrackets != null)
            code.Append(arrayBrackets);

        return code.ToString();
    }
}