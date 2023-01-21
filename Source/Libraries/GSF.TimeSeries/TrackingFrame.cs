//******************************************************************************************************
//  TrackingFrame.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System.Collections.Concurrent;
using System.Collections.Generic;
using GSF.Threading;

namespace GSF.TimeSeries
{
    /// <summary>
    /// <see cref="IFrame"/> container used to track <see cref="IMeasurement"/> values for down-sampling.
    /// </summary>
    internal class TrackingFrame
    {
        #region [ Members ]

        // Fields
        private readonly DownsamplingMethod m_downsamplingMethod;
        private readonly ConcurrentDictionary<MeasurementKey, List<IMeasurement>> m_measurements;
        private long m_derivedMeasurements;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="TrackingFrame"/> given the specified parameters.
        /// </summary>
        /// <param name="sourceFrame">Source <see cref="IFrame"/> to track.</param>
        /// <param name="downsamplingMethod"><see cref="DownsamplingMethod"/> to apply.</param>
        public TrackingFrame(IFrame sourceFrame, DownsamplingMethod downsamplingMethod)
        {
            SourceFrame = sourceFrame;
            Timestamp = sourceFrame.Timestamp;
            m_downsamplingMethod = downsamplingMethod;
            Lock = new ReaderWriterSpinLock();

            if (downsamplingMethod != DownsamplingMethod.LastReceived)
                m_measurements = new ConcurrentDictionary<MeasurementKey, List<IMeasurement>>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets instance of <see cref="IFrame"/> being tracked.
        /// </summary>
        public IFrame SourceFrame { get; }

        /// <summary>
        /// Gets timestamp of <see cref="IFrame"/> being tracked.
        /// </summary>
        public long Timestamp { get; }

        /// <summary>
        /// Total number of measurements down-sampled by <see cref="TrackingFrame"/>.
        /// </summary>
        public long DownsampledMeasurements =>
            // Only measurements that came in after initial sorts were downsampled...
            m_derivedMeasurements - SourceFrame.SortedMeasurements;

        /// <summary>
        /// Gets the <see cref="TrackingFrame"/> locking primitive.
        /// </summary>
        public ReaderWriterSpinLock Lock { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Derives measurement value, down-sampling if needed.
        /// </summary>
        /// <param name="measurement">New <see cref="IMeasurement"/> value.</param>
        /// <returns>New derived <see cref="IMeasurement"/> value, or null if value should not be assigned to <see cref="IFrame"/>.</returns>
        public IMeasurement DeriveMeasurementValue(IMeasurement measurement)
        {
            IMeasurement derivedMeasurement;
            List<IMeasurement> values;

            switch (m_downsamplingMethod)
            {
                case DownsamplingMethod.LastReceived:
                    // Keep track of total number of derived measurements
                    m_derivedMeasurements++;

                    // This is the simplest case, just apply latest value
                    return measurement;
                case DownsamplingMethod.Closest:
                    // Get tracked measurement values
                    if (m_measurements.TryGetValue(measurement.Key, out values))
                    {
                        if (values is not null && values.Count > 0)
                        {
                            // Get first tracked value (should only be one for "Closest")
                            derivedMeasurement = values[0];

                            if (derivedMeasurement is not null)
                            {
                                // Determine if new measurement's timestamp is closer to frame
                                if (measurement.Timestamp < derivedMeasurement.Timestamp && measurement.Timestamp >= Timestamp)
                                {
                                    // This measurement came in out-of-order and is closer to frame timestamp, so 
                                    // we sort this measurement instead of the original
                                    values[0] = measurement;

                                    // Keep track of total number of derived measurements
                                    m_derivedMeasurements++;

                                    return measurement;
                                }

                                // Prior measurement is closer to frame than new one
                                return null;
                            }
                        }
                    }

                    // No prior measurement exists, track this initial one
                    values = new List<IMeasurement> { measurement };
                    m_measurements[measurement.Key] = values;

                    // Keep track of total number of derived measurements
                    m_derivedMeasurements++;

                    return measurement;
                case DownsamplingMethod.Filtered:
                    // Get tracked measurement values
                    if (m_measurements.TryGetValue(measurement.Key, out values))
                    {
                        if (values is not null && values.Count > 0)
                        {
                            // Get first tracked value - we clone the measurement since we are updating its value
                            derivedMeasurement = Measurement.Clone(values[0]);

                            if (derivedMeasurement is not null)
                            {
                                // Get function defined for measurement value filtering
                                MeasurementValueFilterFunction measurementValueFilter = derivedMeasurement.MeasurementValueFilter;

                                // Default to average value filter if none is specified
                                if ((object)measurementValueFilter is null)
                                    measurementValueFilter = Measurement.AverageValueFilter;

                                // Add new measurement to tracking collection
                                values.Add(measurement);

                                // Perform filter calculation as specified by device measurement
                                if (values.Count > 1)
                                {
                                    derivedMeasurement.Value = measurementValueFilter(values);

                                    // Keep track of total number of derived measurements
                                    m_derivedMeasurements++;

                                    return derivedMeasurement;
                                }

                                // No change from existing measurement
                                return null;
                            }
                        }
                    }

                    // No prior measurement exists, track this initial one
                    values = new List<IMeasurement> { measurement };
                    m_measurements[measurement.Key] = values;

                    // Keep track of total number of derived measurements
                    m_derivedMeasurements++;

                    return measurement;
                case DownsamplingMethod.BestQuality:
                    // Get tracked measurement values
                    if (m_measurements.TryGetValue(measurement.Key, out values))
                    {
                        if (values is not null && values.Count > 0)
                        {
                            // Get first tracked value (should only be one for "BestQuality")
                            derivedMeasurement = values[0];

                            if (derivedMeasurement is not null)
                            {
                                // Determine if new measurement's quality is better than existing one or if new measurement's timestamp is closer to frame
                                if
                                (
                                    (
                                        (!derivedMeasurement.ValueQualityIsGood() || !derivedMeasurement.TimestampQualityIsGood())
                                            &&
                                        (measurement.ValueQualityIsGood() || measurement.TimestampQualityIsGood())
                                    )
                                        ||
                                    (
                                        measurement.Timestamp < derivedMeasurement.Timestamp && measurement.Timestamp >= Timestamp
                                    )
                                )
                                {
                                    // This measurement has a better quality or came in out-of-order and is closer to frame timestamp, so 
                                    // we sort this measurement instead of the original
                                    values[0] = measurement;

                                    // Keep track of total number of derived measurements
                                    m_derivedMeasurements++;

                                    return measurement;
                                }

                                // Prior measurement is closer to frame than new one
                                return null;
                            }
                        }
                    }

                    // No prior measurement exists, track this initial one
                    values = new List<IMeasurement> { measurement };
                    m_measurements[measurement.Key] = values;

                    // Keep track of total number of derived measurements
                    m_derivedMeasurements++;

                    return measurement;
            }

            return null;
        }

        #endregion
    }
}