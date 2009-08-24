//*******************************************************************************************************
//  CollectionExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/23/2003 - James R. Carroll
//       Generated original version of source code.
//  01/23/2005 - James R. Carroll
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Common).
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/11/2008 - James R. Carroll
//       Converted to C# extension functions.
//  02/17/2009 - Josh Patterson
//       Edited Code Comments.
//  02/20/2009 - James R. Carroll
//       Added predicate based IndexOf that extends IList<T>.
//  04/02/2009 - James R. Carroll
//       Added seed based scramble and unscramble IList<T> extensions.
//  06/05/2009 - Pinal C. Patel
//       Added generic AddRange() extension method for IList<T>.
//  06/09/2009 - Pinal C. Patel
//       Added generic GetRange() extension method for IList<T>.
//  08/05/2009 - Josh Patterson
//       Update comments
//
//*******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TVA.Collections
{
    /// <summary>
    /// Defines extension functions related to manipulation of arrays and collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds the specified <paramref name="items"/> to the <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of <paramref name="items"/> to be added.</typeparam>
        /// <param name="collection">The collection to which the <paramref name="items"/> are to be added.</param>
        /// <param name="items">The items to be added to the <paramref name="collection"/>.</param>
        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Returns elements in the specified range from the <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of elements in the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The collection from which elements are to be retrieved.</param>
        /// <param name="index">The 0-based index position in the <paramref name="collection"/> from which elements are to be retrieved.</param>
        /// <param name="count">The number of elements to be retrieved from the <paramref name="collection"/> starting at the <paramref name="index"/>.</param>
        /// <returns>An <see cref="IList{T}"/> object.</returns>
        public static IList<T> GetRange<T>(this IList<T> collection, int index, int count)
        {
            List<T> result = new List<T>();

            for (int i = index; i < index + count; i++)
			{
                result.Add(collection[i]);
			}

            return result;
        }

        /// <summary>
        /// Returns the index of the first element of the sequence that satisfies a condition or <c>-1</c> if no such element is found.
        /// </summary>
        /// <param name="source">A <see cref="IList{T}"/> to find an index in.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>Index of the first element in <paramref name="source"/> that matches the specified <paramref name="predicate"/>; otherwise, <c>-1</c>.</returns>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        public static int IndexOf<T>(this IList<T> source, Func<T, bool> predicate)
        {
            for (int index = 0; index < source.Count; index++)
            {
                if (predicate(source[index]))
                    return index;
            }

            return -1;
        }

        /// <summary>
        /// Returns a copy of the <see cref="Array"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the <see cref="Array"/> to be copied.</typeparam>
        /// <param name="source">The source <see cref="Array"/> whose elements are to be copied.</param>
        /// <param name="startIndex">The source array index from where the elements are to be copied.</param>
        /// <param name="length">The number of elements to be copied starting from the startIndex.</param>
        /// <returns>An <see cref="Array"/> of elements copied from the specified portion of the source <see cref="Array"/>.</returns>
        /// <remarks>
        /// Returned <see cref="Array"/> will be extended as needed to make it the specified <paramref name="length"/>, but
        /// it will never be less than the source <see cref="Array"/> length - <paramref name="startIndex"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of valid indexes for the source <see cref="Array"/> -or-
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        public static T[] Copy<T>(this T[] source, int startIndex, int length)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= source.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            // Create a new array that will be returned with the specified array elements.
            T[] copyOfSource = new T[source.Length - startIndex < length ? source.Length - startIndex : length];
            Array.Copy(source, startIndex, copyOfSource, 0, copyOfSource.Length);

            return copyOfSource;
        }

        /// <summary>Returns the smallest item from the enumeration.</summary>
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="source">An enumeration that is compared against.</param>
        /// <param name="comparer">A delegate that takes two generic types to compare, and returns an integer based on the comparison.</param>
        /// <returns>Returns a generic type.</returns>
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
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="source">An enumeration that is compared against.</param>
        /// <param name="comparer">A comparer object.</param>
        /// <returns>Returns a generic type.</returns>
        public static TSource Min<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
        {
            return source.Min<TSource>(comparer.Compare);
        }

        /// <summary>Returns the largest item from the enumeration.</summary>
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="source">An enumeration that is compared against.</param>
        /// <param name="comparer">A delegate that takes two generic types to compare, and returns an integer based on the comparison.</param>
        /// <returns>Returns a generic type.</returns>
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
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="source">An enumeration that is compared against.</param>
        /// <param name="comparer">A comparer object.</param>
        /// <returns>Returns a generic type.</returns>
        public static TSource Max<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
        {
            return source.Max<TSource>(comparer.Compare);
        }

        /// <summary>Converts an enumeration to a string, using the default delimeter ("|") that can later be
        /// converted back to a list using LoadDelimitedString.</summary>
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="source">The source object to be converted into a delimited string.</param>
        /// <returns>Returns a <see cref="String"/> that is result of combining all elements in the list delimited by the '|' character.</returns>
        public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source)
        {
            return source.ToDelimitedString<TSource>('|');
        }

        /// <summary>Converts an enumeration to a string that can later be converted back to a list using
        /// LoadDelimitedString.</summary>
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="source">The source object to be converted into a delimited string.</param>
        /// <param name="delimiter">The delimiting character used.</param>
        /// <returns>Returns a <see cref="String"/> that is result of combining all elements in the list delimited by <paramref name="delimiter"/>.</returns>
        public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source, char delimiter)
        {
            return ToDelimitedString<TSource, char>(source, delimiter);
        }

        /// <summary>Converts an enumeration to a string that can later be converted back to a list using
        /// LoadDelimitedString.</summary>
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="source">The source object to be converted into a delimited string.</param>
        /// <param name="delimiter">The delimiting <see cref="string"/> used.</param>
        /// <returns>Returns a <see cref="String"/> that is result of combining all elements in the list delimited by <paramref name="delimiter"/>.</returns>
        public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source, string delimiter)
        {
            return ToDelimitedString<TSource, string>(source, delimiter);
        }

        /// <summary>Converts an enumeration to a string that can later be converted back to a list using
        /// LoadDelimitedString.</summary>
        /// <typeparam name="TSource">The generic enumeration type used.</typeparam>
        /// <typeparam name="TDelimiter">The generic delimiter type used.</typeparam>
        /// <param name="source">The source object to be converted into a delimited string.</param>
        /// <param name="delimiter">The delimeter of type TDelimiter used.</param>
        /// <returns>Returns a <see cref="String"/> that is result of combining all elements in the list delimited by <paramref name="delimiter"/>.</returns>
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
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="destination">The list we are adding items to.</param>
        /// <param name="delimitedString">The delimited string to parse for items.</param>
        /// <param name="convertFromString">Delegate that takes one parameter and converts from string to type TSource.</param>
        public static void LoadDelimitedString<TSource>(this IList<TSource> destination, string delimitedString, Func<string, TSource> convertFromString)
        {
            destination.LoadDelimitedString(delimitedString, '|', convertFromString);
        }

        /// <summary>Appends items parsed from delimited string, created with ToDelimitedString, into the given list.</summary>
        /// <remarks>Items that are converted are added to list. The list is not cleared in advance.</remarks>
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="destination">The list we are adding items to.</param>
        /// <param name="delimitedString">The delimited string to parse for items.</param>
        /// <param name="delimeter">The <see cref="char"/> value to look for in the <paramref name="delimitedString"/> as the delimiter.</param>
        /// <param name="convertFromString">Delegate that takes one parameter and converts from string to type TSource.</param>
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
        /// <typeparam name="TSource">The generic type used.</typeparam>
        /// <param name="destination">The list we are adding items to.</param>
        /// <param name="delimitedString">The delimited string to parse for items.</param>
        /// <param name="delimiters">An array of delimiters to look for in the <paramref name="delimitedString"/> as the delimiter.</param>
        /// <param name="convertFromString">Delegate that takes a <see cref="String"/> and converts to type TSource.</param>
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

        /// <summary>
        /// Rearranges all the elements in the list into a random order.
        /// </summary>
        /// <typeparam name="TSource">The generic type of the list.</typeparam>
        /// <param name="source">The input list of generic types to scramble.</param>
        /// <remarks>This function uses a cryptographically strong random number generator to perform the scramble.</remarks>
        public static void Scramble<TSource>(this IList<TSource> source)
        {
            if (source.IsReadOnly)
                throw new ArgumentException("Cannot modify items in a read only list");

            int x, y;
            TSource currentItem;

            // Mixes up the data in random order.
            for (x = 0; x < source.Count; x++)
            {
                // Calls random function from TVA namespace.
                y = TVA.Security.Cryptography.Random.Int32Between(0, source.Count - 1);

                if (x != y)
                {
                    // Swaps items
                    currentItem = source[x];
                    source[x] = source[y];
                    source[y] = currentItem;
                }
            }
        }

        /// <summary>
        /// Rearranges all the elements in the list into a repeatable pseudo-random order.
        /// </summary>
        /// <param name="source">The input list of generic types to scramble.</param>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random number sequence.</param>
        /// <typeparam name="TSource">The generic type of the list.</typeparam>
        /// <remarks>This function uses the <see cref="System.Random"/> generator to perform the scramble using a sequence that is repeatable.</remarks>
        public static void Scramble<TSource>(this IList<TSource> source, int seed)
        {
            if (source.IsReadOnly)
                throw new ArgumentException("Cannot modify items in a read only list");

            Random random = new Random(seed);
            int x, y, count = source.Count;
            TSource currentItem;

            // Mixes up the data in random order.
            for (x = 0; x < count; x++)
            {
                // Calls random function from System namespace.
                y = random.Next(count);

                if (x != y)
                {
                    // Swaps items
                    currentItem = source[x];
                    source[x] = source[y];
                    source[y] = currentItem;
                }
            }
        }
        /// <summary>
        /// Rearranges all the elements in the list previously scrambled with <see cref="Scramble{TSource}(IList{TSource},int)"/> back into their original order.
        /// </summary>
        /// <param name="source">The input list of generic types to unscramble.</param>
        /// <param name="seed">The same number used in <see cref="Scramble{TSource}(IList{TSource},int)"/> call to scramble original list.</param>
        /// <typeparam name="TSource">The generic type of the list.</typeparam>
        /// <remarks>This function uses the <see cref="System.Random"/> generator to perform the unscramble using a sequence that is repeatable.</remarks>
        public static void Unscramble<TSource>(this IList<TSource> source, int seed)
        {
            if (source.IsReadOnly)
                throw new ArgumentException("Cannot modify items in a read only list");

            Random random = new Random(seed);
            List<int> sequence = new List<int>();
            int x, y, count = source.Count;
            TSource currentItem;

            // Generate original scramble sequence.
            for (x = 0; x < count; x++)
            {
                // Calls random function from System namespace.
                sequence.Add(random.Next(count));
            }

            // Unmix the data order (traverse same sequence in reverse order).
            for (x = count - 1; x >= 0; x--)
            {
                y = sequence[x];

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
        /// <param name="array1">The first type array to compare to.</param>
        /// <param name="array2">The second type array to compare against.</param>
        /// <returns>An <see cref="int"/> which returns 0 if they are equal, 1 if <paramref name="array1"/> is larger, or -1 if <paramref name="array2"/> is larger.</returns>
        /// <typeparam name="TSource">The generic type of the list.</typeparam>
        public static int CompareTo<TSource>(this TSource[] array1, TSource[] array2)
        {
            return CompareTo(array1, array2, Comparer<TSource>.Default);
        }

        /// <summary>Compares two arrays.</summary>
        /// <param name="array1">The first <see cref="Array"/> to compare to.</param>
        /// <param name="array2">The second <see cref="Array"/> to compare against.</param>
        /// <param name="comparer">An interface <see cref="IComparer"/> that exposes a method to compare the two arrays.</param>
        /// <returns>An <see cref="int"/> which returns 0 if they are equal, 1 if <paramref name="array1"/> is larger, or -1 if <paramref name="array2"/> is larger.</returns>
        /// <remarks>This is a default comparer to make arrays comparable.</remarks>
        public static int CompareTo(this Array array1, Array array2, IComparer comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

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
                    if (array1.Length == array2.Length)
                    {
                        int comparison = 0;

                        for (int x = 0; x < array1.Length; x++)
                        {
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
                        return array1.Length.CompareTo(array2.Length);
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