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

using System.Collections.Generic;
using System.Threading.Tasks;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.Model.Annotations;
using GrafanaAdapters.Model.Common;
using GrafanaAdapters.Model.Database;
using GrafanaAdapters.Model.Functions;
using GrafanaAdapters.Model.MetaData;
using GSF.Historian.DataServices;

namespace GrafanaAdapters;

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
    public Task<IEnumerable<TimeSeriesValues>> Query(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.Query(request, m_cancellationSource.Token);
    }

    /// <summary>
    /// Queries openPDC alarm states as a Grafana data source.
    /// </summary>
    /// <param name="request">Query request.</param>
    public Task<IEnumerable<AlarmDeviceStateView>> GetAlarmState(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.GetAlarmState(request, m_cancellationSource.Token);
    }

    /// <summary>
    /// Queries openHistorian Device alarm states.
    /// </summary>
    /// <param name="request">Query request.</param>
    public Task<IEnumerable<AlarmState>> GetDeviceAlarms(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.GetDeviceAlarms(request, m_cancellationSource.Token);
    }

    /// <summary>
    /// Queries openHistorian Device Groups.
    /// </summary>
    /// <param name="request">Query request.</param>
    public Task<IEnumerable<DeviceGroup>> GetDeviceGroups(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.GetDeviceGroups(request, m_cancellationSource.Token);
    }       

    /// <summary>
    /// Queries openHistorian as a Grafana Metadata source.
    /// </summary>
    /// <param name="request">Query request.</param>
    public Task<string> GetMetadata(Target request)
    {
        // TODO: JRC - fix this if 'isPhasor' gets updated to 'dataType'
        return request.isPhasor ? 
            m_dataSource.GetMetadata<PhasorValue>(request) : 
            m_dataSource.GetMetadata<DataSourceValue>(request);
    }

    /// <summary>
    /// Search openHistorian for a target.
    /// </summary>
    /// <param name="request">Search target.</param>
    public Task<string[]> Search(Target request)
    {
        return m_dataSource.Search(request, m_cancellationSource.Token);
    }

    /// <summary>
    /// Queries openHistorian for annotations in a time-range (e.g., Alarms).
    /// </summary>
    /// <param name="request">Annotation request.</param>
    public Task<List<AnnotationResponse>> Annotations(AnnotationRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.Annotations(request, m_cancellationSource.Token);
    }
       
    /// <summary>
    /// Queries available metadata options.
    /// </summary>
    /// <param name="isPhasor">A boolean indicating whether the data is a phasor.</param>
    public Task<string[]> GetTableOptions(bool isPhasor)
    {
        return m_dataSource.GetTableOptions(isPhasor, m_cancellationSource.Token);
    }

    /// <summary>
    /// Queries description of available functions.
    /// </summary>
    // TODO: JRC - suggest passing a new parameter e.g. 'string dataType' to filter function descriptions by data type, e.g., "PhasorValue" or "DataSourceValue"
    public Task<FunctionDescription[]> GetFunctions()
    {
        return m_dataSource.GetFunctionDescription(m_cancellationSource.Token);
    }

    /// <summary>
    /// Queries available metadata fields for a given source.
    /// </summary>
    public Task<Dictionary<string, string[]>> GetMetadataOptions(MetadataOptionsRequest request)
    {
        return m_dataSource.GetMetadataOptions(request, m_cancellationSource.Token);
    }
}

#endif