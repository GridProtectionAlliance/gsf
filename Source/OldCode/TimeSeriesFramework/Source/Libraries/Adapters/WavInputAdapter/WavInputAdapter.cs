//******************************************************************************************************
//  WavInputAdapter.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  06/29/2011 - Stephen C. Wills
//       Generated original version of source code.
//  07/01/2011 - Stephen C. Wills
//       Modified WaveData dependency to refer to TVA.Media and moved the library
//       into TimeSeriesFramework.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Media;

namespace WavInputAdapter
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a WAV file.
    /// </summary>
    [Description("WAV: Reads measurements from a WAV file.")]
    public class WavInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private WaveDataReader m_data;
        private int m_dataIndex;
        private int m_channels;
        private int m_sampleRate;
        private int m_numSamples;
        private TimeSpan m_audioLength;
        private PrecisionTimer m_timer;
        private long m_startTime;

        private bool m_disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the file from which to read measurements.
        /// </summary>
        [ConnectionStringParameter, Description("The name of the file from which to read measurements.")]
        public string WavFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the output measurements.
        /// </summary>
        /// <remarks>
        /// The output measurements are like templates. New measurements generated
        /// by this adapter will be copies of these output measurements, in which
        /// only the values and timestamps will differ.
        /// </remarks>
        public override IMeasurement[] OutputMeasurements
        {
            get
            {
                return base.OutputMeasurements;
            }
            set
            {
                base.OutputMeasurements = value.OrderBy(measurement => measurement.Key.ToString()).ToArray();
            }
        }

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        /// <remarks>
        /// This returns false because this adapter does not connect asynchronously.
        /// </remarks>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the connection string parameters of this <see cref="WavInputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (!settings.TryGetValue("wavFileName", out setting))
                throw new ArgumentException("wavFileName is missing from settings - Example: wavFileName=Bohemian Rhapsody.wav");

            WavFileName = setting;
        }

        /// <summary>
        /// Attempts to open the file to start getting wave data.
        /// </summary>
        protected override void AttemptConnection()
        {
            PrecisionTimer.SetMinimumTimerResolution(1);
            WaveFile fileInfo = WaveFile.Load(WavFileName, false);

            m_data = WaveDataReader.FromFile(WavFileName);
            m_dataIndex = 0;
            m_channels = fileInfo.Channels;
            m_sampleRate = fileInfo.SampleRate;
            m_numSamples = fileInfo.DataChunk.ChunkSize / fileInfo.BlockAlignment;
            m_audioLength = fileInfo.AudioLength;

            //if (file.Channels != OutputMeasurements.Length)
            //    throw new ArgumentException(string.Format("The number of channels in the WAV file must match the number of output measurements. Channels: {0}, Measurements: {1}", file.Channels, OutputMeasurements.Length));

            m_timer = new PrecisionTimer();
            m_timer.Period = 1;
            m_timer.Tick += Timer_Tick;

            m_timer.Start();
            m_startTime = PrecisionTimer.UtcNow.Ticks;
        }

        /// <summary>
        /// Attempts to close the file and release resources held by the adapter.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_timer != null)
            {
                m_timer.Tick -= Timer_Tick;
                m_timer.Stop();
                m_timer.Dispose();
            }

            if (m_data != null)
            {
                m_data.Close();
            }

            m_timer = null;
            m_data = null;

            PrecisionTimer.ClearMinimumTimerResolution(1);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="WavInputAdapter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            TimeSpan time = new TimeSpan(Ticks.FromSeconds(m_dataIndex / (double)m_sampleRate));
            return string.Format("Streaming {0} at time {1} / {2} - {3:0.00%}.", Path.GetFileName(WavFileName), time.ToString("m:ss"), m_audioLength.ToString("m:ss"), time.TotalSeconds / m_audioLength.TotalSeconds);
        }

        // Generates new measurements since the last time this was called.
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Determine what time it is now.
            long now = PrecisionTimer.UtcNow.Ticks;

            // Assign a timestamp to the next sample based on its location
            // in the file relative to the other samples in the file.
            long timestamp = m_startTime + (m_dataIndex * Ticks.PerSecond / m_sampleRate);

            // Declare the variables use in this method.
            List<IMeasurement> measurements = new List<IMeasurement>();
            LittleBinaryValue[] sample;
            IMeasurement channelMeasurement;

            // Keep generating measurements until
            // we catch up to the current time.
            while (timestamp < now)
            {
                sample = m_data.GetNextSample();

                // If the sample is null, we've reached the end of the file.
                // Close and reopen it, resetting the data index and start time.
                if (sample == null)
                {
                    m_data.Close();
                    m_data = WaveDataReader.FromFile(WavFileName);
                    m_dataIndex = 0;
                    m_startTime = timestamp;
                    sample = m_data.GetNextSample();
                }

                // Create new measurements, one for each channel,
                // and add them to the measurements list.
                for (int i = 0; i < m_channels; i++)
                {
                    channelMeasurement = Measurement.Clone(OutputMeasurements[i]);
                    channelMeasurement.Value = sample[i].ConvertToType(TypeCode.Double);
                    channelMeasurement.Timestamp = timestamp;
                    measurements.Add(channelMeasurement);
                }

                // Update the data index and recalculate
                // the assigned timestamp for the next sample.
                m_dataIndex++;
                timestamp = m_startTime + (m_dataIndex * Ticks.PerSecond / m_sampleRate);
            }

            OnNewMeasurements(measurements);
        }

        #endregion
    }
}
