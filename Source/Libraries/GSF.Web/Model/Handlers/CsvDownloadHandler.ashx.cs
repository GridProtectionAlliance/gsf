//******************************************************************************************************
//  CsvDownloadHandler.ashx.cs - Gbtc
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
//  07/29/2016 - Billy Ernest
//       Generated original version of source code.
//  08/10/2016 - J. Ritchie Carroll
//       Combined ASP.NET and self-hosted handlers into a single shared embedded resource.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using GSF.Security;
using GSF.Web.Hosting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GSF.Collections;
using GSF.Data;
using GSF.Data.Model;
using GSF.Reflection;

namespace GSF.Web.Model.Handlers
{
    /// <summary>
    /// Handles downloading of modeled table data as a comma-separated value file.
    /// </summary>
    public class CsvDownloadHandler : IHttpHandler, IHostedHttpHandler
    {
        #region [ Members ]

        // Constants
        private const string CsvContentType = "text/csv";

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="IHttpHandler"/> instance is reusable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReusable => false;

        /// <summary>
        /// Determines if client cache should be enabled for rendered handler content.
        /// </summary>
        /// <remarks>
        /// If rendered handler content does not change often, the server and client will use the
        /// <see cref="IHostedHttpHandler.GetContentHash"/> to determine if the client needs to refresh the content.
        /// </remarks>
        public bool UseClientCache => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets hash of response content based on any <paramref name="request"/> parameters.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <remarks>
        /// Value is only used when <see cref="IHostedHttpHandler.UseClientCache"/> is <c>true</c>.
        /// </remarks>
        public long GetContentHash(HttpRequestMessage request) => 0;

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            HttpResponse response = HttpContext.Current.Response;
            NameValueCollection requestParameters = context.Request.QueryString;

            response.ClearContent();
            response.Clear();
            response.AddHeader("Content-Type", CsvContentType);
            response.AddHeader("Content-Disposition", "attachment;filename=" + GetModelFileName(requestParameters["ModelName"]));
            response.BufferOutput = true;
            
            try
            {
                CopyModelAsCsvToStreamAsync(requestParameters, response.OutputStream, () => !response.IsClientConnected, () => response.FlushAsync()).ContinueWith(task =>
                {
                    if ((object)task.Exception != null)
                        throw task.Exception;
                },
                TaskContinuationOptions.OnlyOnFaulted);
            }
            finally
            {
                response.End();
            }
        }

        /// <summary>
        /// Enables processing of HTTP web requests by a custom handler that implements the <see cref="IHostedHttpHandler"/> interface.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="response">HTTP response message.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        public Task ProcessRequestAsync(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            NameValueCollection requestParameters = request.RequestUri.ParseQueryString();

