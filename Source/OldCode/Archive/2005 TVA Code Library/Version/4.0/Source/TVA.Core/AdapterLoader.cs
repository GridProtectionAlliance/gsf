//*******************************************************************************************************
//  AdapterLoader.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/20/2009 - Pinal C. Patel
//       Generated original version of source code.
//  08/06/2009 - Pinal C. Patel
//       Modified Dispose(boolean) to iterate through the adapter collection correctly.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using TVA.Collections;
using TVA.IO;

namespace TVA
{
    /// <summary>
    /// Represents a generic loader of adapters.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of adapters to be loaded.</typeparam>
    public class AdapterLoader<T> : ISupportLifecycle
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Occurs when a new adapter is loaded to the <see cref="Adapters"/> list.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the adapter that was loaded.
        /// </remarks>
        public event EventHandler<EventArgs<T>> AdapterLoaded;

        /// <summary>
        /// Occurs when an existing adapter is unloaded from the <see cref="Adapters"/> list.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the adapter that was unloaded.
        /// </remarks>
        public event EventHandler<EventArgs<T>> AdapterUnloaded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when loading an adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="Type"/> of adapter that was being loaded.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when loading the adapter.
        /// </remarks>
        public event EventHandler<EventArgs<Type, Exception>> AdapterLoadException;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while executing a queued operation on one the <see cref="Adapters"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the adapter on which the operation was being executed.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the <see cref="Exception"/> encountered when executing an operation on the adapter.
        /// </remarks>
        public event EventHandler<EventArgs<T, Exception>> OperationExecutionException;

