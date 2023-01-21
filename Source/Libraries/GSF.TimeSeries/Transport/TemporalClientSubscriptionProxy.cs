//******************************************************************************************************
//  TemporalClientSubscriptionProxy.cs - Gbtc
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
//  08/27/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.ComponentModel;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents an action adapter that exists within a temporal <see cref="IaonSession"/> to proxy data back to its parent <see cref="IClientSubscription"/> instance.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TemporalClientSubscriptionProxy : FacileActionAdapterBase
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => true;

        /// <summary>
        /// Gets or sets parent subscription for the proxy used to deliver data.
        /// </summary>
        public IClientSubscription Parent { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Proxies measurements to parent adapter for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            if (Parent is not null)
                Parent.QueueMeasurementsForProcessing(measurements);
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="TemporalClientSubscriptionProxy"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status for this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            int inputCount = 0, outputCount = 0;

            if (InputMeasurementKeys is not null)
                inputCount = InputMeasurementKeys.Length;

            if (OutputMeasurements is not null)
                outputCount = OutputMeasurements.Length;

            return $"Total input measurements: {inputCount}, total output measurements: {outputCount}".PadLeft(maxLength);
        }

        #endregion
    }
}
