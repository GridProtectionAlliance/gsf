//*******************************************************************************************************
//  CollectionExtensions.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
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
//       Converted to C# extension functions.
//  02/17/2009 - Josh L. Patterson
//       Edited Code Comments.
//  02/20/2009 - J. Ritchie Carroll
//       Added predicate based IndexOf that extends IList<T>.
//  04/02/2009 - J. Ritchie Carroll
//       Added seed based scramble and unscramble IList<T> extensions.
//  06/05/2009 - Pinal C. Patel
//       Added generic AddRange() extension method for IList<T>.
//  06/09/2009 - Pinal C. Patel
//       Added generic GetRange() extension method for IList<T>.
//  08/05/2009 - Josh L. Patterson
//       Update comments.
//  9/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

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