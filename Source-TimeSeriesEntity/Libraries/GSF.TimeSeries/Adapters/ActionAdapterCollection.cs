//******************************************************************************************************
//  ActionAdapterCollection.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  11/04/2013 - Stephen C. Wills
//       Updated to process time-series entities.
//
//******************************************************************************************************

using System;
using GSF.TimeSeries.Routing;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Represents a collection of <see cref="IActionAdapter"/> implementations.
    /// </summary>
    public class ActionAdapterCollection : AdapterCollectionBase<IActionAdapter>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// This event will be raised when there are new time-series entities available from the action adapter.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is collection of new entities for host to process.
        /// </remarks>
        public event EventHandler<RoutingEventArgs> NewEntities;

        /// <summary>
        /// This event is raised every five seconds allowing consumer to track current number of unpublished seconds of data in the queue.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the total number of unpublished seconds of data.
        /// </remarks>
        public event EventHandler<EventArgs<int>> UnpublishedSamples;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="ActionAdapterCollection"/>.
        /// </summary>
        public ActionAdapterCollection()
        {
            base.Name = "Action Adapter Collection";
            base.DataMember = "ActionAdapters";
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Raises the <see cref="UnpublishedSamples"/> event.
        /// </summary>
        /// <param name="unprocessedEntities">Total number of unpublished seconds of data in the queue.</param>
        protected virtual void OnUnprocessedEntities(int unprocessedEntities)
        {
            try
            {
                if ((object)UnpublishedSamples != null)
                    UnpublishedSamples(this, new EventArgs<int>(unprocessedEntities));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for UnpublishedSamples event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Wires events and initializes new <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item">New <see cref="IActionAdapter"/> implementation.</param>
        protected override void InitializeItem(IActionAdapter item)
        {
            if ((object)item != null)
            {
                // Wire up events
                item.NewEntities += item_NewEntities;
                item.UnpublishedSamples += item_UnprocessedSamples;
                base.InitializeItem(item);
            }
        }

        /// <summary>
        /// Unwires events and disposes of <see cref="IActionAdapter"/> implementation.
        /// </summary>
        /// <param name="item"><see cref="IActionAdapter"/> to dispose.</param>
        protected override void DisposeItem(IActionAdapter item)
        {
            if ((object)item != null)
            {
                // Un-wire events
                item.NewEntities -= item_NewEntities;
                item.UnpublishedSamples -= item_UnprocessedSamples;
                base.DisposeItem(item);
            }
        }

        // Raise new entities event on behalf of each item in collection
        private void item_NewEntities(object sender, RoutingEventArgs e)
        {
            if ((object)NewEntities != null)
                NewEntities(sender, e);
        }

        // Raise unpublished samples event on behalf of each item in collection
        private void item_UnprocessedSamples(object sender, EventArgs<int> e)
        {
            if ((object)UnpublishedSamples != null)
                UnpublishedSamples(sender, e);
        }

        #endregion
    }
}