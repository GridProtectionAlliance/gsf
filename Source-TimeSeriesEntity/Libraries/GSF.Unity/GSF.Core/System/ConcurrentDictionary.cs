#if MONO
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
namespace System.Collections.Concurrent
{
    /// <summary>Represents a thread-safe collection of key-value pairs that can be accessed by multiple threads concurrently. </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [__DynamicallyInvokable, DebuggerDisplay("Count = {Count}"), ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    [Serializable]
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        private class Tables
        {
            internal readonly ConcurrentDictionary<TKey, TValue>.Node[] m_buckets;
            internal readonly object[] m_locks;
            internal volatile int[] m_countPerLock;
            internal Tables(ConcurrentDictionary<TKey, TValue>.Node[] buckets, object[] locks, int[] countPerLock)
            {
                this.m_buckets = buckets;
                this.m_locks = locks;
                this.m_countPerLock = countPerLock;
            }
        }
        private class Node
        {
            internal TKey m_key;
            internal TValue m_value;
            internal volatile ConcurrentDictionary<TKey, TValue>.Node m_next;
            internal int m_hashcode;
            internal Node(TKey key, TValue value, int hashcode, ConcurrentDictionary<TKey, TValue>.Node next)
            {
                this.m_key = key;
                this.m_value = value;
                this.m_next = next;
                this.m_hashcode = hashcode;
            }
        }
        private class DictionaryEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> m_enumerator;
            public DictionaryEntry Entry
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = this.m_enumerator.Current;
                    object arg_30_0 = current.Key;
                    KeyValuePair<TKey, TValue> current2 = this.m_enumerator.Current;
                    return new DictionaryEntry(arg_30_0, current2.Value);
                }
            }
            public object Key
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = this.m_enumerator.Current;
                    return current.Key;
                }
            }
            public object Value
            {
                get
                {
                    KeyValuePair<TKey, TValue> current = this.m_enumerator.Current;
                    return current.Value;
                }
            }
            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }
            internal DictionaryEnumerator(ConcurrentDictionary<TKey, TValue> dictionary)
            {
                this.m_enumerator = dictionary.GetEnumerator();
            }
            public bool MoveNext()
            {
                return this.m_enumerator.MoveNext();
            }
            public void Reset()
            {
                this.m_enumerator.Reset();
            }
        }
        [NonSerialized]
        private volatile ConcurrentDictionary<TKey, TValue>.Tables m_tables;
        private readonly IEqualityComparer<TKey> m_comparer;
        [NonSerialized]
        private readonly bool m_growLockArray;
        [NonSerialized]
        private int m_budget;
        private KeyValuePair<TKey, TValue>[] m_serializationArray;
        private int m_serializationConcurrencyLevel;
        private int m_serializationCapacity;
        private static readonly bool s_isValueWriteAtomic = ConcurrentDictionary<TKey, TValue>.IsValueWriteAtomic();
        private const int DEFAULT_CONCURRENCY_MULTIPLIER = 4;
        private const int DEFAULT_CAPACITY = 31;
        private const int MAX_LOCK_NUMBER = 1024;
        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <returns>Returns the Value property of the <see cref="T:System.Collections.Generic.KeyValuePair`2" /> at the specified index.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
        [__DynamicallyInvokable]
        public TValue this[TKey key]
        {
            [__DynamicallyInvokable]
            get
            {
                TValue result;
                if (!this.TryGetValue(key, out result))
                {
                    throw new KeyNotFoundException();
                }
                return result;
            }
            [__DynamicallyInvokable]
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                this.TryAddInternal(key, value, true, true, out tValue);
            }
        }
        /// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
        /// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements, <see cref="F:System.Int32.MaxValue" />.</exception>
        [__DynamicallyInvokable]
        public int Count
        {
            [__DynamicallyInvokable]
            get
            {
                int num = 0;
                int toExclusive = 0;
                try
                {
                    this.AcquireAllLocks(ref toExclusive);
                    for (int i = 0; i < this.m_tables.m_countPerLock.Length; i++)
                    {
                        num += this.m_tables.m_countPerLock[i];
                    }
                }
                finally
                {
                    this.ReleaseLocks(0, toExclusive);
                }
                return num;
            }
        }
        /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> is empty.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> is empty; otherwise, false.</returns>
        [__DynamicallyInvokable]
        public bool IsEmpty
        {
            [__DynamicallyInvokable]
            get
            {
                int toExclusive = 0;
                try
                {
                    this.AcquireAllLocks(ref toExclusive);
                    for (int i = 0; i < this.m_tables.m_countPerLock.Length; i++)
                    {
                        if (this.m_tables.m_countPerLock[i] != 0)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    this.ReleaseLocks(0, toExclusive);
                }
                return true;
            }
        }
        /// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection{TKey}" /> containing the keys in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.</returns>
        [__DynamicallyInvokable]
        public ICollection<TKey> Keys
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetKeys();
            }
        }
        /// <summary>Gets a collection containing the values in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection{TValue}" /> containing the values in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.</returns>
        [__DynamicallyInvokable]
        public ICollection<TValue> Values
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetValues();
            }
        }
        [__DynamicallyInvokable]
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            [__DynamicallyInvokable]
            get
            {
                return false;
            }
        }
        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> has a fixed size.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> has a fixed size; otherwise, false. For <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />, this property always returns false.</returns>
        [__DynamicallyInvokable]
        bool IDictionary.IsFixedSize
        {
            [__DynamicallyInvokable]
            get
            {
                return false;
            }
        }
        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> is read-only.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> is read-only; otherwise, false. For <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />, this property always returns false.</returns>
        [__DynamicallyInvokable]
        bool IDictionary.IsReadOnly
        {
            [__DynamicallyInvokable]
            get
            {
                return false;
            }
        }
        /// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</summary>
        /// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</returns>
        [__DynamicallyInvokable]
        ICollection IDictionary.Keys
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetKeys();
            }
        }
        /// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</summary>
        /// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</returns>
        [__DynamicallyInvokable]
        ICollection IDictionary.Values
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetValues();
            }
        }
        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <returns>The value associated with the specified key, or a null reference (Nothing in Visual Basic) if <paramref name="key" /> is not in the dictionary or <paramref name="key" /> is of a type that is not assignable to the key type  of the <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentException">A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the key type  of the <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" />. -or- A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the value type  of the <see cref="T:System.Collections.Generic.ConcurrentDictionary{TKey,TValue}" /></exception>
        [__DynamicallyInvokable]
        object IDictionary.this[object key]
        {
            [__DynamicallyInvokable]
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                if (key is TKey && this.TryGetValue((TKey)((object)key), out tValue))
                {
                    return tValue;
                }
                return null;
            }
            [__DynamicallyInvokable]
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (!(key is TKey))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfKeyIncorrect"));
                }
                if (!(value is TValue))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfValueIncorrect"));
                }
                this[(TKey)((object)key)] = (TValue)((object)value);
            }
        }
        /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot.</summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}" />, this property always returns false.</returns>
        [__DynamicallyInvokable]
        bool ICollection.IsSynchronized
        {
            [__DynamicallyInvokable]
            get
            {
                return false;
            }
        }
        /// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />. This property is not supported.</summary>
        /// <returns>Always returns null.</returns>
        /// <exception cref="T:System.NotSupportedException">The SyncRoot property is not supported.</exception>
        [__DynamicallyInvokable]
        object ICollection.SyncRoot
        {
            [__DynamicallyInvokable]
            get
            {
                throw new NotSupportedException("ConcurrentCollection_SyncRoot_NotSupported");
            }
        }
        private static int DefaultConcurrencyLevel
        {
            get
            {
                return 4 * PlatformHelper.ProcessorCount;
            }
        }
        private static bool IsValueWriteAtomic()
        {
            Type typeFromHandle = typeof(TValue);
            bool flag = typeFromHandle.IsClass || typeFromHandle == typeof(bool) || typeFromHandle == typeof(char) || typeFromHandle == typeof(byte) || typeFromHandle == typeof(sbyte) || typeFromHandle == typeof(short) || typeFromHandle == typeof(ushort) || typeFromHandle == typeof(int) || typeFromHandle == typeof(uint) || typeFromHandle == typeof(float);
            if (!flag && IntPtr.Size == 8)
            {
                flag |= (typeFromHandle == typeof(double) || typeFromHandle == typeof(long));
            }
            return flag;
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.</summary>
        [__DynamicallyInvokable]
        public ConcurrentDictionary()
            : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 31, true, EqualityComparer<TKey>.Default)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the specified concurrency level and capacity, and uses the default comparer for the key type.</summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="concurrencyLevel" /> is less than 1.-or-<paramref name="capacity" /> is less than 0.</exception>
        [__DynamicallyInvokable]
        public ConcurrentDictionary(int concurrencyLevel, int capacity)
            : this(concurrencyLevel, capacity, false, EqualityComparer<TKey>.Default)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" />, has the default concurrency level, has the default initial capacity, and uses the default comparer for the key type.</summary>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> or any of its keys is a null reference (Nothing in Visual Basic)</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="collection" /> contains one or more duplicate keys.</exception>
        [__DynamicallyInvokable]
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this(collection, EqualityComparer<TKey>.Default)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the default concurrency level and capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        [__DynamicallyInvokable]
        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
            : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 31, true, comparer)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" />, has the default concurrency level, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> is a null reference (Nothing in Visual Basic). -or- <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        [__DynamicallyInvokable]
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.InitializeFromCollection(collection);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable" />, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.</param>
        /// <param name="collection">The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="collection" /> is a null reference (Nothing in Visual Basic). -or- <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="concurrencyLevel" /> is less than 1.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="collection" /> contains one or more duplicate keys.</exception>
        [__DynamicallyInvokable]
        public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(concurrencyLevel, 31, false, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.InitializeFromCollection(collection);
        }
        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (KeyValuePair<TKey, TValue> current in collection)
            {
                if (current.Key == null)
                {
                    throw new ArgumentNullException("key");
                }
                TValue tValue;
                if (!this.TryAddInternal(current.Key, current.Value, false, false, out tValue))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_SourceContainsDuplicateKeys"));
                }
            }
            if (this.m_budget == 0)
            {
                this.m_budget = this.m_tables.m_buckets.Length / this.m_tables.m_locks.Length;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class that is empty, has the specified concurrency level, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" />.</summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.</param>
        /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{TKey}" /> implementation to use when comparing keys.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="comparer" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="concurrencyLevel" /> is less than 1. -or- <paramref name="capacity" /> is less than 0.</exception>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
            : this(concurrencyLevel, capacity, false, comparer)
        {
        }
        internal ConcurrentDictionary(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<TKey> comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel", this.GetResource("ConcurrentDictionary_ConcurrencyLevelMustBePositive"));
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", this.GetResource("ConcurrentDictionary_CapacityMustNotBeNegative"));
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            if (capacity < concurrencyLevel)
            {
                capacity = concurrencyLevel;
            }
            object[] array = new object[concurrencyLevel];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new object();
            }
            int[] countPerLock = new int[array.Length];
            ConcurrentDictionary<TKey, TValue>.Node[] array2 = new ConcurrentDictionary<TKey, TValue>.Node[capacity];
            this.m_tables = new ConcurrentDictionary<TKey, TValue>.Tables(array2, array, countPerLock);
            this.m_comparer = comparer;
            this.m_growLockArray = growLockArray;
            this.m_budget = array2.Length / array.Length;
        }
        /// <summary>Attempts to add the specified key and value to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
        /// <returns>true if the key/value pair was added to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> successfully. If the key already exists, this method returns false.</returns>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements, <see cref="F:System.Int32.MaxValue" />.</exception>
        [__DynamicallyInvokable]
        public bool TryAdd(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TValue tValue;
            return this.TryAddInternal(key, value, false, true, out tValue);
        }
        /// <summary>Determines whether the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> contains the specified key.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        [__DynamicallyInvokable]
        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TValue tValue;
            return this.TryGetValue(key, out tValue);
        }
        /// <summary>Attempts to remove and return the value with the specified key from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, <paramref name="value" /> contains the object removed from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> or the default value of  if the operation failed.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        [__DynamicallyInvokable]
        public bool TryRemove(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return this.TryRemoveInternal(key, out value, false, default(TValue));
        }
        private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
        {
            while (true)
            {
                ConcurrentDictionary<TKey, TValue>.Tables tables = this.m_tables;
                int num;
                int num2;
                this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out num, out num2, tables.m_buckets.Length, tables.m_locks.Length);
                lock (tables.m_locks[num2])
                {
                    if (tables != this.m_tables)
                    {
                        continue;
                    }
                    ConcurrentDictionary<TKey, TValue>.Node node = null;
                    ConcurrentDictionary<TKey, TValue>.Node node2 = tables.m_buckets[num];
                    while (node2 != null)
                    {
                        if (this.m_comparer.Equals(node2.m_key, key))
                        {
                            bool result;
                            if (matchValue && !EqualityComparer<TValue>.Default.Equals(oldValue, node2.m_value))
                            {
                                value = default(TValue);
                                result = false;
                                return result;
                            }
                            if (node == null)
                            {
                                Volatile.Write<ConcurrentDictionary<TKey, TValue>.Node>(ref tables.m_buckets[num], node2.m_next);
                            }
                            else
                            {
                                node.m_next = node2.m_next;
                            }
                            value = node2.m_value;
                            tables.m_countPerLock[num2]--;
                            result = true;
                            return result;
                        }
                        else
                        {
                            node = node2;
                            node2 = node2.m_next;
                        }
                    }
                }
                break;
            }
            value = default(TValue);
            return false;
        }
        /// <summary>Attempts to get the value associated with the specified key from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
        /// <returns>true if the key was found in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />; otherwise, false.</returns>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, <paramref name="value" /> contains the object from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> with the specified key or the default value of , if the operation failed.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        [__DynamicallyInvokable]
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            ConcurrentDictionary<TKey, TValue>.Tables tables = this.m_tables;
            int num;
            int num2;
            this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out num, out num2, tables.m_buckets.Length, tables.m_locks.Length);
            for (ConcurrentDictionary<TKey, TValue>.Node node = Volatile.Read<ConcurrentDictionary<TKey, TValue>.Node>(ref tables.m_buckets[num]); node != null; node = node.m_next)
            {
                if (this.m_comparer.Equals(node.m_key, key))
                {
                    value = node.m_value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }
        /// <summary>Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value.</summary>
        /// <returns>true if the value with <paramref name="key" /> was equal to <paramref name="comparisonValue" /> and replaced with <paramref name="newValue" />; otherwise, false.</returns>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue" /> and possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element with <paramref name="key" /> if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element with <paramref name="key" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference.</exception>
        [__DynamicallyInvokable]
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int hashCode = this.m_comparer.GetHashCode(key);
            IEqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
            bool result;
            while (true)
            {
                ConcurrentDictionary<TKey, TValue>.Tables tables = this.m_tables;
                int num;
                int num2;
                this.GetBucketAndLockNo(hashCode, out num, out num2, tables.m_buckets.Length, tables.m_locks.Length);
                lock (tables.m_locks[num2])
                {
                    if (tables != this.m_tables)
                    {
                        continue;
                    }
                    ConcurrentDictionary<TKey, TValue>.Node node = null;
                    ConcurrentDictionary<TKey, TValue>.Node node2 = tables.m_buckets[num];
                    while (node2 != null)
                    {
                        if (this.m_comparer.Equals(node2.m_key, key))
                        {
                            if (@default.Equals(node2.m_value, comparisonValue))
                            {
                                if (ConcurrentDictionary<TKey, TValue>.s_isValueWriteAtomic)
                                {
                                    node2.m_value = newValue;
                                }
                                else
                                {
                                    ConcurrentDictionary<TKey, TValue>.Node node3 = new ConcurrentDictionary<TKey, TValue>.Node(node2.m_key, newValue, hashCode, node2.m_next);
                                    if (node == null)
                                    {
                                        tables.m_buckets[num] = node3;
                                    }
                                    else
                                    {
                                        node.m_next = node3;
                                    }
                                }
                                result = true;
                                return result;
                            }
                            result = false;
                            return result;
                        }
                        else
                        {
                            node = node2;
                            node2 = node2.m_next;
                        }
                    }
                    result = false;
                }
                break;
            }
            return result;
        }
        /// <summary>Removes all keys and values from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
        [__DynamicallyInvokable]
        public void Clear()
        {
            int toExclusive = 0;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                ConcurrentDictionary<TKey, TValue>.Tables tables = new ConcurrentDictionary<TKey, TValue>.Tables(new ConcurrentDictionary<TKey, TValue>.Node[31], this.m_tables.m_locks, new int[this.m_tables.m_countPerLock.Length]);
                this.m_tables = tables;
                this.m_budget = Math.Max(1, tables.m_buckets.Length / tables.m_locks.Length);
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        [__DynamicallyInvokable]
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", this.GetResource("ConcurrentDictionary_IndexIsNegative"));
            }
            int toExclusive = 0;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                int num = 0;
                int num2 = 0;
                while (num2 < this.m_tables.m_locks.Length && num >= 0)
                {
                    num += this.m_tables.m_countPerLock[num2];
                    num2++;
                }
                if (array.Length - num < index || num < 0)
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayNotLargeEnough"));
                }
                this.CopyToPairs(array, index);
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        /// <summary>Copies the key and value pairs stored in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> to a new array.</summary>
        /// <returns>A new array containing a snapshot of key and value pairs copied from the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
        [__DynamicallyInvokable]
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            int toExclusive = 0;
            checked
            {
                KeyValuePair<TKey, TValue>[] result;
                try
                {
                    this.AcquireAllLocks(ref toExclusive);
                    int num = 0;
                    for (int i = 0; i < this.m_tables.m_locks.Length; i++)
                    {
                        num += this.m_tables.m_countPerLock[i];
                    }
                    KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[num];
                    this.CopyToPairs(array, 0);
                    result = array;
                }
                finally
                {
                    this.ReleaseLocks(0, toExclusive);
                }
                return result;
            }
        }
        private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_tables.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    index++;
                }
            }
        }
        private void CopyToEntries(DictionaryEntry[] array, int index)
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_tables.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new DictionaryEntry(node.m_key, node.m_value);
                    index++;
                }
            }
        }
        private void CopyToObjects(object[] array, int index)
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_tables.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    index++;
                }
            }
        }
        /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
        /// <returns>An enumerator for the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
        [__DynamicallyInvokable]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = this.m_tables.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (ConcurrentDictionary<TKey, TValue>.Node node = Volatile.Read<ConcurrentDictionary<TKey, TValue>.Node>(ref buckets[i]); node != null; node = node.m_next)
                {
                    yield return new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                }
            }
            yield break;
        }
        private bool TryAddInternal(TKey key, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue)
        {
            int hashCode = this.m_comparer.GetHashCode(key);
            checked
            {
                ConcurrentDictionary<TKey, TValue>.Tables tables;
                bool flag;
                while (true)
                {
                    tables = this.m_tables;
                    int num;
                    int num2;
                    this.GetBucketAndLockNo(hashCode, out num, out num2, tables.m_buckets.Length, tables.m_locks.Length);
                    flag = false;
                    bool flag2 = false;
                    try
                    {
                        if (acquireLock)
                        {
                            Monitor.Enter(tables.m_locks[num2]);
                            flag2 = true;
                        }
                        if (tables != this.m_tables)
                        {
                            continue;
                        }
                        ConcurrentDictionary<TKey, TValue>.Node node = null;
                        for (ConcurrentDictionary<TKey, TValue>.Node node2 = tables.m_buckets[num]; node2 != null; node2 = node2.m_next)
                        {
                            if (this.m_comparer.Equals(node2.m_key, key))
                            {
                                if (updateIfExists)
                                {
                                    if (ConcurrentDictionary<TKey, TValue>.s_isValueWriteAtomic)
                                    {
                                        node2.m_value = value;
                                    }
                                    else
                                    {
                                        ConcurrentDictionary<TKey, TValue>.Node node3 = new ConcurrentDictionary<TKey, TValue>.Node(node2.m_key, value, hashCode, node2.m_next);
                                        if (node == null)
                                        {
                                            tables.m_buckets[num] = node3;
                                        }
                                        else
                                        {
                                            node.m_next = node3;
                                        }
                                    }
                                    resultingValue = value;
                                }
                                else
                                {
                                    resultingValue = node2.m_value;
                                }
                                return false;
                            }
                            node = node2;
                        }
                        Volatile.Write<ConcurrentDictionary<TKey, TValue>.Node>(ref tables.m_buckets[num], new ConcurrentDictionary<TKey, TValue>.Node(key, value, hashCode, tables.m_buckets[num]));
                        tables.m_countPerLock[num2]++;
                        if (tables.m_countPerLock[num2] > this.m_budget)
                        {
                            flag = true;
                        }
                    }
                    finally
                    {
                        if (flag2)
                        {
                            Monitor.Exit(tables.m_locks[num2]);
                        }
                    }
                    break;
                }
                if (flag)
                {
                    this.GrowTable(tables);
                }
                resultingValue = value;
                return true;
            }
        }
        /// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key does not already exist.</summary>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value for the key as returned by valueFactory if the key was not in the dictionary.</returns>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).-or-<paramref name="valueFactory" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements, <see cref="F:System.Int32.MaxValue" />.</exception>
        [__DynamicallyInvokable]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }
            TValue result;
            if (this.TryGetValue(key, out result))
            {
                return result;
            }
            this.TryAddInternal(key, valueFactory(key), false, true, out result);
            return result;
        }
        /// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key does not already exist.</summary>
        /// <returns>The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements, <see cref="F:System.Int32.MaxValue" />.</exception>
        [__DynamicallyInvokable]
        public TValue GetOrAdd(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            TValue result;
            this.TryAddInternal(key, value, false, true, out result);
            return result;
        }
        /// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key does not already exist, or updates a key/value pair in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key already exists.</summary>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).-or-<paramref name="addValueFactory" /> is a null reference (Nothing in Visual Basic).-or-<paramref name="updateValueFactory" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements, <see cref="F:System.Int32.MaxValue" />.</exception>
        [__DynamicallyInvokable]
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (addValueFactory == null)
            {
                throw new ArgumentNullException("addValueFactory");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            TValue tValue2;
            while (true)
            {
                TValue tValue;
                if (this.TryGetValue(key, out tValue))
                {
                    tValue2 = updateValueFactory(key, tValue);
                    if (this.TryUpdate(key, tValue2, tValue))
                    {
                        break;
                    }
                }
                else
                {
                    tValue2 = addValueFactory(key);
                    TValue result;
                    if (this.TryAddInternal(key, tValue2, false, true, out result))
                    {
                        return result;
                    }
                }
            }
            return tValue2;
        }
        /// <summary>Adds a key/value pair to the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key does not already exist, or updates a key/value pair in the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> if the key already exists.</summary>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).-or-<paramref name="updateValueFactory" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements, <see cref="F:System.Int32.MaxValue" />.</exception>
        [__DynamicallyInvokable]
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            TValue tValue2;
            while (true)
            {
                TValue tValue;
                if (this.TryGetValue(key, out tValue))
                {
                    tValue2 = updateValueFactory(key, tValue);
                    if (this.TryUpdate(key, tValue2, tValue))
                    {
                        break;
                    }
                }
                else
                {
                    TValue result;
                    if (this.TryAddInternal(key, addValue, false, true, out result))
                    {
                        return result;
                    }
                }
            }
            return tValue2;
        }
        [__DynamicallyInvokable]
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            if (!this.TryAdd(key, value))
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_KeyAlreadyExisted"));
            }
        }
        [__DynamicallyInvokable]
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            TValue tValue;
            return this.TryRemove(key, out tValue);
        }
        [__DynamicallyInvokable]
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ((IDictionary<TKey, TValue>)this).Add(keyValuePair.Key, keyValuePair.Value);
        }
        [__DynamicallyInvokable]
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue x;
            return this.TryGetValue(keyValuePair.Key, out x) && EqualityComparer<TValue>.Default.Equals(x, keyValuePair.Value);
        }
        [__DynamicallyInvokable]
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            if (keyValuePair.Key == null)
            {
                throw new ArgumentNullException(this.GetResource("ConcurrentDictionary_ItemKeyIsNull"));
            }
            TValue tValue;
            return this.TryRemoveInternal(keyValuePair.Key, out tValue, true, keyValuePair.Value);
        }
        /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</summary>
        /// <returns>An enumerator for the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.</returns>
        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        /// <summary>Adds the specified key and value to the dictionary.</summary>
        /// <param name="key">The object to use as the key.</param>
        /// <param name="value">The object to use as the value.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="key" /> is of a type that is not assignable to the key type  of the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />. -or- <paramref name="value" /> is of a type that is not assignable to , the type of values in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />. -or- A value with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary{TKey,TValue}" />.</exception>
        /// <exception cref="T:System.OverflowException">The dictionary already contains the maximum number of elements, <see cref="F:System.Int32.MaxValue" />.</exception>
        [__DynamicallyInvokable]
        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!(key is TKey))
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfKeyIncorrect"));
            }
            TValue value2;
            try
            {
                value2 = (TValue)((object)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfValueIncorrect"));
            }
            ((IDictionary<TKey, TValue>)this).Add((TKey)((object)key), value2);
        }
        /// <summary>Gets whether the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the specified key.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" /> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        [__DynamicallyInvokable]
        bool IDictionary.Contains(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return key is TKey && this.ContainsKey((TKey)((object)key));
        }
        /// <summary>Provides an <see cref="T:System.Collections.Generics.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generics.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.Generic.IDictionary{TKey,TValue}" />.</returns>
        [__DynamicallyInvokable]
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new ConcurrentDictionary<TKey, TValue>.DictionaryEnumerator(this);
        }
        /// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" />.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is a null reference (Nothing in Visual Basic).</exception>
        [__DynamicallyInvokable]
        void IDictionary.Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key is TKey)
            {
                TValue tValue;
                this.TryRemove((TKey)((object)key), out tValue);
            }
        }
        /// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an array, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.ICollection" />. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" />. -or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
        [__DynamicallyInvokable]
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", this.GetResource("ConcurrentDictionary_IndexIsNegative"));
            }
            int toExclusive = 0;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                ConcurrentDictionary<TKey, TValue>.Tables tables = this.m_tables;
                int num = 0;
                int num2 = 0;
                while (num2 < tables.m_locks.Length && num >= 0)
                {
                    num += tables.m_countPerLock[num2];
                    num2++;
                }
                if (array.Length - num < index || num < 0)
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayNotLargeEnough"));
                }
                KeyValuePair<TKey, TValue>[] array2 = array as KeyValuePair<TKey, TValue>[];
                if (array2 != null)
                {
                    this.CopyToPairs(array2, index);
                }
                else
                {
                    DictionaryEntry[] array3 = array as DictionaryEntry[];
                    if (array3 != null)
                    {
                        this.CopyToEntries(array3, index);
                    }
                    else
                    {
                        object[] array4 = array as object[];
                        if (array4 == null)
                        {
                            throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayIncorrectType"), "array");
                        }
                        this.CopyToObjects(array4, index);
                    }
                }
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        private void GrowTable(ConcurrentDictionary<TKey, TValue>.Tables tables)
        {
            int toExclusive = 0;
            try
            {
                this.AcquireLocks(0, 1, ref toExclusive);
                if (tables == this.m_tables)
                {
                    long num = 0L;
                    for (int i = 0; i < tables.m_countPerLock.Length; i++)
                    {
                        num += (long)tables.m_countPerLock[i];
                    }
                    if (num < (long)(tables.m_buckets.Length / 4))
                    {
                        this.m_budget = 2 * this.m_budget;
                        if (this.m_budget < 0)
                        {
                            this.m_budget = 2147483647;
                        }
                    }
                    else
                    {
                        int num2 = 0;
                        bool flag = false;
                        object[] array;
                        checked
                        {
                            try
                            {
                                num2 = tables.m_buckets.Length * 2 + 1;
                                while (num2 % 3 == 0 || num2 % 5 == 0 || num2 % 7 == 0)
                                {
                                    num2 += 2;
                                }
                                if (num2 > 2146435071)
                                {
                                    flag = true;
                                }
                            }
                            catch (OverflowException)
                            {
                                flag = true;
                            }
                            if (flag)
                            {
                                num2 = 2146435071;
                                this.m_budget = 2147483647;
                            }
                            this.AcquireLocks(1, tables.m_locks.Length, ref toExclusive);
                            array = tables.m_locks;
                        }
                        if (this.m_growLockArray && tables.m_locks.Length < 1024)
                        {
                            array = new object[tables.m_locks.Length * 2];
                            Array.Copy(tables.m_locks, array, tables.m_locks.Length);
                            for (int j = tables.m_locks.Length; j < array.Length; j++)
                            {
                                array[j] = new object();
                            }
                        }
                        ConcurrentDictionary<TKey, TValue>.Node[] array2 = new ConcurrentDictionary<TKey, TValue>.Node[num2];
                        int[] array3 = new int[array.Length];
                        for (int k = 0; k < tables.m_buckets.Length; k++)
                        {
                            checked
                            {
                                ConcurrentDictionary<TKey, TValue>.Node next;
                                for (ConcurrentDictionary<TKey, TValue>.Node node = tables.m_buckets[k]; node != null; node = next)
                                {
                                    next = node.m_next;
                                    int num3;
                                    int num4;
                                    this.GetBucketAndLockNo(node.m_hashcode, out num3, out num4, array2.Length, array.Length);
                                    array2[num3] = new ConcurrentDictionary<TKey, TValue>.Node(node.m_key, node.m_value, node.m_hashcode, array2[num3]);
                                    array3[num4]++;
                                }
                            }
                        }
                        this.m_budget = Math.Max(1, array2.Length / array.Length);
                        this.m_tables = new ConcurrentDictionary<TKey, TValue>.Tables(array2, array, array3);
                    }
                }
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
        }
        private void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
        {
            bucketNo = (hashcode & 2147483647) % bucketCount;
            lockNo = bucketNo % lockCount;
        }
        private void AcquireAllLocks(ref int locksAcquired)
        {
            //if (CDSCollectionETWBCLProvider.Log.IsEnabled())
            //{
            //    CDSCollectionETWBCLProvider.Log.ConcurrentDictionary_AcquiringAllLocks(this.m_tables.m_buckets.Length);
            //}
            this.AcquireLocks(0, 1, ref locksAcquired);
            this.AcquireLocks(1, this.m_tables.m_locks.Length, ref locksAcquired);
        }
        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
        {
            object[] locks = this.m_tables.m_locks;
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                bool flag = false;
                try
                {
                    Monitor.Enter(locks[i]);
                    flag = true;
                }
                finally
                {
                    if (flag)
                    {
                        locksAcquired++;
                    }
                }
            }
        }
        private void ReleaseLocks(int fromInclusive, int toExclusive)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                Monitor.Exit(this.m_tables.m_locks[i]);
            }
        }
        private ReadOnlyCollection<TKey> GetKeys()
        {
            int toExclusive = 0;
            ReadOnlyCollection<TKey> result;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                List<TKey> list = new List<TKey>();
                for (int i = 0; i < this.m_tables.m_buckets.Length; i++)
                {
                    for (ConcurrentDictionary<TKey, TValue>.Node node = this.m_tables.m_buckets[i]; node != null; node = node.m_next)
                    {
                        list.Add(node.m_key);
                    }
                }
                result = new ReadOnlyCollection<TKey>(list);
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
            return result;
        }
        private ReadOnlyCollection<TValue> GetValues()
        {
            int toExclusive = 0;
            ReadOnlyCollection<TValue> result;
            try
            {
                this.AcquireAllLocks(ref toExclusive);
                List<TValue> list = new List<TValue>();
                for (int i = 0; i < this.m_tables.m_buckets.Length; i++)
                {
                    for (ConcurrentDictionary<TKey, TValue>.Node node = this.m_tables.m_buckets[i]; node != null; node = node.m_next)
                    {
                        list.Add(node.m_value);
                    }
                }
                result = new ReadOnlyCollection<TValue>(list);
            }
            finally
            {
                this.ReleaseLocks(0, toExclusive);
            }
            return result;
        }
        [Conditional("DEBUG")]
        private void Assert(bool condition)
        {
        }
        private string GetResource(string key)
        {
            return key;
        }
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            ConcurrentDictionary<TKey, TValue>.Tables tables = this.m_tables;
            this.m_serializationArray = this.ToArray();
            this.m_serializationConcurrencyLevel = tables.m_locks.Length;
            this.m_serializationCapacity = tables.m_buckets.Length;
        }
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            KeyValuePair<TKey, TValue>[] serializationArray = this.m_serializationArray;
            ConcurrentDictionary<TKey, TValue>.Node[] buckets = new ConcurrentDictionary<TKey, TValue>.Node[this.m_serializationCapacity];
            int[] countPerLock = new int[this.m_serializationConcurrencyLevel];
            object[] array = new object[this.m_serializationConcurrencyLevel];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new object();
            }
            this.m_tables = new ConcurrentDictionary<TKey, TValue>.Tables(buckets, array, countPerLock);
            this.InitializeFromCollection(serializationArray);
            this.m_serializationArray = null;
        }
    }
}

#endif