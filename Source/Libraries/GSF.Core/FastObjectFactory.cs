//******************************************************************************************************
//  FastObjectFactory.cs - Gbtc
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
//  01/25/2010 - James R. Carroll
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//  12/20/2013 - Pinal C. Patel
//       Updated the dictionary used for caching the activators returned by GetCreateObjectFunction<T>() 
//       to not use Type for the key but instead use a combined hash of Type whose activator is to be
//       created along with the Type being returned. This enables scenarios where multiple activators
//       of the same type needs to be created but the return type needs to different.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace GSF
{
    /// <summary>
    /// Quickly creates new objects based on specified type.
    /// </summary>
    /// <typeparam name="T">Type of object to create quickly.</typeparam>
    /// <remarks>
    /// You can use the alternate <see cref="FastObjectFactory"/> implementation if you only have the <see cref="Type"/> of
    /// an object available (such as when you are using reflection).
    /// </remarks>
    public static class FastObjectFactory<T> where T : class, new()
    {
        static FastObjectFactory()
        {
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);

            if (constructor is null)
                throw new InvalidOperationException("No parameterless constructor exists for type " + type.FullName);

            // This is markedly faster than using Activator.CreateInstance
            DynamicMethod method = new("ctor$" + type.Name, type, null, type);
            ILGenerator generator = method.GetILGenerator();

            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Ret);

            // Static object creation delegate specific to type T - one instance will be created per type by the compiler
            CreateObjectFunction = (Func<T>)method.CreateDelegate(typeof(Func<T>));
        }

        /// <summary>
        /// Gets delegate that quickly creates new instance of the specified type.
        /// </summary>
        public static Func<T> CreateObjectFunction { get; }
    }

    /// <summary>
    /// Quickly creates new objects based on specified type.
    /// </summary>
    /// <remarks>
    /// <see cref="FastObjectFactory"/> should be used when you only have the <see cref="Type"/> of an object available (such as when you are
    /// using reflection), otherwise you should use the generic <see cref="FastObjectFactory{T}"/>.
    /// </remarks>
    public static class FastObjectFactory
    {
        // We cache object creation functions by type so they are only created once
        private static readonly ConcurrentDictionary<int, Delegate> s_createObjectFunctions = new();

        /// <summary>
        /// Gets delegate that creates new instance of the <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of object to create quickly.</param>
        /// <returns>Delegate to use to quickly create new objects.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="type"/> does not support parameterless public constructor.</exception>
        public static Func<object> GetCreateObjectFunction(Type type) => 
            GetCreateObjectFunction<object>(type);

        /// <summary>
        /// Gets delegate of specified return type that creates new instance of the <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of object to create quickly.</param>
        /// <typeparam name="T">Type of returned object function used to create objects quickly.</typeparam>
        /// <returns>Delegate to use to quickly create new objects.</returns>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="type"/> does not support parameterless public constructor -or- 
        /// <paramref name="type"/> is not a subclass or interface implementation of function type definition.
        /// </exception>
        /// <remarks>
        /// This function will validate that <typeparamref name="T"/> is related to <paramref name="type"/>.
        /// </remarks>
        public static Func<T> GetCreateObjectFunction<T>(Type type)
        {
            // Since user can call this function with any type, we verify that it is related to the return type. If return type
            // is a class, see if type derives from it, else if return type is an interface, see if type implements it.
            Type typeT = typeof(T);

            if (type.IsAbstract || (!typeT.IsClass || !type.IsSubclassOf(typeT)) && (!typeT.IsInterface || type.GetInterface(typeT.Name) is null))
                throw new InvalidOperationException("Specified type parameter is not a subclass or interface implementation of function type definition");

        #if MONO
            // Type.GUID always returns zero guid on Mono (still true as of 3/25/2022)
            int key = type.FullName!.GetHashCode() ^ typeT.FullName!.GetHashCode();
        #else
            int key = type.GUID.GetHashCode() ^ typeT.GUID.GetHashCode();
        #endif

            return (Func<T>)s_createObjectFunctions.GetOrAdd(key, _ =>
            {
                // Get parameterless constructor for this type
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);

                if (constructor is null)
                    throw new InvalidOperationException("No parameterless constructor exists for type " + type.FullName);

                // This is markedly faster than using Activator.CreateInstance
                DynamicMethod method = new("ctor_type$" + type.Name, type, null, type);
                ILGenerator generator = method.GetILGenerator();

                generator.Emit(OpCodes.Newobj, constructor);
                generator.Emit(OpCodes.Ret);

                return (Func<T>)method.CreateDelegate(typeof(Func<T>));
            });
        }
    }
}