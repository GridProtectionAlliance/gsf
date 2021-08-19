
using System;
using System.Collections.Generic;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;

namespace GEPDataExtractor
{
    /// <summary>
    /// Retrieves data from openHistorian using GEP
    /// </summary>
    internal sealed class DataReceiver : IDisposable
    {
        #region [ Members ]

        // Fields
        private DataSubscriber m_subscriber;
        private readonly UnsynchronizedSubscriptionInfo m_subscriptionInfo;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataReceiver"/> instance with the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">GEP connection string for openHistorian.</param>
        /// <param name="filterExpression">Filter expression defining measurements to query.</param>
        /// <param name="startTime">Temporal subscription start time.</param>
        /// <param name="stopTime">Temporal subscription stop time.</param>
        public DataReceiver(string connectionString, string filterExpression, DateTime startTime, DateTime stopTime)
        {
            m_subscriber = new DataSubscriber
            {
                ConnectionString = connectionString,
                OperationalModes = OperationalModes.UseCommonSerializationFormat | OperationalModes.CompressPayloadData,
                CompressionModes = CompressionModes.TSSC
            };

            m_subscriptionInfo = new UnsynchronizedSubscriptionInfo(false)
            {
                FilterExpression = filterExpression,
                StartTime = startTime.ToString(TimeTagBase.DefaultFormat),
                StopTime = stopTime.ToString(TimeTagBase.DefaultFormat),
                ProcessingInterval = 0, // Zero value requests data as fast as possible
                UseMillisecondResolution = true
            };

            // Attach to needed subscriber events
            m_subscriber.ConnectionEstablished += m_subscriber_ConnectionEstablished;
            m_subscriber.NewMeasurements += m_subscriber_NewMeasurements;
            m_subscriber.StatusMessage += m_subscriber_StatusMessage;
            m_subscriber.ProcessException += m_subscriber_ProcessException;
            m_subscriber.ProcessingComplete += m_subscriber_ProcessingComplete;

            // Initialize the subscriber
            m_subscriber.Initialize();

            // Start subscriber connection cycle
            m_subscriber.Start();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="DataReceiver"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~DataReceiver() => Dispose(false);

        public Action<ICollection<IMeasurement>> NewMeasurementsCallback { get; set; }

        public Action<string> StatusMessageCallback { get; set; }

        public Action<Exception> ProcessExceptionCallback { get; set; }

        public Action ReadCompletedCallback { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="DataReceiver"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataReceiver"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (m_subscriber is null)
                    return;

                m_subscriber.Unsubscribe();
                m_subscriber.Stop();

                // Detach from subscriber events
                m_subscriber.ConnectionEstablished -= m_subscriber_ConnectionEstablished;
                m_subscriber.NewMeasurements -= m_subscriber_NewMeasurements;
                m_subscriber.StatusMessage -= m_subscriber_StatusMessage;
                m_subscriber.ProcessException -= m_subscriber_ProcessException;
                m_subscriber.ProcessingComplete -= m_subscriber_ProcessingComplete;

                m_subscriber.Dispose();
                m_subscriber = null;
            }
            finally
            {
                m_disposed = true; // Prevent duplicate dispose.
            }
        }

        private void m_subscriber_ConnectionEstablished(object sender, EventArgs e) => m_subscriber.UnsynchronizedSubscribe(m_subscriptionInfo);

        private void m_subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e) => NewMeasurementsCallback?.Invoke(e.Argument);

        private void m_subscriber_StatusMessage(object sender, EventArgs<string> e) => StatusMessageCallback?.Invoke(e.Argument);

        private void m_subscriber_ProcessException(object sender, EventArgs<Exception> e) => ProcessExceptionCallback?.Invoke(e.Argument);

        private void m_subscriber_ProcessingComplete(object sender, EventArgs<string> e) => ReadCompletedCallback?.Invoke();

        #endregion
    }
}
