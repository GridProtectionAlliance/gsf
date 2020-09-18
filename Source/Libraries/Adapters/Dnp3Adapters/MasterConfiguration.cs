//******************************************************************************************************
//  MasterConfiguration.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  10/05/2012 - Adam Crain
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header. 
//
//******************************************************************************************************

using System;
using Automatak.DNP3.Interface;

namespace DNP3Adapters
{
    /// <summary>
    /// Master Configuration
    /// </summary>
    public class MasterConfiguration
    {
        /// <summary>
        /// All of the settings for the connection
        /// </summary>
        public TcpClientConfig client = new TcpClientConfig();

        /// <summary>
        /// All of the settings for the master
        /// </summary>
        public MasterStackConfig master = new MasterStackConfig();
    }

    /// <summary>
    /// TCP Client Configuration class.
    /// </summary>
    public class TcpClientConfig
    {
        /// <summary>
        /// IP address of host
        /// </summary>
        public string address = "127.0.0.1";

        /// <summary>
        /// TCP port for connection
        /// </summary>
        public ushort port = 20000;

        /// <summary>
        /// Minimum connection retry interval in milliseconds
        /// </summary>
        public ulong minRetryMs = 5000;

        /// <summary>
        /// Maximum connection retry interval in milliseconds
        /// </summary>
        public ulong maxRetryMs = 60000;

        /// <summary>
        /// DNP3 filter level for port messages
        /// </summary>
        public uint level = LogLevels.NORMAL;
    }
}
