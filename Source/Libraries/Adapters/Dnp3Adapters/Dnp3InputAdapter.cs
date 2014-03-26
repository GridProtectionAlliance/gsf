//******************************************************************************************************
//  Dnp3InputAdapter.cs - Gbtc
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
//  10/05/2012 - Adam Crain
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using DNP3.Adapter;
using DNP3.Interface;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DNP3Adapters
{
    /// <summary>
    /// DNP3 Adapter
    /// </summary>
    [Description("DNP3: Reads measurements from a remote dnp3 endpoint")]
    public class DNP3InputAdapter : InputAdapterBase
    {
        /// <summary>
        /// DNP3 manager shared across all of the DNP3 input adapters
        /// Concurrency level defaults to number of processors
        /// </summary>
        private static readonly IDNP3Manager m_Manager = DNP3.Adapter.DNP3ManagerFactory.CreateManager(Environment.ProcessorCount);

        /// <summary>
        /// The filename for the communication configuration file
        /// </summary>
        private String m_commsFilePath;

        /// <summary>
        /// The filename for the measurement mapping configuration file
        /// </summary>
        private String m_mappingFilePath;

        /// <summary>
        /// Configuration for the master. Set during the Initialize call.
        /// </summary>
        private MasterConfiguration m_MasterConfig;

        /// <summary>
        /// Configuration for the measurement map. Set during the Initialize call.
        /// </summary>
        private MeasurementMap m_MeasMap;

        /// <summary>
        /// Gets set during the AttemptConnection() call and using during AttemptDisconnect()
        /// </summary>
        private IChannel m_Channel = null;

        /// <summary>
        /// A counter of the number of measurements received
        /// </summary>
        private int m_numMeasurementsReceived;

        /// <summary>
        /// Flag that records wether or not the port/master have been added so that the resource can be cleaned
        /// up in the Dispose/AttemptDisconnect methods
        /// </summary>
        private bool m_active;

        /// <summary>
        /// Gets or sets the name of the xml file from which the communication parameters will be read
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the XML file from which the communication configuration will be read"),
        DefaultValue("comms1.xml")]
        public string CommsFilePath
        {
            get
            {
                return m_commsFilePath;
            }
            set
            {
                m_commsFilePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the xml file from which the measurement mapping is read
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the XML file from which the communication configuration will be read"),
        DefaultValue("device1.xml")]
        public string MappingFilePath
        {
            get
            {
                return m_mappingFilePath;
            }
            set
            {
                m_mappingFilePath = value;
            }
        }

       /// <summary>
       /// Initializes <see cref="DNP3InputAdapter"/>
       /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Settings.TryGetValue("CommsFilePath", out m_commsFilePath);
            Settings.TryGetValue("MappingFilePath", out m_mappingFilePath);                                  

            try
            {
                if (m_commsFilePath == null)
                {
                    throw new ArgumentException("The required commsFile parameter was not specified");
                }

                if (m_mappingFilePath == null)
                {
                    throw new ArgumentException("The required mappingFile parameter was not specified");
                }

                m_MasterConfig = ReadConfig<MasterConfiguration>(m_commsFilePath);
                m_MeasMap = ReadConfig<MeasurementMap>(m_mappingFilePath);                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                OnProcessException(ex);                
            }            
        }

        /// <summary>
        /// Disposes the <see cref="DNP3InputAdapter"/>
        /// </summary>
        /// <param name="disposing"><c>true</c> if disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (m_active)
            {                
                m_active = false;
                if (m_Channel != null)
                {
                    m_Channel.Shutdown();
                    m_Channel = null;
                }
            }
            base.Dispose(disposing);
        }

        private T ReadConfig<T>(string path)
        {
            var ser = new XmlSerializer(typeof(T));
            var stream = new StreamReader(path);
            try
            {
                return (T)ser.Deserialize(stream);
            }
            finally
            {
                stream.Close();
            }
        }       
        
        /// <summary>
        /// Async connection flag
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get { return false; }
        }

        /// <summary>
        /// Connection attempt method
        /// </summary>
        protected override void AttemptConnection()
        {            
            var tcp = m_MasterConfig.client;            
            var portName = tcp.address + ":" + tcp.port;
            var minRetry = TimeSpan.FromMilliseconds(tcp.minRetryMs);
            var maxRetry = TimeSpan.FromMilliseconds(tcp.maxRetryMs);
            var channel = m_Manager.AddTCPClient(portName, tcp.level, minRetry, maxRetry, tcp.address, tcp.port);
            m_Channel = channel;
            var soeHandler = new TimeSeriesSOEHandler(new MeasurementLookup(m_MeasMap));
            soeHandler.NewMeasurements += adapter_NewMeasurements;
            soeHandler.NewMeasurements += OnNewMeasurements;           
            var master = channel.AddMaster(portName, soeHandler, m_MasterConfig.master);
            m_active = true;
        }

        void adapter_NewMeasurements(ICollection<IMeasurement> measurements)
        {
            m_numMeasurementsReceived += measurements.Count;                      
        }        

        /// <summary>
        /// Disconnect attempt method
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_active)
            {
                if (m_Channel != null)
                {
                    m_Channel.Shutdown();
                    m_Channel = null;
                }
                m_active = false;
            }
        }

        /// <summary>
        /// Temporal support flag
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get { return false; }
        }

        /// <summary>
        /// Short status of adapter
        /// </summary>
        /// <param name="maxLength">Maximum length of status message</param>
        /// <returns>Short status of adapter</returns>
        public override string GetShortStatus(int maxLength)
        {
            return "The adapter has received " + m_numMeasurementsReceived + " measurements";
        }
    }    
    
}
