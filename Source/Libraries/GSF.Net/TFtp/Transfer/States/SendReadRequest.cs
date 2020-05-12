//******************************************************************************************************
//  SendReadRequest.cs - Gbtc
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

namespace GSF.Net.TFtp.Transfer.States
{
    internal class SendReadRequest : StateWithNetworkTimeout
    {
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            SendRequest(); // Send a read request to the server
        }

        private void SendRequest()
        {
            ReadRequest request = new ReadRequest(Context.Filename, Context.TransferMode, Context.ProposedOptions.ToOptionList());
            SendAndRepeat(request);
        }

        public override void OnCommand(ITFtpCommand command, EndPoint endpoint)
        {
            if (command is Commands.Data || command is OptionAcknowledgement)
            {
                // The server acknowledged our read request.
                // Fix out remote endpoint
                Context.GetConnection().RemoteEndpoint = endpoint;
            }

            switch (command)
            {
                case Commands.Data _:
                {
                    if (Context.NegotiatedOptions == null)
                        Context.FinishOptionNegotiation(TransferOptionSet.NewEmptySet());

                    // Switch to the receiving state...
                    ITransferState nextState = new Receiving();
                    Context.SetState(nextState);

                    // ...and let it handle the data packet
                    nextState.OnCommand(command, endpoint);
                    break;
                }
                
                case OptionAcknowledgement optionAcknowledgement:
                    // Check which options were acknowledged
                    Context.FinishOptionNegotiation(new TransferOptionSet(optionAcknowledgement.Options));

                    // the server acknowledged our options. Confirm the final options
                    SendAndRepeat(new Acknowledgement(0));
                    break;
                
                case Error error:
                    Context.SetState(new ReceivedError(error));
                    break;

                default:
                    base.OnCommand(command, endpoint);
                    break;
            }
        }

        public override void OnCancel(TFtpErrorPacket reason)
        {
            Context.SetState(new CancelledByUser(reason));
        }
    }
}
