//******************************************************************************************************
//  ModelController.cs - Gbtc
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
//  10/04/2019 - Billy Ernest
//       Generated original version of source code.
//  05/25/2021 - C. Lackner
//       Added Additional Fields Search Query and moved to GSF
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using GSF.Data;
using GSF.Data.Model;
using GSF.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GSF.Web.Model
{
    /// <summary>
    /// Base Class for A Webcontroller that provides common Model endpoints such as Edit, Delete, Search.
    /// </summary>
    /// <typeparam name="T">The corresponding Model.</typeparam>
    public class ModelController<T> : ApiController where T : class, new()
    {
        #region [Members ]
        /// <summary>
        /// Class Providing Search Parameters for the Search Endpoints
        /// </summary>
        public class Search
        {
            public string FieldName { get; set; }
            public string SearchText { get; set; }
            public string Operator { get; set; }
            public string Type { get; set; }

            public bool isPivotColumn { get; set; } = false;
        }

        public class PostData
        {
            public IEnumerable<Search> Searches { get; set; }
            public string OrderBy { get; set; }
            public bool Ascending { get; set; }
        }
        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new <see cref="ModelController{T}"/>
        /// </summary>
        public ModelController()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ModelController{T}"/>
        /// </summary>
        /// <param name="hasParent"> Indicates if the Model has a Parent Model.</param>
        /// <param name="parentKey"> The key Associated with the Parent Model.</param>
        /// <param name="primaryKeyField"> The Primary Identifier Field for this Model.</param>
        public ModelController(bool hasParent, string parentKey, string primaryKeyField = "ID")
        {
            HasParent = hasParent;
            ParentKey = parentKey;
            HasUniqueKey = false;
            UniqueKeyField = "";
            PrimaryKeyField = primaryKeyField;

        }

        /// <summary>
        /// Creates a new <see cref="ModelController{T}"/>
        /// </summary>
        /// <param name="hasParent"> Indicates if the Model has a Parent Model.</param>
        /// <param name="parentKey"> The key Associated with the Parent Model.</param>
        /// <param name="hasUniqueKey"> Indicates if the Model also has a unique Key.</param>
        /// <param name="uniqueKey"> The Unique Key Field for this Model.</param>
        public ModelController(bool hasParent, string parentKey, bool hasUniqueKey, string uniqueKey)
        {
            HasParent = hasParent;
            ParentKey = parentKey;
            HasUniqueKey = hasUniqueKey;
            UniqueKeyField = uniqueKey;
        }

        #endregion

        #region [ Properties ]
        protected virtual bool HasParent { get; set; } = false;
        protected virtual string ParentKey { get; set; } = "";
        protected virtual string PrimaryKeyField { get; set; } = "ID";
        protected bool HasUniqueKey { get; set; } = false;
        protected string UniqueKeyField { get; set; } = "";
        protected virtual string Connection { get; } = "systemSettings";
        protected virtual string GetRoles { get; } = "";
        protected virtual string PostRoles { get; } = "Administrator";
        protected virtual string PatchRoles { get; } = "Administrator";
        protected virtual string DeleteRoles { get; } = "Administrator";
        protected virtual string DefaultSort { get; } = null;
        protected virtual bool ViewOnly { get; } = false;
        protected virtual bool AllowSearch { get; } = false;
        protected virtual string CustomView { get; } = "";
        #endregion

        #region [ Http Methods ]
        [HttpGet, Route("New")]
        public virtual IHttpActionResult GetNew()
        {
            if (GetRoles == string.Empty || User.IsInRole(GetRoles))
            {
                using (AdoDataConnection connection = new AdoDataConnection(Connection))
                {

                    try
                    {
                        return Ok(new TableOperations<T>(connection).NewRecord());
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }
                }
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpGet, Route("{parentID?}")]
        public virtual IHttpActionResult Get(string parentID = null)
        {
            if (GetRoles == string.Empty || User.IsInRole(GetRoles))
            {

                try
                {
                    IEnumerable<T> result;
                    if (HasParent && parentID != null)
                    {
                        PropertyInfo parentKey = typeof(T).GetProperty(ParentKey);
                        if (parentKey.PropertyType == typeof(int))
                            result = QueryRecordsWhere(ParentKey + " = {0}", int.Parse(parentID));
                        else if (parentKey.PropertyType == typeof(Guid))
                            result = QueryRecordsWhere(ParentKey + " = {0}", Guid.Parse(parentID));
                        else
                            result = QueryRecordsWhere(ParentKey + " = {0}", parentID);
                    }
                    else
                        result = QueryRecords();

                    if (DefaultSort != null)
                    {
                        PropertyInfo prop = typeof(T).GetProperty(DefaultSort);
                        return Ok(result.OrderBy(x => prop.GetValue(x)));
                    }
                    else
                        return Ok(result);
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpGet, Route("One/{id}")]
        public virtual IHttpActionResult GetOne(string id)
        {
            if (GetRoles == string.Empty || User.IsInRole(GetRoles))
            {

                try
                {
                    T result = null;
                    PropertyInfo primaryKey = typeof(T).GetProperty(PrimaryKeyField);
                    if (primaryKey.PropertyType == typeof(int))
                        result = QueryRecordWhere(PrimaryKeyField + " = {0}", int.Parse(id));
                    else if (primaryKey.PropertyType == typeof(Guid))
                        result = QueryRecordWhere(PrimaryKeyField + " = {0}", Guid.Parse(id));
                    else
                        result = QueryRecordWhere(PrimaryKeyField + " = {0}", id);

                    if (result == null)
                    {
                        TableNameAttribute tableNameAttribute;
                        string tableName;
                        if (typeof(T).TryGetAttribute(out tableNameAttribute))
                            tableName = tableNameAttribute.TableName;
                        else
                            tableName = typeof(T).Name;
                        return BadRequest(string.Format(PrimaryKeyField + " provided does not exist in '{0}'.", tableName));
                    }
                    else
                        return Ok(result);
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpGet, Route("{sort}/{ascending:int}")]
        public virtual IHttpActionResult Get(string sort, int ascending)
        {
            if (GetRoles == string.Empty || User.IsInRole(GetRoles))
            {

                string orderByExpression = DefaultSort;

                if (sort != null && sort != string.Empty)
                    orderByExpression = $"{sort} {(ascending == 1 ? "ASC" : "DESC")}";

                try
                {
                    IEnumerable<T> result = QueryRecords(orderByExpression);

                    return Ok(JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpGet, Route("{parentID}/{sort}/{ascending:int}")]
        public virtual IHttpActionResult Get(string parentID, string sort, int ascending)
        {
            if (GetRoles == string.Empty || User.IsInRole(GetRoles))
            {

                string orderByExpression = DefaultSort;

                if (sort != null && sort != string.Empty)
                    orderByExpression = $"{sort} {(ascending == 1 ? "ASC" : "DESC")}";

                try
                {
                    IEnumerable<T> result;
                    if (HasParent && parentID != null)
                    {
                        PropertyInfo parentKey = typeof(T).GetProperty(ParentKey);
                        if (parentKey.PropertyType == typeof(int))
                            result = QueryRecords(orderByExpression, new RecordRestriction(ParentKey + " = {0}", int.Parse(parentID)));
                        else if (parentKey.PropertyType == typeof(Guid))
                            result = QueryRecords(orderByExpression, new RecordRestriction(ParentKey + " = {0}", Guid.Parse(parentID)));
                        else
                            result = QueryRecords(orderByExpression, new RecordRestriction(ParentKey + " = {0}", parentID));
                    }
                    else
                        result = QueryRecords(orderByExpression);

                    return Ok(JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPost, Route("Add")]
        public virtual IHttpActionResult Post([FromBody] JObject record)
        {
            try
            {
                if (PostRoles == string.Empty || User.IsInRole(PostRoles) && !ViewOnly)
                {
                    using (AdoDataConnection connection = new AdoDataConnection(Connection))
                    {

                        T newRecord = record.ToObject<T>();
                        int result = new TableOperations<T>(connection).AddNewRecord(newRecord);
                        if (HasUniqueKey)
                        {
                            PropertyInfo prop = typeof(T).GetProperty(UniqueKeyField);
                            if (prop != null)
                            {
                                object uniqueKey = prop.GetValue(newRecord);
                                newRecord = new TableOperations<T>(connection).QueryRecordWhere(UniqueKeyField + " = {0}", uniqueKey);
                                return Ok(newRecord);
                            }

                        }
                        return Ok(result);
                    }
                }
                else
                {
                    return Unauthorized();
                }

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPatch, Route("Update")]
        public virtual IHttpActionResult Patch([FromBody] T record)
        {
            try
            {
                if (PatchRoles == string.Empty || User.IsInRole(PatchRoles) && !ViewOnly && CustomView == string.Empty)
                {

                    using (AdoDataConnection connection = new AdoDataConnection(Connection))
                    {
                        int result = new TableOperations<T>(connection).AddNewOrUpdateRecord(record);
                        return Ok(result);
                    }
                }
                else
                {
                    return Unauthorized();
                }


            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete, Route("Delete")]
        public virtual IHttpActionResult Delete(T record)
        {
            try
            {
                if (DeleteRoles == string.Empty || User.IsInRole(DeleteRoles) && !ViewOnly && CustomView == string.Empty)
                {

                    using (AdoDataConnection connection = new AdoDataConnection(Connection))
                    {
                        TableNameAttribute tableNameAttribute;
                        string tableName;
                        if (typeof(T).TryGetAttribute(out tableNameAttribute))
                            tableName = tableNameAttribute.TableName;
                        else
                            tableName = typeof(T).Name;

                        PropertyInfo idProp = typeof(T).GetProperty(PrimaryKeyField);
                        if (idProp.PropertyType == typeof(int))
                        {
                            int id = (int)idProp.GetValue(record);
                            int result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete '{tableName}', '{PrimaryKeyField} = {id}'");
                            return Ok(result);

                        }
                        else if (idProp.PropertyType == typeof(Guid))
                        {
                            Guid id = (Guid)idProp.GetValue(record);
                            int result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete '{tableName}', '{PrimaryKeyField} = ''{id}'''");
                            return Ok(result);

                        }
                        else if (idProp.PropertyType == typeof(string))
                        {
                            string id = (string)idProp.GetValue(record);
                            int result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete '{tableName}', '{PrimaryKeyField} = ''{id}'''");
                            return Ok(result);

                        }
                        else
                        {
                            int result = new TableOperations<T>(connection).DeleteRecord(record);
                            return Ok(result);
                        }
                    }
                }
                else
                {
                    return Unauthorized();
                }

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("SearchableList")]
        public virtual IHttpActionResult GetSearchableList([FromBody] PostData postData)
        {
            if (!AllowSearch || (GetRoles != string.Empty && !User.IsInRole(GetRoles)))
                return Unauthorized();

            try
            {

                string whereClause = BuildWhereClause(postData.Searches);

                using (AdoDataConnection connection = new AdoDataConnection(Connection))
                {
                    string tableName = new TableOperations<T>(connection).TableName;

                    string sql = "";
                    if (CustomView == string.Empty)
                        sql = $@"
                    DECLARE @SQLStatement NVARCHAR(MAX) = N'
                        SELECT * FROM {tableName}
                        {whereClause.Replace("'", "''")}
                        ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")}
                    '
                    exec sp_executesql @SQLStatement";

                    else
                        sql = $@"
                    DECLARE @SQLStatement NVARCHAR(MAX) = N'
                        SELECT * FROM ({CustomView.Replace("'", "''")}) T1
                        {whereClause.Replace("'", "''")}
                        ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")}
                    '
                    exec sp_executesql @SQLStatement";
                    DataTable table = connection.RetrieveData(sql, "");

                    return Ok(JsonConvert.SerializeObject(table));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected class ExtDB
        {
            public string name;
            public DateTime lastupdate;
        }

        [HttpGet, Route("extDataBases")]
        public virtual IHttpActionResult GetExtendedDataBases()
        {

            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                try
                {
                    TableNameAttribute tableNameAttribute;
                    string tableName;
                    if (typeof(T).TryGetAttribute(out tableNameAttribute))
                        tableName = tableNameAttribute.TableName;
                    else
                        tableName = typeof(T).Name;

                    string query = @"SELECT MIN(UpdatedOn) AS lastUpdate, AdditionalField.ExternalDB AS name  
                                    FROM 
                                    AdditionalField LEFT JOIN AdditionalFieldValue ON AdditionalField.ID = AdditionalFieldValue.AdditionalFieldID
                                    WHERE AdditionalField.OpenXDAParentTable = {0} AND AdditionalField.ExternalDB IS NOT NULL AND AdditionalField.ExternalDB <> ''
                                    GROUP BY AdditionalField.ExternalDB";

                    DataTable table = connection.RetrieveData(query, tableName);

                    List<ExtDB> result = new List<ExtDB>();
                    foreach (DataRow row in table.Rows)
                    {
                        result.Add(new ExtDB() { name = row.ConvertField<string>("name"), lastupdate = row.ConvertField<DateTime>("lastUpdate") });
                    }

                    return Ok(result);

                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }

        [HttpPost, Route("ExtendedSearchableList")]
        public virtual IHttpActionResult GetExtendedSearchableList([FromBody] PostData postData)
        {
            if (!AllowSearch || (GetRoles != string.Empty && !User.IsInRole(GetRoles)))
                return Unauthorized();

            try
            {

                string whereClause = BuildWhereClause(postData.Searches);

                string pivotCollums = "(" + String.Join(",", postData.Searches.Where(item => item.isPivotColumn).Select(search => "'" + search.FieldName + "'")) + ")";

                if (pivotCollums == "()")
                    pivotCollums = "('')";

                using (AdoDataConnection connection = new AdoDataConnection(Connection))
                {
                    string tableName = new TableOperations<T>(connection).TableName;


                    string sql = "";

                    if (CustomView == string.Empty)
                        sql = $@"
                        DECLARE @PivotColumns NVARCHAR(MAX) = N''
                        SELECT @PivotColumns = @PivotColumns + '[AFV_' + t.FieldName + '],'
                            FROM (Select DISTINCT FieldName FROM [SystemCenter.AdditionalField] WHERE ParentTable = '{tableName}' AND FieldName IN {pivotCollums} ) AS t

                        DECLARE @SQLStatement NVARCHAR(MAX) = N'
                            SELECT * INTO #Tbl FROM (
                            SELECT 
                                M.*,
                                (CONCAT(''AFV_'',af.FieldName)) AS FieldName,
	                            afv.Value
                            FROM ( {tableName}  M LEFT JOIN 
                                [SystemCenter.AdditionalField] af on af.ParentTable = ''{tableName}'' AND af.FieldName IN {pivotCollums.Replace("'", "''")} LEFT JOIN
	                            [SystemCenter.AdditionalFieldValue] afv ON m.ID = afv.ParentTableID AND af.ID = afv.AdditionalFieldID
                            ) as T ' + (SELECT CASE WHEN Len(@PivotColumns) > 0 THEN 'PIVOT (
                                Max(T.Value) FOR T.FieldName IN ('+ SUBSTRING(@PivotColumns,0, LEN(@PivotColumns)) + ')) AS PVT' ELSE '' END) + ' 
                            {whereClause.Replace("'", "''")}
                            ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")};

                            DECLARE @NoNPivotColumns NVARCHAR(MAX) = N''''
                                SELECT @NoNPivotColumns = @NoNPivotColumns + ''[''+ name + ''],''
                                    FROM tempdb.sys.columns WHERE  object_id = Object_id(''tempdb..#Tbl'') AND name NOT LIKE ''AFV%''; 
		                    DECLARE @CleanSQL NVARCHAR(MAX) = N''SELECT '' + SUBSTRING(@NoNPivotColumns,0, LEN(@NoNPivotColumns)) + ''FROM #Tbl''

		                    exec sp_executesql @CleanSQL
                        '
                        exec sp_executesql @SQLStatement";

                    else
                        sql = $@"
                        DECLARE @PivotColumns NVARCHAR(MAX) = N''
                        SELECT @PivotColumns = @PivotColumns + '[AFV_' + t.FieldName + '],'
                            FROM (Select DISTINCT FieldName FROM [SystemCenter.AdditionalField] WHERE ParentTable = '{tableName}'  AND FieldName IN {pivotCollums}) AS t

                        DECLARE @SQLStatement NVARCHAR(MAX) = N'
                            SELECT * INTO #Tbl FROM (
                            SELECT 
                                M.*,
                                (CONCAT(''AFV_'',af.FieldName)) AS FieldName,
	                            afv.Value
                            FROM ({CustomView.Replace("'", "''")}) M LEFT JOIN 
                                [SystemCenter.AdditionalField] af on af.ParentTable = ''{tableName}'' AND af.FieldName IN {pivotCollums.Replace("'", "''")} LEFT JOIN
	                            [SystemCenter.AdditionalFieldValue] afv ON m.ID = afv.ParentTableID AND af.ID = afv.AdditionalFieldID
                            ) as T ' + (SELECT CASE WHEN Len(@PivotColumns) > 0 THEN 'PIVOT (
                                Max(T.Value) FOR T.FieldName IN ('+ SUBSTRING(@PivotColumns,0, LEN(@PivotColumns)) + ')) AS PVT' ELSE '' END) + '
                            {whereClause.Replace("'", "''")}
                            ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")};

                            DECLARE @NoNPivotColumns NVARCHAR(MAX) = N''''
                                SELECT @NoNPivotColumns = @NoNPivotColumns + ''[''+ name + ''],''
                                    FROM tempdb.sys.columns WHERE  object_id = Object_id(''tempdb..#Tbl'') AND name NOT LIKE ''AFV%''; 
		                    DECLARE @CleanSQL NVARCHAR(MAX) = N''SELECT '' + SUBSTRING(@NoNPivotColumns,0, LEN(@NoNPivotColumns)) + ''FROM #Tbl''

		                    exec sp_executesql @CleanSQL
                        '
                        exec sp_executesql @SQLStatement";

                    DataTable table = connection.RetrieveData(sql, "");

                    return Ok(JsonConvert.SerializeObject(table));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region [Helper Methods]

        protected string BuildWhereClause(IEnumerable<Search> searches)
        {

            string whereClause = string.Join(" AND ", searches.Select(search => {
                if (search.SearchText == string.Empty) search.SearchText = "%";
                else search.SearchText = search.SearchText.Replace("*", "%");

                if (search.Type == "string" || search.Type == "datetime")
                    search.SearchText = $"'{search.SearchText}'";
                else if (Array.IndexOf(new[] { "integer", "number", "boolean" }, search.Type) < 0)
                {
                    string text = search.SearchText.Replace("(", "").Replace(")", "");
                    List<string> things = text.Split(',').ToList();
                    things = things.Select(t => $"'{t}'").ToList();
                    search.SearchText = $"({string.Join(",", things)})";
                }

                return $"[{(search.isPivotColumn ? "AFV_" : "") + search.FieldName}] {search.Operator} {search.SearchText}";
            }));

            if (searches.Any())
                whereClause = "WHERE \n" + whereClause;

            return whereClause;
        }

        private IEnumerable<T> QueryRecordsWhere(string filterExpression, params object[] parameters)
        {
            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                    return new TableOperations<T>(connection).QueryRecordsWhere(filterExpression, parameters);

                string whereClause = " WHERE " + filterExpression;
                string sql = "SELECT * FROM (" + CustomView + ") T1";
                DataTable dataTbl = connection.RetrieveData(sql + whereClause, parameters);

                List<T> result = new List<T>();
                TableOperations<T> tblOperations = new TableOperations<T>(connection);
                foreach (DataRow row in dataTbl.Rows)
                {
                    result.Add(tblOperations.LoadRecord(row));
                }
                return result;
            }
        }

        private IEnumerable<T> QueryRecords() => QueryRecords(DefaultSort);

        private T QueryRecordWhere(string filterExpression, params object[] parameters)
        {
            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                    return new TableOperations<T>(connection).QueryRecordWhere(filterExpression, parameters);

                string whereClause = " WHERE " + filterExpression;
                string sql = "SELECT * FROM (" + CustomView + ") T1";
                DataTable dataTbl = connection.RetrieveData(sql + whereClause, parameters);

                TableOperations<T> tblOperations = new TableOperations<T>(connection);
                if (dataTbl.Rows.Count > 0)
                    return tblOperations.LoadRecord(dataTbl.Rows[0]);
                return null;

            }
        }

        private IEnumerable<T> QueryRecords(string orderBy)
        {
            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                    return new TableOperations<T>(connection).QueryRecords(orderBy);

                string sql = "SELECT * FROM (" + CustomView + ") T1";

                DataTable dataTbl = connection.RetrieveData(sql + "ORDER BY " + orderBy);

                List<T> result = new List<T>();
                TableOperations<T> tblOperations = new TableOperations<T>(connection);
                foreach (DataRow row in dataTbl.Rows)
                {
                    result.Add(tblOperations.LoadRecord(row));
                }
                return result;
            }
        }

        private IEnumerable<T> QueryRecords(string orderBy, RecordRestriction restriction)
        {
            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                    return new TableOperations<T>(connection).QueryRecords(orderBy, restriction);

                string restrictionString = "" + restriction.FilterExpression;

                string sql = "SELECT * FROM (" + CustomView + ") T1";

                DataTable dataTbl = connection.RetrieveData(sql + " WHERE " + restrictionString + "ORDER BY " + orderBy, restriction.Parameters);

                List<T> result = new List<T>();
                TableOperations<T> tblOperations = new TableOperations<T>(connection);
                foreach (DataRow row in dataTbl.Rows)
                {
                    result.Add(tblOperations.LoadRecord(row));
                }
                return result;
            }
        }
        #endregion
    }
}
