//******************************************************************************************************
//  ZeroMqTransportTransport.cs - Gbtc
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
//  05/28/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

namespace GSF.Communication
{
    /// <summary>
    /// Indicates the transport protocol used in ZeroMQ communications.
    /// </summary>
    public enum ZeroMQTransportProtocol
    {
        /// <summary>
        /// Transmission Control Protocol.
        /// </summary>
        Tcp,
        /// <summary>
        /// In-process Transmissions.
        /// </summary>
        InProc,
        /// <summary>
        /// Pragmatic General Multicast.
        /// </summary>
        /// <remarks>
        /// <para>
        /// PGM is a reliable multicast transport protocol for applications that
        /// require ordered or unordered, duplicate-free, multicast data delivery
        /// from multiple sources to multiple receivers.
        /// </para>
        /// <para>
        /// PGM guarantees that a receiver in the group either receives all data
        /// packets from transmissions and repairs, or is able to detect unrecoverable
        /// data packet loss. PGM is specifically intended as a workable solution for
        /// multicast applications with basic reliability requirements. Its central
        /// design goal is simplicity of operation with due regard for scalability and
        /// network efficiency.
        /// </para>
        /// On Windows this requires MSMQ.
        /// </remarks>
        Pgm,
        /// <summary>
        /// Encapsulated PGM.
        /// </summary>
        Epgm = Pgm
    }
}
