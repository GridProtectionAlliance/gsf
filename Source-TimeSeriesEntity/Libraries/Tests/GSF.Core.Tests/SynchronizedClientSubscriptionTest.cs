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

#region [ Using ]
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries;
using GSF.TestsSuite.TimeSeries.Cases;
#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for SynchronizedClientSubscriptionTest and is intended
    ///to contain all SynchronizedClientSubscriptionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SynchronizedClientSubscriptionTest
    {
        #region [ Members ]
        private Guid clientID;
        private DataPublisher datapublisher;
        private IMeasurementsCase m_IMeasurements;
        private IClientSubscription secondarytarget;
        private Guid subscriberID;
        private SynchronizedClientSubscription target;
        private IWaitHandlesCase waitHandlesHelper;
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
        ///A test for ClientID
        ///</summary>
        [TestMethod()]
        public void ClientIDTest()
        {
            System.Guid actual;
            actual = target.ClientID;
        }

        /// <summary>
        ///A test for HostName
        ///</summary>
        [TestMethod()]
        public void HostNameTest()
        {
            string expected = "UnitTests";
            string actual;
            target.HostName = expected;
            actual = target.HostName;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for InputMeasurementKeys
        ///</summary>
        //[TestMethod()]
        //public void InputMeasurementKeysTest()
        //{
        //    MeasurementKey[] expected = m_IMeasurements.MeasurementKeys;
        //    MeasurementKey[] actual;
        //    target.InputMeasurementKeys = expected;
        //    actual = target.InputMeasurementKeys;
        //    Assert.AreEqual(expected, actual);
        //}

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            waitHandlesHelper.Dispose();
            m_IMeasurements.Dispose();
            datapublisher.Dispose();
            target.Dispose();
            secondarytarget.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            clientID = Guid.NewGuid();
            subscriberID = Guid.NewGuid();

            waitHandlesHelper = new IWaitHandlesCase();
            m_IMeasurements = new IMeasurementsCase();
            datapublisher = new DataPublisher(); //waitHandlesHelper.waitHandles);
            target = new SynchronizedClientSubscription(datapublisher, clientID, subscriberID);
            target.SetInitializedState(true);
            target.Enabled = true;
            secondarytarget = target;
        }

        #endregion

        /// <summary>
        ///A test for Transport.IClientSubscription.OnProcessException
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void OnProcessExceptionTest()
        {
            System.Exception ex = new System.Exception();
            secondarytarget.InitializationTimeout = 1;
            secondarytarget.OnProcessException(ex);
        }

        /// <summary>
        ///A test for Transport.IClientSubscription.OnProcessingCompleted
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void OnProcessingCompletedTest()
        {
            object sender = new object();
            System.EventArgs e = new System.EventArgs();
            secondarytarget.OnProcessingCompleted(sender, e);
        }

        /// <summary>
        ///A test for Transport.IClientSubscription.OnStatusMessage
        ///</summary>
        [TestMethod()]
        [DeploymentItem("TimeSeriesFramework.dll")]
        public void OnStatusMessageTest()
        {
            string status = string.Empty;
            secondarytarget.OnStatusMessage(status);
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
        ///A test for QueueMeasurementsForProcessing
        ///</summary>
        //[TestMethod()]
        //public void QueueMeasurementsForProcessingTest()
        //{
        //    List<Measurement> items = new List<Measurement>();
        //    items.Add(new Measurement<double>(m_guid, m_ticks,m_flags, m_value));
        //    IEnumerable<IMeasurement> measurements = items;
        //    target.QueueMeasurementsForProcessing(measurements);
        //}

        /// <summary>
        ///A test for SignalIndexCache
        ///</summary>
        [TestMethod()]
        public void SignalIndexCacheTest()
        {
            SignalIndexCache actual;
            actual = target.SignalIndexCache;
        }

        /// <summary>
        ///A test for Status
        ///</summary>
        [TestMethod()]
        public void StatusTest()
        {
            string actual;
            actual = target.Status;
            bool expected = (actual.Length > 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for SubscriberID
        ///</summary>
        [TestMethod()]
        public void SubscriberIDTest()
        {
            System.Guid actual;
            actual = target.SubscriberID;
            Assert.AreEqual(actual, subscriberID);
        }

        /// <summary>
        ///A test for SynchronizedClientSubscription Constructor
        ///</summary>
        [TestMethod()]
        public void SynchronizedClientSubscriptionConstructorTest()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(SynchronizedClientSubscription));
        }

        /// <summary>
        ///A test for TimestampSize
        ///</summary>
        [TestMethod()]
        public void TimestampSizeTest()
        {
            int actual;
            actual = target.TimestampSize;
            bool expected = (actual > 0);
            Assert.IsTrue(expected);
        }

        /// <summary>
        ///A test for UseCompactMeasurementFormat
        ///</summary>
        [TestMethod()]
        public void UseCompactMeasurementFormatTest()
        {
            bool expected = false;
            bool actual;
            target.UseCompactMeasurementFormat = expected;
            actual = target.UseCompactMeasurementFormat;
            Assert.AreEqual(expected, actual);
        }
    }
}