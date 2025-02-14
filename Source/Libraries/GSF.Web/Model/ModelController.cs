﻿//******************************************************************************************************
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS1591 // TODO: Fix missing XML comments for publicly visible types and members

namespace GSF.Web.Model
{
    /// <summary>
    /// Base Class for A Webcontroller that provides common Model endpoints such as Edit, Delete, Search.
    /// </summary>
    /// <typeparam name="T">The corresponding Model.</typeparam>
    public class ModelController<T> : ModelController<T, T> where T : class, new() { }

    /// <summary>
    /// Base Class for A Webcontroller that provides common Model endpoints such as Edit, Delete, Search.
    /// </summary>
    /// <typeparam name="T">The corresponding Model for Search/Fetch Results.</typeparam>
    /// <typeparam name="U">The corresponding Model for database editing.</typeparam>
    public class ModelController<T, U> : ApiController
        where T : class, U, new()
        where U : class, new()
    {
        #region [ Members ]

        public class PostData
        {
            public IEnumerable<SQLSearchFilter> Searches { get; set; }
            public string OrderBy { get; set; }
            public bool Ascending { get; set; }
        }

        public class PagedResults
        {
            public int RecordsPerPage { get; set; }
            public int NumberOfPages { get; set; }
            public int TotalRecords { get; set; }
            public string Data { get; set; }
        }

        #endregion

        #region [ Constructor ]

        /// <summary>
        /// Creates a new <see cref="ModelController{T,U}"/>
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

                IEnumerable<ClaimAttribute> claimViewAttributes = typeof(T).GetCustomAttributes<ClaimAttribute>();
                IEnumerable<ClaimAttribute> claimEditAttributes = typeof(U).GetCustomAttributes<ClaimAttribute>();

                foreach (ClaimAttribute claimAttribute in claimViewAttributes)
                {
                    if (claimAttribute.Verb != "GET") continue;
                    if (Claims.ContainsKey(claimAttribute.Verb))
                        Claims[claimAttribute.Verb].Add(claimAttribute.Claim);
                    else
                        Claims.Add(claimAttribute.Verb, new List<Claim>() { claimAttribute.Claim });
                }
                foreach (ClaimAttribute claimAttribute in claimEditAttributes)
                {
                    if (claimAttribute.Verb == "GET") continue;
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
                PostRoles = typeof(U).GetCustomAttribute<PostRolesAttribute>()?.Roles ?? "Administrator";
                GetRoles = typeof(T).GetCustomAttribute<GetRolesAttribute>()?.Roles ?? "";
                PatchRoles = typeof(U).GetCustomAttribute<PatchRolesAttribute>()?.Roles ?? "Administrator";
                DeleteRoles = typeof(U).GetCustomAttribute<DeleteRolesAttribute>()?.Roles ?? "Administrator";
            }
            CustomView = typeof(T).GetCustomAttribute<CustomViewAttribute>()?.CustomView ?? "";
            AllowSearch = typeof(T).GetCustomAttribute<AllowSearchAttribute>()?.AllowSearch ?? false;

            SearchSettings = typeof(T).GetCustomAttribute<AdditionalFieldSearchAttribute>();
            Take = typeof(T).GetCustomAttribute<ReturnLimitAttribute>()?.Limit ?? null;

            SQLSearchModifier = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(p => p.GetCustomAttributes<SQLSearchModifierAttribute>().Any());

            // Custom View Models are ViewOnly.
            ViewOnly = (typeof(U).GetCustomAttribute<ViewOnlyAttribute>()?.ViewOnly ?? false) ||
                (typeof(U).GetCustomAttribute<CustomViewAttribute>()?.CustomView ?? "") != "";

            RootQueryRestrictionAttribute rqra = typeof(T).GetCustomAttribute<RootQueryRestrictionAttribute>();
            if (rqra != null)
                RootQueryRestriction = new RecordRestriction(rqra.FilterExpression, rqra.Parameters.ToArray());
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
        protected RecordRestriction RootQueryRestriction { get; } = null;
        protected int? Take { get; } = null;

        private string SecurityType = "";
        protected Dictionary<string, List<Claim>> Claims { get; } = new Dictionary<string, List<Claim>>();
        protected AdditionalFieldSearchAttribute SearchSettings { get; } = null;
        protected MethodInfo SQLSearchModifier { get; } = null;

        #endregion

        #region [ Http Methods ]

        /// <summary>
        /// Used to get an empty record
        /// </summary>
        /// <returns>An empty record</returns>
        [HttpGet, Route("New")]
        public virtual IHttpActionResult GetNew()
        {
            if (ViewOnly || !GetAuthCheck())
                return Unauthorized();

            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                return Ok(new TableOperations<U>(connection).NewRecord());
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
            if (!GetAuthCheck())
                return Unauthorized();
            
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

        /// <summary>
        /// Gets record from associated table with a primary key matching the <paramref name="id"/> provided.
        /// </summary>
        /// <param name="id">ID to be used</param>
        /// <returns><see cref="IHttpActionResult"/> containing <typeparamref name="T"/> or <see cref="Exception"/></returns>
        [HttpGet, Route("One/{id}")]
        public virtual IHttpActionResult GetOne(string id)
        {
            if (!GetAuthCheck())
                return Unauthorized();
            
            T result;
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

        /// <summary>
        /// Gets a sorted list of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="sort"> the Field to be sorted by.</param>
        /// <param name="ascending"> parameter to indicate whether the list is in ascending order.</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>
        [HttpGet, Route("{sort}/{ascending:int}")]
        public virtual IHttpActionResult Get(string sort, int ascending)
        {
            if (!GetAuthCheck())
                return Unauthorized();
              
            IEnumerable<T> result = QueryRecords(sort,ascending > 0);
            return Ok(JsonConvert.SerializeObject(result));
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
            if (!GetAuthCheck())
                return Unauthorized();
        
            IEnumerable<T> result;
            if (ParentKey != string.Empty && parentID != null)
            {
                PropertyInfo parentKey = typeof(T).GetProperty(ParentKey);
                if (parentKey.PropertyType == typeof(int))
                    result = QueryRecordsWhere(sort, ascending > 0, ParentKey + " = {0}", int.Parse(parentID));
                else if (parentKey.PropertyType == typeof(Guid))
                    result = QueryRecordsWhere(sort, ascending > 0, ParentKey + " = {0}", Guid.Parse(parentID));
                else
                    result = QueryRecordsWhere(sort, ascending > 0, ParentKey + " = {0}", parentID);
            }
            else
                result = QueryRecords(sort,ascending > 0);

            return Ok(JsonConvert.SerializeObject(result));
        }


        /// <summary>
        /// Adds a new Record.
        /// </summary>
        /// <param name="record"> The <typeparamref name="U"/> record to be added.</param>
        /// <returns><see cref="IHttpActionResult"/> containing the added record or <see cref="Exception"/> </returns>
        [HttpPost, Route("Add")]
        public virtual IHttpActionResult Post([FromBody] JObject record)
        {
            if (!PostAuthCheck() || ViewOnly)
                return Unauthorized();
                
            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                U newRecord = record.ToObject<U>();
                int result = new TableOperations<U>(connection).AddNewRecord(newRecord);
                return Ok(result);
            }
        }

        /// <summary>
        /// Updates an existing Record.
        /// </summary>
        /// <param name="record"> The <typeparamref name="U"/> record to be updated.</param>
        /// <returns><see cref="IHttpActionResult"/> containing the updated record or <see cref="Exception"/> </returns>

        [HttpPatch, Route("Update")]
        public virtual IHttpActionResult Patch([FromBody] U record)
        {
            if (!PatchAuthCheck() || ViewOnly)
                return Unauthorized();

            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                int result = new TableOperations<U>(connection).AddNewOrUpdateRecord(record);

                if (PrimaryKeyField != string.Empty)
                {
                    U newRecord = record;
                    PropertyInfo prop = typeof(U).GetProperty(PrimaryKeyField);
                    if (prop != null)
                    {
                        object uniqueKey = prop.GetValue(newRecord);
                        newRecord = new TableOperations<U>(connection).QueryRecordWhere(PrimaryKeyField + " = {0}", uniqueKey);
                        return Ok(newRecord);
                    }
                }

                return Ok(result);
            }
        }

        /// <summary>
        /// Deletes an existing Record.
        /// </summary>
        /// <param name="record"> The <typeparamref name="U"/> record to be deleted.</param>
        /// <returns><see cref="IHttpActionResult"/> containing the number of records deleted or <see cref="Exception"/> </returns>

        [HttpDelete, Route("Delete")]
        public virtual IHttpActionResult Delete(U record)
        {
            if (!DeleteAuthCheck() || ViewOnly)
                return Unauthorized();

            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                string tableName = new TableOperations<U>(connection).TableName;

                PropertyInfo idProp = typeof(U).GetProperty(PrimaryKeyField);
                int result;

                if (idProp.PropertyType == typeof(int))
                {
                    int id = (int)idProp.GetValue(record);
                    result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete {tableName}, '{PrimaryKeyField} = {id}'");
                }
                else if (idProp.PropertyType == typeof(Guid))
                {
                    Guid id = (Guid)idProp.GetValue(record);
                    result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete {tableName}, '{PrimaryKeyField} = ''{id}'''");
                }
                else if (idProp.PropertyType == typeof(string))
                {
                    string id = (string)idProp.GetValue(record);
                    result = connection.ExecuteNonQuery($"EXEC UniversalCascadeDelete {tableName}, '{PrimaryKeyField} = ''{id}'''");
                }
                else
                    result = new TableOperations<U>(connection).DeleteRecord(record);
                
                return Ok(result);
            }
        }

