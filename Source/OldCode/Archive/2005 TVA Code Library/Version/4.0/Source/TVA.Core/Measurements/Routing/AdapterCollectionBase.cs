//*******************************************************************************************************
//  AdapterCollectionBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/06/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using TVA.IO;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents a collection of <see cref="IAdapter"/> implementations.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IAdapter"/> this collection contains.</typeparam>
    [CLSCompliant(false)]
    public abstract class AdapterCollectionBase<T> : Collection<T>, IAdapterCollection where T : IAdapter
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        /// <summary>
        /// Event is raised when there is an exception encountered while processing.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the exception that was thrown.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Fields
        private string m_name;
        private uint m_id;
        private bool m_initialized;
        private string m_connectionString;
        private Dictionary<string, string> m_settings;
        private DataSet m_dataSource;
        private string m_dataMember;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        protected AdapterCollectionBase()
        {
            m_name = this.GetType().Name;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="AdapterCollectionBase{T}"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~AdapterCollectionBase()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets numeric ID associated with this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual uint ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets flag indicating if the adapter collection has been initialized successfully.
        /// </summary>
        public virtual bool Initialized
        {
            get
            {
                return m_initialized;
            }
            set
            {
                m_initialized = value;
            }
        }

        /// <summary>
        /// Gets or sets key/value pair connection information specific to this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;

                // Preparse settings upon connection string assignment
                if (string.IsNullOrEmpty(m_connectionString))
                    m_settings = new Dictionary<string, string>();
                else
                    m_settings = m_connectionString.ParseKeyValuePairs();
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source used to load each <see cref="IAdapter"/>.
        /// Updates to this property will cascade to all items in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </remarks>
        public virtual DataSet DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                m_dataSource = value;
                
                // Update data source for items in this collection
                foreach (T item in this)
                {
                    item.DataSource = m_dataSource;
                }
            }
        }

        /// <summary>
        /// Gets or sets specific data member (e.g., table name) in <see cref="DataSource"/> used to <see cref="Initialize"/> this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        /// <remarks>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </remarks>
        public virtual string DataMember
        {
            get
            {
                return m_dataMember;
            }
            set
            {
                m_dataMember = value;
            }
        }

        /// <summary>
        /// Gets or sets enabled state of this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (m_enabled && !value)
                    Stop();
                else if (!m_enabled && value)
                    Start();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="AdapterCollectionBase{T}"/> is read-only.
        /// </summary>
        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets flag that detemines if <see cref="IAdapter"/> implementations are automatically initialized
        /// when they are added to the collection.
        /// </summary>
        protected virtual bool AutoInitialize
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets settings <see cref="Dictionary{TKey,TValue}"/> parsed when <see cref="ConnectionString"/> was assigned.
        /// </summary>
        protected Dictionary<string, string> Settings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Gets the descriptive status of this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                // Show collection status
                status.AppendFormat("  Total adapter components: {0}", Count);
                status.AppendLine();
                status.AppendFormat("    Collection initialized: {0}", m_initialized);
                status.AppendLine();
                status.AppendFormat(" Current operational state: {0}", (m_enabled ? "Enabled" : "Disabled"));
                status.AppendLine();
                status.AppendFormat("     Configuration defined: {0}", (m_dataSource != null));
                status.AppendLine();
                status.AppendFormat("    Referenced data source: {0}, {1} tables", DataSource.DataSetName, DataSource.Tables.Count);
                status.AppendLine();
                status.AppendFormat("    Data source table name: {0}", m_dataMember);
                status.AppendLine();

                if (Count > 0)
                {
                    int index = 0;

                    status.AppendLine();
                    status.AppendFormat("Status of each {0} component:", Name);
                    status.AppendLine();
                    status.Append(new string('-', 79));
                    status.AppendLine();

                    // Show the status of registered components.
                    foreach (T item in this)
                    {
                        IProvideStatus statusProvider = item as IProvideStatus;

                        if (statusProvider != null)
                        {
                            // This component provides status information.                       
                            status.AppendLine();
                            status.AppendFormat("Status of {0} component {1}, {2}:", typeof(T).Name, ++index, statusProvider.Name);
                            status.AppendLine();
                            status.Append(statusProvider.Status);
                        }
                    }

                    status.AppendLine();
                    status.Append(new string('-', 79));
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="AdapterCollectionBase{T}"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AdapterCollectionBase{T}"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        Clear();        // This disposes all items in collection...
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Loads all <see cref="IAdapter"/> implementations defined in <see cref="DataSource"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Table name specified in <see cref="DataMember"/> from <see cref="DataSource"/> is expected
        /// to have the following table column names:<br/>
        /// ID, AdapterName, AssemblyName, TypeName, ConnectionString<br/>
        /// ID column type should be integer based, all other column types are expected to be string based.
        /// </para>
        /// <para>
        /// Note that when calling this method any existing items will be cleared allowing a "re-initialize".
        /// </para>
        /// </remarks>
        /// <exception cref="NullReferenceException">DataSource is null.</exception>
        /// <exception cref="InvalidOperationException">DataMember is null or empty.</exception>
        public virtual void Initialize()
        {
            if (m_dataSource == null)
                throw new NullReferenceException(string.Format("DataSource is null, cannot load {0}.", Name));

            if (string.IsNullOrEmpty(m_dataMember))
                throw new InvalidOperationException(string.Format("DataMember is null or empty, cannot load {0}.", Name));

            T item;

            Clear();

            foreach (DataRow adapterRow in m_dataSource.Tables[m_dataMember].Rows)
            {
                if (TryCreateAdapter(adapterRow, out item))
                    Add(item);
            }

            m_initialized = true;
        }

        /// <summary>
        /// Attempts to create an <see cref="IAdapter"/> from the specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="adapterRow"><see cref="DataRow"/> containing item information to initialize.</param>
        /// <param name="adapter">Initialized adapter if successful; otherwise null.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// See <see cref="DataSource"/> property for expected <see cref="DataRow"/> column names.
        /// </remarks>
        /// <exception cref="NullReferenceException"><paramref name="adapterRow"/> is null.</exception>
        public virtual bool TryCreateAdapter(DataRow adapterRow, out T adapter)
        {
            if (adapterRow == null)
                throw new NullReferenceException(string.Format("Cannot initialize from null adpater DataRow"));

            Assembly assembly;
            string adapterName = "", assemblyName = "", typeName = "", connectionString;
            uint id;

            try
            {
                adapterName = adapterRow["AdapterName"].ToNonNullString("[IAdapter]");
                assemblyName = FilePath.GetAbsolutePath(adapterRow["AssemblyName"].ToNonNullString());
                typeName = adapterRow["TypeName"].ToNonNullString();
                connectionString = adapterRow["ConnectionString"].ToNonNullString();
                id = uint.Parse(adapterRow["ID"].ToNonNullString("0"));

                if (string.IsNullOrEmpty(typeName))
                    throw new InvalidOperationException("Type was undefined");

                if (!File.Exists(assemblyName))
                    throw new InvalidOperationException("Assembly does not exist.");

                assembly = Assembly.LoadFrom(assemblyName);
                adapter = (T)Activator.CreateInstance(assembly.GetType(typeName));

                adapter.Name = adapterName;
                adapter.ID = id;
                adapter.ConnectionString = connectionString;
                adapter.DataSource = m_dataSource;

                return true;
            }
            catch (Exception ex)
            {
                // We report any errors encountered during type creation...
                OnProcessException(new InvalidOperationException(string.Format("Failed to load adapter \"{0}\" [{1}] from \"{2}\": {3}", adapterName, typeName, assemblyName, ex.Message), ex));
            }

            adapter = default(T);
            return false;
        }

        // Explicit IAdapter implementation of TryCreateAdapter
        bool IAdapterCollection.TryCreateAdapter(DataRow adapterRow, out IAdapter adapter)
        {
            T adapterT;
            bool result = TryCreateAdapter(adapterRow, out adapterT);
            adapter = adapterT as IAdapter;
            return result;
        }

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="ID"/>.
        /// </summary>
        /// <param name="ID">ID of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="ID"/> was found; otherwise <c>false</c>.</returns>
        public virtual bool TryGetAdapterByID(uint ID, out T adapter)
        {
            return TryGetAdapter<uint>(ID, (item, value) => item.ID == value, out adapter);
        }

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of adapter to get.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="name"/> was found; otherwise <c>false</c>.</returns>
        public virtual bool TryGetAdapterByName(string name, out T adapter)
        {
            return TryGetAdapter<string>(name, (item, value) => string.Compare(item.Name, value, true) == 0, out adapter);
        }

        /// <summary>
        /// Attempts to get the adapter with the specified <paramref name="value"/> given <paramref name="testItem"/> function.
        /// </summary>
        /// <param name="value">Value of adapter to get.</param>
        /// <param name="testItem">Function delegate used to test item <paramref name="value"/>.</param>
        /// <param name="adapter">Adapter reference if found; otherwise null.</param>
        /// <returns><c>true</c> if adapter with the specified <paramref name="value"/> was found; otherwise <c>false</c>.</returns>
        protected virtual bool TryGetAdapter<TValue>(TValue value, Func<T, TValue, bool> testItem, out T adapter)
        {
            foreach (T item in this)
            {
                if (testItem(item, value))
                {
                    adapter = item;
                    return true;
                }
            }

            adapter = default(T);
            return false;
        }

        // Explicit IAdapter implementation of TryGetAdapterByID
        bool IAdapterCollection.TryGetAdapterByID(uint ID, out IAdapter adapter)
        {
            T adapterT;
            bool result = TryGetAdapterByID(ID, out adapterT);
            adapter = adapterT as IAdapter;
            return result;
        }

        // Explicit IAdapter implementation of TryGetAdapterByName
        bool IAdapterCollection.TryGetAdapterByName(string name, out IAdapter adapter)
        {
            T adapterT;
            bool result = TryGetAdapterByName(name, out adapterT);
            adapter = adapterT as IAdapter;
            return result;
        }

        /// <summary>
        /// Attempts to initialize (or reinitialize) an individual <see cref="IAdapter"/> based on its ID.
        /// </summary>
        /// <param name="id">The numeric ID associated with the <see cref="IAdapter"/> to be initialized.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        public virtual bool TryInitializeAdapterByID(uint id)
        {
            T newAdapter, oldAdapter;
            uint rowID;

            foreach (DataRow adapterRow in m_dataSource.Tables[m_dataMember].Rows)
            {
                rowID = uint.Parse(adapterRow["ID"].ToNonNullString("0"));

                if (rowID == id)
                {
                    if (TryCreateAdapter(adapterRow, out newAdapter))
                    {
                        // Found and created new item - update collection reference
                        bool foundItem = false;

                        for (int i = 0; i < Count; i++)
                        {
                            oldAdapter = this[i];

                            if (oldAdapter.ID == id)
                            {
                                // Cache original running state
                                bool enabled = oldAdapter.Enabled;

                                // Stop old item
                                oldAdapter.Stop();

                                // Dispose old item, initialize new item
                                this[i] = newAdapter;

                                // If old item was running, start new item
                                if (enabled)
                                    newAdapter.Start();

                                foundItem = true;
                                break;
                            }
                        }

                        // Add item to collection if it didn't exist
                        if (!foundItem)
                            Add(newAdapter);

                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Starts each <see cref="IAdapter"/> implementation in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual void Start()
        {
            m_enabled = true;

            foreach (T item in this)
            {
                item.Start();
            }
        }
        /// <summary>
        /// Stops each <see cref="IAdapter"/> implementation in this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual void Stop()
        {
            m_enabled = false;

            foreach (T item in this)
            {
                item.Stop();
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public virtual string GetShortStatus(int maxLength)
        {
            return string.Format("Total components: {0}", Count.ToString().PadLeft(5)).PadLeft(maxLength);
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        /// <param name="status">New status message.</param>
        protected virtual void OnStatusMessage(string status)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(status));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event with a formatted status message.
        /// </summary>
        /// <param name="formattedStatus">Formatted status message.</param>
        /// <param name="args">Arguments for <paramref name="formattedStatus"/>.</param>
        /// <remarks>
        /// This overload combines string.Format and SendStatusMessage for convienence.
        /// </remarks>
        protected virtual void OnStatusMessage(string formattedStatus, params object[] args)
        {
            if (StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(string.Format(formattedStatus, args)));
        }

        /// <summary>
        /// Raises <see cref="ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected virtual void OnProcessException(Exception ex)
        {
            if (ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Removes all elements from the <see cref="Collection{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            // Dispose each item before clearing the collection
            foreach (T item in this)
            {
                DisposeItem(item);
            }

            base.ClearItems();
        }

        /// <summary>
        /// Inserts an element into the <see cref="Collection{T}"/> the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The <see cref="IAdapter"/> implementation to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            // Wire up item events and handle item initialization
            InitializeItem(item);
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Assigns a new element to the <see cref="Collection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index for which item should be assigned.</param>
        /// <param name="item">The <see cref="IAdapter"/> implementation to assign.</param>
        protected override void SetItem(int index, T item)
        {
            // Dispose of existing item
            DisposeItem(this[index]);

            // Wire up item events and handle initialization of new item
            InitializeItem(item);

            base.SetItem(index, item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            // Dispose of item before removing it from the collection
            DisposeItem(this[index]);
            base.RemoveItem(index);
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IAdapter"/> implementation.</param>
        /// <remarks>
        /// Derived classes should override if more events are defined.
        /// </remarks>
        protected virtual void InitializeItem(T item)
        {
            if (item != null)
            {
                // Wire up events
                item.StatusMessage += StatusMessage;
                item.ProcessException += ProcessException;

                // If automatically initializing new elements, handle object initialization from
                // thread pool so it can take needed amount of time
                if (AutoInitialize)
                    ThreadPool.QueueUserWorkItem(InitializeItem, item);
            }
        }

        // Thread pool delegate to handle item initialization
        private void InitializeItem(object state)
        {
            T item = (T)state;

            try
            {
                item.Initialize();
                item.Initialized = true;
            }
            catch (Exception ex)
            {
                // We report any errors encountered during initialization...
                OnProcessException(ex);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IAdapter"/> to dispose.</param>
        /// <remarks>
        /// Derived classes should override if more events are defined.
        /// </remarks>
        protected virtual void DisposeItem(T item)
        {
            if (item != null)
            {
                // Un-wire events
                item.StatusMessage -= StatusMessage;
                item.ProcessException -= ProcessException;
                item.Dispose();
            }
        }

        #region [ Explicit ICollection<IAdapter> Implementation ]

        void ICollection<IAdapter>.Add(IAdapter item)
        {
            Add((T)item);
        }

        bool ICollection<IAdapter>.Contains(IAdapter item)
        {
            return Contains((T)item);
        }

        void ICollection<IAdapter>.CopyTo(IAdapter[] array, int arrayIndex)
        {
            CopyTo(array.Cast<T>().ToArray(), arrayIndex);
        }

        bool ICollection<IAdapter>.Remove(IAdapter item)
        {
            return Remove((T)item);
        }

        IEnumerator<IAdapter> IEnumerable<IAdapter>.GetEnumerator()
        {
            foreach (IAdapter item in this)
            {
                yield return item;
            }
        }

        #endregion

        #endregion
    }
}