//******************************************************************************************************
//  ReceivedError.cs - Gbtc
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
    internal class ReceivedError : BaseState
    {
        private readonly TFtpTransferError m_error;

        public ReceivedError(Error error)
            : this(new TFtpErrorPacket(error.ErrorCode, GetNonEmptyErrorMessage(error))) { }

        private static string GetNonEmptyErrorMessage(Error error)
        {
            return string.IsNullOrEmpty(error.Message) ? "(No error message provided)" : error.Message;
        }

        public ReceivedError(TFtpTransferError error)
        {
            m_error = error;
        }

        public override void OnStateEnter()
        {
            TFtpTrace.Trace($"Received error: {m_error}", Context);
            Context.RaiseOnError(m_error);
            Context.SetState(new Closed());
        }
    }
}