        /// <summary>
        /// Gets all records from associated table, filtered and sorted as defined in <paramref name="postData"/>.
        /// </summary>
        /// <param name="postData"><see cref="PostData"/> containing the search and sort parameters</param>
       /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>

        [HttpPost, Route("SearchableList")]
        public virtual IHttpActionResult GetSearchableList([FromBody] PostData postData)
        {
            if (!GetAuthCheck() || !AllowSearch)
                return Unauthorized();

            DataTable table = GetSearchResults(postData);
            return Ok(JsonConvert.SerializeObject(table));         
        }

        /// <summary>
        /// Gets a subset of records from associated table, filtered and sorted as defined in <paramref name="postData"/>.
        /// based on <see cref="Take" /> and 0-based <paramref name="page"/> index
        /// </summary>
        /// <param name="postData"><see cref="PostData"/> containing the search and sort parameters</param>
        /// <param name="page">The 0-based index of the page to be retrieved</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>
        [HttpPost, Route("PagedList/{page}")]
        public virtual IHttpActionResult GetPagedList([FromBody] PostData postData, int page)
        {
            if (!GetAuthCheck())
                return Unauthorized();

            if (!AllowSearch)
                postData.Searches = new List<SQLSearchFilter>();

            using DataTable table = GetSearchResults(postData, page);
            int recordCount = CountSearchResults(postData);
            int recordsPerPage = Take ?? 50;

            return Ok(new PagedResults()
            {
                Data = JsonConvert.SerializeObject(table),
                RecordsPerPage = recordsPerPage,
                TotalRecords = recordCount,
                NumberOfPages = (recordCount + recordsPerPage - 1) / recordsPerPage
            });
        }

