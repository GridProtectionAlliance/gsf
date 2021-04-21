//******************************************************************************************************
//  LineFrequency.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.ComponentModel;

namespace GSF.Units.EE
{
    /// <summary>
    /// Nominal line frequencies enumeration.
    /// </summary>
    [Serializable]
    public enum LineFrequency
    {
        /// <summary>
        /// 50Hz nominal frequency.
        /// </summary>
        [Description("Selects 50Hz as the nominal system frequency")]
        Hz50 = 50,
        /// <summary>
        /// 60Hz nominal frequency.
        /// </summary>
        [Description("Selects 60Hz as the nominal system frequency")]
        Hz60 = 60
    }
}
