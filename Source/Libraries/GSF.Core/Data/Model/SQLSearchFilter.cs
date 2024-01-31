//******************************************************************************************************
//  SQLSearchFilter.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  01/11/2024 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines a Filter to be applied to a Query
    /// </summary>
    public class SQLSearchFilter
    {
        private string m_operator;
        private string m_type;

        /// <summary>
        /// Gets or sets the Name of the field to be searched.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the value to be searched for.
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// Gets or sets the Operator to be used for the Search.
        /// </summary>
        /// <remarks>
        /// <para>The list of supported operators includes:</para>
        ///
        /// <list type="bullet">
        ///   <item>=</item>
        ///   <item><![CDATA[<>]]></item>
        ///   <item><![CDATA[<]]></item>
        ///   <item><![CDATA[>]]></item>
        ///   <item>IN</item>
        ///   <item>NOT IN</item>
        ///   <item>LIKE</item>
        ///   <item>NOT LIKE</item>
        ///   <item><![CDATA[<=]]></item>
        ///   <item><![CDATA[>=]]></item>
        /// </list>
        /// </remarks>
        /// <exception cref="NotSupportedException">Attempted to assign an operator that is not supported.</exception>
        public string Operator
        {
            get => m_operator;
            set
            {
                if (s_validOperators.Contains(value, StringComparer.OrdinalIgnoreCase))
                    m_operator = value;
                else
                    throw new NotSupportedException($"{value} is not a valid operator");
            }
        }

        /// <summary>
        /// Gets or sets the type of the field to be searched.
        /// </summary>
        /// <remarks>
        /// <para>The list of supported types includes:</para>
        ///
        /// <list type="bullet">
        ///   <item>integer</item>
        ///   <item>number</item>
        ///   <item>boolean</item>
        ///   <item>datetime</item>
        ///   <item>string</item>
        /// </list>
        /// </remarks>
        /// <exception cref="NotSupportedException">Attempted to assign a type that is not supported.</exception>
        public string Type
        {
            get => m_type;
            set //
            {
                if (s_validTypes.Contains(value, StringComparer.OrdinalIgnoreCase))
                    m_type = value;
                // This is needed for legacy support so we do not break anything it can be removed in the future.
                else if (string.Equals(value, "query", StringComparison.OrdinalIgnoreCase))
                    m_type = value;
                else
                    throw new NotSupportedException($"{value} is not a valid type");
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether this is a pivot column
        /// in which case the <see cref="SearchableAttribute"/> should be used.
        /// </summary>
        public bool IsPivotColumn { get; set; }

        /// <summary>
        /// Generates the conditional statement to be used in the WHERE clause of the SQL statement.
        /// </summary>
        /// <param name="parameters">A list of the parameters to be substituted into the full query.</param>
        /// <returns>The conditional statement with format string syntax for parameter substitution."</returns>
        public string GenerateConditional(List<object> parameters)
        {
            string escape = "";
            if (string.Equals(Operator, "LIKE", StringComparison.OrdinalIgnoreCase) || string.Equals(Operator, "NOT LIKE", StringComparison.OrdinalIgnoreCase))
                escape = "ESCAPE '$'";

            string query = "";
            string searchText = SearchText;

            // For legacy support. In the future, remove the query type.
            if (string.Equals(Type, "query", StringComparison.OrdinalIgnoreCase))
                query = $"{(IsPivotColumn ? "AFV_" : "") + FieldName} {Operator} {SearchText}";
            else
            {
                query = $"[{(IsPivotColumn ? "AFV_" : "") + FieldName.RemoveCharacter('[').RemoveCharacter(']')}] {Operator} ";

                /* This requires switch to the new search format
                    if (Operator == "IN" || Operator == "NOT IN")
                    {
                        IEnumerable<string> valueList = searchText
                            .Replace("(", "")
                            .Replace(")", "")
                            .Split(',');

                        searchText = $"({string.Join(",", valueList.Select((s, i) => $"{{{i + parameters.Count}}}"))})";
                        parameters.AddRange(valueList);
                    }
                */

                /* We update to remove the following once we move to new SQL injection logic,
                 * but it is needed in here for now since I had to leave the logic out
                 * of the above to avoid legacy issues
                 */

                if (string.Equals(Operator, "IN", StringComparison.OrdinalIgnoreCase) || string.Equals(Operator, "NOT IN", StringComparison.OrdinalIgnoreCase))
                {
                    IEnumerable<string> valueList = searchText
                        .Replace("(", "")
                        .Replace(")", "")
                        .Split(',')
                        .Select(t => $"'{t}'");

                    searchText = $"({string.Join(",", valueList)})";
                    query += $" {searchText}";
                }
                else
                {
                    query += $" {{{parameters.Count}}} {escape}";
                    parameters.Add(searchText);
                }
            }

            return query;
        }

        private static readonly string[] s_validOperators = { "=", "<>", "<", ">", "IN", "NOT IN", "LIKE", "NOT LIKE", "<=", ">=" };
        private static readonly string[] s_validTypes = { "integer", "number", "boolean", "datetime", "string" };
    }
}