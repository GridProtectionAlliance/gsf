//******************************************************************************************************
//  IGrafanaDataService.cs - Gbtc
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
//  09/12/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace GrafanaAdapters
{
    /// <summary>
    /// Defines needed API calls for a Grafana data source.
    /// </summary>
    [ServiceContract]
    public partial interface IGrafanaDataService
    {
        /// <summary>
        /// Validates that openHistorian Grafana data source is responding as expected.
        /// </summary>
        [OperationContract, WebInvoke(UriTemplate = "/", Method ="GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void TestDataSource();

        /// <summary>
        /// Queries openHistorian as a Grafana data source.
        /// </summary>
        /// <param name="request">Query request.</param>
        [OperationContract, WebInvoke(UriTemplate = "/query", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<List<TimeSeriesValues>> Query(QueryRequest request);

        /// <summary>
        /// Queries openPDC alarm states as a Grafana data source.
        /// </summary>
        /// <param name="request">Query request.</param>
        [OperationContract, WebInvoke(UriTemplate = "/GetAlarmState", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<IEnumerable<AlarmDeviceStateView>> GetAlarmState(QueryRequest request);

        /// <summary>
        /// Queries openHistorian as a Grafana Metadata source.
        /// </summary>
        /// <param name="request">Query request.</param>
        [OperationContract, WebInvoke(UriTemplate = "/getmetadata", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<string> GetMetadata(Target request);

        /// <summary>
        /// Search openHistorian for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        [OperationContract, WebInvoke(UriTemplate = "/search", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<string[]> Search(Target request);

        /// <summary>
        /// Search data source for a list of columns from a specific table.
        /// </summary>
        /// <param name="request">Table Name.</param>
        [OperationContract, WebInvoke(UriTemplate = "/searchfields", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<string[]> SearchFields(Target request);

        /// <summary>
        /// Search data source for a list of tables.
        /// </summary>
        /// <param name="request">Request.</param>
        [OperationContract, WebInvoke(UriTemplate = "/searchfilters", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<string[]> SearchFilters(Target request);

        /// <summary>
        /// Search data source for a list of columns from a specific table to use for ORDER BY expression.
        /// </summary>
        /// <param name="request">Table Name.</param>
        [OperationContract, WebInvoke(UriTemplate = "/searchorderbys", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<string[]> SearchOrderBys(Target request);

        /// <summary>
        /// Queries openHistorian for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        [OperationContract, WebInvoke(UriTemplate = "/annotations", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<List<AnnotationResponse>> Annotations(AnnotationRequest request);

        /// <summary>
        /// Requests available tag keys.
        /// </summary>
        /// <param name="_">Tag keys request.</param>
        [OperationContract, WebInvoke(UriTemplate = "/tag-keys", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<TagKeysResponse[]> TagKeys(TagKeysRequest _);

        /// <summary>
        /// Requests available tag values.
        /// </summary>
        /// <param name="request">Tag values request.</param>
        [OperationContract, WebInvoke(UriTemplate = "/tag-values", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<TagValuesResponse[]> TagValues(TagValuesRequest request);
    }
}
