//******************************************************************************************************
//  OutOfRangeService.cs - Gbtc
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
//  12/16/2009 - Stephen C. Wills
//       Generated original version of source code.
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
    /// Represents a REST web service for out-of-range measurements.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class OutOfRangeService : SelfHostingService, IOutOfRangeService
    {
        #region [ Members ]

        // Fields
        private readonly Dictionary<string, RangeTest> m_tests;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfRangeService"/> class.
        /// </summary>
        public OutOfRangeService()
        {
            m_tests = new Dictionary<string, RangeTest>();
            PublishMetadata = true;
            PersistSettings = true;
            Endpoints = "http.rest://localhost:6101/rangetest";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the collection of <see cref="RangeTest"/>s that have registered with the service.
        /// </summary>
        public ICollection<RangeTest> Tests
        {
            get
            {
                return m_tests.Values;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads all out-of-range measurements from the <see cref="Tests"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadAllOutOfRangeMeasurementsAsXml()
        {
            return ReadOutOfRangeMeasurements();
        }

        /// <summary>
        /// Reads all out-of-range measurements from the <see cref="Tests"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format.
        /// </summary>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadAllOutOfRangeMeasurementsAsJson()
        {
            return ReadOutOfRangeMeasurements();
        }

        /// <summary>
        /// Reads all out-of-range measurements with the specified signal type and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="signalType">The signal type of the desired out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadOutOfRangeMeasurementsWithSignalTypeAsXml(string signalType)
        {
            return ReadOutOfRangeMeasurementsWithSignalType(signalType);
        }

        /// <summary>
        /// Reads all out-of-range measurements with the specified signal type and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="signalType">The signal type of the desired out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadOutOfRangeMeasurementsWithSignalTypeAsJson(string signalType)
        {
            return ReadOutOfRangeMeasurementsWithSignalType(signalType);
        }

        /// <summary>
        /// Reads all out-of-range measurements from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromDeviceAsXml(string device)
        {
            return ReadOutOfRangeMeasurementsFromDevice(device);
        }

        /// <summary>
        /// Reads all out-of-range measurements from the specified device and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="device">The name of the device to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromDeviceAsJson(string device)
        {
            return ReadOutOfRangeMeasurementsFromDevice(device);
        }

        /// <summary>
        /// Reads all out-of-range measurements from the specified <see cref="RangeTest"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Xml"/> format. 
        /// </summary>
        /// <param name="test">The name of the <see cref="RangeTest"/> to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromTestAsXml(string test)
        {
            return ReadOutOfRangeMeasurementsFromTest(test);
        }

        /// <summary>
        /// Reads all out-of-range measurements from the specified <see cref="RangeTest"/> and sends it in <see cref="System.ServiceModel.Web.WebMessageFormat.Json"/> format. 
        /// </summary>
        /// <param name="test">The name of the <see cref="RangeTest"/> to check for out-of-range measurements.</param>
        /// <returns>A <see cref="SerializableRangeTestCollection"/> object.</returns>
        public SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromTestAsJson(string test)
        {
            return ReadOutOfRangeMeasurementsFromTest(test);
        }

        /// <summary>
        /// Attaches a <see cref="RangeTest"/> to this <see cref="OutOfRangeService"/>.
        /// </summary>
        /// <param name="test">The <see cref="RangeTest"/> to be attached to this <see cref="OutOfRangeService"/>.</param>
        public void AttachRangeTest(RangeTest test)
        {
            m_tests[test.Name] = test;
        }

        /// <summary>
        /// Detaches a <see cref="RangeTest"/> from this <see cref="OutOfRangeService"/>.
        /// </summary>
        /// <param name="test">The <see cref="RangeTest"/> to be detached from this <see cref="OutOfRangeService"/>.</param>
        public void DetachRangeTest(RangeTest test)
        {
            m_tests.Remove(test.Name);
        }

        private SerializableRangeTestCollection ReadOutOfRangeMeasurements()
        {
            SerializableRangeTestCollection serializableCollection = new SerializableRangeTestCollection();
            List<SerializableRangeTest> serializableTests = new List<SerializableRangeTest>();

            // Convert RangeTests to SerializableRangeTests and add them to the list of serializable tests.
            foreach (RangeTest test in m_tests.Values)
            {
                SerializableRangeTest serializableTest = new SerializableRangeTest(test.Name);
                List<SerializableOutOfRangeMeasurement> serializableMeasurements = new List<SerializableOutOfRangeMeasurement>();
                ICollection<IMeasurement> outOfRangeMeasurements = test.GetOutOfRangeMeasurements();

                // Convert IMeasurements to SerializableOutOfRangeMeasurements and add them to the list of serializable measurements.
                foreach (IMeasurement measurement in outOfRangeMeasurements)
                {
                    SerializableOutOfRangeMeasurement serializableMeasurement = CreateSerializableOutOfRangeMeasurement(test, measurement);
                    serializableMeasurements.Add(serializableMeasurement);
                }

                serializableTest.OutOfRangeMeasurements = serializableMeasurements.ToArray();
                serializableTests.Add(serializableTest);
            }

            serializableCollection.RangeTests = serializableTests.ToArray();
            return serializableCollection;
        }

        private SerializableRangeTestCollection ReadOutOfRangeMeasurementsWithSignalType(string signalType)
        {
            SerializableRangeTestCollection serializableCollection = new SerializableRangeTestCollection();
            List<SerializableRangeTest> serializableTests = new List<SerializableRangeTest>();

            // Convert RangeTests to SerializableRangeTests and add them to the list of serializable tests.
            foreach (RangeTest test in m_tests.Values)
            {
                SerializableRangeTest serializableTest = new SerializableRangeTest(test.Name);
                List<SerializableOutOfRangeMeasurement> serializableMeasurements = new List<SerializableOutOfRangeMeasurement>();
                ICollection<IMeasurement> outOfRangeMeasurements = test.GetOutOfRangeMeasurements();

                // Convert IMeasurements to SerializableOutOfRangeMeasurements and add them to the list of serializable measurements.
                foreach (IMeasurement measurement in outOfRangeMeasurements)
                {
                    SerializableOutOfRangeMeasurement serializableMeasurement = CreateSerializableOutOfRangeMeasurement(test, measurement);

                    if (serializableMeasurement.SignalType == signalType)
                        serializableMeasurements.Add(serializableMeasurement);
                }

                serializableTest.OutOfRangeMeasurements = serializableMeasurements.ToArray();
                serializableTests.Add(serializableTest);
            }

            serializableCollection.RangeTests = serializableTests.ToArray();
            return serializableCollection;
        }

        private SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromDevice(string device)
        {
            SerializableRangeTestCollection serializableCollection = new SerializableRangeTestCollection();
            List<SerializableRangeTest> serializableTests = new List<SerializableRangeTest>();

            // Convert RangeTests to SerializableRangeTests and add them to the list of serializable tests.
            foreach (RangeTest test in m_tests.Values)
            {
                SerializableRangeTest serializableTest = new SerializableRangeTest(test.Name);
                List<SerializableOutOfRangeMeasurement> serializableMeasurements = new List<SerializableOutOfRangeMeasurement>();
                ICollection<IMeasurement> outOfRangeMeasurements = test.GetOutOfRangeMeasurements();

                // Convert IMeasurements to SerializableOutOfRangeMeasurements and add them to the list of serializable measurements.
                foreach (IMeasurement measurement in outOfRangeMeasurements)
                {
                    SerializableOutOfRangeMeasurement serializableMeasurement = CreateSerializableOutOfRangeMeasurement(test, measurement);

                    if(serializableMeasurement.Device == device)
                        serializableMeasurements.Add(serializableMeasurement);
                }

                serializableTest.OutOfRangeMeasurements = serializableMeasurements.ToArray();
                serializableTests.Add(serializableTest);
            }

            serializableCollection.RangeTests = serializableTests.ToArray();
            return serializableCollection;
        }

        private SerializableRangeTestCollection ReadOutOfRangeMeasurementsFromTest(string acronym)
        {
            SerializableRangeTestCollection serializableCollection = new SerializableRangeTestCollection();
            List<SerializableRangeTest> serializableTests = new List<SerializableRangeTest>();
            RangeTest test = m_tests[acronym];

            // Convert RangeTest to SerializableRangeTest and add it to the list of serializable tests.
            SerializableRangeTest serializableTest = new SerializableRangeTest(test.Name);
            List<SerializableOutOfRangeMeasurement> serializableMeasurements = new List<SerializableOutOfRangeMeasurement>();
            ICollection<IMeasurement> outOfRangeMeasurements = test.GetOutOfRangeMeasurements();

            // Convert IMeasurements to SerializableOutOfRangeMeasurements and add them to the list of serializable measurements.
            foreach (IMeasurement measurement in outOfRangeMeasurements)
            {
                SerializableOutOfRangeMeasurement serializableMeasurement = CreateSerializableOutOfRangeMeasurement(test, measurement);
                serializableMeasurements.Add(serializableMeasurement);
            }

            serializableTest.OutOfRangeMeasurements = serializableMeasurements.ToArray();
            serializableTests.Add(serializableTest);

            serializableCollection.RangeTests = serializableTests.ToArray();
            return serializableCollection;
        }

        private SerializableOutOfRangeMeasurement CreateSerializableOutOfRangeMeasurement(RangeTest test, IMeasurement measurement)
        {
            SerializableOutOfRangeMeasurement serializableMeasurement = new SerializableOutOfRangeMeasurement(measurement, test.LowRange, test.HighRange);
            serializableMeasurement.ProcessException += serializableMeasurement_ProcessException;
            serializableMeasurement.SetDeviceAndSignalType(test.DataSource);
            return serializableMeasurement;
        }

        // Exceptions from out-of-range measurements get forwarded to the ServiceProcessException event.
        private void serializableMeasurement_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnServiceProcessException(e.Argument);
        }

        #endregion
    }
}
