//*******************************************************************************************************
//  KeyedProcessQueue.cs - Gbtc
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
//  08/17/2007 - Darrell Zuercher
//       Edited code comments.
//  09/11/2008 - J. Ritchie Carroll
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

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TVA.Collections
{
    /// <summary>
    /// Represents a keyed collection of items that get processed on independent threads with
    /// a consumer provided function.
    /// </summary>
    /// <typeparam name="TKey">Type of keys used to reference process items.</typeparam>
    /// <typeparam name="TValue">Type of values to process.</typeparam>
    /// <remarks>
    /// <para>This class acts as a strongly-typed sorted dictionary of objects to be processed.</para>
    /// <para>
    /// Consumers are expected to create new instances of this class through the static construction functions (e.g., 
    /// <see cref="KeyedProcessQueue{TKey,TValue}.CreateAsynchronousQueue(ProcessItemFunctionSignature)"/>, 
    /// <see cref="KeyedProcessQueue{TKey,TValue}.CreateSynchronousQueue(ProcessItemFunctionSignature)"/>, etc.)
    /// </para>
    /// <para>Note that the <see cref="KeyedProcessQueue{TKey,TValue}"/> will not start processing until the Start method is called.</para>
    /// <para>Because this <see cref="KeyedProcessQueue{TKey,TValue}"/> represents a dictionary style collection, all keys must be unique.</para>
    /// <para>
    /// Be aware that this class is based on a <see cref="DictionaryList{TKey,TValue}"/> (i.e., a <see cref="SortedList{TKey,TValue}"/>
    /// that implements <see cref="IList{T}"/>), and since items in this kind of list are automatically sorted, items will be processed
    /// in "sorted" order regardless of the order in which they are added to the list.
    /// </para>
    /// <para>
    /// Important note about using an "Integer" as the key for this class: because the <see cref="KeyedProcessQueue{TKey,TValue}"/> base class must
    /// implement IList, a normal dictionary cannot be used for the base class. IDictionary implementations
    /// do not normally implement the IList interface because of ambiguity that is caused when implementing
    /// an integer key. For example, if you implement this class with a key of type "Integer," you will not
    /// be able to access items in the <see cref="KeyedProcessQueue{TKey,TValue}"/> by index without "casting" the 
    /// <see cref="KeyedProcessQueue{TKey,TValue}"/> as IList. This is because the Item property in both the IDictionary and IList would
    /// have the same parameters (see the <see cref="DictionaryList{TKey,TValue}"/> class for more details.).
    /// </para>
    /// </remarks>
    public class KeyedProcessQueue<TKey, TValue> : ProcessQueue<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        #region [ Members ]

        // Delegates

        /// <summary>
        /// Function signature that defines a method to process a key and value one at a time.
        /// </summary>
        /// <param name="key">key to be processed.</param>
        /// <param name="value">value to be processed.</param>
        /// <remarks>
        /// <para>Required unless <see cref="KeyedProcessQueue{TKey,TValue}.ProcessItemsFunction"/> is implemented.</para>
        /// <para>Used when creating a <see cref="KeyedProcessQueue{TKey,TValue}"/> to process one item at a time.</para>
        /// <para>Asynchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> will process individual items on multiple threads</para>
        /// </remarks>
        public new delegate void ProcessItemFunctionSignature(TKey key, TValue value);

        /// <summary>
        /// Function signature that determines if a key and value can be currently processed.
        /// </summary>
        /// <param name="key">key to be checked for processing availablity.</param>
        /// <param name="value">value to be checked for processing availablity.</param>
        /// <returns>True, if key and value can be processed.</returns>
        /// <remarks>
        /// <para>Implementation of this function is optional. It will be assumed that an item can be processed if this
        /// function is not defined</para>
        /// <para>Items must eventually get to a state where they can be processed or they will remain in the <see cref="KeyedProcessQueue{TKey,TValue}"/>
        /// indefinitely.</para>
        /// <para>
        /// Note that when this function is implemented and <see cref="QueueProcessingStyle"/> = ManyAtOnce (i.e., 
        /// <see cref="KeyedProcessQueue{TKey,TValue}.ProcessItemsFunction"/> is defined), then each item presented
        /// for processing must evaluate as "CanProcessItem = True" before any items are processed.
        /// </para>
        /// </remarks>
        public new delegate bool CanProcessItemFunctionSignature(TKey key, TValue value);

        // Fields
        private ProcessItemFunctionSignature m_processItemFunction;
        private CanProcessItemFunctionSignature m_canProcessItemFunction;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a ProcessList based on the generic DictionaryList class.
        /// </summary>
        /// <param name="processItemFunction">A delegate <see cref="ProcessItemFunctionSignature"/> that defines a function signature to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate <see cref="CanProcessItemFunctionSignature"/> that determines of a key and value can currently be processed.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether the process should requeue the item after an exception.</param>
        protected KeyedProcessQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
            : base(null, null, null, new DictionaryList<TKey, TValue>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
            m_processItemFunction = processItemFunction; // Defining this function creates a ProcessingStyle = OneAtATime keyed process queue
            m_canProcessItemFunction = canProcessItemFunction;

            // Assigns translator functions for base class.
            base.ProcessItemFunction = ProcessKeyedItem;

            if (m_canProcessItemFunction != null)
                base.CanProcessItemFunction = CanProcessKeyedItem;
        }

        /// <summary>
        /// Creates a bulk-item ProcessList based on the generic DictionaryList class.
        /// </summary>
        /// <param name="processItemsFunction">A delegate ProcessItemsFunctionSignature that defines a function signature to process multiple items at once.</param>
        /// <param name="canProcessItemFunction">A delegate <see cref="CanProcessItemFunctionSignature"/> that determines of a key and value can currently be processed.</param>
        /// <param name="processInterval">A <see cref="double"/> which represents the process interval.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> that represents the max number of threads to use.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> that represents the amount of time before a process times out.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether the process should requeue the item after a timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether the process should requeue the item after an exception.</param>
        protected KeyedProcessQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
            : base(null, processItemsFunction, null, new DictionaryList<TKey, TValue>(), processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException)
        {
            m_canProcessItemFunction = canProcessItemFunction;

            // Assigns translator functions for base class.
            if (m_canProcessItemFunction != null)
                base.CanProcessItemFunction = CanProcessKeyedItem;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the user function used to process items in the list one at a time.
        /// </summary>
        /// <remarks>
        /// <para>This function and <see cref="KeyedProcessQueue{TKey,TValue}.ProcessItemFunction"/> cannot be defined at the same time.</para>
        /// <para>A <see cref="KeyedProcessQueue{TKey,TValue}"/> must be defined to process either a single item at a time or many items at once.</para>
        /// <para>Implementation of this function makes <see cref="QueueProcessingStyle"/> = OneAtATime.</para>
        /// </remarks>
        public virtual new ProcessItemFunctionSignature ProcessItemFunction
        {
            get
            {
                return m_processItemFunction;
            }
            set
            {
                if (value != null)
                {
                    m_processItemFunction = value;

                    // Assigns translator function for base class.
                    base.ProcessItemFunction = ProcessKeyedItem;
                }
            }
        }

        /// <summary>
        /// Gets or sets the user function used to process multiple items in the list at once.
        /// </summary>
        /// <remarks>
        /// <para>This function and <see cref="KeyedProcessQueue{TKey,TValue}.ProcessItemFunction"/> cannot be defined at the same time.</para>
        /// <para>A <see cref="KeyedProcessQueue{TKey,TValue}"/> must be defined to process either a single item at a time or many items at once.</para>
        /// <para>Implementation of this function makes <see cref="QueueProcessingStyle"/> = ManyAtOnce.</para>
        /// </remarks>
        public override ProcessItemsFunctionSignature ProcessItemsFunction
        {
            get
            {
                return base.ProcessItemsFunction;
            }
            set
            {
                if (value != null)
                {
                    m_processItemFunction = null;
                    base.ProcessItemsFunction = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the user function used to determine if an item is ready to be processed.
        /// </summary>
        public virtual new CanProcessItemFunctionSignature CanProcessItemFunction
        {
            get
            {
                return m_canProcessItemFunction;
            }
            set
            {
                m_canProcessItemFunction = value;

                // Assigns translator function for base class.
                if (m_canProcessItemFunction == null)
                    base.CanProcessItemFunction = null;
                else
                    base.CanProcessItemFunction = CanProcessKeyedItem;
            }
        }

        /// <summary>
        /// Gets the class name.
        /// </summary>
        /// <returns>Class name.</returns>
        /// <remarks>
        /// <para>This name is used for class identification in strings (e.g., used in error message).</para>
        /// <para>Derived classes should override this method with a proper class name.</para>
        /// </remarks>
        public override string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation
        /// throws a KeyNotFoundException, and a set operation creates a new element with the specified key.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                lock (SyncRoot)
                {
                    return InternalDictionary[key];
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    InternalDictionary[key] = value;
                    DataAdded();
                }
            }
        }

        /// <summary>Gets an ICollection containing the keys of the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</summary>
        /// <returns>An ICollection containing the keys of the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public ICollection<TKey> Keys
        {
            get
            {
                return InternalDictionary.Keys;
            }
        }

        /// <summary>Gets an ICollection containing the values of the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</summary>
        /// <returns>An ICollection containing the values of the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public ICollection<TValue> Values
        {
            get
            {
                return InternalDictionary.Values;
            }
        }

        /// <summary>
        /// Gets the internal sorted dictionary for direct use by derived classes.
        /// </summary>
        protected DictionaryList<TKey, TValue> InternalDictionary
        {
            get
            {
                return (DictionaryList<TKey, TValue>)InternalList;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="KeyedProcessQueue{TKey, TValue}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        m_processItemFunction = null;
                        m_canProcessItemFunction = null;
                    }
                }
                finally
                {
                    base.Dispose(disposing);    // Call base class Dispose().
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        #region [ Item Processing Translation Functions ]

        // These functions act as intermediate "translators" between the delegate implementations of
        // ProcessQueue and KeyedProcessQueue. Users implementing a KeyedProcessQueue will likely be
        // thinking in terms of "keys" and "values", and not a KeyValuePair structure. Note that the
        // bulk item ProcessItems delegate is not translated since an array of KeyValuePair structures
        // would make more sense and be more efficient than two separate arrays of keys and values.
        private void ProcessKeyedItem(KeyValuePair<TKey, TValue> item)
        {
            m_processItemFunction(item.Key, item.Value);
        }

        private bool CanProcessKeyedItem(KeyValuePair<TKey, TValue> item)
        {
            return m_canProcessItemFunction(item.Key, item.Value);
        }

        //private void ProcessKeyedItems(KeyValuePair<TKey, TValue>[] items)
        //{
        //    // Copies an array of KeyValuePairs into an array of keys and values.
        //    TKey[] keys = new TKey[items.Length];
        //    TValue[] values = new TValue[items.Length];
        //    KeyValuePair<TKey, TValue> kvPair;

        //    for (int x = 0; x < items.Length; x++)
        //    {
        //        kvPair = items[x];
        //        keys[x] = kvPair.Key;
        //        values[x] = kvPair.Value;
        //    }

        //    m_processItemsFunction(keys, values);
        //}

        #endregion

        #region [ Generic IDictionary(Of TKey, TValue) Implementation ]

        /// <summary>Adds an element with the provided key and value to the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</summary>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <exception cref="NotSupportedException">The <see cref="KeyedProcessQueue{TKey,TValue}"/> is read-only.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</exception>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public void Add(TKey key, TValue value)
        {
            lock (SyncRoot)
            {
                InternalDictionary.Add(key, value);
                DataAdded();
            }
        }

        /// <summary>Determines whether the <see cref="KeyedProcessQueue{TKey,TValue}"/> contains an element with the specified key.</summary>
        /// <returns>True, if the <see cref="KeyedProcessQueue{TKey,TValue}"/> contains an element with the key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.ContainsKey(key);
            }
        }

        /// <summary>Determines whether the <see cref="KeyedProcessQueue{TKey,TValue}"/> contains an element with the specified value.</summary>
        /// <returns>True, if the <see cref="KeyedProcessQueue{TKey,TValue}"/> contains an element with the value; otherwise, false.</returns>
        /// <param name="value">The value to locate in the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</param>
        public bool ContainsValue(TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.ContainsValue(value);
            }
        }

        /// <summary>
        /// Searches for the specified key and returns the zero-based index within the entire <see cref="KeyedProcessQueue{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</param>
        /// <returns>The zero-based index of key within the entire <see cref="KeyedProcessQueue{TKey,TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfKey(TKey key)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.IndexOfKey(key);
            }
        }

        /// <summary>
        /// Searches for the specified value and returns the zero-based index of the first occurrence within the entire <see cref="KeyedProcessQueue{TKey,TValue}"/>.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="KeyedProcessQueue{TKey,TValue}"/>. The value can be null for reference types.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire <see cref="KeyedProcessQueue{TKey,TValue}"/>, if found; otherwise, -1.</returns>
        public int IndexOfValue(TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.IndexOfValue(value);
            }
        }

        /// <summary>Removes the element with the specified key from the <see cref="KeyedProcessQueue{TKey,TValue}"/>.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        /// <returns>This method returns a <see cref="Boolean"/> value indicating whether the item was removed.</returns>
        public bool Remove(TKey key)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.Remove(key);
            }
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <returns>True, if the <see cref="KeyedProcessQueue{TKey,TValue}"/> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter. This parameter is passed
        /// uninitialized.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (SyncRoot)
            {
                return InternalDictionary.TryGetValue(key, out value);
            }
        }

        #endregion

        #region [ Overriden List(T) Functions ]

        // Because consumers will be able to call these functions in their "dictionary" style queue, we'll make
        // sure they return something that makes sense in case they get called, but we will hide the functions from
        // the editor to help avoid confusion.

        ///	<summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int BinarySearch(KeyValuePair<TKey, TValue> item)
        {
            return IndexOfKey(item.Key);
        }

        ///	<summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int BinarySearch(KeyValuePair<TKey, TValue> item, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            return IndexOfKey(item.Key);
        }

        ///	<summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        ///	<param name="item">The object to locate. The value can be null for reference types.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or- null to use
        /// the default comparer: Generic.Comparer(Of T).Default</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int BinarySearch(int index, int count, KeyValuePair<TKey, TValue> item, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        ///	<returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int IndexOf(KeyValuePair<TKey, TValue> item, int index, int count)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int LastIndexOf(KeyValuePair<TKey, TValue> item)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int LastIndexOf(KeyValuePair<TKey, TValue> item, int index)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// This function doesn't have the same meaning in the <see cref="KeyedProcessQueue{TKey,TValue}"/> as it does in
        /// <see cref="ProcessQueue{T}"/>, so it is marked as hidden from the editor.  However it returns
        /// <see cref="KeyedProcessQueue{TKey,TValue}.IndexOfKey"/> so that it returns a value that at least makes sense
        /// in case it gets called.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ProcessQueue{T}"/>. The value can be null for reference types.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>This method returns an <see cref="Int32"/> that is the index of the item.Key.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int LastIndexOf(KeyValuePair<TKey, TValue> item, int index, int count)
        {
            return IndexOfKey(item.Key);
        }

        /// <summary>
        /// <see cref="KeyedProcessQueue{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort()
        {
            // This list is already sorted.
        }

        /// <summary>
        /// <see cref="KeyedProcessQueue{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort(IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            // This list is already sorted.
        }

        /// <summary>
        /// <see cref="KeyedProcessQueue{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        /// <param name="comparison">The comparison to use when comparing elements.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort(System.Comparison<KeyValuePair<TKey, TValue>> comparison)
        {
            // This list is already sorted.
        }

        /// <summary>
        /// <see cref="KeyedProcessQueue{TKey,TValue}"/> is based on a <see cref="DictionaryList{TKey,TValue}"/> which is already
        /// sorted, so calling this function has no effect.  As a result this function is marked as hidden from the editor.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to search.</param>
        /// <param name="count">The length of the range to search.</param>
        /// <param name="comparer">The Generic.IComparer implementation to use when comparing elements -or-
        /// null to use the default comparer: Generic.Comparer(Of T).Default</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Sort(int index, int count, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            // This list is already sorted.
        }

        #endregion

        #endregion

        #region [ Static ]

        #region [ Single-Item Processing Constructors ]

        /// <summary>
        /// Creates a new, keyed, asynchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateAsynchronousQueue(processItemFunction, null, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, asynchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate which defines a method to indicate whether a key and value can be processed at this time.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, asynchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemFunction, null, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, asynchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate which defines a method to indicate whether a key and value can be processed at this time.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, asynchronous <see cref="KeyedProcessQueue{TKey,TValue}"/>, using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateAsynchronousQueue(processItemFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, asynchronous <see cref="KeyedProcessQueue{TKey,TValue}"/>, using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate which defines a method to indicate whether a key and value can be processed at this time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="maximumThreads">The maximum number of threads for the queue to use.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new KeyedProcessQueue<TKey, TValue>(processItemFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }




        /// <summary>
        /// Creates a new, keyed, synchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateSynchronousQueue(processItemFunction, null, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, synchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate which defines a method to indicate whether a key and value can be processed at this time.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateSynchronousQueue(processItemFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, synchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread), using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateSynchronousQueue(processItemFunction, null, processInterval, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, synchronous <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread), using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate which defines a method to indicate whether a key and value can be processed at this time.</param>
        /// <param name="processInterval">a <see cref="double"/> value which represents the process interval in milliseconds.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new KeyedProcessQueue<TKey, TValue>(processItemFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }






        /// <summary>
        /// Creates a new, keyed, real-time <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction)
        {
            return CreateRealTimeQueue(processItemFunction, null, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, real-time <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate which defines a method to indicate whether a key and value can be processed at this time.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateRealTimeQueue(processItemFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, real-time <see cref="KeyedProcessQueue{TKey,TValue}"/>, using specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateRealTimeQueue(processItemFunction, null, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new, keyed, real-time <see cref="KeyedProcessQueue{TKey,TValue}"/>, using the specified settings.
        /// </summary>
        /// <param name="processItemFunction">Delegate which defines a method to process a key and value one at a time.</param>
        /// <param name="canProcessItemFunction">A delegate which defines a method to indicate whether a key and value can be processed at this time.</param>
        /// <param name="processTimeout">The number of seconds before a process should timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue KeyedProcessQueue.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemFunctionSignature processItemFunction, CanProcessItemFunctionSignature canProcessItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new KeyedProcessQueue<TKey, TValue>(processItemFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        #endregion

        #region [ Multi-Item Processing Constructors ]

        /// <summary>
        /// Creates a new asynchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static new KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="canProcessItemFunction">Delegate which defines a method to know if a key and value can currently be processed.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value indicating the maximum number of threads to use for processing items.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static new KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessInterval = 100,
        /// ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="canProcessItemFunction">Delegate which defines a method to know if a key and value can currently be processed.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value indicating the maximum number of threads to use for processing items.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, int maximumThreads)
        {
            return CreateAsynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/>, using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="processInterval">Number of milliseconds between each process.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value indicating the maximum number of threads to use for processing items.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> value indicating the number of seconds to wait for a process timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static new KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateAsynchronousQueue(processItemsFunction, null, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new asynchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/>, using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="canProcessItemFunction">Delegate which defines a method to know if a key and value can currently be processed.</param>
        /// <param name="processInterval">Number of milliseconds between each process.</param>
        /// <param name="maximumThreads">An <see cref="Int32"/> value indicating the maximum number of threads to use for processing items.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> value indicating the number of seconds to wait for a process timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateAsynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int maximumThreads, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new KeyedProcessQueue<TKey, TValue>(processItemsFunction, canProcessItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static new KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateSynchronousQueue(processItemsFunction, null, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread) with the default settings:
        /// ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="canProcessItemFunction">Delegate which defines a method to know if a key and value can currently be processed.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateSynchronousQueue(processItemsFunction, canProcessItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread), using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="processInterval">Number of milliseconds between each process.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> value indicating the number of seconds to wait for a process timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static new KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateSynchronousQueue(processItemsFunction, null, processInterval, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new synchronous bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> (i.e., single process thread), using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="canProcessItemFunction">Delegate which defines a method to know if a key and value can currently be processed.</param>
        /// <param name="processInterval">Number of milliseconds between each process.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> value indicating the number of seconds to wait for a process timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateSynchronousQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, double processInterval, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new KeyedProcessQueue<TKey, TValue>(processItemsFunction, canProcessItemFunction, processInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static new KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction)
        {
            return CreateRealTimeQueue(processItemsFunction, null, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/> with the default settings: ProcessTimeout = Infinite,
        /// RequeueOnTimeout = False, RequeueOnException = False.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="canProcessItemFunction">Delegate which defines a method to know if a key and value can currently be processed.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction)
        {
            return CreateRealTimeQueue(processItemsFunction, canProcessItemFunction, DefaultProcessTimeout, DefaultRequeueOnTimeout, DefaultRequeueOnException);
        }

        /// <summary>
        /// Creates a new real-time bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/>, using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> value indicating the number of seconds to wait for a process timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static new KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return CreateRealTimeQueue(processItemsFunction, null, processTimeout, requeueOnTimeout, requeueOnException);
        }

        /// <summary>
        /// Creates a new real-time bulk-item <see cref="KeyedProcessQueue{TKey,TValue}"/>, using specified settings.
        /// </summary>
        /// <param name="processItemsFunction">Delegate which defines a method to process at once.</param>
        /// <param name="canProcessItemFunction">Delegate which defines a method to know if a key and value can currently be processed.</param>
        /// <param name="processTimeout">An <see cref="Int32"/> value indicating the number of seconds to wait for a process timeout.</param>
        /// <param name="requeueOnTimeout">A <see cref="Boolean"/> value that indicates whether a process should requeue an item on timeout.</param>
        /// <param name="requeueOnException">A <see cref="Boolean"/> value that indicates whether a process should requeue after an exception.</param>
        /// <returns>Returns the process queue <see cref="KeyedProcessQueue{TKey,TValue}"/>.</returns>
        public static KeyedProcessQueue<TKey, TValue> CreateRealTimeQueue(ProcessItemsFunctionSignature processItemsFunction, CanProcessItemFunctionSignature canProcessItemFunction, int processTimeout, bool requeueOnTimeout, bool requeueOnException)
        {
            return new KeyedProcessQueue<TKey, TValue>(processItemsFunction, canProcessItemFunction, RealTimeProcessInterval, 1, processTimeout, requeueOnTimeout, requeueOnException);
        }

        #endregion

        #endregion
    }
}