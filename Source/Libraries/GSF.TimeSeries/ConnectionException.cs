//******************************************************************************************************
//  ConnectionException.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  07/10/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents an exception related to connection activities.
    /// </summary>
    /// <remarks>
    /// This exception is used to filter connection exceptions into a separate log since
    /// these types of exceptions can be so frequent when a device is offline.
    /// </remarks>
    [Serializable]
    public class ConnectionException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="ConnectionException"/>.
        /// </summary>
        public ConnectionException()
        {            
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionException"/> with the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public ConnectionException(string message) : base(message)
        {            
        }

        /// <summary>
        /// Creates a new <see cref="ConnectionException"/> with the specified <paramref name="message"/>
        /// and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {            
        }
    }
}