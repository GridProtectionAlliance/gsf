#region [ Using ]
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
#endregion

namespace GSF.TestsSuite.TimeSeries.Cases
{
    public class IAdapterCollectionCase
    {
        #region [ Members ]
        private List<IAdapter> items;
        private IInputAdapterCase m_CsvInputAdapter;
        private IOutputAdapterCase m_CsvOutputAdapter;
        private IAdapterCollection m_InputAdapterCollection;
        private ReadOnlyCollection<IAdapter> m_ReadOnlyCollection;
        #endregion [ Members ]

        #region [ Properties ]
        public IAdapterCollection AdapterCollection
        {
            get
            {
                return m_InputAdapterCollection;
            }
        }

        /// <summary>
        /// Provides testing interface for read only adapters collection
        /// </summary>
        public ReadOnlyCollection<IAdapter> ReadOnlyCollection
        {
            get
            {
                return m_ReadOnlyCollection;
            }
        }

        #endregion [ Properties ]

        #region [ Constructors ]
        public IAdapterCollectionCase()
        {
            //Initialize adapters for collection;
            m_CsvInputAdapter = new IInputAdapterCase();
            m_CsvOutputAdapter = new IOutputAdapterCase();

            //Initialize collection
            m_InputAdapterCollection = new InputAdapterCollection(false);
            m_InputAdapterCollection.Add(m_CsvInputAdapter);

            //Initialize ReadOnly collection
            items = new List<IAdapter>();
            items.Add(m_CsvInputAdapter);
            m_ReadOnlyCollection = new ReadOnlyCollection<IAdapter>(items);
        }

        #endregion [ Constructors ]

        #region [ Methods ]
        #endregion [ Methods ]

        #region [ Dispose ]
        private bool isDisposed = false;

        ~IAdapterCollectionCase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    items.Clear();
                    m_CsvOutputAdapter.Dispose();
                    m_CsvInputAdapter.Dispose();
                    m_InputAdapterCollection.Dispose();
                }
                isDisposed = false;
            }
        }

        #endregion [ Dispose ]
    }
}