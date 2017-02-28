//******************************************************************************************************
//  AmendExpressionAttribute.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  02/24/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Data.Model
{
    #region [ Enumerations ]

    /// <summary>
    /// Statement types for amendment application.
    /// </summary>
    [Flags]
    public enum StatementTypes
    {
        /// <summary>
        /// Apply amendment to all SELECT statements, i.e., <see cref="SelectCount"/>, <see cref="SelectSet"/> and <see cref="SelectRow"/>.
        /// </summary>
        Select = SelectCount | SelectSet | SelectRow,
        /// <summary>
        /// Apply amendment to SELECT COUNT statements.
        /// </summary>
        SelectCount = (int)Bits.Bit00,
        /// <summary>
        /// Apply amendment to SELECT statements that can return multiple rows of data.
        /// </summary>
        SelectSet = (int)Bits.Bit01,
        /// <summary>
        /// Apply amendment to SELECT statements that will return a single row of data.
        /// </summary>
        SelectRow = (int)Bits.Bit02,
        /// <summary>
        /// Apply amendment to INSERT statements.
        /// </summary>
        Insert = (int)Bits.Bit03,
        /// <summary>
        /// Apply amendment to UPDATE statements.
        /// </summary>
        Update = (int)Bits.Bit04,
        /// <summary>
        /// Apply amendment to DELETE statements.
        /// </summary>
        Delete = (int)Bits.Bit05
    }

    /// <summary>
    /// Target expressions for amendment application.
    /// </summary>
    public enum TargetExpression
    {
        /// <summary>
        /// Apply amendment to table name.
        /// </summary>
        TableName,
        /// <summary>
        /// Apply amendment to full field list.
        /// </summary>
        FieldList
    }

    /// <summary>
    /// Locations for target expression amendment application.
    /// </summary>
    public enum AffixPosition
    {
        /// <summary>
        /// Apply amendment to beginning of <see cref="TargetExpression"/>.
        /// </summary>
        Prefix,
        /// <summary>
        /// Apply amendment to ending of <see cref="TargetExpression"/>.
        /// </summary>
        Suffix
    }

    #endregion

    /// <summary>
    /// Defines an attribute that will request amendment of a table name or field list with the specified text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// As an example, this can be used to add table hints, e.g., <c>WITH (NOLOCK)</c> for select statements in
    /// SQL Server.
    /// </para>
    /// <para>
    /// Applying the <see cref="AmendExpressionAttribute"/> to a modeled table with no specified database type parameters
    /// will be meant to infer that the amendment be used for all database types. Using a specific database type as a
    /// parameter to the attribute, e.g., <c>[AmendExpression("WITH (NOLOCK)", DatabaseType.SQLServer)]</c>, means the
    /// amendment text will only be applied to the specific database - however, the attribute allows multiple instances
    /// on the same identifier so you could specify that amendment application only be applied to two databases:
    /// <code>
    /// [AmendExpression("AS T1", DatabaseType.SQLServer), AmendExpression("AS T1", DatabaseType.MySQL)]
    /// </code>
    /// Other parameters exist to fully customize target expression, set affix position and specify statement types:
    /// <code>
    /// [AmendExpression("TOP 200",
    ///     TargetExpression = TargetExpression.FieldList,
    ///     AffixPosition = AffixPosition.Prefix,
    ///     StatementTypes = StatementTypes.SelectSet)]
    /// </code>
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]    
    public sealed class AmendExpressionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets amendment text to be applied.
        /// </summary>
        public string AmendmentText
        {
            get;
        }

        /// <summary>
        /// Gets or sets target expression for amendment application; defaults to <see cref="Model.TargetExpression.TableName"/>.
        /// </summary>
        public TargetExpression TargetExpression
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets location for amendment application; defaults to <see cref="Model.AffixPosition.Suffix"/>.
        /// </summary>
        public AffixPosition AffixPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets statement types for amendment application; defaults to <see cref="Model.StatementTypes.Select"/>.
        /// </summary>
        public StatementTypes StatementTypes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets target <see cref="DatabaseType"/> for amendment application.
        /// </summary>
        /// <remarks>
        /// When value is <c>null</c>, amendment text will be applied to all database types.
        /// </remarks>
        public DatabaseType? TargetDatabaseType
        {
            get;
        }

        /// <summary>
        /// Creates a new <see cref="AmendExpressionAttribute"/> that will request application of amendment text for any database.
        /// </summary>
        /// <param name="amendmentText">Amendment text to apply.</param>
        public AmendExpressionAttribute(string amendmentText)
        {
            AmendmentText = amendmentText;
            TargetDatabaseType = null;
            TargetExpression = TargetExpression.TableName;
            AffixPosition = AffixPosition.Suffix;
            StatementTypes = StatementTypes.Select;
        }

        /// <summary>
        /// Creates a new <see cref="AmendExpressionAttribute"/> that will request application of amendment text for the
        /// specified <see cref="DatabaseType"/>.
        /// </summary>
        /// <param name="amendmentText">Amendment text to apply.</param>
        /// <param name="targetDatabaseType">Target <see cref="DatabaseType"/> for amendment text.</param>
        public AmendExpressionAttribute(string amendmentText, DatabaseType targetDatabaseType)
        {
            AmendmentText = amendmentText;
            TargetDatabaseType = targetDatabaseType;
            TargetExpression = TargetExpression.TableName;
            AffixPosition = AffixPosition.Suffix;
            StatementTypes = StatementTypes.Select;
        }
    }
}