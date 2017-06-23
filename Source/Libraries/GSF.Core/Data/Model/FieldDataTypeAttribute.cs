//******************************************************************************************************
//  FieldDataTypeAttribute.cs - Gbtc
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
//  06/23/2017 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will request use of a specific data type on a modeled table field.
    /// Typically only needed to enforce use of a specific <see cref="DbType"/> for parameters.
    /// </summary>
    /// <remarks>
    /// Applying this attribute to a modeled table field with no database type parameter will specify that
    /// the field data type be used for all database types. Using a specific database type as a parameter
    /// to the attribute, e.g., [FieldDataType(DbType.DateTime2, DatabaseType.SQLServer)], means the field
    /// data type will only be applied to the specific database - however, the attribute allows multiple
    /// instances on the same identifier so you could specify different field data types for different
    /// database types. When multiple instances of the attribute are applied to the same identifier, the
    /// first attribute encountered that does not target a specific database will be considered the default
    /// value for all non-specified database types.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class FieldDataTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets field data type that determines which <see cref="DbType"/> should be used when using
        /// database command operations.
        /// </summary>
        public DbType FieldDataType
        {
            get;
        }

        /// <summary>
        /// Gets target <see cref="DatabaseType"/> for this <see cref="FieldDataTypeAttribute"/>.
        /// </summary>
        /// <remarks>
        /// When value is <c>null</c>, specified <see cref="FieldDataType"/> will be applied to all database types.
        /// </remarks>
        public DatabaseType? TargetDatabaseType
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="FieldDataTypeAttribute"/> that will request use of specific <see cref="DbType"/>.
        /// </summary>
        /// <param name="fieldDataType">Specific <see cref="DbType"/> to target for field.</param>
        public FieldDataTypeAttribute(DbType fieldDataType)
        {
            FieldDataType = fieldDataType;
            TargetDatabaseType = null;
        }

        /// <summary>
        /// Creates a new <see cref="FieldDataTypeAttribute"/> that will request use of specific <see cref="DbType"/>
        /// for the specified <see cref="DatabaseType"/>.
        /// </summary>
        /// <param name="fieldDataType">Specific <see cref="DbType"/> to target for field.</param>
        /// <param name="targetDatabaseType">Target <see cref="DatabaseType"/> for applying <paramref name="fieldDataType"/>.</param>
        public FieldDataTypeAttribute(DbType fieldDataType, DatabaseType targetDatabaseType)
        {
            FieldDataType = fieldDataType;
            TargetDatabaseType = targetDatabaseType;
        }
    }
}