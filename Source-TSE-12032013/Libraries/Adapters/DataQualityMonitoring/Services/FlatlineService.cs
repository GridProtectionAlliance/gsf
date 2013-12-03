//******************************************************************************************************
//  FlatlineService.cs - Gbtc
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
//  12/10/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/11/2009 - Pinal C. Patel
//       Added error checking to TryGetMeasurementInfo().
//  12/16/2009 - Stephen C. Wills
//       Replaced TryGetMeasurementInfo() with SerializableMeasurement.SetDeviceAndSignalType().
//  11/07/2010 - Pinal C. Patel
//       Modified to fix breaking changes made to SelfHostingService.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ServiceModel;
using GSF;
using GSF.ServiceModel;
using GSF.TimeSeries;

namespace DataQualityMonitoring.Services
{
    /// <summary>
    /// Represents a REST web service for flatlined measurements.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class FlatlineService : SelfHostingService, IFlatlineService
    {

        #region [ Members ]

        // Fields
        private FlatlineTest m_test;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatlineService"/> class.
        /// </summary>
        /// <param name="test">The test to be used by this <see cref="FlatlineService"/>.</param>
        public FlatlineService(FlatlineTest test)
        {
            m_test = test;
            PublishMetadata = true;
            PersistSettings = true;
            Endpoints = "http.rest://localhost:6100/flatlinetest";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="FlatlineTest"/> used by the web service for its data.
        /// </summary>
        public FlatlineTest Test
        {
            get
            {
                return m_test;
            }
            set
            {
                m_test = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all flatlined measurements from the <see cref="Test"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableFlatlineTest"/> object.</returns>
        public SerializableFlatlineTest ReadAllFlatlinedMeasurementsAsXml()
        {
            return ReadFlatlinedMeasurements();
        }

        /// <summary>
        /// Reads all flatlined measurements from the <see cref="Test"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableFlatlineTest"/> object.</returns>
        public SerializableFlatlineTest ReadAllFlatlinedMeasurementsAsJson()
        {
            return ReadFlatlinedMeasurements();
        }

        /// <summary>
        /// Reads all flatlined measurements from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for flatlined measurements.</param>
        /// <returns>A <see cref="SerializableFlatlineTest"/> object.</returns>
        public SerializableFlatlineTest ReadFlatlinedMeasurementsFromDeviceAsXml(string device)
        {
            return ReadFlatlinedMeasurements(device);
        }

        /// <summary>
        /// Reads all flatlined measurements from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for flatlined measurements.</param>
        /// <returns>A <see cref="SerializableFlatlineTest"/> object.</returns>
        public SerializableFlatlineTest ReadFlatlinedMeasurementsFromDeviceAsJson(string device)
        {
            return ReadFlatlinedMeasurements(device);
        }

        // Reads all flatlined measurements.
        private SerializableFlatlineTest ReadFlatlinedMeasurements()
        {
            SerializableFlatlineTest serializableTest = new SerializableFlatlineTest();
            ICollection<IMeasurement> flatlinedMeasurements = m_test.GetFlatlinedMeasurements();

            List<SerializableFlatlinedMeasurement> serializableFlatlinedMeasurements = new List<SerializableFlatlinedMeasurement>();
            foreach (IMeasurement measurement in flatlinedMeasurements)
            {
                SerializableFlatlinedMeasurement serializableFlatlinedMeasurement = CreateSerializableFlatlinedMeasurement(measurement);
                serializableFlatlinedMeasurements.Add(serializableFlatlinedMeasurement);
            }

            serializableTest.FlatlinedMeasurements = serializableFlatlinedMeasurements.ToArray();
            return serializableTest;
        }

        // Reads all flatlined measurements associated with a particular device.
        private SerializableFlatlineTest ReadFlatlinedMeasurements(string device)
        {
            SerializableFlatlineTest serializableTest = new SerializableFlatlineTest();
            ICollection<IMeasurement> flatlinedMeasurements = m_test.GetFlatlinedMeasurements();

            List<SerializableFlatlinedMeasurement> serializableFlatlinedMeasurements = new List<SerializableFlatlinedMeasurement>();
            foreach (IMeasurement measurement in flatlinedMeasurements)
            {
                SerializableFlatlinedMeasurement serializableFlatlinedMeasurement = CreateSerializableFlatlinedMeasurement(measurement);

                if (serializableFlatlinedMeasurement.Device == device)
                    serializableFlatlinedMeasurements.Add(serializableFlatlinedMeasurement);
            }

            serializableTest.FlatlinedMeasurements = serializableFlatlinedMeasurements.ToArray();
            return serializableTest;
        }

        // Properly creates a SerializableFlatlinedMeasurement by sending in TimeSinceLastChange, attaching to the exception event, and setting device and signal type.
        private SerializableFlatlinedMeasurement CreateSerializableFlatlinedMeasurement(IMeasurement measurement)
        {
            SerializableFlatlinedMeasurement serializableMeasurement = new SerializableFlatlinedMeasurement(measurement, m_test.RealTime - measurement.Timestamp);
            serializableMeasurement.ProcessException += serializableMeasurement_ProcessException;
            serializableMeasurement.SetDeviceAndSignalType(m_test.DataSource);
            return serializableMeasurement;
        }

        // Exceptions from flatlined measurements get forwarded to the ServiceProcessException event.
        private void serializableMeasurement_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnServiceProcessException(e.Argument);
        }

        #endregion
    }
}
