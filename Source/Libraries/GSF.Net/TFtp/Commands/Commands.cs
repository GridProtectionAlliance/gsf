//******************************************************************************************************
//  Commands.cs - Gbtc
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

namespace GSF.Net.TFtp.Commands
{
    internal interface ITFtpCommand
    {
        void Visit(ITFtpCommandVisitor visitor);
    }

    internal interface ITFtpCommandVisitor
    {
        void OnReadRequest(ReadRequest command);
        void OnWriteRequest(WriteRequest command);
        void OnData(Data command);
        void OnAcknowledgement(Acknowledgement command);
        void OnError(Error command);
        void OnOptionAcknowledgement(OptionAcknowledgement command);
    }

    internal abstract class ReadOrWriteRequest
    {
        protected ReadOrWriteRequest(ushort opCode, string filename, TFtpTransferMode mode, IEnumerable<TransferOption> options)
        {
            TargetOpCode = opCode;
            Filename = filename;
            Mode = mode;
            Options = options;
        }

        public ushort TargetOpCode { get; }

        public string Filename { get; }

        public TFtpTransferMode Mode { get; }

        public IEnumerable<TransferOption> Options { get; }

        public override string ToString() => 
            $"Target OP Code = {TargetOpCode}; Filename = {Filename}; Mode = {Mode}";
    }

    internal class ReadRequest : ReadOrWriteRequest, ITFtpCommand
    {
        public const ushort OpCode = 1;

        public ReadRequest(string filename, TFtpTransferMode mode, IEnumerable<TransferOption> options)
            : base(OpCode, filename, mode, options)
        {
        }

        public void Visit(ITFtpCommandVisitor visitor) => 
            visitor.OnReadRequest(this);
    }

    internal class WriteRequest : ReadOrWriteRequest, ITFtpCommand
    {
        public const ushort OpCode = 2;

        public WriteRequest(string filename, TFtpTransferMode mode, IEnumerable<TransferOption> options)
            : base(OpCode, filename, mode, options)
        {
        }

        public void Visit(ITFtpCommandVisitor visitor) => 
            visitor.OnWriteRequest(this);
    }

    internal class Data : ITFtpCommand
    {
        public const ushort OpCode = 3;

        public Data(ushort blockNumber, byte[] data)
        {
            BlockNumber = blockNumber;
            Bytes = data;
        }

        public ushort BlockNumber { get; }

        public byte[] Bytes { get; }

        public void Visit(ITFtpCommandVisitor visitor) => 
            visitor.OnData(this);

        public override string ToString() =>
            $"Block Number = {BlockNumber}; Op Code = {OpCode}";
    }

    internal class Acknowledgement : ITFtpCommand
    {
        public const ushort OpCode = 4;

        public Acknowledgement(ushort blockNumber)
        {
            BlockNumber = blockNumber;
        }

        public ushort BlockNumber { get; }

        public void Visit(ITFtpCommandVisitor visitor) => 
            visitor.OnAcknowledgement(this);

        public override string ToString() =>
            $"Block Number = {BlockNumber}; Op Code = {OpCode}";
    }

    internal class Error : ITFtpCommand
    {
        public const ushort OpCode = 5;

        public Error(ushort errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }

        public ushort ErrorCode { get; }

        public string Message { get; }

        public void Visit(ITFtpCommandVisitor visitor)
        {
            visitor.OnError(this);
        }

        public override string ToString() =>
            $"Error Code = {ErrorCode}; Op Code = {OpCode}; Message = {Message}";
    }

    internal class OptionAcknowledgement : ITFtpCommand
    {
        public const ushort OpCode = 6;

        public OptionAcknowledgement(IEnumerable<TransferOption> options)
        {
            Options = options;
        }

        public IEnumerable<TransferOption> Options { get; }

        public void Visit(ITFtpCommandVisitor visitor) => 
            visitor.OnOptionAcknowledgement(this);

        public override string ToString() =>
            $"Op Code = {OpCode}";
    }
}