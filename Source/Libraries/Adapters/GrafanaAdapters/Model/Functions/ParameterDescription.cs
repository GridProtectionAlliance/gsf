//******************************************************************************************************
//  ParameterDescription.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  10/17/2023 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GrafanaAdapters
{
    /// <summary>
    /// Describes a Grafana Function Parameter.
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        /// The Description of the Parameter
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Indicates if this Parameter is required 
        /// </summary>
        public bool Required { get; }

        /// <summary>
        /// Derscribes the type of the parameter.
        /// </summary>
        public string ParameterTypeName { get; }

        public string Name { get; }

    }
}