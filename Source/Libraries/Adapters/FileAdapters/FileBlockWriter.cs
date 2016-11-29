//******************************************************************************************************
//  FileBlockWriter.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  04/26/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using GSF;
using GSF.Diagnostics;
using GSF.IO;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.UI.Editors;

namespace FileAdapters
{
    /// <summary>
    /// Output adapter that receives blocks of data as measurements and writes them to a file.
    /// </summary>
    [Description("FileBlockWriter: Receives buffer block measurements and writes them to files")]
    public class FileBlockWriter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_outputDirectory;

        private FileStream m_activeFileStream;
        private long m_activeFileSize;
        private long m_bytesWritten;

        private bool m_disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the directory to which files are written.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the directory to which files are written."),
        CustomConfigurationEditor(typeof(FolderBrowserEditor))]
        public string OutputDirectory
        {
            get
            {
                return m_outputDirectory;
            }
            set
            {
                m_outputDirectory = value;
            }
        }

        /// <summary>
        /// Gets the flag that determines if measurements sent to this <see cref="FileBlockWriter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive => false;

        /// <summary>
        /// Gets flag that determines if the data output stream connects asynchronously.
        /// </summary>
        protected override bool UseAsyncConnect => false;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="FileBlockWriter"/>.
        /// </summary>
        public override void Initialize()
        {
            const string errorMessage = @"{0} is missing from Settings - Example: outputDirectory=C:\Files";
            Dictionary<string, string> settings;
            string setting;

            base.Initialize();
            settings = Settings;

            // Required parameters

            if (!settings.TryGetValue("outputDirectory", out setting))
                throw new ArgumentException(string.Format(errorMessage, "outputDirectory"));

            m_outputDirectory = FilePath.GetAbsolutePath(setting);

            if (!Directory.Exists(m_outputDirectory))
                Directory.CreateDirectory(m_outputDirectory);
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="FileBlockWriter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if ((object)m_activeFileStream != null)
                return $"Currently writing to file {Path.GetFileName(m_activeFileStream.Name)}".CenterText(maxLength);

            return $"{FilePath.GetFileList(Path.Combine(m_outputDirectory, "*")).Length} files written by {Name}".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to data output stream.
        /// </summary>
        protected override void AttemptConnection()
        {
            // Nothing to connect to until we start receiving file data
        }

        /// <summary>
        /// Attempts to disconnect from data output stream.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_activeFileStream != null)
            {
                m_activeFileStream.Dispose();
                m_activeFileStream = null;
            }
        }

        /// <summary>
        /// Writes buffer blocks out to files.
        /// </summary>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            BufferBlockMeasurement bufferBlockMeasurement;

            foreach (IMeasurement measurement in measurements)
            {
                bufferBlockMeasurement = measurement as BufferBlockMeasurement;

                // Only buffer block measurements can be processed
                if ((object)bufferBlockMeasurement != null)
                    ProcessBufferBlockMeasurement(bufferBlockMeasurement);
            }
        }

        private void ProcessBufferBlockMeasurement(BufferBlockMeasurement measurement)
        {
            byte[] bufferBlock = measurement.Buffer;
            int index = 1;

            if (bufferBlock[0] != 0)
            {
                // Start of a new file - read file info
                int fileNameByteLength = BigEndian.ToInt32(bufferBlock, 1);
                string fileName = Encoding.Unicode.GetString(bufferBlock, 5, fileNameByteLength);
                long fileSize = BigEndian.ToInt64(bufferBlock, 5 + fileNameByteLength);

                // Notify of new file creation
                OnStatusMessage(MessageLevel.Info, "Now writing to file {0}...", fileName);

                // ReSharper disable once UnusedVariable > Justification: Implementation pattern closes any existing stream

                // Create new file
                using (FileStream activeFileStream = m_activeFileStream)
                    m_activeFileStream = File.Create(Path.Combine(m_outputDirectory, fileName));

                m_activeFileStream.SetLength(fileSize);
                m_activeFileSize = fileSize;
                m_bytesWritten = 0L;

                // Advance buffer pointer to file data
                index = 1 + 4 + fileNameByteLength + 8;
            }

            if ((object)m_activeFileStream != null)
            {
                // Write data into the file
                int bytesOfData = measurement.Length - index;
                m_activeFileStream.Write(bufferBlock, index, bytesOfData);
                m_bytesWritten += bytesOfData;

                // Close the file when we detect that
                // we've written all bytes to the file
                if (m_bytesWritten >= m_activeFileSize)
                {
                    OnStatusMessage(MessageLevel.Info, "Finished writing to file {0}.", Path.GetFileName(m_activeFileStream.Name));
                    m_activeFileStream.Dispose();
                    m_activeFileStream = null;
                    m_bytesWritten = 0L;
                }
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileBlockWriter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_activeFileStream != null)
                        {
                            m_activeFileStream.Dispose();
                            m_activeFileStream = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        #endregion
    }
}
