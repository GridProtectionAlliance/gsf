//******************************************************************************************************
//  GrafanaDataService.NET.cs - Gbtc
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

using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Threading.Tasks;
using GSF.Historian.DataServices;
using Newtonsoft.Json;

namespace GrafanaAdapters
{
    /// <summary>
    /// Represents a REST based API for a simple JSON based Grafana data source for the openHistorian 1.0.
    /// </summary>
    // .NET Implementation
    public partial class GrafanaDataService : DataService, IGrafanaDataService
    {
        /// <summary>
        /// Queries openHistorian as a Grafana data source.
        /// </summary>
        /// <param name="request">Query request.</param>
        public async Task<List<TimeSeriesValues>> Query(QueryRequest request)
        {
            // Abort if services are not enabled
            if (!Enabled || Archive is null)
                return null;

            return await m_dataSource.Query(request, m_cancellationSource.Token);
        }

        /// <summary>
        /// Queries openPDC alarm states as a Grafana data source.
        /// </summary>
        /// <param name="request">Query request.</param>
        public async Task<IEnumerable<AlarmDeviceStateView>> GetAlarmState(QueryRequest request)
        {
            // Abort if services are not enabled
            if (!Enabled || Archive is null)
                return null;

            return await m_dataSource.GetAlarmState(request, m_cancellationSource.Token);
        }

        /// <summary>
        /// Queries openHistorian Device alarm states.
        /// </summary>
        /// <param name="request">Query request.</param>
        public async Task<IEnumerable<AlarmState>> GetDeviceAlarms(QueryRequest request)
        {
            // Abort if services are not enabled
            if (!Enabled || Archive is null)
                return null;

            return await m_dataSource.GetDeviceAlarms(request, m_cancellationSource.Token);
        }

        /// <summary>
        /// Queries openHistorian Device Groups.
        /// </summary>
        /// <param name="request">Query request.</param>
        public async Task<IEnumerable<DeviceGroup>> GetDeviceGroups(QueryRequest request)
        {
            // Abort if services are not enabled
            if (!Enabled || Archive is null)
                return null;

            return await m_dataSource.GetDeviceGroups(request, m_cancellationSource.Token);
        }

        /// <summary>
        /// Queries openPDC Alarms as a Grafana alarm data source.
        /// </summary>
        /// <param name="request">Query request.</param>
        public async Task<IEnumerable<GrafanaAlarm>> GetAlarms(QueryRequest request)
        {
            // Abort if services are not enabled
            if (!Enabled || Archive is null)
                return null;

            return await m_dataSource.GetAlarms(request, m_cancellationSource.Token);
        }

        /// <summary>
        /// Queries openHistorian as a Grafana Metadata source.
        /// </summary>
        /// <param name="request">Query request.</param>
        public Task<string> GetMetadata(Target request)
        {
            return Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrWhiteSpace(request.target))
                    return string.Empty;

                DataTable table = new();
                DataRow[] rows = m_dataSource?.Metadata.Tables["ActiveMeasurements"].Select($"PointTag IN ({request.target})") ?? Array.Empty<DataRow>();

                if (rows.Length > 0)
                    table = rows.CopyToDataTable();

                return JsonConvert.SerializeObject(table);
            });
        }

        /// <summary>
        /// Search openHistorian for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        public async Task<string[]> Search(Target request) => 
            await m_dataSource.Search(request, m_cancellationSource.Token);

        /// <summary>
        /// Search data source for a list of columns from a specific table.
        /// </summary>
        /// <param name="request">Table Name.</param>
        public async Task<string[]> SearchFields(Target request) => 
            await m_dataSource.SearchFields(request, m_cancellationSource.Token);

        /// <summary>
        /// Search data source for a list of tables.
        /// </summary>
        /// <param name="request">Request.</param>
        public async Task<string[]> SearchFilters(Target request) => 
            await m_dataSource.SearchFilters(request, m_cancellationSource.Token);

        /// <summary>
        /// Search data source for a list of columns from a specific table.
        /// </summary>
        /// <param name="request">Table Name.</param>
        public async Task<string[]> SearchOrderBys(Target request) => 
            await m_dataSource.SearchOrderBys(request, m_cancellationSource.Token);

        /// <summary>
        /// Queries openHistorian for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        public async Task<List<AnnotationResponse>> Annotations(AnnotationRequest request)
        {
            // Abort if services are not enabled
            if (!Enabled || Archive is null)
                return null;

            return await m_dataSource.Annotations(request, m_cancellationSource.Token);
        }

        /// <summary>
        /// Queries openHistorian location data for Grafana offsetting duplicate coordinates using a radial distribution.
        /// </summary>
        /// <param name="request"> Query request.</param>
        /// <returns>JSON serialized location metadata for specified targets.</returns>
        public async Task<string> GetLocationData(LocationRequest request)
        {
            if (request.zoom is null || request.radius is null)
                return await LocationData.GetLocationData(request.request, m_cancellationSource.Token);

            return await LocationData.GetLocationData((double)request.radius, (double)request.zoom, request.request, m_cancellationSource.Token);
        }

        /// <summary>
        /// Requests available tag keys.
        /// </summary>
        /// <param name="_">Tag keys request.</param>
        public async Task<TagKeysResponse[]> TagKeys(TagKeysRequest _) => 
            await m_dataSource.TagKeys(_, m_cancellationSource.Token);

        /// <summary>
        /// Requests available tag values.
        /// </summary>
        /// <param name="request">Tag values request.</param>
        public async Task<TagValuesResponse[]> TagValues(TagValuesRequest request) => 
            await m_dataSource.TagValues(request, m_cancellationSource.Token);

        /// <summary>
        /// Queries available MetaData Options.
        /// </summary>
        /// <param name="request">A boolean indicating whether the data is a phasor.</param>
        public async Task<string[]> GetTableOptions(bool request) => 
            await m_dataSource.GetTableOptions(request, m_cancellationSource.Token);

        /// <summary>
        /// Requests Grafana Metadatas source for multiple targets.
        /// </summary>
        /// <param name="requests">Array of query requests.</param>
        public async Task<string> GetMetadatas(MetadataTargetRequest[] requests) => 
            await m_dataSource.GetMetadatas(requests, m_cancellationSource.Token);

        /// <summary>
        /// Queries description of available functions.
        /// </summary>
        public async Task<FunctionDescription[]> GetFunctions() =>
            await m_dataSource.GetFunctionDescription(m_cancellationSource.Token);

        /// <summary>
        /// Queries svailable metaDataFields for a given source.
        /// </summary>
        public async Task<Dictionary<string, string[]>> GetMetadataOptions(MetadataOptionsRequest request) => 
            await m_dataSource.GetMetadataOptions(request,m_cancellationSource.Token);
    }
}

#endif