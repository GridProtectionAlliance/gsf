//******************************************************************************************************
//  AggregateFunctions.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  01/18/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Ciloci.Flee;
using GSF.NumericalAnalysis;
using System;
using System.Collections.Concurrent;
using System.Linq;
using CompiledExpression = System.Tuple<Ciloci.Flee.ExpressionContext, Ciloci.Flee.IGenericExpression<bool>>;

namespace DynamicCalculator
{
    /// <summary>
    /// Common aggregate functions for use with array variables in the <see cref="DynamicCalculator"/>.
    /// </summary>
    public static class AggregateFunctions
    {
        /// <summary>
        /// Gets the number of items in the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <returns>Array length.</returns>
        public static int Count(double[] array) => array.Length;

        /// <summary>
        /// Gets the sum of the values in the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <returns>Array values sum.</returns>
        public static double Sum(double[] array) => array.Sum();

        /// <summary>
        /// Gets the minimum of the values in the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <returns>Array values minimum.</returns>
        public static double Min(double[] array) => array.Min();

        /// <summary>
        /// Gets the maximum of the values in the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <returns>Array values maximum.</returns>
        public static double Max(double[] array) => array.Max();

        /// <summary>
        /// Gets the average of the values in the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <returns>Array values average.</returns>
        public static double Avg(double[] array) => array.Average();

        /// <summary>
        /// Gets the standard deviation of the values in the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <returns>Array values standard deviation.</returns>
        public static double StdDev(double[] array) => array.StandardDeviation();

        /// <summary>
        /// Gets flag that determines if any of the values in the <paramref name="array"/>
        /// evaluate to <c>true</c> for the provided <paramref name="comparisonExpr"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <param name="comparisonExpr">Comparison expression, e.g., "> 0".</param>
        /// <returns>
        /// Cumulative boolean result for any values evaluating to <c>true</c> for
        /// provided comparison expression.
        /// </returns>
        /// <remarks>
        /// Expression is compiled with support for <see cref="Math"/> and <see cref="DateTime"/> types so
        /// related static functions and constants can be used in expressions, e.g., <c>">= 2 * PI"</c>.
        /// </remarks>
        public static bool Any(double[] array, string comparisonExpr)
        {
            CompiledExpression compiledExpr = GetCompiledExpression(comparisonExpr);
            return array.Any(value => EvaluateExpression(compiledExpr, value));
        }

        /// <summary>
        /// Gets flag that determines if all of the values in the <paramref name="array"/>
        /// evaluate to <c>true</c> for the provided <paramref name="comparisonExpr"/>.
        /// </summary>
        /// <param name="array">Source value array.</param>
        /// <param name="comparisonExpr">Comparison expression, e.g., "> 0".</param>
        /// <returns>
        /// Cumulative boolean result for all values evaluating to <c>true</c> for
        /// provided comparison expression.
        /// </returns>
        /// <remarks>
        /// Expression is compiled with support for <see cref="Math"/> and <see cref="DateTime"/> types so
        /// related static functions and constants can be used in expressions, e.g., <c>">= 2 * PI"</c>.
        /// </remarks>
        public static bool All(double[] array, string comparisonExpr)
        {
            CompiledExpression compiledExpr = GetCompiledExpression(comparisonExpr);
            return array.All(value => EvaluateExpression(compiledExpr, value));
        }

        private static bool EvaluateExpression(CompiledExpression compiledExpr, double value)
        {
            ExpressionContext context = compiledExpr.Item1;
            
            lock (context)
            {
                context.Variables["value"] = value;
                IGenericExpression<bool> expression = compiledExpr.Item2;
                return expression.Evaluate();
            }
        }

        private static CompiledExpression GetCompiledExpression(string comparisonExpr) =>
            s_comparisonExpressions.GetOrAdd(comparisonExpr, expr =>
            {
                ExpressionContext context = new();
                context.Imports.AddType(typeof(Math));
                context.Imports.AddType(typeof(DateTime));
                context.Variables.Add("value", double.NaN);
                IGenericExpression<bool> expression = context.CompileGeneric<bool>($"value {expr}");
                return Tuple.Create(context, expression);
            });

        private static readonly ConcurrentDictionary<string, CompiledExpression> s_comparisonExpressions = new(StringComparer.OrdinalIgnoreCase);
    }
}
