//******************************************************************************************************
//  PIBufferInputAdapter.cs - Gbtc
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
//  11/10/2025 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PIAdapters;

/// <summary>
/// Retrieves historical OSI-PI data using AF-SDK.
/// </summary>
[Description("OSI-PI: Reads historical measurements from an OSI-PI server using AF-SDK for a specific Timeframe on Command.")]
public class PIBufferInputAdapter : InputAdapterBase
{
    #region [ Members ]

    // Constants
    private const long DefaultPublicationInterval = 333333;
    private const int DefaultPageFactor = 1;

    // Fields
    private PIConnection m_connection;                                  // PI server connection
    private IEnumerator<AFValue> m_dataReader;                          // Data reader
    private AFTime m_startTime;
    private AFTime m_stopTime;
    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new instance of the <see cref="PIAdapters.PIBufferInputAdapter"/>.
    /// </summary>
    public PIBufferInputAdapter() {}

    #endregion

    #region [ Properties ]

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
    /// Gets or sets value paging factor to read more data per page from PI.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define a paging factor to read more data per page from PI.")]
    [DefaultValue(DefaultPageFactor)]
    public int PageFactor { get; set; } = DefaultPageFactor;

    /// <summary>
    /// Gets the flag indicating if this adapter supports temporal processing.
    /// </summary>
    public override bool SupportsTemporalProcessing => false;
    

    /// <summary>
    /// Returns the detailed status of the data input source.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();
            status.Append(base.Status);

