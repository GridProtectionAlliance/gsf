//*******************************************************************************************************
//  CurveFit.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/24/2006 - James R. Carroll
//       Generated original version of source code.
//  09/17/2008 - James R. Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TVA.NumericalAnalysis
{
    /// <summary>
    /// Linear regression algorithm.
    /// </summary>
    public static class CurveFit
    {
        /// <summary>
        /// Computes linear regression over given values.
        /// </summary>
        public static double[] Compute(int polynomialOrder, IEnumerable<Point> values)
        {
            return Compute(polynomialOrder, values.Select(point => point.X).ToList(), values.Select(point => point.Y).ToList());
        }

        /// <summary>
        /// Computes linear regression over given values.
        /// </summary>
        public static double[] Compute(int polynomialOrder, IList<double> xValues, IList<double> yValues)
        {
            if (xValues == null)
                throw new ArgumentNullException("xValues");

            if (yValues == null)
                throw new ArgumentNullException("yValues");

            if (xValues.Count != yValues.Count)
                throw new ArgumentException("Point count for x-values and y-values must be equal");

            if (!(xValues.Count >= polynomialOrder + 1))
                throw new ArgumentException("Point count must be greater than requested polynomial order");

            if (!(polynomialOrder >= 1) && (polynomialOrder <= 7))
                throw new ArgumentOutOfRangeException("polynomialOrder", "Polynomial order must be between 1 and 7");

            // Curve fit function (courtesy of Brian Fox from DatAWare client code)
            double[] coeffs = new double[8];
            double[] sum = new double[22];
            double[] v = new double[12];
            double[,] b = new double[12, 13];
            double p, divB, fMultB, sigma;
            int ls, lb, lv, i1, i, j, k, l;
            int pointCount = xValues.Count;

            ls = polynomialOrder * 2;
            lb = polynomialOrder + 1;
            lv = polynomialOrder;
            sum[0] = pointCount;

            for (i = 0; i < pointCount; i++)
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
}