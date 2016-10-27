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
using GSF.IO;

namespace GSF.EMAX
{
    /// <summary>
    /// EMAX data file(s) parser.
    /// </summary>
    public class Parser : IDisposable
    {
        #region [ Members ]

        // Fields
        private ControlFile m_controlFile;
        private string m_fileName;
        private bool m_disposed;
        private FileStream[] m_fileStreams;
        private DateTime m_timestamp;
        private double[] m_values;
        private ushort[] m_eventGroups;
        private DateTime m_baseTime;
        private TimeZoneInfo m_sourceTimeZone;
        private bool m_timeError;

        #endregion

        #region [ Constructors ]

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
        /// Gets or sets associated EMAX control file for this <see cref="Parser"/>.
        /// </summary>
        /// <remarks>
        /// This is similar in function to a COMTRADE schema file.
        /// </remarks>
        public ControlFile ControlFile
        {
            get
            {
                return m_controlFile;
            }
            set
            {
                m_controlFile = value;

                if ((object)m_controlFile != null)
                {
                    if (m_controlFile.AnalogChannelCount > 0)
                        m_values = new double[m_controlFile.AnalogChannelCount];
                    else
                        throw new InvalidOperationException("Invalid control file: total analog channels defined in control file is zero.");

                    if (m_controlFile.EventGroupCount > 0)
                        m_eventGroups = new ushort[m_controlFile.EventGroupCount];
                    else
                        throw new InvalidOperationException("Invalid control file: total event groups defined in control file is zero.");

                    SYSTEM_PARAMETERS systemParameters = m_controlFile.SystemParameters;

                    m_baseTime = systemParameters.FaultTime.BaselinedTimestamp(BaselineTimeInterval.Year);
                    m_sourceTimeZone = systemParameters.GetTimeZoneInfo();
                }
                else
                {
                    m_values = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets EMAX data filename.
        /// </summary>
        /// <remarks>
        /// If there are more than one data files in a set (e.g., RCL/RCU), this should be set to first file name in the set, e.g., DATA123.RCL.
        /// </remarks>
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;
            }
        }

        /// <summary>
        /// Gets timestamp of current record in the timezone of provided IRIG signal.
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return m_timestamp;
            }
        }

        /// <summary>
        /// Gets a flag that indicates whether the parser encountered an error
        /// while parsing the timestamp in the last call to <see cref="ReadNext"/>.
        /// </summary>
        public bool TimeError
        {
            get
            {
                return m_timeError;
            }
        }

        /// <summary>
        /// Attempts to get current timestamp converted to UTC.
        /// </summary>
        /// <remarks>
        /// This will only be accurate if timezone configured in device matches IRIG clock.
        /// </remarks>
        public DateTime TimestampAsUtc
        {
            get
            {
                return TimeZoneInfo.ConvertTimeToUtc(m_timestamp, m_sourceTimeZone);
            }
        }

        /// <summary>
        /// Gets values of current record.
        /// </summary>
        public double[] Values
        {
            get
            {
                return m_values;
            }
        }

        /// <summary>
        /// Gets event groups for current record.
        /// </summary>
        public ushort[] EventGroups
        {
            get
            {
                return m_eventGroups;
            }
        }

