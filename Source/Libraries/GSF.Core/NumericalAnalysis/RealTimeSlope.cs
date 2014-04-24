//******************************************************************************************************
//  RealTimeSlope.cs - Gbtc
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
//  01/24/2006 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/17/2008 - J. Ritchie Carroll
//       Converted to C#.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using GSF.Units;

namespace GSF.NumericalAnalysis
{
    /// <summary>Calculates slope for a real-time continuous data stream.</summary>
    public class RealTimeSlope
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// Raised when new status messages come from the <see cref="RealTimeSlope"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is status message from the <see cref="RealTimeSlope"/>.
        /// </remarks>
        public event EventHandler<EventArgs<string>> Status;

        /// <summary>
        /// Raised when new real-time <see cref="Slope"/> has been calculated and is available.
        /// </summary>
        public event EventHandler Recalculated;

        // Fields
        private int m_regressionInterval;
        private int m_pointCount;
        private List<double> m_xValues;
        private List<double> m_yValues;
        private double m_slope;
        private double m_lastSlope;
        private DateTime m_slopeRun;
        private bool m_calculating;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a default instance of the real-time slope calculation class. Must call Initialize before using.
        /// </summary>
        public RealTimeSlope()
        {
        }

        /// <summary>Creates a new instance of the real-time slope calculation class.</summary>
        /// <param name="regressionInterval">Time span over which to calculate slope.</param>
        /// <param name="estimatedRefreshInterval">Estimated data points per second.</param>
        public RealTimeSlope(int regressionInterval, double estimatedRefreshInterval)
            : this()
        {
            Initialize(regressionInterval, estimatedRefreshInterval);
        }

        #endregion

        #region [ Properties ]

        /// <summary>Gets current calculated slope for data set.</summary>
        public double Slope
        {
            get
            {
                return m_slope;
            }
        }

        /// <summary>Gets run-time, in seconds, for which slope has maintained a continuous positive or negative
        /// trend.</summary>
        public Time RunTime
        {
            get
            {
                return (m_regressionInterval + Ticks.ToSeconds(DateTime.UtcNow.Ticks - m_slopeRun.Ticks));
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>Adds a new x, y data pair to continuous data set.</summary>
        /// <param name="x">New x-axis value.</param>
        /// <param name="y">New y-axis value.</param>
        public void Calculate(double x, double y)
        {
            lock (m_xValues)
            {
                // Adds latest values to regression data set.
                m_xValues.Add(x);
                m_yValues.Add(y);

                // Keeps a constant number of points by removing them from the left.
                while (m_xValues.Count > m_pointCount)
                {
                    m_xValues.RemoveAt(0);
                }

                while (m_yValues.Count > m_pointCount)
                {
                    m_yValues.RemoveAt(0);
                }
            }

            if (m_xValues.Count >= m_pointCount && !m_calculating)
            {
                // Performs curve fit calculation on separate thread, since it could be time consuming.
#if ThreadTracking
                ManagedThread thread = ManagedThreadPool.QueueUserWorkItem(PerformCalculation);
                thread.Name = "GSF.Math.RealTimeSlope.PerformCalculation()";
#else
                ThreadPool.QueueUserWorkItem(PerformCalculation);
#endif
            }
        }

        /// <summary>
        /// Initializes real-time slope calculation.
        /// </summary>
        /// <param name="regressionInterval">Time span over which to calculate slope.</param>
        /// <param name="estimatedRefreshInterval">Estimated data points per second.</param>
        public void Initialize(int regressionInterval, double estimatedRefreshInterval)
        {
            m_slopeRun = DateTime.UtcNow;
            m_regressionInterval = regressionInterval;
            m_pointCount = m_regressionInterval * (int)(1 / estimatedRefreshInterval);

            if ((object)m_xValues == null)
            {
                m_xValues = new List<double>();
            }
            else
            {
                lock (m_xValues)
                {
                    m_xValues.Clear();
                }
            }
            if ((object)m_yValues == null)
            {
                m_yValues = new List<double>();
            }
            else
            {
                lock (m_yValues)
                {
                    m_yValues.Clear();
                }
            }
        }

        private void PerformCalculation(object state)
        {
            try
            {
                m_calculating = true;

                double[] xValues;
                double[] yValues;

                // Calculations are made against a copy of the current data set to keep lock time on
                // data values down to a minimum. This allows data to be added with minimal delay.
                lock (m_xValues)
                {
                    xValues = m_xValues.ToArray();
                    yValues = m_yValues.ToArray();
                }

                // Takes new values and calculates slope (curve fit for 1st order polynomial).
                m_slope = CurveFit.Compute(1, xValues, yValues)[1];
            }
            catch (Exception ex)
            {
                if ((object)Status != null)
                    Status(this, new EventArgs<string>("CurveFit failed: " + ex.Message));
            }
            finally
            {
                m_calculating = false;
            }

            if (Math.Sign(m_slope) != Math.Sign(m_lastSlope))
                m_slopeRun = DateTime.UtcNow;

            m_lastSlope = m_slope;

            // Notifies consumer of new calculated slope.
            if ((object)Recalculated != null)
                Recalculated(this, EventArgs.Empty);
        }

        #endregion
    }
}
