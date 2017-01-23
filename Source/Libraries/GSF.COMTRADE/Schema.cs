//******************************************************************************************************
//  Schema.cs - Gbtc
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
//  05/18/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GSF.COMTRADE
{
    #region [ Enumerations ]

    /// <summary>
    /// <see cref="Schema"/> file type enumeration.
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// ASCII file type.
        /// </summary>
        Ascii,
        /// <summary>
        /// Binary file type.
        /// </summary>
        Binary
    }

    #endregion

    /// <summary>
    /// Represents the schema for a configuration file in the COMTRADE file standard, IEEE Std C37.111-1999.
    /// </summary>
    public class Schema
    {
        #region [ Members ]

        // Fields
        private string m_stationName;
        private string m_deviceID;
        private int m_version;
        private AnalogChannel[] m_analogChannels;
        private DigitalChannel[] m_digitalChannels;
        private double m_nominalFrequency;
        private SampleRate[] m_sampleRates;
        private Timestamp m_startTime;
        private Timestamp m_triggerTime;
        private FileType m_fileType;
        private double m_timeFactor;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Schema"/>.
        /// </summary>
        public Schema()
        {
            m_version = 1999;
            m_nominalFrequency = 60.0D;
            m_fileType = FileType.Binary;
            m_timeFactor = 1.0D;
            SampleRates = null;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Schema"/> from an existing configuration file name.
        /// </summary>
        /// <param name="fileName">File name of configuration file to parse.</param>
        public Schema(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            string[] parts;
            int lineNumber = 0;

            // Parse version line
            parts = lines[lineNumber++].Split(',');

            if (parts.Length != 2 && parts.Length != 3)
                throw new InvalidOperationException(string.Format("Unexpected number of line image elements for first configuration file line: {0} - expected 2 or 3\r\nImage = {1}", parts.Length, lines[0]));

            StationName = parts[0].Trim();
            DeviceID = parts[1].Trim();

            if (parts.Length == 3 && !string.IsNullOrWhiteSpace(parts[2]))
                m_version = int.Parse(parts[2].Trim());
            else
                m_version = 1991;

            // Parse totals line
            parts = lines[lineNumber++].Split(',');

            if (parts.Length != 3)
                throw new InvalidOperationException(string.Format("Unexpected number of line image elements for second configuration file line: {0} - expected 3\r\nImage = {1}", parts.Length, lines[1]));

            int totalChannels = int.Parse(parts[0].Trim());
            int totalAnalogChannels = int.Parse(parts[1].Trim().Split('A')[0]);
            int totalDigitalChannels = int.Parse(parts[2].Trim().Split('D')[0]);

            if (totalChannels != totalAnalogChannels + totalDigitalChannels)
                throw new InvalidOperationException(string.Format("Total defined channels must equal the sum of the total number of analog and digital channel definitions.\r\nImage = {0}", lines[1]));

            // Parse analog definitions
            List<AnalogChannel> analogChannels = new List<AnalogChannel>();

            for (int i = 0; i < totalAnalogChannels; i++)
            {
                analogChannels.Add(new AnalogChannel(lines[lineNumber++]));
            }

            AnalogChannels = analogChannels.ToArray();

            // Parse digital definitions
            List<DigitalChannel> digitalChannels = new List<DigitalChannel>();

            for (int i = 0; i < totalDigitalChannels; i++)
            {
                digitalChannels.Add(new DigitalChannel(lines[lineNumber++]));
            }

            DigitalChannels = digitalChannels.ToArray();

            // Parse line frequency
            NominalFrequency = double.Parse(lines[lineNumber++]);

            // Parse total number of sample rates
            int totalSampleRates = int.Parse(lines[lineNumber++]);

            if (totalSampleRates == 0)
                totalSampleRates = 1;

            // Parse each sample rate
            List<SampleRate> sampleRates = new List<SampleRate>();

            for (int i = 0; i < totalSampleRates; i++)
                sampleRates.Add(new SampleRate(lines[lineNumber++]));

            SampleRates = sampleRates.ToArray();

            // Parse timestamps
            StartTime = new Timestamp(lines[lineNumber++]);
            TriggerTime = new Timestamp(lines[lineNumber++]);

            // Parse file type
            FileType = (FileType)Enum.Parse(typeof(FileType), lines[lineNumber++], true);

            // Parse time factor
            TimeFactor = (lines.Length < lineNumber ? double.Parse(lines[lineNumber]) : 1);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets free-form station name for this <see cref="Schema"/>.
        /// </summary>
        public string StationName
        {
            get
            {
                return m_stationName;
            }
            set
            {
                m_stationName = value;
            }
        }

        /// <summary>
        /// Gets or sets free-form device ID for this <see cref="Schema"/>.
        /// </summary>
        public string DeviceID
        {
            get
            {
                return m_deviceID;
            }
            set
            {
                m_deviceID = value;
            }
        }

        /// <summary>
        /// Gets or sets version number of the IEEE Std C37.111 used by this <see cref="Schema"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Only IEEE Std C37.111 version 1999 is supported by this implementation of the phasor data schema.</exception>
        public int Version
        {
            get
            {
                return m_version;
            }
            set
            {
                //if (value != 1999)
                //    throw new ArgumentOutOfRangeException("value", value + " is an invalid version number. Only IEEE Std C37.111 version 1999 is supported by this implementation of the phasor data schema.");

                m_version = value;
            }
        }

        /// <summary>
        /// Gets total number of analog and digital channels of this <see cref="Schema"/>.
        /// </summary>
        public int TotalChannels
        {
            get
            {
                return TotalAnalogChannels + TotalDigitalChannels;
            }
        }

        /// <summary>
        /// Gets total number of analog channels of this <see cref="Schema"/>.
        /// </summary>
        public int TotalAnalogChannels
        {
            get
            {
                if (m_analogChannels != null)
                    return m_analogChannels.Length;

                return 0;
            }
        }

        /// <summary>
        /// Gets total number of digital channels of this <see cref="Schema"/>.
        /// </summary>
        public int TotalDigitalChannels
        {
            get
            {
                if (m_digitalChannels != null)
                    return m_digitalChannels.Length;

                return 0;
            }
        }

        /// <summary>
        /// Gets or sets analog channels of this <see cref="Schema"/>.
        /// </summary>
        public AnalogChannel[] AnalogChannels
        {
            get
            {
                return m_analogChannels;
            }
            set
            {
                m_analogChannels = value;
            }
        }

        /// <summary>
        /// Gets or sets digital channels of this <see cref="Schema"/>.
        /// </summary>
        public DigitalChannel[] DigitalChannels
        {
            get
            {
                return m_digitalChannels;
            }
            set
            {
                m_digitalChannels = value;
            }
        }

        /// <summary>
        /// Gets or sets nominal frequency of this <see cref="Schema"/>.
        /// </summary>
        public double NominalFrequency
        {
            get
            {
                return m_nominalFrequency;
            }
            set
            {
                m_nominalFrequency = value;

                // Cascade nominal frequency update to analog channels
                if (m_analogChannels != null)
                {
                    foreach (AnalogChannel analogChannel in m_analogChannels)
                    {
                        analogChannel.NominalFrequency = m_nominalFrequency;
                    }
                }
            }
        }

        /// <summary>
        /// Gets total number of sample rates of this <see cref="Schema"/>.
        /// </summary>
        public int TotalSampleRates
        {
            get
            {
                if (m_sampleRates.Length == 1 && m_sampleRates[0].Rate == 0)
                    return 0;

                return m_sampleRates.Length;
            }
        }

        /// <summary>
        /// Gets or sets sampling rates of this <see cref="Schema"/>. A file of phasor data will normally be made using a single sampling rate, so this will usually be 1.
        /// </summary>
        public SampleRate[] SampleRates
        {
            get
            {
                return m_sampleRates;
            }
            set
            {
                if ((object)value == null)
                    m_sampleRates = new SampleRate[] { new SampleRate() { Rate = 0, EndSample = 1 } };
                else
                    m_sampleRates = value;
            }
        }

        /// <summary>
        /// Gets or sets start timestamp of this <see cref="Schema"/>.
        /// </summary>
        public Timestamp StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                m_startTime = value;
            }
        }

        /// <summary>
        /// Gets or sets trigger timestamp of this <see cref="Schema"/>.
        /// </summary>
        public Timestamp TriggerTime
        {
            get
            {
                return m_triggerTime;
            }
            set
            {
                m_triggerTime = value;
            }
        }

        /// <summary>
        /// Gets or sets file type of this <see cref="Schema"/>.
        /// </summary>
        public FileType FileType
        {
            get
            {
                return m_fileType;
            }
            set
            {
                m_fileType = value;
            }
        }

        /// <summary>
        /// Gets or sets the multiplication factor for the time differential (timestamp) field in this <see cref="Schema"/>.
        /// </summary>
        public double TimeFactor
        {
            get
            {
                return m_timeFactor;
            }
            set
            {
                m_timeFactor = value;
            }
        }

        /// <summary>
        /// Gets the total digital words for given number of digital values when the file type is binary.
        /// </summary>
        public int DigitalWords
        {
            get
            {
                return (int)Math.Ceiling(m_digitalChannels.Length / 16.0D);
            }
        }

        /// <summary>
        /// Calculates the size of a record, in bytes, when the file type is binary.
        /// </summary>
        public int BinaryRecordLength
        {
            get
            {
                return 8 + 2 * m_analogChannels.Length + 2 * DigitalWords;
            }
        }

        /// <summary>
        /// Gets the file image of this <see cref="Schema"/>.
        /// </summary>
        public string FileImage
        {
            get
            {
                StringBuilder fileImage = new StringBuilder();

                // Write version line
                fileImage.AppendFormat("{0},{1},{2}\r\n", StationName, DeviceID, Version);

                // Write totals line
                fileImage.AppendFormat("{0},{1}A,{2}D\r\n", TotalChannels, TotalAnalogChannels, TotalDigitalChannels);

                // Write analog definitions
                for (int i = 0; i < TotalAnalogChannels; i++)
                {
                    fileImage.AppendLine(AnalogChannels[i].ToString());
                }

                // Write digital definitions
                for (int i = 0; i < TotalDigitalChannels; i++)
                {
                    fileImage.AppendLine(DigitalChannels[i].ToString());
                }

                // Write line frequency
                fileImage.AppendLine(NominalFrequency.ToString(CultureInfo.InvariantCulture));

                // Write total number of sample rates
                fileImage.AppendLine(TotalSampleRates.ToString());

                // Write sample rates
                for (int i = 0; i < TotalSampleRates; i++)
                {
                    fileImage.AppendLine(SampleRates[i].ToString());
                }

                // Write timestamps
                fileImage.AppendLine(StartTime.ToString());
                fileImage.AppendLine(TriggerTime.ToString());

                // Write file type
                fileImage.AppendLine(FileType.ToString().ToUpper());

                // Write time factor
                fileImage.AppendLine(TimeFactor.ToString(CultureInfo.InvariantCulture));

                return fileImage.ToString();
            }
        }

        #endregion
    }
}
