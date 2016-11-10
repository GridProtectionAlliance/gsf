//******************************************************************************************************
//  TimeTagException.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/10/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Historian
{
    /// <summary>
    /// Represents an exception related to <see cref="TimeTag"/> instances.
    /// </summary>
    public class TimeTagException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTagException" /> class.
        /// </summary>
        public TimeTagException()
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTagException" /> class with a specified error <paramref name="message"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public TimeTagException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTagException" /> class with a specified error <paramref name="message"/> and a reference to the <paramref name="innerException"/> that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <c>null</c> reference if no inner exception is specified.</param>
        public TimeTagException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}