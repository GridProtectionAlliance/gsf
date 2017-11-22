//******************************************************************************************************
//  FilterAdapterCollection.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/10/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using GSF.Data;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a collection of <see cref="IFilterAdapter"/> implementations.
    /// </summary>
    public class FilterAdapterCollection : AdapterCollectionBase<IFilterAdapter>, IFilterAdapter
    {
        #region [ Members ]

        // Fields
        private List<IFilterAdapter> m_sortedFilterAdapters;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="FilterAdapterCollection"/>.
        /// </summary>
        public FilterAdapterCollection()
        {
            base.Name = "Filter Adapter Collection";
            base.DataMember = "FilterAdapters";
            base.MonitorTimerEnabled = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the values that determines the order in which filter adapters are executed.
        /// </summary>
        public int ExecutionOrder { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handler for new measurements that have not yet been routed.
        /// </summary>
        /// <param name="measurements">Measurements that have not yet been routed.</param>
        public void HandleNewMeasurements(ICollection<IMeasurement> measurements)
        {
            List<IFilterAdapter> sortedFilterAdapters = m_sortedFilterAdapters;

            if ((object)sortedFilterAdapters == null)
            {
                lock (this)
                {
                    // Because we entered a lock, some time may have passed since
                    // the first time we checked for null so we should check again
                    sortedFilterAdapters = m_sortedFilterAdapters;

                    if ((object)sortedFilterAdapters == null)
                    {
                        sortedFilterAdapters = this
                            .Cast<IFilterAdapter>()
                            .OrderBy(adapter => adapter.ExecutionOrder)
                            .ToList();

                        m_sortedFilterAdapters = sortedFilterAdapters;
                    }
                }
            }

            foreach (IFilterAdapter adapter in sortedFilterAdapters)
            {
                if (adapter.Enabled)
                    adapter.HandleNewMeasurements(measurements);
            }
        }

        /// <summary>
        /// Attempts to create an <see cref="IAdapter"/> from the specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="adapterRow"><see cref="DataRow"/> containing item information to initialize.</param>
        /// <param name="adapter">Initialized adapter if successful; otherwise null.</param>
        /// <returns><c>true</c> if item was successfully initialized; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// See <see cref="AdapterCollectionBase{T}.DataSource"/> property for expected <see cref="DataRow"/> column names.
        /// </remarks>
        /// <exception cref="NullReferenceException"><paramref name="adapterRow"/> is null.</exception>
        public override bool TryCreateAdapter(DataRow adapterRow, out IFilterAdapter adapter)
        {
            bool success = base.TryCreateAdapter(adapterRow, out adapter);

            if (success && adapterRow.Table.Columns.Contains("LoadOrder"))
                adapter.ExecutionOrder = adapterRow.ConvertField<int>("LoadOrder");

            return success;
        }

        /// <summary>
        /// Inserts an element into the <see cref="Collection{T}"/> the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The <see cref="IAdapter"/> implementation to insert.</param>
        protected override void InsertItem(int index, IFilterAdapter item)
        {
            base.InsertItem(index, item);
            m_sortedFilterAdapters = null;
        }

        /// <summary>
        /// Assigns a new element to the <see cref="Collection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index for which item should be assigned.</param>
        /// <param name="item">The <see cref="IAdapter"/> implementation to assign.</param>
        protected override void SetItem(int index, IFilterAdapter item)
        {
            base.SetItem(index, item);
            m_sortedFilterAdapters = null;
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            m_sortedFilterAdapters = null;
        }

        #endregion
    }
}
