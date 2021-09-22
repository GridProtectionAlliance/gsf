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
using System.Security.Claims;
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
        #region [ Members ]
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
            PrimaryKeyField = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<PrimaryKeyAttribute>().Any())?.Name ?? "ID";

            ParentKey = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<ParentKeyAttribute>().Any())?.Name ?? "";
            Connection = typeof(T).GetCustomAttribute<SettingsCategoryAttribute>()?.SettingsCategory ?? "systemSettings";

            PropertyInfo pi = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes<DefaultSortOrderAttribute>().Any());
            DefaultSortOrderAttribute dsoa = pi?.GetCustomAttribute<DefaultSortOrderAttribute>();
            if (dsoa != null)
                DefaultSort = $"{pi.Name} {(dsoa.Ascending ? "ASC" : "DESC")}";

            if (User.GetType() == typeof(ClaimsPrincipal))
            {
                SecurityType = "Claims";

                IEnumerable<ClaimAttribute> claimAttributes = typeof(T).GetCustomAttributes<ClaimAttribute>();

                foreach (ClaimAttribute claimAttribute in claimAttributes)
                {
                    if (Claims.ContainsKey(claimAttribute.Verb))
                        Claims[claimAttribute.Verb].Add(claimAttribute.Claim);
                    else
                        Claims.Add(claimAttribute.Verb, new List<Claim>() { claimAttribute.Claim });
                }

                if (!Claims.ContainsKey("POST"))
                    Claims.Add("POST", new List<Claim>() { new Claim("Role", "Administrator") });
                if (!Claims.ContainsKey("PATCH"))
                    Claims.Add("PATCH", new List<Claim>() { new Claim("Role", "Administrator") });
                if (!Claims.ContainsKey("DELETE"))
                    Claims.Add("DELETE", new List<Claim>() { new Claim("Role", "Administrator") });

            }
            else
            {

                SecurityType = "Roles";
                PostRoles = typeof(T).GetCustomAttribute<PostRolesAttribute>()?.Roles ?? "Administrator";
                GetRoles = typeof(T).GetCustomAttribute<GetRolesAttribute>()?.Roles ?? "";
                PatchRoles = typeof(T).GetCustomAttribute<PatchRolesAttribute>()?.Roles ?? "Administrator";
                DeleteRoles = typeof(T).GetCustomAttribute<DeleteRolesAttribute>()?.Roles ?? "Administrator";
            }
            CustomView = typeof(T).GetCustomAttribute<CustomViewAttribute>()?.CustomView ?? "";
            ViewOnly = typeof(T).GetCustomAttribute<ViewOnlyAttribute>()?.ViewOnly ?? false;
            AllowSearch = typeof(T).GetCustomAttribute<AllowSearchAttribute>()?.AllowSearch ?? false;

            SearchSettings = typeof(T).GetCustomAttribute<AdditionalFieldSearchAttribute>();
            Take = typeof(T).GetCustomAttribute<ReturnLimitAttribute>()?.Limit ?? null;

            // Custom View Models are ViewOnly.
            ViewOnly = ViewOnly || CustomView != String.Empty;

        }

        #endregion

        #region [ Properties ]
        protected bool ViewOnly { get; } = false;
        protected bool AllowSearch { get; } = false;
        protected string CustomView { get; } = "";
        protected string PrimaryKeyField { get; set; } = "ID";
        protected string ParentKey { get; set; } = "";
        protected string Connection { get; } = "systemSettings";
        protected string DefaultSort { get; } = null;
        protected string GetRoles { get; } = "";
        protected string PostRoles { get; } = "Administrator";
        protected string PatchRoles { get; } = "Administrator";
        protected string DeleteRoles { get; } = "Administrator";
        private int? Take { get; } = null;
        private string SecurityType = "";

        protected Dictionary<string, List<Claim>> Claims { get; } = new Dictionary<string, List<Claim>>();

        protected AdditionalFieldSearchAttribute SearchSettings { get; } = null;
        #endregion

        #region [ Http Methods ]
        /// <summary>
        /// Used to get an empty record
        /// </summary>
        /// <returns>An empty record</returns>
        [HttpGet, Route("New")]
        public virtual IHttpActionResult GetNew()
        {
            if (ViewOnly)
                return Unauthorized();

            if (GetAuthCheck())
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

        /// <summary>
        /// Gets all records from associated table, filtered to parent key ID if provided
        /// </summary>
        /// <param name="parentID">Parent ID to be used if Table has a set Parent Key</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>
        [HttpGet, Route("{parentID?}")]
        public virtual IHttpActionResult Get(string parentID = null)
        {
            if (GetAuthCheck())
            {

                try
                {
                    IEnumerable<T> result;
                    if (ParentKey != string.Empty && parentID != null)
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

        /// <summary>
        /// Gets record from associated table with a primary key matching the id provided
        /// </summary>
        /// <param name="id">ID to be used</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="T"/> or <see cref="Exception"/></returns>
        [HttpGet, Route("One/{id}")]
        public virtual IHttpActionResult GetOne(string id)
        {
            if (GetAuthCheck())
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
                        string tableName = TableOperations<T>.GetTableName();
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

        /// <summary>
        /// Gets a sorted list of <see cref="T"/>.
        /// </summary>
        /// <param name="sort"> the Field to be sorted by.</param>
        /// <param name="ascending"> parameter to indicate whether the list is in ascending order.</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>
        [HttpGet, Route("{sort}/{ascending:int}")]
        public virtual IHttpActionResult Get(string sort, int ascending)
        {
            if (GetAuthCheck())
            {

                try
                {
                    IEnumerable<T> result = QueryRecords();

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

        /// <summary>
        /// Gets all records from associated table, filtered to parent key ID if provided, sorted by the provided Field.
        /// </summary>
        /// <param name="parentID">Parent ID to be used if Table has a set Parent Key</param>
        /// <param name="sort"> the Field to be sorted by.</param>
        /// <param name="ascending"> parameter to indicate whether the list is in ascending order.</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>
        [HttpGet, Route("{parentID}/{sort}/{ascending:int}")]
        public virtual IHttpActionResult Get(string parentID, string sort, int ascending)
        {
            if (GetAuthCheck())
            {


                try
                {
                    IEnumerable<T> result;
                    if (ParentKey != string.Empty && parentID != null)
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


        /// <summary>
        /// Adds a new Record.
        /// </summary>
        /// <param name="record"> The <see cref="T"/> record to be added.</param>
        /// <returns><see cref="IHttpActionResult"/> containing the added record or <see cref="Exception"/> </returns>
        [HttpPost, Route("Add")]
        public virtual IHttpActionResult Post([FromBody] JObject record)
        {
            try
            {
                if (PostAuthCheck() && !ViewOnly)
                {
                    using (AdoDataConnection connection = new AdoDataConnection(Connection))
                    {

                        T newRecord = record.ToObject<T>();
                        int result = new TableOperations<T>(connection).AddNewRecord(newRecord);
                        
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

        /// <summary>
        /// Updates an existing Record.
        /// </summary>
        /// <param name="record"> The <see cref="T"/> record to be updated.</param>
        /// <returns><see cref="IHttpActionResult"/> containing the updated record or <see cref="Exception"/> </returns>

        [HttpPatch, Route("Update")]
        public virtual IHttpActionResult Patch([FromBody] T record)
        {
            try
            {
                if (PatchAuthCheck() && !ViewOnly)
                {

                    using (AdoDataConnection connection = new AdoDataConnection(Connection))
                    {
                        int result = new TableOperations<T>(connection).AddNewOrUpdateRecord(record);

                        if (PrimaryKeyField != string.Empty)
                        {
                            T newRecord = record;
                            PropertyInfo prop = typeof(T).GetProperty(PrimaryKeyField);
                            if (prop != null)
                            {
                                object uniqueKey = prop.GetValue(newRecord);
                                newRecord = new TableOperations<T>(connection).QueryRecordWhere(PrimaryKeyField + " = {0}", uniqueKey);
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

        /// <summary>
        /// Deletes an existing Record.
        /// </summary>
        /// <param name="record"> The <see cref="T"/> record to be deleted.</param>
        /// <returns><see cref="IHttpActionResult"/> containing the number of records deleted or <see cref="Exception"/> </returns>

        [HttpDelete, Route("Delete")]
        public virtual IHttpActionResult Delete(T record)
        {
            try
            {
                if (DeleteAuthCheck() && !ViewOnly)
                {

                    using (AdoDataConnection connection = new AdoDataConnection(Connection))
                    {
                        string tableName = new TableOperations<T>(connection).TableName;

                        PropertyInfo idProp = typeof(T).GetProperty(PrimaryKeyField);
                        if (idProp.PropertyType == typeof(int))
                        {
                            int id = (int)idProp.GetValue(record);
                            int result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete {tableName}, '{PrimaryKeyField} = {id}'");
                            return Ok(result);

                        }
                        else if (idProp.PropertyType == typeof(Guid))
                        {
                            Guid id = (Guid)idProp.GetValue(record);
                            int result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete {tableName}, '{PrimaryKeyField} = ''{id}'''");
                            return Ok(result);

                        }
                        else if (idProp.PropertyType == typeof(string))
                        {
                            string id = (string)idProp.GetValue(record);
                            int result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete {tableName}, '{PrimaryKeyField} = ''{id}'''");
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

        /// <summary>
        /// Gets all records from associated table, filtered and sorted as defined in <see cref="postData"/>.
        /// </summary>
        /// <param name="postData"><see cref="PostData"/> containing the search and sort parameters</param>
       /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>

        [HttpPost, Route("SearchableList")]
        public virtual IHttpActionResult GetSearchableList([FromBody] PostData postData)
        {
            if (GetAuthCheck() && !AllowSearch)
                return Unauthorized();

            try
            {

                string whereClause = BuildWhereClause(postData.Searches);

                using (AdoDataConnection connection = new AdoDataConnection(Connection))
                {
                    string tableName = TableOperations<T>.GetTableName();

                    string sql = "";

                    if (SearchSettings == null && CustomView == String.Empty)
                        sql = $@" SELECT * FROM {tableName} {whereClause}
                            ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")} ";

                    else if (SearchSettings == null)
                        sql = $@" SELECT* FROM({CustomView}) T1 
                         {whereClause}
                        ORDER BY {postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")}";

                    else
                    {
                        string pivotCollums = "(" + String.Join(",", postData.Searches.Where(item => item.isPivotColumn).Select(search => "'" + search.FieldName + "'")) + ")";

                        if (pivotCollums == "()")
                            pivotCollums = "('')";

                        string collumnCondition = SearchSettings.Condition;
                        if (collumnCondition != String.Empty)
                            collumnCondition = $"AF.{collumnCondition} AND ";
                        collumnCondition = collumnCondition + $"{SearchSettings.FieldKeyField} IN {pivotCollums}";

                        string joinCondition = $"af.FieldName IN {pivotCollums.Replace("'", "''")} AND ";
                        joinCondition = joinCondition + SearchSettings.Condition.Replace("'", "''");
                        if (SearchSettings.Condition != String.Empty)
                            joinCondition = $"{joinCondition} AND ";
                        joinCondition = joinCondition + $"SRC.{PrimaryKeyField} = AF.{SearchSettings.PrimaryKeyField}";

                        if (CustomView == String.Empty)
                            sql = $@"
                            DECLARE @PivotColumns NVARCHAR(MAX) = N''
                            SELECT @PivotColumns = @PivotColumns + '[AFV_' + [Key] + '],'
                                FROM (Select DISTINCT {SearchSettings.FieldKeyField} AS [Key] FROM {SearchSettings.AdditionalFieldTable} AS AF WHERE {collumnCondition}  ) AS [Fields]

                            DECLARE @SQLStatement NVARCHAR(MAX) = N'
                                SELECT * INTO #Tbl FROM (
                                SELECT 
                                    SRC.*,
                                    ''AFV_'' + AF.{SearchSettings.FieldKeyField} AS AFFieldKey,
	                                AF.{SearchSettings.ValueField} AS AFValue
                                FROM  {tableName} SRC LEFT JOIN 
                                    {SearchSettings.AdditionalFieldTable} AF ON {joinCondition}
                                ) as FullTbl ' + (SELECT CASE WHEN Len(@PivotColumns) > 0 THEN 'PIVOT (
                                    Max(FullTbl.AFValue) FOR FullTbl.AFFieldKey IN ('+ SUBSTRING(@PivotColumns,0, LEN(@PivotColumns)) + ')) AS PVT' ELSE '' END) + ' 
                                {whereClause.Replace("'", "''")};

                                DECLARE @NoNPivotColumns NVARCHAR(MAX) = N''''
                                    SELECT @NoNPivotColumns = @NoNPivotColumns + ''[''+ name + ''],''
                                        FROM tempdb.sys.columns WHERE  object_id = Object_id(''tempdb..#Tbl'') AND name NOT LIKE ''AFV%''; 
		                        DECLARE @CleanSQL NVARCHAR(MAX) = N''SELECT '' + SUBSTRING(@NoNPivotColumns,0, LEN(@NoNPivotColumns)) + ''FROM #Tbl ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")}''

		                        exec sp_executesql @CleanSQL
                            '
                            exec sp_executesql @SQLStatement";
                        else
                            sql = $@"
                            DECLARE @PivotColumns NVARCHAR(MAX) = N''
                            SELECT @PivotColumns = @PivotColumns + '[AFV_' + [Key] + '],'
                                FROM (Select DISTINCT {SearchSettings.FieldKeyField} AS [Key] FROM {SearchSettings.AdditionalFieldTable} AS AF WHERE {collumnCondition}  ) AS [Fields]

                            DECLARE @SQLStatement NVARCHAR(MAX) = N'
                                SELECT * INTO #Tbl FROM (
                                SELECT 
                                    SRC.*,
                                    ''AFV_'' + AF.{SearchSettings.FieldKeyField} AS AFFieldKey,
	                                AF.{SearchSettings.ValueField} AS AFValue
                                FROM  ({CustomView.Replace("'", "''")}) SRC LEFT JOIN 
                                    {SearchSettings.AdditionalFieldTable} AF ON {joinCondition}
                                ) as FullTbl ' + (SELECT CASE WHEN Len(@PivotColumns) > 0 THEN 'PIVOT (
                                    Max(FullTbl.AFValue) FOR FullTbl.AFFieldKey IN ('+ SUBSTRING(@PivotColumns,0, LEN(@PivotColumns)) + ')) AS PVT' ELSE '' END) + ' 
                                {whereClause.Replace("'", "''")};

                                DECLARE @NoNPivotColumns NVARCHAR(MAX) = N''''
                                    SELECT @NoNPivotColumns = @NoNPivotColumns + ''[''+ name + ''],''
                                        FROM tempdb.sys.columns WHERE  object_id = Object_id(''tempdb..#Tbl'') AND name NOT LIKE ''AFV%''; 
		                        DECLARE @CleanSQL NVARCHAR(MAX) = N''SELECT '' + SUBSTRING(@NoNPivotColumns,0, LEN(@NoNPivotColumns)) + ''FROM #Tbl ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")}''

		                        exec sp_executesql @CleanSQL
                            '
                            exec sp_executesql @SQLStatement";
                    }
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
                {
                    if(Take == null)
                        return new TableOperations<T>(connection).QueryRecords(DefaultSort, new RecordRestriction(filterExpression, parameters));
                    else
                        return new TableOperations<T>(connection).QueryRecords(DefaultSort, new RecordRestriction(filterExpression, parameters)).Take((int)Take);

                }
                string sql = $@"
                    SELECT * FROM 
                    ({CustomView}) T1 
                    WHERE {filterExpression}
                    {(DefaultSort != null ? " ORDER BY " +DefaultSort : "")}";
                DataTable dataTbl = connection.RetrieveData(sql, parameters);

                List<T> result = new List<T>();
                TableOperations<T> tblOperations = new TableOperations<T>(connection);
                foreach (DataRow row in dataTbl.Rows)
                {
                    result.Add(tblOperations.LoadRecord(row));
                }
                if (Take == null)
                    return result;
                else
                    return result.Take((int)Take);
            }
        }


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

        private IEnumerable<T> QueryRecords()
        {
            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                {
                    if (Take == null)
                        return new TableOperations<T>(connection).QueryRecords(DefaultSort);
                    else
                        return new TableOperations<T>(connection).QueryRecords(DefaultSort).Take((int)Take);


                }

                string sql = $@"
                    SELECT * FROM 
                    ({CustomView}) T1 
                    {(DefaultSort != null ? " ORDER BY " + DefaultSort : "")}";

                DataTable dataTbl = connection.RetrieveData(sql);

                List<T> result = new List<T>();
                TableOperations<T> tblOperations = new TableOperations<T>(connection);
                foreach (DataRow row in dataTbl.Rows)
                {
                    result.Add(tblOperations.LoadRecord(row));
                }
                if (Take == null)
                    return result;
                else
                    return result.Take((int)Take);
            }
        }

        private bool GetAuthCheck()
        {
            if (SecurityType == "Claims")
            {
                List<Claim> claims = Claims["GET"];
                ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)User;
                return claims.Count() == 0 || claimsPrincipal.HasClaim(claim => claims.Any(c => c.Type == claim.Type && c.Value == claim.Value));
            }
            else
                return GetRoles == string.Empty || User.IsInRole(GetRoles);
        }

        private bool PostAuthCheck()
        {
            if (SecurityType == "Claims")
            {
                List<Claim> claims = Claims["POST"];
                ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)User;
                return claims.Count() == 0 || claimsPrincipal.HasClaim(claim => claims.Any(c => c.Type == claim.Type && c.Value == claim.Value));
            }
            else
                return PostRoles == string.Empty || User.IsInRole(PostRoles);
        }

        private bool PatchAuthCheck()
        {
            if (SecurityType == "Claims")
            {
                List<Claim> claims = Claims["PATCH"];
                ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)User;
                return claims.Count() == 0 || claimsPrincipal.HasClaim(claim => claims.Any(c => c.Type == claim.Type && c.Value == claim.Value));
            }
            else
                return PatchRoles == string.Empty || User.IsInRole(PatchRoles);
        }

        private bool DeleteAuthCheck()
        {
            if (SecurityType == "Claims")
            {
                List<Claim> claims = Claims["DELETE"];
                ClaimsPrincipal claimsPrincipal = (ClaimsPrincipal)User;
                return claims.Count() == 0 || claimsPrincipal.HasClaim(claim => claims.Any(c => c.Type == claim.Type && c.Value == claim.Value));
            }
            else
                return DeleteRoles == string.Empty || User.IsInRole(DeleteRoles);
        }


        #endregion
    }
}