            response.Content = new PushStreamContent(async (stream, content, context) => 
            {
                try
                {
                    await CopyModelAsCsvToStreamAsync(requestParameters, stream, () => cancellationToken.IsCancellationRequested);
                }
                finally
                {
                    stream.Close();
                }
            }, new MediaTypeHeaderValue(CsvContentType));

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = GetModelFileName(requestParameters["ModelName"])
            };

            return Task.CompletedTask;
        }

        private async Task CopyModelAsCsvToStreamAsync(NameValueCollection requestParameters, Stream responseStream, Func<bool> isCancelled, Func<Task> flushResponse = null)
        {
            SecurityProviderCache.ValidateCurrentProvider();

            string modelName = requestParameters["ModelName"];
            string hubName = requestParameters["HubName"];
            string filterText = requestParameters["FilterText"];
            string sortField = requestParameters["SortField"];
            bool sortAscending = requestParameters["SortAscending"].ParseBoolean();
            bool showDeleted = requestParameters["ShowDeleted"].ParseBoolean();
            string[] parentKeys = requestParameters["ParentKeys"].Split(',');
            const int PageSize = 250;

            if (string.IsNullOrEmpty(modelName))
                throw new ArgumentNullException(nameof(modelName), "Cannot download CSV data: no model type name was specified.");

            if (string.IsNullOrEmpty(hubName))
                throw new ArgumentNullException(nameof(hubName), "Cannot download CSV data: no hub type name was specified.");

            Type modelType = AssemblyInfo.FindType(modelName);

            if ((object)modelType == null)
                throw new InvalidOperationException($"Cannot download CSV data: failed to find model type \"{modelName}\" in loaded assemblies.");

            Type hubType = AssemblyInfo.FindType(hubName);

            if ((object)hubType == null)
                throw new InvalidOperationException($"Cannot download CSV data: failed to find hub type \"{hubName}\" in loaded assemblies.");

            IRecordOperationsHub hub;

            // Record operation tuple defines method name and allowed roles
            Tuple<string, string> queryRecordCountOperation;
            Tuple<string, string> queryRecordsOperation;
            string queryRoles;

            try
            {
                hub = Activator.CreateInstance(hubType) as IRecordOperationsHub;

                if ((object)hub == null)
                    throw new SecurityException($"Cannot download CSV data: hub type \"{hubName}\" is not a IRecordOperationsHub, access cannot be validated.");

                Tuple<string, string>[] recordOperations;

                try
                {
                    // Get any authorized query roles as defined in hub records operations for modeled table, default to read allowed for query
                    recordOperations = hub.RecordOperationsCache.GetRecordOperations(modelType);

                    if ((object)recordOperations == null)
                        throw new NullReferenceException();
                }
                catch (KeyNotFoundException ex)
                {
                    throw new SecurityException($"Cannot download CSV data: hub type \"{hubName}\" does not define record operations for \"{modelName}\", access cannot be validated.", ex);
                }

                // Get record operation for querying record count
                queryRecordCountOperation = recordOperations[(int)RecordOperation.QueryRecordCount];

                if ((object)queryRecordCountOperation == null)
                    throw new NullReferenceException();

                // Get record operation for querying records
                queryRecordsOperation = recordOperations[(int)RecordOperation.QueryRecords];

                if ((object)queryRecordsOperation == null)
                    throw new NullReferenceException();

                // Get any defined role restrictions for record query operation - access to CSV download will based on these roles
                queryRoles = string.IsNullOrEmpty(queryRecordsOperation.Item1) ? "*" : queryRecordsOperation.Item2 ?? "*";
            }
            catch (Exception ex)
            {
                throw new SecurityException($"Cannot download CSV data: failed to instantiate hub type \"{hubName}\" or access record operations, access cannot be validated.", ex);
            }

            using (DataContext dataContext = new DataContext())
            using (StreamWriter writer = new StreamWriter(responseStream))
            {
                // Validate current user has access to requested data
                if (!dataContext.UserIsInRole(queryRoles))
                    throw new SecurityException($"Cannot download CSV data: access is denied for user \"{Thread.CurrentPrincipal.Identity?.Name ?? "Undefined"}\", minimum required roles = {queryRoles.ToDelimitedString(", ")}.");

                AdoDataConnection connection = dataContext.Connection;
                ITableOperations table = dataContext.Table(modelType);
                string[] fieldNames = table.GetFieldNames(false);

                Func<Task> flushAsync = async () =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    await writer.FlushAsync();

                    if ((object)flushResponse != null)
                        await flushResponse();
                };

                // Write column headers
                await writer.WriteLineAsync(string.Join(",", fieldNames.Select(fieldName => connection.EscapeIdentifier(fieldName, true))));
                await flushAsync();

                // See if modeled table has a flag field that represents a deleted row
                bool hasDeletedField = !string.IsNullOrEmpty(dataContext.GetIsDeletedFlag(modelType));

                // Get query operation methods
                MethodInfo queryRecordCount = hubType.GetMethod(queryRecordCountOperation.Item1);
                MethodInfo queryRecords = hubType.GetMethod(queryRecordsOperation.Item1);

                // Setup query parameters
                List<object> queryRecordCountParameters = new List<object>();
                List<object> queryRecordsParameters = new List<object>();

                // Add current show deleted state parameter, if model defines a show deleted field
                if (hasDeletedField)
                    queryRecordCountParameters.Add(showDeleted);

                // Add any parent key restriction parameters
                if (parentKeys.Length > 0 && parentKeys[0].Length > 0)
                    queryRecordCountParameters.AddRange(parentKeys);

                // Add parameters for query records from query record count parameters - they match up to this point
                queryRecordsParameters.AddRange(queryRecordCountParameters);

                // Add sort field parameter
                queryRecordsParameters.Add(sortField);

                // Add ascending sort order parameter
                queryRecordsParameters.Add(sortAscending);

                // Track parameter index for current page to query
                int pageParameterIndex = queryRecordsParameters.Count;

                // Add page index parameter
                queryRecordsParameters.Add(0);

                // Add page size parameter
                queryRecordsParameters.Add(PageSize);

                // Add filter text parameter
                queryRecordCountParameters.Add(filterText);
                queryRecordsParameters.Add(filterText);

                // Read queried records in page sets so there is not a memory burden and long initial query delay on very large data sets
                int recordCount = (int)queryRecordCount.Invoke(hub, queryRecordCountParameters.ToArray());
                int totalPages = Math.Max((int)Math.Ceiling(recordCount / (double)PageSize), 1);

                // Write data pages
                for (int page = 0; page < totalPages && !isCancelled(); page++)
                {
                    // Update desired page to query
                    queryRecordsParameters[pageParameterIndex] = page + 1;

                    // Query page records
                    IEnumerable records = queryRecords.Invoke(hub, queryRecordsParameters.ToArray()) as IEnumerable ?? Enumerable.Empty<object>();
                    int exportCount = 0;

                    // Export page records
                    foreach (object record in records)
                    {
                        // Periodically check for client cancellation
                        if (exportCount++ % (PageSize / 4) == 0 && isCancelled())
                            break;

                        await writer.WriteLineAsync(string.Join(",", fieldNames.Select(fieldName => $"\"{table.GetFieldValue(record, fieldName)}\"")));
                    }

                    await flushAsync();
                }
            }
        }

        private static string GetModelFileName(string modelName)
        {
            int lastDotIndex = modelName.LastIndexOf('.');

            if (lastDotIndex > -1 && lastDotIndex < modelName.Length - 1)
                modelName = modelName.Substring(lastDotIndex + 1);

            return $"{modelName}Export.csv";
        }

        #endregion
    }
}