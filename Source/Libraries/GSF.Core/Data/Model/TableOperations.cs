//******************************************************************************************************
//  TableOperations.cs - Gbtc
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
//  02/01/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ExpressionEvaluator;
using GSF.Collections;
using GSF.ComponentModel;
using GSF.Reflection;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable UnusedMember.Local
namespace GSF.Data.Model
{
    /// <summary>
    /// Defines database operations for a modeled table.
    /// </summary>
    /// <typeparam name="T">Modeled table.</typeparam>
    public class TableOperations<T> : ITableOperations where T : class, new()
    {
        #region [ Members ]

        // Nested Types
        private class CurrentScope : ValueExpressionScopeBase<T>
        {
            // Define instance variables exposed to ValueExpressionAttributeBase expressions
            #pragma warning disable 169, 414, 649
            public TableOperations<T> TableOperations;
            public AdoDataConnection Connection;
            #pragma warning restore 169, 414, 649
        }

        // Constants
        private const string SelectCountSqlFormat = "SELECT COUNT(*) FROM {0}";
        private const string SelectSetSqlFormat = "SELECT {0} FROM {1} ORDER BY {{0}}";
        private const string SelectSetWhereSqlFormat = "SELECT {0} FROM {1} WHERE {{0}} ORDER BY {{1}}";
        private const string SelectRowSqlFormat = "SELECT * FROM {0} WHERE {1}";
        private const string AddNewSqlFormat = "INSERT INTO {0}({1}) VALUES ({2})";
        private const string UpdateSqlFormat = "UPDATE {0} SET {1} WHERE {2}";
        private const string DeleteSqlFormat = "DELETE FROM {0} WHERE {1}";
        private const string TableNamePrefixToken = "<!TNP/>";
        private const string TableNameSuffixToken = "<!TNS/>";
        private const string FieldListPrefixToken = "<!FLP/>";
        private const string FieldListSuffixToken = "<!FLS/>";

