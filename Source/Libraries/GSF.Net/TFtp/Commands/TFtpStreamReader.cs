//******************************************************************************************************
//  TFtpStreamReader.cs - Gbtc
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
using System.IO;

// ReSharper disable InconsistentNaming
namespace GSF.Net.TFtp.Commands
{
    internal class TFtpStreamReader
    {
        private readonly Stream m_stream;

        public TFtpStreamReader(Stream stream)
        {
            m_stream = stream;
        }

        public ushort ReadUInt16()
        {
            int byte1 = m_stream.ReadByte();
            int byte2 = m_stream.ReadByte();
            
            return (ushort)((byte)byte1 << 8 | (byte)byte2);
        }

        public byte ReadByte()
        {
            int nextByte = m_stream.ReadByte();

            if (nextByte == -1)
                throw new IOException();

            return (byte)nextByte;
        }

        public byte[] ReadBytes(int maxBytes)
        {
            byte[] buffer = new byte[maxBytes];
            int bytesRead = m_stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == -1)
                throw new IOException();

            Array.Resize(ref buffer, bytesRead);
            return buffer;
        }
    }
}
