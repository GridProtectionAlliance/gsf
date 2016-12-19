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

using System;
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
        /// Defines filter SQL expression for restriction - does not include WHERE. When escaping is needed for field names, use standard ANSI quotes.
        /// </summary>
        /// <remarks>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression will be updated to reflect what is defined in the user model.
        /// </remarks>
        public string FilterExpression;

        /// <summary>
        /// Defines restriction parameter values.
        /// </summary>
        public object[] Parameters = new object[0];

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new parameterized <see cref="RecordRestriction"/>.
        /// </summary>
        public RecordRestriction()
        {
        }

        /// <summary>
        /// Creates a new parameterized <see cref="RecordRestriction"/> with the specified filter and parameters.
        /// </summary>
        /// <param name="filterExpression">Filter SQL expression for restriction - does not include WHERE. When escaping is needed for field names, use standard ANSI quotes.</param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <remarks>
        /// If needed, field names that are escaped with standard ANSI quotes in the <paramref name="filterExpression"/> will be updated to reflect what is defined in the user model.
        /// </remarks>
        public RecordRestriction(string filterExpression, params object[] parameters)
        {
            FilterExpression = filterExpression;
            Parameters = parameters;
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Combines two record restrictions with an AND condition.
        /// </summary>
        /// <param name="left"><see cref="RecordRestriction"/> left operand.</param>
        /// <param name="right"><see cref="RecordRestriction"/> right operand.</param>
        /// <returns>New combined record restriction.</returns>
        /// <exception cref="ArgumentNullException">Both record restriction parameters are <c>null</c>.</exception>
        /// <remarks>
        /// If one parameter is <c>null</c> and the other parameter is not, the non-null parameter will be returned.
        /// </remarks>
        public static RecordRestriction operator +(RecordRestriction left, RecordRestriction right)
        {
            return And(left, right);
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
        /// <exception cref="ArgumentNullException">Both record restriction parameters are <c>null</c>.</exception>
        /// <remarks>
        /// If one parameter is <c>null</c> and the other parameter is not, the non-null parameter will be returned.
        /// </remarks>
        public static RecordRestriction And(RecordRestriction left, RecordRestriction right)
        {
            return Combine(left, right, "AND");
        }

        /// <summary>
        /// Combines two record restrictions with an OR condition.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>New combined record restriction.</returns>
        /// <exception cref="ArgumentNullException">Both record restriction parameters are <c>null</c>.</exception>
        /// <remarks>
        /// If one parameter is <c>null</c> and the other parameter is not, the non-null parameter will be returned.
        /// </remarks>
        public static RecordRestriction Or(RecordRestriction left, RecordRestriction right)
        {
            return Combine(left, right, "OR");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RecordRestriction Combine(RecordRestriction left, RecordRestriction right, string operation)
        {
            if ((object)left == null && (object)right == null)
                throw new ArgumentNullException(nameof(left), "Both record restriction parameters are null - cannot combine");

            if ((object)left == null)
                return right;

            if ((object)right == null)
                return left;

            object[] parameters = left.Parameters.Combine(right.Parameters);
            int leftLength = left.Parameters.Length;
            int rightLength = right.Parameters.Length;

            for (int i = rightLength - 1; i >= 0; i--)
                right.FilterExpression = right.FilterExpression.Replace($"{{{i}}}", $"{{{leftLength + i}}}");

            return new RecordRestriction($"({left.FilterExpression}) {operation} ({right.FilterExpression})", parameters);
        }
    }

    #endregion
}