//******************************************************************************************************
//  TransformEqualityComparer.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  08/24/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF
{
    /// <summary>
    /// Equality comparer that compares objects by the results of a transformation.
    /// </summary>
    /// <typeparam name="T">The type of the source objects to be compared.</typeparam>
    public class TransformEqualityComparer<T> : IEqualityComparer<T>
    {
        #region [ Members ]

        // Fields
        private Func<T, object> m_transformFunction;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="TransformEqualityComparer{T}"/> class.
        /// </summary>
        /// <param name="transformFunction">The function used to transform source objects to the type used for comparison.</param>
        public TransformEqualityComparer(Func<T, object> transformFunction)
        {
            if ((object)transformFunction == null)
                throw new ArgumentNullException(nameof(transformFunction));

            m_transformFunction = transformFunction;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <typeparamref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <typeparamref name="T"/> to compare.</param>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        public bool Equals(T x, T y)
        {
            return Equals(Transform(x), Transform(y));
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> for which a hash code is to be returned.</param>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(T obj)
        {
            return Transform(obj).GetHashCode();
        }

        // Applies the transformation function to the
        // given source object and returns the result.
        private object Transform(T obj)
        {
            return m_transformFunction(obj);
        }

        #endregion
    }
}
