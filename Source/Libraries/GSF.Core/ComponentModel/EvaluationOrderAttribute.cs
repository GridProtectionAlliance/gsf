//******************************************************************************************************
//  EvaluationOrderAttribute.cs - Gbtc
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
//  04/10/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.ComponentModel
{
    /// <summary>
    /// Defines an attribute that will specify an evaluation order for properties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful for providing an order of operations to evaluations of
    /// <see cref="DefaultValueExpressionAttribute"/> attributes where one
    /// expression may be dependent on another. Note that properties are normally
    /// evaluated in the order in which they are defined in the class, using this
    /// property allows the order of evaluation to be changed. 
    /// </para>
    /// <para>
    /// When no <see cref="EvaluationOrderAttribute"/> is specified, the sort
    /// order for a property is assumed to be zero. Properties will be ordered
    /// numerically based on the specified <see cref="OrderIndex"/>.
    /// </para>
    /// <para>
    /// See <see cref="DefaultValueExpressionParser{T}.CreateInstance{TExpressionScope}"/>.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EvaluationOrderAttribute : Attribute
    {
        /// <summary>
        /// Gets the specified numeric evaluation order for a property.
        /// </summary>
        public int OrderIndex
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="EvaluationOrderAttribute"/>.
        /// </summary>
        /// <param name="orderIndex">Evaluation order for a property.</param>
        public EvaluationOrderAttribute(int orderIndex)
        {
            OrderIndex = orderIndex;
        }
    }
}