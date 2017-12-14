//******************************************************************************************************
//  TargetOptions.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  12/14/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GrafanaAdapters
{
    /// <summary>
    /// Defines options associated with a Grafana target.
    /// </summary>
    public class TargetOptions
    {
        /// <summary>
        /// Excluded data flags.
        /// </summary>
        public readonly uint ExcludedFlags;

        /// <summary>
        /// Exclude normal flag.
        /// </summary>
        public readonly bool ExcludeNormalFlag;

        /// <summary>
        /// Creates a new <see cref="TargetOptions"/> instance.
        /// </summary>
        /// <param name="target">Source <see cref="GrafanaAdapters.Target"/></param>
        public TargetOptions(Target target)
        {
            ExcludedFlags = Convert.ToUInt32(target.excludedFlags ?? "0x00000000", 16); // 0x00000000
            ExcludeNormalFlag = target.excludeNormalFlag;
        }
    }
}
