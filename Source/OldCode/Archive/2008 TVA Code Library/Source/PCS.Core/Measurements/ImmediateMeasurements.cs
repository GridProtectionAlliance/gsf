//*******************************************************************************************************
//  ImmediateMeasurements.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated for Super Phasor Data Concentrator.
//  02/23/2006 - J. Ritchie Carroll
//       Classes abstracted for general use and added to PCS code library.
//  09/17/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;

namespace PCS.Measurements
{
    /// <summary>Represents the absolute latest measurement values received by a <see cref="ConcentratorBase"/> implementation.</summary>
    public class ImmediateMeasurements : IDisposable
    {
        #region [ Members ]

        // Fields
        private ConcentratorBase m_parent;
        private Dictionary<MeasurementKey, TemporalMeasurement> m_measurements;
        private Dictionary<string, List<MeasurementKey>> m_taggedMeasurements;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        internal ImmediateMeasurements(ConcentratorBase parent)
        {
            m_parent = parent;
            m_parent.LagTimeUpdated += m_parent_LagTimeUpdated;
            m_parent.LeadTimeUpdated += m_parent_LeadTimeUpdated;
            m_measurements = new Dictionary<MeasurementKey, TemporalMeasurement>();
            m_taggedMeasurements = new Dictionary<string, List<MeasurementKey>>();
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
        public double this[int measurementID, string source]
        {
            get
            {
                return this[new MeasurementKey(measurementID, source)];
            }
        }

        /// <summary>We retrieve adjusted measurement values within time tolerance of concentrator real-time.</summary>
        public double this[MeasurementKey key]
        {
            get
            {
                return Measurement(key).GetAdjustedValue(m_parent.RealTimeTicks);
            }
        }

        /// <summary>Returns key collection of measurement keys.</summary>
        public Dictionary<MeasurementKey, TemporalMeasurement>.KeyCollection MeasurementKeys
        {
            get
            {
                return m_measurements.Keys;
            }
        }

        /// <summary>Returns key collection for measurement tags.</summary>
        public Dictionary<string, List<MeasurementKey>>.KeyCollection Tags
        {
            get
            {
                return m_taggedMeasurements.Keys;
            }
        }

        /// <summary>Returns the minimum value of all measurements.</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement.</remarks>
        public double Minimum
        {
            get
            {
                double minValue = double.MaxValue;
                double measurement;

                lock (m_measurements)
                {
                    foreach (MeasurementKey key in m_measurements.Keys)
                    {
                        measurement = this[key];
                        if (!double.IsNaN(measurement))
                        {
                            if (measurement < minValue)
                            {
                                minValue = measurement;
                            }
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

                lock (m_measurements)
                {
                    foreach (MeasurementKey key in m_measurements.Keys)
                    {
                        measurement = this[key];
                        if (!double.IsNaN(measurement))
                        {
                            if (measurement > maxValue)
                            {
                                maxValue = measurement;
                            }
                        }
                    }
                }

                return maxValue;
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
                        if (m_parent != null)
                        {
                            m_parent.LagTimeUpdated -= m_parent_LagTimeUpdated;
                            m_parent.LeadTimeUpdated -= m_parent_LeadTimeUpdated;
                        }
                        m_parent = null;

                        if (m_measurements != null)
                        {
                            m_measurements.Clear();
                        }
                        m_measurements = null;

                        if (m_taggedMeasurements != null)
                        {
                            m_taggedMeasurements.Clear();
                        }
                        m_taggedMeasurements = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>Returns measurement key list of specified tag, if it exists.</summary>
        public List<MeasurementKey> TagMeasurementKeys(string tag)
        {
            return m_taggedMeasurements[tag];
        }

        /// <summary>We only store a new measurement value that is newer than the cached value.</summary>
        internal void UpdateMeasurementValue(IMeasurement newMeasurement)
        {
            Measurement(newMeasurement.Key).SetValue(newMeasurement.Ticks, newMeasurement.Value);
        }

        /// <summary>Retrieves the specified immediate temporal measurement, creating it if needed.</summary>
        public TemporalMeasurement Measurement(int measurementID, string source)
        {
            return Measurement(new MeasurementKey(measurementID, source));
        }

        /// <summary>Retrieves the specified immediate temporal measurement, creating it if needed.</summary>
        public TemporalMeasurement Measurement(MeasurementKey key)
        {
            lock (m_measurements)
            {
                TemporalMeasurement value;

                if (!m_measurements.TryGetValue(key, out value))
                {
                    // Create new temporal measurement if it doesn't exist
                    value = new TemporalMeasurement(key.ID, key.Source, double.NaN, m_parent.RealTimeTicks, m_parent.LagTime, m_parent.LeadTime);
                    m_measurements.Add(key, value);
                }

                return value;
            }
        }

        /// <summary>Defines tagged measurements from a data table.</summary>
        /// <remarks>Expects tag field to be aliased as "Tag", measurement ID field to be aliased as "ID" and source field to be aliased as "Source".</remarks>
        public void DefineTaggedMeasurements(DataTable taggedMeasurements)
        {
            foreach (DataRow row in taggedMeasurements.Rows)
            {
                AddTaggedMeasurement(row["Tag"].ToString(), new MeasurementKey(System.Convert.ToInt32(row["ID"]), row["Source"].ToString()));
            }
        }

        /// <summary>Associates a new measurement ID with a tag, creating the new tag if needed.</summary>
        /// <remarks>Allows you to define "grouped" points so you can aggregate certain measurements.</remarks>
        public void AddTaggedMeasurement(string tag, MeasurementKey key)
        {
            // Check for new tag
            if (!m_taggedMeasurements.ContainsKey(tag))
                m_taggedMeasurements.Add(tag, new List<MeasurementKey>());

            // Add measurement to tag's measurement list
            List<MeasurementKey> measurements = m_taggedMeasurements[tag];

            if (measurements.BinarySearch(key) < 0)
            {
                measurements.Add(key);
                measurements.Sort();
            }
        }

        /// <summary>Calculates an average of all measurements.</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement.</remarks>
        public double CalculateAverage(ref int count)
        {
            double measurement;
            double total = 0.0D;

            lock (m_measurements)
            {
                foreach (MeasurementKey key in m_measurements.Keys)
                {
                    measurement = this[key];
                    if (!double.IsNaN(measurement))
                    {
                        total += measurement;
                        count++;
                    }
                }
            }

            return total / count;
        }

        /// <summary>Calculates an average of all measurements associated with the specified tag.</summary>
        public double CalculateTagAverage(string tag, ref int count)
        {
            double measurement;
            double total = 0.0D;

            foreach (MeasurementKey key in m_taggedMeasurements[tag])
            {
                measurement = this[key];
                if (!double.IsNaN(measurement))
                {
                    total += measurement;
                    count++;
                }
            }

            return total / count;
        }

        /// <summary>Returns the minimum value of all measurements associated with the specified tag.</summary>
        public double TagMinimum(string tag)
        {
            double minValue = double.MaxValue;
            double measurement;

            foreach (MeasurementKey key in m_taggedMeasurements[tag])
            {
                measurement = this[key];
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
        public double TagMaximum(string tag)
        {
            double maxValue = double.MinValue;
            double measurement;

            foreach (MeasurementKey key in m_taggedMeasurements[tag])
            {
                measurement = this[key];
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

        // We dyanmically respond to real-time changes in lead or lag time...
        private void m_parent_LagTimeUpdated(double lagTime)
        {
            lock (m_measurements)
            {
                foreach (MeasurementKey key in m_measurements.Keys)
                {
                    Measurement(key).LagTime = lagTime;
                }
            }
        }

        private void m_parent_LeadTimeUpdated(double leadTime)
        {
            lock (m_measurements)
            {
                foreach (MeasurementKey key in m_measurements.Keys)
                {
                    Measurement(key).LeadTime = leadTime;
                }
            }
        }

        #endregion
    }
}