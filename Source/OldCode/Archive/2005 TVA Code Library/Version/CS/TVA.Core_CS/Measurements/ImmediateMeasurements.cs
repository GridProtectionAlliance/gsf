using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;

//*******************************************************************************************************
//  TVA.Measurements.ImmediateMeasurements.vb - Lastest received measurements collection
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - J. Ritchie Carroll
//       Initial version of source generated for Super Phasor Data Concentrator
//  02/23/2006 - J. Ritchie Carroll
//       Classes abstracted for general use and added to TVA code library
//
//*******************************************************************************************************

namespace TVA.Measurements
{
    /// <summary>This class represents the absolute latest received measurement values</summary>
    public class ImmediateMeasurements
    {


        private ConcentratorBase m_parent;
        private Dictionary<MeasurementKey, TemporalMeasurement> m_measurements;
        private Dictionary<string, List<MeasurementKey>> m_taggedMeasurements;

        internal ImmediateMeasurements(ConcentratorBase parent)
			{
				
				m_parent = parent;
				m_parent.LagTimeUpdated += new TVA.Measurements.ConcentratorBase.LagTimeUpdatedEventHandler(m_parent_LagTimeUpdated);
				m_parent.LeadTimeUpdated += new TVA.Measurements.ConcentratorBase.LeadTimeUpdatedEventHandler(m_parent_LeadTimeUpdated);
				m_measurements = new Dictionary<MeasurementKey, TemporalMeasurement>();
				m_taggedMeasurements = new Dictionary<string, List<MeasurementKey>>();
				
			}

        /// <summary>Handy instance reference to self</summary>
        public ImmediateMeasurements This
        {
            get
            {
                return this;
            }
        }

        /// <summary>Returns key collection of measurement keys</summary>
        public Dictionary<MeasurementKey, TemporalMeasurement> MeasurementKeys
        {
            get
            {
                return m_measurements.Keys;
            }
        }

        /// <summary>Returns key collection for measurement tags</summary>
        public Dictionary<string, List<MeasurementKey>> Tags
        {
            get
            {
                return m_taggedMeasurements.Keys;
            }
        }

        /// <summary>Returns measurement key list of specified tag, if it exists</summary>
        public List TagMeasurementKeys(string tag)
        {
            return m_taggedMeasurements(tag);
        }

        /// <summary>We retrieve adjusted measurement values within time tolerance of concentrator real-time</summary>
        public double this[int measurementID, string source]
        {
            get
            {
                return value(new MeasurementKey(measurementID, source));
            }
        }

        /// <summary>We retrieve adjusted measurement values within time tolerance of concentrator real-time</summary>
        public double this[MeasurementKey key]
        {
            get
            {
                return Measurement(key)[m_parent.RealTimeTicks];
            }
        }

        /// <summary>We retrieve measurement values within time tolerance of concentrator real-time</summary>
        public double Value(int measurementID, string source)
        {
            return Value(new MeasurementKey(measurementID, source));
        }

        /// <summary>We retrieve measurement values within time tolerance of concentrator real-time</summary>
        public double Value(MeasurementKey key)
        {
            return Measurement(key).Value(m_parent.RealTimeTicks);
        }

        /// <summary>We only store a new measurement value that is newer than the cached value</summary>
        internal void UpdateMeasurementValue(IMeasurement newMeasurement)
        {

            IMeasurement with_1 = newMeasurement;
            Measurement(with_1.Key).Value(with_1.Ticks) = with_1.Value;

        }

        /// <summary>Retrieves the specified immediate temporal measurement, creating it if needed</summary>
        public TemporalMeasurement Measurement(int measurementID, string source)
        {
            return Measurement(new MeasurementKey(measurementID, source));
        }

        /// <summary>Retrieves the specified immediate temporal measurement, creating it if needed</summary>
        public TemporalMeasurement Measurement(MeasurementKey key)
        {
            lock (m_measurements)
            {
                TemporalMeasurement value;

                if (!m_measurements.TryGetValue(key, value))
                {
                    // Create new temporal measurement if it doesn't exist
                    value = new TemporalMeasurement(key.ID, key.Source, double.NaN, m_parent.RealTimeTicks, m_parent.lagTime, m_parent.leadTime);
                    m_measurements.Add(key, value);
                }

                return value;
            }
        }

        /// <summary>Defines tagged measurements from a data table</summary>
        /// <remarks>Expects tag field to be aliased as "Tag", measurement ID field to be aliased as "ID" and source field to be aliased as "Source"</remarks>
        public void DefineTaggedMeasurements(DataTable taggedMeasurements)
        {

            foreach (DataRow row in taggedMeasurements.Rows)
            {
                AddTaggedMeasurement(row["Tag"].ToString(), new MeasurementKey(System.Convert.ToInt32(row["ID"]), row["Source"].ToString()));
            }

        }

        /// <summary>Associates a new measurement ID with a tag, creating the new tag if needed</summary>
        /// <remarks>Allows you to define "grouped" points so you can aggregate certain measurements</remarks>
        public void AddTaggedMeasurement(string tag, MeasurementKey key)
			{
				
				// Check for new tag
				if (! m_taggedMeasurements.ContainsKey(tag))
				{
					m_taggedMeasurements.Add(tag, new List<MeasurementKey>());
				}
				
				// Add measurement to tag's measurement list
				object with_2 = m_taggedMeasurements.Item(tag);
				if (with_2.BinarySearch(key) < 0)
				{
					with_2.Add(key);
					with_2.Sort();
				}
				
			}

        /// <summary>Calculates an average of all measurements</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
        public double CalculateAverage(ref int count)
        {

            double measurement;
            double total;

            lock (m_measurements)
            {
                foreach (MeasurementKey key in m_measurements.Keys)
                {
                    measurement = Value(key);
                    if (!double.IsNaN(measurement))
                    {
                        total += measurement;
                        count++;
                    }
                }
            }

            return total / count;

        }

        /// <summary>Calculates an average of all measurements associated with the specified tag</summary>
        public double CalculateTagAverage(string tag, ref int count)
        {

            double measurement;
            double total;

            foreach (MeasurementKey key in m_taggedMeasurements(tag))
            {
                measurement = Value(key);
                if (!double.IsNaN(measurement))
                {
                    total += measurement;
                    count++;
                }
            }

            return total / count;

        }

        /// <summary>Returns the minimum value of all measurements</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
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
                        measurement = value(key);
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

        /// <summary>Returns the maximum value of all measurements</summary>
        /// <remarks>This is only useful if all measurements represent the same type of measurement</remarks>
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
                        measurement = value(key);
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

        /// <summary>Returns the minimum value of all measurements associated with the specified tag</summary>
        public double TagMinimum(string tag)
        {
            double minValue = double.MaxValue;
            double measurement;

            foreach (MeasurementKey key in m_taggedMeasurements(tag))
            {
                measurement = Value(key);
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

        /// <summary>Returns the maximum value of all measurements associated with the specified tag</summary>
        public double TagMaximum(string tag)
        {
            double maxValue = double.MinValue;
            double measurement;

            foreach (MeasurementKey key in m_taggedMeasurements(tag))
            {
                measurement = Value(key);
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
    }
}