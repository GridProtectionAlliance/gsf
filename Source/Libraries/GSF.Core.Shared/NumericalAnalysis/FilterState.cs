//******************************************************************************************************
//  FilterState.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  01/20/2023 - C. Lacker
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.NumericalAnalysis
{
    /// <summary>
    /// Represents the internal state of a <see cref="DigitalFilter"/> or <see cref="AnalogFilter"/>
    /// </summary>
    /// #ToDo Implement ability to derrive state from constant value (f(t) = 5 for infinity
    public class FilterState
    {
        /// <summary>
        /// Creates a new <see cref="FilterState"/> with all internal states set to 0
        /// </summary>
        public FilterState()
        {
            StateValue = new double[0];
        }

        public double[] StateValue { get; set; }
    }
}
