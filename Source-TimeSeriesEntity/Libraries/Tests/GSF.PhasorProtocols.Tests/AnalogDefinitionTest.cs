#region [ Modification History ]
/*
 * 11/02/2012 Denis Kholine
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
using GSF.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using GSF.PhasorProtocols;
using GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118;
using GSF.TestsSuite.PhasorProtocols.Wrappers.IEEE.C37118;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace GSF.PhasorProtocols.Tests
{

    /// <summary>
    ///This is a test class for AnalogDefinitionTest and is intended
    ///to contain all AnalogDefinitionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AnalogDefinitionTest
    {
        #region [ Members ]
        private ConfigurationCellCase m_ConfigurationCellCase;
        private IConfigurationCell m_IConfigurationCell;
        private ConfigurationCell m_ConfigurationCell;
        private AnalogDefinition target;
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
            m_ConfigurationCellCase  =  new ConfigurationCellCase();
            m_ConfigurationCellCase.ConfigurationFrame2.Cells.TryGetByIDLabel("configCell", out m_IConfigurationCell);
            target = new AnalogDefinition(m_IConfigurationCell);
            m_ConfigurationCell = new ConfigurationCell(m_ConfigurationCellCase.ConfigurationFrame2);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for AnalogDefinition Constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("GSF.PhasorProtocols.dll")]
        public void AnalogDefinitionConstructorTest()
        {
            Assert.IsInstanceOfType(target,typeof(AnalogDefinition));
        }

        /// <summary>
        ///A test for AnalogDefinition Constructor
        ///</summary>
        [TestMethod()]
        public void AnalogDefinitionConstructorTest1()
        {
            string label = "UT";
            uint scale = 10;
            double offset = 1F;
            AnalogType type = new AnalogType();
            AnalogDefinition target = new AnalogDefinition(m_ConfigurationCell, label, scale, offset, type);
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for AnalogDefinition Constructor
        ///</summary>
        [TestMethod()]
        public void AnalogDefinitionConstructorTest2()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CreateNewDefinition
        ///</summary>
        [TestMethod()]
        public void CreateNewDefinitionTest()
        {
            byte[] buffer = new byte[16];
            int startIndex = 0;
            int parsedLength = 4;
            int parsedLengthExpected = 16;
            IAnalogDefinition expected = target;
            IAnalogDefinition actual;
            actual = AnalogDefinition.CreateNewDefinition(m_IConfigurationCell, buffer, startIndex, out parsedLength);
            Assert.AreEqual(parsedLengthExpected, parsedLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParseConversionFactor
        ///</summary>
        [TestMethod()]
        public void ParseConversionFactorTest()
        {
            AnalogDefinition target = new AnalogDefinition(m_IConfigurationCell);
            byte[] buffer = new byte[16];
            int startIndex = 0;
            int expected = 4;
            int actual;
            actual = target.ParseConversionFactor(buffer, startIndex);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ConversionFactorImage
        ///</summary>
        [TestMethod()]
        public void ConversionFactorImageTest()
        {
            AnalogDefinition target = new AnalogDefinition(m_IConfigurationCell);
            byte[] actual;
            actual = target.ConversionFactorImage;
            CollectionAssert.AreEqual(m_ConfigurationCellCase.AnalogDefinition.ConversionFactorImage, actual);
            //Assert.AreEqual(m_ConfigurationCellCase.AnalogDefinition.ConversionFactorImage, actual);
        }

        /// <summary>
        ///A test for Parent
        ///</summary>
        [TestMethod()]
        public void ParentTest()
        {
            AnalogDefinition target = new AnalogDefinition(m_IConfigurationCell);
            ConfigurationCell expected = null;
            ConfigurationCell actual;
            target.Parent = expected;
            actual = target.Parent;
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}
