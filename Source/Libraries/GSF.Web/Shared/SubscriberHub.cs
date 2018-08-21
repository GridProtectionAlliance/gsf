//******************************************************************************************************
//  SubscriberHub.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/12/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GSF.Collections;
using GSF.Configuration;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
using GSF.Web.Security;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;

namespace GSF.Web.Shared
{
    /// <summary>
    /// SignalR hub that exposes server-side functions for the subscriber API.
    /// </summary>
    [AuthorizeHubRole]
    public class SubscriberHub : Hub
    {
        #region [ Members ]

        // Nested Types

        private sealed class Connection : IDisposable
        {
            public readonly Dictionary<string, Subscriber> SubscriberLookup;
            private bool m_disposed;

            public Connection()
            {
                SubscriberLookup = new Dictionary<string, Subscriber>();
            }

            public void Dispose()
            {
                if (m_disposed)
                    return;

                try
                {
                    foreach (Subscriber subscriptionState in SubscriberLookup.Values)
                        subscriptionState.Dispose();
                }
                finally
                {
                    m_disposed = true;
                }
            }
        }

        private sealed class Subscriber : IDisposable
        {
            public DataSubscriber DataSubscriber;
            public DataSet Metadata;

            private Dictionary<Guid, Tuple<string, string>> m_formats;
            private bool m_disposed;

            public void SetFormat(Guid signalID, string format, string dataType)
            {
                if ((object)m_formats == null)
                    m_formats = new Dictionary<Guid, Tuple<string, string>>();

                m_formats[signalID] = new Tuple<string, string>(format, dataType);
            }

            public bool TryGetFormat(Guid signalID, out string format, out string dataType)
            {
                format = null;
                dataType = null;

                if ((object)m_formats == null)
                    return false;

                if (m_formats.TryGetValue(signalID, out Tuple<string, string> tuple))
                {
                    format = tuple.Item1;
                    dataType = tuple.Item2;
                    return true;
                }

                return false;
            }

            public void RemoveFormat(Guid signalID)
            {
                if ((object)m_formats == null || !m_formats.ContainsKey(signalID))
                    return;

                if (m_formats.Remove(signalID) && m_formats.Count == 0)
                    m_formats = null;
            }

            public void Dispose()
            {
                if (m_disposed)
                    return;

                try
                {
                    DataSubscriber?.Dispose();
                    Metadata?.Dispose();
                }
                finally
                {
                    m_disposed = true;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initiates the subscriber connection.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber to be connected.</param>
        public void Connect(string subscriberID)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);
            subscriber.DataSubscriber.Start();
        }

        /// <summary>
        /// Sends a command to the publisher.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        /// <param name="commandCode">The command to be sent.</param>
        /// <param name="message">The message to be sent to the publisher.</param>
        public void SendCommand(string subscriberID, ServerCommand commandCode, string message)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);
            subscriber.DataSubscriber.SendServerCommand(commandCode, message);
        }

