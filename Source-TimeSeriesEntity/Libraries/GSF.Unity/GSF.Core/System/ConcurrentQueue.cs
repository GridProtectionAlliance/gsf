#if MONO
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections.Concurrent
{
    internal struct VolatileBool
	{
		public volatile bool m_value;
		public VolatileBool(bool value)
		{
			this.m_value = value;
		}
	}

	/// <summary>Defines methods to manipulate thread-safe collections intended for producer/consumer usage. This interface provides a unified representation for producer/consumer collections so that higher level abstractions such as <see cref="T:System.Collections.Concurrent.BlockingCollection`1" /> can use the collection as the underlying storage mechanism.</summary>
	/// <typeparam name="T">Specifies the type of elements in the collection.</typeparam>
	[__DynamicallyInvokable]
	public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection, IEnumerable
	{
		/// <summary>Copies the elements of the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" /> to an <see cref="T:System.Array" />, starting at a specified index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than zero.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- The number of elements in the collection is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. </exception>
		[__DynamicallyInvokable]
		void CopyTo(T[] array, int index);
		/// <summary>Attempts to add an object to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</summary>
		/// <returns>true if the object was added successfully; otherwise, false.</returns>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</param>
		/// <exception cref="T:System.ArgumentException">The <paramref name="item" /> was invalid for this collection.</exception>
		[__DynamicallyInvokable]
		bool TryAdd(T item);
		/// <summary>Attempts to remove and return an object from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</summary>
		/// <returns>true if an object was removed and returned successfully; otherwise, false.</returns>
		/// <param name="item">When this method returns, if the object was removed and returned successfully, <paramref name="item" /> contains the removed object. If no object was available to be removed, the value is unspecified.</param>
		[__DynamicallyInvokable]
		bool TryTake(out T item);
		/// <summary>Copies the elements contained in the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" /> to a new array.</summary>
		/// <returns>A new array containing the elements copied from the <see cref="T:System.Collections.Concurrent.IProducerConsumerCollection`1" />.</returns>
		[__DynamicallyInvokable]
		T[] ToArray();
	}


    /// <summary>Represents a thread-safe first in-first out (FIFO) collection.</summary>
    /// <typeparam name="T">The type of the elements contained in the queue.</typeparam>
    [__DynamicallyInvokable, DebuggerDisplay("Count = {Count}"), ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    [Serializable]
    public class ConcurrentQueue<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private class Segment
        {
            internal volatile T[] m_array;
            internal volatile VolatileBool[] m_state;
            private volatile ConcurrentQueue<T>.Segment m_next;
            internal readonly long m_index;
            private volatile int m_low;
            private volatile int m_high;
            private volatile ConcurrentQueue<T> m_source;
            internal ConcurrentQueue<T>.Segment Next
            {
                get
                {
                    return this.m_next;
                }
            }
            internal bool IsEmpty
            {
                get
                {
                    return this.Low > this.High;
                }
            }
            internal int Low
            {
                get
                {
                    return Math.Min(this.m_low, 32);
                }
            }
            internal int High
            {
                get
                {
                    return Math.Min(this.m_high, 31);
                }
            }
            internal Segment(long index, ConcurrentQueue<T> source)
            {
                this.m_array = new T[32];
                this.m_state = new VolatileBool[32];
                this.m_high = -1;
                this.m_index = index;
                this.m_source = source;
            }
            internal void UnsafeAdd(T value)
            {
                this.m_high++;
                this.m_array[this.m_high] = value;
                this.m_state[this.m_high].m_value = true;
            }
            internal ConcurrentQueue<T>.Segment UnsafeGrow()
            {
                ConcurrentQueue<T>.Segment segment = new ConcurrentQueue<T>.Segment(this.m_index + 1L, this.m_source);
                this.m_next = segment;
                return segment;
            }
            internal void Grow()
            {
                ConcurrentQueue<T>.Segment next = new ConcurrentQueue<T>.Segment(this.m_index + 1L, this.m_source);
                this.m_next = next;
                this.m_source.m_tail = this.m_next;
            }
            internal bool TryAppend(T value)
            {
                if (this.m_high >= 31)
                {
                    return false;
                }
                int num = 32;
                try
                {
                }
                finally
                {
                    num = Interlocked.Increment(ref this.m_high);
                    if (num <= 31)
                    {
                        this.m_array[num] = value;
                        this.m_state[num].m_value = true;
                    }
                    if (num == 31)
                    {
                        this.Grow();
                    }
                }
                return num <= 31;
            }
            internal bool TryRemove(out T result)
            {
                SpinWait spinWait = default(SpinWait);
                int i = this.Low;
                int high = this.High;
                while (i <= high)
                {
                    if (Interlocked.CompareExchange(ref this.m_low, i + 1, i) == i)
                    {
                        SpinWait spinWait2 = default(SpinWait);
                        while (!this.m_state[i].m_value)
                        {
                            spinWait2.SpinOnce();
                        }
                        result = this.m_array[i];
                        if (this.m_source.m_numSnapshotTakers <= 0)
                        {
                            this.m_array[i] = default(T);
                        }
                        if (i + 1 >= 32)
                        {
                            spinWait2 = default(SpinWait);
                            while (this.m_next == null)
                            {
                                spinWait2.SpinOnce();
                            }
                            this.m_source.m_head = this.m_next;
                        }
                        return true;
                    }
                    spinWait.SpinOnce();
                    i = this.Low;
                    high = this.High;
                }
                result = default(T);
                return false;
            }
            internal bool TryPeek(out T result)
            {
                result = default(T);
                int low = this.Low;
                if (low > this.High)
                {
                    return false;
                }
                SpinWait spinWait = default(SpinWait);
                while (!this.m_state[low].m_value)
                {
                    spinWait.SpinOnce();
                }
                result = this.m_array[low];
                return true;
            }
            internal void AddToList(List<T> list, int start, int end)
            {
                for (int i = start; i <= end; i++)
                {
                    SpinWait spinWait = default(SpinWait);
                    while (!this.m_state[i].m_value)
                    {
                        spinWait.SpinOnce();
                    }
                    list.Add(this.m_array[i]);
                }
            }
        }
        [NonSerialized]
        private volatile ConcurrentQueue<T>.Segment m_head;
        [NonSerialized]
        private volatile ConcurrentQueue<T>.Segment m_tail;
        private T[] m_serializationArray;
        [NonSerialized]
        internal volatile int m_numSnapshotTakers;
        private const int SEGMENT_SIZE = 32;
        /// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot.</summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized with the SyncRoot; otherwise, false. For <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />, this property always returns false.</returns>
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
        /// <returns>Returns null  (Nothing in Visual Basic).</returns>
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
        /// <summary>Gets a value that indicates whether the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> is empty.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> is empty; otherwise, false.</returns>
        [__DynamicallyInvokable]
        public bool IsEmpty
        {
            [__DynamicallyInvokable]
            get
            {
                ConcurrentQueue<T>.Segment head = this.m_head;
                if (!head.IsEmpty)
                {
                    return false;
                }
                if (head.Next == null)
                {
                    return true;
                }
                SpinWait spinWait = default(SpinWait);
                while (head.IsEmpty)
                {
                    if (head.Next == null)
                    {
                        return true;
                    }
                    spinWait.SpinOnce();
                    head = this.m_head;
                }
                return false;
            }
        }
        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</returns>
        [__DynamicallyInvokable]
        public int Count
        {
            [__DynamicallyInvokable]
            get
            {
                ConcurrentQueue<T>.Segment segment;
                ConcurrentQueue<T>.Segment segment2;
                int num;
                int num2;
                this.GetHeadTailPositions(out segment, out segment2, out num, out num2);
                if (segment == segment2)
                {
                    return num2 - num + 1;
                }
                int num3 = 32 - num;
                num3 += 32 * (int)(segment2.m_index - segment.m_index - 1L);
                return num3 + (num2 + 1);
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> class.</summary>
        [__DynamicallyInvokable]
        public ConcurrentQueue()
        {
            this.m_head = (this.m_tail = new ConcurrentQueue<T>.Segment(0L, this));
        }
        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            ConcurrentQueue<T>.Segment segment = new ConcurrentQueue<T>.Segment(0L, this);
            this.m_head = segment;
            int num = 0;
            foreach (T current in collection)
            {
                segment.UnsafeAdd(current);
                num++;
                if (num >= 32)
                {
                    segment = segment.UnsafeGrow();
                    num = 0;
                }
            }
            this.m_tail = segment;
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> class that contains elements copied from the specified collection</summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> argument is null.</exception>
        [__DynamicallyInvokable]
        public ConcurrentQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.InitializeFromCollection(collection);
        }
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.m_serializationArray = this.ToArray();
        }
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.InitializeFromCollection(this.m_serializationArray);
            this.m_serializationArray = null;
        }
        /// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentBag" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="array" /> is multidimensional. -or- <paramref name="array" /> does not have zero-based indexing. -or- <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />. -or- The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
        [__DynamicallyInvokable]
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ((ICollection)this.ToList()).CopyTo(array, index);
        }
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
        [__DynamicallyInvokable]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
        [__DynamicallyInvokable]
        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            this.Enqueue(item);
            return true;
        }
        [__DynamicallyInvokable]
        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return this.TryDequeue(out item);
        }
        /// <summary>Copies the elements stored in the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> to a new array.</summary>
        /// <returns>A new array containing a snapshot of elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</returns>
        [__DynamicallyInvokable]
        public T[] ToArray()
        {
            return this.ToList().ToArray();
        }
        private List<T> ToList()
        {
            Interlocked.Increment(ref this.m_numSnapshotTakers);
            List<T> list = new List<T>();
            try
            {
                ConcurrentQueue<T>.Segment segment;
                ConcurrentQueue<T>.Segment segment2;
                int start;
                int end;
                this.GetHeadTailPositions(out segment, out segment2, out start, out end);
                if (segment == segment2)
                {
                    segment.AddToList(list, start, end);
                }
                else
                {
                    segment.AddToList(list, start, 31);
                    for (ConcurrentQueue<T>.Segment next = segment.Next; next != segment2; next = next.Next)
                    {
                        next.AddToList(list, 0, 31);
                    }
                    segment2.AddToList(list, 0, end);
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.m_numSnapshotTakers);
            }
            return list;
        }
        private void GetHeadTailPositions(out ConcurrentQueue<T>.Segment head, out ConcurrentQueue<T>.Segment tail, out int headLow, out int tailHigh)
        {
            head = this.m_head;
            tail = this.m_tail;
            headLow = head.Low;
            tailHigh = tail.High;
            SpinWait spinWait = default(SpinWait);
            while (head != this.m_head || tail != this.m_tail || headLow != head.Low || tailHigh != tail.High || head.m_index > tail.m_index)
            {
                spinWait.SpinOnce();
                head = this.m_head;
                tail = this.m_tail;
                headLow = head.Low;
                tailHigh = tail.High;
            }
        }
        /// <summary>Copies the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="array" /> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="index" /> is equal to or greater than the length of the <paramref name="array" /> -or- The number of elements in the source <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
        [__DynamicallyInvokable]
        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            this.ToList().CopyTo(array, index);
        }
        /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</summary>
        /// <returns>An enumerator for the contents of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</returns>
        [__DynamicallyInvokable]
        public IEnumerator<T> GetEnumerator()
        {
            Interlocked.Increment(ref this.m_numSnapshotTakers);
            ConcurrentQueue<T>.Segment head;
            ConcurrentQueue<T>.Segment tail;
            int headLow;
            int tailHigh;
            this.GetHeadTailPositions(out head, out tail, out headLow, out tailHigh);
            return this.GetEnumerator(head, tail, headLow, tailHigh);
        }
        private IEnumerator<T> GetEnumerator(ConcurrentQueue<T>.Segment head, ConcurrentQueue<T>.Segment tail, int headLow, int tailHigh)
        {
            try
            {
                SpinWait spinWait = default(SpinWait);
                if (head == tail)
                {
                    for (int i = headLow; i <= tailHigh; i++)
                    {
                        spinWait.Reset();
                        while (!head.m_state[i].m_value)
                        {
                            spinWait.SpinOnce();
                        }
                        yield return head.m_array[i];
                    }
                }
                else
                {
                    for (int j = headLow; j < 32; j++)
                    {
                        spinWait.Reset();
                        while (!head.m_state[j].m_value)
                        {
                            spinWait.SpinOnce();
                        }
                        yield return head.m_array[j];
                    }
                    for (ConcurrentQueue<T>.Segment next = head.Next; next != tail; next = next.Next)
                    {
                        for (int k = 0; k < 32; k++)
                        {
                            spinWait.Reset();
                            while (!next.m_state[k].m_value)
                            {
                                spinWait.SpinOnce();
                            }
                            yield return next.m_array[k];
                        }
                    }
                    for (int l = 0; l <= tailHigh; l++)
                    {
                        spinWait.Reset();
                        while (!tail.m_state[l].m_value)
                        {
                            spinWait.SpinOnce();
                        }
                        yield return tail.m_array[l];
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.m_numSnapshotTakers);
            }
            yield break;
        }
        /// <summary>Adds an object to the end of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</summary>
        /// <param name="item">The object to add to the end of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
        [__DynamicallyInvokable]
        public void Enqueue(T item)
        {
            SpinWait spinWait = default(SpinWait);
            while (true)
            {
                ConcurrentQueue<T>.Segment tail = this.m_tail;
                if (tail.TryAppend(item))
                {
                    break;
                }
                spinWait.SpinOnce();
            }
        }
        /// <summary>Attempts to remove and return the object at the beginning of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" />.</summary>
        /// <returns>true if an element was removed and returned from the beginning of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> successfully; otherwise, false.</returns>
        /// <param name="result">When this method returns, if the operation was successful, <paramref name="result" /> contains the object removed. If no object was available to be removed, the value is unspecified.</param>
        [__DynamicallyInvokable]
        public bool TryDequeue(out T result)
        {
            while (!this.IsEmpty)
            {
                ConcurrentQueue<T>.Segment head = this.m_head;
                if (head.TryRemove(out result))
                {
                    return true;
                }
            }
            result = default(T);
            return false;
        }
        /// <summary>Attempts to return an object from the beginning of the <see cref="T:System.Collections.Concurrent.ConcurrentQueue`1" /> without removing it.</summary>
        /// <returns>true if and object was returned successfully; otherwise, false.</returns>
        /// <param name="result">When this method returns, <paramref name="result" /> contains an object from the beginning of the <see cref="T:System.Collections.Concurrent.ConccurrentQueue{T}" /> or an unspecified value if the operation failed.</param>
        [__DynamicallyInvokable]
        public bool TryPeek(out T result)
        {
            while (!this.IsEmpty)
            {
                ConcurrentQueue<T>.Segment head = this.m_head;
                if (head.TryPeek(out result))
                {
                    return true;
                }
            }
            result = default(T);
            return false;
        }
    }
}

#endif
