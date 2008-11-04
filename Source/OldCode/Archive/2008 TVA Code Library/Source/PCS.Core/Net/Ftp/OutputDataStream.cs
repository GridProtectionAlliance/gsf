//*******************************************************************************************************
//  OutputDataStream.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/23/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Net.Sockets;

namespace PCS.Net.Ftp
{
    public class OutputDataStream : FtpDataStream
    {
        internal OutputDataStream(FtpControlChannel ctrl, TcpClient client)
            : base(ctrl, client)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }
    }
}
