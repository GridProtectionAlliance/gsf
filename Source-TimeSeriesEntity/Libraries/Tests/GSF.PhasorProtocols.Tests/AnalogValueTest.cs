#region [ Modification History ]
/*
 * 11/06/2012 Denis Kholine
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
using GSF.PhasorProtocols; 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118;
#endregion

namespace GSF.PhasorProtocols.Tests
{
    /// <summary>
    ///This is a test class for AnalogValueTest and is intended
    ///to contain all AnalogValueTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AnalogValueTest
    {
        #region [ Members ]
        private AnalogValue target;
        private DataCellCase m_DataCellCase;
        private ConfigurationCellCase m_ConfigurationCellCase;
        // Declare Remote types
        private IDataCell parent;
        private IAnalogDefinition analogDefinition;
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
            #region [ Instantiate ]
            m_DataCellCase = new DataCellCase();
            m_ConfigurationCellCase = new ConfigurationCellCase();
            #endregion

            #region [ Cast ]
            parent = (IDataCell)m_DataCellCase.DataCell;
            analogDefinition = (IAnalogDefinition)m_ConfigurationCellCase.AnalogDefinition;
            #endregion

            #region [ Set target ]
            target = new AnalogValue(parent, analogDefinition);
            #endregion
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        #endregion

        #region [ Methods ]
        /// <summary>
        ///A test for AnalogValue Constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("GSF.PhasorProtocols.dll")]
        public void AnalogValueConstructorTest()
        {
            Assert.IsInstanceOfType(target, typeof(AnalogValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for AnalogValue Constructor
        ///</summary>
        [TestMethod()]
        public void AnalogValueConstructorTest1()
        {
            double value = 10F;
            AnalogValue target = new AnalogValue((DataCell)parent, (AnalogDefinition)analogDefinition, value);
            Assert.IsInstanceOfType(target, typeof(AnalogValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for AnalogValue Constructor
        ///</summary>
        [TestMethod()]
        public void AnalogValueConstructorTest2()
        {
            AnalogValue target = new AnalogValue(parent, analogDefinition);
            Assert.IsInstanceOfType(target, typeof(AnalogValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CreateNewValue
        ///</summary>
        [TestMethod()]
        public void CreateNewValueTest()
        {
            byte[] buffer = new byte[4];
            int startIndex = 0;
            int parsedLength = 4;
            int parsedLengthExpected = 4;
            IAnalogValue expected = target;
            IAnalogValue actual;
            actual = AnalogValue.CreateNewValue(parent, analogDefinition, buffer, startIndex, out parsedLength);
            Assert.AreEqual(parsedLengthExpected, parsedLength);
            Assert.AreEqual(expected.Value, actual.Value);
            Assert.AreEqual(expected.IntegerValue, actual.IntegerValue);
        }

        /// <summary>
        ///A test for Definition
        ///</summary>
        [TestMethod()]
        public void DefinitionTest()
        {
            AnalogValue target = new AnalogValue(parent, analogDefinition);
            AnalogDefinition expected = (AnalogDefinition)analogDefinition;
            AnalogDefinition actual;
            target.Definition = expected;
            actual = target.Definition;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Parent
        ///</summary>
        [TestMethod()]
        public void ParentTest()
        {
            AnalogValue target = new AnalogValue(parent, analogDefinition);
            DataCell expected = (DataCell)parent;
            DataCell actual;
            target.Parent = expected;
            actual = target.Parent;
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}
