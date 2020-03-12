//******************************************************************************************************
//  DeviceGroupModel.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/12/2020 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel;
using GSF.Data.Model;

namespace GrafanaAdapters
{
   
    /// <summary>
    /// Represents a Group of Devices
    /// Also modeled as a seperate virtual Device with connection string
    /// </summary>
    public class DeviceGroup
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
        /// List of attached DeviceIds
        /// </summary>
        public List<int> Devices { get; set; }

    }
}

