#region [ Modification History ]
/*
 * 12/10/2012 Denis Kholine
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
using GSF.TestsSuite.PhasorProtocols.Wrappers.IEEE.C37118;
using GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118;
using System.Runtime.Serialization;
using GSF.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace GSF.PhasorProtocols.Tests
{

    /// <summary>
    ///This is a test class for ConfigurationCellTest and is intended
    ///to contain all ConfigurationCellTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ConfigurationCellTest
    {
        #region [ Members ]
        private ConfigurationCellCase m_ConfigurationCellCase;
        private ConfigurationCell target;
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
            m_ConfigurationCellCase = new ConfigurationCellCase();
            target = m_ConfigurationCellCase.ConfigurationCell;
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        //
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for ConfigurationCell Constructor
        ///</summary>
        [TestMethod()]
        public void ConfigurationCellConstructorTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            Assert.IsInstanceOfType(target, typeof(ConfigurationCell));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for ConfigurationCell Constructor
        ///</summary>
        [TestMethod()]
        public void ConfigurationCellConstructorTest1()
        {
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationFrame1 parent = m_ConfigurationCellCase.ConfigurationFrame1;
            ushort idCode = 0;
            GSF.PhasorProtocols.LineFrequency nominalFrequency = new GSF.PhasorProtocols.LineFrequency();
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent, idCode, nominalFrequency);
            Assert.IsInstanceOfType(target, typeof(ConfigurationCell));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CreateNewCell
        ///</summary>o
        [TestMethod()]
        public void CreateNewCellTest()
        {
            GSF.PhasorProtocols.IChannelFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IChannelFrameParsingState<IConfigurationCell> state = null;
            int index = 0;
            //byte[] buffer = new byte[494];
            int startIndex = 0;
            int parsedLength = 330;
            int parsedLengthExpected = 330;
            GSF.PhasorProtocols.IConfigurationCell expected = m_ConfigurationCellCase.ConfigurationCell;
            GSF.PhasorProtocols.IConfigurationCell actual;
            actual = GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell.CreateNewCell(parent, state, index, target.BinaryImage, startIndex, out parsedLength);
            Assert.AreEqual(parsedLengthExpected, parsedLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetObjectData
        ///</summary>
        [TestMethod()]
        public void GetObjectDataTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            FormatterConverter converter = new FormatterConverter();
            System.Runtime.Serialization.SerializationInfo info = new SerializationInfo(parent.GetType(), (IFormatterConverter)converter);
            System.Runtime.Serialization.StreamingContext context = new System.Runtime.Serialization.StreamingContext();

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
        ///A test for AnalogDataFormat
        ///</summary>
        [TestMethod()]
        public void AnalogDataFormatTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            GSF.PhasorProtocols.DataFormat expected = new GSF.PhasorProtocols.DataFormat();
            GSF.PhasorProtocols.DataFormat actual;
            target.AnalogDataFormat = expected;
            actual = target.AnalogDataFormat;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Attributes
        ///</summary>
        [TestMethod()]
        public void AttributesTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            System.Collections.Generic.Dictionary<string, string> actual;
            actual = target.Attributes;
        }

        /// <summary>
        ///A test for FormatFlags
        ///</summary>
        [TestMethod()]
        public void FormatFlagsTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            GSF.PhasorProtocols.IEEEC37_118.FormatFlags expected = new GSF.PhasorProtocols.IEEEC37_118.FormatFlags();
            GSF.PhasorProtocols.IEEEC37_118.FormatFlags actual;
            target.FormatFlags = expected;
            actual = target.FormatFlags;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FrequencyDataFormat
        ///</summary>
        [TestMethod()]
        public void FrequencyDataFormatTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            GSF.PhasorProtocols.DataFormat expected = new GSF.PhasorProtocols.DataFormat();
            GSF.PhasorProtocols.DataFormat actual;
            target.FrequencyDataFormat = expected;
            actual = target.FrequencyDataFormat;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Parent
        ///</summary>
        [TestMethod()]
        public void ParentTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationFrame1 expected = null;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationFrame1 actual;
            target.Parent = expected;
            actual = target.Parent;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PhasorCoordinateFormat
        ///</summary>
        [TestMethod()]
        public void PhasorCoordinateFormatTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            GSF.PhasorProtocols.CoordinateFormat expected = new GSF.PhasorProtocols.CoordinateFormat();
            GSF.PhasorProtocols.CoordinateFormat actual;
            target.PhasorCoordinateFormat = expected;
            actual = target.PhasorCoordinateFormat;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PhasorDataFormat
        ///</summary>
        [TestMethod()]
        public void PhasorDataFormatTest()
        {
            GSF.PhasorProtocols.IConfigurationFrame parent = m_ConfigurationCellCase.ConfigurationFrame1;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell target = new GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell(parent);
            GSF.PhasorProtocols.DataFormat expected = new GSF.PhasorProtocols.DataFormat();
            GSF.PhasorProtocols.DataFormat actual;
            target.PhasorDataFormat = expected;
            actual = target.PhasorDataFormat;
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}
