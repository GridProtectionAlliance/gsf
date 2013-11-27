//******************************************************************************************************
//  SignalReferenceMeasurement.cs - Gbtc
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
//  05/26/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Added generic type to handle values other than double.
//
//******************************************************************************************************

using System;
using GSF.TimeSeries;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents an <see cref="IMeasurement{T}"/> wrapper that is associated with a <see cref="SignalReference"/>.
    /// </summary>
    public class SignalReferenceMeasurement : IMeasurement<double>
    {
        #region [ Members ]

        // Fields
        private readonly IMeasurement<double> m_measurement;
        private readonly SignalReference m_signalReference;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="SignalReferenceMeasurement"/> from the specified parameters.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement{T}"/> value.</param>
        /// <param name="signalReference">Associated <see cref="SignalReference"/>.</param>
        public SignalReferenceMeasurement(IMeasurement<double> measurement, SignalReference signalReference)
        {
            m_measurement = measurement;
            m_signalReference = signalReference;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="SignalReferenceMeasurement"/>, if available.
        /// </summary>
        public Guid ID
        {
            get
            {
                return m_measurement.ID;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks Timestamp
        {
            get
            {
                return m_measurement.Timestamp;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get
            {
                return m_measurement.StateFlags;
            }
        }

        /// <summary>
        /// Gets the raw value of this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public double Value
        {
            get
            {
                return m_measurement.Value;
            }
        }

        /// Gets the raw value of this measurement
        object IMeasurement.Value
        {
            get
            {
                return m_measurement.Value;
            }
        }

        /// <summary>
        /// Gets the <see cref="SignalReference"/> associated with this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public SignalReference SignalReference
        {
            get
            {
                return m_signalReference;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a copy of the specified measurement using new state flags.
        /// </summary>
        /// <param name="stateFlags">New <see cref="MeasurementStateFlags"/></param>
        /// <returns>A copy of the <see cref="IMeasurement{T}"/> object.</returns>
        public IMeasurement<double> Alter(MeasurementStateFlags stateFlags)
        {
            return new SignalReferenceMeasurement(m_measurement, m_signalReference);
        }

        #endregion
    }
}
