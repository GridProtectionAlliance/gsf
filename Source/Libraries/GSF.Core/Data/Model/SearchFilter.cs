//******************************************************************************************************
//  SearchFilter.cs - Gbtc
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
using System.Web.UI.WebControls;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines a Filter to be applied to a Query
    /// </summary>
    public class SQLSearchFilter  
    {
        private string m_Operator;
        private string m_Type;

        /// <summary>
        /// The Name of the Field to be Searched
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The Vakue to be searched for
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// The Operator to be used for the Search
        /// </summary>
        public string Operator 
        {
            get => m_Operator; 
            set 
            { 
                if (validOperators.Contains(value))
                    m_Operator = value;
                else
                    throw new ArgumentException($" {value} is not a valid operator");
            }
        }

        /// <summary>
        /// The Type of the Field
        /// </summary>
        public string Type {
            get => m_Type;
            set 
            {
                if (validTypes.Contains(value))
                    m_Type = value;
                else if (value == "query") // This is needed for legacy support so we do not break anything it can be removed in the future.
                    m_Type = value;
                else
                    throw new ArgumentException($" {value} is not a valid type");
            } 
        }

        /// <summary>
        /// A Flag Indicating wheter this is a Pivot Column and the <see cref="SearchableAttribute"/> should be used
        /// </summary>
        public bool isPivotColumn { get; set; } = false;

        /// <summary>
        /// Generates the Query to be used in SQL
        /// </summary>
        /// <param name="parameters"> an array of the Parameters to be substituted into the query</param>
        /// <param name="nParameters"> The number of Parameters already used. If not specified 0 is used</param>
        /// <returns>The query with the parameters substituted as "{nP+1...}" </returns>
        public string GenerateQuery(out object[] parameters, int nParameters = 0)
        {
            string escape = "";
            if (Operator == "LIKE" || Operator == "NOT LIKE")
                escape = "ESCAPE '$'";

            string query = "";
            string searchText = SearchText;

            // For Legacy Support. In Future remove IsQuery option.
            if (Type == "query")
                query = $"{(isPivotColumn ? "AFV_" : "") + FieldName} {Operator} ";
            else
                query = $"[{(isPivotColumn ? "AFV_" : "") + FieldName.RemoveCharacter('[').Remove(']')}] {Operator} ";
            

            /* This requires switch to the new search format 
                if (Operator == "IN")
                {
                    string text = SearchText.Replace("(", "").Replace(")", "");
                    List<string> things = text.Split(',').ToList();
                    parameters = things.ToArray();
                    searchText =  $"({string.Join(",",things.Select((s,i) => $"{{{i+nParameters}}}"))})";
                }
            */

            /* We update remove the following once we move to new SQL injection logic,
            * but it is needed in here for now since I had to leave the logic out 
            * of the above to avoid legacy issues
            */

            if (Type != "query" && (Operator == "IN" || Operator == "NOT IN"))
            {
                string text = searchText.Replace("(", "").Replace(")", "");
                List<string> things = text.Split(',').ToList();
                things = things.Select(t => $"'{t}'").ToList();
                searchText = $"({string.Join(",", things)})";
                query += $" {searchText}";
                parameters = new object[] {};
            }
            else
            {
                parameters = new object[] { searchText };
                query += $" {{{nParameters}}} {escape}";
            }

            return query;
        }

        private static string[] validOperators = new string[] { "=", "<>", "<", ">", "IN", "NOT IN", "LIKE", "NOT LIKE", "<=", ">=" };
        private static string[] validTypes = new string[] { "integer", "number", "boolean", "datetime", "string" };
    }
}