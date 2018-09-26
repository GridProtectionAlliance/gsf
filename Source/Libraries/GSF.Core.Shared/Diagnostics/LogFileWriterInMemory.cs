//******************************************************************************************************
//  LogEventPublisherDetails.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/24/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System.IO;
using Ionic.Zlib;
using GSF.IO;

namespace GSF.Diagnostics
{
    /// <summary>
    /// A log subscriber that will log messages to a file.
    /// </summary>
    public class LogFileWriterInMemory
    {
        private readonly MemoryStream m_tmpStream;
        private MemoryStream m_stream;
        private DeflateStream m_zipStream;
        private LogMessageSaveHelper m_saveHelper;
        private int m_logCount;
        private byte[] m_tmpBuffer;

        /// <summary>
        /// Creates a LogFileWriter that initially queues message
        /// </summary>
        public LogFileWriterInMemory()
        {
            m_saveHelper = LogMessageSaveHelper.Create();

            m_stream = new MemoryStream();
            m_stream.Write(282497); //VersionNumber: Compressed. With LogSaveHelper

            m_zipStream = new DeflateStream(m_stream, CompressionMode.Compress, CompressionLevel.Level1, true);
            m_zipStream.FlushMode = FlushType.Sync;

            m_tmpBuffer = new byte[40960];
            m_tmpStream = new MemoryStream();
            m_logCount = 0;
        }


        /// <summary>
        /// The number of logs that have been written to this file.
        /// </summary>
        public int LogCount
        {
            get
            {
                return m_logCount;
            }
        }

        /// <summary>
        /// Writes the specified log to the file
        /// </summary>
        /// <param name="log"></param>
        public void Write(LogMessage log)
        {
            m_tmpStream.Position = 0;
            m_tmpStream.Write(true);
            log.Save(m_tmpStream, m_saveHelper);
            long length = m_tmpStream.Position;
            m_tmpStream.Position = 0;
            m_tmpStream.CopyTo(m_zipStream, length, m_tmpBuffer);

            if (log.Level >= MessageLevel.Info)
            {
                m_zipStream.Flush();
            }

            m_logCount++;
        }

        public byte[] ToByteArray()
        {
            m_zipStream.Write(false);
            m_zipStream.Flush();
            m_zipStream.Dispose();
            m_stream.Write(false);
            return m_stream.ToArray();
        }

    }
}
