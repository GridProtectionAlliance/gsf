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
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TVA.IO;
using System.Collections.ObjectModel;

namespace TVA.Historian
{
    /// <summary>
    /// Represents a generic loader of adapters.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> of adapters to be loaded.</typeparam>
    public class AdapterLoader<T> : ISupportLifecycle
    {
        #region [ Members ]

        // Events

        ///// <summary>
        ///// Occurs when a new adapter is loaded.
        ///// </summary>
        ///// <remarks>
        ///// <see cref="EventArgs{T}.Argument"/> is the adapter that was loaded.
        ///// </remarks>
        //public event EventHandler<EventArgs<T>> AdapterLoaded;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when loading an adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered when loading an adapter.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> AdapterLoadException;

        // Fields
        private ObservableCollection<T> m_adapters;
        private FileSystemWatcher m_adapterWatcher;
        private bool m_disposed;
        private bool m_initialized;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterLoader{T}"/> class.
        /// </summary>
        public AdapterLoader()
        {
            m_adapters = new ObservableCollection<T>();
            m_adapterWatcher = new FileSystemWatcher();
            m_adapterWatcher.Path = FilePath.GetAbsolutePath("");
            m_adapterWatcher.Filter = "*.dll";
            m_adapterWatcher.Created += AdapterWatcher_Created;
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
        /// Gets or sets a boolean value that indicates whether the <see cref="AdapterLoader{T}"/> is currently enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_adapterWatcher.EnableRaisingEvents;
            }
            set
            {
                if (m_initialized)
                {
                    // Start or stop watching for adapters.
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
        /// Gets a list of loaded adapters.
        /// </summary>
        public IList<T> Adapters
        {
            get
            {
                return m_adapters;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public void Initialize()
        {
            if (!m_initialized)
            {
                // Process adapters.
                foreach (Type type in typeof(T).LoadImplementations())
                {
                    ProcessAdapter(type);
                }

                // Watch for adapters.
                m_adapterWatcher.EnableRaisingEvents = true;

                // Initialize only once.
                m_initialized = true; 
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="AdapterLoader{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///// <summary>
        ///// Raises the <see cref="AdapterLoaded"/> event.
        ///// </summary>
        ///// <param name="adapter">Adapter to send to <see cref="AdapterLoaded"/> event.</param>
        //protected virtual void OnAdapterLoaded(T adapter)
        //{
        //    if (AdapterLoaded != null)
        //        AdapterLoaded(this, new EventArgs<T>(adapter));
        //}

        /// <summary>
        /// Raises the <see cref="AdapterLoadException"/> event.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> to send to <see cref="AdapterLoadException"/> event.</param>
        protected virtual void OnAdapterLoadException(Exception exception)
        {
            if (AdapterLoadException != null)
                AdapterLoadException(this, new EventArgs<Exception>(exception));
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
                        if (m_adapterWatcher != null)
                            m_adapterWatcher.Dispose();

                        lock (m_adapters)
                        {
                            foreach (T adapter in m_adapters)
                            {
                                if (adapter is ISupportLifecycle)
                                    ((ISupportLifecycle)adapter).Dispose();
                                m_adapters.Remove(adapter);
                            }
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        private void ProcessAdapter(Type adapterType)
        {
            try
            {
                // Instantiate adapter instance.
                T adapterInstance = (T)(Activator.CreateInstance(adapterType));

                // Initialize adapter if supported.
                if (adapterInstance is ISupportLifecycle)
                    ((ISupportLifecycle)adapterInstance).Initialize();

                // Add adapter and notify via event.
                lock (m_adapters)
                {
                    m_adapters.Add(adapterInstance);
                }
                //OnAdapterLoaded(adapterInstance);
            }
            catch (Exception ex)
            {
                OnAdapterLoadException(ex);
            }
        }

        private void AdapterWatcher_Created(object sender, FileSystemEventArgs e)
        {
            // Process newly added assemblies at runtime for adapter.
            foreach (Type type in typeof(T).LoadImplementations(e.FullPath))
            {
                ProcessAdapter(type);
            }
        }

        #endregion
    }
}
