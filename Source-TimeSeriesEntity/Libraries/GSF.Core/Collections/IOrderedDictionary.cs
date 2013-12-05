//******************************************************************************************************
//  IOrderedDictionary.cs - Gbtc
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
//  12/04/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable PossibleInterfaceMemberAmbiguity

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a generic ordered collection of key/value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
    /// <remarks>
    /// Implementations of this interface using a <typeparamref name="TKey"/> of type <see cref="Int32"/> will cause ambiguity between
    /// accessing items by key and accessing items by index. If an integer is absolutely needed for the key, one could try using an
    /// <see cref="Int64"/> as the <typeparamref name="TKey"/> type instead.
    /// </remarks>
    public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>, IOrderedDictionary
    {
        /// <summary>
        /// Inserts a new entry into the <see cref="IOrderedDictionary{TKey,TValue}"/> with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than -or- <paramref name="index"/> is greater than <see cref="System.Collections.ICollection.Count"/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IOrderedDictionary{TKey,TValue}"/>.</exception>
        void Insert(int index, TKey key, TValue value);
    }
}
