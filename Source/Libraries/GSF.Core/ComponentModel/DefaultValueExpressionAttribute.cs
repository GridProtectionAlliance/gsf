//******************************************************************************************************
//  DefaultValueExpressionAttribute.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  04/07/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.ComponentModel
{
    /// <summary>
    /// Defines a C# expression attribute that when evaluated will specify the default value for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DefaultValueExpressionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets C# expression that will evaluate to the desired default value.
        /// </summary>
        public string Expression
        {
            get;
        }

        /// <summary>
        /// Gets or sets value that determines if value should be cached after first evaluation.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool Cached
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the numeric evaluation order for this expression. Defaults to zero.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is useful for providing an order of operations to evaluations of
        /// <see cref="DefaultValueExpressionAttribute"/> attributes where one expression may
        /// be dependent on another. Note that properties are normally evaluated in the order
        /// in which they are defined in the class, using this attribute allows the order of
        /// evaluation to be changed. 
        /// </para>
        /// <para>
        /// When no <see cref="EvaluationOrder"/> is specified, the sort order for a property
        /// will be zero. Properties will be ordered numerically based on this value.
        /// </para>
        /// <para>
        /// For any the <see cref="Expression"/> value that references the <c>this</c> keyword,
        /// a positive evaluation order will be required.
        /// </para>
        /// <para>
        /// See <see cref="DefaultValueExpressionParser{T}.CreateInstance{TExpressionScope}"/>.
        /// </para>
        /// </remarks>
        public int EvaluationOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new <see cref="DefaultValueExpressionAttribute"/>
        /// </summary>
        /// <param name="expression">C# expression that will evaluate to the desired default value.</param>
        public DefaultValueExpressionAttribute(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentNullException(nameof(expression));

            Expression = expression;
        }
    }
}
