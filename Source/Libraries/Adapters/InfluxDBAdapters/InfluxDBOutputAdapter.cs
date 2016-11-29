//******************************************************************************************************
//  InfluxDBOutputAdapter.cs - Gbtc
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
//  07/02/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace InfluxDBAdapters
{
    /// <summary>
    /// Represents an output adapter that archives measurements to a local archive.
    /// </summary>
    [Description("InfluxDB: Archives measurements to an InfluxDB instance")]
    public class InfluxDBOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="UserName"/>.
        /// </summary>
        public const string DefaultUserName = "root";

        /// <summary>
        /// Default value for <see cref="Password"/>.
        /// </summary>
        public const string DefaultPassword = "root";

        /// <summary>
        /// Default value for <see cref="UseParallelPosting"/>.
        /// </summary>
        public const bool DefaultUseParallelPosting = true;

        /// <summary>
        /// Default value for <see cref="ValuesPerPost"/>.
        /// </summary>
        public const int DefaultValuesPerPost = 50;

        // Fields
        private readonly ShortSynchronizedOperation m_requestRestart;

        private Uri m_requestUri;               // Data posting URI for InfluxDB connection
        private string m_serverUri;             // Base server URI for InfluxDB connection
        private string m_databaseName;          // Database name forInfluxDB connection
        private string m_userName;              // Username for InfluxDB connection
        private string m_password;              // Password for InfluxDB connection
        private bool m_useParallelPosting;      // Enable parallel posting
        private int m_valuesPerPost;            // Maximum values to send per post (when using parallel posting)
        private string m_connectionResponse;    // Response from connection attempt
        private long m_totalValues;             // Total archived values
        private long m_totalPosts;              // Total post to the InfluxDB connection
        private long m_totalParallelGroups;     // Total measurement groups processed in parallel
        private long m_ignoredErrors;           // Total ignored 400 and 500 HTTP status code errors

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="InfluxDBOutputAdapter"/>;
        /// </summary>
        public InfluxDBOutputAdapter()
        {
            m_requestRestart = new ShortSynchronizedOperation(() =>
            {
                if (Enabled)
                    Start();
            });
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the base server URI for the InfluxDB connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the base server URI for the InfluxDB connection (e.g., http://localhost:8086).")]
        public string ServerUri
        {
            get
            {
                return m_serverUri;
            }
            set
            {
                m_serverUri = value;
            }
        }

        /// <summary>
        /// Gets or sets the database name for the InfluxDB connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the database name for the InfluxDB connection. Database will not be created, it is expected to already exist.")]
        public string DatabaseName
        {
            get
            {
                return m_databaseName;
            }
            set
            {
                m_databaseName = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name for the InfluxDB connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the the user name for the InfluxDB connection."), DefaultValue(DefaultUserName)]
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// Gets or sets the password for the InfluxDB connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the password for the InfluxDB connection."), DefaultValue(DefaultPassword)]
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if multiple posts to InfluxDB should be made in parallel.
        /// </summary>
        [ConnectionStringParameter, Description("Defines flag that determines if multiple posts to InfluxDB should be made in parallel."), DefaultValue(DefaultUseParallelPosting)]
        public bool UseParallelPosting
        {
            get
            {
                return m_useParallelPosting;
            }
            set
            {
                m_useParallelPosting = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum values to send per post when <see cref="UseParallelPosting"/> is <c>true</c> for the InfluxDB connection.
        /// </summary>
        [ConnectionStringParameter, Description("When parallel posting is enabled, defines the maximum values to send per post for the InfluxDB connection."), DefaultValue(DefaultValuesPerPost)]
        public int ValuesPerPost
        {
            get
            {
                return m_valuesPerPost;
            }
            set
            {
                m_valuesPerPost = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not to automatically place measurements back into the processing
        /// queue if an exception occurs while processing.  Defaults to false.
        /// </summary>
        [ConnectionStringParameter, Description("Defines whether or not to automatically place measurements back into the processing queue if an exception occurs while processing.  For InfluxDB adapter this defaults to true."), DefaultValue(true)]
        public override bool RequeueOnException
        {
            get
            {
                return base.RequeueOnException;
            }
            set
            {
                base.RequeueOnException = value;
            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this <see cref="InfluxDBOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="InfluxDBOutputAdapter"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a detailed status for this <see cref="InfluxDBOutputAdapter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);

                status.AppendFormat("         InfluxDB base URI: {0}", m_serverUri);
                status.AppendLine();
                status.AppendFormat("      Use parallel posting: {0}", m_useParallelPosting);
                status.AppendLine();

                if (m_useParallelPosting)
                {
                    status.AppendFormat("   Maximum values per post: {0:N0}", m_valuesPerPost);
                    status.AppendLine();
                    status.AppendFormat("  Average parallel threads: {0:0.0}", m_totalParallelGroups / (double)InternalProcessQueue.TotalFunctionCalls);
                    status.AppendLine();
                }

                status.AppendFormat("     Total archived values: {0:N0}", m_totalValues);
                status.AppendLine();
                status.AppendFormat("               Total posts: {0:N0}", m_totalPosts);
                status.AppendLine();
                status.AppendFormat("   Average values per post: {0:R}", Math.Round(m_totalValues / (double)m_totalPosts, 2));
                status.AppendLine();
                status.AppendFormat("       Connection response: {0}", m_connectionResponse);
                status.AppendLine();
                status.AppendFormat("Ignored error status codes: {0:N0} total 400 or 500", m_ignoredErrors);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a brief status of this <see cref="InfluxDBOutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0:N0} measurements via {1:N0} posts to \"{2}\".", m_totalValues, m_totalPosts, m_databaseName).CenterText(maxLength);
        }

        /// <summary>
        /// Initializes <see cref="InfluxDBOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (!settings.TryGetValue("serverUri", out m_serverUri))
                throw new InvalidOperationException("Base server URI is a required setting for the InfluxDB connection. Please add a server URI setting in the format of \"serverUri=http://localhost:8086\" to the connection string.");

            if (!settings.TryGetValue("databaseName", out m_databaseName))
                throw new InvalidOperationException("Database name is a required setting for the InfluxDB connection. Please add a database name setting in the format of \"databaseName=MyDB\" to the connection string.");

            if (settings.TryGetValue("userName", out setting))
                m_userName = setting;
            else
                m_userName = DefaultUserName;

            if (settings.TryGetValue("password", out setting))
                m_password = setting;
            else
                m_password = DefaultPassword;

            if (settings.TryGetValue("useParallelPosting", out setting))
                m_useParallelPosting = setting.ParseBoolean();
            else
                m_useParallelPosting = DefaultUseParallelPosting;

            if (!settings.TryGetValue("valuesPerPost", out setting) || !int.TryParse(setting, out m_valuesPerPost))
                m_valuesPerPost = DefaultValuesPerPost;

            // Since InfluxDB API is web based, you generally want to retry posts in case of issues
            if (!settings.ContainsKey("requeueOnException"))
                RequeueOnException = true;

            // Define request URI
            UriBuilder requestUri = new UriBuilder(m_serverUri);
            requestUri.Path = string.Format("db/{0}/series", m_databaseName.UriEncode());
            requestUri.Query = string.Format("u={0}&p={1}", m_userName.UriEncode(), m_password.UriEncode());
            m_requestUri = requestUri.Uri;
        }

        /// <summary>
        /// Attempts to connect to InfluxDB database.
        /// </summary>
        protected override void AttemptConnection()
        {
            try
            {
                // Setup an authenticate request
                UriBuilder requestUri = new UriBuilder(m_serverUri);
                requestUri.Path = string.Format("db/{0}/authenticate", m_databaseName.UriEncode());
                requestUri.Query = string.Format("u={0}&p={1}", m_userName.UriEncode(), m_password.UriEncode());

                // Create a web request for authentication that will at least make sure the server is available
                HttpWebRequest request = WebRequest.Create(requestUri.Uri) as HttpWebRequest;

                if ((object)request != null)
                {
                    request.ContentType = "application/json";

                    // Attempt query - if this doesn't throw an exception, query succeeded
                    using (WebResponse response = request.GetResponse())
                    {
                        m_connectionResponse = ((HttpWebResponse)response).StatusDescription;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Failed to create an HTTP request from " + requestUri);
                }
            }
            catch (Exception ex)
            {
                m_connectionResponse = ex.Message;

                // Rethrow any captured exceptions, this will restart connection cycle in OutputAdapterBase
                throw;
            }
        }

        /// <summary>
        /// Attempts to disconnect from InfluxDB.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            // Connections are stateless, nothing to do here...
        }

        /// <summary>
        /// Serializes measurements to InfluxDB.
        /// </summary>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if (measurements.Length == 0)
                return;

            if (m_useParallelPosting)
            {
                List<IMeasurement[]> measurementGroups = new List<IMeasurement[]>();
                IMeasurement[] measurementGroup = measurements.Take(m_valuesPerPost).ToArray();
                int skipCount = measurementGroup.Length;

                while (measurementGroup.Length > 0)
                {
                    measurementGroups.Add(measurementGroup);
                    measurementGroup = measurements.Skip(skipCount).Take(m_valuesPerPost).ToArray();
                    skipCount += measurementGroup.Length;
                }

                Parallel.ForEach(measurementGroups, PostMeasurementsToArchive);
                m_totalParallelGroups += measurementGroups.Count;
            }
            else
            {
                PostMeasurementsToArchive(measurements);
            }
        }

        private void PostMeasurementsToArchive(IMeasurement[] measurements)
        {
            const string PostFormat = "{{\"name\":\"{0}\",\"columns\":[\"time\",\"value\",\"quality\"],\"points\":[[{1},{2},\"{3}\"]]}}";

            try
            {
                HttpWebRequest request = WebRequest.Create(m_requestUri) as HttpWebRequest;

                if ((object)request == null)
                    throw new InvalidOperationException("Failed to create an HTTP request from " + m_requestUri);

                request.Method = "POST";
                request.KeepAlive = true;
                request.SendChunked = true;
                request.AllowWriteStreamBuffering = true;
                request.ContentType = "application/json";

                // Build a JSON post expression with measurement values to use as post data
                StringBuilder jsonData = new StringBuilder();

                foreach (IMeasurement measurement in measurements)
                {
                    if (jsonData.Length > 0)
                        jsonData.Append(',');
                    else
                        jsonData.Append('[');

                    jsonData.AppendFormat(PostFormat, measurement.Key, GetEpochMilliseconds(measurement.Timestamp), measurement.AdjustedValue, measurement.StateFlags);
                }

                jsonData.Append(']');

                // Encode JSON data as UTF8
                byte[] postData = Encoding.UTF8.GetBytes(jsonData.ToString());

                // Write data to request stream
                request.ContentLength = postData.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(postData, 0, postData.Length);
                }

                // Post data
                WebResponse response = request.GetResponse();
                response.Dispose();

                Interlocked.Add(ref m_totalValues, measurements.Length);
                Interlocked.Increment(ref m_totalPosts);
            }
            catch (Exception ex)
            {
                if (RequeueOnException)
                    InternalProcessQueue.InsertRange(0, measurements);

                bool ignoreError = false;

                // Under load, 400 and 500 status code errors seem to be occasionally reported - we ignore these
                WebException webex = ex as WebException;

                if ((object)webex != null)
                {
                    HttpWebResponse webResponse = webex.Response as HttpWebResponse;

                    if ((object)webResponse != null)
                    {
                        HttpStatusCode statusCode = webResponse.StatusCode;
                        ignoreError = (statusCode == HttpStatusCode.BadRequest || statusCode == HttpStatusCode.InternalServerError);
                    }
                }

                if (!ignoreError)
                {
                    OnProcessException(MessageLevel.Warning, ex);

                    // So long as user hasn't requested to stop, restart connection cycle when exceptions occur
                    m_requestRestart.RunOnceAsync();
                }
                else
                {
                    Interlocked.Increment(ref m_ignoredErrors);
                }
            }
        }

        private long GetEpochMilliseconds(Ticks timestamp)
        {
            return (long)(timestamp - UnixTimeTag.BaseTicks).ToMilliseconds();
        }

        #endregion
    }
}
