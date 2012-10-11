//******************************************************************************************************
//  FastObjectFactory.cs - Gbtc
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
//  01/25/2010 - James R. Carroll
//       Generated original version of source code.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
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
        // Static object creation delegate specific to type T - one instance will be created per type by the compiler
        private static Func<T> s_createObjectFunction;

        static FastObjectFactory()
        {
            // This is markedly faster than using Activator.CreateInstance
            Type type = typeof(T);
            DynamicMethod dynMethod = new DynamicMethod("ctor$" + type.Name, type, null, type);
            ILGenerator ilGen = dynMethod.GetILGenerator();
            ilGen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Ret);
            s_createObjectFunction = (Func<T>)dynMethod.CreateDelegate(typeof(Func<T>));
        }

        /// <summary>
        /// Gets delegate that quickly creates new instance of the specfied type.
        /// </summary>
        public static Func<T> CreateObjectFunction
        {
            get
            {
                return s_createObjectFunction;
            }
        }
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
        private static ConcurrentDictionary<Type, Delegate> s_createObjectFunctions = new ConcurrentDictionary<Type, Delegate>();

        /// <summary>
        /// Gets delegate that creates new instance of the <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of object to create quickly.</param>
        /// <returns>Delegate to use to quickly create new objects.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="type"/> does not support parameterless public constructor.</exception>
        public static Func<object> GetCreateObjectFunction(Type type)
        {
            return GetCreateObjectFunction<object>(type);
        }

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
            if (!type.IsAbstract &&
            (
               (typeof(T).IsClass && type.IsSubclassOf(typeof(T))) ||
               (typeof(T).IsInterface && (object)type.GetInterface(typeof(T).Name) != null))
            )
            {
                return (Func<T>)s_createObjectFunctions.GetOrAdd(type, (objType) =>
                {
                    // Get parameterless constructor for this type
                    ConstructorInfo typeCtor = objType.GetConstructor(Type.EmptyTypes);

                    if ((object)typeCtor == null)
                        throw new InvalidOperationException("Specified type parameter does not support parameterless public constructor");

                    // This is markedly faster than using Activator.CreateInstance
                    DynamicMethod dynMethod = new DynamicMethod("ctor_type$" + objType.Name, objType, null, objType);
                    ILGenerator ilGen = dynMethod.GetILGenerator();
                    ilGen.Emit(OpCodes.Newobj, typeCtor);
                    ilGen.Emit(OpCodes.Ret);
                    return (Func<T>)dynMethod.CreateDelegate(typeof(Func<T>));
                });
            }
            else
                throw new InvalidOperationException("Specified type parameter is not a subclass or interface implementation of function type definition");
        }
    }
}
