//*******************************************************************************************************
//  ReusableObjectPool.cs - Gbtc
//
//  Tennessee Valley Authority, 2011
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to GSF under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/23/2011 - J. Ritchie Carroll
//       Generated original version of source code.
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
 Original Software Title: The GSF Open Source Phasor Data Concentrator
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

#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using System.Collections.Concurrent;

namespace GSF
{
    /// <summary>
    /// Represents a reusable object pool that can be used by an application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Object pooling can offer a significant performance boost in some scenarios. It is most effective when the cost
    /// of construction is high and/or the rate of instantiation is high.
    /// </para>
    /// <para>
    /// This object pool is statically created at application startup, one per type, and is available for all components
    /// and classes within an application domain. Every time you need to use an object, you take one from the pool, use
    /// it, then return it to the pool when done. This process can be much faster than creating a new object every time
    /// you need to use one.
    /// </para>
    /// <para>
    /// When objects implement <see cref="ISupportLifecycle"/>, the <see cref="ReusableObjectPool{T}"/> will automatically
    /// attach to the <see cref="ISupportLifecycle.Disposed"/> event and return the object to the pool when it's disposed.
    /// When an object is taken from the pool, the <see cref="ISupportLifecycle.Initialize"/> method will be called to 
    /// reinstantiate any need member variables. If class tracks a disposed flag, it should be reset during call to the
    /// <see cref="ISupportLifecycle.Initialize"/> method so that class will be "un-disposed". Also, typical dispose
    /// implementations call <see cref="GC.SuppressFinalize"/> in the <see cref="IDisposable.Dispose"/> method, as such the
    /// <see cref="GC.ReRegisterForFinalize"/> should be called during <see cref="ISupportLifecycle.Initialize"/> method to
    /// make sure class will be disposed by finalizer in case <see cref="IDisposable.Dispose"/> method is never called.
    /// <example>
    /// Here is an example class that implements <see cref="ISupportLifecycle"/> such that it will be automatically returned
    /// to the pool when the class is disposed and automatically initialized when taken from the pool:
    /// <code>
    /// /// &lt;summary&gt;
    /// /// Class that shows example implementation of reusable &lt;see cref="ISupportLifecycle"/&gt;.
    /// /// &lt;/summary&gt;
    /// public class ReusableClass : ISupportLifecycle
    /// {
    ///     #region [ Members ]
    /// 
    ///     // Events
    ///     
    ///     /// &lt;summary&gt;
    ///     /// Occurs when the &lt;see cref="ReusableClass"/&gt; is disposed.
    ///     /// &lt;/summary&gt;
    ///     public event EventHandler Disposed;
    ///     
    ///     // Fields
    ///     private System.Timers.Timer m_timer;    // Example member variable that needs initialization and disposal
    ///     private bool m_enabled;
    ///     private bool m_disposed;
    /// 
    ///     #endregion
    ///
    ///     #region [ Constructors ]
    /// 
    ///     /// &lt;summary&gt;
    ///     /// Releases the unmanaged resources before the &lt;see cref="ReusableClass"/&gt; object is reclaimed by &lt;see cref="GC"/&gt;.
    ///     /// &lt;/summary&gt;
    ///     ~ReusableClass()
    ///     {
    ///         Dispose(false);
    ///     }
    /// 
    ///     #endregion
    /// 
    ///     #region [ Properties ]
    /// 
    ///     /// &lt;summary&gt;
    ///     /// Gets or sets a boolean value that indicates whether the &lt;see cref="ReusableClass"/&gt; object is currently enabled.
    ///     /// &lt;/summary&gt;
    ///     public bool Enabled
    ///     {
    ///         get
    ///         {
    ///             return m_enabled;
    ///         }
    ///         set
    ///         {
    ///             m_enabled = value;
    /// 
    ///             if (m_timer != null)
    ///                 m_timer.Enabled = m_enabled;
    ///         }
    ///     }
    /// 
    ///     #endregion
    /// 
    ///     #region [ Methods ]
    /// 
    ///     /// &lt;summary&gt;
    ///     /// Initializes the &lt;see cref="ReusableClass"/&gt; object.
    ///     /// &lt;/summary&gt;
    ///     public void Initialize()
    ///     {
    ///         // Undispose class instance if it is being reused
    ///         if (m_disposed)
    ///         {
    ///             m_disposed = false;
    ///             GC.ReRegisterForFinalize(this);
    ///         }
    ///         
    ///         m_enabled = true;
    /// 
    ///         // Initialize member variables
    ///         if (m_timer == null)
    ///         {
    ///             m_timer = new System.Timers.Timer(2000.0D);
    ///             m_timer.Elapsed += m_timer_Elapsed;
    ///             m_timer.Enabled = m_enabled;
    ///         }
    ///     }
    /// 
    ///     /// &lt;summary&gt;
    ///     /// Releases all the resources used by the &lt;see cref="ReusableClass"/&gt; object.
    ///     /// &lt;/summary&gt;
    ///     public void Dispose()
    ///     {
    ///         Dispose(true);
    ///         GC.SuppressFinalize(this);
    ///     }
    /// 
    ///     /// &lt;summary&gt;
    ///     /// Releases the unmanaged resources used by the &lt;see cref="ReusableClass"/&gt; object and optionally releases the managed resources.
    ///     /// &lt;/summary&gt;
    ///     /// &lt;param name="disposing"&gt;true to release both managed and unmanaged resources; false to release only unmanaged resources.&lt;/param&gt;
    ///     protected virtual void Dispose(bool disposing)
    ///     {
    ///         if (!m_disposed)
    ///         {
    ///             try
    ///             {
    ///                 if (disposing)
    ///                 {
    ///                     // Dispose member variables
    ///                     if (m_timer != null)
    ///                     {
    ///                         m_timer.Elapsed -= m_timer_Elapsed;
    ///                         m_timer.Dispose();
    ///                     }
    ///                     m_timer = null;
    ///                     
    ///                     m_enabled = false;
    ///                 }
    ///             }
    ///             finally
    ///             {
    ///                 m_disposed = true;
    /// 
    ///                 if (Disposed != null)
    ///                     Disposed(this, EventArgs.Empty);
    ///             }
    ///         }
    ///     }
    /// 
    ///     // Handle timer event
    ///     private void m_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    ///     {
    ///         // Do work here...
    ///     }
    /// 
    ///     #endregion
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// If the pool objects do not implement <see cref="ISupportLifecycle"/> you can still use the object pool, but it
    /// will be very imporant to return the object to the pool when you are finished using it. If you are using an object
    /// scoped within a method, make sure to use a try/finally so that you can take the object within the try and return
    /// the object within the finally. If you are using an object as a member scoped class field, make sure you use the
    /// standard dispose pattern and return the object during the <see cref="IDisposable.Dispose"/> method call. In this
    /// mode you will also need to manually initialze your object after you get it from the pool to reset its state.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Type of object to pool.</typeparam>
    public class ReusableObjectPool<T> where T : class, new()
    {
        private ConcurrentQueue<T> m_objectPool = new ConcurrentQueue<T>();

