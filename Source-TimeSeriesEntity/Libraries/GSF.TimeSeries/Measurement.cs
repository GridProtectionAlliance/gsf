//******************************************************************************************************
//  Measurement.cs - Gbtc
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
//  04/14/2011 - J. Ritchie Carroll
//       Added received and published timestamps for measurements.
//  10/16/2013 - Stephen C. Wills
//       Redefined the Measurement class to Measurement<T> to allow for measurements with values of
//       different types. Also simplified the class by removing extraneous fields and properties.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Implementation of a basic measurement.
    /// </summary>
    [Serializable]
    public class Measurement<T> : TimeSeriesEntityBase, IMeasurement<T>
    {
        #region [ Members ]

        // Fields
        private readonly MeasurementStateFlags m_stateFlags;
        private readonly T m_value;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Measurement{T}"/> class.
        /// </summary>
        /// <param name="id">The fundamental identifier of the <see cref="Measurement{T}"/></param>
        /// <param name="timestamp">The exact timestamp, in ticks, of the data represented by this <see cref="Measurement{T}"/></param>
        /// <param name="stateFlags">The <see cref="MeasurementStateFlags"/> associated with this <see cref="Measurement{T}"/></param>
        /// <param name="value">The raw value of this <see cref="Measurement{T}"/></param>
        public Measurement(Guid id, Ticks timestamp, MeasurementStateFlags stateFlags, T value)
            : base(id, timestamp)
        {
            m_stateFlags = stateFlags;
            m_value = value;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Measurement{T}"/> class.
        /// </summary>
        /// <param name="measurement">Measurement to be copied or converted.</param>
        public Measurement(IMeasurement<T> measurement)
            : this(measurement.ID, measurement.Timestamp, measurement.StateFlags, measurement.Value)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="MeasurementStateFlags"/> associated with this <see cref="Measurement{T}"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get
            {
                return m_stateFlags;
            }
        }

        /// <summary>
        /// Gets the raw value of this <see cref="Measurement{T}"/>.
        /// </summary>
        public T Value
        {
            get
            {
                return m_value;
            }
        }

        /// <summary>
        /// Gets the raw value of this <see cref="IMeasurement"/>.
        /// </summary>
        object IMeasurement.Value
        {
            get
            {
                return m_value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a copy of the specified measurement using new state flags.
        /// </summary>
        /// <param name="stateFlags">New <see cref="MeasurementStateFlags"/></param>
        /// <returns>A copy of the <see cref="IMeasurement{T}"/> object.</returns>
        public IMeasurement<T> Alter(MeasurementStateFlags stateFlags)
        {
            return new Measurement<T>(ID, Timestamp, stateFlags, Value);
        }

        #endregion
    }
}
