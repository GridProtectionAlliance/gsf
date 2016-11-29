//******************************************************************************************************
//  ArchivistOutputAdapter.cs - Gbtc
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
//  10/31/2012 - J. Adam Crain
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Automatak.Archivist.Client;
using Automatak.Archivist.Client.Impl;
using Automatak.Archivist.Protocol;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Measurement = Automatak.Archivist.Protocol.Measurement;
using Type = Automatak.Archivist.Protocol.Type;

namespace ArchivistAdapters
{
    /// <summary>
    /// Represents an output adapter that writes measurements to a CSV file.
    /// </summary>
    [Description("Archivist: Archives measurements to a an archivist/openArchiveMediator instance")]
    public class ArchivistOutputAdapter : OutputAdapterBase
    {

        #region [ Members ]

        // Fields
        private string m_host;
        private ushort m_port;
        private readonly int m_measurementCount;
        private IArchivistClient m_client;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchivistOutputAdapter"/> class.
        /// </summary>
        public ArchivistOutputAdapter()
        {
            m_host = "127.0.0.1";
            m_port = 20000;
            m_measurementCount = 0;
            m_client = null;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the host address
        /// </summary>
        [ConnectionStringParameter,
        Description("THe host address to which the adapter will attempt to connect"),
        DefaultValue("127.0.0.1")]
        public string Host
        {
            get
            {
                return m_host;
            }
            set
            {
                m_host = value;
            }
        }

        /// <summary>
        /// Gets or sets the host address
        /// </summary>
        [ConnectionStringParameter,
        Description("The port to which the adapter will attempt to connect"),
        DefaultValue(20000)]
        public ushort Port
        {
            get
            {
                return m_port;
            }
            set
            {
                m_port = value;
            }
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this
        /// <see cref="ArchivistOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive => true;

        /// <summary>
        /// Gets a flag that determines if this <see cref="ArchivistOutputAdapter"/>
        /// uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Returns the detailed status of this <see cref="ArchivistOutputAdapter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendFormat("                 Host: {0}", m_host);
                status.AppendFormat("                 Port: {0}", m_port);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes this <see cref="ArchivistOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters

            if (settings.TryGetValue("host", out setting))
                m_host = setting;

            if (settings.TryGetValue("port", out setting))
                m_port = ushort.Parse(setting);

            PoolingMessageClient msgClient = new PoolingMessageClient(m_host, m_port);
            m_client = new DefaultArchivistClient(msgClient);
        }

        /// <summary>
        /// Attempts to connect to this <see cref="ArchivistOutputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {

        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="ArchivistOutputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_client.Shutdown();
        }

        /// <summary>
        /// Archives <paramref name="measurements"/> to remote archivist instance.
        /// </summary>
        /// <param name="measurements">Measurements to be archived.</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements != null)
            {
                try
                {
                    ICollection<MeasurementWithId> meas = Convert(measurements);
                    m_client.Insert(meas).Await().Get();
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, ex);
                }
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="ArchivistOutputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"Archived {m_measurementCount} measurements to Archive.".CenterText(maxLength);
        }

        #endregion

        #region  [ Static ]

        private static ICollection<MeasurementWithId> Convert(IMeasurement[] m)
        {
            List<MeasurementWithId> list = new List<MeasurementWithId>();

            for (int i = 0; i < m.Length; ++i)
                list.Add(Convert(m[i]));

            return list;
        }

        private static MeasurementWithId Convert(IMeasurement m)
        {
            Measurement.Builder builder = Measurement.CreateBuilder();

            builder.SetTime(m.Timestamp);
            builder.SetType(Type.FLOAT64);
            builder.SetDoubleValue(m.Value);
            builder.SetQuality(0); // TODO - convert quality types

            return MeasurementWithId.CreateBuilder().SetId(m.TagName).SetMeas(builder.Build()).Build();
        }

        #endregion
    }
}