        /// <summary>
        /// Gets an object from the pool, or creates a new one if no pool items are available.
        /// </summary>
        /// <returns>An available object from the pool, or a new one if no pool items are available.</returns>
        /// <remarks>
        /// If type of <typeparamref name="T"/> implements <see cref="ISupportLifecycle"/>, the pool will attach
        /// to the item's <see cref="ISupportLifecycle.Disposed"/> event such that the object can be automatically
        /// restored to the pool upon <see cref="IDisposable.Dispose"/>. It will be up to class implementors to
        /// make sure <see cref="ISupportLifecycle.Initialize"/> makes the class ready for use as this method will
        /// always be called for an object being taken from the pool.
        /// </remarks>
        public T TakeObject()
        {
            T item;
            bool newItem = false;

            // Attempt to provide user with a queued item
            if (!m_objectPool.TryDequeue(out item))
            {
                // No items are available, create a new item for the object pool
                item = FastObjectFactory<T>.CreateObjectFunction();
                newItem = true;
            }

            // Automatically handle class life cycle if item implements support for this
            ISupportLifecycle lifecycleItem = item as ISupportLifecycle;

            if (lifecycleItem != null)
            {
                // Attach to dispose event so item can be automatically returned to the pool
                if (newItem)
                    lifecycleItem.Disposed += LifecycleItem_Disposed;

                // Initialize or reinitialize (i.e., un-dispose) the item
                lifecycleItem.Initialize();
            }

            return item;
        }

