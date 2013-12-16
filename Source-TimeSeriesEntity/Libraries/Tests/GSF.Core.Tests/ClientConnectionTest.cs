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
using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.Communication;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
#endregion

namespace TimeSeriesFramework.UnitTests
{
    /// <summary>
    ///This is a test class for ClientConnectionTest and is intended
    ///to contain all ClientConnectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ClientConnectionTest
    {
        #region [ Classes ]
        /// <summary>
        /// Wait Handles Helper
        /// </summary>
        private class WaitHandlesHelper : IDisposable
        {
            #region [ Members ]
            private AutoResetEvent m_ARE;
            private string m_Key;
            private ConcurrentDictionary<string, AutoResetEvent> m_waitHandles;
            #endregion

            #region [ Properties ]
            public AutoResetEvent AutoResetEvent
            {
                get
                {
                    return m_ARE;
                }
            }

            public string Key
            {
                get
                {
                    return m_Key;
                }
            }

            public ConcurrentDictionary<string, AutoResetEvent> waitHandles
            {
                get
                {
                    return m_waitHandles;
                }
            }

            #endregion

            #region [ Constructors ]
            public WaitHandlesHelper()
            {
                m_Key = Guid.NewGuid().ToString();
                m_waitHandles = new ConcurrentDictionary<string, AutoResetEvent>();
                m_ARE = new AutoResetEvent(false);

                Func<string, AutoResetEvent> m_func_ARE = r =>
                {
                    return m_ARE;
                };

                Func<string, AutoResetEvent, AutoResetEvent> m_func_ARE_Updater = (in1, in2) =>
                {
                    m_ARE.Set();
                    return m_ARE;
                };

                m_waitHandles.AddOrUpdate(m_Key, m_func_ARE(m_Key),
                (string Key, AutoResetEvent m_ResetEvt) =>
                {
                    m_ARE.Set();
                    m_ARE.Dispose();
                    m_ARE = new AutoResetEvent(false);
                    return m_ARE;
                });
            }

            #endregion

            #region [ Dispose ]
            private bool isDisposed = false;

            ~WaitHandlesHelper()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool isDisposing)
            {
                if (!isDisposed)
                {
                    if (isDisposing)
                    {
                        m_waitHandles.Clear();
                    }
                    isDisposed = false;
                }
            }

            #endregion
        }

        #endregion [ Classes ]

        #region [ Members ]
        private WaitHandlesHelper waitHandlesHelper;

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
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            waitHandlesHelper.Dispose();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            waitHandlesHelper = new WaitHandlesHelper();
        }

        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for Authenticated
        ///</summary>
        [TestMethod()]
        public void AuthenticatedTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            bool expected = false;
            bool actual;
            target.Authenticated = expected;
            actual = target.Authenticated;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ClientConnection Constructor
        ///</summary>
        [TestMethod()]
        public void ClientConnectionConstructorTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(ClientConnection));
        }

        /// <summary>
        ///A test for ClientID
        ///</summary>
        [TestMethod()]
        public void ClientIDTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            System.Guid actual;
            actual = target.ClientID;
        }

        /// <summary>
        ///A test for ConnectionID
        ///</summary>
        [TestMethod()]
        public void ConnectionIDTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            string actual;
            actual = target.ConnectionID;
        }

        /// <summary>
        ///A test for DataChannel
        ///</summary>
        [TestMethod()]
        public void DataChannelTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            UdpServer expected = new UdpServer();
            UdpServer actual;
            target.DataChannel = expected;
            actual = target.DataChannel;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest1()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            target.Dispose();
        }

        /// <summary>
        ///A test for Encoding
        ///</summary>
        [TestMethod()]
        public void EncodingTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            System.Text.Encoding actual;
            actual = target.Encoding;
        }

        /// <summary>
        ///A test for IPAddress
        ///</summary>
        [TestMethod()]
        public void IPAddressTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            System.Net.IPAddress actual;
            actual = target.IPAddress;
        }

        /// <summary>
        ///A test for IsConnected
        ///</summary>
        [TestMethod()]
        public void IsConnectedTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            bool actual;
            actual = target.IsConnected;
        }

        /// <summary>
        ///A test for KeyIVs
        ///</summary>
        [TestMethod()]
        public void KeyIVsTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            byte[][][] actual;
            actual = target.KeyIVs;
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            string actual;
            actual = target.Name;
        }

        /// <summary>
        ///A test for OperationalModes
        ///</summary>
        [TestMethod()]
        public void OperationalModesTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            OperationalModes expected = new OperationalModes();
            OperationalModes actual;
            target.OperationalModes = expected;
            actual = target.OperationalModes;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PublishChannel
        ///</summary>
        [TestMethod()]
        public void PublishChannelTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            IServer actual;
            actual = target.PublishChannel;
        }

        /// <summary>
        ///A test for SharedSecret
        ///</summary>
        [TestMethod()]
        public void SharedSecretTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            string expected = string.Empty;
            string actual;
            target.SharedSecret = expected;
            actual = target.SharedSecret;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SubscriberAcronym
        ///</summary>
        [TestMethod()]
        public void SubscriberAcronymTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            string expected = string.Empty;
            string actual;
            target.SubscriberAcronym = expected;
            actual = target.SubscriberAcronym;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SubscriberID
        ///</summary>
        [TestMethod()]
        public void SubscriberIDTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            System.Guid expected = new System.Guid();
            System.Guid actual;
            target.SubscriberID = expected;
            actual = target.SubscriberID;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SubscriberName
        ///</summary>
        [TestMethod()]
        public void SubscriberNameTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            string expected = string.Empty;
            string actual;
            target.SubscriberName = expected;
            actual = target.SubscriberName;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for UpdateKeyIVs
        ///</summary>
        [TestMethod()]
        public void UpdateKeyIVsTest()
        {
            DataPublisher parent = new DataPublisher();
            System.Guid clientID = new System.Guid();
            TcpServer commandChannel = new TcpServer();
            ClientConnection target = new ClientConnection(parent, clientID, commandChannel);
            target.UpdateKeyIVs();
        }
        #endregion
    }
}