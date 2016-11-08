//******************************************************************************************************
//  CorrectiveParser.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  12/18/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;

namespace GSF.EMAX
{
    /// <summary>
    /// EMAX data file(s) parser that calculates timestamps based
    /// on sample rate and attempts to correct inverted values.
    /// </summary>
    public class CorrectiveParser : IDisposable
    {
        #region [ Members ]

        // Fields
        private Parser m_parser;

        private DateTime m_currentSecond;
        private Ticks[] m_subsecondDistribution;
        private int m_currentIndex;

        private DateTime m_calculatedTimestamp;
        private double[] m_correctedValues;

        private TimeZoneInfo m_sourceTimeZone;

        private bool m_disposed;

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="CorrectiveParser"/> class.
        /// </summary>
        public CorrectiveParser()
        {
            m_parser = new Parser();
        }

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
                return m_parser.ControlFile;
            }
            set
            {
                m_parser.ControlFile = value;

                if ((object)value != null)
                {
                    m_correctedValues = new double[Values.Length];
                    m_sourceTimeZone = value.SystemParameters.GetTimeZoneInfo();
                }
                else
                {
                    m_correctedValues = null;
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
                return m_parser.FileName;
            }
            set
            {
                m_parser.FileName = value;
            }
        }

        /// <summary>
        /// Gets timestamp of current record, parsed from the file, in the timezone of provided IRIG signal.
        /// </summary>
        public DateTime ParsedTimestamp
        {
            get
            {
                return m_parser.Timestamp;
            }
        }

        /// <summary>
        /// Attempts to get current timestamp, parsed from the file, converted to UTC.
        /// </summary>
        /// <remarks>
        /// This will only be accurate if timezone configured in device matches IRIG clock.
        /// </remarks>
        public DateTime ParsedTimestampAsUtc
        {
            get
            {
                return TimeZoneInfo.ConvertTimeToUtc(m_calculatedTimestamp, m_sourceTimeZone);
            }
        }

        /// <summary>
        /// Gets calculated timestamp of current record in the timezone of provided IRIG signal.
        /// </summary>
        public DateTime CalculatedTimestamp
        {
            get
            {
                return m_calculatedTimestamp;
            }
        }

        /// <summary>
        /// Attempts to get calculated timestamp of current record converted to UTC.
        /// </summary>
        /// <remarks>
        /// This will only be accurate if timezone configured in device matches IRIG clock.
        /// </remarks>
        public DateTime CalculatedTimestampAsUtc
        {
            get
            {
                return TimeZoneInfo.ConvertTimeToUtc(m_calculatedTimestamp, m_sourceTimeZone);
            }
        }

        /// <summary>
        /// Gets values of current record.
        /// </summary>
        public double[] Values
        {
            get
            {
                return m_parser.Values;
            }
        }

        /// <summary>
        /// Gets corrected values, which may be inverted based on the difference
        /// between the calculated timestamp and the parsed timestamp.
        /// </summary>
        public double[] CorrectedValues
        {
            get
            {
                return m_correctedValues;
            }
        }