        /// <summary>
        /// Gets all records from associated table, filtered and sorted as defined in <paramref name="postData"/>.
        /// </summary>
        /// <param name="parentID">Parent ID to be used if Table has a set Parent Key</param>
        /// <param name="postData"><see cref="PostData"/> containing the search and sort parameters</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>

        [HttpPost, Route("{parentID?}/SearchableList")]
        public virtual IHttpActionResult GetSearchableList([FromBody] PostData postData, string parentID = null)
        {
            if (!GetAuthCheck() || !AllowSearch)
                return Unauthorized();

            if (ParentKey != string.Empty && parentID != null)
            {
                List<SQLSearchFilter> searches = postData.Searches.ToList();
                PropertyInfo parentKey = typeof(T).GetProperty(ParentKey);
                if (parentKey.PropertyType == typeof(int))
                    searches.Add(new SQLSearchFilter() { FieldName = ParentKey, IsPivotColumn = false, Operator = "=", Type = "number", SearchText = parentID });
                else 
                    searches.Add(new SQLSearchFilter() { FieldName = ParentKey, IsPivotColumn = false, Operator = "=", Type = "string", SearchText = parentID });

                postData.Searches = searches;
            }

            DataTable table = GetSearchResults(postData);
            return Ok(JsonConvert.SerializeObject(table));
        }

