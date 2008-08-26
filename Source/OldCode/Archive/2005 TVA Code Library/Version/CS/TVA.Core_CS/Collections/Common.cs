using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
//using TVA.Math.Common;

//*******************************************************************************************************
//  TVA.Collections.Common.vb - Common Collection Functions
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/23/2003 - J. Ritchie Carroll
//       Generated original version of source code.
//  01/23/2005 - J. Ritchie Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Common).
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
	namespace Collections
	{
		
		/// <summary>Defines common global functions related to manipulation of collections.</summary>
		public sealed class Common
		{
			
			
			private Common()
			{
				
				// This class contains only global functions and is not meant to be instantiated.
				
			}
			
			/// <summary>Returns the smallest item from a list of parameters.</summary>
			public static object Minimum(params object[] itemList)
			{
				
				return Minimum((System.Collections.IEnumerable) itemList);
				
			}
			
			/// <summary>Returns the smallest item from a list of parameters.</summary>
			public static T Minimum<T>(params T[] itemList)
			{
				
				return Minimum<T>((IEnumerable<T>) itemList);
				
			}
			
			/// <summary>Returns the smallest item from the specified enumeration.</summary>
			public static T Minimum<T>(IEnumerable<T> items)
			{
				
				T minItem;
				
				System.Collections.IEnumerator with_1 = items.GetEnumerator();
				if (with_1.MoveNext())
				{
					minItem = with_1.Current;
					while (with_1.MoveNext())
					{
						if (Compare<T>(with_1.Current, minItem) < 0)
						{
							minItem = with_1.Current;
						}
					}
				}
				
				return minItem;
				
			}
			
			/// <summary>Returns the smallest item from the specified enumeration.</summary>
			public static object Minimum(IEnumerable items)
			{
				
				object minItem;
				
				System.Collections.IEnumerator with_1 = items.GetEnumerator();
				if (with_1.MoveNext())
				{
					minItem = with_1.Current;
					while (with_1.MoveNext())
					{
						if (Compare(with_1.Current, minItem) < 0)
						{
							minItem = with_1.Current;
						}
					}
				}
				
				return minItem;
				
			}
			
			/// <summary>Returns the largest item from a list of parameters.</summary>
			public static object Maximum(params object[] itemList)
			{
				
				return Maximum((System.Collections.IEnumerable) itemList);
				
			}
			
			/// <summary>Returns the largest item from a list of parameters.</summary>
			public static T Maximum<T>(params T[] itemList)
			{
				
				return Maximum<T>((IEnumerable<T>) itemList);
				
			}
			
			/// <summary>Returns the largest item from the specified enumeration.</summary>
			public static T Maximum<T>(IEnumerable<T> items)
			{
				
				T maxItem;
				
				System.Collections.IEnumerator with_1 = items.GetEnumerator();
				if (with_1.MoveNext())
				{
					maxItem = with_1.Current;
					while (with_1.MoveNext())
					{
						if (Compare<T>(with_1.Current, maxItem) > 0)
						{
							maxItem = with_1.Current;
						}
					}
				}
				
				return maxItem;
				
			}
			
			/// <summary>Returns the largest item from the specified enumeration.</summary>
			public static object Maximum(IEnumerable items)
			{
				
				object maxItem;
				
				System.Collections.IEnumerator with_1 = items.GetEnumerator();
				if (with_1.MoveNext())
				{
					maxItem = with_1.Current;
					while (with_1.MoveNext())
					{
						if (Compare(with_1.Current, maxItem) > 0)
						{
							maxItem = with_1.Current;
						}
					}
				}
				
				return maxItem;
				
			}
			
			/// <summary>Compares two elements of the specified type.</summary>
			public static int Compare<T>(T x, T y)
			{
				
				return System.Collections.Generic.Comparer<T>.Default.Compare(x, y);
				
			}
			
			/// <summary>Compares two elements of any type.</summary>
			public static int Compare(object x, object y)
			{
				
				if (Information.IsReference(x) && Information.IsReference(y))
				{
					// If both items are reference objects, then it tests object equality by reference.
					// If not equal by overridable Object.Equals function, use default Comparer.
					if (x == y)
					{
						return 0;
					}
					else if (x.GetType().Equals(y.GetType()))
					{
						// Compares two items that are the same type. Sees if the type supports IComparable interface.
						if (x is IComparable)
						{
							return ((IComparable) x).CompareTo(y);
						}
						else if (x.Equals(y))
						{
							return 0;
						}
						else
						{
							return Comparer.Default.Compare(x, y);
						}
					}
					else
					{
						return Comparer.Default.Compare(x, y);
					}
				}
				else
				{
					// Compares non-reference (i.e., value) types, using VB rules.
					// ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.VisualStudio.v80.en/dv_vbalr/html/d6cb12a8-e52e-46a7-8aaf-f804d634a825.htm
					return (x < y ? - 1 : (x > y ? 1 : 0));
				}
				
			}
			
			/// <summary>Compares two arrays.</summary>
			public static int CompareArrays(Array arrayA, Array arrayB)
			{
				
				return CompareArrays(arrayA, arrayB, null);
				
			}
			
			/// <summary>Compares two arrays.</summary>
			public static int CompareArrays(Array arrayA, Array arrayB, IComparer comparer)
			{
				
				if (arrayA == null && arrayB == null)
				{
					return 0;
				}
				else if (arrayA == null)
				{
					return - 1;
				}
				else if (arrayB == null)
				{
					return 1;
				}
				else
				{
					if (arrayA.Rank == 1 && arrayB.Rank == 1)
					{
						if (arrayA.GetUpperBound(0) == arrayB.GetUpperBound(0))
						{
							int comparison;
							
							for (int x = 0; x <= arrayA.Length - 1; x++)
							{
								if (comparer == null)
								{
									comparison = Compare(arrayA.GetValue(x), arrayB.GetValue(x));
								}
								else
								{
									comparison = comparer.Compare(arrayA.GetValue(x), arrayB.GetValue(x));
								}
								
								if (comparison != 0)
								{
									break;
								}
							}
							
							return comparison;
						}
						else
						{
							// For arrays that do not have the same number of elements, the array with most elements
							// is assumed to be larger.
							return Compare(arrayA.GetUpperBound(0), arrayB.GetUpperBound(0));
						}
					}
					else
					{
						throw (new ArgumentException("Cannot compare multidimensional arrays"));
					}
				}
				
			}
			
			/// <summary>Changes the type of all elements in the source enumeration, and adds the conversion
			/// result to destination list.</summary>
			/// <remarks>Items in source enumeration that are converted are added to destination list. The \
			/// destination list is not cleared in advance.</remarks>
			public static void ConvertList(IEnumerable source, IList destination, System.Type toType)
			{
				
				if (source == null)
				{
					throw (new ArgumentNullException("Source list is null"));
				}
				if (destination == null)
				{
					throw (new ArgumentNullException("Destination list is null"));
				}
				if (destination.IsReadOnly)
				{
					throw (new ArgumentException("Cannot add items to a read only list"));
				}
				if (destination.IsFixedSize)
				{
					throw (new ArgumentException("Cannot add items to a fixed size list"));
				}
				
				foreach (object Item in source)
				{
					destination.Add(Convert.ChangeType(Item, toType));
				}
				
			}
			
			/// <summary>Converts a list (i.e., any collection implementing IList) to an array.</summary>
			public static Array ListToArray(IList sourceList, System.Type toType)
			{
				
				Array destination = Array.CreateInstance(toType, sourceList.Count);
				
				ConvertList(sourceList, destination, toType);
				
				return destination;
				
			}
			
			/// <summary>Converts an array to a string, using the default delimeter ("|") that can later be
			/// converted back to array using StringToArray.</summary>
			/// <remarks>
			/// This function is a semantic reference to the ListToString function (the Array class implements
			/// IEnumerable) and is only provided for the sake of completeness.
			/// </remarks>
			public static string ArrayToString(Array source)
			{
				
				return ListToString(source);
				
			}
			
			/// <summary>Converts an array to a string that can later be converted back to array using StringToArray.</summary>
			/// <remarks>
			/// This function is a semantic reference to the ListToString function (the Array class implements
			/// IEnumerable) and is only provided for the sake of completeness.
			/// </remarks>
			public static string ArrayToString(Array source, char delimeter)
			{
				
				return ListToString(source, delimeter);
				
			}
			
			/// <summary>Converts an enumeration to a string, using the default delimeter ("|") that can later be
			/// converted back to array using StringToList.</summary>
			public static string ListToString(IEnumerable source)
			{
				
				return ListToString(source, '|');
				
			}
			
			/// <summary>Converts an enumeration to a string that can later be converted back to array using
			/// StringToList.</summary>
			public static string ListToString(IEnumerable source, char delimeter)
			{
				
				if (source == null)
				{
					throw (new ArgumentNullException("Source list is null"));
				}
				
				System.Text.StringBuilder with_1 = new StringBuilder;
				foreach (object item in source)
				{
					if (with_1.Length > 0)
					{
						with_1.Append(delimeter);
					}
					with_1.Append(item.ToString());
				}
				
				return with_1.ToString();
				
			}
			
			/// <summary>Converts a string, created with ArrayToString, using the default delimeter ("|") back into
			/// an array.</summary>
			public static Array StringToArray(string source, System.Type toType)
			{
				
				return StringToArray(source, toType, '|');
				
			}
			
			/// <summary>Converts a string, created with ArrayToString, back into an array.</summary>
			public static Array StringToArray(string source, System.Type toType, char delimeter)
			{
				
				ArrayList items = new ArrayList();
				
				StringToList(source, items, delimeter);
				
				return ListToArray(items, toType);
				
			}
			
			/// <summary>Appends items parsed from delimited string, created with ArrayToString or ListToString,
			/// using the default delimeter ("|") into the given list.</summary>
			/// <remarks>Items that are converted are added to destination list. The destination list is not
			/// cleared in advance.</remarks>
			public static void StringToList(string source, IList destination)
			{
				
				StringToList(source, destination, '|');
				
			}
			
			/// <summary>Appends items parsed from delimited string, created with ArrayToString or ListToString,
			/// into the given list.</summary>
			/// <remarks>Items that are converted are added to destination list. The destination list is not
			/// cleared in advance.</remarks>
			public static void StringToList(string source, IList destination, char delimeter)
			{
				
				if (source == null)
				{
					return;
				}
				if (destination == null)
				{
					throw (new ArgumentNullException("Destination list is null"));
				}
				if (destination.IsFixedSize)
				{
					throw (new ArgumentException("Cannot add items to a fixed size list"));
				}
				if (destination.IsReadOnly)
				{
					throw (new ArgumentException("Cannot add items to a read only list"));
				}
				
				foreach (string item in source.Split(delimeter))
				{
					if (! string.IsNullOrEmpty(item))
					{
						item = item.Trim();
						if (item.Length > 0)
						{
							destination.Add(item);
						}
					}
				}
				
			}
			
			/// <summary>Rearranges all the elements in the array into a random order.</summary>
			/// <remarks>
			/// <para>
			/// This function is a semantic reference to the ScrambleList function (the Array class implements
			/// IList) and is only provided for the sake of completeness.
			/// </para>
			/// <para>This function uses a cryptographically strong random number generator to perform the scramble.</para>
			/// </remarks>
			public static void ScrambleArray(Array source)
			{
				
				ScrambleList(source);
				
			}
			
			/// <summary>Rearranges all the elements in the list (i.e., any collection implementing IList) into
			/// a random order.</summary>
			/// <remarks>This function uses a cryptographically strong random number generator to perform the
			/// scramble.</remarks>
			public static void ScrambleList(IList source)
			{
				
				if (source == null)
				{
					throw (new ArgumentNullException("Source list is null"));
				}
				if (source.IsReadOnly)
				{
					throw (new ArgumentException("Cannot modify items in a read only list"));
				}
				
				int x;
				int y;
				object currentItem;
				
				// Mixes up the data in random order.
				for (x = 0; x <= source.Count - 1; x++)
				{
					// Calls random function in Math namespace.
					y = TVA.Math.Common.RandomInt32Between(0, source.Count - 1);
					
					if (x != y)
					{
						// Swaps items
						currentItem = source[x];
						source[x] = source[y];
						source[y] = currentItem;
					}
				}
				
			}
			
		}
		
	}
	
}
