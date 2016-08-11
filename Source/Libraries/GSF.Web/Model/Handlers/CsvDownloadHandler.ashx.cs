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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using GSF.Security;
using GSF.Web.Hosting;
using System.Net.Http.Headers;
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
            NameValueCollection parameters = context.Request.QueryString;
            string modelName = parameters["ModelName"];
            string hubName = parameters["HubName"];

            response.ClearContent();
            response.Clear();
            response.AddHeader("Content-Type", CsvContentType);
            response.AddHeader("Content-Disposition", "attachment;filename=" + GetModelFileName(modelName));
            response.BufferOutput = true;

            CopyModelAsCsvToStreamAsync(modelName, hubName, response.OutputStream, () => !response.IsClientConnected).ContinueWith(task =>
            {
                if ((object)task.Exception != null)
                    throw task.Exception;
            }, 
            TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Enables processing of HTTP web requests by a custom handler that implements the <see cref="IHostedHttpHandler"/> interface.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="response">HTTP response message.</param>
        /// <param name="cancellationToken">Propagates notification from client that operations should be canceled.</param>
        public Task ProcessRequestAsync(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            NameValueCollection parameters = request.RequestUri.ParseQueryString();
            string modelName = parameters["ModelName"];
            string hubName = parameters["HubName"];

            response.Content = new PushStreamContent(async (stream, content, context) => await CopyModelAsCsvToStreamAsync(modelName, hubName, stream, () => cancellationToken.IsCancellationRequested), new MediaTypeHeaderValue(CsvContentType));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = GetModelFileName(modelName)
            };

            return Task.CompletedTask;
        }

        private string GetModelFileName(string modelName) => $"{modelName}Export.csv";

        private async Task CopyModelAsCsvToStreamAsync(string modelName, string hubName, Stream responseStream, Func<bool> isCancelled)
        {
            SecurityProviderCache.ValidateCurrentProvider();

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

            string queryRoles;

            try
            {
                using (IRecordOperationsHub hub = Activator.CreateInstance(hubType) as IRecordOperationsHub)
                {
                    if ((object)hub == null)
                        throw new SecurityException($"Cannot download CSV data: hub type \"{hubName}\" is not a IRecordOperationsHub, access cannot be validated.");

                    try
                    {
                        // Get any authorized query roles as defined in hub records operations for modeled table, default to read allowed for query
                        Tuple<string, string> recordOperation = hub.RecordOperationsCache.GetRecordOperations(modelType)[(int)RecordOperation.QueryRecords];
                        queryRoles = string.IsNullOrEmpty(recordOperation?.Item1) ? "*" : recordOperation.Item2 ?? "*";
                    }
                    catch (KeyNotFoundException ex)
                    {
                        throw new SecurityException($"Cannot download CSV data: hub type \"{hubName}\" does not define record operations for \"{modelName}\", access cannot be validated.", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SecurityException($"Cannot download CSV data: failed to instantiate hub type \"{hubName}\", access cannot be validated.", ex);
            }

            using (DataContext dataContext = new DataContext())
            using (StreamWriter writer = new StreamWriter(responseStream))
            {
                if (!dataContext.UserIsInRole(queryRoles))
                    throw new SecurityException($"Cannot download CSV data: access is denied for user \"{Thread.CurrentPrincipal.Identity.Name}\", minimum required roles = {queryRoles.ToDelimitedString(", ")}.");

                AdoDataConnection connection = dataContext.Connection;
                ITableOperations table = dataContext.Table(modelType);
                string[] fieldNames = table.GetFieldNames(false);

                // Write column headers
                await writer.WriteLineAsync(string.Join(",", fieldNames.Select(fieldName => connection.EscapeIdentifier(fieldName, true))));

                // Read queried records in page sets so there is not a memory burden and long initial query delay on very large data sets
                const int PageSize = 500;
                int recordCount = table.QueryRecordCount();
                int totalPages = Math.Max((int)Math.Ceiling(recordCount / (double)PageSize), 1);

                // Write data pages
                for (int page = 0; page < totalPages && !isCancelled(); page++)
                {
                    foreach (object record in table.QueryRecords(null, true, page + 1, PageSize))
                    {
                        if (isCancelled())
                            break;

                        await writer.WriteLineAsync(string.Join(",", fieldNames.Select(fieldName => $"\"{table.GetFieldValue(record, fieldName)}\"")));
                    }
                }
            }
        }

        #endregion
    }
}