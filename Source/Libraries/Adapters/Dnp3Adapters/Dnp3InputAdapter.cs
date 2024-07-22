//******************************************************************************************************
//  Dnp3InputAdapter.cs - Gbtc
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
//  10/05/2012 - Adam Crain
//       Generated original version of source code.
//  03/27/2014 - Adam Crain
//       Updated to used latest opendnp3 code. 
//  03/28/2014 - J. Ritchie Carroll
//       Attached to an adapter instance to proxy IDNP3Manager singleton log messages.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using GSF;
using GSF.Diagnostics;
using GSF.IO;
using GSF.TimeSeries.Adapters;

namespace DNP3Adapters;

/// <summary>
/// Input adapter that reads measurements from a remote dnp3 endpoint.
/// </summary>
[Description("DNP3: Reads measurements from a remote dnp3 endpoint")]
public class DNP3InputAdapter : InputAdapterBase
{
    #region [ Members ]

    // Nested Types
    private class ChannelListener(Action<ChannelState> onStateChange) : IChannelListener
    {
        public void OnStateChange(ChannelState state)
        {
            onStateChange(state);
        }
    }

    // Class used to proxy dnp3 manager log entries to Iaon session
    private class IaonProxyLogHandler : ILogHandler
    {
        /// <summary>
        /// Handler for log entries.
        /// </summary>
        /// <param name="entry"><see cref="LogEntry"/> to handle.</param>
        public void Log(LogEntry entry)
        {
            // We avoid race conditions by always making sure access to status proxy is locked - this only
            // contends with adapter initialization and disposal so contention will not be the normal case
            lock (s_adapters)
            {
                if (s_statusProxy is null || s_statusProxy.m_disposed)
                    return;

                if ((entry.filter.Flags & LogFilters.ERROR) > 0)
                {
                    // Expose errors through exception processor
                    InvalidOperationException exception = new(FormatLogEntry(entry));
                    s_statusProxy.OnProcessException(MessageLevel.Error, exception);
                }
                else
                {
                    // For other messages, we just expose as a normal status
                    string message = FormatLogEntry(entry);

                    if ((entry.filter.Flags & LogFilters.WARNING) > 0)
                        s_statusProxy.OnStatusMessage(MessageLevel.Warning, message);
                    else if ((entry.filter.Flags & LogFilters.DEBUG) > 0)
                        s_statusProxy.OnStatusMessage(MessageLevel.Debug, message);
                    else
                        s_statusProxy.OnStatusMessage(MessageLevel.Info, message);
                }
            }
        }

        private static string FormatLogEntry(LogEntry entry)
        {
            StringBuilder entryText = new();

            entryText.Append(entry.message);
            entryText.Append($" ({LogFilters.GetFilterString(entry.filter.Flags)})");

            if (!string.IsNullOrWhiteSpace(entry.location))
                entryText.Append($" @ {entry.location}");

            return entryText.ToString();
        }
    }

    // Constants
    private const double DefaultPollingInterval = 2.0D;
    private const double DefaultTimestampDifferentiation = 1.0D;
    private const bool DefaultMapQualityToStateFlags = true;
    private const bool DefaultPublishFlagsAsSeparateMeasurements = false;
    private const string DefaultTagMatchPattern = @"(?<TagName>.+)(?<SignalType>\:(ALOG|DIGI))\d+";
    private const string DefaultQualityTagSuffix = "!FLAGS";