        /// <summary>
        /// Filters metadata and returns the result.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        /// <param name="tableName">The metadata table from which to return rows.</param>
        /// <param name="filter">The filter to apply to the metadata table.</param>
        /// <param name="sortField">The field by which to sort the result set.</param>
        /// <param name="takeCount">The maximum number of records to be returned from the result set.</param>
        /// <returns>The result of the metadata query.</returns>
        public IEnumerable<object> GetMetadata(string subscriberID, string tableName, string filter, string sortField, int takeCount)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);
            DataSet metadata = subscriber.Metadata;

            if ((object)metadata == null)
                return null;

            if (string.IsNullOrEmpty(tableName) || !metadata.Tables.Contains(tableName))
                return null;

            DataTable table = metadata.Tables[tableName];
            IEnumerable<DataRow> rows;

            if (string.IsNullOrEmpty(filter))
                rows = table.Select();
            else
                rows = table.Select(filter);

            if (!string.IsNullOrEmpty(sortField) && table.Columns.Contains(sortField))
                rows = rows.OrderBy(row => row[sortField]);

            if (takeCount > 0)
                rows = rows.Take(takeCount);

            return rows.Select(FromDataRow);
        }

        /// <summary>
        /// Defines a string format to apply for a given measurement.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        /// <param name="signalID">Measurement signal ID to which to apply the format.</param>
        /// <param name="format">String format to apply, e.g., "{0:N3} seconds".</param>
        /// <param name="dataType">Fully qualified data type for measurement, defaults to "System.Double" if <c>null</c>.</param>
        /// <remarks>
        /// <para>
        /// Set <paramref name="format"/> to <c>null</c> to remove any existing formatting.
        /// </para>
        /// <para>
        /// Conversion of double-precision measurement floating point value to specified
        /// <paramref name="dataType"/> will be attempted when type is provided.
        /// </para>
        /// </remarks>
        public void SetMeasurementFormat(string subscriberID, Guid signalID, string format, string dataType)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);

            if (string.IsNullOrWhiteSpace(format))
                subscriber.RemoveFormat(signalID);
            else
                subscriber.SetFormat(signalID, format, dataType);
        }

        /// <summary>
        /// Defines string formats to apply for the collection of format records.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        /// <param name="formatRecords">Collection of format records.</param>
        /// <remarks>
        /// This is simply a bulk operation for <see cref="SetMeasurementFormat"/>.
        /// Each format record should be a Json object similar to the following:
        /// <code>
        /// {
        ///     "signalID":"4B1DEE7C-72EC-41EA-AAF3-7E8094355740",
        ///     "format":"{0:N3} seconds",
        ///     "dataType":"System.Double"
        /// }
        /// </code>
        /// Where
        ///  - "signalID" is the measurement signal ID to which to apply the format
        ///  - "format" is the string format to apply, and
        ///  - "dataType" is the fully qualified data type for measurement
        /// </remarks>
        public void SetMeasurementFormats(string subscriberID, IEnumerable<dynamic> formatRecords)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);

            foreach (dynamic formatRecord in formatRecords)
            {
                Guid signalID = formatRecord.signalID;
                string format = formatRecord.format;
                string dataType = formatRecord.dataType;

                if (string.IsNullOrWhiteSpace(format))
                    subscriber.RemoveFormat(signalID);
                else
                    subscriber.SetFormat(signalID, format, dataType);
            }
        }

        /// <summary>
        /// Subscribes to the internal data publisher.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        /// <param name="data">Data from the client describing the subscription.</param>
        public void Subscribe(string subscriberID, JObject data)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);
            SubscriptionInfo subscriptionInfo = ToSubscriptionInfo(subscriberID, data);
            subscriber.DataSubscriber.Subscribe(subscriptionInfo);
        }

        /// <summary>
        /// Unsubscribes from the publisher.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        public void Unsubscribe(string subscriberID)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);
            subscriber.DataSubscriber.Unsubscribe();
        }

        /// <summary>
        /// Disconnects from the publisher.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        public void Disconnect(string subscriberID)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);
            subscriber.DataSubscriber.Stop();
        }

        /// <summary>
        /// Closes the connection and releases all resources retained by the subscriber with this ID.
        /// </summary>
        /// <param name="subscriberID">The ID of the subscriber.</param>
        public void Dispose(string subscriberID)
        {
            string connectionID = Context.ConnectionId;

            if (s_connectionLookup.TryGetValue(connectionID, out Connection connection) && connection.SubscriberLookup.TryGetValue(subscriberID, out Subscriber subscriber))
            {
                connection.SubscriberLookup.Remove(subscriberID);
                subscriber.Dispose();
            }
        }

        /// <summary>
        /// Handles when a client disconnects from the hub.
        /// </summary>
        /// <param name="stopCalled">Indicates whether the client called stop.</param>
        /// <returns>The task to handle the disconnect.</returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            return Task.Run(() =>
            {
                string connectionID = Context.ConnectionId;

                if (s_connectionLookup.TryRemove(connectionID, out Connection connection))
                    connection.Dispose();
            });
        }

        private Subscriber GetOrCreate(string subscriberID)
        {
            string connectionID = Context.ConnectionId;
            Connection connection = s_connectionLookup.GetOrAdd(connectionID, id => new Connection());

            return connection.SubscriberLookup.GetOrAdd(subscriberID, id =>
            {
                Subscriber subscriber = new Subscriber();
                subscriber.DataSubscriber = new DataSubscriber();
                subscriber.DataSubscriber.ConnectionEstablished += (sender, args) => Subscriber_ConnectionEstablished(connectionID, subscriberID);
                subscriber.DataSubscriber.ConnectionTerminated += (sender, args) => Subscriber_ConnectionTerminated(connectionID, subscriberID);
                subscriber.DataSubscriber.MetaDataReceived += (sender, args) => Subscriber_MetadataReceived(connectionID, subscriberID, subscriber, args.Argument);
                subscriber.DataSubscriber.ConfigurationChanged += (sender, args) => Subscriber_ConfigurationChanged(connectionID, subscriberID);
                subscriber.DataSubscriber.NewMeasurements += (sender, args) => Subscriber_NewMeasurements(connectionID, subscriberID, args.Argument);
                subscriber.DataSubscriber.StatusMessage += (sender, args) => Subscriber_StatusMessage(connectionID, subscriberID, args.Argument);
                subscriber.DataSubscriber.ProcessException += (sender, args) => Subscriber_ProcessException(connectionID, subscriberID, args.Argument);
                subscriber.DataSubscriber.ConnectionString = GetInternalPublisherConnectionString();
                subscriber.DataSubscriber.CompressionModes = CompressionModes.TSSC | CompressionModes.GZip;
                subscriber.DataSubscriber.AutoSynchronizeMetadata = false;
                subscriber.DataSubscriber.ReceiveInternalMetadata = true;
                subscriber.DataSubscriber.ReceiveExternalMetadata = true;
                subscriber.DataSubscriber.Initialize();
                return subscriber;
            });
        }

        private string GetInternalPublisherConnectionString()
        {
            ConfigurationFile configurationFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection internalDataPublisher = configurationFile.Settings["internaldatapublisher"];
            string configurationString = internalDataPublisher["ConfigurationString"]?.Value ?? "";
            Dictionary<string, string> settings = configurationString.ParseKeyValuePairs();

            if (!settings.TryGetValue("port", out string portSetting) || !int.TryParse(portSetting, out int port))
                port = 6165;

            if (settings.TryGetValue("interface", out string interfaceSetting))
                interfaceSetting = $"Interface={interfaceSetting}";
            else
                interfaceSetting = string.Empty;

            return $"Server=localhost:{port};{interfaceSetting};BypassStatistics=true";
        }

        private SubscriptionInfo ToSubscriptionInfo(string subscriberID, JObject obj)
        {
            dynamic info = obj;
            bool synchronized = info.Synchronized ?? false;

            IEnumerable<dynamic> formatRecords = info.FormatRecords;

            if (formatRecords != null)
                SetMeasurementFormats(subscriberID, formatRecords);

            if (synchronized)
                return ToSynchronizedSubscriptionInfo(obj);

            return ToUnsynchronizedSubscriptionInfo(obj);
        }

        private SynchronizedSubscriptionInfo ToSynchronizedSubscriptionInfo(JObject obj)
        {
            dynamic info = obj;

            if (info.remotelySynchronized == null)
                info.remotelySynchronized = info.RemotelySynchronized ?? false;

            if (info.framesPerSecond == null)
                info.framesPerSecond = info.FramesPerSecond ?? 30;

            return obj.ToObject<SynchronizedSubscriptionInfo>();
        }

        private UnsynchronizedSubscriptionInfo ToUnsynchronizedSubscriptionInfo(JObject obj)
        {
            dynamic info = obj;

            if (info.throttled == null)
                info.throttled = info.Throttled ?? false;

            return obj.ToObject<UnsynchronizedSubscriptionInfo>();
        }

        private object FromDataRow(DataRow dataRow)
        {
            JObject obj = new JObject();

            foreach (DataColumn dataColumn in dataRow.Table.Columns)
                obj[dataColumn.ColumnName] = JToken.FromObject(dataRow[dataColumn]);

            return obj;
        }

        private object ToJsonMeasurement(Subscriber subscriber, IMeasurement measurement)
        {
            Guid signalID = measurement.ID;
            double value = measurement.AdjustedValue;
            dynamic obj = new JObject();

            obj.signalID = signalID;

            if (subscriber.TryGetFormat(signalID, out string format, out string dataType))
                obj.value = string.Format(format, ConvertValueToType(value, dataType));
            else
                obj.value = value;

            obj.timestamp = (measurement.Timestamp - UnixTimeTag.BaseTicks).ToMilliseconds();

            return obj;
        }

        private object ConvertValueToType(double value, string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
                dataType = "System.Double";

            try
            {
                switch (dataType)
                {
                    case "System.Double":
                        return value;
                    case "System.DateTime":
                        return new DateTime((long)value);
                }

                return Convert.ChangeType(value, Type.GetType(dataType) ?? typeof(double));
            }
            catch
            {
                return value;
            }
        }

        private void Subscriber_ConnectionEstablished(string connectionID, string subscriberID)
        {
            Clients.Client(connectionID).ConnectionEstablished(subscriberID);
        }

        private void Subscriber_ConnectionTerminated(string connectionID, string subscriberID)
        {
            Clients.Client(connectionID).ConnectionTerminated(connectionID, subscriberID);
        }

        private void Subscriber_NewMeasurements(string connectionID, string subscriberID, ICollection<IMeasurement> measurements)
        {
            Subscriber subscriber = GetOrCreate(subscriberID);

            object data = measurements
                .Select(measurement => ToJsonMeasurement(subscriber, measurement))
                .ToArray();

            Clients.Client(connectionID).NewMeasurements(subscriberID, data);
        }

        private void Subscriber_MetadataReceived(string connectionID, string subscriberID, Subscriber subscriber, DataSet metadata)
        {
            subscriber.Metadata = metadata;
            Clients.Client(connectionID).MetadataReceived(subscriberID);
        }

        private void Subscriber_ConfigurationChanged(string connectionID, string subscriberID)
        {
            Clients.Client(connectionID).ConfigurationChanged(subscriberID);
        }

        private void Subscriber_StatusMessage(string connectionID, string subscriberID, string message)
        {
            Clients.Client(connectionID).StatusMessage(subscriberID, message);
        }

        private void Subscriber_ProcessException(string connectionID, string subscriberID, Exception ex)
        {
            Clients.Client(connectionID).ProcessException(subscriberID, ex);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<string, Connection> s_connectionLookup;

        // Static Constructor
        static SubscriberHub()
        {
            s_connectionLookup = new ConcurrentDictionary<string, Connection>();
        }

        #endregion
    }
}