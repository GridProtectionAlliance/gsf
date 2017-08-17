//******************************************************************************************************
//  Default.cs - Gbtc
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
//  03/09/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Threading;

namespace eDNAAdapters
{
    // Define default values for eDNA adapters
    internal static class Default
    {
        public const string PointIDFormat = "{0}.{1}.{2}";
        public const string IniFilePathFormat = @"{0}\InStep\DNASYS.ini";
        public const ushort PrimaryPort = 8000;
        public const string SecondaryServer = "";
        public const ushort SecondaryPort = 0;
        public const string LocalCacheFileName = "";
        public const bool ClearCacheOnStartup = true;
        public const bool AcknowledgeDataPackets = true;
        public const bool EnableQueuing = true;
        public const bool EnableCaching = true;
        public const bool RunMetadataSync = true;
        public const bool AutoCreateTags = true;
        public const bool AutoUpdateTags = true;
        public const int TagNamePrefixRemoveCount = 0;
        public const string PointMapCacheFileName = "";
        public const string DigitalSetString = "ON";
        public const string DigitalClearedString = "OFF";
        public const bool ValidateINIFileExists = false;
        public const double MaximumPointResolution = 0.0D;
        public const int ConnectionMonitoringInterval = 1000;
        public const int WriteTimeout = Timeout.Infinite;
        public const bool ExpandDigitalWordBits = false;
    }
}
