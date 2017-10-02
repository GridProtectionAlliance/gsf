//******************************************************************************************************
//  Parser.cs - Gbtc
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
//  06/17/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GSF.IO;

namespace GSF.COMTRADE
{
    /// <summary>
    /// COMTRADE data file(s) parser.
    /// </summary>
    public class Parser : IDisposable
    {
        #region [ Members ]

        // Fields
        private Schema m_schema;
        private bool m_disposed;
        private FileStream[] m_fileStreams;
        private StreamReader[] m_fileReaders;
        private int m_streamIndex;
        private double[] m_primaryValues;
        private double[] m_secondaryValues;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the COMTRADE <see cref="Parser"/>.
        /// </summary>
        public Parser()
        {
            InferTimeFromSampleRates = true;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="Parser"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~Parser()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets associated COMTRADE schema for this <see cref="Parser"/>.
        /// </summary>
        public Schema Schema
        {
            get
            {
                return m_schema;
            }
            set
            {
                m_schema = value;

                if ((object)m_schema != null)
                {
                    if (m_schema.TotalChannels > 0)
                        Values = new double[m_schema.TotalChannels];
                    else
                        throw new InvalidOperationException("Invalid schema: total channels defined in schema is zero.");

                    if (m_schema.TotalSampleRates == 0)
                        InferTimeFromSampleRates = false;
                }
                else
                {
                    Values = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets COMTRADE data filename. If there are more than one data files in a set, this should be set to first file name in the set, e.g., DATA123.D00.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if time should be inferred from sample rates.
        /// </summary>
        public bool InferTimeFromSampleRates { get; set; }

        /// <summary>
        /// Gets timestamp of current record.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Gets values of current record.
        /// </summary>
        public double[] Values { get; private set; }

        /// <summary>
        /// Gets values of current record with secondary analog channels scaled to primary values.
        /// </summary>
        public double[] PrimaryValues
        {
            get
            {
                if ((object)m_primaryValues == null)
                {
                    m_primaryValues = new double[Values.Length];

                    for (int i = 0; i < m_primaryValues.Length; i++)
                    {
                        double value = Values[i];

                        if (i < m_schema.AnalogChannels.Length)
                        {
                            if (char.ToUpper(m_schema.AnalogChannels[i].ScalingIdentifier) == 'S')
                                value *= m_schema.AnalogChannels[i].PrimaryRatio / m_schema.AnalogChannels[i].SecondaryRatio;
                        }

                        m_primaryValues[i] = value;
                    }
                }

                return m_primaryValues;
            }
        }

        /// <summary>
        /// Gets values of current record with primary analog channels scaled to secondary values.
        /// </summary>
        public double[] SecondaryValues
        {
            get
            {
                if ((object)m_secondaryValues == null)
                {
                    m_secondaryValues = new double[Values.Length];

                    for (int i = 0; i < m_secondaryValues.Length; i++)
                    {
                        double value = Values[i];

                        if (i < m_schema.AnalogChannels.Length)
                        {
                            if (char.ToUpper(m_schema.AnalogChannels[i].ScalingIdentifier) == 'P')
                                value *= m_schema.AnalogChannels[i].SecondaryRatio / m_schema.AnalogChannels[i].PrimaryRatio;
                        }

                        m_secondaryValues[i] = value;
                    }
                }

                return m_secondaryValues;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if schemas that include a time code for a UTC offset
        /// should apply the adjustment so that timestamps are parsed as UTC.
        /// </summary>
        public bool AdjustToUTC { get; set; } = true;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="Parser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Parser"/> object and optionally releases the managed resources.
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
                        CloseFiles();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Opens all COMTRADE data file streams.
        /// </summary>
        public void OpenFiles()
        {
            if (string.IsNullOrWhiteSpace(FileName))
                throw new InvalidOperationException("First COMTRADE data file name was not specified, cannot open files.");

            if (!File.Exists(FileName))
                throw new FileNotFoundException($"Specified COMTRADE data file \"{FileName}\" was not found, cannot open files.");

            // Get all data files in the collection
            const string FileRegex = @"(?:\.dat|\.d\d\d)$";
            string directory = FilePath.GetDirectoryName(FileName);
            string rootFileName = FilePath.GetFileNameWithoutExtension(FileName);
            string extension = FilePath.GetExtension(FileName).Substring(0, 2) + "*";

            string[] fileNames = FilePath.GetFileList(Path.Combine(directory, rootFileName + extension))
                .Where(fileName => Regex.IsMatch(fileName, FileRegex, RegexOptions.IgnoreCase))
                .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            // Create a new file stream for each file
            m_fileStreams = new FileStream[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
                m_fileStreams[i] = new FileStream(fileNames[i], FileMode.Open, FileAccess.Read, FileShare.Read);

            m_streamIndex = 0;
        }

        /// <summary>
        /// Closes all COMTRADE data file streams.
        /// </summary>
        public void CloseFiles()
        {
            if ((object)m_fileStreams != null)
            {
                foreach (FileStream fileStream in m_fileStreams)
                    fileStream?.Dispose();
            }

            m_fileStreams = null;
        }

        /// <summary>
        /// Reads next COMTRADE record.
        /// </summary>
        /// <returns><c>true</c> if read succeeded; otherwise <c>false</c> if end of data set was reached.</returns>
        public bool ReadNext()
        {
            if ((object)m_fileStreams == null)
                throw new InvalidOperationException("COMTRADE data files are not open, cannot read next record.");

            if ((object)m_schema == null)
                throw new InvalidOperationException("No COMTRADE schema has been defined, cannot read records.");

            if (m_streamIndex > m_fileStreams.Length)
                throw new EndOfStreamException("All COMTRADE data has been read, cannot read more records.");

            m_primaryValues = null;
            m_secondaryValues = null;

            switch (m_schema.FileType)
            {
                case FileType.Ascii:
                    return ReadNextAscii();
                case FileType.Binary:
                    return ReadNextBinary();
                case FileType.Binary32:
                    return ReadNextBinary32();
                case FileType.Float32:
                    return ReadNextFloat32();
                default:
                    return false;
            }
        }

        // Handle ASCII file read
        private bool ReadNextAscii()
        {
            if ((object)m_fileReaders == null)
            {
                m_fileReaders = new StreamReader[m_fileStreams.Length];

                for (int i = 0; i < m_fileStreams.Length; i++)
                    m_fileReaders[i] = new StreamReader(m_fileStreams[i]);
            }

            // Read next line of record values
            StreamReader reader = m_fileReaders[m_streamIndex];
            string line = reader.ReadLine();
            string[] elems = ((object)line != null) ? line.Split(',') : null;

            // See if we have reached the end of this file
            if ((object)elems == null || elems.Length != Values.Length + 2)
            {
                if (reader.EndOfStream)
                {
                    m_streamIndex++;

                    // There is more to read if there is another file
                    return m_streamIndex < m_fileStreams.Length && ReadNext();
                }

                throw new InvalidOperationException("COMTRADE schema does not match number of elements found in ASCII data file.");
            }

            // Parse row of data
            uint sample = uint.Parse(elems[0]);

            // Get timestamp of this record
            Timestamp = DateTime.MinValue;

            // If sample rates are defined, this is the preferred method for timestamp resolution
            if (InferTimeFromSampleRates && m_schema.SampleRates.Length > 0)
            {
                // Find rate for given sample
                SampleRate sampleRate = m_schema.SampleRates.LastOrDefault(sr => sample <= sr.EndSample);

                if (sampleRate.Rate > 0.0D)
                    Timestamp = new DateTime(Ticks.FromSeconds(1.0D / sampleRate.Rate * sample) + m_schema.StartTime.Value);
            }

            // Fall back on specified microsecond time
            if (Timestamp == DateTime.MinValue)
                Timestamp = new DateTime(Ticks.FromMicroseconds(uint.Parse(elems[1]) * m_schema.TimeFactor) + m_schema.StartTime.Value);

            // Apply timestamp offset to restore UTC timezone
            if (AdjustToUTC)
            {
                TimeOffset offset = Schema.TimeCode ?? new TimeOffset();
                Timestamp = new DateTime(Timestamp.Ticks + offset.TickOffset, DateTimeKind.Utc);
            }

            // Parse all record values
            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = double.Parse(elems[i + 2]);

                if (i < m_schema.AnalogChannels.Length)
                {
                    Values[i] *= m_schema.AnalogChannels[i].Multiplier;
                    Values[i] += m_schema.AnalogChannels[i].Adder;
                }
            }

            return true;
        }

        // Handle binary file read
        private bool ReadNextBinary()
        {
            FileStream currentFile = m_fileStreams[m_streamIndex];
            int recordLength = m_schema.BinaryRecordLength;
            byte[] buffer = new byte[recordLength];

            // Read next record from file
            int bytesRead = currentFile.Read(buffer, 0, recordLength);

            // See if we have reached the end of this file
            if (bytesRead == 0)
            {
                m_streamIndex++;

                // There is more to read if there is another file
                return m_streamIndex < m_fileStreams.Length && ReadNext();
            }

            if (bytesRead == recordLength)
            {
                int index = ReadTimestamp(buffer);

                // Parse all analog record values
                for (int i = 0; i < m_schema.AnalogChannels.Length; i++)
                {
                    // Read next value
                    Values[i] = LittleEndian.ToInt16(buffer, index) * m_schema.AnalogChannels[i].Multiplier + m_schema.AnalogChannels[i].Adder;
                    index += 2;
                }

                int valueIndex = m_schema.AnalogChannels.Length;
                int digitalWords = m_schema.DigitalWords;
                ushort digitalWord;

                for (int i = 0; i < digitalWords; i++)
                {
                    // Read next digital word
                    digitalWord = LittleEndian.ToUInt16(buffer, index);
                    index += 2;

                    // Distribute each bit of digital word through next 16 digital values
                    for (int j = 0; j < 16 && valueIndex < Values.Length; j++, valueIndex++)
                        Values[valueIndex] = digitalWord.CheckBits(BitExtensions.BitVal(j)) ? 1.0D : 0.0D;
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to read enough bytes from COMTRADE file for a record as defined by schema - possible schema/data file mismatch or file corruption.");
            }

            return true;
        }

        // Handle binary32 file read
        private bool ReadNextBinary32()
        {
            FileStream currentFile = m_fileStreams[m_streamIndex];
            int recordLength = m_schema.Binary32RecordLength;
            byte[] buffer = new byte[recordLength];

            // Read next record from file
            int bytesRead = currentFile.Read(buffer, 0, recordLength);

            // See if we have reached the end of this file
            if (bytesRead == 0)
            {
                m_streamIndex++;

                // There is more to read if there is another file
                return m_streamIndex < m_fileStreams.Length && ReadNext();
            }

            if (bytesRead == recordLength)
            {
                int index = ReadTimestamp(buffer);

                // Parse all analog record values
                for (int i = 0; i < m_schema.AnalogChannels.Length; i++)
                {
                    // Read next value
                    Values[i] = LittleEndian.ToSingle(buffer, index) * m_schema.AnalogChannels[i].Multiplier + m_schema.AnalogChannels[i].Adder;
                    index += 4;
                }

                int valueIndex = m_schema.AnalogChannels.Length;
                int digitalWords = m_schema.DigitalWords;
                ushort digitalWord;

                for (int i = 0; i < digitalWords; i++)
                {
                    // Read next digital word
                    digitalWord = LittleEndian.ToUInt16(buffer, index);
                    index += 2;

                    // Distribute each bit of digital word through next 16 digital values
                    for (int j = 0; j < 16 && valueIndex < Values.Length; j++, valueIndex++)
                        Values[valueIndex] = digitalWord.CheckBits(BitExtensions.BitVal(j)) ? 1.0D : 0.0D;
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to read enough bytes from COMTRADE file for a record as defined by schema - possible schema/data file mismatch or file corruption.");
            }

            return true;
        }

        // Handle float32 file read
        private bool ReadNextFloat32()
        {
            FileStream currentFile = m_fileStreams[m_streamIndex];
            int recordLength = m_schema.Float32RecordLength;
            byte[] buffer = new byte[recordLength];

            // Read next record from file
            int bytesRead = currentFile.Read(buffer, 0, recordLength);

            // See if we have reached the end of this file
            if (bytesRead == 0)
            {
                m_streamIndex++;

                // There is more to read if there is another file
                return m_streamIndex < m_fileStreams.Length && ReadNext();
            }

            if (bytesRead == recordLength)
            {
                int index = ReadTimestamp(buffer);

                // Parse all analog record values
                for (int i = 0; i < m_schema.AnalogChannels.Length; i++)
                {
                    // Read next value
                    Values[i] = LittleEndian.ToSingle(buffer, index) * m_schema.AnalogChannels[i].Multiplier + m_schema.AnalogChannels[i].Adder;
                    index += 4;
                }

                int valueIndex = m_schema.AnalogChannels.Length;
                int digitalWords = m_schema.DigitalWords;
                ushort digitalWord;

                for (int i = 0; i < digitalWords; i++)
                {
                    // Read next digital word
                    digitalWord = LittleEndian.ToUInt16(buffer, index);
                    index += 2;

                    // Distribute each bit of digital word through next 16 digital values
                    for (int j = 0; j < 16 && valueIndex < Values.Length; j++, valueIndex++)
                        Values[valueIndex] = digitalWord.CheckBits(BitExtensions.BitVal(j)) ? 1.0D : 0.0D;
                }
            }
            else
            {
                throw new InvalidOperationException("Failed to read enough bytes from COMTRADE file for a record as defined by schema - possible schema/data file mismatch or file corruption.");
            }

            return true;
        }

        private int ReadTimestamp(byte[] buffer)
        {
            int index = 0;
            
            // Read sample index
            uint sample = LittleEndian.ToUInt32(buffer, index);
            index += 4;

            // Get timestamp of this record
            Timestamp = DateTime.MinValue;

            // If sample rates are defined, this is the preferred method for timestamp resolution
            if (InferTimeFromSampleRates && m_schema.SampleRates.Length > 0)
            {
                // Find rate for given sample
                SampleRate sampleRate = m_schema.SampleRates.LastOrDefault(sr => sample <= sr.EndSample);

                if (sampleRate.Rate > 0.0D)
                    Timestamp = new DateTime(Ticks.FromSeconds(1.0D / sampleRate.Rate * sample) + m_schema.StartTime.Value);
            }

            // Read microsecond timestamp
            uint microseconds = LittleEndian.ToUInt32(buffer, index);
            index += 4;

            // Fall back on specified microsecond time
            if (Timestamp == DateTime.MinValue)
                Timestamp = new DateTime(Ticks.FromMicroseconds(microseconds * m_schema.TimeFactor) + m_schema.StartTime.Value);

            // Apply timestamp offset to restore UTC timezone
            if (AdjustToUTC)
            {
                TimeOffset offset = Schema.TimeCode ?? new TimeOffset();
                Timestamp = new DateTime(Timestamp.Ticks + offset.TickOffset, DateTimeKind.Utc);
            }

            return index;
        }

        #endregion
    }
}
