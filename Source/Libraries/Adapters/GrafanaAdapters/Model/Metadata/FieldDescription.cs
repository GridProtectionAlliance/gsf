//******************************************************************************************************
//  FieldDescription.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  01/15/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

namespace GrafanaAdapters.Model.Metadata;

/// <summary>
/// Defines a field description for a data source value type metadata table.
/// </summary>
public class FieldDescription
{
    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Gets or sets the data type of the field.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// Gets or sets flag indicating if field is required.
    /// </summary>
    public bool required { get; set; }
}