        /// <summary>
        /// Gets a subset of records from associated table, filtered and sorted as defined in <paramref name="postData"/>.
        /// based on <see cref="Take" /> and 0-based <paramref name="page"/> index
        /// </summary>
        /// <param name="postData"><see cref="PostData"/> containing the search and sort parameters</param>
        /// <param name="page">The 0-based index of the page to be retrieved</param>
        /// <param name="parentID"> Parent ID to be used if Table has a set Parent Key</param>
        /// <returns><see cref="IHttpActionResult"/> containing <see cref="IEnumerable{T}"/> or <see cref="Exception"/></returns>
        [HttpPost, Route("{parentID?}/PagedList/{page}")]
        public virtual IHttpActionResult GetPagedList([FromBody] PostData postData, int page, string parentID = null)
        {
            if (!GetAuthCheck())
                return Unauthorized();

            if (!AllowSearch)
                postData.Searches = new List<SQLSearchFilter>();

            if (ParentKey != string.Empty && parentID != null)
            {
                List<SQLSearchFilter> searches = postData.Searches.ToList();
                PropertyInfo parentKey = typeof(T).GetProperty(ParentKey);
                if (parentKey.PropertyType == typeof(int))
                    searches.Add(new SQLSearchFilter() { FieldName = ParentKey, IsPivotColumn = false, Operator = "=", Type = "number", SearchText = parentID });
                else
                    searches.Add(new SQLSearchFilter() { FieldName = ParentKey, IsPivotColumn = false, Operator = "=", Type = "string", SearchText = parentID });

                postData.Searches = searches;
            }

            using DataTable table = GetSearchResults(postData, page);
            int recordCount = CountSearchResults(postData);
            int recordsPerPage = Take ?? 50;

            return Ok(new PagedResults()
            {
                Data = JsonConvert.SerializeObject(table),
                RecordsPerPage = recordsPerPage,
                TotalRecords = recordCount,
                NumberOfPages = (recordCount + recordsPerPage - 1) / recordsPerPage
            });
        }

        #endregion

        #region [Helper Methods]

        protected string BuildWhereClause(IEnumerable<SQLSearchFilter> searches, List<object> parameters)
        {
            List<string> clauses = new();

            foreach (SQLSearchFilter search in searches)
            {
                if (search.Operator == "LIKE" || search.Operator == "NOT LIKE")
                {
                    if (search.SearchText == string.Empty)
                        search.SearchText = "%";
                    else
                        search.SearchText = search.SearchText.Replace("*", "%");
                }

                if (SQLSearchModifier is not null)
                    clauses.Add((string)SQLSearchModifier.Invoke(null, new object[] { search, parameters }));
                else
                    clauses.Add(search.GenerateConditional(parameters));
            }

            return string.Join(" AND ", clauses);
        }

        protected virtual IEnumerable<T> QueryRecordsWhere(string filterExpression, params object[] parameters) => QueryRecordsWhere(null, false, filterExpression, parameters);
        
