#region [ Modification History ]
/*
 * 12/03/2012 Denis Kholine
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
using GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118;
using GSF.TestsSuite.PhasorProtocols.Wrappers.IEEE.C37118;
using GSF.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
#endregion

namespace GSF.PhasorProtocols.Tests
{


    /// <summary>
    ///This is a test class for DigitalDefinitionTest and is intended
    ///to contain all DigitalDefinitionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DigitalDefinitionTest
    {
        #region [ Members ]
        private ConfigurationCellCase m_ConfigurationCellCase;
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
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for DigitalDefinition Constructor
        ///</summary>
        [TestMethod()]
        public void DigitalDefinitionConstructorTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            Assert.IsInstanceOfType(target, typeof(GSF.PhasorProtocols.IEEEC37_118.DigitalDefinition));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for DigitalDefinition Constructor
        ///</summary>
        [TestMethod()]
        public void DigitalDefinitionConstructorTest1()
        {
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell parent = (ConfigurationCell)m_ConfigurationCellCase.ConfigurationCell;
            string label = string.Empty;
            ushort normalStatus = 0;
            ushort validInputs = 0;
            DigitalDefinition target = new DigitalDefinition(parent, label, normalStatus, validInputs);
            Assert.IsInstanceOfType(target, typeof(GSF.PhasorProtocols.IEEEC37_118.DigitalDefinition));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CreateNewDefinition
        ///</summary>
        [TestMethod()]
        public void CreateNewDefinitionTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            byte[] buffer = new byte[256];
            int startIndex = 0;
            int parsedLength = 5;
            int parsedLengthExpected = 256;
            GSF.PhasorProtocols.IDigitalDefinition expected = m_ConfigurationCellCase.DigitalDefinition;
            GSF.PhasorProtocols.IDigitalDefinition actual;
            actual = DigitalDefinition.CreateNewDefinition(parent, buffer, startIndex, out parsedLength);
            Assert.AreEqual(parsedLengthExpected, parsedLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetLabel
        ///</summary>
        [TestMethod()]
        public void GetLabelTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            int index = 5;
            string expected = string.Empty;
            string actual;
            actual = target.GetLabel(index);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for GetObjectData
        ///</summary>
        [TestMethod()]
        public void GetObjectDataTest()
        {
            FormatterConverter converter = new FormatterConverter();
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            SerializationInfo info = new SerializationInfo(typeof(DigitalDefinition), (IFormatterConverter)converter);
            StreamingContext context = new StreamingContext();
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
        ///A test for ParseConversionFactor
        ///</summary>
        [TestMethod()]
        public void ParseConversionFactorTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            byte[] buffer = new byte[16];
            int startIndex = 0;
            int expected = 4;
            int actual;
            actual = target.ParseConversionFactor(buffer, startIndex);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for SetLabel
        ///</summary>
        [TestMethod()]
        public void SetLabelTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            int index = 5;
            string value = "Label";
            target.SetLabel(index, value);
        }

        /// <summary>
        ///A test for Attributes
        ///</summary>
        [TestMethod()]
        public void AttributesTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            System.Collections.Generic.Dictionary<string, string> actual;
            actual = target.Attributes;
            Assert.IsNotNull(actual);

        }

        /// <summary>
        ///A test for ConversionFactorImage
        ///</summary>
        [TestMethod()]
        public void ConversionFactorImageTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            byte[] actual;
            actual = target.ConversionFactorImage;

        }

        /// <summary>
        ///A test for DraftRevision
        ///</summary>
        [TestMethod()]
        public void DraftRevisionTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            GSF.PhasorProtocols.IEEEC37_118.DraftRevision actual;
            actual = target.DraftRevision;

        }

        /// <summary>
        ///A test for Label
        ///</summary>
        [TestMethod()]
        public void LabelTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            string expected = "undefined";
            string actual;
            target.Label = expected;
            actual = target.Label;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for LabelCount
        ///</summary>
        [TestMethod()]
        public void LabelCountTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            int actual;
            actual = target.LabelCount;

        }

        /// <summary>
        ///A test for MaximumLabelLength
        ///</summary>
        [TestMethod()]
        public void MaximumLabelLengthTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            int actual;
            actual = target.MaximumLabelLength;

        }

        /// <summary>
        ///A test for NormalStatus
        ///</summary>
        [TestMethod()]
        public void NormalStatusTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            ushort expected = 0;
            ushort actual;
            target.NormalStatus = expected;
            actual = target.NormalStatus;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for Parent
        ///</summary>
        [TestMethod()]
        public void ParentTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell expected = m_ConfigurationCellCase.ConfigurationCell;
            GSF.PhasorProtocols.IEEEC37_118.ConfigurationCell actual;
            target.Parent = expected;
            actual = target.Parent;
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for ValidInputs
        ///</summary>
        [TestMethod()]
        public void ValidInputsTest()
        {
            IConfigurationCell parent = m_ConfigurationCellCase.ConfigurationCell;
            DigitalDefinition target = m_ConfigurationCellCase.DigitalDefinition;
            ushort expected = 0;
            ushort actual;
            target.ValidInputs = expected;
            actual = target.ValidInputs;
            Assert.AreEqual(expected, actual);

        }
        #endregion
    }
}
