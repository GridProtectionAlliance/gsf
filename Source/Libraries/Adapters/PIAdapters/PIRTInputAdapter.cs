//******************************************************************************************************
//  PIRTInputAdapter.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  08/19/2012 - Ryan McCoy
//       Generated original version of source code.
//  06/18/2014 - J. Ritchie Carroll
//       Updated code to use PIConnection instance.
//  12/17/2014 - J. Ritchie Carroll
//       Updated to use AF-SDK
//
//******************************************************************************************************
// ReSharper disable EventNeverSubscribedTo.Local

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;

namespace PIAdapters;

/// <summary>
/// Uses PI event pipes to deliver real-time PI data to GSF host
/// </summary>
[Description("OSI-PI: Reads real-time measurements from an OSI-PI server using AF-SDK.")]
public class PIRTInputAdapter : InputAdapterBase
{
    #region [ Members ]

    // Nested Types
    private class DataUpdateObserver : IObserver<AFDataPipeEvent>
    {
        public event EventHandler<EventArgs<AFValue>> DataUpdated;
        public event EventHandler Completed;
            
        private readonly Action<Exception> m_exceptionHandler;

        public DataUpdateObserver(Action<Exception> exceptionHandler)
        {
            m_exceptionHandler = exceptionHandler;
        }

        public void OnCompleted() => Completed?.Invoke(this, EventArgs.Empty);

        public void OnError(Exception error) => m_exceptionHandler?.Invoke(error);

        public void OnNext(AFDataPipeEvent value)
        {
            if (value.Action != AFDataPipeAction.Delete)
                DataUpdated?.Invoke(this, new EventArgs<AFValue>(value.Value));
        }
    }

