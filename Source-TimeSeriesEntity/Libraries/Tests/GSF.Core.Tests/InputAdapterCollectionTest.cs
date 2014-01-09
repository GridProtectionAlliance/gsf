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
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TimeSeries.Adapters;

#endregion

namespace GSF.Core.Tests
{
    /// <summary>
    ///This is a test class for InputAdapterCollectionTest and is intended
    ///to contain all InputAdapterCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InputAdapterCollectionTest
    {
        /// <summary>
        ///A test for InputAdapterCollection Constructor
        ///</summary>
        [TestMethod()]
        public void InputAdapterCollectionConstructorTest()
        {
            ConcurrentDictionary<string, AutoResetEvent> waitHandles = waitHandlesCase.waitHandles;
            InputAdapterCollection target = new InputAdapterCollection(false);//waitHandles);
            //target
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(InputAdapterCollection));
        }

        /// <summary>
        ///A test for InputAdapterCollection Constructor
        ///</summary>
        [TestMethod()]
        public void InputAdapterCollectionConstructorTest1()
        {
            InputAdapterCollection target = new InputAdapterCollection(false);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(InputAdapterCollection));
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            waitHandlesCase.Dispose();
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
        /// Initialize
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            waitHandlesCase = new WaitHandlesHelper();
        }

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

        #region [ Members ]
        private WaitHandlesHelper waitHandlesCase;

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
        #endregion
    }
}