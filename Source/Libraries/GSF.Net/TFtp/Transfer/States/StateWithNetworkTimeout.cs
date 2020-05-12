//******************************************************************************************************
//  StateWithNetworkTimeout.cs - Gbtc
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

using GSF.Net.TFtp.Commands;
using GSF.Net.TFtp.Trace;

namespace GSF.Net.TFtp.Transfer.States
{
    internal class StateWithNetworkTimeout : BaseState
    {
        private SimpleTimer m_timer;
        private ITFtpCommand m_lastCommand;
        private int m_retriesUsed;

        public override void OnStateEnter()
        {
            m_timer = new SimpleTimer(Context.RetryTimeout);
        }

        public override void OnTimer()
        {
            if (!m_timer.IsTimeout())
                return;

            TFtpTrace.Trace("Network timeout.", Context);
            m_timer.Restart();

            if (m_retriesUsed++ >= Context.RetryCount)
            {
                TFtpTransferError error = new TimeoutError(Context.RetryTimeout, Context.RetryCount);
                Context.SetState(new ReceivedError(error));
            }
            else
            {
                HandleTimeout();
            }
        }

        private void HandleTimeout()
        {
            if (m_lastCommand != null)
                Context.GetConnection().Send(m_lastCommand);
        }

        protected void SendAndRepeat(ITFtpCommand command)
        {
            Context.GetConnection().Send(command);
            m_lastCommand = command;
            ResetTimeout();
        }

        protected void ResetTimeout() 
        {
            m_timer.Restart();
            m_retriesUsed = 0;
        }
    }
}
