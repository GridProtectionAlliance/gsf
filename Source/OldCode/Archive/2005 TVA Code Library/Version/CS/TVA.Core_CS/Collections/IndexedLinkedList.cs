using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Collections.IndexedLinkedList.vb - Implements an indexed doubly linked list
//  Copyright © 2007 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/27/2007 - J. Ritchie Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace TVA
{
	namespace Collections
	{
		
		[Obsolete("This class is still under development", true)]public class IndexedLinkedList<T> : IList<T>, ICollection
		{
			
			
			
			private LinkedList<T> m_list;
			private LinkedListNode<T> m_currentNode;
			private int m_currentPosition;
			
			public IndexedLinkedList()
			{
				
				m_list = new LinkedList<T>;
				
			}
			
			public IndexedLinkedList(IEnumerable<T> collection)
			{
				
				m_list = new LinkedList<T>(collection);
				
			}
			
			public virtual int BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
			{
				
				if (comparer == null)
				{
					comparer = Generic.Comparer<T>.Default;
				}
				
				int startIndex = index;
				int stopIndex = (index + length) - 1;
				int result;
				int halfPoint;
				
				while (startIndex <= stopIndex)
				{
					halfPoint = startIndex + ((stopIndex - startIndex) >> 1);
					
					try
					{
						result = comparer.Compare(array[halfPoint], value);
					}
					catch (Exception Exception)
					{
						throw (new InvalidOperationException("InvalidOperation: IComparerFailed", Exception));
					}
					
					if (result == 0)
					{
						return halfPoint;
					}
					
					if (result < 0)
					{
						startIndex = halfPoint + 1;
					}
					else
					{
						stopIndex = halfPoint - 1;
					}
				}
				
				return ! startIndex;
				
			}
			
			#region " Generic IList(Of T) Implementation "
			
			/// <summary>Adds an item to the list.</summary>
			/// <param name="item">The item to add to the list.</param>
			public virtual void Add(T item)
			{
				
				m_list.AddLast(item);
				
			}
			
			/// <summary>Inserts an element into the list at the specified index.</summary>
			/// <param name="item">The object to insert. The value can be null for reference types.</param>
			/// <param name="index">The zero-based index at which item should be inserted.</param>
			/// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than list length.</exception>
			public virtual void Insert(int index, T item)
			{
				
				//m_processQueue.Insert(index, item)
				
			}
			
			/// <summary>Copies the entire list to a compatible one-dimensional array, starting at the beginning of the
			/// target array.</summary>
			/// <param name="array">The one-dimensional array that is the destination of the elements copied from list. The
			/// array must have zero-based indexing.</param>
			/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
			/// <exception cref="ArgumentException">arrayIndex is equal to or greater than the length of array -or- the
			/// number of elements in the source list is greater than the available space from arrayIndex to the end of the
			/// destination array.</exception>
			/// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
			/// <exception cref="ArgumentNullException">array is null.</exception>
			public virtual void CopyTo(T[] array, int arrayIndex)
			{
				
				m_list.CopyTo(array, arrayIndex);
				
			}
			
			/// <summary>Returns an enumerator that iterates through the list.</summary>
			/// <returns>An enumerator for the list.</returns>
			public virtual IEnumerator<T> GetEnumerator()
			{
				
				return m_list.GetEnumerator();
				
			}
			
			/// <summary>Gets or sets the element at the specified index.</summary>
			/// <returns>The element at the specified index.</returns>
			/// <param name="index">The zero-based index of the element to get or set.</param>
			/// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than
			/// list length. </exception>
			public virtual T this[int index]
			{
				get
				{
					//Return m_processQueue(index)
				}
				set
				{
					//m_processQueue(index) = value
				}
			}
			
			/// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within
			/// the entire list.</summary>
			/// <returns>The zero-based index of the first occurrence of item within the entire list, if found; otherwise, –1.</returns>
			/// <param name="item">The object to locate in the list. The value can be null for reference types.</param>
			public virtual int IndexOf(T item)
			{
				
				//Return m_processQueue.IndexOf(item)
				
			}
			
			/// <summary>Gets the number of elements actually contained in the list.</summary>
			/// <returns>The number of elements actually contained in the list.</returns>
			public virtual int Count
			{
				get
				{
					return m_list.Count;
				}
			}
			
			/// <summary>Removes all elements from the list.</summary>
			public virtual void Clear()
			{
				
				m_list.Clear();
				
			}
			
			/// <summary>Determines whether an element is in the list.</summary>
			/// <returns>True, if item is found in the list; otherwise, false.</returns>
			/// <param name="item">The object to locate in the list. The value can be null for reference types.</param>
			public virtual bool Contains(T item)
			{
				
				return m_list.Contains(item);
				
			}
			
			/// <summary>Removes the first occurrence of a specific object from the list.</summary>
			/// <returns>True, if item is successfully removed; otherwise, false. This method also returns false if item was
			/// not found in the list.</returns>
			/// <param name="item">The object to remove from the list. The value can be null for reference types.</param>
			public virtual bool Remove(T item)
			{
				
				m_list.Remove(item);
				
			}
			
			/// <summary>Removes the element at the specified index of the list.</summary>
			/// <param name="index">The zero-based index of the element to remove.</param>
			/// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is equal to or greater than
			/// list length.</exception>
			public virtual void RemoveAt(int index)
			{
				
				//m_processQueue.RemoveAt(index)
				
			}
			
			/// <summary>Gets a value indicating whether the list is read-only.</summary>
			/// <returns>True, if the list is read-only; otherwise, false. In the default implementation, this property
			/// always returns false.</returns>
			public virtual bool IsReadOnly
			{
				get
				{
					return false;
				}
			}
			
			#endregion
			
			#region " IEnumerable Implementation "
			
			/// <summary>
			/// Gets an enumerator of all items within the list.
			/// </summary>
			public IEnumerator GetEnumerator()
			{
				return this.IEnumerableGetEnumerator();
			}
			
			public IEnumerator IEnumerableGetEnumerator()
			{
				
				return ((System.Collections.IEnumerable) m_list).GetEnumerator();
				
			}
			
			#endregion
			
			#region " ICollection Implementation "
			
			/// <summary>Returns reference to internal IList that should be used to synchronize access to the list.</summary>
			/// <returns>Reference to internal IList that should be used to synchronize access to the list.</returns>
			/// <remarks>
			/// <para>
			/// Note that all the methods of this class are already individually synchronized; however, to safely enumerate
			/// through each list element (i.e., to make sure list elements do not change during enumeration), derived
			/// classes and end users should perform their own synchronization by implementing a SyncLock using this SyncRoot
			/// property.
			/// </para>
			/// <para>
			/// We return a typed object for synchronization as an optimization. Returning a generic object requires that
			/// SyncLock implementations validate that the referenced object is not a value type at run time.
			/// </para>
			/// </remarks>
			public IList<T> SyncRoot
			{
				get
				{
					return m_list;
				}
			}
			
			/// <summary>Gets an object that can be used to synchronize access to the list.</summary>
			/// <returns>An object that can be used to synchronize access to the list.</returns>
			/// <remarks>
			/// Note that all the methods of this class are already individually synchronized; however, to safely enumerate
			/// through each list element (i.e., to make sure list elements do not change during enumeration), derived
			/// classes and end users should perform their own synchronization by implementing a SyncLock using this SyncRoot
			/// property.
			/// </remarks>
			public object SyncRoot
			{
				get
				{
					return this.ICollectionSyncRoot;
				}
			}
			
			public object ICollectionSyncRoot
			{
				get
				{
					return m_list;
				}
			}
			
			/// <summary>Gets a value indicating whether access to the list is synchronized (thread safe).</summary>
			/// <returns>True, if access to the list is synchronized (thread safe); otherwise, false. In the default
			/// implementation, this property always returns false.</returns>
			public bool IsSynchronized
			{
				get
				{
					return false;
				}
			}
			
			public void CopyTo(System.Array array, int index)
			{
				this.ICollectionCopyTo(array, index);
			}
			
			public void ICollectionCopyTo(System.Array array, int index)
			{
				
				CopyTo(array, index);
				
			}
			
			public int Count
			{
				get
				{
					return this.ICollectionCount;
				}
			}
			
			public int ICollectionCount
			{
				get
				{
					return Count;
				}
			}
			
			#endregion
			
		}
		
	}
}
