//******************************************************************************************************
//  DeviceStateModels.cs - Gbtc
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

using System;
using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel;
using GSF.Data.Model;

namespace GrafanaAdapters;

/// <summary>
/// Represents the Alarm State of a connected Device.
/// </summary>
public class AlarmState
{
    /// <summary>
    /// Unique ID.
    /// </summary>
    [PrimaryKey(true)]
    public int ID { get; set; }

    /// <summary>
    /// Description of the <see cref="AlarmState"/>.
    /// </summary>
    [StringLength(50)]
    public string State { get; set; }

    /// <summary>
    /// Recommended Action for the User if a <see cref="AlarmDevice"/> is in this <see cref="AlarmState"/>
    /// </summary>
    [StringLength(500)]
    public string RecommendedAction { get; set; }

    /// <summary>
    /// Color associated with the <see cref="AlarmState"/>.
    /// </summary>
    [StringLength(50)]
    public string Color { get; set; }
}

/// <summary>
/// Represents the a connected Device in an AlarmState.
/// </summary>
public class AlarmDevice
{
    /// <summary>
    /// Unique ID.
    /// </summary>
    [PrimaryKey(true)]
    public int ID { get; set; }

    /// <summary>
    /// Device ID of the Alarmed Device.
    /// </summary>
    public int DeviceID { get; set; }

    /// <summary>
    /// ID of the <see cref="AlarmState"/>.
    /// </summary>
    public int StateID { get; set; }

    /// <summary>
    /// Time of the last update.
    /// </summary>
    [DefaultValueExpression("DateTime.UtcNow")]
    [UpdateValueExpression("DateTime.UtcNow")]
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// String to display on the Grafana Alarm Dashboard.
    /// </summary>
    [StringLength(10)]
    public string DisplayData { get; set; }
}

/// <summary>
/// Represents a Grafana Alarm Panel Block
/// </summary>
public class AlarmDeviceStateView
{
    /// <summary>
    /// Unique ID.
    /// </summary>
    [PrimaryKey(true)]
    public int ID { get; set; }

    /// <summary>
    /// name of the Device.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of the Device State.
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// Color of the Device State.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Additional data to be displayed.
    /// </summary>
    public string DisplayData { get; set; }

    /// <summary>
    /// Device ID of the Alarmed Device.
    /// </summary>
    public int DeviceID { get; set; }
}