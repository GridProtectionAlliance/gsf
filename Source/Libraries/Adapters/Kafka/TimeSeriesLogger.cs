//******************************************************************************************************
//  TimeSeriesLogger.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  10/22/2015 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using Misakai.Kafka;

namespace KafkaAdapters
{
    /// <summary>
    /// Defines an proxy for Kafka log messages.
    /// </summary>
    internal class TimeSeriesLogger : IKafkaLog
    {
        private readonly Action<string, object[]> m_statusMessage;
        private readonly Action<Exception> m_errorMessage;

        public TimeSeriesLogger(Action<string, object[]> statusMessage, Action<Exception> errorMessage)
        {
            m_statusMessage = statusMessage;
            m_errorMessage = errorMessage;
        }

        public void InfoFormat(string format, params object[] args)
        {
            m_statusMessage(format, args);
        }

        public void DebugFormat(string format, params object[] args)
        {
            m_statusMessage("DEBUG: " + format, args);
        }

        public void WarnFormat(string format, params object[] args)
        {
            m_statusMessage("WARNING: " + format, args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            m_errorMessage(new InvalidOperationException(string.Format(format, args)));
        }

        public void FatalFormat(string format, params object[] args)
        {
            m_errorMessage(new ApplicationException(string.Format(format, args)));
        }
    }
}
