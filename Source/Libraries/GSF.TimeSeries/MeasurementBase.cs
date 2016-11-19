//******************************************************************************************************
//  MeasurementBase.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/18/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a base class for <see cref="IMeasurement"/> implementations.
    /// </summary>
    /// <remarks>
    /// This is not a full implementation of <see cref="IMeasurement"/> - this class only defines
    /// the core functionality for handling <see cref="Metadata"/> implementations.
    /// </remarks>
    [Serializable]
    public abstract class MeasurementBase
    {
        #region [ Members ]

        // Fields
        private double m_value;
        private Ticks m_timestamp;
        private MeasurementMetadata m_metadata;
        private MeasurementStateFlags m_stateFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> using default settings.
        /// </summary>
        protected MeasurementBase()
        {
            m_metadata = MeasurementMetadata.Undefined;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="Guid"/> based signal ID of the <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public Guid ID => m_metadata.Key.SignalID;

        /// <summary>
        /// Gets the primary <see cref="MeasurementKey"/> of this <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public MeasurementKey Key => m_metadata.Key;

        /// <summary>
        /// Gets the text based tag name of this <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public string TagName => m_metadata.TagName;

        /// <summary>
        /// Gets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        [DefaultValue(0.0)]
        public double Adder => m_metadata.Adder;

        /// <summary>
        /// Gets a multiplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        [DefaultValue(1.0)]
        public double Multiplier => m_metadata.Multiplier;

        /// <summary>
        /// Gets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        public MeasurementValueFilterFunction MeasurementValueFilter => m_metadata.MeasurementValueFilter;

        /// <summary>
        /// Gets or sets associated metadata values for the <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public MeasurementMetadata Metadata
        {
            get
            {
                return m_metadata;
            }
            set
            {
                m_metadata = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="IMeasurement"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get
            {
                return m_stateFlags;
            }
            set
            {
                m_stateFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the raw measurement value that is not offset by <see cref="MeasurementBase.Adder"/> and <see cref="MeasurementBase.Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="Measurement"/> (i.e., value that is not offset by <see cref="MeasurementBase.Adder"/> and <see cref="MeasurementBase.Multiplier"/>).</returns>
        public double Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="Measurement"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        #endregion
    }
}