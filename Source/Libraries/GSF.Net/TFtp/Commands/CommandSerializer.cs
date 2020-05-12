//******************************************************************************************************
//  CommandSerializer.cs - Gbtc
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

using System.IO;
using System.Text;

namespace GSF.Net.TFtp.Commands
{
    /// <summary>
    /// Serializes an ITFtpCommand into a stream of bytes.
    /// </summary>
    internal class CommandSerializer
    {
        /// <summary>
        /// Call this method to serialize the given <code>command</code> using the given <code>writer</code>.
        /// </summary>
        public void Serialize(ITFtpCommand command, Stream stream)
        {
            CommandComposerVisitor visitor = new CommandComposerVisitor(stream);
            command.Visit(visitor);
        }

        private class CommandComposerVisitor : ITFtpCommandVisitor
        {
            private readonly TFtpStreamWriter m_writer;

            public CommandComposerVisitor(Stream stream)
            {
                m_writer = new TFtpStreamWriter(stream);
            }

            private void OnReadOrWriteRequest(ReadOrWriteRequest command)
            {
                m_writer.WriteBytes(Encoding.ASCII.GetBytes(command.Filename));
                m_writer.WriteByte(0);
                m_writer.WriteBytes(Encoding.ASCII.GetBytes(command.Mode.ToString()));
                m_writer.WriteByte(0);

                if (command.Options == null)
                    return;

                foreach (TransferOption option in command.Options)
                {
                    m_writer.WriteBytes(Encoding.ASCII.GetBytes(option.Name));
                    m_writer.WriteByte(0);
                    m_writer.WriteBytes(Encoding.ASCII.GetBytes(option.Value));
                    m_writer.WriteByte(0);
                }
            }

            public void OnReadRequest(ReadRequest command)
            {
                m_writer.WriteUInt16(ReadRequest.OpCode);
                OnReadOrWriteRequest(command);
            }

            public void OnWriteRequest(WriteRequest command)
            {
                m_writer.WriteUInt16(WriteRequest.OpCode);
                OnReadOrWriteRequest(command);
            }

            public void OnData(Data command)
            {
                m_writer.WriteUInt16(Data.OpCode);
                m_writer.WriteUInt16(command.BlockNumber);
                m_writer.WriteBytes(command.Bytes);
            }

            public void OnAcknowledgement(Acknowledgement command)
            {
                m_writer.WriteUInt16(Acknowledgement.OpCode);
                m_writer.WriteUInt16(command.BlockNumber);
            }

            public void OnError(Error command)
            {
                m_writer.WriteUInt16(Error.OpCode);
                m_writer.WriteUInt16(command.ErrorCode);
                m_writer.WriteBytes(Encoding.ASCII.GetBytes(command.Message));
                m_writer.WriteByte(0);
            }

            public void OnOptionAcknowledgement(OptionAcknowledgement command)
            {
                m_writer.WriteUInt16(OptionAcknowledgement.OpCode);

                foreach (TransferOption option in command.Options)
                {
                    m_writer.WriteBytes(Encoding.ASCII.GetBytes(option.Name));
                    m_writer.WriteByte(0);
                    m_writer.WriteBytes(Encoding.ASCII.GetBytes(option.Value));
                    m_writer.WriteByte(0);
                }
            }
        }
    }
}
