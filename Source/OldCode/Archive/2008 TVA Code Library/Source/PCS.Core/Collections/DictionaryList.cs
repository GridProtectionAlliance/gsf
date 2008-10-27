//*******************************************************************************************************
//  DictionaryList.cs
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
//  01/07/2006 - J. Ritchie Carroll
//       Generated original version of source code
//  09/10/2008 - J. Ritchie Carroll
//      Converted to C#
//
//*******************************************************************************************************

using System.Collections;
using System.Collections.Generic;

namespace PCS.Collections
{
    /// <summary>Sorted dictionary style list that supports IList.</summary>
    /// <remarks>
    /// <para>
    /// Have you ever needed the quick look-up feature on a Dictionary (e.g., Hashtable), but ended
    /// up missing the indexed or sequential access like you have in a list? You may have wondered why
    /// the .NET dictionary class doesn’t implement the IList interface which allows this. The reason
    /// IDictionary implementations do not normally implement the IList interface is because of
    /// ambiguity that is caused when implementing an integer key. For example, if you created a
    /// dictionary style class with a key of type "Integer" that actually did implement IList(Of T),
    /// you would not be able to access items in the IList interface by index without "casting" the
    /// class as IList. This is because the Item property in both the IDictionary and IList would have
    /// the same parameters. Note, however, that generics in .NET 2.0 gladly allow a class to implement
    /// both IDictionary and IList (even specifying as Integer as the key) so long you as you are happy
    /// knowing that the compiler will choose if you access your items by index or key. Given that
    /// caveat, there are many times when you need a dictionary style collection but also desire an
    /// IList implementation so the class can be used in other ways without conversion. As a result of
    /// these needs, we’ve added a generic class to code library called a DictionaryList -- which is
    /// essentially just a sorted dictionary style list (i.e., SortedList) that implements the
    /// IList(Of T) interface (specifically as IList(Of KeyValuePair(Of TKey, TValue))). You will find
    /// all of your convenient expected methods related to both dictionaries and lists; that is, you can
    /// look-up items by key or by index. The class works perfectly for any non-Integer based key
    /// (e.g., String, custom class, etc.) -- note that specifying an Integer as the key for the class
    /// won’t cause an error, but it also will not be very useful. However, you can specify the key for
    /// your DictionaryList as a "Long," which allows you to use long integers for keyed look-ups and
    /// regular integers for indexed access--the best of both worlds! In summary, I would not change
    /// your programming habits to start using this for "my collection for everything," as nothing comes
    /// for free; however, if you have a need for a "hybrid" collection class, this fits the bill.
    /// </para>
    /// <para>
    /// Important note about using an "Integer" as the key for this class: IDictionary implementations
    /// do not normally implement the IList interface because of ambiguity that is caused when implementing
    /// an integer key. For example, if you implement this class with a key of type "Integer" you will not
    /// be able to access items in the queue by index without "casting" the class as IList. This is because
    /// the Item property in both the IDictionary and IList would have the same parameters.
    /// </para>
    /// <para>
    /// Note that prior to the addition of Generics in .NET, the class that performed a similar function
    /// was the "NameObjectCollectionBase" in the System.Collections.Specialized namespace which
    /// specifically allowed item access by either key or by index.  This class is similar in function
    /// but instead is a generic class allowing use with any strongly typed key or value. 
    /// </para>
    /// </remarks>
    public class DictionaryList<TKey, TValue> : IList<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        #region [ Members ]

        // Fields
        private SortedList<TKey, TValue> m_list;

        #endregion

        #region [ Constructors ]

        public DictionaryList()
        {
            m_list = new SortedList<TKey, TValue>();
        }

        #endregion

        #region [ Properties ]

        // Generic IList(Of KeyValuePair(Of TKey, TValue)) Properties

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

        public KeyValuePair<TKey, TValue> this[int index]
        {
            get
            {
                return new KeyValuePair<TKey, TValue>(m_list.Keys[index], m_list.Values[index]);
            }
            set
            {
                m_list[value.Key] = value.Value;
            }
        }

        // Generic IDictionary(Of TKey, TValue) Properties

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

        public ICollection<TValue> Values
        {
            get
            {
                return m_list.Values;
            }
        }

        #endregion

        #region [ Methods ]

        // Generic IList(Of KeyValuePair(Of TKey, TValue)) Methods

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
                array[arrayIndex + x] = new KeyValuePair<TKey, TValue>(m_list.Keys[x], m_list.Values[x]);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return m_list.Remove(item.Key);
        }

        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return m_list.IndexOfKey(item.Key);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_list).GetEnumerator();
        }

        // Generic IDictionary(Of TKey, TValue) Methods

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

        public bool Remove(TKey key)
        {
            return m_list.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_list.TryGetValue(key, out value);
        }

        #endregion
    }
}