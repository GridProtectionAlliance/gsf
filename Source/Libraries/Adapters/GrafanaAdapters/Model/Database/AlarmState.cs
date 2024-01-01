//******************************************************************************************************
//  AlarmState.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  07/18/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//  27/11/2019 - C. Lackner
//      Moved Adapter to GSF
//
//******************************************************************************************************

using System.ComponentModel.DataAnnotations;
using GSF.Data.Model;

namespace GrafanaAdapters.Model.Database;

/// <summary>
/// Represents the alarm state of a connected device.
/// </summary>
public class AlarmState
{
    /// <summary>
    /// Gets or sets unique ID.
    /// </summary>
    [PrimaryKey(true)]
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets description of the <see cref="AlarmState"/>.
    /// </summary>
    [StringLength(50)]
    public string State { get; set; }

    /// <summary>
    /// Gets or sets recommended action for the user if a <see cref="AlarmDevice"/> is in this <see cref="AlarmState"/>.
    /// </summary>
    [StringLength(500)]
    public string RecommendedAction { get; set; }

    /// <summary>
    /// Gets or sets color associated with the <see cref="AlarmState"/>.
    /// </summary>
    [StringLength(50)]
    public string Color { get; set; }
}