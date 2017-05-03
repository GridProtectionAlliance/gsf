//******************************************************************************************************
//  ValueExpressionAttributeBase.cs - Gbtc
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GSF.ComponentModel
{
    /// <summary>
    /// Represents a base attribute class for C# expressions that when evaluated will specify a new value for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ValueExpressionAttributeBase : Attribute
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ValueExpressionAttributeBase"/>
        /// </summary>
        /// <param name="expression">C# expression that will evaluate to the desired value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> cannot be <c>null</c>.</exception>
        protected ValueExpressionAttributeBase(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentNullException(nameof(expression));

            Expression = expression;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets C# expression that will evaluate to the desired value.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="GetPropertyUpdateValue"/> method to get a per-property
        /// based value for use with the <see cref="ValueExpressionParser"/>. Use
        /// <see cref="DeriveExpression"/> to automatically replace any <c>this</c>
        /// keywords with <c>Instance</c> so as to properly reference the modeled
        /// <see cref="ValueExpressionScopeBase{T}.Instance"/> value.
        /// </remarks>
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
        /// <see cref="ValueExpressionAttributeBase"/> attributes where one expression may
        /// be dependent on another. Note that properties are normally evaluated in the order
        /// in which they are defined in the class, but this is not guaranteed, using this
        /// attribute allows the order of evaluation to be changed. 
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
        /// See <see cref="ValueExpressionParser{T}.CreateInstance{TExpressionScope}"/>.
        /// </para>
        /// </remarks>
        public int EvaluationOrder
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="Expression"/> based value used to update a modeled property.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Expression based on source property.</returns>
        /// <remarks>
        /// <para>
        /// This function allows derived attribute implementations to adjust the expression based
        /// on property information, e.g., property type.
        /// </para>
        /// <para>
        /// The property update value is typically used to assign expression values to a modeled type.
        /// </para>
        /// </remarks>
        public virtual string GetPropertyUpdateValue(PropertyInfo property) => Expression;

        /// <summary>
        /// Gets the modeled property based value used to update the <see cref="Expression"/>.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Assignment expression based on source property.</returns>
        /// <remarks>
        /// <para>
        /// This function allows derived attribute implementations to adjust the update value
        /// based on property information, e.g., property type.
        /// </para>
        /// <para>
        /// The expression update value is typically used to assign modeled property values back
        /// to expressions allowing synchronization of a model with an external source, e.g., a
        /// user interface element.
        /// </para>
        /// </remarks>
        public virtual string GetExpressionUpdateValue(PropertyInfo property) => $"Instance.{property.Name}";

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Derives an expression based on <paramref name="property"/> info with any <c>this</c>
        /// keywords properly referencing <see cref="ValueExpressionScopeBase{T}.Instance"/> value.
        /// </summary>
        /// <param name="expression">Expression to derive, typically from <see cref="GetPropertyUpdateValue"/> or <see cref="GetExpressionUpdateValue"/>.</param>
        /// <param name="valueExpressionAttribute">Associated <see cref="ValueExpressionAttributeBase"/> instance.</param>
        /// <param name="property">Associated <see cref="PropertyInfo"/> instance.</param>
        /// <param name="typeName">Modeled type name, e.g., typeof&lt;T&gt;.FullName.</param>
        /// <returns>Derived expression with any <c>this</c> keywords properly referencing <see cref="ValueExpressionScopeBase{T}.Instance"/> value.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ValueExpressionAttributeBase only targets properties")]
        public static string DeriveExpression(string expression, ValueExpressionAttributeBase valueExpressionAttribute, PropertyInfo property, string typeName)
        {
            if ((object)expression == null)
                throw new ArgumentNullException(nameof(expression));

            // Check for "this" keywords in expression
            if (ValueExpressionParser.HasThisKeywords(expression))
            {
                if (valueExpressionAttribute.EvaluationOrder < 1)
                    throw new EvaluationOrderException($"Value expression attribute for property \"{typeName}.{property.Name}\" references the \"this\" keyword and must specify a positive \"EvaluationOrder\".");

                // Replace all references to "this" with "Instance"
                expression = ValueExpressionParser.ReplaceThisKeywords(expression, "Instance");
            }

            return expression;
        }

        #endregion
    }
}
