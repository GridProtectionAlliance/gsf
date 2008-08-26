using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Collections.DictionaryList.vb - Sorted dictionary style list that supports IList
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/07/2006 - J. Ritchie Carroll
//       Generated original version of source code
//
//*******************************************************************************************************

namespace TVA
{
    namespace Collections
    {

        /// <summary>This is essentially a sorted dictionary style list that implements IList.</summary>
        /// <remarks>
        /// <para>
        /// Important note about using an "Integer" as the key for this class: IDictionary implementations
        /// do not normally implement the IList interface because of ambiguity that is caused when implementing
        /// an integer key. For example, if you implement this class with a key of type "Integer" you will not
        /// be able to access items in the queue by index without "casting" the class as IList. This is because
        /// the Item property in both the IDictionary and IList would have the same parameters.
        /// </para>
        /// </remarks>
        public class DictionaryList<TKey, TValue> : IList<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
        {



            private System.Collections.SortedList<TKey, TValue> m_list;

            public DictionaryList()
			{
				
				m_list = new SortedList<TKey, TValue>();
				
			}

            #region " Generic IList(Of KeyValuePair(Of TKey, TValue)) Implementation "

            public void Add(KeyValuePair<TKey, TValue> item)
            {

                m_list.Add(item.Key, item.Value);

            }

            public void Clear()
            {

                m_list.Clear();

            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {

                return m_list.ContainsKey(item.Key);

            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                for (int x = 0; x <= m_list.Count - 1; x++)
                {
                    array[arrayIndex + x] = new KeyValuePair<TKey, TValue>(m_list.Keys(x), m_list.Values(x));
                }

            }

            public int Count
            {
                get
                {
                    return m_list.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {

                m_list.Remove(item.Key);

            }

            public int IndexOf(KeyValuePair<TKey, TValue> item)
            {

                return m_list.IndexOfKey(item.Key);

            }

            public KeyValuePair<TKey, TValue> this[int index]
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(m_list.Keys(index), m_list.Values(index));
                }
                set
                {
                    m_list[value.Key] = value.Value;
                }
            }

            public void RemoveAt(int index)
            {

                m_list.RemoveAt(index);

            }

            public void Insert(int index, KeyValuePair<TKey, TValue> item)
            {

                // It does not matter where you try to insert the value, since it will be inserted into its sorted
                // location, so we just add the value.
                m_list.Add(item.Key, item.Value);

            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {

                return m_list.GetEnumerator();

            }

            public IEnumerator GetEnumerator()
            {
                return this.IEnumerableGetEnumerator();
            }

            public IEnumerator IEnumerableGetEnumerator()
            {

                return ((System.Collections.IEnumerable)m_list).GetEnumerator();

            }

            #endregion

            #region " Generic IDictionary(Of TKey, TValue) Implemenentation "

            public void Add(TKey key, TValue value)
            {

                m_list.Add(key, value);

            }

            public bool ContainsKey(TKey key)
            {

                return m_list.ContainsKey(key);

            }

            public bool ContainsValue(TValue value)
            {

                return m_list.ContainsValue(value);

            }

            public int IndexOfKey(TKey key)
            {

                return m_list.IndexOfKey(key);

            }

            public int IndexOfValue(TValue value)
            {

                return m_list.IndexOfValue(value);

            }

            public TValue this[TKey key]
            {
                get
                {
                    return m_list[key];
                }
                set
                {
                    m_list[key] = value;
                }
            }

            public ICollection<TKey> Keys
            {
                get
                {
                    return m_list.Keys;
                }
            }

            public bool Remove(TKey key)
            {

                return m_list.Remove(key);

            }

            public bool TryGetValue(TKey key, ref TValue value)
            {

                return m_list.TryGetValue(key, value);

            }

            public ICollection<TValue> Values
            {
                get
                {
                    return m_list.Values;
                }
            }

            #endregion

        }

    }

}
