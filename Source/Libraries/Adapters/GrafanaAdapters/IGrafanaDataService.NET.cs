//******************************************************************************************************
//  IGrafanaDataService.NET.cs - Gbtc
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

#if !MONO

using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using GrafanaAdapters.Model.Annotations;
using GrafanaAdapters.Model.Common;
using GrafanaAdapters.Model.Database;
using GrafanaAdapters.Model.Functions;
using GrafanaAdapters.Model.MetaData;

namespace GrafanaAdapters;

/// <summary>
/// Defines needed API calls for a Grafana data source.
/// </summary>
// .NET Implementation
public partial interface IGrafanaDataService
{
    /// <summary>
    /// Queries openHistorian as a Grafana data source.
    /// </summary>
    /// <param name="request">Query request.</param>
    [OperationContract, WebInvoke(UriTemplate = "/query", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<IEnumerable<TimeSeriesValues>> Query(QueryRequest request);

    /// <summary>
    /// Queries openPDC alarm states as a Grafana data source.
    /// </summary>
    /// <param name="request">Query request.</param>
    [OperationContract, WebInvoke(UriTemplate = "/getalarmstate", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<IEnumerable<AlarmDeviceStateView>> GetAlarmState(QueryRequest request);

    /// <summary>
    /// Queries openHistorian Device alarm states.
    /// </summary>
    /// <param name="request">Query request.</param>
    [OperationContract, WebInvoke(UriTemplate = "/getdevicealarms", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<IEnumerable<AlarmState>> GetDeviceAlarms(QueryRequest request);

    /// <summary>
    /// Queries openHistorian Device Groups.
    /// </summary>
    /// <param name="request">Query request.</param>
    [OperationContract, WebInvoke(UriTemplate = "/getdevicegroups", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<IEnumerable<DeviceGroup>> GetDeviceGroups(QueryRequest request);

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
    /// Queries openHistorian for annotations in a time-range (e.g., Alarms).
    /// </summary>
    /// <param name="request">Annotation request.</param>
    [OperationContract, WebInvoke(UriTemplate = "/annotations", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<List<AnnotationResponse>> Annotations(AnnotationRequest request);

    /// <summary>
    /// Queries available MetaData Options.
    /// </summary>
    /// <param name="isPhasor">A boolean indicating whether the data is a phasor.</param>
    [OperationContract, WebInvoke(UriTemplate = "/gettableoptions", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<string[]> GetTableOptions(bool isPhasor);

    /// <summary>
    /// Queries description of available functions.
    /// </summary>
    [OperationContract, WebInvoke(UriTemplate = "/getfunctions", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<FunctionDescription[]> GetFunctions();

    /// <summary>
    /// Queries available metaDataFields for a given set of tables.
    /// </summary>
    /// <param name="request"> the set of Tables to be queried. </param>
    [OperationContract, WebInvoke(UriTemplate = "/getmetadataoptions", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    Task<Dictionary<string, string[]>> GetMetadataOptions(MetadataOptionsRequest request);
}

#endif