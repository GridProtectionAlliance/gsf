//******************************************************************************************************
//  PIOutputAdapter.cs - Gbtc
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
//  08/13/2012 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using PISDK;
using PISDKCommon;

namespace PIAdapters
{
    /// <summary>
    /// Exports measurements to PI if the point tag or alternate tag corresponds to a PI point's tag name.
    /// </summary>
    [Description("PI : Archives measurements to a PI server using PI SDK")]
    public class PIOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields
        private string m_servername;                                         // server for PI connection
        private string m_userName;                                           // username for PI connection
        private string m_password;                                           // password for PI connection
        private int m_connectTimeout;                                        // PI connection timeout
        private PISDK.PISDK m_pi;                                            // PI SDK object
        private Server m_server;                                             // PI server object for archiving data
        private ConcurrentDictionary<MeasurementKey, PIPoint> m_tagKeyMap;   // cache the mapping between openPDC measurements and PI points
        private int m_processedMeasurements;                                 // track the processed measurements
        private object m_queuedMetadataRefreshPending;                       // sync object to prevent multiple metadata refreshes from occurring concurrently
        private AutoResetEvent m_metadataRefreshComplete;                    // Auto reset event to flag when metadata refresh has completed
        private bool m_runMetadataSync;                                      // whether or not to automatically create/update PI points on the server
        private string m_piPointSource;                                      // Point source to set on PI points when automatically created by the adapter
        private string m_piPointClass;                                       // Point class to use for new PI points when automatically created by the adapter

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PIOutputAdapter"/>
        /// </summary>
        public PIOutputAdapter()
        {
            m_connectTimeout = 30000;
            m_queuedMetadataRefreshPending = new object();
            m_metadataRefreshComplete = new AutoResetEvent(true);
            //m_pi = new PISDK.PISDK();
            m_tagKeyMap = new ConcurrentDictionary<MeasurementKey, PIPoint>();
            m_processedMeasurements = 0;
            m_runMetadataSync = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns true to indicate that this <see cref="PIOutputAdapter"/> is sending measurements to a historian, OSISoft PI.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns false to indicate that this <see cref="PIOutputAdapter"/> will connect synchronously
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI server for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI server for the adapter's PI connection.")]
        public string ServerName
        {
            get
            {
                return m_servername;
            }
            set
            {
                m_servername = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI user ID for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI user ID for the adapter's PI connection."), DefaultValue("")]
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
        /// Gets or sets the password used for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the password used for the adapter's PI connection."), DefaultValue("")]
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
        /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection
        /// </summary>
        [ConnectionStringParameter, Description("Defines the timeout interval (in milliseconds) for the adapter's connection"), DefaultValue(30000)]
        public int ConnectTimeout
        {
            get
            {
                return m_connectTimeout;
            }
            set
            {
                m_connectTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage metadata for PI points.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if this adapter should automatically manage metadata for PI points (recommended)."), DefaultValue(true)]
        public bool RunMetadataSync
        {
            get
            {
                return m_runMetadataSync;
            }
            set
            {
                m_runMetadataSync = value;
            }
        }

        /// <summary>
        /// Gets or sets the point source string used when automatically creating new PI points during the metadata update
        /// </summary>
        [ConnectionStringParameter, Description("Defines the point source string used when automatically creating new PI points during the metadata update"), DefaultValue("TSF")]
        public string PIPointSource
        {
            get
            {
                return m_piPointSource;
            }
            set
            {
                m_piPointSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic."), DefaultValue("classic")]
        public string PIPointClass
        {
            get
            {
                return m_piPointClass;
            }
            set
            {
                m_piPointClass = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes this <see cref="PIOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (!settings.TryGetValue("ServerName", out m_servername))
                throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format server=myservername to the connection string.");

            if (settings.TryGetValue("UserName", out setting))
                m_userName = setting;
            else
                m_userName = null;

            if (settings.TryGetValue("Password", out setting))
                m_password = setting;
            else
                m_password = null;

            if (settings.TryGetValue("ConnectTimeout", out setting))
                m_connectTimeout = Convert.ToInt32(setting);
            else
                m_connectTimeout = 30000;

            if (settings.TryGetValue("RunMetadataSync", out setting))
                m_runMetadataSync = Convert.ToBoolean(setting);
            else
                m_runMetadataSync = true; // By default, assume that PI tags will be automatically maintained

            if (settings.TryGetValue("PIPointSource", out setting))
                m_piPointSource = setting;
            else
                m_piPointSource = "TSF";

            if (settings.TryGetValue("PIPointClass", out setting))
                m_piPointClass = setting;
            else
                m_piPointClass = "classic";
        }

        /// <summary>
        /// Connects to the configured PI server.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_pi = new PISDK.PISDK();
            m_server = m_pi.Servers[m_servername];

            // PI server only allows independent connections to the same PI server from STA threads.
            // We're spinning up a thread here to connect STA, since our current thread is MTA.
            ManualResetEvent connectionEvent = new ManualResetEvent(false);
            Thread connectThread = new Thread(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(m_userName) && !string.IsNullOrEmpty(m_password))
                        m_server.Open(string.Format("UID={0};PWD={1}", m_userName, m_password));
                    else
                        m_server.Open();
                    connectionEvent.Set();
                }
                catch (Exception e)
                {
                    OnProcessException(e);
                }
            });
            connectThread.SetApartmentState(ApartmentState.STA);
            connectThread.Start();

            if (!connectionEvent.WaitOne(m_connectTimeout))
            {
                connectThread.Abort();
                throw new InvalidOperationException("Timeout occurred while connecting to configured PI server.");
            }
        }

        /// <summary>
        /// Closes this <see cref="PIOutputAdapter"/> connections to the PI server.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_server != null && m_server.Connected)
                m_server.Close();
        }

        /// <summary>
        /// Sorts measurements and sends them to the configured PI server in batches
        /// </summary>
        /// <param name="measurements">Measurements to queue</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            Dictionary<MeasurementKey, PIValues> values = new Dictionary<MeasurementKey, PIValues>();

            if (measurements != null)
            {
                foreach (IMeasurement measurement in measurements)
                {
                    if (!m_tagKeyMap.ContainsKey(measurement.Key))
                        MapKeysToPoints();

                    if (!values.ContainsKey(measurement.Key))
                    {
                        values.Add(measurement.Key, new PIValues());
                        values[measurement.Key].ReadOnly = false;
                    }

                    values[measurement.Key].Add(new DateTime(measurement.Timestamp).ToLocalTime(), measurement.AdjustedValue, null);
                    m_processedMeasurements++;
                }
            }

            foreach (MeasurementKey key in values.Keys)
            {
                try
                {
                    // If the key isn't in the dictionary, something has gone wrong finding this point in PI
                    if (m_tagKeyMap.ContainsKey(key))
                        m_tagKeyMap[key].Data.UpdateValues(values[key]);
                }
                catch (Exception e)
                {
                    OnProcessException(e);
                }
            }
        }

        /// <summary>
        /// Resets the PI tag to MeasurementKey mapping for loading data into PI by finding PI points that match either the openPDC pointtag or alternatetag
        /// </summary>
        public void MapKeysToPoints()
        {
            m_tagKeyMap.Clear();
            int mapped = 0;
            StringBuilder filterstring = new StringBuilder();

            if (InputMeasurementKeys != null)
            {
                foreach (MeasurementKey key in InputMeasurementKeys)
                {
                    DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("SIGNALID='{0}'", key.SignalID));
                    if (rows.Length > 0)
                    {
                        string tagname = rows[0]["POINTTAG"].ToString();
                        if (!String.IsNullOrWhiteSpace(rows[0]["ALTERNATETAG"].ToString()))
                            tagname = rows[0]["ALTERNATETAG"].ToString();

                        PointList points = null;

                        // Two ways to find points here
                        // 1. if we are running metadata sync from the adapter, look for the signal ID in the exdesc field
                        // 2. if the pi points are being manually maintained, look for either the point tag or alternate tag in the actual pi point tag
                        string filter = "";
                        if (!m_runMetadataSync)
                            filter = string.Format("TAG='{0}'", tagname);
                        else
                            filter = string.Format("EXDESC='{0}'", key.SignalID.ToString());

                        points = m_server.GetPoints(filter);

                        if (points == null || points.Count == 0)
                        {
                            OnStatusMessage("No PI points found with {0}. Data will not be archived for signal {1}.", new object[] { filter, rows[0]["SIGNALID"] });
                        }
                        else
                        {
                            if (points.Count > 1)
                                OnStatusMessage("Multiple PI points were found with tagname matching '{0}' or '{1}' for signal {2}. The first match will be used.", new object[] { rows[0]["POINTTAG"], rows[0]["ALTERNATETAG"], rows[0]["SIGNALID"] });

                            m_tagKeyMap.AddOrUpdate(key, points[1], (k, v) => points[1]); // NOTE - The PointList is NOT 0-based (index 1 is the first item in points)
                            mapped++;
                        }
                    }
                }
            }

            OnStatusMessage("Mapped {0} keys to points successfully.", new object[] { mapped });
        }

        /// <summary>
        /// Refreshes metadata using all available and enabled providers.
        /// </summary>
        [AdapterCommand("Sends updated metadata to PI")]
        public override void RefreshMetadata()
        {
            if (m_runMetadataSync)
            {
                ThreadPool.QueueUserWorkItem(QueueMetadataRefresh);
            }
        }

        private void QueueMetadataRefresh(object state)
        {
            try
            {
                // Queue up a metadata refresh unless another thread is already running
                if (Monitor.TryEnter(m_queuedMetadataRefreshPending))
                {
                    try
                    {
                        // Queue new metadata refresh after waiting for any prior refresh to complete
                        if (m_metadataRefreshComplete.WaitOne())
                            ThreadPool.QueueUserWorkItem(ExecuteMetadataRefresh);
                    }
                    finally
                    {
                        Monitor.Exit(m_queuedMetadataRefreshPending);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        public void ExecuteMetadataRefresh(object state)
        {
            OnStatusMessage("Beginning metadata refresh...");

            try
            {
                base.RefreshMetadata();

                if (InputMeasurementKeys != null && InputMeasurementKeys.Any())
                {
                    IPIPoints2 pts2 = (IPIPoints2)m_server.PIPoints;
                    //NamedValues edits = new NamedValues();
                    //PIErrors errors = new PIErrors();
                    PointList piPoints;
                    DataTable dtMeasurements = DataSource.Tables["ActiveMeasurements"];
                    DataRow[] rows;
                    foreach (MeasurementKey key in InputMeasurementKeys)
                    {
                        rows = dtMeasurements.Select(string.Format("SIGNALID='{0}'", key.SignalID));

                        if (rows.Any())
                        {
                            string tagname = rows[0]["POINTTAG"].ToString();
                            if (!String.IsNullOrWhiteSpace(rows[0]["ALTERNATETAG"].ToString()))
                                tagname = rows[0]["ALTERNATETAG"].ToString();

                            piPoints = m_server.GetPoints(string.Format("EXDESC='{0}'", rows[0]["SIGNALID"].ToString()));

                            if (piPoints.Count == 0)
                            {
                                m_server.PIPoints.Add(rows[0]["POINTTAG"].ToString(), m_piPointClass, PointTypeConstants.pttypFloat32);
                            }
                            else if (piPoints[1].Name != rows[0]["POINTTAG"].ToString())
                            {
                                pts2.Rename(piPoints[1].Name, rows[0]["POINTTAG"].ToString());
                            }
                            else
                            {
                                foreach (PIPoint pt in piPoints)
                                {
                                    if (pt.Name != rows[0]["POINTTAG"].ToString())
                                        pts2.Rename(pt.Name, rows[0]["POINTTAG"].ToString());
                                }
                            }

                            PIErrors errors = new PIErrors();
                            NamedValues edits = new NamedValues();
                            NamedValues edit = new NamedValues();
                            edit.Add("pointsource", m_piPointSource);
                            edit.Add("Descriptor", rows[0]["SIGNALREFERENCE"].ToString());
                            edit.Add("exdesc", rows[0]["SIGNALID"].ToString());
                            edit.Add("sourcetag", rows[0]["POINTTAG"].ToString());

                            if (dtMeasurements.Columns.Contains("ENGINEERINGUNITS")) // engineering units is a new field for this view -- handle the case that its not there
                                edit.Add("engunits", rows[0]["ENGINEERINGUNITS"].ToString());

                            edits.Add(rows[0]["POINTTAG"].ToString(), edit);
                            pts2.EditTags(edits, out errors, null);

                            if (errors.Count > 0)
                                OnStatusMessage(errors[0].Description);
                        }
                    }
                }
                else
                {
                    OnStatusMessage("PI Historian is not configured with any input measurements. Therefore, metadata sync will not do work.");
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
            finally
            {
                m_metadataRefreshComplete.Set();
            }

            OnStatusMessage("Completed metadata refresh successfully.");
        }

        /// <summary>
        /// Returns a brief status of this <see cref="PIOutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Archived {0} measurements to PI.", m_processedMeasurements).CenterText(maxLength);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            m_pi = null;
            m_server = null;

            if (m_tagKeyMap != null)
            {
                m_tagKeyMap.Clear();
                m_tagKeyMap = null;
            }
        }

        #endregion
    }
}
