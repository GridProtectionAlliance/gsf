//******************************************************************************************************
//  ImmediateMeasurements.cs - Gbtc
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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents the absolute latest measurement values received by a <see cref="ConcentratorBase"/> implementation.
    /// </summary>
    public class ImmediateMeasurements : IEnumerable<TemporalMeasurement>, IDisposable
    {
        #region [ Members ]

        // Fields
        private ConcentratorBase m_parent;
        private ConcurrentDictionary<MeasurementKey, TemporalMeasurement> m_measurements;
        private ConcurrentDictionary<string, List<MeasurementKey>> m_taggedMeasurements;
        private Func<Ticks> m_realTimeFunction;
        private double m_lagTime;                           // Allowed past time deviation tolerance, in seconds
        private double m_leadTime;                          // Allowed future time deviation tolerance, in seconds
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="ImmediateMeasurements"/> class.
        /// </summary>
        public ImmediateMeasurements()
        {
            m_measurements = new ConcurrentDictionary<MeasurementKey, TemporalMeasurement>();
            m_taggedMeasurements = new ConcurrentDictionary<string, List<MeasurementKey>>();
            m_realTimeFunction = () => DateTime.UtcNow.Ticks;
        }

        internal ImmediateMeasurements(ConcentratorBase parent)
            : this()
        {
            m_parent = parent;

            if (m_parent is not null)
            {
                m_parent.LagTimeUpdated += OnLagTimeUpdated;
                m_parent.LeadTimeUpdated += OnLeadTimeUpdated;
                m_realTimeFunction = () => m_parent.RealTime;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="ImmediateMeasurements"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~ImmediateMeasurements()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>We retrieve adjusted measurement values within time tolerance of concentrator real-time.</summary>
        /// <param name="id">A <see cref="Guid"/> representing the measurement ID.</param>
        /// <returns>A <see cref="Double"/> representing the adjusted measurement value.</returns>
        public double this[MeasurementKey id] => Measurement(id).GetAdjustedValue(m_realTimeFunction());

        /// <summary>Returns collection of measurement ID's.</summary>
        public ICollection<MeasurementKey> MeasurementIDs => m_measurements.Keys;

        /// <summary>Returns ID collection for measurement tags.</summary>
        public ICollection<string> Tags => m_taggedMeasurements.Keys;

        /// <summary>Returns the minimum value of all measurements.</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement.</remarks>
        public double Minimum
        {
            get
            {
                double minValue = double.MaxValue;
                double measurement;

                foreach (MeasurementKey id in m_measurements.Keys)
                {
                    measurement = this[id];
                    if (!double.IsNaN(measurement))
                    {
                        if (measurement < minValue)
                        {
                            minValue = measurement;
                        }
                    }
                }

                return minValue;
            }
        }

        /// <summary>Returns the maximum value of all measurements.</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement.</remarks>
        public double Maximum
        {
            get
            {
                double maxValue = double.MinValue;
                double measurement;

                foreach (MeasurementKey id in m_measurements.Keys)
                {
                    measurement = this[id];
                    if (!double.IsNaN(measurement))
                    {
                        if (measurement > maxValue)
                        {
                            maxValue = measurement;
                        }
                    }
                }

                return maxValue;
            }
        }

        /// <summary>
        /// Gets or sets function to return real-time.
        /// </summary>
        public Func<Ticks> RealTimeFunction
        {
            get => m_realTimeFunction;
            set
            {
                m_realTimeFunction = value ?? (() => DateTime.UtcNow.Ticks);
            }
        }

        /// <summary>
        /// Gets or sets the allowed past time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to past measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        public double LagTime
        {
            get => m_lagTime;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "LagTime must be greater than zero, but it can be less than one");

                m_lagTime = value;

                foreach (MeasurementKey id in m_measurements.Keys)
                {
                    Measurement(id).LagTime = m_lagTime;
                }
            }
        }

        /// <summary>
        /// Gets or sets the allowed future time deviation tolerance, in seconds (can be sub-second).
        /// </summary>
        /// <remarks>
        /// <para>Defines the time sensitivity to future measurement timestamps.</para>
        /// <para>The number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        public double LeadTime
        {
            get => m_leadTime;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "LeadTime must be greater than zero, but it can be less than one");

                m_leadTime = value;

                foreach (MeasurementKey id in m_measurements.Keys)
                {
                    Measurement(id).LeadTime = m_leadTime;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ImmediateMeasurements"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ImmediateMeasurements"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (m_parent is not null)
                        {
                            m_parent.LagTimeUpdated -= OnLagTimeUpdated;
                            m_parent.LeadTimeUpdated -= OnLagTimeUpdated;
                        }
                        m_parent = null;

                        if (m_measurements is not null)
                            m_measurements.Clear();

                        m_measurements = null;

                        if (m_taggedMeasurements is not null)
                            m_taggedMeasurements.Clear();

                        m_taggedMeasurements = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>Returns measurement list of specified tag, if it exists.</summary>
        /// <param name="tag">A <see cref="String"/> that indicates the tag to use.</param>
        /// <returns>A collection of measurement keys.</returns>
        public ReadOnlyCollection<MeasurementKey> TaggedMeasurementKeys(string tag)
        {
            return new ReadOnlyCollection<MeasurementKey>(m_taggedMeasurements[tag]);
        }

        /// <summary>Store new measurement.</summary>
        /// <param name="newMeasurement">New measurement value to update.</param>
        /// <remarks>Value is only stored if it is newer than the cached value.</remarks>
        public void UpdateMeasurementValue(IMeasurement newMeasurement)
        {
            TemporalMeasurement measurement = Measurement(newMeasurement);

            // Set new value updating state flags if value was updated...
            if (measurement.SetValue(newMeasurement.Timestamp, newMeasurement.Value))
            {
                measurement.Metadata = newMeasurement.Metadata;
                measurement.StateFlags = newMeasurement.StateFlags;
            }
        }

        /// <summary>Retrieves the specified immediate temporal measurement, creating it if needed.</summary>
        /// <param name="id"><see cref="Guid"/> based signal ID of measurement.</param>
        /// <returns>A <see cref="TemporalMeasurement"/> object.</returns>
        public TemporalMeasurement Measurement(MeasurementKey id)
        {
            return m_measurements.GetOrAdd(id, key => new TemporalMeasurement(m_lagTime, m_leadTime)
            {
                Metadata = key.Metadata,
            });
        }

        /// <summary>Retrieves the specified immediate temporal measurement, creating it if needed.</summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <returns>A <see cref="TemporalMeasurement"/> object.</returns>
        public TemporalMeasurement Measurement(IMeasurement measurement)
        {
            TemporalMeasurement temporalMeasurement = m_measurements.GetOrAdd(measurement.Key, key => new TemporalMeasurement(m_lagTime, m_leadTime)
            {
                Metadata = key.Metadata.ChangeAdderMultiplier(measurement.Adder, measurement.Multiplier)
            });

            return temporalMeasurement;
        }

        /// <summary>
        /// Clears the existing measurement cache.
        /// </summary>
        public void ClearMeasurementCache()
        {
            m_measurements.Clear();
        }

        /// <summary>Defines tagged measurements from a data table.</summary>
        /// <remarks>Expects <see cref="String"/> based tag field to be aliased as "Tag" and <see cref="Guid"/> based measurement ID field to be aliased as "ID".</remarks>
        /// <param name="taggedMeasurements">A <see cref="DataTable"/> to use for defining the tagged measurements.</param>
        public void DefineTaggedMeasurements(DataTable taggedMeasurements)
        {
            foreach (DataRow row in taggedMeasurements.Rows)
            {
                Guid id = row["ID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                MeasurementKey key = MeasurementKey.LookUpBySignalID(id);
                if (key != MeasurementKey.Undefined)
                {
                    AddTaggedMeasurement(row["Tag"].ToNonNullString("_tag_"), key);
                }
            }
        }

        /// <summary>Associates a new measurement ID with a tag, creating the new tag if needed.</summary>
        /// <remarks>Allows you to define "grouped" points so you can aggregate certain measurements.</remarks>
        /// <param name="tag">A <see cref="String"/> to represent the key.</param>
        /// <param name="id">A <see cref="Guid"/> ID to associate with the tag.</param>
        public void AddTaggedMeasurement(string tag, MeasurementKey id)
        {
            // Get tag's measurement list
            List<MeasurementKey> measurements = m_taggedMeasurements.GetOrAdd(tag, new List<MeasurementKey>());

            if (measurements.BinarySearch(id) < 0)
            {
                measurements.Add(id);
                measurements.Sort();
            }
        }

        /// <summary>Calculates an average of all measurements.</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement.</remarks>
        /// <param name="count">An <see cref="Int32"/> value to get the count of values averaged.</param>
        /// <returns>A <see cref="Double"/> value representing the average of the measurements.</returns>
        public double CalculateAverage(ref int count)
        {
            double measurement;
            double total = 0.0D;

            foreach (MeasurementKey id in m_measurements.Keys)
            {
                measurement = this[id];
                if (!double.IsNaN(measurement))
                {
                    total += measurement;
                    count++;
                }
            }

            return total / count;
        }

        /// <summary>Calculates an average of all measurements associated with the specified tag.</summary>
        /// <param name="count">An <see cref="Int32"/> value to get the count of values averaged.</param>
        /// <param name="tag">The type of measurements to average.</param>
        /// <returns>A <see cref="Double"/> value representing the average of the tags.</returns>
        public double CalculateTagAverage(string tag, ref int count)
        {
            double measurement;
            double total = 0.0D;

            foreach (MeasurementKey id in m_taggedMeasurements[tag])
            {
                measurement = this[id];
                if (!double.IsNaN(measurement))
                {
                    total += measurement;
                    count++;
                }
            }

            return total / count;
        }

        /// <summary>Returns the minimum value of all measurements associated with the specified tag.</summary>
        /// <returns>A <see cref="Double"/> value representing the tag minimum.</returns>
        /// <param name="tag">The tag group to evaluate.</param>
        public double TagMinimum(string tag)
        {
            double minValue = double.MaxValue;
            double measurement;

            foreach (MeasurementKey id in m_taggedMeasurements[tag])
            {
                measurement = this[id];
                if (!double.IsNaN(measurement))
                {
                    if (measurement < minValue)
                    {
                        minValue = measurement;
                    }
                }
            }

            return minValue;
        }

        /// <summary>Returns the maximum value of all measurements associated with the specified tag.</summary>
        /// <returns>A <see cref="Double"/> value representing the tag maximum.</returns>
        /// <param name="tag">The tag group to evaluate.</param>
        public double TagMaximum(string tag)
        {
            double maxValue = double.MinValue;
            double measurement;

            foreach (MeasurementKey id in m_taggedMeasurements[tag])
            {
                measurement = this[id];
                if (!double.IsNaN(measurement))
                {
                    if (measurement > maxValue)
                    {
                        maxValue = measurement;
                    }
                }
            }

            return maxValue;
        }

        /// <summary>
        /// Updates the tracked temporal measurements lag time.
        /// </summary>
        /// <param name="lagTime">New lag time.</param>
        protected void OnLagTimeUpdated(double lagTime)
        {
            LagTime = lagTime;
        }

        /// <summary>
        /// Updates the tracked temporal measurements lead time.
        /// </summary>
        /// <param name="leadTime">New lead time.</param>
        protected void OnLeadTimeUpdated(double leadTime)
        {
            LeadTime = leadTime;
        }

        IEnumerator<TemporalMeasurement> IEnumerable<TemporalMeasurement>.GetEnumerator()
        {
            return m_measurements.Values.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_measurements.Values.ToList().GetEnumerator();
        }

        #endregion
    }
}