        // Fields
        private string m_adapterDirectory;
        private bool m_watchForAdapters;
        private ObservableCollection<T> m_adapters;
        private FileSystemWatcher m_adapterWatcher;
        private ProcessQueue<object> m_operationQueue;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterLoader{T}"/> class.
        /// </summary>
        public AdapterLoader()
        {
            m_adapterDirectory = string.Empty;
            m_watchForAdapters = true;
            m_adapters = new ObservableCollection<T>();
            m_adapters.CollectionChanged += Adapters_CollectionChanged;
            m_adapterWatcher = new FileSystemWatcher();
            m_adapterWatcher.Created += AdapterWatcher_Created;
            m_operationQueue = ProcessQueue<object>.CreateRealTimeQueue(ExecuteOperation);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AdapterLoader{T}"/> is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AdapterLoader()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the directory where <see cref="Adapters"/> are located.
        /// </summary>
        /// <remarks>
        /// When an empty string is assigned to <see cref="AdapterDirectory"/>, <see cref="Adapters"/> are loaded from the directory where application is executing.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value being assigned is a null string.</exception>
        public string AdapterDirectory
        {
            get
            {
                return m_adapterDirectory;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                m_adapterDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether new assemblies added at runtime will be processed for <see cref="Adapters"/>.
        /// </summary>
        public bool WatchForAdapters
        {
            get
            {
                return m_watchForAdapters;
            }
            set
            {
                m_watchForAdapters = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="AdapterLoader{T}"/> is currently enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (!m_watchForAdapters)
                    return m_initialized;
                else
                    return m_adapterWatcher.EnableRaisingEvents;
            }
            set
            {
                if (m_initialized)
                {
                    // Start or stop watching for adapters.
                    if (m_watchForAdapters)
                        m_adapterWatcher.EnableRaisingEvents = value;
                }
                else
                {
                    // Initialize if uninitialized when enabled.
                    if (value)
                        Initialize();
                }
            }
        }

        /// <summary>
        /// Gets a list of adapters loaded from the <see cref="AdapterDirectory"/>.
        /// </summary>
        public IList<T> Adapters
        {
            get
            {
                return m_adapters;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileSystemWatcher"/> object watching for new adapter assemblies added at runtime.
        /// </summary>
        protected FileSystemWatcher AdapterWatcher
        {
            get 
            {
                return m_adapterWatcher;
            }
        }

        /// <summary>
        /// Gets the <see cref="ProcessQueue{T}"/> object to be used for queuing operations to be executed on <see cref="Adapters"/>.
        /// </summary>
        protected ProcessQueue<object> OperationQueue
        {
            get 
            {
                return m_operationQueue;
            }
        }

        #endregion

        #region [ Methods ]
        
        /// <summary>
        /// Releases all the resources used by the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public virtual void Initialize()
        {
            if (!m_initialized)
            {
                if (string.IsNullOrEmpty(m_adapterDirectory))
                    Initialize(typeof(T).LoadImplementations(FilePath.GetAbsolutePath("*.*")));
                else
                    Initialize(typeof(T).LoadImplementations(FilePath.GetAbsolutePath(Path.Combine(m_adapterDirectory, "*.*"))));
            }
        }

        /// <summary>
        /// Initializes the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        /// <param name="adapterTypes">Collection of adapter <see cref="Type"/>s from which <see cref="Adapters"/> are to be created.</param>
        public virtual void Initialize(IEnumerable<Type> adapterTypes)
        {
            if (!m_initialized)
            {
                // Process adapters.
                foreach (Type type in adapterTypes)
                {
                    ProcessAdapter(type);
                }

                // Watch for adapters.
                if (m_watchForAdapters)
                {
                    m_adapterWatcher.Path = FilePath.GetAbsolutePath(m_adapterDirectory);
                    m_adapterWatcher.Filter = "*.dll";
                    m_adapterWatcher.EnableRaisingEvents = true;
                }

                // Start process queue.
                m_operationQueue.Start();

                // Initialize only once.
                m_initialized = true;
            }
        }

        /// <summary>
        /// Processes the <paramref name="adapterType"/> by creating its instance and initializing it.
        /// </summary>
        /// <param name="adapterType"><see cref="Type"/> of the adapter to be instantiated and initialized.</param>
        protected virtual void ProcessAdapter(Type adapterType)
        {
            try
            {
                // Instantiate adapter instance.
                T adapter = (T)(Activator.CreateInstance(adapterType));

                // Initialize adapter if supported.
                ISupportLifecycle initializableAdapter = adapter as ISupportLifecycle;
                if (initializableAdapter != null)
                    initializableAdapter.Initialize();

                // Add adapter and notify via event.
                lock (m_adapters)
                {
                    m_adapters.Add(adapter);
                }
            }
            catch (Exception ex)
            {
                OnAdapterLoadException(adapterType, ex);
            }
        }

        /// <summary>
        /// Executes an operation on the <paramref name="adapter"/> with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="adapter">Adapter on which an operation is to be executed.</param>
        /// <param name="data">Data to be used when executing an operation.</param>
        /// <exception cref="NotSupportedException">Always</exception>
        protected virtual void ExecuteAdapterOperation(T adapter, object data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdapterLoader{T}"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.				
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_operationQueue != null)
                            m_operationQueue.Dispose();

                        if (m_adapterWatcher != null)
                        {
                            m_adapterWatcher.Created -= AdapterWatcher_Created;
                            m_adapterWatcher.Dispose();
                        }

                        if (m_adapters != null)
                        {
                            lock (m_adapters)
                            {
                                T adapter;
                                IDisposable disposableAdapter;
                                while (m_adapters.GetEnumerator().MoveNext())
                                {
                                    adapter = m_adapters[0];
                                    disposableAdapter = adapter as IDisposable;
                                    if (disposableAdapter != null)
                                        disposableAdapter.Dispose();

                                    m_adapters.Remove(adapter);
                                }
                            }
                            m_adapters.CollectionChanged -= Adapters_CollectionChanged;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="AdapterLoaded"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="AdapterLoaded"/> event.</param>
        protected virtual void OnAdapterLoaded(T adapter)
        {
            if (AdapterLoaded != null)
                AdapterLoaded(this, new EventArgs<T>(adapter));
        }

        /// <summary>
        /// Raises the <see cref="AdapterUnloaded"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="AdapterUnloaded"/> event.</param>
        protected virtual void OnAdapterUnloaded(T adapter)
        {
            if (AdapterUnloaded != null)
                AdapterUnloaded(this, new EventArgs<T>(adapter));
        }

        /// <summary>
        /// Raises the <see cref="AdapterLoadException"/> event.
        /// </summary>
        /// <param name="adapter"><see cref="Type"/> to send to <see cref="AdapterLoadException"/> event.</param>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="AdapterLoadException"/> event.</param>
        protected virtual void OnAdapterLoadException(Type adapter, Exception exception)
        {
            if (AdapterLoadException != null)
                AdapterLoadException(this, new EventArgs<Type, Exception>(adapter, exception));
        }

        /// <summary>
        /// Raises the <see cref="OperationExecutionException"/> event.
        /// </summary>
        /// <param name="adapter">Adapter instance to send to <see cref="OperationExecutionException"/> event.</param>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="OperationExecutionException"/> event.</param>
        protected virtual void OnOperationExecutionException(T adapter, Exception exception)
        {
            if (OperationExecutionException != null)
                OperationExecutionException(this, new EventArgs<T, Exception>(adapter, exception));
        }

        private void ExecuteOperation(object[] data)
        {
            foreach (object operationData in data)
            {
                lock (m_adapters)
                {
                    foreach (T adapter in m_adapters)
                    {
                        try
                        {
                            ExecuteAdapterOperation(adapter, operationData);
                        }
                        catch (Exception ex)
                        {
                            OnOperationExecutionException(adapter, ex);
                        }
                    }
                }
            }
        }

        private void AdapterWatcher_Created(object sender, FileSystemEventArgs e)
        {
            // Process newly added assemblies at runtime for adapters.
            foreach (Type type in typeof(T).LoadImplementations(e.FullPath))
            {
                ProcessAdapter(type);
            }
        }

        private void Adapters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Notify additions.
                    foreach (T adapter in e.NewItems)
                    {
                        OnAdapterLoaded(adapter);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Notify deletions.
                    foreach (T adapter in e.OldItems)
                    {
                        OnAdapterUnloaded(adapter);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Notify deletions.
                    foreach (T adapter in e.OldItems)
                    {
                        OnAdapterUnloaded(adapter);
                    }
                    // Notify additions.
                    foreach (T adapter in e.NewItems)
                    {
                        OnAdapterLoaded(adapter);
                    }
                    break;
            }
        }

        #endregion
    }
}
