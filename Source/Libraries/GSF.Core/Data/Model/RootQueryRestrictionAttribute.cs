//******************************************************************************************************
//  RootQueryRestrictionAttribute.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  05/14/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will mark a modeled table with a record restriction that applies
    /// to all query functions for modeled <see cref="TableOperations{T}"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RootQueryRestrictionAttribute : Attribute
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
        public readonly string FilterExpression;

        /// <summary>
        /// Defines restriction parameter values.
        /// </summary>
        public readonly object[] Parameters;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new parameterized <see cref="RootQueryRestrictionAttribute"/> with the specified
        /// SQL filter expression and parameters.
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
        public RootQueryRestrictionAttribute(string filterExpression, params object[] parameters)
        {
            FilterExpression = filterExpression;
            Parameters = parameters;
        }

        #endregion
    }
}