    // Fields
    private readonly ConcurrentDictionary<int, MeasurementKey> m_tagKeyMap; // Map PI tag ID to GSFSchema measurement keys
    private readonly ShortSynchronizedOperation m_restartConnection;        // Restart connection operation
    private readonly ShortSynchronizedOperation m_readEvents;               // Reads PI events
    private readonly System.Timers.Timer m_eventTimer;                      // Timer to trigger event reader
    private PIConnection m_connection;                                      // PI server connection
    private PIDataPipe m_dataPipe;                                          // PI data pipe
    private List<PIPoint> m_dataPoints;                                     // Last list of subscribed data points
    private readonly DataUpdateObserver m_dataUpdateObserver;               // Custom observer class for handling point updates
    private Ticks m_lastReceivedTimestamp;                                  // Last received timestamp from PI event pipe
    private double m_lastReceivedValue;
    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Receives real-time updates from PI
    /// </summary>
    public PIRTInputAdapter()
    {
        m_tagKeyMap = new ConcurrentDictionary<int, MeasurementKey>();
        m_restartConnection = new ShortSynchronizedOperation(Start);
        m_readEvents = new ShortSynchronizedOperation(ReadEvents);
        m_dataUpdateObserver = new DataUpdateObserver(ex => OnProcessException(MessageLevel.Error, ex));
        m_dataUpdateObserver.DataUpdated += m_dataUpdateObserver_DataUpdated;
        m_eventTimer = new System.Timers.Timer(1000.0D);
        m_eventTimer.Elapsed += m_eventTimer_Elapsed;
        m_eventTimer.AutoReset = true;
        m_eventTimer.Enabled = false;
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Returns false to indicate that this <see cref="PIRTInputAdapter"/> does NOT connect asynchronously
    /// </summary>
    protected override bool UseAsyncConnect => false;

    /// <summary>
    /// Returns false to indicate that this <see cref="PIRTInputAdapter"/> does NOT support temporal processing. 
    /// Temporal processing is supported in a separate adapter that is not driven by event pipes.
    /// </summary>
    public override bool SupportsTemporalProcessing => false;

    /// <summary>
    /// Gets or sets the measurements that this <see cref="PIRTInputAdapter"/> has been requested to provide
    /// </summary>
    public override MeasurementKey[] RequestedOutputMeasurementKeys
    {
        get => base.RequestedOutputMeasurementKeys;
        set
        {
            base.RequestedOutputMeasurementKeys = value;

            if (value is not null && value.Any())
                SubscribeToPointUpdates(value);
        }
    }


    /// <summary>
    /// Gets or sets the name of the PI server for the adapter's PI connection.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the name of the PI server for the adapter's PI connection.")]
    public string ServerName { get; set; }

    /// <summary>
    /// Gets or sets the name of the PI user ID for the adapter's PI connection.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the name of the PI user ID for the adapter's PI connection.")]
    [DefaultValue("")]
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password used for the adapter's PI connection.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the password used for the adapter's PI connection.")]
    [DefaultValue("")]
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the timeout interval (in milliseconds) for the adapter's connection")]
    [DefaultValue(PIConnection.DefaultConnectTimeout)]
    public int ConnectTimeout { get; set; }

    /// <summary>
    /// Last timestamp received from PI
    /// </summary>
    public DateTime LastReceivedTimestamp => m_lastReceivedTimestamp;

    /// <summary>
    /// Returns the status of the adapter
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.Append(base.Status);

            status.AppendFormat("   Last Received Timestamp: {0}", m_lastReceivedTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff"));
            status.AppendLine();
            status.AppendFormat("       Last Received Value: {0:N3}", m_lastReceivedValue);
            status.AppendLine();
            status.AppendFormat("  Latency of Last Received: {0}", (DateTime.UtcNow.Ticks - m_lastReceivedTimestamp).ToElapsedTimeString(3));
            status.AppendLine();

            if (m_dataPoints is not null)
            {
                status.AppendFormat("       PI Data Point Count: {0:N0}: {1}...", m_dataPoints.Count, m_dataPoints.Select(p => p.ID.ToString()).Take(5).ToDelimitedString(", "));
                status.AppendLine();
            }

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="PIRTInputAdapter"/> object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (m_disposed)
            return;

        try
        {
            if (!disposing)
                return;

            if (m_connection is not null)
            {
                m_connection.Disconnected -= m_connection_Disconnected;
                m_connection.Dispose();
                m_connection = null;
            }

            m_dataPipe?.Dispose();

            if (m_eventTimer is not null)
            {
                m_eventTimer.Elapsed -= m_eventTimer_Elapsed;
                m_eventTimer.Dispose();
            }
        }
        finally
        {
            m_disposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    /// <summary>
    /// Reads values from the connection string and prepares this <see cref="PIRTInputAdapter"/> for connecting to PI
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        //m_measurements = new List<IMeasurement>();

        Dictionary<string, string> settings = Settings;

        if (!settings.TryGetValue(nameof(ServerName), out string setting))
            throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format 'servername=myservername' to the connection string.");

        ServerName = setting;

        UserName = settings.TryGetValue(nameof(UserName), out setting) ? setting : null;

        Password = settings.TryGetValue(nameof(Password), out setting) ? setting : null;

        if (settings.TryGetValue(nameof(ConnectTimeout), out setting) && int.TryParse(setting, out int value))
            ConnectTimeout = value;
        else
            ConnectTimeout = PIConnection.DefaultConnectTimeout;            
    }

    /// <summary>
    /// Connects to the configured PI server.
    /// </summary>
    protected override void AttemptConnection()
    {
        m_connection = new PIConnection
        {
            ServerName = ServerName,
            UserName = UserName,
            Password = Password,
            ConnectTimeout = ConnectTimeout
        };

        m_connection.Disconnected += m_connection_Disconnected;
        m_connection.Open();

        m_dataPipe = new PIDataPipe(AFDataPipeType.Snapshot);
        m_dataPipe.Subscribe(m_dataUpdateObserver);

        if (AutoStart && OutputMeasurements is not null && OutputMeasurements.Any())
            SubscribeToPointUpdates(this.OutputMeasurementKeys());
    }

    /// <summary>
    /// Disconnects from the configured PI server if a connection is open
    /// </summary>
    protected override void AttemptDisconnection()
    {
        m_eventTimer.Enabled = false;

        if (m_dataPoints is not null)
        {
            m_dataPoints.Clear();
            m_dataPoints = null;
        }

        if (m_dataPipe is not null)
        {
            m_dataPipe.Dispose();
            m_dataPipe = null;
        }

        if (m_connection is not null)
        {
            m_connection.Dispose();
            m_connection = null;
        }
    }

    /// <summary>
    /// Returns brief status with measurements processed
    /// </summary>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public override string GetShortStatus(int maxLength) => $"Received {ProcessedMeasurements} measurements from PI...".CenterText(maxLength);

    private void SubscribeToPointUpdates(MeasurementKey[] keys)
    {
        OnStatusMessage(MessageLevel.Info, $"Subscribing to updates for {keys.Length} measurements...");

        var query = from row in DataSource.Tables["ActiveMeasurements"].AsEnumerable()
            from key in keys
            where row["ID"].ToString() == key.ToString()
            select new
            {
                Key = key,
                AlternateTag = row["AlternateTag"].ToString(),
                PointTag = row["PointTag"].ToString()
            };

        List<PIPoint> dataPoints = [];

        foreach (var row in query)
        {
            string tagName = row.PointTag;

            if (!string.IsNullOrWhiteSpace(row.AlternateTag))
                tagName = row.AlternateTag;

        #if DEBUG
            OnStatusMessage(MessageLevel.Debug, $"DEBUG: Looking up point tag '{tagName}'...");
        #endif

            PIPoint point = GetPIPoint(m_connection.Server, tagName);

            if (point is not null)
            {
            #if DEBUG
                OnStatusMessage(MessageLevel.Debug, $"DEBUG: Found point tag '{tagName}'...");
            #endif
                dataPoints.Add(point);
                m_tagKeyMap[point.ID] = row.Key;
            }
        #if DEBUG
            else
            {
                OnStatusMessage(MessageLevel.Debug, $"DEBUG: Failed to find point tag '{tagName}'...");
            }
        #endif
        }

        // Remove sign-ups for any existing point list
        if (m_dataPoints is not null)
            m_dataPipe.RemoveSignups(m_dataPoints);

        // Sign up for updates on selected points
        AFListResults<PIPoint, AFDataPipeEvent> initialEvents = m_dataPipe.AddSignupsWithInitEvents(dataPoints);

    #if DEBUG
        OnStatusMessage(MessageLevel.Debug, $"DEBUG: Initial event count = {initialEvents.Results.Count}...");
    #endif

        foreach (AFDataPipeEvent item in initialEvents.Results)
        {
        #if DEBUG
            OnStatusMessage(MessageLevel.Debug, "DEBUG: Found initial event for action...");
        #endif

            if (item.Action != AFDataPipeAction.Delete)
                m_dataUpdateObserver_DataUpdated(this, new EventArgs<AFValue>(item.Value));
        }

        m_dataPoints = dataPoints;

        m_eventTimer.Enabled = true;
    }

    private PIPoint GetPIPoint(PIServer server, string tagName)
    {
        PIPoint.TryFindPIPoint(server, tagName, out PIPoint point);
        return point;
    }

    private void m_eventTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
    #if DEBUG
        OnStatusMessage(MessageLevel.Debug, "DEBUG: Timer elapsed...");
    #endif
        m_readEvents.TryRunOnceAsync();
    }

    private void ReadEvents()
    {
        if (m_dataPipe is null)
            return;

    #if DEBUG
        OnStatusMessage(MessageLevel.Debug, "DEBUG: Data pipe called for next 100 GetUpdateEvents...");
    #endif

        AFListResults<PIPoint, AFDataPipeEvent> updateEvents = m_dataPipe.GetUpdateEvents(100);

    #if DEBUG
        OnStatusMessage(MessageLevel.Debug, $"DEBUG: Update event count = {updateEvents.Count}...");
    #endif

        foreach (AFDataPipeEvent item in updateEvents.Results)
        {
        #if DEBUG
            OnStatusMessage(MessageLevel.Debug, "DEBUG: Found update event for action...");
        #endif

            if (item.Action != AFDataPipeAction.Delete)
                m_dataUpdateObserver_DataUpdated(this, new EventArgs<AFValue>(item.Value));
        }
    }

    // PI data updated handler
    private void m_dataUpdateObserver_DataUpdated(object sender, EventArgs<AFValue> e)
    {
    #if DEBUG
        OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler called with a new value: {Convert.ToDouble(e.Argument.Value):N3}...");
    #endif

        AFValue value = e.Argument;

    #if DEBUG
        OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler looking up point ID {value?.PIPoint.ID:N0} in table...");
    #endif

        if (value is not null && m_tagKeyMap.TryGetValue(value.PIPoint.ID, out MeasurementKey key))
        {
        #if DEBUG
            OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler found point ID {value.PIPoint.ID:N0} in table: {key}...");
        #endif

            Measurement measurement = new()
            { 
                Metadata = key.Metadata,
                Value = Convert.ToDouble(value.Value),
                Timestamp = value.Timestamp.UtcTime
            };

            OnNewMeasurements(new[] { measurement });

            m_lastReceivedTimestamp = measurement.Timestamp;
            m_lastReceivedValue = measurement.Value;
        }
    #if DEBUG
        else
        {
            OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler did not find point ID {value?.PIPoint.ID:N0} in table...");
        }
    #endif
    }

    // Since we may get a plethora of these requests, we use a synchronized operation to restart once
    private void m_connection_Disconnected(object sender, EventArgs e) => 
        m_restartConnection.RunOnceAsync();

    #endregion
}