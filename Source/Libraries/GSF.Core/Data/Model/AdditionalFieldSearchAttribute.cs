//******************************************************************************************************
//  AdditionalFieldSearchAttribute.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  06/12/2021 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will allow setting a custom view a modeled table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class AdditionalFieldSearchAttribute : Attribute
    {
        /// <summary>
        /// Gets the condition used to reduce the number of rows.
        /// </summary>
        public string Condition
        {
            get;
        }

        /// <summary>
        /// Gets Table name to use for AdditionalFields.
        /// </summary>
        public string AdditionalFieldTable
        {
            get;
        }

        /// <summary>
        /// Gets Field name to use for matching <see cref="PrimaryKeyAttribute"/>.
        /// </summary>
        public string PrimaryKeyField
        {
            get;
        }

        /// <summary>
        /// Gets Field name to use for matching Values.
        /// </summary>
        public string ValueField
        {
            get;
        }

        /// <summary>
        /// Gets Field name to use for matching FieldNames.
        /// </summary>
        public string FieldKeyField
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="AdditionalFieldSearchAttribute"/>.
        /// </summary>
        /// <param name="condition">Condition to be true for any row in the additionalFields Table to be used.</param>
        /// <param name="additionalFieldsTable">SQL Table containing additional Fields.</param>
        /// <param name="primaryKeyCollumn">SQL Column that will be matched to <see cref="PrimaryKeyAttribute"/>.</param>
        /// <param name="valueCollumn">SQL Column that will be matched to the Value.</param>
        /// <param name="fieldKeyCollumn">SQL Column that will be matched to the Field name.</param>
        public AdditionalFieldSearchAttribute(string condition, string additionalFieldsTable, string primaryKeyCollumn, string valueCollumn, string fieldKeyCollumn)
        {
            Condition = condition;
            AdditionalFieldTable = additionalFieldsTable;
            PrimaryKeyField = primaryKeyCollumn;
            ValueField = valueCollumn;
            FieldKeyField = fieldKeyCollumn;
        }

        /// <summary>
        /// Creates a new <see cref="AdditionalFieldSearchAttribute"/>.
        /// </summary>
        /// <param name="condition">Condition to be true for any row in the additionalFields Table to be used.</param>
        /// <remarks>
        /// This Assumes the following View is set up:
        /// CREATE VIEW AdditionalFieldSearch AS 
        /// SELECT
        ///    AdditionalFieldValue.ParentTableID,
        ///    AdditionalFieldValue.Value,
        ///    AdditionalField.ParentTable,
        ///    AdditionalField.FieldName,
        ///    FROM
        ///        AdditionalField LEFT JOIN
        ///        AdditionalFieldValue ON AdditionalFieldValue.AdditionalFieldID = AdditionalField.ID
        /// GO
        /// </remarks>
        public AdditionalFieldSearchAttribute(string condition): this(condition, "AdditionalFieldSearch", "ParentTableID", "Value", "FieldName")
        { }
    }
}