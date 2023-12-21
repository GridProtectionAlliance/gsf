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

using System.ServiceModel;
using System.ServiceModel.Web;

namespace GrafanaAdapters;

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
}