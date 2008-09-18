//*******************************************************************************************************
//  CurveFit.cs
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
using TVA.Threading;

/// <summary>Linear regression algorithm.</summary>
public static class CurveFit
{
    /// <summary>Calculates slope for a real-time continuous data stream.</summary>
    public class RealTimeSlope
    {
        #region [ Members ]

        // Delegates
        public delegate void StatusEventHandler(string message);
        public delegate void RecalculatedEventHandler();

        // Events
        public event StatusEventHandler Status;
        public event RecalculatedEventHandler Recalculated;

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
        public double RunTime
        {
            get
            {
                return (m_regressionInterval + Common.TicksToSeconds(DateTime.Now.Ticks - m_slopeRun.Ticks));
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
                thread.Name = "TVA.Math.RealTimeSlope.PerformCalculation()";
#else
                ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(PerformCalculation));
#endif
            }
        }

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
                m_slope = CurveFit.Calculate(1, m_pointCount, xValues, yValues)[1];
            }
            catch (Exception ex)
            {
                if (Status != null)
                    Status("CurveFit failed: " + ex.Message);
            }
            finally
            {
                m_calculating = false;
            }

            if (System.Math.Sign(m_slope) != System.Math.Sign(m_lastSlope))
            {
                m_slopeRun = DateTime.Now;
            }
            m_lastSlope = m_slope;

            // Notifies consumer of new calculated slope.
            if (Recalculated != null)
                Recalculated();
        }

        #endregion
    }

    /// <summary> Computes linear regression over given values.</summary>
    public static double[] Calculate(int polynomialOrder, int pointCount, IList<double> xValues, IList<double> yValues)
    {
        // Curve fit function (courtesy of Brian Fox from DatAWare client code)
        double[] coeffs = new double[8];
        double[] sum = new double[22];
        double[] v = new double[12];
        double[,] b = new double[12, 13];
        double p;
        double divB;
        double fMultB;
        double sigma;
        int ls;
        int lb;
        int lv;
        int i1;
        int i;
        int j;
        int k;
        int l;

        if (!(pointCount >= polynomialOrder + 1))
            throw new ArgumentException("Point count must be greater than requested polynomial order");

        if (!(polynomialOrder >= 1) && (polynomialOrder <= 7))
            throw new ArgumentOutOfRangeException("polynomialOrder", "Polynomial order must be between 1 and 7");

        ls = polynomialOrder * 2;
        lb = polynomialOrder + 1;
        lv = polynomialOrder;
        sum[0] = pointCount;

        for (i = 0; i <= pointCount - 1; i++)
        {
            p = 1.0;
            v[0] = v[0] + yValues[i];

            for (j = 1; j <= lv; j++)
            {
                p = xValues[i] * p;
                sum[j] = sum[j] + p;
                v[j] = v[j] + yValues[i] * p;
            }

            for (j = lb; j <= ls; j++)
            {
                p = xValues[i] * p;
                sum[j] = sum[j] + p;
            }
        }

        for (i = 0; i <= lv; i++)
        {
            for (k = 0; k <= lv; k++)
            {
                b[k, i] = sum[k + i];
            }
        }

        for (k = 0; k <= lv; k++)
        {
            b[k, lb] = v[k];
        }

        for (l = 0; l <= lv; l++)
        {
            divB = b[0, 0];
            for (j = l; j <= lb; j++)
            {
                if (divB == 0) divB = 1;
                b[l, j] = b[l, j] / divB;
            }

            i1 = l + 1;

            if (i1 - lb < 0)
            {
                for (i = i1; i <= lv; i++)
                {
                    fMultB = b[i, l];
                    for (j = l; j <= lb; j++)
                    {
                        b[i, j] = b[i, j] - b[l, j] * fMultB;
                    }
                }
            }
            else
            {
                break;
            }
        }

        coeffs[lv] = b[lv, lb];
        i = lv;

        do
        {
            sigma = 0;
            for (j = i; j <= lv; j++)
            {
                sigma = sigma + b[i - 1, j] * coeffs[j];
            }
            i--;
            coeffs[i] = b[i, lb] - sigma;
        }
        while (i - 1 > 0);

        #region [ Old Code ]

        //    For i = 1 To 7
        //        Debug.Print "Coeffs(" & i & ") = " & Coeffs(i)
        //    Next i

        //For i = 1 To 60
        //    '        CalcY(i).TTag = xValues(1) + ((i - 1) / (xValues(pointCount) - xValues(1)))

        //    CalcY(i).TTag = ((i - 1) / 59) * xValues(pointCount) - xValues(1)
        //    CalcY(i).Value = Coeffs(1)

        //    For j = 1 To polynomialOrder
        //        CalcY(i).Value = CalcY(i).Value + Coeffs(j + 1) * CalcY(i).TTag ^ j
        //    Next
        //Next

        //    SSERROR = 0
        //    For i = 1 To pointCount
        //        SSERROR = SSERROR + (yValues(i) - CalcY(i).Value) * (yValues(i) - CalcY(i).Value)
        //    Next i
        //    SSERROR = SSERROR / (pointCount - polynomialOrder)
        //    sError = SSERROR

        #endregion

        // Return slopes...
        return coeffs;
    }
}