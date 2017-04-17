//******************************************************************************************************
//  ValueExpressionScopeBase.cs - Gbtc
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
//  04/10/2017 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.ComponentModel
{
    /// <summary>
    /// Represent a base class used for providing contextual scope when evaluating
    /// instances of the <see cref="ValueExpressionAttributeBase"/>.
    /// </summary>
    /// <remarks>
    /// This class should be extended with public instance fields that will be automatically
    /// exposed to <see cref="ValueExpressionAttributeBase"/> expressions.
    /// </remarks>
    /// <typeparam name="T">Type of associated instance.</typeparam>
    public abstract class ValueExpressionScopeBase<T>
    {
        /// <summary>
        /// References the current <typeparamref name="T"/> instance.
        /// </summary>
        /// <remarks>
        /// By using the <see cref="ValueExpressionParser.ReplaceThisKeywords"/> function, expressions
        /// can reference the current <typeparamref name="T"/> instance using the <c>this</c> keyword.
        /// See <see cref="ValueExpressionParser{T}.CreateInstance{TExpressionScope}"/>.
        /// </remarks>
        public T Instance;

        /// <summary>
        /// Returns <paramref name="nonNullValue"/> if <paramref name="value"/> is null.
        /// </summary>
        /// <typeparam name="TReturn">Strongly typed non-null value.</typeparam>
        /// <param name="value">Value to test.</param>
        /// <param name="nonNullValue">Value to return if primary value is null.</param>
        /// <returns><paramref name="value"/> if not <c>null</c>; otherwise <paramref name="nonNullValue"/>.</returns>
        /// <remarks>
        /// This function is useful for parameter type matching when using the ExpressionEvaluator.
        /// </remarks>
        public object NotNull<TReturn>(object value, TReturn nonNullValue) => Common.NotNull(value, nonNullValue);
    }
}
