//******************************************************************************************************
//  DictionaryDebugView.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/18/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable UnusedTypeParameter

#region [ Contributor License Agreements ]

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the NOTICE.txt file in the project root for more information.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GSF.Collections;

internal sealed class DictionaryDebugView<TK, TV>
{
    private readonly IDictionary<TK, TV> m_dict;

    public DictionaryDebugView(IDictionary<TK, TV> dictionary)
    {
        m_dict = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public KeyValuePair<TK, TV>[] Items
    {
        get
        {
            KeyValuePair<TK, TV>[] items = new KeyValuePair<TK, TV>[m_dict.Count];
            m_dict.CopyTo(items, 0);
            return items;
        }
    }
}

internal sealed class DictionaryKeyCollectionDebugView<TKey, TValue>
{
    private readonly ICollection<TKey> m_collection;

    public DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
    {
        m_collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TKey[] Items
    {
        get
        {
            TKey[] items = new TKey[m_collection.Count];
            m_collection.CopyTo(items, 0);
            return items;
        }
    }
}

internal sealed class DictionaryValueCollectionDebugView<TKey, TValue>
{
    private readonly ICollection<TValue> m_collection;

    public DictionaryValueCollectionDebugView(ICollection<TValue> collection)
    {
        m_collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TValue[] Items
    {
        get
        {
            TValue[] items = new TValue[m_collection.Count];
            m_collection.CopyTo(items, 0);
            return items;
        }
    }
}