//******************************************************************************************************
//  MicrophoneInputAdapter.cs - Gbtc
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
//  11/29/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AudioAdapters
{
    /// <summary>
    /// Represents an input adapter that reads measurements from a recording device.
    /// </summary>
    [Description("MIC: Reads measurements from a recording device")]
    public class MicrophoneInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private IWaveIn m_waveIn;
        private long m_ticksPerSample;
        private int m_sampleSize;
        private int m_channels;
        private TypeCode m_sampleTypeCode;

        private long m_lastSampleTime;
        private long m_samplesProcessed;

        private bool m_disposed;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        /// <remarks>
        /// Derived classes should return true when data input source is connects asynchronously, otherwise return false.
        /// </remarks>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder(base.Status);

                status.AppendFormat("          Ticks Per Sample: {0}", m_ticksPerSample);
                status.AppendLine();
                status.AppendFormat("               Sample Size: {0}", m_sampleSize);
                status.AppendLine();
                status.AppendFormat("                  Channels: {0}", m_channels);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"{m_samplesProcessed} samples processed so far...".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt connection to data input source here.  Any exceptions thrown
        /// by this implementation will result in restart of the connection cycle.
        /// </remarks>
        protected override void AttemptConnection()
        {
            // ReSharper disable once UnusedVariable > Justification: implementation pattern disposes any existing object
            using (IWaveIn waveIn = m_waveIn)
                m_waveIn = new WasapiCapture();

            m_ticksPerSample = Ticks.PerSecond / m_waveIn.WaveFormat.SampleRate;
            m_sampleSize = m_waveIn.WaveFormat.BitsPerSample / 8;
            m_channels = m_waveIn.WaveFormat.Channels;
            m_sampleTypeCode = GetSampleTypeCode();

            Interlocked.Exchange(ref m_lastSampleTime, 0L);
            m_samplesProcessed = 0L;

            if (m_channels != OutputMeasurements.Length)
                OnStatusMessage(MessageLevel.Warning, "Number of output measurements does not match the number of channels.");

            m_waveIn.DataAvailable += WaveIn_DataAvailable;
            m_waveIn.RecordingStopped += WaveIn_RecordingStopped;
            m_waveIn.StartRecording();
        }

        /// <summary>
        /// Attempts to disconnect from data input source.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt disconnect from data input source here.  Any exceptions thrown
        /// by this implementation will be reported to host via <see cref="AdapterBase.ProcessException"/> event.
        /// </remarks>
        protected override void AttemptDisconnection()
        {
            if ((object)m_waveIn != null)
            {
                m_waveIn.Dispose();
                m_waveIn = null;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MicrophoneInputAdapter"/> object and optionally releases the managed resources.
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
                        if ((object)m_waveIn != null)
                        {
                            m_waveIn.Dispose();
                            m_waveIn = null;
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

        private TypeCode GetSampleTypeCode()
        {
            // Determine sample data type based on bit size and audio format
            switch (m_waveIn.WaveFormat.BitsPerSample)
            {
                case 8:
                    return TypeCode.Byte;
                case 16:
                    return TypeCode.Int16;
                case 24:
                    // .NET does not define an Int24 type code and since an Int24 will
                    // fit inside an Int32, the Int32 type code is returned.
                    return TypeCode.Int32;
                case 32:
                    if (m_waveIn.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        return TypeCode.Single;

                    return TypeCode.Int32;
                case 64:
                    if (m_waveIn.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        return TypeCode.Double;

                    return TypeCode.Int64;
                default:
                    // Unable to determine proper type code, consumer may be using a special data format...
                    return TypeCode.Empty;
            }
        }

        private short ConvertToPCM16(double sample)
        {
            switch (m_waveIn.WaveFormat.BitsPerSample)
            {
                case 8:
                    return (short)((sample - 128.0D) / sbyte.MaxValue * short.MaxValue);
                case 16:
                    return (short)sample;
                case 24:
                    return (short)((double)sample / Int24.MaxValue * short.MaxValue);
                case 32:
                    if (m_waveIn.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        return (short)(sample * short.MaxValue);

                    return (short)((double)sample / int.MaxValue * short.MaxValue);
                case 64:
                    if (m_waveIn.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        return (short)(sample * short.MaxValue);

                    return (short)((double)sample / long.MaxValue * short.MaxValue);
                default:
                    throw new InvalidOperationException($"Cannot convert sample \'{sample}\' into 16-bits.");
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            int index = 0;
            int numSamples = waveInEventArgs.BytesRecorded / m_sampleSize / m_channels;
            List<IMeasurement> measurements = new List<IMeasurement>();

            long sampleTime;
            LittleBinaryValue sampleValue;

            // Get the timestamp for the first recorded sample
            if (Interlocked.Read(ref m_lastSampleTime) == 0L)
                Interlocked.Exchange(ref m_lastSampleTime, DateTime.UtcNow.Ticks - (m_ticksPerSample * numSamples));

            // Get the timestamp for the first sample in this block of data
            sampleTime = Interlocked.Read(ref m_lastSampleTime) + m_ticksPerSample;

            // Parse each recorded sample in this block of data
            for (int sampleIndex = 0; sampleIndex < numSamples; sampleIndex++)
            {
                // Parse one value per channel per sample
                for (int channelIndex = 0; channelIndex < m_channels; channelIndex++)
                {
                    if (channelIndex < OutputMeasurements.Length)
                    {
                        // Create a measurement for this value
                        sampleValue = new LittleBinaryValue(m_sampleTypeCode, waveInEventArgs.Buffer, index + (channelIndex * m_sampleSize), m_sampleSize);
                        measurements.Add(Measurement.Clone(OutputMeasurements[channelIndex], ConvertToPCM16(sampleValue.ConvertToType(TypeCode.Double).ToDouble()), sampleTime));
                    }
                }

                // Update the last sample time
                Interlocked.Exchange(ref m_lastSampleTime, sampleTime);

                // Get the timestamp for the next sample in this block of data
                sampleTime += m_ticksPerSample;

                // Update the index to the next sample in this block of data
                index += m_sampleSize * m_channels;
            }

            // Publish streaming microphone data
            OnNewMeasurements(measurements);
            m_samplesProcessed += numSamples;
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if ((object)stoppedEventArgs.Exception != null)
            {
                string errorMessage = $"An error occurred during recording: {stoppedEventArgs.Exception.Message}";
                OnProcessException(MessageLevel.Warning, new InvalidOperationException(errorMessage, stoppedEventArgs.Exception));
            }
        }

        #endregion
    }
}