        protected virtual IEnumerable<T> QueryRecordsWhere(string orderBy, bool ascending, string filterExpression, params object[] parameters)
        {
            string orderString = "";
            if (!string.IsNullOrEmpty(orderBy))
                orderString = orderBy + (ascending ? " ASC" : " DESC");
            if (string.IsNullOrEmpty(orderBy) && !string.IsNullOrEmpty(DefaultSort))
                orderString = DefaultSort;

            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                {
                    if(Take == null)
                        return new TableOperations<T>(connection).QueryRecords(orderString, new RecordRestriction(filterExpression, parameters));
                    else
                        return new TableOperations<T>(connection).QueryRecords(orderString, new RecordRestriction(filterExpression, parameters)).Take((int)Take);

                }

                string flt = filterExpression;
                object[] param = parameters;

                if (RootQueryRestriction != null)
                {
                    flt = RootQueryRestriction.FilterExpression + " AND " + filterExpression;
                    param = RootQueryRestriction.Parameters.Concat(parameters).ToArray();
                }
                    
                string sql = $@"
                    SELECT * FROM 
                    ({CustomView}) FullTbl 
                    WHERE {flt}
                    {(orderBy != null ? " ORDER BY " + orderString : "")}";

                DataTable dataTbl;
                if (orderBy != null) dataTbl = connection.RetrieveData("SELECT * FROM ({0}) FullTbl WHERE {1} ORDER BY {2}", CustomView, flt, orderString, param);
                else dataTbl = connection.RetrieveData("SELECT * FROM ({0}) FullTbl WHERE {1}", CustomView, flt, param);

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

        protected virtual T QueryRecordWhere(string filterExpression, params object[] parameters)
        {
            
            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                    return new TableOperations<T>(connection).QueryRecordWhere(filterExpression, parameters);

                string whereClause = filterExpression;
                object[] param = parameters;

                if (RootQueryRestriction != null)
                {
                    whereClause = RootQueryRestriction.FilterExpression + " AND " + filterExpression;
                    param = RootQueryRestriction.Parameters.Concat(parameters).ToArray();
                }

                whereClause = " WHERE " + whereClause;
                DataTable dataTbl = connection.RetrieveData("SELECT * FROM (0) FullTbl{1}", CustomView, whereClause, param);

                TableOperations<T> tblOperations = new TableOperations<T>(connection);
                if (dataTbl.Rows.Count > 0)
                    return tblOperations.LoadRecord(dataTbl.Rows[0]);
                return null;

            }
        }

        protected virtual IEnumerable<T> QueryRecords() => QueryRecords(null, false);
    