        /// <summary>
        /// Returns object to the pool.
        /// </summary>
        /// <param name="item">Reference to the object being returned.</param>
        /// <remarks>
        /// If type of <typeparamref name="T"/> implements <see cref="ISupportLifecycle"/>, the pool will automatically
        /// return the item to the pool when the <see cref="ISupportLifecycle.Disposed"/> event is raised, usually when
        /// the object's <see cref="IDisposable.Dispose"/> method is called.
        /// </remarks>
        public void ReturnObject(T item)
        {
            if (item != null)
                m_objectPool.Enqueue(item);
        }

        /// <summary>
        /// Releases all the objects currently cached in the pool.
        /// </summary>
        public void Clear()
        {
            T item;

            while (!m_objectPool.IsEmpty)
            {
                m_objectPool.TryDequeue(out item);
            }
        }

        /// <summary>
        /// Allocates the pool to the desired <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Desired pool size.</param>
        /// <exception cref="ArgumentOutOfRangeException">Pool <paramref name="size"/> must at least be one.</exception>
        public void SetPoolSize(int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", "pool size must at least be one");

            for (int i = 0; i < size - m_objectPool.Count; i++)
            {
                T item = FastObjectFactory<T>.CreateObjectFunction();

                // Automatically handle life cycle if item implements support for this
                ISupportLifecycle lifecycleItem = item as ISupportLifecycle;

                // Attach to dispose event so item can be automatically returned to the pool
                if (lifecycleItem != null)
                    lifecycleItem.Disposed += LifecycleItem_Disposed;

                m_objectPool.Enqueue(item);
            }
        }

        /// <summary>
        /// Gets the number of items currently contained in the pool.
        /// </summary>
        /// <returns>The size of the pool.</returns>
        public int GetPoolSize()
        {
            return m_objectPool.Count;
        }

        // Handler to automatically to return items to the queue 
        private void LifecycleItem_Disposed(object sender, EventArgs e)
        {
            ReturnObject(sender as T);
        }

        /// <summary>
        /// Default static instance which can be shared throughout the application.
        /// </summary>
        public static readonly ReusableObjectPool<T> Default = new ReusableObjectPool<T>(); 
    }

    /// <summary>
    /// Represents a reusable object pool that can be used by an application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <see cref="ReusableObjectPool{T}"/> for more details on using object pooling.
    /// </para>
    /// <para>
    /// <see cref="ReusableObjectPool"/> should be used when you only have the <see cref="Type"/> of an object available (such as when you are
    /// using reflection), otherwise you should use the generic <see cref="ReusableObjectPool{T}"/>.
    /// </para>
    /// </remarks>
    public class ReusableObjectPool
    {
        private static ConcurrentDictionary<Type, ConcurrentQueue<object>> s_objectPools = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();

        /// <summary>
        /// Gets an object from the pool, or creates a new one if no pool items are available.
        /// </summary>
        /// <param name="type">Type of object to get from pool.</param>
        /// <returns>An available object from the pool, or a new one if no pool items are available.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="type"/> does not support parameterless public constructor.</exception>
        /// <remarks>
        /// If <paramref name="type"/> implements <see cref="ISupportLifecycle"/>, the pool will attach
        /// to the item's <see cref="ISupportLifecycle.Disposed"/> event such that the object can be automatically
        /// restored to the pool upon <see cref="IDisposable.Dispose"/>. It will be up to class implementors to
        /// make sure <see cref="ISupportLifecycle.Initialize"/> makes the class ready for use as this method will
        /// always be called for an object being taken from the pool.
        /// </remarks>
        public static object TakeObject(Type type)
        {
            return TakeObject<object>(type);
        }

