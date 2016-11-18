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
    /// the core functionality for handling <see cref="MeasurementMetadata"/> implementations.
    /// </remarks>
    [Serializable]
    public abstract class MeasurementBase
    {
        #region [ Members ]

        // Fields
        private MeasurementMetadata m_measurementMetadata;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="Measurement"/> using default settings.
        /// </summary>
        protected MeasurementBase()
        {
            m_measurementMetadata = MeasurementMetadata.Undefined;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="Guid"/> based signal ID of the <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public Guid ID => m_measurementMetadata.Key.SignalID;

        /// <summary>
        /// Gets the primary <see cref="MeasurementKey"/> of this <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public MeasurementKey Key => m_measurementMetadata.Key;

        /// <summary>
        /// Gets the text based tag name of this <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public string TagName => m_measurementMetadata.TagName;

        /// <summary>
        /// Gets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        [DefaultValue(0.0)]
        public double Adder => m_measurementMetadata.Adder;

        /// <summary>
        /// Gets a multiplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        [DefaultValue(1.0)]
        public double Multiplier => m_measurementMetadata.Multiplier;

        /// <summary>
        /// Gets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        public MeasurementValueFilterFunction MeasurementValueFilter => m_measurementMetadata.MeasurementValueFilter;

        /// <summary>
        /// Gets or sets associated metadata values for the <see cref="MeasurementBase"/> implementation.
        /// </summary>
        public MeasurementMetadata MeasurementMetadata
        {
            get
            {
                return m_measurementMetadata;
            }
            set
            {
                m_measurementMetadata = value;
            }
        }

        #endregion
    }
}