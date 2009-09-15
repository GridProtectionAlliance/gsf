//*******************************************************************************************************
//  DictionaryList.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/07/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/10/2008 - J. Ritchie Carroll
//       Converted to C#.
//  02/19/2009 - Josh L. Patterson
//       Edited Code Comments.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
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

using System.Collections;
using System.Collections.Generic;

namespace TVA.Collections
{
    /// <summary>
    /// Represents a sorted dictionary style list that supports <see cref="IList"/>.
    /// </summary>
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
    /// <typeparam name="TKey">Generic key type.</typeparam>
    /// <typeparam name="TValue">Genervic value type.</typeparam>
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