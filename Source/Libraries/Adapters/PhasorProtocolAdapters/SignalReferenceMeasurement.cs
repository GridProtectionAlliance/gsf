//******************************************************************************************************
//  SignalReferenceMeasurement.cs - Gbtc
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
//  05/26/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF;
using GSF.TimeSeries;
using GSF.Units.EE;

namespace PhasorProtocolAdapters
{
    /// <summary>
    /// Represents an <see cref="IMeasurement"/> wrapper that is associated with a <see cref="SignalReference"/>.
    /// </summary>
    internal sealed class SignalReferenceMeasurement : IMeasurement
    {
        #region [ Members ]

        // Fields
        private readonly IMeasurement m_measurement;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="SignalReferenceMeasurement"/> from the specified parameters.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="signalReference">Associated <see cref="SignalReference"/>.</param>
        public SignalReferenceMeasurement(IMeasurement measurement, SignalReference signalReference)
        {
            m_measurement = measurement;
            SignalReference = signalReference;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="SignalReference"/> associated with this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public SignalReference SignalReference { get; }


        /// <summary>
        /// Gets the primary key (a <see cref="MeasurementKey"/>, of this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public MeasurementKey Key => m_measurement.Key;

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="SignalReferenceMeasurement"/>, if available.
        /// </summary>
        public Guid ID => m_measurement.ID;

        /// <summary>
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get => m_measurement.StateFlags;
            set => m_measurement.StateFlags = value;
        }

        /// <summary>
        /// Gets the text based tag name of this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        public string TagName => m_measurement.TagName;

        /// <summary>
        /// Gets or sets the raw measurement value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="SignalReferenceMeasurement"/> (i.e., value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>).</returns>
        public double Value
        {
            get => m_measurement.Value;
            set => m_measurement.Value = value;
        }

        /// <summary>
        /// Gets the adjusted numeric value of this measurement, taking into account the specified <see cref="Adder"/> and <see cref="Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// Note that returned value will be offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </remarks>
        /// <returns><see cref="Value"/> offset by <see cref="Adder"/> and <see cref="Multiplier"/> (i.e., <c><see cref="Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>).</returns>
        public double AdjustedValue => m_measurement.AdjustedValue;

        /// <summary>
        /// Gets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        public double Adder => m_measurement.Adder;

        /// <summary>
        /// Gets a multiplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        public double Multiplier => m_measurement.Multiplier;

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks Timestamp
        {
            get => m_measurement.Timestamp;
            set => m_measurement.Timestamp = value;
        }

        /// <summary>
        /// Gets the life-span of this <see cref="SignalReferenceMeasurement"/> since its creation.
        /// </summary>
        public ShortTime Lifespan => m_measurement.Lifespan;

        /// <summary>
        /// Gets timestamp, in ticks, of when this <see cref="SignalReferenceMeasurement"/> was created.
        /// </summary>
        public Ticks CreatedTimestamp => m_measurement.CreatedTimestamp;

        /// <summary>
        /// Gets function used to apply a down-sampling filter over a sequence of <see cref="SignalReferenceMeasurement"/> values.
        /// </summary>
        public MeasurementValueFilterFunction MeasurementValueFilter => m_measurement.MeasurementValueFilter;

        /// <summary>
        /// Gets or sets associated metadata values for the <see cref="SignalReferenceMeasurement"/> .
        /// </summary>
        public MeasurementMetadata Metadata
        {
            get => m_measurement.Metadata;
            set => m_measurement.Metadata = value;
        }

        BigBinaryValue ITimeSeriesValue.Value
        {
            get => ((ITimeSeriesValue)m_measurement).Value;
            set => ((ITimeSeriesValue)m_measurement).Value = value;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the specified <see cref="ITimeSeriesValue"/> is equal to the current <see cref="SignalReferenceMeasurement"/>.
        /// </summary>
        /// <param name="other">The <see cref="ITimeSeriesValue"/> to compare with the current <see cref="SignalReferenceMeasurement"/>.</param>
        /// <returns>
        /// true if the specified <see cref="ITimeSeriesValue"/> is equal to the current <see cref="SignalReferenceMeasurement"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(ITimeSeriesValue other) => m_measurement.Equals(other);

        /// <summary>
        /// Compares the <see cref="SignalReferenceMeasurement"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="SignalReferenceMeasurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(object obj) => m_measurement.CompareTo(obj);

        /// <summary>
        /// Compares the <see cref="SignalReferenceMeasurement"/> with an <see cref="ITimeSeriesValue"/>.
        /// </summary>
        /// <param name="other">The <see cref="ITimeSeriesValue"/> to compare with the current <see cref="SignalReferenceMeasurement"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(ITimeSeriesValue other) => m_measurement.CompareTo(other);

        #endregion
    }
}
