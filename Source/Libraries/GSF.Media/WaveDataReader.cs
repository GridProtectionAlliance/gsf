//******************************************************************************************************
//  WaveDataReader.cs - Gbtc
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
//  06/29/2011 - Stephen C. Wills
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/**************************************************************************\
   Copyright © 2011 - J. Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

#endregion

using System;
using System.IO;

namespace GSF.Media
{
    /// <summary>
    /// Represents wave data coming from a stream.
    /// </summary>
    /// <remarks>
    /// This class is meant to be used when loading the entirety
    /// of a wave file into memory is not a viable option. With
    /// it, samples can be read directly from the data stream in
    /// the order that they appear.
    /// </remarks>
    public class WaveDataReader : IDisposable
    {
        #region [ Members ]

        // Fields
        private readonly WaveFormatChunk m_format;
        private Stream m_waveStream;
        private readonly int m_blockSize;
        private readonly int m_sampleSize;
        private readonly int m_channels;
        private readonly TypeCode m_sampleType;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="WaveDataReader"/> class.
        /// </summary>
        /// <param name="waveFormat">The format of the wave stream.</param>
        /// <param name="waveStream">The stream containing wave data.</param>
        public WaveDataReader(WaveFormatChunk waveFormat, Stream waveStream)
        {
            m_format = waveFormat ?? DEFAULT_WAVE_FORMAT_CHUNK;
            m_waveStream = waveStream;
            m_blockSize = m_format.BlockAlignment;
            m_sampleSize = m_format.BitsPerSample / 8;
            m_channels = m_format.Channels;
            m_sampleType = m_format.GetSampleTypeCode();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WaveDataReader"/> class.
        /// </summary>
        /// <param name="waveStream">The stream containing wave data.</param>
        public WaveDataReader(Stream waveStream)
            : this(null, waveStream)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the format of the wave data.
        /// </summary>
        public WaveFormatChunk Format
        {
            get
            {
                return m_format;
            }
        }

        /// <summary>
        /// Gets the underlying stream providing wave data.
        /// </summary>
        public Stream WaveStream
        {
            get
            {
                return m_waveStream;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads and returns the next sample from the stream.
        /// </summary>
        /// <returns>
        /// The next sample from the stream. The sample is returned as
        /// an array of <see cref="LittleBinaryValue"/>s. Each
        /// LittleBinaryValue represents a different channel.
        /// A return value of null indicates the end of stream has been reached.
        /// </returns>
        public LittleBinaryValue[] GetNextSample()
        {
            byte[] buffer = new byte[m_blockSize];
            int bytesRead = m_waveStream.Read(buffer, 0, m_blockSize);
            LittleBinaryValue[] sample;

            if (bytesRead == m_blockSize)
            {
                sample = new LittleBinaryValue[m_channels];

                for (int i = 0; i < m_channels; i++)
                    sample[i] = new LittleBinaryValue(m_sampleType, buffer, i * m_sampleSize, m_sampleSize);

                return sample;
            }

            return null;
        }

        /// <summary>
        /// Closes the underlying stream.
        /// </summary>
        public void Close()
        {
            m_waveStream?.Close();
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="WaveDataReader"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="WaveDataReader"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_waveStream != null)
                        {
                            m_waveStream.Dispose();
                            m_waveStream = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly WaveFormatChunk DEFAULT_WAVE_FORMAT_CHUNK = new WaveFormatChunk(44100, 16, 2, 0x1);

        // Static Methods

        /// <summary>
        /// Creates a <see cref="WaveDataReader"/> instance created from a WAV file.
        /// </summary>
        /// <param name="fileName">The name of the WAV file.</param>
        /// <returns>The WaveData instance created from a WAV file.</returns>
        public static WaveDataReader FromFile(string fileName)
        {
            return FromStream(File.OpenRead(fileName));
        }

        /// <summary>
        /// Creates a <see cref="WaveDataReader"/> instance created from a WAV stream.
        /// </summary>
        /// <param name="waveStream">The WAV stream. The data in the stream must include all headers that would be present in a WAV file.</param>
        /// <returns>The WaveData instance created from a WAV stream.</returns>
        /// <remarks>
        /// This method is similar to the <see cref="WaveDataReader(Stream)"/> constructor,
        /// however this method will first search the stream for a format chunk in order to set
        /// up the WaveData object with proper format info.
        /// </remarks>
        public static WaveDataReader FromStream(Stream waveStream)
        {
            RiffChunk riffChunk;
            WaveFormatChunk waveFormat = null;
            WaveDataReader waveData = null;

            while ((object)waveData == null)
            {
                riffChunk = RiffChunk.ReadNext(waveStream);

                switch (riffChunk.TypeID)
                {
                    case RiffHeaderChunk.RiffTypeID:
                        // ReSharper disable once ObjectCreationAsStatement
                        new RiffHeaderChunk(riffChunk, waveStream, "WAVE");
                        break;
                    case WaveFormatChunk.RiffTypeID:
                        waveFormat = new WaveFormatChunk(riffChunk, waveStream);
                        break;
                    case WaveDataChunk.RiffTypeID:
                        waveData = new WaveDataReader(waveFormat, waveStream);
                        break;
                    default:
                        // Skip unnecessary sections
                        waveStream.Seek(riffChunk.ChunkSize, SeekOrigin.Current);
                        break;
                }
            }

            return waveData;
        }

        #endregion
    }
}