        // Fields
        private readonly AdoDataConnection m_connection;
        private Action<Exception> m_exceptionHandler;
        private DataTable m_primaryKeyCache;
        private string m_lastSortField;
        private RecordRestriction m_lastRestriction;
        private RecordRestriction m_rootQueryRestriction;
        private bool m_useCaseSensitiveFieldNames;
        private readonly string m_selectCountSql;
        private readonly string m_selectSetSql;
        private readonly string m_selectSetWhereSql;
        private readonly string m_selectKeysSql;
        private readonly string m_selectKeysWhereSql;
        private readonly string m_selectRowSql;
        private readonly string m_addNewSql;
        private readonly string m_updateSql;
        private readonly string m_updateWhereSql;
        private readonly string m_deleteSql;
        private readonly string m_deleteWhereSql;
        private readonly string m_searchFilterSql;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TableOperations{T}"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> instance to use for database operations.</param>
        /// <param name="customTokens">Custom run-time tokens to apply to any modeled <see cref="AmendExpressionAttribute"/> values.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> cannot be <c>null</c>.</exception>
        /// <remarks>
        /// The <paramref name="customTokens"/> can be used to apply run-time tokens to any defined <see cref="AmendExpressionAttribute"/> values,
        /// for example, given the following amendment expression applied to a modeled class:
        /// <code>
        /// [AmendExpression("TOP {count}", 
        ///     TargetExpression = TargetExpression.FieldList,
        ///     AffixPosition = AffixPosition.Prefix,
        ///     StatementTypes = StatementTypes.SelectSet)]]
        /// </code>
        /// The <paramref name="customTokens"/> key/value pairs could be set as follows at run-time:
        /// <code>
        /// int count = 200;
        /// customTokens = new[] { new KeyValuePair&lt;string, string&gt;("{count}", $"{count}") };
        /// </code>
        /// </remarks>
        public TableOperations(AdoDataConnection connection, IEnumerable<KeyValuePair<string, string>> customTokens = null)
        {
            if ((object)connection == null)
                throw new ArgumentNullException(nameof(connection));

            m_connection = connection;
            m_selectCountSql = s_selectCountSql;
            m_selectSetSql = s_selectSetSql;
            m_selectSetWhereSql = s_selectSetWhereSql;
            m_selectKeysSql = s_selectKeysSql;
            m_selectKeysWhereSql = s_selectKeysWhereSql;
            m_selectRowSql = s_selectRowSql;
            m_addNewSql = s_addNewSql;
            m_updateSql = s_updateSql;
            m_updateWhereSql = s_updateWhereSql;
            m_deleteSql = s_deleteSql;
            m_deleteWhereSql = s_deleteWhereSql;
            m_searchFilterSql = s_searchFilterSql;
            m_rootQueryRestriction = s_rootQueryRestriction?.Clone();

            // When any escape targets are defined for the modeled identifiers, i.e., table or field names,
            // the static SQL statements are defined with ANSI standard escape delimiters. We check if the
            // user model has opted to instead use common database escape delimiters, or no delimiters, that
            // will apply to the active database type and make any needed adjustments. As a result, it will
            // be slightly faster to construct this class when ANSI standard escape delimiters are used.
            if ((object)s_escapedTableNameTargets != null)
            {
                string derivedTableName = GetEscapedTableName();
                string ansiEscapedTableName = $"\"{s_tableName}\"";

                if (!derivedTableName.Equals(ansiEscapedTableName))
                {
                    m_selectCountSql = m_selectCountSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_selectSetSql = m_selectSetSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_selectSetWhereSql = m_selectSetWhereSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_selectKeysSql = m_selectKeysSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_selectKeysWhereSql = m_selectKeysWhereSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_selectRowSql = m_selectRowSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_addNewSql = m_addNewSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_updateSql = m_updateSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_updateWhereSql = m_updateWhereSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_deleteSql = m_deleteSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_deleteWhereSql = m_deleteWhereSql.Replace(ansiEscapedTableName, derivedTableName);
                }
            }

            if ((object)s_escapedFieldNameTargets != null)
            {
                foreach (KeyValuePair<string, Dictionary<DatabaseType, bool>> escapedFieldNameTarget in s_escapedFieldNameTargets)
                {
                    string fieldName = escapedFieldNameTarget.Key;
                    string derivedFieldName = GetEscapedFieldName(fieldName, escapedFieldNameTarget.Value);
                    string ansiEscapedFieldName = $"\"{fieldName}\"";

                    if (!derivedFieldName.Equals(ansiEscapedFieldName))
                    {
                        m_selectKeysSql = m_selectKeysSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_selectKeysWhereSql = m_selectKeysWhereSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_selectRowSql = m_selectRowSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_addNewSql = m_addNewSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_updateSql = m_updateSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_updateWhereSql = m_updateWhereSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_deleteSql = m_deleteSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_deleteWhereSql = m_deleteWhereSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_searchFilterSql = m_searchFilterSql.Replace(ansiEscapedFieldName, derivedFieldName);
                    }
                }
            }

            // Handle any modeled expression amendments
            if ((object)s_expressionAmendments != null)
            {
                foreach (Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string> expressionAmendment in s_expressionAmendments)
                {
                    // See if expression amendment applies to current database type
                    if (expressionAmendment.Item1 != m_connection.DatabaseType)
                        continue;

                    // Get expression amendment properties
                    TargetExpression targetExpression = expressionAmendment.Item2;
                    StatementTypes statementTypes = expressionAmendment.Item3;
                    AffixPosition affixPosition = expressionAmendment.Item4;
                    string amendmentText = expressionAmendment.Item5;
                    string tableNameToken = affixPosition == AffixPosition.Prefix ? TableNamePrefixToken : TableNameSuffixToken;
                    string fieldListToken = affixPosition == AffixPosition.Prefix ? FieldListPrefixToken : FieldListSuffixToken;
                    string targetToken = targetExpression == TargetExpression.TableName ? tableNameToken : fieldListToken;

                    // Apply amendment to target statement types
                    if (statementTypes.HasFlag(StatementTypes.SelectCount) && targetExpression == TargetExpression.TableName)
                        m_selectCountSql = m_selectCountSql.Replace(targetToken, amendmentText);

                    if (statementTypes.HasFlag(StatementTypes.SelectSet))
                    {
                        m_selectSetSql = m_selectSetSql.Replace(targetToken, amendmentText);
                        m_selectSetWhereSql = m_selectSetWhereSql.Replace(targetToken, amendmentText);
                        m_selectKeysSql = m_selectKeysSql.Replace(targetToken, amendmentText);
                        m_selectKeysWhereSql = m_selectKeysWhereSql.Replace(targetToken, amendmentText);
                    }

                    if (statementTypes.HasFlag(StatementTypes.SelectRow))
                        m_selectRowSql = m_selectRowSql.Replace(targetToken, amendmentText);

                    if (statementTypes.HasFlag(StatementTypes.Insert))
                        m_addNewSql = m_addNewSql.Replace(targetToken, amendmentText);

                    if (statementTypes.HasFlag(StatementTypes.Update))
                    {
                        m_updateSql = m_updateSql.Replace(targetToken, amendmentText);
                        m_updateWhereSql = m_updateWhereSql.Replace(targetToken, amendmentText);
                    }

                    if (statementTypes.HasFlag(StatementTypes.Delete))
                    {
                        m_deleteSql = m_deleteSql.Replace(targetToken, amendmentText);
                        m_deleteWhereSql = m_deleteWhereSql.Replace(targetToken, amendmentText);
                    }
                }

                // Remove any remaining tokens from instance expressions
                Func<string, string> removeRemainingTokens = sql => sql
                    .Replace(TableNamePrefixToken, "")
                    .Replace(TableNameSuffixToken, "")
                    .Replace(FieldListPrefixToken, "")
                    .Replace(FieldListSuffixToken, "");

                m_selectCountSql = removeRemainingTokens(m_selectCountSql);
                m_selectSetSql = removeRemainingTokens(m_selectSetSql);
                m_selectSetWhereSql = removeRemainingTokens(m_selectSetWhereSql);
                m_selectKeysSql = removeRemainingTokens(m_selectKeysSql);
                m_selectKeysWhereSql = removeRemainingTokens(m_selectKeysWhereSql);
                m_selectRowSql = removeRemainingTokens(m_selectRowSql);
                m_addNewSql = removeRemainingTokens(m_addNewSql);
                m_updateSql = removeRemainingTokens(m_updateSql);
                m_updateWhereSql = removeRemainingTokens(m_updateWhereSql);
                m_deleteSql = removeRemainingTokens(m_deleteSql);
                m_deleteWhereSql = removeRemainingTokens(m_deleteWhereSql);

                // Execute replacements on any provided custom run-time tokens
                if ((object)customTokens != null)
                {
                    foreach (KeyValuePair<string, string> customToken in customTokens)
                    {
                        m_selectCountSql = m_selectCountSql.Replace(customToken.Key, customToken.Value);
                        m_selectSetSql = m_selectSetSql.Replace(customToken.Key, customToken.Value);
                        m_selectSetWhereSql = m_selectSetWhereSql.Replace(customToken.Key, customToken.Value);
                        m_selectKeysSql = m_selectKeysSql.Replace(customToken.Key, customToken.Value);
                        m_selectKeysWhereSql = m_selectKeysWhereSql.Replace(customToken.Key, customToken.Value);
                        m_selectRowSql = m_selectRowSql.Replace(customToken.Key, customToken.Value);
                        m_addNewSql = m_addNewSql.Replace(customToken.Key, customToken.Value);
                        m_updateSql = m_updateSql.Replace(customToken.Key, customToken.Value);
                        m_updateWhereSql = m_updateWhereSql.Replace(customToken.Key, customToken.Value);
                        m_deleteSql = m_deleteSql.Replace(customToken.Key, customToken.Value);
                        m_deleteWhereSql = m_deleteWhereSql.Replace(customToken.Key, customToken.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="TableOperations{T}"/> using provided <paramref name="exceptionHandler"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> instance to use for database operations.</param>
        /// <param name="exceptionHandler">Delegate to handle table operation exceptions.</param>
        /// <param name="customTokens">Custom run-time tokens to apply to any modeled <see cref="AmendExpressionAttribute"/> values.</param>
        /// <remarks>
        /// <para>
        /// When exception handler is provided, table operations will not throw exceptions for database calls, any
        /// encountered exceptions will be passed to handler for processing.
        /// </para>
        /// <para>
        /// The <paramref name="customTokens"/> can be used to apply run-time tokens to any defined <see cref="AmendExpressionAttribute"/> values,
        /// for example, given the following amendment expression applied to a modeled class:
        /// <code>
        /// [AmendExpression("TOP {count}", 
        ///     TargetExpression = TargetExpression.FieldList,
        ///     AffixPosition = AffixPosition.Prefix,
        ///     StatementTypes = StatementTypes.SelectSet)]]
        /// </code>
        /// The <paramref name="customTokens"/> key/value pairs could be set as follows at run-time:
        /// <code>
        /// int count = 200;
        /// customTokens = new[] { new KeyValuePair&lt;string, string&gt;("{count}", $"{count}") };
        /// </code>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> cannot be <c>null</c>.</exception>
        public TableOperations(AdoDataConnection connection, Action<Exception> exceptionHandler, IEnumerable<KeyValuePair<string, string>> customTokens = null) : this(connection, customTokens)
        {
            m_exceptionHandler = exceptionHandler;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="AdoDataConnection"/> instance associated with this <see cref="TableOperations{T}"/> used for database operations.
        /// </summary>
        public AdoDataConnection Connection => m_connection;

        /// <summary>
        /// Gets the table name defined for the modeled table, includes any escaping as defined in model.
        /// </summary>
        public string TableName => GetEscapedTableName();

        /// <summary>
        /// Gets the table name defined for the modeled table without any escape characters.
        /// </summary>
        /// <remarks>
        /// A table name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        public string UnescapedTableName => s_tableName;

        /// <summary>
        /// Gets flag that determines if modeled table has a primary key that is an identity field.
        /// </summary>
        public bool HasPrimaryKeyIdentityField => s_hasPrimaryKeyIdentityField;

        /// <summary>
        /// Gets or sets delegate used to handle table operation exceptions.
        /// </summary>
        /// <remarks>
        /// When exception handler is provided, table operations will not throw exceptions for database calls, any
        /// encountered exceptions will be passed to handler for processing. Otherwise, exceptions will be thrown
        /// on the call stack.
        /// </remarks>
        public Action<Exception> ExceptionHandler
        {
            get
            {
                return m_exceptionHandler;
            }
            set
            {
                m_exceptionHandler = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if field names should be treated as case sensitive. Defaults to <c>false</c>.
        /// </summary>
        /// <remarks>
        /// In cases where modeled table fields have applied <see cref="UseEscapedNameAttribute"/>, this flag will be used
        /// to properly update escaped field names that may be case sensitive.
        /// </remarks>
        public bool UseCaseSensitiveFieldNames
        {
            get
            {
                return m_useCaseSensitiveFieldNames;
            }
            set
            {
                m_useCaseSensitiveFieldNames = value;
            }
        }

        /// <summary>
        /// Gets or sets primary key cache.
        /// </summary>
        public DataTable PrimaryKeyCache
        {
            get
            {
                return m_primaryKeyCache;
            }
            set
            {
                m_primaryKeyCache = value;
            }
        }

        /// <summary>
        /// Gets or sets root record restriction that applies to query table operations.
        /// </summary>
        public RecordRestriction RootQueryRestriction
        {
            get
            {
                return m_rootQueryRestriction;
            }
            set
            {
                m_rootQueryRestriction = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a new modeled record instance, applying any modeled default values as specified by a
        /// <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/> on the
        /// model properties.
        /// </summary>
        /// <returns>New modeled record instance with any defined default values applied.</returns>
        public T NewRecord()
        {
            try
            {
                return s_createRecordInstance(new CurrentScope
                {
                    TableOperations = this,
                    Connection = m_connection
                });
            }
            catch (Exception ex)
            {
                if ((object)m_exceptionHandler == null)
                    throw;

                m_exceptionHandler(ex);
                return null;
            }
        }

        object ITableOperations.NewRecord()
        {
            return NewRecord();
        }

        /// <summary>
        /// Applies the default values on the specified modeled table <paramref name="record"/>
        /// where any of the properties are marked with either <see cref="DefaultValueAttribute"/>
        /// or <see cref="DefaultValueExpressionAttribute"/>.
        /// </summary>
        /// <param name="record">Record to update.</param>
        public void ApplyRecordDefaults(T record)
        {
            try
            {
                s_applyRecordDefaults(new CurrentScope
                {
                    Instance = record,
                    TableOperations = this,
                    Connection = m_connection
                });
            }
            catch (Exception ex)
            {
                if ((object)m_exceptionHandler == null)
                    throw;

                m_exceptionHandler(ex);
            }
        }

        void ITableOperations.ApplyRecordDefaults(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot apply defaults for record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            ApplyRecordDefaults(record);
        }

        /// <summary>
        /// Queries database and returns a single modeled table record for the specified <paramref name="restriction"/>.
        /// </summary>
        /// <param name="restriction">Record restriction to apply.</param>
        /// <returns>A single modeled table record for the queried record.</returns>
        /// <remarks>
        /// <para>
        /// If no record is found for specified <paramref name="restriction"/>, <c>null</c> will be returned.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecords(string, RecordRestriction, int)"/>
        /// specifying the <see cref="RecordRestriction"/> parameter with a limit of 1 record.
        /// </para>
        /// </remarks>
        public T QueryRecord(RecordRestriction restriction)
        {
            return QueryRecord(null, restriction);
        }

        object ITableOperations.QueryRecord(RecordRestriction restriction)
        {
            return QueryRecord(restriction);
        }

        /// <summary>
        /// Queries database and returns a single modeled table record for the specified <paramref name="restriction"/>,
        /// execution of query will apply <paramref name="orderByExpression"/>.
        /// </summary>
        /// <param name="orderByExpression">Field name expression used for sort order, include ASC or DESC as needed - does not include ORDER BY; defaults to primary keys.</param>
        /// <param name="restriction">Record restriction to apply.</param>
        /// <returns>A single modeled table record for the queried record.</returns>
        /// <remarks>
        /// <para>
        /// If no record is found for specified <paramref name="restriction"/>, <c>null</c> will be returned.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecords(string, RecordRestriction, int)"/>
        /// specifying the <see cref="RecordRestriction"/> parameter with a limit of 1 record.
        /// </para>
        /// </remarks>
        public T QueryRecord(string orderByExpression, RecordRestriction restriction)
        {
            return QueryRecords(orderByExpression, restriction, 1).FirstOrDefault();
        }

        object ITableOperations.QueryRecord(string orderByExpression, RecordRestriction restriction)
        {
            return QueryRecord(orderByExpression, restriction);
        }

        /// <summary>
        /// Queries database and returns a single modeled table record for the specified SQL filter
        /// expression and parameters.
        /// </summary>
        /// <param name="filterExpression">
        /// Filter SQL expression for restriction as a composite format string - does not include WHERE.
        /// When escaping is needed for field names, use standard ANSI quotes.
        /// </param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <returns>A single modeled table record for the queried record.</returns>
        /// <remarks>
        /// <para>
        /// If no record is found for specified filter expression and parameters, <c>null</c> will be returned.
        /// </para>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format <paramref name="filterExpression"/>
        /// will be converted into query parameters where each of the corresponding values in the
        /// <paramref name="parameters"/> collection will be applied as <see cref="IDbDataParameter"/>
        /// values to an executed <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecords(string, RecordRestriction, int)"/>
        /// specifying the <see cref="RecordRestriction"/> parameter with a limit of 1 record.
        /// </para>
        /// </remarks>
        public T QueryRecordWhere(string filterExpression, params object[] parameters)
        {
            return QueryRecord(new RecordRestriction(filterExpression, parameters));
        }

        object ITableOperations.QueryRecordWhere(string filterExpression, params object[] parameters)
        {
            return QueryRecordWhere(filterExpression, parameters);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified parameters.
        /// </summary>
        /// <param name="orderByExpression">Field name expression used for sort order, include ASC or DESC as needed - does not include ORDER BY; defaults to primary keys.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <param name="limit">Limit of number of record to return.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// If no record <paramref name="restriction"/> or <paramref name="limit"/> is provided, all rows will be returned.
        /// </remarks>
        public IEnumerable<T> QueryRecords(string orderByExpression = null, RecordRestriction restriction = null, int limit = -1)
        {
            if (string.IsNullOrWhiteSpace(orderByExpression))
                orderByExpression = s_primaryKeyFields;

            string sqlExpression = null;

            try
            {
                if ((object)m_rootQueryRestriction != null)
                    restriction = m_rootQueryRestriction + restriction;

                if (limit < 1)
                {
                    // No record limit specified
                    if ((object)restriction == null)
                    {
                        sqlExpression = string.Format(m_selectSetSql, orderByExpression);
                        return m_connection.RetrieveData(sqlExpression).AsEnumerable().Select(LoadRecord);
                    }

                    sqlExpression = string.Format(m_selectSetWhereSql, UpdateFieldNames(restriction.FilterExpression), orderByExpression);
                    return m_connection.RetrieveData(sqlExpression, restriction.Parameters).AsEnumerable().Select(LoadRecord);
                }

                if ((object)restriction == null)
                {
                    sqlExpression = string.Format(m_selectSetSql, orderByExpression);
                    return m_connection.RetrieveData(sqlExpression).AsEnumerable().Take(limit).Select(LoadRecord);
                }

                sqlExpression = string.Format(m_selectSetWhereSql, UpdateFieldNames(restriction.FilterExpression), orderByExpression);
                return m_connection.RetrieveData(sqlExpression, restriction.Parameters).AsEnumerable().Take(limit).Select(LoadRecord);
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record query for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction?.Parameters)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return Enumerable.Empty<T>();
            }
        }

        IEnumerable ITableOperations.QueryRecords(string orderByExpression, RecordRestriction restriction, int limit)
        {
            return QueryRecords(orderByExpression, restriction, limit);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified <paramref name="restriction"/>.
        /// </summary>
        /// <param name="restriction">Record restriction to apply.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecords(string, RecordRestriction, int)"/> only
        /// specifying the <see cref="RecordRestriction"/> parameter.
        /// </para>
        /// </remarks>
        public IEnumerable<T> QueryRecords(RecordRestriction restriction)
        {
            return QueryRecords(null, restriction);
        }

        IEnumerable ITableOperations.QueryRecords(RecordRestriction restriction)
        {
            return QueryRecords(restriction);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified SQL filter expression
        /// and parameters.
        /// </summary>
        /// <param name="filterExpression">
        /// Filter SQL expression for restriction as a composite format string - does not include WHERE.
        /// When escaping is needed for field names, use standard ANSI quotes.
        /// </param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format <paramref name="filterExpression"/>
        /// will be converted into query parameters where each of the corresponding values in the
        /// <paramref name="parameters"/> collection will be applied as <see cref="IDbDataParameter"/>
        /// values to an executed <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecords(string, RecordRestriction, int)"/> only
        /// specifying the <see cref="RecordRestriction"/> parameter.
        /// </para>
        /// </remarks>
        public IEnumerable<T> QueryRecordsWhere(string filterExpression, params object[] parameters)
        {
            return QueryRecords(new RecordRestriction(filterExpression, parameters));
        }

        IEnumerable ITableOperations.QueryRecordsWhere(string filterExpression, params object[] parameters)
        {
            return QueryRecordsWhere(filterExpression, parameters);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified sorting, paging and search parameters.
        /// </summary>
        /// <param name="sortField">Field name to order-by.</param>
        /// <param name="ascending">Sort ascending flag; set to <c>false</c> for descending.</param>
        /// <param name="page">Page number of records to return (1-based).</param>
        /// <param name="pageSize">Current page size.</param>
        /// <param name="searchText">Text to search.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// <para>
        /// This function is used for record paging. Primary keys are cached server-side, typically per user session,
        /// to maintain desired per-page sort order. Call <see cref="ClearPrimaryKeyCache"/> to manually clear cache
        /// when table contents are known to have changed.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecords(string, bool, int, int, RecordRestriction)"/> where restriction
        /// is generated by <see cref="GetSearchRestriction(string)"/> using <paramref name="searchText"/>.
        /// </para>
        /// </remarks>
        public IEnumerable<T> QueryRecords(string sortField, bool ascending, int page, int pageSize, string searchText)
        {
            return QueryRecords(sortField, ascending, page, pageSize, GetSearchRestriction(searchText));
        }

        IEnumerable ITableOperations.QueryRecords(string sortField, bool ascending, int page, int pageSize, string searchText)
        {
            return QueryRecords(sortField, ascending, page, pageSize, searchText);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified sorting and paging parameters.
        /// </summary>
        /// <param name="sortField">Field name to order-by.</param>
        /// <param name="ascending">Sort ascending flag; set to <c>false</c> for descending.</param>
        /// <param name="page">Page number of records to return (1-based).</param>
        /// <param name="pageSize">Current page size.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// <para>
        /// This function is used for record paging. Primary keys are cached server-side, typically per user session,
        /// to maintain desired per-page sort order. Call <see cref="ClearPrimaryKeyCache"/> to manually clear cache
        /// when table contents are known to have changed.
        /// </para>
        /// </remarks>
        public IEnumerable<T> QueryRecords(string sortField, bool ascending, int page, int pageSize, RecordRestriction restriction = null)
        {
            if (string.IsNullOrWhiteSpace(sortField))
                sortField = s_fieldNames[s_primaryKeyProperties[0].Name];

            if ((object)m_primaryKeyCache == null || !sortField.Equals(m_lastSortField, StringComparison.OrdinalIgnoreCase) || restriction != m_lastRestriction)
            {
                string orderByExpression = $"{sortField}{(ascending ? "" : " DESC")}";
                string sqlExpression = null;

                try
                {
                    if ((object)m_rootQueryRestriction != null)
                        restriction = m_rootQueryRestriction + restriction;

                    if ((object)restriction == null)
                    {
                        sqlExpression = string.Format(m_selectKeysSql, orderByExpression);
                        m_primaryKeyCache = m_connection.RetrieveData(sqlExpression);
                    }
                    else
                    {
                        sqlExpression = string.Format(m_selectKeysWhereSql, UpdateFieldNames(restriction.FilterExpression), orderByExpression);
                        m_primaryKeyCache = m_connection.RetrieveData(sqlExpression, restriction.Parameters);
                    }
                }
                catch (Exception ex)
                {
                    InvalidOperationException opex = new InvalidOperationException($"Exception during record query for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction?.Parameters)}\": {ex.Message}", ex);

                    if ((object)m_exceptionHandler == null)
                        throw opex;

                    m_exceptionHandler(opex);
                    return Enumerable.Empty<T>();
                }

                m_lastSortField = sortField;
                m_lastRestriction = restriction;
            }

            return m_primaryKeyCache.AsEnumerable().ToPagedList(page, pageSize).Select(row => LoadRecord(row.ItemArray)).Where(record => record != null);
        }

        IEnumerable ITableOperations.QueryRecords(string sortField, bool ascending, int page, int pageSize, RecordRestriction restriction)
        {
            return QueryRecords(sortField, ascending, page, pageSize, restriction);
        }

        /// <summary>
        /// Gets the record count for the modeled table based on search parameter.
        /// </summary>
        /// <param name="searchText">Text to search.</param>
        /// <returns>Record count for the modeled table based on search parameter.</returns>
        /// <remarks>
        /// This is a convenience call to <see cref="QueryRecordCount(RecordRestriction)"/> where restriction
        /// is generated by <see cref="GetSearchRestriction(string)"/>
        /// </remarks>
        public int QueryRecordCount(string searchText)
        {
            return QueryRecordCount(GetSearchRestriction(searchText));
        }

        /// <summary>
        /// Gets the record count for the specified <paramref name="restriction"/> - or - total record
        /// count for the modeled table if <paramref name="restriction"/> is <c>null</c>.
        /// </summary>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>
        /// Record count for the specified <paramref name="restriction"/> - or - total record count
        /// for the modeled table if <paramref name="restriction"/> is <c>null</c>.
        /// </returns>
        public int QueryRecordCount(RecordRestriction restriction = null)
        {
            string sqlExpression = null;

            try
            {
                if ((object)m_rootQueryRestriction != null)
                    restriction = m_rootQueryRestriction + restriction;

                if ((object)restriction == null)
                {
                    sqlExpression = m_selectCountSql;
                    return m_connection.ExecuteScalar<int>(sqlExpression);
                }

                sqlExpression = $"{m_selectCountSql} WHERE {UpdateFieldNames(restriction.FilterExpression)}";
                return m_connection.ExecuteScalar<int>(sqlExpression, restriction.Parameters);
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record count query for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction?.Parameters)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return -1;
            }
        }

        /// <summary>
        /// Gets the record count for the modeled table for the specified SQL filter expression and parameters.
        /// </summary>
        /// <param name="filterExpression">
        /// Filter SQL expression for restriction as a composite format string - does not include WHERE.
        /// When escaping is needed for field names, use standard ANSI quotes.
        /// </param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <returns>Record count for the modeled table for the specified parameters.</returns>
        /// <remarks>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format <paramref name="filterExpression"/>
        /// will be converted into query parameters where each of the corresponding values in the
        /// <paramref name="parameters"/> collection will be applied as <see cref="IDbDataParameter"/>
        /// values to an executed <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecordCount(RecordRestriction)"/>.
        /// </para>
        /// </remarks>
        public int QueryRecordCountWhere(string filterExpression, params object[] parameters)
        {
            return QueryRecordCount(new RecordRestriction(filterExpression, parameters));
        }

        /// <summary>
        /// Creates a new modeled table record queried from the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="primaryKeys"/>.</returns>
        public T LoadRecord(params object[] primaryKeys)
        {
            try
            {
                return LoadRecord(m_connection.RetrieveRow(m_selectRowSql, primaryKeys));
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record load for {typeof(T).Name} \"{m_selectRowSql}, {ValueList(primaryKeys)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return null;
            }
        }

        object ITableOperations.LoadRecord(params object[] primaryKeys)
        {
            return LoadRecord(primaryKeys);
        }

        /// <summary>
        /// Creates a new modeled table record queried from the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be loaded.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="row"/>.</returns>
        public T LoadRecord(DataRow row)
        {
            try
            {
                T record = new T();

                // Make sure record exists, if not return null instead of a blank record
                if (s_hasPrimaryKeyIdentityField && GetPrimaryKeys(row).All(Common.IsDefaultValue))
                    return null;

                foreach (PropertyInfo property in s_properties.Values)
                {
                    try
                    {
                        property.SetValue(record, row.ConvertField(s_fieldNames[property.Name], property.PropertyType), null);
                    }
                    catch (Exception ex)
                    {
                        InvalidOperationException opex = new InvalidOperationException($"Exception during record load field assignment for \"{typeof(T).Name}.{property.Name} = {row[s_fieldNames[property.Name]]}\": {ex.Message}", ex);

                        if ((object)m_exceptionHandler == null)
                            throw opex;

                        m_exceptionHandler(opex);
                    }
                }

                return record;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record load for {typeof(T).Name} from data row: {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return null;
            }
        }

        object ITableOperations.LoadRecord(DataRow row)
        {
            return LoadRecord(row);
        }

        /// <summary>
        /// Deletes the record referenced by the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>Number of rows affected.</returns>
        public int DeleteRecord(params object[] primaryKeys)
        {
            try
            {
                int affectedRecords = m_connection.ExecuteNonQuery(m_deleteSql, primaryKeys);

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record delete for {typeof(T).Name} \"{m_deleteSql}, {ValueList(primaryKeys)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        /// <summary>
        /// Deletes the specified modeled table <paramref name="record"/> from the database.
        /// </summary>
        /// <param name="record">Record to delete.</param>
        /// <returns>Number of rows affected.</returns>
        public int DeleteRecord(T record)
        {
            return DeleteRecord(GetPrimaryKeys(record));
        }

        int ITableOperations.DeleteRecord(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot delete record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return DeleteRecord(record);
        }

        /// <summary>
        /// Deletes the record referenced by the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be deleted.</param>
        /// <returns>Number of rows affected.</returns>
        public int DeleteRecord(DataRow row)
        {
            return DeleteRecord(GetPrimaryKeys(row));
        }

        /// <summary>
        /// Deletes the records referenced by the specified <paramref name="restriction"/>.
        /// </summary>
        /// <param name="restriction">Record restriction to apply</param>
        /// <returns>Number of rows affected.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="restriction"/> cannot be <c>null</c>.</exception>
        public int DeleteRecord(RecordRestriction restriction)
        {
            if ((object)restriction == null)
                throw new ArgumentNullException(nameof(restriction));

            string sqlExpression = null;

            try
            {
                sqlExpression = $"{m_deleteWhereSql}{UpdateFieldNames(restriction.FilterExpression)}";
                int affectedRecords = m_connection.ExecuteNonQuery(sqlExpression, restriction.Parameters);

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record delete for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction.Parameters)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        /// <summary>
        /// Deletes the records referenced by the specified SQL filter expression and parameters.
        /// </summary>
        /// <param name="filterExpression">
        /// Filter SQL expression for restriction as a composite format string - does not include WHERE.
        /// When escaping is needed for field names, use standard ANSI quotes.
        /// </param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format <paramref name="filterExpression"/>
        /// will be converted into query parameters where each of the corresponding values in the
        /// <paramref name="parameters"/> collection will be applied as <see cref="IDbDataParameter"/>
        /// values to an executed <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="DeleteRecord(RecordRestriction)"/>.
        /// </para>
        /// </remarks>
        public int DeleteRecordWhere(string filterExpression, params object[] parameters)
        {
            return DeleteRecord(new RecordRestriction(filterExpression, parameters));
        }

        /// <summary>
        /// Updates the database with the specified modeled table <paramref name="record"/>,
        /// any model properties marked with <see cref="UpdateValueExpressionAttribute"/> will
        /// be evaluated any applied before the record is provided to the data source.
        /// </summary>
        /// <param name="record">Record to update.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </remarks>
        public int UpdateRecord(T record, RecordRestriction restriction = null)
        {
            List<object> values = new List<object>();

            try
            {
                s_updateRecordInstance(new CurrentScope
                {
                    Instance = record,
                    TableOperations = this,
                    Connection = m_connection
                });
            }
            catch (Exception ex)
            {
                if ((object)m_exceptionHandler == null)
                    throw;

                m_exceptionHandler(ex);
                return 0;
            }

            if ((object)restriction == null)
            {
                try
                {
                    foreach (PropertyInfo property in s_updateProperties)
                        values.Add(property.GetValue(record));

                    foreach (PropertyInfo property in s_primaryKeyProperties)
                        values.Add(property.GetValue(record));

                    return m_connection.ExecuteNonQuery(m_updateSql, values.ToArray());
                }
                catch (Exception ex)
                {
                    InvalidOperationException opex = new InvalidOperationException($"Exception during record update for {typeof(T).Name} \"{m_updateSql}, {ValueList(values)}\": {ex.Message}", ex);

                    if ((object)m_exceptionHandler == null)
                        throw opex;

                    m_exceptionHandler(opex);
                    return 0;
                }
            }

            string sqlExpression = null;

            try
            {
                foreach (PropertyInfo property in s_updateProperties)
                    values.Add(property.GetValue(record));

                values.AddRange(restriction.Parameters);

                List<object> updateWhereOffsets = new List<object>();
                int updateFieldIndex = s_updateProperties.Length;

                for (int i = 0; i < restriction.Parameters.Length; i++)
                    updateWhereOffsets.Add($"{{{updateFieldIndex + i}}}");

                sqlExpression = $"{m_updateWhereSql}{string.Format(UpdateFieldNames(restriction.FilterExpression), updateWhereOffsets.ToArray())}";
                return m_connection.ExecuteNonQuery(sqlExpression, values.ToArray());
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record update for {typeof(T).Name} \"{sqlExpression}, {ValueList(values)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        int ITableOperations.UpdateRecord(object value, RecordRestriction restriction)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot update record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return UpdateRecord(record);
        }

        /// <summary>
        /// Updates the database with the specified modeled table <paramref name="record"/>
        /// referenced by the specified SQL filter expression and parameters, any model properties
        /// marked with <see cref="UpdateValueExpressionAttribute"/> will be evaluated any applied
        /// before the record is provided to the data source.
        /// </summary>
        /// <param name="record">Record to update.</param>
        /// <param name="filterExpression">
        /// Filter SQL expression for restriction as a composite format string - does not include WHERE.
        /// When escaping is needed for field names, use standard ANSI quotes.
        /// </param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// <para>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </para>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format <paramref name="filterExpression"/>
        /// will be converted into query parameters where each of the corresponding values in the
        /// <paramref name="parameters"/> collection will be applied as <see cref="IDbDataParameter"/>
        /// values to an executed <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="UpdateRecord(T, RecordRestriction)"/>.
        /// </para>
        /// </remarks>
        public int UpdateRecordWhere(T record, string filterExpression, params object[] parameters)
        {
            return UpdateRecord(record, new RecordRestriction(filterExpression, parameters));
        }

        int ITableOperations.UpdateRecordWhere(object value, string filterExpression, params object[] parameters)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot update record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return UpdateRecordWhere(record, filterExpression, parameters);
        }

        /// <summary>
        /// Updates the database with the specified <paramref name="row"/>, any model properties
        /// marked with <see cref="UpdateValueExpressionAttribute"/> will be evaluated any applied
        /// before the record is provided to the data source.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be updated.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </remarks>
        public int UpdateRecord(DataRow row, RecordRestriction restriction = null)
        {
            return UpdateRecord(LoadRecord(row), restriction);
        }

        /// <summary>
        /// Updates the database with the specified <paramref name="row"/> referenced by the
        /// specified SQL filter expression and parameters, any model properties marked with
        /// <see cref="UpdateValueExpressionAttribute"/> will be evaluated any applied before
        /// the record is provided to the data source.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be updated.</param>
        /// <param name="filterExpression">
        /// Filter SQL expression for restriction as a composite format string - does not include WHERE.
        /// When escaping is needed for field names, use standard ANSI quotes.
        /// </param>
        /// <param name="parameters">Restriction parameter values.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// <para>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </para>
        /// <para>
        /// Each indexed parameter, e.g., "{0}", in the composite format <paramref name="filterExpression"/>
        /// will be converted into query parameters where each of the corresponding values in the
        /// <paramref name="parameters"/> collection will be applied as <see cref="IDbDataParameter"/>
        /// values to an executed <see cref="IDbCommand"/> query.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="UpdateRecord(DataRow, RecordRestriction)"/>.
        /// </para>
        /// </remarks>
        public int UpdateRecordWhere(DataRow row, string filterExpression, params object[] parameters)
        {
            return UpdateRecord(row, new RecordRestriction(filterExpression, parameters));
        }

        /// <summary>
        /// Adds the specified modeled table <paramref name="record"/> to the database.
        /// </summary>
        /// <param name="record">Record to add.</param>
        /// <returns>Number of rows affected.</returns>
        public int AddNewRecord(T record)
        {
            List<object> values = new List<object>();

            try
            {
                foreach (PropertyInfo property in s_addNewProperties)
                    values.Add(property.GetValue(record));

                int affectedRecords = m_connection.ExecuteNonQuery(m_addNewSql, values.ToArray());

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record insert for {typeof(T).Name} \"{m_addNewSql}, {ValueList(values)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        int ITableOperations.AddNewRecord(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot add new record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return AddNewRecord(record);
        }

        /// <summary>
        /// Adds the specified <paramref name="row"/> to the database.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be added.</param>
        /// <returns>Number of rows affected.</returns>
        public int AddNewRecord(DataRow row)
        {
            return AddNewRecord(LoadRecord(row));
        }

        /// <summary>
        /// Adds the specified modeled table <paramref name="record"/> to the database if the
        /// record has not defined any of its primary key values; otherwise, the database will
        /// be updated with the specified modeled table <paramref name="record"/>.
        /// </summary>
        /// <param name="record">Record to add or update.</param>
        /// <returns>Number of rows affected.</returns>
        public int AddNewOrUpdateRecord(T record)
        {
            return s_primaryKeyProperties.All(property => Common.IsDefaultValue(property.GetValue(record))) ? AddNewRecord(record) : UpdateRecord(record);
        }

        int ITableOperations.AddNewOrUpdateRecord(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot add new or update record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return AddNewOrUpdateRecord(record);
        }

        /// <summary>
        /// Gets the primary key values from the specified <paramref name="record"/>.
        /// </summary>
        /// <param name="record">Record of data to retrieve primary keys from.</param>
        /// <returns>Primary key values from the specified <paramref name="record"/>.</returns>
        public object[] GetPrimaryKeys(T record)
        {
            try
            {
                List<object> values = new List<object>();

                foreach (PropertyInfo property in s_primaryKeyProperties)
                    values.Add(property.GetValue(record));

                return values.ToArray();
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception loading primary key fields for {typeof(T).Name} \"{s_primaryKeyProperties.Select(property => property.Name).ToDelimitedString(", ")}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return new object[0];
            }
        }

        object[] ITableOperations.GetPrimaryKeys(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot get primary keys for record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return GetPrimaryKeys(record);
        }

        /// <summary>
        /// Gets the primary key values from the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data.</param>
        /// <returns>Primary key values from the specified <paramref name="row"/>.</returns>
        public object[] GetPrimaryKeys(DataRow row)
        {
            try
            {
                List<object> values = new List<object>();

                foreach (PropertyInfo property in s_primaryKeyProperties)
                    values.Add(row[s_fieldNames[property.Name]]);

                return values.ToArray();
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception loading primary key fields for {typeof(T).Name} \"{s_primaryKeyProperties.Select(property => property.Name).ToDelimitedString(", ")}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return new object[0];
            }
        }

        /// <summary>
        /// Gets the field names for the table; if <paramref name="escaped"/> is <c>true</c>, also includes any escaping as defined in model.
        /// </summary>
        /// <param name="escaped">Flag that determines if field names should include any escaping as defined in the model; defaults to <c>true</c>.</param>
        /// <returns>Array of field names.</returns>
        /// <remarks>
        /// A field name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        public string[] GetFieldNames(bool escaped = true)
        {
            if (escaped)
                return s_fieldNames.Values.Select(fieldName => GetEscapedFieldName(fieldName)).ToArray();

            // Fields in the field names dictionary are stored in unescaped format
            return s_fieldNames.Values.ToArray();
        }

        /// <summary>
        /// Get the primary key field names for the table; if <paramref name="escaped"/> is <c>true</c>, also includes any escaping as defined in model.
        /// </summary>
        /// <param name="escaped">Flag that determines if field names should include any escaping as defined in the model; defaults to <c>true</c>.</param>
        /// <returns>Array of primary key field names.</returns>
        /// <remarks>
        /// A field name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        public string[] GetPrimaryKeyFieldNames(bool escaped = true)
        {
            if (escaped)
                return s_primaryKeyFields.Split(',').Select(fieldName => GetEscapedFieldName(fieldName.Trim())).ToArray();

            return s_primaryKeyFields.Split(',').Select(fieldName => GetUnescapedFieldName(fieldName.Trim())).ToArray();
        }

        /// <summary>
        /// Attempts to get the specified <paramref name="attribute"/> for a field.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to attempt to get.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attribute">Attribute that was found, if any.</param>
        /// <returns><c>true</c> if attribute was found; otherwise, <c>false</c>.</returns>
        public bool TryGetFieldAttribute<TAttribute>(string fieldName, out TAttribute attribute) where TAttribute : Attribute
        {
            string propertyName;
            PropertyInfo property;

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property) && property.TryGetAttribute(out attribute))
                return true;

            attribute = default(TAttribute);
            return false;
        }

        /// <summary>
        /// Attempts to get the specified <paramref name="attributeType"/> for a field.
        /// </summary>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attributeType">Type of attribute to attempt to get.</param>
        /// <param name="attribute">Attribute that was found, if any.</param>
        /// <returns><c>true</c> if attribute was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/>.</exception>
        public bool TryGetFieldAttribute(string fieldName, Type attributeType, out Attribute attribute)
        {
            string propertyName;
            PropertyInfo property;

            if (!attributeType.IsInstanceOfType(typeof(Attribute)))
                throw new ArgumentException($"The specified type \"{attributeType.Name}\" is not an Attribute.", nameof(attributeType));

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property) && property.TryGetAttribute(attributeType, out attribute))
                return true;

            attribute = null;
            return false;
        }

        /// <summary>
        /// Determines if the specified field has an associated attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to search for.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <returns><c>true</c> if field has attribute; otherwise, <c>false</c>.</returns>
        public bool FieldHasAttribute<TAttribute>(string fieldName) where TAttribute : Attribute
        {
            return FieldHasAttribute(fieldName, typeof(TAttribute));
        }

        /// <summary>
        /// Determines if the specified field has an associated attribute.
        /// </summary>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attributeType">Type of attribute to search for.</param>
        /// <returns><c>true</c> if field has attribute; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/>.</exception>
        public bool FieldHasAttribute(string fieldName, Type attributeType)
        {
            string propertyName;
            PropertyInfo property;
            HashSet<Type> attributes;

            if (!attributeType.IsSubclassOf(typeof(Attribute)))
                throw new ArgumentException($"The specified type \"{attributeType.Name}\" is not an Attribute.", nameof(attributeType));

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property) && s_attributes.TryGetValue(property, out attributes))
                return attributes.Contains(attributeType);

            return false;
        }

        /// <summary>
        /// Gets the value for the specified field.
        /// </summary>
        /// <param name="record">Modeled table record.</param>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field value or <c>null</c> if field is not found.</returns>
        public object GetFieldValue(T record, string fieldName)
        {
            string propertyName;
            PropertyInfo property;

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property))
                return property.GetValue(record);

            return null;
        }

        object ITableOperations.GetFieldValue(object value, string fieldName)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot get \"{fieldName}\" field value for record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return GetFieldValue(record, fieldName);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> for the specified field.
        /// </summary>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field <see cref="Type"/> or <c>null</c> if field is not found.</returns>
        public Type GetFieldType(string fieldName)
        {
            string propertyName;
            PropertyInfo property;

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property))
                return property.PropertyType;

            return null;
        }

        /// <summary>
        /// Generates a <see cref="RecordRestriction"/> based on fields marked with <see cref="SearchableAttribute"/> and specified <paramref name="searchText"/>.
        /// </summary>
        /// <param name="searchText">Text to search.</param>
        /// <returns><see cref="RecordRestriction"/> based on fields marked with <see cref="SearchableAttribute"/> and specified <paramref name="searchText"/>.</returns>
        public RecordRestriction GetSearchRestriction(string searchText)
        {
            if (string.IsNullOrWhiteSpace(m_searchFilterSql) || string.IsNullOrWhiteSpace(searchText))
                return null;

            searchText = searchText.Trim();

            string[] keyWords = searchText.RemoveDuplicateWhiteSpace().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (keyWords.Length == 1)
                return new RecordRestriction(m_searchFilterSql, $"%{searchText}%", searchText);

            StringBuilder multiKeyWordFilter = new StringBuilder();

            for (int i = 0; i < keyWords.Length * 2; i+=2)
            {
                if (i > 0)
                    multiKeyWordFilter.Append(" AND ");

                multiKeyWordFilter.Append('(');
                multiKeyWordFilter.AppendFormat(m_searchFilterSql, $"{{{i}}}", $"{{{i + 1}}}");
                multiKeyWordFilter.Append(')');
            }

            return new RecordRestriction(multiKeyWordFilter.ToString(), keyWords.SelectMany(keyWord => new object[] { $"%{keyWord}%", keyWord }).ToArray());
        }

        /// <summary>
        /// Calculates the size of the current primary key cache, in number of records.
        /// </summary>
        /// <returns>Number of records in the current primary key cache.</returns>
        public int GetPrimaryKeyCacheSize()
        {
            return m_primaryKeyCache?.Rows.Count ?? 0;
        }

        /// <summary>
        /// Clears the primary key cache for this <see cref="TableOperations{T}"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is intended to be used in conjunction with calls to the overloads for
        /// <see cref="QueryRecords(string, bool, int, int, RecordRestriction)"/> which are
        /// used for record pagination.
        /// </para>
        /// <para>
        /// If record set is known to have changed outside purview of this class, this method
        /// should be called so that primary key cache can be reloaded.
        /// </para>
        /// </remarks>
        public void ClearPrimaryKeyCache()
        {
            m_primaryKeyCache = null;
        }

        // Derive table name, escaping it if requested by model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetEscapedTableName()
        {
            if ((object)s_escapedTableNameTargets == null)
                return s_tableName;

            bool useAnsiQuotes;

            if (s_escapedTableNameTargets.TryGetValue(m_connection.DatabaseType, out useAnsiQuotes))
                return m_connection.EscapeIdentifier(s_tableName, useAnsiQuotes);

            return s_tableName;
        }

        // Derive field name, escaping it if requested by model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetEscapedFieldName(string fieldName, Dictionary<DatabaseType, bool> escapedFieldNameTargets = null)
        {
            if ((object)s_escapedFieldNameTargets == null)
                return fieldName;

            if ((object)escapedFieldNameTargets == null && !s_escapedFieldNameTargets.TryGetValue(fieldName, out escapedFieldNameTargets))
                return fieldName;

            bool useAnsiQuotes;

            if (escapedFieldNameTargets.TryGetValue(m_connection.DatabaseType, out useAnsiQuotes))
                return m_connection.EscapeIdentifier(fieldName, useAnsiQuotes);

            return fieldName;
        }

        // Derive field name, unescaping it if it was escaped by the model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetUnescapedFieldName(string fieldName)
        {
            if ((object)s_escapedFieldNameTargets == null)
                return fieldName;

            Dictionary<DatabaseType, bool> escapedFieldNameTargets;

            if (!s_escapedFieldNameTargets.TryGetValue(fieldName, out escapedFieldNameTargets))
                return fieldName;

            return fieldName.Substring(1, fieldName.Length - 2);
        }

        // Update field names in expression, escaping or unescaping as needed as defined by model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string UpdateFieldNames(string filterExpression)
        {
            if ((object)s_escapedFieldNameTargets != null)
            {
                foreach (KeyValuePair<string, Dictionary<DatabaseType, bool>> escapedFieldNameTarget in s_escapedFieldNameTargets)
                {
                    string fieldName = escapedFieldNameTarget.Key;
                    string derivedFieldName = GetEscapedFieldName(fieldName, escapedFieldNameTarget.Value);
                    string ansiEscapedFieldName = $"\"{fieldName}\"";

                    if (m_useCaseSensitiveFieldNames)
                    {
                        if (!derivedFieldName.Equals(ansiEscapedFieldName))
                            filterExpression = filterExpression.Replace(ansiEscapedFieldName, derivedFieldName);
                    }
                    else
                    {
                        if (!derivedFieldName.Equals(ansiEscapedFieldName, StringComparison.OrdinalIgnoreCase))
                            filterExpression = filterExpression.ReplaceCaseInsensitive(ansiEscapedFieldName, derivedFieldName);
                    }
                }
            }

            return filterExpression;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly string s_tableName;
        private static readonly Dictionary<string, PropertyInfo> s_properties;
        private static readonly Dictionary<string, string> s_fieldNames;
        private static readonly Dictionary<string, string> s_propertyNames;
        private static readonly Dictionary<PropertyInfo, HashSet<Type>> s_attributes;
        private static readonly PropertyInfo[] s_addNewProperties;
        private static readonly PropertyInfo[] s_updateProperties;
        private static readonly PropertyInfo[] s_primaryKeyProperties;
        private static readonly Dictionary<DatabaseType, bool> s_escapedTableNameTargets;
        private static readonly Dictionary<string, Dictionary<DatabaseType, bool>> s_escapedFieldNameTargets;
        private static readonly List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>> s_expressionAmendments;
        private static readonly RecordRestriction s_rootQueryRestriction;
        private static readonly string s_selectCountSql;
        private static readonly string s_selectSetSql;
        private static readonly string s_selectSetWhereSql;
        private static readonly string s_selectKeysSql;
        private static readonly string s_selectKeysWhereSql;
        private static readonly string s_selectRowSql;
        private static readonly string s_addNewSql;
        private static readonly string s_updateSql;
        private static readonly string s_updateWhereSql;
        private static readonly string s_deleteSql;
        private static readonly string s_deleteWhereSql;
        private static readonly string s_primaryKeyFields;
        private static readonly string s_searchFilterSql;
        private static readonly bool s_hasPrimaryKeyIdentityField;
        private static readonly Func<CurrentScope, T> s_createRecordInstance;
        private static readonly Action<CurrentScope> s_updateRecordInstance;
        private static readonly Action<CurrentScope> s_applyRecordDefaults;
        private static TypeRegistry s_typeRegistry;

        // Static Constructor
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        static TableOperations()
        {
            StringBuilder addNewFields = new StringBuilder();
            StringBuilder addNewFormat = new StringBuilder();
            StringBuilder updateFormat = new StringBuilder();
            StringBuilder whereFormat = new StringBuilder();
            StringBuilder allFields = new StringBuilder("*");
            StringBuilder primaryKeyFields = new StringBuilder();
            StringBuilder searchFilterSql = new StringBuilder();
            List<PropertyInfo> addNewProperties = new List<PropertyInfo>();
            List<PropertyInfo> updateProperties = new List<PropertyInfo>();
            List<PropertyInfo> primaryKeyProperties = new List<PropertyInfo>();
            int primaryKeyIndex = 0;
            int addNewFieldIndex = 0;
            int updateFieldIndex = 0;

            // Table name will default to class name of modeled table
            s_tableName = typeof(T).Name;

            // Check for overridden table name
            TableNameAttribute tableNameAttribute;

            if (typeof(T).TryGetAttribute(out tableNameAttribute) && !string.IsNullOrWhiteSpace(tableNameAttribute.TableName))
                s_tableName = tableNameAttribute.TableName;

            // Check for escaped table name targets
            UseEscapedNameAttribute[] useEscapedNameAttributes;

            if (typeof(T).TryGetAttributes(out useEscapedNameAttributes))
                s_escapedTableNameTargets = DeriveEscapedNameTargets(useEscapedNameAttributes);

            // Check for expression amendments
            AmendExpressionAttribute[] amendExpressionAttributes;

            if (typeof(T).TryGetAttributes(out amendExpressionAttributes))
                s_expressionAmendments = DeriveExpressionAmendments(amendExpressionAttributes);

            // Check for root query restriction
            RootQueryRestrictionAttribute rootQueryRestrictionAttribute;

            if (typeof(T).TryGetAttribute(out rootQueryRestrictionAttribute))
                s_rootQueryRestriction = new RecordRestriction(rootQueryRestrictionAttribute.FilterExpression, rootQueryRestrictionAttribute.Parameters);

            s_properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite)
                .Where(property => !property.AttributeExists<PropertyInfo, NonRecordFieldAttribute>())
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

            s_fieldNames = s_properties.ToDictionary(kvp => kvp.Key, kvp => GetFieldName(kvp.Value), StringComparer.OrdinalIgnoreCase);
            s_propertyNames = s_fieldNames.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
            s_attributes = new Dictionary<PropertyInfo, HashSet<Type>>();
            s_hasPrimaryKeyIdentityField = false;

            foreach (PropertyInfo property in s_properties.Values)
            {
                string fieldName = s_fieldNames[property.Name];
                PrimaryKeyAttribute primaryKeyAttribute;
                SearchableAttribute searchableAttribute;

                property.TryGetAttribute(out primaryKeyAttribute);
                property.TryGetAttribute(out searchableAttribute);

                if (property.TryGetAttributes(out useEscapedNameAttributes))
                {
                    if ((object)s_escapedFieldNameTargets == null)
                        s_escapedFieldNameTargets = new Dictionary<string, Dictionary<DatabaseType, bool>>(StringComparer.OrdinalIgnoreCase);

                    s_escapedFieldNameTargets[fieldName] = DeriveEscapedNameTargets(useEscapedNameAttributes);

                    // If any database has been targeted for escaping the field name, pre-apply the standard ANSI escaped
                    // field name in the static SQL expressions. This will provide a unique replaceable identifier should
                    // the common database delimiters, or no delimiters, be applicable for an active database connection
                    fieldName = $"\"{fieldName}\"";
                }

                if ((object)primaryKeyAttribute != null)
                {
                    if (primaryKeyAttribute.IsIdentity)
                    {
                        s_hasPrimaryKeyIdentityField = true;
                    }
                    else
                    {
                        addNewFields.Append($"{(addNewFields.Length > 0 ? ", " : "")}{fieldName}");
                        addNewFormat.Append($"{(addNewFormat.Length > 0 ? ", " : "")}{{{addNewFieldIndex++}}}");
                        addNewProperties.Add(property);
                    }

                    whereFormat.Append($"{(whereFormat.Length > 0 ? " AND " : "")}{fieldName}={{{primaryKeyIndex++}}}");
                    primaryKeyFields.Append($"{(primaryKeyFields.Length > 0 ? ", " : "")}{fieldName}");
                    primaryKeyProperties.Add(property);
                }
                else
                {
                    addNewFields.Append($"{(addNewFields.Length > 0 ? ", " : "")}{fieldName}");
                    addNewFormat.Append($"{(addNewFormat.Length > 0 ? ", " : "")}{{{addNewFieldIndex++}}}");
                    updateFormat.Append($"{(updateFormat.Length > 0 ? ", " : "")}{fieldName}={{{updateFieldIndex++}}}");
                    addNewProperties.Add(property);
                    updateProperties.Add(property);
                }

                if ((object)searchableAttribute != null)
                {
                    if (searchFilterSql.Length > 0)
                        searchFilterSql.Append(" OR ");

                    if (searchableAttribute.SearchType == SearchType.Default)
                    {
                        if (property.PropertyType == typeof(string))
                            searchFilterSql.Append($"{fieldName} LIKE {{0}}");
                        else
                            searchFilterSql.Append($"{fieldName}={{1}}");
                    }
                    else
                    {
                        if (searchableAttribute.SearchType == SearchType.LikeExpression)
                            searchFilterSql.Append($"{fieldName} LIKE {{0}}");
                        else
                            searchFilterSql.Append($"{fieldName}={{1}}");
                    }
                }

                s_attributes.Add(property, new HashSet<Type>(property.CustomAttributes.Select(attributeData => attributeData.AttributeType)));
            }

            // Have to assume all fields are primary when none are specified
            if (primaryKeyProperties.Count == 0)
            {
                foreach (PropertyInfo property in s_properties.Values)
                {
                    string fieldName = s_fieldNames[property.Name];

                    if (s_escapedFieldNameTargets?.ContainsKey(fieldName) ?? false)
                        fieldName = $"\"{fieldName}\"";

                    whereFormat.Append($"{(whereFormat.Length > 0 ? " AND " : "")}{fieldName}={{{primaryKeyIndex++}}}");
                    primaryKeyFields.Append($"{(primaryKeyFields.Length > 0 ? ", " : "")}{fieldName}");
                    primaryKeyProperties.Add(property);
                }

                s_primaryKeyFields = primaryKeyFields.ToString();

                // Default to all
                primaryKeyFields.Clear();
                primaryKeyFields.Append("*");
            }
            else
            {
                s_primaryKeyFields = primaryKeyFields.ToString();
            }

            List<object> updateWhereOffsets = new List<object>();

            for (int i = 0; i < primaryKeyIndex; i++)
                updateWhereOffsets.Add($"{{{updateFieldIndex + i}}}");

            // If any database has been targeted for escaping the table name, pre-apply the standard ANSI escaped
            // table name in the static SQL expressions. This will provide a unique replaceable identifier should
            // the common database delimiters, or no delimiters, be applicable for an active database connection
            string tableName = s_tableName;

            if ((object)s_escapedTableNameTargets != null)
                tableName = $"\"{tableName}\"";

            if ((object)s_expressionAmendments != null)
            {
                // Add tokens to primary expressions for easy replacement
                tableName = $"{TableNamePrefixToken}{tableName}{TableNameSuffixToken}";
                allFields.Insert(0, FieldListPrefixToken);
                allFields.Append(FieldListSuffixToken);
                primaryKeyFields.Insert(0, FieldListPrefixToken);
                primaryKeyFields.Append(FieldListSuffixToken);
                addNewFields.Insert(0, FieldListPrefixToken);
                addNewFields.Append(FieldListSuffixToken);
                updateFormat.Insert(0, FieldListPrefixToken);
                updateFormat.Append(FieldListSuffixToken);
            }

            s_selectCountSql = string.Format(SelectCountSqlFormat, tableName);
            s_selectSetSql = string.Format(SelectSetSqlFormat, allFields, tableName);
            s_selectSetWhereSql = string.Format(SelectSetWhereSqlFormat, allFields, tableName);
            s_selectKeysSql = string.Format(SelectSetSqlFormat, primaryKeyFields, tableName);
            s_selectKeysWhereSql = string.Format(SelectSetWhereSqlFormat, primaryKeyFields, tableName);
            s_selectRowSql = string.Format(SelectRowSqlFormat, tableName, whereFormat);
            s_addNewSql = string.Format(AddNewSqlFormat, tableName, addNewFields, addNewFormat);
            s_updateSql = string.Format(UpdateSqlFormat, tableName, updateFormat, string.Format(whereFormat.ToString(), updateWhereOffsets.ToArray()));
            s_deleteSql = string.Format(DeleteSqlFormat, tableName, whereFormat);
            s_updateWhereSql = s_updateSql.Substring(0, s_updateSql.IndexOf(" WHERE ", StringComparison.Ordinal) + 7);
            s_deleteWhereSql = s_deleteSql.Substring(0, s_deleteSql.IndexOf(" WHERE ", StringComparison.Ordinal) + 7);
            s_searchFilterSql = searchFilterSql.ToString();

            s_addNewProperties = addNewProperties.ToArray();
            s_updateProperties = updateProperties.ToArray();
            s_primaryKeyProperties = primaryKeyProperties.ToArray();

            // Create an instance of modeled table to allow any static functionality to be initialized,
            // such as registering any custom types or symbols that may be useful for value expressions
            ValueExpressionParser<T>.InitializeType();

            // Generate compiled "create new" and "update" record functions for modeled table
            s_createRecordInstance = ValueExpressionParser<T>.CreateInstance<CurrentScope>(s_properties.Values, s_typeRegistry);
            s_updateRecordInstance = ValueExpressionParser<T>.UpdateInstance<CurrentScope>(s_properties.Values, s_typeRegistry);
            s_applyRecordDefaults = ValueExpressionParser<T>.ApplyDefaults<CurrentScope>(s_properties.Values, s_typeRegistry);
        }

        // Static Properties

        /// <summary>
        /// Gets or sets <see cref="ExpressionEvaluator.TypeRegistry"/> instance used for evaluating encountered instances
        /// of the <see cref="ValueExpressionAttributeBase"/> on modeled table properties.
        /// </summary>
        /// <remarks>
        /// Accessing this property will create a unique type registry for the current type <typeparamref name="T"/> which
        /// will initially contain the values found in the <see cref="ValueExpressionParser.DefaultTypeRegistry"/>
        /// and can be augmented with custom types. Set to <c>null</c> to restore use of the default type registry.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public static TypeRegistry TypeRegistry
        {
            get
            {
                return s_typeRegistry ?? (s_typeRegistry = ValueExpressionParser.DefaultTypeRegistry.Clone());
            }
            set
            {
                s_typeRegistry = value;
            }
        }

        // Static Methods
        private static string GetFieldName(PropertyInfo property)
        {
            FieldNameAttribute fieldNameAttribute;

            if (property.TryGetAttribute(out fieldNameAttribute) && !string.IsNullOrEmpty(fieldNameAttribute?.FieldName))
                return fieldNameAttribute.FieldName;

            return property.Name;
        }

        private static Dictionary<DatabaseType, bool> DeriveEscapedNameTargets(UseEscapedNameAttribute[] useEscapedNameAttributes)
        {
            if (useEscapedNameAttributes == null || useEscapedNameAttributes.Length == 0)
                return null;

            DatabaseType[] databaseTypes;
            bool allDatabasesTargeted = false;

            // If any attribute has no database target type specified, then all database types are assumed
            if (useEscapedNameAttributes.Any(attribute => attribute.TargetDatabaseType == null))
            {
                allDatabasesTargeted = true;
                databaseTypes = Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>().ToArray();
            }
            else
            {
                databaseTypes = useEscapedNameAttributes.Select(attribute => attribute.TargetDatabaseType.GetValueOrDefault()).Distinct().ToArray();
            }

            Dictionary<DatabaseType, bool> escapedNameTargets = new Dictionary<DatabaseType, bool>(databaseTypes.Length);

            foreach (DatabaseType databaseType in databaseTypes)
            {
                UseEscapedNameAttribute useEscapedNameAttribute = useEscapedNameAttributes.FirstOrDefault(attribute => attribute.TargetDatabaseType == databaseType);
                bool useAnsiQuotes = ((object)useEscapedNameAttribute != null && useEscapedNameAttribute.UseAnsiQuotes) || (allDatabasesTargeted && databaseType != DatabaseType.MySQL);
                escapedNameTargets[databaseType] = useAnsiQuotes;
            }

            return escapedNameTargets;
        }

        private static List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>> DeriveExpressionAmendments(AmendExpressionAttribute[] amendExpressionAttributes)
        {
            if (amendExpressionAttributes == null || amendExpressionAttributes.Length == 0)
                return null;

            List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>> typedExpressionAmendments = new List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>>();
            List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>> untypedExpressionAmendments = new List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>>();
            List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>> expressionAmendments;

            foreach (AmendExpressionAttribute attribute in amendExpressionAttributes)
            {
                if ((object)attribute == null)
                    continue;

                DatabaseType[] databaseTypes;

                // If any attribute has no database target type specified, then all database types are assumed
                if (attribute.TargetDatabaseType == null)
                {
                    databaseTypes = Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>().ToArray();
                    expressionAmendments = untypedExpressionAmendments;
                }
                else
                {
                    databaseTypes = new[] { attribute.TargetDatabaseType.Value };
                    expressionAmendments = typedExpressionAmendments;
                }

                foreach (DatabaseType databaseType in databaseTypes)
                {
                    string amendmentText = attribute.AmendmentText.Trim();
                    amendmentText = attribute.AffixPosition == AffixPosition.Prefix ? $"{amendmentText} " : $" {amendmentText}";
                    expressionAmendments.Add(new Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>(databaseType, attribute.TargetExpression, attribute.StatementTypes, attribute.AffixPosition, amendmentText));
                }
            }

            // Sort expression amendments with a specified database type higher in the execution order to allow for database specific overrides
            expressionAmendments = new List<Tuple<DatabaseType, TargetExpression, StatementTypes, AffixPosition, string>>(typedExpressionAmendments);
            expressionAmendments.AddRange(untypedExpressionAmendments);

            return expressionAmendments.Count > 0 ? expressionAmendments : null;
        }

        private static string ValueList(IReadOnlyList<object> values)
        {
            if (values == null)
                return "";

            StringBuilder delimitedString = new StringBuilder();


            for (int i = 0; i < values.Count; i++)
            {
                if (delimitedString.Length > 0)
                    delimitedString.Append(", ");

                delimitedString.AppendFormat("{0}:{1}", i, values[i]);
            }

            return delimitedString.ToString();
        }

        #endregion
    }
}