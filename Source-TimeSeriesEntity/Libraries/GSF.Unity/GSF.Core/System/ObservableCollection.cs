using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
namespace System.Collections.ObjectModel
{
    /// <summary>Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.</summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [Serializable,__DynamicallyInvokable]
    public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        // Fields
        private SimpleMonitor _monitor;
        [NonSerialized]
        private NotifyCollectionChangedEventHandler m_collectionChanged;
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        [NonSerialized]
        private PropertyChangedEventHandler m_propertyChanged;

        // Events
        [__DynamicallyInvokable]
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            [__DynamicallyInvokable]
            add
            {
                NotifyCollectionChangedEventHandler handler2;
                NotifyCollectionChangedEventHandler collectionChanged = m_collectionChanged;
                do
                {
                    handler2 = collectionChanged;
                    NotifyCollectionChangedEventHandler handler3 = (NotifyCollectionChangedEventHandler)Delegate.Combine(handler2, value);
                    collectionChanged = Interlocked.CompareExchange<NotifyCollectionChangedEventHandler>(ref m_collectionChanged, handler3, handler2);
                }
                while (collectionChanged != handler2);
            }
            [__DynamicallyInvokable]
            remove
            {
                NotifyCollectionChangedEventHandler handler2;
                NotifyCollectionChangedEventHandler collectionChanged = m_collectionChanged;
                do
                {
                    handler2 = collectionChanged;
                    NotifyCollectionChangedEventHandler handler3 = (NotifyCollectionChangedEventHandler)Delegate.Remove(handler2, value);
                    collectionChanged = Interlocked.CompareExchange<NotifyCollectionChangedEventHandler>(ref m_collectionChanged, handler3, handler2);
                }
                while (collectionChanged != handler2);
            }
        }

        [__DynamicallyInvokable]
        protected event PropertyChangedEventHandler PropertyChanged
        {
            [__DynamicallyInvokable]
            add
            {
                PropertyChangedEventHandler handler2;
                PropertyChangedEventHandler propertyChanged = m_propertyChanged;
                do
                {
                    handler2 = propertyChanged;
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler)Delegate.Combine(handler2, value);
                    propertyChanged = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref m_propertyChanged, handler3, handler2);
                }
                while (propertyChanged != handler2);
            }
            [__DynamicallyInvokable]
            remove
            {
                PropertyChangedEventHandler handler2;
                PropertyChangedEventHandler propertyChanged = m_propertyChanged;
                do
                {
                    handler2 = propertyChanged;
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler)Delegate.Remove(handler2, value);
                    propertyChanged = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref m_propertyChanged, handler3, handler2);
                }
                while (propertyChanged != handler2);
            }
        }

        [__DynamicallyInvokable]
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            add
            {
                m_propertyChanged += value;
            }
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            remove
            {
                m_propertyChanged -= value;
            }
        }

        // Methods
        [__DynamicallyInvokable]
        public ObservableCollection()
        {
            this._monitor = new SimpleMonitor();
        }

        [__DynamicallyInvokable]
        public ObservableCollection(IEnumerable<T> collection)
        {
            this._monitor = new SimpleMonitor();
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.CopyFrom(collection);
        }

        public ObservableCollection(List<T> list)
            : base((list != null) ? new List<T>(list.Count) : list)
        {
            this._monitor = new SimpleMonitor();
            this.CopyFrom(list);
        }

        [__DynamicallyInvokable]
        protected IDisposable BlockReentrancy()
        {
            this._monitor.Enter();
            return this._monitor;
        }

        [__DynamicallyInvokable]
        protected void CheckReentrancy()
        {
            if ((this._monitor.Busy && (m_collectionChanged != null)) && (m_collectionChanged.GetInvocationList().Length > 1))
            {
                throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
            }
        }

        [__DynamicallyInvokable]
        protected override void ClearItems()
        {
            this.CheckReentrancy();
            base.ClearItems();
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionReset();
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            IList<T> items = base.Items;
            if ((collection != null) && (items != null))
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }
                }
            }
        }

        [__DynamicallyInvokable]
        protected override void InsertItem(int index, T item)
        {
            this.CheckReentrancy();
            base.InsertItem(index, item);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Move(int oldIndex, int newIndex)
        {
            this.MoveItem(oldIndex, newIndex);
        }

        [__DynamicallyInvokable]
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            this.CheckReentrancy();
            T item = base[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
        }

        [__DynamicallyInvokable]
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (m_collectionChanged != null)
            {
                using (this.BlockReentrancy())
                {
                    m_collectionChanged(this, e);
                }
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [__DynamicallyInvokable]
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (m_propertyChanged != null)
            {
                m_propertyChanged(this, e);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        [__DynamicallyInvokable]
        protected override void RemoveItem(int index)
        {
            this.CheckReentrancy();
            T item = base[index];
            base.RemoveItem(index);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        [__DynamicallyInvokable]
        protected override void SetItem(int index, T item)
        {
            this.CheckReentrancy();
            T oldItem = base[index];
            base.SetItem(index, item);
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
        }

        // Nested Types
        [Serializable]
        private class SimpleMonitor : IDisposable
        {
            // Fields
            private int _busyCount;

            // Methods
            public void Dispose()
            {
                this._busyCount--;
            }

            public void Enter()
            {
                this._busyCount++;
            }

            // Properties
            public bool Busy
            {
                get
                {
                    return (this._busyCount > 0);
                }
            }
        }
    }
}
