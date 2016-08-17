//******************************************************************************************************
//  RecordOperationsCache.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  01/14/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using GSF.Collections;
using GSF.Data.Model;
using GSF.Reflection;
using GSF.Web.Security;
using Microsoft.AspNet.SignalR.Hubs;

namespace GSF.Web.Hubs
{
    /// <summary>
    /// Defines class that caches data hub operations.
    /// </summary>
    public class RecordOperationsCache
    {
        #region [ Members ]

        // Fields
        private readonly Dictionary<Type, Tuple<string, string>[]> m_recordOperations;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="RecordOperationsCache"/>.
        /// </summary>
        /// <param name="hub">Hub to analyze for record operations.</param>
        public RecordOperationsCache(Type hub)
        {
            int recordOperations = Enum.GetValues(typeof(RecordOperation)).Length;
            m_recordOperations = new Dictionary<Type, Tuple<string, string>[]>();

            // Analyze and cache data hub methods that are targeted for record operations
            foreach (MethodInfo method in hub.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                AuthorizeHubRoleAttribute authorizeHubRoleAttribute;
                RecordOperationAttribute recordOperationAttribute;

                method.TryGetAttribute(out authorizeHubRoleAttribute);

                if (method.TryGetAttribute(out recordOperationAttribute))
                {
                    // Cache method name and any defined authorized roles for current record operation
                    m_recordOperations.GetOrAdd(recordOperationAttribute.ModelType,
                        type => new Tuple<string, string>[recordOperations])[(int)recordOperationAttribute.Operation] =
                        new Tuple<string, string>(method.Name, authorizeHubRoleAttribute?.Roles);
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets record operation methods for specified modeled table.
        /// </summary>
        /// <typeparam name="TModel">Modeled table.</typeparam>
        /// <returns>record operation methods for specified modeled table.</returns>
        public Tuple<string, string>[] GetRecordOperations<TModel>() where TModel : class, new()
        {
            return GetRecordOperations(typeof(TModel));
        }

        /// <summary>
        /// Gets record operation methods for specified modeled table type.
        /// </summary>
        /// <param name="model">Model type.</param>
        /// <returns>record operation methods for specified modeled table type.</returns>
        public Tuple<string, string>[] GetRecordOperations(Type model)
        {
            return m_recordOperations[model];
        }

        #endregion
    }

    /// <summary>
    /// Defines class that caches data hub operations for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Specific <see cref="IHub"/> type for record operations cache.</typeparam>
    public class RecordOperationsCache<T> : RecordOperationsCache where T : IHub
    {
        /// <summary>
        /// Creates a new <see cref="RecordOperationsCache{T}"/>.
        /// </summary>
        public RecordOperationsCache() : base(typeof(T))
        {
        }
    }
}
