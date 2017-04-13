//******************************************************************************************************
//  RecordRestriction.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/13/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines a parameterized record restriction that can be applied to queries.
    /// </summary>
    public class RecordRestriction
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Defines filter SQL expression for restriction as a composite format string - does not
        /// include WHERE. When escaping is needed for field names, use standard ANSI quotes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format string will be converted into
        /// query parameters where each of the corresponding values in the <see cref="Parameters"/>
        /// collection will be applied as <see cref="IDbDataParameter"/> values to an executed
        /// <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// </remarks>
        public string FilterExpression;

        /// <summary>
        /// Defines restriction parameter values.
        /// </summary>
        public object[] Parameters;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new parameterized <see cref="RecordRestriction"/>.
        /// </summary>
        public RecordRestriction()
        {
            Parameters = new object[0];
        }

        /// <summary>
        /// Creates a new parameterized <see cref="RecordRestriction"/> with the specified SQL filter
        /// expression and parameters.
        /// </summary>
        /// <param name="filterExpression">
        /// Filter SQL expression for restriction as a composite format string - does not include WHERE.
        /// When escaping is needed for field names, use standard ANSI quotes.
        /// </param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <remarks>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format <paramref name="filterExpression"/>
        /// will be converted into query parameters where each of the corresponding values in the
        /// <paramref name="parameters"/> collection will be applied as <see cref="IDbDataParameter"/>
        /// values to an executed <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// </remarks>
        public RecordRestriction(string filterExpression, params object[] parameters)
        {
            FilterExpression = filterExpression;
            Parameters = parameters;
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Implicitly converts a <see cref="string"/> based filter expression into a <see cref="RecordRestriction"/>.
        /// </summary>
        /// <param name="value">Operand.</param>
        /// <returns>Record operation representing the specified filter expression.</returns>
        public static implicit operator RecordRestriction(string value) => new RecordRestriction(value);

        /// <summary>
        /// Combines two record restrictions with an AND condition.
        /// </summary>
        /// <param name="left"><see cref="RecordRestriction"/> left operand.</param>
        /// <param name="right"><see cref="RecordRestriction"/> right operand.</param>
        /// <returns>New combined record restriction.</returns>
        /// <remarks>
        /// If both parameters are <c>null</c>, result will be <c>null</c>. If one parameter is <c>null</c>
        /// and the other parameter is not, the non-null parameter will be returned. Equally, if the
        /// <see cref="FilterExpression"/> of both parameters are <c>null</c>, empty or only whitespace,
        /// then the result will be <c>null</c>. If one parameter has a filter expression that is
        /// <c>null</c>, empty or whitespace and the other parameter's filter expression is defined, then
        /// the parameter that has a filter expression will be returned.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1013:OverloadOperatorEqualsOnOverloadingAddAndSubtract")]
        public static RecordRestriction operator +(RecordRestriction left, RecordRestriction right)
        {
            return CombineAnd(left, right);
        }

        /// <summary>
        /// Combines two record restrictions with an AND condition.
        /// </summary>
        /// <param name="left"><see cref="RecordRestriction"/> left operand.</param>
        /// <param name="right"><see cref="RecordRestriction"/> right operand.</param>
        /// <returns>New combined record restriction.</returns>
        /// <remarks>
        /// If both parameters are <c>null</c>, result will be <c>null</c>. If one parameter is <c>null</c>
        /// and the other parameter is not, the non-null parameter will be returned. Equally, if the
        /// <see cref="FilterExpression"/> of both parameters are <c>null</c>, empty or only whitespace,
        /// then the result will be <c>null</c>. If one parameter has a filter expression that is
        /// <c>null</c>, empty or whitespace and the other parameter's filter expression is defined, then
        /// the parameter that has a filter expression will be returned.
        /// </remarks>
        public static RecordRestriction operator &(RecordRestriction left, RecordRestriction right)
        {
            return CombineAnd(left, right);
        }

        /// <summary>
        /// Combines two record restrictions with an OR condition.
        /// </summary>
        /// <param name="left"><see cref="RecordRestriction"/> left operand.</param>
        /// <param name="right"><see cref="RecordRestriction"/> right operand.</param>
        /// <returns>New combined record restriction.</returns>
        /// <remarks>
        /// If both parameters are <c>null</c>, result will be <c>null</c>. If one parameter is <c>null</c>
        /// and the other parameter is not, the non-null parameter will be returned. Equally, if the
        /// <see cref="FilterExpression"/> of both parameters are <c>null</c>, empty or only whitespace,
        /// then the result will be <c>null</c>. If one parameter has a filter expression that is
        /// <c>null</c>, empty or whitespace and the other parameter's filter expression is defined, then
        /// the parameter that has a filter expression will be returned.
        /// </remarks>
        public static RecordRestriction operator |(RecordRestriction left, RecordRestriction right)
        {
            return CombineOr(left, right);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Combines two record restrictions with an AND condition.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>New combined record restriction.</returns>
        /// <remarks>
        /// If both parameters are <c>null</c>, result will be <c>null</c>. If one parameter is <c>null</c>
        /// and the other parameter is not, the non-null parameter will be returned. Equally, if the
        /// <see cref="FilterExpression"/> of both parameters are <c>null</c>, empty or only whitespace,
        /// then the result will be <c>null</c>. If one parameter has a filter expression that is
        /// <c>null</c>, empty or whitespace and the other parameter's filter expression is defined, then
        /// the parameter that has a filter expression will be returned.
        /// </remarks>
        public static RecordRestriction CombineAnd(RecordRestriction left, RecordRestriction right)
        {
            return Combine(left, right, "AND");
        }

        /// <summary>
        /// Combines two record restrictions with an OR condition.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>New combined record restriction.</returns>
        /// <remarks>
        /// If both parameters are <c>null</c>, result will be <c>null</c>. If one parameter is <c>null</c>
        /// and the other parameter is not, the non-null parameter will be returned. Equally, if the
        /// <see cref="FilterExpression"/> of both parameters are <c>null</c>, empty or only whitespace,
        /// then the result will be <c>null</c>. If one parameter has a filter expression that is
        /// <c>null</c>, empty or whitespace and the other parameter's filter expression is defined, then
        /// the parameter that has a filter expression will be returned.
        /// </remarks>
        public static RecordRestriction CombineOr(RecordRestriction left, RecordRestriction right)
        {
            return Combine(left, right, "OR");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RecordRestriction Combine(RecordRestriction left, RecordRestriction right, string operation)
        {
            // Check for null operands
            if ((object)left == null && (object)right == null)
                return null;

            if ((object)left == null)
                return right;

            if ((object)right == null)
                return left;

            // Check for empty filter expressions
            bool leftEmpty = string.IsNullOrWhiteSpace(left.FilterExpression);
            bool rightEmpty = string.IsNullOrWhiteSpace(right.FilterExpression);

            if (leftEmpty && rightEmpty)
                return null;

            if (leftEmpty)
                return right;

            if (rightEmpty)
                return left;

            // Check for missing parameters
            int leftLength = left.Parameters?.Length ?? 0;
            int rightLength = right.Parameters?.Length ?? 0;

            if (leftLength == 0 && rightLength == 0)
                return new RecordRestriction($"({left.FilterExpression}) {operation} ({right.FilterExpression})");

            object[] parameters = leftLength == 0 ? right.Parameters : (rightLength == 0 ? left.Parameters : left.Parameters.Combine(right.Parameters));

            object[] offsetArgs = Enumerable.Range(leftLength, rightLength).Select(index => (object)$"{{{index}}}").ToArray();

            return new RecordRestriction($"({left.FilterExpression}) {operation} ({string.Format(right.FilterExpression, offsetArgs)})", parameters);
        }
    }

    #endregion
}