#region [ Code Modification History ]
/*
 *  07/03/2012 Denis Kholine
 *    Generated original version of source code
 */
#endregion

#region  [ UIUC NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois>
All rights reserved.

Developed by: <ITI>
<University of Illinois>
<http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
• Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/
#endregion

#region [ Code Using ]
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Transport;
using GSF.TestsSuite.TimeSeries.Cases;
using GSF;
using GSF.TimeSeries;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for DataPublisherTest and is intended
    ///to contain all DataPublisherTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataPublisherTest
    {
        #region [ Classes ]
        #endregion [ Classes ]

        #region [ Members ]
        private IDataPublisherCase datapublisher;
        private IMeasurementsCase m_IMeasurements;
        private DataPublisher target;
        #endregion

        #region [ Context ]
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        //
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            m_IMeasurements.Dispose();
            datapublisher.Dispose();
            target.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_IMeasurements = new IMeasurementsCase();
            datapublisher = new IDataPublisherCase();
            target = datapublisher.DataPublisher;
            target.InitializationTimeout = 1000;
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for AllowSynchronizedSubscription
        ///</summary>
        [TestMethod()]
        public void AllowSynchronizedSubscriptionTest()
        {
            bool expected = false;
            bool actual;
            target.AllowSynchronizedSubscription = expected;
            actual = target.AllowSynchronizedSubscription;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for CipherKeyRotationPeriod
        ///</summary>
        [TestMethod()]
        public void CipherKeyRotationPeriodTest()
        {
            double expected = 60000F;
            double actual;
            target.EncryptPayload = false;
            //target.CipherKeyRotationPeriod = expected;
            actual = target.CipherKeyRotationPeriod;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ClientConnections
        ///</summary>
        [TestMethod()]
        public void ClientConnectionsTest()
        {
            ConcurrentDictionary<Guid, ClientConnection> actual;
            actual = target.ClientConnections;
        }

        /// <summary>
        ///A test for DataPublisher Constructor
        ///</summary>
        [TestMethod()]
        public void DataPublisherConstructorTest()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(DataPublisher));
        }

        /// <summary>
        ///A test for DataPublisher Constructor
        ///</summary>
        [TestMethod()]
        public void DataPublisherConstructorTest1()
        {
            target = new DataPublisher();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(DataPublisher));
        }

        /// <summary>
        ///A test for EncryptPayload
        ///</summary>
        [TestMethod()]
        public void EncryptPayloadTest()
        {
            bool expected = false;
            bool actual;
            target.EncryptPayload = expected;
            actual = target.EncryptPayload;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetClientEncoding
        ///</summary>
        [TestMethod()]
        public void GetClientEncodingTest()
        {
            Guid clientID = new Guid();
            System.Text.Encoding expected = ASCIIEncoding.ASCII;
            expected = target.GetClientEncoding(clientID);
            Assert.IsNotNull(expected);
        }  

        /// <summary>
        ///A test for GetShortStatus
        ///</summary>
        [TestMethod()]
        public void GetShortStatusTest()
        {
            int maxLength = 0;
            string expected = string.Empty;
            string actual;
            actual = target.GetShortStatus(maxLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetSubscriberInfo
        ///</summary>
        [TestMethod()]
        public void GetSubscriberInfoTest()
        {
            int clientIndex = 0;
            string expected = string.Empty;
            string actual;
            actual = target.GetSubscriberInfo(clientIndex);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetSubscriberStatus
        ///</summary>
        [TestMethod()]
        public void GetSubscriberStatusTest()
        {
            Guid subscriberID = Guid.NewGuid();
            Tuple<Guid, bool, string> expected = new Tuple<Guid, bool, string>(subscriberID, false, "");
            Tuple<Guid, bool, string> actual;
            actual = target.GetSubscriberStatus(subscriberID);
            Assert.AreEqual(expected.Item1.ToString(), actual.Item1.ToString());
        }

        /// <summary>
        ///A test for Initialized
        ///</summary>
        [TestMethod()]
        public void InitializedTest()
        {
            bool expected = false;
            bool actual;
            target.Initialized = expected;
            actual = target.Initialized;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            try
            {
                target.Initialize();

                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for MetadataTables
        ///</summary>
        [TestMethod()]
        public void MetadataTablesTest()
        {
            string expected = string.Empty;
            string actual;
            target.MetadataTables = expected;
            actual = target.MetadataTables;
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            string expected = "Data Publisher Connection";
            string actual;
            target.Name = expected;
            actual = target.Name;
            Assert.AreEqual(expected.ToLower(), actual.ToLower());
        }

        /// <summary>
        ///A test for QueueMeasurementsForProcessing
        ///</summary>
        [TestMethod()]
        public void QueueMeasurementsForProcessingTest()
        {
            List<Measurement> items = new List<Measurement>();
            items.Add(new Measurement());
            IEnumerable<IMeasurement> measurements = items;
            try
            {
                target.QueueMeasurementsForProcessing(measurements);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for RequireAuthentication
        ///</summary>
        [TestMethod()]
        public void RequireAuthenticationTest()
        {
            bool expected = false;
            bool actual;
            target.RequireAuthentication = expected;
            actual = target.RequireAuthentication;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RotateCipherKeys
        ///</summary>
        [TestMethod()]
        public void RotateCipherKeysTest()
        {
            //TODO
            int clientIndex = 0;
            target.RotateCipherKeys(clientIndex);
            try
            {
                target.Start();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for SendClientResponse
        ///</summary>
        [TestMethod()]
        public void SendClientResponseTest1()
        {
            Guid clientID = new Guid();
            ServerResponse response = new ServerResponse();
            ServerCommand command = new ServerCommand();
            string status = string.Empty;
            bool expected = false;
            bool actual;
            actual = target.SendClientResponse(clientID, response, command, status);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SendClientResponse
        ///</summary>
        [TestMethod()]
        public void SendClientResponseTest2()
        {
            Guid clientID = new Guid();
            ServerResponse response = new ServerResponse();
            ServerCommand command = new ServerCommand();
            bool expected = false;
            bool actual;
            actual = target.SendClientResponse(clientID, response, command);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SendClientResponse
        ///</summary>
        [TestMethod()]
        public void SendClientResponseTest3()
        {
            Guid clientID = new Guid();
            ServerResponse response = new ServerResponse();
            ServerCommand command = new ServerCommand();
            byte[] data = new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
            bool expected = false;
            bool actual;
            actual = target.SendClientResponse(clientID, response, command, data);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SendClientResponse
        ///</summary>
        [TestMethod()]
        public void SendClientResponseTest4()
        {
            Guid clientID = new Guid();
            ServerResponse response = new ServerResponse();
            ServerCommand command = new ServerCommand();
            string formattedStatus = string.Empty;
            object[] args = new object[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
            bool expected = false;
            bool actual;
            actual = target.SendClientResponse(clientID, response, command, formattedStatus, args);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SendDataStartTime
        ///</summary>
        [TestMethod()]
        public void SendDataStartTimeTest()
        {
            Guid clientID = new Guid();
            Ticks startTime = new Ticks();
            bool expected = false;
            bool actual;
            actual = target.SendDataStartTime(clientID, startTime);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SharedDatabase
        ///</summary>
        [TestMethod()]
        public void SharedDatabaseTest()
        {
            bool expected = false;
            bool actual;
            target.SharedDatabase = expected;
            actual = target.SharedDatabase;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            try
            {
                //DOTO
                target.Start();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for Status
        ///</summary>
        [TestMethod()]
        public void StatusTest()
        {
            string actual;
            actual = target.Status;
        }

        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod()]
        public void StopTest()
        {
            try
            {
                target.Stop();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for UpdateSignalIndexCache
        ///</summary>
        [TestMethod()]
        public void UpdateSignalIndexCacheTest()
        {
            Guid clientID = new Guid();
            SignalIndexCache signalIndexCache = new SignalIndexCache();

            MeasurementKey[] inputMeasurementKeys = m_IMeasurements.MeasurementKeys;

            try
            {
                target.UpdateSignalIndexCache(clientID, signalIndexCache, inputMeasurementKeys);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for UseBaseTimeOffsets
        ///</summary>
        [TestMethod()]
        public void UseBaseTimeOffsetsTest()
        {
            bool expected = false;
            bool actual;
            target.UseBaseTimeOffsets = expected;
            actual = target.UseBaseTimeOffsets;
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}