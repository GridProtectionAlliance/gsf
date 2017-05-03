//******************************************************************************************************
//  TypeConvertedValueExpressionAttribute.cs - Gbtc
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
//  05/02/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using System.Reflection;

namespace GSF.ComponentModel
{
    /// <summary>
    /// Defines a C# expression attribute that when evaluated will type convert its value for a property.
    /// </summary>
    /// <remarks>
    /// This attribute is typically used when synchronizing modeled values to an external sources, e.g.,
    /// user interface elements.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TypeConvertedValueExpressionAttribute : ValueExpressionAttributeBase
    {
        /// <summary>
        /// Gets the return <see cref="Type"/> for this <see cref="ValueExpressionAttributeBase.Expression"/>.
        /// </summary>
        public Type ReturnType
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="TypeConvertedValueExpressionAttribute"/>
        /// </summary>
        /// <param name="expression">C# expression that will evaluate to the type converted property value.</param>
        /// <param name="returnType">Return type for specified C# <paramref name="expression"/>.</param>
        public TypeConvertedValueExpressionAttribute(string expression, Type returnType) : base(expression)
        {
            ReturnType = returnType;
        }

        /// <summary>
        /// Gets the <see cref="ValueExpressionAttributeBase.Expression"/> based value used to update a modeled property.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Expression based on source property.</returns>
        /// <remarks>
        /// The property update value is typically used to assign expression values to a modeled type. For example:
        /// <code>
        /// [TypeConvertedValueExpression("Form.maskedTextBoxMessageInterval.Text", typeof(string))]
        /// public int MessageInterval { get; set; }
        /// </code>
        /// Would generate an expression of "Common.TypeConvertFromString(Form.maskedTextBoxMessageInterval.Text, typeof(int))"
        /// which would be executed as part of an overall expression that looked like
        /// <c>Instance.MessageInterval = Common.TypeConvertFromString(Form.maskedTextBoxMessageInterval.Text, typeof(int))</c>
        /// when called from <see cref="ValueExpressionParser{T}.UpdateProperties"/>.
        /// </remarks>
        public override string GetPropertyUpdateValue(PropertyInfo property)
        {
            Type sourceType = property.PropertyType;

            if (ReturnType == sourceType)
                return $"{Expression}";

            if (ReturnType == typeof(string))
                return $"Common.TypeConvertFromString({Expression}, typeof({sourceType.FullName}))";

            return $"Convert.ChangeType({Expression}, typeof({sourceType.FullName}))";
        }

        /// <summary>
        /// Gets the modeled property based value used to update the <see cref="ValueExpressionAttributeBase.Expression"/>.
        /// </summary>
        /// <param name="property">Property from which attribute was derived.</param>
        /// <returns>Expression based on source property.</returns>
        /// <remarks>
        /// The expression update value is typically used to assign modeled property values back
        /// to expressions allowing synchronization of a model with an external source, e.g., a
        /// user interface element. For example:
        /// <code>
        /// [TypeConvertedValueExpression("Form.maskedTextBoxMessageInterval.Text", typeof(string))]
        /// public int MessageInterval { get; set; }
        /// </code>
        /// Would generate an update expression of "Common.TypeConvertToString(Instance.MessageInterval)"
        /// which would be executed as part of an overall expression that looked like
        /// <c>Form.maskedTextBoxMessageInterval.Text = Common.TypeConvertToString(Instance.MessageInterval)</c>
        /// when called from <see cref="ValueExpressionParser{T}.UpdateExpressions"/>.
        /// </remarks>
        public override string GetExpressionUpdateValue(PropertyInfo property)
        {
            if (ReturnType == property.PropertyType)
                return $"Instance.{property.Name}";

            if (ReturnType == typeof(string))
                return $"Common.TypeConvertToString(Instance.{property.Name})";

            return $"Convert.ChangeType(Instance.{property.Name}, typeof({ReturnType.FullName}))";
        }
    }
}