        protected virtual IEnumerable<T> QueryRecords(string sortBy, bool ascending)
        {
            string orderString = "";
            if (!string.IsNullOrEmpty(sortBy))
                orderString = sortBy + (ascending ? " ASC" : " DESC");
            if (string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(DefaultSort))
                orderString = DefaultSort;

            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                if (CustomView == String.Empty)
                {
                    if (Take == null)
                        return new TableOperations<T>(connection).QueryRecords(orderString);
                    else
                        return new TableOperations<T>(connection).QueryRecords(orderString).Take((int)Take);
                }

                string sql = $@"
                    SELECT * FROM 
                    ({CustomView}) FullTbl 
                    {(sortBy != null ? " ORDER BY " + orderString : "")}";

                DataTable dataTbl;
                if (RootQueryRestriction != null)
                {
                    if (sortBy != null) dataTbl = connection.RetrieveData("SELECT * FROM ({0}) FullTbl WHERE ({1}) ORDER BY {2}",
                        CustomView, RootQueryRestriction.FilterExpression, orderString, RootQueryRestriction.Parameters);
                    else dataTbl = connection.RetrieveData("SELECT * FROM ({0}) FullTbl WHERE ({1})", 
                        CustomView, RootQueryRestriction.FilterExpression, RootQueryRestriction.Parameters);
                }
                else
                    if (sortBy != null) dataTbl = connection.RetrieveData("SELECT * FROM ({0}) FullTbl ORDER BY {1}",
                        CustomView, orderString);
                    else dataTbl = connection.RetrieveData("SELECT * FROM ({0}) FullTbl WHERE ({1})",
                        CustomView);

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

        /// <summary>
        /// Check if current User is authorized for GET Requests
        /// </summary>
        /// <returns>True if User is authorized for GET requests</returns>
        protected bool GetAuthCheck()
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

        /// <summary>
        /// Check if current User is authorized for POST Requests
        /// </summary>
        /// <returns>True if User is authorized for POST requests</returns>
        protected bool PostAuthCheck()
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

        /// <summary>
        /// Check if current User is authorized for PATCH Requests
        /// </summary>
        /// <returns>True if User is authorized for PATCH requests</returns>
        protected bool PatchAuthCheck()
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

        /// <summary>
        /// Check if current User is authorized for DELETE Requests
        /// </summary>
        /// <returns>True if User is authorized for DELETE requests</returns>
        protected bool DeleteAuthCheck()
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

        /// <summary>
        /// Gets the <see cref="DataTable"/> with the SearchResults as specified in <see cref="PostData"/>. 
        /// </summary>
        /// <param name="postData"><see cref="PostData"/> containing the search and sort parameters</param>
        /// <param name="page">The 0-based index of the page to be retrieved</param>
        /// <returns>A <see cref="DataTable"/> containing the results of the search.</returns>
        protected virtual DataTable GetSearchResults(PostData postData, int? page = null)
        {
            string whereClause = "";
            List<object> param = new();

            if (RootQueryRestriction != null)
            {
                whereClause = $"WHERE {RootQueryRestriction.FilterExpression}";
                param = RootQueryRestriction.Parameters.ToList();
            }

            string conditions = BuildWhereClause(postData.Searches, param);
            if (string.IsNullOrEmpty(whereClause) && !string.IsNullOrEmpty(conditions))
                whereClause = $"WHERE {conditions}";
            else if (!string.IsNullOrEmpty(conditions))
                whereClause += $" AND {conditions}";

            using (AdoDataConnection connection = new AdoDataConnection(Connection))
            {
                string tableName = TableOperations<T>.GetTableName();

                string sql = "";

                string limit;

                if (Take is null || page is not null)
                    limit = "";
                else
                    limit = $"TOP {(int)Take}";

                if (SearchSettings == null && CustomView == String.Empty)
                    sql = $@" SELECT {limit} * FROM {tableName} FullTbl {whereClause}
                        ORDER BY { postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")} ";
                else if (SearchSettings == null)
                    sql = $@" SELECT {limit} * FROM({CustomView}) FullTbl 
                        {whereClause}
                        ORDER BY {postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")}";
                else
                {
                    string pivotCollums = "(" + String.Join(",", postData.Searches.Where(item => item.IsPivotColumn).Select(search => "'" + search.FieldName + "'")) + ")";

                    if (pivotCollums == "()")
                        pivotCollums = "('')";

                    string collumnCondition = SearchSettings.Condition;
                    if (collumnCondition != String.Empty)
                        collumnCondition = $"({collumnCondition}) AND ";
                    collumnCondition = collumnCondition + $"{SearchSettings.FieldKeyField} IN {pivotCollums}";

                    string searchSettingConditions = SearchSettings.Condition;
                    if (searchSettingConditions != String.Empty)
                        searchSettingConditions = "(" + searchSettingConditions + ")";

                    string joinCondition = $"af.FieldName IN {pivotCollums} AND ";
                    joinCondition = joinCondition + searchSettingConditions;
                    if (SearchSettings.Condition != String.Empty)
                        joinCondition = $"{joinCondition} AND ";
                    joinCondition = joinCondition + $"SRC.{PrimaryKeyField} = AF.{SearchSettings.PrimaryKeyField}";
                    
                    string sqlPivotColumns = $@"
                        SELECT '[AFV_' + [Key] + ']'
                        FROM (Select DISTINCT {SearchSettings.FieldKeyField} AS [Key] 
                        FROM {SearchSettings.AdditionalFieldTable} AS AF WHERE {collumnCondition}  ) AS [Fields]";
                    sqlPivotColumns = string.Join(",", connection.RetrieveData(sqlPivotColumns).Select().Select(r => r[0].ToString()));
                    string tblSelect = $@"
                        (SELECT 
                            SRC.*,
                            'AFV_' + AF.{SearchSettings.FieldKeyField} AS AFFieldKey,
                            AF.{SearchSettings.ValueField} AS AFValue
                        FROM  {(string.IsNullOrEmpty(CustomView) ? tableName : $"({CustomView})")} SRC LEFT JOIN
                            {SearchSettings.AdditionalFieldTable} AF ON {joinCondition}
                        ) as FullTbl {(string.IsNullOrEmpty(sqlPivotColumns) ? "" : $"PIVOT (Max(FullTbl.AFValue) FOR FullTbl.AFFieldKey IN ({sqlPivotColumns})) AS FullTbl")}";
                    string sqlNoPivot = $@"
                        SELECT TOP 0 * INTO #Tbl FROM {tblSelect}
                        SELECT '[' + name + ']'
                                FROM tempdb.sys.columns WHERE  object_id = Object_id('tempdb..#Tbl') AND name NOT LIKE 'AFV%';";
                    sqlNoPivot = string.Join(",", connection.RetrieveData(sqlNoPivot).Select().Select(r => r[0].ToString()));
                    sql = $@"
                        SELECT {limit} {sqlNoPivot} FROM {tblSelect}
                        {whereClause}
                        ORDER BY {postData.OrderBy} {(postData.Ascending ? "ASC" : "DESC")}";
                }

                if (page is not null)
                {
                    int recordsPerPage = Take ?? 50;
                    sql += $" OFFSET {page * recordsPerPage} ROWS FETCH NEXT {recordsPerPage} ROWS ONLY";
                }

                object[] paramArray = param.ToArray();
                if (param.Count() > 0)
                    return connection.RetrieveData(sql, paramArray);
                return connection.RetrieveData(sql,"");
            }
        }

        /// <summary>
        /// Counts the number of records with the SearchResults as specified in <see cref="PostData"/>.
        /// </summary>
        /// <param name="postData"><see cref="PostData"/> containing the search and sort parameters</param>
        /// <returns>The number of records that match the search filters.</returns>
        protected virtual int CountSearchResults(PostData postData)
        {
            string whereClause = "";
            List<object> param = new();

            if (RootQueryRestriction is not null)
            {
                whereClause = $"WHERE {RootQueryRestriction.FilterExpression}";
                param = RootQueryRestriction.Parameters.ToList();
            }

            string conditions = BuildWhereClause(postData.Searches, param);
            if (string.IsNullOrEmpty(whereClause) && !string.IsNullOrEmpty(conditions))
                whereClause = $"WHERE {conditions}";
            else if (!string.IsNullOrEmpty(conditions))
                whereClause += $" AND {conditions}";

            using AdoDataConnection connection = new(Connection);
            string tableName = TableOperations<T>.GetTableName();

            string sql = "";

            if (SearchSettings is null && CustomView == string.Empty)
                sql = $@"SELECT COUNT(*) FROM {tableName} FullTbl {whereClause}";
            else if (SearchSettings is null)
                sql = $@"SELECT COUNT(*) FROM ({CustomView}) FullTbl {whereClause}";
            else
            {
                string pivotColumns = "(" + string.Join(",", postData.Searches.Where(item => item.IsPivotColumn).Select(search => "'" + search.FieldName + "'")) + ")";

                if (pivotColumns == "()")
                    pivotColumns = "('')";

                string columnCondition = SearchSettings.Condition;
                if (columnCondition != string.Empty)
                    columnCondition = $"({columnCondition}) AND ";
                columnCondition += $"{SearchSettings.FieldKeyField} IN {pivotColumns}";

                string searchSettingConditions = SearchSettings.Condition;
                if (searchSettingConditions != string.Empty)
                    searchSettingConditions = "(" + searchSettingConditions + ")";

                string joinCondition = $"af.FieldName IN {pivotColumns} AND ";
                joinCondition += searchSettingConditions;
                if (SearchSettings.Condition != string.Empty)
                    joinCondition = $"{joinCondition} AND ";
                joinCondition += $"SRC.{PrimaryKeyField} = AF.{SearchSettings.PrimaryKeyField}";

                string sqlPivotColumns = $@"
                        SELECT '[AFV_' + [Key] + ']'
                        FROM (SELECT DISTINCT {SearchSettings.FieldKeyField} AS [Key] FROM {SearchSettings.AdditionalFieldTable} AS AF WHERE {columnCondition}) AS [Fields]";
                sqlPivotColumns = string.Join(",", connection.RetrieveData(sqlPivotColumns).Select().Select(r => r[0].ToString()));
                string tblSelect = $@"
                        (SELECT
                            SRC.*,
                            'AFV_' + AF.{SearchSettings.FieldKeyField} AS AFFieldKey,
                            AF.{SearchSettings.ValueField} AS AFValue
                        FROM {(string.IsNullOrEmpty(CustomView) ? tableName : $"({CustomView})")} SRC LEFT JOIN
                            {SearchSettings.AdditionalFieldTable} AF ON {joinCondition}
                        ) as FullTbl {(string.IsNullOrEmpty(sqlPivotColumns) ? "" : $"PIVOT (Max(FullTbl.AFValue) FOR FullTbl.AFFieldKey IN ({sqlPivotColumns})) AS FullTbl")}";

                sql = $"SELECT COUNT(*) FROM {tblSelect} {whereClause}";
            }

            object[] paramArray = param.ToArray();
            if (paramArray.Any())
                return connection.ExecuteScalar<int>(sql, paramArray);
            return connection.ExecuteScalar<int>(sql, "");
        }

        #endregion
    }
}
