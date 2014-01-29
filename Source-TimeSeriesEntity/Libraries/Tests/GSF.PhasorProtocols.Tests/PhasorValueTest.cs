#region [ Modification History ]
/*
 * 12/17/2012 Denis Kholine
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118;
#endregion

namespace GSF.PhasorProtocols.Tests
{


    /// <summary>
    ///This is a test class for PhasorValueTest and is intended
    ///to contain all PhasorValueTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PhasorValueTest
    {
        #region [ Members ]
        private DataCellCase m_DataCellCase;
        private ConfigurationCellCase m_ConfigurationCellCase;
        #endregion

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

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_DataCellCase = new DataCellCase();
            m_ConfigurationCellCase = new ConfigurationCellCase();
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        #endregion


        /// <summary>
        ///A test for PhasorValue Constructor
        ///</summary>
        [TestMethod()]
        public void PhasorValueConstructorTest()
        {
            GSF.PhasorProtocols.IDataCell parent = (IDataCell)m_DataCellCase.DataCell;
            GSF.PhasorProtocols.IPhasorDefinition phasorDefinition = (IPhasorDefinition)m_ConfigurationCellCase.PhasorDefinition;
            GSF.PhasorProtocols.IEEEC37_118.PhasorValue target = new GSF.PhasorProtocols.IEEEC37_118.PhasorValue(parent, phasorDefinition);
            Assert.IsInstanceOfType(target, typeof(PhasorValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for PhasorValue Constructor
        ///</summary>
        [TestMethod()]
        public void PhasorValueConstructorTest1()
        {
            GSF.PhasorProtocols.IEEEC37_118.DataCell parent = m_DataCellCase.DataCell;
            GSF.PhasorProtocols.IEEEC37_118.PhasorDefinition phasorDefinition = m_ConfigurationCellCase.PhasorDefinition;
            double real = 10F;
            double imaginary = 20F;
            GSF.PhasorProtocols.IEEEC37_118.PhasorValue target = new GSF.PhasorProtocols.IEEEC37_118.PhasorValue(parent, phasorDefinition, real, imaginary);
            Assert.IsInstanceOfType(target, typeof(PhasorValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for PhasorValue Constructor
        ///</summary>
        [TestMethod()]
        public void PhasorValueConstructorTest2()
        {
            GSF.PhasorProtocols.IEEEC37_118.DataCell parent = m_DataCellCase.DataCell;
            GSF.PhasorProtocols.IEEEC37_118.PhasorDefinition phasorDefinition = m_ConfigurationCellCase.PhasorDefinition;
            GSF.Units.Angle angle = new GSF.Units.Angle(10);
            double magnitude = 20F;
            GSF.PhasorProtocols.IEEEC37_118.PhasorValue target = new GSF.PhasorProtocols.IEEEC37_118.PhasorValue(parent, phasorDefinition, angle, magnitude);
            Assert.IsInstanceOfType(target, typeof(PhasorValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CreateNewValue
        //////</summary>
        [TestMethod()]
        public void CreateNewValueTest()
        {
            GSF.PhasorProtocols.IDataCell parent = m_DataCellCase.DataCell;
            GSF.PhasorProtocols.IPhasorDefinition definition =(IPhasorDefinition)m_ConfigurationCellCase.PhasorDefinition;
            byte[] buffer = null;
            int startIndex = 0;
            int parsedLength = 0;
            int parsedLengthExpected = 0;
            GSF.PhasorProtocols.IPhasorValue expected = null;
            GSF.PhasorProtocols.IPhasorValue actual;
            actual = GSF.PhasorProtocols.IEEEC37_118.PhasorValue.CreateNewValue(parent, definition, buffer, startIndex, out parsedLength);
            Assert.AreEqual(parsedLengthExpected, parsedLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Definition
        ///</summary>
        [TestMethod()]
        public void DefinitionTest()
        {
            GSF.PhasorProtocols.IDataCell parent = (IDataCell)m_DataCellCase.DataCell;
            GSF.PhasorProtocols.IPhasorDefinition phasorDefinition = (IPhasorDefinition)m_ConfigurationCellCase.PhasorDefinition;
            GSF.PhasorProtocols.IEEEC37_118.PhasorValue target = new GSF.PhasorProtocols.IEEEC37_118.PhasorValue(parent, phasorDefinition);
            GSF.PhasorProtocols.IEEEC37_118.PhasorDefinition expected = (PhasorDefinition)phasorDefinition;
            GSF.PhasorProtocols.IEEEC37_118.PhasorDefinition actual;
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
            GSF.PhasorProtocols.IDataCell parent = (IDataCell)m_DataCellCase.DataCell;
            GSF.PhasorProtocols.IPhasorDefinition phasorDefinition = (IPhasorDefinition)m_ConfigurationCellCase.PhasorDefinition;
            GSF.PhasorProtocols.IEEEC37_118.PhasorValue target = new GSF.PhasorProtocols.IEEEC37_118.PhasorValue(parent, phasorDefinition);
            GSF.PhasorProtocols.IEEEC37_118.DataCell expected = (DataCell)parent;
            GSF.PhasorProtocols.IEEEC37_118.DataCell actual;
            target.Parent = expected;
            actual = target.Parent;
            Assert.AreEqual(expected, actual);
        }
    }
}
