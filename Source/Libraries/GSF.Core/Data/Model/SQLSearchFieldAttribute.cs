//******************************************************************************************************
//  SQLSearchFieldAttribute.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  09/20/2023 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************


using System;
using System.Collections.Generic;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that translates Search Queries to more complex SQL Queries 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class SQLSearchFieldAttribute : Attribute 
    {
        /// <summary>
        /// Gets field name to use for search.
        /// </summary>
        public string Field
        {
            get;
        }

        /// <summary>
        /// Generates the Fieldname used in SQL
        /// </summary>
        public string SQLFieldName
        {
            get;
        }

        /// <summary>
        /// Generates the Condition used in SQL
        /// </summary>
        public Func<string,string> Condition
        {
            get;
        }

        /// <summary>
        /// Generates the Query used in SQL
        /// </summary>
        public Func<string,string, string> Query
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="SQLSearchFieldAttribute"/>.
        /// </summary>
        /// <param name="field">field used on the UI to trigegr this transformation.</param>
        /// <param name="sqlFieldName"> SQL field used in the query. Defaults to <see cref='Field' />.</param>
        /// <param name="condition"> Function that takes the  condition specified (=,>,<,<>,IN, NOT IN) and returns the condition used in the query.</param>
        /// <param name="query"> Function that takes the  condition specified and the substitution that will be used for value e.g. {0} or ({1},{2}) etc.</param>        
        public SQLSearchFieldAttribute(string field, string sqlFieldName = null, Func<string,string> condition = null, Func<string,string,string> query = null)
        {
            Field = field;
            SQLFieldName = sqlFieldName ?? field;
            Condition = condition ?? ((c) => c);
            Query = query ?? ((cond, val) => $"{val}");

        }

    }
}