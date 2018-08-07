//******************************************************************************************************
//  RecordOperationAttribute.cs - Gbtc
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
//  02/26/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Data.Model
{
    /// <summary>
    /// Record operations for modeled tables.
    /// </summary>
    public enum RecordOperation
    {
        /// <summary>
        /// Operation for getting record count.
        /// </summary>
        QueryRecordCount,
        /// <summary>
        /// Operation for retrieving paged records.
        /// </summary>
        QueryRecords,
        /// <summary>
        /// Operation for deleting records.
        /// </summary>
        /// <remarks>
        /// Delete rights should be derived from methods marked with this operation.
        /// </remarks>
        DeleteRecord,
        /// <summary>
        /// Operation for creating a new record, i.e., instantiating a new modeled table record instance.
        /// </summary>
        CreateNewRecord,
        /// <summary>
        /// Operation for adding records.
        /// </summary>
        /// <remarks>
        /// Add rights should be derived from methods marked with this operation.
        /// </remarks>
        AddNewRecord,
        /// <summary>
        /// Operation for updating records.
        /// </summary>
        /// <remarks>
        /// Update rights should be derived from methods marked with this operation.
        /// </remarks>
        UpdateRecord
    }

    /// <summary>
    /// Defines an attribute that marks a method as a specific <see cref="RecordOperation"/> for a modeled table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RecordOperationAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets <see cref="Type"/> of modeled table.
        /// </summary>
        public Type ModelType
        {
            get;
        }

        /// <summary>
        /// Gets or sets <see cref="RecordOperation"/> the method represents.
        /// </summary>
        public RecordOperation Operation
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="RecordOperationAttribute"/>
        /// </summary>
        /// <param name="modelType">The <see cref="Type"/> of modeled table.</param>
        /// <param name="operation">The <see cref="RecordOperation"/> the method represents.</param>
        public RecordOperationAttribute(Type modelType, RecordOperation operation)
        {
            ModelType = modelType;
            Operation = operation;
        }
    }
}