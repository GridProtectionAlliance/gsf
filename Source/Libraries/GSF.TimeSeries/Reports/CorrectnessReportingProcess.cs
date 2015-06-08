//******************************************************************************************************
//  CorrectnessReportingProcess.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  06/08/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.TimeSeries.Reports
{
    /// <summary>
    /// Represents the process that generates correctness reports for the time-series service.
    /// </summary>
    public class CorrectnessReportingProcess : ReportingProcessBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CorrectnessReportingProcess"/> class.
        /// </summary>
        public CorrectnessReportingProcess()
        {
            Title = "Eval(securityProvider.ApplicationName) Correctness Report";
            SettingsCategory = "CorrectnessReporting";
        }
    }
}
