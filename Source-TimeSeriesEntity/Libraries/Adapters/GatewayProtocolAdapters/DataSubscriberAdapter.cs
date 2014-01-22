//******************************************************************************************************
//  DataSubscriberAdapter.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  01/13/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using GSF;
using GSF.Communication;
using GSF.Configuration;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Transport;

namespace GatewayProtocolAdapters
{
    [Description("DataSubscriber: client that subscribes to a publishing server for a streaming data.")]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DataSubscriberAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Nested Types

        // Constants

        // Delegates

        // Events

        // Fields
        private DataSubscriber m_dataSubscriber;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public DataSubscriberAdapter()
        {
            m_dataSubscriber = new DataSubscriber();
        }

        #endregion

        #region [ Properties ]

        public override string ConnectionString
        {
            get
            {
                return base.ConnectionString;
            }
            set
            {
                base.ConnectionString = value;
                new ConnectionStringParser<ConnectionStringParameterAttribute, NestedConnectionStringParameterAttribute>().ParseConnectionString(value, SettingsObject);
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the status of this <see cref="DataSubscriber"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("         Subscription mode: {0}", m_synchronizedSubscription ? "Remotely Synchronized" : (object)m_localConcentrator == null ? "Unsynchronized" : "Locally Synchronized");
                status.AppendLine();
                status.AppendFormat("  Pending command requests: {0}", m_requests.Count);
                status.AppendLine();
                status.AppendFormat("             Authenticated: {0}", m_authenticated);
                status.AppendLine();
                status.AppendFormat("                Subscribed: {0}", m_subscribed);
                status.AppendLine();
                status.AppendFormat("      Data packet security: {0}", (object)m_keyIVs == null ? "Unencrypted" : "Encrypted");
                status.AppendLine();
                status.AppendFormat("      Data monitor enabled: {0}", (object)m_dataStreamMonitor != null && m_dataStreamMonitor.Enabled);
                status.AppendLine();

                if (DataLossInterval > 0.0D)
                    status.AppendFormat("No data reconnect interval: {0} seconds", DataLossInterval.ToString("0.000"));
                else
                    status.Append("No data reconnect interval: disabled");

                status.AppendLine();

                if ((object)m_dataChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Data Channel Status".CenterText(50));
                    status.AppendLine("-------------------".CenterText(50));
                    status.Append(m_dataChannel.Status);
                }

                if ((object)m_commandChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Command Channel Status".CenterText(50));
                    status.AppendLine("----------------------".CenterText(50));
                    status.Append(m_commandChannel.Status);
                }

                if ((object)m_localConcentrator != null)
                {
                    status.AppendLine();
                    status.AppendLine("Local Concentrator Status".CenterText(50));
                    status.AppendLine("-------------------------".CenterText(50));
                    status.Append(m_localConcentrator.Status);
                }

                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        public override string GetShortStatus(int maxLength)
        {
            throw new NotImplementedException();
        }

        protected override void AttemptConnection()
        {
            throw new NotImplementedException();
        }

        protected override void AttemptDisconnection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataSubscriberAdapter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.

                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        #endregion

        #region [ Operators ]

        #endregion

        #region [ Static ]

        // Static Fields

        // Static Constructor

        // Static Properties

        // Static Methods

        #endregion
    }
}