    // Fields
    private TimeSpan m_pollingInterval;         // Interval, in seconds, at which the adapter will poll the DNP3 device
    private MasterConfiguration m_masterConfig; // Configuration for the master set during the Initialize call
    private TimeSeriesSOEHandler m_soeHandler;  // Time-series sequence of events handler
    private IChannel m_channel;                 // Communications channel set during the AttemptConnection call and used in AttemptDisconnect
    private bool m_active;                      // Flag that determines if the port/master has been added so that the resource can be cleaned up
    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new instance of the <see cref="DNP3Adapters"/> class.
    /// </summary>
    public DNP3InputAdapter()
    {
        m_pollingInterval = TimeSpan.FromSeconds(DefaultPollingInterval);
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the name of the XML file from which the communication parameters will be read.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the name of the XML file from which the communication configuration will be read. Include fully qualified path if file name is not in installation folder.")]
    public string CommsFilePath { get; set; }

    /// <summary>
    /// Gets or sets the name of the XML file from which the measurement mapping is read.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the name of the XML file from which the communication configuration will be read. Include fully qualified path if file name is not in installation folder.")]
    public string MappingFilePath { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if DNP3 quality flags should be mapped to measurement state flags.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define flag that determines if DNP3 quality flags should be mapped to measurement state flags.")]
    [DefaultValue(DefaultMapQualityToStateFlags)]
    public bool MapQualityToStateFlags { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if DNP3 quality flags should be published as separate measurement outputs.
    /// </summary>
    // NOTE: Like value measurements, quality measurements are expected to be pre-defined outside the adapter, i.e., the
    // adapter does not currently auto-create tags. There is an external Python script that exists for this purpose.
    // This flag is used to determine if the adapter should include DNP3 quality flags as part of the measurement outputs.
    // The "TagMatchPattern" and "QualityTagSuffix" properties are used to determine the tag name for the quality flags.
    // Generally, the value tag format is assumed to be "<TagName><SignalType>" and the quality tag format is assumed to
    // be a variation of the value tag format with a suffix appended to the tag name, e.g., "<TagName>!FLAGS<SignalType>".
    // In this current implementation, quality flags measurements are expected to be an analog signal type, i.e.: "ALOG".
    [ConnectionStringParameter]
    [Description("Define flag that determines if DNP3 quality flags should be published as separate measurement outputs.")]
    [DefaultValue(DefaultPublishFlagsAsSeparateMeasurements)]
    public bool PublishFlagsAsSeparateMeasurements { get; set; }

    /// <summary>
    /// Gets or sets the regular expression pattern used to match tag names for quality flag outputs.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the regular expression pattern used to match tag names for quality flag outputs. Expects capture groups: \"TagName\" and \"SignalType\".")]
    [DefaultValue(DefaultTagMatchPattern)]
    public string TagMatchPattern { get; set; }

    /// <summary>
    /// Gets or sets the suffix to append to the tag name for DNP3 quality flag measurement outputs.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the suffix to append to the tag name for DNP3 quality flag measurement outputs.")]
    [DefaultValue(DefaultQualityTagSuffix)]
    public string QualityTagSuffix { get; set; }

    /// <summary>
    /// Gets or sets the time interval, in milliseconds, to insert between consecutive
    /// data points for a given signal that were received at the exact same time.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the time interval, in milliseconds, to insert between consecutive data points for with the same ID and timestamp.")]
    [DefaultValue(DefaultTimestampDifferentiation)]
    public double TimestampDifferentiation
    {
        get => m_soeHandler?.TimestampDifferentiation.TotalMilliseconds ?? 0.0D;
        set
        {
            if (m_soeHandler is not null)
                m_soeHandler.TimestampDifferentiation = TimeSpan.FromMilliseconds(value);
        }
    }

    /// <summary>
    /// Gets or sets the interval, in seconds, at which the adapter will poll the DNP3 device.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the interval, in seconds, at which the adapter will poll the DNP3 device.")]
    [DefaultValue(DefaultPollingInterval)]
    public double PollingInterval
    {
        get => m_pollingInterval.TotalSeconds;
        set => m_pollingInterval = TimeSpan.FromSeconds(value);
    }

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

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="DNP3InputAdapter"/> object and optionally releases the managed resources.
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

            if (m_active)
            {
                m_active = false;

                if (m_channel is not null)
                {
                    // Shutdown the communications channel
                    m_channel.Shutdown();
                    m_channel = null;
                }
            }

            // Detach from the time-series sequence of events new measurements event
            if (m_soeHandler is not null)
            {
                m_soeHandler.NewMeasurements -= OnNewMeasurements;
                m_soeHandler = null;
            }

            lock (s_adapters)
            {
                // Remove this adapter from the available list
                s_adapters.Remove(this);

                // See if we are disposing the status proxy instance
                if (ReferenceEquals(s_statusProxy, this))
                {
                    // Attempt to find a new status proxy
                    s_statusProxy = s_adapters.Count > 0 ? s_adapters[0] : null;
                }
            }
        }
        finally
        {
            m_disposed = true;          // Prevent duplicate dispose.
            base.Dispose(disposing);    // Call base class Dispose().
        }
    }

    /// <summary>
    /// Initializes <see cref="DNP3InputAdapter"/>
    /// </summary>
    public override void Initialize()
    {
        Dictionary<string, string> settings = Settings;

        base.Initialize();

        if (!settings.TryGetValue(nameof(CommsFilePath), out string setting) || string.IsNullOrWhiteSpace(setting))
            throw new ArgumentException($"The required {nameof(CommsFilePath)} parameter was not specified");

        CommsFilePath = FilePath.GetAbsolutePath(setting);

        if (!File.Exists(CommsFilePath))
            throw new FileNotFoundException($"The specified {nameof(CommsFilePath)} \"{CommsFilePath}\" was not found");

        m_masterConfig = ReadConfig<MasterConfiguration>(CommsFilePath);

        if (!settings.TryGetValue(nameof(MappingFilePath), out setting) || string.IsNullOrWhiteSpace(setting))
            throw new ArgumentException($"The required {nameof(MappingFilePath)} parameter was not specified");

        MappingFilePath = FilePath.GetAbsolutePath(setting);

        if (!File.Exists(MappingFilePath))
            throw new FileNotFoundException($"The specified {nameof(MappingFilePath)} \"{MappingFilePath}\" was not found");

        MeasurementMap measurementMap = ReadConfig<MeasurementMap>(MappingFilePath);

        MapQualityToStateFlags = !settings.TryGetValue(nameof(MapQualityToStateFlags), out setting) || setting.ParseBoolean();
        PublishFlagsAsSeparateMeasurements = settings.TryGetValue(nameof(PublishFlagsAsSeparateMeasurements), out setting) && setting.ParseBoolean();
        TagMatchPattern = settings.TryGetValue(nameof(TagMatchPattern), out setting) ? setting : DefaultTagMatchPattern;

        if (!TagMatchPattern.Contains("<TagName>") || !TagMatchPattern.Contains("<SignalType>"))
            throw new ArgumentException($"The {nameof(TagMatchPattern)} must contain capture groups for \"TagName\" and \"SignalType\"");

        QualityTagSuffix = settings.TryGetValue(nameof(QualityTagSuffix), out setting) ? setting : DefaultQualityTagSuffix;

        MeasurementLookup measurementLookup = new(measurementMap)
        {
            GetDataSource = () => DataSource,
            MapQualityToStateFlags = MapQualityToStateFlags,
            PublishFlagsAsSeparateMeasurements = PublishFlagsAsSeparateMeasurements,
            TagMatchRegex = new Regex(TagMatchPattern, RegexOptions.Compiled),
            QualityTagSuffix = QualityTagSuffix
        };

        m_soeHandler = new TimeSeriesSOEHandler(measurementLookup);
        m_soeHandler.NewMeasurements += OnNewMeasurements;

        // The TimestampDifferentiation property is a pass-through to the m_soeHandler.TimestampDifferentiation,
        // so m_soeHandler must be initialized before reading this property from the connection string
        if (settings.TryGetValue(nameof(TimestampDifferentiation), out setting) && double.TryParse(setting, out double timestampDifferentiation))
            TimestampDifferentiation = timestampDifferentiation;
        else
            TimestampDifferentiation = DefaultTimestampDifferentiation;

        m_soeHandler.TimestampDifferentiation = TimeSpan.FromMilliseconds(TimestampDifferentiation);

        if (settings.TryGetValue(nameof(PollingInterval), out setting) && double.TryParse(setting, out double pollingInterval))
            PollingInterval = pollingInterval;

        // Attach to output measurements for DNP3 device - just informs routing engine of expected measurements
        if (OutputMeasurements?.Length == 0)
            OutputMeasurements = ParseOutputMeasurements(DataSource, false, $"FILTER ActiveMeasurements WHERE Device = '{Name}'");

        lock (s_adapters)
        {
            // Add adapter to list of available adapters 
            s_adapters.Add(this);

            // If no adapter has been designated as the status proxy, assign this one
            s_statusProxy ??= this;
        }
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
        TcpClientConfig tcpConfig = m_masterConfig.client;
        string endPoint = $"{tcpConfig.address}:{tcpConfig.port}";
        TimeSpan minRetry = TimeSpan.FromMilliseconds(tcpConfig.minRetryMs);
        TimeSpan maxRetry = TimeSpan.FromMilliseconds(tcpConfig.maxRetryMs);
        TimeSpan reconnectDelay = TimeSpan.FromMilliseconds(tcpConfig.reconnectDelayMs);
        ChannelRetry channelRetry = new(minRetry, maxRetry, reconnectDelay);
        IChannelListener channelListener = new ChannelListener(state => OnStatusMessage(MessageLevel.Info, $"{endPoint} - Channel state change: {state}"));

        IChannel channel = s_manager.AddTCPClient(endPoint, tcpConfig.level, channelRetry, [new IPEndpoint(tcpConfig.address, tcpConfig.port)], channelListener);
        m_channel = channel;

        IMaster master = channel.AddMaster(endPoint, m_soeHandler, DefaultMasterApplication.Instance, m_masterConfig.master);

        if (m_pollingInterval > TimeSpan.Zero)
            master.AddClassScan(ClassField.AllClasses, m_pollingInterval, m_soeHandler, TaskConfig.Default);

        master.Enable();
        m_active = true;
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
        if (!m_active)
            return;

        m_active = false;

        if (m_channel is null)
            return;

        m_channel.Shutdown();
        m_channel = null;
    }

    /// <summary>
    /// Gets a short one-line status of this <see cref="DNP3InputAdapter"/>.
    /// </summary>
    /// <param name="maxLength">Maximum number of available characters for display.</param>
    /// <returns>A short one-line summary of the current status of this <see cref="DNP3InputAdapter"/>.</returns>
    public override string GetShortStatus(int maxLength)
    {
        return $"Received {ProcessedMeasurements:N0} measurements so far...".CenterText(maxLength);
    }

    #endregion

    #region [ Static ]

    // Static Fields

    // DNP3 manager shared across all the DNP3 input adapters, concurrency level defaults to number of processors
    private static readonly IDNP3Manager s_manager;

    // We maintain a list of dnp3 adapters that can be used as status adapters for proxying messages from manager
    private static readonly List<DNP3InputAdapter> s_adapters;
    private static DNP3InputAdapter s_statusProxy;

    // Static Constructor
    static DNP3InputAdapter()
    {
        s_adapters = [];
        s_manager = DNP3ManagerFactory.CreateManager(Environment.ProcessorCount, new IaonProxyLogHandler());
    }

    // Static Methods
    private static T ReadConfig<T>(string path)
    {
        XmlSerializer serializer = new(typeof(T));

        using TextReader reader = new StreamReader(FilePath.GetAbsolutePath(path));
        return (T)serializer.Deserialize(reader);
    }

    #endregion
}