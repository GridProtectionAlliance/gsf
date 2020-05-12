//******************************************************************************************************
//  Sending.cs - Gbtc
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

using System;
using GSF.Net.TFtp.Commands;

namespace GSF.Net.TFtp.Transfer.States
{
    internal class Sending : StateThatExpectsMessagesFromDefaultEndPoint
    {
        private byte[] m_lastData;
        private ushort m_lastBlockNumber;
        private long m_bytesSent;
        private bool m_lastPacketWasSent;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            m_lastData = new byte[Context.BlockSize];
 	        SendNextPacket(1);
        }

        public override void OnAcknowledgement(Acknowledgement command)
        {
            // Drop acknowledgments for other packets than the previous one
            if (command.BlockNumber != m_lastBlockNumber)
                return;

            // Notify our observers about our progress
            m_bytesSent += m_lastData.Length;
            Context.RaiseOnProgress(m_bytesSent);

            if (m_lastPacketWasSent)
            {
                // We're done here
                Context.RaiseOnFinished();
                Context.SetState(new Closed());
            }
            else
            {
                SendNextPacket(Context.BlockCounterWrapping.CalculateNextBlockNumber(m_lastBlockNumber));
            }
        }

        public override void OnError(Error command)
        {
            Context.SetState(new ReceivedError(command));
        }

        public override void OnCancel(TFtpErrorPacket reason)
        {
            Context.SetState(new CancelledByUser(reason));
        }

        #region Helper Methods

        private void SendNextPacket(ushort blockNumber)
        {
            if (Context.InputOutputStream == null)
                return;

            int packetLength = Context.InputOutputStream.Read(m_lastData, 0, m_lastData.Length);
            m_lastBlockNumber = blockNumber;

            if (packetLength != m_lastData.Length)
            {
                // This means we just sent the last packet
                m_lastPacketWasSent = true;
                Array.Resize(ref m_lastData, packetLength);
            }

            ITFtpCommand dataCommand = new Commands.Data(blockNumber, m_lastData);
            SendAndRepeat(dataCommand);
        }

        #endregion
    }
}
