//******************************************************************************************************
//  UseEscapedNameAttribute.cs - Gbtc
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
//  04/29/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines an attribute that will request use of the escaped name of a modeled identifier, i.e.,
    /// a table or field name. Typically needed when identifier names are reserved SQL key words.
    /// </summary>
    /// <remarks>
    /// Applying [UseEscapedName] to a modeled table or field with no parameters will specify that an
    /// escaped name be used for all database types. Using a specific database type as a parameter
    /// to the attribute, e.g., [UseEscapedName(DatabaseType.SQLServer)], means the escaped name will
    /// only be applied to the specific database - however, the attribute allows multiple instances
    /// on the same identifier so you could specify that escaping only be applied to two databases,
    /// for example: [UseEscapedName(DatabaseType.SQLServer), UseEscapedName(DatabaseType.MySQL)].
    /// In the event multiple attributes have been applied to a modeled identifier where one instance
    /// has specified no target database type and others have, the system will assume to target all
    /// databases for escaping the name and ignore the specific targets.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class UseEscapedNameAttribute : Attribute
    {
        /// <summary>
        /// Gets target <see cref="DatabaseType"/> for using escaped name.
        /// </summary>
        /// <remarks>
        /// When value is <c>null</c>, escaped name will be applied to all database types.
        /// </remarks>
        public DatabaseType? TargetDatabaseType
        {
            get;
        }

        /// <summary>
        /// Gets or sets flag that determines if double quotes should be used as the identifier delimiter,
        /// per SQL-99 standard, regardless of database type. Defaults to <c>true</c> except for MySQL.
        /// </summary>
        /// <remarks>
        /// If <c>true</c>, ANSI standard double-quotes will be used for escaping the identifier; otherwise,
        /// the escaping identifier will be based on the connected database type.
        /// </remarks>
        public bool UseAnsiQuotes
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new <see cref="UseEscapedNameAttribute"/> that will request use of escaped name for any database.
        /// </summary>
        public UseEscapedNameAttribute()
        {
            TargetDatabaseType = null;
            UseAnsiQuotes = true;
        }

        /// <summary>
        /// Creates a new <see cref="UseEscapedNameAttribute"/> that will request use of escaped name for the
        /// specified <see cref="DatabaseType"/>.
        /// </summary>
        /// <param name="targetDatabaseType">Target <see cref="DatabaseType"/> for escaping identifier.</param>
        public UseEscapedNameAttribute(DatabaseType targetDatabaseType)
        {
            TargetDatabaseType = targetDatabaseType;
            UseAnsiQuotes = targetDatabaseType != DatabaseType.MySQL;
        }
    }
}