        /// <summary>
        /// Gets event groups for current record.
        /// </summary>
        public ushort[] EventGroups
        {
            get
            {
                return m_parser.EventGroups;
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
            long previousMilliseconds;
            long milliseconds;
            int count;

            int sampleRate;
            int distributionIndex;

            m_parser.OpenFiles();

            sampleRate = m_parser.ControlFile.SystemParameters.samples_per_second;
            m_subsecondDistribution = Ticks.SubsecondDistribution(sampleRate);

            using (Parser parser = new Parser())
            {
                parser.ControlFile = m_parser.ControlFile;
                parser.FileName = m_parser.FileName;

                // Open EMAX data file
                parser.OpenFiles();

                m_currentSecond = DateTime.MinValue;
                previousMilliseconds = -1;
                milliseconds = 0;
                count = 0;

                while (parser.ReadNext())
                {
                    if (parser.TimeError)
                        continue;

                    // Set currentSecond to this frame's timestamp
                    m_currentSecond = parser.Timestamp;

                    // Get total milliseconds since epoch
                    milliseconds = m_currentSecond.Ticks / Ticks.PerMillisecond;

                    // If the milliseconds are exactly one millisecond greater than the previous
                    // timestamp's milliseconds, we can accurately find the timestamp of this frame
                    if (previousMilliseconds > 0 && milliseconds - previousMilliseconds == 1)
                        break;

                    // Update previousMilliseconds and count
                    previousMilliseconds = milliseconds;
                    count++;
                }

                if (m_currentSecond == DateTime.MinValue)
                {
                    // If there was an error reading the timestamps in the parser,
                    // calculate the timestamp from the data in the control file
                    TimeZoneInfo tzInfo = parser.ControlFile.SystemParameters.GetTimeZoneInfo();
                    DateTime faultTime = parser.ControlFile.SystemParameters.FaultTime;
                    string daylightName = parser.ControlFile.SystemParameters.time_zone_information.DaylightName;
                    short faultMilliseconds = parser.ControlFile.SystemParameters.mS_time;
                    short prefaultSamples = parser.ControlFile.SystemParameters.prefault_samples;
                    short startOffsetSamples = parser.ControlFile.SystemParameters.start_offset_samples;

                    // Set currentSecond to the timestamp of the first frame
                    m_currentSecond = faultTime.AddMilliseconds(faultMilliseconds).AddSeconds(-(prefaultSamples + startOffsetSamples) / (double)sampleRate);

                    // Get total milliseconds since epoch
                    milliseconds = m_currentSecond.Ticks / Ticks.PerMillisecond;

                    // Timestamps in the control file are stored in UTC (or close to it),
                    // but it seems that sometimes EMAX adjusts timestamps without applying DST
                    // rules so we use the DaylightName of the time zone info as a sanity check
                    // and fall back on the BaseUtcOffset property in these weird cases
                    if (daylightName == tzInfo.DaylightName || daylightName != tzInfo.Id)
                        m_currentSecond = TimeZoneInfo.ConvertTimeFromUtc(m_currentSecond, tzInfo);
                    else
                        m_currentSecond += tzInfo.BaseUtcOffset;

                    m_currentSecond = DateTime.SpecifyKind(m_currentSecond, DateTimeKind.Unspecified);
                }

                // Remove subseconds from currentSecond
                m_currentSecond = m_currentSecond.AddTicks(-(m_currentSecond.Ticks % Ticks.PerSecond));

                // Get the milliseconds since the top of the second
                milliseconds %= 1000;

                // This should get very near to the index of the
                // desired value in the subsecond distribution
                distributionIndex = ((int)milliseconds * sampleRate) / 1000;

                // Scan forward in the subsecond distribution until we are sure we've found the correct index
                while ((long)m_subsecondDistribution[distributionIndex].ToMilliseconds() != milliseconds)
                    distributionIndex++;

                // Set currentIndex to the index in the distribution of the first timestamp in this file
                m_currentIndex = distributionIndex - count;

                // Subtract seconds from currentSecond and add the equivalent number of indexes
                // to currentIndex until currentIndex is greater than or equal to zero
                while (m_currentIndex < 0)
                {
                    m_currentSecond = m_currentSecond.AddSeconds(-1.0D);
                    m_currentIndex += sampleRate;
                }
            }
        }

        /// <summary>
        /// Closes all EMAX data file streams.
        /// </summary>
        public void CloseFiles()
        {
            if ((object)m_parser != null)
            {
                m_parser.Dispose();
                m_parser = null;
            }
        }

        /// <summary>
        /// Reads next EMAX record.
        /// </summary>
        /// <returns><c>true</c> if read succeeded; otherwise <c>false</c> if end of data set was reached.</returns>
        public bool ReadNext()
        {
            double frequency;
            double diff;
            int halfCycles;

            if (m_parser.ReadNext())
            {
                // Calculate the timestamp
                m_calculatedTimestamp = m_currentSecond.AddTicks(m_subsecondDistribution[m_currentIndex]);

                if (!m_parser.TimeError)
                {
                    // Correct the values
                    frequency = m_parser.ControlFile.SystemParameters.frequency;
                    diff = Math.Abs(m_calculatedTimestamp.Subtract(m_parser.Timestamp).TotalSeconds);
                    halfCycles = (int)Math.Round(diff * frequency * 2.0D);

                    if (halfCycles % 2 == 0)
                        Array.Copy(Values, m_correctedValues, m_correctedValues.Length);
                    else
                        Values.Select(v => -v).ToList().CopyTo(m_correctedValues);
                }
                else
                {
                    // We can't determine whether the values need correction
                    // without a proper timestamp from the underlying parser
                    Array.Copy(Values, m_correctedValues, m_correctedValues.Length);
                }

                // Move to the next subsecond in the distribution
                m_currentIndex++;

                if (m_currentIndex == m_subsecondDistribution.Length)
                {
                    // If we reach the end of the subsecond distribution,
                    // add one to currentSecond and reset the currentIndex to zero
                    m_currentSecond = m_currentSecond.AddSeconds(1.0D);
                    m_currentIndex = 0;
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}
