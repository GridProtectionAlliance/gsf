//******************************************************************************************************
//  GZipWriter.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
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
//  11/24/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

namespace GEPDataExtractor
{
    internal class GZipWriter : TextWriter
    {
        private readonly GZipStream m_gzip;
        private bool m_disposed;

        private GZipWriter(GZipStream gzip) => 
            m_gzip = gzip;

        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                m_gzip.Dispose();
            }
            finally
            {
                m_disposed = true;
                base.Dispose(disposing);
            }
        }

        public override Encoding Encoding => 
            Encoding.UTF8;

        public override void Write(char value) => 
            Write(Encoding.GetBytes(new[] { value }));

        public override void Write(string value) => 
            Write(Encoding.GetBytes(value));

        public override void Flush() => 
            m_gzip.Flush();

        public override void Close() =>
            m_gzip.Close();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(byte[] buffer) =>
            m_gzip.Write(buffer, 0, buffer.Length);

        public static GZipWriter CreateBinary(string fileName) =>
            new GZipWriter(new GZipStream(File.Create(fileName, 4096), CompressionMode.Compress));
    }
}
