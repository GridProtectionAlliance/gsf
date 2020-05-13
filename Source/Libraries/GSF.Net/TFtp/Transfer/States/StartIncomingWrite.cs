//******************************************************************************************************
//  StartIncomingWrite.cs - Gbtc
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

using System.Collections.Generic;
using GSF.Net.TFtp.Commands;

namespace GSF.Net.TFtp.Transfer.States
{
    internal class StartIncomingWrite : BaseState
    {
        private readonly IEnumerable<TransferOption> m_optionsRequestedByClient;

        public StartIncomingWrite(IEnumerable<TransferOption> optionsRequestedByClient)
        {
            m_optionsRequestedByClient = optionsRequestedByClient;
        }

        public override void OnStateEnter()
        {
            Context.ProposedOptions = new TransferOptionSet(m_optionsRequestedByClient);
        }

        public override void OnStart()
        {
            // Do we have any acknowledged options?
            Context.FinishOptionNegotiation(Context.ProposedOptions);
            List<TransferOption> options = Context.NegotiatedOptions.ToOptionList();

            if (options.Count > 0)
                Context.SetState(new SendOptionAcknowledgementForWriteRequest());
            else
                Context.SetState(new AcknowledgeWriteRequest()); // Start receiving
        }

        public override void OnCancel(TFtpErrorPacket reason)
        {
            Context.SetState(new CancelledByUser(reason));
        }
    }
}
