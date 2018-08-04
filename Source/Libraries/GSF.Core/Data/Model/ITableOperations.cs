//******************************************************************************************************
//  ITableOperations.cs - Gbtc
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
//  05/12/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.ComponentModel;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Data.Model
{
    /// <summary>
    /// Defines a common interface for implementations of <see cref="TableOperations{T}"/>.
    /// </summary>
    public interface ITableOperations
    {
        /// <summary>
        /// Gets the table name defined for the modeled table, includes any escaping as defined in model.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Gets the table name defined for the modeled table without any escape characters.
        /// </summary>
        /// <remarks>
        /// A table name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        string UnescapedTableName { get; }

        /// <summary>
        /// Gets flag that determines if modeled table has a primary key that is an identity field.
        /// </summary>
        bool HasPrimaryKeyIdentityField { get; }

        /// <summary>
        /// Gets or sets delegate used to handle table operation exceptions.
        /// </summary>
        /// <remarks>
        /// When exception handler is provided, table operations will not throw exceptions for database calls, any
        /// encountered exceptions will be passed to handler for processing. Otherwise, exceptions will be thrown
        /// on the call stack.
        /// </remarks>
        Action<Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if field names should be treated as case sensitive. Defaults to <c>false</c>.
        /// </summary>
        /// <remarks>
        /// In cases where modeled table fields have applied <see cref="UseEscapedNameAttribute"/>, this flag will be used
        /// to properly update escaped field names that may be case sensitive. For example, escaped field names in Oracle
        /// are case sensitive. This value is typically <c>false</c>.
        /// </remarks>
        bool UseCaseSensitiveFieldNames { get; set; }

        /// <summary>
        /// Gets or sets primary key cache.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="QueryRecords(string, bool, int, int, string)"/> overloads that include paging parameters
        /// cache the sorted and filtered primary keys of queried records between calls so that paging is fast and
        /// efficient. Since the primary keys are cached, an instance of the <see cref="TableOperations{T}"/> should
        /// exist per user session when using query functions that support pagination. In web based implementations,
        /// the primary cache should be stored with user session state data and then restored between instances of
        /// the <see cref="TableOperations{T}"/> that are created along with a connection that is opened per page.
        /// </para>
        /// <para>
        /// The function <see cref="ClearPrimaryKeyCache"/> should be called to manually clear cache when table
        /// contents are known to have changed. Note that calls to any <see cref="DeleteRecord(object)"/> overload
        /// will automatically clear any existing primary key cache.
        /// </para>
        /// <para>
        /// Primary keys values are stored in data table without interpretation, i.e., in their raw form as queried
        /// from the database. Primary key data in cache will be encrypted for models with primary key fields that
        /// are marked with the <see cref="EncryptDataAttribute"/>
        /// </para>
        /// </remarks>
        DataTable PrimaryKeyCache { get; set; }

        /// <summary>
        /// Gets or sets root record restriction that applies to query table operations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defining a root query restriction creates a base query filter that gets applied to all query operations,
        /// even when another restriction is applied - in this case the root restriction will be pre-pended to the
        /// specified query, e.g.:
        /// <code>
        /// restriction = RootQueryRestriction + restriction;
        /// </code>
        /// A root query restriction is useful to apply a common state to the query operations, e.g., always
        /// filtering records for a specific user or context.
        /// </para>
        /// <para>
        /// A root query restriction can be manually assigned to a <see cref="TableOperations{T}"/> instance or
        /// automatically assigned by marking a model with the <see cref="RootQueryRestrictionAttribute"/>.
        /// </para>
        /// <para>
        /// If any of the <see cref="RecordRestriction.Parameters"/> reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        RecordRestriction RootQueryRestriction { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="RootQueryRestriction"/> should be applied to update operations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="RootQueryRestriction"/> only references primary key fields, then this property value should be set
        /// to <c>false</c> since update operations against a modeled record already take into account primary key fields.
        /// </para>
        /// <para>
        /// This flag can be manually set per <see cref="TableOperations{T}"/> instance or handled automatically by marking
        /// a model with the <see cref="RootQueryRestrictionAttribute"/> and assigning a value to the attribute property
        /// <see cref="RootQueryRestrictionAttribute.ApplyToUpdates"/>.
        /// </para>
        /// </remarks>
        bool ApplyRootQueryRestrictionToUpdates { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="RootQueryRestriction"/> should be applied to delete operations.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="RootQueryRestriction"/> only references primary key fields, then this property value should be set
        /// to <c>false</c> since delete operations against a modeled record already take into account primary key fields.
        /// </para>
        /// <para>
        /// This flag can be manually set per <see cref="TableOperations{T}"/> instance or handled automatically by marking
        /// a model with the <see cref="RootQueryRestrictionAttribute"/> and assigning a value to the attribute property
        /// <see cref="RootQueryRestrictionAttribute.ApplyToDeletes"/>.
        /// </para>
        /// </remarks>
        bool ApplyRootQueryRestrictionToDeletes { get; set; }

        /// <summary>
        /// Creates a new modeled record instance, applying any modeled default values as specified by a
        /// <see cref="DefaultValueAttribute"/> or <see cref="DefaultValueExpressionAttribute"/> on the
        /// model properties.
        /// </summary>
        /// <returns>New modeled record instance with any defined default values applied.</returns>
        object NewRecord();

        /// <summary>
        /// Applies the default values on the specified modeled table <paramref name="record"/>
        /// where any of the properties are marked with either <see cref="DefaultValueAttribute"/>
        /// or <see cref="DefaultValueExpressionAttribute"/>.
        /// </summary>
        /// <param name="record">Record to update.</param>
        void ApplyRecordDefaults(object record);

        /// <summary>
        /// Applies the update values on the specified modeled table <paramref name="record"/> where
        /// any of the properties are marked with <see cref="UpdateValueExpressionAttribute"/>.
        /// </summary>
        /// <param name="record">Record to update.</param>
        void ApplyRecordUpdates(object record);

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
        /// <para>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        object QueryRecord(RecordRestriction restriction);

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
        /// <para>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        object QueryRecord(string orderByExpression, RecordRestriction restriction);

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
        /// If any of the specified <paramref name="parameters"/> reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
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
        object QueryRecordWhere(string filterExpression, params object[] parameters);

        /// <summary>
        /// Queries database and returns modeled table records for the specified parameters.
        /// </summary>
        /// <param name="orderByExpression">Field name expression used for sort order, include ASC or DESC as needed - does not include ORDER BY; defaults to primary keys.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <param name="limit">Limit of number of record to return.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// <para>
        /// If no record <paramref name="restriction"/> or <paramref name="limit"/> is provided, all rows will be returned.
        /// </para>
        /// <para>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        IEnumerable QueryRecords(string orderByExpression = null, RecordRestriction restriction = null, int limit = -1);

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
        /// <para>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        IEnumerable QueryRecords(RecordRestriction restriction);

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
        /// If any of the specified <paramref name="parameters"/> reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
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
        IEnumerable QueryRecordsWhere(string filterExpression, params object[] parameters);

        /// <summary>
        /// Queries database and returns modeled table records for the specified sorting, paging and search parameters.
        /// Search executed against fields modeled with <see cref="SearchableAttribute"/>.
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
        /// If the specified <paramref name="sortField"/> has been marked with <see cref="EncryptDataAttribute"/>,
        /// establishing the primary key cache operation will take longer to execute since query data will need to
        /// be downloaded locally and decrypted so the proper sort order can be determined.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecords(string, bool, int, int, RecordRestriction)"/> where restriction
        /// is generated by <see cref="GetSearchRestriction(string)"/> using <paramref name="searchText"/>.
        /// </para>
        /// </remarks>
        IEnumerable QueryRecords(string sortField, bool ascending, int page, int pageSize, string searchText);

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
        /// <para>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// <para>
        /// If the specified <paramref name="sortField"/> has been marked with <see cref="EncryptDataAttribute"/>,
        /// establishing the primary key cache operation will take longer to execute since query data will need to
        /// be downloaded locally and decrypted so the proper sort order can be determined.
        /// </para>
        /// </remarks>
        IEnumerable QueryRecords(string sortField, bool ascending, int page, int pageSize, RecordRestriction restriction = null);

        /// <summary>
        /// Gets the record count for the modeled table based on search parameter.
        /// Search executed against fields modeled with <see cref="SearchableAttribute"/>.
        /// </summary>
        /// <param name="searchText">Text to search.</param>
        /// <returns>Record count for the modeled table based on search parameter.</returns>
        /// <remarks>
        /// This is a convenience call to <see cref="QueryRecordCount(RecordRestriction)"/> where restriction
        /// is generated by <see cref="GetSearchRestriction(string)"/>
        /// </remarks>
        int QueryRecordCount(string searchText);

        /// <summary>
        /// Gets the record count for the specified <paramref name="restriction"/> - or - total record
        /// count for the modeled table if <paramref name="restriction"/> is <c>null</c>.
        /// </summary>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>
        /// Record count for the specified <paramref name="restriction"/> - or - total record count
        /// for the modeled table if <paramref name="restriction"/> is <c>null</c>.
        /// </returns>
        /// <remarks>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </remarks>
        int QueryRecordCount(RecordRestriction restriction = null);

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
        /// If any of the specified <paramref name="parameters"/> reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="QueryRecordCount(RecordRestriction)"/>.
        /// </para>
        /// </remarks>
        int QueryRecordCountWhere(string filterExpression, params object[] parameters);

        /// <summary>
        /// Locally searches retrieved table records after queried from database for the specified sorting and search parameters.
        /// Search executed against fields modeled with <see cref="SearchableAttribute"/>.
        /// Function only typically used for record models that apply the <see cref="EncryptDataAttribute"/>.
        /// </summary>
        /// <param name="sortField">Field name to order-by.</param>
        /// <param name="ascending">Sort ascending flag; set to <c>false</c> for descending.</param>
        /// <param name="searchText">Text to search.</param>
        /// <param name="comparison"><see cref="StringComparison"/> to use when searching string fields; defaults to ordinal ignore case.</param>
        /// <returns>An array of modeled table row instances for the queried records that match the search.</returns>
        /// <remarks>
        /// <para>
        /// This function searches records locally after query from database, this way <see cref="SearchableAttribute"/> functionality will work
        /// even with fields that are modeled with the <see cref="EncryptDataAttribute"/> and use the <see cref="SearchType.LikeExpression"/>.
        /// Primary keys for this function will not be cached server-side and this function will be slower and more expensive than similar calls
        /// to <see cref="QueryRecords(string, bool, int, int, string)"/>. Usage should be restricted to cases searching for field data that has
        /// been modeled with the <see cref="EncryptDataAttribute"/>.
        /// </para>
        /// <para>
        /// This function does not paginate records, instead a full list of search records is returned. User can cache returned records and page
        /// through them using the <see cref="GetPageOfRecords"/> function. As a result, usage should be restricted to smaller data sets. 
        /// </para>
        /// </remarks>
        object[] SearchRecords(string sortField, bool ascending, string searchText, StringComparison comparison = StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines if any <paramref name="record"/> fields modeled with the <see cref="SearchableAttribute"/> match any of the
        /// specified <paramref name="searchValues"/>.
        /// </summary>
        /// <param name="record">Modeled table record.</param>
        /// <param name="searchValues">Values to search.</param>
        /// <returns>
        /// <c>true</c> if any <paramref name="record"/> fields modeled with <see cref="SearchableAttribute"/> match any of the
        /// specified <paramref name="searchValues"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// String comparisons will be ordinal ignoring case.
        /// </remarks>
        bool IsSearchMatch(object record, params string[] searchValues);

        /// <summary>
        /// Determines if any <paramref name="record"/> fields modeled with the <see cref="SearchableAttribute"/> match any of the
        /// specified <paramref name="searchValues"/>.
        /// </summary>
        /// <param name="record">Modeled table record.</param>
        /// <param name="comparison"><see cref="StringComparison"/> to use when searching string fields.</param>
        /// <param name="searchValues">Values to search.</param>
        /// <returns>
        /// <c>true</c> if any <paramref name="record"/> fields modeled with <see cref="SearchableAttribute"/> match any of the
        /// specified <paramref name="searchValues"/>; otherwise, <c>false</c>.
        /// </returns>
        bool IsSearchMatch(object record, StringComparison comparison, params string[] searchValues);

        /// <summary>
        /// Gets the specified <paramref name="page"/> of records from the provided source <paramref name="records"/> array.
        /// </summary>
        /// <param name="records">Source records array.</param>
        /// <param name="page">Desired page of records.</param>
        /// <param name="pageSize">Desired page size.</param>
        /// <returns>A page of records.</returns>
        IEnumerable GetPageOfRecords(object[] records, int page, int pageSize);

        /// <summary>
        /// Creates a new modeled table record queried from the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="primaryKeys"/>.</returns>
        object LoadRecord(params object[] primaryKeys);

        /// <summary>
        /// Creates a new modeled table record queried from the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be loaded.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="row"/>.</returns>
        object LoadRecord(DataRow row);

        /// <summary>
        /// Converts the given collection of <paramref name="records"/> into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="records">The collection of records to be inserted into the data table.</param>
        /// <returns>A data table containing data from the given records.</returns>
        DataTable ToDataTable(IEnumerable records);

        /// <summary>
        /// Deletes the record referenced by the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>Number of rows affected.</returns>
        int DeleteRecord(params object[] primaryKeys);

        /// <summary>
        /// Deletes the specified modeled table <paramref name="record"/> from the database.
        /// </summary>
        /// <param name="record">Record to delete.</param>
        /// <returns>Number of rows affected.</returns>
        int DeleteRecord(object record);

        /// <summary>
        /// Deletes the record referenced by the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be deleted.</param>
        /// <returns>Number of rows affected.</returns>
        int DeleteRecord(DataRow row);

        /// <summary>
        /// Deletes the records referenced by the specified <paramref name="restriction"/>.
        /// </summary>
        /// <param name="restriction">Record restriction to apply</param>
        /// <param name="applyRootQueryRestriction">
        /// Flag that determines if any existing <see cref="RootQueryRestriction"/> should be applied. Defaults to
        /// <see cref="ApplyRootQueryRestrictionToDeletes"/> setting.
        /// </param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="restriction"/> cannot be <c>null</c>.</exception>
        int DeleteRecord(RecordRestriction restriction, bool? applyRootQueryRestriction = null);

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
        /// If any of the specified <paramref name="parameters"/> reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="DeleteRecord(RecordRestriction, bool?)"/>.
        /// </para>
        /// </remarks>
        int DeleteRecordWhere(string filterExpression, params object[] parameters);

        /// <summary>
        /// Updates the database with the specified modeled table <paramref name="record"/>,
        /// any model properties marked with <see cref="UpdateValueExpressionAttribute"/> will
        /// be evaluated and applied before the record is provided to the data source.
        /// </summary>
        /// <param name="record">Record to update.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <param name="applyRootQueryRestriction">
        /// Flag that determines if any existing <see cref="RootQueryRestriction"/> should be applied. Defaults to
        /// <see cref="ApplyRootQueryRestrictionToUpdates"/> setting.
        /// </param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// <para>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </para>
        /// <para>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        int UpdateRecord(object record, RecordRestriction restriction = null, bool? applyRootQueryRestriction = null);

        /// <summary>
        /// Updates the database with the specified modeled table <paramref name="record"/>
        /// referenced by the specified SQL filter expression and parameters, any model properties
        /// marked with <see cref="UpdateValueExpressionAttribute"/> will be evaluated and applied
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
        /// If any of the specified <paramref name="parameters"/> reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="UpdateRecord(object, RecordRestriction, bool?)"/>.
        /// </para>
        /// </remarks>
        int UpdateRecordWhere(object record, string filterExpression, params object[] parameters);

        /// <summary>
        /// Updates the database with the specified <paramref name="row"/>, any model properties
        /// marked with <see cref="UpdateValueExpressionAttribute"/> will be evaluated and applied
        /// before the record is provided to the data source.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be updated.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// <para>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </para>
        /// <para>
        /// If any of the <paramref name="restriction"/> parameters reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        int UpdateRecord(DataRow row, RecordRestriction restriction = null);

        /// <summary>
        /// Updates the database with the specified <paramref name="row"/> referenced by the
        /// specified SQL filter expression and parameters, any model properties marked with
        /// <see cref="UpdateValueExpressionAttribute"/> will be evaluated and applied before
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
        /// If any of the specified <paramref name="parameters"/> reference a table field that is modeled with
        /// either an <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/>, then the function
        /// <see cref="GetInterpretedFieldValue"/> will need to be called, replacing the target parameter with the
        /// returned value so that the field value will be properly set prior to executing the database function.
        /// </para>
        /// <para>
        /// If needed, field names that are escaped with standard ANSI quotes in the filter expression
        /// will be updated to reflect what is defined in the user model.
        /// </para>
        /// <para>
        /// This is a convenience call to <see cref="UpdateRecord(DataRow, RecordRestriction)"/>.
        /// </para>
        /// </remarks>
        int UpdateRecordWhere(DataRow row, string filterExpression, params object[] parameters);

        /// <summary>
        /// Adds the specified modeled table <paramref name="record"/> to the database.
        /// </summary>
        /// <param name="record">Record to add.</param>
        /// <returns>Number of rows affected.</returns>
        int AddNewRecord(object record);

        /// <summary>
        /// Adds the specified <paramref name="row"/> to the database.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be added.</param>
        /// <returns>Number of rows affected.</returns>
        int AddNewRecord(DataRow row);
        
        /// <summary>
        /// Adds the specified modeled table <paramref name="record"/> to the database if the
        /// record has not defined any of its primary key values; otherwise, the database will
        /// be updated with the specified modeled table <paramref name="record"/>.
        /// </summary>
        /// <param name="record">Record to add or update.</param>
        /// <returns>Number of rows affected.</returns>
        int AddNewOrUpdateRecord(object record);

        /// <summary>
        /// Gets the primary key values from the specified <paramref name="record"/>.
        /// </summary>
        /// <param name="record">Record of data to retrieve primary keys from.</param>
        /// <returns>Primary key values from the specified <paramref name="record"/>.</returns>
        object[] GetPrimaryKeys(object record);

        /// <summary>
        /// Gets the primary key values from the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data.</param>
        /// <returns>Primary key values from the specified <paramref name="row"/>.</returns>
        /// <remarks>
        /// Function returns raw data from <paramref name="row"/> without interpretation, it may be
        /// necessary to call <see cref="GetInterpretedFieldValue"/> for models with primary key
        /// fields that are marked with either <see cref="EncryptDataAttribute"/> or
        /// <see cref="FieldDataTypeAttribute"/>.
        /// </remarks>
        object[] GetPrimaryKeys(DataRow row);

        /// <summary>
        /// Gets the field names for the table; if <paramref name="escaped"/> is <c>true</c>, also includes any escaping as defined in model.
        /// </summary>
        /// <param name="escaped">Flag that determines if field names should include any escaping as defined in the model; defaults to <c>true</c>.</param>
        /// <returns>Array of field names.</returns>
        /// <remarks>
        /// A field name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        string[] GetFieldNames(bool escaped = true);

        /// <summary>
        /// Get the primary key field names for the table; if <paramref name="escaped"/> is <c>true</c>, also includes any escaping as defined in model.
        /// </summary>
        /// <param name="escaped">Flag that determines if field names should include any escaping as defined in the model; defaults to <c>true</c>.</param>
        /// <returns>Array of primary key field names.</returns>
        /// <remarks>
        /// A field name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        string[] GetPrimaryKeyFieldNames(bool escaped = true);

        /// <summary>
        /// Attempts to get the specified <paramref name="attribute"/> for a field.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to attempt to get.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attribute">Attribute that was found, if any.</param>
        /// <returns><c>true</c> if attribute was found; otherwise, <c>false</c>.</returns>
        bool TryGetFieldAttribute<TAttribute>(string fieldName, out TAttribute attribute) where TAttribute : Attribute;
        
        /// <summary>
        /// Attempts to get the specified <paramref name="attributeType"/> for a field.
        /// </summary>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attributeType">Type of attribute to attempt to get.</param>
        /// <param name="attribute">Attribute that was found, if any.</param>
        /// <returns><c>true</c> if attribute was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/>.</exception>
        bool TryGetFieldAttribute(string fieldName, Type attributeType, out Attribute attribute);

        /// <summary>
        /// Determines if the specified field has an associated attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to search for.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <returns><c>true</c> if field has attribute; otherwise, <c>false</c>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        bool FieldHasAttribute<TAttribute>(string fieldName) where TAttribute : Attribute;

        /// <summary>
        /// Determines if the specified field has an associated attribute.
        /// </summary>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attributeType">Type of attribute to search for.</param>
        /// <returns><c>true</c> if field has attribute; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/>.</exception>
        bool FieldHasAttribute(string fieldName, Type attributeType);

        /// <summary>
        /// Gets the value for the specified field.
        /// </summary>
        /// <param name="record">Modeled table record.</param>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field value or <c>null</c> if field is not found.</returns>
        object GetFieldValue(object record, string fieldName);

        /// <summary>
        /// Gets the interpreted value for the specified field, encrypting or returning any intermediate <see cref="IDbDataParameter"/>
        /// value as needed.
        /// </summary>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <param name="value">Field value to use.</param>
        /// <returns>
        /// Interpreted value for the specified field, encrypting or returning any intermediate <see cref="IDbDataParameter"/> value
        /// as needed.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This function will need to be used when calling overloads that take a <see cref="RecordRestriction"/> or composite format
        /// filter expression where the <see cref="EncryptDataAttribute"/> or <see cref="FieldDataTypeAttribute"/> have been modeled
        /// on a field referenced by one of the <see cref="RecordRestriction"/> parameters. Since the record restrictions are used
        /// with a free-form expression, the <see cref="TableOperations{T}"/> class cannot be aware of the fields accessed in the
        /// expression without attempting to parse the expression which would be time consuming and error prone; as a result, users
        /// will need to be aware to call this function when using record restriction that references fields that are either marked
        /// for encryption or use a specific field data-type attribute.
        /// </para>
        /// <para>
        /// If a <see cref="RecordRestriction"/> parameter references a field that is modeled with an <see cref="EncryptDataAttribute"/>,
        /// this function will need to be called, replacing the restriction parameter with the returned value, so that the field data
        /// value will be properly encrypted prior to executing the database function.
        /// </para>
        /// <para>
        /// If a <see cref="RecordRestriction"/> parameter references a field that is modeled with a <see cref="FieldDataTypeAttribute"/>,
        /// this function will need to be called, replacing the restriction parameter with the returned value, so that the field data
        /// type will be properly set prior to executing the database function.
        /// </para>
        /// </remarks>
        object GetInterpretedFieldValue(string fieldName, object value);

        /// <summary>
        /// Gets the <see cref="Type"/> for the specified field.
        /// </summary>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field <see cref="Type"/> or <c>null</c> if field is not found.</returns>
        Type GetFieldType(string fieldName);

        /// <summary>
        /// Generates a <see cref="RecordRestriction"/> based on fields marked with <see cref="SearchableAttribute"/> and specified <paramref name="searchText"/>.
        /// </summary>
        /// <param name="searchText">Text to search.</param>
        /// <returns><see cref="RecordRestriction"/> based on fields marked with <see cref="SearchableAttribute"/> and specified <paramref name="searchText"/>.</returns>
        /// <remarks>
        /// Any fields marked with both <see cref="SearchableAttribute"/> and <see cref="EncryptDataAttribute"/> will be automatically managed, i.e.,
        /// the returned <see cref="RecordRestriction"/> parameters will already apply any field based encryption as needed. Database query functions
        /// executed for fields marked for encryption will only be searched using <see cref="SearchType.FullValueMatch"/>, regardless of any otherwise
        /// specified value in the <see cref="SearchableAttribute"/> as encryption is handled locally. However, the <see cref="SearchRecords"/> function
        /// can be used to find data in encrypted fields that are marked for search with a <see cref="SearchType.LikeExpression"/>.
        /// </remarks>
        RecordRestriction GetSearchRestriction(string searchText);

        /// <summary>
        /// Calculates the size of the current primary key cache, in number of records.
        /// </summary>
        /// <returns>Number of records in the current primary key cache.</returns>
        int GetPrimaryKeyCacheSize();

       /// <summary>
       /// Clears the primary key cache for this <see cref="ITableOperations"/> instance.
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
        void ClearPrimaryKeyCache();
    }
}