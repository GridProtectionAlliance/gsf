//******************************************************************************************************
//  WavInputAdapter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//       Modified WaveData dependency to refer to GSF.Media and moved the library
//       into TimeSeriesFramework.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using GSF;
using GSF.Media;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace WavInputAdapter
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a WAV file.
    /// </summary>
    [Description("WAV: Reads measurements from a WAV file")]
    [EditorBrowsable(EditorBrowsableState.Advanced)] // Normally defined as an input device protocol
    public class WavInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        private const double DefaultRecoveryDelay = 5.0D;
        private const long GapThreshold = Ticks.PerSecond;

        // Fields
        private WaveDataReader m_data;
        private long m_dataIndex;
        private int m_channels;
        private int m_sampleRate;
        private int m_numSamples;
        private TimeSpan m_audioLength;
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
        /// Gets or sets the amount of time, in seconds, needed to recover from a back log.
        /// </summary>
        [ConnectionStringParameter,
        Description("The amount of time, in seconds, needed to recover from a back log."),
        DefaultValue(DefaultRecoveryDelay)]
        public double RecoveryDelay
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
        [ConnectionStringParameter,
        DefaultValue(null),
        Description("Defines primary keys of output measurements the adapter expects; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
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

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return true;
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

            if (settings.TryGetValue("recoveryDelay", out setting))
                RecoveryDelay = double.Parse(setting);
            else
                RecoveryDelay = DefaultRecoveryDelay;

            if (!settings.TryGetValue("wavFileName", out setting))
                throw new ArgumentException("wavFileName is missing from settings - Example: wavFileName=Bohemian Rhapsody.wav");

            WavFileName = setting;
        }

        /// <summary>
        /// Attempts to open the file to start getting wave data.
        /// </summary>
        protected override void AttemptConnection()
        {
            WaveFile fileInfo = WaveFile.Load(WavFileName, false);

            m_channels = fileInfo.Channels;
            m_sampleRate = fileInfo.SampleRate;
            m_numSamples = fileInfo.DataChunk.ChunkSize / fileInfo.BlockAlignment;
            m_audioLength = fileInfo.AudioLength;

            m_data = WaveDataReader.FromFile(WavFileName);
            m_dataIndex = 0;

            //if (file.Channels != OutputMeasurements.Length)
            //    throw new ArgumentException(string.Format("The number of channels in the WAV file must match the number of output measurements. Channels: {0}, Measurements: {1}", file.Channels, OutputMeasurements.Length));

            m_startTime = DateTime.UtcNow.Ticks;

            Thread t = new Thread(ProcessMeasurements);
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// Attempts to close the file and release resources held by the adapter.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_data != null)
            {
                m_data.Close();
                m_data.Dispose();
            }
            m_data = null;
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
                    if (disposing)
                    {
                        if (m_data != null)
                        {
                            m_data.Close();
                            m_data.Dispose();
                        }
                        m_data = null;
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
            TimeSpan time = Ticks.FromSeconds(m_dataIndex / (double)m_sampleRate);
            return string.Format("Streaming {0} at time {1} / {2} - {3:0.00%}.", Path.GetFileName(WavFileName), time.ToString(@"m\:ss"), m_audioLength.ToString(@"m\:ss"), time.TotalSeconds / m_audioLength.TotalSeconds);
        }

        // Generates new measurements since the last time this was called.
        private void ProcessMeasurements()
        {
            // Declare the variables use in this method.
            List<IMeasurement> measurements = new List<IMeasurement>((int)(Ticks.ToSeconds(GapThreshold) * m_sampleRate * m_channels * 1.1D));
            LittleBinaryValue[] sample;

            while (Enabled)
            {
                try
                {
                    SpinWait spinner = new SpinWait();

                    // Determine what time it is now.
                    long now = DateTime.UtcNow.Ticks;

                    // Assign a timestamp to the next sample based on its location
                    // in the file relative to the other samples in the file.
                    long timestamp = m_startTime + (m_dataIndex * Ticks.PerSecond / m_sampleRate);

                    if (now - timestamp > GapThreshold)
                    {
                        // Reset the start time and delay next transmission in an attempt to catch up
                        m_startTime = now - (m_dataIndex * Ticks.PerSecond / m_sampleRate) + Ticks.FromSeconds(RecoveryDelay);
                        timestamp = now;
                        OnStatusMessage("Start time reset.");
                    }

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
                            m_data.Dispose();

                            m_data = WaveDataReader.FromFile(WavFileName);
                            m_dataIndex = 0;

                            m_startTime = timestamp;
                            sample = m_data.GetNextSample();
                        }

                        // Create new measurements, one for each channel,
                        // and add them to the measurements list.
                        for (int i = 0; i < m_channels; i++)
                        {
                            measurements.Add(Measurement.Clone(OutputMeasurements[i], sample[i].ConvertToType(TypeCode.Double), timestamp));
                        }

                        // Update the data index and recalculate
                        // the assigned timestamp for the next sample.
                        m_dataIndex++;
                        timestamp = m_startTime + (m_dataIndex * Ticks.PerSecond / m_sampleRate);
                    }

                    OnNewMeasurements(measurements);
                    measurements.Clear();

                    while (DateTime.UtcNow.Ticks - timestamp <= GapThreshold / 100)
                    {
                        // Ahead of schedule -- pause for a moment
                        spinner.SpinOnce();
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(ex);
                }
            }
        }

        #endregion
    }
}
