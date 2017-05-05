//******************************************************************************************************
//  TypeRegistryExtensions.cs - Gbtc
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
//  05/04/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using ExpressionEvaluator;

namespace GSF.ComponentModel
{
    /// <summary>
    /// Defines extension functions related to <see cref="TypeRegistry"/> manipulation.
    /// </summary>
    public static class TypeRegistryExtensions
    {
        /// <summary>
        /// Clones the specified <paramref name="typeRegistry"/>.
        /// </summary>
        /// <param name="typeRegistry"><see cref="TypeRegistry"/> to clone.</param>
        /// <returns>Clone of the specified <paramref name="typeRegistry"/>.</returns>
        public static TypeRegistry Clone(this TypeRegistry typeRegistry)
        {
            if ((object)typeRegistry == null)
                return null;

            TypeRegistry clonedTypeRegistry = new TypeRegistry();

            foreach (KeyValuePair<string, object> item in typeRegistry)
                clonedTypeRegistry[item.Key] = item.Value;

            return clonedTypeRegistry;
        }

        /// <summary>
        /// Registers the specified <paramref name="symbols"/> into the <paramref name="targetTypeRegistry"/>.
        /// </summary>
        /// <param name="symbols">Symbols to register.</param>
        /// <param name="targetTypeRegistry">Type registry to target; set to <c>null</c> for a clone of the default registry.</param>
        /// <returns>Target registry that with new registered symbols.</returns>
        public static TypeRegistry RegisterSymbols(this IEnumerable<KeyValuePair<string, object>> symbols, TypeRegistry targetTypeRegistry = null)
        {
            if ((object)targetTypeRegistry == null)
                targetTypeRegistry = ValueExpressionParser.DefaultTypeRegistry.Clone();

            if ((object)symbols != null)
            {
                foreach (KeyValuePair<string, object> item in symbols)
                    targetTypeRegistry.RegisterSymbol(item.Key, item.Value);
            }

            return targetTypeRegistry;
        }
    }
}