        // Gets total number of offset bytes in the file
        private int OffsetBytes
        {
            get
            {
                return m_controlFile.SystemParameters.channel_offset * 2;
            }
        }

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
        /// Opens all EMAX data file streams.
        /// </summary>
        public void OpenFiles()
        {
            if (string.IsNullOrWhiteSpace(m_fileName))
                throw new InvalidOperationException("Initial EMAX data file name was not specified, cannot open file(s).");

            if (!File.Exists(m_fileName))
                throw new FileNotFoundException(string.Format("Specified EMAX data file \"{0}\" was not found, cannot open file(s).", m_fileName));

            string extension = FilePath.GetExtension(m_fileName);
            string[] fileNames;

            if (extension.Equals(".RCU", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Please specify the .RCL file instead of the .RCU as the initial file - the .RCU will be automatically loaded: " + m_fileName);

            if (extension.Equals(".RCD", StringComparison.OrdinalIgnoreCase))
            {
                // RCD files mean there is only one card in the system
                fileNames = new string[1];
                fileNames[0] = m_fileName;
            }
            else if (extension.Equals(".RCL", StringComparison.OrdinalIgnoreCase))
            {
                // RCL files mean there are two cards in the system - one low (RCL) and one high (RCU)
                fileNames = new string[2];
                fileNames[0] = m_fileName;
                fileNames[1] = Path.Combine(FilePath.GetDirectoryName(m_fileName), FilePath.GetFileNameWithoutExtension(m_fileName) + ".RCU");
            }
            else
            {
                throw new InvalidOperationException("Specified file name does not have a valid EMAX extension: " + m_fileName);
            }

            // Create a new file stream for each file
            m_fileStreams = new FileStream[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
                m_fileStreams[i] = new FileStream(fileNames[i], FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Closes all EMAX data file streams.
        /// </summary>
        public void CloseFiles()
        {
            if ((object)m_fileStreams != null)
            {
                foreach (FileStream fileStream in m_fileStreams)
                {
                    if ((object)fileStream != null)
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                    }
                }
            }

            m_fileStreams = null;
        }

        /// <summary>
        /// Reads next EMAX record.
        /// </summary>
        /// <returns><c>true</c> if read succeeded; otherwise <c>false</c> if end of data set was reached.</returns>
        public bool ReadNext()
        {
            if ((object)m_fileStreams == null)
                throw new InvalidOperationException("EMAX data files are not open, cannot read next record.");

            if ((object)m_controlFile == null)
                throw new InvalidOperationException("No EMAX schema has been defined, cannot read records.");

            byte[] buffer = null;

            FileStream currentFile;
            int recordLength;
            ushort[] clockWords = new ushort[4];
            ushort value;
            int index;
            double scalingFactor;

            for (int streamIndex = 0; streamIndex < m_fileStreams.Length; streamIndex++)
            {
                currentFile = m_fileStreams[streamIndex];
                recordLength = m_controlFile.FrameLength[streamIndex];
                index = 4;

                if ((object)buffer == null || buffer.Length < recordLength)
                    buffer = new byte[recordLength];

                // Read next record from file
                int bytesRead = currentFile.Read(buffer, 0, recordLength);

                if (bytesRead == 0)
                    return false;

                if (bytesRead == recordLength)
                {
                    int start = streamIndex * 64;
                    int end = Math.Min(start + 64, m_values.Length);

                    // Parse all analog record values
                    for (int i = start; i < end; i++)
                    {
                        // Read next value
                        value = LittleEndian.ToUInt16(buffer, index);

                        if (m_controlFile.ScalingFactors.TryGetValue(i, out scalingFactor))
                        {
                            if (value >= 32768)
                                m_values[i] = (65535 - value) / 32768.0D * scalingFactor;
                            else
                                m_values[i] = -value / 32768.0D * scalingFactor;
                        }
                        else if (m_controlFile.DataSize == DataSize.Bits12)
                        {
                            m_values[i] = value >> 4;
                        }
                        else
                        {
                            m_values[i] = value;
                        }

                        index += 2;
                    }

                    // There are always either 32 or 64 data words depending on the number of configured channels
                    index = (m_controlFile.AnalogChannelCount <= 32) ? 32 : 64;
                    index *= sizeof(ushort);

                    // Read event group values (first set)
                    for (int i = 0; i < 4; i++)
                    {
                        m_eventGroups[i] = LittleEndian.ToUInt16(buffer, index);
                        index += 2;
                    }

                    // Read timestamp from next four word values
                    for (int i = 0; i < 4; i++)
                    {
                        clockWords[i] = LittleEndian.ToUInt16(buffer, index);
                        index += 2;
                    }

                    m_timestamp = ParseTimestamp(clockWords);

                    if (ControlFile.ConfiguredAnalogChannels > 32 && ControlFile.SystemSettings.samples_per_second <= 5760)
                    {
                        // Read next set of event group values
                        for (int i = 4; i < 8; i++)
                        {
                            m_eventGroups[i] = LittleEndian.ToUInt16(buffer, index);
                            index += 2;
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Failed to read enough bytes from EMAX file for a record as defined by control file - possible control file/data file mismatch or file corruption.");
                }
            }

            return true;
        }

        private DateTime ParseTimestamp(ushort[] clockWords)
        {
            if ((object)clockWords == null)
                throw new NullReferenceException("Clock words array was null - cannot parse timestamp");

            if (clockWords.Length != 4)
                throw new InvalidOperationException("Clock words array must have four values - cannot parse timestamp");

            int days, hours, minutes, seconds, milliseconds, microseconds;
            byte highByte, lowByte;

            m_timeError = clockWords
                .SelectMany(clockWord => new byte[]
                {
                    clockWord.HighByte().HighNibble(),
                    clockWord.HighByte().LowNibble(),
                    clockWord.LowByte().HighNibble(),
                    clockWord.LowByte().LowNibble()
                })
                .Take(15)
                .Any(b => b > 9);

            if (m_timeError)
                return DateTime.MaxValue;

            highByte = clockWords[0].HighByte();
            lowByte = clockWords[0].LowByte();

            days = highByte.HighNibble() * 100 + highByte.LowNibble() * 10 + lowByte.HighNibble();
            hours = lowByte.LowNibble() * 10;

            highByte = clockWords[1].HighByte();
            lowByte = clockWords[1].LowByte();

            hours += highByte.HighNibble();
            minutes = highByte.LowNibble() * 10 + lowByte.HighNibble();
            seconds = lowByte.LowNibble() * 10;

            highByte = clockWords[2].HighByte();
            lowByte = clockWords[2].LowByte();

            seconds += highByte.HighNibble();

            milliseconds = highByte.LowNibble() * 100 + lowByte.HighNibble() * 10 + lowByte.LowNibble();

            if (milliseconds > 999)
                milliseconds = 0;

            highByte = clockWords[3].HighByte();
            lowByte = clockWords[3].LowByte();

            microseconds = highByte.HighNibble() * 100 + highByte.LowNibble() * 10 + lowByte.HighNibble();

            if (microseconds > 999)
                microseconds = 0;

            return m_baseTime
                    .AddDays(days - 1) // Base time starts at day one, so we subtract one for target day
                    .AddHours(hours)
                    .AddMinutes(minutes)
                    .AddSeconds(seconds)
                    .AddMilliseconds(milliseconds)
                    .AddTicks(Ticks.FromMicroseconds(microseconds));
        }

        #endregion
    }
}
