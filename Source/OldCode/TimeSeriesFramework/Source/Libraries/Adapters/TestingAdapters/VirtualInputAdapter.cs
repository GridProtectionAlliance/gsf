//******************************************************************************************************
//  VirtualInputAdapter.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  11/16/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.ComponentModel;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;

namespace TestingAdapters
{
    /// <summary>
    /// Represents a virtual input adapter used for testing purposes - no data gets produced.
    /// </summary>
    [Description("Virtual: defines a testing input that does not provide measurements.")]
    public class VirtualInputAdapter : InputAdapterBase
    {
        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if this <see cref="VirtualInputAdapter"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="VirtualInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            // In case user defines no inputs or outputs for virutal adapter, we "turn off" interaction with any
            // other real-time adapters by removing this virtual adapter from external routes. To accomplish this
            // we expose I/O demands for an undefined measurement. Leaving values assigning to null would mean
            // that this adapter desires a full "broadcast" of all data - and hence routing demands from all.
            // User can override if desired using standard connection string parameters for I/O measurements.
            InputMeasurementKeys = new MeasurementKey[] { MeasurementKey.Undefined };
            OutputMeasurements = new Measurement[] { Measurement.Undefined };

            base.Initialize();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="VirtualInputAdapter"/>.
        /// </summary>
        public override string GetShortStatus(int maxLength)
        {
            return "Virtual input adapter happily exists...".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to this <see cref="VirtualInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
        }

        /// <summary>
        /// Attempts to disconnect to this <see cref="VirtualInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
        }

        #endregion
    }
}