        /// <summary>
        /// Gets an object from the pool, or creates a new one if no pool items are available.
        /// </summary>
        /// <param name="type">Type of object to get from pool.</param>
        /// <typeparam name="T">Type of returned object to get from pool.</typeparam>
        /// <returns>An available object from the pool, or a new one if no pool items are available.</returns>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="type"/> does not support parameterless public constructor -or- 
        /// <paramref name="type"/> is not a subclass or interface implementation of function type definition.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function will validate that <typeparamref name="T"/> is related to <paramref name="type"/>.
        /// </para>
        /// <para>
        /// If <paramref name="type"/> implements <see cref="ISupportLifecycle"/>, the pool will attach
        /// to the item's <see cref="ISupportLifecycle.Disposed"/> event such that the object can be automatically
        /// restored to the pool upon <see cref="IDisposable.Dispose"/>. It will be up to class implementors to
        /// make sure <see cref="ISupportLifecycle.Initialize"/> makes the class ready for use as this method will
        /// always be called for an object being taken from the pool.
        /// </para>
        /// </remarks>
        public static T TakeObject<T>(Type type)
        {
            object item;
            bool newItem = false;

            // Get or create object pool associated with specified type
            ConcurrentQueue<object> objectPool = s_objectPools.GetOrAdd(type, (objType) => new ConcurrentQueue<object>());

            // Attempt to provide user with a queued item
            if (!objectPool.TryDequeue(out item))
            {
                // No items are available, create a new item for the object pool
                item = FastObjectFactory.GetCreateObjectFunction<T>(type)();
                newItem = true;
            }

            // Automatically handle class life cycle if item implements support for this
            ISupportLifecycle lifecycleItem = item as ISupportLifecycle;

            if (lifecycleItem != null)
            {
                // Attach to dispose event so item can be automatically returned to the pool
                if (newItem)
                    lifecycleItem.Disposed += LifecycleItem_Disposed;

                // Initialize or reinitialize (i.e., un-dispose) the item
                lifecycleItem.Initialize();
            }

            return (T)item;
        }

        /// <summary>
        /// Returns object to the pool.
        /// </summary>
        /// <param name="item">Reference to the object being returned.</param>
        /// <remarks>
        /// If type of <paramref name="item"/> implements <see cref="ISupportLifecycle"/>, the pool will automatically
        /// return the item to the pool when the <see cref="ISupportLifecycle.Disposed"/> event is raised, usually when
        /// the object's <see cref="IDisposable.Dispose"/> method is called.
        /// </remarks>
        public static void ReturnObject(object item)
        {
            if (item != null)
            {
                ConcurrentQueue<object> objectPool;

                // Try to get object pool associated with the specified item's type
                if (s_objectPools.TryGetValue(item.GetType(), out objectPool))
                    objectPool.Enqueue(item);
            }
        }

        /// <summary>
        /// Releases all the objects currently cached in the specified pool.
        /// </summary>
        /// <param name="type">Type of pool to clear.</param>
        public static void Clear(Type type)
        {
            ConcurrentQueue<object> objectPool;

            // Try to get object pool associated with the specified type
            if (s_objectPools.TryGetValue(type, out objectPool))
            {
                object item;

                while (!objectPool.IsEmpty)
                {
                    objectPool.TryDequeue(out item);
                }
            }
        }

        /// <summary>
        /// Allocates the pool to the desired <paramref name="size"/>.
        /// </summary>
        /// <param name="type">Type of pool to initialize.</param>
        /// <param name="size">Desired pool size.</param>
        /// <exception cref="ArgumentOutOfRangeException">Pool <paramref name="size"/> must at least be one.</exception>
        public static void SetPoolSize(Type type, int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", "pool size must at least be one");

            // Get or create object pool associated with specified type
            ConcurrentQueue<object> objectPool = s_objectPools.GetOrAdd(type, (objType) => new ConcurrentQueue<object>());

            for (int i = 0; i < size - objectPool.Count; i++)
            {
                object item = FastObjectFactory.GetCreateObjectFunction(type)();

                // Automatically handle life cycle if item implements support for this
                ISupportLifecycle lifecycleItem = item as ISupportLifecycle;

                // Attach to dispose event so item can be automatically returned to the pool
                if (lifecycleItem != null)
                    lifecycleItem.Disposed += LifecycleItem_Disposed;

                objectPool.Enqueue(item);
            }
        }

        // Handler to automatically to return items to the queue 
        private static void LifecycleItem_Disposed(object sender, EventArgs e)
        {
            ReturnObject(sender);
        }
    }
}
