//******************************************************************************************************
//  FtpInputDataStream.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/23/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/06/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

//*******************************************************************************************************
//
//   Code based on the following project:
//        http://www.codeproject.com/KB/IP/net_ftp_upload.aspx
//  
//   Copyright Alex Kwok & Uwe Keim 
//
//   The Code Project Open License (CPOL):
//        http://www.codeproject.com/info/cpol10.aspx
//
//*******************************************************************************************************

#endregion

using System;
using System.Net.Sockets;

namespace GSF.Net.Ftp
{
    /// <summary>
    /// Defines a FTP data input stream for remote files.
    /// </summary>
    public class FtpInputDataStream : FtpDataStream
    {
        internal FtpInputDataStream(FtpControlChannel ctrl, TcpClient client)
            : base(ctrl, client)
        {
        }

        /// <summary>
        /// Cannot write to input stream, method is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">Cannot write to input stream.</exception>
        /// <param name="buffer">A <see cref="Byte"/> array buffer to use.</param>
        /// <param name="count">An <see cref="Int32"/> offset into the buffer.</param>
        /// <param name="offset">An <see cref="Int32"/> count of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Cannot write to input stream, method is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">Cannot write to input stream.</exception>
        /// <param name="b">A <see cref="Byte"/> to write.</param>
        public override void WriteByte(byte b)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns false, cannot write to input stream.
        /// </summary>
        public override bool CanWrite => false;
    }
}
