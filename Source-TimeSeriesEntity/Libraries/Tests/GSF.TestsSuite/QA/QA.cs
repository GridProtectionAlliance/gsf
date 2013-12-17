#region [ Modification History ]
/*
 * 5/17/2013 Denis Kholine
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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
#endregion


namespace UnitTestsSuite.QA
{

    #region [ QA objects ]
    /// <summary>
    /// Drop by weight of 1 gm equal 1 cm square
    /// </summary>
    public class WaterDrop : IEquatable<WaterDrop>
    {
        #region [ Constructors ]
        public WaterDrop()
        { }
        #endregion

        #region [ Interfaces ]
        public bool Equals(WaterDrop drop)
        {
            if (drop == null)
            {
                return false;
            }
            else if (this == drop)
            {
                return true;
            }
            else return false;
        }
        #endregion

    }

    [DefaultValue(false)]
    public class Pool : WaterDrop
    {
        #region [ Members ]
        private Double mass;
        #endregion


        #region [ Constructors ]
        public Pool(Double mass)
        {
            this.mass = mass;
        }
        #endregion
    }

    #endregion

    /// <summary>
    /// Test class to reproduce question logic and code behavior
    /// Verify default behavior for classes and structures
    /// See Microsoft help: DefaultValueAttribute Class
    /// http://msdn.microsoft.com/en-us/library/system.componentmodel.defaultvalueattribute.aspx
    /// Reference equality means that two object references refer to the same underlying object
    /// ps. not if values are null by default not initialized object can be presented as equal
    /// </summary>
    [TestClass]
    public class QA
    {
        #region [ Members ]
        private Pool m_Pool;
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
            m_Pool = new Pool(10);

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
        [TestMethod]
        public void ClassAsserts()
        {
            //Pool item01 = new Pool(10);
            //Pool item02 = new Pool(10);
            //Pool item03 = m_Pool;
            Pool item01 = m_Pool;
            Pool item02 = (Pool)m_Pool;

            Assert.AreEqual(item01, item02);

        }
        #endregion

    }
}