            status.AppendFormat("        OSI-PI server name: {0}\r\n", ServerName);
            status.AppendFormat("       Connected to server: {0}\r\n", m_connection is null ? "No" : m_connection.Connected ? "Yes" : "No");
            status.AppendFormat("             Paging factor: {0:#,##0}\r\n", PageFactor);
            status.AppendFormat("            Start time-tag: {0}\r\n", m_startTime);
            status.AppendFormat("             Stop time-tag: {0}\r\n", m_stopTime);
            return status.ToString();
        }
    }

    protected override bool UseAsyncConnect => false;

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="PIAdapters.PIPBInputAdapter"/> object and optionally releases the managed resources.
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
                    if (m_connection is not null)
                    {
                        m_connection.Dispose();
                        m_connection = null;
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

    /// <summary>
    /// Initializes this <see cref="PIAdapters.PIBufferInputAdapter"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><b>HistorianID</b>, <b>Server</b>, <b>Port</b>, <b>Protocol</b>, or <b>InitiateConnection</b> is missing from the <see cref="AdapterBase.Settings"/>.</exception>
    public override void Initialize()
    {
        base.Initialize();

        Dictionary<string, string> settings = Settings;

        // Validate settings.
        if (!settings.TryGetValue(nameof(ServerName), out string setting))
            throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format serverName=myServerName to the connection string.");

        ServerName = setting;

        UserName = settings.TryGetValue(nameof(UserName), out setting) ? setting : null;

        Password = settings.TryGetValue(nameof(Password), out setting) ? setting : null;

        ConnectTimeout = settings.TryGetValue(nameof(ConnectTimeout), out setting) ? Convert.ToInt32(setting) : PIConnection.DefaultConnectTimeout;



        if (settings.TryGetValue(nameof(PageFactor), out setting) && int.TryParse(setting, out int pageFactor) && pageFactor > 0)
            PageFactor = pageFactor;
        else
            PageFactor = DefaultPageFactor;

        GetPIConnection();
    }

    /// <summary>
    /// Gets a short one-line status of this <see cref="PIAdapters.PIPBInputAdapter"/>.
    /// </summary>
    /// <param name="maxLength">Maximum length of the status message.</param>
    /// <returns>Text of the status message.</returns>
    public override string GetShortStatus(int maxLength)
    {
        return $"Last Published data for {m_startTime:yyyy-MM-dd HH:mm:ss.fff} to {m_stopTime:yyyy-MM-dd HH:mm:ss.fff}";
    }

    /// <summary>
    /// Attempts to connect to this <see cref="PIAdapters.PIPBInputAdapter"/>.
    /// </summary>
    private void GetPIConnection()
    {
            m_connection = new PIConnection
            {
                ServerName = ServerName,
                UserName = UserName,
                Password = Password,
                ConnectTimeout = ConnectTimeout
            };

            m_connection.Open();
    }

    private IEnumerable<AFValue> ReadData(AFTime startTime, AFTime endTime, PIPointList points, int sampleRate)
    {
        TimeSortedValueScanner scanner = new TimeSortedValueScanner
        {
            Points = points,
            StartTime = startTime,
            EndTime = endTime,
            DataReadExceptionHandler = ex => OnProcessException(MessageLevel.Warning, ex)
        };

        if (sampleRate <= 0)
            return scanner.Read(PageFactor);
        else
            return scanner.ReadInterpolated(new AFTimeSpan(new TimeSpan(0,0,sampleRate)), PageFactor);
     }


    /// <summary>
    /// Commnad that reads the PI Buffer for the specified time range and tags.
    /// </summary>
    /// <param name="start"> The start Time of the requested data.</param>
    /// <param name="end">The end time for the data requested.</param>
    /// <param name="tags">The list of PI Tags of the data requested separated by ;.</param>
    /// <returns>A string representing the read buffer data as comma-separated values in form tag:data1,time1;data2,time2 \newLine</returns>
    public string ReadBuffer(DateTime start, DateTime end, string tags, int interpolationInterval)
    {
        if (start.Kind == DateTimeKind.Unspecified)
            start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        if (end.Kind == DateTimeKind.Unspecified)
            end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        string[] tagArray = tags.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        if (!Enabled || m_connection is null || tagArray.Length == 0)
        {
            OnStatusMessage(MessageLevel.Info, "No PI Tags have been requested for reading, historian read canceled.");
            return string.Empty;
        }
        if (start > end)
        {
            OnStatusMessage(MessageLevel.Info, "The start time is greater than the end time, historian read canceled.");
            return string.Empty;
        }

        Dictionary<string, List<Tuple<long, double>>> tagList = new(StringComparer.OrdinalIgnoreCase);
        PIPointList points = [];


        foreach (string tag in tagArray)
        {

            if (PIPoint.TryFindPIPoint(m_connection.Server, tag, out PIPoint point))
            {
                tagList.Add(tag, new List<Tuple<long, double>>());
                points.Add(point);
            }
        }

        if (tagList.Count == 0)
        {
            OnStatusMessage(MessageLevel.Info, "No matching PI points found for configured tags, historian read canceled.");
            return string.Empty;
        }

        m_startTime = start < DateTime.MinValue ? DateTime.MinValue : start > DateTime.MaxValue ? DateTime.MaxValue : start;
        m_stopTime = end < DateTime.MinValue ? DateTime.MinValue : end > DateTime.MaxValue ? DateTime.MaxValue : end;

        m_dataReader = ReadData(m_startTime, m_stopTime, points, interpolationInterval).GetEnumerator();

        while (m_dataReader.MoveNext())
        {
            AFValue currentPoint = m_dataReader.Current;

            if (currentPoint is null)
                throw new NullReferenceException("PI data read returned a null value.");

            long timestamp = currentPoint.Timestamp.UtcTime.Ticks;
            double Value = Convert.ToDouble(currentPoint.Value);

            if (tagList.ContainsKey(currentPoint.PIPoint.Name))
            {
                tagList[currentPoint.PIPoint.Name].Add(Tuple.Create(timestamp, Value));
            }
        }

        return string.Join(Environment.NewLine,
            tagList.Select(item => $"{item.Key}:{string.Join(";", item.Value.Select(v => $"{v.Item2},{v.Item1}"))}"));
    }

    
    /// <inheritdoc/>
    protected override void AttemptConnection() {}

    /// <inheritdoc/>
    protected override void AttemptDisconnection() {}

    #endregion
}