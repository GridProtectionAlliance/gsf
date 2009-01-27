//*******************************************************************************************************
//  RealTimeSlope.cs
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
//  01/24/2006 - James R Carroll
//       Generated original version of source code.
//  09/17/2008 - James R Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Units;
using PCS.Threading;

namespace PCS.NumericalAnalysis
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
        /// <see cref="System.EventArgs{T}.Argument"/> is status message from the <see cref="RealTimeSlope"/>.
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
                return (m_regressionInterval + Ticks.ToSeconds(DateTime.Now.Ticks - m_slopeRun.Ticks));
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
                // Performs curve fit calculation on seperate thread, since it could be time consuming.
#if ThreadTracking
                ManagedThread thread = ManagedThreadPool.QueueUserWorkItem(PerformCalculation);
                thread.Name = "PCS.Math.RealTimeSlope.PerformCalculation()";
#else
                ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(PerformCalculation));
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
            m_slopeRun = DateTime.Now;
            m_regressionInterval = regressionInterval;
            m_pointCount = m_regressionInterval * (int)(1 / estimatedRefreshInterval);

            if (m_xValues == null)
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
            if (m_yValues == null)
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
                m_slope = CurveFit.Compute(1, m_pointCount, xValues, yValues)[1];
            }
            catch (Exception ex)
            {
                if (Status != null)
                    Status(this, new EventArgs<string>("CurveFit failed: " + ex.Message));
            }
            finally
            {
                m_calculating = false;
            }

            if (Math.Sign(m_slope) != Math.Sign(m_lastSlope))
            {
                m_slopeRun = DateTime.Now;
            }
            m_lastSlope = m_slope;

            // Notifies consumer of new calculated slope.
            if (Recalculated != null)
                Recalculated(this, EventArgs.Empty);
        }

        #endregion
    }
}
