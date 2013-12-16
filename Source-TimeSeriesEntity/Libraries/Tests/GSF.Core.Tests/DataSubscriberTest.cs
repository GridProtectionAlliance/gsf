#region [ Modification History ]
/*
 * 07/07/2012 Denis Kholine
 *  Generated Original version of source code.
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for DataSubscriberTest and is intended
    ///to contain all DataSubscriberTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataSubscriberTest
    {
        #region [ Members ]
        private DataSubscriber target;
        #endregion

        #region [ Properties ]
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

        #endregion

        #region
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

        /// <summary>
        ///A test for Authenticated
        ///</summary>
        [TestMethod()]
        public void AuthenticatedTest()
        {
            bool actual;
            actual = target.Authenticated;
        }

        /// <summary>
        ///A test for Authenticate
        ///</summary>
        [TestMethod()]
        public void AuthenticateTest()
        {
            string sharedSecret = string.Empty;
            string authenticationID = string.Empty;
            bool expected = false;
            bool actual;
            actual = target.Authenticate(sharedSecret, authenticationID);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for AutoConnect
        ///</summary>
        [TestMethod()]
        public void AutoConnectTest()
        {
            bool expected = false;
            bool actual;
            target.AutoConnect = expected;
            actual = target.AutoConnect;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for DataSubscriber Constructor
        ///</summary>
        [TestMethod()]
        public void DataSubscriberConstructorTest()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(DataSubscriber));
        }

        /// <summary>
        ///A test for DisposeLocalConcentrator
        ///</summary>
        [TestMethod()]
        public void DisposeLocalConcentratorTest()
        {
            try
            {
                target.DisposeLocalConcentrator();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
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
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            try
            {
                target.Initialize();
                target.SetInitializedState(true);
                Assert.IsTrue(target.Initialized);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            target.Stop();
            target.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new DataSubscriber();
            target.InitializationTimeout = 1;
            target.Start();

            target.ConnectionString = "Server=localhost:8888";
        }

        #endregion

        /// <summary>
        ///A test for OperationalEncoding
        ///</summary>
        [TestMethod()]
        public void OperationalEncodingTest()
        {
            OperationalEncoding expected = new OperationalEncoding();
            OperationalEncoding actual;
            target.OperationalEncoding = expected;
            actual = target.OperationalEncoding;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for OperationalModes
        ///</summary>
        [TestMethod()]
        public void OperationalModesTest()
        {
            OperationalModes expected = new OperationalModes();
            OperationalModes actual;
            target.OperationalModes = expected;
            actual = target.OperationalModes;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ProcessingInterval
        ///</summary>
        [TestMethod()]
        public void ProcessingIntervalTest()
        {
            int expected = 0;
            int actual;
            target.ProcessingInterval = expected;
            actual = target.ProcessingInterval;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RefreshMetadata
        ///</summary>
        [TestMethod()]
        public void RefreshMetadataTest()
        {
            target.RefreshMetadata();
            try
            {
                target.RefreshMetadata();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for RemotelySynchronizedSubscribe
        ///</summary>
        [TestMethod()]
        public void RemotelySynchronizedSubscribeTest()
        {
            bool compactFormat = false;
            int framesPerSecond = 0;
            double lagTime = 0F;
            double leadTime = 0F;
            string filterExpression = string.Empty;
            string dataChannel = string.Empty;
            bool useLocalClockAsRealTime = false;
            bool ignoreBadTimestamps = false;
            bool allowSortsByArrival = false;
            long timeResolution = 0;
            bool allowPreemptivePublishing = false;
            DownsamplingMethod downsamplingMethod = new DownsamplingMethod();
            string startTime = string.Empty;
            string stopTime = string.Empty;
            string constraintParameters = string.Empty;
            int processingInterval = 0;
            string waitHandleNames = string.Empty;
            int waitHandleTimeout = 0;
            bool expected = false;
            bool actual;
            actual = target.RemotelySynchronizedSubscribe(compactFormat, framesPerSecond, lagTime, leadTime, filterExpression, dataChannel, useLocalClockAsRealTime, ignoreBadTimestamps, allowSortsByArrival, timeResolution, allowPreemptivePublishing, downsamplingMethod, startTime, stopTime, constraintParameters, processingInterval, waitHandleNames, waitHandleTimeout);
            Assert.AreEqual(expected, actual);
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
        ///A test for SendServerCommand
        ///</summary>
        [TestMethod()]
        public void SendServerCommandTest()
        {
            ServerCommand commandCode = new ServerCommand();
            byte[] data = new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
            bool expected = false;
            bool actual;
            actual = target.SendServerCommand(commandCode, data);
            Assert.AreEqual(expected, actual);
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
        ///A test for Subscribe
        ///</summary>
        [TestMethod()]
        public void SubscribeTest()
        {
            bool remotelySynchronized = false;
            bool compactFormat = false;
            string connectionString = string.Empty;
            bool expected = false;
            bool actual;
            actual = target.Subscribe(remotelySynchronized, compactFormat, connectionString);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SupportsTemporalProcessing
        ///</summary>
        [TestMethod()]
        public void SupportsTemporalProcessingTest()
        {
            bool actual;
            actual = target.SupportsTemporalProcessing;
        }

        /// <summary>
        ///A test for TotalBytesReceived
        ///</summary>
        [TestMethod()]
        public void TotalBytesReceivedTest()
        {
            long actual;
            actual = target.TotalBytesReceived;
        }

        /// <summary>
        ///A test for Unsubscribe
        ///</summary>
        [TestMethod()]
        public void UnsubscribeTest()
        {
            bool expected = false;
            bool actual;
            actual = target.Unsubscribe();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UnsynchronizedSubscribe
        ///</summary>
        [TestMethod()]
        public void UnsynchronizedSubscribeTest()
        {
            bool compactFormat = false;
            bool throttled = false;
            string filterExpression = string.Empty;
            string dataChannel = string.Empty;
            bool includeTime = false;
            double lagTime = 0F;
            double leadTime = 0F;
            bool useLocalClockAsRealTime = false;
            string startTime = string.Empty;
            string stopTime = string.Empty;
            string constraintParameters = string.Empty;
            int processingInterval = 0;
            string waitHandleNames = string.Empty;
            int waitHandleTimeout = 0;
            bool expected = false;
            bool actual;
            actual = target.UnsynchronizedSubscribe(compactFormat, throttled, filterExpression, dataChannel, includeTime, lagTime, leadTime, useLocalClockAsRealTime, startTime, stopTime, constraintParameters, processingInterval, waitHandleNames, waitHandleTimeout);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UseMillisecondResolution
        ///</summary>
        [TestMethod()]
        public void UseMillisecondResolutionTest()
        {
            bool expected = false;
            bool actual;
            target.UseMillisecondResolution = expected;
            actual = target.UseMillisecondResolution;
            Assert.AreEqual(expected, actual);
        }
    }
}