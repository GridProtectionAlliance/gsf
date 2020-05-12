//******************************************************************************************************
//  CommandParser.cs - Gbtc
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSF.Net.TFtp.Commands
{
    /// <summary>
    /// Parses a ITFtpCommand.
    /// </summary>
    internal class CommandParser
    {
        /// <summary>
        /// Parses an ITFtpCommand from the given byte array. If the byte array cannot be parsed for some reason, a TFtpParserException is thrown.
        /// </summary>
        public ITFtpCommand Parse(byte[] message)
        {
            try
            {
                return ParseInternal(message);
            }
            catch (TFtpParserException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TFtpParserException(ex);
            }
        }

        private ITFtpCommand ParseInternal(byte[] message)
        {
            TFtpStreamReader reader = new TFtpStreamReader(new MemoryStream(message));

            ushort opcode = reader.ReadUInt16();

            switch (opcode)
            {
                case ReadRequest.OpCode:
                    return ParseReadRequest(reader);

                case WriteRequest.OpCode:
                    return ParseWriteRequest(reader);

                case Data.OpCode:
                    return ParseData(reader);

                case Acknowledgement.OpCode:
                    return ParseAcknowledgement(reader);

                case Error.OpCode:
                    return ParseError(reader);

                case OptionAcknowledgement.OpCode:
                    return ParseOptionAcknowledgement(reader);

                default:
                    throw new TFtpParserException("Invalid opcode");
            }
        }

        private OptionAcknowledgement ParseOptionAcknowledgement(TFtpStreamReader reader)
        {
            IEnumerable<TransferOption> options = ParseTransferOptions(reader);
            return new OptionAcknowledgement(options);
        }

        private Error ParseError(TFtpStreamReader reader)
        {
            ushort errorCode = reader.ReadUInt16();
            string message = ParseNullTerminatedString(reader);
            return new Error(errorCode, message);
        }

        private Acknowledgement ParseAcknowledgement(TFtpStreamReader reader)
        {
            ushort blockNumber = reader.ReadUInt16();
            return new Acknowledgement(blockNumber);
        }

        private Data ParseData(TFtpStreamReader reader)
        {
            ushort blockNumber = reader.ReadUInt16();
            byte[] data = reader.ReadBytes(10000);
            return new Data(blockNumber, data);
        }

        private WriteRequest ParseWriteRequest(TFtpStreamReader reader)
        {
            string filename = ParseNullTerminatedString(reader);
            TFtpTransferMode mode = ParseModeType(ParseNullTerminatedString(reader));
            IEnumerable<TransferOption> options = ParseTransferOptions(reader);
            return new WriteRequest(filename, mode, options);
        }

        private ReadRequest ParseReadRequest(TFtpStreamReader reader)
        {
            string filename = ParseNullTerminatedString(reader);
            TFtpTransferMode mode = ParseModeType(ParseNullTerminatedString(reader));
            IEnumerable<TransferOption> options = ParseTransferOptions(reader);
            return new ReadRequest(filename, mode, options);
        }

        private List<TransferOption> ParseTransferOptions(TFtpStreamReader reader)
        {
            List<TransferOption> options = new List<TransferOption>();

            while (true)
            {
                string name;

                try
                {
                    name = ParseNullTerminatedString(reader);
                }
                catch (IOException)
                {
                    name = "";
                }

                if (string.IsNullOrEmpty(name))
                    break;

                string value = ParseNullTerminatedString(reader);
                options.Add(new TransferOption(name, value));
            }
            return options;
        }

        private static TFtpTransferMode ParseModeType(string mode)
        {
            if (Enum.TryParse(mode, true, out TFtpTransferMode result))
                return result; 
            
            throw new TFtpParserException($"Unknown mode type: {mode}");
        }

        private static string ParseNullTerminatedString(TFtpStreamReader reader)
        {
            byte b;
            StringBuilder str = new StringBuilder();

            while ((b = reader.ReadByte()) > 0)
                str.Append((char)b);

            return str.ToString();
        }
    }

    [Serializable]
    internal class TFtpParserException : Exception
    {
        public TFtpParserException(string message)
            : base(message) { }

        public TFtpParserException(Exception e)
            : base("Error while parsing message.", e) { }
    }
}
