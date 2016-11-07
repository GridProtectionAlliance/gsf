//******************************************************************************************************
//  ReusableObjectPool.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/23/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

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
    [Obsolete("It is not recommended to use this class because the need for pooling is rare and implementations of pooling can be dangerous.")]
    public class ReusableObjectPool<T> where T : class, new()
    {
        private readonly ConcurrentQueue<T> m_objectPool = new ConcurrentQueue<T>();

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
                throw new ArgumentOutOfRangeException(nameof(size), "pool size must at least be one");

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
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
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
    [Obsolete("It is not recommended to use this class because the need for pooling is rare and implementations of pooling can be dangerous.")]
    public static class ReusableObjectPool
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> s_objectPools = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();

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
                throw new ArgumentOutOfRangeException(nameof(size), "pool size must at least be one");

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
