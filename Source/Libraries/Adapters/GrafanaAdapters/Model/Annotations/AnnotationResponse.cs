//******************************************************************************************************
//  AnnotationResponse.cs - Gbtc
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
// ReSharper disable InconsistentNaming

namespace GrafanaAdapters.Model.Annotations;

/// <summary>
/// Defines a Grafana annotation response.
/// </summary>
public class AnnotationResponse
{
    /// <summary>
    /// Gets or sets annotation title.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// Gets or sets annotation time.
    /// </summary>
    public double time { get; set; }

    /// <summary>
    /// Gets or sets annotation end time.
    /// </summary>
    public double endTime { get; set; }

    /// <summary>
    /// Gets or sets annotation text.
    /// </summary>
    public string text { get; set; }

    /// <summary>
    /// Gets or sets annotation tags.
    /// </summary>
    public string tags { get; set; }
}