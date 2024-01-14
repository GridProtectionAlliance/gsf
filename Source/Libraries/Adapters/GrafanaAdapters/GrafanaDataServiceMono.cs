//******************************************************************************************************
//  GrafanaDataServiceMono.cs - Gbtc
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

// WCF service async Task method responses on Mono are always wrapped with "Result" object,
// so async implementations of this service are skipped for Mono
#if MONO

using System.Collections.Generic;
using GrafanaAdapters.DataSources.BuiltIn;
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
// Mono Implementation
public partial class GrafanaDataService : DataService, IGrafanaDataService
{
    /// <inheritdoc/>
    public IEnumerable<TimeSeriesValues> Query(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.Query(request, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public IEnumerable<AlarmDeviceStateView> GetAlarmState(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.GetAlarmState(request, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public IEnumerable<AlarmState> GetDeviceAlarms(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.GetDeviceAlarms(request, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public IEnumerable<DeviceGroup> GetDeviceGroups(QueryRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.GetDeviceGroups(request, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public string GetMetadata(Target request)
    {
        // TODO: JRC - fix this if 'isPhasor' gets updated to 'dataType'
        if (request.isPhasor)
            return m_dataSource.GetMetadata<PhasorValue>(request).Result;

        return m_dataSource.GetMetadata<DataSourceValue>(request).Result;
    }

    /// <inheritdoc/>
    public string[] Search(Target request)
    {
        return m_dataSource.Search(request, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public List<AnnotationResponse> Annotations(AnnotationRequest request)
    {
        // Abort if services are not enabled
        if (!Enabled || Archive is null)
            return null;

        return m_dataSource.Annotations(request, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public string[] GetTableOptions(bool isPhasor)
    {
        return m_dataSource.GetTableOptions(isPhasor, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public IEnumerable<FunctionDescription> GetFunctions(int dataTypeIndex)
    {
        return m_dataSource.GetFunctionDescription(dataTypeIndex, m_cancellationSource.Token).Result;
    }

    /// <inheritdoc/>
    public Dictionary<string, string[]> GetMetadataOptions(MetadataOptionsRequest request)
    {
        return m_dataSource.GetMetadataOptions(request, m_cancellationSource.Token).Result;
    }
}

#endif