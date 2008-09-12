//*******************************************************************************************************
//  TVA.Collections.Extensions.vb - Defines extension functions related to manipulation of collections
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
//  09/11/2008 - J. Ritchie Carroll
//      Converted to C# extension functions.
//
//*******************************************************************************************************

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TVA;

namespace TVA
{
    namespace Collections
    {
        /// <summary>Defines extension functions related to manipulation of collections.</summary>
        public static class Extensions
        {
            /// <summary>Returns the smallest item from the enumeration.</summary>
            public static TSource Min<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, int> comparer)
            {
                TSource minItem = default(TSource);

                IEnumerator<TSource> enumerator = source.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    minItem = enumerator.Current;
                    while (enumerator.MoveNext())
                    {
                        if (comparer(enumerator.Current, minItem) < 0)
                            minItem = enumerator.Current;
                    }
                }

                return minItem;
            }

            /// <summary>Returns the smallest item from the enumeration.</summary>
            public static TSource Min<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
            {
                return source.Min<TSource>(comparer.Compare);
            }

            /// <summary>Returns the largest item from the enumeration.</summary>
            public static TSource Max<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, int> comparer)
            {
                TSource maxItem = default(TSource);

                IEnumerator<TSource> enumerator = source.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    maxItem = enumerator.Current;
                    while (enumerator.MoveNext())
                    {
                        if (comparer(enumerator.Current, maxItem) > 0)
                            maxItem = enumerator.Current;
                    }
                }

                return maxItem;
            }

            /// <summary>Returns the largest item from the enumeration.</summary>
            public static TSource Max<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
            {
                return source.Max<TSource>(comparer.Compare);
            }

            /// <summary>Converts an enumeration to a string, using the default delimeter ("|") that can later be
            /// converted back to a list using LoadDelimitedString.</summary>
            public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source)
            {
                return source.ToDelimitedString<TSource>('|');
            }

            /// <summary>Converts an enumeration to a string that can later be converted back to a list using
            /// LoadDelimitedString.</summary>
            public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source, char delimiter)
			{
                return ToDelimitedString<TSource, char>(source, delimiter);
			}

            /// <summary>Converts an enumeration to a string that can later be converted back to a list using
            /// LoadDelimitedString.</summary>
            public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source, string delimiter)
            {
                return ToDelimitedString<TSource, string>(source, delimiter);
            }

            private static string ToDelimitedString<TSource, TDelimiter>(IEnumerable<TSource> source, TDelimiter delimiter)
            {
                if (Common.IsReference(delimiter) && delimiter == null) throw new ArgumentNullException("delimiter", "delimiter cannot be null");

                StringBuilder delimetedString = new StringBuilder();

                foreach (TSource item in source)
                {
                    if (delimetedString.Length > 0) delimetedString.Append(delimiter);
                    delimetedString.Append(item.ToString());
                }

                return delimetedString.ToString();
            }

            /// <summary>Appends items parsed from delimited string, created with ToDelimitedString, using the default
            /// delimeter ("|") into the given list.</summary>
            /// <remarks>Items that are converted are added to list. The list is not cleared in advance.</remarks>
            public static void LoadDelimitedString<TSource>(this IList<TSource> destination, string delimitedString, Func<string, TSource> convertFromString)
            {
                destination.LoadDelimitedString(delimitedString, '|', convertFromString);
            }

            /// <summary>Appends items parsed from delimited string, created with ToDelimitedString, into the given list.</summary>
            /// <remarks>Items that are converted are added to list. The list is not cleared in advance.</remarks>
            public static void LoadDelimitedString<TSource>(this IList<TSource> destination, string delimitedString, char delimeter, Func<string, TSource> convertFromString)
            {
                if (delimitedString == null) throw new ArgumentNullException("delimitedString", "delimitedString cannot be null");
                if (destination.IsReadOnly) throw new ArgumentException("Cannot add items to a read only list");

                foreach (string item in delimitedString.Split(delimeter))
                {
                    destination.Add(convertFromString(item.Trim()));
                }
            }

            /// <summary>Appends items parsed from delimited string, created with ToDelimitedString, into the given list.</summary>
            /// <remarks>Items that are converted are added to list. The list is not cleared in advance.</remarks>
            public static void LoadDelimitedString<TSource>(this IList<TSource> destination, string delimitedString, string[] delimiters, Func<string, TSource> convertFromString)
            {
                if (delimiters == null) throw new ArgumentNullException("delimiters", "delimiters cannot be null");
                if (delimitedString == null) throw new ArgumentNullException("delimitedString", "delimitedString cannot be null");
                if (destination.IsReadOnly) throw new ArgumentException("Cannot add items to a read only list");

                foreach (string item in delimitedString.Split(delimiters, StringSplitOptions.None))
                {
                    destination.Add(convertFromString(item.Trim()));
                }
            }

            /// <summary>Rearranges all the elements in the list into a random order.</summary>
            /// <remarks>This function uses a cryptographically strong random number generator to perform the scramble.</remarks>
            public static void ScrambleList<TSource>(this IList<TSource> source)
            {
                if (source.IsReadOnly) throw new ArgumentException("Cannot modify items in a read only list");

                int x;
                int y;
                TSource currentItem;

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

            /// <summary>Compares two arrays.</summary>
            public static int CompareTo(this Array array1, Array array2)
            {
                return CompareTo(array1, array2, null);
            }

            /// <summary>Compares two arrays.</summary>
            public static int CompareTo(this Array array1, Array array2, IComparer comparer)
            {
                if (array1 == null && array2 == null)
                {
                    return 0;
                }
                else if (array1 == null)
                {
                    return -1;
                }
                else if (array2 == null)
                {
                    return 1;
                }
                else
                {
                    if (array1.Rank == 1 && array2.Rank == 1)
                    {
                        if (array1.GetUpperBound(0) == array2.GetUpperBound(0))
                        {
                            int comparison = 0;

                            for (int x = 0; x <= array1.Length - 1; x++)
                            {
                                if (comparer == null)
                                    comparison = Common.Compare(array1.GetValue(x), array2.GetValue(x));
                                else
                                    comparison = comparer.Compare(array1.GetValue(x), array2.GetValue(x));

                                if (comparison != 0)
                                    break;
                            }

                            return comparison;
                        }
                        else
                        {
                            // For arrays that do not have the same number of elements, the array with most elements
                            // is assumed to be larger.
                            return Common.Compare(array1.GetUpperBound(0), array2.GetUpperBound(0));
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Cannot compare multidimensional arrays");
                    }
                }
            }
        }
    }
}
