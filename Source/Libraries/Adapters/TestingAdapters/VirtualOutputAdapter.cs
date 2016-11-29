//******************************************************************************************************
//  VirtualOutputAdapter.cs - Gbtc
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
//  05/04/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.ComponentModel;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace TestingAdapters
{
    /// <summary>
    /// Represents a virtual historian output adapter used for testing purposes - no data gets archived.
    /// </summary>
    [Description("Virtual: Defines a testing output that does not archive measurements")]
    public class VirtualOutputAdapter : OutputAdapterBase
    {
        #region [ Properties ]

        /// <summary>
        /// Causes this adapter to receive no measurements
        /// </summary>
        public override MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return new MeasurementKey[0];
            }
            set
            {

            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this <see cref="VirtualOutputAdapter"/> are
        /// destined for archival.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="VirtualOutputAdapter"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to connect to this <see cref="VirtualOutputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="VirtualOutputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
        }

        /// <summary>
        /// Serializes measurements to data output stream.
        /// </summary>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="VirtualOutputAdapter"/>.
        /// </summary>
        public override string GetShortStatus(int maxLength)
        {
            return "Virtual historian is sending all data to null...".CenterText(maxLength);
        }

        #endregion
    }
}