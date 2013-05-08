//******************************************************************************************************
//  TimestampService.cs - Gbtc
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
//  12/21/2009 - Stephen C. Wills
//       Generated original version of source code.
//  11/07/2010 - Pinal C. Patel
//       Modified to fix breaking changes made to SelfHostingService.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       MOdified Header.
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
    /// Represents a REST web service for measurements with bad timestamps.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TimestampService : SelfHostingService, ITimestampService
    {
        #region [ Members ]

        // Fields
        private TimestampTest m_test;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampService"/> class.
        /// </summary>
        /// <param name="test">The test to be used by this <see cref="TimestampService"/>.</param>
        public TimestampService(TimestampTest test)
        {
            m_test = test;
            PublishMetadata = true;
            PersistSettings = true;
            Endpoints = "http.rest://localhost:6102/timestamptest";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="TimestampTest"/> used by the web service for its data.
        /// </summary>
        public TimestampTest Test
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
        /// Reads all measurements with bad timestamps from the <see cref="Test"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        public SerializableTimestampTest ReadAllBadTimestampMeasurementsAsXml()
        {
            return ReadBadTimestampMeasurements();
        }

        /// <summary>
        /// Reads all measurements with bad timestamps from the <see cref="Test"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        public SerializableTimestampTest ReadAllBadTimestampMeasurementsAsJson()
        {
            return ReadBadTimestampMeasurements();
        }

        /// <summary>
        /// Reads all measurements with bad timestamps from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for flatlined measurements.</param>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        public SerializableTimestampTest ReadBadTimestampMeasurementsFromDeviceAsXml(string device)
        {
            return ReadBadTimestampMeasurements(device);
        }

        /// <summary>
        /// Reads all measurements with bad timestamps from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for flatlined measurements.</param>
        /// <returns>A <see cref="SerializableTimestampTest"/> object.</returns>
        public SerializableTimestampTest ReadBadTimestampMeasurementsFromDeviceAsJson(string device)
        {
            return ReadBadTimestampMeasurements(device);
        }

        private SerializableTimestampTest ReadBadTimestampMeasurements()
        {
            SerializableTimestampTest serializableTest = new SerializableTimestampTest();
            Dictionary<Ticks, LinkedList<IMeasurement>> badTimestampMeasurements = m_test.GetMeasurementsWithBadTimestamps();
            List<SerializableBadTimestampMeasurement> serializableBadTimestampMeasurements = new List<SerializableBadTimestampMeasurement>();

            foreach (Ticks arrivalTime in badTimestampMeasurements.Keys)
            {
                foreach (IMeasurement measurement in badTimestampMeasurements[arrivalTime])
                {
                    SerializableBadTimestampMeasurement serializableMeasurement = CreateSerializableBadTimestampMeasurement(measurement, arrivalTime);
                    serializableBadTimestampMeasurements.Add(serializableMeasurement);
                }
            }

            serializableTest.BadTimestampMeasurements = serializableBadTimestampMeasurements.ToArray();
            return serializableTest;
        }

        private SerializableTimestampTest ReadBadTimestampMeasurements(string device)
        {
            SerializableTimestampTest serializableTest = new SerializableTimestampTest();
            Dictionary<Ticks, LinkedList<IMeasurement>> badTimestampMeasurements = m_test.GetMeasurementsWithBadTimestamps();
            List<SerializableBadTimestampMeasurement> serializableBadTimestampMeasurements = new List<SerializableBadTimestampMeasurement>();

            foreach (Ticks arrivalTime in badTimestampMeasurements.Keys)
            {
                foreach (IMeasurement measurement in badTimestampMeasurements[arrivalTime])
                {
                    SerializableBadTimestampMeasurement serializableMeasurement = CreateSerializableBadTimestampMeasurement(measurement, arrivalTime);

                    if(device == serializableMeasurement.Device)
                        serializableBadTimestampMeasurements.Add(serializableMeasurement);
                }
            }

            serializableTest.BadTimestampMeasurements = serializableBadTimestampMeasurements.ToArray();
            return serializableTest;
        }

        private SerializableBadTimestampMeasurement CreateSerializableBadTimestampMeasurement(IMeasurement measurement, Ticks arrivalTime)
        {
            SerializableBadTimestampMeasurement serializableMeasurement = new SerializableBadTimestampMeasurement(measurement, arrivalTime);
            serializableMeasurement.ProcessException += serializableMeasurement_ProcessException;
            serializableMeasurement.SetDeviceAndSignalType(m_test.DataSource);
            return serializableMeasurement;
        }

        private void serializableMeasurement_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnServiceProcessException(e.Argument);
        }

        #endregion
    }
}
