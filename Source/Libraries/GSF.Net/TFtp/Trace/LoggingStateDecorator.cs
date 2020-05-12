//******************************************************************************************************
//  LoggingStateDecorator.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  05/12/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        https://github.com/Callisto82/tftp.net
//  
//   Copyright © 2011, Michael Baer
//
//*******************************************************************************************************

#endregion

using System.Net;
using GSF.Net.TFtp.Commands;
using GSF.Net.TFtp.Transfer;
using GSF.Net.TFtp.Transfer.States;

namespace GSF.Net.TFtp.Trace
{
    internal class LoggingStateDecorator : ITransferState
    {
        public TFtpTransfer Context 
        {
            get => m_decoratee.Context;
            set => m_decoratee.Context = value;
        }

        private readonly ITransferState m_decoratee;
        private readonly TFtpTransfer m_transfer;

        public LoggingStateDecorator(ITransferState decoratee, TFtpTransfer transfer)
        {
            m_decoratee = decoratee;
            m_transfer = transfer;
        }

        public string GetStateName()
        {
            return $"[{m_decoratee.GetType().Name}]";
        }

        public void OnStateEnter()
        {
            TFtpTrace.Trace($"{GetStateName()} OnStateEnter", m_transfer);
            m_decoratee.OnStateEnter();
        }

        public void OnStart()
        {
            TFtpTrace.Trace($"{GetStateName()} OnStart", m_transfer);
            m_decoratee.OnStart();
        }

        public void OnCancel(TFtpErrorPacket reason)
        {
            TFtpTrace.Trace($"{GetStateName()} OnCancel: {reason}", m_transfer);
            m_decoratee.OnCancel(reason);
        }

        public void OnCommand(ITFtpCommand command, EndPoint endpoint)
        {
            TFtpTrace.Trace($"{GetStateName()} OnCommand: {command} from {endpoint}", m_transfer);
            m_decoratee.OnCommand(command, endpoint);
        }

        public void OnTimer()
        {
            m_decoratee.OnTimer();
        }
    }
}
