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
using System.Linq;
using System.Net;
using System.Xml;
using System.Data.SqlClient;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF;
using GSF.IO;
using GSF.Data;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.IEEEC37_118;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118;
#endregion

namespace GSF.PhasorProtocols.Tests
{


    /// <summary>
    ///This is a test class for DigitalValueTest and is intended
    ///to contain all DigitalValueTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DigitalValueTest
    {
        #region [ Members ]
        private IDataCell parent;
        private DataCellCase m_DataCellCase;
        private IDigitalDefinition digitalDefinition;
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

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_ConfigurationCellCase = new ConfigurationCellCase();
            m_DataCellCase = new DataCellCase();
            parent = m_DataCellCase.DataCell;
            digitalDefinition = new DigitalDefinition(m_ConfigurationCellCase.ConfigurationCell);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        #endregion

        #region [ Members ]
        /// <summary>
        ///A test for DigitalValue Constructor
        ///</summary>
        [TestMethod()]
        public void DigitalValueConstructorTest()
        {
            DigitalValue target = new DigitalValue(parent, digitalDefinition);
            Assert.IsInstanceOfType(target, typeof(DigitalValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for DigitalValue Constructor
        ///</summary>
        [TestMethod()]
        public void DigitalValueConstructorTest1()
        {
            ushort value = 10;
            DigitalValue target = new DigitalValue((DataCell)parent, (DigitalDefinition)digitalDefinition, value);
            Assert.IsInstanceOfType(target, typeof(DigitalValue));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for CreateNewValue
        ///</summary>
        [TestMethod()]
        public void CreateNewValueTest()
        {
            DigitalValue target = new DigitalValue(parent, digitalDefinition);
            byte[] buffer = new byte[target.BinaryLength];
            int startIndex = 0;
            int parsedLength = 2;
            int parsedLengthExpected = 2;
            IDigitalValue expected = target;
            IDigitalValue actual;
            actual = DigitalValue.CreateNewValue(parent, target.Definition, buffer, startIndex, out parsedLength);
            Assert.AreEqual(parsedLengthExpected, parsedLength);
            Assert.ReferenceEquals(expected, actual);
            //TODO: equality verification needed
            //Assert.AreEqual(actual, expected);
        }

        /// <summary>
        ///A test for Definition
        ///</summary>
        [TestMethod()]
        public void DefinitionTest()
        {
            DigitalValue target = new DigitalValue((IDataCell)parent, digitalDefinition);
            DigitalDefinition expected = (DigitalDefinition)digitalDefinition;
            DigitalDefinition actual;
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
            DigitalValue target = new DigitalValue((IDataCell)parent, digitalDefinition);
            DataCell expected = (DataCell)parent;
            DataCell actual;
            target.Parent = expected;
            actual = target.Parent;
            Assert.AreEqual(expected, actual);
        }
        #endregion
    }
}
