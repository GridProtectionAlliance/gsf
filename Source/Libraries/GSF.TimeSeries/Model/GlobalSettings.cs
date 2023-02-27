//******************************************************************************************************
//  GlobalSettings.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  12/01/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Configuration;
using GSF.Diagnostics;
using GSF.Security;
using System;

namespace GSF.TimeSeries.Model
{
    internal class GlobalSettings
    {
        public Guid NodeID => AdoSecurityProvider.DefaultNodeID;

        public string CompanyAcronym => s_companyAcronym;

        private static readonly string s_companyAcronym;

        static GlobalSettings()
        {
            try
            {
                CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings[nameof(systemSettings)];
                s_companyAcronym = systemSettings[nameof(CompanyAcronym)]?.Value;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex, "Failed to initialize default company acronym");
            }

            if (string.IsNullOrWhiteSpace(s_companyAcronym))
                s_companyAcronym = "GPA";
        }
    }
}
