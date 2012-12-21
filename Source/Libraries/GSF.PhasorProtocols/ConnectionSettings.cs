//******************************************************************************************************
//  ConnectionSettings.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/06/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using GSF.Communication;
using System;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents information defined in a PMU Connection Tester connection file.
    /// </summary>
    [Serializable()]
    public class ConnectionSettings
    {
        /// <summary>
        /// Defines <see cref="PhasorProtocol"/> from PmuConnection file.
        /// </summary>
        public PhasorProtocol PhasorProtocol;

        /// <summary>
        /// Defines <see cref="TransportProtocol"/> from PmuConnection file.
        /// </summary>
        public TransportProtocol TransportProtocol;

        /// <summary>
        /// Defines connection string from PmuConnection file.
        /// </summary>
        public string ConnectionString;

        /// <summary>
        /// Defines ID of the source from PmuConnection file.
        /// </summary>
        public int PmuID;

        /// <summary>
        /// Defines frame rate from PmuConnection file.
        /// </summary>
        public int FrameRate;

        /// <summary>
        /// Defines boolean flag if data needs to be repeated again and again.
        /// </summary>
        public bool AutoRepeatPlayback;

        /// <summary>
        /// Defines byte encoding format from PmuConnection file.
        /// </summary>
        public int ByteEncodingDisplayFormat;

        /// <summary>
        /// Defines additional connection information such as alternate command channel from PmuConnection file.
        /// </summary>
        public IConnectionParameters ConnectionParameters;
    }
}
