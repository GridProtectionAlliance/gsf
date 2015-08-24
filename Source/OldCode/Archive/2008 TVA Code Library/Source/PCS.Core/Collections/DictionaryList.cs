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
//  02/19/2009 - Josh Patterson
//      Edited Code Comments
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

        /// <summary>
        /// Creates a new <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        public DictionaryList()
        {
            m_list = new SortedList<TKey, TValue>();
        }

        #endregion

        #region [ Properties ]

        // Generic IList(Of KeyValuePair(Of TKey, TValue)) Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return m_list.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="DictionaryList{TKey,TValue}"/> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
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

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
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

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                return m_list.Keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
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

        /// <summary>
        /// Adds an item to the <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        /// <param name="item">The key value pair item to add.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            m_list.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all items from the <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        public void Clear()
        {
            m_list.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="DictionaryList{TKey,TValue}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="DictionaryList{TKey,TValue}"/>.</param>
        /// <returns>true if item is found in the <see cref="DictionaryList{TKey,TValue}"/>; otherwise, false</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return m_list.ContainsKey(item.Key);
        }

        /// <summary>
        /// Copies the elements of the <see cref="DictionaryList{TKey,TValue}"/> to an <see cref="System.Array"/>, starting at a particular index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="System.Array"/> that is the destination of the elements 
        /// copied from <see cref="DictionaryList{TKey,TValue}"/>. The array must 
        /// have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            for (int x = 0; x < m_list.Count; x++)
            {
                array[arrayIndex + x] = new KeyValuePair<TKey, TValue>(m_list.Keys[x], m_list.Values[x]);
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="DictionaryList{TKey,TValue}"/>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="DictionaryList{TKey,TValue}"/>; 
        /// otherwise, false. This method also returns false if item is not found in 
        /// the original <see cref="DictionaryList{TKey,TValue}"/>.
        /// </returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return m_list.Remove(item.Key);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="DictionaryList{TKey,TValue}"/>.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return m_list.IndexOfKey(item.Key);
        }

        /// <summary>
        /// Removes the <see cref="DictionaryList{TKey,TValue}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            m_list.RemoveAt(index);
        }

        /// <summary>
        /// Inserts an item to the <see cref="DictionaryList{TKey,TValue}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="DictionaryList{TKey,TValue}"/>.</param>
        public void Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            // It does not matter where you try to insert the value, since it will be inserted into its sorted
            // location, so we just add the value.
            m_list.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_list).GetEnumerator();
        }

        // Generic IDictionary(Of TKey, TValue) Methods

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(TKey key, TValue value)
        {
            m_list.Add(key, value);
        }

        /// <summary>
        /// Determines whether the <see cref="DictionaryList{TKey,TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="DictionaryList{TKey,TValue}"/>.</param>
        /// <returns>true if the <see cref="DictionaryList{TKey,TValue}"/> contains an element with the key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            return m_list.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="DictionaryList{TKey,TValue}"/> contains a specific value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="DictionaryList{TKey,TValue}"/>. The value can be null for reference types.</param>
        /// <returns>true if the <see cref="DictionaryList{TKey,TValue}"/> contains an element with the specified value; otherwise, false.</returns>
        public bool ContainsValue(TValue value)
        {
            return m_list.ContainsValue(value);
        }

        /// <summary>
        /// Searches for the specified key and returns the zero-based index within the entire <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="DictionaryList{TKey,TValue}"/>.</param>
        /// <returns>The zero-based index of key within the entire <see cref="DictionaryList{TKey,TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfKey(TKey key)
        {
            return m_list.IndexOfKey(key);
        }

        /// <summary>
        /// Searches for the specified value and returns the zero-based index of the first occurrence within the entire <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="DictionaryList{TKey,TValue}"/>. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire <see cref="DictionaryList{TKey,TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfValue(TValue value)
        {
            return m_list.IndexOfValue(value);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="DictionaryList{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false. This method also returns false if key
        /// was not found in the original <see cref="DictionaryList{TKey,TValue}"/>.</returns>
        public bool Remove(TKey key)
        {
            return m_list.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if 
        /// the key is found; otherwise, the default value for the type of the value
        /// parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the <see cref="DictionaryList{TKey,TValue}"/> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_list.TryGetValue(key, out value);
        }

        #endregion
    }
}