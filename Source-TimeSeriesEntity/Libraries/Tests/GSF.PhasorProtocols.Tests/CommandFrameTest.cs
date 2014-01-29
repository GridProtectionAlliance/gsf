#region [ Modification History ]
/*
 * 12/04/2012 Denis Kholine
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
using System.Linq;
using System.Text;
using GSF.TestsSuite.PhasorProtocols.Wrappers;
using GSF.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118;
using System.Runtime.Serialization;
#endregion



namespace GSF.PhasorProtocols.Tests 
{
    /// <summary>
    ///This is a test class for CommandFrameTest and is intended
    ///to contain all CommandFrameTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CommandFrameTest
    {
        #region [ Members ]
        private ConfigurationCellCase m_ConfigurationCellCase;
        private CommandFrame target;
        private CommandFrameCase m_CommandFrameCase;
        private SerializationInfo info;
        private StreamingContext context;
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

        #region [ Additional test attributes ]
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
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            context = new StreamingContext();
            m_ConfigurationCellCase = new ConfigurationCellCase();
            IFormatterConverter converter = new FormatterConverter();
            info = new SerializationInfo(typeof(CommandFrame), converter);
            m_CommandFrameCase = new CommandFrameCase();
            target = m_CommandFrameCase.CommandFrame;
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for CommandFrame Constructor
        ///</summary>
        [TestMethod()]
        public void CommandFrameConstructorTest()
        {
            byte[] buffer = new byte[m_CommandFrameCase.buffer.Length];
            buffer = m_CommandFrameCase.buffer;
            int startIndex = m_CommandFrameCase.startIndex;
            int length = m_CommandFrameCase.length;
            CommandFrame target = new CommandFrame(buffer, startIndex, length);
            Assert.IsInstanceOfType(target, typeof(CommandFrame));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CommandFrame Constructor
        ///</summary>
        [TestMethod()]
        public void CommandFrameConstructorTest1()
        {
            ushort idCode = 0;
            DeviceCommand command = new DeviceCommand();
            byte version = 0;
            CommandFrame target = new CommandFrame(idCode, command, version);
            Assert.IsInstanceOfType(target, typeof(CommandFrame));
            Assert.IsNotNull(target);

        }

        /// <summary>
        ///A test for GetObjectData
        ///</summary>
        [TestMethod()]
        public void GetObjectDataTest()
        {
            try
            {
                target.GetObjectData(info, context);
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }

        /// <summary>
        ///A test for Attributes
        ///</summary>
        [TestMethod()]
        public void AttributesTest()
        {
            //TODO: Fill in dictionary with data
            System.Collections.Generic.Dictionary<string, string> actual;
            actual = target.Attributes;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for CommonHeader
        ///</summary>
        [TestMethod()]
        public void CommonHeaderTest()
        {
            CommonFrameHeader expected = m_ConfigurationCellCase.CommonFrameHeader;
            CommonFrameHeader actual;
            target.CommonHeader = expected;
            actual = target.CommonHeader;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Timestamp
        ///</summary>
        [TestMethod()]
        public void TimestampTest()
        {
            GSF.Ticks expected = new GSF.Ticks();
            GSF.Ticks actual;
            target.Timestamp = expected;
            actual = target.Timestamp;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Version
        ///</summary>
        [TestMethod()]
        public void VersionTest()
        {
            byte expected = 0;
            byte actual;
            target.Version = expected;
            actual = target.Version;
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}
