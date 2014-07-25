//******************************************************************************************************
//  MongoOutputAdapter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/05/2010 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using MongoDB;

namespace MongoAdapters
{
    /// <summary>
    /// Represents an output adapter that archives measurements to a MongoDB database.
    /// </summary>
    [Description("MongoDB: Archives measurements to a MongoDB database")]
    public class MongoOutputAdapter : OutputAdapterBase
    {

        #region [ Members ]

        // Fields

        private string m_databaseName;
        private string m_collectionName;
        private string m_server;
        private int m_port;

        private Mongo m_mongo;
        private IMongoDatabase m_measurementDatabase;
        private IMongoCollection<MeasurementWrapper> m_measurementCollection;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MongoOutputAdapter"/> class.
        /// </summary>
        public MongoOutputAdapter()
        {
            m_databaseName = "TSFData";
            m_collectionName = "measurements";
            m_server = "localhost";
            m_port = 27017;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the MongoDB database.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the MongoDB database."),
        DefaultValue("TSFData")]
        public string DatabaseName
        {
            get
            {
                return m_databaseName;
            }
            set
            {
                if (Enabled)
                {
                    string errorMessage = "Cannot modify database name while MongoOutputAdapter is running.";
                    throw new InvalidOperationException(errorMessage);
                }

                m_databaseName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the measurement collection.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the collection of measurements."),
        DefaultValue("measurements")]
        public string CollectionName
        {
            get
            {
                return m_collectionName;
            }
            set
            {
                if (Enabled)
                {
                    string errorMessage = "Cannot modify collection name while MongoOutputAdapter is running.";
                    throw new InvalidOperationException(errorMessage);
                }

                m_collectionName = value;
            }
        }

        /// <summary>
        /// Gets or sets the server on which the MongoDB daemon is running.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the IP or host name of the MongoDB server."),
        DefaultValue("localhost")]
        public string Server
        {
            get
            {
                return m_server;
            }
            set
            {
                if (Enabled)
                {
                    string errorMessage = "Cannot modify server while MongoInputAdapter is running.";
                    throw new InvalidOperationException(errorMessage);
                }

                m_server = value;
            }
        }

        /// <summary>
        /// Gets or sets the port on which the MongoDB daemon is listening.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the port on which the MongoDB server is listening."),
        DefaultValue(27017)]
        public int Port
        {
            get
            {
                return m_port;
            }
            set
            {
                if (Enabled)
                {
                    string errorMessage = "Cannot modify the port while MongoInputAdapter is running.";
                    throw new InvalidOperationException(errorMessage);
                }

                m_port = value;
            }
        }

        /// <summary>
        /// Returns true; the measurements are archived by this adapter into a MongoDB database.
        /// </summary>
        public override bool OutputIsForArchive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns false; this adapter connects to MongoDB synchronously.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="MongoOutputAdapter"/> using settings from the connection string.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Optional settings

            if (settings.TryGetValue("databaseName", out setting))
                m_databaseName = setting;

            if (settings.TryGetValue("collectionName", out setting))
                m_collectionName = setting;

            if (settings.TryGetValue("server", out setting))
                m_server = setting;

            if (settings.TryGetValue("port", out setting))
                m_port = int.Parse(setting);
        }

        /// <summary>
        /// Attempts to connect to MongoDB.
        /// </summary>
        protected override void AttemptConnection()
        {
            string connectionString = string.Format("server={0}:{1}", m_server, m_port);
            m_mongo = new Mongo(connectionString);
            m_mongo.Connect();

            m_measurementDatabase = m_mongo.GetDatabase(m_databaseName);
            m_measurementCollection = m_measurementDatabase.GetCollection<MeasurementWrapper>(m_collectionName);
        }

        /// <summary>
        /// Attempts to disconnect from MongoDB.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_mongo.Disconnect();
        }

        /// <summary>
        /// Stores a collection of measurements in the MongoDB database.
        /// </summary>
        /// <param name="measurements">The measurements to be stored in the MongoDB database.</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements != null)
            {
                foreach (IMeasurement measurement in measurements)
                {
                    MeasurementWrapper wrapper = new MeasurementWrapper(measurement);
                    m_measurementCollection.Insert(wrapper);
                }
            }
        }

        /// <summary>
        /// Returns a short message describing the current status of the adapter.
        /// </summary>
        /// <param name="maxLength">The maximum length of the status message.</param>
        /// <returns>The short status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Measurements archived: {0}", ProcessedMeasurements);
        }

        #endregion
    }
}
